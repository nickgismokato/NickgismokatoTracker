// File: MainWindow.xaml.cs
// Namespace: NickgismokatoTracker.Frontend.UI

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NickgismokatoTracker.Backend.Data;
using NickgismokatoTracker.Backend.Data.Item;
using NickgismokatoTracker.Backend.Util.Logging;

namespace NickgismokatoTracker.Frontend.UI {
	public partial class MainWindow : Window {
		private readonly DataManager _dataManager = new();

		// Filtered collections for watching sub-tabs
		private ObservableCollection<MediaItem> _watchingMovies = new();
		private ObservableCollection<MediaItem> _watchingSeries = new();
		private ObservableCollection<MediaItem> _watchingAnime = new();

		private MediaItem? _selectedItem;
		private string _selectedCategory = "";
		private bool _isEditMode = false;

		public MainWindow() {
			InitializeComponent();

			Loaded += async (_, _) => {
				await _dataManager.LoadAsync();
				BindDataGrids();
				UpdateTotalItemsCount();
				Logger.Instance.Info("MainWindow loaded successfully");
			};
		}

		private void BindDataGrids() {
			// Bind main collections
			PlanningToWatchGrid.ItemsSource = _dataManager.PlanningToWatch;
			CompletedGrid.ItemsSource = _dataManager.Completed;

			// Filter and bind watching sub-collections
			UpdateWatchingCollections();
			WatchingMoviesGrid.ItemsSource = _watchingMovies;
			WatchingSeriesGrid.ItemsSource = _watchingSeries;
			WatchingAnimeGrid.ItemsSource = _watchingAnime;

			// Subscribe to collection change events to update filtering
			_dataManager.Watching.CollectionChanged += (_, _) => UpdateWatchingCollections();
			_dataManager.PlanningToWatch.CollectionChanged += (_, _) => UpdateTotalItemsCount();
			_dataManager.Completed.CollectionChanged += (_, _) => UpdateTotalItemsCount();
		}

		private void UpdateWatchingCollections() {
			_watchingMovies.Clear();
			_watchingSeries.Clear();
			_watchingAnime.Clear();

			foreach(var item in _dataManager.Watching) {
				switch(item.Type) {
					case "Movie":
						_watchingMovies.Add(item);
						break;
					case "Series":
						_watchingSeries.Add(item);
						break;
					case "Anime":
						_watchingAnime.Add(item);
						break;
				}
			}

			UpdateTotalItemsCount();
		}

		private void UpdateTotalItemsCount() {
			int total = _dataManager.PlanningToWatch.Count +
					   _dataManager.Watching.Count +
					   _dataManager.Completed.Count;
			TotalItemsLabel.Text = total.ToString();
		}

		private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(sender is DataGrid grid && grid.SelectedItem is MediaItem item) {
				_selectedItem = item;

				// Determine which category the selected item belongs to
				if(grid == PlanningToWatchGrid) {
					_selectedCategory = "PlanningToWatch";
				} else if(grid == WatchingMoviesGrid || grid == WatchingSeriesGrid || grid == WatchingAnimeGrid) {
					_selectedCategory = "Watching";
				} else if(grid == CompletedGrid) {
					_selectedCategory = "Completed";
				}

				EditButton.IsEnabled = true;
				DeleteButton.IsEnabled = true;
			} else {
				_selectedItem = null;
				_selectedCategory = "";
				EditButton.IsEnabled = false;
				DeleteButton.IsEnabled = false;
			}
		}

