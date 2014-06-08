using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace GTill
{
    class frmReceiveOnAccount : Form
    {
        frmAccSel fasGetAccountCode;
        frmInput fiGetAmount;
        frmPaymentInput fpiGetPaymentMethod;
        TillEngine.TillEngine tEngine;

        string sFontName;
        Point pLocation;
        Size sSize;
        Color cFrmBackColour, cFrmForeColour;

        string sAccountCodeToReceive;
        string sPaymentMethod;
        float fAmountToReceive;

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

        protected override void SetVisibleCore(bool value)
        {
            // Do nothing. Basically don't show the form at all!
        }

        void ShowAccountDialog()
        {
            fasGetAccountCode = new frmAccSel(ref tEngine);
            fasGetAccountCode.ChangeInstruction = "Select the account to receive money on:";
            fasGetAccountCode.Show();
            fasGetAccountCode.FormClosing += new System.Windows.Forms.FormClosingEventHandler(fasGetAccountCode_FormClosing);
        }

        void fasGetAccountCode_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            sAccountCodeToReceive = fasGetAccountCode.sGetAccountCode();
            if (sAccountCodeToReceive != "NONE_SELECTED")
            {
                string[] sExtraInfo = { tEngine.sGetAccountDetailsFromCode(sAccountCodeToReceive)[2].TrimEnd('\0').TrimEnd(' ') };
                fiGetAmount = new frmInput(frmInput.FormType.ReceivedOnAccount, pLocation, new Size(sSize.Width, 75), sExtraInfo);
                fiGetAmount.Show();
                fiGetAmount.FormClosing += new FormClosingEventHandler(fiGetAmount_FormClosing);
            }
        }

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
            }
            else
            {
                MessageBox.Show("Cancelled Receive on Account");
            }
            this.Close();
        }
    }
}
