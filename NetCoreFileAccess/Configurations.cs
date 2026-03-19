using NetCoreFileAccess.Models; 
using NetCoreFileAccess.SourceAccess;
using System.IO;

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
                }
            }
            catch
            {
                // swallow or log as appropriate
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
            // set default values
            Config.sourceType = SourceType.Local;
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
        } 
    }
}
