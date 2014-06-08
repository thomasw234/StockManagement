using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Text;
using System.Drawing;

namespace BackOffice
{
    class frmAddOrder : ScalableForm
    {
        class RequisitionItem : IComparable
        {
            public enum SortOrder {Barcode, Desc, Category, RRP, AveSales};
            public SortOrder sOrder = SortOrder.Category;
            public string Barcode;
            public string Desc;
            public string Received;
            public string CategoryCode;
            public decimal RRP;

            public decimal QuantityOrdered;
            public decimal CostPrice;
            public string MinOrder;
            public decimal QIS;
            public decimal YTD;
            public decimal AvePeriod;
            public decimal OnOrder;

            public RequisitionItem(string sBarcode, string sDesc, string sCatCode, decimal dRRP, decimal dAvePeriod)
            {
                Barcode = sBarcode;
                Desc = sDesc;
                CategoryCode = sCatCode;
                RRP = dRRP;
                AvePeriod = dAvePeriod;
            }

            #region IComparable Members

            public int CompareTo(object obj)
            {
                switch (sOrder)
                {
                    case SortOrder.AveSales:
                        return Decimal.Compare(((RequisitionItem)obj).AvePeriod, AvePeriod);
                        break;
                    case SortOrder.Barcode:
                        return String.Compare(Barcode, ((RequisitionItem)obj).Barcode);
                        break;
                    case SortOrder.Category:
                        return String.Compare(CategoryCode, ((RequisitionItem)obj).CategoryCode);
                        break;
                    case SortOrder.Desc:
                        return String.Compare(Desc, ((RequisitionItem)obj).Desc);
                        break;
                    case SortOrder.RRP:
                        return Decimal.Compare(((RequisitionItem)obj).RRP, RRP);
                        break;
                }
                return 0;
            }

            #endregion
        }

        StockEngine sEngine;
        frmOrderSetup fOS;
        string[] SupplierRecord;
        string[] sBarcodes = new string[0];
        string[] sReceived = new string[0];
        decimal[] dOrderAmounts = new decimal[0];
        decimal[] dOrderQtys = new decimal[0];
        decimal dOrderTotal = 0;
        decimal dDaysPeriod = 7;
        bool bCloseQAsked = false;
        public string OrderToLoad = "";
        public bool bCancelled = false;

        CListBox lbBarcode;
        CListBox lbDesc;
        CListBox lbCategory;
        CListBox lbQtyOrdered;
        CListBox lbCostPrice;
        CListBox lbRetailPrice;
        CListBox lbMinOrder;
        CListBox lbOnOrder;
        CListBox lbQIS;
        CListBox lbYTD;
        CListBox lbAvePeriod;

        ContextMenu mRightClickItem = new ContextMenu();

