using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmReceiveOrder : ScalableForm
    {
        StockEngine sEngine;
        frmOrderSetup fOS;
        string[] SupplierRecord;
        string OrderNumber;
        string[] sBarcodes = new string[0];
        decimal dOrderTotal = 0;
        string sDefaultLineDisc;
        string sSettlement;
        bool bAskedAboutSaving = false;
        public bool bClosed = false;

        CListBox lbSupCode;
        CListBox lbDesc;
        CListBox lbRRP;
        CListBox lbQtyOrdered;
        CListBox lbCostPrice;
        CListBox lbQtyReceived;
        CListBox lbCostQty;
        CListBox lbMarked;
        CListBox lbDiscLine;
        CListBox lbDiscSettlement;
        ContextMenu mRightClick;

        public frmReceiveOrder(ref StockEngine se)
        {
            sEngine = se;
            this.FormClosing += new FormClosingEventHandler(frmReceiveOrder_FormClosing);
            this.SurroundListBoxes = true;
            frmListOfOrders floo = new frmListOfOrders(ref sEngine);
            floo.AllowAnEmptyOrder = true;
           
            floo.ShowDialog();
            if (floo.OrderNumber != "$NONE")
            {
                OrderNumber = floo.OrderNumber;

                fOS = new frmOrderSetup(ref sEngine);
                fOS.OrderNumberToCheck = OrderNumber;
                fOS.ShowDialog();
                if (!fOS.OrderIsOk)
                {
                    bClosed = true;
                    this.Close();
                    return;
                }
                else
                    SupplierRecord = fOS.OrderHeaderRecord;

                frmSingleInputBox fGetSettlement = new frmSingleInputBox("Enter the settlement discount in percentage here (comma separated if multiple):", ref sEngine);
                fGetSettlement.tbResponse.Text = "0.00";
                fGetSettlement.ShowDialog();
                if (fGetSettlement.Response != "$NONE")
                {
                    sSettlement = fGetSettlement.Response;
                }
                else
                    sSettlement = "0.00";

                frmSingleInputBox fGetLineDisc = new frmSingleInputBox("Enter the default line discount", ref sEngine);
                fGetLineDisc.tbResponse.Text = "0.00";
                fGetLineDisc.ShowDialog();
                if (fGetLineDisc.Response != "$NONE")
                {
                    sDefaultLineDisc = fGetLineDisc.Response;
                }
                else
                    sDefaultLineDisc = "";

                AddMessage("SUPNAME", "Supplier : " + sEngine.GetSupplierDetails(SupplierRecord[1])[1], new Point(10, 30));
                AddMessage("ORDNUM", "Order Number : " + SupplierRecord[0], new Point(10, BelowLastControl));
                AddMessage("ORDTOT", "Order Total : 0.00", new Point(this.Width - Convert.ToInt32(this.CreateGraphics().MeasureString("Order Total : 999999.99", this.Font).Width), 30));

                AddMessage("INST", "Enter to enter quantity received and cost, F3 to batch add items, F4 to edit item, F6 to complete the order by setting received quantities to ordered quantities, F7 for the opposite.", new Point(10, 10));
                AddMessage("INST2", "Press Y to mark an item, F8 to enter cost quantity. Discounts : F9 for this item, F10 for marked items and F11 for whole order. F12 to apply discounts, and Esc to save.", new Point(10, 10));


                MessageLabel("INST2").Top = this.ClientSize.Height - MessageLabel("INST2").Height - 10;
                MessageLabel("INST").Top = MessageLabel("INST2").Top - MessageLabel("INST").Height - 5;

                int desiredHeight = MessageLabel("INST").Top - 10 - 100;

                mRightClick = new ContextMenu();

                lbSupCode = new CListBox();
                lbSupCode.Location = new Point(10, 100);
                lbSupCode.Size = new Size(100, desiredHeight);
                lbSupCode.BorderStyle = BorderStyle.None;
                lbSupCode.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
                lbSupCode.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
                lbSupCode.ContextMenu = mRightClick;
                lbSupCode.MouseDown += new MouseEventHandler(lb_MouseDown);
                this.Controls.Add(lbSupCode);
                AddMessage("CODE", "Code", new Point(10, 80));

                lbDesc = new CListBox();
                lbDesc.Location = new Point(lbSupCode.Left + lbSupCode.Width, 100);
                lbDesc.Size = new Size(250, desiredHeight);
                lbDesc.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
                lbDesc.BorderStyle = BorderStyle.None;
                lbDesc.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
                lbDesc.ContextMenu = mRightClick;
                lbDesc.MouseDown += new MouseEventHandler(lb_MouseDown);
                this.Controls.Add(lbDesc);
                AddMessage("DESC", "Description", new Point(lbDesc.Left, 80));
                lbSupCode.ShowScrollbar = false;

                lbRRP = new CListBox();
                lbRRP.Location = new Point(lbDesc.Left + lbDesc.Width, lbDesc.Top);
                lbRRP.ContextMenu = mRightClick;
                lbRRP.Size = new Size(75, desiredHeight);
                lbRRP.BorderStyle = BorderStyle.None;
                lbRRP.MouseDown += new MouseEventHandler(lb_MouseDown);
                lbRRP.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
                lbRRP.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
                lbRRP.RightToLeft = RightToLeft.Yes;
                this.Controls.Add(lbRRP);
                AddMessage("RRP", "RRP", new Point(lbRRP.Left, 80));
                lbRRP.ShowScrollbar = false;

                lbQtyOrdered = new CListBox();
                lbQtyOrdered.Location = new Point(lbRRP.Left + lbRRP.Width, lbRRP.Top);
                lbQtyOrdered.ContextMenu = mRightClick;
                lbQtyOrdered.Size = new Size(60, desiredHeight);
                lbQtyOrdered.BorderStyle = BorderStyle.None;
                lbQtyOrdered.MouseDown += new MouseEventHandler(lb_MouseDown);
                lbQtyOrdered.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
                lbQtyOrdered.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
                lbQtyOrdered.RightToLeft = RightToLeft.Yes;
                this.Controls.Add(lbQtyOrdered);
                AddMessage("QTY", "O/S", new Point(lbQtyOrdered.Left, 80));
                lbQtyOrdered.ShowScrollbar = false;

                lbQtyReceived = new CListBox();
                lbQtyReceived.Location = new Point(lbQtyOrdered.Left + lbQtyOrdered.Width, lbQtyOrdered.Top);
                lbQtyReceived.Size = new Size(50, desiredHeight);
                lbQtyReceived.MouseDown += new MouseEventHandler(lb_MouseDown);
                lbQtyReceived.BorderStyle = BorderStyle.None;
                lbQtyReceived.ContextMenu = mRightClick;
                lbQtyReceived.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
                lbQtyReceived.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
                lbQtyReceived.RightToLeft = RightToLeft.Yes;
                this.Controls.Add(lbQtyReceived);
                AddMessage("ONORDER", "Rec'd", new Point(lbQtyReceived.Left, 80));
                lbQtyReceived.ShowScrollbar = false;

                lbCostPrice = new CListBox();
                lbCostPrice.Location = new Point(lbQtyReceived.Left + lbQtyReceived.Width, lbQtyOrdered.Top);
                lbCostPrice.Size = new Size(100, desiredHeight);
                lbCostPrice.BorderStyle = BorderStyle.None;
                lbCostPrice.ContextMenu = mRightClick;
                lbCostPrice.MouseDown += new MouseEventHandler(lb_MouseDown);
                lbCostPrice.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
                lbCostPrice.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
                lbCostPrice.RightToLeft = RightToLeft.Yes;
                this.Controls.Add(lbCostPrice);
                AddMessage("COST", "Cost", new Point(lbCostPrice.Left, 80));
                lbCostPrice.ShowScrollbar = false;

                lbCostQty = new CListBox();
                lbCostQty.Location = new Point(lbCostPrice.Left + lbCostPrice.Width, lbCostPrice.Top);
                lbCostQty.Size = new Size(75, desiredHeight);
                lbCostQty.BorderStyle = BorderStyle.None;
                lbCostQty.ContextMenu = mRightClick;
                lbCostQty.MouseDown += new MouseEventHandler(lb_MouseDown);
                lbCostQty.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
                lbCostQty.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
                lbCostQty.RightToLeft = RightToLeft.Yes;
                this.Controls.Add(lbCostQty);
                AddMessage("COSTQTY", "Cost Qty", new Point(lbCostQty.Left, 80));
                lbCostQty.ShowScrollbar = false;

                lbMarked = new CListBox();
                lbMarked.Location = new Point(lbCostQty.Left + lbCostQty.Width, lbCostQty.Top);
                lbMarked.Size = new Size(50, desiredHeight);
                lbMarked.BorderStyle = BorderStyle.None;
                lbMarked.ContextMenu = mRightClick;
                lbMarked.MouseDown += new MouseEventHandler(lb_MouseDown);
                lbMarked.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
                lbMarked.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
                this.Controls.Add(lbMarked);
                AddMessage("MARKED", "Marked", new Point(lbMarked.Left, 80));
                lbMarked.ShowScrollbar = false;

                lbDiscLine = new CListBox();
                lbDiscLine.Location = new Point(lbMarked.Left + lbMarked.Width, lbMarked.Top);
                lbDiscLine.Size = new Size(100, desiredHeight);
                lbDiscLine.BorderStyle = BorderStyle.None;
                lbDiscLine.ContextMenu = mRightClick;
                lbDiscLine.MouseDown += new MouseEventHandler(lb_MouseDown);
                lbDiscLine.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
                lbDiscLine.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
                this.Controls.Add(lbDiscLine);
                AddMessage("DISC", "Discounts (%)", new Point(lbDiscLine.Left, 80));
                lbDiscLine.ShowScrollbar = false;

                lbDiscSettlement = new CListBox();
                lbDiscSettlement.Location = new Point(lbDiscLine.Left + lbDiscLine.Width, lbDiscLine.Top);
                lbDiscSettlement.Size = new Size(this.Width - 20 - lbDiscSettlement.Left, desiredHeight);
                lbDiscSettlement.BorderStyle = BorderStyle.None;
                lbDiscSettlement.ContextMenu = mRightClick;
                lbDiscSettlement.MouseDown += new MouseEventHandler(lb_MouseDown);
                lbDiscSettlement.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
                lbDiscSettlement.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
                this.Controls.Add(lbDiscSettlement);
                AddMessage("SETTLE", "Settlement (%)", new Point(lbDiscSettlement.Left, 80));
                lbDiscSettlement.ShowScrollbar = false;


                if (fOS.OrderExists && sEngine.DoesOrderExist(SupplierRecord[0]))
                {
                    string[] sBarcodes2 = new string[0];
                    string[] sQuantities = new string[0];
                    string[] sReceived = new string[0];
                    string[] sCost = new string[0];

                    sEngine.GetOrderData(SupplierRecord[0], ref sBarcodes2, ref sQuantities, ref sReceived, ref sCost);
                    frmProgressBar fp = new frmProgressBar("Loading Order");
                    fp.pb.Maximum = sBarcodes2.Length;
                    fp.Show();
                    for (int i = 0; i < sBarcodes2.Length; i++)
                    {
                        fp.pb.Value = i;
                        AddRow(sBarcodes2[i], Convert.ToDecimal(sQuantities[i]));
                        lbCostPrice.Items[i] = FormatMoneyForDisplay(sCost[i]);
                        lbQtyOrdered.Items[i] = FormatMoneyForDisplay(Convert.ToDecimal(sQuantities[i]) - Convert.ToDecimal(sReceived[i]));
                        lbQtyReceived.Items[i] = "0.00";
                    }
                    fp.Close();
                }

                MenuItem[] mItems = new MenuItem[8];
                MenuItem[] mDiscItems = new MenuItem[5];

                mDiscItems[0] = new MenuItem("This Item");
                mDiscItems[1] = new MenuItem("Marked Items");
                mDiscItems[2] = new MenuItem("All Items");
                mDiscItems[3] = new MenuItem("Apply Pending Discounts");
                mDiscItems[4] = new MenuItem("Edit Discount Row");

                mItems[0] = new MenuItem("Edit Quantity Received");
                mItems[1] = new MenuItem("Edit Invoice Cost");
                mItems[2] = new MenuItem("Discount", mDiscItems);
                mItems[3] = new MenuItem("Edit Cost Quantity");
                mItems[4] = new MenuItem("Remove Item From Order");
                mItems[5] = new MenuItem("Edit Quantity Ordered");
                mItems[6] = new MenuItem("Match Order Qty To Received");
                mItems[7] = new MenuItem("Edit Item Information");

                for (int i = 0; i < mItems.Length; i++)
                {
                    mRightClick.MenuItems.Add(mItems[i]);
                }

                mItems[0].Click += new EventHandler(EditQtyClick);
                mItems[1].Click += new EventHandler(EditInvoiceCost);
                mItems[3].Click += new EventHandler(EditCostQtyClick);
                mItems[4].Click += new EventHandler(RemoveItemClick);
                mItems[5].Click += new EventHandler(EditQtyOrdClick);
                mItems[6].Click += new EventHandler(MatchOrderedRecClick);
                mItems[7].Click += new EventHandler(EditItemClick);
                mDiscItems[0].Click += new EventHandler(ThisItemClick);
                mDiscItems[1].Click += new EventHandler(MarkedItemsClick);
                mDiscItems[2].Click += new EventHandler(AllItemsClick);
                mDiscItems[3].Click += new EventHandler(ApplyDiscClick);
                mDiscItems[4].Click += new EventHandler(EditDiscRowClick);

                this.VisibleChanged += new EventHandler(frmReceiveOrder_VisibleChanged);
                this.WindowState = FormWindowState.Maximized;
                this.ResizeEnd += new EventHandler(frmReceiveOrder_ResizeEnd);
                this.Text = "Receive An Order";

                if (sBarcodes.Length == 0)
                {
                    if (MessageBox.Show("This is an empty order. Would you like to batch add and receive some items? If not, you can do so later by pressing F3.", "Batch-Add?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        BatchAddAndReceive();
                    }
                }

            }
            else
            {
                bClosed = true;
                this.Close();
            }

            // Don't try to sum the order total if the window was closed! Crashes otherwise
            if (!bClosed)
                SumOrderTotal();

            this.recalculateSizes();
        }

        void frmReceiveOrder_FormClosing(object sender, FormClosingEventArgs e)
        {
            bClosed = true;
        }

        void frmReceiveOrder_ResizeEnd(object sender, EventArgs e)
        {
            this.recalculateSizes(); 
        }

        private void recalculateSizes()
        {
            try
            {
                MessageLabel("INST2").Top = this.ClientSize.Height - MessageLabel("INST2").Height - 10;
                MessageLabel("INST").Top = MessageLabel("INST2").Top - MessageLabel("INST").Height - 5;

                int desiredHeight = MessageLabel("INST").Top - 10 - lbCostQty.Top;
                lbCostPrice.Height = desiredHeight;
                lbCostQty.Height = desiredHeight;
                lbDesc.Height = desiredHeight;
                lbDiscLine.Height = desiredHeight;
                lbDiscSettlement.Height = desiredHeight;
                lbMarked.Height = desiredHeight;
                lbQtyOrdered.Height = desiredHeight;
                lbQtyReceived.Height = desiredHeight;
                lbRRP.Height = desiredHeight;
                lbSupCode.Height = desiredHeight;

                lbDiscSettlement.Width = this.ClientSize.Width - lbDiscSettlement.Left - lbSupCode.Left;
            }
            catch (NullReferenceException)
            {
                // Form hasn't drawn yet, not a problem
            }
        }

        void EditItemClick(object sender, EventArgs e)
        {
            if (lbSupCode.SelectedIndex != -1)
            {
                frmAddEditItem faei = new frmAddEditItem(ref sEngine);
                faei.EditingBarcode = sBarcodes[lbDesc.SelectedIndex];
                faei.ShowDialog();
                string[] sMainStock = sEngine.GetMainStockInfo(sBarcodes[lbDesc.SelectedIndex]);
                lbDesc.Items[lbDesc.SelectedIndex] = sMainStock[1];
                lbRRP.Items[lbDesc.SelectedIndex] = FormatMoneyForDisplay(sMainStock[2]);
            }
        }

        void EditDiscRowClick(object sender, EventArgs e)
        {
            if (lbSupCode.SelectedIndex != -1)
            {
                EditDiscounts(lbDesc.SelectedIndex);
            }
        }

        void MatchOrderedRecClick(object sender, EventArgs e)
        {
            if (lbSupCode.SelectedIndex != -1)
            {
                MatchOrderedQuantityToReceived();
            }
        }

        void EditDiscounts(int nRow)
        {
            frmSingleInputBox fEditDisc = new frmSingleInputBox("Enter the discount percentages with a percentage sign, separated by a comma:", ref sEngine);
            fEditDisc.tbResponse.Text = lbDiscLine.Items[nRow].ToString();
            fEditDisc.ShowDialog();
            lbDiscLine.Items[nRow] = fEditDisc.tbResponse.Text;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!bAskedAboutSaving)
            {
                switch (MessageBox.Show("Save this order?", "Save", MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.Yes:
                        SaveOrder();
                        break;
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
            base.OnClosing(e);
        }

        void MatchOrderedQuantityToReceived()
        {
            if (MessageBox.Show("This will change the order quantity for each item to the amount that you have entered have been received. Continue?", "Match Ordered with Received?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                for (int i = 0; i < lbDesc.Items.Count; i++)
                {
                    if (lbQtyOrdered.Items[i].ToString() != lbQtyReceived.Items[i].ToString())
                    {
                        EditQuantityOrdered(i, lbQtyReceived.Items[i].ToString());
                        this.Refresh();
                    }
                }
            }
        }

        void EditQtyOrdClick(object sender, EventArgs e)
        {
            if (lbSupCode.SelectedIndex != -1)
            {
                EditQuantityOrdered(lbDesc.SelectedIndex);
            }
        }

        void RemoveItemClick(object sender, EventArgs e)
        {
            if (lbSupCode.SelectedIndex != -1)
            {
                RemoveItemFromOrder(lbDesc.SelectedIndex);
            }
        }

        void EditCostQtyClick(object sender, EventArgs e)
        {
            if (lbSupCode.SelectedIndex != -1)
            {
                EditCostQty(lbDesc.SelectedIndex);
            }
        }

        void ApplyDiscClick(object sender, EventArgs e)
        {
            WorkOutCosts();
        }

        void AllItemsClick(object sender, EventArgs e)
        {
            if (lbSupCode.SelectedIndex != -1)
            {
                DiscountAllItems();
            }
        }

        void MarkedItemsClick(object sender, EventArgs e)
        {
            if (lbSupCode.SelectedIndex != -1)
            {
                DiscountMarkedItems();
            }
        }

        void ThisItemClick(object sender, EventArgs e)
        {
            if (lbSupCode.SelectedIndex != -1)
            {
                DiscountSingleItem(lbDesc.SelectedIndex);
            }
        }

        void EditInvoiceCost(object sender, EventArgs e)
        {
            if (lbSupCode.SelectedIndex != -1)
            {
                EditCostPrice(lbDesc.SelectedIndex);
            }
        }

        void EditQtyClick(object sender, EventArgs e)
        {
            if (lbSupCode.SelectedIndex != -1)
            {
                EditQtyReceived(lbDesc.SelectedIndex);
            }
        }

        void lb_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ((ListBox)sender).SelectedIndex = ((ListBox)sender).IndexFromPoint(new Point(e.X, e.Y));
            }
        }

        void RemoveItemFromOrder(int nRow)
        {
            if (nRow < 0)
                return;
            // Take the item off the order if it hasn't been delivered
            string[] sBCodes = new string[0];
            string[] sQtyOrd = new string[0];
            string[] sQtyRec = new string[0];
            string[] sCostPrice = new string[0];

            sEngine.GetOrderData(OrderNumber, ref sBCodes, ref sQtyOrd, ref sQtyRec, ref sCostPrice);
            sEngine.RemoveQuantityOnOrder(sBCodes[nRow], Convert.ToDecimal(sQtyOrd[nRow]) - Convert.ToDecimal(sQtyRec[nRow]), fOS.OrderHeaderRecord[6]);
            for (int i = nRow; i < sBarcodes.Length - 1; i++)
            {
                sBCodes[i] = sBCodes[i + 1];
                sQtyOrd[i] = sQtyOrd[i + 1];
                sQtyRec[i] = sQtyRec[i + 1];
                sCostPrice[i] = sCostPrice[i + 1];
            }
            /*frmOrderSetup fOS;
        string[] SupplierRecord;
        string OrderNumber;
        decimal dOrderTotal = 0;
        string sDefaultLineDisc;
        string sSettlement;
        bool bAskedAboutSaving = false;
        public bool bClosed = false;

        ContextMenu mRightClick;*/
            Array.Resize<string>(ref sBCodes, sBCodes.Length - 1);
            Array.Resize<string>(ref sQtyOrd, sQtyOrd.Length - 1);
            Array.Resize<string>(ref sQtyRec, sQtyRec.Length - 1);
            Array.Resize<string>(ref sCostPrice, sCostPrice.Length - 1);
            sEngine.AddEditOrderData(sBCodes, sQtyOrd, sQtyRec, sCostPrice, OrderNumber);


            sBarcodes = sBCodes;
            lbDesc.Items.RemoveAt(nRow);
            //lbRRP.Items.Remove(nRow);
            lbCostPrice.Items.RemoveAt(nRow);
            lbCostQty.Items.RemoveAt(nRow);
            lbDiscLine.Items.RemoveAt(nRow);
            lbMarked.Items.RemoveAt(nRow);
            lbQtyOrdered.Items.RemoveAt(nRow);
            lbQtyReceived.Items.RemoveAt(nRow);
            lbSupCode.Items.RemoveAt(nRow);
            lbRRP.Items.RemoveAt(nRow);
            lbDiscSettlement.Items.RemoveAt(nRow);
            if (lbSupCode.Items.Count == 0)
            {
                ; // Do nothing, it's an empty order!
            }
            else if (nRow < lbSupCode.Items.Count)
            {
                lbSupCode.SelectedIndex = nRow;
            }
            else if (nRow >= 0)
            {
                lbSupCode.SelectedIndex = nRow -1;
            }
            //else { empty order! }
            SumOrderTotal();
        }

        void EditQuantityOrdered(int nRow)
        {
            frmSingleInputBox fGetNewQty = new frmSingleInputBox("How many were ordered?", ref sEngine);
            fGetNewQty.ShowDialog();
            if (fGetNewQty.Response != "$NONE")
            {
                string[] sBCodes = new string[0];
                string[] sQtyOrd = new string[0];
                string[] sQtyRec = new string[0];
                string[] sCostPrice = new string[0];

                sEngine.GetOrderData(OrderNumber, ref sBCodes, ref sQtyOrd, ref sQtyRec, ref sCostPrice);
                sQtyOrd[nRow] = fGetNewQty.Response;
                lbQtyOrdered.Items[nRow] = fGetNewQty.Response;
                sEngine.AddEditOrderData(sBCodes, sQtyOrd, sQtyRec, sCostPrice, OrderNumber);
            }
        }
        void EditQuantityOrdered(int nRow, string sQtyOrdered)
        {
                string[] sBCodes = new string[0];
                string[] sQtyOrd = new string[0];
                string[] sQtyRec = new string[0];
                string[] sCostPrice = new string[0];

                sEngine.GetOrderData(OrderNumber, ref sBCodes, ref sQtyOrd, ref sQtyRec, ref sCostPrice);
                sQtyOrd[nRow] = sQtyOrdered;
                lbQtyOrdered.Items[nRow] = sQtyOrdered;
                sEngine.AddEditOrderData(sBCodes, sQtyOrd, sQtyRec, sCostPrice, OrderNumber);
        }
            

        void frmReceiveOrder_VisibleChanged(object sender, EventArgs e)
        {
            lbSupCode.Size = new Size(MessageLabel("DESC").Left - lbSupCode.Left, this.Height - 200);
            lbDesc.Size = new Size(MessageLabel("RRP").Left - lbDesc.Left, this.Height - 200);
            lbRRP.Size = new Size(MessageLabel("QTY").Left - lbRRP.Left, this.Height - 200);
            lbQtyOrdered.Size = new Size(MessageLabel("ONORDER").Left - lbQtyOrdered.Left, this.Height - 200);
            lbQtyReceived.Size = new Size(MessageLabel("COST").Left - lbQtyReceived.Left, this.Height - 200);
            lbCostPrice.Size = new Size(MessageLabel("COSTQTY").Left - lbCostPrice.Left, this.Height - 200);
            lbCostQty.Size = new Size(MessageLabel("MARKED").Left - lbCostQty.Left, this.Height - 200);
            lbDiscLine.Size = new Size(MessageLabel("SETTLE").Left - lbDiscLine.Left, this.Height - 200);
            lbDiscSettlement.Size = new Size(this.Width - lbDiscSettlement.Left - 20, this.Height - 200);
            lbMarked.Size = new Size(MessageLabel("DISC").Left - lbMarked.Left, this.Height - 200);
            MessageLabel("INST").Top = lbDesc.Top + lbDesc.Height + 10;
            MessageLabel("INST2").Top = lbDesc.Top + lbDesc.Height + 35;

            this.recalculateSizes();

            MessageLabel("RRP").AutoSize = false;
            MessageLabel("RRP").Width = lbRRP.Width;
            MessageLabel("RRP").TextAlign = ContentAlignment.TopRight;
            MessageLabel("QTY").AutoSize = false;
            MessageLabel("QTY").Width = lbQtyOrdered.Width;
            MessageLabel("QTY").TextAlign = ContentAlignment.TopRight;
            MessageLabel("COST").AutoSize = false;
            MessageLabel("COST").Width = lbCostPrice.Width;
            MessageLabel("COST").TextAlign = ContentAlignment.TopRight;
            MessageLabel("ONORDER").AutoSize = false;
            MessageLabel("ONORDER").Width = lbQtyReceived.Width;
            MessageLabel("ONORDER").TextAlign = ContentAlignment.TopRight;
            MessageLabel("COSTQTY").AutoSize = false;
            MessageLabel("COSTQTY").Width = lbCostQty.Width;
            MessageLabel("COSTQTY").TextAlign = ContentAlignment.TopRight;


        }

        string ValidKey(KeyEventArgs e)
        {
            if (e.KeyCode.ToString().StartsWith("NumPad") || (e.KeyCode.ToString().Length > 1 && (e.KeyCode.ToString()[0] == 'D') && (e.KeyCode.ToString()[1] >= 48 && e.KeyCode.ToString()[1] <= 57)) || (e.KeyCode.ToString().Length > 1 &&e.KeyCode.ToString()[1] == 46))
            {
                return e.KeyCode.ToString().TrimStart("NumPad".ToCharArray()).TrimStart('D');
            }
            else
                return "No";
        }

        void lbDesc_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                EditItem(((ListBox)sender).SelectedIndex);
                if (lbDesc.Items.Count -1 > ((ListBox)sender).SelectedIndex)
                    ((ListBox)sender).SelectedIndex = ((ListBox)sender).SelectedIndex + 1;
            }
            else if (e.KeyCode == Keys.Insert)
            {
                AddItemToOrder();
            }
            else if (ValidKey(e) != "No")
            {
                e.SuppressKeyPress = false;
                EditItem(((ListBox)sender).SelectedIndex, ValidKey(e));
                lbDesc.SelectedIndex = ((ListBox)sender).SelectedIndex;
                if (lbDesc.Items.Count > ((ListBox)sender).SelectedIndex + 1)
                    ((ListBox)sender).SelectedIndex = ((ListBox)sender).SelectedIndex + 1;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                switch (MessageBox.Show("Save this order?", "Save", MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.Yes:
                        bAskedAboutSaving = true;
                        SaveOrder();
                        this.Close();
                        break;
                    case DialogResult.No:
                        bAskedAboutSaving = true;
                        this.Close();
                        break;
                }
            }
            else if (e.KeyCode == Keys.F3)
            {
                BatchAddAndReceive();
            }
            else if (e.KeyCode == Keys.F4)
            {
                frmAddEditItem faei = new frmAddEditItem(ref sEngine);
                faei.EditingBarcode = sBarcodes[lbDesc.SelectedIndex];
                faei.ShowDialog();
                string[] sMainStock = sEngine.GetMainStockInfo(sBarcodes[lbDesc.SelectedIndex]);
                lbDesc.Items[lbDesc.SelectedIndex] = sMainStock[1];
                lbRRP.Items[lbDesc.SelectedIndex] = FormatMoneyForDisplay(sMainStock[2]);
            }
            else if (e.KeyCode == Keys.A || e.KeyCode == Keys.F6)
            {
                CompleteOrder();
            }
            else if (e.KeyCode == Keys.Y)
            {
                if (lbMarked.Items[((ListBox)sender).SelectedIndex].ToString() == "")
                    lbMarked.Items[((ListBox)sender).SelectedIndex] = "Y";
                else
                    lbMarked.Items[((ListBox)sender).SelectedIndex] = "";
            }
            else if (e.KeyCode == Keys.F7)
            {
                MatchOrderedQuantityToReceived();
            }
            else if (e.KeyCode == Keys.F8)
            {
                EditCostQty(((ListBox)sender).SelectedIndex);
            }
            else if (e.KeyCode == Keys.F9)
            {
                DiscountSingleItem(((ListBox)sender).SelectedIndex);
            }
            else if (e.KeyCode == Keys.F10)
            {
                DiscountMarkedItems();
            }
            else if (e.KeyCode == Keys.F11)
            {
                DiscountAllItems();
            }
            else if (e.KeyCode == Keys.F12)
            {
                WorkOutCosts();
            }
            else if (e.Shift && e.KeyCode == Keys.Delete)
            {
                RemoveItemFromOrder(((ListBox)sender).SelectedIndex);
            }
        }

        void EditCostQty(int nRow)
        {
            frmSingleInputBox fGetCostQty = new frmSingleInputBox("How many items is the given cost for?", ref sEngine);
            fGetCostQty.ShowDialog();
            if (fGetCostQty.Response != "$NONE")
            {
                try
                {
                    Convert.ToDecimal(fGetCostQty.Response);
                    lbCostQty.Items[nRow] = fGetCostQty.Response;
                }
                catch
                {
                    lbCostQty.Items[nRow] = "";
                }
            }
        }

        void DiscountAllItems()
        {
            decimal dPercent = GetDiscountPercent();
            if (dPercent != 0)
            {
                for (int i = 0; i < lbDesc.Items.Count; i++)
                {
                    lbDiscLine.Items[i] = lbDiscLine.Items[i].ToString() + dPercent.ToString() + "%, ";
                }
            }
        }

        void DiscountSingleItem(int nRowNum)
        {
            decimal dPercent = GetDiscountPercent();
            if (dPercent != 0)
            {
                lbDiscLine.Items[nRowNum] = lbDiscLine.Items[nRowNum].ToString() + dPercent.ToString() + "%, ";
            }
        }

        void DiscountMarkedItems()
        {
            decimal dPercent = GetDiscountPercent();
            if (dPercent != 0)
            {
                for (int i = 0; i < lbDesc.Items.Count; i++)
                {
                    if (lbMarked.Items[i].ToString() == "Y")
                    {
                        lbDiscLine.Items[i] = lbDiscLine.Items[i].ToString() + dPercent.ToString() + "%, ";
                        lbMarked.Items[i] = "";
                    }
                }
            }
        }

        decimal GetDiscountPercent()
        {
            frmSingleInputBox fGetAmount = new frmSingleInputBox("Enter the percentage to discount :", ref sEngine);
            fGetAmount.ShowDialog();

            if (fGetAmount.Response != "$NONE")
            {
                try
                {
                    return Convert.ToDecimal(fGetAmount.Response);
                }
                catch
                {
                    return 0;
                }
            }
            else
                return 0;
        }

        void WorkOutCosts()
        {
            for (int i = 0; i < lbDesc.Items.Count; i++)
            {
                decimal dCost = Convert.ToDecimal(lbCostPrice.Items[i]);
                if (lbCostQty.Items[i].ToString() != "")
                {
                    dCost /= Convert.ToDecimal(lbCostQty.Items[i]);
                }
                string[] sDiscounts = lbDiscLine.Items[i].ToString().Split(',');
                for (int x = 0; x < sDiscounts.Length; x++)
                {
                    sDiscounts[x] = sDiscounts[x].TrimEnd('%');
                    if (sDiscounts[x].TrimEnd(' ') != "")
                    {
                        decimal dPercToDisc = Convert.ToDecimal(sDiscounts[x]);
                        dPercToDisc /= 100;
                        dPercToDisc = 1 - dPercToDisc;
                        dCost *= dPercToDisc;
                    }
                }
                sDiscounts = lbDiscSettlement.Items[i].ToString().Split(',');
                for (int x = 0; x < sDiscounts.Length; x++)
                {
                    sDiscounts[x] = sDiscounts[x].TrimEnd('%');
                    if (sDiscounts[x].TrimEnd(' ') != "")
                    {
                        decimal dPercToDisc = Convert.ToDecimal(sDiscounts[x]);
                        dPercToDisc /= 100;
                        dPercToDisc = 1 - dPercToDisc;
                        dCost *= dPercToDisc;
                    }
                }
                lbCostPrice.Items[i] = FormatMoneyForDisplay(dCost);
                lbDiscLine.Items[i] = "";
                lbDiscSettlement.Items[i] = "";
                lbCostQty.Items[i] = "";
            }
        }

        void CompleteOrder()
        {
            for (int i = 0; i < lbDesc.Items.Count; i++)
            {
                lbQtyReceived.Items[i] = lbQtyOrdered.Items[i];
                lbDiscLine.Items[i] = sDefaultLineDisc;
                lbDiscSettlement.Items[i] = sSettlement;
            }
        }

        void EditItem(int nRow)
        {
            EditItem(nRow, "");
        }
        void EditItem(int nRow, string sStartKey)
        {
            // No row highlighted - prevents a crash
            if (nRow == -1)
                return;

            frmSingleInputBox fGetQty = new frmSingleInputBox("Received Number Of " + sEngine.GetMainStockInfo(sBarcodes[nRow])[1] + " :", ref sEngine);
            if (sStartKey == "")
            {
                if (lbQtyReceived.Items[nRow].ToString() != "0.00")
                    fGetQty.tbResponse.Text = lbQtyReceived.Items[nRow].ToString();
                else
                    fGetQty.tbResponse.Text = lbQtyOrdered.Items[nRow].ToString();
            }
            else
            {
                fGetQty.tbResponse.Text = sStartKey;
                fGetQty.tbResponse.SelectionStart = fGetQty.tbResponse.Text.Length;
            }
            fGetQty.ShowDialog();
            if (fGetQty.Response != "$NONE")
            {
                decimal dQtyReceived = Convert.ToDecimal(fGetQty.Response);
                frmSingleInputBox fGetCost = new frmSingleInputBox("Invoice Cost Price :", ref sEngine);
                fGetCost.tbResponse.Text = FormatMoneyForDisplay(Convert.ToDecimal(lbCostPrice.Items[nRow].ToString()));
                fGetCost.ShowDialog();
                if (fGetCost.Response != "$NONE")
                {
                    decimal dCost = Convert.ToDecimal(fGetCost.Response);
                    frmSingleInputBox fInvQty = new frmSingleInputBox("Enter the number of items that the invoice costs represents :", ref sEngine);
                    fInvQty.tbResponse.Text = "1.00";
                    fInvQty.ShowDialog();
                    if (fInvQty.Response != "$NONE")
                    {
                        frmSingleInputBox fGetLineDisc = new frmSingleInputBox("Enter the line discount % (comma separated for multiple)", ref sEngine);
                        fGetLineDisc.tbResponse.Text = sDefaultLineDisc;
                        fGetLineDisc.ShowDialog();
                        if (fGetLineDisc.Response != "$NONE")
                        {
                            lbDiscLine.Items[nRow] = fGetLineDisc.Response;
                            lbCostQty.Items[nRow] = fInvQty.Response;
                            lbCostPrice.Items[nRow] = FormatMoneyForDisplay(dCost);
                            lbQtyReceived.Items[nRow] = FormatMoneyForDisplay(dQtyReceived);
                            lbDiscSettlement.Items[nRow] = sSettlement;

                            if (Convert.ToDecimal(fGetQty.Response) > Convert.ToDecimal(lbQtyOrdered.Items[nRow]))
                            {
                                if (MessageBox.Show("Quantity received is greater than ordered! Update quantity ordered?", "Quantity Received", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                {
                                    string[] sBCodes = new string[0];
                                    string[] sQtyOrd = new string[0];
                                    string[] sQtyRec = new string[0];
                                    string[] sCostPrice = new string[0];
                                    sEngine.GetOrderData(OrderNumber, ref sBCodes, ref sQtyOrd, ref sQtyRec, ref sCostPrice);
                                    int nNum = -1;
                                    for (int i = 0; i < sBCodes.Length; i++)
                                    {
                                        if (sBCodes[i] == sBarcodes[nRow])
                                            nNum = i;
                                    }
                                    // Crashes on next line?
                                    sEngine.RemoveQuantityOnOrder(sBarcodes[nNum], Convert.ToDecimal(lbQtyOrdered.Items[nNum]), SupplierRecord[6]);
                                    sEngine.AddQuantityOnOrder(sBarcodes[nNum], Convert.ToDecimal(fGetQty.Response), SupplierRecord[6]);
                                    sQtyOrd[nNum] = (Convert.ToDecimal(sQtyRec[nNum]) + Convert.ToDecimal(fGetQty.Response)).ToString();
                                    lbQtyOrdered.Items[nNum] = "0.00";
                                    sEngine.AddEditOrderData(sBCodes, sQtyOrd, sQtyRec, sCostPrice, OrderNumber);
                                }
                                else
                                    lbQtyReceived.Items[nRow] = "0.00";
                            }
                            else if (Convert.ToDecimal(fGetQty.Response) < Convert.ToDecimal(lbQtyOrdered.Items[nRow]))
                            {
                                if (MessageBox.Show("Quantity received is less than ordered. Will you be receiving the remaining items at a later date? (If not, then they will be removed from the order)", "On Back Order?", MessageBoxButtons.YesNo) == DialogResult.No)
                                {
                                    string[] sBCodes = new string[0];
                                    string[] sQtyOrd = new string[0];
                                    string[] sQtyRec = new string[0];
                                    string[] sCostPrice = new string[0];
                                    sEngine.GetOrderData(OrderNumber, ref sBCodes, ref sQtyOrd, ref sQtyRec, ref sCostPrice);
                                    int nNum = -1;
                                    for (int i = 0; i < sBCodes.Length; i++)
                                    {
                                        if (sBCodes[i] == sBarcodes[nRow])
                                            nNum = i;
                                    }
                                    sEngine.RemoveQuantityOnOrder(sBarcodes[nNum], Convert.ToDecimal(lbQtyOrdered.Items[nNum]) - Convert.ToDecimal(fGetQty.Response), SupplierRecord[6]);
                                    
                                    sQtyOrd[nNum] = fGetQty.Response;
                                    lbQtyOrdered.Items[nNum] = "0.00";
                                    sEngine.AddEditOrderData(sBCodes, sQtyOrd, sQtyRec, sCostPrice, OrderNumber);
                                    sEngine.GetOrderData(OrderNumber, ref sBarcodes, ref sQtyOrd, ref sQtyRec, ref sCostPrice);

                                    if (sBarcodes.Length != sBCodes.Length)
                                    {
                                        lbSupCode.Items.RemoveAt(nRow);
                                        lbDesc.Items.RemoveAt(nRow);
                                        lbCostPrice.Items.RemoveAt(nRow);
                                        lbCostQty.Items.RemoveAt(nRow);
                                        lbDiscLine.Items.RemoveAt(nRow);
                                        lbDiscSettlement.Items.RemoveAt(nRow);
                                        lbMarked.Items.RemoveAt(nRow);
                                        lbQtyOrdered.Items.RemoveAt(nRow);
                                        lbQtyReceived.Items.RemoveAt(nRow);
                                        lbRRP.Items.RemoveAt(nRow);
                                        if (sBarcodes.Length > 0)
                                        {
                                            if (nRow < lbSupCode.Items.Count)
                                                lbSupCode.SelectedIndex = nRow - 1;
                                            else
                                                lbSupCode.SelectedIndex = nRow - 2;
                                        }
                                    }
                                    else
                                    {
                                        lbQtyOrdered.Items[nRow] = FormatMoneyForDisplay(fGetQty.Response);
                                    }
                                }
                            }
                        }
                    }
                }

            }
            SumOrderTotal();
        }

        void EditQtyReceived(int nRow)
        {
            frmSingleInputBox fGetQty = new frmSingleInputBox("Received Number Of " + sEngine.GetMainStockInfo(sBarcodes[nRow])[1] + " :", ref sEngine);
            if (lbQtyReceived.Items[nRow].ToString() != "0.00")
                fGetQty.tbResponse.Text = lbQtyReceived.Items[nRow].ToString();
            else
                fGetQty.tbResponse.Text = lbQtyReceived.Items[nRow].ToString();
            fGetQty.ShowDialog();
            if (fGetQty.Response != "$NONE")
            {
                decimal dQtyReceived = Convert.ToDecimal(fGetQty.Response);
                lbQtyReceived.Items[nRow] = FormatMoneyForDisplay(dQtyReceived);
            }
        }

        void EditCostPrice(int nRow)
        {
            frmSingleInputBox fGetCost = new frmSingleInputBox("Cost Price :", ref sEngine);
            fGetCost.tbResponse.Text = lbCostPrice.Items[nRow].ToString();
            fGetCost.ShowDialog();
            if (fGetCost.Response != "$NONE")
            {
                decimal dCost = Convert.ToDecimal(fGetCost.Response);
                lbCostPrice.Items[nRow] = FormatMoneyForDisplay(dCost);
            }
        }

        void lbSelectedChanged(object sender, EventArgs e)
        {
            lbSupCode.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbDesc.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbRRP.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbCostPrice.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbQtyReceived.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbQtyOrdered.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbCostQty.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbMarked.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbDiscLine.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbDiscSettlement.SelectedIndex = ((ListBox)sender).SelectedIndex;
        }

        void AddItemToOrder()
        {
            frmGetBarcode fgb = new frmGetBarcode(ref sEngine);
            fgb.ShowDialog();
            if (fgb.Barcode != null && fgb.Barcode != "")
            {
                frmSingleInputBox fsfi = new frmSingleInputBox("How many were ordered?", ref sEngine);
                decimal dDefaultQty = sEngine.GetSensibleDecimal(sEngine.GetItemStockStaRecord(fgb.Barcode, SupplierRecord[6])[37]); // Get a sensible answer incase default is 6PACK etc.
                if (sEngine.GetItemStockStaRecord(fgb.Barcode, SupplierRecord[6])[37] != "" && dDefaultQty != 0)
                {
                    fsfi.tbResponse.Text = sEngine.GetItemStockStaRecord(fgb.Barcode, SupplierRecord[6])[37];
                }
                else
                {
                    fsfi.tbResponse.Text = "1.00";
                }
                fsfi.ShowDialog();
                if (fsfi.Response != "$NONE")
                {
                    if (sEngine.GetMainStockInfo(fgb.Barcode)[5] == "5")
                    {
                        if (MessageBox.Show("Child items can't be ordered. Would you like to use the parent item instead?", "Use Parent?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            fgb.Barcode = sEngine.GetMainStockInfo(fgb.Barcode)[7];
                        }
                        else
                        {
                            return;
                        }
                    }
                   
                    if (sEngine.GetMainStockInfo(fgb.Barcode)[5] != "1" && sEngine.GetMainStockInfo(fgb.Barcode)[5] != "5")
                    {
                        if (MessageBox.Show("Only type 1 (stock) items can be added to an order. Would you like to make this item (" + fgb.Barcode + " - " + sEngine.GetMainStockInfo(fgb.Barcode)[1] + ") into a stock item?", "Non-Stock Item", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            // Change the item to a type 1
                            sEngine.ChangeItemType(fgb.Barcode, 1);

                            // The item might have  put into a discontinued items category, check if the user wants to change it

                            if (MessageBox.Show("The item is currently in the category \"" + sEngine.GetCategoryDesc(sEngine.GetMainStockInfo(fgb.Barcode)[4]) + "\", would you like to change this to another category?", "Change Category?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                frmCategorySelect fNewCat = new frmCategorySelect(ref sEngine);
                                fNewCat.ShowDialog();
                                if (fNewCat.SelectedItemCategory != "$NULL")
                                {
                                    sEngine.ChangeCategoryOfItem(fgb.Barcode, fNewCat.SelectedItemCategory);
                                }
                            }
                        }
                        else
                        {
                            return;
                        }
                    }

                    if (sEngine.GetSensibleDecimal(fsfi.Response) == 0)
                    {
                        // No point in adding an item with quantity 0!

                        MessageBox.Show("You can't add an item to the order with quantity 0.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }
                    sEngine.AddItemToOrder(OrderNumber, fgb.Barcode, sEngine.GetSensibleDecimal(fsfi.Response).ToString(), "0.00", sEngine.GetItemCostBySupplier(fgb.Barcode, SupplierRecord[1]).ToString());
                    AddRow(fgb.Barcode, sEngine.GetSensibleDecimal(fsfi.Response));
                    lbDesc.SelectedIndex = lbDesc.Items.Count - 1;
                    decimal dCost = sEngine.GetItemCostBySupplier(fgb.Barcode, SupplierRecord[1]);
                    if (dCost == 0)
                        dCost = sEngine.GetItemAverageCost(fgb.Barcode);
                    lbCostPrice.Items[lbDesc.SelectedIndex] = dCost.ToString();
                    lbQtyOrdered.Items[lbDesc.SelectedIndex] = FormatMoneyForDisplay(sEngine.GetSensibleDecimal(fsfi.Response));
                    lbQtyReceived.Items[lbDesc.SelectedIndex] = "0.00";
                    EditItem(lbDesc.SelectedIndex);

                }
            }
        }
        void AddItemToOrder(string sBarcode, decimal dToReceive, bool bDoDiscounts)
        {
            if (sEngine.GetMainStockInfo(sBarcode)[5] == "5")
            {
                if (MessageBox.Show("Child items can't be ordered (Barcode " + sBarcode + "). Would you like to use the parent item instead?", "Use Parent?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    sBarcode = sEngine.GetMainStockInfo(sBarcode)[7];
                }
            }
            if (sEngine.GetMainStockInfo(sBarcode)[5] != "1")
            {
                //MessageBox.Show("Sorry, only type 1 (stock) items can be added to an order! (Barcode " + sBarcode + ")");
                if (MessageBox.Show("Only type 1 (stock) items can be added to an order. Would you like to make this item (" + sBarcode + " - " + sEngine.GetMainStockInfo(sBarcode)[1] + ") into a stock item?", "Non-Stock Item", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    // Change the item to a type 1
                    sEngine.ChangeItemType(sBarcode, 1);

                    // The item might have  put into a discontinued items category, check if the user wants to change it

                    if (MessageBox.Show("The item is currently in the category \"" + sEngine.GetCategoryDesc(sEngine.GetMainStockInfo(sBarcode)[4]) + "\", would you like to change this to another category?", "Change Category?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        frmCategorySelect fNewCat = new frmCategorySelect(ref sEngine);
                        fNewCat.ShowDialog();
                        if (fNewCat.SelectedItemCategory != "$NULL")
                        {
                            sEngine.ChangeCategoryOfItem(sBarcode, fNewCat.SelectedItemCategory);
                        }
                    }
                }
                else
                    return;
            }

            bool bAlreadyInOrder = false;

            for (int i = 0; i < sBarcodes.Length; i++)
            {
                if (sBarcodes[i] == sBarcode)
                {
                    // The barcode already exists
                    if (MessageBox.Show("The item already exists in this order, should I increase the quantity ordered of that item?", "Item Exists", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        lbDesc.SelectedIndex = i;
                        lbQtyOrdered.Items[i] = FormatMoneyForDisplay(Convert.ToDecimal(lbQtyOrdered.Items[i].ToString()) + dToReceive);
                    }
                    bAlreadyInOrder = true;
                }
            }                    

            // If the user has declined to change the item to a type 1, then the code will have returned so below here won't be executed
            sEngine.AddItemToOrder(OrderNumber, sBarcode, FormatMoneyForDisplay(dToReceive), "0.00", sEngine.GetItemCostBySupplier(sBarcode, SupplierRecord[1]).ToString());
            AddRow(sBarcode, dToReceive);
            lbDesc.SelectedIndex = lbDesc.Items.Count - 1;
            decimal dCost = sEngine.GetItemCostBySupplier(sBarcode, SupplierRecord[1]);
            if (dCost == 0)
                dCost = sEngine.GetItemAverageCost(sBarcode);
            lbCostPrice.Items[lbDesc.SelectedIndex] = dCost.ToString();
            lbQtyOrdered.Items[lbDesc.SelectedIndex] = FormatMoneyForDisplay(dToReceive);
            lbQtyReceived.Items[lbDesc.SelectedIndex] = "0.00";
            //EditItem(lbDesc.SelectedIndex);
            if (!bDoDiscounts)
            {
                lbDiscLine.Items[lbDesc.SelectedIndex] = "0";
                lbDiscSettlement.Items[lbDesc.SelectedIndex] = sSettlement;
            }
            else
            {
                lbDiscLine.Items[lbDesc.SelectedIndex] = sDefaultLineDisc;
                lbDiscSettlement.Items[lbDesc.SelectedIndex] = sSettlement;
            }
            lbCostQty.Items[lbDesc.SelectedIndex] = "1.00";
            lbQtyReceived.Items[lbDesc.SelectedIndex] = FormatMoneyForDisplay(dToReceive);

            SumOrderTotal();
        }

        void AddRow(string sBarcode, decimal dQTYToOrder)
        {
            Array.Resize<string>(ref sBarcodes, sBarcodes.Length + 1);
            sBarcodes[sBarcodes.Length - 1] = sBarcode;

            string[] sMainStockInfo = sEngine.GetMainStockInfo(sBarcode);
            string[] sStockStaInfo = sEngine.GetItemStockStaRecord(sBarcode, SupplierRecord[6]);

            lbSupCode.Items.Add(sEngine.GetItemCodeBySupplier(sBarcode, SupplierRecord[1]));
            //lbSupCode.Items.Add("");
            lbDesc.Items.Add(sMainStockInfo[1]);
            lbRRP.Items.Add(FormatMoneyForDisplay(sMainStockInfo[2]));
            lbCostPrice.Items.Add("");
            lbQtyOrdered.Items.Add("");
            lbQtyReceived.Items.Add("");
            lbCostQty.Items.Add("");
            lbMarked.Items.Add("");
            lbDiscLine.Items.Add("");
            lbDiscSettlement.Items.Add("");

            if (lbDesc.Items.Count > 0)
                lbDesc.SelectedIndex = 0;
        }

        void GetNextItemCode()
        {
            frmGetBarcode fGetCode = new frmGetBarcode(ref sEngine);
            fGetCode.ShowDialog();
            if (fGetCode.Barcode != null)
            {
                frmSingleInputBox fGetQty = new frmSingleInputBox("Order Quantity for " + sEngine.GetMainStockInfo(fGetCode.Barcode)[1] + " :", ref sEngine);
                fGetQty.tbResponse.Text = "1.00";
                fGetQty.ShowDialog();
                if (fGetQty.Response != "$NONE")
                {
                    decimal dQtyToOrder = Convert.ToDecimal(fGetQty.Response);
                    AddRow(fGetCode.Barcode, dQtyToOrder);
                    SumOrderTotal();
                }
            }
        }

        void SumOrderTotal()
        {
            dOrderTotal = 0;
            for (int i = 0; i < lbDesc.Items.Count; i++)
            {
                decimal dCost = Convert.ToDecimal(lbCostPrice.Items[i]);
                decimal dQTY = Convert.ToDecimal(lbQtyOrdered.Items[i]);
                dOrderTotal += (dCost * dQTY);
            }
            MessageLabel("ORDTOT").Text = "Order Total : " + FormatMoneyForDisplay(dOrderTotal);
        }

        void CheckAllDiscountsHaveBeenApplied()
        {
            bool bAllDone = true;
            for (int i = 0; i < lbDesc.Items.Count; i++)
            {
                if (lbDiscLine.Items[i].ToString() != "" || lbDiscSettlement.Items[i].ToString() != "")
                    bAllDone = false;
            }
            if (!bAllDone)
            {
                if (MessageBox.Show("Not all discounts have been applied. Apply now?", "Apply Discounts?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    WorkOutCosts();
                }
            }
        }

        void SaveOrder()
        {
            /*
             * Try and catch are for debugging
             * The bug occurs when the user has messed around with the order a bit
             * But the source of the bug is unknown!
             * Hopefully this additional information should help
             */
            try
            {
                CheckAllDiscountsHaveBeenApplied();
                sEngine.AddEditOrderHeader(SupplierRecord);

                string[] sReceived = new string[sBarcodes.Length];
                string[] sCost = new string[sBarcodes.Length];

                for (int i = 0; i < sBarcodes.Length; i++)
                {
                    sReceived[i] = lbQtyReceived.Items[i].ToString();
                    sCost[i] = lbCostPrice.Items[i].ToString();
                }
                sEngine.ReceiveOrder(OrderNumber, sBarcodes, sReceived, sCost);
            }
            catch
            {
                string sBarcodeContents = "";
                foreach (string s in sBarcodes)
                {
                    sBarcodeContents += s + ",";
                }
                frmErrorCatcher.AdditionalErrorInformation = "sBarcodes  = {" + sBarcodeContents + "}";
                frmErrorCatcher.AdditionalErrorInformation += "\nsBarcodes.Length = " + sBarcodes.Length.ToString();
                frmErrorCatcher.AdditionalErrorInformation += "\nlbSupCode.Items.Count = " + lbSupCode.Items.Count.ToString();
                frmErrorCatcher.AdditionalErrorInformation += "\nlbDesc.Items.Count = " + lbDesc.Items.Count.ToString();
                frmErrorCatcher.AdditionalErrorInformation += "\nlbRRP.Items.Count = " + lbRRP.Items.Count.ToString();
                frmErrorCatcher.AdditionalErrorInformation += "\nlbQtyOrdered.Items.Count = " + lbQtyOrdered.Items.Count.ToString();
                frmErrorCatcher.AdditionalErrorInformation += "\nlbCostPrice.Items.Count = " + lbCostPrice.Items.Count.ToString();
                frmErrorCatcher.AdditionalErrorInformation += "\nlbQtyReceived.Items.Count = " + lbQtyReceived.Items.Count.ToString();
                frmErrorCatcher.AdditionalErrorInformation += "\nlbCostQty.Items.Count = " + lbCostQty.Items.Count.ToString();
                frmErrorCatcher.AdditionalErrorInformation += "\nlbMarked.Items.Count = " + lbMarked.Items.Count.ToString();
                frmErrorCatcher.AdditionalErrorInformation += "\nlbDiscLine.Items.Count = " + lbDiscLine.Items.Count.ToString();
                frmErrorCatcher.AdditionalErrorInformation += "\nlbDiscSettlement.Items.Count = " + lbDiscSettlement.Items.Count.ToString();
            }
            if (MessageBox.Show("Upload changes to all tills now?", "Upload", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                sEngine.CopyWaitingFilesToTills();
            }
        }

        void BatchAddAndReceive()
        {
            frmBatchAddItems fbAdd = new frmBatchAddItems(ref sEngine, true);
            fbAdd.Receiving = true;
            fbAdd.ReceivingSupplier = SupplierRecord[1];
            fbAdd.ShowDialog();
            if (fbAdd.bSaved)
            {
                string[] sToAdd = fbAdd.GetListOfBarcodes();
                for (int i = 0; i < sToAdd.Length; i++)
                {
                    AddItemToOrder(sToAdd[i], fbAdd.GetQuantityReceiving(sToAdd[i]), true);
                }
            }
        }

    }
}
