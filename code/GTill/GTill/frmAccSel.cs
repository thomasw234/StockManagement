using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using TillEngine;

namespace GTill
{
    class frmAccSel : Form
    {
        /// <summary>
        /// The backcolour to use
        /// </summary>
        Color cFrmBackColour;
        /// <summary>
        /// The forecolour to use
        /// </summary>
        Color cFrmForeColour;
        /// <summary>
        /// A reference to the main TillEngine
        /// </summary>
        TillEngine.TillEngine tEngine;
        /// <summary>
        /// A list of account codes
        /// </summary>
        string[] sAccountCode;
        /// <summary>
        /// A list of account names, matches up with account codes
        /// </summary>
        string[] sAccountName;
        /// <summary>
        /// Account Addresses
        /// </summary>
        string[,] sAddress;
        /// <summary>
        /// Whether the list should be sorted by code (alphabetically). Otherwise it's by name
        /// </summary>
        bool bSortByCode = false;
        /// <summary>
        /// The account that has been selected
        /// </summary>
        string sCodeToReturn;
        /// <summary>
        /// The font to use on this form
        /// </summary>
        string sFontName;

        // Form Components

        /// <summary>
        /// Used to display the list of account names
        /// </summary>
        ListBox lbAccountSelection;
        /// <summary>
        /// Shows the account code of the currently selected account
        /// </summary>
        Label lblAccountCode;
        /// <summary>
        /// Helps the user
        /// </summary>
        Label lblInstructions;
        /// <summary>
        /// Shows the address of the currently selected account
        /// </summary>
        Label[] lblAddress;
        /// <summary>
        /// Tells the user how the list of accounts is being sorted (by code or name)
        /// </summary>
        Label lblSortMethod;

        /// <summary>
        /// Initialises the Account Selection form
        /// </summary>
        /// <param name="te"></param>
        public frmAccSel(ref TillEngine.TillEngine te)
        {
            this.BackColor = Properties.Settings.Default.cFrmForeColour;
            this.ForeColor = Properties.Settings.Default.cFrmBackColour;
            cFrmBackColour = Properties.Settings.Default.cFrmForeColour;
            cFrmForeColour = Properties.Settings.Default.cFrmBackColour;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(640, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            tEngine = te;
            sFontName = Properties.Settings.Default.sFontName;
            DrawForm();
            GetAccountsAndDisplay();
        }

        /// <summary>
        /// Sets up the form controls
        /// </summary>
        void DrawForm()
        {
            lblInstructions = new Label();
            lblInstructions.Top = 10;
            lblInstructions.Left = 0;
            lblInstructions.AutoSize = false;
            lblInstructions.Width = this.Width;
            lblInstructions.TextAlign = ContentAlignment.MiddleCenter;
            lblInstructions.Height = 35;
            lblInstructions.Font = new Font(sFontName, 18.0f);
            lblInstructions.Text = "Select the account to charge, or press ESC to exit";
            this.Controls.Add(lblInstructions);

            lbAccountSelection = new ListBox();
            lbAccountSelection.BackColor = cFrmBackColour;
            lbAccountSelection.ForeColor = cFrmForeColour;
            lbAccountSelection.BorderStyle = BorderStyle.FixedSingle;
            lbAccountSelection.Top = lblInstructions.Top + lblInstructions.Height + 10;
            lbAccountSelection.Width = (this.Width / 2) - 20;
            lbAccountSelection.Left = 10;
            lbAccountSelection.Height = this.Height - lbAccountSelection.Top;
            lbAccountSelection.Font = new Font(sFontName, 16.0f);
            lbAccountSelection.SelectedIndexChanged += new EventHandler(lbAccountSelection_SelectedIndexChanged);
            lbAccountSelection.KeyDown += new KeyEventHandler(lbAccountSelection_KeyDown);
            this.Controls.Add(lbAccountSelection);

            lblAccountCode = new Label();
            lblAccountCode.Top = lbAccountSelection.Top;
            lblAccountCode.Left = (this.Width / 2) + 10;
            lblAccountCode.Width = this.Width - lbAccountSelection.Left - 10;
            lblAccountCode.Height = 35;
            lblAccountCode.Font = new Font(sFontName, 16.0f);
            lblAccountCode.BackColor = cFrmBackColour;
            lblAccountCode.ForeColor = cFrmForeColour;
            this.Controls.Add(lblAccountCode);

            lblAddress = new Label[5];
            for (int i = 0; i < lblAddress.Length; i++)
            {
                lblAddress[i] = new Label();
                lblAddress[i].BackColor = cFrmBackColour;
                lblAddress[i].ForeColor = cFrmForeColour;
                lblAddress[i].Font = new Font(sFontName, 16.0f);
                lblAddress[i].Left = lblAccountCode.Left;
                lblAddress[i].Top = lblAccountCode.Top + lblAccountCode.Height + 20 + (i * 35);
                lblAddress[i].Height = 35;
                lblAddress[i].Width = lblAccountCode.Width;
                this.Controls.Add(lblAddress[i]);
            }

            lblSortMethod = new Label();
            lblSortMethod.Font = new Font(sFontName, 10.0f);
            lblSortMethod.BackColor = cFrmBackColour;
            lblSortMethod.ForeColor = cFrmForeColour;
            lblSortMethod.Left = lblAccountCode.Left;
            lblSortMethod.AutoSize = true;
            lblSortMethod.Text = "Loading, please wait!";
            lblSortMethod.Top = lbAccountSelection.Top + lbAccountSelection.Height - lblSortMethod.Height;
            this.Controls.Add(lblSortMethod);

            this.Refresh();
        }

        /// <summary>
        /// Works out what to do when a key is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The key that has been pressed</param>
        void lbAccountSelection_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F10) // Swap the sorting method
            {
                bSortByCode = !bSortByCode;
                GetAccountsAndDisplay();
            }
            else if (e.KeyCode == Keys.Escape) // Close this form
            {
                sCodeToReturn = "NONE_SELECTED";
                this.Close();
            }
            else if (e.KeyCode == Keys.Enter) // Select the currently highlighted account
            {
                sCodeToReturn = sAccountCode[lbAccountSelection.SelectedIndex];
                this.Close();
            }
        }

