using System;
using System.Linq;
using Formulas;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS;

namespace SpreadSheetTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            AbstractSpreadsheet sheet = new SpreadSheet();
        }

        [TestMethod]
        public void TestMethod2()
        {
            AbstractSpreadsheet sheet = new SpreadSheet();
            sheet.SetCellContents("A1", "A");
            Assert.AreEqual("A1", sheet.GetNamesOfAllNonemptyCells().ElementAt(0));

        }
        [TestMethod]
        public void TestMethod3()
        {
            AbstractSpreadsheet sheet = new SpreadSheet();
            sheet.SetCellContents("A1", "A");
            object test = sheet.GetCellContents("A1");
            Assert.AreEqual("A", (string)test);
        }
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestMethod4()
        {
            AbstractSpreadsheet sheet = new SpreadSheet();
            sheet.SetCellContents(null, "A");
            object test = sheet.GetCellContents("A1");
            Assert.AreEqual("A", (string)test);
        }
        [TestMethod]
        public void TestMethod5()
        {
            AbstractSpreadsheet sheet = new SpreadSheet();
            sheet.SetCellContents("A1", 2.0);
            object test = sheet.GetCellContents("A1");
            Assert.AreEqual(2.0, (double)test);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestMethod6()
        {
            AbstractSpreadsheet sheet = new SpreadSheet();
            sheet.SetCellContents(null, 2.0);
            object test = sheet.GetCellContents("A1");
            Assert.AreEqual(2.0, (double)test);
        }

        [TestMethod]
        public void TestMethod7()
        {
            AbstractSpreadsheet sheet = new SpreadSheet();
            sheet.SetCellContents("A1", new Formula("B2"));
            object test = sheet.GetCellContents("A1");
            Assert.AreEqual("B2", ((Formula)test).ToString());
        }
    }
}
