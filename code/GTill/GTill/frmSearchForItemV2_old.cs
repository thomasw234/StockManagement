using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using TillEngine;

namespace GTill
{
    struct ParentItems
    {
        public Item ParentItem;
        public Item[] ChildItems;
    }

    class frmSearchForItemV2 : Form
    {
        TillEngine.TillEngine tEngine;
        TillEngine.Item[] iResults;
        string sSearchTerm = "";

        int nCodeLeft = 12;
        int nDescLeft = 202;
        int nPriceLeft = 623;
        int nStockLeft = 553;
        int nCategoryLeft = 728;
        int nCatCodeLeft = 787;

        int nOfSearchResultsBeingDisplayed = 0;
        int nScrolledDown = 0;
        int nCurrentItemSelected = 0;
        int nOfResultsDrawnToScreen = 0;

        ParentItems[] pItems;
        Timer tmrCursorBlink;

        bool bDrawnCursor = false;
        bool bTextBoxHasFocus = true;
        bool bPartialSearchFromScanner = false;
        bool bHideNoStockItems = false;

        string sSelectedBarcode = "NONE_SELECTED";
        public string OriginalBarcode = "";


        frmCategorySelect fcSelect;


        public frmSearchForItemV2(ref TillEngine.TillEngine te)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = GTill.Properties.Settings.Default.cFrmForeColour;
            this.ForeColor = GTill.Properties.Settings.Default.cFrmBackColour;
            this.Size = new Size(1024, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;
            iResults = new TillEngine.Item[0];
            tEngine = te;
            pItems = new ParentItems[0];
            this.Paint += new PaintEventHandler(frmLookupTransactionsV2_Paint);
            this.KeyDown += new KeyEventHandler(frmLookupTransactionsV2_KeyDown);
            this.FormClosed += new FormClosedEventHandler(frmSearchForItemV2_FormClosed);
            tmrCursorBlink = new Timer();
            tmrCursorBlink.Interval = 500;
            tmrCursorBlink.Tick += new EventHandler(tmrCursorBlink_Tick);
            tmrCursorBlink.Enabled = true;
        }

        void frmSearchForItemV2_FormClosed(object sender, FormClosedEventArgs e)
        {
            tmrCursorBlink.Enabled = false;
            tmrCursorBlink.Dispose();
        }

        void tmrCursorBlink_Tick(object sender, EventArgs e)
        {
            Font fFont = new Font(GTill.Properties.Settings.Default.sFontName, 16.0f);
            Point pTop = new Point(10 + Convert.ToInt32(this.CreateGraphics().MeasureString("Search Term : " + sSearchTerm, fFont, this.Width, StringFormat.GenericTypographic).Width), 30);
            Point pBottom = new Point(10 + Convert.ToInt32(this.CreateGraphics().MeasureString("Search Term : " + sSearchTerm, fFont, this.Width, StringFormat.GenericTypographic).Width), 55);
            if (sSearchTerm.Length > 0 && sSearchTerm[sSearchTerm.Length - 1] == ' ')
            {
                pTop.X += 10;
                pBottom.X += 10;
            }
            if (!bDrawnCursor && bTextBoxHasFocus)
            {
                this.CreateGraphics().DrawLine(new Pen(GTill.Properties.Settings.Default.cFrmBackColour), pTop, pBottom);
            }
            else if (bTextBoxHasFocus && bDrawnCursor)
            {
                this.CreateGraphics().DrawLine(new Pen(Color.FromArgb(210, 210, 210)), pTop, pBottom);
            }
            else
            {
                this.CreateGraphics().DrawLine(new Pen(GTill.Properties.Settings.Default.cFrmForeColour), pTop, pBottom);
            }
            bDrawnCursor = !bDrawnCursor;
        }

