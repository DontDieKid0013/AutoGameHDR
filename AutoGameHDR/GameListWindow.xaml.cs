using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.IO;
// 【关键】引入 WinForms 别名，用于稳定的文件对话框
using WinForms = System.Windows.Forms;

namespace AutoGameHDR
{
    // 用于列表显示的数据模型
    public class GameItem
    {
        public string ProcessName { get; set; }
        public bool IsEnabled { get; set; }
    }

    public partial class GameListWindow : Window
    {
        private List<GameItem> _items = new List<GameItem>();

        public GameListWindow(HashSet<string> enabledGames, HashSet<string> disabledGames)
        {
            InitializeComponent();

            // 1. 加载启用列表
            foreach (var game in enabledGames)
            {
                _items.Add(new GameItem { ProcessName = game, IsEnabled = true });
            }

            // 2. 加载禁用列表
            foreach (var game in disabledGames)
            {
                _items.Add(new GameItem { ProcessName = game, IsEnabled = false });
            }

            RefreshDataGrid();
        }

        private void RefreshDataGrid()
        {
            // 排序并重新绑定
            _items = _items.OrderBy(x => x.ProcessName).ToList();
            GameGrid.ItemsSource = null;
            GameGrid.ItemsSource = _items;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var item = ((FrameworkElement)sender).DataContext as GameItem;
            if (item != null)
            {
                _items.Remove(item);
                RefreshDataGrid();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var app = (App)Application.Current;
            app.UpdateUserList(_items);
            this.Close();
        }

        // ==========================================
        //  【新增】导入功能
        // ==========================================
        private void Import_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var openDialog = new WinForms.OpenFileDialog())
                {
                    openDialog.Title = "Import Game List (TXT)";
                    openDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

                    if (openDialog.ShowDialog() == WinForms.DialogResult.OK)
                    {
                        var lines = File.ReadAllLines(openDialog.FileName);
                        int count = 0;

                        foreach (var line in lines)
                        {
                            string cleanName = line.Trim();
                            if (string.IsNullOrWhiteSpace(cleanName)) continue;

                            // 检查是否已存在 (忽略大小写)
                            if (!_items.Any(x => x.ProcessName.Equals(cleanName, StringComparison.OrdinalIgnoreCase)))
                            {
                                // 导入的默认为启用状态
                                _items.Add(new GameItem { ProcessName = cleanName, IsEnabled = true });
                                count++;
                            }
                        }

                        RefreshDataGrid();
                        MessageBox.Show($"Successfully imported {count} new games!\n(Duplicates automatically ignored)", "Import successful", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Import failed: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==========================================
        //  【新增】导出功能
        // ==========================================
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var saveDialog = new WinForms.SaveFileDialog())
                {
                    saveDialog.Title = "Export game list";
                    saveDialog.Filter = "Text files (*.txt)|*.txt";
                    saveDialog.FileName = "AutoGameHDR_Backup.txt";

                    if (saveDialog.ShowDialog() == WinForms.DialogResult.OK)
                    {
                        // 只导出名字，一行一个
                        var lines = _items.Select(x => x.ProcessName).ToList();
                        File.WriteAllLines(saveDialog.FileName, lines);

                        MessageBox.Show("Export successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export failed: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}