        public frmAddOrder(ref StockEngine se, string OrderNum)
        {
            OrderToLoad = OrderNum;
            sEngine = se;
            fOS = new frmOrderSetup(ref sEngine);
            if (OrderToLoad != "")
                fOS.OrderNumberToCheck = OrderToLoad;
            fOS.OrderExists = true;
            this.SurroundListBoxes = true;
            fOS.ShowDialog();
            if (fOS.OrderHeaderRecord.Length == 0)
            {
                bCancelled = true;
                this.Close();
                return;
            }
            else
                SupplierRecord = fOS.OrderHeaderRecord;

            AddMessage("SUPNAME", "Supplier : " + sEngine.GetSupplierDetails(SupplierRecord[1])[1], new Point(10, 30));
            AddMessage("ORDNUM", "Order Number : " + SupplierRecord[0], new Point(10, BelowLastControl));
            AddMessage("ORDTOT", "Order Total : 0.00", new Point(this.Width - Convert.ToInt32(this.CreateGraphics().MeasureString("Order Total : 999999.99", this.Font).Width), 30));
            AddMessage("ORDDATE", "Order Date : ", new Point(MessageLabel("ORDTOT").Left, BelowLastControl));

            lbBarcode = new CListBox();
            lbBarcode.Location = new Point(10, 100);
            lbBarcode.Size = new Size(125, this.Height - 200);
            lbBarcode.BorderStyle = BorderStyle.None;
            lbBarcode.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
            lbBarcode.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
            lbBarcode.MouseDown += new MouseEventHandler(lb_MouseDown);
            lbBarcode.ContextMenu = mRightClickItem;
            this.Controls.Add(lbBarcode);
            AddMessage("BARCODE", "Barcode", new Point(10, 80));

            lbDesc = new CListBox();
            lbDesc.Location = new Point(lbBarcode.Left + lbBarcode.Width, 100);
            lbDesc.Size = new Size(230, this.Height - 200);
            lbDesc.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
            lbDesc.MouseDown += new MouseEventHandler(lb_MouseDown);
            lbDesc.BorderStyle = BorderStyle.None;
            lbDesc.ContextMenu = mRightClickItem;
            lbDesc.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
            this.Controls.Add(lbDesc);
            AddMessage("DESC", "Description", new Point(lbBarcode.Left + lbBarcode.Width, 80));

            lbCategory = new CListBox();
            lbCategory.Location = new Point(lbDesc.Left + lbDesc.Width, lbDesc.Top);
            lbCategory.Size = new Size(150, this.Height - 200);
            lbCategory.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
            lbCategory.BorderStyle = BorderStyle.None;
            lbCategory.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
            lbCategory.ContextMenu = mRightClickItem;
            lbCategory.MouseDown += new MouseEventHandler(lb_MouseDown);
            this.Controls.Add(lbCategory);
            AddMessage("CAT", "Category", new Point(lbCategory.Left, 80));

            lbQIS = new CListBox();
            lbQIS.Location = new Point(lbCategory.Left + lbCategory.Width, lbCategory.Top);
            lbQIS.Size = new Size(40, this.Height - 200);
            lbQIS.BorderStyle = BorderStyle.None;
            lbQIS.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
            lbQIS.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
            lbQIS.ContextMenu = mRightClickItem;
            lbQIS.MouseDown += new MouseEventHandler(lb_MouseDown);
            lbQIS.RightToLeft = RightToLeft.Yes;
            this.Controls.Add(lbQIS);
            AddMessage("QIS", "QIS", new Point(lbQIS.Left, 80));
            MessageLabel("QIS").AutoSize = false;
            MessageLabel("QIS").Width = lbQIS.Width;
            MessageLabel("QIS").TextAlign = ContentAlignment.MiddleRight;

            lbQtyOrdered = new CListBox();
            lbQtyOrdered.Location = new Point(lbQIS.Left + lbQIS.Width, lbQIS.Top);
            lbQtyOrdered.Size = new Size(60, this.Height - 200);
            lbQtyOrdered.BorderStyle = BorderStyle.None;
            lbQtyOrdered.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
            lbQtyOrdered.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
            lbQtyOrdered.ContextMenu = mRightClickItem;
            lbQtyOrdered.RightToLeft = RightToLeft.Yes;
            lbQtyOrdered.MouseDown += new MouseEventHandler(lb_MouseDown);
            this.Controls.Add(lbQtyOrdered);
            AddMessage("QTY", "Qty", new Point(lbQtyOrdered.Left, 80));

            lbCostPrice = new CListBox();
            lbCostPrice.Location = new Point(lbQtyOrdered.Left + lbQtyOrdered.Width, lbQtyOrdered.Top);
            lbCostPrice.Size = new Size(70, this.Height - 200);
            lbCostPrice.BorderStyle = BorderStyle.None;
            lbCostPrice.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
            lbCostPrice.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
            lbCostPrice.ContextMenu = mRightClickItem;
            lbCostPrice.RightToLeft = RightToLeft.Yes;
            lbCostPrice.MouseDown += new MouseEventHandler(lb_MouseDown);
            this.Controls.Add(lbCostPrice);
            AddMessage("COST", "Cost", new Point(lbCostPrice.Left, 80));

            lbRetailPrice = new CListBox();
            lbRetailPrice.Location = new Point(lbCostPrice.Left + lbCostPrice.Width, lbCostPrice.Top);
            lbRetailPrice.Size = new Size(70, this.Height - 200);
            lbRetailPrice.BorderStyle = BorderStyle.None;
            lbRetailPrice.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
            lbRetailPrice.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
            lbRetailPrice.ContextMenu = mRightClickItem;
            lbRetailPrice.MouseDown += new MouseEventHandler(lb_MouseDown);
            lbRetailPrice.RightToLeft = RightToLeft.Yes;
            this.Controls.Add(lbRetailPrice);
            AddMessage("RRP", "RRP", new Point(lbRetailPrice.Left, 80));

            lbMinOrder = new CListBox();
            lbMinOrder.Location = new Point(lbRetailPrice.Left + lbRetailPrice.Width, lbRetailPrice.Top);
            lbMinOrder.Size = new Size(60, this.Height - 200);
            lbMinOrder.BorderStyle = BorderStyle.None;
            lbMinOrder.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
            lbMinOrder.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
            lbMinOrder.MouseDown += new MouseEventHandler(lb_MouseDown);
            lbMinOrder.ContextMenu = mRightClickItem;
            lbMinOrder.RightToLeft = RightToLeft.Yes;
            this.Controls.Add(lbMinOrder);
            AddMessage("MINORD", "Min Order", new Point(lbMinOrder.Left, 80));

            lbOnOrder = new CListBox();
            lbOnOrder.Location = new Point(lbMinOrder.Left + lbMinOrder.Width, lbMinOrder.Top);
            lbOnOrder.Size = new Size(40, this.Height - 200);
            lbOnOrder.BorderStyle = BorderStyle.None;
            lbOnOrder.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
            lbOnOrder.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
            lbOnOrder.ContextMenu = mRightClickItem;
            lbOnOrder.MouseDown += new MouseEventHandler(lb_MouseDown);
            lbOnOrder.RightToLeft = RightToLeft.Yes;
            this.Controls.Add(lbOnOrder);
            AddMessage("ON", "On", new Point(lbOnOrder.Left, 60));
            AddMessage("ORDER", "Order", new Point(lbOnOrder.Left, 80));

            lbYTD = new CListBox();
            lbYTD.Location = new Point(lbOnOrder.Left + lbOnOrder.Width, lbOnOrder.Top);
            lbYTD.Size = new Size(75, this.Height - 200);
            lbYTD.BorderStyle = BorderStyle.None;
            lbYTD.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
            lbYTD.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
            lbYTD.ContextMenu = mRightClickItem;
            lbYTD.RightToLeft = RightToLeft.Yes;
            lbYTD.MouseDown += new MouseEventHandler(lb_MouseDown);
            this.Controls.Add(lbYTD);
            AddMessage("YTD", "Y.T.D Sales", new Point(lbYTD.Left, 80));

            lbAvePeriod = new CListBox();
            lbAvePeriod.Location = new Point(lbYTD.Left + lbYTD.Width, lbYTD.Top);
            lbAvePeriod.Size = new Size(75, this.Height - 200);
            lbAvePeriod.BorderStyle = BorderStyle.None;
            lbAvePeriod.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
            lbAvePeriod.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
            lbAvePeriod.ContextMenu = mRightClickItem;
            lbAvePeriod.RightToLeft = RightToLeft.Yes;
            lbAvePeriod.MouseDown += new MouseEventHandler(lb_MouseDown);
            this.Controls.Add(lbAvePeriod);
            AddMessage("AVEPSALES", "Ave/7 Days", new Point(lbAvePeriod.Left, 80));


            AddMessage("INST", "Enter to add a row, Space bar to edit the order quantity, Esc to save, or F4 to edit the current item.", new Point(10, lbDesc.Top + lbDesc.Height + 10));
            AddMessage("INST2", "Shift + Del to remove an item from the order, Shift + D to discontinue item, F7 to view item details, F8 to view related orders.", new Point(10, BelowLastControl));
            AddMessage("INST3", "Shift + P to print the order so far", new Point(10, BelowLastControl));
            if (fOS.OrderExists && sEngine.DoesOrderExist(SupplierRecord[0])) 
            {
                string[] sBarcodes2 = new string[0];
                string[] sQuantities = new string[0];
                string[] sCost = new string[0];

                sEngine.GetOrderData(SupplierRecord[0], ref sBarcodes2, ref sQuantities, ref sReceived, ref sCost);

                for (int i = 0; i < sBarcodes2.Length; i++)
                {
                    // Tries to add rows. False returned if not managed, but nothing to do
                    AddRow(sBarcodes2[i], Convert.ToDecimal(sQuantities[i]));
                    lbCostPrice.Items[i] = FormatMoneyForDisplay(sCost[i]);
                }
                ClearUpDeletedRows();
                string sDate = SupplierRecord[5][0].ToString() + SupplierRecord[5][1].ToString() + "/" + SupplierRecord[5][2].ToString() + SupplierRecord[5][3].ToString() + "/" + SupplierRecord[5][4].ToString() + SupplierRecord[5][5].ToString();
                MessageLabel("ORDDATE").Text = "Order Date : " + sDate;
                //OrderItems();
            }
            else if (fOS.bRequisitionOrder)
            {
                frmRequisitionSettings frs = new frmRequisitionSettings(ref sEngine);
                frs.ShowDialog();
                if (frs.bOK)
                {
                    string[] sToAdd;
                    if (frs.sCategory == "")
                        sToAdd = sEngine.GetBarcodesOfItemsBySpec(SupplierRecord[1], frs.dAveSalesMin, frs.dNumberOfDays, SupplierRecord[6]);
                    else
                        sToAdd = sEngine.GetBarcodesOfItemsBySpec(frs.dAveSalesMin, frs.dNumberOfDays, SupplierRecord[6], frs.sCategory);
                    frmProgressBar fp = new frmProgressBar("Loading Order");
                    fp.pb.Maximum = sToAdd.Length;
                    fp.Show();
                    dDaysPeriod = frs.dNumberOfDays;
                    MessageLabel("AVEPSALES").Text = "Ave/" + dDaysPeriod.ToString() + " Days";
                    for (int i = 0; i < sToAdd.Length; i++)
                    {
                        AddRow(sToAdd[i], 0);
                        fp.pb.Value = i;
                    }
                    ClearUpDeletedRows();
                    fp.Close();
                    OrderItems();
                    SumOrderTotal();
                }
                if (lbDesc.Items.Count > 0)
                {
                    lbDesc.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("No items matching that criteria were found!");
                }
            }
            
            this.VisibleChanged += new EventHandler(frmAddOrder_VisibleChanged);
            this.WindowState = FormWindowState.Maximized;

            MenuItem[] mItems = new MenuItem[6];
            mItems[0] = new MenuItem("Add An Item To The Order");
            mItems[1] = new MenuItem("Edit Item Order Quantity");
            mItems[2] = new MenuItem("Edit Item's Information");
            mItems[3] = new MenuItem("Detailed Item Information");
            mItems[4] = new MenuItem("Delete Item From Order");
            mItems[5] = new MenuItem("Discontinue Item");

            for (int i = 0; i < mItems.Length; i++)
            {
                mRightClickItem.MenuItems.Add(mItems[i]);
            }

            mItems[0].Click +=new EventHandler(AddItemMenuClick);
            mItems[1].Click += new EventHandler(EditItemQtyClick);
            mItems[2].Click += new EventHandler(EditItemDetailsClick);
            mItems[3].Click += new EventHandler(DetailedItemEnqClick);
            mItems[4].Click += new EventHandler(DeleteRowClick);
            mItems[5].Click += new EventHandler(DiscontinueItemClick);

            if (sEngine.AnySuggestedItemsForSupplier(SupplierRecord[1], SupplierRecord[6]))
            {
                frmOrderSuggestions frmOS = new frmOrderSuggestions(ref sEngine, SupplierRecord[6], SupplierRecord[1]);
                for (int i = 0; i < lbBarcode.Items.Count; i++)
                {
                    frmOS.RemoveSuggestion(lbBarcode.Items[i].ToString());
                }
                frmOS.ShowDialog();
                if (frmOS.BarcodesToInclude != null && frmOS.BarcodesToInclude.Length > 0)
                {
                    for (int i = 0; i < frmOS.BarcodesToInclude.Length; i++)
                    {
                        AddRow(frmOS.BarcodesToInclude[i], 0);
                    }
                    ClearUpDeletedRows();
                }
            }
            SumOrderTotal();
        }

