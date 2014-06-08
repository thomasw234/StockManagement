using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace GTill
{
    class frmReceiveOnAccount : Form
    {
        /// <summary>
        /// The form to get the account to receive money from
        /// </summary>
        frmAccSel fasGetAccountCode;
        /// <summary>
        /// The form to get the amount of money that was received
        /// </summary>
        frmInput fiGetAmount;
        /// <summary>
        /// The form to get the payment method that the money was received using
        /// </summary>
        frmPaymentInput fpiGetPaymentMethod;
        /// <summary>
        /// A reference to the TillEngine
        /// </summary>
        TillEngine.TillEngine tEngine;

        /// <summary>
        /// The name of the font to use on this form
        /// </summary>
        string sFontName;
        /// <summary>
        /// The location to place this form on the screen
        /// </summary>
        Point pLocation;
        /// <summary>
        /// The size of this form
        /// </summary>
        Size sSize;
        /// <summary>
        /// The form background colour and foreground (text) colour
        /// </summary>
        Color cFrmBackColour, cFrmForeColour;

        /// <summary>
        /// The code of the account which was selected
        /// </summary>
        string sAccountCodeToReceive;
        /// <summary>
        /// The code of the payment method to use
        /// </summary>
        string sPaymentMethod;
        /// <summary>
        /// The amount to receive on the account
        /// </summary>
        float fAmountToReceive;

        /// <summary>
        /// Initialises the form
        /// </summary>
        /// <param name="p">The location on screen to place the form</param>
        /// <param name="s">The size of the form</param>
        /// <param name="te">A reference to the TillEngine</param>
        public frmReceiveOnAccount(Point p, Size s, ref TillEngine.TillEngine te)
        {
            cFrmBackColour = Properties.Settings.Default.cFrmBackColour;
            cFrmForeColour = Properties.Settings.Default.cFrmForeColour;
            sSize = s;
            pLocation = p;
            tEngine = te;
            sFontName = Properties.Settings.Default.sFontName;
            ShowAccountDialog();
        }

        /// <summary>
        /// Overrides the form visible procedure, to prevent it from never being shown, as this is really made up of 3 other forms
        /// </summary>
        /// <param name="value"></param>
        protected override void SetVisibleCore(bool value)
        {
            // Do nothing. Never allow the form to become visible
        }

        /// <summary>
        /// Shows the account selection window
        /// </summary>
        void ShowAccountDialog()
        {
            fasGetAccountCode = new frmAccSel(ref tEngine);
            fasGetAccountCode.ChangeInstruction = "Select the account to receive money on:";
            fasGetAccountCode.Show();
            fasGetAccountCode.FormClosing += new System.Windows.Forms.FormClosingEventHandler(fasGetAccountCode_FormClosing);
        }

        /// <summary>
        /// Occurs when the user has closed the account selection form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fasGetAccountCode_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            sAccountCodeToReceive = fasGetAccountCode.sGetAccountCode();
            if (sAccountCodeToReceive != "NONE_SELECTED") // If an account has been selected
            {
                string[] sExtraInfo = { tEngine.sGetAccountDetailsFromCode(sAccountCodeToReceive)[2].TrimEnd('\0').TrimEnd(' ') };
                // Show the form to get the amount being received
                fiGetAmount = new frmInput(frmInput.FormType.ReceivedOnAccount, pLocation, new Size(sSize.Width, 75), sExtraInfo);
                fiGetAmount.Show();
                fiGetAmount.FormClosing += new FormClosingEventHandler(fiGetAmount_FormClosing);
            }
        }

        /// <summary>
        /// Occurs when the use has entered the amount being received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fiGetAmount_FormClosing(object sender, FormClosingEventArgs e)
        {
            string sToReturn = fiGetAmount.sGetDataToReturn();
            if (sToReturn != "CANCELLED")
            {
                fAmountToReceive = TillEngine.TillEngine.fFormattedMoneyString(sToReturn);
                fAmountToReceive = TillEngine.TillEngine.FixFloatError(fAmountToReceive);
                fpiGetPaymentMethod = new frmPaymentInput(pLocation, new Size(sSize.Width, sSize.Height), tEngine.GetCreditCards(), fAmountToReceive, false, false, false);
                fpiGetPaymentMethod.Show();
                fpiGetPaymentMethod.GetAmountFromUser = false;
                fpiGetPaymentMethod.FormClosing += new FormClosingEventHandler(fpiGetPaymentMethod_FormClosing);
            }
        }

        /// <summary>
        /// Occurs when the user has selected the payment method to receive money on
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fpiGetPaymentMethod_FormClosing(object sender, FormClosingEventArgs e)
        {
            sPaymentMethod = fpiGetPaymentMethod.GetPaymentMethod();
            if (sPaymentMethod == "DEPO")
            {
                MessageBox.Show("Sorry, you can't receive money on deposit paid.");
            }
            if (sPaymentMethod != "NULL" && sPaymentMethod != "DEPO")
            {
                tEngine.ReceiveMoneyOnAccount(sAccountCodeToReceive, fAmountToReceive, sPaymentMethod);
                tEngine.OpenTillDrawer(false);
                tEngine.PrintReceivedOnAccount(sAccountCodeToReceive, fAmountToReceive, sPaymentMethod);
                MessageBox.Show("Money successfully received.");
            }
            else
            {
                MessageBox.Show("Cancelled Receive on Account");
            }
            this.Close();
        }
    }
}
