using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace PUBG_MAPHACK
{
    class ReplayMonitor
    {
        FileSystemWatcher watcher;
        public GoogleDriveUploader GoogleDriveUploader = new GoogleDriveUploader();

        public ReplayMonitor()
        {
            // Generate new google drive token (saved in token.json)
            // GoogleDriveUploader.generateNewToken();

            // Lets watch the pubg replay directory for filesystem changes
            if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\TslGame\Saved\Demos")) 
            {
                watcher = new FileSystemWatcher();
                watcher.Path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\TslGame\Saved\Demos";
                watcher.NotifyFilter = NotifyFilters.DirectoryName;
                watcher.Filter = "*.*";
                watcher.Created += new FileSystemEventHandler(OnNewReplayCreation);
                watcher.EnableRaisingEvents = true;
            }
        }

        private void OnNewReplayCreation(object source, FileSystemEventArgs e)
        {
            if (Directory.Exists(e.FullPath))
            {
                /* Should probably make sure files are done being written to disc before starting
                 * upload but i'm lazy to handle that so let's just wait 5 seconds and hope it's done */
                Thread.Sleep(5000);

                Program.debug_log("Replay was created - Attemping upload now...");

                // Let's upload newly created replay to google drive
                var t = Task.Run(() => GoogleDriveUploader.UploadFile(e.FullPath));
            }
        }

    }
}
