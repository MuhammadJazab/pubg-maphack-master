using System;
using System.IO;
using System.Threading;
using System.IO.Compression;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace PUBG_MAPHACK
{
    class GoogleDriveUploader
    {
        // If modifying these scopes, remember to generate new token
        static string[] Scopes = { DriveService.Scope.DriveFile };
        // ClientId & ClientSecret needs to be created at google developer console
        static readonly ClientSecrets secrets = new ClientSecrets()
        {
            ClientId = "",
            ClientSecret = ""
        };
        // Refresh token is generate by generateNewToken(); see line 41
        public string refreshToken = "";

        public GoogleDriveUploader()
        {
            if (refreshToken == "" || secrets.ClientId == "" || secrets.ClientSecret == "")
            {
                System.Windows.Forms.MessageBox.Show("Google drive uploading is disabled - you need to create ClientId and ClientSecret in Google Developer Console. Check my GoogleDriveUploader class for more info.", "Developer helper", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
            }
        }

        public void UploadFile(string path)
        {
            if (refreshToken == "" || secrets.ClientId == "" || secrets.ClientSecret == "")
            {
                return;
            }

            // Generate new google drive token (saved in token.json)
            // Uncomment following line to generate new credentials for a google drive account (remember to comment out the predefined refresh token on line 31 first)
            // UserCredential credential =  generateNewToken();

            // Authorize with predefined RefreshToken (RefreshTokens never expire on it's own)
            UserCredential credential = AuthorizeWithRefreshToken(refreshToken);

            // Zip directory before uploading to google drive
            string zipFile = ZipDirectory(path);

            // Make sure zip was successful before proceeding
            if (!File.Exists(zipFile))
            {
                return;
            }

            Program.debug_log("Replay zipped and ready for upload");

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "PUBG REPLAY UPLOADER",
            });

            // File information for google drive
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(path) + ".zip",
                MimeType = "application/zip, application/octet-stream, application/x-zip-compressed, multipart/x-zip"
            };

            FilesResource.CreateMediaUpload request;

            Program.debug_log("Uploading replay");

            // Do the actual file upload to google drive
            using (var stream = new System.IO.FileStream(zipFile, System.IO.FileMode.Open))
            {
                request = service.Files.Create(fileMetadata, stream, "application/zip");
                request.Fields = "id";
                request.Upload();
            }

            // Recieve the response from google drive upload
            var file = request.ResponseBody;

            if(file.Id.Length > 0)
            {
                Program.debug_log("Upload complete");
            } else
            {
                Program.debug_log("Upload failed");
            }

            // Cleanup after upload
            if (File.Exists(zipFile))
            {
                File.Delete(zipFile);  // Delete zip file
            }
        }

        public UserCredential generateNewToken()
        {
            UserCredential credential;

            // Delete existing token directory (saved where program is run from)
            if (Directory.Exists("token.json"))
            {
                Directory.Delete("token.json", true);
            }

            // Generate new credentials (will open google drive login in browser)
            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new GoogleAuthorizationCodeFlow.Initializer { ClientSecrets = secrets },
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore("token.json", true)).Result;

            // Return credentials after signin
            return credential;
        }

        private UserCredential AuthorizeWithRefreshToken(string token)
        {
            UserCredential credential;

            // Get existing credentials using RefreshToken (can be found inside token.json after generating new token)
            credential = new UserCredential(
                new GoogleAuthorizationCodeFlow(
                    new GoogleAuthorizationCodeFlow.Initializer { ClientSecrets = secrets }
                ),
                "user",
                new TokenResponse
                {
                    RefreshToken = token
                });

            // Return credentials
            return credential;
        }

        private string ZipDirectory(string dirPath)
        {
            string zipPath = Path.GetTempPath() + Path.GetFileName(dirPath) + ".zip";

            if (!File.Exists(zipPath) && Directory.Exists(dirPath))
            {
                // Create log file before zipping that shows if any funny stuff happened during match
                string logPath = dirPath + @"\log.txt";
                if (!File.Exists(logPath))
                {
                    // Create a file to write to.
                    using (StreamWriter sw = File.CreateText(logPath))
                    {
                        sw.WriteLine(Program.triggerTracker);
                    }
                }
                Program.triggerTracker = "";
                // Zip Directory
                ZipFile.CreateFromDirectory(dirPath, zipPath);
            }

            return zipPath;
        }
    }
}
