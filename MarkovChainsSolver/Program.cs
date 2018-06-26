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
            //List<List<decimal>> mchain = new List<List<decimal>>
            //{
            //    new List<decimal>{0.65m, 0.15m, 0.1m},
            //    new List<decimal>{0.25m, 0.65m, 0.4m},
            //    new List<decimal>{0.1m,  0.2m,  0.5m},
            //};
            List<List<decimal>> mchain = new List<List<decimal>>
            {
                new List<decimal>{0m, 2/5m, 1/3m, 0m},
                new List<decimal>{2/3m, 0m, 1/3m, 2/3m},
                new List<decimal>{1/3m, 1/5m, 0m, 1/3m},
                new List<decimal>{0m, 2/5m, 1/3m, 0m},
            };

            MarkovChain markovChain = new MarkovChain(mchain);
            Console.WriteLine(markovChain.findSteadyStates());

            Console.ReadLine();
        }
    }

    class MarkovChain
    {
        private List<List<decimal>> markovChain;
        private List<string> names;
        private List<SteadyStateEquation> steadyStateEquations; //TODO: Make this a hashtable with the equiv values as keys
        private List<SolvedSteadyStateValue> solvedSteadyStateValues;
        private static StringBuilder texString;
        private static bool logging = false;

        public MarkovChain(List<List<decimal>> markovChain)
        {
            this.markovChain = markovChain;
            GenerateEquations();
            solvedSteadyStateValues = new List<SolvedSteadyStateValue>();
            InitialiseTexString();
        }

        private void InitialiseTexString()
        {
            texString = new StringBuilder();
            //    string starString = "";
            //    for (int i = 0; i < steadyStateEquations.Count - 1; i++)
            //    {
            //        texString.AppendLine(steadyStateEquations[i] + $"      ({i+1})");
            //        starString += $"{steadyStateEquations[i].Equivalent} + ";
            //    }

            //    texString.AppendLine(steadyStateEquations.Last() + $"      ({steadyStateEquations.Count})");

            //    starString += $"{steadyStateEquations.Last().Equivalent} = 1      (*)";
            //    texString.AppendLine(starString);
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
            if (logging)
            {
                texString.AppendLine(s);
            }
        }

        public string findSteadyStates()
        {
            writeEquations();

            steadyStateEquations.ForEach(s => s.solve());

            writeEquations();

            steadyStateEquations.First().SteadyStateValues.Clear();
            steadyStateEquations.First().SteadyStateValues.Add(new SteadyStateValue(steadyStateEquations.First().Equivalent.K, 1));

            writeEquations();
            logging = true;

            for (int i = 1; i < steadyStateEquations.Count; i++)
            {
                for (int j = 1; j < steadyStateEquations.Count; j++)
                {
                    if (i != j)
                    {
                        writeToTex($"Substitute {steadyStateEquations[i].Equivalent} into {steadyStateEquations[j].Equivalent}\n");
                        steadyStateEquations[j].substituteEquation(steadyStateEquations[i]);
                    }
                }
            }
            
            SubstituteIntoOne();

            writeEquations();
            writeSolvedValues();

            return texString.ToString();
        }

        private void SubstituteIntoOne() //NOTE: This method assumes that all equations are solved in terms of π1
        {
            decimal sum = 0;

            foreach (SteadyStateEquation s in steadyStateEquations)
                sum += s.SteadyStateValues.First().Value;
            
            adjustAll(1 / sum);
        }

        private void adjustAll(decimal solvedValue)
        {
            foreach (SteadyStateEquation equation in steadyStateEquations)
                solvedSteadyStateValues.Add(new SolvedSteadyStateValue(equation.Equivalent.K, equation.SteadyStateValues.First().Value * solvedValue));
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
                    equation += $"{SteadyStateValues[j].ToString()} + ";
                equation += $"{SteadyStateValues.Last().ToString()} = {Equivalent.ToString()}";

                return equation;
            }

            public string ValuesAsString()
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
                    if (SteadyStateValues[i].K == (subEquation.Equivalent.K))
                    {
                        writeToTex(ToString().Replace(SteadyStateValues[i].ToString(), Math.Round(SteadyStateValues[i].Value, 4) +  subEquation.ValuesAsString()));
                        SubstituteValue(i, subEquation);
                        writeToTex(ToString());
                    }
                        
                    
                solve();

                writeToTex("");
            }

            private void SubstituteValue(int oldSteadyStateValueIndex, SteadyStateEquation SubEquation)
            {
                decimal p = SteadyStateValues[oldSteadyStateValueIndex].Value;

                foreach (SteadyStateValue newSteadyStateValue in SubEquation.SteadyStateValues)
                    SteadyStateValues.Add(new SteadyStateValue(newSteadyStateValue.K, newSteadyStateValue.Value * p));

                SteadyStateValues.RemoveAt(oldSteadyStateValueIndex);
            }

            public void solve()
            {
                Consolidate();
                bool needsSolving = false;
                //step 1: take relevant value out
                for (int i = SteadyStateValues.Count - 1; i >= 0; i--)
                    if (SteadyStateValues[i].K.Equals(Equivalent.K))
                    {
                        Equivalent.Value = 1 - SteadyStateValues[i].Value;
                        SteadyStateValues.RemoveAt(i);
                        needsSolving = true;
                        break;
                    } //NOTE: not entirely necessary unless showing working is required
                if (!needsSolving)
                    return;
                writeToTex(ToString());

                //step 2: adjust such that the equiv = 1
                SteadyStateValues.ForEach(s => s.Value /= Equivalent.Value);
                Equivalent.Value = 1;
                writeToTex(ToString());
            }

            public void Consolidate() //there is probably a better way to do this
            {
                List<int> removalIndices = new List<int>();

                for (int i = SteadyStateValues.Count - 1; i >= 0; i--)
                    for (int j = SteadyStateValues.Count - 1; j >= 0; j--)
                        if (i != j && SteadyStateValues[i].K.Equals(SteadyStateValues[j].K) && !removalIndices.Contains(j))
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
            public string K { get; set; }
            public decimal Value { get; set; }

            public SteadyStateValue(string k, decimal value)
            {
                K = k;
                Value = value;
            }

            public override string ToString()
            {
                return (Value == 1) ? $"p{K}" : $"{Math.Round(Value, 4)}π{K}";
            }
        }

        private class SolvedSteadyStateValue : SteadyStateValue
        {
            public SolvedSteadyStateValue(string k, decimal value) : base(k, value) { }
            
            public override string ToString()
            {
                return $"p{K} = {Math.Round(Value, 4)}";
            }
        }

    }
}
