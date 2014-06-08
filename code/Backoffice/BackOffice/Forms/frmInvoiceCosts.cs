using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmInvoiceCosts : ScalableForm
    {
        StockEngine sEngine;
        CListBox lbBarcode;
        CListBox lbDesc;
        CListBox lbQtyRecd;
        CListBox lbCost;
        string sOrderNumber = "";
        bool bSaved = false;
        bool[] bCostChanged;
        decimal[] dOldCosts;
        decimal dSettlementDiscount = 0;

        public frmInvoiceCosts(ref StockEngine se)
        {
            sEngine = se;
            this.Size = new Size(1024, 768);
            this.SurroundListBoxes = true;

            lbBarcode = new CListBox();
            lbBarcode.Location = new Point(10, 31);
            lbBarcode.Size = new Size(200, this.ClientSize.Height - 10 - lbBarcode.Top);
            lbBarcode.BorderStyle = BorderStyle.None;
            lbBarcode.KeyDown += new KeyEventHandler(lbKeyDown);
            lbBarcode.SelectedIndexChanged += new EventHandler(lbSelChanged);
            this.Controls.Add(lbBarcode);
            AddMessage("BARCODE", "Barcode", new Point(10, 10));

            lbDesc = new CListBox();
            lbDesc.Location = new Point(210, 31);
            lbDesc.Size = new Size(300, this.ClientSize.Height - 10 - lbBarcode.Top);
            lbDesc.BorderStyle = BorderStyle.None;
            lbDesc.KeyDown +=new KeyEventHandler(lbKeyDown);
            lbDesc.SelectedIndexChanged +=new EventHandler(lbSelChanged);
            this.Controls.Add(lbDesc);
            AddMessage("DESC", "Description", new Point(210, 10));

            lbQtyRecd = new CListBox();
            lbQtyRecd.Location = new Point(510, 31);
            lbQtyRecd.Size = new Size(100, this.ClientSize.Height - 10 - lbBarcode.Top);
            lbQtyRecd.BorderStyle = BorderStyle.None;
            lbQtyRecd.KeyDown +=new KeyEventHandler(lbKeyDown);
            lbQtyRecd.RightToLeft = RightToLeft.Yes;
            lbQtyRecd.SelectedIndexChanged +=new EventHandler(lbSelChanged);
            this.Controls.Add(lbQtyRecd);
            AddMessage("RECD", "Qty Received", new Point(510, 10));

            lbCost = new CListBox();
            lbCost.Location = new Point(610, 31);
            lbCost.Size = new Size(100, this.ClientSize.Height - 10 - lbBarcode.Top);
            lbCost.BorderStyle = BorderStyle.None;
            lbCost.KeyDown +=new KeyEventHandler(lbKeyDown);
            lbCost.RightToLeft = RightToLeft.Yes;
            lbCost.SelectedIndexChanged +=new EventHandler(lbSelChanged);
            this.Controls.Add(lbCost);
            AddMessage("COST", "Cost", new Point(610, 10));

            this.VisibleChanged += new EventHandler(frmInvoiceCosts_VisibleChanged);
            this.FormClosing += new FormClosingEventHandler(frmInvoiceCosts_FormClosing);
            this.AllowScaling = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Text = "Enter Invoice Costs";
            this.Width = lbCost.Left + lbCost.Width + 10 + (this.Width - this.ClientSize.Width);
            // Get the settlement discount
            EnterSettlementDiscount();

        }

        /// <summary>
        /// Gets the settlement discount amount to apply at the end, if any
        /// </summary>
        private void EnterSettlementDiscount()
        {
            frmSingleInputBox fsiGetSettlement = new frmSingleInputBox("Enter the settlement discount percentage, if any:", ref sEngine);
            fsiGetSettlement.tbResponse.Text = "0.00";
            fsiGetSettlement.ShowDialog();
            try
            {
                dSettlementDiscount = Convert.ToDecimal(fsiGetSettlement.Response);
            }
            catch
            {
                dSettlementDiscount = 0;
            }
        }

        void frmInvoiceCosts_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!bSaved)
            {
                if (MessageBox.Show("Save Changes?", "Save Changes?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    SaveChanges();
            }
        }

        void lbSelChanged(object sender, EventArgs e)
        {
            lbBarcode.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbCost.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbDesc.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbQtyRecd.SelectedIndex = ((ListBox)sender).SelectedIndex;
        }

        void frmInvoiceCosts_VisibleChanged(object sender, EventArgs e)
        {
            if (sOrderNumber == "")
            {
                frmListOfOrders flos = new frmListOfOrders(ref sEngine);
                flos.ShowDialog();
                if (flos.OrderNumber == "$NONE")
                {
                    bSaved = true;
                    this.Close();
                }
                else
                {
                    sOrderNumber = flos.OrderNumber;
                    string[] sBarcodes = new string[0];
                    string[] sOrderQty = new string[0];
                    string[] sReceived = new string[0];
                    string[] sCost = new string[0];
                    string[] sInvoiced = new string[0];

                    sEngine.GetOrderData(sOrderNumber, ref sBarcodes, ref sOrderQty, ref sReceived, ref sCost, ref sInvoiced);
                    dOldCosts = new decimal[0];

                    for (int i = 0; i < sBarcodes.Length; i++)
                    {
                        if (Convert.ToDecimal(sReceived[i]) <= Convert.ToDecimal(sInvoiced[i]))
                            continue;
                        else
                        {
                            Array.Resize<decimal>(ref dOldCosts, dOldCosts.Length + 1);
                            dOldCosts[dOldCosts.Length - 1] = Convert.ToDecimal(sCost[i]);
                            lbBarcode.Items.Add(sBarcodes[i]);
                            lbCost.Items.Add(sCost[i]);
                            lbDesc.Items.Add(sEngine.GetMainStockInfo(sBarcodes[i])[1]);
                            lbQtyRecd.Items.Add(sReceived[i]);
                        }
                    }
                    if (lbBarcode.Items.Count <= 0)
                    {
                        MessageBox.Show("There aren't any invoice costs to enter. Do you need to receive the items first?");
                        bSaved = true;
                        this.Close();
                    }
                    else
                    {
                        bCostChanged = new bool[lbBarcode.Items.Count];
                        for (int i = 0; i < bCostChanged.Length; i++)
                            bCostChanged[i] = false;
                        lbBarcode.SelectedIndex = 0;
                    }
                }
            }
        }

        void lbKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                frmSingleInputBox fGetCost = new frmSingleInputBox("Enter the invoice cost :", ref sEngine);
                fGetCost.tbResponse.Text = lbCost.Items[((ListBox)sender).SelectedIndex].ToString();
                fGetCost.ShowDialog();
                if (fGetCost.Response != "$NONE")
                {
                    frmSingleInputBox fGetQuantity = new frmSingleInputBox("Enter the quantity of items that this cost represents :", ref sEngine);
                    fGetQuantity.tbResponse.Text = "1.00";
                    fGetQuantity.ShowDialog();
                    if (fGetQuantity.Response != "$NONE")
                    {
                        decimal dQuantity = Convert.ToDecimal(fGetQuantity.Response);
                        frmSingleInputBox fGetDiscount = new frmSingleInputBox("Enter the discount in percentage (excluding settlement), if any :", ref sEngine);
                        fGetDiscount.tbResponse.Text = "0.00";
                        fGetDiscount.ShowDialog();
                        if (fGetDiscount.Response != "$NONE")
                        {
                            decimal dInvoicePrice = Convert.ToDecimal(fGetCost.Response);
                            dInvoicePrice /= dQuantity;
                            dInvoicePrice -= (dInvoicePrice * (Convert.ToDecimal(fGetDiscount.Response) / 100));
                            bCostChanged[((ListBox)sender).SelectedIndex] = true;
                            lbCost.Items[((ListBox)sender).SelectedIndex] = FormatMoneyForDisplay(dInvoicePrice);
                        }
                    }
                }
                if (((ListBox)sender).SelectedIndex + 1 < ((ListBox)sender).Items.Count)
                {
                    ((ListBox)sender).SelectedIndex = ((ListBox)sender).SelectedIndex + 1;
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                if (MessageBox.Show("Save Changes?", "Save Changes?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    SaveChanges();
                }
                bSaved = true;
                this.Close();
            }
        }

        void SaveChanges()
        {
            bool bApplySettlement = false;
            string sSettlement = (dSettlementDiscount).ToString();
            if (dSettlementDiscount != 0 && MessageBox.Show("Would you like to apply the settlement discount of " + sSettlement + "%?", "Apply Settlement?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                bApplySettlement = true;
            }
            string[] sBarcodes = new string[lbBarcode.Items.Count];
            decimal[] dQtyInvoiced = new decimal[lbQtyRecd.Items.Count];
            decimal[] dInvCosts = new decimal[lbCost.Items.Count];
            for (int i = 0; i < sBarcodes.Length; i++)
            {
                sBarcodes[i] = lbBarcode.Items[i].ToString();
                if (bCostChanged[i])
                    dQtyInvoiced[i] = Convert.ToDecimal(lbQtyRecd.Items[i].ToString());
                else
                    dQtyInvoiced[i] = 0;

                dInvCosts[i] = Convert.ToDecimal(lbCost.Items[i].ToString());

                // Work out the settlement discount, if applicable
                if (bApplySettlement)
                {
                    dInvCosts[i] = Math.Round(((100 - (dSettlementDiscount)) / 100) * dInvCosts[i], 2);
                }
            }
            sEngine.EnterInvoiceCost(sOrderNumber, sBarcodes, dQtyInvoiced, dInvCosts, dOldCosts);
        }
    }
}
