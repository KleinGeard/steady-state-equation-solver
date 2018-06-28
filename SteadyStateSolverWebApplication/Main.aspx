<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Main.aspx.cs" Inherits="SteadyStateSolverWebApplication.Main" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Steady State Solver</title>
    <script type="text/javascript" async
        src="https://cdnjs.cloudflare.com/ajax/libs/mathjax/2.7.4/MathJax.js?config=TeX-MML-AM_CHTML" async>
    </script>
    <script>
        function copyTeX() {
          var copyText = document.getElementById("hiddenInput");
          copyText.select();
          document.execCommand("copy");
          alert("TeX code copied to clipboard");
        }
    </script>
    <style type="text/css">
        .auto-style1 {
            width: 850px;
        }
        .githublink:hover {
            background-color: lightgray;
        }
        .hidden
        {
            position: absolute;
            left: -9999px;
        }
    </style>
</head>
<body>
    <h3>Markov Chain Steady State Solver</h3>
    <p>Created by Klein Geard </p>
    <a href="https://github.com/KleinGeard/steady-state-equation-solver">
        <img id="imgGithubLink" src="githubicon.png" width="50" height="50" class="githublink" />
    </a>
    <form id="form1" runat="server">
        <div>
            <table>
                <tr>
                    <td style="padding-top:0px;padding-left:20px; " class="auto-style1">
                        <asp:Label ID="lblEnterMatrixDimensions" runat="server" Text="Matrix Dimensions: "></asp:Label>
                        <asp:TextBox TextMode="Number" MaxLength="2" runat="server" ID="txtMatrixDim" Text="3" Width="42px"></asp:TextBox>
                        <asp:Button ID="btnSubmitDim" Text="Update" runat="server" OnClick="btnSubmitDim_Click" />
                        <asp:Label ID="lblDimensionsInputError" runat="server" ForeColor="Red"></asp:Label>
                    </td>
                </tr>
                <table>
                    <tr>
                        <td align="left&quot;" class="modal-sm" style="width: 52px">
                            <h3>P =</h3>
                        </td>
                        <td align="left">
                                <div ID="divTransitionMatrix" runat="server" />
                        </td>
                    </tr>
                </table>
                <tr>
                </tr>
                <tr>
                    <td class="modal-sm" style="width: 320px">
                        <asp:Button ID="btnCalc" runat="server" Text="Calculate" align="center" Width="69px" OnClick="btnCalc_Click"/>
                    </td>
                </tr>
            </table>
            <asp:Label ID="lblMatrixInputError" runat="server" ForeColor="Red"></asp:Label></div>
        <asp:Label ID="lblEquations" runat="server" Text="" allign="right"></asp:Label>
    </form>
    <button id="btnCopyTex" onclick="copyTeX()">TeX</button>
    <input type="text" value="3223" runat="server" id="hiddenInput" class="hidden"/>
</body>
</html>
