// File: MediaItemJsonConverter.cs
// Namespace: NickgismokatoTracker.Backend.Data

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using NickgismokatoTracker.Backend.Data.Item;

namespace NickgismokatoTracker.Backend.Data{
    // To handle polymorphic (de)serialization of MediaItem subclasses
    public class MediaItemJsonConverter : JsonConverter<MediaItem>{
        public override MediaItem? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options){
            using(var jsonDoc = JsonDocument.ParseValue(ref reader)){
                var jsonObject = jsonDoc.RootElement;

                var type = jsonObject.GetProperty("Type").GetString();

                string title = jsonObject.GetProperty("Title").GetString() ?? "";
                string date = jsonObject.GetProperty("Date").GetString() ?? "";

                switch(type){
                    case "Movie":
                        return new MovieItem(title, date);

                    case "Series":
                        int season = jsonObject.TryGetProperty("Season", out var sProp) && sProp.TryGetInt32(out var sVal) ? sVal : -1;
                        int episode = jsonObject.TryGetProperty("Episode", out var eProp) && eProp.TryGetInt32(out var eVal) ? eVal : -1;
                        return new SeriesItem(title, date, season, episode);

                    case "Anime":
                        season = jsonObject.TryGetProperty("Season", out sProp) && sProp.TryGetInt32(out sVal) ? sVal : -1;
                        episode = jsonObject.TryGetProperty("Episode", out eProp) && eProp.TryGetInt32(out eVal) ? eVal : -1;
                        return new AnimeItem(title, date, season, episode);

                    default:
                        // Unknown type: fallback to MovieItem
                        return new MovieItem(title, date);
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, MediaItem value, JsonSerializerOptions options){
            writer.WriteStartObject();
            writer.WriteString("Title", value.Title);
            writer.WriteString("Type", value.Type);
            writer.WriteString("Date", value.Date);

            if(value is SeriesItem s){
                writer.WriteNumber("Season", s.Season);
                writer.WriteNumber("Episode", s.Episode);
            }else if(value is AnimeItem a){
                writer.WriteNumber("Season", a.Season);
                writer.WriteNumber("Episode", a.Episode);
            }
            writer.WriteEndObject();
        }
    }
}
