using System;
using System.Collections.Generic;
using System.Text;
using TillEngine;
using System.Windows.Forms;
using System.Drawing;

namespace GTill
{
    class frmLookupTransactions : Form
    {
        /// <summary>
        /// The form background colour
        /// </summary>
        Color cFrmBackColour;
        /// <summary>
        /// The form foreground colour
        /// </summary>
        Color cFrmForeColour;
        /// <summary>
        /// Shows a list of today's transactions
        /// </summary>
        ListBox lbTransactions;
        /// <summary>
        /// Shows a list of the barcodes of products within the selected transaction
        /// </summary>
        ListBox lbProductCodes;
        /// <summary>
        /// Shows a list of the descriptions of products within the selected transaction
        /// </summary>
        ListBox lbProductDesc;
        /// <summary>
        /// Shows a list of the prices of products within the selected transaction
        /// </summary>
        ListBox lbProductPrice;
        /// <summary>
        /// Shows a list of the quantities of products within the selected transaction
        /// </summary>
        ListBox lbProductQuantity;
        /// <summary>
        /// Shows a list of payment methods for the selected transaction
        /// </summary>
        ListBox lbPaymentMethods;
        /// <summary>
        /// Shows a list of the amount paid using the listed payment methods for the selected transaction
        /// </summary>
        ListBox lbPaymentAmounts;
        /// <summary>
        /// Tells the user what the lists show
        /// </summary>
        Label[] lblListBoxDescs;
        /// <summary>
        /// Tells the user other information about the transaction
        /// </summary>
        Label[] lblOtherInfo;
        /// <summary>
        /// The title of the form
        /// </summary>
        Label lblTitle;
        /// <summary>
        /// A reference to the TillEngine
        /// </summary>
        TillEngine.TillEngine tillEngine;
        /// <summary>
        /// A list of transaction numbers for that day
        /// </summary>
        string[] sTransactionList;
        /// <summary>
        /// The transaction number that is currently selected
        /// </summary>
        string sTransactionSelected = "";
        /// <summary>
        /// The title of the form
        /// </summary>
        string sWindowTitle = "Lookup Transactions";
        /// <summary>
        /// Whether or not this form is being used to mark transactions as void
        /// </summary>
        bool bVoidTransactions = false;
        /// <summary>
        /// Whether or not the currently selected transaction is void
        /// </summary>
        bool bCurrentTransactionIsVoid = false;
        /// <summary>
        /// Whether or not the currently selected transaction can be marked as void
        /// </summary>
        bool bCanVoidTransaction = true;
        /// <summary>
        /// The font name to use on the form
        /// </summary>
        string sFontName;

        /// <summary>
        /// Initialises the form
        /// </summary>
        /// <param name="s">The size of the form</param>
        /// <param name="tE">A reference to the TillEngine</param>
        /// <param name="pStartLocation">The start location of the form</param>
        /// <param name="sInstruction">The purpose of this form (to look at transactions, void transactions etc)</param>
        public frmLookupTransactions(Size s, ref TillEngine.TillEngine tE, Point pStartLocation, string sInstruction)
        {
            cFrmBackColour = Properties.Settings.Default.cFrmBackColour;
            cFrmForeColour = Properties.Settings.Default.cFrmForeColour;
            this.ForeColor = cFrmForeColour;
            this.BackColor = cFrmBackColour;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = s;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = pStartLocation;
            tillEngine = tE;
            sFontName = Properties.Settings.Default.sFontName;
            if (sInstruction != null)
            {
                if (sInstruction == "REMOVE_TRANSACTION")
                {
                    sWindowTitle = "REMOVE TRANSACTION";
                    bVoidTransactions = false;
                }
                else if (sInstruction == "VOID_TRANSACTION")
                {
                    sWindowTitle = "Void Transaction";
                    bVoidTransactions = true;
                }
                else if (sInstruction == "REPRINT_RECEIPT")
                {
                    sWindowTitle = "Reprint Receipt";
                    bVoidTransactions = false;
                }
            }
                    
            SetupForm();
            GetListOfTransactions();
        }

