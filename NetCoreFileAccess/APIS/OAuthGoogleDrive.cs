using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCoreFileAccess.APIS
{
    public class OAuthGoogleDrive
    {
        #region FIELDS
        string _ApplicationName;


        #endregion

        #region PROPERTIES

        public DriveService Service { get; }

        #endregion

        #region CONSTRUCTOR
        public OAuthGoogleDrive(string clientID, string clientSecret, string[] scopes, string credentialPath, string AppName)
        {
            _ApplicationName = AppName;
            Service = Authenticate(clientID, clientSecret, scopes, credentialPath);
            // check if the authentication was successful and the service is not null

            Service = Service ?? throw new Exception("Failed to authenticate with Google Drive API. Service is null.");
        }

        #endregion

        #region PRIVATE METHODS
        private DriveService Authenticate(string clientId, string clientSecret, string[] scopes, string credPath)
        {
            // Create OAuth 2.0 authorization flow
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                },
                Scopes = scopes,
                DataStore = new FileDataStore(credPath, true),
            });

            // Authorize the application
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                flow.ClientSecrets,
                scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true)).Result;
            
            // Create Drive service
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _ApplicationName,
            });

            return service;
        }

        #endregion
    }
}