        RequisitionItem.SortOrder sOrder = RequisitionItem.SortOrder.Category;

        void CycleSortOrder()
        {
            switch (sOrder)
            {
                case RequisitionItem.SortOrder.AveSales:
                    MessageLabel("AVEPSALES").BackColor = this.BackColor;
                    MessageLabel("BARCODE").BackColor = Color.DarkGray;
                    sOrder = RequisitionItem.SortOrder.Barcode;
                    break;
                case RequisitionItem.SortOrder.Barcode:
                    MessageLabel("BARCODE").BackColor = this.BackColor;
                    MessageLabel("CAT").BackColor = Color.DarkGray;
                    sOrder = RequisitionItem.SortOrder.Category;
                    break;
                case RequisitionItem.SortOrder.Category:
                    MessageLabel("CAT").BackColor = this.BackColor;
                    MessageLabel("DESC").BackColor = Color.DarkGray;
                    sOrder = RequisitionItem.SortOrder.Desc;
                    break;
                case RequisitionItem.SortOrder.Desc:
                    MessageLabel("DESC").BackColor = this.BackColor;
                    MessageLabel("RRP").BackColor = Color.DarkGray;
                    sOrder = RequisitionItem.SortOrder.RRP;
                    break;
                case RequisitionItem.SortOrder.RRP:
                    MessageLabel("RRP").BackColor = this.BackColor;
                    MessageLabel("AVEPSALES").BackColor = Color.DarkGray;
                    sOrder = RequisitionItem.SortOrder.AveSales;
                    break;
            }
        }

        void OrderItems()
        {
            RequisitionItem[] rItems = new RequisitionItem[lbBarcode.Items.Count];
            for (int i = 0; i < rItems.Length; i++)
            {
                rItems[i] = new RequisitionItem(lbBarcode.Items[i].ToString(), lbDesc.Items[i].ToString(), sEngine.GetMainStockInfo(lbBarcode.Items[i].ToString())[4], Convert.ToDecimal(lbRetailPrice.Items[i].ToString()), Convert.ToDecimal(lbAvePeriod.Items[i].ToString()));
                rItems[i].CostPrice = Convert.ToDecimal(lbCostPrice.Items[i]);
                rItems[i].MinOrder = lbMinOrder.Items[i].ToString();
                rItems[i].QIS = Convert.ToDecimal(lbQIS.Items[i]);
                rItems[i].QuantityOrdered = Convert.ToDecimal(lbQtyOrdered.Items[i]);
                rItems[i].sOrder = sOrder;
                rItems[i].YTD = Convert.ToDecimal(lbYTD.Items[i]);
                rItems[i].OnOrder = Convert.ToDecimal(lbOnOrder.Items[i]);
                rItems[i].Received = sReceived[i];
            }
            Array.Sort(rItems);
            lbAvePeriod.Items.Clear();
            lbBarcode.Items.Clear();
            lbCategory.Items.Clear();
            lbCostPrice.Items.Clear();
            lbDesc.Items.Clear();
            lbMinOrder.Items.Clear();
            lbOnOrder.Items.Clear();
            lbQIS.Items.Clear();
            lbQtyOrdered.Items.Clear();
            lbRetailPrice.Items.Clear();
            lbYTD.Items.Clear();
            sBarcodes = new string[rItems.Length];
            for (int i = 0; i < rItems.Length; i++)
            {
                sBarcodes[i] = rItems[i].Barcode;
                sReceived[i] = rItems[i].Received;
                dOrderQtys[i] = rItems[i].QuantityOrdered;
                dOrderAmounts[i] = rItems[i].CostPrice;
                lbAvePeriod.Items.Add(FormatMoneyForDisplay(rItems[i].AvePeriod));
                lbBarcode.Items.Add(rItems[i].Barcode);
                lbCategory.Items.Add(sEngine.GetCategoryDesc(rItems[i].CategoryCode));
                lbCostPrice.Items.Add(FormatMoneyForDisplay(rItems[i].CostPrice));
                lbDesc.Items.Add(rItems[i].Desc);
                lbMinOrder.Items.Add(rItems[i].MinOrder);
                lbOnOrder.Items.Add(FormatMoneyForDisplay(rItems[i].OnOrder));
                lbQIS.Items.Add(FormatMoneyForDisplay(FormatMoneyForDisplay(rItems[i].QIS)));
                lbQtyOrdered.Items.Add(FormatMoneyForDisplay(rItems[i].QuantityOrdered));
                lbRetailPrice.Items.Add(FormatMoneyForDisplay(rItems[i].RRP));
                lbYTD.Items.Add(FormatMoneyForDisplay(rItems[i].YTD));
            }
        }

