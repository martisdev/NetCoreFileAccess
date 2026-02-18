using System.IO;

namespace NetCoreFileAccess.SourceAccess.AccessTypes
{
    public class LocalAccess: ISourceAccess
    {
        #region CONSTANTS
        
        private const string DATA_FOLDER = "data";
        
        private const string FILE_EXTENSION = ".fscr";
        #endregion

        #region PROPERTIES        
        
        public string UserName { get; set; }

        public string Password { get; set; }

        public bool IsInicializing { get; set; }

        public string PathFile { get; set; }
            
        #endregion

        #region CONSTRUCTORS
        public LocalAccess()
        {
            PathFile = GetPathDataFile();
            UserName = string.Empty;
            Password = string.Empty;
        }
        #endregion

        #region PUBLIC METHODS 

        public bool Login(string User ,string Password)
        {
            if(IsInicializing)
            {
                IsInicializing = false;
                UserName = User;
                this.Password = Password;
                return true;
            }
            else
            {                
                if (File.Exists(PathFile))
                {
                    //try to open existing file with provided credentials
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (FileStream file = new FileStream(PathFile, FileMode.Open, FileAccess.Read))
                        {
                            file.CopyTo(ms);
                            bool result = CredentialsUtils.ValidateCredentials(ms, User, Password);
                            if(result)
                            {
                                UserName = User;
                                this.Password = Password;
                            }
                            return result;
                        }
                    }                    
                }
            }
            UserName = string.Empty;
            this.Password = string.Empty;
            return false;
        }

        public string GetFile()
        {
            return string.Empty;
        }

        public bool SaveFile(MemoryStream content)
        {
            try
            {
                FileMode FM = FileMode.OpenOrCreate;
                if (File.Exists(PathFile))
                    FM = FileMode.Truncate;

                using (FileStream stream = new FileStream(PathFile, FM, FileAccess.Write))
                {
                    stream.Position = 0;                    
                    content.CopyTo(stream);
                }
                return true;
            }
            catch {return false;}                
        }

        public MemoryStream GetFileData()
        {
            using (FileStream stream = new FileStream(PathFile, FileMode.Open, FileAccess.Read))
            {
                MemoryStream ms = new MemoryStream();
                stream.CopyTo(ms);
                ms.Position = 0;
                return ms;
            }
        }
        #endregion

        #region PRIVATE METHODS

        private string GetPathDataFile()
        {
            string path = Path.Combine(AppContext.BaseDirectory, DATA_FOLDER);

            //Create data folder if not exists
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            
            //Search existing file
            string? _File = Directory.GetFiles(path, "*" + FILE_EXTENSION).FirstOrDefault();
            if (string.IsNullOrEmpty(_File))
            {
                //file not exist
                string fileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + FILE_EXTENSION;
                _File = Path.Combine(path, fileName);
                IsInicializing = true;                
                return _File;
            }
            else
            {
                FileInfo fileInfo = new FileInfo(_File);
                if (fileInfo.Length == 0)
                {
                    //the file is empty
                    IsInicializing = true;
                    return string.Empty;
                }
            }

            //file exist and is not empty
            IsInicializing = false;
            return _File;
        }
        
        #endregion
    }
}
