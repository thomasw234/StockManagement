using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Forms.WormaldForms;

namespace BackOffice
{
    struct ParentItems : IComparable
    {
        public enum SortOrder { AlphabeticalCode, AlphabeticalDesc, QtyDesc, GrossDesc, Category };
        public SortOrder sOrder;
        public Item ParentItem;
        public Item[] ChildItems;

        #region IComparable Members

        public int CompareTo(object obj)
        {
            ParentItems iToCompareTo = (ParentItems)obj;
            switch (sOrder)
            {
                case SortOrder.Category:
                    return String.Compare(ParentItem.CodeCategory, iToCompareTo.ParentItem.CodeCategory);
                case SortOrder.AlphabeticalCode:
                    return String.Compare(ParentItem.Barcode, iToCompareTo.ParentItem.Barcode, true);
                    break;
                case SortOrder.AlphabeticalDesc:
                    return String.Compare(ParentItem.Description, iToCompareTo.ParentItem.Description, true);
                    break;
                case SortOrder.GrossDesc:
                    decimal fMaxGross = ParentItem.GrossAmount;
                    for (int i = 0; i < ChildItems.Length; i++)
                    {
                        if (ChildItems[i] != null)
                        {
                            if (ChildItems[i].GrossAmount > fMaxGross)
                                fMaxGross = ChildItems[i].GrossAmount;
                        }
                    }
                    decimal dObjMax = iToCompareTo.ParentItem.GrossAmount;
                    for (int i = 0; i < iToCompareTo.ChildItems.Length; i++)
                    {
                        if (iToCompareTo.ChildItems[i] != null)
                        {
                            if (iToCompareTo.ChildItems[i].GrossAmount > dObjMax)
                                dObjMax = iToCompareTo.ChildItems[i].GrossAmount;
                        }
                    }
                    if (fMaxGross < dObjMax)
                    {
                        return 1;
                    }
                    else if (fMaxGross == dObjMax)
                        return 0;
                    else
                        return -1;
                    break;
                case SortOrder.QtyDesc:
                    decimal dMaxThisQty = ParentItem.StockLevel;
                    for (int i = 0; i < ChildItems.Length; i++)
                    {
                        if (ChildItems[i] != null)
                        {
                            if (ChildItems[i].StockLevel > dMaxThisQty)
                                dMaxThisQty = ChildItems[i].StockLevel;
                        }
                    }
                    decimal dMaxOtherQty = iToCompareTo.ParentItem.StockLevel;
                    for (int i = 0; i < iToCompareTo.ChildItems.Length; i++)
                    {
                        if (iToCompareTo.ChildItems[i] != null)
                        {
                            if (iToCompareTo.ChildItems[i].Quantity > dMaxOtherQty)
                                dMaxOtherQty = iToCompareTo.ChildItems[i].StockLevel;
                        }
                    }
                    if (dMaxThisQty < dMaxOtherQty)
                        return 1;
                    else if (dMaxOtherQty == dMaxThisQty)
                        return 0;
                    else
                        return -1;
            }
            return 0;
        }

        #endregion
    }

    class frmSearchForItemV2 : ScalableForm
    {
        StockEngine tEngine;
        Item[] iResults;
        string sSearchTerm = "";
        ParentItems.SortOrder sOrder = ParentItems.SortOrder.AlphabeticalDesc;
        Button btnCategorySelect;
        
        int nCodeLeft = 12;
        int nDescLeft = 202;
        int nPriceLeft = 623;
        int nStockLeft = 553;
        int nCategoryLeft = 728;
        int nCatCodeLeft = 787;
        int[] nDrawLoc;
        List<Item> iItems;

        int nOfSearchResultsBeingDisplayed = 0;
        int nScrolledDown = 0;
        int nCurrentItemSelected = 0;
        int nOfResultsDrawnToScreen = 0;

        ParentItems[] pItems;
        Timer tmrCursorBlink;
        Timer sBarTimer;

        bool bDrawnCursor = false;
        bool bTextBoxHasFocus = true;
        bool bPartialSearchFromScanner = false;
        bool bHideNoStockItems = false;

        string sSelectedBarcode = "NONE_SELECTED";
        public bool bSelectedType4 = false;
        string sShopCodeLookingFor = "";
        public string OriginalBarcode = "";
        public bool JumpedToCategory = false;


        frmCategorySelect fcSelect;
        VScrollBar sBar;


