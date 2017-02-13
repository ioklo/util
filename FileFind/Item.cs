using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileFind
{
    class Item
    {        
        public string FileName { get; private set; }
        public int Line { get; private set; }
        public int Col { get; private set; }

        public Item(string filename, int line, int col)
        {
            FileName = filename;
            Line = line;
            Col = col;
        }

        public override string ToString()
        {
            
            return FileName + "(" + Line.ToString() + ", " + Col.ToString() + ")";            
        }
    }
}