        public frmAddOrder(ref StockEngine se)
        {
            sEngine = se;
            fOS = new frmOrderSetup(ref sEngine);
            if (OrderToLoad != "")
                fOS.OrderNumberToCheck = OrderToLoad;
            fOS.OrderExists = true;
            this.SurroundListBoxes = true;
            fOS.ShowDialog();
            if (fOS.OrderHeaderRecord.Length == 0)
            {
                bCancelled = true;
                this.Close();
                return;
            }
            else
                SupplierRecord = fOS.OrderHeaderRecord;

            AddMessage("SUPNAME", "Supplier : " + sEngine.GetSupplierDetails(SupplierRecord[1])[1], new Point(10, 30));
            AddMessage("ORDNUM", "Order Number : " + SupplierRecord[0], new Point(10, BelowLastControl));
            AddMessage("ORDTOT", "Order Total : 0.00", new Point(this.Width - Convert.ToInt32(this.CreateGraphics().MeasureString("Order Total : 999999.99", this.Font).Width), 30));
            AddMessage("ORDDATE", "Order Date : ", new Point(MessageLabel("ORDTOT").Left, BelowLastControl));

            lbBarcode = new CListBox();
            lbBarcode.Location = new Point(10, 100);
            lbBarcode.Size = new Size(125, this.Height - 200);
            lbBarcode.BorderStyle = BorderStyle.None;
            lbBarcode.SelectedIndexChanged +=new EventHandler(lbSelectedChanged);
            lbBarcode.KeyDown +=new KeyEventHandler(lbDesc_KeyDown);
            lbBarcode.MouseDown += new MouseEventHandler(lb_MouseDown);
            lbBarcode.ContextMenu = mRightClickItem;
            this.Controls.Add(lbBarcode);
            AddMessage("BARCODE", "Barcode", new Point(10, 80));

            lbDesc = new CListBox();
            lbDesc.Location = new Point(lbBarcode.Left + lbBarcode.Width, 100);
            lbDesc.Size = new Size(230, this.Height - 200);
            lbDesc.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
            lbDesc.MouseDown +=new MouseEventHandler(lb_MouseDown);
            lbDesc.BorderStyle = BorderStyle.None;
            lbDesc.ContextMenu = mRightClickItem;
            lbDesc.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
            this.Controls.Add(lbDesc);
            AddMessage("DESC", "Description", new Point(lbBarcode.Left +lbBarcode.Width,80));

            lbCategory = new CListBox();
            lbCategory.Location = new Point(lbDesc.Left + lbDesc.Width, lbDesc.Top);
            lbCategory.Size = new Size(150, this.Height - 200);
            lbCategory.SelectedIndexChanged +=new EventHandler(lbSelectedChanged);
            lbCategory.BorderStyle = BorderStyle.None;
            lbCategory.KeyDown +=new KeyEventHandler(lbDesc_KeyDown);
            lbCategory.ContextMenu = mRightClickItem;
            lbCategory.MouseDown +=new MouseEventHandler(lb_MouseDown);
            this.Controls.Add(lbCategory);
            AddMessage("CAT", "Category", new Point(lbCategory.Left, 80));

            lbQIS = new CListBox();
            lbQIS.Location = new Point(lbCategory.Left + lbCategory.Width, lbCategory.Top);
            lbQIS.Size = new Size(40, this.Height - 200);
            lbQIS.BorderStyle = BorderStyle.None;
            lbQIS.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
            lbQIS.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
            lbQIS.ContextMenu = mRightClickItem;
            lbQIS.MouseDown += new MouseEventHandler(lb_MouseDown);
            lbQIS.RightToLeft = RightToLeft.Yes;
            this.Controls.Add(lbQIS);
            AddMessage("QIS", "QIS", new Point(lbQIS.Left, 80));

            lbQtyOrdered = new CListBox();
            lbQtyOrdered.Location = new Point(lbQIS.Left + lbQIS.Width, lbQIS.Top);
            lbQtyOrdered.Size = new Size(60, this.Height - 200);
            lbQtyOrdered.BorderStyle = BorderStyle.None;
            lbQtyOrdered.SelectedIndexChanged +=new EventHandler(lbSelectedChanged);
            lbQtyOrdered.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
            lbQtyOrdered.ContextMenu = mRightClickItem;
            lbQtyOrdered.RightToLeft = RightToLeft.Yes;
            lbQtyOrdered.MouseDown += new MouseEventHandler(lb_MouseDown);
            this.Controls.Add(lbQtyOrdered);
            AddMessage("QTY", "Qty", new Point(lbQtyOrdered.Left, 80));

            lbCostPrice = new CListBox();
            lbCostPrice.Location = new Point(lbQtyOrdered.Left + lbQtyOrdered.Width, lbQtyOrdered.Top);
            lbCostPrice.Size = new Size(70, this.Height - 200);
            lbCostPrice.BorderStyle = BorderStyle.None;
            lbCostPrice.SelectedIndexChanged +=new EventHandler(lbSelectedChanged);
            lbCostPrice.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
            lbCostPrice.ContextMenu = mRightClickItem;
            lbCostPrice.RightToLeft = RightToLeft.Yes;
            lbCostPrice.MouseDown += new MouseEventHandler(lb_MouseDown);
            this.Controls.Add(lbCostPrice);
            AddMessage("COST", "Cost", new Point(lbCostPrice.Left, 80));

            lbRetailPrice = new CListBox();
            lbRetailPrice.Location = new Point(lbCostPrice.Left + lbCostPrice.Width, lbCostPrice.Top);
            lbRetailPrice.Size = new Size(70, this.Height - 200);
            lbRetailPrice.BorderStyle = BorderStyle.None;
            lbRetailPrice.SelectedIndexChanged +=new EventHandler(lbSelectedChanged);
            lbRetailPrice.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
            lbRetailPrice.ContextMenu = mRightClickItem;
            lbRetailPrice.MouseDown += new MouseEventHandler(lb_MouseDown);
            lbRetailPrice.RightToLeft = RightToLeft.Yes;
            this.Controls.Add(lbRetailPrice);
            AddMessage("RRP", "RRP", new Point(lbRetailPrice.Left, 80));

            lbMinOrder = new CListBox();
            lbMinOrder.Location = new Point(lbRetailPrice.Left + lbRetailPrice.Width, lbRetailPrice.Top);
            lbMinOrder.Size = new Size(60, this.Height - 200);
            lbMinOrder.BorderStyle = BorderStyle.None;
            lbMinOrder.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
            lbMinOrder.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
            lbMinOrder.MouseDown += new MouseEventHandler(lb_MouseDown);
            lbMinOrder.ContextMenu = mRightClickItem;
            lbMinOrder.RightToLeft = RightToLeft.Yes;
            this.Controls.Add(lbMinOrder);
            AddMessage("MINORD", "Min Order", new Point(lbMinOrder.Left, 80));

            lbOnOrder = new CListBox();
            lbOnOrder.Location = new Point(lbMinOrder.Left + lbMinOrder.Width, lbMinOrder.Top);
            lbOnOrder.Size = new Size(40, this.Height - 200);
            lbOnOrder.BorderStyle = BorderStyle.None;
            lbOnOrder.SelectedIndexChanged +=new EventHandler(lbSelectedChanged);
            lbOnOrder.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
            lbOnOrder.ContextMenu = mRightClickItem;
            lbOnOrder.MouseDown += new MouseEventHandler(lb_MouseDown);
            lbOnOrder.RightToLeft = RightToLeft.Yes;
            this.Controls.Add(lbOnOrder);
            AddMessage("ON", "On", new Point(lbOnOrder.Left, 60));
            AddMessage("ORDER", "Order", new Point(lbOnOrder.Left, 80));

            lbYTD = new CListBox();
            lbYTD.Location = new Point(lbOnOrder.Left + lbOnOrder.Width, lbOnOrder.Top);
            lbYTD.Size = new Size(75, this.Height - 200);
            lbYTD.BorderStyle = BorderStyle.None;
            lbYTD.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
            lbYTD.KeyDown += new KeyEventHandler(lbDesc_KeyDown);
            lbYTD.ContextMenu = mRightClickItem;
            lbYTD.RightToLeft = RightToLeft.Yes;
            lbYTD.MouseDown += new MouseEventHandler(lb_MouseDown);
            this.Controls.Add(lbYTD);
            AddMessage("YTD", "Y.T.D Sales", new Point(lbYTD.Left, 80));

            lbAvePeriod = new CListBox();
            lbAvePeriod.Location = new Point(lbYTD.Left + lbYTD.Width, lbYTD.Top);
            lbAvePeriod.Size = new Size(this.ClientSize.Width - 10 - lbAvePeriod.Left, this.Height - 200);
            lbAvePeriod.BorderStyle = BorderStyle.None;
            lbAvePeriod.SelectedIndexChanged +=new EventHandler(lbSelectedChanged);
            lbAvePeriod.KeyDown +=new KeyEventHandler(lbDesc_KeyDown);
            lbAvePeriod.ContextMenu = mRightClickItem;
            lbAvePeriod.RightToLeft = RightToLeft.Yes;
            lbAvePeriod.MouseDown += new MouseEventHandler(lb_MouseDown);
            this.Controls.Add(lbAvePeriod);
            AddMessage("AVEPSALES", "Ave/7 Days", new Point(lbAvePeriod.Left, 80));

            AddMessage("INST", "Insert to add a row, Enter bar to edit the order quantity, Esc to save, or F4 to edit the select item's details.", new Point(10, lbDesc.Top + lbDesc.Height + 10));
            AddMessage("INST2", "Shift + Del to remove an item from the order, Shift + D to discontinue item, F6 to view stock levels, F7 to view item details, F8 to view orders with this item outstanding.", new Point(10, BelowLastControl));
            if (fOS.OrderExists && sEngine.DoesOrderExist(SupplierRecord[0]))
            {
                string[] sBarcodes2 = new string[0];
                string[] sQuantities = new string[0];
                string[] sCost = new string[0];

                sEngine.GetOrderData(SupplierRecord[0], ref sBarcodes2, ref sQuantities, ref sReceived, ref sCost);

                for (int i = 0; i < sBarcodes2.Length; i++)
                {
                    AddRow(sBarcodes2[i], Convert.ToDecimal(sQuantities[i]));}

                // Now format for money
                for (int i = 0; i < lbCostPrice.Items.Count; i++)
                {
                    lbCostPrice.Items[i] = FormatMoneyForDisplay(lbCostPrice.Items[i].ToString());
                }

                ClearUpDeletedRows();
                string[] OrderHeader = sEngine.GetOrderHeader(SupplierRecord[0]);
                string sDate = OrderHeader[5][0].ToString() + OrderHeader[5][1].ToString() + "/" + OrderHeader[5][2].ToString() + OrderHeader[5][3].ToString() + "/" + OrderHeader[5][4].ToString() + OrderHeader[5][5].ToString();
                MessageLabel("ORDDATE").Text = "Order Date : " + sDate;
                SumOrderTotal();
            }
            else if (fOS.bRequisitionOrder)
            {
                frmRequisitionSettings frs = new frmRequisitionSettings(ref sEngine);
                frs.ShowDialog();
                if (frs.bOK)
                {
                    string[] sToAdd;
                    if (frs.sCategory == "")
                        sToAdd = sEngine.GetBarcodesOfItemsBySpec(SupplierRecord[1], frs.dAveSalesMin, frs.dNumberOfDays, SupplierRecord[6]);
                    else
                        sToAdd = sEngine.GetBarcodesOfItemsBySpec(frs.dAveSalesMin, frs.dNumberOfDays, SupplierRecord[6], frs.sCategory);
                    frmProgressBar fp = new frmProgressBar("Loading Order");
                    fp.pb.Maximum = sToAdd.Length;
                    fp.Show();
                    dDaysPeriod = frs.dNumberOfDays;
                    MessageLabel("AVEPSALES").Text = "Ave/" + dDaysPeriod.ToString() + " Days";
                    for (int i = 0; i < sToAdd.Length; i++)
                    {
                        AddRow(sToAdd[i], 0);
                        fp.pb.Value = i;
                    }
                    ClearUpDeletedRows();
                    fp.Close();
                    //OrderItems();
                    SumOrderTotal();
                }
                if (lbDesc.Items.Count > 0)
                {
                    lbDesc.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("No items matching that criteria were found!");
                }
            }
            this.VisibleChanged += new EventHandler(frmAddOrder_VisibleChanged);
            this.WindowState = FormWindowState.Maximized;

            MenuItem[] mItems = new MenuItem[6];
            mItems[0] = new MenuItem("Add An Item To The Order");
            mItems[1] = new MenuItem("Edit Item Order Quantity");
            mItems[2] = new MenuItem("Edit Item's Information");
            mItems[3] = new MenuItem("Detailed Item Information");
            mItems[4] = new MenuItem("Delete Item From Order");
            mItems[5] = new MenuItem("Discontinue Item");

            for (int i = 0; i < mItems.Length; i++)
            {
                mRightClickItem.MenuItems.Add(mItems[i]);
            }

            mItems[0].Click +=new EventHandler(AddItemMenuClick);
            mItems[1].Click += new EventHandler(EditItemQtyClick);
            mItems[2].Click += new EventHandler(EditItemDetailsClick);
            mItems[3].Click += new EventHandler(DetailedItemEnqClick);
            mItems[4].Click += new EventHandler(DeleteRowClick);
            mItems[5].Click += new EventHandler(DiscontinueItemClick);

            if (sEngine.AnySuggestedItemsForSupplier(SupplierRecord[1], SupplierRecord[6]))
            {
                frmOrderSuggestions frmOS = new frmOrderSuggestions(ref sEngine, SupplierRecord[6], SupplierRecord[1]);
                for (int i = 0; i < lbBarcode.Items.Count; i++)
                {
                    frmOS.RemoveSuggestion(lbBarcode.Items[i].ToString());
                }
                frmOS.ShowDialog();
                if (frmOS.BarcodesToInclude != null && frmOS.BarcodesToInclude.Length > 0)
                {
                    for (int i = 0; i < frmOS.BarcodesToInclude.Length; i++)
                    {
                        AddRow(frmOS.BarcodesToInclude[i], 0);
                    }
                    ClearUpDeletedRows();
                }
                this.WindowState = FormWindowState.Maximized;
            }
            this.Text = "Add / Edit Order";
            if (OrderToLoad == "")
            {
                OrderToLoad = SupplierRecord[0];
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!bCloseQAsked)
            {
                switch (MessageBox.Show("Save this order?", "Save", MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.Yes:
                        SaveOrder(true);
                        base.OnClosing(e);
                        break;
                    case DialogResult.No:
                        base.OnClosing(e);
                        break;
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
        }

        private void ClearUpDeletedRows()
        {
            for (int i = 0; i < lbBarcode.Items.Count; i++)
            {
                if (lbBarcode.Items[i].ToString().StartsWith("$:"))
                {
                    // Delete the item as it is due to be deleted
                    lbBarcode.Items[i] = lbBarcode.Items[i].ToString().Remove(0, 2);
                    DeleteRow(i);
                    i--;
                }
            }
        }

        void lb_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ((ListBox)sender).SelectedIndex = ((ListBox)sender).IndexFromPoint(new Point(e.X, e.Y));
            }
        }

        void DiscontinueItemClick(object sender, EventArgs e)
        {
            if (lbBarcode.SelectedIndex != -1)
            {
                sEngine.DiscontinueItem(sBarcodes[lbBarcode.SelectedIndex]);
                DeleteRow(lbBarcode.SelectedIndex);
            }
        }

        void DeleteRowClick(object sender, EventArgs e)
        {
            if (lbBarcode.SelectedIndex != -1)
            {
                DeleteRow(lbBarcode.SelectedIndex);
            }
        }

        void DetailedItemEnqClick(object sender, EventArgs e)
        {
            if (lbBarcode.SelectedIndex != -1)
            {
                frmDetailedItemEnquiry fdie = new frmDetailedItemEnquiry(ref sEngine, sBarcodes[lbBarcode.SelectedIndex]);
                fdie.ForAnOrder = true;
                fdie.ShowDialog();
                if (fdie.ItemAdded)
                {
                    AddRow(fdie.sBarcode, 0);
                    EditItem(lbBarcode.Items.Count - 1);
                }
                ClearUpDeletedRows();
            }
        }

        void EditItemDetailsClick(object sender, EventArgs e)
        {
            if (lbBarcode.SelectedIndex != -1)
            {
                frmAddEditItem faei = new frmAddEditItem(ref sEngine);
                faei.EditingBarcode = sBarcodes[lbBarcode.SelectedIndex];
                faei.ShowDialog();
            }
        }

        void EditItemQtyClick(object sender, EventArgs e)
        {
            if (lbBarcode.SelectedIndex != -1)
            {
                EditItem(lbBarcode.SelectedIndex);
            }
        }

        void AddItemMenuClick(object sender, EventArgs e)
        {
            GetNextItemCode();
        }

        bool bShownDialog = false;
        void frmAddOrder_VisibleChanged(object sender, EventArgs e)
        {
            frmAddOrder_ResizeEnd(sender, e);
            
            if (sBarcodes.Length == 0 & !bShownDialog)
            {
                this.Refresh();
                bShownDialog = true;
                GetNextItemCode();
            }
        }

        void frmAddOrder_ResizeEnd(object sender, EventArgs e)
        {
            this.recalculateSizes();
            
            MessageLabel("QIS").AutoSize = false;
            MessageLabel("QIS").Width = lbQIS.Width;
            MessageLabel("QIS").TextAlign = ContentAlignment.MiddleRight;
            MessageLabel("QTY").AutoSize = false;
            MessageLabel("QTY").Width = lbQtyOrdered.Width;
            MessageLabel("QTY").TextAlign = ContentAlignment.MiddleRight;
            MessageLabel("COST").AutoSize = false;
            MessageLabel("COST").Width = lbCostPrice.Width;
            MessageLabel("COST").TextAlign = ContentAlignment.MiddleRight;
            MessageLabel("MINORD").AutoSize = false;
            MessageLabel("MINORD").Width = lbMinOrder.Width;
            MessageLabel("MINORD").TextAlign = ContentAlignment.MiddleRight;
            MessageLabel("ON").AutoSize = false;
            MessageLabel("ON").Width = lbOnOrder.Width;
            MessageLabel("ON").TextAlign = ContentAlignment.MiddleRight;
            MessageLabel("ORDER").AutoSize = false;
            MessageLabel("ORDER").Width = lbOnOrder.Width;
            MessageLabel("ORDER").TextAlign = ContentAlignment.MiddleRight;
            MessageLabel("YTD").AutoSize = false;
            MessageLabel("YTD").Width = lbYTD.Width;
            MessageLabel("YTD").TextAlign = ContentAlignment.MiddleRight;
            MessageLabel("AVEPSALES").AutoSize = false;
            MessageLabel("AVEPSALES").Width = lbAvePeriod.Width;
            MessageLabel("AVEPSALES").TextAlign = ContentAlignment.MiddleRight;
            MessageLabel("RRP").AutoSize = false;
            MessageLabel("RRP").Width = lbRetailPrice.Width;
            MessageLabel("RRP").TextAlign = ContentAlignment.MiddleRight;
        }

        private void recalculateSizes()
        {
            MessageLabel("INST2").Top = this.ClientSize.Height - 10 - MessageLabel("INST2").Height;
            MessageLabel("INST").Top = MessageLabel("INST2").Top - MessageLabel("INST").Height - 5;

            int desiredHeight = MessageLabel("INST").Top - 10 - lbBarcode.Top;

            lbBarcode.Size = new Size(MessageLabel("DESC").Left - lbBarcode.Left, desiredHeight);
            lbDesc.Size = new Size(MessageLabel("CAT").Left - lbDesc.Left, desiredHeight);
            lbCategory.Size = new Size(MessageLabel("QIS").Left - lbCategory.Left, desiredHeight);
            lbQIS.Size = new Size(MessageLabel("QTY").Left - lbQIS.Left, desiredHeight);
            lbQtyOrdered.Size = new Size(MessageLabel("COST").Left - lbQtyOrdered.Left, desiredHeight);
            lbCostPrice.Size = new Size(MessageLabel("RRP").Left - lbCostPrice.Left, desiredHeight);
            lbRetailPrice.Size = new Size(MessageLabel("MINORD").Left - lbRetailPrice.Left, desiredHeight);
            lbMinOrder.Size = new Size(MessageLabel("ON").Left - lbMinOrder.Left, desiredHeight);
            lbOnOrder.Size = new Size(MessageLabel("YTD").Left - lbOnOrder.Left, desiredHeight);
            lbYTD.Size = new Size(MessageLabel("AVEPSALES").Left - lbYTD.Left, desiredHeight);
            lbAvePeriod.Size = new Size(this.ClientSize.Width - lbAvePeriod.Left - lbBarcode.Left, desiredHeight);
        }

        void lbDesc_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Insert || e.KeyCode == Keys.Add || e.KeyCode == Keys.Space || e.KeyCode == Keys.F5)
                GetNextItemCode();
            else if (e.KeyCode == Keys.Enter)
            {
                if (((ListBox)sender).SelectedIndex != -1)
                {
                    EditItem(((ListBox)sender).SelectedIndex);
                }
            }
            else if (e.Shift && e.KeyCode == Keys.P)
            {
                PrintOrder();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                switch (MessageBox.Show("Save this order?", "Save", MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.Yes:
                        SaveOrder(true);
                        bCloseQAsked = true;
                        this.Close();
                        break;
                    case DialogResult.No:
                        bCloseQAsked = true;
                        this.Close();
                        break;
                }
            }
            else if (e.Shift && e.KeyCode == Keys.Delete)
            {
                DeleteRow(((ListBox)sender).SelectedIndex);
            }
            else if (e.KeyCode == Keys.F3)
            {
                CycleSortOrder();
                OrderItems();
            }
            else if (e.KeyCode == Keys.F8)
            {
                frmOrdersWithItemIn fowii = new frmOrdersWithItemIn(ref sEngine, sBarcodes[lbDesc.SelectedIndex]);
                fowii.ShowDialog();
            }
            else if (e.KeyCode == Keys.D && e.Shift)
            {
                sEngine.DiscontinueItem(sBarcodes[((ListBox)sender).SelectedIndex]);
                DeleteRow(((ListBox)sender).SelectedIndex);
            }
            else if (e.KeyCode == Keys.F6)
            {
                frmSearchForItemV2 fsfi = new frmSearchForItemV2(ref sEngine);
                fsfi.ShowDialog();
            }
            else if (e.KeyCode == Keys.F7)
            {
                frmDetailedItemEnquiry fdie = new frmDetailedItemEnquiry(ref sEngine, sBarcodes[((ListBox)sender).SelectedIndex]);
                fdie.ForAnOrder = true;
                fdie.ShowDialog();
                RefreshRowDetails(((ListBox)sender).SelectedIndex);
                if (fdie.ItemAdded)
                {
                    AddRow(fdie.sBarcode, 0);
                    EditItem(lbBarcode.Items.Count - 1);
                }
            }
            else if (e.KeyCode == Keys.F4)
            {
                frmAddEditItem faei = new frmAddEditItem(ref sEngine);
                faei.EditingBarcode = sBarcodes[((ListBox)sender).SelectedIndex];
                faei.ShowDialog();
                RefreshRowDetails(((ListBox)sender).SelectedIndex);
            }
        }

