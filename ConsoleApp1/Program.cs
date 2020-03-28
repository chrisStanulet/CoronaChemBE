using System;
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
            Dictionary<int,List<Element>> Terms = new Dictionary<int, List<Element>>();
            int termNum = 0;
            string input = "Fe+AgNO3=Ag+Fe(NO3)3";
            string[] halves = input.Split("=");
            string eq1 = halves[0];
            string eq2 = halves[1];
            Console.WriteLine("Ok so its running");
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
            foreach (var e in Terms)
            {
                List<Element> j = e.Value;
                Console.WriteLine("Term number " + e.Key);
                foreach (var h in j)
                {
                    Console.WriteLine(h.Name + " - " + h.Number );
                }
                Console.WriteLine("---------------------------");
            }
            /* matrix is            [A] | [B]
                term1    term2    term3 |   term4
             e1                         |
             e2                         |
             e3                         |
             e4                         |                            
             */
            Balence(Terms,termNum);

    
            
            
        }
        

        public static List<Element> getElements(String _molecule)
        {
            List<Element> list1 = new List<Element>();
            var findMatches = Regex.Matches(_molecule, @"\(?[A-Z][a-z]?\d*\)?"); // Get all elements
            Double endNumber = Double.Parse(Regex.IsMatch(_molecule, @"\)\d+") ? Regex.Match(_molecule, @"\)\d+").Value.Remove(0, 1) : "1"); // Finds the number after the ')'
            foreach (Match i in findMatches)
            {
                String element = Regex.Match(i.Value, "[A-Z][a-z]?").Value; // Gets the element
                Double amountOfElement = 0;
                if (!Regex.IsMatch(i.Value, @"[\(\)]"))
                    amountOfElement = Double.Parse(String.IsNullOrWhiteSpace(i.Value.Replace(element, "")) ? "1" : i.Value.Replace(element, ""));
                else
                {
                    if (!Double.TryParse(Regex.Replace(i.Value, @"(\(|\)|[A-Z]|[a-z])", ""), out amountOfElement))
                        amountOfElement = endNumber; // If the element has either '(' or ')' and doesn't specify an amount, then set it equal to the endnumber
                    else
                        amountOfElement *= endNumber; // If the element has either '(' or ')' and specifies an amount, then multiply it by the end number
                }
                list1.Add(new Element(element, Convert.ToInt32(amountOfElement)));
                //Console.WriteLine(element + " - " + amountOfElement);
            }
            return list1;
        }

        public static void Balence(Dictionary<int,List<Element>> terms, int termnum)
        {
            Dictionary<int,List<Element>> Terms = terms;
            int termNum = termnum;
            List<Element> elements = new List<Element>();


            foreach (var v in terms)
            {
                List<Element> j = v.Value;
                foreach (var h in j)//make a list of the elements that make up the eq
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
            foreach (var e in elements)//going through each of the possible elements essentially the rows
            {

                foreach (var t in terms)//going through thr element in each term essentially the columns
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
                        if (column == matDim-1)
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

            for(int i=0; i<matDim;)
            {
                Console.WriteLine("");
                for(int j=0; j<matDim;)
                {
                    Console.Write(Matrix[i, j] + " ");
                    j++;
                }

                i++;
            }
            // [A] needs to have dimensions that are the number of possible elements x number of possible elements
            //[B] needs to have dimensoins that are the 1 x the number of elements
            int[,] A = new int[matDim,matDim];
            int[,] B = new int[matDim,1];
            if (matDim == otherDim)
            {
                List<int[,]> matricies = splitWithOnes(Matrix, otherDim);
                 A = matricies[0];
                 B = matricies[1];
            }

            if (matDim == otherDim + 1)
            {
                splitWithoutOnes(Matrix,otherDim);
            }
            Console.WriteLine("----------------------");
            for(int i=0; i<matDim;)
            {
                Console.WriteLine("");
                for(int j=0; j<matDim;)
                {
                    Console.Write(A[i, j] + " ");
                    j++;
                }

                i++;
            }
            Console.WriteLine("------------------");
            for(int i=0; i<matDim;)
            {
                Console.WriteLine("");
                for(int j=0; j<1;)
                {
                    Console.Write(B[i, j] + " ");
                    j++;
                }

                i++;
            }
        }

        public static List<int[,]> splitWithOnes(int[,] matrix, int dim)
        {
            //dim = dim - 1;
            int[,] A = new int[dim, dim];
            int[,] B = new int[dim, 1];

            for(int i=0; i<dim;)
            {
                Console.WriteLine("");
                for(int j=0; j<dim;)
                { 
                    if(j<dim-1)
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

            for(int i=0; i<dim;)
            {
                Console.WriteLine("");
                for(int j=0; j<dim;)
                { 
                    if(j<dim-1)
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



    