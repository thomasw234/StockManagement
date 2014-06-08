using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Text;
using System.Drawing;

namespace BackOffice
{
    class frmBatchAddItems : ScalableForm
    {
        private class STextBox : TextBox
        {
            public int nRowNum = 0;
            public int nMaxCharCount = 0;

            protected override void OnKeyDown(KeyEventArgs e)
            {
                if (nMaxCharCount != 0 && Text.Length < nMaxCharCount)
                {
                    base.OnKeyDown(e);
                }
                else if (nMaxCharCount == 0)
                    base.OnKeyDown(e);
                else
                {
                    if (e.KeyCode != Keys.Enter && e.KeyCode != Keys.Tab && e.KeyCode != Keys.Delete && e.KeyCode != Keys.Back  && e.KeyCode != Keys.Left && e.KeyCode != Keys.Right)
                        e.SuppressKeyPress = true;
                    base.OnKeyDown(e);
                }
            }

            protected override void OnKeyPress(KeyPressEventArgs e)
            {
                if (e.KeyChar == (char)13)
                    e.Handled = true;
                base.OnKeyPress(e);
            }
        }
    
        StockEngine sEngine;
        frmBatchOptions fbOptions;
        frmBatchEditOptions fbEdit;
        Label[] lblRowNums;
        STextBox[] tbBarcode;
        STextBox[] tbDesc;
        STextBox[] tbType;
        STextBox[] tbCategory;
        STextBox[] tbRRP;
        STextBox[] tbVAT;
        STextBox[] tbMinQty;
        STextBox[] tbSupCode;
        STextBox[] tbSupCost;
        Label[] lblProfit;
        STextBox[] tbSupProdCode;
        bool bEditing = false;
        bool bReceiving = false;
        string sRecSupplier = "";

        string[] sCodes;
        bool bClosing = false;

        public bool Receiving
        {
            get
            {
                return bReceiving;
            }
            set
            {
                bReceiving = value;
                if (value)
                    MessageLabel("MINQTY").Text = "Receiving";
                else
                    MessageLabel("MINQTY").Text = "Minimum Quantity";
            }
        }
        public string ReceivingSupplier
        {
            set
            {
                sRecSupplier = value;
                fbOptions.InputTextBox("SUPPLIER").Text = sRecSupplier;
            }
        }

        int nOfItems
        {
            get
            {
                return tbBarcode.Length;
            }
        }
        string[] sParentBarcodes;
        decimal[] dChildParts;

        int nStartItem = 0;
        int nEndItem = 0;

        public frmBatchAddItems(ref StockEngine se, bool bAdding)
        {
            this.WindowState = FormWindowState.Maximized;
            this.VisibleChanged += new EventHandler(frmBatchAddItems_VisibleChanged);
            sEngine = se;
            bEditing = !bAdding;
            if (bAdding)
            {
                fbOptions = new frmBatchOptions(ref sEngine);
                fbOptions.ShowDialog();
            }

            //this.AllowScaling = false;
            lblRowNums = new Label[0];
            tbBarcode = new STextBox[0];
            tbDesc = new STextBox[0];
            tbType = new STextBox[0];
            tbCategory = new STextBox[0];
            tbRRP = new STextBox[0];
            tbVAT = new STextBox[0];
            tbMinQty = new STextBox[0];
            tbSupCode = new STextBox[0];
            tbSupCost = new STextBox[0];
            tbSupProdCode = new STextBox[0];
            sParentBarcodes = new string[0];
            dChildParts = new decimal[0];
            lblProfit = new Label[0];

            AddMessage("BARCODE", "Barcode", new Point(30, 10));
            AddMessage("DESC", "Description", new Point(150, 10));
            AddMessage("TYPE", "Type", new Point(330, 10));
            AddMessage("CATEGORY", "Category", new Point(360, 10));
            AddMessage("RRP", "RRP", new Point(450, 10));
            AddMessage("VAT", "VAT", new Point(500, 10));
            AddMessage("MINQTY", "Minimum Quantity", new Point(550, 10));
            AddMessage("SUPCODE", "Supplier Code", new Point(670, 10));
            AddMessage("SUPCOST", "Cost", new Point(760, 10));
            AddMessage("PROFIT", "Profit (%)", new Point(810, 10));
            AddMessage("SUPPRODCODE", "Product Code", new Point(880, 10));

            if (bAdding)
                AddNewItemRow(true);
            else
            {
                fbEdit = new frmBatchEditOptions(ref sEngine);
                fbEdit.ShowDialog();
                if (fbEdit.bFinished)
                    sCodes = sEngine.GetListOfItemsByCatAndSup(fbEdit.ShopCode, fbEdit.CategoryCode, fbEdit.SupplierCode);
            }

            this.Paint += new PaintEventHandler(frmBatchAddItems_Paint);
            this.FormClosing += new FormClosingEventHandler(frmBatchAddItems_FormClosing);
            this.Text = "Batch Add / Edit Items";
        }