        /// <summary>
        /// Sets up controls on the form
        /// </summary>
        void SetupForm()
        {
            lblTitle = new Label();
            lblTitle.BackColor = cFrmBackColour;
            lblTitle.ForeColor = cFrmForeColour;
            lblTitle.Font = new Font(sFontName, 14.0f);
            lblTitle.AutoSize = false;
            lblTitle.Location = new Point(0, 0);
            lblTitle.Size = new Size(this.Width, 25);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblTitle.Text = sWindowTitle;
            this.Controls.Add(lblTitle);

            lbTransactions = new ListBox();
            lbTransactions.BackColor = cFrmForeColour;
            lbTransactions.ForeColor = cFrmBackColour;
            lbTransactions.Font = new Font(sFontName, 14.0f);
            lbTransactions.Left = this.Width / 9;
            lbTransactions.Width = 100;
            lbTransactions.Top = this.Height / 5;
            lbTransactions.Height = 0;
            lbTransactions.BorderStyle = BorderStyle.None;
            lbTransactions.SelectedIndexChanged += new EventHandler(lbTransactions_SelectedIndexChanged);
            lbTransactions.KeyDown += new KeyEventHandler(lbTransactions_KeyDown);
            this.Controls.Add(lbTransactions);

            lbProductCodes = new ListBox();
            lbProductCodes.BackColor = cFrmForeColour;
            lbProductCodes.ForeColor = cFrmBackColour;
            lbProductCodes.Font = new Font(sFontName, 14.0f);
            lbProductCodes.Size = new Size(this.Width / 6, this.Height / 2);
            lbProductCodes.Location = new Point((this.Width / 8) + lbTransactions.Width, lbTransactions.Top);
            lbProductCodes.BorderStyle = BorderStyle.None;
            this.Controls.Add(lbProductCodes);

            lbProductDesc = new ListBox();
            lbProductDesc.BackColor = cFrmForeColour;
            lbProductDesc.ForeColor = cFrmBackColour;
            lbProductDesc.Font = new Font(sFontName, 14.0f);
            lbProductDesc.Size = new Size(this.Width / 3, this.Height / 2);
            lbProductDesc.Location = new Point(lbProductCodes.Left + lbProductCodes.Width, lbProductCodes.Top);
            lbProductDesc.BorderStyle = BorderStyle.None;
            this.Controls.Add(lbProductDesc);

            lbProductQuantity = new ListBox();
            lbProductQuantity.BackColor = cFrmForeColour;
            lbProductQuantity.ForeColor = cFrmBackColour;
            lbProductQuantity.Font = new Font(sFontName, 14.0f);
            lbProductQuantity.Size = new Size(this.Width / 12, this.Height / 2);
            lbProductQuantity.Location = new Point(lbProductDesc.Left + lbProductDesc.Width, lbProductDesc.Top);
            lbProductQuantity.BorderStyle = BorderStyle.None;
            this.Controls.Add(lbProductQuantity);

            lbProductPrice = new ListBox();
            lbProductPrice.BackColor = cFrmForeColour;
            lbProductPrice.ForeColor = cFrmBackColour;
            lbProductPrice.Font = new Font(sFontName, 14.0f);
            lbProductPrice.Size = new Size(this.Width / 8, lbProductDesc.Height);
            lbProductPrice.Location = new Point(lbProductQuantity.Left + lbProductQuantity.Width, lbProductDesc.Top);
            lbProductPrice.BorderStyle = BorderStyle.None;
            this.Controls.Add(lbProductPrice);

            lblListBoxDescs = new Label[7];
            for (int i = 0; i < lblListBoxDescs.Length; i++)
            {
                lblListBoxDescs[i] = new Label();
                lblListBoxDescs[i].BackColor = cFrmBackColour;
                lblListBoxDescs[i].ForeColor = cFrmForeColour;
                lblListBoxDescs[i].Font = new Font(sFontName, 12.0f);
                lblListBoxDescs[i].AutoSize = true;
                this.Controls.Add(lblListBoxDescs[i]);
                lblListBoxDescs[i].Text = "Test";
                lblListBoxDescs[i].Top = lbProductCodes.Top - lblListBoxDescs[i].Height - 3;
            }
            lblListBoxDescs[0].Text = "Product Code";
            lblListBoxDescs[0].Left = lbProductCodes.Left;
            lblListBoxDescs[1].Text = "Description";
            lblListBoxDescs[1].Left = lbProductDesc.Left;
            lblListBoxDescs[2].Text = "Qty";
            lblListBoxDescs[2].Left = lbProductQuantity.Left;
            lblListBoxDescs[3].Text = "Amount";
            lblListBoxDescs[3].Left = lbProductPrice.Left;
            lblListBoxDescs[4].Text = "Number";
            lblListBoxDescs[4].Left = lbTransactions.Left;

            lbPaymentMethods = new ListBox();
            lbPaymentMethods.BackColor = cFrmForeColour;
            lbPaymentMethods.ForeColor = cFrmBackColour;
            lbPaymentMethods.Font = new Font(sFontName, 14.0f);
            lbPaymentMethods.Size = new Size(lbProductQuantity.Left - lbProductCodes.Left - lbProductPrice.Width, this.Height / 10);
            lbPaymentMethods.Location = new Point(lbProductCodes.Left, lbProductCodes.Top + lbProductCodes.Height + lblListBoxDescs[0].Height);
            lbPaymentMethods.BorderStyle = BorderStyle.None;
            this.Controls.Add(lbPaymentMethods);

            lbPaymentAmounts = new ListBox();
            lbPaymentAmounts.BackColor = cFrmForeColour;
            lbPaymentAmounts.ForeColor = cFrmBackColour;
            lbPaymentAmounts.Font = new Font(sFontName, 14.0f);
            lbPaymentAmounts.Size = new Size(lbProductPrice.Width, lbPaymentMethods.Height);
            lbPaymentAmounts.Location = new Point(lbPaymentMethods.Left + lbPaymentMethods.Width, lbPaymentMethods.Top);
            lbPaymentAmounts.BorderStyle = BorderStyle.None;
            this.Controls.Add(lbPaymentAmounts);

            lblListBoxDescs[5].Text = "Payment Method";
            lblListBoxDescs[5].Top = lbPaymentMethods.Top - lblListBoxDescs[4].Height;
            lblListBoxDescs[5].Left = lbPaymentMethods.Left;
            lblListBoxDescs[6].Text = "Amount";
            lblListBoxDescs[6].Top = lblListBoxDescs[5].Top;
            lblListBoxDescs[6].Left = lbPaymentAmounts.Left;

            lblOtherInfo = new Label[5];
            for (int i = 0; i < lblOtherInfo.Length; i++)
            {
                lblOtherInfo[i] = new Label();
                lblOtherInfo[i].BackColor = cFrmBackColour;
                lblOtherInfo[i].ForeColor = cFrmForeColour;
                lblOtherInfo[i].Font = new Font(sFontName, 14.0f);
                lblOtherInfo[i].AutoSize = false;
                lblOtherInfo[i].Size = new Size((lbProductPrice.Width + lbProductPrice.Left) - lbProductQuantity.Left, (lbPaymentAmounts.Height + lbPaymentAmounts.Top - lblListBoxDescs[5].Top) / 5);
                if (lblOtherInfo[i].Height < 25)
                    lblOtherInfo[i].Height = 25;
                lblOtherInfo[i].TextAlign = ContentAlignment.MiddleRight;
                lblOtherInfo[i].Left = lbProductQuantity.Left;
                lblOtherInfo[i].Top = lblListBoxDescs[5].Top + (i * lblOtherInfo[i].Height);
                this.Controls.Add(lblOtherInfo[i]);
            }

        }

