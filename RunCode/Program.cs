using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunCode
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8; // VSCode를 위해
            // Console.InputEncoding = Encoding.UTF8; // VSCode를 위해

            // vsvarargs
            var vsvars32 = @"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\Tools\vsvars32.bat";

            // 파일 이름 하나만 받습니다.
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: RunCode code.cpp");
                Console.WriteLine("       RunCode code.cs");
                return;
            }

            var fileInfo = new FileInfo(args[0]);

            if( !fileInfo.Exists )
            {
                Console.WriteLine($"{fileInfo.Name} 파일이 없습니다");
                return;
            }

            // cs인지 cpp인지 판별
            if (fileInfo.Extension == ".cs")
            {
                var exeFileInfo = new FileInfo(Path.ChangeExtension(fileInfo.FullName, "exe"));

                // clean process
                if (File.Exists(exeFileInfo.FullName)) exeFileInfo.Delete();

                RunCommand($"\"{vsvars32}\" & csc /nologo /out:\"{exeFileInfo.FullName}\" \"{fileInfo.FullName}\"");

                if (!File.Exists(exeFileInfo.FullName))
                {
                    Console.WriteLine("컴파일 실패");
                    return;
                }

                RunCommand(exeFileInfo.FullName);

                // clean process 
                if (File.Exists(exeFileInfo.FullName)) exeFileInfo.Delete();
            }
            else if (fileInfo.Extension == ".cpp")
            {
                var objFileInfo = new FileInfo(Path.ChangeExtension(fileInfo.FullName, "obj"));
                var exeFileInfo = new FileInfo(Path.ChangeExtension(fileInfo.FullName, "exe"));

                // clean process
                if (File.Exists(exeFileInfo.FullName)) exeFileInfo.Delete();
                if (File.Exists(objFileInfo.FullName)) objFileInfo.Delete();

                RunCommand($"\"{vsvars32}\" & cl /nologo /Fe:\"{exeFileInfo.FullName}\" /Fo\"{objFileInfo.FullName}\" \"{fileInfo.FullName}\"");

                if (!File.Exists(exeFileInfo.FullName))
                {
                    Console.WriteLine("컴파일 실패");
                    return;
                }

                RunCommand(exeFileInfo.FullName);

                // clean process 
                if (File.Exists(exeFileInfo.FullName)) exeFileInfo.Delete();
                if (File.Exists(objFileInfo.FullName)) objFileInfo.Delete();
            }
            else
            {
                Console.WriteLine($"{fileInfo.Name} 파일은 cs나 cpp로 끝나지 않습니다");
                return;
            }                        

        }

        private static void RunCommand(string command)
        {
            // cmd /c를 바로 쓰면 따옴표로 둘러싼 인자가 하나까지만 허용된다, 인자 두개를 따옴표로 둘러싸거나 몇몇 경우에, cmd /s /c로 동작한다 
            // cmd /s /c 는 인자들을 따옴표 한개로 크게 묶고, 안에서 일반적으로 사용하듯이 따옴표로 인자들을 구분하면 된다
            var compileProcessStartInfo = new ProcessStartInfo("cmd", $"/s /c \"{command}\"");
            compileProcessStartInfo.UseShellExecute = false;
            compileProcessStartInfo.RedirectStandardOutput = true;
            compileProcessStartInfo.RedirectStandardError = true;

            compileProcessStartInfo.StandardOutputEncoding = Encoding.Default;
            // compileProcessStartInfo.StandardErrorEncoding = Encoding.UTF8;

            var compileProcess = Process.Start(compileProcessStartInfo);

            compileProcess.OutputDataReceived += (sender, e) => Console.Out.WriteLine(e.Data);
            compileProcess.ErrorDataReceived += (sender, e) => Console.Error.WriteLine(e.Data);

            compileProcess.BeginOutputReadLine();
            compileProcess.BeginErrorReadLine();

            compileProcess.WaitForExit();
            compileProcess.Close();
        }

    }
}