        void frmBatchAddItems_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!bClosing)
                SaveChanges(false);
        }

        void frmBatchAddItems_VisibleChanged(object sender, EventArgs e)
        {
            if (sCodes != null)
            {
                frmProgressBar fp = new frmProgressBar("Loading Items");
                fp.pb.Maximum = sCodes.Length;
                fp.Show();
                for (int i = 0; i < sCodes.Length; i++)
                {
                    AddNewItemRow(false);
                    tbBarcode[tbBarcode.Length - 1].Text = sCodes[i];
                    LoadRowInfo(i);
                    fp.pb.Value = i;
                }
                fp.Close();
                sCodes = null;
                MoveToLine(1);
            }
            if (bEditing && fbEdit.bFinished)
            {
                UpdateRowsDisplayed();
                if (tbBarcode.Length > 0)
                    tbDesc[0].Focus();
            }
            else if (bEditing && !fbEdit.bFinished)
            {
                bClosing = true;
                this.Close();
            }
            AutoReWidthTextBoxes();
        }

        void frmBatchAddItems_Paint(object sender, PaintEventArgs e)
        {
            if (nOfItems > 0)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.Gray), new Rectangle(lblRowNums[nStartItem].Left - 2, tbBarcode[nStartItem].Top - 2, (tbSupProdCode[nOfItems - 1].Left + tbSupProdCode[nOfItems - 1].Width) - lblRowNums[nOfItems - 1].Left + 4, (tbBarcode[nOfItems-1].Top + tbBarcode[nOfItems - 1].Height) - tbBarcode[nStartItem].Top + 4));
            }
        }

        void AutoReWidthTextBoxes()
        {
            for (int i = 0; i < nOfItems; i++)
            {
                int nTop = tbBarcode[i].Top;
                lblRowNums[i].Left = 10;
                lblRowNums[i].Width = MessageLabel("BARCODE").Left - 10;
                tbBarcode[i].Location = new Point(MessageLabel("BARCODE").Left, nTop);
                tbBarcode[i].Width = MessageLabel("DESC").Left - MessageLabel("BARCODE").Left - 2;
                tbDesc[i].Location = new Point(MessageLabel("DESC").Left, nTop);
                tbDesc[i].Width = MessageLabel("TYPE").Left - MessageLabel("DESC").Left - 2;
                tbType[i].Location = new Point(MessageLabel("TYPE").Left, nTop);
                tbType[i].Width = MessageLabel("CATEGORY").Left - MessageLabel("TYPE").Left - 2;
                tbCategory[i].Location = new Point(MessageLabel("CATEGORY").Left, nTop);
                tbCategory[i].Width = MessageLabel("RRP").Left - MessageLabel("CATEGORY").Left - 2;
                tbRRP[i].Location = new Point(MessageLabel("RRP").Left, nTop);
                tbRRP[i].Width = MessageLabel("VAT").Left - MessageLabel("RRP").Left - 2;
                tbVAT[i].Location = new Point(MessageLabel("VAT").Left, nTop);
                tbVAT[i].Width = MessageLabel("MINQTY").Left - MessageLabel("VAT").Left - 2;
                tbMinQty[i].Location = new Point(MessageLabel("MINQTY").Left, nTop);
                tbMinQty[i].Width = MessageLabel("SUPCODE").Left - MessageLabel("MINQTY").Left - 2;
                tbSupCode[i].Location = new Point(MessageLabel("SUPCODE").Left, nTop);
                tbSupCode[i].Width = MessageLabel("SUPCOST").Left - MessageLabel("SUPCODE").Left - 2;
                tbSupCost[i].Location = new Point(MessageLabel("SUPCOST").Left, nTop);
                tbSupCost[i].Width = MessageLabel("PROFIT").Left - MessageLabel("SUPCOST").Left - 2;
                tbSupProdCode[i].Location = new Point(MessageLabel("SUPPRODCODE").Left, nTop);
                tbSupProdCode[i].Width = this.Width - 10 - MessageLabel("SUPPRODCODE").Left - 2;
                lblProfit[i].Location = new Point(MessageLabel("PROFIT").Left, nTop - 2);
                lblProfit[i].Width = MessageLabel("SUPPRODCODE").Left - MessageLabel("PROFIT").Left - 2;
            }
        }

        void LoadRowInfo(int nRow)
        {
            string[] sMainStock = sEngine.GetMainStockInfo(tbBarcode[nRow].Text);
            if (sMainStock.Length > 1)
            {
                string[] sStockStaInfo = sEngine.GetItemStockStaRecord(tbBarcode[nRow].Text, fbEdit.ShopCode);
                tbDesc[nRow].Text = sMainStock[1];
                tbCategory[nRow].Text = sMainStock[4];
                tbMinQty[nRow].Text = sStockStaInfo[37];
                tbRRP[nRow].Text = sMainStock[2];
                if (fbEdit.SupplierCode != "")
                    tbSupCode[nRow].Text = fbEdit.SupplierCode;
                else
                    tbSupCode[nRow].Text = sMainStock[6];
                tbSupCost[nRow].Text = FormatMoneyForDisplay(sEngine.GetItemCostBySupplier(tbBarcode[nRow].Text, tbSupCode[nRow].Text));
                tbSupProdCode[nRow].Text = sEngine.GetItemCodeBySupplier(tbBarcode[nRow].Text, tbSupCode[nRow].Text);
                tbType[nRow].Text = sMainStock[5];
                tbVAT[nRow].Text = sMainStock[3];
                sParentBarcodes[nRow] = sMainStock[7];
                dChildParts[nRow] = Convert.ToDecimal(sStockStaInfo[38]);
            }
        }

        void UpdateRowsDisplayed()
        {
            int nLeft = MessageLabel("PROFIT").Left;
            int nPTop = MessageLabel("PROFIT").Top;
            RemoveMessage("PROFIT");
            AddMessage("PROFIT", "Profit (%)", new Point(nLeft, nPTop));
            int nTop = BelowLastControl;
            for (int i = 0; i < tbBarcode.Length; i++)
            {
                if (i < nStartItem || i > nEndItem)
                {
                    if (lblRowNums[i].Visible == true)
                    {
                        lblRowNums[i].Visible = false;
                        tbBarcode[i].Visible = false;
                        tbDesc[i].Visible = false;
                        tbType[i].Visible = false;
                        tbCategory[i].Visible = false;
                        tbRRP[i].Visible = false;
                        tbVAT[i].Visible = false;
                        tbMinQty[i].Visible = false;
                        tbSupCode[i].Visible = false;
                        tbSupCost[i].Visible = false;
                        tbSupProdCode[i].Visible = false;
                        lblProfit[i].Visible = false;
                    }
                }
                else
                {
                    if (lblRowNums[i].Visible == false)
                    {
                        lblRowNums[i].Visible = true;
                        tbBarcode[i].Visible = true;
                        tbDesc[i].Visible = true;
                        tbType[i].Visible = true;
                        tbCategory[i].Visible = true;
                        tbRRP[i].Visible = true;
                        tbVAT[i].Visible = true;
                        tbMinQty[i].Visible = true;
                        tbSupCode[i].Visible = true;
                        tbSupCost[i].Visible = true;
                        tbSupProdCode[i].Visible = true;
                        lblProfit[i].Visible = true;
                    }
                    lblRowNums[i].Location = new Point(10, nTop);
                    tbBarcode[i].Location = new Point(MessageLabel("BARCODE").Left, nTop);
                    tbDesc[i].Location = new Point(MessageLabel("DESC").Left, nTop);
                    tbType[i].Location = new Point(MessageLabel("TYPE").Left, nTop);
                    tbCategory[i].Location = new Point(MessageLabel("CATEGORY").Left, nTop);
                    tbRRP[i].Location = new Point(MessageLabel("RRP").Left, nTop);
                    tbVAT[i].Location = new Point(MessageLabel("VAT").Left, nTop);
                    tbMinQty[i].Location = new Point(MessageLabel("MINQTY").Left, nTop);
                    tbSupCode[i].Location = new Point(MessageLabel("SUPCODE").Left, nTop);
                    tbSupCost[i].Location = new Point(MessageLabel("SUPCOST").Left, nTop);
                    lblProfit[i].Location = new Point(MessageLabel("PROFIT").Left, nTop);
                    tbSupProdCode[i].Location = new Point(MessageLabel("SUPPRODCODE").Left, nTop);

                    nTop += tbBarcode[i].Height + 2;
                }
            }
        }

        void AddNewItemRow(bool bUpdateRowsDisplayed)
        {
            if (tbBarcode.Length > 0 && tbBarcode[tbBarcode.Length - 1].Top + (tbBarcode[tbBarcode.Length - 1].Height * 2) + 50 > this.Height)
            {
                // Can't fit another row on!
                //SaveChanges(true);
                nStartItem++;
                if (bUpdateRowsDisplayed)
                    UpdateRowsDisplayed();
                AddNewItemRow(true);
            }
            else
            {
                int nTop = 0;
                if (tbBarcode.Length == 0)
                    nTop = BelowLastControl;
                else
                    nTop = tbBarcode[tbBarcode.Length - 1].Top + tbBarcode[tbBarcode.Length - 1].Height + 2;
                nEndItem++;
                Array.Resize<Label>(ref lblRowNums, lblRowNums.Length + 1);
                lblRowNums[lblRowNums.Length - 1] = new Label();
                lblRowNums[lblRowNums.Length - 1].Location = new Point(10, nTop);
                lblRowNums[lblRowNums.Length - 1].AutoSize = false;
                lblRowNums[lblRowNums.Length - 1].Width = MessageLabel("BARCODE").Left - 10;
                lblRowNums[lblRowNums.Length - 1].BackColor = Color.Transparent;
                lblRowNums[lblRowNums.Length - 1].ForeColor = Color.White;
                lblRowNums[lblRowNums.Length - 1].Text = lblRowNums.Length.ToString();
                lblRowNums[lblRowNums.Length - 1].TextAlign = ContentAlignment.MiddleLeft;
                this.Controls.Add(lblRowNums[lblRowNums.Length - 1]);
                Array.Resize<STextBox>(ref tbBarcode, tbBarcode.Length + 1);
                tbBarcode[tbBarcode.Length - 1] = new STextBox();
                tbBarcode[tbBarcode.Length - 1].Location = new Point(MessageLabel("BARCODE").Left, nTop);
                tbBarcode[tbBarcode.Length - 1].BorderStyle = BorderStyle.None;
                tbBarcode[tbBarcode.Length - 1].Width = MessageLabel("DESC").Left - MessageLabel("BARCODE").Left - 2;
                tbBarcode[tbBarcode.Length - 1].KeyDown += new KeyEventHandler(BarcodeKeyDown);
                tbBarcode[tbBarcode.Length - 1].GotFocus += new EventHandler(BarcodeGotFocus);
                tbBarcode[tbBarcode.Length - 1].KeyUp += new KeyEventHandler(BarcodeKeyup);
                tbBarcode[tbBarcode.Length - 1].nRowNum = tbBarcode.Length - 1;
                tbBarcode[tbBarcode.Length - 1].nMaxCharCount = 13;
                this.Controls.Add(tbBarcode[tbBarcode.Length - 1]);
                lblRowNums[lblRowNums.Length - 1].Height = tbBarcode[tbBarcode.Length - 1].Height;
                Array.Resize<STextBox>(ref tbDesc, tbDesc.Length + 1);
                tbDesc[tbDesc.Length - 1] = new STextBox();
                tbDesc[tbDesc.Length - 1].Location = new Point(MessageLabel("DESC").Left, nTop);
                tbDesc[tbDesc.Length - 1].BorderStyle = BorderStyle.None;
                tbDesc[tbDesc.Length - 1].Width = MessageLabel("TYPE").Left - MessageLabel("DESC").Left - 2;
                tbDesc[tbDesc.Length - 1].nRowNum = tbDesc.Length - 1;
                tbDesc[tbDesc.Length - 1].KeyDown += new KeyEventHandler(DescKeyDown);
                tbDesc[tbDesc.Length - 1].KeyUp += new KeyEventHandler(DescKeyUp);
                tbDesc[tbDesc.Length - 1].nMaxCharCount = 30;
                this.Controls.Add(tbDesc[tbDesc.Length - 1]);
                tbDesc[tbDesc.Length - 1].GotFocus += new EventHandler(DescGotFocus);
                Array.Resize<STextBox>(ref tbType, tbType.Length + 1);
                tbType[tbType.Length - 1] = new STextBox();
                tbType[tbType.Length - 1].Location = new Point(MessageLabel("TYPE").Left, nTop);
                tbType[tbType.Length - 1].BorderStyle = BorderStyle.None;
                tbType[tbType.Length - 1].Width = MessageLabel("CATEGORY").Left - MessageLabel("TYPE").Left - 2;
                tbType[tbType.Length - 1].nRowNum = tbType.Length - 1;
                tbType[tbType.Length - 1].KeyDown += new KeyEventHandler(TypeKeyDown);
                tbType[tbType.Length - 1].GotFocus += new EventHandler(TypeGotFocus);
                this.Controls.Add(tbType[tbType.Length - 1]);
                Array.Resize<STextBox>(ref tbCategory, tbCategory.Length + 1);
                tbCategory[tbCategory.Length - 1] = new STextBox();
                tbCategory[tbCategory.Length - 1].Location = new Point(MessageLabel("CATEGORY").Left, nTop);
                tbCategory[tbCategory.Length - 1].BorderStyle = BorderStyle.None;
                tbCategory[tbCategory.Length - 1].Width = MessageLabel("RRP").Left - MessageLabel("CATEGORY").Left - 2;
                tbCategory[tbCategory.Length - 1].nRowNum = tbCategory.Length - 1;
                tbCategory[tbCategory.Length - 1].KeyDown += new KeyEventHandler(CategoryKeyDown);
                tbCategory[tbCategory.Length - 1].GotFocus += new EventHandler(CategoryGotFocus);
                this.Controls.Add(tbCategory[tbCategory.Length - 1]);
                Array.Resize<STextBox>(ref tbRRP, tbRRP.Length + 1);
                tbRRP[tbRRP.Length - 1] = new STextBox();
                tbRRP[tbRRP.Length - 1].Location = new Point(MessageLabel("RRP").Left, nTop);
                tbRRP[tbRRP.Length - 1].BorderStyle = BorderStyle.None;
                tbRRP[tbRRP.Length - 1].Width = MessageLabel("VAT").Left - MessageLabel("RRP").Left - 2;
                tbRRP[tbRRP.Length - 1].nRowNum = tbRRP.Length - 1;
                tbRRP[tbRRP.Length - 1].KeyDown += new KeyEventHandler(RRPKeyDown);
                tbRRP[tbRRP.Length - 1].GotFocus += new EventHandler(RRPGotFocus);
                this.Controls.Add(tbRRP[tbRRP.Length - 1]);
                Array.Resize<STextBox>(ref tbVAT, tbVAT.Length + 1);
                tbVAT[tbVAT.Length - 1] = new STextBox();
                tbVAT[tbVAT.Length - 1].Location = new Point(MessageLabel("VAT").Left, nTop);
                tbVAT[tbVAT.Length - 1].BorderStyle = BorderStyle.None;
                tbVAT[tbVAT.Length - 1].Width = MessageLabel("MINQTY").Left - MessageLabel("VAT").Left - 2;
                tbVAT[tbVAT.Length - 1].nRowNum = tbVAT.Length - 1;
                tbVAT[tbVAT.Length - 1].KeyDown += new KeyEventHandler(VATKeyDown);
                tbVAT[tbVAT.Length - 1].GotFocus += new EventHandler(VATGotFocus);
                this.Controls.Add(tbVAT[tbVAT.Length - 1]);
                Array.Resize<STextBox>(ref tbMinQty, tbMinQty.Length + 1);
                tbMinQty[tbMinQty.Length - 1] = new STextBox();
                tbMinQty[tbMinQty.Length - 1].Location = new Point(MessageLabel("MINQTY").Left, nTop);
                tbMinQty[tbMinQty.Length - 1].BorderStyle = BorderStyle.None;
                tbMinQty[tbMinQty.Length - 1].Width = MessageLabel("SUPCODE").Left - MessageLabel("MINQTY").Left - 2;
                tbMinQty[tbMinQty.Length - 1].nRowNum = tbMinQty.Length - 1;
                tbMinQty[tbMinQty.Length - 1].KeyDown += new KeyEventHandler(MinQTYKeyDown);
                tbMinQty[tbMinQty.Length - 1].GotFocus += new EventHandler(MinQTYGotFocus);
                tbMinQty[tbMinQty.Length - 1].nMaxCharCount = 6;
                this.Controls.Add(tbMinQty[tbMinQty.Length - 1]);
                Array.Resize<STextBox>(ref tbSupCode, tbSupCode.Length + 1);
                tbSupCode[tbSupCode.Length - 1] = new STextBox();
                tbSupCode[tbSupCode.Length - 1].Location = new Point(MessageLabel("SUPCODE").Left, nTop);
                tbSupCode[tbSupCode.Length - 1].BorderStyle = BorderStyle.None;
                tbSupCode[tbSupCode.Length - 1].Width = MessageLabel("SUPCOST").Left - MessageLabel("SUPCODE").Left - 2;
                tbSupCode[tbSupCode.Length - 1].nRowNum = tbSupCode.Length - 1;
                tbSupCode[tbSupCode.Length - 1].KeyDown += new KeyEventHandler(SupCodeKeyDown);
                tbSupCode[tbSupCode.Length - 1].GotFocus += new EventHandler(SupCodeGotFocus);
                this.Controls.Add(tbSupCode[tbSupCode.Length - 1]);
                Array.Resize<STextBox>(ref tbSupCost, tbSupCost.Length + 1);
                tbSupCost[tbSupCost.Length - 1] = new STextBox();
                tbSupCost[tbSupCost.Length - 1].Location = new Point(MessageLabel("SUPCOST").Left, nTop);
                tbSupCost[tbSupCost.Length - 1].BorderStyle = BorderStyle.None;
                tbSupCost[tbSupCost.Length - 1].Width = MessageLabel("PROFIT").Left - MessageLabel("SUPCOST").Left - 2;
                tbSupCost[tbSupCost.Length - 1].nRowNum = tbSupCost.Length - 1;
                tbSupCost[tbSupCost.Length - 1].KeyDown += new KeyEventHandler(SupCostKeyDown);
                tbSupCost[tbSupCost.Length - 1].GotFocus += new EventHandler(SupCostGotFocus);
                this.Controls.Add(tbSupCost[tbSupCost.Length - 1]);
                Array.Resize<Label>(ref lblProfit, lblProfit.Length + 1);
                lblProfit[lblProfit.Length - 1] = new Label();
                lblProfit[lblProfit.Length - 1].AutoSize = false;
                lblProfit[lblProfit.Length - 1].Location = new Point(MessageLabel("PROFIT").Left, nTop - 2);
                lblProfit[lblProfit.Length - 1].Width = MessageLabel("SUPPRODCODE").Left - MessageLabel("PROFIT").Left - 2;
                lblProfit[lblProfit.Length - 1].TextAlign = ContentAlignment.MiddleRight;
                this.Controls.Add(lblProfit[lblProfit.Length - 1]);
                Array.Resize<STextBox>(ref tbSupProdCode, tbSupProdCode.Length + 1);
                tbSupProdCode[tbSupProdCode.Length - 1] = new STextBox();
                tbSupProdCode[tbSupProdCode.Length - 1].Location = new Point(MessageLabel("SUPPRODCODE").Left, nTop);
                tbSupProdCode[tbSupProdCode.Length - 1].BorderStyle = BorderStyle.None;
                tbSupProdCode[tbSupProdCode.Length - 1].Width = this.Width - 10 - MessageLabel("SUPPRODCODE").Left - 13;
                tbSupProdCode[tbSupProdCode.Length - 1].nRowNum = tbSupProdCode.Length - 1;
                tbSupProdCode[tbSupProdCode.Length - 1].KeyDown += new KeyEventHandler(SupProdCodeKeyDown);
                tbSupProdCode[tbSupProdCode.Length - 1].GotFocus += new EventHandler(SupProdCodeGotFocus);
                tbSupProdCode[tbSupProdCode.Length - 1].nMaxCharCount = 13;
                this.Controls.Add(tbSupProdCode[tbSupProdCode.Length - 1]);
                Array.Resize<string>(ref sParentBarcodes, sParentBarcodes.Length + 1);
                Array.Resize<decimal>(ref dChildParts, dChildParts.Length + 1);
                this.Refresh();
            }
        }

        void SupProdCodeGotFocus(object sender, EventArgs e)
        {
            if (bEditing)
            {
                SendKeys.Send("{ENTER}");
            }
            if (((STextBox)sender).Text == "" && !bEditing)
                ((STextBox)sender).Text = fbOptions.SupplierCode;
            ((STextBox)sender).SelectionStart = ((STextBox)sender).Text.Length;
        }

        void SupCostGotFocus(object sender, EventArgs e)
        {
            try
            {
                if (bEditing && !fbEdit.EditSupCost)
                {
                    tbSupProdCode[((STextBox)sender).nRowNum].Focus();
                }
                if (((STextBox)sender).Text == "" && !bEditing)
                    ((STextBox)sender).Text = fbOptions.SupplierCost;
            }
            catch
            {
                ;
            }
        }

        void RRPGotFocus(object sender, EventArgs e)
        {
            if (bEditing && !fbEdit.EditRRP)
            {
                tbVAT[((STextBox)sender).nRowNum].Focus();
            }
            if (((STextBox)sender).Text == "" && !bEditing)
                ((STextBox)sender).Text = fbOptions.RRP;
        }

        void BarcodeKeyup(object sender, KeyEventArgs e)
        {
            MessageLabel("BARCODE").Text = "Barcode [" + ((STextBox)sender).Text.Length.ToString() + " / 13]";
        }

        void DescKeyUp(object sender, KeyEventArgs e)
        {
            MessageLabel("DESC").Text = "Description [" + ((STextBox)sender).Text.Length.ToString() + " / 30]";
        }

        void SupCodeGotFocus(object sender, EventArgs e)
        {
            if (bEditing)
            {
                SendKeys.Send("{ENTER}");
            }
            if (((STextBox)sender).Text.Length == 0 && !bEditing)
            {
                ((STextBox)sender).Text = fbOptions.Supplier;
            }
        }

        void MinQTYGotFocus(object sender, EventArgs e)
        {
            if (((STextBox)sender).Text.Length == 0 && !bEditing)
            {
                ((STextBox)sender).Text = fbOptions.MinimumQuantity;
            }
            /*if (tbType[(((STextBox)sender).nRowNum)].Text == "6")
            {
                Receiving = true;
            }
            else
            {
                Receiving = false;
            }*/ // I'm not sure what this code did
        }

        void VATGotFocus(object sender, EventArgs e)
        {
            if (bEditing && !fbEdit.EditVAT)
            {
                tbSupCost[((STextBox)sender).nRowNum].Focus();
            }
            if (((STextBox)sender).Text.Length == 0 && !bEditing)
            {
                ((STextBox)sender).Text = fbOptions.VATRate;
            }
        }

        void CategoryGotFocus(object sender, EventArgs e)
        {
            if (bEditing && !fbEdit.EditCat)
            {
                tbRRP[((STextBox)sender).nRowNum].Focus();
            }
            if (((STextBox)sender).Text.Length == 0 && !bEditing)
            {
                ((STextBox)sender).Text = fbOptions.Category;
            }
        }

        void TypeGotFocus(object sender, EventArgs e)
        {
            if (bEditing && !fbEdit.EditType)
            {
                tbCategory[((STextBox)sender).nRowNum].Focus();
            }
            if (((STextBox)sender).Text.Length == 0 && !bEditing)
            {
                ((STextBox)sender).Text = fbOptions.Type;
            }
        }

        void DescGotFocus(object sender, EventArgs e)
        {
            if (bEditing && !fbEdit.EditDesc)
            {
                tbType[((STextBox)sender).nRowNum].Focus();
            }
            if (((STextBox)sender).Text.Length == 0 && !bEditing)
            {
                ((STextBox)sender).Text = fbOptions.Description;
                int nSelectionStart = -1;
                int nSelectionLength = 0;
                for (int i = 0; i < ((STextBox)sender).Text.Length; i++)
                {
                    if (((STextBox)sender).Text[i] == '#')
                    {
                        nSelectionStart = i;
                        break;
                    }
                }
                if (nSelectionStart != -1)
                {
                    while ((nSelectionLength + nSelectionStart) < ((STextBox)sender).Text.Length && ((STextBox)sender).Text[nSelectionLength + nSelectionStart] == '#')
                        nSelectionLength++;
                    ((STextBox)sender).Select(nSelectionStart, nSelectionLength);
                }
                else
                    ((STextBox)sender).SelectionStart = ((STextBox)sender).Text.Length;
            }
        }

        void BarcodeGotFocus(object sender, EventArgs e)
        {
            if (bEditing)
            {
                tbDesc[((STextBox)sender).nRowNum].Focus();
            }
            if (((STextBox)sender).Text.Length == 0 && !bEditing)
            {
                ((STextBox)sender).Text = fbOptions.BarcodeStart;
                ((STextBox)sender).SelectionStart = fbOptions.BarcodeStart.Length;
            }
        }

        void SupProdCodeKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (tbType[((STextBox)sender).nRowNum].Text == "5" && sParentBarcodes[((STextBox)sender).nRowNum] == null)
                {
                    GetParentAndChildInfo(sender);
                }
                if (CheckRow(((STextBox)sender).nRowNum))
                {
                    if (((STextBox)sender).nRowNum + 1 == tbBarcode.Length)
                    {
                        ClearBackColourRows();
                        if (!bEditing)
                        {
                            AddNewItemRow(true);
                            tbBarcode[tbBarcode.Length - 1].Focus();
                        }
                        else
                        {
                            SaveChanges(false);
                        }
                    }
                    else
                    {
                        tbBarcode[((STextBox)sender).nRowNum + 1].Focus();
                        if (((STextBox)sender).nRowNum + 1 >= nEndItem)
                        {
                            nEndItem++;
                            nStartItem++;
                            UpdateRowsDisplayed();
                        }
                    }
                }
            }
            else if (e.KeyCode == Keys.F1 && !bEditing)
            {
                fbOptions.ShowDialog();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                SaveChanges(false);
            }
            else if (e.Shift && e.KeyCode == Keys.Delete)
            {
                DeleteRow(((STextBox)sender).nRowNum);
            }
            else if (e.KeyCode == Keys.F2)
            {
                MoveToLine();
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (((STextBox)sender).nRowNum + 1 < tbSupProdCode.Length)
                {
                    MoveToLine(((STextBox)sender).nRowNum + 1);
                    tbSupProdCode[((STextBox)sender).nRowNum + 1].Focus();
                }
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (((STextBox)sender).nRowNum > 0)
                {
                    MoveToLine(((STextBox)sender).nRowNum);
                    tbSupProdCode[((STextBox)sender).nRowNum - 1].Focus();
                }
            }
        }

        void SupCostKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    if (tbSupCost[((STextBox)sender).nRowNum].Text != "")
                    {
                        tbSupCost[((STextBox)sender).nRowNum].Text = FormatMoneyForDisplay(tbSupCost[((STextBox)sender).nRowNum].Text);
                    }
                    else
                    {
                        tbSupCost[((STextBox)sender).nRowNum].Text = "0.00";
                    }
                    tbSupProdCode[((STextBox)sender).nRowNum].Focus();
                    if (tbType[((STextBox)sender).nRowNum].ToString() != "6")
                    {
                        try
                        {
                            decimal dVATRate = 1 + (sEngine.GetVATRateFromVATCode(tbVAT[((STextBox)sender).nRowNum].Text) / 100);
                            decimal dGross = Convert.ToDecimal(tbRRP[((STextBox)sender).nRowNum].Text);
                            decimal dNet = dGross / dVATRate;
                            decimal dCost = Convert.ToDecimal(((STextBox)sender).Text);
                            decimal dProfitAmount = dNet - dCost;
                            decimal dProfitPercent = (100 / dNet) * dProfitAmount;
                            lblProfit[((STextBox)sender).nRowNum].Text = FormatMoneyForDisplay(dProfitPercent);
                        }
                        catch
                        {
                            lblProfit[((STextBox)sender).nRowNum].Text = "-";
                        }
                    }
                    else
                    {
                        lblProfit[((STextBox)sender).nRowNum].Text = "100.00";
                    }

                }
                catch
                {
                    if (tbSupCode[((STextBox)sender).nRowNum].Text == "")
                    {
                        tbSupProdCode[((STextBox)sender).nRowNum].Focus();
                    }
                    else
                    {
                        MessageBox.Show("Invalid amount entered");
                    }
                }
            }
            else if (e.KeyCode == Keys.F1 && !bEditing)
            {
                fbOptions.ShowDialog();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                SaveChanges(false);
            }
            else if (e.Shift && e.KeyCode == Keys.Delete)
            {
                DeleteRow(((STextBox)sender).nRowNum);
            }
            else if (e.KeyCode == Keys.F2)
            {
                MoveToLine();
            }
            else if (e.KeyCode == Keys.F3)
            {
                frmSingleInputBox fGetPrice = new frmSingleInputBox("Enter the cost price to change all items to :", ref sEngine);
                fGetPrice.ShowDialog();
                if (fGetPrice.Response != "$NONE")
                {
                    for (int i = 0; i < tbRRP.Length; i++)
                    {
                        tbSupCost[i].Text = fGetPrice.Response;
                    }
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (((STextBox)sender).nRowNum + 1 < tbSupCost.Length)
                {
                    MoveToLine(((STextBox)sender).nRowNum + 1);
                    tbSupCost[((STextBox)sender).nRowNum + 1].Focus();
                }
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (((STextBox)sender).nRowNum > 0)
                {
                    MoveToLine(((STextBox)sender).nRowNum);
                    tbSupCost[((STextBox)sender).nRowNum - 1].Focus();
                }
            }
        }

        void SupCodeKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                tbSupCost[((STextBox)sender).nRowNum].Focus();
            }
            else if (e.KeyCode == Keys.F5)
            {
                if (tbType[((STextBox)sender).nRowNum].Text != "6")
                {
                    frmListOfSuppliers flos = new frmListOfSuppliers(ref sEngine);
                    flos.ShowDialog();
                    if (flos.sSelectedSupplierCode != "NULL")
                    {
                        ((STextBox)sender).Text = flos.sSelectedSupplierCode;
                        tbSupCost[((STextBox)sender).nRowNum].Focus();
                    }
                }
                else
                {
                    frmListOfCommissioners floc = new frmListOfCommissioners(ref sEngine);
                    floc.ShowDialog();
                    if (floc.Commissioner != "$NONE")
                    {
                        ((STextBox)sender).Text = floc.Commissioner;
                        tbSupCost[((STextBox)sender).nRowNum].Focus();
                    }
                }
            }
            else if (e.KeyCode == Keys.F1 && !bEditing)
            {
                fbOptions.ShowDialog();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                SaveChanges(false);
            }
            else if (e.Shift && e.KeyCode == Keys.Delete)
            {
                DeleteRow(((STextBox)sender).nRowNum);
            }
            else if (e.KeyCode == Keys.F2)
            {
                MoveToLine();
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (((STextBox)sender).nRowNum + 1 < tbSupCode.Length)
                {
                    MoveToLine(((STextBox)sender).nRowNum + 1);
                    tbSupCode[((STextBox)sender).nRowNum + 1].Focus();
                }
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (((STextBox)sender).nRowNum > 0)
                {
                    MoveToLine(((STextBox)sender).nRowNum);
                    tbSupCode[((STextBox)sender).nRowNum - 1].Focus();
                }
            }
        }

        void MinQTYKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                tbSupCode[((STextBox)sender).nRowNum].Focus();
            }
            else if (e.KeyCode == Keys.F1 && !bEditing)
            {
                fbOptions.ShowDialog();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                SaveChanges(false);
            }
            else if (e.Shift && e.KeyCode == Keys.Delete)
            {
                DeleteRow(((STextBox)sender).nRowNum);
            }
            else if (e.KeyCode == Keys.F2)
            {
                MoveToLine();
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (((STextBox)sender).nRowNum + 1 < tbMinQty.Length)
                {
                    MoveToLine(((STextBox)sender).nRowNum + 1);
                    tbMinQty[((STextBox)sender).nRowNum + 1].Focus();
                }
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (((STextBox)sender).nRowNum > 0)
                {
                    MoveToLine(((STextBox)sender).nRowNum);
                    tbMinQty[((STextBox)sender).nRowNum - 1].Focus();
                }
            }
        }

        void VATKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (!bEditing)
                    tbMinQty[((STextBox)sender).nRowNum].Focus();
                else
                    tbSupCost[((STextBox)sender).nRowNum].Focus();
            }
            else if (e.KeyCode == Keys.F5)
            {
                frmListOfVATRates flovr = new frmListOfVATRates(ref sEngine);
                flovr.ShowDialog();
                if (flovr.sSelectedVATCode != "NULL")
                {
                    ((STextBox)sender).Text = flovr.sSelectedVATCode;
                    tbMinQty[((STextBox)sender).nRowNum].Focus();
                }
            }
            else if (e.KeyCode == Keys.F1 && !bEditing)
            {
                fbOptions.ShowDialog();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                SaveChanges(false);
            }
            else if (e.Shift && e.KeyCode == Keys.Delete)
            {
                DeleteRow(((STextBox)sender).nRowNum);
            }
            else if (e.KeyCode == Keys.F2)
            {
                MoveToLine();
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (((STextBox)sender).nRowNum + 1 < tbVAT.Length)
                {
                    MoveToLine(((STextBox)sender).nRowNum + 1);
                    tbVAT[((STextBox)sender).nRowNum + 1].Focus();
                }
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (((STextBox)sender).nRowNum > 0)
                {
                    MoveToLine(((STextBox)sender).nRowNum);
                    tbVAT[((STextBox)sender).nRowNum - 1].Focus();
                }
            }
        }
        
        void RRPKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                tbVAT[((STextBox)sender).nRowNum].Focus();
                /*try
                {
                    tbRRP[((STextBox)sender).nRowNum].Text = FormatMoneyForDisplay(tbRRP[((STextBox)sender).nRowNum].Text);
                    tbVAT[((STextBox)sender).nRowNum].Focus();
                }
                catch
                {
                    MessageBox.Show("Invalid Amount Entered for the RRP.");
                }*/
            }
            else if (e.KeyCode == Keys.F1 && !bEditing)
            {
                fbOptions.ShowDialog();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                SaveChanges(false);
            }
            else if (e.Shift && e.KeyCode == Keys.Delete)
            {
                DeleteRow(((STextBox)sender).nRowNum);
            }
            else if (e.KeyCode == Keys.F2)
            {
                MoveToLine();
            }
            else if (e.KeyCode == Keys.F3)
            {
                frmSingleInputBox fCurrentPrice = new frmSingleInputBox("Where the price is currently ...", ref sEngine);
                fCurrentPrice.tbResponse.Text = ((STextBox)sender).Text;
                fCurrentPrice.ShowDialog();
                if (fCurrentPrice.Response != "$NONE")
                {
                    frmSingleInputBox fNewPrice = new frmSingleInputBox("Change items whose RRP is " + fCurrentPrice.Response + " to...", ref sEngine);
                    fNewPrice.ShowDialog();
                    if (fNewPrice.Response != "$NONE")
                    {
                        for (int i = 0; i < tbRRP.Length; i++)
                        {
                            if (Convert.ToDecimal(tbRRP[i].Text) == Convert.ToDecimal(fCurrentPrice.Response))
                                tbRRP[i].Text = fNewPrice.Response;
                        }
                    }
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (((STextBox)sender).nRowNum + 1 < tbRRP.Length)
                {
                    MoveToLine(((STextBox)sender).nRowNum + 1);
                    tbRRP[((STextBox)sender).nRowNum + 1].Focus();
                }
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (((STextBox)sender).nRowNum > 0)
                {
                    MoveToLine(((STextBox)sender).nRowNum);
                    tbRRP[((STextBox)sender).nRowNum - 1].Focus();
                }
            }
        }

        void CategoryKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (tbType[((STextBox)sender).nRowNum].Text == "5")
                {
                    GetParentAndChildInfo(sender);
                }
                else
                    tbRRP[((STextBox)sender).nRowNum].Focus();
            }
            else if (e.KeyCode == Keys.F5)
            {
                frmCategorySelect fcs = new frmCategorySelect(ref sEngine);
                fcs.ShowDialog();
                if (fcs.SelectedItemCategory != "$NULL")
                {
                    tbCategory[((STextBox)sender).nRowNum].Text = fcs.SelectedItemCategory;
                    if (tbType[((STextBox)sender).nRowNum].Text == "5")
                    {
                        GetParentAndChildInfo(sender);
                    }
                    else
                        tbRRP[((STextBox)sender).nRowNum].Focus();
                }
            }
            else if (e.KeyCode == Keys.F1 && !bEditing)
            {
                fbOptions.ShowDialog();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                SaveChanges(false);
            }
            else if (e.Shift && e.KeyCode == Keys.Delete)
            {
                DeleteRow(((STextBox)sender).nRowNum);
            }
            else if (e.KeyCode == Keys.F2)
            {
                MoveToLine();
            }
            else if (e.KeyCode == Keys.F3)
            {
                frmSingleInputBox fsfi = new frmSingleInputBox("Where the category is currently...", ref sEngine);
                fsfi.tbResponse.Text = ((STextBox)sender).Text;
                fsfi.ShowDialog();
                if (fsfi.Response != "$NONE")
                {
                    frmSingleInputBox fsfiGetCat = new frmSingleInputBox("Change it to...", ref sEngine);
                    fsfiGetCat.ShowDialog();
                    if (fsfiGetCat.Response != "$NONE")
                    {
                        for (int i = 0; i < tbCategory.Length; i++)
                        {
                            if (tbCategory[i].Text.ToUpper() == fsfi.Response.ToUpper())
                            {
                                tbCategory[i].Text = fsfiGetCat.Response.ToUpper();
                            }
                        }
                    }
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (((STextBox)sender).nRowNum +1 < tbCategory.Length)
                {
                    MoveToLine(((STextBox)sender).nRowNum + 1);
                    tbCategory[((STextBox)sender).nRowNum + 1].Focus();
                }
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (((STextBox)sender).nRowNum > 0)
                {
                    MoveToLine(((STextBox)sender).nRowNum);
                    tbCategory[((STextBox)sender).nRowNum - 1].Focus();
                }
            }
        }

        void GetParentAndChildInfo(object sender)
        {
            frmSingleInputBox fsParentCode = new frmSingleInputBox("Enter the barcode of the parent item. (F5 to view choices)", ref sEngine);
            fsParentCode.ProductLookupHelp = "CAT:" + ((STextBox)sender).Text;
            if (sParentBarcodes != null)
                fsParentCode.tbResponse.Text = sParentBarcodes[((STextBox)sender).nRowNum];
            fsParentCode.ShowDialog();
            if (fsParentCode.Response != "$NONE")
            {
                sParentBarcodes[((STextBox)sender).nRowNum] = fsParentCode.Response;
                frmSingleInputBox fsCNumber = new frmSingleInputBox("How many of this item make up the parent item?", ref sEngine);
                if (dChildParts[((STextBox)sender).nRowNum] != null && dChildParts[((STextBox)sender).nRowNum] != 0)
                    fsCNumber.tbResponse.Text = FormatMoneyForDisplay(dChildParts[((STextBox)sender).nRowNum]);
                else
                    fsCNumber.tbResponse.Text = "1.00";
                fsCNumber.ShowDialog();
                if (fsCNumber.Response != "$NONE")
                {
                    try
                    {
                        dChildParts[((STextBox)sender).nRowNum] = Convert.ToDecimal(fsCNumber.Response);
                    }
                    catch
                    {
                        MessageBox.Show("Invalid number entered!");
                    }
                    tbRRP[((STextBox)sender).nRowNum].Focus();
                }
            }
            else
            {
                // Do nowt;
                ;
            }
        }

        void PageUp()
        {
            int nOldStart = nStartItem;
            int nOldEnd = nEndItem;
            nStartItem -= (nEndItem - nStartItem);
            nEndItem -= (nEndItem - nOldStart);
            if (nStartItem < 0)
            {
                nStartItem = 0;
                nEndItem = nOldEnd - (nOldStart - nStartItem);
            }
            UpdateRowsDisplayed();
        }
        void PageDown()
        {
            int nOldStart = nStartItem;
            int nOldEnd = nEndItem;
            nStartItem += (nEndItem - nStartItem);
            nEndItem += (nEndItem - nOldStart);
            if (nEndItem >= nOfItems)
            {
                nEndItem = nOfItems - 1;
                nStartItem = (nEndItem - nOldEnd);
            }
            tbBarcode[nStartItem].Focus();
            UpdateRowsDisplayed();
        }
    


        void TypeKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                tbCategory[((STextBox)sender).nRowNum].Focus();
            }
            else if (e.KeyCode == Keys.F5)
            {
                frmListOfItemTypes flit = new frmListOfItemTypes();
                int nCurrentType = 1;
                try
                {
                    nCurrentType = Convert.ToInt32(((STextBox)sender).Text);
                }
                catch
                {
                    ;
                }
                flit.ShowDialog();
                if (flit.SelectedItemType != -1)
                {
                    if (flit.SelectedItemType == 4)
                    {
                        MessageBox.Show("Sorry, you can't add a multi-item using batch add.");
                    }
                    else
                    {
                        ((STextBox)sender).Text = flit.SelectedItemType.ToString();
                        tbCategory[((STextBox)sender).nRowNum].Focus();
                    }
                }
            }
            else if (e.KeyCode == Keys.F1 && !bEditing)
            {
                fbOptions.ShowDialog();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                SaveChanges(false);
            }
            else if (e.Shift && e.KeyCode == Keys.Delete)
            {
                DeleteRow(((STextBox)sender).nRowNum);
            }
            else if (e.KeyCode == Keys.F2)
            {
                MoveToLine();
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (((STextBox)sender).nRowNum + 1< tbType.Length)
                {
                    MoveToLine(((STextBox)sender).nRowNum + 1);
                    tbType[((STextBox)sender).nRowNum + 1].Focus();
                }
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (((STextBox)sender).nRowNum > 0)
                {
                    MoveToLine(((STextBox)sender).nRowNum);
                    tbType[((STextBox)sender).nRowNum - 1].Focus();
                }
            }
        }

        void DescKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (((STextBox)sender).Text.Contains("#"))
                {
                    int nSelectionStart = -1;
                    int nSelectionLength = 0;
                    for (int i = 0; i < ((STextBox)sender).Text.Length; i++)
                    {
                        if (((STextBox)sender).Text[i] == '#')
                        {
                            nSelectionStart = i;
                            break;
                        }
                    }
                    if (nSelectionStart != -1)
                    {
                        while ((nSelectionLength + nSelectionStart) < ((STextBox)sender).Text.Length && ((STextBox)sender).Text[nSelectionLength + nSelectionStart] == '#')
                            nSelectionLength++;
                        ((STextBox)sender).Select(nSelectionStart, nSelectionLength);
                        e.SuppressKeyPress = true;
                    }
                }
                else
                {
                    tbType[((STextBox)sender).nRowNum].Focus();
                }
            }
            else if (e.KeyCode == Keys.F1 && !bEditing)
            {
                fbOptions.ShowDialog();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                SaveChanges(false);
            }
            else if (e.Shift && e.KeyCode == Keys.Delete)
            {
                DeleteRow(((STextBox)sender).nRowNum);
            }
            else if (e.KeyCode == Keys.F2)
            {
                MoveToLine();
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (((STextBox)sender).nRowNum + 1 < tbDesc.Length)
                {
                    MoveToLine(((STextBox)sender).nRowNum + 1);
                    tbDesc[((STextBox)sender).nRowNum + 1].Focus();
                }
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (((STextBox)sender).nRowNum > 0)
                {
                    MoveToLine(((STextBox)sender).nRowNum);
                    tbDesc[((STextBox)sender).nRowNum - 1].Focus();
                }
            }
            else if (e.KeyCode == Keys.PageDown)
                PageDown();
        }

        void BarcodeKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                tbDesc[((STextBox)sender).nRowNum].Focus();
                if (!bEditing)
                {
                    if (((STextBox)sender).Text.Length != 0 && ((STextBox)sender).Text != fbOptions.BarcodeStart)
                    {
                        tbDesc[((STextBox)sender).nRowNum].Focus();
                        if (sEngine.GetMainStockInfo(((STextBox)sender).Text).Length > 1)
                        {
                            MessageBox.Show("This item already exists, continuing with this code will edit the item instead of create a new one.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        if (tbBarcode.Length == 1 && tbBarcode[0].Text == "")
                        {
                            bClosing = true;
                            this.Close();
                        }
                        else
                        {
                            DeleteRow(((STextBox)sender).nRowNum);
                            this.Refresh();
                            SaveChanges(false);
                        }
                    }
                }
            }
            else if (e.KeyCode == Keys.F1 && !bEditing)
            {
                fbOptions.ShowDialog();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                if (tbBarcode.Length != 1 && tbBarcode[0].Text != "" || (fbOptions.BarcodeStart.Length > 0 && tbBarcode[0].Text != fbOptions.BarcodeStart && tbBarcode.Length >= 1))
                    SaveChanges(false);
                else
                {
                    bClosing = true;
                    this.Close();
                }
            }
            else if (e.Shift && e.KeyCode == Keys.Delete)
            {
                DeleteRow(((STextBox)sender).nRowNum);
            }
            else if (e.KeyCode == Keys.F2)
            {
                MoveToLine();
            }
            else if (e.KeyCode == Keys.F6 && !bEditing)
            {
                tbBarcode[((STextBox)sender).nRowNum].Text = sEngine.GetNextAutoBarcode();
                tbDesc[((STextBox)sender).nRowNum].Focus();
            }
            else if (e.KeyCode == Keys.PageDown)
            {
                PageDown();
            }
            else if (e.KeyCode == Keys.PageUp)
            {
                //PageUp();
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (((STextBox)sender).nRowNum + 1 < tbBarcode.Length)
                {
                    MoveToLine(((STextBox)sender).nRowNum + 1);
                    tbBarcode[((STextBox)sender).nRowNum + 1].Focus();
                }
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (((STextBox)sender).nRowNum > 0)
                {
                    MoveToLine(((STextBox)sender).nRowNum);
                    tbBarcode[((STextBox)sender).nRowNum - 1].Focus();
                }
            }
        }

        bool CheckRow(int nRow)
        {
            return true;
            bool bOK = true;
            try
            {
                decimal dAmount = Convert.ToDecimal(tbSupCost[nRow].Text);
                if (dAmount < 0)
                {
                    MessageBox.Show("Supplier cost is invalid!");
                    tbSupCost[nRow].Focus();
                    tbSupCost[nRow].BackColor = Color.Red;
                    bOK = false;
                }
                decimal dVATMultiply = 1 + (sEngine.GetVATRateFromVATCode(tbVAT[nRow].Text) / 100);
                decimal dRetailPrice = Convert.ToDecimal(tbRRP[nRow].Text);
                if (dAmount * dVATMultiply >= dRetailPrice)
                {
                    if (MessageBox.Show("You will be making either a loss or no profit on this item. Is this correct?", "Loss!?", MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        tbSupCost[nRow].BackColor = Color.Orange;
                        tbSupCost[nRow].Focus();
                        bOK = false;
                    }
                }
            }
            catch
            {
                if (tbSupCode[nRow].Text != "")
                {
                    MessageBox.Show("Error trying to calculate profit. Check supplier cost & RRP");
                    tbSupCost[nRow].BackColor = Color.Red;
                    tbRRP[nRow].BackColor = Color.Red;
                    bOK = false;
                }
            }
            if ((sEngine.GetSupplierDetails(tbSupCode[nRow].Text)[0] == null && tbType[nRow].Text != "6") || (tbType[nRow].Text == "6" && sEngine.GetCommissionerName(tbSupCode[nRow].Text) == ""))
            {
                MessageBox.Show("Supplier code doesn't exist!");
                tbSupCode[nRow].Focus();
                tbSupCode[nRow].BackColor = Color.Red;
                bOK = false;
            }
            if (sEngine.GetVATRateFromVATCode(tbVAT[nRow].Text) == -1)
            {
                MessageBox.Show("VAT Code doesn't exist!");
                tbVAT[nRow].Focus();
                tbVAT[nRow].BackColor = Color.Red;
                bOK = false;
            }
            try
            {
                decimal dAmount = Convert.ToDecimal(tbRRP[nRow].Text);
                if (dAmount <= 0)
                {
                    MessageBox.Show("RRP is an invalid amount!");
                    tbRRP[nRow].Focus();
                    tbRRP[nRow].BackColor = Color.Red;
                    bOK = false;
                }
            }
            catch
            {
                MessageBox.Show("RRP is an invalid amount!");
                tbRRP[nRow].Focus();
                tbRRP[nRow].BackColor = Color.Red;
                bOK = false;
            }
            if (sEngine.GetCategoryDesc(tbCategory[nRow].Text) == "" || sEngine.GetCategoryDesc(tbCategory[nRow].Text) == null)
            {
                MessageBox.Show("Category code doesn't exist!");
                tbCategory[nRow].Focus();
                tbCategory[nRow].BackColor = Color.Red;
                bOK = false;
            }
            try
            {
                int nItemType = Convert.ToInt32(tbType[nRow].Text);
                if (nItemType < 1 || nItemType > 6)
                {
                    MessageBox.Show("Item is an invalid type!");
                    tbType[nRow].Focus();
                    tbType[nRow].BackColor = Color.Red;
                    bOK = false;
                }
                else if (nItemType == 4)
                {
                    MessageBox.Show("Sorry, you can't add a multi-item using batch add.");
                }
            }
            catch
            {
                MessageBox.Show("Item is an invalid type!");
                tbType[nRow].Focus();
                tbType[nRow].BackColor = Color.Red;
                bOK = false;
            }
            if (tbDesc[nRow].Text.Length > 30)
            {
                MessageBox.Show("Description is too long!");
                tbDesc[nRow].Focus();
                tbDesc[nRow].BackColor = Color.Red;
                bOK = false;
            }
            if (tbDesc[nRow].Text.Length == 0)
            {
                MessageBox.Show("Description hasn't been filled in!");
                tbDesc[nRow].Focus();
                tbDesc[nRow].BackColor = Color.Red;
                bOK = false;
            }
            for (int i = 0; i < nOfItems-1; i++)
            {
                if (tbBarcode[i].Text == tbBarcode[nRow].Text && tbBarcode[i].nRowNum != i)
                {
                    MessageBox.Show("Item has already been entered");
                    tbBarcode[nRow].BackColor = Color.Red;
                    tbBarcode[nRow].Focus();
                    tbBarcode[i].BackColor = Color.Red;
                    bOK = false;
                }
            }
            if (sEngine.GetMainStockInfo(tbBarcode[nRow].Text).Length > 1)
            {
                // Item already exists
                if (!bEditing)
                {
                    MessageBox.Show("Item already exists!");
                    tbBarcode[nRow].Focus();
                    tbBarcode[nRow].BackColor = Color.Red;
                    bOK = false;
                }
            }
            if (tbBarcode[nRow].Text.Length > 13)
            {
                MessageBox.Show("Barcode is too long!");
                tbBarcode[nRow].Focus();
                tbBarcode[nRow].BackColor = Color.Red;
                bOK = false;
            }
            return bOK;
        }

        void ClearBackColourRows()
        {
            for (int i = 0; i < tbBarcode.Length; i++)
            {
                tbBarcode[i].BackColor = Color.White;
                tbCategory[i].BackColor = Color.White;
                tbDesc[i].BackColor = Color.White;
                tbMinQty[i].BackColor = Color.White;
                tbRRP[i].BackColor = Color.White;
                tbSupCode[i].BackColor = Color.White;
                tbSupCost[i].BackColor = Color.White;
                tbSupProdCode[i].BackColor = Color.White;
                tbType[i].BackColor = Color.White;
                tbVAT[i].BackColor = Color.White;
            }
        }
        public bool bSaved = false;
        void SaveChanges(bool bFull)
        {
            switch (MessageBox.Show("Would you like to save the new items?", "Save or Discard?", MessageBoxButtons.YesNoCancel))
            {
                case DialogResult.Yes:
                    if (AddAllItems())
                    {
                        if (!bFull)
                        {
                            // Removed bReceiving to fix it not asking when receiving commission items
                            // Previously checked that !bReceiving
                            // May need to fix this in the future?
                            if (MessageBox.Show("Would you like to upload the information to all tills now?", "Upload now?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                sEngine.CopyWaitingFilesToTills();
                            }
                            bSaved = true;
                            bClosing = true;
                            this.Close();
                        }
                    }
                    break;
                case DialogResult.No:
                    bClosing = true;
                    this.Close();
                    break;
            }
        }

        /// <summary>
        /// Adds all of the items that the user has entered
        /// </summary>
        /// <returns>True if all items were added successfully</returns>
        bool AddAllItems()
        {
            bool bAllok = true;
            int nofItemsOK = 0;
            // Go through each item and check that it's acceptable
            for (int i = nOfItems - 1; i >= 0; i -= 1)
            {
                if (!CheckRow(i))
                    bAllok = false;
                else
                    nofItemsOK++;
            }
            // If a row starts with the BarcodeStart string, then delete the row as the user hasn't added anything
            if (!bEditing && tbBarcode[nOfItems - 1].Text == fbOptions.BarcodeStart)
            {
                DeleteRow(nOfItems - 1);
                nofItemsOK -= 1;
            }
            if (bAllok)
            {
                frmProgressBar fp = new frmProgressBar("Loading Items");
                fp.pb.Maximum = nofItemsOK;
                fp.Show();
                for (int i = 0; i < nofItemsOK; i++)
                {
                    // If the user is receiving, then ?? Why does this take place? May be a bug? Wish I made comments whilst coding this.
                    if (!bReceiving)
                    {
                        AddItem(tbType[i].Text, tbBarcode[i].Text, tbDesc[i].Text, tbCategory[i].Text, tbRRP[i].Text, tbVAT[i].Text,
                            tbMinQty[i].Text, Convert.ToDecimal(tbSupCost[i].Text), sParentBarcodes[i], dChildParts[i],
                            tbSupCode[i].Text, Convert.ToDecimal(tbSupCost[i].Text), tbSupProdCode[i].Text);
                    }
                    else
                    {
                        AddItem(tbType[i].Text, tbBarcode[i].Text, tbDesc[i].Text, tbCategory[i].Text, tbRRP[i].Text, tbVAT[i].Text,
                            "1.00", Convert.ToDecimal(tbSupCost[i].Text), sParentBarcodes[i], dChildParts[i],
                            tbSupCode[i].Text, Convert.ToDecimal(tbSupCost[i].Text), tbSupProdCode[i].Text);
                    }
                    fp.pb.Value = i;
                }
                fp.Close();
            }
            return bAllok;
        }

        public decimal GetQuantityReceiving(string sBarcode)
        {
            for (int i = 0; i < tbBarcode.Length; i++)
            {
                if (sBarcode == tbBarcode[i].Text)
                {
                    try { return Convert.ToDecimal(tbMinQty[i].Text); }
                    catch { ;}
                }
            }
            return 0;
        }

        public string[] GetListOfBarcodes()
        {
            string[] sList = new string[tbBarcode.Length];
            for (int i = 0; i < tbBarcode.Length; i++)
            {
                sList[i] = tbBarcode[i].Text;
            }
            return sList;
        }

        /// <summary>
        /// Adds an individial row to the databases
        /// </summary>
        /// <param name="sType">The item's type (1=Stock...)</param>
        /// <param name="sBarcode">The item's barcode</param>
        /// <param name="sDesc">The item's description</param>
        /// <param name="sCategory">The category of the item</param>
        /// <param name="sRRP">The RRP of the item</param>
        /// <param name="sVAT">The VAT code of the item</param>
        /// <param name="sMinQty">The minimum quantity of the item that can be ordered</param>
        /// <param name="dCatTwoItemCost"></param>
        /// <param name="sParentCode"></param>
        /// <param name="dCPartQTY"></param>
        /// <param name="sSupplier"></param>
        /// <param name="dSupCost"></param>
        /// <param name="sSupCode"></param>
        void AddItem(string sType, string sBarcode, string sDesc, string sCategory, string sRRP, string sVAT, string sMinQty, decimal dCatTwoItemCost, string sParentCode, decimal dCPartQTY, string sSupplier, decimal dSupCost, string sSupCode)
        {
            string sShopCode = "";
            if (bEditing)
                sShopCode = fbEdit.ShopCode;
            else
                sShopCode = fbOptions.ShopCode;
            sEngine.RemoveSuppliersForItem(sBarcode, sSupplier);
            if (sType == "1" || sType == "3")
            {
                sEngine.AddEditItem(sShopCode, sBarcode, sDesc,
                    sType, sCategory, sRRP, sVAT,
                    sMinQty, FormatMoneyForDisplay(dSupCost));
                if (sSupplier != "")
                    sEngine.AddSupplierForItem(sBarcode, sSupplier, dSupCost.ToString(), sSupCode);
                
            }
            else if (sType == "2")
            {
                try
                {
                    sEngine.AddEditItem(sShopCode, sBarcode, sDesc, sType,
                        sCategory, sVAT, sMinQty, dCatTwoItemCost, Convert.ToDecimal(sEngine.GetItemStockStaRecord(sBarcode, sShopCode)[39])); // Last field should be target margin
                }
                catch
                {
                    sEngine.AddEditItem(sShopCode, sBarcode, sDesc, sType,
                        sCategory, sVAT, sMinQty, dCatTwoItemCost, 0.00m); // Last field should be target margin

                }
            }
            else if (sType == "5")
            {
                if (sBarcode == "50723409")
                {
                    ;
                }
                sEngine.AddEditItem(sShopCode, sBarcode, sDesc,
                    sType, sCategory, sRRP, sVAT,
                    sMinQty, sParentCode, dCPartQTY.ToString());
            }
            else if (sType == "6")
            {
                sEngine.AddEditItem(sShopCode, sBarcode, sDesc,
                    sType, sCategory, sRRP, sVAT,
                    "1", sSupplier, dSupCost, sMinQty);

                sEngine.ReceiveComissionItem(sBarcode, sMinQty, sShopCode);
            }
        }

        void DeleteRow(int nRow)
        {
            for (int i = nRow; i < nOfItems - 1; i++)
            {
                tbBarcode[i].Text = tbBarcode[i + 1].Text;
                tbCategory[i].Text = tbCategory[i + 1].Text;
                tbDesc[i].Text = tbDesc[i + 1].Text;
                tbMinQty[i].Text = tbMinQty[i + 1].Text;
                tbRRP[i].Text = tbRRP[i + 1].Text;
                tbSupCode[i].Text = tbSupCode[i + 1].Text;
                tbSupCost[i].Text = tbSupCost[i + 1].Text;
                tbSupProdCode[i].Text = tbSupProdCode[i + 1].Text;
                tbType[i].Text = tbType[i + 1].Text;
                tbVAT[i].Text = tbVAT[i + 1].Text;
                lblProfit[i].Text = lblProfit[i + 1].Text;
            }
            this.Controls.Remove(lblRowNums[lblRowNums.Length - 1]);
            this.Controls.Remove(tbBarcode[tbBarcode.Length - 1]);
            this.Controls.Remove(tbCategory[tbCategory.Length - 1]);
            this.Controls.Remove(tbDesc[tbDesc.Length - 1]);
            this.Controls.Remove(tbMinQty[tbMinQty.Length - 1]);
            this.Controls.Remove(tbRRP[tbRRP.Length - 1]);
            this.Controls.Remove(tbSupCode[tbSupCode.Length - 1]);
            this.Controls.Remove(tbSupCost[tbSupCost.Length - 1]);
            this.Controls.Remove(tbSupProdCode[tbSupProdCode.Length - 1]);
            this.Controls.Remove(tbType[tbType.Length - 1]);
            this.Controls.Remove(tbVAT[tbVAT.Length - 1]);
            this.Controls.Remove(lblProfit[lblProfit.Length - 1]);
            Array.Resize<Label>(ref lblRowNums, lblRowNums.Length - 1);
            Array.Resize<STextBox>(ref tbBarcode, tbBarcode.Length - 1);
            Array.Resize<STextBox>(ref tbCategory, tbCategory.Length - 1);
            Array.Resize<STextBox>(ref tbDesc, tbDesc.Length - 1);
            Array.Resize<STextBox>(ref tbMinQty, tbMinQty.Length - 1);
            Array.Resize<STextBox>(ref tbRRP, tbRRP.Length - 1);
            Array.Resize<STextBox>(ref tbSupCode, tbSupCode.Length - 1);
            Array.Resize<STextBox>(ref tbSupCost, tbSupCost.Length - 1);
            Array.Resize<STextBox>(ref tbSupProdCode, tbSupProdCode.Length - 1);
            Array.Resize<STextBox>(ref tbType, tbType.Length - 1);
            Array.Resize<STextBox>(ref tbVAT, tbVAT.Length - 1);
            Array.Resize<Label>(ref lblProfit, lblProfit.Length - 1);
        }

        void MoveToLine()
        {
            frmSingleInputBox fsi = new frmSingleInputBox("Which line would you like to go to? (First Row : 1, End Row : " + nOfItems.ToString() + ")", ref sEngine);
            fsi.ShowDialog();
            if (fsi.Response != "$NONE")
            {
                try
                {
                    tbBarcode[Convert.ToInt32(fsi.Response) - 1].Focus();
                    if (Convert.ToInt32(fsi.Response) - 1 >= nEndItem)
                    {
                        int nOldEnd = nEndItem;
                        nEndItem = Convert.ToInt32(fsi.Response);
                        nStartItem += (nEndItem - nOldEnd);
                        UpdateRowsDisplayed();
                        tbBarcode[Convert.ToInt32(fsi.Response) - 1].Focus();
                    }
                    else if (Convert.ToInt32(fsi.Response) - 1 < nStartItem)
                    {
                        int nOldStart = nStartItem;
                        nStartItem = Convert.ToInt32(fsi.Response) -1;
                        nEndItem -= (nStartItem + nOldStart);
                        UpdateRowsDisplayed();
                        tbBarcode[Convert.ToInt32(fsi.Response) - 1].Focus();
                    }
                    
                }
                catch
                {
                    return;
                }
            }
        }
        void MoveToLine(int nLine)
        {
            try
            {
                if (!bEditing)
                {
                    tbBarcode[nLine - 1].Focus();
                    if (nLine - 1 >= nEndItem)
                    {
                        int nOldEnd = nEndItem;
                        nEndItem = nLine;
                        nStartItem += (nEndItem - nOldEnd);
                        UpdateRowsDisplayed();
                        tbBarcode[nLine - 1].Focus();
                    }
                    else if (nLine - 1 < nStartItem)
                    {
                        int nOldStart = nStartItem;
                        nStartItem = nLine - 1;
                        nEndItem -= (nOldStart - nStartItem);
                        UpdateRowsDisplayed();
                        tbBarcode[nLine - 1].Focus();
                    }
                }
                else
                {
                    tbDesc[nLine - 1].Focus();
                    if (nLine - 1 >= nEndItem)
                    {
                        int nOldEnd = nEndItem;
                        nEndItem = nLine;
                        nStartItem += (nEndItem - nOldEnd);
                        UpdateRowsDisplayed();
                        tbDesc[nLine - 1].Focus();
                    }
                    else if (nLine - 1 < nStartItem)
                    {
                        int nOldStart = nStartItem;
                        nStartItem = nLine - 1;
                        nEndItem -= (nOldStart - nStartItem);
                        UpdateRowsDisplayed();
                        tbDesc[nLine - 1].Focus();
                    }
                }


            }
            catch
            {
                return;
            }
        }
    }
}
