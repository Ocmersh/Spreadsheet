﻿using SS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Formulas;
using System.Xml;

namespace SpreadsheetTests
{
    /// <summary>
    /// These are grading tests for PS5
    ///</summary>
    [TestClass()]
    public class UnitTest6
    {
        /// <summary>
        /// Used to make assertions about set equality.  Everything is converted first to
        /// upper case.
        /// </summary>
        public static void AssertSetEqualsIgnoreCase(IEnumerable<string> s1, IEnumerable<string> s2)
        {
            var set1 = new HashSet<String>();
            foreach (string s in s1)
            {
                set1.Add(s.ToUpper());
            }

            var set2 = new HashSet<String>();
            foreach (string s in s2)
            {
                set2.Add(s.ToUpper());
            }

            Assert.IsTrue(new HashSet<string>(set1).SetEquals(set2));
        }

        // EMPTY SPREADSHEETS
        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test1()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.GetCellContents(null);
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test2()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.GetCellContents("AA");
        }

        [TestMethod()]
        public void Test3()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            Assert.AreEqual("", s.GetCellContents("A2"));
        }

        // SETTING CELL TO A DOUBLE
        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test4()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell(null, 1.5.ToString());
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test5()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1A", 1.5.ToString());
        }

        [TestMethod()]
        public void Test6()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("Z7", 1.5.ToString());
            Assert.AreEqual(1.5, (double)s.GetCellContents("Z7"), 1e-9);
        }

        // SETTING CELL TO A STRING
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test7()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A8", (string)null);
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test8()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell(null, "hello");
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test9()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("AZ", "hello");
        }

        [TestMethod()]
        public void Test10()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("Z7", "hello");
            Assert.AreEqual("hello", s.GetCellContents("Z7"));
        }

        // SETTING CELL TO A FORMULA
        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test11()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell(null, new Formula("2").ToString());
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test12()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("AZ", new Formula("2").ToString());
        }

        [TestMethod()]
        public void Test13()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("Z7", new Formula("3").ToString());
            Formula f = new Formula(s.GetCellContents("Z7").ToString());
            Assert.AreEqual(3, f.Evaluate(x => 0), 1e-6);
        }

        // CIRCULAR FORMULA DETECTION
        [TestMethod()]
        [ExpectedException(typeof(CircularException))]
        public void Test14()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "="+new Formula("A2").ToString());
            s.SetContentsOfCell("A2", "="+new Formula("A1").ToString());
        }

        [TestMethod()]
        public void Value1()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=" + new Formula("A2").ToString());
            s.SetContentsOfCell("A2", "=" + new Formula("A3").ToString());
            s.SetContentsOfCell("A3", "=" + new Formula("A4").ToString());
            s.SetContentsOfCell("A4", "=" + new Formula("5").ToString());
            Assert.AreEqual(5.0, s.GetCellValue("A1"));
        }

        [TestMethod()]
        public void Value2()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=" + new Formula("A2").ToString());
            s.SetContentsOfCell("A2", "=" + new Formula("A3+A4").ToString());
            s.SetContentsOfCell("A3", "=" + new Formula("A4").ToString());
            s.SetContentsOfCell("A4", "=" + new Formula("5").ToString());
            Assert.AreEqual(10.0, s.GetCellValue("A1"));
        }

        [TestMethod()]
        public void Value3()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=" + new Formula("A2").ToString());
            s.SetContentsOfCell("A2", "2");
            Assert.AreEqual(2.0, s.GetCellValue("A1"));
        }

        [TestMethod()]
        public void Value4()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", 3.0.ToString());
            Assert.AreEqual(3.0, s.GetCellValue("A1"));
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void Value5()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=");
            Assert.AreEqual("=", s.GetCellValue("A1"));
        }

        [TestMethod()]
        public void Value6()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "string");
            Assert.AreEqual("string", s.GetCellValue("A1"));
        }

        [TestMethod()]
        public void Setup1()
        {
            AbstractSpreadsheet s = new Spreadsheet(new Regex(".*"));
            s.SetContentsOfCell("A1", "string");
            Assert.AreEqual("string", s.GetCellValue("A1"));
        }

        [TestMethod()]
        public void Setup2()
        {
            StreamReader reader = new StreamReader("B:\\School FIles\\CS Repo\\PS2\\Spreadsheet\\Spreadsheet\\Spreadsheet.xsd");
            AbstractSpreadsheet s = new Spreadsheet(reader, new Regex(".*"));
            StreamWriter writer = new StreamWriter("B:\\Test2.xml");
            s.Save(writer);
        }

    [TestMethod()]
        [ExpectedException(typeof(CircularException))]
        public void Test15()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=" + new Formula("A2+A3").ToString());
            s.SetContentsOfCell("A3", "=" + new Formula("A4+A5").ToString());
            s.SetContentsOfCell("A5", "=" + new Formula("A6+A7").ToString());
            s.SetContentsOfCell("A7", "=" + new Formula("A1+A1").ToString());
        }

        [TestMethod()]
        [ExpectedException(typeof(CircularException))]
        public void Test16()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            try
            {
                s.SetContentsOfCell("A1", "=" + new Formula("A2+A3"));
                s.SetContentsOfCell("A2", 15.ToString());
                s.SetContentsOfCell("A3", 30.ToString());
                s.SetContentsOfCell("A2", "=" + new Formula("A3*A1"));
            }
            catch (CircularException e)
            {
                Assert.AreEqual(15, (double)s.GetCellContents("A2"), 1e-9);
                throw e;
            }
        }

        // NONEMPTY CELLS
        [TestMethod()]
        public void Test17()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            Assert.IsFalse(s.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());
        }

        [TestMethod()]
        public void Test18()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "");
            Assert.IsFalse(s.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());
        }

        [TestMethod()]
        public void Test19()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("C2", "hello");
            s.SetContentsOfCell("C2", "");
            Assert.IsFalse(s.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());
        }

        [TestMethod()]
        public void Test20()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "hello");
            AssertSetEqualsIgnoreCase(s.GetNamesOfAllNonemptyCells(), new string[] { "B1" });
        }

        [TestMethod()]
        public void Test21()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", 52.25.ToString());
            AssertSetEqualsIgnoreCase(s.GetNamesOfAllNonemptyCells(), new string[] { "B1" });
        }

        [TestMethod()]
        public void Test22()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", new Formula("3.5").ToString());
            AssertSetEqualsIgnoreCase(s.GetNamesOfAllNonemptyCells(), new string[] { "B1" });
        }

        [TestMethod()]
        public void Test23()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", 17.2.ToString());
            s.SetContentsOfCell("C1", "hello");
            s.SetContentsOfCell("B1", new Formula("3.5").ToString());
            AssertSetEqualsIgnoreCase(s.GetNamesOfAllNonemptyCells(), new string[] { "A1", "B1", "C1" });
        }

        // RETURN VALUE OF SET CELL CONTENTS
        [TestMethod()]
        public void Test24()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "hello");
            s.SetContentsOfCell("C1", new Formula("5").ToString());
            AssertSetEqualsIgnoreCase(s.SetContentsOfCell("A1", 17.2.ToString()), new string[] { "A1" });
        }

        [TestMethod()]
        public void Test25()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", 17.2.ToString());
            s.SetContentsOfCell("C1", new Formula("5").ToString());
            AssertSetEqualsIgnoreCase(s.SetContentsOfCell("B1", "hello"), new string[] { "B1" });
        }

        [TestMethod()]
        public void Test26()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", 17.2.ToString());
            s.SetContentsOfCell("B1", "hello");
            AssertSetEqualsIgnoreCase(s.SetContentsOfCell("C1", new Formula("5").ToString()), new string[] { "C1" });
        }

        [TestMethod()]
        public void Test27()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=" + new Formula("A2+A3").ToString());
            s.SetContentsOfCell("A2", 6.ToString());
            s.SetContentsOfCell("A3", "=" + new Formula("A2+A4").ToString());
            s.SetContentsOfCell("A4", "=" + new Formula("A2+A5").ToString());
            HashSet<string> result = new HashSet<string>(s.SetContentsOfCell("A5", 82.5.ToString()));
            AssertSetEqualsIgnoreCase(result, new string[] { "A5", "A4", "A3", "A1" });
        }

        // CHANGING CELLS
        [TestMethod()]
        public void Test28()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", new Formula("A2+A3").ToString());
            s.SetContentsOfCell("A1", 2.5.ToString());
            Assert.AreEqual(2.5, (double)s.GetCellContents("A1"), 1e-9);
        }

        [TestMethod()]
        public void Test29()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", new Formula("A2+A3").ToString());
            s.SetContentsOfCell("A1", "Hello");
            Assert.AreEqual("Hello", (string)s.GetCellContents("A1"));
        }

        [TestMethod()]
        public void Test30()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "Hello");
            s.SetContentsOfCell("A1", "=" + new Formula("23"));
            Assert.AreEqual(23, ((Formula)s.GetCellContents("A1")).Evaluate(x => 0));
        }

        [TestMethod()]
        public void Saved1()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=" + new Formula("B1+B2").ToString());
            s.SetContentsOfCell("B1", "=" + new Formula("C1-C2").ToString());
            s.SetContentsOfCell("B2", "=" + new Formula("C3*C4").ToString());
            s.SetContentsOfCell("C1", "=" + new Formula("D1*D2").ToString());
            s.SetContentsOfCell("C2", "=" + new Formula("D3*D4").ToString());
            s.SetContentsOfCell("C3", "=" + new Formula("D5*D6").ToString());
            s.SetContentsOfCell("C4", "=" + new Formula("D7*D8").ToString());
            s.SetContentsOfCell("D1", "=" + new Formula("E1").ToString());
            s.SetContentsOfCell("D2", "=" + new Formula("E1").ToString());
            s.SetContentsOfCell("D3", "=" + new Formula("E1").ToString());
            s.SetContentsOfCell("D4", "=" + new Formula("E1").ToString());
            s.SetContentsOfCell("D5", "=" + new Formula("E1").ToString());
            s.SetContentsOfCell("D6", "=" + new Formula("E1").ToString());
            s.SetContentsOfCell("D7", "=" + new Formula("E1").ToString());
            s.SetContentsOfCell("D8", "=" + new Formula("E1").ToString());
            ISet<String> cells = s.SetContentsOfCell("E1", 0.ToString());
            StreamWriter writer = new StreamWriter("B:\\Test.xml");
            s.Save(writer);

            AssertSetEqualsIgnoreCase(new HashSet<string>() { "A1", "B1", "B2", "C1", "C2", "C3", "C4", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "E1" }, cells);
        }

        // STRESS TESTS
        [TestMethod()]
        public void Test31()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=" + new Formula("B1+B2").ToString());
            s.SetContentsOfCell("B1", "=" + new Formula("C1-C2").ToString());
            s.SetContentsOfCell("B2", "=" + new Formula("C3*C4").ToString());
            s.SetContentsOfCell("C1", "=" + new Formula("D1*D2").ToString());
            s.SetContentsOfCell("C2", "=" + new Formula("D3*D4").ToString());
            s.SetContentsOfCell("C3", "=" + new Formula("D5*D6").ToString());
            s.SetContentsOfCell("C4", "=" + new Formula("D7*D8").ToString());
            s.SetContentsOfCell("D1", "=" + new Formula("E1").ToString());
            s.SetContentsOfCell("D2", "=" + new Formula("E1").ToString());
            s.SetContentsOfCell("D3", "=" + new Formula("E1").ToString());
            s.SetContentsOfCell("D4", "=" + new Formula("E1").ToString());
            s.SetContentsOfCell("D5", "=" + new Formula("E1").ToString());
            s.SetContentsOfCell("D6", "=" + new Formula("E1").ToString());
            s.SetContentsOfCell("D7", "=" + new Formula("E1").ToString());
            s.SetContentsOfCell("D8", "=" + new Formula("E1").ToString());
            ISet<String> cells = s.SetContentsOfCell("E1", 0.ToString());
            AssertSetEqualsIgnoreCase(new HashSet<string>() { "A1", "B1", "B2", "C1", "C2", "C3", "C4", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "E1" }, cells);
        }
        [TestMethod()]
        public void Test32()
        {
            Test31();
        }
        [TestMethod()]
        public void Test33()
        {
            Test31();
        }
        [TestMethod()]
        public void Test34()
        {
            Test31();
        }

        [TestMethod()]
        public void Test35()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            ISet<String> cells = new HashSet<string>();
            for (int i = 1; i < 200; i++)
            {
                cells.Add("A" + i);
                AssertSetEqualsIgnoreCase(cells, s.SetContentsOfCell("A" + i, "=" + new Formula("A" + (i + 1)).ToString()));
            }
        }
        [TestMethod()]
        public void Test36()
        {
            Test35();
        }
        [TestMethod()]
        public void Test37()
        {
            Test35();
        }
        [TestMethod()]
        public void Test38()
        {
            Test35();
        }
        [TestMethod()]
        public void Test39()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            for (int i = 1; i < 200; i++)
            {
                s.SetContentsOfCell("A" + i, "=" + new Formula("A" + (i + 1)).ToString());
            }
            try
            {
                s.SetContentsOfCell("A150", "=" + new Formula("A50").ToString());
                Assert.Fail();
            }
            catch (CircularException)
            {
            }
        }
        [TestMethod()]
        public void Test40()
        {
            Test39();
        }
        [TestMethod()]
        public void Test41()
        {
            Test39();
        }
        [TestMethod()]
        public void Test42()
        {
            Test39();
        }

        [TestMethod()]
        public void Test43()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            for (int i = 0; i < 500; i++)
            {
                s.SetContentsOfCell("A1" + i, "=" + new Formula("A1" + (i + 1)).ToString());
            }

            ISet<string> sss = s.SetContentsOfCell("A1499", 25.0.ToString());
            Assert.AreEqual(500, sss.Count);
            for (int i = 0; i < 500; i++)
            {
                Assert.IsTrue(sss.Contains("A1" + i) || sss.Contains("a1" + i));
            }

            sss = s.SetContentsOfCell("A1249", 25.0.ToString());
            Assert.AreEqual(250, sss.Count);
            for (int i = 0; i < 250; i++)
            {
                Assert.IsTrue(sss.Contains("A1" + i) || sss.Contains("a1" + i));
            }


        }

        [TestMethod()]
        public void Test44()
        {
            Test43();
        }
        [TestMethod()]
        public void Test45()
        {
            Test43();
        }
        [TestMethod()]
        public void Test46()
        {
            Test43();
        }

        [TestMethod()]
        public void Test47()
        {
            RunRandomizedTest(47, 2519);
        }
        [TestMethod()]
        public void Test48()
        {
            RunRandomizedTest(48, 2521);
        }
        [TestMethod()]
        public void Test49()
        {
            RunRandomizedTest(49, 2526);
        }
        [TestMethod()]
        public void Test50()
        {
            RunRandomizedTest(50, 2521);
        }

        public void RunRandomizedTest(int seed, int size)
        {
            AbstractSpreadsheet s = new Spreadsheet();
            Random rand = new Random(seed);
            for (int i = 0; i < 10000; i++)
            {
                try
                {
                    switch (rand.Next(3))
                    {
                        case 0:
                            s.SetContentsOfCell(randomName(rand), 3.14.ToString());
                            break;
                        case 1:
                            s.SetContentsOfCell(randomName(rand), "hello");
                            break;
                        case 2:
                            s.SetContentsOfCell(randomName(rand), randomFormula(rand));
                            break;
                    }
                }
                catch (CircularException)
                {
                }
            }
            ISet<string> set = new HashSet<string>(s.GetNamesOfAllNonemptyCells());
            Assert.AreEqual(size, set.Count);
        }

        private String randomName(Random rand)
        {
            return "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Substring(rand.Next(26), 1) + (rand.Next(99) + 1);
        }

        private String randomFormula(Random rand)
        {
            String f = randomName(rand);
            for (int i = 0; i < 10; i++)
            {
                switch (rand.Next(4))
                {
                    case 0:
                        f += "+";
                        break;
                    case 1:
                        f += "-";
                        break;
                    case 2:
                        f += "*";
                        break;
                    case 3:
                        f += "/";
                        break;
                }
                switch (rand.Next(2))
                {
                    case 0:
                        f += 7.2;
                        break;
                    case 1:
                        f += randomName(rand);
                        break;
                }
            }
            return f;
        }

    }
}
