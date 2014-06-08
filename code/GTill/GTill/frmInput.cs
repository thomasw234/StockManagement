using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Text;

namespace GTill
{
    class frmInput : Form
    {
        /// <summary>
        /// The possible types of input form
        /// </summary>
        public enum FormType { MultiplicationAmount, DiscountAmount, DeleteLineNum, CashPaidOut, PresetKeyEntry, ReceivedOnAccount, GetUserID, GetOrderBarcode };
        public enum DiscountType { Percentage, Amount, SetPrice, SwapItemToLast };

        /// <summary>
        /// What the form is currently being used for
        /// </summary>
        FormType currentFormState;
        /// <summary>
        /// The background and font colours
        /// </summary>
        Color cFormBackColour, cFormForeColour;
        /// <summary>
        /// The input textbox
        /// </summary>
        TextBox tbInput;
        /// <summary>
        /// Tells the user what to do
        /// </summary>
        Label lblInstruction;
        /// <summary>
        /// Any other relevant information
        /// </summary>
        string[] sOtherData;
        /// <summary>
        /// The data that the user has input
        /// </summary>
        string sDataToReturn;
        /// <summary>
        /// The name of the font to use on the form
        /// </summary>
        string sFontName;
        /// <summary>
        /// If the form is used for a discount, this is the type of discount used
        /// </summary>
        DiscountType dType;
        /// <summary>
        /// If multiply, whether or not to set the quantity instead of multiply it
        /// </summary>
        public bool bSetQuantity = false;

        /// <summary>
        /// Initialises the form
        /// </summary>
        /// <param name="frmType">The type of input form that this is</param>
        /// <param name="pStartLocation">The location to start the form (the top-left corner)</param>
        /// <param name="sSize">The size of the form</param>
        /// <param name="sExtraInfo">Any extra relevant information</param>
        public frmInput(FormType frmType, Point pStartLocation, Size sSize, string[] sExtraInfo)
        {
            currentFormState = frmType;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = pStartLocation;
            this.Size = sSize;
            cFormBackColour = Properties.Settings.Default.cFrmBackColour;
            cFormForeColour = Properties.Settings.Default.cFrmForeColour;
            this.BackColor = cFormBackColour;
            this.ForeColor = cFormForeColour;
            this.FormBorderStyle = FormBorderStyle.None;
            sOtherData = sExtraInfo;
            sFontName = Properties.Settings.Default.sFontName;
            SetupForm();
        }