        void RefreshRowDetails(int nRowNum)
        {
            string sBarcode = sBarcodes[nRowNum];
            string[] sMainStockInfo = sEngine.GetMainStockInfo(sBarcode);
            string[] sStockStaInfo = sEngine.GetItemStockStaRecord(sBarcode, SupplierRecord[6]);

            if (sMainStockInfo[5] == "1")
            {
                lbBarcode.Items[nRowNum] = sBarcode;
                lbDesc.Items[nRowNum] = sMainStockInfo[1];;
                lbCategory.Items[nRowNum] = sEngine.GetCategoryDesc(sMainStockInfo[4]);
                decimal dCost = sEngine.GetItemCostBySupplier(sBarcode, SupplierRecord[2]);
                if (dCost == 0)
                    dCost = sEngine.GetItemAverageCost(sBarcode);
                lbCostPrice.Items[nRowNum] = (FormatMoneyForDisplay(dCost));
                lbRetailPrice.Items[nRowNum] = (FormatMoneyForDisplay(sMainStockInfo[2]));
                lbMinOrder.Items[nRowNum] = (FormatMoneyForDisplay(sEngine.GetSensibleDecimal(sStockStaInfo[37])));
                lbOnOrder.Items[nRowNum] = (sStockStaInfo[3]);
                lbQIS.Items[nRowNum] = (sStockStaInfo[36]);
                lbYTD.Items[nRowNum] = (FormatMoneyForDisplay(sStockStaInfo[17]));
                lbAvePeriod.Items[nRowNum] = FormatMoneyForDisplay(dDaysPeriod * Convert.ToDecimal(sStockStaInfo[2]));
            }
            else
            {
                DeleteRow(nRowNum);
            }
        }


