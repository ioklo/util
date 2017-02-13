using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileFind
{
    class Item
    {        
        public string FileName { get; set; }
        public int Line { get; set; }
        public int Col { get; set; }

        public override string ToString()
        {
            
            return FileName + "(" + Line.ToString() + ", " + Col.ToString() + ")";            
        }
    }
}
