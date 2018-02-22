using System;
using System.Collections.Generic;
using System.Dynamic;
using Dependencies;
using Formulas;

namespace SS
{
    /// <summary>
    /// A string s is a valid cell name if and only if it consists of one or more letters, 
    /// followed by a non-zero digit, followed by zero or more digits.
    /// For example, "A15", "a15", "XY32", and "BC7" are valid cell names.  On the other hand, 
    /// "Z", "X07", and "hello" are not valid cell names.
    /// 
    /// The contents of a cell can be (1) a string, (2) a double, or (3) a Formula.  If the
    /// contents is an empty string, we say that the cell is empty.  (By analogy, the contents
    /// of a cell in Excel is what is displayed on the editing line when the cell is selected.)
    /// In an empty spreadsheet, the contents of every cell is the empty string.
    /// 
    /// The value of a cell can be (1) a string, (2) a double, or (3) a FormulaError.  
    /// 
    /// If a cell's contents is a string, its value is that string.
    /// 
    /// If a cell's contents is a double, its value is that double.
    ///
    ///  If a cell's contents is a Formula, its value is either a double or a FormulaError.
    /// The value of a Formula, of course, can depend on the values of variables.  The value 
    /// of a Formula variable is the value of the spreadsheet cell it names (if that cell's 
    /// value is a double) or is undefined (otherwise).  If a Formula depends on an undefined
    /// variable or on a division by zero, its value is a FormulaError.  Otherwise, its value
    /// is a double, as specified in Formula.Evaluate.
    /// 
    /// Spreadsheets are never allowed to contain a combination of Formulas that establish
    /// a circular dependency.  A circular dependency exists when a cell depends on itself.
    /// For example, suppose that A1 contains B1*2, B1 contains C1*2, and C1 contains A1*2.
    /// A1 depends on B1, which depends on C1, which depends on A1.  That's a circular
    /// dependency.
    /// </summary> 
    public class Spreadsheet : AbstractSpreadsheet
    {
        //Framework for the spreadsheet
        private Dictionary<string, SheetCell> basicSheetCells;
        private DependencyGraph sheetDependencyGraph;
        private HashSet<string> recalcCells = new HashSet<string>();

