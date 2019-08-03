using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Diagnostics;
using System.Text;
using BetterImport.Models;
using BetterImport.DAO;

namespace BetterImport
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("BetterImport (C)2019 Ivan Kouznetsov. AGPLv3. Read README.md for usage.");
            Console.WriteLine("Press Esc to pause.");

            const int batchSize = 1000;

            if (args.Length != 1)
            {
                Console.WriteLine("Usage: BetterImport job.json");
                return;
            }

            string jobFile = args[0];

            if (!File.Exists(jobFile))
            {
                Console.WriteLine("File not found: " + jobFile);
                return;
            }

            Job job = Job.LoadFromFile(jobFile, out Exception exception);
            if (job == null)
            {
                Console.WriteLine(jobFile + " has errors:");
                Console.WriteLine(exception.Message);
                return;
            }

            if (job.Columns.Count == 0)
            {
                Console.WriteLine("No columns mapped.");
                return;
            }

            if (job.StartRow < 1)
            {
                Console.WriteLine("StartRow is less than 1. StartRow is 1-indexed.");
                return;
            }

            if (!File.Exists(job.DataFile))
            {
                Console.WriteLine("File not found: " + job.DataFile);
                return;
            }

            FaultTolerantDAO faultTolerantDAO = new FaultTolerantDAO(job.ConnectionString);

            if (!faultTolerantDAO.TestConfiguration(job.TableName, job.Columns, out Exception dbException))
            {
                Console.WriteLine("Database error:");
                Console.WriteLine(dbException.Message);
                return;
            }

            try
            {
                File.Create(job.ErrorLogFile).Dispose();
                File.Create(job.SkippedRowFile).Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not create log files:");
                Console.WriteLine(ex.Message);
            }
                        
            Console.WriteLine("Started at " + DateTime.Now.ToLongTimeString());
            IEnumerator<string> dataFileEnumerator = File.ReadLines(job.DataFile).GetEnumerator();
            string[][] values = new string[batchSize][];
            string[] originalLines = new string[batchSize];
            int rowCount;

            ReadOnlyCollection<int> skippedRows;

            rowCount = job.StartRow - 1; // for 1-indexing

            while(rowCount > 0)
            {
                dataFileEnumerator.MoveNext();
                rowCount--;
            }
           
            while (dataFileEnumerator.MoveNext())
            {
                if (rowCount == batchSize)
                {
                    // Save batch
                    SaveAndLog(faultTolerantDAO, job, values, originalLines, out skippedRows);

                    // Report progress
                    ReportProgress(rowCount, skippedRows.Count);

                    // Start new batch
                    rowCount = 0;
                    values = new string[batchSize][];
                    originalLines = new string[batchSize];

                }
                values[rowCount] = dataFileEnumerator.Current.Split(job.ValueSeparator);
                originalLines[rowCount] = dataFileEnumerator.Current;

                rowCount++;

                if (Console.KeyAvailable)
                {
                    var consoleKey = Console.ReadKey(true);
                    if (consoleKey.Key == ConsoleKey.Escape)
                    {                       
                        Console.WriteLine("Paused. Press any key to continue...");
                        while (Console.ReadKey(true).Key == ConsoleKey.Escape) { }
                    }
                }
            }

            // Save last batch
            SaveAndLog(faultTolerantDAO, job, values, originalLines, out skippedRows);

            // Report progress
            ReportProgress(rowCount, skippedRows.Count);
            
            Console.WriteLine("Finished at " + DateTime.Now.ToString());
        }

        private static void SaveAndLog(FaultTolerantDAO faultTolerantDAO, Job job, string[][] values, string[] originalLines, out ReadOnlyCollection<int> skippedRows)
        {
            faultTolerantDAO.SaveBatch(job.TableName, job.Columns, values, out skippedRows, out ReadOnlyCollection<Exception> exceptions);
            LogSkippedRows(job.SkippedRowFile, originalLines, skippedRows);
            LogErrors(job.ErrorLogFile, exceptions);
        }

        static int progressSaved = 0;
        static int progressSkipped = 0;

        private static void ReportProgress(int batchSize, int skipped)
        {
            progressSaved += batchSize - skipped;
            progressSkipped += skipped;

            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " Saved: " + progressSaved + " Skipped: " + progressSkipped);
        }

        private static void LogSkippedRows(string filepath, string[] originalLines, IList<int> skippedRows)
        {
            List<string> lines = new List<string>();
            for (int i = 0; i < skippedRows.Count; i++)
            {
                lines.Add(originalLines[skippedRows[i]]);
            }

            File.AppendAllLines(filepath, lines);            
        }

        private static void LogErrors(string filepath, IList<Exception> exceptions)
        {
            List<string> lines = new List<string>();
            for (int i = 0; i < exceptions.Count; i++)
            {
                lines.Add(exceptions[i].Message);
            }

            File.AppendAllLines(filepath,lines);            
        }
    }
}
