using System.IO;
using System.Net;

namespace NetCoreFileAccess.SourceAccess
{
    public class FtpAccess : BaseAccess, ISourceAccess
    {
        #region MyRegion
        private static int DEF_PORT = 21;        

        private static string PROTOCOL_FTP = "FTP://";

        #endregion

        #region FIELDS

        private string URI = string.Empty;
        
        private int Port = DEF_PORT;
        
        private string FTPUserName = string.Empty;  

        private string FTPPassword = string.Empty;
        
        #endregion

        #region CONSTRUCTORS
        public FtpAccess(string clientApp)
        {
            this.ClientAPP = clientApp;
            URI = string.Empty;
            FTPUserName = string.Empty;
            FTPPassword = string.Empty;

        }
        #endregion

        #region OVERRIDE METHODS
        /// <summary>
        /// Attempts to log in to the FTP server using the provided credentials and connection options.
        /// </summary>
        /// <remarks>This method overrides the base login behavior to support FTP-specific connection
        /// parameters. The login attempt will fail if the connection to the FTP server cannot be established or if the
        /// provided credentials are invalid.</remarks>
        /// <param name="Options">An array of objects containing connection parameters in the following order: <list type="number">
        /// <item><description>Pattern object used for credential validation or login prompt
        /// customization.</description></item> <item><description>FTP server URI as a <see
        /// cref="string"/>.</description></item> <item><description>FTP username as a <see
        /// cref="string"/>.</description></item> <item><description>FTP password as a <see
        /// cref="string"/>.</description></item> <item><description>Remote file path as a <see cref="string"/>
        /// (optional).</description></item> </list> Each parameter must be supplied in the correct position and type.
        /// If any required parameter is missing or invalid, the login attempt may fail.</param>
        /// <returns><see langword="true"/> if the login is successful; otherwise, <see langword="false"/>.</returns>
        public override bool TryLogin(params object[] Options)
        {
            bool OKConnection = false;
            using (ProgressInfo progress = new ProgressInfo("Connecting ..."))
            {
                progress.Show();
                progress.UpdateMessage("Connecting to FTP server...");
                // For FTP, we expect Options to contain the FTP credentials (username and password, ...).
                this.URI = Options != null && Options.Length > 0 && Options[1] is string uri ? uri : string.Empty;
                this.Port = Options != null && Options.Length > 0 && Options[2] is int port ? port : DEF_PORT;
                this.FTPUserName = Options != null && Options.Length > 0 && Options[3] is string username ? username : string.Empty;
                this.FTPPassword = Options != null && Options.Length > 0 && Options[4] is string password ? password : string.Empty;
                this.PathFile = Options != null && Options.Length > 0 && Options[5] is string PathFile ? PathFile : string.Empty;

                OKConnection = Connect();
                if (!OKConnection)
                    progress.ErrorMessage("Failed to connect to FTP server. Please check the URI and credentials.");                
            }
            if( OKConnection)
            {
                // try to loging with provided credentials,
                // using the base TryLogin to show the login window if needed and validate the credentials pattern
                object PatternObj = Options != null && Options.Length > 0 ? Options[0] : string.Empty;
                return base.TryLogin(PatternObj);
            }

            return false;
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

            if(baseUri == null)
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
            
            if(baseUri == null)
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

        #endregion

        #region PRIVETE METHODS

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
            
            if (uri == null)
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

        /// <summary>
        /// Create the correct URI to connect to the FTP server, based on the provided URI and PathFile.
        /// </summary>
        /// <param name="uri">URI to return</param>
        /// <returns>true id succefull, false otherwise</returns>
        private bool TryCreateUri(out Uri? uri)
        {
            uri = null!;
            try
            {
                string UriPort = string.Format("{0}:{1}", this.URI, this.Port);
                if(!UriPort.StartsWith(PROTOCOL_FTP))
                    UriPort = string.Format("{0}{1}", PROTOCOL_FTP, UriPort);
                
                 return Uri.TryCreate(UriPort, UriKind.Absolute, out uri);
            }
            catch
            {
                uri = null!;
                return false;
            }
        }

        #endregion

    }
}
