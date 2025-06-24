using System;
using System.Text.Json.Serialization;

namespace NickgismokatoTracker.Backend.Data.Item{
    public class SeriesItem : MediaItem{
        public int Season { get; set; } = -1;
        public int Episode { get; set; } = -1;

        public SeriesItem(string title, string date, int season = -1, int episode = -1) : base(title, "Series", date){
            Season = season;
            Episode = episode;
        }
    }
}
