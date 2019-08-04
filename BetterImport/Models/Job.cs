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
        public int BatchSize { get; private set; }
        public string ConnectionString { get; private set; }
        public string TableName { get; private set; }
        public string DataFile { get; private set; }
        public string ValueSeparator { get; private set; }
        public string SkippedRowFile { get; private set; }
        public string ErrorLogFile { get; private set; }
        public int StartRow { get; private set; }

        public ReadOnlyCollection<ColumnMapping> Columns { get; private set; }
        public ReadOnlyCollection<Preprocessor> Preprocessors { get; private set; }
        
        public Job(string connectionString, string tableName, string dataFile, string valueSeparator, string skippedRowFile, string errorLogFile, int startRow, List<ColumnMapping> columns, List<Preprocessor> preprocessors = null, int batchsize = 1000)
        {
            BatchSize = batchsize == 0 ? 1000 : batchsize;
            ConnectionString = connectionString;
            TableName = tableName;
            DataFile = dataFile;
            ValueSeparator = valueSeparator;
            SkippedRowFile = skippedRowFile;
            ErrorLogFile = errorLogFile;
            StartRow = startRow;
            Columns = new ReadOnlyCollection<ColumnMapping>(columns);
            if (preprocessors == null)
            {
                Preprocessors = new ReadOnlyCollection<Preprocessor>(new List<Preprocessor>());
            }
            else
            {
                Preprocessors = new ReadOnlyCollection<Preprocessor>(preprocessors);
            }
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
