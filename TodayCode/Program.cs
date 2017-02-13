using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TodayCode
{
    class Program
    {
        static void Main(string[] args)
        {
            DialogResult result = MessageBox.Show("c#입니까? 아니라면 c++로 만듭니다", "질문", MessageBoxButtons.YesNo);

            // 일단 vscode로 합시다
            var editor = @"C:\Program Files (x86)\Microsoft VS Code\Code.exe";
            var folder = @"Z:\Proj\TodayCode\";
            var fileName = (result == DialogResult.Yes) 
                ? $"{DateTime.Now.ToString("yyyy-MM-dd")}.cs" 
                : $"{DateTime.Now.ToString("yyyy-MM-dd")}.cpp";

            var fileInfo = new FileInfo(Path.Combine(folder, fileName));
            if( !fileInfo.Exists )
            {
                var writer = fileInfo.CreateText();
                writer.Close();
            }

            Process.Start(editor, $"\"{fileInfo.FullName}\" \"{folder}\"");
        }
    }
}