        void frmLookupTransactionsV2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue >= 65 && e.KeyValue <= 90 && !e.Shift)
            {
                sSearchTerm += e.KeyCode.ToString();
                bTextBoxHasFocus = true;
            }
            else if (e.KeyValue >= 47 && e.KeyValue <= 57)
            {
                sSearchTerm += (e.KeyValue - 48).ToString();
                bTextBoxHasFocus = true;
            }
            else if (e.KeyValue >= 96 && e.KeyValue <= 106)
            {
                sSearchTerm += (e.KeyValue - 96).ToString();
                bTextBoxHasFocus = true;
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
                    this.CreateGraphics().DrawString("Searching by code, please wait.", new Font(GTill.Properties.Settings.Default.sFontName, 12.0f), new SolidBrush(Color.Red), new PointF(10, 107));
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
                        // Prevent it from going to the top when going too far down
                        //bTextBoxHasFocus = true;
                    }
                    if (nCurrentItemSelected > nScrolledDown + nOfResultsDrawnToScreen && nCurrentItemSelected < nOfSearchResultsBeingDisplayed)
                    {
                        nScrolledDown++;
                    }
                }
            }
            else if (e.KeyValue == 38)
            {
                // Move up
                if (nCurrentItemSelected > 0)
                {
                    nCurrentItemSelected -= 1;
                }
                else if (nCurrentItemSelected == 0 && bTextBoxHasFocus)
                {
                    /*bTextBoxHasFocus = false;
                    nCurrentItemSelected = nOfSearchResultsBeingDisplayed - 1;
                    nScrolledDown = nOfSearchResultsBeingDisplayed - nOfResultsDrawnToScreen - 1;
                    if (nScrolledDown < 0)
                        nScrolledDown = 0;*/
                    // Prevent it from going to the bottom when going too far up
                }
                else if (nCurrentItemSelected == 0 && !bTextBoxHasFocus)
                {
                    bTextBoxHasFocus = true;
                }
                if (nCurrentItemSelected < nScrolledDown)
                    nScrolledDown -= 1;
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
            else if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus)
            {
                if (bTextBoxHasFocus)
                {
                    sSearchTerm += "-";
                    bTextBoxHasFocus = true;
                }
            }
            else if (e.KeyCode == Keys.Enter)
            {
                sSelectedBarcode = getSelectedBarcode();
                this.Close();
            }
            else if (e.Shift && e.KeyCode == Keys.P)
            {
                // Print out a barcode
                string sCode = getSelectedBarcode();
                if (sCode != "")
                {
                    tEngine.PrintBarcode(sCode, false);
                }
            }
            else if (e.KeyCode == Keys.F5)
            {
                fcSelect = new frmCategorySelect(ref tEngine);
                fcSelect.ShowDialog();
                if (fcSelect.SelectedCategory != "NONE_SELECTED")
                {
                    sSearchTerm = "CAT:" + fcSelect.SelectedCategory;
                    SearchForItems();
                    if (nOfSearchResultsBeingDisplayed > 0)
                    {
                        nScrolledDown = 0;
                        nCurrentItemSelected = 0;
                        bTextBoxHasFocus = false;
                    }
                }
                fcSelect.Dispose();
            }
            else if (e.KeyCode == Keys.F6)
            {
                this.Hide();
                frmSearchForItem fsi = new frmSearchForItem(ref tEngine);
                fsi.ShowDialog();
                sSelectedBarcode = fsi.GetItemBarcode();
                this.Close();
            }
            else if (e.KeyCode == Keys.Oem1)
            {
                sSearchTerm += ":";
            }
            else if (e.KeyCode == Keys.PageDown)
            {
                nScrolledDown += nOfResultsDrawnToScreen;
                if (nScrolledDown + nOfResultsDrawnToScreen >= nOfSearchResultsBeingDisplayed)
                    nScrolledDown = nOfSearchResultsBeingDisplayed - nOfResultsDrawnToScreen;
                nCurrentItemSelected = nScrolledDown;

            }
            else if (e.Shift)
            {
                // Do nothing - wait for the next key
                ;
            }
            else
            {
                bTextBoxHasFocus = true;
            }
            this.Refresh();
        }

        private string getSelectedBarcode()
        {
            int nSelectedItem = -1;
            string selectedCode = "";
            for (int i = 0; i < pItems.Length; i++)
            {
                if (bHideNoStockItems && pItems[i].ParentItem.StockLevel > 0 || !bHideNoStockItems)
                {
                    nSelectedItem++;
                    if (nSelectedItem == nCurrentItemSelected)
                    {
                        selectedCode = pItems[i].ParentItem.Barcode;
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
                            selectedCode = pItems[i].ChildItems[x].Barcode;
                            break;
                        }
                    }
                }

            }
            return selectedCode;
        }

        void frmLookupTransactionsV2_Paint(object sender, PaintEventArgs e)
        {
            Font fDrawingFont = new Font(GTill.Properties.Settings.Default.sFontName, 16.0f);
            if (bTextBoxHasFocus)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(210, 210, 210)), new Rectangle(135, 30, this.Width - 150, 25));
            }
            e.Graphics.DrawString("Lookup Item", fDrawingFont, new SolidBrush(GTill.Properties.Settings.Default.cFrmBackColour), new PointF((this.Width / 2) - (e.Graphics.MeasureString("Lookup Item", fDrawingFont).Width / 2), 10));
            e.Graphics.DrawString("Search Term : " + sSearchTerm, fDrawingFont, new SolidBrush(GTill.Properties.Settings.Default.cFrmBackColour), new PointF(10, 30));
            DrawResults(e.Graphics);
            SolidBrush s = new SolidBrush(GTill.Properties.Settings.Default.cFrmBackColour);
            fDrawingFont = new Font(GTill.Properties.Settings.Default.sFontName, 14.0f);
            if (!bPartialSearchFromScanner)
            {
                e.Graphics.DrawString("Enter a partial barcode or description of the item that you are searching for, then press Enter.", fDrawingFont, s, new PointF(10.0f, 60.0f));
                e.Graphics.DrawString("Press TAB to hide / show stock items that are not in stock. Press F5 to choose a category to show.", fDrawingFont, s, new PointF(10, 85));
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(GTill.Properties.Settings.Default.cFrmBackColour), new Rectangle(0, 60, this.Width, 50));
                e.Graphics.DrawString("The barcode that you entered wasn't recognised. The search results below may be what you were trying to enter.", fDrawingFont, new SolidBrush(GTill.Properties.Settings.Default.cFrmForeColour), new PointF(10, 75));

            }
            e.Graphics.DrawString("Code", fDrawingFont, s, new PointF((float)nCodeLeft, 125.0f));
            e.Graphics.DrawString("Description", fDrawingFont, s, new PointF((float)nDescLeft, 125.0f));
            e.Graphics.DrawString("Stock", fDrawingFont, s, new PointF((float)nStockLeft, 105.0f));
            e.Graphics.DrawString("Level", fDrawingFont, s, new PointF((float)nStockLeft, 125.0f));
            e.Graphics.DrawString("Price", fDrawingFont, s, new PointF((float)nPriceLeft, 125.0f));
            e.Graphics.DrawString("Item", fDrawingFont, s, new PointF((float)nCategoryLeft, 105.0f));
            e.Graphics.DrawString("Type", fDrawingFont, s, new PointF((float)nCategoryLeft, 125.0f));
            e.Graphics.DrawString("Category", fDrawingFont, s, new PointF((float)nCatCodeLeft, 125.0f));
        }

        void SearchForItems()
        {
            Graphics g = this.CreateGraphics();
            nScrolledDown = 0;
            nCurrentItemSelected = 0;
            iResults = new TillEngine.Item[0];
            int nOfItems = 0;
            if (!sSearchTerm.StartsWith("CAT:"))
            {
                string[,] sSearchResults = tEngine.sGetAccordingToPartialBarcode(sSearchTerm, ref nOfItems);
                Array.Resize<TillEngine.Item>(ref iResults, nOfItems);
                string[] sRecordContents;
                for (int i = 0; i < nOfItems; i++)
                {
                    if ((i % 10) == 0)
                    {
                        g.FillRectangle(new SolidBrush(GTill.Properties.Settings.Default.cFrmForeColour), new Rectangle(10, 107, this.Width, 20));
                        g.DrawString("Adding search by code result " + i.ToString() + " of " + nOfItems.ToString(), new Font(GTill.Properties.Settings.Default.sFontName, 12.0f), new SolidBrush(Color.Red), new PointF(10, 107));
                    }
                    sRecordContents = new string[10];
                    for (int x = 0; x < sRecordContents.Length; x++)
                    {
                        sRecordContents[x] = sSearchResults[i, x].TrimEnd(' ');
                    }
                    iResults[i] = new TillEngine.Item(sRecordContents);
                    iResults[i].StockLevel = tEngine.GetItemStockLevel(iResults[i].Barcode);
                    if (sRecordContents[7].TrimEnd(' ').Length > 0)
                    {
                        // Has a parent item
                        iResults[i].ParentBarcode = sRecordContents[7].TrimEnd(' ');
                    }
                    else
                    {
                        iResults[i].ParentBarcode = "";
                    }

                }
                g.FillRectangle(new SolidBrush(GTill.Properties.Settings.Default.cFrmForeColour), new Rectangle(10, 107, this.Width, 20));
                g.DrawString("Searching by description, please wait.", new Font(GTill.Properties.Settings.Default.sFontName, 12.0f), new SolidBrush(Color.Red), new PointF(10, 107));
                sSearchResults = tEngine.sGetAccordingToPartialDescription(sSearchTerm, ref nOfItems);
                int nPrevLength = iResults.Length;
                bool[] bItemAlreadyExists = new bool[nOfItems];
                int nOfItemsAlreadyExist = 0;
                g.FillRectangle(new SolidBrush(GTill.Properties.Settings.Default.cFrmForeColour), new Rectangle(10, 107, this.Width, 20));
                g.DrawString("Removing duplicate results (" + nOfItemsAlreadyExist.ToString() + " found so far), please wait.", new Font(GTill.Properties.Settings.Default.sFontName, 12.0f), new SolidBrush(Color.Red), new PointF(10, 107));
                for (int i = 0; i < nOfItems; i++)
                {
                    bItemAlreadyExists[i] = false;
                    for (int x = 0; x < iResults.Length; x++)
                    {
                        if (iResults[x].Barcode.TrimEnd(' ') == sSearchResults[i, 0].TrimEnd(' ') && bItemAlreadyExists[i] == false)
                        {
                            bItemAlreadyExists[i] = true;
                            nOfItemsAlreadyExist++;
                            break;
                        }
                    }
                }
                Array.Resize<TillEngine.Item>(ref iResults, iResults.Length + nOfItems - nOfItemsAlreadyExist);
                int nOfItemsSkipped = 0;
                for (int i = nPrevLength; i < iResults.Length + nOfItemsAlreadyExist; i++)
                {
                    if (!bItemAlreadyExists[i - nPrevLength])
                    {
                        if ((i % 10) == 0)
                        {
                            g.FillRectangle(new SolidBrush(GTill.Properties.Settings.Default.cFrmForeColour), new Rectangle(10, 107, this.Width, 20));
                            g.DrawString("Adding search by description result " + (i - nPrevLength).ToString() + " of " + (nOfItems - nOfItemsAlreadyExist).ToString() + ", please wait.", new Font(GTill.Properties.Settings.Default.sFontName, 12.0f), new SolidBrush(Color.Red), new PointF(10, 107));
                        }
                        sRecordContents = new string[10];
                        for (int x = 0; x < sRecordContents.Length; x++)
                        {
                            sRecordContents[x] = sSearchResults[i - nPrevLength - nOfItemsSkipped, x];
                        }
                        iResults[i - nOfItemsSkipped] = new TillEngine.Item(sRecordContents);
                        iResults[i - nOfItemsSkipped].StockLevel = tEngine.GetItemStockLevel(iResults[i - nOfItemsSkipped].Barcode);
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
                if ((i & 10) == 0)
                {
                    g.FillRectangle(new SolidBrush(GTill.Properties.Settings.Default.cFrmForeColour), new Rectangle(10, 107, this.Width, 20));
                    g.DrawString("Searching for parent and child items of result number " + i.ToString() + " of " + iResults.Length.ToString() + ", please wait.", new Font(GTill.Properties.Settings.Default.sFontName, 12.0f), new SolidBrush(Color.Red), new PointF(10, 107));
                }
                if (iResults[i].ParentBarcode != "")
                {
                    bool bAlreadyExists = false;
                    for (int x = 0; x < iResults.Length; x++)
                    {
                        if (iResults[x].Barcode.TrimEnd(' ') == iResults[i].ParentBarcode.TrimEnd(' '))
                            bAlreadyExists = true;
                    }
                    if (!bAlreadyExists)
                    {
                        Array.Resize<TillEngine.Item>(ref iResults, iResults.Length + 1);
                        iResults[iResults.Length - 1] = new TillEngine.Item(tEngine.GetItemRecordContents(iResults[i].ParentBarcode));
                        iResults[iResults.Length - 1].StockLevel = tEngine.GetItemStockLevel(iResults[iResults.Length - 1].Barcode);
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
                                Array.Resize<TillEngine.Item>(ref iResults, iResults.Length + 1);
                                iResults[iResults.Length - 1] = new TillEngine.Item(tEngine.GetItemRecordContents(sChildren[x].TrimEnd(' ')));
                            }
                        }
                    }
                }
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
                string[] sCodesInCategory = tEngine.GetCodesOfItemsInCategory(sCategory);
                Array.Resize<Item>(ref iResults, sCodesInCategory.Length);
                for (int i = 0; i < iResults.Length; i++)
                {
                    iResults[i] = new Item(tEngine.GetItemRecordContents(sCodesInCategory[i]));
                    iResults[i].StockLevel = tEngine.GetItemStockLevel(sCodesInCategory[i]);
                }
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
                        pItems[pItems.Length - 1].ChildItems = new Item[0];
                        // Search for children
                        for (int x = 0; x < iResults.Length; x++)
                        {
                            if (iResults[x].ParentBarcode.TrimEnd(' ') == iResults[i].Barcode.TrimEnd(' '))
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
        }

        void DrawResults(Graphics g)
        {
            int nTop = 150;
            int nItemNumber = 0;
            nOfResultsDrawnToScreen = 0;
            for (int i = 0; i < pItems.Length && nTop < this.Height; i++)
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
                        nTop += 25;
                    }
                    nItemNumber++;
                }
                for (int x = 0; x < pItems[i].ChildItems.Length; x++)
                {
                    if (bHideNoStockItems && pItems[i].ChildItems[x].StockLevel > 0 || !bHideNoStockItems)
                    {
                        if (nItemNumber >= nScrolledDown)
                        {
                            DrawItem(pItems[i].ChildItems[x], 10, nTop, g);
                            nTop += 25;
                        }
                        nItemNumber++;
                    }
                }
            }
        }
        

        void DrawItem(Item iItem, int nOffSet, int nTop, Graphics g)
        {
            Font fDrawFont = new Font(GTill.Properties.Settings.Default.sFontName, 14.0f);
            SolidBrush s = new SolidBrush(GTill.Properties.Settings.Default.cFrmBackColour);
            if (iItem.ParentBarcode != "")
                s = new SolidBrush(Color.FromArgb(255, 128, 0));
            if (iItem.bDiscontinued)
                s = new SolidBrush(Color.Red);
            if (iItem.StockLevel == 0 && !iItem.bDiscontinued && iItem.ParentBarcode == "")
                s = new SolidBrush(Color.Gray);
            else if (iItem.StockLevel == 0 && !iItem.bDiscontinued && iItem.ParentBarcode != "")
                s = new SolidBrush(Color.FromArgb(255, 167, 79));
            if (nTop == 150 + (nScrolledDown * -25) + (nCurrentItemSelected * 25) && !bTextBoxHasFocus)
                g.FillRectangle(new SolidBrush(Color.FromArgb(210, 210, 210)), new Rectangle(0, nTop, this.Width, 25));
            g.DrawString(iItem.Barcode, fDrawFont, s, new PointF(nOffSet + nCodeLeft, nTop));
            g.DrawString(iItem.Description, fDrawFont, s, new PointF(nOffSet + nDescLeft, nTop));
            if (iItem.StockLevel != -1024)
                g.DrawString(iItem.StockLevel.ToString(), fDrawFont, s, new PointF(nStockLeft + nOffSet, nTop));
            else
                g.DrawString("-", fDrawFont, s, new PointF(nStockLeft + nOffSet, nTop));
            g.DrawString(TillEngine.TillEngine.FormatMoneyForDisplay(iItem.Amount), fDrawFont, s, new PointF(nPriceLeft + nOffSet, nTop));
            g.DrawString(iItem.ItemCategory.ToString(), fDrawFont, s, new PointF(nCategoryLeft + nOffSet, nTop));
            g.DrawString(tEngine.GetCategoryDescription(iItem.CodeCategory.ToString()), fDrawFont, s, new PointF(nCatCodeLeft + nOffSet, nTop));
            if (nTop + 25 < this.Height)
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
            bPartialSearchFromScanner = true;
            sSearchTerm = sScannerInput;
            OriginalBarcode = sScannerInput;
            SearchForItems();
            bTextBoxHasFocus = false;
            if (iResults.Length == 0)
                this.Close();
        }
    }
}
