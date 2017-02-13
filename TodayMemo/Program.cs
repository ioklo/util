using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodayMemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var folder = @"Z:\Onedrive\Memo\2017";
            var filename = $"{DateTime.Today.ToString(@"yyyy-MM-dd")}.txt";

            var fileInfo = new FileInfo(Path.Combine(folder, filename));

            if (!fileInfo.Exists)
            {
                var streamWriter = fileInfo.CreateText();
                streamWriter.Close();
            }

            Process.Start(fileInfo.FullName);
        }
    }
}
