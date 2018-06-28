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

        private void retainDynamicControls()
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
                    txt.Text = Request.Form.GetValues($"x{i}y{j}")[0];
                    DataTable dt = new DataTable(); //TODO: Make safe
                    var d = dt.Compute(txt.Text, "");
                    row.Add(Convert.ToDecimal(d));
                    divTransitionMatrix.Controls.Add(txt);
                }
                transitionMatrix.Add(row);
                divTransitionMatrix.Controls.Add(new LiteralControl("<br/>"));
            }

        }

        protected void btnSubmitDim_Click(object sender, EventArgs e)
        {
            try
            {
                createTransitionMatrix(Convert.ToInt32(txtMatrixDim.Text));
            } catch
            {
                //Message: Please enter a valid input
            }
            
        }



        protected void btnCalc_Click(object sender, EventArgs e)
        {
            retainDynamicControls();

            MarkovChain markovChain = new MarkovChain(transitionMatrix);
            lblEquations.Text = markovChain.findSteadyStates();
        }



    }
}