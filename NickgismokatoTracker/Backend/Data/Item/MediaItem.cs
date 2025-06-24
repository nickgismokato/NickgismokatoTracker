// File: MediaItems.cs
// Namespace: NickgismokatoTracker.Backend.Data

using System;
using System.Text.Json.Serialization;

namespace NickgismokatoTracker.Backend.Data.Item{
    public abstract class MediaItem{
        public string Title { get; set; }
        public string Type { get; set; }
        public string Date { get; set; } // Format "dd/MM-yyyy"

        protected MediaItem(string title, string type, string date){
            Title = title;
            Type = type;
            Date = date;
        }
    }
}