        /// <summary>
        /// Initialises and places controls based on the type on input form
        /// </summary>
        void SetupForm()
        {
            if (currentFormState == FormType.MultiplicationAmount)
            {
                lblInstruction = new Label();
                lblInstruction.Location = new Point(0, 0);
                lblInstruction.Font = new Font(sFontName, Properties.Settings.Default.fMainScreenFontSize);
                lblInstruction.ForeColor = cFormForeColour;
                lblInstruction.BackColor = cFormBackColour;
                lblInstruction.AutoSize = true;
                lblInstruction.Text = "Multiply Item Quantity By: ";
                this.Controls.Add(lblInstruction);

                tbInput = new TextBox();
                tbInput.BackColor = cFormBackColour;
                tbInput.ForeColor = cFormForeColour;
                tbInput.Location = lblInstruction.Location;
                tbInput.Left += lblInstruction.Width;
                tbInput.Font = lblInstruction.Font;
                tbInput.KeyDown += new KeyEventHandler(tbInput_KeyDown);
                this.Controls.Add(tbInput);
            }
            else if (currentFormState == FormType.DiscountAmount)
            {
                lblInstruction = new Label();
                lblInstruction.Location = new Point(0, 0);
                lblInstruction.Font = new Font(sFontName, Properties.Settings.Default.fMainScreenFontSize);
                lblInstruction.ForeColor = cFormForeColour;
                lblInstruction.BackColor = cFormBackColour;
                lblInstruction.AutoSize = true;
                lblInstruction.Text = "Discount Type: [P]ercentage, [A]mount, [S]et Price, [C]hange Line To Be Edited";
                dType = DiscountType.Amount;
                this.Controls.Add(lblInstruction);

                tbInput = new TextBox();
                tbInput.BackColor = cFormBackColour;
                tbInput.ForeColor = cFormForeColour;
                tbInput.Location = lblInstruction.Location;
                tbInput.Left += lblInstruction.Width;
                tbInput.Font = lblInstruction.Font;
                tbInput.Visible = false;
                tbInput.KeyDown += new KeyEventHandler(tbInput_KeyDown);
                this.Controls.Add(tbInput);

                this.KeyDown += new KeyEventHandler(frmInput_KeyDown);
            }
            else if (currentFormState == FormType.DeleteLineNum)
            {
                lblInstruction = new Label();
                lblInstruction.Location = new Point(0, 0);
                lblInstruction.Font = new Font(sFontName, Properties.Settings.Default.fMainScreenFontSize);
                lblInstruction.ForeColor = cFormForeColour;
                lblInstruction.BackColor = cFormBackColour;
                lblInstruction.AutoSize = true;
                lblInstruction.Text = "Enter the line number to delete :";
                this.Controls.Add(lblInstruction);

                tbInput = new TextBox();
                tbInput.BackColor = cFormBackColour;
                tbInput.ForeColor = cFormForeColour;
                tbInput.Location = lblInstruction.Location;
                tbInput.Left += lblInstruction.Width;
                tbInput.Font = lblInstruction.Font;
                tbInput.Visible = true;
                tbInput.KeyDown += new KeyEventHandler(tbInput_KeyDown);
                this.Controls.Add(tbInput);

                tbInput.Focus();
            }
            else if (currentFormState == FormType.CashPaidOut)
            {
                lblInstruction = new Label();
                lblInstruction.Location = new Point(0, 0);
                lblInstruction.Font = new Font(sFontName, Properties.Settings.Default.fMainScreenFontSize);
                lblInstruction.ForeColor = cFormForeColour;
                lblInstruction.BackColor = cFormBackColour;
                lblInstruction.AutoSize = true;
                lblInstruction.Text = "Enter amount of cash to pay out :";
                this.Controls.Add(lblInstruction);

                tbInput = new TextBox();
                tbInput.BackColor = cFormBackColour;
                tbInput.ForeColor = cFormForeColour;
                tbInput.Location = lblInstruction.Location;
                tbInput.Left += lblInstruction.Width;
                tbInput.Font = lblInstruction.Font;
                tbInput.Visible = true;
                tbInput.KeyDown += new KeyEventHandler(tbInput_KeyDown);
                this.Controls.Add(tbInput);
            }
            else if (currentFormState == FormType.PresetKeyEntry)
            {
                lblInstruction = new Label();
                lblInstruction.Location = new Point(0, 0);
                lblInstruction.Font = new Font(sFontName, Properties.Settings.Default.fMainScreenFontSize);
                lblInstruction.ForeColor = cFormForeColour;
                lblInstruction.BackColor = cFormBackColour;
                lblInstruction.AutoSize = true;
                lblInstruction.Text = "Enter the new barcode for " + sOtherData[0] + ":   ";
                this.Controls.Add(lblInstruction);

                tbInput = new TextBox();
                tbInput.BackColor = cFormBackColour;
                tbInput.ForeColor = cFormForeColour;
                tbInput.Location = lblInstruction.Location;
                tbInput.Left += lblInstruction.Width;
                tbInput.Width = 150;
                tbInput.Font = lblInstruction.Font;
                tbInput.BorderStyle = BorderStyle.None;
                tbInput.Visible = true;
                tbInput.KeyDown += new KeyEventHandler(tbInput_KeyDown);
                this.Controls.Add(tbInput);
            }
            else if (currentFormState == FormType.ReceivedOnAccount)
            {
                lblInstruction = new Label();
                lblInstruction.Location = new Point(0, 0);
                lblInstruction.Font = new Font(sFontName, Properties.Settings.Default.fMainScreenFontSize);
                lblInstruction.ForeColor = cFormForeColour;
                lblInstruction.BackColor = cFormBackColour;
                lblInstruction.AutoSize = true;
                lblInstruction.Text = "Enter amount to receive from " + sOtherData[0] + ":   ";
                this.Controls.Add(lblInstruction);

                tbInput = new TextBox();
                tbInput.BackColor = cFormBackColour;
                tbInput.ForeColor = cFormForeColour;
                tbInput.Location = lblInstruction.Location;
                tbInput.Left += lblInstruction.Width;
                tbInput.Font = lblInstruction.Font;
                tbInput.Visible = true;
                tbInput.KeyDown += new KeyEventHandler(tbInput_KeyDown);
                this.Controls.Add(tbInput);
            }
            else if (currentFormState == FormType.GetUserID)
            {
                lblInstruction = new Label();
                lblInstruction.Location = new Point(0, 0);
                lblInstruction.Font = new Font(sFontName, Properties.Settings.Default.fMainScreenFontSize);
                lblInstruction.ForeColor = cFormForeColour;
                lblInstruction.BackColor = cFormBackColour;
                lblInstruction.AutoSize = true;
                lblInstruction.Text = "Enter your Identification Number to start a transaction:";
                this.Controls.Add(lblInstruction);

                tbInput = new TextBox();
                tbInput.BackColor = cFormBackColour;
                tbInput.ForeColor = cFormForeColour;
                tbInput.Location = lblInstruction.Location;
                tbInput.Left += lblInstruction.Width;
                tbInput.Width = 150;
                tbInput.Font = lblInstruction.Font;
                tbInput.BorderStyle = BorderStyle.None;
                tbInput.Visible = true;
                tbInput.KeyDown += new KeyEventHandler(tbInput_KeyDown);
                this.Controls.Add(tbInput);
            }
        }

