using System.Windows;

namespace AutoGameHDR
{
    public partial class GlobalListWindow : Window
    {
        public GlobalListWindow(HashSet<string> globalGames)
        {
            InitializeComponent();

            var sortedList = globalGames.OrderBy(x => x).ToList();
            GameList.ItemsSource = sortedList;

            this.Title = $"Online Cloud Whitelist ({sortedList.Count} items total)";
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}