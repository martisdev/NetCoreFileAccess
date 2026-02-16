using System.IO;

namespace NetCoreFileAccess.SourceAccess
{
    public interface ISourceAccess
    {
        #region PROPERTIES        
        public string PathFile{get; set;}
        
        public string UserName { get; set; }
        
        public string Password { get; set; }

        public bool IsInicializing { get; set; }

        #endregion

        #region FUNCTIONS

        public bool Login(string User, string Password)
        {
            return false;
        }

        public string GetFile()
        {
            return string.Empty;
        }

        public bool SaveFile(MemoryStream content)
        {
            return false;
        }

        public MemoryStream GetFileData()
        {
            return  new MemoryStream();
        }
        #endregion
    }
}
