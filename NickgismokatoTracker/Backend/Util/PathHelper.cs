// File: PathHelper.cs
// Namespace: NickgismokatoTracker.Backend.Util

using System;
using System.IO;

namespace NickgismokatoTracker.Backend.Util{
    public static class PathHelper{
        public static string GetAppDataFolder(){
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string folderPath = Path.Combine(appData, "NickgismokatoTracker");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            return folderPath;
        }
    }
}
