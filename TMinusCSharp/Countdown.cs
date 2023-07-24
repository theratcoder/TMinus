using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using Windows.Storage;
using Windows.UI.WindowManagement;

namespace TMinusCSharp {
    internal class Countdown {
        public static int nextId = 0;
        public static int nextFileId = 0;
        public static Dictionary<int, Countdown> countdowns = new Dictionary<int, Countdown>();

        public int id;
        public string title;
        public DateTimeOffset time;

        public AppWindow window;

        public int fileId;

        public delegate void OnDelete();
        public OnDelete delete;

        public Countdown(string tt, DateTimeOffset dt) {
            title = tt;
            time = dt;

            id = nextId++;
            fileId = nextFileId++;

            delete += async delegate {
                // check if the window has already been closed
                if (window != null) {
                    await window.CloseAsync();
                }
            };
        }

        public Countdown(CountdownInfo info, int fId) {
            title = info.title;
            time = info.dateTime;

            id = nextId++;

            fileId = fId;
            if (fileId >= nextFileId) {
                nextFileId = fileId + 1;
            }

            delete += async delegate {
                if (window != null) {
                    await window.CloseAsync();
                }
                deleteFile();
            };
        }

        public string displayRemaining() {
            TimeSpan remaining = time - DateTimeOffset.Now;
            if (remaining <= new TimeSpan(0, 0, 0)) {
                return "00:00:00";
            }
            else if (remaining.Days != 0) {
                string s1 = remaining.Days == 1 ? "" : "s";
                string s2 = remaining.Hours == 1 ? "" : "s";
                string s3 = remaining.Minutes == 1 ? "" : "s";

                return $"{remaining.Days} day{s1}, {remaining.Hours} hour{s2}, {remaining.Minutes} minute{s3}";
            }
            else {
                return $"{remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
            }
        }

        public async void save() {
            CountdownInfo info = new CountdownInfo(this);
            string json = JsonConvert.SerializeObject(info);

            Windows.Storage.StorageFolder folder;
            try {
                folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("countdowns");
            }
            catch (FileNotFoundException) {
                folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("countdowns");
            }

            try {
                Windows.Storage.StorageFile file = await folder.CreateFileAsync($"{fileId}.ctdn", CreationCollisionOption.ReplaceExisting);
                await Windows.Storage.FileIO.WriteTextAsync(file, json);
            }
            catch (Exception) { } // sometimes throws a straitforward exception in which case we can just give up
        }

        public async void deleteFile() {
            try {
                StorageFolder folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("countdowns");
                StorageFile file = await folder.GetFileAsync($"{fileId}.ctdn");
                await file.DeleteAsync();
            }
            catch (FileNotFoundException) { } // file doesn't exist so don't bother
        }
    }
}
