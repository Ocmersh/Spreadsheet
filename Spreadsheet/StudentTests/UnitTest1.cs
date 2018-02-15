///Test class for DependencyGraph written by Bryce Hansen
/// 
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dependencies;

namespace DependencyGraphTestCases
{
    [TestClass]
    public class UnitTest1
    {
        /// <summary>
        /// This class tests each method is DependencyGraph.
        /// </summary>

        DependencyGraph testCase = new DependencyGraph();

        /// <summary>
        /// Test to make sure adding multiple key values work.
        /// </summary>
        [TestMethod]
        public void TestAddDep()
        {
            testCase = new DependencyGraph();

            testCase.AddDependency("a", "b");
            testCase.AddDependency("a", "c");

            IEnumerable<string> result = new List<string>() { "b", "c" };

            Assert.AreEqual(result.ElementAt(1), testCase.GetDependents("a").ElementAt(1));
            Assert.AreEqual(result.ElementAt(0), testCase.GetDependents("a").ElementAt(0));
        }

        /// <summary>
        /// Verify that values with no dependents are correct after removal.
        /// </summary>
        [TestMethod]
        public void TestRemoveDep()
        {
            testCase = new DependencyGraph();

            testCase.AddDependency("a", "b");
            testCase.AddDependency("a", "c");

            IEnumerable<string> result = new List<string>() { "b", "c" };

            testCase.RemoveDependency("a", "b");

            Assert.AreEqual(false, testCase.GetDependents("a").Contains("b"));
        }

        /// <summary>
        /// Using duplicates, test to make sure accurate size is reported.
        /// </summary>
        [TestMethod]
        public void TestSize()
        {
            testCase = new DependencyGraph();

            testCase.AddDependency("a", "b");
            testCase.AddDependency("a", "c");
            testCase.AddDependency("a", "c");
            testCase.AddDependency("b", "a");

            Assert.AreEqual(3, testCase.Size);
        }

        /// <summary>
        /// Tests the boolean expression to see if a given element has dependees
        /// </summary>
        [TestMethod]
        public void TestHasDependees()
        {
            testCase = new DependencyGraph();

            testCase.AddDependency("a", "b");
            testCase.AddDependency("a", "c");

            Assert.AreEqual(true, testCase.HasDependees("c"));
            Assert.AreEqual(true, testCase.HasDependees("b"));
        }

        /// <summary>
        /// Tests the boolean expression to see if a given element has dependents, with duplicates
        /// </summary>
        [TestMethod]
        public void TestHasDependents()
        {
            testCase = new DependencyGraph();

            testCase.AddDependency("a", "b");
            testCase.AddDependency("a", "c");
            testCase.AddDependency("b", "b");
            testCase.AddDependency("c", "z");

            Assert.AreEqual(true, testCase.HasDependents("a"));
            Assert.AreEqual(true, testCase.HasDependents("b"));
        }

        /// <summary>
        /// With duplicates, verify list return contains a set of dependees.
        /// </summary>
        [TestMethod]
        public void TestGetDependees()
        {
            testCase = new DependencyGraph();

            testCase.AddDependency("a", "b");
            testCase.AddDependency("a", "c");
            testCase.AddDependency("a", "b");
            testCase.AddDependency("a", "c");

            Assert.AreEqual("a", testCase.GetDependees("b").ElementAt(0));
            Assert.AreEqual("a", testCase.GetDependees("c").ElementAt(0));
        }

        /// <summary>
        /// With duplicates, verify list return contains a set of dependents.
        /// </summary>
        [TestMethod]
        public void TestGetDependents()
        {
            testCase = new DependencyGraph();

            testCase.AddDependency("a", "b");
            testCase.AddDependency("a", "c");
            testCase.AddDependency("a", "b");
            testCase.AddDependency("a", "c");

            IEnumerable<string> result = new List<string>() { "b", "c" };

            Assert.AreEqual(result.ElementAt(1), testCase.GetDependents("a").ElementAt(1));
            Assert.AreEqual(result.ElementAt(0), testCase.GetDependents("a").ElementAt(0));
        }

