using System;
using System.Text.Json.Serialization;

namespace NickgismokatoTracker.Backend.Data.Item{
	// Fix for CS7036: Add a constructor to MovieItem to match the usage in MainWindow.xaml.cs  
	public class MovieItem : MediaItem {
		public MovieItem(string title, string date) : base(title, "Movie", date) {
		}
	}
}
