using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using TillEngine;

namespace GTill
{
    class frmPaymentInput : Form
    {
        // Global Variables
        
        /// <summary>
        /// The label that tells the user what to do
        /// </summary>
        Label lblInstruction;
        /// <summary>
        /// The form background colour
        /// </summary>
        Color cBackColour;
        /// <summary>
        /// The form foreground (text) colour
        /// </summary>
        Color cForeColour;
        /// <summary>
        /// The possible methods of payment labels
        /// </summary>
        Label[] lblPaymentChoices;
        /// <summary>
        /// The selection grid height and width
        /// </summary>
        int nGridHeight, nGridWidth;
        /// <summary>
        /// The payment selection label that is currently selected
        /// </summary>
        int nCurrentlySelected = 0;
        /// <summary>
        /// The state of the form - 0 = Payment Selection, 1 = Amount entry, 2 = Credit card type selection
        /// </summary>
        int nState = 0;
        /// <summary>
        /// The names of the possible credit cards to choose from
        /// </summary>
        string[] sCreditCards;
        /// <summary>
        /// The choice of credit card labels
        /// </summary>
        Label[] lblCreditCardChoices;
        /// <summary>
        /// The input textbox to get the amount of the selected payment method
        /// </summary>
        TextBox tbGetAmount;
        /// <summary>
        /// The amount paid using the selected payment method
        /// </summary>
        float fAmountInput;
        /// <summary>
        /// The selected payment type
        /// </summary>
        string sPaymentType = "NULL";
        /// <summary>
        /// The amount that is remaining to be paid - automatically put in the input textbox
        /// </summary>
        float fAmountRemainingDue;
        /// <summary>
        /// Whether or not the cash shortcut was used (multiply key on number pad)
        /// </summary>
        bool bJumpedStraightToCash = false;
        /// <summary>
        /// The form to get the discount amount / percentage
        /// </summary>
        frmInput fiDiscountAmnt;
        /// <summary>
        /// The form to select the account to charge
        /// </summary>
        frmAccSel fasChargeToAcc;
        /// <summary>
        /// Whether or not change from cheques is allowed
        /// </summary>
        bool bChangeFromCheques;
        /// <summary>
        /// Whether or not discount is allowed
        /// </summary>
        bool bDiscountAllowed;
        /// <summary>
        /// Whether or not charge to account is allowed
        /// </summary>
        bool bChargeToAccountAllowed;
        /// <summary>
        /// The name of the font to use on this form
        /// </summary>
        string sFontName;
        /// <summary>
        /// Whether or not the amount needs to be input. False for refunds
        /// </summary>
        bool bGetAmount;

        /// <summary>
        /// Initialises the form
        /// </summary>
        /// <param name="pStartLocation">The starting location of the form</param>
        /// <param name="sFormSize">The size of the form</param>
        /// <param name="sCCTypes">The different credit cards that can be used</param>
        /// <param name="fFullAmountDue">The total amount that is still to be paid</param>
        /// <param name="bChangeAllowedFromCheque">Whether or not change from cheques is allowed</param>
        /// <param name="bAllowDisc">Whether or not discount is allowed</param>
        /// <param name="bChangePayment">Whether or not this is being opened to change the previous payment method</param>
        public frmPaymentInput(Point pStartLocation, Size sFormSize, string[] sCCTypes, float fFullAmountDue, bool bChangeAllowedFromCheque, bool bAllowDisc, bool bChangePayment)
        {
            this.StartPosition = FormStartPosition.Manual;
            this.Location = pStartLocation;
            this.Size = sFormSize;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Properties.Settings.Default.cFrmBackColour;
            this.ForeColor = Properties.Settings.Default.cFrmForeColour;
            sFontName = Properties.Settings.Default.sFontName;
            sCreditCards = sCCTypes;
            fAmountRemainingDue = fFullAmountDue;
            cForeColour = Properties.Settings.Default.cFrmForeColour;
            cBackColour = Properties.Settings.Default.cFrmBackColour;
            bChangeFromCheques = bChangeAllowedFromCheque;
            bDiscountAllowed = bAllowDisc;
            bChargeToAccountAllowed = bChangePayment;
            SetupForm();
            bGetAmount = true;
            this.KeyDown += new KeyEventHandler(frmPaymentInput_KeyDown);
        }