        /// <summary>
        /// Verify replace dependees by changing multiple values and checking thier results
        /// </summary>
        [TestMethod]
        public void TestReplaceDependees()
        {
            testCase = new DependencyGraph();

            testCase.AddDependency("a", "b");
            testCase.AddDependency("a", "c");
            testCase.AddDependency("a", "d");
            testCase.AddDependency("a", "e"); //change a to x
            testCase.AddDependency("e", "b");
            testCase.AddDependency("f", "c");
            testCase.AddDependency("g", "d");
            testCase.AddDependency("i", "e"); //change a to z

            IEnumerable<string> result = new List<string>() { "x", "z" };
            testCase.ReplaceDependees("e", result);

            Assert.AreEqual(result.ElementAt(1), testCase.GetDependees("e").ElementAt(1));
            Assert.AreEqual(result.ElementAt(0), testCase.GetDependees("e").ElementAt(0));
        }

        /// <summary>
        /// Verify replace dependents by changing multiple values and checking thier results
        /// </summary>
        [TestMethod]
        public void TestReplaceDependents()
        {
            testCase = new DependencyGraph();

            testCase.AddDependency("a", "b"); //change b to w
            testCase.AddDependency("a", "c"); //change c to x
            testCase.AddDependency("a", "d"); //change d to y
            testCase.AddDependency("a", "e"); //change e to z
            testCase.AddDependency("e", "b");
            testCase.AddDependency("f", "c");
            testCase.AddDependency("g", "d");
            testCase.AddDependency("i", "e");

            IEnumerable<string> result = new List<string>() { "w", "x", "y", "z" };
            testCase.ReplaceDependents("a", result);

            Assert.AreEqual(result.ElementAt(0), testCase.GetDependents("a").ElementAt(0));
            Assert.AreEqual(result.ElementAt(1), testCase.GetDependents("a").ElementAt(1));
            Assert.AreEqual(result.ElementAt(2), testCase.GetDependents("a").ElementAt(2));
            Assert.AreEqual(result.ElementAt(3), testCase.GetDependents("a").ElementAt(3));
        }

        /// <summary>
        /// Add a large number of dependencies and verify the size is correct
        /// </summary>
        [TestMethod]
        public void TestSizeLarge()
        {
            testCase.AddDependency("a", "b");
            testCase.AddDependency("b", "b");
            testCase.AddDependency("c", "c");
            testCase.AddDependency("d", "b");
            testCase.AddDependency("e", "b");
            testCase.AddDependency("f", "b");
            testCase.AddDependency("g", "b");
            testCase.AddDependency("h", "b");
            testCase.AddDependency("i", "b");
            testCase.AddDependency("j", "b");
            testCase.AddDependency("k", "c");
            testCase.AddDependency("l", "x");
            testCase.AddDependency("m", "a");
            testCase.AddDependency("n", "a");
            testCase.AddDependency("o", "b");
            testCase.AddDependency("p", "b");
            testCase.AddDependency("q", "b");
            testCase.AddDependency("r", "k");
            testCase.AddDependency("s", "r");
            testCase.AddDependency("b", "a");

            Assert.AreEqual(20, testCase.Size);
        }

        /// <summary>
        /// Single test to verify no exception thrown when non existent element is requested.
        /// </summary>
        [TestMethod]
        public void TestHasDependeesComplex()
        {
            testCase = new DependencyGraph();

            testCase.AddDependency("a", "c");

            Assert.AreEqual(false, testCase.HasDependees("a"));
        }

        /// <summary>
        ///  Single test to verify no exception thrown when non existent element is requested.
        /// </summary>
        [TestMethod]
        public void TestHasDependentsComplex()
        {
            testCase = new DependencyGraph();

            testCase.AddDependency("a", "c");

            Assert.AreEqual(false, testCase.HasDependents("c"));
        }

        /// <summary>
        /// Test to remove a non-existent dependency
        /// </summary>
        [TestMethod]
        public void TestRemoveDependeesComplex()
        {
            testCase = new DependencyGraph();

            testCase.AddDependency("a", "b");
            testCase.AddDependency("a", "c");

            IEnumerable<string> result = new List<string>() { "b", "c" };

            testCase.RemoveDependency("c", "a");

            Assert.AreEqual(true, testCase.GetDependents("a").Contains("b"));
        }

