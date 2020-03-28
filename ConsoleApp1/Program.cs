﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace ConsoleApp1
{
    class Program
    {
        public static void Main(string[] args)
        {
            Dictionary<int, List<Element>> Terms = new Dictionary<int, List<Element>>();
            int termNum = 0;
            string input = "Fe+AgNO3=Ag+Fe(NO3)3";
            string[] halves = input.Split("=");
            string eq1 = halves[0];
            string eq2 = halves[1];
            string[] terms1 = eq1.Split("+");
            string[] terms2 = eq2.Split("+");

            foreach (var v in terms1)
            {
                termNum++;
                Terms.Add(termNum, getElements(v));
            }

            foreach (var v in terms2)
            {
                termNum++;
                Terms.Add(termNum, getElements(v));
            }
            

            /* matrix is            [A] | [B]
                term1    term2    term3 |   term4
             e1                         |
             e2                         |
             e3                         |
             e4                         |                            
             */
            Balence(Terms, termNum);
        }


        public static List<Element> getElements(String _molecule)
        {
            List<Element> list1 = new List<Element>();
            var findMatches = Regex.Matches(_molecule, @"\(?[A-Z][a-z]?\d*\)?"); // Get all elements
            Double endNumber = Double.Parse(Regex.IsMatch(_molecule, @"\)\d+")
                ? Regex.Match(_molecule, @"\)\d+").Value.Remove(0, 1)
                : "1"); // Finds the number after the ')'
            foreach (Match i in findMatches)
            {
                String element = Regex.Match(i.Value, "[A-Z][a-z]?").Value; // Gets the element
                Double amountOfElement = 0;
                if (!Regex.IsMatch(i.Value, @"[\(\)]"))
                    amountOfElement = Double.Parse(String.IsNullOrWhiteSpace(i.Value.Replace(element, ""))
                        ? "1"
                        : i.Value.Replace(element, ""));
                else
                {
                    if (!Double.TryParse(Regex.Replace(i.Value, @"(\(|\)|[A-Z]|[a-z])", ""), out amountOfElement))
                        amountOfElement =
                            endNumber; // If the element has either '(' or ')' and doesn't specify an amount, then set it equal to the endnumber
                    else
                        amountOfElement *=
                            endNumber; // If the element has either '(' or ')' and specifies an amount, then multiply it by the end number
                }

                list1.Add(new Element(element, Convert.ToInt32(amountOfElement)));
                //Console.WriteLine(element + " - " + amountOfElement);
            }

            return list1;
        }

        public static void Balence(Dictionary<int, List<Element>> terms, int termnum)
        {
            Dictionary<int, List<Element>> Terms = terms;
            int termNum = termnum;
            List<Element> elements = new List<Element>();


            foreach (var v in terms)
            {
                List<Element> j = v.Value;
                foreach (var h in j) //make a list of the elements that make up the eq
                {
                    Element wonderIfItsPresent = h;
                    bool containsItem = elements.Any(item => item.Name == wonderIfItsPresent.Name);
                    if (!containsItem)
                    {
                        elements.Add(h);
                    }
                }
            }

            //set up the matrix
            //[A]^-1 * B * Det[A] = answer
            //last term is matrix b
            // second to last term get multiplied by -1
            int matDim = termNum;
            int otherDim = elements.Count;
            int[,] Matrix = new int[otherDim, matDim];
            int column = 0;
            int row = 0;
            foreach (var e in elements) //going through each of the possible elements essentially the rows
            {
                foreach (var t in terms) //going through thr element in each term essentially the columns
                {
                    if (column < matDim)
                    {
                        int token = 0;

                        List<Element> list = t.Value; //setting specific term to a list
                        bool containsItem = list.Any(item => item.Name == e.Name);
                        if (containsItem)
                        {
                            foreach (var suicide in list) //going through each element in that list from the term
                            {
                                if (suicide.Name == e.Name)
                                {
                                    token = suicide.Number;
                                }

                                if (column == matDim - 2)
                                {
                                    token = token * (-1);
                                }
                            }
                        }
                        else
                        {
                            token = 0;
                        }

                        Matrix[row, column] = token;
                        if (column == matDim - 1)
                        {
                            row++;
                            column = 0;
                        }
                        else
                        {
                            column++;
                        }
                    }
                }
            }

            // [A] needs to have dimensions that are the number of possible elements x number of possible elements
            //[B] needs to have dimensoins that are the 1 x the number of elements
            int[,] A = new int[matDim, matDim];
            int[,] B = new int[matDim, 1];
            if (matDim == otherDim)
            {
                List<int[,]> matrices = splitWithOnes(Matrix, otherDim);
                A = matrices[0];
                B = matrices[1];
            }

            if (matDim == otherDim + 1)
            {
                splitWithoutOnes(Matrix, otherDim);
                List<int[,]> matrices = splitWithOnes(Matrix, otherDim);
                A = matrices[0];
                B = matrices[1];
            }
            
        }

        public static List<int[,]> splitWithOnes(int[,] matrix, int dim)
        {
            //dim = dim - 1;
            int[,] A = new int[dim, dim];
            int[,] B = new int[dim, 1];

            for (int i = 0; i < dim;)
            {
                Console.WriteLine("");
                for (int j = 0; j < dim;)
                {
                    if (j < dim - 1)
                    {
                        A[i, j] = matrix[i, j];
                    }
                    else
                    {
                        A[i, j] = 1;
                        B[i, 0] = matrix[i, j];
                    }

                    j++;
                }

                i++;
            }

            List<int[,]> matricies = new List<int[,]>();
            matricies.Add(A);
            matricies.Add(B);
            return matricies;
        }

        public static List<int[,]> splitWithoutOnes(int[,] matrix, int dim)
        {
            dim = dim - 1;
            int[,] A = new int[dim, dim];
            int[,] B = new int[dim, 1];

            for (int i = 0; i < dim;)
            {
                Console.WriteLine("");
                for (int j = 0; j < dim;)
                {
                    if (j < dim - 1)
                    {
                        A[i, j] = matrix[i, j];
                    }
                    else
                    {
                        B[i, 0] = matrix[i, j];
                    }

                    j++;
                }

                i++;
            }

            List<int[,]> matricies = new List<int[,]>();
            matricies.Add(A);
            matricies.Add(B);
            return matricies;
        }

        static double[][] MatrixProduct(double[][] matrixA, double[][] matrixB)
        {
            int aRows = matrixA.Length;
            int aCols = matrixA[0].Length;
            int bRows = matrixB.Length;
            int bCols = matrixB[0].Length;
            if (aCols != bRows)
                throw new Exception("Non-conformable matrices in MatrixProduct");

            double[][] result = MatrixCreate(aRows, bCols);

            for (int i = 0; i < aRows; ++i) // each row of A
            for (int j = 0; j < bCols; ++j) // each col of B
            for (int k = 0; k < aCols; ++k) // could use k less-than bRows
                result[i][j] += matrixA[i][k] * matrixB[k][j];

            return result;
        }

        static double[][] MatrixCreate(int rows, int cols)
        {
            double[][] result = new double[rows][];
            for (int i = 0; i < rows; ++i)
                result[i] = new double[cols];
            return result;
        }

        static double[][] MatrixInverse(double[][] matrix)
        {
            int n = matrix.Length;
            double[][] result = MatrixDuplicate(matrix);

            int[] perm;
            int toggle;
            double[][] lum = MatrixDecompose(matrix, out perm,
                out toggle);
            if (lum == null)
                throw new Exception("Unable to compute inverse");

            double[] b = new double[n];
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                {
                    if (i == perm[j])
                        b[j] = 1.0;
                    else
                        b[j] = 0.0;
                }

                double[] x = HelperSolve(lum, b);

                for (int j = 0; j < n; ++j)
                    result[j][i] = x[j];
            }

            return result;
        }

        static double[][] MatrixDuplicate(double[][] matrix)
        {
            // allocates/creates a duplicate of a matrix.
            double[][] result = MatrixCreate(matrix.Length, matrix[0].Length);
            for (int i = 0; i < matrix.Length; ++i) // copy the values
            for (int j = 0; j < matrix[i].Length; ++j)
                result[i][j] = matrix[i][j];
            return result;
        }

        static double[] HelperSolve(double[][] luMatrix, double[] b)
        {
            // before calling this helper, permute b using the perm array
            // from MatrixDecompose that generated luMatrix
            int n = luMatrix.Length;
            double[] x = new double[n];
            b.CopyTo(x, 0);

            for (int i = 1; i < n; ++i)
            {
                double sum = x[i];
                for (int j = 0; j < i; ++j)
                    sum -= luMatrix[i][j] * x[j];
                x[i] = sum;
            }

            x[n - 1] /= luMatrix[n - 1][n - 1];
            for (int i = n - 2; i >= 0; --i)
            {
                double sum = x[i];
                for (int j = i + 1; j < n; ++j)
                    sum -= luMatrix[i][j] * x[j];
                x[i] = sum / luMatrix[i][i];
            }

            return x;
        }

        static double[][] MatrixDecompose(double[][] matrix, out int[] perm, out int toggle)
        {
            // Doolittle LUP decomposition with partial pivoting.
            // rerturns: result is L (with 1s on diagonal) and U;
            // perm holds row permutations; toggle is +1 or -1 (even or odd)
            int rows = matrix.Length;
            int cols = matrix[0].Length; // assume square
            if (rows != cols)
                throw new Exception("Attempt to decompose a non-square m");

            int n = rows; // convenience

            double[][] result = MatrixDuplicate(matrix);

            perm = new int[n]; // set up row permutation result
            for (int i = 0; i < n; ++i)
            {
                perm[i] = i;
            }

            toggle = 1; // toggle tracks row swaps.
            // +1 -greater-than even, -1 -greater-than odd. used by MatrixDeterminant

            for (int j = 0; j < n - 1; ++j) // each column
            {
                double colMax = Math.Abs(result[j][j]); // find largest val in col
                int pRow = j;
                //for (int i = j + 1; i less-than n; ++i)
                //{
                //  if (result[i][j] greater-than colMax)
                //  {
                //    colMax = result[i][j];
                //    pRow = i;
                //  }
                //}

                // reader Matt V needed this:
                for (int i = j + 1; i < n; ++i)
                {
                    if (Math.Abs(result[i][j]) > colMax)
                    {
                        colMax = Math.Abs(result[i][j]);
                        pRow = i;
                    }
                }
                // Not sure if this approach is needed always, or not.

                if (pRow != j) // if largest value not on pivot, swap rows
                {
                    double[] rowPtr = result[pRow];
                    result[pRow] = result[j];
                    result[j] = rowPtr;

                    int tmp = perm[pRow]; // and swap perm info
                    perm[pRow] = perm[j];
                    perm[j] = tmp;

                    toggle = -toggle; // adjust the row-swap toggle
                }

                // --------------------------------------------------
                // This part added later (not in original)
                // and replaces the 'return null' below.
                // if there is a 0 on the diagonal, find a good row
                // from i = j+1 down that doesn't have
                // a 0 in column j, and swap that good row with row j
                // --------------------------------------------------

                if (result[j][j] == 0.0)
                {
                    // find a good row to swap
                    int goodRow = -1;
                    for (int row = j + 1; row < n; ++row)
                    {
                        if (result[row][j] != 0.0)
                            goodRow = row;
                    }

                    if (goodRow == -1)
                        throw new Exception("Cannot use Doolittle's method");

                    // swap rows so 0.0 no longer on diagonal
                    double[] rowPtr = result[goodRow];
                    result[goodRow] = result[j];
                    result[j] = rowPtr;

                    int tmp = perm[goodRow]; // and swap perm info
                    perm[goodRow] = perm[j];
                    perm[j] = tmp;

                    toggle = -toggle; // adjust the row-swap toggle
                }
                // --------------------------------------------------
                // if diagonal after swap is zero . .
                //if (Math.Abs(result[j][j]) less-than 1.0E-20) 
                //  return null; // consider a throw

                for (int i = j + 1; i < n; ++i)
                {
                    result[i][j] /= result[j][j];
                    for (int k = j + 1; k < n; ++k)
                    {
                        result[i][k] -= result[i][j] * result[j][k];
                    }
                }
            } // main j column loop

            return result;
        }
    }
}

class Element
{
    public string Name { get; set; }
    public int Number { get; set; }

    public Element(string name, int number)
    {
        Name = name;
        Number = number;
    }
}