        /// <summary>
        /// Sets whether or not the user is required to enter the amount
        /// </summary>
        public bool GetAmountFromUser
        {
            set
            {
                bGetAmount = value;
            }
        }

        /// <summary>
        /// Handles the keydown event of the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void frmPaymentInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (nState == 0) // Form is in payment selection stage
            {
                if (e.KeyCode == Keys.Right)
                {
                    if (nCurrentlySelected < 7)
                        nCurrentlySelected++;
                }
                else if (e.KeyCode == Keys.Left)
                {
                    if (nCurrentlySelected > 0)
                        nCurrentlySelected -= 1;
                }
                else if (e.KeyCode == Keys.Down)
                {
                    if (nCurrentlySelected < 4)
                        nCurrentlySelected += 4;
                }
                else if (e.KeyCode == Keys.Up)
                {
                    if (nCurrentlySelected > 3)
                        nCurrentlySelected -= 4;
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    sPaymentType = "NULL";
                    fAmountInput = 0.0f;
                    this.Close();
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    switch (nCurrentlySelected)
                    {
                        case 0:
                            sPaymentType = "CASH";
                            break;
                        case 1:
                            sPaymentType = "CHEQ";
                            break;
                        case 2:
                            sPaymentType = "CRCD";
                            break;
                        case 3:
                            sPaymentType = "DEPO";
                            break;
                        case 4:
                            sPaymentType = "VOUC";
                            break;
                        case 5:
                            sPaymentType = "CHRG";
                            this.Close();
                            break;
                        case 6:
                            fiDiscountAmnt = new frmInput(frmInput.FormType.DiscountAmount, this.Location, this.Size, new string[0]);
                            fiDiscountAmnt.Show();
                            fiDiscountAmnt.FormClosing += new FormClosingEventHandler(fiDiscountAmnt_FormClosing);
                            break;
                        case 7:
                            sPaymentType = "NULL";
                            this.Close();
                            break;
                    }
                    if (nCurrentlySelected != 6) // If discount hasn't been selected
                        SetupForGettingAmount();
                }
                if (nCurrentlySelected == 6 && !bDiscountAllowed)
                    nCurrentlySelected++; // Just move along to close because discount isn't allowed
                if (nCurrentlySelected == 5 && !bChargeToAccountAllowed)
                    nCurrentlySelected = 7; // Move along to close because charge to account isn't allowed
                for (int i = 0; i < lblPaymentChoices.Length; i++) // Unhighlight all options
                {
                    lblPaymentChoices[i].BackColor = cBackColour;
                    lblPaymentChoices[i].ForeColor = cForeColour;
                }
                // Highlight the currently selected option
                lblPaymentChoices[nCurrentlySelected].BackColor = cForeColour;
                lblPaymentChoices[nCurrentlySelected].ForeColor = cBackColour;
            }
            else if (nState == 2) // Credit card selection
            {
                if (e.KeyCode == Keys.Escape) // Go back to payment selection
                {
                    for (int i = 0; i < lblCreditCardChoices.Length; i++)
                    {
                        this.Controls.Remove(lblCreditCardChoices[i]);
                        lblCreditCardChoices[i].Dispose();
                    }
                    this.Controls.Remove(lblInstruction);
                    lblInstruction.Dispose();
                    SetupForm();
                    nState = 0;
                    nCurrentlySelected = 0;
                }
                else
                {
                    try
                    {
                        // Try to get the credit card that was selected
                        int nKeyNumber = Convert.ToInt32(e.KeyCode.ToString()[e.KeyCode.ToString().Length - 1].ToString());
                        if (nKeyNumber > 0 && nKeyNumber <= lblCreditCardChoices.Length)
                        {
                            sPaymentType += (e.KeyCode.ToString()[e.KeyCode.ToString().Length - 1]);
                            fAmountInput = TillEngine.TillEngine.fFormattedMoneyString(tbGetAmount.Text);
                            this.Close();
                        }
                    }
                    catch
                    {
                        ;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the form closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fiDiscountAmnt_FormClosing(object sender, FormClosingEventArgs e)
        {
            string sReturned = fiDiscountAmnt.sGetDataToReturn();
            fiDiscountAmnt.Dispose();

            if (sReturned == "CANCELLED")
            {
                sPaymentType = "NULL";
                this.Close();
            }
            else
            {
                string[] sCommand = sReturned.Split(',');
                sPaymentType = sCommand[0];
                try
                {
                    if (sPaymentType == "ADISCOUNT")
                        fAmountInput = TillEngine.TillEngine.FixFloatError(TillEngine.TillEngine.fFormattedMoneyString(sCommand[1]));
                    else
                        fAmountInput = (float)Convert.ToDecimal(sCommand[1]);
                }
                catch
                {
                    sPaymentType = "NULL";
                }
                this.Close();
            }
        }

        /// <summary>
        /// Sets up the form controls
        /// </summary>
        void SetupForm()
        {
            lblInstruction = new Label();
            lblInstruction.Location = new Point(0, 0);
            lblInstruction.BackColor = cBackColour;
            lblInstruction.ForeColor = cForeColour;
            lblInstruction.Font = new Font(sFontName, 12.0f);
            lblInstruction.Text = "Select Payment Type: (Press Esc to Cancel)";
            lblInstruction.AutoSize = true;
            this.Controls.Add(lblInstruction);

            lblPaymentChoices = new Label[8];
            nGridWidth = this.Width;
            nGridHeight = this.Height - lblInstruction.Height;
            int nMaxWidth = nGridWidth / 4;
            int nMaxHeight = nGridHeight / 2;
            lblPaymentChoices[0] = new Label();
            lblPaymentChoices[0].Text = "Additional Charges";
            lblPaymentChoices[0].AutoSize = true;
            float fFontSize = 1.0f;
            lblPaymentChoices[0].Font = new Font(sFontName, fFontSize);
            this.Controls.Add(lblPaymentChoices[0]);
            while (lblPaymentChoices[0].Width < nMaxWidth && lblPaymentChoices[0].Height < nMaxHeight)
            {
                fFontSize += 1.0f;
                lblPaymentChoices[0].Font = new Font(sFontName, fFontSize);
            }
            fFontSize -= 1.0f;
            lblPaymentChoices[0].Dispose();
            for (int i = 0; i < lblPaymentChoices.Length; i++)
            {
                lblPaymentChoices[i] = new Label();
                lblPaymentChoices[i].Font = new Font(sFontName, fFontSize);
                lblPaymentChoices[i].ForeColor = cForeColour;
                lblPaymentChoices[i].BackColor = cBackColour;
                lblPaymentChoices[i].Size = new Size(nMaxWidth, nMaxHeight);
                lblPaymentChoices[i].Left = (i * nMaxWidth);
                lblPaymentChoices[i].Top = lblInstruction.Height;
                lblPaymentChoices[i].TextAlign = ContentAlignment.MiddleCenter;
                if (i >= 4)
                {
                    lblPaymentChoices[i].Left -= nGridWidth;
                    lblPaymentChoices[i].Top += nMaxHeight;
                }
                this.Controls.Add(lblPaymentChoices[i]);
            }

            lblPaymentChoices[0].Text = "Cash";
            lblPaymentChoices[1].Text = "Cheque";
            lblPaymentChoices[2].Text = "Credit Card";
            lblPaymentChoices[3].Text = "Deposit Paid";
            lblPaymentChoices[4].Text = "Voucher";
            if (bDiscountAllowed)
                lblPaymentChoices[6].Text = "Discount";
            else
                lblPaymentChoices[6].Text = "";
            if (bChargeToAccountAllowed)
                lblPaymentChoices[5].Text = "Charge To Account";
            else
                lblPaymentChoices[5].Text = "";
            lblPaymentChoices[7].Text = "Close";

            lblPaymentChoices[0].BackColor = cForeColour;
            lblPaymentChoices[0].ForeColor = cBackColour;
        }

        /// <summary>
        /// Shows the list of credit cards to choose from
        /// </summary>
        void ListCreditCards()
        {
            nState = 2;

            tbGetAmount.Visible = false;

            lblInstruction.Text = "Select Credit Card:";

            lblCreditCardChoices = new Label[sCreditCards.Length];

            int nCRCDGridWidth = this.Width - lblInstruction.Width;
            int nCRCDGridHeight = this.Height;
            int nMaxCRCDWidth = nCRCDGridWidth / 3;
            int nMaxCRCDHeight = nCRCDGridHeight / 3;

            float fFontSize = 1.0f;

            Label temp = new Label();
            this.Controls.Add(temp);
            temp.AutoSize = true;
            temp.Text = "6. AAAAAA";
            temp.Font = new Font(sFontName, fFontSize);

            while (temp.Width < nMaxCRCDWidth && temp.Height < nMaxCRCDHeight)
            {
                fFontSize += 1.0f;
                temp.Font = new Font(sFontName, fFontSize);
            }

            fFontSize -= 1.0f;
            temp.Dispose();

            for (int i = 0; i < lblCreditCardChoices.Length; i++)
            {
                lblCreditCardChoices[i] = new Label();
                lblCreditCardChoices[i].Font = new Font(sFontName, fFontSize);
                lblCreditCardChoices[i].Text = (i + 1).ToString() + ". " + sCreditCards[i];
                lblCreditCardChoices[i].Size = new Size(nMaxCRCDWidth, nMaxCRCDHeight);
                lblCreditCardChoices[i].BackColor = cBackColour;
                lblCreditCardChoices[i].ForeColor = cForeColour;
                lblCreditCardChoices[i].Left = lblInstruction.Width;
                if (i >= 3)
                    lblCreditCardChoices[i].Left += nMaxCRCDWidth;
                if (i >= 6)
                    lblCreditCardChoices[i].Left += nMaxCRCDWidth;
                if (i == 0 || i == 3 || i == 6)
                    lblCreditCardChoices[i].Top = 0;
                else if (i == 1 || i == 4 || i == 7)
                    lblCreditCardChoices[i].Top = nMaxCRCDHeight;
                else if (i == 2 || i == 5 || i == 8)
                    lblCreditCardChoices[i].Top = (2 * nMaxCRCDHeight);
                this.Controls.Add(lblCreditCardChoices[i]);
            }

            this.Focus();
        }

        /// <summary>
        /// Sets up the form ready to get the amount from the user
        /// </summary>
        void SetupForGettingAmount()
        {
            for (int i = 0; i < lblPaymentChoices.Length; i++)
            {
                lblPaymentChoices[i].Visible = false;
            }

            nState = 1;

            tbGetAmount = new TextBox();
            tbGetAmount.Size = lblPaymentChoices[1].Size;
            tbGetAmount.Location = lblPaymentChoices[1].Location;
            tbGetAmount.Font = lblPaymentChoices[1].Font;
            tbGetAmount.BorderStyle = BorderStyle.None;
            tbGetAmount.BackColor = cBackColour;
            tbGetAmount.ForeColor = cForeColour;
            tbGetAmount.KeyDown += new KeyEventHandler(tbGetAmount_KeyDown);
            tbGetAmount.Text = TillEngine.TillEngine.FormatMoneyForDisplay(fAmountRemainingDue);
            tbGetAmount.Tag = "FIRSTKEYDOWN";
            tbGetAmount.SelectionStart = 0;
            this.Controls.Add(tbGetAmount);
            lblInstruction.Text = "Enter Amount to pay using " + GetPaymentDescription(sPaymentType) + ":";
            tbGetAmount.Focus();
            if (!bGetAmount && nCurrentlySelected != 2)
                this.Close();
            else if (!bGetAmount && nCurrentlySelected == 2)
                ListCreditCards();
        }

        /// <summary>
        /// Handles the keydown in the get amount textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tbGetAmount_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || (bJumpedStraightToCash == true && e.KeyCode == Keys.Multiply))
            {
                try
                {
                    fAmountInput = TillEngine.TillEngine.fFormattedMoneyString(tbGetAmount.Text);
                }
                catch
                {
                    sPaymentType = "NULL"; // No payment method selected
                    this.Close();
                }
                if (nCurrentlySelected == 1 && !bChangeFromCheques && fAmountInput > fAmountRemainingDue)
                {
                    sPaymentType = "NULL_CFC"; // Change from cheques not allowed
                    this.Close();
                }
                else if ((fAmountInput > fAmountRemainingDue && nCurrentlySelected != 0) || fAmountInput <= 0.0f)
                {
                    sPaymentType = "NULL_WPM"; // Wrong payment method input
                    this.Close();
                }
                else if (nCurrentlySelected == 2)
                    ListCreditCards();
                else
                {
                    this.Close();
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                tbGetAmount.Text = "";
                tbGetAmount.Visible = false;
                lblInstruction.Text = "Select Payment Type: (Press Esc to Cancel)";
                for (int i = 0; i < lblPaymentChoices.Length; i++)
                {
                    lblPaymentChoices[i].Visible = true;
                }
                nState = 0;
                this.Focus();
            }
            else if (tbGetAmount.Tag.ToString() == "FIRSTKEYDOWN")
            {
                tbGetAmount.Text = "";
                tbGetAmount.Tag = "NOTFIRSTKEYDOWN";
            }
            else if (tbGetAmount.Text.Length == 1 && e.KeyCode == Keys.Back)
            {
                tbGetAmount.Text = TillEngine.TillEngine.FormatMoneyForDisplay(fAmountRemainingDue);
                tbGetAmount.Tag = "FIRSTKEYDOWN";
            }
        }

        /// <summary>
        /// Gets the amount that was input by the user
        /// </summary>
        /// <returns></returns>
        public float GetAmount()
        {
            return fAmountInput;
        }

        /// <summary>
        /// Gets the selected payment method
        /// </summary>
        /// <returns></returns>
        public string GetPaymentMethod()
        {
            return sPaymentType;
        }

        /// <summary>
        /// Takes a shortcut to cash (the multiply key on the number pad)
        /// </summary>
        public void SelectCash()
        {
            sPaymentType = "CASH";
            SetupForGettingAmount();
            bJumpedStraightToCash = true;
        }

        /// <summary>
        /// Gets a description of the payment method
        /// </summary>
        /// <param name="sPaymentCode">The payment method code to get the description of</param>
        /// <returns>The description of the payment method</returns>
        public string GetPaymentDescription(string sPaymentCode)
        {
            switch (sPaymentType)
            {
                case "CASH":
                    return "Cash";
                    break;
                case "CRCD":
                    return "Credit Card";
                    break;
                case "CRCD1":
                    return "Credit Card (" + sCreditCards[0] + ")";
                    break;
                case "CRCD2":
                    return "Credit Card (" + sCreditCards[1] + ")";
                    break;
                case "CRCD3":
                    return "Credit Card (" + sCreditCards[2] + ")";
                    break;
                case "CRCD4":
                    return "Credit Card (" + sCreditCards[3] + ")";
                    break;
                case "CRCD5":
                    return "Credit Card (" + sCreditCards[4] + ")";
                    break;
                case "CRCD6":
                    return "Credit Card (" + sCreditCards[5] + ")";
                    break;
                case "CRCD7":
                    return "Credit Card (" + sCreditCards[6] + ")";
                    break;
                case "CRCD8":
                    return "Credit Card (" + sCreditCards[7] + ")";
                    break;
                case "CRCD9":
                    return "Credit Card (" + sCreditCards[8] + ")";
                    break;
                case "CHEQ":
                    return "Cheque";
                    break;
                case "DEPO":
                    return "Deposit Paid";
                    break;
                case "VOUC":
                    return "Voucher";
                    break;
            }
            // The payment code wasn't found
            GTill.ErrorHandler.LogError("Payment descritption not found for code : " + sPaymentType);
            return "SERIOUS ERROR!";
        }
    }
}
