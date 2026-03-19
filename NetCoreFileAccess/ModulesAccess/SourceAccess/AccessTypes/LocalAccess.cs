using System.Diagnostics.Eventing.Reader;
using System.IO;

namespace NetCoreFileAccess.SourceAccess
{
    public class LocalAccess: BaseAccess, ISourceAccess
    {
        #region CONSTANTS
        
        private const string FILE_EXTENSION = ".fscr";
        #endregion

        #region CONSTRUCTORS
        public LocalAccess(string clientApp)
        {
            this.SourceType = SourceType.Local;
            this.ClientAPP = clientApp;
            this.PathFile = GetPathDataFile();
        }
        #endregion

        #region OVERRIDE METHODS

        /// <summary>
        /// Attempts to log in using the specified options.
        /// </summary>
        /// <remarks>This method overrides <see cref="base.TryLogin"/> and delegates the login attempt to
        /// the base implementation. Refer to the documentation of the base class for details on supported options and
        /// expected behavior.</remarks>
        /// <param name="Options">An array of option values used to configure the login attempt. The meaning and required types of the options
        /// depend on the implementation.</param>
        /// <returns><see langword="true"/> if the login attempt is successful; otherwise, <see langword="false"/>.</returns>
        public override bool TryLogin(params object[] Options)
        {            
            return base.TryLogin(Options);
        }

        /// <summary>
        /// Save to a specific file name within the PathFile (which may be a directory or base URI).
        /// </summary>
        public override bool SaveFile(MemoryStream content)
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

        /// <summary>
        /// Downloads the file indicated by PathFile (can be a full URI to a file).
        /// </summary>
        public override MemoryStream GetFileData()
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

        /// <summary>
        /// Obtain the path of the data file to be used for saving and loading data. If the file does not exist, it creates a new one with a random name. If the file exists but is empty, it sets the IsInicializing flag to true to indicate that the source access is being initialized for the first time.
        /// </summary>
        /// <returns></returns>
        private string GetPathDataFile()
        {
            string path = Path.Combine(Environment.GetFolderPath( Environment.SpecialFolder.Personal) , this.ClientAPP);
            
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
            }
            else
            {
                FileInfo fileInfo = new FileInfo(_File);
                if (fileInfo.Length == 0)
                {                    
                    IsInicializing = true;  //file is empty                  
                }
                else
                    IsInicializing = false; //file exist and is not empty
            }
            
            return _File;
        }
        
        #endregion
    }
}
