using NetCoreFileAccess.Criptography;
using NetCoreFileAccess.SourceAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace NetCoreFileAccess
{
    public static class Configurations
    {
        #region CONSTANTS        

        private const string CONFIG_FILE_NAME = "config.json";


        #endregion

        #region FIELDS

        private static string? ConfigFile;

        private const string ConfigFolderRelative = "config";
        private static string GetConfigFolderPath()
            => Path.Combine(AppContext.BaseDirectory, ConfigFolderRelative);

        #endregion

        #region PROPERTIES
        
        #endregion

        #region PUBLIC METHODS 
        // Loads stored source type. Returns SourceType.Local if not found or on error.
        public static SourceType SelectTypeSource()
        {
            try
            {
                // Default to None; consider Local if you want a fallback that always works without config.
                SourceType sourceType = SourceType.None;
                var path = GetConfigFilePath();

                var json = System.IO.File.ReadAllText(path);
                using var doc = JsonDocument.Parse(json);
                // Try to get the "Source" property from the root of the JSON document and parse it as SourceType enum.
                if (doc.RootElement.TryGetProperty("Source", out var prop))
                {
                    var s = prop.GetString();
                    if (Enum.TryParse<SourceType>(s, out var result))
                        sourceType = result;
                }

                // Get the Google Drive credentials from the config file and set them to the static properties for later use.
                if (doc.RootElement.TryGetProperty("GoogleDrive", out var GoogleSection) && GoogleSection.ValueKind == JsonValueKind.Object)
                {
                    Config.GoogleConfig.PathFile = GoogleSection.TryGetProperty("PathFile", out var PathFileProp) ? PathFileProp.GetString() ?? string.Empty : string.Empty;
                    Config.GoogleConfig.BlockCredentials = GoogleSection.TryGetProperty("CCMng", out var CCMngProp) ? CCMngProp.GetString() ?? string.Empty : string.Empty;                    
                }

                // Get the ftp credentials from the config file and set them to the static properties for later use.
                if (doc.RootElement.TryGetProperty("Ftp", out var ftpSection) && ftpSection.ValueKind == JsonValueKind.Object)
                {
                    // Host (or Url)
                    string host = ftpSection.TryGetProperty("Host", out var hostProp) ? hostProp.GetString() ?? string.Empty : string.Empty;
                    string url = ftpSection.TryGetProperty("Url", out var urlProp) ? urlProp.GetString() ?? string.Empty : string.Empty;

                    // Port
                    int port = 21;
                    if (ftpSection.TryGetProperty("Port", out var portProp) && portProp.TryGetInt32(out var p))
                        port = p;

                    // UseSsl
                    bool useSsl = ftpSection.TryGetProperty("UseSsl", out var useSslProp) && useSslProp.GetBoolean();

                    // If Url not provided, compose from Host/Port/UseSsl
                    if (string.IsNullOrWhiteSpace(url) && !string.IsNullOrWhiteSpace(host))
                    {
                        var scheme = useSsl ? "ftps" : "ftp";
                        // If host already contains scheme, use it as-is (but still append port when non-default)
                        if (host.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase) || host.StartsWith("ftps://", StringComparison.OrdinalIgnoreCase))
                            url = host;
                        else
                            url = $"{scheme}://{host}";

                        if (port != 21 && port != 0)
                            url = $"{url}:{port}";
                    }

                    Config.FTPConfig.Host = url ?? string.Empty;
                    Config.FTPConfig.Port = port;
                    Config.FTPConfig.Username = ftpSection.TryGetProperty("UserName", out var userProp) ? userProp.GetString() ?? string.Empty : string.Empty;
                    Config.FTPConfig.Password = ftpSection.TryGetProperty("Password", out var pwdProp) ? pwdProp.GetString() ?? string.Empty : string.Empty;

                    // PathFile: prefer explicit PathFile; fall back to RemotePath or empty
                    if (ftpSection.TryGetProperty("PathFile", out var pfProp))
                        Config.FTPConfig.PathFile = pfProp.GetString() ?? string.Empty;
                    else if (ftpSection.TryGetProperty("RemotePath", out var rpProp))
                        Config.FTPConfig.PathFile = rpProp.GetString() ?? string.Empty;
                    else
                        Config.FTPConfig.PathFile = string.Empty;
                }

                return sourceType;
            }
            catch{}

            return SourceType.None;
        }

        
        /// <summary>
        ///Save the selected source type to Program\Data\config.json 
        /// </summary>
        /// <param name="type"></param>
        public static void SaveSelectedSourceType()
        {
            if (string.IsNullOrEmpty(ConfigFile))
                return;

            try
            {
                var payload = new 
                { 
                    
                };
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(payload, options);

                System.IO.File.WriteAllText(ConfigFile, json);
            }
            catch
            {
                // Consider bubbling/logging errors. Writing to app folder may require privileges.
                throw;
            }
        }


        /// <summary>
        /// Extracts and decrypts Google Drive API credentials from an encrypted credentials block using the specified
        /// key.
        /// </summary>
        /// <remarks>This method updates <c>Config.GoogleConfig.ClientId</c> and
        /// <c>Config.GoogleConfig.ClientSecret</c> with the decrypted values if decryption succeeds. If decryption
        /// fails, the configuration values are not updated.</remarks>
        /// <param name="BlockCredentials">A UTF-8 encoded string containing the encrypted credentials block. The block must be formatted as expected
        /// by the decryption logic.</param>
        /// <param name="Key">The decryption key used to decrypt the client ID and client secret. Must not be null or empty.</param>
        public static void GetGoogleDriveCredentials(string BlockCredentials, string Key)
        {
            Byte[] Appkey = Encoding.UTF8.GetBytes(Key);
            //Secret Manager overview
            //https://docs.cloud.google.com/secret-manager/docs/overview

            using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(BlockCredentials));
            stream.Position = Cryptography.RAMDOM_LENGTH;
            
            byte[] keyIV = new byte[Cryptography.IV_LENGTH];

            int bytesRead = stream.Read(keyIV, 0, keyIV.Length);
            Cryptography.KEY_IV = keyIV;

            byte[] DataID = new byte[128];
            bytesRead = stream.Read(DataID, 0, DataID.Length);

            byte[]? DecriptID = Cryptography.AESDecrypt(DataID, Appkey);
            if(DecriptID == null)
                return;

            byte[] DataSecret = new byte[128];
            bytesRead = stream.Read(DataSecret, 0, DataSecret.Length);

            byte[]? DecriptSecret = Cryptography.AESDecrypt(DataSecret, Appkey);
            if (DecriptSecret == null)
                return;

            Config.GoogleConfig.ClientId = Encoding.UTF8.GetString(DecriptID);
            Config.GoogleConfig.ClientSecret = Encoding.UTF8.GetString(DecriptSecret); 
        }

        /// <summary>
        /// Encrypts and packages Google Drive API credentials for secure storage or transmission.
        /// </summary>
        /// <remarks>Use this method to securely store Google Drive API credentials by encrypting them
        /// with a specified key. The resulting string should be kept confidential and can be stored in a configuration
        /// file or secret manager. To use the credentials, a corresponding decryption method and the original key are
        /// required.</remarks>
        /// <param name="ClientId">The Google Drive API client ID to be encrypted. Cannot be <see langword="null"/> or empty.</param>
        /// <param name="ClientSecret">The Google Drive API client secret to be encrypted. Cannot be <see langword="null"/> or empty.</param>
        /// <param name="Key">The encryption key used to secure the credentials. Must be a non-empty string and suitable for AES
        /// encryption.</param>
        /// <returns>A UTF-8 encoded string containing the encrypted client ID and client secret, along with associated
        /// cryptographic metadata. This string can be stored securely and later used to retrieve the credentials.</returns>
        public static string SetGoogleDriveCredentials(string ClientId, string ClientSecret, string Key)
        {
            Byte[] Appkey = Encoding.UTF8.GetBytes(Key);
            Cryptography.GenerateIV();
            byte[] StrRamdom = Cryptography.GererateRamdomKEY(Cryptography.RAMDOM_LENGTH);

            byte[] BytesID = Encoding.UTF8.GetBytes(ClientId);           
            Array.Resize(ref BytesID, 128);
            
            byte[] BytesSecret = Encoding.UTF8.GetBytes(ClientSecret);
            Array.Resize(ref BytesSecret, 128);


            byte[] EncryptId = Cryptography.AESEncrypt(BytesID, Appkey);
            if(EncryptId == null)
                return string.Empty;

            byte[] EncryptSecret = Cryptography.AESEncrypt(BytesSecret, Appkey);
            if (EncryptSecret == null)
                return string.Empty;

            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(StrRamdom, 0, StrRamdom.Length);
                stream.Write(Cryptography.KEY_IV, 0, Cryptography.KEY_IV.Length);

                stream.Write(EncryptId, 0, EncryptId.Length);
                stream.Write(EncryptSecret, 0, EncryptSecret.Length);

                // The resulting string can be stored in the config file or secret manager as needed.            
                return Encoding.UTF8.GetString(stream.ToArray());
            }    
        }
        #endregion

        #region PRIVATE METHODS 

        private static string GetConfigFilePath()
        {
            string ConfigFolder = GetConfigFolderPath();
            if (!Directory.Exists(ConfigFolder))
                Directory.CreateDirectory(ConfigFolder);

            ConfigFile = Path.Combine(ConfigFolder, CONFIG_FILE_NAME);

            if (!string.IsNullOrEmpty(ConfigFile))
                return ConfigFile;

            //Create config file if not exists
            SaveSelectedSourceType();
            return ConfigFile;
        }
        #endregion

    }

    public static class Config
    {
        public static SourceType sourceType;

        public static class FTPConfig
        {
            #region PROPERTIES

            public static string Host { get; set; } = string.Empty;

            public static int Port { get; set; } = 21;

            public static string Username { get; set; } = string.Empty;

            public static string Password { get; set; } = string.Empty;

            public static string PathFile { get; set; } = string.Empty;

            #endregion

        }

        public static class GoogleConfig
        {
            public static string PathFile { get; set; } = string.Empty;

            public static string ClientId { get; set; } = string.Empty;

            public static string ClientSecret { get; set; } = string.Empty;

            public static string BlockCredentials { get; set; } = string.Empty;
        
        } 
    }
}
