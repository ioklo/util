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

        void SetButtonLabel(string text)
        {
            if (button1.Dispatcher.CheckAccess())
            {
                button1.Content = text;
            }
            else
            {
                button1.Dispatcher.Invoke( (Action<string>)SetButtonLabel, text);
            }
        }

        private void Search( dynamic arg )
        {
            string searchStr = arg.searchStr;
            string[] paths = ((string)arg.path).Split(';');
            string[] patterns = ((string)arg.pattern).Split(';');
            Encoding encoding = arg.encoding;

            Regex regex = new Regex(searchStr, RegexOptions.IgnoreCase);

            var stack = new List<string>();            
            stack.AddRange(paths);

            Dispatcher.Invoke((Action)(()=>listView1.Items.Clear()));
            
            while (stack.Count != 0)
            {
                string dir = stack[stack.Count - 1];
                stack.RemoveAt(stack.Count - 1);

                try 
                {
                    stack.AddRange(
                        from d in Directory.GetDirectories(dir)
                        orderby d descending
                        select d);
                }
                catch(Exception)
                {

                }

                try
                {
                    foreach (var pattern in patterns)
                        foreach (string filename in
                            Directory.EnumerateFiles(dir, pattern, SearchOption.TopDirectoryOnly))
                        {
                            int lineCount = 1;
                            Dispatcher.Invoke((Action)(() => label4.Content = filename));

                            if (searchStr.Length == 0)
                            {
                                var item = new Item
                                {
                                    FileName = filename,
                                    Line = 0,
                                    Col = 0
                                };

                                listView1.Dispatcher.Invoke(
                                    (Action)(() => listView1.Items.Add(item)));

                                continue;
                            }

                            using (var reader = new StreamReader(filename, encoding))
                            {
                                string line = reader.ReadLine();
                                while (line != null)
                                {
                                    foreach (Match m in regex.Matches(line))
                                    {
                                        var item = new Item
                                        {
                                            FileName = filename,
                                            Line = lineCount,
                                            Col = m.Index + 1
                                        };

                                        listView1.Dispatcher.Invoke(
                                            (Action)(() => listView1.Items.Add(item)));
                                    }

                                    lineCount++;
                                    line = reader.ReadLine();
                                }
                            }
                        }
                }
                catch(DirectoryNotFoundException)
                { 
                    

                }

            }
            SetButtonLabel("찾기");
            thread = null;
        }

        private Thread thread = null;

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (thread != null)
            {
                // 중지 모드니까 중지
                thread.Abort();
                thread = null;
                SetButtonLabel("찾기");
                return;
            }

            if (!Directory.Exists(textBox2.Text))
            {
                MessageBox.Show(string.Format("{0}\n디렉토리가 없습니다", textBox2.Text));
                return;
            }

            thread = new Thread(Search);

            dynamic arg = new ExpandoObject();
            arg.searchStr = textBox1.Text;
            arg.path = textBox2.Text;
            arg.pattern = textBox3.Text;

            arg.encoding = Encoding.GetEncoding((string)encodingBox.SelectionBoxItem.ToString());

            thread.Start(arg);
            SetButtonLabel("중지");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (thread != null)
                thread.Abort();

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
