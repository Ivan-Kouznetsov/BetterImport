using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Collections.Generic;
using BetterImport.Models;

namespace BetterImportTests
{
    [TestClass]
    public class Tests
    {

        const string TestData = "TestData";

        [TestMethod]
        public void ParseJobFile()
        {
            Job job = Job.LoadFromFile(TestData + Path.DirectorySeparatorChar + "job.json", out _);

            Assert.AreEqual("dummyConnString", job.ConnectionString);
            Assert.AreEqual("dummyTable", job.TableName);
            Assert.AreEqual("data.tsv", job.DataFile);
            Assert.AreEqual("\t", job.ValueSeparator);
            Assert.AreEqual("skippedrows.txt", job.SkippedRowFile);
            Assert.AreEqual(2, job.StartRow);
            Assert.AreEqual("Id", job.Columns[0].Name);
            Assert.AreEqual(1, job.Columns[0].DataFileIndex);
            Assert.AreEqual("Name", job.Columns[1].Name);
            Assert.AreEqual(2, job.Columns[1].DataFileIndex);
        }
    }
}