        void DeleteRow(int nRowNum)
        {
            
            sEngine.RemoveQuantityOnOrder(lbBarcode.Items[nRowNum].ToString(), Convert.ToDecimal(lbQtyOrdered.Items[nRowNum]) - Convert.ToDecimal(sReceived[nRowNum]), SupplierRecord[6]);
            lbBarcode.Items.RemoveAt(nRowNum);
            lbDesc.Items.RemoveAt(nRowNum);
            lbCategory.Items.RemoveAt(nRowNum);
            lbYTD.Items.RemoveAt(nRowNum);
            lbCostPrice.Items.RemoveAt(nRowNum);
            lbMinOrder.Items.RemoveAt(nRowNum);
            lbOnOrder.Items.RemoveAt(nRowNum);
            lbQIS.Items.RemoveAt(nRowNum);
            lbQtyOrdered.Items.RemoveAt(nRowNum);
            lbRetailPrice.Items.RemoveAt(nRowNum);
            lbAvePeriod.Items.RemoveAt(nRowNum);
            for (int i = nRowNum; i < sBarcodes.Length - 1; i++)
            {
                sBarcodes[i] = sBarcodes[i + 1];
                sReceived[i] = sReceived[i + 1];
                dOrderAmounts[i] = dOrderAmounts[i + 1];
                dOrderQtys[i] = dOrderQtys[i + 1];
            }
            if (lbDesc.Items.Count != 0)
            {
                if (nRowNum == 0)
                    lbDesc.SelectedIndex = 0;
                else
                    lbDesc.SelectedIndex = nRowNum - 1;
            }
            Array.Resize<string>(ref sBarcodes, sBarcodes.Length - 1);
            Array.Resize<decimal>(ref dOrderAmounts, dOrderAmounts.Length - 1);
            Array.Resize<decimal>(ref dOrderQtys, dOrderQtys.Length - 1);
            Array.Resize<string>(ref sReceived, sReceived.Length - 1);
            //SumOrderTotal();
        }