        /// <summary>
        /// Show the details of the currently selected account
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void lbAccountSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            int nSelected = lbAccountSelection.SelectedIndex;
            lblAccountCode.Text = "Account code : " + sAccountCode[nSelected];
            if (sAddress[nSelected, 0].TrimEnd(' ').Length == 0)
            {
                lblAddress[0].Text = "No address found.";
                lblAddress[1].Text = "";
                lblAddress[2].Text = "";
                lblAddress[3].Text = "";
                lblAddress[4].Text = "";
            }
            else
            {
                lblAddress[0].Text = "Address:";
                lblAddress[1].Text = sAddress[nSelected, 0];
                lblAddress[2].Text = sAddress[nSelected, 1];
                lblAddress[3].Text = sAddress[nSelected, 2];
                lblAddress[4].Text = sAddress[nSelected, 3];
            }
        }

        /// <summary>
        /// Gets a list of accounts from the databases and displays them
        /// </summary>
        void GetAccountsAndDisplay()
        {
            lbAccountSelection.Items.Clear();
            if (bSortByCode)
            {
                int nOfAccounts = 0;
                string[,] sAccounts = tEngine.sGetAccounts(ref nOfAccounts);
                sAccountName = new string[nOfAccounts];
                sAccountCode = new string[nOfAccounts];
                sAddress = new string[nOfAccounts, 4];
                for (int i = 0; i < nOfAccounts; i++)
                {
                    sAccountCode[i] = sAccounts[i, 0].TrimEnd(' ');
                }
                Array.Sort(sAccountCode);
                for (int i = 0; i < sAccountCode.Length; i++)
                {
                    FillInDetailsBasedOnCode(sAccountCode[i], i);
                    lbAccountSelection.Items.Add(sAccountName[i]);
                }
            }
            else
            {
                int nOfAccounts = 0;
                string[,] sAccounts = tEngine.sGetAccounts(ref nOfAccounts);
                sAccountName = new string[nOfAccounts];
                sAccountCode = new string[nOfAccounts];
                sAddress = new string[nOfAccounts, 4];
                for (int i = 0; i < nOfAccounts; i++)
                {
                    sAccountName[i] = sAccounts[i, 2].TrimEnd(' ');
                }
                Array.Sort(sAccountName);
                for (int i = 0; i < sAccountName.Length; i++)
                {
                    FillInDetailsBasedOnName(sAccountName[i], i);
                    lbAccountSelection.Items.Add(sAccountName[i]);
                }
            }
            if (lbAccountSelection.Items.Count > 0)
                lbAccountSelection.SelectedIndex = 0;

            if (bSortByCode)
                lblSortMethod.Text = "Currently sorting by code, press F10 to change";
            else
                lblSortMethod.Text = "Currently sorting by name, press F10 to change";
        }

        /// <summary>
        /// Gets the details about the currently selected account
        /// </summary>
        /// <param name="sCode">The code of the currently selected account</param>
        /// <param name="nArrayPosition">The position in the list of accounts of the currently selected account</param>
        void FillInDetailsBasedOnCode(string sCode, int nArrayPosition)
        {
            string[] sDetails = tEngine.sGetAccountDetailsFromCode(sCode);
            sAccountName[nArrayPosition] = sDetails[2];
            sAddress[nArrayPosition, 0] = sDetails[3];
            sAddress[nArrayPosition, 1] = sDetails[4];
            sAddress[nArrayPosition, 2] = sDetails[5];
            sAddress[nArrayPosition, 3] = sDetails[6];
        }

        /// <summary>
        /// Gets the details about the currently selected account
        /// </summary>
        /// <param name="sCode">The name of the currently selected account</param>
        /// <param name="nArrayPosition">The position in the list of accounts of the currently selected accoun</param>
        void FillInDetailsBasedOnName(string sCode, int nArrayPosition)
        {
            string[] sDetails = tEngine.sGetAccountDetailsFromDesc(sCode);
            sAccountCode[nArrayPosition] = sDetails[0];
            sAddress[nArrayPosition, 0] = sDetails[3];
            sAddress[nArrayPosition, 1] = sDetails[4];
            sAddress[nArrayPosition, 2] = sDetails[5];
            sAddress[nArrayPosition, 3] = sDetails[6];
        }

        /// <summary>
        /// Gets the code of the account that was selected
        /// </summary>
        /// <returns></returns>
        public string sGetAccountCode()
        {
            return sCodeToReturn;
        }

        /// <summary>
        /// Changes the method of sorting (public so can be called by other objects)
        /// </summary>
        public string ChangeInstruction
        {
            set
            {
                lblInstructions.Text = value;
            }
        }
    }
}
