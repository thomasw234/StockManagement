using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace GTill
{
    class frmPasswordInput : Form
    {
        /// <summary>
        /// The password that must be entered by the user
        /// </summary>
        string sPassword;
        /// <summary>
        /// The message to show above the password input box
        /// </summary>
        string sMessageToShow;
        /// <summary>
        /// The form's background colour
        /// </summary>
        Color cFrmBackColour;
        /// <summary>
        /// The form's foreground (text) colour
        /// </summary>
        Color cFrmForeColour;
        /// <summary>
        /// Label showing the message to the user
        /// </summary>
        Label lblMessage;
        /// <summary>
        /// The textbox that the user enters the password into
        /// </summary>
        TextBox tbPasswordInput;
        /// <summary>
        /// The possible types of outcome from this form
        /// </summary>
        public enum PasswordDialogResult {Correct, Incorrect, Cancelled};
        /// <summary>
        /// The outcome from the user input
        /// </summary>
        PasswordDialogResult pdrResult;
        /// <summary>
        /// The number of retries that the user has at entering the password before the window closes
        /// </summary>
        int nRetriesRemaining = 2;
        /// <summary>
        /// The reason that this was shown, no presets, can be anything
        /// </summary>
        string sShowCode;
        /// <summary>
        /// The name of the font to use on the form
        /// </summary>
        string sFontName;

        /// <summary>
        /// Initialises the form
        /// </summary>
        /// <param name="sPasswordRequired">The password that is required by the user</param>
        /// <param name="sMessage">The message to show to the user</param>
        /// <param name="s">The size of this form</param>
        /// <param name="sSC">The ShowCode - the reason for opening, no presets, can be anything</param>
        public frmPasswordInput(string sPasswordRequired, string sMessage, Size s, string sSC)
        {
            sFontName = Properties.Settings.Default.sFontName;
            sPassword = sPasswordRequired;
            sMessageToShow = sMessage;
            cFrmBackColour = Properties.Settings.Default.cFrmBackColour;
            cFrmForeColour = Properties.Settings.Default.cFrmForeColour;
            sShowCode = sSC;
            pdrResult = PasswordDialogResult.Incorrect;
            this.Size = s;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = cFrmBackColour;
            this.ForeColor = cFrmForeColour;
            this.FormBorderStyle = FormBorderStyle.None;

            lblMessage = new Label();
            lblMessage.AutoSize = false;
            lblMessage.Width = this.Width;
            lblMessage.Height = (int)(Properties.Settings.Default.fMainScreenFontSize * 2);
            lblMessage.Top = this.Height / 4;
            lblMessage.Left = 0;
            lblMessage.Font = new Font(sFontName, Properties.Settings.Default.fMainScreenFontSize);
            lblMessage.BackColor = cFrmBackColour;
            lblMessage.ForeColor = cFrmForeColour;
            lblMessage.Text = sMessage;
            lblMessage.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblMessage);

            tbPasswordInput = new TextBox();
            tbPasswordInput.Font = new Font(sFontName, 18.0f);
            tbPasswordInput.PasswordChar = ' ';
            tbPasswordInput.Width = this.Width / 5;
            tbPasswordInput.Left = (this.Width / 2) - (tbPasswordInput.Width / 2);
            tbPasswordInput.Top = this.Height / 2;
            tbPasswordInput.BackColor = cFrmForeColour;
            tbPasswordInput.ForeColor = Properties.Settings.Default.cFrmBackColour;
            tbPasswordInput.BorderStyle = BorderStyle.None;
            tbPasswordInput.KeyDown += new KeyEventHandler(tbPasswordInput_KeyDown);
            this.Controls.Add(tbPasswordInput);
        }

        /// <summary>
        /// Handles a key press on in password input textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tbPasswordInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                pdrResult = PasswordDialogResult.Cancelled;
                this.Close();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                if (tbPasswordInput.Text.ToUpper() == sPassword.ToUpper())
                {
                    pdrResult = PasswordDialogResult.Correct;
                    this.Close();
                }
                else
                {
                    if (nRetriesRemaining == 0)
                    {
                        pdrResult = PasswordDialogResult.Incorrect;
                        this.Close();
                    }
                    else
                    {
                        nRetriesRemaining -= 1;
                        tbPasswordInput.Clear();
                    }
                }
            }
        }

        /// <summary>
        /// So that an external process can get the result of the password entry
        /// </summary>
        /// <returns>The result of the password entry</returns>
        public PasswordDialogResult GetResult()
        {
            return pdrResult;
        }

        /// <summary>
        /// So that an external process can get the reason for opening code
        /// </summary>
        /// <returns>The reason for opening code (ShowCode)</returns>
        public string GetOpeningReason()
        {
            return sShowCode;
        }
    }
}
