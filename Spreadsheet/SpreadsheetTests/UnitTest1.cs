using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetTests;
using SS;

namespace SpreadsheetTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            AbstractSpreadsheet sheet = new SpreadsheetTests();
        }
    }
}
