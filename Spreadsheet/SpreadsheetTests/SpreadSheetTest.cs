using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spreadsheet;
using SS;

namespace Spreadsheets
{
    [TestClass]
    public class SpreadSheetTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();
        }
    }
}