        /// <summary>
        /// Handles a key being pressed when the textbox has focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The key that has been pressed</param>
        void tbInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (currentFormState == FormType.DiscountAmount)
                {
                    if (dType == DiscountType.Percentage)
                    {
                        sDataToReturn = "PDISCOUNT,";
                    }
                    else if (dType == DiscountType.Amount)
                    {
                        sDataToReturn = "ADISCOUNT,";
                    }
                    else if (dType == DiscountType.SetPrice)
                    {
                        sDataToReturn = "SDISCOUNT,";
                    }
                    else if(dType == DiscountType.SwapItemToLast)
                    {
                        sDataToReturn = "SWAPITEM,";
                    }
                }
                else if (currentFormState == FormType.MultiplicationAmount)
                {
                    sDataToReturn = "MULTIPLY,";
                }
                else if (currentFormState == FormType.DeleteLineNum)
                {
                    sDataToReturn = "DELETELINE,";
                }
                else if (currentFormState == FormType.CashPaidOut || currentFormState == FormType.ReceivedOnAccount)
                {
                    try
                    {
                        float fAmountToPayOut = (float)Convert.ToDecimal(tbInput.Text);
                        if (fAmountToPayOut < 0.0f) // If a negative amount is input, then cancel
                            sDataToReturn = "CANCELLED";
                    }
                    catch
                    {
                        sDataToReturn = "CANCELLED";
                    }
                }
                // Don't do negative input check here, as discount could be price increase, unless SDISCOUNT
                if (sDataToReturn == "SDISCOUNT,")
                {
                    if (tbInput.Text.StartsWith("-"))
                        sDataToReturn = "CANCELLED";
                }
                if (sDataToReturn != "CANCELLED")
                    sDataToReturn += tbInput.Text;
                this.Close();
            }
            else if (e.KeyCode == Keys.Oemtilde && currentFormState == FormType.MultiplicationAmount)
            {
                if (!bSetQuantity)
                {
                    // Change to setting new quantity
                    lblInstruction.Text = "Enter The Quantity of the Item : ";
                    bSetQuantity = true;
                }
                else
                {
                    lblInstruction.Text = "Multiply Item Quantity By :";
                    bSetQuantity = false;
                }
                tbInput.Left = lblInstruction.Width;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Space) // If the user wants to exit
            {
                sDataToReturn = "CANCELLED";
                this.Close();
            }
        }

        /// <summary>
        /// Handles a keydown when the form has focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The key that has been pressed (should be A, S or P)</param>
        void frmInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (currentFormState == FormType.DiscountAmount && tbInput.Visible == false)
            {
                bool bCorrectKeyDown = false;
                if (e.KeyCode == Keys.A)
                {
                    dType = DiscountType.Amount;
                    lblInstruction.Text = "Enter amount to discount: ";
                    bCorrectKeyDown = true;
                }
                else if (e.KeyCode == Keys.P)
                {
                    dType = DiscountType.Percentage;
                    lblInstruction.Text = "Enter percentage to discount: ";
                    bCorrectKeyDown = true;
                }
                else if (e.KeyCode == Keys.S)
                {
                    dType = DiscountType.SetPrice;
                    lblInstruction.Text = "Enter new price: ";
                    bCorrectKeyDown = true;
                }
                else if (e.KeyCode == Keys.C)
                {
                    dType = DiscountType.SwapItemToLast;
                    lblInstruction.Text = "Enter the line of the item that you want to discount: ";
                    bCorrectKeyDown = true;
                }
                if (bCorrectKeyDown)
                {
                    tbInput.Left = lblInstruction.Width;
                    tbInput.Visible = true;
                    tbInput.Focus();
                }
            }
            if (e.KeyCode == Keys.Escape)
            {
                sDataToReturn = "CANCELLED";
                this.Close();
            }
        }

        /// <summary>
        /// Gets the data that has been input
        /// </summary>
        /// <returns>The data that has been input</returns>
        public string sGetDataToReturn()
        {
            return sDataToReturn;
        }
    }
}
