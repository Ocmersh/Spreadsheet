using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using System.Xml;
using Dependencies;
using Formulas;
using System.Xml.Serialization;


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
        private Regex IsValid;

        // ADDED FOR PS6. Code Added.
        /// <summary>
        /// True if this spreadsheet has been modified since it was created or saved
        /// (whichever happened most recently); false otherwise.
        /// </summary>
        public override bool Changed { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }

        /// <summary>
        /// Creates an empty Spreadsheet whose IsValid regular expression accepts every string.
        /// </summary>
        public Spreadsheet()
        {
            basicSheetCells = new Dictionary<string, SheetCell>();
            sheetDependencyGraph = new DependencyGraph();
            IsValid = new Regex(".*");
        }

        //Added for PS6. Code Added.
        /// <summary>
        /// Creates an empty Spreadsheet whose IsValid regular expression is provided as the parameter
        /// </summary>
        /// <param name="isValid"></param>
        public Spreadsheet(Regex isValid)
        {
            basicSheetCells = new Dictionary<string, SheetCell>();
            sheetDependencyGraph = new DependencyGraph();
            this.IsValid = isValid;
        }

        //Added for PS6
        /// Creates a Spreadsheet that is a duplicate of the spreadsheet saved in source.
        ///
        /// See the AbstractSpreadsheet.Save method and Spreadsheet.xsd for the file format 
        /// specification.  
        ///
        /// If there's a problem reading source, throws an IOException.
        ///
        /// Else if the contents of source are not consistent with the schema in Spreadsheet.xsd, 
        /// throws a SpreadsheetReadException.  
        ///
        /// Else if the IsValid string contained in source is not a valid C# regular expression, throws
        /// a SpreadsheetReadException.  (If the exception is not thrown, this regex is referred to
        /// below as oldIsValid.)
        ///
        /// Else if there is a duplicate cell name in the source, throws a SpreadsheetReadException.
        /// (Two cell names are duplicates if they are identical after being converted to upper case.)
        ///
        /// Else if there is an invalid cell name or an invalid formula in the source, throws a 
        /// SpreadsheetReadException.  (Use oldIsValid in place of IsValid in the definition of 
        /// cell name validity.)
        ///
        /// Else if there is an invalid cell name or an invalid formula in the source, throws a
        /// SpreadsheetVersionException.  (Use newIsValid in place of IsValid in the definition of
        /// cell name validity.)
        ///
        /// Else if there's a formula that causes a circular dependency, throws a SpreadsheetReadException. 
        ///
        /// Else, create a Spreadsheet that is a duplicate of the one encoded in source except that
        /// the new Spreadsheet's IsValid regular expression should be newIsValid.
        public Spreadsheet(TextReader source, Regex newIsValid)
        {
            try {source.Peek();}
            catch (Exception e)
            {
                throw new IOException();
            }




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
            if (basicSheetCells.ContainsKey(name)) return basicSheetCells[name].GetContent();
            else return "";
        }

        /// <summary>
        /// Checks to see if a given input is a valid name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
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
                        if (count != validCheck.Length) return true;
                        else return false;
                    }
                    else return true;
                }
                else return true;
            }
            else return true;
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
        protected override ISet<String> SetCellContents(String name, double number)
        {
            //null and invalid check
            if (name == null || IsInvalid(name)) throw new InvalidNameException();

            //store new content in cell, if cell is empty, initialize it
            if (basicSheetCells.ContainsKey(name)) basicSheetCells[name].SetContent(number);
            else basicSheetCells.Add(name, new SheetCell(name, number));

            //create an ISet of the affected dependents
            ISet<String> names = new HashSet<string>(sheetDependencyGraph.GetDependents(name));

            //get new dependent list
            HashSet<string> dependentSet = new HashSet<string>() {name};

            //add each dependent and each dependents dependent to the list
            foreach (var dependent in sheetDependencyGraph.GetDependents(name))
            {
                GetDependentSet(dependent, dependentSet);
            }

            return dependentSet;
        }

        /// <summary>
        /// Helper method to get the dependents of a dependent.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dependentSet"></param>
        /// <returns></returns>
        private ISet<string> GetDependentSet(string name, HashSet<string> dependentSet)
        {
            //add dependent
            dependentSet.Add(name);

            //get and add each dependent of each dependent
            foreach (var dependent in sheetDependencyGraph.GetDependents(name))
            {
                dependentSet.Add(dependent);
                if (sheetDependencyGraph.HasDependents(dependent))
                    foreach (var subDep in sheetDependencyGraph.GetDependents(name))
                        //recursive call
                        GetDependentSet(subDep, dependentSet);
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
        protected override ISet<String> SetCellContents(String name, String text)
        {
            //null and invalid check
            if (text == null) throw new ArgumentNullException();
            else if (name == null || IsInvalid(name))
                throw new InvalidNameException();

            //empty set to be returned later
            HashSet<string> dependentSet = new HashSet<string>() {name};

            //if empty, return without adding cell to 'active' cells
            if (basicSheetCells.ContainsKey(name) && text.Trim().Equals(""))
            {
                basicSheetCells.Remove(name);
                return dependentSet;
            }
            else if (text.Trim().Equals(""))
                return dependentSet;

            //store new content in cell, if cell is empty, initialize it
            if (basicSheetCells.ContainsKey(name)) basicSheetCells[name].SetContent(text);
            else basicSheetCells.Add(name, new SheetCell(name, text));

            //create an ISet of the affected dependents
            ISet<String> names = new HashSet<string>(sheetDependencyGraph.GetDependents(name));

            //get new dependent list
            foreach (var dependent in sheetDependencyGraph.GetDependents(name))
            {
                GetDependentSet(dependent, dependentSet);
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
        protected override ISet<String> SetCellContents(String name, Formula formula)
        {
            //rollback var if exception is thrown
            Dictionary<string, SheetCell> sheetRollback = new Dictionary<string, SheetCell>();

            //loop to generate 'deep' copies
            foreach (KeyValuePair<string, SheetCell> entry in basicSheetCells)
                sheetRollback.Add(entry.Key, new SheetCell(entry.Key, entry.Value.GetContent()));

            //rollback for dependency graph
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
            if (basicSheetCells.ContainsKey(name)) basicSheetCells[name].SetContent(formula);
            else basicSheetCells.Add(name, new SheetCell(name, formula));

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
                GetDependentSet(dependent, dependentSet);
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
            if (name == null) throw new ArgumentNullException();
            else if (IsInvalid(name))
                throw new InvalidNameException();

            return sheetDependencyGraph.GetDependents(name);
        }

        // ADDED FOR PS6. Code Added
        /// <summary>
        /// Writes the contents of this spreadsheet to dest using an XML format.
        /// The XML elements should be structured as follows:
        ///
        /// <spreadsheet IsValid="IsValid regex goes here">
        ///   <cell name="cell name goes here" contents="cell contents go here"></cell>
        ///   <cell name="cell name goes here" contents="cell contents go here"></cell>
        ///   <cell name="cell name goes here" contents="cell contents go here"></cell>
        /// </spreadsheet>
        ///
        /// The value of the IsValid attribute should be IsValid.ToString()
        /// 
        /// There should be one cell element for each non-empty cell in the spreadsheet.
        /// If the cell contains a string, the string (without surrounding double quotes) should be written as the contents.
        /// If the cell contains a double d, d.ToString() should be written as the contents.
        /// If the cell contains a Formula f, f.ToString() with "=" prepended should be written as the contents.
        ///
        /// If there are any problems writing to dest, the method should throw an IOException.
        /// </summary>
        public override void Save(TextWriter dest)
        {
            try
            {
                using (XmlWriter writer = XmlWriter.Create(dest))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("spreadsheet");
                    writer.WriteAttributeString("IsValid", IsValid.ToString());


                    //write each cell to new line
                    foreach (var cell in basicSheetCells)
                    {
                        if (cell.Value.GetContent() is Formula)
                        {
                            writer.WriteStartElement("cell");
                            writer.WriteAttributeString("name", cell.Key);
                            writer.WriteAttributeString("contents", "="+((Formula) cell.Value.GetContent()));
                            writer.WriteEndElement();
                        }
                        else if ((cell.Value.GetContent() is double))
                        {
                            writer.WriteStartElement("cell");
                            writer.WriteAttributeString("name", cell.Key);
                            writer.WriteAttributeString("contents", ((double) cell.Value.GetContent()).ToString());
                            writer.WriteEndElement();
                        }
                        else if ((cell.Value.GetContent() is string))
                        {
                            writer.WriteStartElement("cell");
                            writer.WriteAttributeString("name", cell.Key);
                            writer.WriteAttributeString("contents", ((string)cell.Value.GetContent()));
                            writer.WriteEndElement();
                        }
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
            catch (Exception e)
            {
                throw new IOException();
            }
        }

        // ADDED FOR PS6. Code Added
        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        ///
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a FormulaError.
        /// </summary>
        public override object GetCellValue(string name)
        {
            if (name == null || IsInvalid(name))
                throw new InvalidNameException();

            if (basicSheetCells[name].GetContent() is string)
                return (string) basicSheetCells[name].GetContent();
            else if (basicSheetCells[name].GetContent() is double)
                return (double) basicSheetCells[name].GetContent();
            else if (basicSheetCells[name].GetContent() is Formula)
                return ((Formula) basicSheetCells[name].GetContent()).Evaluate(Lookup); //return value that is calculated

            return "";
        }

        /// <summary>
        /// Takes a variable name as a parameter and returns its value.
        /// </summary>
        /// <param name="eval"></param>
        /// <returns></returns>
        public double Lookup(String eval)
        {
            if(!basicSheetCells.ContainsKey(eval))
                throw new UndefinedVariableException(eval);

            double result = 0.0;

            //check to see if variable contains a formula, evaluate that formula first before proceeding.
            if(basicSheetCells[eval].GetContent() is Formula && ((Formula) basicSheetCells[eval].GetContent()).GetVariables().Count > 0)
                result = ((Formula)basicSheetCells[eval].GetContent()).Evaluate(Lookup);
            else
                {
                    //if a double value, return it
                    if (basicSheetCells[eval].GetContent() is double)
                        result = (double) basicSheetCells[eval].GetContent();
                    //if string does not parse to double, create new error
                    else if (!double.TryParse(basicSheetCells[eval].GetContent().ToString(), out result))
                        new FormulaError(eval);
                }

            return result;
        }

        // ADDED FOR PS6. Code Added.
        /// <summary>
        /// If content is null, throws an ArgumentNullException.
        ///
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        ///
        /// Otherwise, if content parses as a double, the contents of the named
        /// cell becomes that double.
        ///
        /// Otherwise, if content begins with the character '=', an attempt is made
        /// to parse the remainder of content into a Formula f using the Formula
        /// constructor with s => s.ToUpper() as the normalizer and a validator that
        /// checks that s is a valid cell name as defined in the AbstractSpreadsheet
        /// class comment.  There are then three possibilities:
        ///
        ///   (1) If the remainder of content cannot be parsed into a Formula, a
        ///       Formulas.FormulaFormatException is thrown.
        ///
        ///   (2) Otherwise, if changing the contents of the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown.
        ///
        ///   (3) Otherwise, the contents of the named cell becomes f.
        ///
        /// Otherwise, the contents of the named cell becomes content.
        ///
        /// If an exception is not thrown, the method returns a set consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell.
        ///
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        public override ISet<string> SetContentsOfCell(string name, string content)
        {
            //exception check
            if (content == null)
                throw new ArgumentNullException();
            if (name == null || IsInvalid(name))
                throw new InvalidNameException();

            //temp variable to check and store double value
            double doubleResult;

            //try to parse double, if so set it
            if (double.TryParse(content, out doubleResult))
                return SetCellContents(name, doubleResult);

            //temp variable for new formula in the form of a string
            string stringResult = "";

            //try to parse formula, if so set it
            if (content.ToCharArray()[0].Equals("="))
            {
                char[] formParse = content.ToCharArray();
                for(int current = 1; current < formParse.Length; current++)
                    stringResult += formParse[current];

                Formula newForm = new Formula(stringResult, s => s.ToUpper(), Validator);

                return SetCellContents(name, newForm);
            }

            //if you've gotten to the point, set string
            return SetCellContents(name, content);
        }

        /// <summary>
        /// Simple validator to identify that key is a valid cell name.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public bool Validator(string s)
        {
            return !IsInvalid(s);
        }

        /// <summary>
        /// Helper cell class to hold information.
        /// </summary>
        private class SheetCell
        {
            /// contents should be either a string, a double, or a Formula.
            private string cellName;
            private object contentString;

            public SheetCell(string name, object newContent)
            {
                cellName = name;
                contentString = newContent;
            }

            //simple method for getting contents
            public object GetContent()
            {
                return contentString;
            }

            //simple method for setting the contents
            public void SetContent(object newContent)
            {
                contentString = newContent;
            }
        }
    }
}
