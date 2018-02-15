// Skeleton written by Joe Zachary for CS 3500, January 2017
// Main body written by Bryce Hansen for 3500, 2/8/18 / U0804551

using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;

namespace Formulas
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  Provides a means to evaluate Formulas.  Formulas can be composed of
    /// non-negative floating-point numbers, variables, left and right parentheses, and
    /// the four binary operator symbols +, -, *, and /.  (The unary operators + and -
    /// are not allowed.)
    /// </summary>
    public struct Formula
    {
        private IEnumerable<string> userForm;
    
        /// <summary>
        /// Creates a Formula from a string that consists of a standard infix expression composed
        /// from non-negative floating-point numbers (using C#-like syntax for double/int literals), 
        /// variable symbols (a letter followed by zero or more letters and/or digits), left and right
        /// parentheses, and the four binary operator symbols +, -, *, and /.  White space is
        /// permitted between tokens, but is not required.
        /// 
        /// Examples of a valid parameter to this constructor are:
        ///     "2.5e9 + x5 / 17"
        ///     "(5 * 2) + 8"
        ///     "x*y-2+35/9"
        ///     
        /// Examples of invalid parameters are:
        ///     "_"
        ///     "-5.3"
        ///     "2 5 + 3"
        /// 
        /// If the formula is syntacticaly invalid, throws a FormulaFormatException with an 
        /// explanatory Message.
        /// </summary>
        /// <exception cref="FormulaFormatException"></exception>
        public Formula(String formula)
        {
            if(formula == null)
                throw new ArgumentNullException();

            //parse out tokens
            userForm = GetTokens(formula);

            //check at least one token   
            if (userForm.ToArray().Length == 0)
                throw new FormulaFormatException("The formula is empty");

            //check for invalid type (i.e. negative number on start)
            else if (userForm.ElementAt(0).Equals("-"))
                throw new FormulaFormatException("The first token cannot be a negative number");

            //check for first token; must be a number, a variable, or an opening parenthesis.
            else if (!IsFirstValid(userForm))
                throw new FormulaFormatException("The first token is not valid.");

            //check for the last token; must be a number, a variable, or a closing parenthesis.
            else if (!IsLastValid(userForm))
                throw new FormulaFormatException("The last token is not valid.");

            //check #opening pren == #closing pren (when read left to right)
            else if (!IsPrenBalanced(userForm))
                throw new FormulaFormatException(
                    "The number of opening and closing parenthesis, when read left to right, is not balanced.");

            //token after open pren or operator must be; a number, a variable, or an opening parenthesis.
            else if (!IsOpenTrailingVarValid(userForm))
                throw new FormulaFormatException(
                    "The token after an open parenthesis or operator was not a number, a variable or open parenthesis.");

            //token that after a number, a variable, or a closing parenthesis must be;  an operator or a closing parenthesis.
            else if (!IsClosingTrailingVarValid(userForm))
                throw new FormulaFormatException(
                    "The token after a number, variable or closing parenthesis an operator or closing parenthesis.");
        }

        public Formula(String formula, Normalizer norm, Validator valid)
        {
            if (formula == null || norm == null || valid == null)
                throw new ArgumentNullException();

            this = new Formula(formula);

            string normForm = norm(string.Join("", userForm.ToArray()));
            userForm = GetTokens(normForm);

            if (!valid(normForm))
            {
                throw new FormulaFormatException(
                    "The formula is not normalized!");
            }

        }

        /// <summary>
        /// Evaluates this Formula, using the Lookup delegate to determine the values of variables.  (The
        /// delegate takes a variable name as a parameter and returns its value (if it has one) or throws
        /// an UndefinedVariableException (otherwise).  Uses the standard precedence rules when doing the evaluation.
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, its value is returned.  Otherwise, throws a FormulaEvaluationException  
        /// with an explanatory Message.
        /// </summary>
        public double Evaluate(Lookup lookup)
        {
            Stack<string> operatorStack = new Stack<string>();
            Stack<double> valueStack = new Stack<double>();
            var tempV = 0.0;
            foreach (string token in userForm)
            {
                if (double.TryParse(token, out tempV))
                {
                    //If * or / is at the top of the operator stack, 
                    if (operatorStack.Count != 0 &&
                        (operatorStack.Peek().Equals("*") || operatorStack.Peek().Equals("/")))
                    {
                        //pop the value stack, pop the operator stack, and 
                        //apply the popped operator to tempV and the popped number. Push the result onto the value stack.
                        valueStack.Push(computeNum(operatorStack.Pop(), valueStack.Pop(), tempV));
                    }
                    else
                        //Otherwise, push tempV onto the value stack
                        valueStack.Push(tempV);
                }
                else if (IsVariable(token))
                {
                    double varVal = 0.0;
                    try
                    {
                        //throws if Looking up up t reveals it has no value
                        varVal = lookup(token);
                    }
                    catch (UndefinedVariableException e)
                    {
                        throw new FormulaEvaluationException("Variables must have a value");
                    }

                    //If * or / is at the top of the operator stack, 
                    if (operatorStack.Count != 0 &&
                        (operatorStack.Peek().Equals("*") || operatorStack.Peek().Equals("/")))
                    {
                        //pop the value stack, pop the operator stack, and 
                        //apply the popped operator to varVal and the popped number. Push the result onto the value stack.
                        valueStack.Push(computeNum(operatorStack.Pop(), valueStack.Pop(), varVal));
                    }
                    else
                        //Otherwise, push varVal onto the value stack
                        valueStack.Push(varVal);
                }
                else if (token.Equals("+") || token.Equals("-"))
                {
                    //If + or - is at the top of the operator stack, pop the value stack 
                    //twice and the operator stack once. 
                    //Push the result onto the value stack.
                    if (operatorStack.Count != 0 &&
                        (operatorStack.Peek().Equals("+") || operatorStack.Peek().Equals("-")))
                        valueStack.Push(computeNum(operatorStack.Pop(), valueStack.Pop(), valueStack.Pop()));

                    //Whether or not you did the first step, push token onto the operator stack
                    operatorStack.Push(token);
                }
                else if (token.Equals("*") || token.Equals("/"))
                {
                    //Push token onto the operator stack
                    operatorStack.Push(token);
                }
                else if (token.Equals("("))
                {
                    //Push t onto the operator stack
                    operatorStack.Push(token);
                }
                else if (token.Equals(")"))
                {
                    //If + or - is at the top of the operator stack, pop the value stack twice and the operator stack once. 
                    //Apply the popped operator to the popped numbers. Push the result onto the value stack.
                    if (operatorStack.Count != 0 &&
                        (operatorStack.Peek().Equals("+") || operatorStack.Peek().Equals("-")))
                        valueStack.Push(computeNum(operatorStack.Pop(), valueStack.Pop(), valueStack.Pop()));

                    //Whether or not you did the first step, the top of the operator stack will be a(. Pop it.
                    operatorStack.Pop();

                    // if *or / is at the top of the operator stack, pop the value stack twice and the operator stack once. Apply the popped operator to the
                    //popped numbers. Push the result onto the value stack.
                    if (operatorStack.Count != 0 &&
                        (operatorStack.Peek().Equals("*") || operatorStack.Peek().Equals("/")))
                    {
                        valueStack.Push(computeNum(operatorStack.Pop(), valueStack.Pop(), valueStack.Pop()));
                    }
                }
            }

            if (operatorStack.Count == 0)
            {
                //Pop it and report as the value of the expression
                return valueStack.Pop();
            }
            else if (operatorStack.Count > 0)
            {
                //Apply the last operator to the two last values 
                //and report the result as the value of the expression.
                return computeNum(operatorStack.Pop(), valueStack.Pop(), valueStack.Pop());
            }

            return tempV;
        }

        /// <summary>
        /// Checks for the first token; must be a number, a variable, or an opening parenthesis.
        /// </summary>
        /// <param name="userInput"></param>
        /// <returns>bool true if a valid token, false if not</returns>
        private bool IsFirstValid(IEnumerable<string> userInput)
        {
            string token = userInput.ElementAt(0);
            double n;
            if (double.TryParse(token, out n) || IsVariable(token) || token.Equals("("))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Checks for the last token, must be a number, a variable, or a closing parenthesis.
        /// </summary>
        /// <param name="userInput"></param>
        /// <returns>bool true if a valid token, false if not</returns>
        private bool IsLastValid(IEnumerable<string> userInput)
        {
            string token = userInput.Last();
            double n;
            if (double.TryParse(token, out n) || IsVariable(token) || token.Equals(")"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Checks to ensure that the number of opening parenthesis is equal to the number of closing pren (when read left to right).
        /// </summary>
        /// <param name="userInput"></param>
        /// <returns>bool true if a valid token, false if not</returns>
        private bool IsPrenBalanced(IEnumerable<string> userInput)
        {
            int openCounter = 0;
            int closedCounter = 0;
            foreach (string token in userInput)
            {
                if (token.Equals("("))
                    openCounter++;
                else if (token.Equals(")"))
                    closedCounter++;
                if (closedCounter > openCounter) return false;
            }

            if (openCounter == closedCounter)
                return true;
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks the token after an open pren or operator, it must be; a number, a variable, or an opening parenthesis.
        /// </summary>
        /// <param name="userInput"></param>
        /// <returns>bool true if a valid token, false if not</returns>
        private bool IsOpenTrailingVarValid(IEnumerable<string> userInput)
        {
            int counter = 0;
            double tempV = 0;
            foreach (string token in userInput)
            {
                if (token.Equals("(") || token.Equals("+") || token.Equals("-") || token.Equals("/") ||
                    token.Equals("*"))
                {
                    string nextVar = userInput.ElementAt(counter + 1);
                    if (!(double.TryParse(nextVar, out tempV) || IsVariable(nextVar) || nextVar.Equals("(")))
                        return false;
                }

                counter++;
            }

            return true;
        }

        /// <summary>
        /// Checks the token that after a number, a variable, or a closing parenthesis, and it must be;  an operator or a closing parenthesis.
        /// </summary>
        /// <param name="userInput"></param>
        /// <returns>bool true if a valid token, false if not</returns>
        private bool IsClosingTrailingVarValid(IEnumerable<string> userInput)
        {
            int counter = 0;
            double tempV = 0;
            foreach (string token in userInput)
            {
                if (token.Equals(")") || IsVariable(token) || double.TryParse(token, out tempV))
                {
                    if (counter + 1 >= userInput.Count()) return true;
                    string nextVar = userInput.ElementAt(counter + 1);
                    if (!(nextVar.Equals(")") || nextVar.Equals("+") || nextVar.Equals("-") || nextVar.Equals("/") ||
                          nextVar.Equals("*")))
                        return false;
                }

                counter++;
            }

            return true;
        }

        /// <summary>
        /// Checks to see if the given token is a variable, defined as: a letter followed by zero or more letters and/or digits
        /// </summary>
        /// <param name="token"></param>
        /// <returns>True if a variable, falso otherwise</returns>
        private bool IsVariable(string token)
        {
            Char[] posVar = token.ToCharArray();
            Char firstLetter = posVar[0];
            int holding = 0;
            if (firstLetter >= 'A' && firstLetter <= 'Z' || firstLetter >= 'a' && firstLetter <= 'z')
                foreach (char subToken in posVar)
                {
                    if ((int.TryParse((subToken.ToString()), out holding) ||
                         ((subToken >= 'A' && subToken <= 'Z' || subToken >= 'a' && subToken <= 'z'))))
                        continue;
                    else
                        return false;
                }
            else
                return false;

            return true;
        }

        /// <summary>
        /// Computes a value given an operand represented as a string.
        /// </summary>
        /// <param name="operand"></param>
        /// <param name="num1"></param>
        /// <param name="num2"></param>
        /// <returns>The result as a double.</returns>
        private double computeNum(string operand, double num1, double num2)
        {
            num2 = Math.Round(num2);
            if (operand.Equals("+"))
            {
                return num1 + num2;
            }
            else if (operand.Equals("-"))
            {
                return num2 - num1;
            }
            else if (operand.Equals("*"))
            {
                return num1 * num2;
            }
            else
            {
                //throws if A division by zero results
                if (num1 == 0 || num2 == 0)
                    throw new FormulaEvaluationException("Cannot divide by Zero");
                else
                    return num1 / num2;
            }
        }

        /// <summary>
        /// Iterates through the formulas and returns an ISet of the list of variables in it.
        /// </summary>
        /// <returns>ISet</returns>
        public ISet<string> GetVariable()
        {
            ISet<string> varList = new HashSet<string>();

            foreach (var token in userForm)
            {
                if (IsVariable(token))
                    varList.Add(token);
            }

            return varList;
        }

        /// <summary>
        /// Returns a string version of the Formula(in normalized form).
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string formulaAsString = "";

            foreach (var token in userForm)
            {
                formulaAsString += token;
            }

            return formulaAsString;
        }

        /// <summary>
        /// Given a formula, enumerates the tokens that compose it.  Tokens are left paren,
        /// right paren, one of the four operator symbols, a string consisting of a letter followed by
        /// zero or more digits and/or letters, a double literal, and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens.
            // NOTE:  These patterns are designed to be used to create a pattern to split a string into tokens.
            // For example, the opPattern will match any string that contains an operator symbol, such as
            // "abc+def".  If you want to use one of these patterns to match an entire string (e.g., make it so
            // the opPattern will match "+" but not "abc+def", you need to add ^ to the beginning of the pattern
            // and $ to the end (e.g., opPattern would need to be @"^[\+\-*/]$".)
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z][0-9a-zA-Z]*";

            // PLEASE NOTE:  I have added white space to this regex to make it more readable.
            // When the regex is used, it is necessary to include a parameter that says
            // embedded white space should be ignored.  See below for an example of this.
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: e[\+-]?\d+)?";
            String spacePattern = @"\s+";

            // Overall pattern.  It contains embedded white space that must be ignored when
            // it is used.  See below for an example of this.  This pattern is useful for 
            // splitting a string into tokens.
            String splittingPattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})", lpPattern,
                rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            // PLEASE NOTE:  Notice the second parameter to Split, which says to ignore embedded white space
            /// in the pattern.
            foreach (String s in Regex.Split(formula, splittingPattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }
        }
    }

    /// <summary>
    /// A Lookup method is one that maps some strings to double values.  Given a string,
    /// such a function can either return a double (meaning that the string maps to the
    /// double) or throw an UndefinedVariableException (meaning that the string is unmapped 
    /// to a value. Exactly how a Lookup method decides which strings map to doubles and which
    /// don't is up to the implementation of the method.
    /// </summary>
    public delegate double Lookup(string var);
    public delegate string Normalizer(string s);
    public delegate bool Validator(string s);

    /// <summary>
    /// Used to report that a Lookup delegate is unable to determine the value
    /// of a variable.
    /// </summary>
    [Serializable]
    public class UndefinedVariableException : Exception
    {
        /// <summary>
        /// Constructs an UndefinedVariableException containing whose message is the
        /// undefined variable.
        /// </summary>
        /// <param name="variable"></param>
        public UndefinedVariableException(String variable) : base(variable)
        {
        }
    }

    /// <summary>
    /// Used to report syntactic errors in the parameter to the Formula constructor.
    /// </summary>
    [Serializable]
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message) : base(message)
        {
        }
    }

    /// <summary>
    /// Used to report errors that occur when evaluating a Formula.
    /// </summary>
    [Serializable]
    public class FormulaEvaluationException : Exception
    {
        /// <summary>
        /// Constructs a FormulaEvaluationException containing the explanatory message.
        /// </summary>
        public FormulaEvaluationException(String message) : base(message)
        {
        }
    }
}