        void EditItem(int nRow)
        {
            frmSingleInputBox fGetQty = new frmSingleInputBox("Order Quantity for " + sEngine.GetMainStockInfo(sBarcodes[nRow])[1] + " :", ref sEngine);
            fGetQty.tbResponse.Text = FormatMoneyForDisplay(dOrderQtys[nRow]);
            fGetQty.ShowDialog();
            if (fGetQty.Response != "$NONE")
            {
                if (Convert.ToDecimal(fGetQty.Response) < Convert.ToDecimal(sReceived[nRow]))
                {
                    MessageBox.Show("You can't alter the order quantity lower than the quantity that has already been received");
                }
                else
                {
                    if (fOS.OrderExists)
                    {
                        sEngine.RemoveQuantityOnOrder(sBarcodes[nRow], Convert.ToDecimal(lbQtyOrdered.Items[nRow]), SupplierRecord[6]);
                        sEngine.AddQuantityOnOrder(sBarcodes[nRow], Convert.ToDecimal(fGetQty.Response), SupplierRecord[6]);
                    }
                    decimal dQtyToOrder = Convert.ToDecimal(fGetQty.Response);
                    dOrderQtys[nRow] = dQtyToOrder;
                    lbQtyOrdered.Items[nRow] = FormatMoneyForDisplay(dQtyToOrder);
                    SumOrderTotal();
                    if (lbBarcode.Items.Count - 1 > nRow)
                    {
                        lbBarcode.SelectedIndex = nRow + 1;
                    }
                }
            }
            SumOrderTotal();
        }

        void lbSelectedChanged(object sender, EventArgs e)
        {
            lbBarcode.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbDesc.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbCategory.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbYTD.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbCostPrice.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbMinOrder.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbOnOrder.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbQIS.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbQtyOrdered.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbRetailPrice.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbAvePeriod.SelectedIndex = ((ListBox)sender).SelectedIndex;
        }


