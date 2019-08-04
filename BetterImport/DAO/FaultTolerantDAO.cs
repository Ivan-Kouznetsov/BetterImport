using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Data.SqlClient;
using BetterImport.Models;

using System.Diagnostics;
namespace BetterImport.DAO
{
    public class FaultTolerantDAO
    {
        private readonly string connectionString = "";
        private readonly bool lockTable = false;
        public FaultTolerantDAO(string connectionString, bool lockTable)
        {
            this.connectionString = connectionString;
            this.lockTable = lockTable;
        }

        public bool TestConfiguration(string tableName, IList<ColumnMapping> columnMappings, out Exception exception)
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    sqlConnection.Open();
                    using (SqlCommand sqlCommand = new SqlCommand("select " + CreateColumnList(columnMappings) + " from " + tableName + " where 1=2;", sqlConnection))
                    {
                        sqlCommand.ExecuteScalar();
                    }
                }
                exception = null;
                return true;
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }
        
        public bool TrySaveTransaction(string tableName, IList<ColumnMapping> columnMappings, string[][] values, out Exception exception)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();              
                using (SqlCommand sqlCommand = new SqlCommand(string.Empty, sqlConnection))
                using (sqlCommand.Transaction = sqlConnection.BeginTransaction())
                {
                    try
                    {
                        for (int row = 0; row < values.GetLength(0); row++)
                        {
                            if (values[row] != null)
                            {                                
                                sqlCommand.CommandText = "insert into " +
                                                         tableName + (lockTable ? " WITH (TABLOCKX) " : "") +
                                                         "(" + CreateColumnList(columnMappings) + ")" +
                                                         " values (" + CreateValuesList(columnMappings, values[row]) + ")";
                                sqlCommand.ExecuteNonQuery();                             
                            }
                        }
                        sqlCommand.Transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                        return false;
                    }
                }
            }
           
            exception = null;
            return true;
        }
        
        public void SaveBatch(string tableName, IList<ColumnMapping> columnMappings, string[][] values, out ReadOnlyCollection<int> skippedRows, out ReadOnlyCollection<Exception> exceptions)
        {
            List<int> skippedRowsList = new List<int>();
            List<Exception> exceptionsList = new List<Exception>();

            if (!TrySaveTransaction(tableName, columnMappings, values, out _)) // the same exception will come up again when saving individual records
            {
                for (int row = 0; row < values.GetLength(0); row++)
                {
                    if (values[row] != null)
                    {
                        if (!TrySaveTransaction(tableName, columnMappings, new string[][] {values[row]}, out Exception exception))
                        {
                            skippedRowsList.Add(row);
                            exceptionsList.Add(exception);
                        }
                    }
                }
            }


            skippedRows = new ReadOnlyCollection<int>(skippedRowsList);
            exceptions = new ReadOnlyCollection<Exception>(exceptionsList);
        }

        private string CreateColumnList(IList<ColumnMapping> columns)
        {
            List<string> columList = new List<string>();

            for (int i = 0; i < columns.Count; i++)
            {
                columList.Add(columns[i].Name);
            }

            return string.Join(',', columList);
        }
        
        private string CreateValuesList(IList<ColumnMapping> columns, string[] rawValues)
        {
            string[] values = new string[columns.Count];

            for (int i = 0; i < columns.Count; i++)
            {
                if ((columns[i].DataFileIndex - 1) < rawValues.Length)
                {
                    string currentValue = rawValues[columns[i].DataFileIndex - 1] // for 1-indexing
                                         .Replace("'", @"''"); // to escape single quotes

                    if (int.TryParse(currentValue, out _))
                    {
                        values[i] = currentValue;
                    }
                    else if (IsUnicode(currentValue))
                    {
                        values[i] = "N'" + currentValue + "'";
                    }
                    else if (currentValue == "NULL")
                    {
                        values[i] = "NULL";
                    }
                    else
                    {
                        values[i] = "'" + currentValue + "'";
                    }
                }
                else
                {
                    values[i] = "NULL";
                }
            }

            return string.Join(',', values);
        }

        private bool IsUnicode(string s)
        {
            char[] chars = s.ToCharArray();
            foreach (char c in chars)
            {
                if (c > SByte.MaxValue) return true;
            }
            return false;
        }
    }
}
