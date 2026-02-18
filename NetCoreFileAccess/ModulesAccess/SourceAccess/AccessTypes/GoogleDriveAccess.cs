using System.IO;

namespace NetCoreFileAccess.SourceAccess.AccessTypes
{

    public class GoogleDriveAccess : ISourceAccess
    {
        #region PROPERTIES        
        public string PathFile { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }
        
        public bool IsInicializing { get; set; }

        #endregion

        #region CONSTRUCTORS   
        public GoogleDriveAccess()
        {
            PathFile = string.Empty;
            UserName = string.Empty;
            Password = string.Empty;
        }
        #endregion
        public bool Login(string User, string Password)
        {
            throw new NotImplementedException();
        }
        public string GetFile()
        {
            throw new NotImplementedException();
        }
        public bool SaveFile(MemoryStream content, string NameFile)
        {
            throw new NotImplementedException();
        }
        public MemoryStream GetFileData()
        {
                throw new NotImplementedException();
        }
    }
}