        /// <summary>
        /// Test to see if false flag is thrown if elment does not exist upon non existent removal.
        /// </summary>
        [TestMethod]
        public void TestRemoveDependentsComplex()
        {
            testCase = new DependencyGraph();

            testCase.AddDependency("a", "b");
            testCase.AddDependency("a", "c");

            IEnumerable<string> result = new List<string>() { "b", "c" };

            testCase.RemoveDependency("x", "y");

            Assert.AreEqual(false, testCase.GetDependents("a").Contains("y"));
        }

        /// <summary>
        /// Test that each method throws the appropriate null exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTests()
        {
            DependencyGraph t = new DependencyGraph();
            t.HasDependents(null);
        }

        /// <summary>
        /// Test that each method throws the appropriate null exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTests2()
        {
            DependencyGraph t = new DependencyGraph();
            t.HasDependees(null);
        }

        /// <summary>
        /// Test that each method throws the appropriate null exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTests3()
        {
            DependencyGraph t = new DependencyGraph();
            t.GetDependees(null);
        }

        /// <summary>
        /// Test that each method throws the appropriate null exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTests4()
        {
            DependencyGraph t = new DependencyGraph();
            t.GetDependents(null);
        }

        /// <summary>
        /// Test that each method throws the appropriate null exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTests5()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", null);
        }

        /// <summary>
        /// Test that each method throws the appropriate null exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTests6()
        {
            DependencyGraph t = new DependencyGraph();
            t.RemoveDependency("b", null);
        }

        /// <summary>
        /// Test that each method throws the appropriate null exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTests7()
        {
            DependencyGraph t = new DependencyGraph();
            t.ReplaceDependees(null, null);
        }

        /// <summary>
        /// Test that each method throws the appropriate null exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTests8()
        {
            DependencyGraph t = new DependencyGraph();
            t.ReplaceDependents(null, null);
        }

        /// <summary>
        /// Test that each method throws the appropriate null exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTests9()
        {
            DependencyGraph t = new DependencyGraph();
            HashSet<string> b = new HashSet<string>(){ "a", null };
            t.ReplaceDependees("a", b);
        }

        /// <summary>
        /// Test that each method throws the appropriate null exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTests10()
        {
            DependencyGraph t = new DependencyGraph();
            HashSet<string> b = new HashSet<string>() { "a", null };
            t.ReplaceDependents("b", b);
        }

        /// <summary>
        /// Check to see if copied dependency list is equal to the first
        /// </summary>
        [TestMethod]
        public void IsEqualCopy()
        {
            testCase = new DependencyGraph();
            testCase.AddDependency("a", "b");
            testCase.AddDependency("b", "b");
            testCase.AddDependency("c", "c");
            testCase.AddDependency("d", "b");
            testCase.AddDependency("e", "b");
            testCase.AddDependency("f", "b");
            testCase.AddDependency("g", "b");
            testCase.AddDependency("h", "b");
            testCase.AddDependency("i", "b");
            testCase.AddDependency("j", "b");
            testCase.AddDependency("k", "c");
            testCase.AddDependency("l", "x");
            testCase.AddDependency("m", "a");
            testCase.AddDependency("n", "a");
            testCase.AddDependency("o", "b");
            testCase.AddDependency("p", "b");
            testCase.AddDependency("q", "b");
            testCase.AddDependency("r", "k");
            testCase.AddDependency("s", "r");
            testCase.AddDependency("b", "a");

            DependencyGraph testCase2 = new DependencyGraph(testCase);

            Assert.AreEqual(testCase.Size, testCase2.Size);
            Assert.AreEqual(testCase.GetDependees("a"), testCase2.GetDependees("a"));
            Assert.AreEqual(testCase.GetDependees("b"), testCase2.GetDependees("b"));

            testCase.ReplaceDependees("b", new HashSet<string>(){"x"});

            Assert.AreNotEqual(testCase.GetDependees("b"), testCase2.GetDependees("b"));
        }



    }
}