using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Upload;
using NetCoreFileAccess.APIS;
using System.IO;

namespace NetCoreFileAccess.SourceAccess
{
    public class GoogleDriveAccess : BaseAccess, ISourceAccess
    {
        #region CONSTS
        
        private readonly string[] Scopes = { DriveService.Scope.Drive };

        #endregion

        #region FIELDS
        
        private OAuthGoogleDrive? _oAuthGoogleDrive = null;
        
        private string _ApplicationName = string.Empty ;
        
        private string _Client_id = string.Empty;

        private string _Client_secret = string.Empty;

        private string _CredentialFile = string.Empty;
        
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
            if (_oAuthGoogleDrive == null || _oAuthGoogleDrive.Service == null)
                return false;

            if (content == null)
                return false;

            // Determine filename to use in Drive
            string fileName;
            if (!string.IsNullOrEmpty(this.PathFile))
            {
                if (Uri.TryCreate(this.PathFile, UriKind.Absolute, out var uri))
                    fileName = Path.GetFileName(uri.LocalPath);
                else
                    fileName = Path.GetFileName(this.PathFile);
            }
            else
            {
                // Create a default filename if PathFile is not provided
                fileName = "securebunker.fscr";
            }

            if (string.IsNullOrEmpty(fileName))
                fileName = "securebunker.fscr";

            try
            {
                if (content.CanSeek) content.Position = 0;

                var service = _oAuthGoogleDrive.Service;
                // Check if a file with the same name already exists
                string? existingId = GetFileIdByName(fileName);

                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = fileName
                };

                IUploadProgress progress;
                if (!string.IsNullOrEmpty(existingId))
                {
                    // Update existing file
                    var updateRequest = service.Files.Update(fileMetadata, existingId, content, "application/octet-stream");
                    updateRequest.Fields = "id, name";
                    progress = updateRequest.Upload();
                }
                else
                {
                    // Create new file
                    var createRequest = service.Files.Create(fileMetadata, content, "application/octet-stream");
                    createRequest.Fields = "id, name";
                    progress = createRequest.Upload();
                }

                return progress != null && progress.Status == UploadStatus.Completed;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (content.CanSeek) content.Position = 0;
            }
        }

        public override MemoryStream GetFileData()
        {
            if(_oAuthGoogleDrive == null)
                return new MemoryStream();
            string fileId = GetFileID();

            if(string.IsNullOrEmpty(fileId))
                return new MemoryStream();

            // Define the request parameters.
            var Request = _oAuthGoogleDrive.Service.Files.Get(fileId);
            var memoryStream = new MemoryStream();

            Request.MediaDownloader.ProgressChanged +=
                    progress =>
                    {
                        switch (progress.Status)
                        {
                            case DownloadStatus.Downloading:
                                {
                                    Console.WriteLine(progress.BytesDownloaded);
                                    break;
                                }
                            case DownloadStatus.Completed:
                                {
                                    Console.WriteLine("Download complete.");
                                    break;
                                }
                            case DownloadStatus.Failed:
                                {
                                    Console.WriteLine("Download failed.");
                                    break;
                                }
                        }
                    };
            Request.Download(memoryStream);

            return memoryStream;
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
                
                return true;
            }
            catch{return false;}         
        }

        private string GetFileID()
        {            
            try
            {
                // If PathFile contains a full path or URL, use the filename portion as the search name
                string searchName = string.Empty;
                if (!string.IsNullOrEmpty(this.PathFile))
                {
                    if (Uri.TryCreate(this.PathFile, UriKind.Absolute, out var uri))
                        searchName = Path.GetFileName(uri.LocalPath);
                    else
                        searchName = Path.GetFileName(this.PathFile);
                }

                if (string.IsNullOrEmpty(searchName))
                    return string.Empty;

                return GetFileIdByName(searchName) ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Searches Google Drive for a file by name and returns its file id.
        /// Returns null if not found or on error.
        /// </summary>
        private string? GetFileIdByName(string fileName)
        {
            if (_oAuthGoogleDrive == null || _oAuthGoogleDrive.Service == null)
                return null;

            try
            {
                var service = _oAuthGoogleDrive.Service;
                string escapedName = EscapeForQuery(fileName);

                var request = service.Files.List();
                // Query: exact name match and not trashed
                request.Q = $"name = '{escapedName}' and trashed = false";
                request.Fields = "nextPageToken, files(id, name)";
                request.PageSize = 100;

                IList<Google.Apis.Drive.v3.Data.File> found = new System.Collections.Generic.List<Google.Apis.Drive.v3.Data.File>();
                string? pageToken = null;
                do
                {
                    request.PageToken = pageToken;
                    var result = request.Execute();
                    if (result.Files != null && result.Files.Count > 0)
                        foreach (var f in result.Files) found.Add(f);

                    pageToken = result.NextPageToken;
                } while (!string.IsNullOrEmpty(pageToken));

                if (found.Count == 0)
                    return null;

                // Prefer exact case-insensitive match
                var exact = found.FirstOrDefault(f => string.Equals(f.Name, fileName, StringComparison.OrdinalIgnoreCase));
                if (exact != null)
                    return exact.Id;

                // Otherwise return the first found
                return found[0].Id;
            }
            catch
            {
                return null;
            }
        }

        private string EscapeForQuery(string input)
        {
            if (input == null) return string.Empty;
            // escape single quotes by backslash as required by Drive query syntax
            return input.Replace("'", "\\'");
        }
    }    
}
