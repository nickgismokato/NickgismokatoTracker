// File: Logger.cs
// Namespace: NickgismokatoTracker.Backend.Util.Logging

using System;
using System.IO;
using System.Threading;

namespace NickgismokatoTracker.Backend.Util.Logging{
    public sealed class Logger{
        private static readonly Lazy<Logger> _instance = new(() => new Logger());
        private static readonly object _lockObj = new();

        private readonly string _logFilePath;

        private Logger(){
            _logFilePath = Path.Combine(Util.PathHelper.GetAppDataFolder(), "NickgismokatoTracker.log");
            Directory.CreateDirectory(Path.GetDirectoryName(_logFilePath) ?? "");
        }

        public static Logger Instance => _instance.Value;

        public void Log(LogLevel level, string message, bool showPopup = false){
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";

            lock(_lockObj){
                try{
                    File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
                }catch{
                    // silently fail on logging errors
                }
            }

            if(level == LogLevel.ERROR && showPopup){
                System.Windows.MessageBox.Show(message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public void Info(string message) => Log(LogLevel.INFO, message);
        public void Debug(string message) => Log(LogLevel.DEBUG, message);
        public void Warn(string message) => Log(LogLevel.WARN, message);
        public void Error(string message, bool showPopup = false) => Log(LogLevel.ERROR, message, showPopup);
    }
}