        public frmSearchForItemV2(ref StockEngine se)
        {
            frmListOfShops flos = new frmListOfShops(ref se);
            while (flos.SelectedShopCode == "$NONE")
            {
                flos.ShowDialog();
            }
            sShopCodeLookingFor = flos.SelectedShopCode;
            iItems = new List<Item>();
            this.AllowScaling = false;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Size = new Size(1040, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;
            iResults = new Item[0];
            tEngine = se;
            pItems = new ParentItems[0];
            this.Paint += new PaintEventHandler(frmLookupTransactionsV2_Paint);
            this.KeyDown += new KeyEventHandler(frmLookupTransactionsV2_KeyDown);
            this.FormClosed += new FormClosedEventHandler(frmSearchForItemV2_FormClosed);
            tmrCursorBlink = new Timer();
            tmrCursorBlink.Interval = 500;
            tmrCursorBlink.Tick += new EventHandler(tmrCursorBlink_Tick);
            tmrCursorBlink.Enabled = true;
            this.Text = "Search For Items";
            this.MouseDown += new MouseEventHandler(frmSearchForItemV2_MouseDown);
            this.MouseClick += new MouseEventHandler(frmSearchForItemV2_MouseClick);
            this.MouseWheel += new MouseEventHandler(frmSearchForItemV2_MouseWheel);
            this.MouseMove += new MouseEventHandler(frmSearchForItemV2_MouseMove);


            sBar = new VScrollBar();
            sBar.Location = new Point(this.ClientSize.Width - sBar.Width, 150);
            sBar.Height = this.ClientSize.Height - 150;
            sBar.Minimum = 0;
            sBar.Maximum = 0;
            sBar.SmallChange = 1;
            sBar.LargeChange = 20;
            sBar.Scroll += new ScrollEventHandler(sBar_Scroll);
            this.Controls.Add(sBar);
            sBar.Visible = false;
            lastMousePos = Cursor.Position;

            sBarTimer = new Timer();
            sBarTimer.Interval = 1000;
            sBarTimer.Enabled = true;
            sBarTimer.Tick += new EventHandler(sBarTimer_Tick);
            
        }

        void sBarTimer_Tick(object sender, EventArgs e)
        {
            nTimeSinceMove++;
            if (nTimeSinceMove > 1)
            {
                sBar.Visible = false;
                sBar.Enabled = false;
                this.Focus();
                
            }
        }

        void sBar_Scroll(object sender, ScrollEventArgs e)
        {
            nScrolledDown = sBar.Value;
            if (nScrolledDown > nCurrentItemSelected)
            {
                nCurrentItemSelected = nScrolledDown;
            }
            else if (nScrolledDown + nOfResultsDrawnToScreen < nCurrentItemSelected)
            {
                nCurrentItemSelected = nScrolledDown + nOfResultsDrawnToScreen;
            }
            this.Refresh();
        }

        void frmSearchForItemV2_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (nDrawLoc == null)
                    return;
                for (int i = 0; i < nDrawLoc.Length; i++)
                {
                    if (i != nDrawLoc.Length - 1)
                    {
                        if (e.Y > nDrawLoc[i] && e.Y < nDrawLoc[i + 1])
                        {
                            nCurrentItemSelected = i;
                            break;
                        }
                        else if (e.Y > nDrawLoc[i] && nDrawLoc[i] != 0 && nDrawLoc[i + 1] == 0)
                        {
                            nCurrentItemSelected = i;
                            break;
                        }
                    }
                    else
                    {
                        if (e.Y > nDrawLoc[nDrawLoc.Length - 1])
                        {
                            nCurrentItemSelected = nDrawLoc.Length - 1;
                        }
                    }
                }
                this.Refresh();
            }
            if (Cursor.Position != lastMousePos || nLastMouseScroll != nCurrentItemSelected + nScrolledDown)
            {
                sBar.Visible = true;
                sBar.Enabled = true;
                nTimeSinceMove = 0;
                lastMousePos = Cursor.Position;
                nLastMouseScroll = nCurrentItemSelected + nScrolledDown;
            }
        }

        Point lastMousePos = new Point(0, 0);
        int nLastMouseScroll = 0;
        int nTimeSinceMove = 0;
        

        void frmSearchForItemV2_MouseDown(object sender, MouseEventArgs e)
        {
            if (nDrawLoc == null)
                return;
            int nNotShown = 0;
            for (int i = 0; i < nDrawLoc.Length; i++)
            {
                if (i != nDrawLoc.Length - 1)
                {
                    if (nDrawLoc[i] == 0)
                    {
                        nNotShown++;
                        continue;
                    }
                    if (e.Y > nDrawLoc[i] && e.Y < nDrawLoc[i + 1])
                    {
                        nCurrentItemSelected = i - nNotShown;
                        break;
                    }

                    /*else if (e.Y > nDrawLoc[i] && nDrawLoc[i] != 0 && nDrawLoc[i + 1] == 0)
                    {
                        nCurrentItemSelected = i;
                        break;
                    }*/
                }
                else
                {
                    if (e.Y > nDrawLoc[nDrawLoc.Length - 1])
                    {
                        nCurrentItemSelected = nDrawLoc.Length - 1;
                    }
                }
            }
            this.Refresh();
        }

        void btnCategorySelect_Click(object sender, EventArgs e)
        {
            ShowCategorySelect();
        }

