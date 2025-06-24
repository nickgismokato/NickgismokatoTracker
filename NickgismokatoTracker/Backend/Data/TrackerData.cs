// File: TrackerData.cs
// Namespace: NickgismokatoTracker.Backend.Data

using System.Collections.Generic;
using System.Text.Json.Serialization;

using NickgismokatoTracker.Backend.Data.Item;

namespace NickgismokatoTracker.Backend.Data{
    public class TrackerData{
        [JsonPropertyName("PlanningToWatch")]
        public List<MediaItem> PlanningToWatch { get; set; } = new();

        [JsonPropertyName("Watching")]
        public List<MediaItem> Watching { get; set; } = new();

        [JsonPropertyName("Completed")]
        public List<MediaItem> Completed { get; set; } = new();
    }
}
