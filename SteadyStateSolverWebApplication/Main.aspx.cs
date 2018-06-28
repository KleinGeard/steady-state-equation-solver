using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SteadyStateSolverWebApplication
{
    public partial class Main : System.Web.UI.Page
    {
        List<List<decimal>> transitionMatrix = new List<List<decimal>>();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                createTransitionMatrix(3);
            } else
            {
                lblMatrixInputError.Text = "";
                lblDimensionsInputError.Text = "";
            }
        }
        
        private void createTransitionMatrix(int dimensions)
        {
            divTransitionMatrix.Controls.Clear();
            for (int i= 0; i < dimensions; i++) {
                for(int j = 0; j < dimensions; j++)
                {
                    TextBox txt = new TextBox
                    {
                        ID = $"x{i.ToString()}y{j.ToString()}",
                        Text = "0",
                        Width = 48,
                        Height = 32
                    };
                    divTransitionMatrix.Controls.Add(txt);
                }
                divTransitionMatrix.Controls.Add(new LiteralControl("<br/>"));
            }
        }

        private void retainDynamicControls(ref bool isValidInput)
        {
            string[] keys = Request.Form.AllKeys;
            int dimensions = 0;

            foreach (string key in keys)
                if (key.Contains("x0"))
                    dimensions += 1;

            for (int i = 0; i < dimensions; i++)
            {
                List<decimal> row = new List<decimal>();
                for (int j = 0; j < dimensions; j++)
                {
                    TextBox txt = new TextBox
                    {
                        ID = $"x{ i }y{ j }",
                        Text = "0",
                        Width = 48,
                        Height = 32,
                    };

                    try
                    {
                        txt.Text = Request.Form.GetValues($"x{i}y{j}")[0];
                        DataTable dt = new DataTable(); //TODO: Make safe
                        var d = dt.Compute(txt.Text, "");
                        row.Add(Convert.ToDecimal(d));
                    } catch
                    {
                        isValidInput = false;
                        lblMatrixInputError.Text = "Matrix values must be valid decimals or fractions";
                    }
                    
                    divTransitionMatrix.Controls.Add(txt);
                }
                transitionMatrix.Add(row);
                divTransitionMatrix.Controls.Add(new LiteralControl("<br/>"));
            }
            if (!isValidInput) return;

            isValidInput = isValidMatrix();
        }

        protected void btnSubmitDim_Click(object sender, EventArgs e)
        {
            try
            {

                int newDimensions = Convert.ToInt32(txtMatrixDim.Text);
                if (newDimensions > 1 && newDimensions < 16)
                {
                    createTransitionMatrix(newDimensions);
                    lblEquations.Text = "";
                } else
                {
                    lblDimensionsInputError.Text = "Please enter a number between 2 and 15";
                }
            } catch
            {
                //Message: Please enter a valid input - likely not needed
            }
            
        }
        
        protected void btnCalc_Click(object sender, EventArgs e)
        {
            bool isValidInput = true;
            retainDynamicControls(ref isValidInput);

            if (isValidInput)
            {
                try
                {
                    MarkovChain markovChain = new MarkovChain(transitionMatrix);
                    string tex = markovChain.findSteadyStates();
                    lblEquations.Text = tex;
                    hiddenInput.Value = tex;
                } catch //TODO: Find a better way to do this
                { 
                    lblMatrixInputError.Text = "Every state in the Markov Chain must be reachable";
                    lblEquations.Text = "";
                }
                
            }
                
        }

        private bool isValidMatrix()
        {
            foreach (List<decimal> row in transitionMatrix)
            {
                decimal rowsum = 0;
                foreach(decimal value in row)
                {
                    if ( value < 0)
                    {
                        lblMatrixInputError.Text = "All values in the matrix put be non-negative";
                        lblEquations.Text = "";
                        return false;
                    } else
                    {
                        rowsum += value;
                    }
                }
                if (rowsum <= 0.995m || rowsum >= 1.005m)
                {
                    lblMatrixInputError.Text = "All rows in the transition matrix must sum to 1";
                    lblEquations.Text = "";
                    return false;
                }
            }
            return true;
        }

    }
}