        void frmSearchForItemV2_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                if (e.Delta < 0)
                {
                    for (int i = e.Delta; i <= 0; i+=120)
                    {
                        MoveDown();
                    }
                }
                else
                {
                    for (int i = e.Delta; i >= 0; i-=120)
                    {
                        MoveUp();
                    }
                }
            }
            this.Refresh();
        }

        void frmSearchForItemV2_MouseClick(object sender, MouseEventArgs e)
        {
            // Text Box Click
            if (e.X > 135 && e.X < this.Width - 150 && e.Y > 30 && e.Y < 55)
            {
                if (!bTextBoxHasFocus)
                    bTextBoxHasFocus = true;
                else
                {
                    ShowCategorySelect();
                }
            }
            else
            {
                /*int nNotDrawn = 0;
                for (int i = 0; i < nDrawLoc.Length; i++)
                {
                    if (nDrawLoc[i] == 0)
                    {
                        nNotDrawn++;
                        continue;
                    }
                    if (e.Y > nDrawLoc[i] && e.Y < nDrawLoc[i] + 25)
                    {
                        nCurrentItemSelected = i;// -nNotDrawn;
                        if (bHideNoStockItems)
                        {
                            nCurrentItemSelected -= nNotDrawn;
                        }
                        break;
                    }
                    else if (e.Y > nDrawLoc[i] && e.Y < nDrawLoc[i] + 25 && nDrawLoc[i] != 0)
                    {
                        nCurrentItemSelected = i - nNotDrawn;
                        break;
                    }*/
                sSelectedBarcode = GetBarcodeFromClickY(e.Y);
                if (sSelectedBarcode == "")
                {
                    sSelectedBarcode = "NONE_SELECTED";
                }
                else
                {
                    this.Close();
                }
                
                //EnterPressed();
            }
            this.Refresh();
        }

        public void ShowCategorySelect()
        {
            fcSelect = new frmCategorySelect(ref tEngine);

            if (sSearchTerm.StartsWith("CAT:"))
            {
                string sTemp = sSearchTerm.Remove(0, 4);
                if (sTemp.Length > 2)
                {

                    fcSelect.ShowCategories(sTemp.Remove(sTemp.Length - 2, 2));
                    fcSelect.SelectCategoryCode(sSearchTerm.Remove(0, 4));
                }
            }
            fcSelect.ShowDialog();
            if (fcSelect.SelectedItemCategory != "$NULL")
            {
                sSearchTerm = "CAT:" + fcSelect.SelectedItemCategory;
                SearchForItems();
                this.Focus();
                if (nOfSearchResultsBeingDisplayed > 0)
                {
                    nScrolledDown = 0;
                    nCurrentItemSelected = 0;
                    bTextBoxHasFocus = false;
                }
            }
            else
            {
                if (JumpedToCategory)
                {
                    JumpedToCategory = false;
                }
            }
            fcSelect.Dispose();
            this.Refresh();
        }

        void frmSearchForItemV2_FormClosed(object sender, FormClosedEventArgs e)
        {
            tmrCursorBlink.Enabled = false;
            tmrCursorBlink.Dispose();
        }

        void tmrCursorBlink_Tick(object sender, EventArgs e)
        {
            Font fFont = new Font(this.Font.FontFamily, 16.0f);
            Point pTop = new Point(10 + Convert.ToInt32(this.CreateGraphics().MeasureString("Search Term : " + sSearchTerm, fFont, this.Width, StringFormat.GenericTypographic).Width), 30);
            Point pBottom = new Point(10 + Convert.ToInt32(this.CreateGraphics().MeasureString("Search Term : " + sSearchTerm, fFont, this.Width, StringFormat.GenericTypographic).Width), 55);
            if (sSearchTerm.Length > 0 && sSearchTerm[sSearchTerm.Length - 1] == ' ')
            {
                pTop.X += 10;
                pBottom.X += 10;
            }
            if (!bDrawnCursor && bTextBoxHasFocus)
            {
                this.CreateGraphics().DrawLine(new Pen(Color.Black), pTop, pBottom);
            }
            else if (bTextBoxHasFocus && bDrawnCursor)
            {
                this.CreateGraphics().DrawLine(new Pen(this.BackColor), pTop, pBottom);
            }
            else
            {
                this.CreateGraphics().DrawLine(new Pen(this.ForeColor), pTop, pBottom);
            }
            bDrawnCursor = !bDrawnCursor;
        }

        void frmLookupTransactionsV2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Shift && e.KeyCode == Keys.P && sSearchTerm != "TYPE:4")
            {
                ReportOrderedBy seOrder = ReportOrderedBy.CodeAlphabetical;
                switch (sOrder)
                {
                    case ParentItems.SortOrder.AlphabeticalCode:
                        seOrder = ReportOrderedBy.CodeAlphabetical;
                        break;
                    case ParentItems.SortOrder.AlphabeticalDesc:
                        seOrder = ReportOrderedBy.DescAlphabetical;
                        break;
                    case ParentItems.SortOrder.GrossDesc:
                        seOrder = ReportOrderedBy.RRP;
                        break;
                    case ParentItems.SortOrder.QtyDesc:
                        seOrder = ReportOrderedBy.QIS;
                        break;
                }
                string[] sBarcodes = new string[iResults.Length];
                for (int i = 0; i < iResults.Length; i++)
                {
                    sBarcodes[i] = iResults[i].Barcode;
                }
                tEngine.StockReportToPrinter(sBarcodes, seOrder, !bHideNoStockItems);

            }
            else if (e.KeyValue >= 65 && e.KeyValue <= 90)
            {
                if (bTextBoxHasFocus || !JumpToItemByKey(e.KeyCode))
                {
                    sSearchTerm += e.KeyCode.ToString();
                    bTextBoxHasFocus = true;
                }
            }
            else if (e.KeyValue >= 47 && e.KeyValue <= 57)
            {
                if (bTextBoxHasFocus || !JumpToItemByKey(e.KeyCode))
                {
                    sSearchTerm += (e.KeyValue - 48).ToString();
                    bTextBoxHasFocus = true;
                }
            }
            else if (e.KeyValue >= 96 && e.KeyValue <= 106)
            {
                if (bTextBoxHasFocus || !JumpToItemByKey(e.KeyCode))
                {
                    sSearchTerm += (e.KeyValue - 96).ToString();
                    bTextBoxHasFocus = true;
                }
            }
            else if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus)
            {
                if (bTextBoxHasFocus || !JumpToItemByKey(e.KeyCode))
                {
                    sSearchTerm += "-";
                    bTextBoxHasFocus = true;
                }
            }
            else if (e.KeyValue == 8)
            {
                string sNewSearchTerm = "";
                for (int i = 0; i < sSearchTerm.Length - 1; i++)
                {
                    sNewSearchTerm += sSearchTerm[i].ToString();
                }
                if (sSearchTerm == "CAT:")
                    sSearchTerm = "";
                else
                    sSearchTerm = sNewSearchTerm;
                bTextBoxHasFocus = true;
            }
            else if (e.KeyValue == 32)
            {
                sSearchTerm += " ";
                bTextBoxHasFocus = true;
            }
            else if (e.KeyValue == 13 && bTextBoxHasFocus)
            {
                // Do search
                if (sSearchTerm.Length > 1)
                {
                    this.CreateGraphics().DrawString("Searching by code, please wait.", new Font(this.Font.FontFamily, 12.0f), new SolidBrush(Color.Red), new PointF(10, 107));
                    SearchForItems();
                    bTextBoxHasFocus = false;
                }
            }
            else if (e.KeyValue == 9)
            {
                bHideNoStockItems = !bHideNoStockItems;
                CountNumberOfResultsToShow();
                nCurrentItemSelected = 0;
                nScrolledDown = 0;
                if (nOfSearchResultsBeingDisplayed == 0)
                    bTextBoxHasFocus = true;
            }
            else if (e.KeyValue == 40)
            {
                MoveDown();
            }
            else if (e.KeyValue == 38)
            {
                MoveUp();
            }
            else if (e.KeyValue == 27)
            {
                if (bTextBoxHasFocus)
                    this.Close();
                else
                {
                    bTextBoxHasFocus = true;
                    nScrolledDown = 0;
                    nCurrentItemSelected = 0;
                }
            }
            else if (e.KeyCode == Keys.Enter)
            {
                EnterPressed();
            }
            else if (e.KeyCode == Keys.F5)
            {
                ShowCategorySelect();
            }
            else if (e.KeyCode == Keys.F6)
            {
                sSearchTerm = "TYPE:4";
                SearchForItems();
            }
            else if (e.KeyCode == Keys.Oem1)
            {
                sSearchTerm += ":";
            }
            else if (e.KeyCode == Keys.F3)
            {
                if (pItems.Length > 0)
                {
                    // Cycle sort order
                    ParentItems.SortOrder sNewOrder = ParentItems.SortOrder.AlphabeticalCode;
                    switch (pItems[pItems.Length - 1].sOrder)
                    {
                        case ParentItems.SortOrder.AlphabeticalCode:
                            sNewOrder = ParentItems.SortOrder.AlphabeticalDesc;
                            break;
                        case ParentItems.SortOrder.AlphabeticalDesc:
                            sNewOrder = ParentItems.SortOrder.QtyDesc;
                            break;
                        case ParentItems.SortOrder.QtyDesc:
                            sNewOrder = ParentItems.SortOrder.GrossDesc;
                            break;
                        case ParentItems.SortOrder.GrossDesc:
                            sNewOrder = ParentItems.SortOrder.Category;
                            break;
                        case ParentItems.SortOrder.Category:
                            sNewOrder = ParentItems.SortOrder.AlphabeticalCode;
                            break;
                    }
                    sOrder = sNewOrder;
                    OrderResults();
                    CountNumberOfResultsToShow();
                }
            }
            else if (e.KeyCode == Keys.PageDown)
            {
                nScrolledDown += nOfResultsDrawnToScreen;
                if (nScrolledDown + nOfResultsDrawnToScreen >= nOfSearchResultsBeingDisplayed)
                    nScrolledDown = nOfSearchResultsBeingDisplayed - nOfResultsDrawnToScreen;
                nCurrentItemSelected = nScrolledDown;

            }
            else if (e.KeyCode == Keys.PageUp)
            {
                nScrolledDown -= nOfResultsDrawnToScreen;
                if (nScrolledDown == 0)
                    bTextBoxHasFocus = true;
                if (nScrolledDown < 0)
                    nScrolledDown = 0;
                nCurrentItemSelected = nScrolledDown;
            }
            else
            {
                bTextBoxHasFocus = true;
            }
            this.Refresh();
        }

        void MoveDown()
        {
            
                // Move Down
                if (bTextBoxHasFocus)
                {
                    bTextBoxHasFocus = false;
                    nCurrentItemSelected = 0;
                    nScrolledDown = 0;
                }
                else
                {
                    if (nCurrentItemSelected + 1 < nOfSearchResultsBeingDisplayed)
                    {
                        nCurrentItemSelected++;
                    }
                    else if (!bTextBoxHasFocus)
                    {
                        // Don't allow it to jump back up to the top text box when the bottom is reached
                        //bTextBoxHasFocus = true;
                    }
                    if (nCurrentItemSelected > nScrolledDown + nOfResultsDrawnToScreen && nCurrentItemSelected < nOfSearchResultsBeingDisplayed)
                    {
                        nScrolledDown++;
                    }
                }
        }

        void MoveUp()
        {
                // Move up
                if (nCurrentItemSelected > 0 && !bTextBoxHasFocus)
                {
                    nCurrentItemSelected -= 1;
                }
                else if (bTextBoxHasFocus)
                {
                    // Don't allow it to jump back to the bottom when pressing up
                    /*bTextBoxHasFocus = false;
                    nCurrentItemSelected = nOfSearchResultsBeingDisplayed - 1;
                    nScrolledDown = nOfSearchResultsBeingDisplayed - nOfResultsDrawnToScreen - 1;
                    if (nScrolledDown < 0)
                        nScrolledDown = 0;*/
                }
                else if (nCurrentItemSelected == 0 && !bTextBoxHasFocus)
                {
                    bTextBoxHasFocus = true;
                }
                if (nCurrentItemSelected < nScrolledDown)
                    nScrolledDown -= 1;
        }

        void EnterPressed()
        {
            int nSelectedItem = -1;
            for (int i = 0; i < pItems.Length; i++)
            {
                if (bHideNoStockItems && pItems[i].ParentItem.StockLevel > 0 || !bHideNoStockItems)
                {
                    nSelectedItem++;
                    if (nSelectedItem == nCurrentItemSelected)
                    {
                        sSelectedBarcode = pItems[i].ParentItem.Barcode;
                        if (pItems[i].ParentItem.ItemCategory == 4)
                            bSelectedType4 = true;
                        break;
                    }
                }
                for (int x = 0; x < pItems[i].ChildItems.Length; x++)
                {
                    if (bHideNoStockItems && pItems[i].ChildItems[x].StockLevel > 0 || !bHideNoStockItems)
                    {
                        nSelectedItem++;
                        if (nSelectedItem == nCurrentItemSelected)
                        {
                            sSelectedBarcode = pItems[i].ChildItems[x].Barcode;
                            break;
                        }
                    }
                }

            }
            this.Close();
        }

        void frmLookupTransactionsV2_Paint(object sender, PaintEventArgs e)
        {
            Font fDrawingFont = new Font(this.Font.FontFamily, 16.0f);
            if (bTextBoxHasFocus)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.White), new Rectangle(135, 30, this.Width - 150, 25));
            }
            e.Graphics.DrawString("Lookup Item", fDrawingFont, new SolidBrush(Color.Black), new PointF((this.Width / 2) - (e.Graphics.MeasureString("Lookup Item", fDrawingFont).Width / 2), 10));
            e.Graphics.DrawString("Search Term : " + sSearchTerm, fDrawingFont, new SolidBrush(Color.Black), new PointF(10, 30));
            DrawResults(e.Graphics);
            SolidBrush s = new SolidBrush(Color.Black);
            fDrawingFont = new Font(this.Font.FontFamily, 10.0f);
            if (!bPartialSearchFromScanner)
            {
                e.Graphics.DrawString("Enter the barcode or description of the item that you are searching for, then press Enter. CAT: followed by category code to show a category.", fDrawingFont, s, new PointF(10.0f, 60.0f));
                e.Graphics.DrawString("Press TAB to hide / show stock items that are not in stock. Press F5 (or click in the search box) to choose a category to show.", fDrawingFont, s, new PointF(10, 75));
                e.Graphics.DrawString("Press F3 to change the sort order. The current sort order is marked with an asterisk. Shift & P to print the search results.", fDrawingFont, s, new PointF(10, 90));
            }
            fDrawingFont = new Font(this.Font.FontFamily, 14.0f);
            if (sOrder == ParentItems.SortOrder.AlphabeticalCode)
                e.Graphics.DrawString("Code*", fDrawingFont, s, new PointF((float)nCodeLeft, 125.0f));
            else
                e.Graphics.DrawString("Code", fDrawingFont, s, new PointF((float)nCodeLeft, 125.0f));
            if (sOrder == ParentItems.SortOrder.AlphabeticalDesc)
                e.Graphics.DrawString("Description*", fDrawingFont, s, new PointF((float)nDescLeft, 125.0f));
            else
                e.Graphics.DrawString("Description", fDrawingFont, s, new PointF((float)nDescLeft, 125.0f));
            if (sOrder == ParentItems.SortOrder.QtyDesc)
            {
                e.Graphics.DrawString("Stock*", fDrawingFont, s, new PointF((float)nStockLeft, 105.0f));
                e.Graphics.DrawString("Level", fDrawingFont, s, new PointF((float)nStockLeft, 125.0f));
            }
            else
            {
                e.Graphics.DrawString("Stock", fDrawingFont, s, new PointF((float)nStockLeft, 105.0f));
                e.Graphics.DrawString("Level", fDrawingFont, s, new PointF((float)nStockLeft, 125.0f));
            }
            if (sOrder == ParentItems.SortOrder.GrossDesc)
            {
                e.Graphics.DrawString("Price*", fDrawingFont, s, new PointF((float)nPriceLeft, 125.0f));
            }
            else
            {
                e.Graphics.DrawString("Price", fDrawingFont, s, new PointF((float)nPriceLeft, 125.0f));
            }
            e.Graphics.DrawString("Item", fDrawingFont, s, new PointF((float)nCategoryLeft, 105.0f));
            e.Graphics.DrawString("Type", fDrawingFont, s, new PointF((float)nCategoryLeft, 125.0f));
            if (sOrder == ParentItems.SortOrder.Category)
            {
                e.Graphics.DrawString("Category*", fDrawingFont, s, new PointF((float)nCatCodeLeft, 125.0f));
            }
            else
            {
                e.Graphics.DrawString("Category", fDrawingFont, s, new PointF((float)nCatCodeLeft, 125.0f));
            }
        }


        void SearchForItems()
        {
            Graphics g = this.CreateGraphics();
            nScrolledDown = 0;
            nCurrentItemSelected = 0;
            iResults = new Item[0];
            int nOfItems = 0;
            if (!sSearchTerm.StartsWith("CAT:") && sSearchTerm != "TYPE:6")
            {
                string[,] sSearchResults = tEngine.sGetAccordingToPartialBarcode(sSearchTerm, ref nOfItems);
                Array.Resize<Item>(ref iResults, nOfItems);
                string[] sRecordContents;
                for (int i = 0; i < nOfItems; i++)
                {
                    if ((i % 200) == 0)
                    {
                        g.FillRectangle(new SolidBrush(this.BackColor), new Rectangle(10, 107, this.Width, 20));
                        g.DrawString("Adding search by code result " + i.ToString() + " of " + nOfItems.ToString(), new Font(this.Font.FontFamily, 12.0f), new SolidBrush(Color.Black), new PointF(10, 107));
                    }
                    sRecordContents = new string[8];
                    for (int x = 0; x < sRecordContents.Length; x++)
                    {
                        sRecordContents[x] = sSearchResults[i, x].TrimEnd(' ');
                    }
                    iResults[i] = new Item(sRecordContents, tEngine.GetItemStockStaRecord(sSearchResults[i, 0], sShopCodeLookingFor));
                    if (sRecordContents[7].TrimEnd(' ').Length > 0 && !sRecordContents[7].StartsWith("@"))
                    {
                        // Has a parent item
                        iResults[i].ParentBarcode = sRecordContents[7].TrimEnd(' ');
                    }
                    else
                    {
                        iResults[i].ParentBarcode = "";
                    }
                }
                g.FillRectangle(new SolidBrush(this.BackColor), new Rectangle(10, 107, this.Width, 20));
                g.DrawString("Searching by description, please wait.", new Font(this.Font.FontFamily, 12.0f), new SolidBrush(Color.Black), new PointF(10, 107));
                sSearchResults = tEngine.sGetAccordingToPartialDescription(sSearchTerm, ref nOfItems);
                int nPrevLength = iResults.Length;
                bool[] bItemAlreadyExists = new bool[nOfItems];
                int nOfItemsAlreadyExist = 0;
                g.FillRectangle(new SolidBrush(this.BackColor), new Rectangle(10, 107, this.Width, 20));
                g.DrawString("Removing duplicate results (" + nOfItemsAlreadyExist.ToString() + " found so far), please wait.", new Font(this.Font.FontFamily, 12.0f), new SolidBrush(Color.Black), new PointF(10, 107));
                for (int i = 0; i < nOfItems; i++)
                {
                    bItemAlreadyExists[i] = false;
                    for (int x = 0; x < iResults.Length; x++)
                    {
                        if (iResults[x].Barcode.TrimEnd(' ') == sSearchResults[i, 0].TrimEnd(' ') && bItemAlreadyExists[i] == false)
                        {
                            bItemAlreadyExists[i] = true;
                            nOfItemsAlreadyExist++;
                        }
                        else if (tEngine.GetItemStockStaRecord(sSearchResults[i, 0], sShopCodeLookingFor)[35] != sShopCodeLookingFor || iResults[x].ShopCode != sShopCodeLookingFor || iResults[x].ParentBarcode.StartsWith("@"))
                        {
                            bItemAlreadyExists[i] = true;
                            nOfItemsAlreadyExist++;
                        }
                    }
                }
                Array.Resize<Item>(ref iResults, iResults.Length + nOfItems - nOfItemsAlreadyExist);
                int nOfItemsSkipped = 0;
                for (int i = nPrevLength; i < iResults.Length + nOfItemsAlreadyExist; i++)
                {
                    if (!bItemAlreadyExists[i - nPrevLength])
                    {
                        if ((i % 200) == 0)
                        {
                            g.FillRectangle(new SolidBrush(this.BackColor), new Rectangle(10, 107, this.Width, 20));
                            g.DrawString("Adding search by description result " + (i - nPrevLength).ToString() + " of " + (nOfItems - nOfItemsAlreadyExist).ToString() + ", please wait.", new Font(this.Font.FontFamily, 12.0f), new SolidBrush(Color.Black), new PointF(10, 107));
                        }
                        sRecordContents = new string[8];
                        for (int x = 0; x < sRecordContents.Length; x++)
                        {
                            //sRecordContents[x] = sSearchResults[i - nPrevLength - nOfItemsSkipped, x];
                            sRecordContents[x] = sSearchResults[i - nPrevLength, x];
                        }
                        //iResults[i - nOfItemsSkipped] = new Item(sRecordContents, tEngine.GetItemStockStaRecord(sSearchResults[i - nPrevLength - nOfItemsSkipped, 0], sShopCodeLookingFor));
                        iResults[i - nOfItemsSkipped] = new Item(sRecordContents, tEngine.GetItemStockStaRecord(sSearchResults[i - nPrevLength, 0], sShopCodeLookingFor));
                        if (sRecordContents[7].Trim(' ').Length > 0)
                        {
                            iResults[i - nOfItemsSkipped].ParentBarcode = sRecordContents[7].TrimEnd(' ');
                        }
                        else
                        {
                            iResults[i - nOfItemsSkipped].ParentBarcode = "";
                        }
                    }
                    else
                        nOfItemsSkipped++;
                }
                // Add parents
                for (int i = 0; i < iResults.Length; i++)
                {
                    if ((i % 200) == 0)
                    {
                        g.FillRectangle(new SolidBrush(this.BackColor), new Rectangle(10, 107, this.Width, 20));
                        g.DrawString("Searching for parent and child items of result number " + i.ToString() + " of " + iResults.Length.ToString() + ", please wait.", new Font(this.Font.FontFamily, 12.0f), new SolidBrush(Color.Black), new PointF(10, 107));
                    }
                    if (iResults[i].ParentBarcode != "")
                    {
                        bool bAlreadyExists = false;
                        for (int x = 0; x < iResults.Length; x++)
                        {
                            if (iResults[x].Barcode.TrimEnd(' ') == iResults[i].ParentBarcode.TrimEnd(' '))
                                bAlreadyExists = true;
                        }
                        if (!bAlreadyExists && !iResults[i].ParentBarcode.StartsWith("@"))
                        {
                            Array.Resize<Item>(ref iResults, iResults.Length + 1);
                            iResults[iResults.Length - 1] = new Item(tEngine.GetMainStockInfo(iResults[i].ParentBarcode), tEngine.GetItemStockStaRecord(iResults[i].ParentBarcode, sShopCodeLookingFor));
                            
                        }
                    }
                    else
                    {
                        // Check for children
                        string[] sChildren = tEngine.CheckIfItemHasChildren(iResults[i].Barcode);
                        if (sChildren.Length > 0)
                        {
                            for (int x = 0; x < sChildren.Length; x++)
                            {
                                bool bAlreadyExists = false;
                                for (int z = 0; z < iResults.Length; z++)
                                {
                                    if (iResults[z].Barcode.TrimEnd(' ') == sChildren[x].TrimEnd(' '))
                                        bAlreadyExists = true;
                                }
                                if (!bAlreadyExists)
                                {
                                    Array.Resize<Item>(ref iResults, iResults.Length + 1);
                                    iResults[iResults.Length - 1] = new Item(tEngine.GetMainStockInfo(sChildren[x].TrimEnd(' ')), tEngine.GetItemStockStaRecord(sChildren[x].TrimEnd(' '), sShopCodeLookingFor));
                                }
                            }
                        }
                    }
                }
            }
            else if (sSearchTerm == "TYPE:4")
            {
                string[] sBarcodes = tEngine.GetListOfTypeFourItems(sShopCodeLookingFor);
                iResults = new Item[sBarcodes.Length];
                for (int i = 0; i < sBarcodes.Length; i++)
                {
                    string[] sStockSta = new string[42];
                    sStockSta[0] = sBarcodes[i];
                    for (int x = 1; x < 41; x++)
                    {
                        sStockSta[x] = "0";
                    }
                    string[] sMainStock = new string[8];
                    for (int x = 0; x < sMainStock.Length; x++)
                    {
                        sMainStock[x] = "0";
                    }
                    sMainStock[0] = sBarcodes[i];
                    string sDesc = "";
                    string[] sSubItems = new string[0];
                    decimal[] dQuantities = new decimal[0];
                    decimal[] dRRP = new decimal[0];

                    tEngine.GetMultiItemInfo(sShopCodeLookingFor, sBarcodes[i], ref sDesc, ref sSubItems, ref dQuantities, ref dRRP);
                    sMainStock[1] = sDesc;
                    decimal dTotal = 0;
                    for (int x = 0; x < dRRP.Length; x++)
                    {
                        dTotal += (dRRP[x] * dQuantities[x]);
                    }
                    sMainStock[2] = dTotal.ToString();
                    sMainStock[5] = "6";
                    iResults[i] = new Item(sMainStock, sStockSta);
                    iResults[i].ParentBarcode = "";                    
                }
            }
            else
            {
                // Do a category search
                string sCategory = "";
                for (int i = 4; i < sSearchTerm.Length; i++)
                {
                    sCategory += sSearchTerm[i].ToString();
                }
                /*
                string[] sCodesInCategory = tEngine.GetCodesOfItemsInCategory(sCategory);
                Array.Resize<Item>(ref iResults, sCodesInCategory.Length);
                for (int i = 0; i < iResults.Length; i++)
                {
                    iResults[i] = new Item(tEngine.GetMainStockInfo(sCodesInCategory[i]), tEngine.GetItemStockStaRecord(sCodesInCategory[i], sShopCodeLookingFor));
                    iResults[i].StockLevel = tEngine.GetItemStockLevel(sCodesInCategory[i]);
                }
                 */
                iResults = tEngine.GetItemsInCategory(sCategory, sShopCodeLookingFor);
            }
            OrderResults();
            CountNumberOfResultsToShow();
        }

        void OrderResults()
        {
            pItems = new ParentItems[0];
            bool[] bAdded = new bool[iResults.Length];
            for (int i = 0; i < bAdded.Length; i++)
                bAdded[i] = false;
            for (int i = 0; i < iResults.Length; i++)
            {
                if (bAdded[i] == false)
                {
                    bool bIsAParent = true;
                    if (iResults[i].ParentBarcode != "")
                    {
                        bIsAParent = false;
                    }
                    if (bIsAParent)
                    {
                        Array.Resize<ParentItems>(ref pItems, pItems.Length + 1);
                        pItems[pItems.Length - 1] = new ParentItems();
                        pItems[pItems.Length - 1].ParentItem = iResults[i];
                        pItems[pItems.Length - 1].sOrder = sOrder;
                        pItems[pItems.Length - 1].ChildItems = new Item[0];
                        // Search for children
                        for (int x = 0; x < iResults.Length; x++)
                        {
                            if (iResults[x].ParentBarcode.Equals(iResults[i].Barcode))
                            {
                                Array.Resize<Item>(ref pItems[pItems.Length - 1].ChildItems, pItems[pItems.Length - 1].ChildItems.Length + 1);
                                pItems[pItems.Length - 1].ChildItems[pItems[pItems.Length - 1].ChildItems.Length - 1] = iResults[x];
                                bAdded[x] = true;
                            }
                        }
                        bAdded[i] = true;
                    }
                }
            }
            Array.Sort(pItems);
            for (int i = 0; i < pItems.Length; i++)
            {
                iItems.Add(pItems[i].ParentItem);
                for (int x = 0; x < pItems[i].ChildItems.Length; x++)
                {
                    iItems.Add(pItems[i].ChildItems[x]);
                }
            }
        }
        List<string> drawnBarcodes;
        void DrawResults(Graphics g)
        {
            drawnBarcodes = new List<string>();
            int nTop = 150;
            int nItemNumber = 0;
            nOfResultsDrawnToScreen = 0;
            nDrawLoc = new int[iResults.Length];
            int nDrawLocPos = 0;
            for (int i = 0; i < pItems.Length && nTop < this.Height - 32; i++)
            {
                bool bChildItemsHaveStock = false;
                for (int x = 0; x < pItems[i].ChildItems.Length; x++)
                {
                    if (pItems[i].ChildItems[x].StockLevel > 0)
                        bChildItemsHaveStock = true;
                }
                if ((bHideNoStockItems && pItems[i].ParentItem.StockLevel > 0) || !bHideNoStockItems || bHideNoStockItems && bChildItemsHaveStock)
                {
                    if (nItemNumber >= nScrolledDown)
                    {
                        DrawItem(pItems[i].ParentItem, 0, nTop, g);
                        nDrawLoc[i] = nTop; //  Changed from nDrawLoc[nDrawLocPos]
                        nTop += 25;
                    }
                    nItemNumber++;
                }
                nDrawLocPos++;
                for (int x = 0; x < pItems[i].ChildItems.Length; x++)
                {
                    if (bHideNoStockItems && pItems[i].ChildItems[x].StockLevel > 0 || !bHideNoStockItems)
                    {
                        if (nItemNumber >= nScrolledDown)
                        {
                            DrawItem(pItems[i].ChildItems[x], 10, nTop, g);
                            nDrawLoc[nDrawLocPos] = nTop;
                            nTop += 25;
                        }
                        nItemNumber++;
                    }
                    nDrawLocPos++;
                }
            }
            sBar.Maximum = nOfSearchResultsBeingDisplayed;
            sBar.LargeChange = nOfResultsDrawnToScreen;
            sBar.Value = nScrolledDown;
        }


        string GetBarcodeFromClickY(int nY)
        {
            if (nY < 150)
                return "";
            int nTop = 150;
            nY -= nTop;
            nY = nY / 25;
            try
            {
                return drawnBarcodes[nY];
            }
            catch
            {
                return "";
            }
        }

        void DrawItem(Item iItem, int nOffSet, int nTop, Graphics g)
        {
            drawnBarcodes.Add(iItem.Barcode);
            Font fDrawFont = new Font(this.Font.FontFamily, 14.0f);
            SolidBrush s = new SolidBrush(Color.Black);
            if (iItem.ParentBarcode != "")
                s = new SolidBrush(Color.FromArgb(255, 128, 0));
            if (iItem.bDiscontinued)
                s = new SolidBrush(Color.Red);
            if (iItem.StockLevel == 0 && !iItem.bDiscontinued && iItem.ParentBarcode == "" && iItem.ItemCategory != 6)
                s = new SolidBrush(Color.Gray);
            else if (iItem.StockLevel == 0 && !iItem.bDiscontinued && iItem.ParentBarcode != "")
                s = new SolidBrush(Color.FromArgb(255, 167, 79));
            else if (iItem.ItemCategory == 6)
                s = new SolidBrush(Color.Blue);
            if (nTop == 150 + (nScrolledDown * -25) + (nCurrentItemSelected * 25) && !bTextBoxHasFocus)
                g.FillRectangle(new SolidBrush(Color.FromArgb(210, 210, 210)), new Rectangle(0, nTop, this.Width, 25));
            g.DrawString(iItem.Barcode, fDrawFont, s, new PointF(nOffSet + nCodeLeft, nTop));
            g.DrawString(iItem.Description, fDrawFont, s, new PointF(nOffSet + nDescLeft, nTop));
            if (iItem.StockLevel != -1024)
                g.DrawString(FormatMoneyForDisplay(iItem.StockLevel), fDrawFont, s, new PointF(nStockLeft + nOffSet, nTop));
            else
                g.DrawString("-", fDrawFont, s, new PointF(nStockLeft + nOffSet, nTop));
            g.DrawString(FormatMoneyForDisplay(iItem.Amount), fDrawFont, s, new PointF(nPriceLeft + nOffSet, nTop));
            g.DrawString(iItem.ItemCategory.ToString(), fDrawFont, s, new PointF(nCategoryLeft + nOffSet, nTop));
            g.DrawString(tEngine.GetCategoryDesc(iItem.CodeCategory.ToString()), fDrawFont, s, new PointF(nCatCodeLeft + nOffSet, nTop));
            if (nTop + 25 < this.Height - 32)
                nOfResultsDrawnToScreen++;
        }

        void CountNumberOfResultsToShow()
        {
            nOfSearchResultsBeingDisplayed = 0;
            for (int i = 0; i < pItems.Length; i++)
            {
                bool bAnyChildrenHaveStock = false;
                for (int x = 0; x < pItems[i].ChildItems.Length; x++)
                {
                    if (pItems[i].ChildItems[x].StockLevel > 0)
                        bAnyChildrenHaveStock = true;
                }
                if (bHideNoStockItems)
                {
                    if (bAnyChildrenHaveStock)
                    {
                        nOfSearchResultsBeingDisplayed++; // Parent item

                        for (int x = 0; x < pItems[i].ChildItems.Length; x++)
                        {
                            if (pItems[i].ChildItems[x].StockLevel > 0)
                                nOfSearchResultsBeingDisplayed++;
                        }
                    }
                    else if (pItems[i].ParentItem.StockLevel > 0)
                        nOfSearchResultsBeingDisplayed++;
                }
                else
                {
                    nOfSearchResultsBeingDisplayed++;
                    nOfSearchResultsBeingDisplayed += pItems[i].ChildItems.Length;
                }
            }
        }

        public string GetItemBarcode()
        {
            return sSelectedBarcode;
        }

        public void CheckForPartialBarcodeFromScanner(string sScannerInput)
        {
            if (sScannerInput == null || sScannerInput == "")
                return;
            bPartialSearchFromScanner = true;
            sSearchTerm = sScannerInput;
            OriginalBarcode = sScannerInput;
            SearchForItems();
            bTextBoxHasFocus = false;
            if (iResults.Length == 0 && !sScannerInput.StartsWith("CAT:"))
                this.Close();
        }
        public bool FoundScannerResults = false;
        public void CheckForPartialBarcodeFromScanner(string sScannerInput, bool bCloseWhenDone)
        {
            bPartialSearchFromScanner = true;
            sSearchTerm = sScannerInput;
            if (sScannerInput == null || bCloseWhenDone == null)
            {
                this.Close();
            }
            else
            {
                OriginalBarcode = sScannerInput;
                SearchForItems();
                bTextBoxHasFocus = false;
                if (iResults.Length == 0 && bCloseWhenDone)
                    this.Close();
                else
                    FoundScannerResults = true;
            }
        }

        bool JumpToItemByKey(Keys e)
        {
            if (bHideNoStockItems)
                return false;
            else if (pItems.Length == 0)
                return false;
            bool bFound = false;
            int nLoc = 0;
            int nFindLoc = 0;
            // Go through each parent item
            for (int i = 0; i < pItems.Length; i++)
            {
                // And check each parent item to see if its currently selected
                if (nLoc == nCurrentItemSelected)
                {
                    // If this parent item is found, then its location in pItems is stored in nFindLoc
                    bFound = true;
                    nFindLoc = i;
                    break;
                }
                nLoc += pItems[i].ChildItems.Length;
                nLoc++;
            }
            if (bFound)
            {
                // Check that we're not already highlighting the last item in the array
                if (pItems.Length <= nFindLoc + 1)
                {
                    // If the user is trying to access the currently hightlighted last parent item starting with the pressed letter, then keep it highlighted and finish
                    if (pItems[nFindLoc].ParentItem.Description.StartsWith(e.ToString()))
                    {
                        nCurrentItemSelected = nFindLoc;
                        nScrolledDown = nCurrentItemSelected;
                        return true;
                    }
                }
            }
            if (!bFound)
                nLoc = 0;
            else
                nLoc = nFindLoc + pItems[nFindLoc].ChildItems.Length + 1;
            if (nFindLoc == pItems.Length)
                nLoc = 0;
            int nToEndAt = nFindLoc - 1;// pItems[nFindLoc].ChildItems.Length - 1;
            if (nToEndAt == -1)
                nToEndAt = pItems.Length -1;
            if (nToEndAt >= pItems.Length)
                nToEndAt = 0;
            for (int i = nFindLoc+1; i != nToEndAt; i++)
            {
                nLoc = 0;
                for (int x=0; x < i; x++)
                {
                    // Include every child
                    nLoc += pItems[x].ChildItems.Length;
                    // And the parent itself
                    nLoc++;
                }

                // INDEX OUT OF BOUNDS EXCEPTION ON THE ROW BELOW WHEN THERE'S ONLY ONE PARENT, AND THE KEY PRESSED IS THE FIRST LETTER OF THE PARENT'S DESCRIPTION
                // TRY `CLUBMAN 62MM' AND PRESS C
                if (pItems[i].ParentItem.Description.StartsWith(e.ToString()) && nLoc != nCurrentItemSelected)
                {
                    nCurrentItemSelected = nLoc;
                    nScrolledDown = nCurrentItemSelected;
                    return true;

                }

                // Cyclic search - checks if we're at the end
                if (i + 1 == pItems.Length)
                {
                    nLoc = 0;
                    i = -1;
                }
            }

            return false;
        }

        public void SetSearchTerm(string sSearchT)
        {
            sSearchTerm = sSearchT;
            SearchForItems();
        }
    }
}
