using System.IO;
using System.Net;

namespace NetCoreFileAccess.SourceAccess
{
    public class FtpAccess : BaseAccess, ISourceAccess
    {
        #region PROPERTIES

        string URI { get; set; }
        string FTPUserName { get; set; }
        string FTPPassword { get; set; }
        #endregion

        #region CONSTRUCTORS
        public FtpAccess()
        {            
            URI = string.Empty;
            FTPUserName = string.Empty;
            FTPPassword = string.Empty;

        }
        #endregion

        #region OVERRIDE METHODS

        public override bool TryLogin(params object[] Options)
        {
            // For FTP, we expect Options to contain the FTP credentials (username and password, ...).
            this.URI = Options != null && Options.Length > 0 && Options[1] is string uri ? uri : string.Empty;
            this.FTPUserName = Options != null && Options.Length > 0 && Options[2] is string username ? username : string.Empty;
            this.FTPPassword = Options != null && Options.Length > 0 && Options[3] is string password ? password : string.Empty;
            this.PathFile = Options != null && Options.Length > 0 && Options[4] is string PathFile ? PathFile : string.Empty;
            if (Connect())
            {
                // try to loging with provided credentials,
                // using the base TryLogin to show the login window if needed and validate the credentials pattern
                object PatternObj = Options != null && Options.Length > 0 ? Options[0] : string.Empty;
                return base.TryLogin(PatternObj);
            }
            return false;
        }

        protected override bool Login(string User, string Password)
        {
            if (IsInicializing)
            {
                IsInicializing = false;
                this.UserName = User;
                this.Password = Password;
                return true;
            }
            else
            {
                //try to open existing file with provided credentials
                using (MemoryStream ms = GetFileData())
                {
                    return CredentialsUtils.ValidateCredentials(ms, User, Password);                    
                }
            }            
        }

        #endregion

        /// <summary>
        /// Attempts a simple FTP connection / directory listing to validate the endpoint and credentials.
        /// Sets IsInicializing = false on success.
        /// </summary>
        private bool Connect()
        {
            if (string.IsNullOrWhiteSpace(PathFile))
                return false;
        
            if (!TryCreateUri(out var uri))
                return false;

            try
            {
                var request = (FtpWebRequest)WebRequest.Create(uri);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(this.FTPUserName, this.FTPPassword);
                request.UsePassive = true;
                request.UseBinary = true;
                request.KeepAlive = false;
                request.Timeout = 15000;

                using var response = (FtpWebResponse)request.GetResponse();
                // If we got a response without exception, connection is good                
                return true;
            }
            catch
            {
                // swallow or log as appropriate for your project
                return false;
            }
        }

        private bool TryCreateUri(out Uri uri)
        {
            uri = null!;
            try
            {
                return Uri.TryCreate(this.URI, UriKind.Absolute, out uri);
            }
            catch
            {
                uri = null!;
                return false;
            }
        }

        /// <summary>
        /// Save to a specific file name within the PathFile (which may be a directory or base URI).
        /// </summary>
        public override bool SaveFile(MemoryStream content)
        {
            if (content == null || string.IsNullOrWhiteSpace(this.PathFile))
                return false;

            if (!TryCreateUri(out var baseUri))
                return false;

            Uri targetUri = new Uri(baseUri, this.PathFile);

            try
            {
                var request = (FtpWebRequest)WebRequest.Create(targetUri);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(this.FTPUserName, this.FTPPassword);
                request.UsePassive = true;
                request.UseBinary = true;
                request.KeepAlive = false;
                request.Timeout = 30000;

                // ensure stream position at start
                if (content.CanSeek) content.Position = 0;

                using (var requestStream = request.GetRequestStream())
                {
                    content.CopyTo(requestStream);
                }

                using var response = (FtpWebResponse)request.GetResponse();
                return response.StatusCode == FtpStatusCode.ClosingData || response.StatusCode == FtpStatusCode.CommandOK || response.StatusCode == FtpStatusCode.CommandOK;
            }
            catch
            {
                // swallow or log as appropriate
                return false;
            }
        }

        /// <summary>
        /// Downloads the file indicated by PathFile (can be a full URI to a file).
        /// </summary>
        public override MemoryStream GetFileData()
        {
            var ms = new MemoryStream();

            if (string.IsNullOrWhiteSpace(this.PathFile))
                return ms;

            if (!TryCreateUri(out var baseUri))
                return ms;

            Uri targetUri = new Uri(baseUri, this.PathFile);

            try
            {
                var request = (FtpWebRequest)WebRequest.Create(targetUri);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(this.FTPUserName, this.FTPPassword);
                request.UsePassive = true;
                request.UseBinary = true;
                request.KeepAlive = false;
                request.Timeout = 30000;
                
                // Read the file from the server &write to destination                
                using var response = (FtpWebResponse)request.GetResponse();
                using var responseStream = response.GetResponseStream();
                if (responseStream != null)
                {
                    responseStream.CopyTo(ms);
                    if (ms.CanSeek) ms.Position = 0;
                    
                    return ms;
                }
            }
            catch
            {
                // swallow or log as appropriate
                return ms;
            }

            return ms;
        }

    }
}