        /// <summary>
        /// Attempts to add a row to the order
        /// </summary>
        /// <param name="sBarcode">The barcode of the item to add</param>
        /// <param name="dQTYToOrder">The quantity to add to the order</param>
        /// <returns>True if a success, false otherwise</returns>
        private bool AddRow(string sBarcode, decimal dQTYToOrder)
        {
            for (int i = 0; i < sBarcodes.Length; i++)
            {
                if (sBarcodes[i] == sBarcode)
                {
                    // The barcode already exists
                    if (MessageBox.Show("The item already exists in this order, should I increase the quantity ordered of that item?", "Item Exists", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        lbDesc.SelectedIndex = i;
                        lbQtyOrdered.Items[i] = FormatMoneyForDisplay(Convert.ToDecimal(lbQtyOrdered.Items[i].ToString()) + dQTYToOrder);
                    }
                    return true;
                }
            }
            string[] sMainStockInfo = sEngine.GetMainStockInfo(sBarcode);
            string[] sStockStaInfo = sEngine.GetItemStockStaRecord(sBarcode, SupplierRecord[6]);

            if (sMainStockInfo[5] == "1")
            {
                Array.Resize<string>(ref sBarcodes, sBarcodes.Length + 1);
                Array.Resize<decimal>(ref dOrderAmounts, dOrderAmounts.Length + 1);
                Array.Resize<decimal>(ref dOrderQtys, dOrderQtys.Length + 1);
                Array.Resize<string>(ref sReceived, sReceived.Length + 1);
                sBarcodes[sBarcodes.Length - 1] = sBarcode;
                sReceived[sReceived.Length - 1] = "0";
                lbBarcode.Items.Add(sBarcode);
                lbDesc.Items.Add(sMainStockInfo[1]);
                lbCategory.Items.Add(sEngine.GetCategoryDesc(sMainStockInfo[4]));
                lbQtyOrdered.Items.Add(FormatMoneyForDisplay(dQTYToOrder));
                decimal dCost = sEngine.GetItemCostBySupplier(sBarcode, SupplierRecord[1]);
                if (dCost == 0)
                    dCost = sEngine.GetItemAverageCost(sBarcode);
                lbCostPrice.Items.Add(FormatMoneyForDisplay(dCost));
                dOrderAmounts[dOrderAmounts.Length - 1] = dCost;
                lbRetailPrice.Items.Add(FormatMoneyForDisplay(sMainStockInfo[2]));
                lbMinOrder.Items.Add(FormatMoneyForDisplay(sEngine.GetSensibleDecimal(sStockStaInfo[37])));
                lbOnOrder.Items.Add(sStockStaInfo[3]);
                lbQIS.Items.Add(FormatMoneyForDisplay(sStockStaInfo[36]));
                lbYTD.Items.Add(FormatMoneyForDisplay(sStockStaInfo[17]));
                lbAvePeriod.Items.Add(FormatMoneyForDisplay(dDaysPeriod * Convert.ToDecimal(sStockStaInfo[2])));
                dOrderQtys[dOrderQtys.Length - 1] = dQTYToOrder;
                lbDesc.SelectedIndex = lbDesc.Items.Count - 1;
            }
            else if (sMainStockInfo[5] == "5")
            {
                if (MessageBox.Show("You can only add type 1 items to an order. Would you like to use the parent barcode for this item instead?", "Child Barcode", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    string sParent = sMainStockInfo[7];
                    AddRow(sParent, dQTYToOrder);
                }
                return false;
            }
            else
            {
                //MessageBox.Show("You can only add type 1 items to an order.");

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


                    return AddRow(sBarcode, dQTYToOrder);
                }
                // The user doesn't want to change the item's type!
                else
                {
                    return false;
                }
            }
            return true;
            //SumOrderTotal();
        }

        void GetNextItemCode()
        {
            frmGetBarcode fGetCode = new frmGetBarcode(ref sEngine);
            fGetCode.ShowDialog();
            if (fGetCode.Barcode != null && fGetCode.Barcode != "")
            {
                frmSingleInputBox fGetQty = new frmSingleInputBox("Order Quantity for " + sEngine.GetMainStockInfo(fGetCode.Barcode)[1] + " :", ref sEngine);
                fGetQty.tbResponse.Text = sEngine.GetItemStockStaRecord(fGetCode.Barcode, fOS.OrderHeaderRecord[6])[37];
                if (fGetQty.tbResponse.Text == null || fGetQty.tbResponse.Text == "" || fGetQty.tbResponse.Text.StartsWith("0"))
                    fGetQty.tbResponse.Text = "1.00";
                fGetQty.ShowDialog();
                if (fGetQty.Response != "$NONE")
                {
                    decimal dQtyToOrder = 0;
                    try
                    {
                        // Try and work out how many the user wants to order
                        dQtyToOrder = Convert.ToDecimal(fGetQty.Response);
                    }
                    catch
                    {
                        dQtyToOrder = sEngine.GetSensibleDecimal(fGetQty.Response);                    
                    }
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
                //decimal dCost = Convert.ToDecimal(lbCostPrice.Items[i]);
                //decimal dQTY = Convert.ToDecimal(lbQtyOrdered.Items[i]);
                dOrderTotal += (dOrderAmounts[i] * dOrderQtys[i]);
            }
            MessageLabel("ORDTOT").Text = "Order Total : " + FormatMoneyForDisplay(dOrderTotal);
        }

        void GetSupplierInfo()
        {
            frmListOfSuppliers flos = new frmListOfSuppliers(ref sEngine);
            while (flos.sSelectedSupplierCode == "NULL")
            {
                flos.ShowDialog();
            }
            SupplierRecord[1] = flos.sSelectedSupplierCode;
        }


        void SaveOrder(bool bUploadOption)
        {
            if (SupplierRecord[1] == "")
            {
                GetSupplierInfo();
            }
            sEngine.AddEditOrderHeader(SupplierRecord);

            string[] sQuantities = new string[sBarcodes.Length];
            string[] sCost = new string[sBarcodes.Length];

            for (int i = 0; i < sBarcodes.Length; i++)
            {
                sQuantities[i] = lbQtyOrdered.Items[i].ToString();
                sCost[i] = lbCostPrice.Items[i].ToString();
            }
            sEngine.AddEditOrderData(sBarcodes, sQuantities, sReceived, sCost, SupplierRecord[0]);
            if (bUploadOption)
            {
                if (MessageBox.Show("Upload changes to all tills now?", "Upload now?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    sEngine.CopyWaitingFilesToTills();
                }
            }
        }

        void PrintOrder()
        {
            string[] sItemCodes = new string[sBarcodes.Length];
            string[] sDescs = new string[sBarcodes.Length];
            string[] sOrdered = new string[sBarcodes.Length];

            if (SupplierRecord[1] == "")
            {
                GetSupplierInfo();
            }

            for (int i = 0; i < sBarcodes.Length; i++)
            {
                sItemCodes[i] = sEngine.GetItemCodeBySupplier(lbBarcode.Items[i].ToString(), SupplierRecord[1]);
                sDescs[i] = lbDesc.Items[i].ToString();
                sOrdered[i] = lbQtyOrdered.Items[i].ToString();
            }
            sEngine.OrderDetailsToPrinter(SupplierRecord[1], SupplierRecord[6], sItemCodes, sDescs, sOrdered, sReceived, OrderToLoad);
        }

    }
}
