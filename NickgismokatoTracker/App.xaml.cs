using System;
using System.Configuration;
using System.Data;
using System.Windows;
using NickgismokatoTracker.Backend.Util.Logging;

namespace NickgismokatoTracker {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {
		protected override void OnStartup(StartupEventArgs e) {
			try {
				Logger.Instance.Info("Application starting up...");
				base.OnStartup(e);
			} catch(Exception ex) {
				Logger.Instance.Error($"Application startup failed: {ex.Message}", true);
				MessageBox.Show($"Failed to start application: {ex.Message}", "Startup Error",
					MessageBoxButton.OK, MessageBoxImage.Error);
				Shutdown();
			}
		}

		private void Application_DispatcherUnhandledException(object sender,
			System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
			Logger.Instance.Error($"Unhandled exception: {e.Exception.Message}", true);
			MessageBox.Show($"An unexpected error occurred: {e.Exception.Message}", "Error",
				MessageBoxButton.OK, MessageBoxImage.Error);
			e.Handled = true;
		}
	}
}