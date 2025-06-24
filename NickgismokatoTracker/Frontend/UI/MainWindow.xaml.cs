// File: MainWindow.xaml.cs
// Namespace: NickgismokatoTracker.Frontend.UI

using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;

using NickgismokatoTracker.Backend.Data;
using NickgismokatoTracker.Backend.Data.Item;

namespace NickgismokatoTracker.Frontend.UI{
    public partial class MainWindow : Window{
        private readonly DataManager _dataManager = new();

        public MainWindow(){
            InitializeComponent();

            Loaded += async (_, _) =>{
                await _dataManager.LoadAsync();
                BindDataGrids();
            };

            AddTypeComboBox.SelectionChanged += AddTypeComboBox_SelectionChanged;
        }

        private void BindDataGrids(){
            SetupDataGridColumns(PlanningToWatchGrid);
            PlanningToWatchGrid.ItemsSource = _dataManager.PlanningToWatch;

            SetupDataGridColumns(WatchingGrid);
            WatchingGrid.ItemsSource = _dataManager.Watching;

            SetupDataGridColumns(CompletedGrid);
            CompletedGrid.ItemsSource = _dataManager.Completed;
        }

        private void SetupDataGridColumns(DataGrid grid){
            grid.Columns.Clear();

            grid.Columns.Add(new DataGridTextColumn{
                Header = "Title",
                Binding = new System.Windows.Data.Binding("Title")
            });

            grid.Columns.Add(new DataGridTextColumn{
                Header = "Type",
                Binding = new System.Windows.Data.Binding("Type")
            });

            grid.Columns.Add(new DataGridTextColumn{
                Header = "Date",
                Binding = new System.Windows.Data.Binding("Date")
            });

            // Add Season and Episode columns for all grids to handle Series/Anime items
            grid.Columns.Add(new DataGridTextColumn{
                Header = "Season",
                Binding = new System.Windows.Data.Binding("Season")
            });
            grid.Columns.Add(new DataGridTextColumn{
                Header = "Episode",
                Binding = new System.Windows.Data.Binding("Episode")
            });
        }

        private void AddTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e){
            var selected = (AddTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

            if(selected == "Series" || selected == "Anime")
                SeasonEpisodePanel.Visibility = Visibility.Visible;
            else
                SeasonEpisodePanel.Visibility = Visibility.Collapsed;
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e){
            var title = AddTitleTextBox.Text.Trim();
            var type = (AddTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            var category = (AddCategoryComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

            if(string.IsNullOrEmpty(title) || string.IsNullOrEmpty(type) || string.IsNullOrEmpty(category)){
                MessageBox.Show("Please fill in all required fields.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Use consistent date format
            string currentDate = DateTime.Now.ToString("dd/MM/yyyy");

            MediaItem? item = type switch{
                "Movie" => new MovieItem(title, currentDate),
                "Series" => new SeriesItem(
                    title,
                    currentDate,
                    int.TryParse(AddSeasonTextBox.Text, out var s) ? s : 1,
                    int.TryParse(AddEpisodeTextBox.Text, out var ep) ? ep : 1
                ),
                "Anime" => new AnimeItem(
                    title,
                    currentDate,
                    int.TryParse(AddSeasonTextBox.Text, out var s2) ? s2 : 1,
                    int.TryParse(AddEpisodeTextBox.Text, out var ep2) ? ep2 : 1
                ),
                null => null,
                _ => null
            };

            if(item == null){
                MessageBox.Show("Unsupported type selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            await _dataManager.AddItemAsync(item, category);

            MessageBox.Show("Item added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // Clear form
            AddTitleTextBox.Clear();
            AddSeasonTextBox.Clear();
            AddEpisodeTextBox.Clear();
            AddTypeComboBox.SelectedIndex = -1;
            AddCategoryComboBox.SelectedIndex = -1;
        }
    }
}