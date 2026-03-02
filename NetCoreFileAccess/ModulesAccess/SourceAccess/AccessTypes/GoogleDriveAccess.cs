using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using NetCoreFileAccess.APIS;
using System.IO;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NetCoreFileAccess.SourceAccess
{
    public class GoogleDriveAccess : BaseAccess, ISourceAccess
    {
        #region CONSTS
        
        private readonly string[] Scopes = { DriveService.Scope.DriveFile };

        #endregion

        #region FIELDS
        private OAuthGoogleDrive? _oAuthGoogleDrive = null;
        private string _ApplicationName = string.Empty ;
        
        private string _Client_id = string.Empty;

        private string _Client_secret = string.Empty;

        private string _CredentialFile = string.Empty;
        #endregion


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


        public override bool TryLogin(params object[] Options)
        {
            this.PathFile = Options != null && Options.Length > 0 && Options[1] is string _PathFile ? _PathFile : string.Empty;
            this._CredentialFile = Options != null && Options.Length > 0 && Options[2] is string _CredentialFile ? _CredentialFile : string.Empty;                        
            this._Client_id = Options != null && Options.Length > 0 && Options[3] is string _ClientId ? _ClientId : string.Empty;
            this._Client_secret = Options != null && Options.Length > 0 && Options[4] is string _ClientSecret ? _ClientSecret : string.Empty;
            this._ApplicationName = Options != null && Options.Length > 0 && Options[5] is string _AppName ? _AppName : string.Empty;


            if (Connect())
            {
                // try to loging with provided credentials,
                // using the base TryLogin to show the login window if needed and validate the credentials pattern
                object PatternObj = Options != null && Options.Length > 0 ? Options[0] : string.Empty;
                return base.TryLogin(PatternObj);
            }
            return false;
        }

        public override bool SaveFile(MemoryStream content)
        {
            return false;
        }
        public override MemoryStream GetFileData()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if the connection is successful, otherwise return false</returns>
        private bool Connect()
        {
            // Here you would implement the logic to connect to Google Drive using the Google Drive API.
            // This typically involves authenticating with OAuth 2.0 and obtaining an access token.
            try
            {
                _oAuthGoogleDrive = new OAuthGoogleDrive(_Client_id, _Client_secret, Scopes, _CredentialFile, _ApplicationName);
                
                // Define the request parameters.
                FilesResource.ListRequest listRequest = _oAuthGoogleDrive.Service.Files.List();
                listRequest.PageSize = 10;
                listRequest.Fields = "nextPageToken, files(id, name)";

                // List of files.
                IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;
                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        Console.WriteLine("{0} ({1})", file.Name, file.Id);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }         
        }

    }
}