		private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			// Clear selection when switching tabs
			_selectedItem = null;
			_selectedCategory = "";
			EditButton.IsEnabled = false;
			DeleteButton.IsEnabled = false;
		}

		private async void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) {
			if(e.EditAction == DataGridEditAction.Commit && e.EditingElement is TextBox textBox) {
				var item = (MediaItem)e.Row.Item;
				var columnHeader = e.Column.Header.ToString();

				// Prevent editing of Date and Type columns
				if(columnHeader == "Date Added" || columnHeader == "Date Completed" || columnHeader == "Type") {
					e.Cancel = true;
					return;
				}

				try {
					// Update the property based on column
					switch(columnHeader) {
						case "Title":
							item.Title = textBox.Text;
							break;
						case "Season":
							if(item is SeriesItem series) {
								if(int.TryParse(textBox.Text, out int seasonValue)) {
									series.Season = seasonValue;
								} else {
									e.Cancel = true;
									MessageBox.Show("Season must be a valid number.", "Invalid Input",
										MessageBoxButton.OK, MessageBoxImage.Warning);
									return;
								}
							} else if(item is AnimeItem anime) {
								if(int.TryParse(textBox.Text, out int seasonValue)) {
									anime.Season = seasonValue;
								} else {
									e.Cancel = true;
									MessageBox.Show("Season must be a valid number.", "Invalid Input",
										MessageBoxButton.OK, MessageBoxImage.Warning);
									return;
								}
							}
							break;
						case "Episode":
							if(item is SeriesItem series2) {
								if(int.TryParse(textBox.Text, out int episodeValue)) {
									series2.Episode = episodeValue;
								} else {
									e.Cancel = true;
									MessageBox.Show("Episode must be a valid number.", "Invalid Input",
										MessageBoxButton.OK, MessageBoxImage.Warning);
									return;
								}
							} else if(item is AnimeItem anime2) {
								if(int.TryParse(textBox.Text, out int episodeValue)) {
									anime2.Episode = episodeValue;
								} else {
									e.Cancel = true;
									MessageBox.Show("Episode must be a valid number.", "Invalid Input",
										MessageBoxButton.OK, MessageBoxImage.Warning);
									return;
								}
							}
							break;
					}

					// Update the date to current date when item is modified
					item.Date = DateTime.Now.ToString("dd/MM/yyyy");

					// Save changes
					string category = DetermineItemCategory(item);
					await _dataManager.UpdateItemAsync(item, category);

					Logger.Instance.Info($"Updated item: {item.Title} in {category}");

				} catch(Exception ex) {
					Logger.Instance.Error($"Error updating item: {ex.Message}", true);
					e.Cancel = true;
				}
			}
		}

		private string DetermineItemCategory(MediaItem item) {
			if(_dataManager.PlanningToWatch.Contains(item)) return "PlanningToWatch";
			if(_dataManager.Watching.Contains(item)) return "Watching";
			if(_dataManager.Completed.Contains(item)) return "Completed";
			return "";
		}

		private void NewButton_Click(object sender, RoutedEventArgs e) {
			_isEditMode = false;
			ShowDialog("Add New Item");
		}

		private void EditButton_Click(object sender, RoutedEventArgs e) {
			if(_selectedItem == null) return;

			_isEditMode = true;
			ShowDialog("Edit Item", _selectedItem);
		}

		private async void DeleteButton_Click(object sender, RoutedEventArgs e) {
			if(_selectedItem == null) return;

			var result = MessageBox.Show(
				$"Are you sure you want to delete '{_selectedItem.Title}'?",
				"Confirm Delete",
				MessageBoxButton.YesNo,
				MessageBoxImage.Question);

			if(result == MessageBoxResult.Yes) {
				try {
					await _dataManager.RemoveItemAsync(_selectedItem, _selectedCategory);
					Logger.Instance.Info($"Deleted item: {_selectedItem.Title} from {_selectedCategory}");

					_selectedItem = null;
					_selectedCategory = "";
					EditButton.IsEnabled = false;
					DeleteButton.IsEnabled = false;

				} catch(Exception ex) {
					Logger.Instance.Error($"Error deleting item: {ex.Message}", true);
				}
			}
		}

		private async void SaveButton_Click(object sender, RoutedEventArgs e) {
			try {
				await _dataManager.SaveAsync();
				MessageBox.Show("Data saved successfully!", "Save Complete",
					MessageBoxButton.OK, MessageBoxImage.Information);
				Logger.Instance.Info("Manual save completed");
			} catch(Exception ex) {
				Logger.Instance.Error($"Error saving data: {ex.Message}", true);
			}
		}

		private void ShowDialog(string title, MediaItem? existingItem = null) {
			DialogTitle.Text = title;

			if(existingItem != null) {
				// Populate fields for editing
				DialogTitleTextBox.Text = existingItem.Title;
				DialogTypeComboBox.SelectedIndex = existingItem.Type switch {
					"Movie" => 0,
					"Series" => 1,
					"Anime" => 2,
					_ => -1
				};

				if(existingItem is SeriesItem series) {
					DialogSeasonTextBox.Text = series.Season.ToString();
					DialogEpisodeTextBox.Text = series.Episode.ToString();
				} else if(existingItem is AnimeItem anime) {
					DialogSeasonTextBox.Text = anime.Season.ToString();
					DialogEpisodeTextBox.Text = anime.Episode.ToString();
				}

				DialogCategoryComboBox.SelectedIndex = _selectedCategory switch {
					"PlanningToWatch" => 0,
					"Watching" => 1,
					"Completed" => 2,
					_ => -1
				};
			} else {
				// Clear fields for new item
				DialogTitleTextBox.Clear();
				DialogSeasonTextBox.Clear();
				DialogEpisodeTextBox.Clear();
				DialogTypeComboBox.SelectedIndex = -1;
				DialogCategoryComboBox.SelectedIndex = -1;
			}

			UpdateDialogSeasonEpisodeVisibility();
			DialogOverlay.Visibility = Visibility.Visible;
			DialogTitleTextBox.Focus();
		}

		private void DialogTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			UpdateDialogSeasonEpisodeVisibility();
		}

		private void UpdateDialogSeasonEpisodeVisibility() {
			var selectedType = (DialogTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
			DialogSeasonEpisodePanel.Visibility = (selectedType == "Series" || selectedType == "Anime")
				? Visibility.Visible : Visibility.Collapsed;
		}

		private void DialogCancelButton_Click(object sender, RoutedEventArgs e) {
			DialogOverlay.Visibility = Visibility.Collapsed;
		}

		private async void DialogSaveButton_Click(object sender, RoutedEventArgs e) {
			var title = DialogTitleTextBox.Text.Trim();
			var type = (DialogTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
			var category = (DialogCategoryComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

			if(string.IsNullOrEmpty(title) || string.IsNullOrEmpty(type) || string.IsNullOrEmpty(category)) {
				MessageBox.Show("Please fill in all required fields.", "Input Error",
					MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			try {
				if(_isEditMode && _selectedItem != null) {
					// Handle category change if needed
					string newCategory = category;
					if(newCategory != _selectedCategory) {
						// Remove from old category and add to new category
						await _dataManager.RemoveItemAsync(_selectedItem, _selectedCategory);

						// Update the item properties
						_selectedItem.Title = title;
						if(_selectedItem is SeriesItem series) {
							series.Season = int.TryParse(DialogSeasonTextBox.Text, out var seasonVal) ? seasonVal : 1;
							series.Episode = int.TryParse(DialogEpisodeTextBox.Text, out var episodeVal) ? episodeVal : 1;
						} else if(_selectedItem is AnimeItem anime) {
							anime.Season = int.TryParse(DialogSeasonTextBox.Text, out var seasonVal2) ? seasonVal2 : 1;
							anime.Episode = int.TryParse(DialogEpisodeTextBox.Text, out var episodeVal2) ? episodeVal2 : 1;
						}

						// Update date when moving categories
						_selectedItem.Date = DateTime.Now.ToString("dd/MM/yyyy");

						await _dataManager.AddItemAsync(_selectedItem, newCategory);
					} else {
						// Update in same category
						_selectedItem.Title = title;
						if(_selectedItem is SeriesItem series) {
							series.Season = int.TryParse(DialogSeasonTextBox.Text, out var seasonUpdate) ? seasonUpdate : 1;
							series.Episode = int.TryParse(DialogEpisodeTextBox.Text, out var episodeUpdate) ? episodeUpdate : 1;
						} else if(_selectedItem is AnimeItem anime) {
							anime.Season = int.TryParse(DialogSeasonTextBox.Text, out var seasonUpdate2) ? seasonUpdate2 : 1;
							anime.Episode = int.TryParse(DialogEpisodeTextBox.Text, out var episodeUpdate2) ? episodeUpdate2 : 1;
						}

						await _dataManager.UpdateItemAsync(_selectedItem, _selectedCategory);
					}

					Logger.Instance.Info($"Updated item: {title} in {category}");
				} else {
					// Create new item
					string currentDate = DateTime.Now.ToString("dd/MM/yyyy");

					MediaItem? item = type switch {
						"Movie" => new MovieItem(title, currentDate),
						"Series" => new SeriesItem(
							title,
							currentDate,
							int.TryParse(DialogSeasonTextBox.Text, out var seasonValue) ? seasonValue : 1,
							int.TryParse(DialogEpisodeTextBox.Text, out var episodeValue) ? episodeValue : 1
						),
						"Anime" => new AnimeItem(
							title,
							currentDate,
							int.TryParse(DialogSeasonTextBox.Text, out var seasonValue2) ? seasonValue2 : 1,
							int.TryParse(DialogEpisodeTextBox.Text, out var episodeValue2) ? episodeValue2 : 1
						),
						_ => null
					};

					if(item == null) {
						MessageBox.Show("Unsupported type selected.", "Error",
							MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}

					await _dataManager.AddItemAsync(item, category);
					Logger.Instance.Info($"Added new item: {title} to {category}");
				}

				DialogOverlay.Visibility = Visibility.Collapsed;

			} catch(Exception ex) {
				Logger.Instance.Error($"Error saving item: {ex.Message}", true);
			}
		}
	}
}