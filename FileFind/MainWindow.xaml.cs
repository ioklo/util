using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Dynamic;
using System.Windows.Threading;
using FileFind.Properties;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FileFind
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            textBox1.Focus();
        }
        
        private async Task Search( string searchText, string pathText, string patternText, Encoding encoding, CancellationToken cancellationToken)
        {
            string[] paths = pathText.Split(';');
            string[] patterns = patternText.Split(';');

            Regex regex = new Regex(searchText, RegexOptions.IgnoreCase);
            listView1.Items.Clear();

            var stack = new Stack<string>();
            foreach (var path in paths.Reverse())
                stack.Push(path);

            while (stack.Count != 0)
            {
                string dir = stack.Pop();
                if (cancellationToken.IsCancellationRequested) return;

                await Task.Run(() =>
                {
                    try
                    {
                        foreach (var childDir in Directory.GetDirectories(dir).OrderByDescending(d => d))
                            stack.Push(childDir);
                    }
                    catch { }
                }, cancellationToken);

                try
                {
                    List<string> filenames = await Task.Run(() =>
                    {
                        return patterns.SelectMany(pattern => Directory.EnumerateFiles(dir, pattern, SearchOption.TopDirectoryOnly))
                                       .Distinct()
                                       .ToList();
                    }, cancellationToken);
                    
                    foreach (string filename in filenames)
                    {
                        label4.Content = filename;

                        if (searchText.Length == 0)
                        {
                            if (cancellationToken.IsCancellationRequested) return;

                            var item = new Item(filename, 0, 0);
                            listView1.Items.Add(item);
                            continue;
                        }

                        List<Item> items = await Task.Run(() => SearchInFile(encoding, cancellationToken, regex, filename), cancellationToken);
                        if (cancellationToken.IsCancellationRequested) return;

                        foreach (var item in items)
                            listView1.Items.Add(item);
                    }
                }
                catch (DirectoryNotFoundException)
                {

                }
            }

            button1.Content = "찾기";
        }

        private List<Item> SearchInFile(Encoding encoding, CancellationToken cancellationToken, Regex regex, string filename)
        {
            List<Item> items = new List<Item>();
            int lineCount = 1;

            using (var reader = new StreamReader(filename, encoding))
            {
                string line = reader.ReadLine();
                while (line != null)
                {
                    foreach (Match m in regex.Matches(line))
                    {
                        var item = new Item(filename, lineCount, m.Index + 1);
                        items.Add(item);
                    }

                    lineCount++;
                    line = reader.ReadLine();
                }
            }

            return items;
        }

        private Task searchTask = null;
        private CancellationTokenSource cancellationTokenSource = null;

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            if (searchTask != null)
            {
                button1.IsEnabled = false;
                cancellationTokenSource.Cancel();

                await searchTask;
                searchTask = null;
                cancellationTokenSource = null;

                button1.Content = "찾기";
                button1.IsEnabled = true;
                return;
            }

            if (!Directory.Exists(textBox2.Text))
            {
                MessageBox.Show(string.Format("{0}\n디렉토리가 없습니다", textBox2.Text));
                return;
            }

            button1.Content = "중지";

            var encoding = Encoding.GetEncoding((string)encodingBox.SelectionBoxItem.ToString());
            cancellationTokenSource = new CancellationTokenSource();
            searchTask = Search(textBox1.Text, textBox2.Text, textBox3.Text, encoding, cancellationTokenSource.Token);
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (searchTask != null)
            {
                cancellationTokenSource.Cancel();
                await searchTask;
            }

            Settings.Default.SearchString = textBox1.Text;
            Settings.Default.Path = textBox2.Text;
            Settings.Default.FilePattern = textBox3.Text;
            Settings.Default.Save();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textBox1.Text = Settings.Default.SearchString;
            textBox2.Text = Settings.Default.Path;
            textBox3.Text = Settings.Default.FilePattern;

            string[] args = Environment.GetCommandLineArgs();            

            for( int i = 1; i < args.Length; i++)
            {
                textBox2.Text = args[i];
            }

        }

        private void listView1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = listView1.SelectedItem as Item;
            if (item != null)
            {
                Process.Start(Settings.Default.Editor, string.Format(Settings.Default.EditorArgs, item.FileName, item.Line, item.Col));                


                //var editPlus = @"C:\Program Files (x86)\EditPlus 3\Editplus.exe";
                //var editArgs = item.FileName + " -cursor " + item.Line.ToString() + ":" + item.Col.ToString();
                //var acroEdit = @"C:\Program Files\AcroSoft\AcroEdit\Acroedit.exe";
                //var acroArgs = item.FileName + @" /L:" + item.Line.ToString() + "/C:" + item.Col.ToString();
                //var crimsonEdit = @"C:\Program Files\Emerald Editor Community\Crimson Editor SVN286\cedt.exe";
                //var crimsonArgs = item.FileName + @" /L:" + item.Line.ToString() + "/C:" + item.Col.ToString();

                //Process.Start(crimsonEdit, crimsonArgs);

            }

        }
    }
}
