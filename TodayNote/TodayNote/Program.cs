using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TodayNote
{
    class Program
    {
        static void Main(string[] args)
        {
            using (EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, "TodayNote"))
            {
                Process.Start(@"C:\Program Files (x86)\Microsoft Office\root\Office16\OneNote.exe");
                waitHandle.WaitOne(60000);
            }
        }
    }
}
