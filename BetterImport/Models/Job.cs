using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace BetterImport.Models
{
    public class Job
    {
        public string ConnectionString { get; private set; }
        public string TableName { get; private set; }
        public string DataFile { get; private set; }
        public string ValueSeparator { get; private set; }
        public string SkippedRowFile { get; private set; }
        public string ErrorLogFile { get; private set; }
        public int StartRow { get; private set; }

        public ReadOnlyCollection<ColumnMapping> Columns { get { return new ReadOnlyCollection<ColumnMapping>(columns); } }

        private readonly List<ColumnMapping> columns = new List<ColumnMapping>();

        public Job(string connectionString, string tableName, string dataFile, string valueSeparator, string skippedRowFile, string errorLogFile, int startRow, List<ColumnMapping> columns)
        {
            ConnectionString = connectionString;
            TableName = tableName;
            DataFile = dataFile;
            ValueSeparator = valueSeparator;
            SkippedRowFile = skippedRowFile;
            ErrorLogFile = errorLogFile;
            StartRow = startRow;
            this.columns = columns;
        }
        
        public static Job LoadFromFile(string filepath, out Exception exception)
        {
            Job job = null;
            exception = null;

            if (File.Exists(filepath))
            {
                try
                {
                    job = JsonConvert.DeserializeObject<Job>(File.ReadAllText(filepath));
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            }

            return job;
        }
    }
}
