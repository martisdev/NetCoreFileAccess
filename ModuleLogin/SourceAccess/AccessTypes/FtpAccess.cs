using System.IO;

namespace NetCoreFileAccess.SourceAccess.AccessTypes
{
    public class FtpAccess : ISourceAccess
    {
        #region PROPERTIES        
        public string PathFile { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public bool IsInicializing { get; set; }
        #endregion

        #region CONSTRUCTORS
        public FtpAccess()
        {
            PathFile = string.Empty;
            UserName = string.Empty;
            Password = string.Empty;
            PathFile = "/";
        }
        #endregion

        public bool Login(string User, string Password)
        {
            //try to connect to FTP server with provided credentials
            if (IsInicializing)
            {
                // Dialog to set configuration FTP server

                //try to connect to FTP server with provided configuration

                //set configuration parameters into cofig file

                return false;
            }
            else
            {
                //try to connect to FTP server with provided configuration
                using (MemoryStream ms = new MemoryStream())
                using (FileStream file = new FileStream(PathFile, FileMode.Open, FileAccess.Read))
                {
                    byte[] bytes = new byte[file.Length];
                    int totalRead = 0;
                    while (totalRead < bytes.Length)
                    {
                        int bytesRead = file.Read(bytes, totalRead, bytes.Length - totalRead);
                        if (bytesRead == 0)
                        {
                            break; // End of file reached before expected
                        }
                        totalRead += bytesRead;
                    }
                    ms.Write(bytes, 0, totalRead);
                    //FileManipulation.ValidateCredentials(ms, User, Password);
                }

                
                return false;
            }            
        }

        public string GetFile()
        {
            throw new NotImplementedException();
        }

        public bool SaveFile(MemoryStream content, string NameFile)
        {
            string SourceFile = Path.Combine(PathFile, NameFile);
            throw new NotImplementedException();
        }      
        public MemoryStream GetFileData()
        {
            throw new NotImplementedException();
        }
    }
}
