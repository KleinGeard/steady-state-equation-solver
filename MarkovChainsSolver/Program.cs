using MarkovChains;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkovChains
{
    class Program
    {

        static void Main(string[] args)
        {
            //List<List<decimal>> mchain = new List<List<decimal>>
            //{
            //    new List<decimal>{0.75m, 0m, 0.25m},
            //    new List<decimal>{0.25m, 2/3m, 0.25m},
            //    new List<decimal>{0m,  1/3m,  0.5m},
            //};
            List<List<decimal>> mchain = new List<List<decimal>>
            {
                new List<decimal>{0.65m, 0.15m, 0.1m},
                new List<decimal>{0.25m, 0.65m, 0.4m},
                new List<decimal>{0.1m,  0.2m,  0.5m},
            };
            //List<List<decimal>> mchain = new List<List<decimal>>
            //{
            //    new List<decimal>{0m, 2/5m, 1/3m, 0m},
            //    new List<decimal>{2/3m, 0m, 1/3m, 2/3m},
            //    new List<decimal>{1/3m, 1/5m, 0m, 1/3m},
            //    new List<decimal>{0m, 2/5m, 1/3m, 0m},
            //};

            MarkovChain markovChain = new MarkovChain(mchain);
            Console.WriteLine(markovChain.findSteadyStates());

            Console.ReadLine();
        }
    }

    class MarkovChain
    {
        private List<List<decimal>> markovChain;
        private List<string> names;
        private List<SteadyStateEquation> steadyStateEquations;

        public MarkovChain(List<List<decimal>> markovChain)
        {
            this.markovChain = markovChain;
            GenerateEquations();
        }

        private void GenerateEquations()
        {
            steadyStateEquations = new List<SteadyStateEquation>();
            for (int i = 0; i < markovChain.Count; i++)
                steadyStateEquations.Add(new SteadyStateEquation(markovChain[i], new SteadyStateValue((i + 1).ToString(), 1)));
        }

        public void Setnames(List<string> names)
        {
            if (names.Count == this.names.Count)
                this.names = names;
            else
                throw new Exception("Length of List 'names' does not match dimensions of markov chain");
        }

        private void writeEquations()
        {
            steadyStateEquations.ForEach(s => texString.AppendLine(s.ToString()));
            texString.AppendLine("");
        }

        private void writeSolvedValues()
        {
            solvedSteadyStateValues.ForEach(v => texString.AppendLine(v.ToString()));
            texString.AppendLine("");
        }

        private static void writeToTex(string s)
        {
                texString.AppendLine(s);
        }

        public string findSteadyStates() //TODO: break up into multiple smaller methods
        {
            writeToTex("The initial equations are as such:");
            writeEquations();

            writeToTex("Remove pk from the equations");
            steadyStateEquations.ForEach(s => s.solve());
            steadyStateEquations.ForEach(Console.WriteLine);
            steadyStateEquations[0].substituteEquation(steadyStateEquations[1]);
            steadyStateEquations[1].substituteEquation(steadyStateEquations[2]);
            steadyStateEquations[2].substituteEquation(steadyStateEquations[0]);
            steadyStateEquations.ForEach(Console.WriteLine);
        }

        private class SteadyStateEquation
        {
            public List<SteadyStateValue> SteadyStateValues { get; set; }
            public SteadyStateValue Equivalent { get; set; }

            public SteadyStateEquation(List<decimal> values, SteadyStateValue equivalent)
            {
                SteadyStateValues = new List<SteadyStateValue>();

                for (int i = 0; i < values.Count; i++)
                    SteadyStateValues.Add(new SteadyStateValue((i + 1).ToString(), values[i]));
                Equivalent = equivalent;
            }

            public override string ToString()
            {
                string equation = "";

                for (int j = 0; j < SteadyStateValues.Count - 1; j++)
                    equation += $"{SteadyStateValues[j]} + ";
                equation += $"{SteadyStateValues.Last()} = {Equivalent}";

                return equation;
            }

            public void substituteEquation(params SteadyStateEquation[] subEquations)
            {
                string valueString = "(";

                for (int i = 0; i < SteadyStateValues.Count - 1; i++)
                    valueString += $"{SteadyStateValues[i]} + ";

                valueString += $"{SteadyStateValues.Last()})";

                return valueString;
            }

            #region substitution_steps
            public void substituteEquation(SteadyStateEquation subEquation)
            {
                for (int i = SteadyStateValues.Count - 1; i >= 0; i--)
                    if (SteadyStateValues[i].PiName == (subEquation.Equivalent.PiName))
                    {
                        writeToTex(ToString().Replace(SteadyStateValues[i].ToString(), SteadyStateValues[i].getRoundedValue() +  subEquation.ValuesAsString()));
                        SubstituteValue(i, subEquation);
                        writeToTex(ToString());
                    }

                Consolidate();
                solve();
            }

            private void SubstituteValue(int oldSteadyStateValueIndex, SteadyStateEquation SubEquation)
            {
                decimal p = SteadyStateValues[oldSteadyStateValueIndex].Value;

                foreach (SteadyStateValue newSteadyStateValue in SubEquation.SteadyStateValues)
                    SteadyStateValues.Add(new SteadyStateValue(newSteadyStateValue.PiName, newSteadyStateValue.Value * p));

                SteadyStateValues.RemoveAt(oldSteadyStateValueIndex);
            }

            #region solve
            public void solve(bool fromSubstituteEquation = true) //TODO: break up into multiple smaller methods
            {
                bool needsSolving = false;
                SolveStepOne(ref needsSolving);
                if (!needsSolving || Equivalent.Value == 1)
                {
                    writeToTex("");
                    return;
                }
                    
                writeToTex(ToString());

                string equationString = "";
                SolveStepTwo(ref equationString);
                writeToTex(equationString);

                writeToTex(ToString());
                writeToTex("");
            }
            
            private void SolveStepOne(ref bool needsSolving) //NOTE: not entirely necessary unless showing working is required
            {
                //step 1: take relevant value out
                for (int i = SteadyStateValues.Count - 1; i >= 0; i--)
                    if (SteadyStateValues[i].PiName.Equals(Equivalent.PiName))
                    {
                        Equivalent.Value = 1 - SteadyStateValues[i].Value;
                        SteadyStateValues.RemoveAt(i);
                        needsSolving = true;
                        break;
                    } // not entirely necessary unless showing working is required

            private void SolveStepTwo(ref string equationString)
            {
                //step 2: adjust such that the equiv = 1
                for (int i = SteadyStateValues.Count - 1; i >= 0; i--)
                    SteadyStateValues[i].Value /= Equivalent.Value;
                Equivalent.Value = 1;
            }
            #endregion solve

            public void Consolidate() //there is probably a better way to do this
            {
                List<int> removalIndices = new List<int>();

                for (int i = SteadyStateValues.Count - 1; i >= 0; i--)
                    for (int j = SteadyStateValues.Count - 1; j >= 0; j--)
                        if (i != j && SteadyStateValues[i].PiName.Equals(SteadyStateValues[j].PiName) && !removalIndices.Contains(j))
                        {
                            decimal p = SteadyStateValues[i].Value;
                            removalIndices.Add(i);
                            SteadyStateValues[j].Value += p;
                        }

                removalIndices.ForEach(i => SteadyStateValues.RemoveAt(i));

                writeToTex(ToString());
            }
            #endregion substitution_steps

        }

        private class SteadyStateValue
        {
            public string PiName { get; set; }
            public decimal Value { get; set; }

            public SteadyStateValue(string piName, decimal value)
            {
                PiName = piName;
                Value = value;
            }

            public override string ToString()
            {
                return (Value == 1) ? $"p{PiName}" : $"{getRoundedValue()}π{PiName}";
            }

            public decimal getRoundedValue()
            {
                return Math.Round(Value, 4);
            }
        }

        private class SolvedSteadyStateValue : SteadyStateValue
        {
            public SolvedSteadyStateValue(string k, decimal value) : base(k, value) { }
            
            public override string ToString()
            {
                return $"p{PiName} = {getRoundedValue()}";
            }
        }

#region dump
//public bool isValidMarkovChain()
//{
//    foreach (List<decimal> list in markovChain)
//    {
//        decimal rowSum = 0;
//        foreach (decimal n in list)
//        {
//            if (n < 0)
//                return false;
//            rowSum += n;
//        }

//        if (rowSum != 1 || list.Count != markovChain.Count)
//            return false;
//    }

//    return true;
//}
//int first = 0;
//int second = 1;
//List<decimal> row1 = markovChain[first];
//List<decimal> row2 = markovChain[second];

//string equation = "";

//            for (int i = 0; i<count; i++)
//            {



//                if (i == first) // within (*)?
//                {
//                    equation += $"{row2[i]}(";
//                    for (int j = 0; j<count; j++)
//                        if (j<count - 1) equation += $"{row1[j]}pi_{j} + ";
//                    equation += $"{row1[count-1]}pi_{count-1})";
//                    if (i<count - 1) equation += " + ";
//                } else
//                {
//                    equation += $"{row2[i]}pi_{i}";
//                    if (i<count - 1) equation += " + ";
//                }
//            }
//            Console.WriteLine(equation);

//public void solveSteadyStates()
//{
//    int count = markovChain.Count;
//    for (int i = 0; i < count; i++)
//    {
//        string assertion = "";

//        for (int j = 0; j < count; j++)
//            if (j < count - 1) assertion += $"{markovChain[i][j]}pi_{j} + ";

//        assertion += $"{markovChain[i][count - 1]}pi_{markovChain[i][count - 1]} = pi_{i}";
//        Console.WriteLine(assertion);
//    }
//}
#endregion dump