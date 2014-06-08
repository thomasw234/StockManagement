using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace GTill
{
    class frmRefund : Form
    {
        /// <summary>
        /// The foreground (text) colour to be used
        /// </summary>
        Color cFrmForeColour;
        /// <summary>
        /// The background colour to be used
        /// </summary>
        Color cFrmBackColour;
        /// <summary>
        /// The name of the font to be used
        /// </summary>
        string sFontName;
        /// <summary>
        /// A reference to the TillEngine
        /// </summary>
        TillEngine.TillEngine tEngine;
        /// <summary>
        /// A list of possible form states
        /// </summary>
        enum FormState { RefundTypeSelection, SpecificRefund, GeneralRefund };
        /// <summary>
        /// The label showing the title of the form
        /// </summary>
        Label lblTitle;
        /// <summary>
        /// The label telling the user what to do
        /// </summary>
        Label lblInstruction;
        /// <summary>
        /// The label saying whether doing a General or Specific refund
        /// </summary>
        Label lblRefundType;
        /// <summary>
        /// An array of labels next to the textboxes where the user enters refund details
        /// </summary>
        Label[] lblRefundDetails;
        /// <summary>
        /// An array of textboxes that the user enters data about the refund into
        /// </summary>
        TextBox[] tbRefundDetails;
        /// <summary>
        /// Allows the user to choose between a Specific and General refund
        /// </summary>
        ListBox lbRefundTypeSelection;
        /// <summary>
        /// The state that the form is currently in
        /// </summary>
        FormState currentFormState;
        /// <summary>
        /// A form to find the barcode of a product
        /// </summary>
        frmSearchForItemV2 fsfiFindBarcode;
        /// <summary>
        /// A form to get the payment method used in the refund
        /// </summary>
        frmPaymentInput fpiGetPaymentType;
        /// <summary>
        /// A form to get the account to refund (if applicable)
        /// </summary>
        frmAccSel fasGetAccountToRefund;
        /// <summary>
        /// Whether or not shift has been pressed for the function keys
        /// </summary>
        bool bShiftFunctionKeys = false;

        /// <summary>
        /// The barcode of the product to refund
        /// </summary>
        string sBarcodeToRefund;
        /// <summary>
        /// The amount to refund
        /// </summary>
        float fAmountToRefund;
        /// <summary>
        /// The quantity of the item to refund
        /// </summary>
        int nQuantityToRefund;
        /// <summary>
        /// The code of the payment method to refund
        /// </summary>
        string sPaymentMethod;
        /// <summary>
        /// Warning message reminding the user (SHARON!!!!!!) to verify the price to refund
        /// </summary>
        Label lblWarning;

        /// <summary>
        /// Initialises the form
        /// </summary>
        /// <param name="s">The size of the form</param>
        /// <param name="pFrmStartLoc">The location of the form on the screen</param>
        /// <param name="t">A reference to the TillEngine</param>
        public frmRefund(Size s, Point pFrmStartLoc, ref TillEngine.TillEngine t)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Properties.Settings.Default.cFrmBackColour;
            this.ForeColor = Properties.Settings.Default.cFrmForeColour;
            this.StartPosition = FormStartPosition.Manual;
            this.Size = s;
            cFrmBackColour = Properties.Settings.Default.cFrmBackColour;
            cFrmForeColour = Properties.Settings.Default.cFrmForeColour;
            sFontName = Properties.Settings.Default.sFontName;
            tEngine = t;
            this.Location = pFrmStartLoc;
            SetupForm(FormState.RefundTypeSelection);
            this.Paint += new PaintEventHandler(frmRefund_Paint);
        }

        /// <summary>
        /// Occurs when the form is painted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void frmRefund_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(new Pen(cFrmForeColour, 2.0f), 10, this.Height - 70, this.Width - 20, 50);

            if (currentFormState == FormState.RefundTypeSelection)
            {
                e.Graphics.DrawRectangle(new Pen(cFrmForeColour, 2.0f), new Rectangle(lbRefundTypeSelection.Left - 7, lbRefundTypeSelection.Top - 7, lbRefundTypeSelection.Width + 14, lbRefundTypeSelection.Height + 14));
                e.Graphics.DrawRectangle(new Pen(cFrmForeColour, 2.0f), new Rectangle(lbRefundTypeSelection.Left - 2, lbRefundTypeSelection.Top - 2, lbRefundTypeSelection.Width + 4, lbRefundTypeSelection.Height + 4));
            }
            else if (currentFormState == FormState.SpecificRefund && lblRefundDetails != null)
            {
                e.Graphics.DrawRectangle(new Pen(cFrmForeColour, 2.0f), new Rectangle(lblRefundDetails[0].Left - 25, lblRefundDetails[0].Top - 25, 460, 190));
                e.Graphics.DrawRectangle(new Pen(cFrmForeColour, 2.0f), new Rectangle(lblRefundDetails[0].Left - 30, lblRefundDetails[0].Top - 30, 470, 200));
            }
        }

        /// <summary>
        /// Sets up the form based on the formState
        /// </summary>
        /// <param name="fsNewFormState">The new formState</param>
        void SetupForm(FormState fsNewFormState)
        {
            foreach (Control c in this.Controls)
            {
                this.Controls.Remove(c);
                c.Dispose();
            }
            if (lblInstruction != null)
                lblInstruction.Dispose();

            currentFormState = fsNewFormState;
            lblTitle = new Label();
            lblTitle.BackColor = cFrmBackColour;
            lblTitle.ForeColor = cFrmForeColour;
            lblTitle.Font = new Font(sFontName, 14.0f);
            lblTitle.AutoSize = false;
            lblTitle.Location = new Point(0, 0);
            lblTitle.Size = new Size(this.Width, 25);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblTitle);

            lblInstruction = new Label();
            lblInstruction.BackColor = cFrmBackColour;
            lblInstruction.ForeColor = cFrmForeColour;
            lblInstruction.Font = new Font(sFontName, 14.0f);
            lblInstruction.AutoSize = false;
            lblInstruction.Width = this.Width - 40;
            lblInstruction.Height = 30;
            lblInstruction.Top = this.Height - 60;
            lblInstruction.Left = 20;
            lblInstruction.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblInstruction);

            if (fsNewFormState == FormState.RefundTypeSelection)
            {
                lbRefundTypeSelection = new ListBox();
                lbRefundTypeSelection.Font = new Font(sFontName, 20.0f);
                lbRefundTypeSelection.ForeColor = cFrmForeColour;
                lbRefundTypeSelection.BackColor = cFrmBackColour;
                lbRefundTypeSelection.BorderStyle = BorderStyle.None;
                lbRefundTypeSelection.Size = new Size(190, 80);
                lbRefundTypeSelection.Location = new Point((this.Width / 2) - (lbRefundTypeSelection.Width / 2), (this.Height / 2) - (lbRefundTypeSelection.Height / 2));
                this.Controls.Add(lbRefundTypeSelection);
                lbRefundTypeSelection.Items.Add("Specific Refund");
                lbRefundTypeSelection.Items.Add("General Refund");
                lbRefundTypeSelection.SelectedIndex = 0;
                lbRefundTypeSelection.KeyDown += new KeyEventHandler(lbRefundTypeSelection_KeyDown);
                lblTitle.Text = "Refund Item";
                lblInstruction.Text = "Select a refund method, or press ESC to exit.";
            }
            else if (fsNewFormState == FormState.SpecificRefund || fsNewFormState == FormState.GeneralRefund)
            {
                lblTitle.Text = "Specific Refund";
                lbRefundTypeSelection.Visible = false;
                lblInstruction.Text = "Enter the barcode of the item to refund. Press the Look Up key to find an item.";

                lblRefundDetails = new Label[4];
                for (int i = 0; i < lblRefundDetails.Length; i++)
                {
                    lblRefundDetails[i] = new Label();
                    lblRefundDetails[i].BackColor = cFrmBackColour;
                    lblRefundDetails[i].ForeColor = cFrmForeColour;
                    lblRefundDetails[i].Font = new Font(sFontName, 16.0f);
                    lblRefundDetails[i].AutoSize = false;
                    this.Controls.Add(lblRefundDetails[i]);
                    lblRefundDetails[i].Size = new Size(150, 35);
                    lblRefundDetails[i].Top = (this.Height / 2) - (lblRefundDetails.Length * lblRefundDetails[i].Height) + (i * lblRefundDetails[i].Height);
                    lblRefundDetails[i].Left = (this.Width / 2) - 200;
                }
                lblRefundDetails[0].Text = "Barcode:";
                lblRefundDetails[1].Text = "Item Price:";
                lblRefundDetails[2].Text = "Quantity:";
                lblRefundDetails[3].Text = "Total:";

                tbRefundDetails = new TextBox[4];
                for (int i = 0; i < tbRefundDetails.Length; i++)
                {
                    tbRefundDetails[i] = new TextBox();
                    tbRefundDetails[i].BackColor = cFrmForeColour;
                    tbRefundDetails[i].ForeColor = cFrmBackColour;
                    tbRefundDetails[i].Font = new Font(sFontName, 16.0f);
                    tbRefundDetails[i].Left = lblRefundDetails[i].Left + lblRefundDetails[i].Width + 10;
                    tbRefundDetails[i].Top = lblRefundDetails[i].Top;
                    tbRefundDetails[i].Size = new Size(250, 35);
                    tbRefundDetails[i].BorderStyle = BorderStyle.None;
                    tbRefundDetails[i].Tag = i;
                    tbRefundDetails[i].KeyDown += new KeyEventHandler(tbRefund_KeyDown);
                    this.Controls.Add(tbRefundDetails[i]);
                }
                tbRefundDetails[3].BackColor = cFrmBackColour;
                tbRefundDetails[3].Enabled = false;
                tbRefundDetails[0].Focus();

                lblWarning = new Label();
                lblWarning.Text = "Press enter to verify the price shown, or enter the correct price";
                lblWarning.Font = new Font(GTill.Properties.Settings.Default.sFontName, tbRefundDetails[0].Font.Size);
                lblWarning.BackColor = GTill.Properties.Settings.Default.cFrmBackColour;
                lblWarning.ForeColor = GTill.Properties.Settings.Default.cFrmForeColour;
                lblWarning.Location = new Point(0, tbRefundDetails[3].Top + 100);
                lblWarning.Width = this.Width;
                lblWarning.TextAlign = ContentAlignment.MiddleCenter;
                lblWarning.Visible = false;
                this.Controls.Add(lblWarning);
                this.Refresh();
            }
            if (fsNewFormState == FormState.GeneralRefund)
            {
                tbRefundDetails[0].Text = "General Refund";
                tbRefundDetails[0].Enabled = false;
                tbRefundDetails[0].BackColor = cFrmBackColour;
                tbRefundDetails[0].ForeColor = cFrmForeColour;
                tbRefundDetails[0].Font = new Font(sFontName, 11.0f);
                tbRefundDetails[2].Text = "1";
                tbRefundDetails[2].BackColor = cFrmBackColour;
                tbRefundDetails[2].ForeColor = cFrmForeColour;
                tbRefundDetails[2].Enabled = false;
                tbRefundDetails[2].Font = new Font(sFontName, 11.0f);
                lblTitle.Text = "General Refund";
                sBarcodeToRefund = "$GENERAL_REFUND";
                nQuantityToRefund = 1;
            }
        }

        /// <summary>
        /// Occurs when one of the refund entry textboxes' keys are pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tbRefund_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox tbCurrent = (TextBox)sender;
            int nCurrentBox = Convert.ToInt32(tbCurrent.Tag.ToString()[0].ToString());
            if (e.Shift && nCurrentBox == 0)
            {
                bShiftFunctionKeys = !bShiftFunctionKeys;
            }
            if (e.KeyCode == Keys.Enter)
            {
                if (nCurrentBox < 2)
                {
                    tbRefundDetails[nCurrentBox + 1].Focus();
                    switch (nCurrentBox+1)
                    {
                        case 0:
                            lblInstruction.Text = "Enter the barcode of the item to refund. Press Look Up to find an item.";
                            break;
                        case 1:
                            lblInstruction.Text = "Enter the price that was paid for the item.";
                            break;
                        case 2:
                            lblInstruction.Text = "Enter the quantity of the item to refund";
                            break;
                    }
                }
                if (nCurrentBox == 0)
                {
                    string[] sItemDetails = tEngine.GetItemDetailsForLookup(tbRefundDetails[0].Text);
                    if (sItemDetails == null)
                    {
                        if (tbRefundDetails[0].Text.Length > 1)
                        {
                            fsfiFindBarcode = new frmSearchForItemV2(ref tEngine);
                            fsfiFindBarcode.FormClosing += new FormClosingEventHandler(fsfiFindBarcode_FormClosing);
                            fsfiFindBarcode.Show();
                            fsfiFindBarcode.CheckForPartialBarcodeFromScanner(tbRefundDetails[0].Text);
                        }
                        else
                        {
                            lblInstruction.Text = "The barcode that was entered was invalid. Please try again.";
                            tbRefundDetails[0].Focus();
                        }
                    }
                    else
                    {
                        sBarcodeToRefund = tbRefundDetails[0].Text.ToUpper();
                        tbRefundDetails[0].BackColor = cFrmBackColour;
                        tbRefundDetails[0].ForeColor = cFrmForeColour;
                        tbRefundDetails[0].Font = new Font(sFontName, 11.0f);
                        tbRefundDetails[0].Text = sItemDetails[0];
                        if ((float)Convert.ToDecimal(sItemDetails[1]) > 0.0f)
                        {
                            tbRefundDetails[1].Text = TillEngine.TillEngine.FormatMoneyForDisplay(TillEngine.TillEngine.fFormattedMoneyString(sItemDetails[1]));
                        }
                        tbRefundDetails[1].KeyDown += new KeyEventHandler(frmRefund_KeyDown);
                        tbRefundDetails[1].Tag = "1";
                        tbRefundDetails[1].SelectAll();
                        lblWarning.Visible = true;
                    }
                }
                else if (nCurrentBox == 2)
                {
                    try
                    {
                        nQuantityToRefund = Convert.ToInt32(tbRefundDetails[2].Text);
                        if (nQuantityToRefund <= 0)
                        {
                            throw new NotSupportedException();
                        }
                        for (int i = 0; i < tbRefundDetails.Length; i++)
                        {
                            tbRefundDetails[i].BackColor = cFrmBackColour;
                            tbRefundDetails[i].ForeColor = cFrmForeColour;
                            tbRefundDetails[i].Font = new Font(sFontName, 11.0f);
                            tbRefundDetails[i].Enabled = false;
                        }
                        fAmountToRefund = TillEngine.TillEngine.FixFloatError((float)Convert.ToDecimal(tbRefundDetails[1].Text));
                        fAmountToRefund *= nQuantityToRefund;
                        fAmountToRefund = TillEngine.TillEngine.FixFloatError(fAmountToRefund);
                        tbRefundDetails[3].Text = TillEngine.TillEngine.FormatMoneyForDisplay(fAmountToRefund);
                        fpiGetPaymentType = new frmPaymentInput(new Point(this.Left, lblRefundDetails[3].Top + 200), new Size(this.Width, this.Height - 70 - (lblRefundDetails[0].Top + 250)), tEngine.GetCreditCards(), fAmountToRefund, false, false, true);
                        fpiGetPaymentType.Show();
                        fpiGetPaymentType.GetAmountFromUser = false;
                        fpiGetPaymentType.FormClosed += new FormClosedEventHandler(fpiGetPaymentType_FormClosing);
                    }
                    catch
                    {
                        tbRefundDetails[2].Text = "";
                        lblInstruction.Text = "The quantity entered was invalid. Please try again.";
                    }

                }
                else if (nCurrentBox == 1)
                {
                    try
                    {
                        fAmountToRefund = (float)Convert.ToDecimal(TillEngine.TillEngine.fFormattedMoneyString(tbRefundDetails[1].Text));
                        fAmountToRefund = tEngine.fFixFloatError(fAmountToRefund);
                        if (fAmountToRefund <= 0.0f || fAmountToRefund >= 100000.0f)
                            throw new NotSupportedException();
                        tbRefundDetails[1].Text = TillEngine.TillEngine.FormatMoneyForDisplay(fAmountToRefund);
                        tbRefundDetails[1].BackColor = cFrmBackColour;
                        tbRefundDetails[1].ForeColor = cFrmForeColour;
                        tbRefundDetails[1].Font = new Font(sFontName, 11.0f);
                        if (sBarcodeToRefund == "$GENERAL_REFUND")
                        {
                            tbRefundDetails[1].Enabled = false;
                            tbRefundDetails[3].Text = tbRefundDetails[1].Text;
                            tbRefundDetails[3].Font = new Font(sFontName, 11.0f);
                            fpiGetPaymentType = new frmPaymentInput(new Point(this.Left, lblRefundDetails[3].Top + 200), new Size(this.Width, this.Height - 70 - (lblRefundDetails[0].Top + 250)), tEngine.GetCreditCards(), fAmountToRefund, false, false, true);
                            fpiGetPaymentType.Show();
                            fpiGetPaymentType.GetAmountFromUser = false;
                            fpiGetPaymentType.FormClosed += new FormClosedEventHandler(fpiGetPaymentType_FormClosing);
                        }
                        else
                        {
                            tbRefundDetails[2].Text = "1";
                            tbRefundDetails[2].SelectAll();
                            lblWarning.Visible = false;
                        }
                    }
                    catch
                    {
                        // Invalid price entered
                        tbRefundDetails[1].Text = "";
                        tbRefundDetails[1].Focus();
                        lblInstruction.Text = "The price that was entered was invalid, please try again.";

                    }
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                if (nCurrentBox > 0)
                {
                    tbRefundDetails[nCurrentBox].Text = "";
                    tbRefundDetails[nCurrentBox - 1].Focus();
                }
                else if (nCurrentBox == 0)
                {
                    this.Close();
                }
                if (nCurrentBox == 1)
                {
                    if (sBarcodeToRefund != "$GENERAL_REFUND")
                    {
                        tbRefundDetails[0].BackColor = cFrmForeColour;
                        tbRefundDetails[0].ForeColor = cFrmBackColour;
                        tbRefundDetails[0].Text = "";
                        tbRefundDetails[0].Font = new Font(sFontName, 16.0f);
                        tbRefundDetails[0].Enabled = true;
                        tbRefundDetails[0].Focus();
                        tbRefundDetails[1].Tag = "0";
                    }
                    else
                    {
                        this.Close();
                    }
                }
                else if (nCurrentBox == 2)
                {
                    tbRefundDetails[1].BackColor = cFrmForeColour;
                    tbRefundDetails[1].ForeColor = cFrmBackColour;
                    tbRefundDetails[1].Text = "";
                    tbRefundDetails[1].Font = new Font(sFontName, 16.0f);
                    tbRefundDetails[1].Enabled = true;
                    tbRefundDetails[1].Focus();
                }
            }
            else if (e.KeyCode == Keys.OemQuestion && nCurrentBox == 0)
            {
                fsfiFindBarcode = new frmSearchForItemV2(ref tEngine);
                fsfiFindBarcode.Show();
                fsfiFindBarcode.FormClosing += new FormClosingEventHandler(fsfiLookupBarcode_FormClosing);
            }
            else if (e.KeyCode == Keys.F1)
            {
                if (!bShiftFunctionKeys)
                    tbCurrent.Text = tEngine.sBarcodeFromFunctionKey("F1");
                else
                    tbCurrent.Text = tEngine.sBarcodeFromFunctionKey("SF1");
                bShiftFunctionKeys = false;
                tbCurrent.Focus();
                SendKeys.Send("{ENTER}");
            }
            else if (e.KeyCode == Keys.F2)
            {
                if (!bShiftFunctionKeys)
                    tbCurrent.Text = tEngine.sBarcodeFromFunctionKey("F2");
                else
                    tbCurrent.Text = tEngine.sBarcodeFromFunctionKey("SF2");
                bShiftFunctionKeys = false;
                tbCurrent.Focus();
                SendKeys.Send("{ENTER}");
            }
            else if (e.KeyCode == Keys.F3)
            {
                if (!bShiftFunctionKeys)
                    tbCurrent.Text = tEngine.sBarcodeFromFunctionKey("F3");
                else
                    tbCurrent.Text = tEngine.sBarcodeFromFunctionKey("SF3");
                bShiftFunctionKeys = false;
                tbCurrent.Focus();
                SendKeys.Send("{ENTER}");
            }
            else if (e.KeyCode == Keys.F4)
            {
                if (!bShiftFunctionKeys)
                    tbCurrent.Text = tEngine.sBarcodeFromFunctionKey("F4");
                else
                    tbCurrent.Text = tEngine.sBarcodeFromFunctionKey("SF4");
                bShiftFunctionKeys = false;
                tbCurrent.Focus();
                SendKeys.Send("{ENTER}");
            }
            else if (e.KeyCode == Keys.F5)
            {
                if (!bShiftFunctionKeys)
                    tbCurrent.Text = tEngine.sBarcodeFromFunctionKey("F5");
                else
                    tbCurrent.Text = tEngine.sBarcodeFromFunctionKey("SF5");
                bShiftFunctionKeys = false;
                tbCurrent.Focus();
                SendKeys.Send("{ENTER}");
            }
            else if (e.KeyCode == Keys.F6)
            {
                if (!bShiftFunctionKeys)
                    tbCurrent.Text = tEngine.sBarcodeFromFunctionKey("F6");
                else
                    tbCurrent.Text = tEngine.sBarcodeFromFunctionKey("SF6");
                bShiftFunctionKeys = false;
                tbCurrent.Focus();
                SendKeys.Send("{ENTER}");
            }
            else if (e.KeyCode == Keys.F7)
            {
                if (!bShiftFunctionKeys)
                    tbCurrent.Text = tEngine.sBarcodeFromFunctionKey("F7");
                else
                    tbCurrent.Text = tEngine.sBarcodeFromFunctionKey("SF7");
                bShiftFunctionKeys = false;
                tbCurrent.Focus();
                SendKeys.Send("{ENTER}");
            }
            else if (e.KeyCode == Keys.F8)
            {
                if (!bShiftFunctionKeys)
                    tbCurrent.Text = tEngine.sBarcodeFromFunctionKey("F8");
                else
                    tbCurrent.Text = tEngine.sBarcodeFromFunctionKey("SF8");
                bShiftFunctionKeys = false;
                tbCurrent.Focus();
                SendKeys.Send("{ENTER}");
            }
            else if (e.KeyCode == Keys.F9)
            {
                if (!bShiftFunctionKeys)
                    tbCurrent.Text = tEngine.sBarcodeFromFunctionKey("F9");
                else
                    tbCurrent.Text = tEngine.sBarcodeFromFunctionKey("SF9");
                bShiftFunctionKeys = false;
                tbCurrent.Focus();
                SendKeys.Send("{ENTER}");
            }
            else if (e.KeyCode == Keys.F10)
            {
                if (!bShiftFunctionKeys)
                    tbCurrent.Text = tEngine.sBarcodeFromFunctionKey("F10");
                else
                    tbCurrent.Text = tEngine.sBarcodeFromFunctionKey("SF10");
                bShiftFunctionKeys = false;
                tbCurrent.Focus();
                SendKeys.Send("{ENTER}");
            }
            else if (e.KeyCode == Keys.F11)
            {
                if (!bShiftFunctionKeys)
                    tbCurrent.Text = tEngine.sBarcodeFromFunctionKey("F11");
                else
                    tbCurrent.Text = tEngine.sBarcodeFromFunctionKey("SF11");
                bShiftFunctionKeys = false;
                tbCurrent.Focus();
                SendKeys.Send("{ENTER}");
            }
            else if (e.KeyCode == Keys.F12)
            {
                if (!bShiftFunctionKeys)
                    tbCurrent.Text = tEngine.sBarcodeFromFunctionKey("F12");
                else
                    tbCurrent.Text = tEngine.sBarcodeFromFunctionKey("SF12");
                bShiftFunctionKeys = false;
                tbCurrent.Focus();
                SendKeys.Send("{ENTER}");
            }
        }

        /// <summary>
        /// Occurs when the payment selection form closes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fpiGetPaymentType_FormClosing(object sender, FormClosedEventArgs e)
        {
            string sResult = fpiGetPaymentType.GetPaymentMethod();
            sPaymentMethod = sResult;
            if (sResult == "CHRG")
            {
                fasGetAccountToRefund = new frmAccSel(ref tEngine);
                fasGetAccountToRefund.FormClosing += new FormClosingEventHandler(fasGetAccountToRefund_FormClosing);
                fasGetAccountToRefund.Show();
            }
            else if (sResult == "NULL")
            {
                if (sBarcodeToRefund != "$GENERAL_REFUND")
                {
                    tbRefundDetails[2].BackColor = cFrmForeColour;
                    tbRefundDetails[2].ForeColor = cFrmBackColour;
                    tbRefundDetails[2].Font = new Font(sFontName, 16.0f);
                    tbRefundDetails[0].Enabled = true;
                    tbRefundDetails[1].Enabled = true;
                    tbRefundDetails[2].Enabled = true;
                    tbRefundDetails[2].Focus();
                    tbRefundDetails[3].Text = "";
                }
                else
                {
                    tbRefundDetails[1].BackColor = cFrmForeColour;
                    tbRefundDetails[1].ForeColor = cFrmBackColour;
                    tbRefundDetails[1].Enabled = true;
                    tbRefundDetails[1].Focus();
                    tbRefundDetails[1].Font = new Font(sFontName, 16.0f);
                }
            }
            else
            {
                if (MessageBox.Show("Complete refund with payment method " + fpiGetPaymentType.GetPaymentDescription(sResult) + "?", "Refund", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (sBarcodeToRefund != "$GENERAL_REFUND")
                    {
                        tEngine.RefundItem(sBarcodeToRefund, fAmountToRefund, nQuantityToRefund, sPaymentMethod);
                    }
                    else
                    {
                        tEngine.RefundGeneral(fAmountToRefund, sPaymentMethod);
                    }
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Refund cancelled.");
                    this.Close();
                }
            }
        }

        /// <summary>
        /// Occurs when the account selection form closes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fasGetAccountToRefund_FormClosing(object sender, FormClosingEventArgs e)
        {
            string sCompanyToCharge = fasGetAccountToRefund.sGetAccountCode();
            sPaymentMethod = "CHRG," + sCompanyToCharge.TrimEnd(' ');
            if (sCompanyToCharge != "NONE_SELECTED" && MessageBox.Show("Complete refund with Charge To Account?", "Charge To Account Refund", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (sBarcodeToRefund != "$GENERAL_REFUND")
                {
                    tEngine.RefundItem(sBarcodeToRefund, fAmountToRefund, nQuantityToRefund, sPaymentMethod);
                }
                else
                {
                    tEngine.RefundGeneral(fAmountToRefund, sPaymentMethod);
                }
                this.Close();
            }
            else
            {
                MessageBox.Show("Refund cancelled.");
                this.Close();
            }
        }

        /// <summary>
        /// Occurs when the form has a key pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void frmRefund_KeyDown(object sender, KeyEventArgs e)
        {
            /*
            // Clears the barcode textbox
            if (tbRefundDetails[1].Tag.ToString() == "1" && e.KeyCode != Keys.Enter)
            {
                tbRefundDetails[1].Tag = "1.0";
                tbRefundDetails[1].Text = "";
            }
             */
        }

        /// <summary>
        /// Occurs when the barcode lookup window closes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fsfiLookupBarcode_FormClosing(object sender, FormClosingEventArgs e)
        {
            string sResult = fsfiFindBarcode.GetItemBarcode();
            string[] sItemDetails = tEngine.GetItemDetailsForLookup(sResult);
            if (sItemDetails != null)
            {
                tbRefundDetails[0].BackColor = cFrmBackColour;
                tbRefundDetails[0].ForeColor = cFrmForeColour;
                tbRefundDetails[0].Font = new Font(sFontName, 11.0f);
                tbRefundDetails[0].Text = sItemDetails[0];
                tbRefundDetails[1].Text = TillEngine.TillEngine.FormatMoneyForDisplay(TillEngine.TillEngine.fFormattedMoneyString(sItemDetails[1]));
                tbRefundDetails[1].KeyDown += new KeyEventHandler(frmRefund_KeyDown);
                tbRefundDetails[1].Tag = "1";
                tbRefundDetails[1].Focus();
                sBarcodeToRefund = sResult.ToUpper();
            }
            else
                tbRefundDetails[0].Text = "";
        }

        /// <summary>
        /// Occurs when the find partial barcode form closes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fsfiFindBarcode_FormClosing(object sender, FormClosingEventArgs e)
        {
            string sResult = fsfiFindBarcode.GetItemBarcode();
            if (sResult == "NONE_SELECTED")
            {
                tbRefundDetails[0].Text = "";
                tbRefundDetails[0].Focus();
            }
            else
            {
                string[] sItemDetails = tEngine.GetItemDetailsForLookup(sResult);
                tbRefundDetails[0].BackColor = cFrmBackColour;
                tbRefundDetails[0].ForeColor = cFrmForeColour;
                tbRefundDetails[0].Font = new Font(sFontName, 11.0f);
                tbRefundDetails[0].Text = sItemDetails[0];
                sBarcodeToRefund = sResult.ToUpper();
            }
        }

        /// <summary>
        /// Occurs when the refund listbox selection has a key pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void lbRefundTypeSelection_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                if (lbRefundTypeSelection.SelectedIndex == 0)
                    SetupForm(FormState.SpecificRefund);
                else if (lbRefundTypeSelection.SelectedIndex == 1)
                    SetupForm(FormState.GeneralRefund);
            }
        }
    }
}