        /// <summary>
        /// Constructor
        /// </summary>
        public Spreadsheet()
        {
            basicSheetCells = new Dictionary<string, SheetCell>();
            sheetDependencyGraph = new DependencyGraph();
        }

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<String> GetNamesOfAllNonemptyCells()
        {
            return basicSheetCells.Keys;
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// contents should be either a string, a double, or a Formula.
        /// </summary>
        public override object GetCellContents(String name)
        {
            if (name == null || IsInvalid(name)) throw new InvalidNameException();
            if (basicSheetCells.ContainsKey(name))
                return basicSheetCells[name].GetContent();
            else
                return "";
        }

        private bool IsInvalid(string name)
        {
            //convert string to char array
            Char[] validCheck = name.ToCharArray();

            //if first isn't a letter, return
            if (Char.IsLetter(validCheck[0]))
            {
                int count = 0;

                //keep incrementing until a non letter is found.
                while (count < validCheck.Length && Char.IsLetter(validCheck[count])) count++;

                //check if non letter is a digit great than zero
                if (count < validCheck.Length && Char.IsDigit(validCheck[count]))
                {
                    int result = 0;
                    int.TryParse(validCheck[count].ToString(), out result);
                    if (result > 0)
                    {
                        //keep going until you run out of array
                        while (count < validCheck.Length && Char.IsDigit(validCheck[count])) count++;
                        if (count != validCheck.Length)
                            return true;
                        else
                            return false;
                    }
                    else
                        return true;
                }
                else
                    return true;
            }
            else
                return true;
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        public override ISet<String> SetCellContents(String name, double number)
        {
            if (name == null || IsInvalid(name)) throw new InvalidNameException();

            //store new content in cell, if cell is empty, initialize it
            if (basicSheetCells.ContainsKey(name))
                basicSheetCells[name].SetContent(number);
            else
                basicSheetCells.Add(name, new SheetCell(name, number));

            //create an ISet of the affected dependents
            ISet<String> names = new HashSet<string>(sheetDependencyGraph.GetDependents(name));

            //get new dependent list
            HashSet<string> dependentSet = new HashSet<string>() {name};

            //add each dependent and each dependents dependent to the list
            foreach (var dependent in sheetDependencyGraph.GetDependents(name))
            {
                getDependentSet(dependent, dependentSet);
            }

            return dependentSet;
        }

        /// <summary>
        /// Helper method to get the dependents of a dependent.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dependentSet"></param>
        /// <returns></returns>
        private ISet<string> getDependentSet(string name, HashSet<string> dependentSet)
        {
            //add dependent
            dependentSet.Add(name);

            //get and add each dependent of each dependent
            foreach (var dependent in sheetDependencyGraph.GetDependents(name))
            {
                dependentSet.Add(dependent);
                if (sheetDependencyGraph.HasDependents(dependent))
                    foreach (var subDep in sheetDependencyGraph.GetDependents(name))
                        getDependentSet(subDep, dependentSet);
            }

            return dependentSet;
        }

        /// <summary>
        /// If text is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        public override ISet<String> SetCellContents(String name, String text)
        {
            if (text == null)
                throw new ArgumentNullException();
            else if (name == null || IsInvalid(name))
                throw new InvalidNameException();
            HashSet<string> dependentSet = new HashSet<string>() {name};
            if (basicSheetCells.ContainsKey(name) && text.Trim().Equals(""))
            {
                basicSheetCells.Remove(name);
                return dependentSet;
            }
            else if (text.Trim().Equals(""))
                return dependentSet;

            //store new content in cell, if cell is empty, initialize it
            if (basicSheetCells.ContainsKey(name))
                basicSheetCells[name].SetContent(text);
            else
                basicSheetCells.Add(name, new SheetCell(name, text));

            //create an ISet of the affected dependents
            ISet<String> names = new HashSet<string>(sheetDependencyGraph.GetDependents(name));

            //get new dependent list
            foreach (var dependent in sheetDependencyGraph.GetDependents(name))
            {
                getDependentSet(dependent, dependentSet);
            }

            return dependentSet;
        }

        /// <summary>
        /// Requires that all of the variables in formula are valid cell names.
        /// 
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException.-*
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// Set consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        public override ISet<String> SetCellContents(String name, Formula formula)
        {
            Dictionary<string, SheetCell> sheetRollback = new Dictionary<string, SheetCell>();
            foreach (KeyValuePair<string, SheetCell> entry in basicSheetCells)
                sheetRollback.Add(entry.Key, new SheetCell(entry.Key, entry.Value.GetContent()));
            DependencyGraph dependRollback = new DependencyGraph(sheetDependencyGraph);
            if (name == null || IsInvalid(name)) throw new InvalidNameException();
            foreach (var variable in formula.GetVariables())
            {
                //find if variables are valid
                if (IsInvalid(variable)) throw new InvalidNameException();
                if (!basicSheetCells.ContainsKey(variable))
                    basicSheetCells.Add(variable, new SheetCell(variable, formula));
                sheetDependencyGraph.AddDependency(variable, name);
            }

            //store new content in cell, if cell is empty, initialize it
            if (basicSheetCells.ContainsKey(name))
                basicSheetCells[name].SetContent(formula);
            else
                basicSheetCells.Add(name, new SheetCell(name, formula));

            //create an ISet of the affected dependents
            ISet<String> names = new HashSet<string>(sheetDependencyGraph.GetDependents(name));

            //update each affected dependent
            try
            {
                GetCellsToRecalculate(names);
            }
            catch (CircularException)
            {
                basicSheetCells = sheetRollback;
                sheetDependencyGraph = dependRollback;
                throw new CircularException();
            }

            //get new dependent list
            HashSet<string> dependentSet = new HashSet<string>() {name};
            foreach (var dependent in sheetDependencyGraph.GetDependents(name))
            {
                getDependentSet(dependent, dependentSet);
            }

            return dependentSet;
        }

        /// <summary>
        /// If name is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name isn't a valid cell name, throws an InvalidNameException.
        /// 
        /// Otherwise, returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        protected override IEnumerable<String> GetDirectDependents(String name)
        {
            if (name == null)
                throw new ArgumentNullException();
            else if (IsInvalid(name))
                throw new InvalidNameException();
            return sheetDependencyGraph.GetDependents(name);
        }

        /// <summary>
        /// Help cell class to hold information.
        /// </summary>
        private class SheetCell
        {
            /// contents should be either a string, a double, or a Formula.
            public string cellName { get; set; }

            public object contentString { get; set; }

            public SheetCell(string name, object newContent)
            {
                cellName = name;
                contentString = newContent;
            }

            public object GetContent()
            {
                return contentString;
            }

            public void SetContent(object newContent)
            {
                contentString = newContent;
            }
        }
    }
}
