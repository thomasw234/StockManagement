using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmAddMultiItem : ScalableForm
    {
        public class STextBox : TextBox
        {
            public int nRowNum;
        }
        public class SLabel : Label
        {
            public int nRowNum;
        }
        public string ShopCode
        {
            set
            {
                InputTextBox("SHOPCODE").Text = value;
                InputTextBox("BARCODE").Focus();
            }
        }
        public string Barcode
        {
            set
            {
                InputTextBox("BARCODEINPUT").Text = value;
                InputTextBox("BARCODEINPUT").Focus();
            }
        }
        public string Description
        {
            set
            {
                InputTextBox("DESCINPUT").Text = value;
                InputTextBox("DESCINPUT").Focus();
            }
        }
        public bool Editing
        {
            set
            {
                if (value)
                {
                    this.Text = "Multi-Item Item Editing";
                }
                else
                {
                    this.Text = "Multi-Item Item Entry";
                }
            }
        }
        StockEngine sEngine;

        SLabel[] lblLineNo;
        STextBox[] tbBarcode;
        SLabel[] lblDesc;
        STextBox[] tbQty;
        SLabel[] lblRRPEach;
        SLabel[] lblFullMargin;
        STextBox[] tbMarginOrRRP;
        STextBox[] tbTargetMargin;
        SLabel[] lblFinalLinePrice;
        SLabel[] lblFinalLineMargin;
        bool bShopCodeOpen = false;

        public frmAddMultiItem(ref StockEngine se, string sBarcode)
        {
            sEngine = se;
            AllowScaling = false;
            this.WindowState = FormWindowState.Maximized;
            this.Paint += new PaintEventHandler(frmAddMultiItem_Paint);

            AddInputControl("SHOPCODEINPUT", "Shop Code :", new Point(10, 10), 200);
            InputTextBox("SHOPCODEINPUT").GotFocus += new EventHandler(ShopCodeGotFocus);
            InputTextBox("SHOPCODEINPUT").MaxCharCount = 2;
            AddInputControl("BARCODEINPUT", "Barcode :", new Point(10, BelowLastControl), 300);
            InputTextBox("BARCODEINPUT").MaxCharCount = 13;
            InputTextBox("BARCODEINPUT").KeyDown += new KeyEventHandler(BarcodeInputKeyDown);
            AddInputControl("DESCINPUT", "Description :", new Point(10, BelowLastControl), 400);
            InputTextBox("DESCINPUT").MaxCharCount = 30;
            InputTextBox("DESCINPUT").KeyDown += new KeyEventHandler(DescInputKeyDown);
            int nTop = BelowLastControl;
            AddMessage("BARCODE", "Barcode", new Point(40, nTop));
            AddMessage("DESC", "Description", new Point(150, nTop));
            AddMessage("QTY", "Quantity", new Point(400, nTop));
            AddMessage("MORR", "M/R", new Point(460, nTop));
            AddMessage("RRP", "RRP", new Point(520, nTop));
            AddMessage("FMARGIN", "RRP Margin", new Point(580, nTop));
            AddMessage("T-MARGIN", "Target Margin", new Point(680, nTop));
            AddMessage("FPRICE", "Price/Item", new Point(800, MessageLabel("BARCODE").Top));
            AddMessage("FMARGIN2", "Final Margin", new Point(900, MessageLabel("BARCODE").Top));

            lblLineNo = new SLabel[0];
            tbBarcode = new STextBox[0];
            lblDesc = new SLabel[0];
            tbQty = new STextBox[0];
            tbMarginOrRRP = new STextBox[0];
            lblRRPEach = new SLabel[0];
            lblFullMargin = new SLabel[0];
            tbTargetMargin = new STextBox[0];
            lblFinalLinePrice = new SLabel[0];
            lblFinalLineMargin = new SLabel[0];

            this.Text = "Multi-Item Item Entry";
            InputTextBox("BARCODEINPUT").Text = sBarcode;
        }
        public frmAddMultiItem(ref StockEngine se)
        {
            sEngine = se;
            AllowScaling = false;
            this.WindowState = FormWindowState.Maximized;
            this.Paint += new PaintEventHandler(frmAddMultiItem_Paint);

            AddInputControl("SHOPCODEINPUT", "Shop Code :", new Point(10, 10), 200);
            InputTextBox("SHOPCODEINPUT").GotFocus += new EventHandler(ShopCodeGotFocus);
            InputTextBox("SHOPCODEINPUT").MaxCharCount = 2;
            AddInputControl("BARCODEINPUT", "Barcode :", new Point(10, BelowLastControl), 300);
            InputTextBox("BARCODEINPUT").MaxCharCount = 13;
            InputTextBox("BARCODEINPUT").KeyDown += new KeyEventHandler(BarcodeInputKeyDown);
            AddInputControl("DESCINPUT", "Description :", new Point(10, BelowLastControl), 400);
            InputTextBox("DESCINPUT").MaxCharCount = 30;
            InputTextBox("DESCINPUT").KeyDown += new KeyEventHandler(DescInputKeyDown);
            int nTop = BelowLastControl;
            AddMessage("BARCODE", "Barcode", new Point(40, nTop));
            AddMessage("DESC", "Description", new Point(150, nTop));
            AddMessage("QTY", "Quantity", new Point(400, nTop));
            AddMessage("MORR", "M/R", new Point(460, nTop));
            AddMessage("RRP", "RRP", new Point(520, nTop));
            AddMessage("FMARGIN", "RRP Margin", new Point(580, nTop));
            AddMessage("T-MARGIN", "Target Margin", new Point(680, nTop));
            AddMessage("FPRICE", "Price/Item", new Point(800, MessageLabel("BARCODE").Top));
            AddMessage("FMARGIN2", "Final Margin", new Point(900, MessageLabel("BARCODE").Top));

            lblLineNo = new SLabel[0];
            tbBarcode = new STextBox[0];
            lblDesc = new SLabel[0];
            tbQty = new STextBox[0];
            tbMarginOrRRP = new STextBox[0];
            lblRRPEach = new SLabel[0];
            lblFullMargin = new SLabel[0];
            tbTargetMargin = new STextBox[0];
            lblFinalLinePrice = new SLabel[0];
            lblFinalLineMargin = new SLabel[0];

            this.Text = "Multi-Item Item Entry";
        }

        void BarcodeInputKeyDown(object sender, KeyEventArgs e)
        {
            // Check to see if item exists as a single product
            if (e.KeyCode == Keys.Enter)
            {
                if (sEngine.GetItemStockStaRecord(InputTextBox("BARCODEINPUT").Text, InputTextBox("SHOPCODEINPUT").Text).Length > 1)
                {
                    if (MessageBox.Show("Item already exists as a single product! Edit that instead?", "Already Exists!", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        frmAddEditItem faei = new frmAddEditItem(ref sEngine);
                        faei.EditingBarcode = InputTextBox("BARCODEINPUT").Text;
                        faei.ShowDialog();
                        this.Close();
                    }
                    else
                    {
                        this.Close();
                    }
                }
                // Check to see if it exists as a multi-item item
                if (sEngine.DoesMultiItemExist(InputTextBox("BARCODEINPUT").Text, InputTextBox("SHOPCODEINPUT").Text))
                {
                    this.Text = "Multi-Item Item Editing";
                    string[] sBarcodes = new string[0];
                    decimal[] dQuantities = new decimal[0];
                    decimal[] dRRP = new decimal[0];
                    string sDesc = "";
                    sEngine.GetMultiItemInfo(InputTextBox("SHOPCODEINPUT").Text, InputTextBox("BARCODEINPUT").Text, ref sDesc, ref sBarcodes, ref dQuantities, ref dRRP);
                    InputTextBox("DESCINPUT").Text = sDesc;
                    for (int i = 0; i < sBarcodes.Length; i++)
                    {
                        AddRow();
                        tbBarcode[i].Text = sBarcodes[i];
                        DisplayItemInfo(i);
                        tbQty[i].Text = FormatMoneyForDisplay(dQuantities[i]);
                        tbMarginOrRRP[i].Text = "R";
                        tbTargetMargin[i].Text = FormatMoneyForDisplay(dRRP[i]);
                    }
                    InputTextBox("DESCINPUT").Focus();
                    InputTextBox("DESCINPUT").SelectionStart = sDesc.Length;
                }
            }
            else if (e.KeyCode == Keys.F5)
            {
                frmSearchForItemV2 fsfi = new frmSearchForItemV2(ref sEngine);
                fsfi.SetSearchTerm("TYPE:6");
                fsfi.ShowDialog();
                if (fsfi.GetItemBarcode() != "NONE_SELECTED")
                {
                    InputTextBox("BARCODEINPUT").Text = fsfi.GetItemBarcode();
                    SendKeys.Send("{ENTER}");
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                if (MessageBox.Show("Would you like to quit, discarding any changes?", "Quit?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    this.Close();
                }
            }
        }

        void frmAddMultiItem_Paint(object sender, PaintEventArgs e)
        {
            if (tbBarcode.Length > 0)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.Gray), new Rectangle(lblLineNo[0].Left - 2, tbBarcode[0].Top - 2, (lblFinalLineMargin[0].Left + lblFinalLineMargin[0].Width + 2) - (lblLineNo[0].Left - 2), ((tbBarcode[0].Height + 5) * tbBarcode.Length)));
            }
        }

        void ShopCodeGotFocus(object sender, EventArgs e)
        {
            if (InputTextBox("SHOPCODEINPUT").Text.Length == 0 && !bShopCodeOpen)
            {
                frmListOfShops flos = new frmListOfShops(ref sEngine);
                bShopCodeOpen = true;
                while (flos.SelectedShopCode == "$NONE")
                {
                    flos.ShowDialog();
                }
                InputTextBox("SHOPCODEINPUT").Text = flos.SelectedShopCode;
                InputTextBox("BARCODEINPUT").Focus();
                bShopCodeOpen = false;
            }
        }

        void DescInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (tbBarcode.Length == 0)
                {
                    AddRow();
                }
                tbBarcode[0].Focus();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                if (MessageBox.Show("Would you like to quit, discarding any changes?", "Quit?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    this.Close();
                }
            }
        }

        public void AddRow()
        {
            Array.Resize<STextBox>(ref tbBarcode, tbBarcode.Length + 1);
            int i = tbBarcode.Length - 1;
            tbBarcode[i] = new STextBox();
            if (i == 0)
            {
                tbBarcode[i].Location = new Point(MessageLabel("BARCODE").Left, MessageLabel("BARCODE").Top + MessageLabel("BARCODE").Height + 10);
            }
            else
            {
                tbBarcode[i].Location = new Point(MessageLabel("BARCODE").Left, tbBarcode[i - 1].Top + tbBarcode[i - 1].Height + 5);
            }
            tbBarcode[i].Width = MessageLabel("DESC").Left - MessageLabel("BARCODE").Left - 2;
            tbBarcode[i].BorderStyle = BorderStyle.None;
            tbBarcode[i].KeyDown += new KeyEventHandler(BarcodeKeyDown);
            tbBarcode[i].GotFocus += new EventHandler(BarcodeGotFocus);
            tbBarcode[i].nRowNum = i;
            this.Controls.Add(tbBarcode[i]);
            Array.Resize<SLabel>(ref lblLineNo, lblLineNo.Length + 1);
            lblLineNo[i] = new SLabel();
            lblLineNo[i].Location = new Point(10, tbBarcode[i].Top);
            lblLineNo[i].Width = 20;
            lblLineNo[i].BackColor = Color.Transparent;
            lblLineNo[i].ForeColor = Color.White;
            lblLineNo[i].nRowNum = i;
            lblLineNo[i].Text = (i + 1).ToString();
            this.Controls.Add(lblLineNo[i]);
            Array.Resize<SLabel>(ref lblDesc, lblDesc.Length + 1);
            lblDesc[i] = new SLabel();
            lblDesc[i].Location = new Point(MessageLabel("DESC").Left, tbBarcode[i].Top);
            lblDesc[i].Width = MessageLabel("QTY").Left - MessageLabel("DESC").Left - 2;
            lblDesc[i].BackColor = Color.Transparent;
            lblDesc[i].ForeColor = Color.White;
            lblDesc[i].nRowNum = i;
            this.Controls.Add(lblDesc[i]);
            Array.Resize<STextBox>(ref tbQty, tbQty.Length + 1);
            tbQty[i] = new STextBox();
            tbQty[i].Location = new Point(MessageLabel("QTY").Left, tbBarcode[i].Top);
            tbQty[i].Width = MessageLabel("MORR").Left - MessageLabel("QTY").Left - 2;
            tbQty[i].BorderStyle = BorderStyle.None;
            tbQty[i].KeyDown += new KeyEventHandler(QtyKeyDown);
            tbQty[i].GotFocus += new EventHandler(QtyGotFocus);
            tbQty[i].nRowNum = i;
            this.Controls.Add(tbQty[i]);
            Array.Resize<SLabel>(ref lblRRPEach, lblRRPEach.Length + 1);
            lblRRPEach[i] = new SLabel();
            lblRRPEach[i].Location = new Point(MessageLabel("RRP").Left, tbBarcode[i].Top);
            lblRRPEach[i].Width = MessageLabel("FMARGIN").Left - MessageLabel("RRP").Left - 2;
            lblRRPEach[i].BackColor = Color.Transparent;
            lblRRPEach[i].ForeColor = Color.White;
            lblRRPEach[i].BorderStyle = BorderStyle.None;
            lblRRPEach[i].nRowNum = i;
            this.Controls.Add(lblRRPEach[i]);
            Array.Resize<SLabel>(ref lblFullMargin, lblFullMargin.Length +1);
            lblFullMargin[i] = new SLabel();
            lblFullMargin[i].Location = new Point(MessageLabel("FMARGIN").Left, tbBarcode[i].Top);
            lblFullMargin[i].Width = MessageLabel("T-MARGIN").Left - MessageLabel("FMARGIN").Left - 2;
            lblFullMargin[i].BorderStyle = BorderStyle.None;
            lblFullMargin[i].BackColor = Color.Transparent;
            lblFullMargin[i].ForeColor = Color.White;
            lblFullMargin[i].nRowNum = i;
            this.Controls.Add(lblFullMargin[i]);
            Array.Resize<STextBox>(ref tbMarginOrRRP, tbMarginOrRRP.Length + 1);
            tbMarginOrRRP[i] = new STextBox();
            tbMarginOrRRP[i].Location = new Point(MessageLabel("MORR").Left, tbBarcode[i].Top);
            tbMarginOrRRP[i].Width = 50;
            tbMarginOrRRP[i].BorderStyle = BorderStyle.None;
            tbMarginOrRRP[i].KeyDown += new KeyEventHandler(MarginOrRRPKeyDown);
            tbMarginOrRRP[i].nRowNum = i;
            this.Controls.Add(tbMarginOrRRP[i]);
            Array.Resize<STextBox>(ref tbTargetMargin, tbTargetMargin.Length + 1);
            tbTargetMargin[i] = new STextBox();
            tbTargetMargin[i].Location = new Point(MessageLabel("T-MARGIN").Left, tbBarcode[i].Top);
            tbTargetMargin[i].Width = 100;
            tbTargetMargin[i].BorderStyle = BorderStyle.None;
            tbTargetMargin[i].KeyDown += new KeyEventHandler(TargetMarginKeyDown);
            tbTargetMargin[i].nRowNum = i;
            this.Controls.Add(tbTargetMargin[i]);
            Array.Resize<SLabel>(ref lblFinalLinePrice, lblFinalLinePrice.Length + 1);
            lblFinalLinePrice[i] = new SLabel();
            lblFinalLinePrice[i].Location = new Point(800, tbBarcode[i].Top);
            lblFinalLinePrice[i].BackColor = Color.Transparent;
            lblFinalLinePrice[i].ForeColor = Color.White;
            lblFinalLinePrice[i].Width = 100;
            lblFinalLinePrice[i].nRowNum = i;
            this.Controls.Add(lblFinalLinePrice[i]);
            Array.Resize<SLabel>(ref lblFinalLineMargin, lblFinalLineMargin.Length + 1);
            lblFinalLineMargin[i] = new SLabel();
            lblFinalLineMargin[i].Location = new Point(900, tbBarcode[i].Top);
            lblFinalLineMargin[i].BackColor = Color.Transparent;
            lblFinalLineMargin[i].ForeColor = Color.White;
            lblFinalLineMargin[i].Width = 100;
            lblFinalLineMargin[i].nRowNum = i;
            this.Controls.Add(lblFinalLineMargin[i]);
            this.Refresh();
        }

        void MarginOrRRPKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                MessageBox.Show("Enter M if you want to enter the target margin for this item in the next box, or R if you want to enter a fixed RRP (per item!) in the next box.");
            }
            else if (e.KeyCode == Keys.Enter)
            {
                tbTargetMargin[((STextBox)sender).nRowNum].Focus();
                if (((STextBox)sender).Text == "M")
                {
                    MessageLabel("T-MARGIN").Text = "Target Margin";
                }
                else
                {
                    MessageLabel("T-MARGIN").Text = "Target Price";
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                if (MessageBox.Show("Would you like to quit, discarding any changes?", "Quit?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    this.Close();
                }
            }
            else if (e.KeyCode == Keys.Delete && e.Shift)
            {
                RemoveRow(((STextBox)sender).nRowNum);
            }
            else if (e.KeyCode == Keys.F2)
            {
                MoveToLine();
            }
        }

        void BarcodeGotFocus(object sender, EventArgs e)
        {
            ((STextBox)sender).SelectionStart = ((STextBox)sender).Text.Length;
        }

        void QtyGotFocus(object sender, EventArgs e)
        {
            if (((STextBox)sender).Text == "")
            {
                ((STextBox)sender).Text = "1";
                ((STextBox)sender).SelectAll();
            }
        }

        void TargetMarginKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (((STextBox)sender).nRowNum == tbBarcode.Length - 1)
                {
                    AddRow();
                }
                tbBarcode[((STextBox)sender).nRowNum + 1].Focus();
            }
            else if (e.KeyCode == Keys.F2)
            {
                MoveToLine();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                if (MessageBox.Show("Would you like to quit, discarding any changes?", "Quit?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    this.Close();
                }
            }
            else if (e.KeyCode == Keys.Delete && e.Shift)
            {
                RemoveRow(((STextBox)sender).nRowNum);
            }
        }

        void QtyKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    Convert.ToDecimal(((STextBox)sender).Text);
                    tbMarginOrRRP[((STextBox)sender).nRowNum].Focus();
                }
                catch
                {
                    MessageBox.Show("Sorry, only integer quantities can be used. Create a child item with fractional units and use that here if necessary.");
                }
            }
            else if (e.KeyCode == Keys.F2)
            {
                MoveToLine();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                if (MessageBox.Show("Would you like to quit, discarding any changes?", "Quit?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    this.Close();
                }
            }
            else if (e.KeyCode == Keys.Delete && e.Shift)
            {
                RemoveRow(((STextBox)sender).nRowNum);
            }
        }

        void BarcodeKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (((STextBox)sender).Text == "")
                {
                    switch (MessageBox.Show("Finished entering items?", "Calculate Prices?", MessageBoxButtons.YesNoCancel))
                    {
                        case DialogResult.Yes:
                            GetInfoAndCalculatePrices();
                            break;
                        case DialogResult.No:
                            tbBarcode[0].Focus();
                            break;
                    }
                }
                else
                {
                    // Check to see if item exists
                    if (sEngine.GetMainStockInfo(tbBarcode[((STextBox)sender).nRowNum].Text).Length > 1)
                    {
                        DisplayItemInfo(((STextBox)sender).nRowNum);
                    }

                    else
                    {
                        if (MessageBox.Show("Item doesn't exist! Add it now?", "Add Item?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            frmAddEditItem faei = new frmAddEditItem(ref sEngine);
                            faei.AddingBarcode = ((STextBox)sender).Text;
                            faei.ShowDialog();
                        }
                        ((STextBox)sender).Text = "";
                    }
                }
            }
            else if (e.KeyCode == Keys.F5)
            {
                frmSearchForItemV2 fsfi = new frmSearchForItemV2(ref sEngine);
                fsfi.ShowDialog();
                if (fsfi.GetItemBarcode() != "NONE_SELECTED")
                {
                    ((STextBox)sender).Text = fsfi.GetItemBarcode();
                    BarcodeKeyDown(sender, new KeyEventArgs(Keys.Enter));
                }
            }
            else if (e.KeyCode == Keys.F2)
            {
                MoveToLine();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                if (MessageBox.Show("Would you like to quit, discarding any changes?", "Quit?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    this.Close();
                }
            }
            else if (e.KeyCode == Keys.Delete && e.Shift)
            {
                RemoveRow(((STextBox)sender).nRowNum);
            }
        }

        void MoveToLine()
        {
            frmSingleInputBox fsi = new frmSingleInputBox("Which line would you like to go to?", ref sEngine);
            fsi.ShowDialog();
            if (fsi.Response != "$NONE")
            {
                try
                {
                    tbBarcode[Convert.ToInt32(fsi.Response) - 1].Focus();
                }
                catch
                {
                    return;
                }
            }
        }

        void RemoveRow(int nRow)
        {
            for (int i = nRow; i < tbBarcode.Length - 1; i++)
            {
                tbBarcode[i].Text = tbBarcode[i + 1].Text;
                lblDesc[i].Text = lblDesc[i + 1].Text;
                tbQty[i].Text = tbQty[i + 1].Text;
                lblRRPEach[i].Text = lblRRPEach[i + 1].Text;
                lblFullMargin[i].Text = lblFullMargin[i + 1].Text;
                tbTargetMargin[i].Text = tbTargetMargin[i + 1].Text;
                tbMarginOrRRP[i].Text = tbMarginOrRRP[i + 1].Text;
            }
            this.Controls.Remove(lblLineNo[lblLineNo.Length - 1]);
            this.Controls.Remove(tbBarcode[tbBarcode.Length - 1]);
            this.Controls.Remove(lblDesc[lblDesc.Length - 1]);
            this.Controls.Remove(tbQty[tbQty.Length - 1]);
            this.Controls.Remove(lblRRPEach[lblRRPEach.Length - 1]);
            this.Controls.Remove(lblFullMargin[lblFullMargin.Length - 1]);
            this.Controls.Remove(tbTargetMargin[tbTargetMargin.Length - 1]);
            this.Controls.Remove(lblFinalLinePrice[lblFinalLinePrice.Length - 1]);
            this.Controls.Remove(tbMarginOrRRP[tbMarginOrRRP.Length - 1]);
            Array.Resize<SLabel>(ref lblLineNo, lblLineNo.Length - 1);
            Array.Resize<STextBox>(ref tbBarcode, tbBarcode.Length - 1);
            Array.Resize<SLabel>(ref lblDesc, lblDesc.Length - 1);
            Array.Resize<STextBox>(ref tbQty, tbQty.Length - 1);
            Array.Resize<SLabel>(ref lblRRPEach, lblRRPEach.Length - 1);
            Array.Resize<SLabel>(ref lblFullMargin, lblFullMargin.Length - 1);
            Array.Resize<STextBox>(ref tbTargetMargin, tbTargetMargin.Length - 1);
            Array.Resize<SLabel>(ref lblFinalLinePrice, lblFinalLinePrice.Length - 1);
            Array.Resize<STextBox>(ref tbMarginOrRRP, tbMarginOrRRP.Length - 1);
            this.Refresh();
        }

        void GetInfoAndCalculatePrices()
        {
            RemoveRow(tbBarcode.Length - 1);
            bool bAllEmpty = true;
            int nNotEmpty = 0;
            for (int i = 0; i < tbTargetMargin.Length; i++)
            {
                if (tbTargetMargin[i].Text.Length > 0)
                {
                    bAllEmpty = false;
                    nNotEmpty++;
                }
            }
            bool bAllFilledIn = false;
            if (nNotEmpty == tbTargetMargin.Length)
            {
                bAllFilledIn = true;
            }
            frmSingleInputBox fGetFinalPrice = new frmSingleInputBox("What is the total price?", ref sEngine);
            decimal dTotal = 0;
            for (int i = 0; i < lblRRPEach.Length; i++)
            {
                if (tbMarginOrRRP[i].Text != "R")
                {
                    dTotal += (Convert.ToDecimal(lblRRPEach[i].Text) * Convert.ToDecimal(tbQty[i].Text));
                }
                else
                {
                    if (tbTargetMargin[i].Text.Length != 0)
                    {
                        dTotal += (Convert.ToDecimal(tbTargetMargin[i].Text) * Convert.ToDecimal(tbQty[i].Text));
                    }
                    else
                    {
                        dTotal += (Convert.ToDecimal(lblRRPEach[i].Text) * Convert.ToDecimal(tbQty[i].Text));
                    }
                }
            }
            fGetFinalPrice.tbResponse.Text = FormatMoneyForDisplay(dTotal);
            decimal dTargetPrice = 0;
            if (!bAllFilledIn)
            {
                fGetFinalPrice.ShowDialog();
                try
                {
                    dTargetPrice = Convert.ToDecimal(fGetFinalPrice.Response);
                }
                catch
                {
                    MessageBox.Show("Invalid Price Entered");
                    return;
                }
            }
            else
            {
                dTargetPrice = dTotal;
            }
            bool[] bPriceCalculated = new bool[tbBarcode.Length];
            for (int i = 0; i < bPriceCalculated.Length; i++)
                bPriceCalculated[i] = false;
            decimal dPriceLeft = dTargetPrice;
            decimal dFullRRPTotal = 0;
            for (int i = 0; i < tbTargetMargin.Length; i++)
            {
                dFullRRPTotal += (Convert.ToDecimal(lblRRPEach[i].Text) * Convert.ToDecimal(tbQty[i].Text));
                if (tbTargetMargin[i].Text != "")
                {
                    string[] sStockStatInfo = sEngine.GetItemStockStaRecord(tbBarcode[i].Text, InputTextBox("SHOPCODEINPUT").Text);
                    string[] sMainStockInfo = sEngine.GetMainStockInfo(tbBarcode[i].Text);
                    if (tbMarginOrRRP[i].Text.ToUpper() == "M")
                    {
                        decimal dCost = Convert.ToDecimal(sStockStatInfo[1]);
                        decimal dTargetMarginPercent = Convert.ToDecimal(tbTargetMargin[i].Text);
                        decimal dVATRate = 1 + (sEngine.GetVATRateFromCode(tbBarcode[0].Text) / 100);
                        decimal dProfitReq = 0;
                        if (dCost != 0)
                        {
                            dProfitReq = dTargetMarginPercent / (100 / dCost);
                        }
                        else
                        {
                            dProfitReq = Convert.ToDecimal(lblRRPEach[i].Text) / dVATRate;
                        }
                        decimal dFinalPrice = Math.Round((dCost + dProfitReq) * dVATRate, 2);
                        decimal dQuantity = Convert.ToDecimal(tbQty[i].Text);
                        lblFinalLinePrice[i].Text = FormatMoneyForDisplay(Math.Round(dFinalPrice, 2));
                        if (dCost == 0)
                        {
                            lblFinalLineMargin[i].Text = "100%";
                        }
                        else
                        {
                            lblFinalLineMargin[i].Text = FormatMoneyForDisplay((100 / dCost) * dProfitReq) + "%";
                        }
                        bPriceCalculated[i] = true;
                        dPriceLeft -= (dFinalPrice * dQuantity);
                    }
                    else
                    {
                        decimal dCost = Convert.ToDecimal(sStockStatInfo[1]);
                        decimal dVATRate = 1 + (sEngine.GetVATRateFromCode(tbBarcode[0].Text) / 100);
                        decimal dRRP = Convert.ToDecimal(tbTargetMargin[i].Text);
                        decimal dNet = dRRP / dVATRate;
                        decimal dProfit = dNet - dCost;
                        lblFinalLinePrice[i].Text = FormatMoneyForDisplay(dRRP);
                        if (dCost != 0)
                        {
                            lblFinalLineMargin[i].Text = FormatMoneyForDisplay((100 / dCost) * dProfit) + "%";
                        }
                        else
                        {
                            lblFinalLineMargin[i].Text = "100%";
                        }
                        bPriceCalculated[i] = true;
                        dPriceLeft -= (dRRP * Convert.ToDecimal(tbQty[i].Text));
                    }

                }
            }
            decimal dTotalRRPLeft = 0;
            for (int i = 0; i < tbTargetMargin.Length; i++)
            {
                if (!bPriceCalculated[i])
                {
                    dTotalRRPLeft += (Convert.ToDecimal(lblRRPEach[i].Text) * Convert.ToDecimal(tbQty[i].Text));
                }
            }

            for (int i = 0; i < tbTargetMargin.Length; i++)
            {
                if (!bPriceCalculated[i])
                {
                    string[] sStockStaInfo = sEngine.GetItemStockStaRecord(tbBarcode[i].Text, InputTextBox("SHOPCODEINPUT").Text);
                    decimal dTotalValueOfItem = Convert.ToDecimal(lblRRPEach[i].Text) * Convert.ToDecimal(tbQty[i].Text);
                    decimal dTotalValueOfTransaction = dTotalRRPLeft;
                    decimal dAmountThatNeedsDiscounting = dTotalRRPLeft - dPriceLeft;
                    decimal dAmountToDiscount = (dTotalValueOfItem / dTotalValueOfTransaction) * dAmountThatNeedsDiscounting;
                    decimal dNewPrice = (dTotalValueOfItem - dAmountToDiscount);
                    dNewPrice /= Convert.ToDecimal(tbQty[i].Text);
                    decimal dVATRate = 1 + (sEngine.GetVATRateFromCode(tbBarcode[0].Text) / 100);
                    decimal dProfit = (dNewPrice / dVATRate) - Convert.ToDecimal(sStockStaInfo[1]);
                    decimal dCost = Convert.ToDecimal(sStockStaInfo[1]);
                    if (dCost == 0)
                    {
                        lblFinalLineMargin[i].Text = "100%";
                    }
                    else
                    {
                        lblFinalLineMargin[i].Text = FormatMoneyForDisplay((100 / dCost) * dProfit) + "%";
                    }
                    lblFinalLinePrice[i].Text = FormatMoneyForDisplay(dNewPrice);
                }
            }
            
            decimal dSumPrice = 0;
            for (int i = 0; i < lblFinalLinePrice.Length; i++)
            {
                dSumPrice += (Convert.ToDecimal(lblFinalLinePrice[i].Text) * Convert.ToDecimal(tbQty[i].Text));
            }
            if (dSumPrice != dTargetPrice)
            {
                lblFinalLinePrice[lblFinalLinePrice.Length - 1].Text = FormatMoneyForDisplay(Convert.ToDecimal(lblFinalLinePrice[lblFinalLinePrice.Length - 1].Text) + (dTargetPrice - dSumPrice));
            }
            dSumPrice = 0;
            for (int i = 0; i < lblFinalLinePrice.Length; i++)
            {
                dSumPrice += (Convert.ToDecimal(lblFinalLinePrice[i].Text) * Convert.ToDecimal(tbQty[i].Text));
            }
            RemoveInputTextBox("ACCEPT");
            AddInputControl("ACCEPT", "Total is " + FormatMoneyForDisplay(dSumPrice) + ". Do you want to accept the above and save?", new Point(10, tbBarcode[tbBarcode.Length - 1].Top + 100), 450, "Enter Y to save, N to restart, Q to Quit. F2 to edit.");
            InputTextBox("ACCEPT").KeyDown += new KeyEventHandler(AcceptKeyDown);
            InputTextBox("ACCEPT").Focus();
        }

        void AcceptKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (InputTextBox("ACCEPT").Text.Length > 0)
                {
                    if (InputTextBox("ACCEPT").Text.ToUpper()[0] == 'Y')
                    {
                        SaveItems();
                        this.Close();
                    }
                    else if (InputTextBox("ACCEPT").Text.ToUpper()[0] == 'Q')
                    {
                        this.Close();
                    }
                    else
                    {
                        InputTextBox("SHOPCODEINPUT").Focus();
                    }
                }
            }
            else if (e.KeyCode == Keys.F2)
            {
                MoveToLine();
            }
        }

        public void SaveItems()
        {
            string[] sSubCodes = new string[tbBarcode.Length];
            decimal[] dSubPrices = new decimal[tbBarcode.Length];
            decimal[] dQuantites = new decimal[tbBarcode.Length];
            for (int i = 0; i < tbBarcode.Length; i++)
            {
                sSubCodes[i] = tbBarcode[i].Text;
                dSubPrices[i] = Convert.ToDecimal(lblFinalLinePrice[i].Text);
                dQuantites[i] = Convert.ToDecimal(tbQty[i].Text);
            }
            sEngine.AddMultiItemItem(InputTextBox("BARCODEINPUT").Text, InputTextBox("DESCINPUT").Text, InputTextBox("SHOPCODEINPUT").Text, sSubCodes, dQuantites, dSubPrices);
            if (MessageBox.Show("Would you like to upload this onto all tills now?", "Upload now?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                sEngine.CopyWaitingFilesToTills();
            }
        }

        void DisplayItemInfo(int nRowNum)
        {
            // Item exists, show info
            string[] sInfo = sEngine.GetMainStockInfo(tbBarcode[nRowNum].Text);
            string[] sStockStaInfo = sEngine.GetItemStockStaRecord(tbBarcode[nRowNum].Text, InputTextBox("SHOPCODEINPUT").Text);
            if (sInfo[5] == "1" || sInfo[5] == "3" || sInfo[5] == "6" || sInfo[5] == "5" || sInfo[5] == "2")
            {
                // Check to make sure it's a type 1, 3, 5 or 6 item
                lblDesc[nRowNum].Text = sInfo[1];
                lblRRPEach[nRowNum].Text = FormatMoneyForDisplay(sInfo[2]);
                decimal dVATRate = (sEngine.GetVATRateFromCode(sInfo[0]) / 100) + 1;
                decimal dNet = Convert.ToDecimal(sInfo[2]) / dVATRate;
                decimal dProfit = dNet - Convert.ToDecimal(sStockStaInfo[1]);
                if (Convert.ToDecimal(sStockStaInfo[1]) != 0)
                {
                    lblFullMargin[nRowNum].Text = Math.Round(((100 / Convert.ToDecimal(sStockStaInfo[1])) * dProfit), 2).ToString() + "%";
                }
                else
                {
                    lblFullMargin[nRowNum].Text = "100%";
                }
                tbQty[nRowNum].Focus();
            }
            else
            {
                MessageBox.Show("You can only use type 1, 2 3, 5 and 6 items here");
                tbBarcode[nRowNum].Text = "";
            }
        }
    }
}
