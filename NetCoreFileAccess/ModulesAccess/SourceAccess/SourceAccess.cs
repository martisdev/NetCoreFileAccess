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
            if(sourceAccess is GoogleDriveAccess)
                _sourceAccess.SourceType =   SourceType.GoogleDrive;
            else if (sourceAccess is LocalAccess)
                _sourceAccess.SourceType = SourceType.Local;
             else if (sourceAccess is FtpAccess)
                _sourceAccess.SourceType = SourceType.Ftp;
             else
                _sourceAccess.SourceType = SourceType.None;
        }

        #endregion
        
        #region PUBLIC METHODS
      
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

        public List<string> GetSources()
        {
            return Enum.GetValues(typeof(SourceType))
                .Cast<SourceType>()
                .Where(s => s != SourceType.None)
                .Select(s => s.ToString())
                .ToList();
        }
        #endregion

    }

}
