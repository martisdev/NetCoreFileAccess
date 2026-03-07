using NetCoreFileAccess.SourceAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
        // Loads stored source type. Returns SourceType.Local if not found or on error.
        public static SourceType SelectTypeSource()
        {
            try
            {
                SourceType sourceType = SourceType.None;
                var path = GetConfigFilePath();

                var json = File.ReadAllText(path);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("Source", out var prop))
                {
                    var s = prop.GetString();
                    if (Enum.TryParse<SourceType>(s, out var result))
                        sourceType = result;
                }
                switch (sourceType)
                {
                    case SourceType.Local:
                        break;
                    case SourceType.GoogleDrive:
                        if (doc.RootElement.TryGetProperty("GoogleDrive", out var GoogleSection) && GoogleSection.ValueKind == JsonValueKind.Object)
                        {
                            GoogleConfig.PathFile = GoogleSection.TryGetProperty("PathFile", out var PathFileProp) ? PathFileProp.GetString() ?? string.Empty : string.Empty;
                            GoogleConfig.PathCredential = GoogleSection.TryGetProperty("PathCredential", out var PathCredentialProp) ? PathCredentialProp.GetString() ?? string.Empty : string.Empty;
                            GoogleConfig.ClientId = GoogleSection.TryGetProperty("clientId", out var clientIdProp) ? clientIdProp.GetString() ?? string.Empty : string.Empty;
                            GoogleConfig.ClientSecret = GoogleSection.TryGetProperty("clientSecret", out var clientSecretProp) ? clientSecretProp.GetString() ?? string.Empty : string.Empty;
                            GoogleConfig.Project_ID = GoogleSection.TryGetProperty("project_id", out var projectIdProp) ? projectIdProp.GetString() ?? string.Empty : string.Empty;
                        }
                        break;
                    case SourceType.Ftp:
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

                            FTPConfig.Url = url ?? string.Empty;
                            FTPConfig.Username = ftpSection.TryGetProperty("UserName", out var userProp) ? userProp.GetString() ?? string.Empty : string.Empty;
                            FTPConfig.Password = ftpSection.TryGetProperty("Password", out var pwdProp) ? pwdProp.GetString() ?? string.Empty : string.Empty;

                            // PathFile: prefer explicit PathFile; fall back to RemotePath or empty
                            if (ftpSection.TryGetProperty("PathFile", out var pfProp))
                                FTPConfig.PathFile = pfProp.GetString() ?? string.Empty;
                            else if (ftpSection.TryGetProperty("RemotePath", out var rpProp))
                                FTPConfig.PathFile = rpProp.GetString() ?? string.Empty;
                            else
                                FTPConfig.PathFile = string.Empty;
                        }

                        break;
                    default:

                        break;
                }

                return sourceType;
            }
            catch
            {
                // Swallow errors and return default; consider logging in real app.
            }

            return SourceType.None;

        }

        // Save the selected source type to Program\Data\config.json
        public static void SaveSelectedSourceType(SourceType type)
        {
            if (string.IsNullOrEmpty(ConfigFile))
                return;

            try
            {
                var payload = new { Source = type.ToString() };
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(payload, options);

                File.WriteAllText(ConfigFile, json);
            }
            catch
            {
                // Consider bubbling/logging errors. Writing to app folder may require privileges.
                throw;
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
            SaveSelectedSourceType(SourceType.Local);
            return ConfigFile;
        }
        #endregion

    }


    public static class FTPConfig
    {
        #region PROPERTIES
        
        public static string Url { get; set; }
        
        public static string Username { get; set; }

        public static string Password { get; set; }

        public static string PathFile { get; set; }
        
        #endregion

    }

    public static class GoogleConfig
    {
        public static string PathFile { get; set; }
        
        public static string PathCredential { get; set; }

        public static string ClientId { get; set; }
        
        public static string ClientSecret { get; set; }

        public static string Project_ID { get; set; }

    }


}
