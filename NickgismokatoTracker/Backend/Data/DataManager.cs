// File: DataManager.cs
// Namespace: NickgismokatoTracker.Backend.Data

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using NickgismokatoTracker.Backend.Data.Item;
using NickgismokatoTracker.Backend.Util.Logging;


namespace NickgismokatoTracker.Backend.Data{
    public class DataManager{
        private readonly string _dataFilePath;

        private TrackerData _trackerData = new();

        public ObservableCollection<MediaItem> PlanningToWatch { get; private set; } = new();
        public ObservableCollection<MediaItem> Watching { get; private set; } = new();
        public ObservableCollection<MediaItem> Completed { get; private set; } = new();

        private readonly JsonSerializerOptions _jsonOptions = new(){
            WriteIndented = true,
            Converters = { new MediaItemJsonConverter() }
        };

        public DataManager(){
            _dataFilePath = Path.Combine(Util.PathHelper.GetAppDataFolder(), "NickgismokatoTracker.json");
        }

        public TrackerData TrackerData => _trackerData;

        public async Task LoadAsync(){
            try{
                await LoadFromFileAsync();

                // Sync ObservableCollections with loaded data
                SyncCollections();
            }catch(Exception ex){
                Logger.Instance.Error($"Failed loading data: {ex.Message}", true);
            }
        }

        private async Task LoadFromFileAsync(){
            if(!System.IO.File.Exists(_dataFilePath)){
                Logger.Instance.Info($"Data file not found at {_dataFilePath}, creating empty data.");
                _trackerData = new TrackerData();
                await SaveAsync();
                return;
            }

            using var stream = System.IO.File.OpenRead(_dataFilePath);
            var loaded = await System.Text.Json.JsonSerializer.DeserializeAsync<TrackerData>(stream, _jsonOptions);
            _trackerData = loaded ?? new TrackerData();

            Logger.Instance.Info($"Loaded data from {_dataFilePath}");
        }

        private void SyncCollections(){
            PlanningToWatch.Clear();
            foreach(var item in _trackerData.PlanningToWatch)
                PlanningToWatch.Add(item);

            Watching.Clear();
            foreach(var item in _trackerData.Watching)
                Watching.Add(item);

            Completed.Clear();
            foreach(var item in _trackerData.Completed)
                Completed.Add(item);
        }

        private void SyncTrackerDataFromCollections(){
            _trackerData.PlanningToWatch = PlanningToWatch.ToList();
            _trackerData.Watching = Watching.ToList();
            _trackerData.Completed = Completed.ToList();
        }

        public async Task AddItemAsync(MediaItem item, string category){
            switch(category){
                case "PlanningToWatch":
                    PlanningToWatch.Add(item);
                    break;
                case "Watching":
                    Watching.Add(item);
                    break;
                case "Completed":
                    Completed.Add(item);
                    break;
                default:
                    Logger.Instance.Warn($"Unknown category {category} in AddItemAsync");
                    return;
            }

            SyncTrackerDataFromCollections();
            await SaveAsync();
        }

        public async Task RemoveItemAsync(MediaItem item, string category){
            bool removed = false;
            switch(category){
                case "PlanningToWatch":
                    removed = PlanningToWatch.Remove(item);
                    break;
                case "Watching":
                    removed = Watching.Remove(item);
                    break;
                case "Completed":
                    removed = Completed.Remove(item);
                    break;
                default:
                    Logger.Instance.Warn($"Unknown category {category} in RemoveItemAsync");
                    return;
            }

            if(removed){
                SyncTrackerDataFromCollections();
                await SaveAsync();
            }else{
                Logger.Instance.Warn($"Item not found in {category} for removal");
            }
        }

        public async Task UpdateItemAsync(MediaItem item, string category){
            // Find index in the ObservableCollection, replace, then save
            ObservableCollection<MediaItem>? collection = category switch{
                "PlanningToWatch" => PlanningToWatch,
                "Watching" => Watching,
                "Completed" => Completed,
                _ => null
            };

            if(collection == null){
                Logger.Instance.Warn($"Unknown category {category} in UpdateItemAsync");
                return;
            }

            int index = -1;
            for(int i = 0; i < collection.Count; i++){
                if(collection[i] == item){
                    index = i;
                    break;
                }
            }

            if(index == -1){
                Logger.Instance.Warn($"Item not found in {category} for update");
                return;
            }

            collection[index] = item;
            SyncTrackerDataFromCollections();
            await SaveAsync();
        }
		// Fix for CS0120: Changed Logger.Info to Logger.Instance.Info to correctly reference the singleton instance of Logger.
		public async Task SaveAsync() {
			try {
				using(FileStream fs = new FileStream(_dataFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true)) {
					await JsonSerializer.SerializeAsync(fs, _trackerData, _jsonOptions);
				}
				Logger.Instance.Info("Data saved successfully."); // Corrected reference to Logger.Instance
			} catch(Exception ex) {
				Logger.Instance.Error("Failed to save data : " + ex, false); // Corrected reference to Logger.Instance
																   // Optionally rethrow or handle error display in UI as needed
			}
		}
	}
}