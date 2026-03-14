using NetCoreFileAccess.Criptography;
using NetCoreFileAccess.SourceAccess;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
//using static System.Net.WebRequestMethods;
using NetCoreFileAccess.Models; // add at top of file

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

        #region PUBLIC METHODS 

        public static void SaveConfigurationFile()
        {
            if (string.IsNullOrEmpty(ConfigFile))
                return;

            try
            {
                var model = new ConfigModel
                {
                    Source = Config.sourceType.ToString(),
                    Ftp = new FtpModel
                    {
                        Host = Config.FTPConfig.Host,
                        Port = Config.FTPConfig.Port,
                        Username = Config.FTPConfig.Username,
                        Password = Config.FTPConfig.Password,
                        PathFile = Config.FTPConfig.PathFile
                    },
                    GoogleDrive = new GoogleDriveModel
                    {
                        PathFile = Config.GoogleConfig.PathFile,
                        CCMng = Config.GoogleConfig.BlockCredentials
                    }
                };

                var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                var json = System.Text.Json.JsonSerializer.Serialize(model, options);
                System.IO.File.WriteAllText(ConfigFile, json);
            }
            catch
            {
                throw;
            }
        }

        public static void LoadConfigurationFile()
        {
            try
            {
                SetConfigFile();

                if (string.IsNullOrEmpty(ConfigFile) || !File.Exists(ConfigFile))
                    return;

                var json = File.ReadAllText(ConfigFile);
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var model = System.Text.Json.JsonSerializer.Deserialize<ConfigModel>(json, options);
                if (model == null)
                    return;

                // map back to static Config
                if (!string.IsNullOrEmpty(model.Source) && Enum.TryParse<SourceType>(model.Source, out var st))
                    Config.sourceType = st;

                if (model.Ftp != null)
                {
                    Config.FTPConfig.Host = model.Ftp.Host ?? string.Empty;
                    Config.FTPConfig.Port = model.Ftp.Port;
                    Config.FTPConfig.Username = model.Ftp.Username ?? string.Empty;
                    Config.FTPConfig.Password = model.Ftp.Password ?? string.Empty;
                    Config.FTPConfig.PathFile = model.Ftp.PathFile ?? string.Empty;
                }

                if (model.GoogleDrive != null)
                {
                    Config.GoogleConfig.PathFile = model.GoogleDrive.PathFile ?? string.Empty;
                    Config.GoogleConfig.BlockCredentials = model.GoogleDrive.CCMng ?? string.Empty;                    
                }
            }
            catch
            {
                // swallow or log as appropriate
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
            //Secret Manager overview
            //https://docs.cloud.google.com/secret-manager/docs/overview
            if (string.IsNullOrEmpty(BlockCredentials) || string.IsNullOrEmpty(Key))
                return;

            byte[] payload;
            try
            {
                payload = Convert.FromBase64String(BlockCredentials);
            }
            catch
            {
                return;
            }

            Byte[] Appkey = Encoding.UTF8.GetBytes(Key);

            using (var stream = new MemoryStream(payload))
            using (var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true))
            {
                // skip random prefix
                stream.Position = Cryptography.RAMDOM_LENGTH;

                // read IV
                byte[] keyIV = reader.ReadBytes(Cryptography.IV_LENGTH);
                Cryptography.KEY_IV = keyIV;

                // read length-prefixed encrypted ID
                int idLen = reader.ReadInt32();
                byte[] DataID = reader.ReadBytes(idLen);
                byte[]? DecriptID = Cryptography.AESDecrypt(DataID, Appkey);
                if (DecriptID == null) return;

                // read length-prefixed encrypted Secret
                int secretLen = reader.ReadInt32();
                byte[] DataSecret = reader.ReadBytes(secretLen);
                byte[]? DecriptSecret = Cryptography.AESDecrypt(DataSecret, Appkey);
                if (DecriptSecret == null) return;

                Config.GoogleConfig.ClientId = Encoding.UTF8.GetString(DecriptID).TrimEnd('\0');
                Config.GoogleConfig.ClientSecret = Encoding.UTF8.GetString(DecriptSecret).TrimEnd('\0');
            }
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
            byte[] randomPrefix = Cryptography.GererateRamdomKEY(Cryptography.RAMDOM_LENGTH);

            byte[] BytesID = Encoding.UTF8.GetBytes(ClientId);           
            Array.Resize(ref BytesID, 128);
            
            byte[] EncryptId = Cryptography.AESEncrypt(BytesID, Appkey);
            if (EncryptId == null)
                return string.Empty;

            byte[] BytesSecret = Encoding.UTF8.GetBytes(ClientSecret);
            Array.Resize(ref BytesSecret, 128);
            
            byte[] EncryptSecret = Cryptography.AESEncrypt(BytesSecret, Appkey);
            if (EncryptSecret == null)
                return string.Empty;


            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
            {
                // write random prefix and IV
                writer.Write(randomPrefix);
                writer.Write(Cryptography.KEY_IV);

                // write length + encrypted id
                writer.Write(EncryptId.Length);
                writer.Write(EncryptId);

                // write length + encrypted secret
                writer.Write(EncryptSecret.Length);
                writer.Write(EncryptSecret);

                writer.Flush();
                return Convert.ToBase64String(stream.ToArray());
            }
        }
        #endregion

        #region PRIVATE METHODS 

        private static void SetConfigFile()
        {
            string ConfigFolder = GetConfigFolderPath();
            if (!Directory.Exists(ConfigFolder))
                Directory.CreateDirectory(ConfigFolder);

            ConfigFile = Path.Combine(ConfigFolder, CONFIG_FILE_NAME);

            if (File.Exists(ConfigFile))
                return ;

            //Create config file if not exists
            SaveConfigurationFile();            
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
