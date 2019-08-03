using System;
using System.Collections.Generic;
using System.Text;

namespace BetterImport.Models
{
    public class ColumnMapping 
    {
        public string Name { get; private set; }
        public int DataFileIndex { get; private set; }

        public ColumnMapping(string name, int dataFileIndex)
        {
            Name = name;
            DataFileIndex = dataFileIndex;
        }
    }
}
