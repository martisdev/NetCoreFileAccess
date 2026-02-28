using System.IO;
using System.Text.Json;
using System.Windows.Navigation;

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

        #region FIELDS 

        private readonly ISourceAccess _sourceAccess;

        private static SourceType sourceType;

        #endregion

        #region PROPERTIES
        
        public bool IsInicializing 
        {
            get { return _sourceAccess.IsInicializing; }            
        }

        public string UserName 
        {
            get { return _sourceAccess.UserName; }
        }

        public string Password 
        {
            get {return _sourceAccess.Password; }
        }

        #endregion

        #region CONSTRUCTORS

        public SourceAccess(ISourceAccess sourceAccess)
        {
            _sourceAccess = sourceAccess;            
        }

        #endregion
        
        #region PUBLIC FUNTIONS
      
        public bool TryLogin(params object[] Options)
        {
            return _sourceAccess.TryLogin(Options);
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

    }

}
