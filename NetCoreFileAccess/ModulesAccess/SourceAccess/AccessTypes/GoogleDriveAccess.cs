using System.IO;

namespace NetCoreFileAccess.SourceAccess
{
    public class GoogleDriveAccess : BaseAccess, ISourceAccess
    {
        #region PROPERTIES        

        #endregion

        #region CONSTRUCTORS   
        public GoogleDriveAccess()
        {
            PathFile = string.Empty;
            UserName = string.Empty;
            Password = string.Empty;
        }
        #endregion


        public override bool SaveFile(MemoryStream content)
        {
            return false;
        }
        public override MemoryStream GetFileData()
        {
            throw new NotImplementedException();
        }
    }
}
