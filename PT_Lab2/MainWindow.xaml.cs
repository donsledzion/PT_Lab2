using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.IO;

namespace PT_Lab2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string _directory;
        string sortType = "size";
        string sortOrder = "asc";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void menuExit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void menuOpen_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _directory = folderBrowserDialog.SelectedPath;

                DisplayTreeView();
            }
        }

        void ListContent(string path, string sortType, string sortOrder, TreeViewItem parent)
        {
            var directories = new List<string>(Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly));
            var files = new List<string>(Directory.GetFiles(path));
            if (sortType == "size")
            {
                if (sortOrder == "asc")
                    files.Sort((f1, f2) => new FileInfo(f1).Length.CompareTo(new FileInfo(f2).Length));
                else
                    files.Sort((f1, f2) => new FileInfo(f2).Length.CompareTo(new FileInfo(f1).Length));
            }
            if (sortType == "time")
            {
                if (sortOrder == "asc")
                    files.Sort((f1, f2) => File.GetLastWriteTime(f1).CompareTo(File.GetLastWriteTime(f2)));
                else
                    files.Sort((f1, f2) => File.GetLastWriteTime(f2).CompareTo(File.GetLastWriteTime(f1)));
            }
            else if (sortType == "name")
            {
                if (sortOrder == "asc")
                    files.Sort((f1, f2) => f1.CompareTo(f2));
                else
                    files.Sort((f1, f2) => f2.CompareTo(f1));
            }

            foreach (string directory in directories)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(directory);
                
                var item = new TreeViewItem
                {
                    Header = Path.GetFileName(directory),
                    Tag = directory
                };
                HandleContextMenu(item);                
                parent.Items.Add(item);
                
                ListContent(directory, sortType, sortOrder, item);
            }

            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                var item = new TreeViewItem
                {
                    Header = Path.GetFileName(file),                    
                    Tag = file
                };

                if (Path.GetExtension(file).ToString() == ".txt")
                {
                    item.Selected += DisplayFile;
                }
                else
                {
                    item.Selected += ClearScrollViewer;
                }
                item.Selected += UpdateStatusBar;
                parent.Items.Add(item);
            }
        }

        private void DisplayFile(object sender, RoutedEventArgs e)
        {            
            TreeViewItem item = sender as TreeViewItem;
            TextBlock myTextBlock = new TextBlock();
            myTextBlock.Text = File.ReadAllText(item.Tag.ToString(), System.Text.Encoding.UTF8);
            myTextBlock.TextWrapping = TextWrapping.Wrap;
            ScrollViewer1.Content = myTextBlock.Text;
        }

        private void ClearScrollViewer(object sender, RoutedEventArgs e)
        {
            //ScrollViewer1.Content = null;
        }

        private void UpdateStatusBar(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            string path = item.Tag.ToString();
            StatusBarTextBlock.Text = HandleAttributes(path);
        }

        private string HandleAttributes(string path)
        {
            char[] attributes = new char[4] ;

            FileAttributes fileAttributes = File.GetAttributes(path);

            if ((fileAttributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                attributes[0] = 'r';
            else attributes[0] = '-';

            if ((fileAttributes & FileAttributes.Archive) == FileAttributes.Archive)
                attributes[1] = 'a';
            else attributes[1] = '-';

            if ((fileAttributes & FileAttributes.System) == FileAttributes.System)
                attributes[2] = 's';
            else attributes[2] = '-';

            if ((fileAttributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                attributes[3] = 'h';
            else attributes[3] = '-';

            return new string(attributes);
        }

        private void DeleteItem(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            string path = item.Tag.ToString();
            FileAttributes attr = File.GetAttributes(path);
            try
            {
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    Directory.Delete(path, true);
                else
                    File.Delete(path);
                DisplayTreeView();
            }
            catch
            {
                System.Windows.MessageBox.Show("Nie udało się usunąć pliku!");
            }
        }

        private void HandleContextMenu(TreeViewItem item)
        {
            string path = item.Tag.ToString();
            FileAttributes attr = File.GetAttributes(path);
            
            ContextMenu myContextMenu = new ContextMenu();
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                MenuItem createMenuItem = new MenuItem();
                createMenuItem.Header = "New";
                createMenuItem.Tag = item.Tag;
                createMenuItem.Click += AddFileDirectory;
                myContextMenu.Items.Add(createMenuItem);
            }
            MenuItem deleteMenuItem = new MenuItem();
            deleteMenuItem.Header = "Delete";
            deleteMenuItem.Tag = item.Tag;
            deleteMenuItem.Click += DeleteItem;
            myContextMenu.Items.Add(deleteMenuItem);
            item.ContextMenu = myContextMenu;
        }

        private void DisplayTreeView()
        {
            var root = new TreeViewItem
            {
                Header = Path.GetFileName(_directory),
                Tag = _directory
            };
            TreeView1.Items.Clear();
            TreeView1.Items.Add(root);

            ListContent(_directory, sortType, sortOrder, root);
        }

        private void AddFileDirectory(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            string path = item.Tag.ToString();

            InputWindow input = new InputWindow();
            input.ShowDialog();
            string fileName = input.FileName;
            try
            {
                if(Path.HasExtension(Path.Combine(path,fileName)))
                    File.Create(Path.Combine(path, fileName));
                else
                    Directory.CreateDirectory(Path.Combine(path, fileName));
                DisplayTreeView();
            }
            catch
            {
                System.Windows.MessageBox.Show("Nie udało się utworzyć pliku");
            }
            
            

        }

    }
}
