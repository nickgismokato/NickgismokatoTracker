using System;
using System.Text.Json.Serialization;

namespace NickgismokatoTracker.Backend.Data.Item{
    public class AnimeItem : MediaItem{
        public int Season { get; set; } = -1;
        public int Episode { get; set; } = -1;

        public AnimeItem(string title, string date, int season = -1, int episode = -1) : base(title, "Anime", date){
            Season = season;
            Episode = episode;
        }
    }
}