        /// <summary>
        /// Decides what to do when a key is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The key that has been pressed</param>
        void lbTransactions_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) // User wants to close
            {
                sTransactionSelected = "CANCELLED";
                this.Close();
            }
            else if (e.KeyCode == Keys.Enter && lbTransactions.SelectedIndex >= 0) // User has selected a transaction
            {
                if ((bVoidTransactions && !bCurrentTransactionIsVoid) || !bVoidTransactions || (sWindowTitle == "REMOVE_TRANSACTIONS" && !bCurrentTransactionIsVoid && bCanVoidTransaction))
                {
                    if ((bVoidTransactions && bCanVoidTransaction) || !bVoidTransactions)
                    {
                        if (sWindowTitle == "REMOVE TRANSACTION" && !bCanVoidTransaction)
                        {
                            MessageBox.Show("You can't remove this transaction"); // The transaction can't be removed
                        }
                        else
                        {
                            sTransactionSelected = lbTransactions.Items[lbTransactions.SelectedIndex].ToString();
                            this.Close();
                        }
                    }
                    else if (bVoidTransactions && !bCanVoidTransaction)
                    {
                        MessageBox.Show("You can't void this transaction"); // The transaction can't be voided
                    }
                }
                else
                {
                    MessageBox.Show("You can only select sale transactions"); // User has tried to select a non-sale transaction (such as a refund)
                }
            }
        }

        /// <summary>
        /// When the user selects a different transaction the display is updated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void lbTransactions_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowTransaction(lbTransactions.Items[lbTransactions.SelectedIndex].ToString());
        }

        /// <summary>
        /// Gets a list of transactions from today and adds them to a listbox
        /// </summary>
        void GetListOfTransactions()
        {
            sTransactionList = tillEngine.GetListOfTransactionNumbers();
            for (int i = 0; i < sTransactionList.Length; i++)
            {
                AddTransactionToListBox(sTransactionList[i]);
            }
            if (sTransactionList.Length > 0)
                lbTransactions.SelectedIndex = 0;
        }

        /// <summary>
        /// Adds a transaction to the listbox and also sorts out the height of that listbox
        /// </summary>
        /// <param name="sTransactionNumber"></param>
        void AddTransactionToListBox(string sTransactionNumber)
        {
            lbTransactions.Items.Add(sTransactionNumber);
            if (lbTransactions.Height + lbTransactions.Top + 25 < lbPaymentAmounts.Top)
                lbTransactions.Height += 25;
        }

        /// <summary>
        /// Shows the details of the selected transaction
        /// </summary>
        /// <param name="sTransactionNumber">The selected transaction</param>
        void ShowTransaction(string sTransactionNumber)
        {
            lbPaymentAmounts.Items.Clear();
            lbPaymentMethods.Items.Clear();
            lbProductCodes.Items.Clear();
            lbProductDesc.Items.Clear();
            lbProductPrice.Items.Clear();
            lbProductQuantity.Items.Clear();

            string[,] sTransactionInfo = tillEngine.GetTransactionInfo(sTransactionNumber);
            int nOfItems = Convert.ToInt32(sTransactionInfo[0, 0]);
            int nOfPaymentMethods = Convert.ToInt32(sTransactionInfo[0, 1]);
            float fNetAmount = 0.0f;
            float fDiscount = 0.0f;
            string sTransactionType = sTransactionInfo[0, 3];
             bCurrentTransactionIsVoid = false;
             bCanVoidTransaction = true;
            if (sTransactionType != "SALE")
            {
                switch (sTransactionType.Split(',')[0])
                {
                    case "CASHPAIDOUT":
                        lbProductDesc.Items.Add("Cash Paid Out");
                        break;
                    case "SPECIFICREFUND":
                        lbProductDesc.Items.Add("Specific Refund");
                        break;
                    case "GENERALREFUND":
                        lbProductDesc.Items.Add("General Refund");
                        break;
                    case "VOID":
                        string sUserNameVoided = sTransactionType.Split(',')[1];
                        lbProductDesc.Items.Add("Transaction Void by " + sUserNameVoided);
                        bCurrentTransactionIsVoid = true;
                        break;
                    case "RECEIVEDONACCOUNT":
                        lbProductDesc.Items.Add("Received on Account");
                        break;
                }
                bCanVoidTransaction = false;
                lbProductPrice.Items.Add("");
                lbProductQuantity.Items.Add("");
                lbProductCodes.Items.Add("");
            }
            else
                bCurrentTransactionIsVoid = false;
            for (int i = 1; i <= nOfItems; i++)
            {
                lbProductCodes.Items.Add(sTransactionInfo[i, 0]);
                lbProductDesc.Items.Add(sTransactionInfo[i, 1]);
                lbProductPrice.Items.Add(TillEngine.TillEngine.FormatMoneyForDisplay((float)Convert.ToDecimal(sTransactionInfo[i, 2])));
                lbProductQuantity.Items.Add(sTransactionInfo[i, 4]);
                fNetAmount += (float)Convert.ToDecimal(sTransactionInfo[i, 2]);
                fDiscount += (float)Convert.ToDecimal(sTransactionInfo[i, 3]);
            }
            lblOtherInfo[0].Text = "Net Amount : " + tillEngine.CurrencySymbol.ToString() + TillEngine.TillEngine.FormatMoneyForDisplay(tillEngine.fFixFloatError(fNetAmount + fDiscount));
            lblOtherInfo[1].Text = "Discount : " + tillEngine.CurrencySymbol.ToString() + TillEngine.TillEngine.FormatMoneyForDisplay(TillEngine.TillEngine.FixFloatError(fDiscount));
            lblOtherInfo[2].Text = "Total : " + tillEngine.CurrencySymbol.ToString() + TillEngine.TillEngine.FormatMoneyForDisplay(TillEngine.TillEngine.FixFloatError(fNetAmount));
            string[] sData = tillEngine.ReturnSensibleDateTimeString(sTransactionInfo[0, 2]);
            lblOtherInfo[3].Text = sData[0];
            lblOtherInfo[4].Text = sData[1];
            if (sTransactionType != "RECEIVEDONACCOUNT")
            {
                for (int i = nOfItems + 1; i < nOfItems + nOfPaymentMethods + 1; i++)
                {
                    lbPaymentMethods.Items.Add(tillEngine.GetPaymentDescription(sTransactionInfo[i, 0]));
                    lbPaymentAmounts.Items.Add(TillEngine.TillEngine.FormatMoneyForDisplay((float)Convert.ToDecimal(sTransactionInfo[i, 1])));
                }
            }
            else
            {
                lbPaymentMethods.Items.Add(tillEngine.GetPaymentDescription(sTransactionInfo[nOfItems +1, 0]));
                lbPaymentAmounts.Items.Add(TillEngine.TillEngine.FormatMoneyForDisplay((float)Convert.ToDecimal(sTransactionInfo[nOfItems +1, 1])));
            }

        }

        /// <summary>
        /// The selected transaction number
        /// </summary>
        public string TransactionNumber
        {
            get
            {
                return sTransactionSelected;
            }
            set
            {
                sTransactionSelected = value;
            }
        }
    }
}
