using System.IO;
using System.Text.Json;

namespace NetCoreFileAccess.SourceAccess
{
    #region ENUMS
    public enum SourceType
    {
        None,
        Local,
        GoogleDrive,
        Ftp,
    }
    #endregion
    public class SourceAccess
    {
        #region CONSTANTS        
        
        private const string CONFIG_FILE_NAME = "config.json";

        #endregion

        #region FIELDS 

        private readonly ISourceAccess _sourceAccess;

        private static string? ConfigFile;

        private static SourceType sourceType;

        

        #endregion

        #region FIELDS STATIC PATHS

        private const string ConfigFolderRelative = "config";
        
        private static string GetConfigFolderPath()
            => Path.Combine(AppContext.BaseDirectory, ConfigFolderRelative);

        #endregion

        #region PROPERTIES
        
        public bool IsInicializing { get; private set; }
        
        #endregion

        #region CONSTRUCTORS

        public SourceAccess(ISourceAccess sourceAccess)
        {
            _sourceAccess = sourceAccess;
            IsInicializing = sourceAccess.IsInicializing;
        }

        #endregion
        
        #region PUBLIC FUNTIONS
        public bool Login(string User, string Password)
        {
            return _sourceAccess.Login(User, Password);    ;
        }

        public string GetFile()
        {
            return _sourceAccess.GetFile();
        }

        public bool SaveFile(MemoryStream content)
        {
            return _sourceAccess.SaveFile(content);
        }

        public MemoryStream GetFileData() 
        { 
            return _sourceAccess.GetFileData();
        }

        #endregion

        #region STATICS PUBLIC METHODS 
        // Loads stored source type. Returns SourceType.Local if not found or on error.
        public static SourceType SelectTypeSource()
        {
            try
            {
                sourceType = SourceType.None;
                var path = GetConfigFilePath();
                
                var json = File.ReadAllText(path);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("Source", out var prop))
                {
                    var s = prop.GetString();
                    if (Enum.TryParse<SourceType>(s, out var result))
                        sourceType = result;
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
            if(string.IsNullOrEmpty(ConfigFile)) 
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

        #region STATICS PRIVATE METHODS 

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

}
