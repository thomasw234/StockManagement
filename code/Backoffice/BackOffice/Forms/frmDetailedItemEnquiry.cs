using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmDetailedItemEnquiry : ScalableForm
    {
        StockEngine sEngine;
        Label lblTitle;
        Label lblShopName;
        Label lblDateTime;
        Label lblStockCode;
        Label lblStockDesc;
        Label lblStockShopName;
        Label lblCategory;
        Label lblUnits;
        Label lblQuantOnOrder;
        Label[] lblSecondRowData;
        Label[] lblSecondRowStat;
        Label[] lblColumnHeaders;
        Label[] lblRowTitles;
        Label[,] lblStats;
        Label[] lblSecondHeaders;
        Label[] lblSecondRowTitles;
        Label[,] lblSecondStats;

        public string sBarcode;
        string sShopCode;
        string[] sStockStat;
        string[] sMainStock;
        string sItemCategory;
        public bool ForAnOrder = false;
        public bool ItemAdded = false;
        bool bJustEscd = false;


        public frmDetailedItemEnquiry(ref StockEngine se, string sItemToShow)
        {
            sEngine = se;
            frmListOfShops flos = new frmListOfShops(ref sEngine);
            while (flos.SelectedShopCode == "$NONE")
            {
                flos.ShowDialog();
            }
            sShopCode = flos.SelectedShopCode;
            sBarcode = sItemToShow;
            sStockStat = sEngine.GetItemStockStaRecord(sBarcode, sShopCode);
            sMainStock = sEngine.GetMainStockInfo(sBarcode);
            FillInBlanks(sStockStat);
            FillInBlanks(sMainStock);
            SetupForm();
            this.Paint += new PaintEventHandler(frmDetailedItemEnquiry_Paint);
            this.WindowState = FormWindowState.Maximized;
            this.VisibleChanged += new EventHandler(frmDetailedItemEnquiry_VisibleChanged);
            this.Text = "Detailed Item Enquiry";
        }

        void frmDetailedItemEnquiry_VisibleChanged(object sender, EventArgs e)
        {
            if (Screen.PrimaryScreen.Bounds.Width > 1024)
            {
                if (!ForAnOrder)
                    AddInputControl("INPUT", "<A>nother Item, <E>dit Item, <O>utstanding Orders, <R>emind To Order, <S>uppliers, <V>iew Previous Sales, <Q>uit", new Point(10, BelowLastControl), 1100);
                else
                    AddInputControl("INPUT", "<A>nother Item, <I>nclude In Order, <E>dit Item, <O>utstanding Orders, <S>uppliers, <V>iew Previous Sales, <Q>uit", new Point(10, BelowLastControl), 1100);
            }
            else
            {
                if (!ForAnOrder)
                    AddInputControl("INPUT", "<A>nother Item, <E>dit Item, <O>utstanding Orders, <R>emind To Order, <S>uppliers, <Q>uit", new Point(10, BelowLastControl), 1000);
                else
                    AddInputControl("INPUT", "<A>nother Item, <I>nclude In Order, <E>dit Item, <O>utstanding Orders, <S>uppliers, <Q>uit", new Point(10, BelowLastControl), 1000);
            }

            InputTextBox("INPUT").KeyUp += new KeyEventHandler(frmDetailedItemEnquiry_KeyUp);
            InputTextBoxAssociatedLabel("INPUT").Click += new EventHandler(lbAssocClick);
        }

        void lbAssocClick(object sender, EventArgs e)
        {
            /*int nMouseLeft = Control.MousePosition.X - this.Left - ((Label)sender).Left;
            string[] sOptions = ((Label)sender).Text.Split(',');
            for (int i = 0; i < sOptions.Length; i++)
            {
                sOptions[i] += ",";
            }
            int nLastWidth = 0;
            for (int i = 0; i < sOptions.Length; i++)
            {
                int nThisWidth = (int)Math.Round(this.CreateGraphics().MeasureString(sOptions[i], ((Label)sender).Font).Width, 0);
                if (nLastWidth + nThisWidth > nMouseLeft)
                {
                    // Was this one
                    sOptions[i] = sOptions[i].Trim();
                    SendKeys.Send(sOptions[i][1].ToString());
                    break;
                }
                else
                {
                    nLastWidth += nThisWidth;
                }
            }*/
        }

        void frmDetailedItemEnquiry_Paint(object sender, PaintEventArgs e)
        {
            decimal dScale = (this.Height / 768.0m);
            // Title Section
            e.Graphics.DrawRectangle(new Pen(Color.Black, 2.0f), new Rectangle(5, 5, this.Width - 25, (int)(dScale*57) + 8));
            e.Graphics.DrawRectangle(new Pen(Color.Black, 2.0f), new Rectangle(9, 9, this.Width - 33, (int)(dScale*57)));
            //Header section
            e.Graphics.DrawLine(new Pen(Color.Black, 2.0f), new Point(0, (int)(85 * dScale)), new Point(this.Width, (int)(85 * dScale)));
            e.Graphics.DrawLine(new Pen(Color.Black, 2.0f), new Point(0, (int)(dScale *155)), new Point(this.Width, (int)(dScale*155)));
            e.Graphics.DrawLine(new Pen(Color.Black, 2.0f), new Point(lblStockShopName.Left - 10, (int)(dScale *85)), new Point(lblStockShopName.Left - 10, (int)(dScale *155)));
            e.Graphics.DrawLine(new Pen(Color.Black, 2.0f), new Point(lblUnits.Left - 50, (int)(dScale *85)), new Point(lblUnits.Left - 50, (int)(dScale *155)));
            //Second Section
            e.Graphics.DrawLine(new Pen(Color.Black, 2.0f), new Point(0, (int)(dScale*280)), new Point(this.Width, (int)(dScale *280)));
            // Third Section
            if (lblRowTitles != null)
            {
                int nTop = lblRowTitles[0].Top;
                for (int i = 0; i < 7; i++)
                {
                    e.Graphics.DrawLine(new Pen(Color.DarkGray, 1.0f), 0, nTop, this.Width, nTop);
                    nTop += (int)(dScale *35);
                }
            }
            // Fourth Section
            if (lblSecondRowTitles != null)
            {
                int nTop = lblSecondRowTitles[0].Top;
                for (int i = 0; i < 2; i++)
                {
                    e.Graphics.DrawLine(new Pen(Color.DarkGray, 1.0f), 0, nTop, this.Width, nTop);
                    nTop += (int)(dScale*35);
                }
            }
            lblTitle.Width = this.Width;
            lblQuantOnOrder.Left = this.ClientSize.Width - lblQuantOnOrder.Width - 10;
            lblUnits.Left = this.ClientSize.Width - 10 - lblUnits.Width;
        }

        string[] sTopFNames = { "D", "W", "M", "Y", "LY" };
        string[] sSideFNames = {"QSOLD", "GSALES", "NSALES", "COGS"};

        void SetupForm()
        {
            this.Font = new Font(this.Font.FontFamily, 14.0f);
           
            lblShopName = new Label();
            lblShopName.Location = new Point(10, 10);
            lblShopName.Text = sEngine.CompanyName;
            lblShopName.AutoSize = true;
            this.Controls.Add(lblShopName);

            lblDateTime = new Label();
            lblDateTime.Text = DateTime.Now.Day.ToString() + "/" + DateTime.Now.Month.ToString() + "/" + DateTime.Now.Year.ToString();
            lblDateTime.AutoSize = true;
            lblDateTime.BackColor = Color.Transparent;
            lblDateTime.Location = new Point(this.Width - lblDateTime.Width - 33, 10);
            this.Controls.Add(lblDateTime);
            lblDateTime.Visible = false;
            
            lblTitle = new Label();
            lblTitle.AutoSize = false;
            lblTitle.Size = new Size(this.Width, 30); 
            lblTitle.Location = new Point(0, 30);
            lblTitle.Text = "Stock Enquiry";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblTitle);
            lblTitle.Font = new Font(this.Font.FontFamily, 16.0f);
            lblTitle.BackColor = Color.Transparent;

            lblStockCode = new Label();
            lblStockCode.Location = new Point(10, 90);
            if (sMainStock[5] == "1")
                lblStockCode.Text = "Stock Item : ";
            else if (sMainStock[5] == "2")
                lblStockCode.Text = "Department Item : ";
            else if (sMainStock[5] == "3")
                lblStockCode.Text = "Non-Stock Item : ";
            else if (sMainStock[5] == "4")
                lblStockCode.Text = "Multi-Item Item : ";
            else if (sMainStock[5] == "5")
                lblStockCode.Text = "Child Item : ";
            else if (sMainStock[5] == "6")
                lblStockCode.Text = "Commission Item : ";
            lblStockCode.Text += sBarcode;
            lblStockCode.AutoSize = true;
            this.Controls.Add(lblStockCode);
            lblStockCode.ContextMenu.MenuItems[0].Click += new EventHandler(frmDetailedItemEnquiry_Click);

            lblStockDesc = new Label();
            lblStockDesc.AutoSize = true;
            lblStockDesc.Text = sMainStock[1];
            lblStockDesc.Location = new Point(10, lblStockCode.Top + lblStockCode.Height + 5);
            this.Controls.Add(lblStockDesc);

            lblStockShopName = new Label();
            lblStockShopName.AutoSize = true;
            lblStockShopName.Text = "Shop : " + sStockStat[35] + " - " + sEngine.GetShopNameFromCode(sStockStat[35]);
            lblStockShopName.Location = new Point(340, 90);
            this.Controls.Add(lblStockShopName);

            lblCategory = new Label();
            lblCategory.AutoSize = true;
            lblCategory.Text = "Category : " + sEngine.GetCategoryDesc(sMainStock[4]) + " (" + sMainStock[4] + ")";
            lblCategory.Location = new Point(340, lblStockShopName.Top + lblStockShopName.Height + 5);
            sItemCategory = sMainStock[4];
            sEngine.LastCategoryCode = sMainStock[4];
            this.Controls.Add(lblCategory);


            lblUnits = new Label();
            lblUnits.Text = "Minimum Order : " + sStockStat[37];
            lblUnits.AutoSize = true;
            lblUnits.Location = new Point(this.ClientSize.Width - 20 - lblUnits.Width, 90);
            this.Controls.Add(lblUnits);

            lblQuantOnOrder = new Label();
            lblQuantOnOrder.Text = "Quantity On Order : " + sStockStat[3];
            lblQuantOnOrder.AutoSize = true;
            lblQuantOnOrder.Location = new Point(this.ClientSize.Width - 20 - lblQuantOnOrder.Width, 90 + lblUnits.Height + 5);
            this.Controls.Add(lblQuantOnOrder);

            lblSecondRowData = new Label[12];
            lblSecondRowStat = new Label[12];
            int nLeft = 10;
            int nHeight = 160;
            for (int i = 0; i < lblSecondRowData.Length; i++)
            {
                lblSecondRowData[i] = new Label();
                lblSecondRowData[i].Location = new Point(nLeft, nHeight);
                lblSecondRowData[i].AutoSize = true;
                lblSecondRowData[i].Text = i.ToString();
                this.Controls.Add(lblSecondRowData[i]);

                lblSecondRowStat[i] = new Label();
                lblSecondRowStat[i].Location = new Point(nLeft + 225, nHeight);
                if (lblSecondRowStat[i].Location.X + 100 > this.Width)
                    lblSecondRowStat[i].Left = this.Width - 125;
                lblSecondRowStat[i].AutoSize = false;
                lblSecondRowStat[i].Size = new Size(100, 30);
                lblSecondRowStat[i].TextAlign = ContentAlignment.TopRight;
                //lblSecondRowStat[i].Text = "0.00";
                this.Controls.Add(lblSecondRowStat[i]);

                nHeight += lblSecondRowData[0].Height + 2;
                if (((i + 1) % 4) == 0 && i != 0)
                {
                    nHeight = 160;
                    nLeft += 350;
                }
            }
            lblSecondRowData[0].Text = "Selling Price : ";
            lblSecondRowData[1].Text = "Last Received : ";
            lblSecondRowData[3].Text = "Last Sold : ";
            lblSecondRowData[2].Text = "Average Cost : ";
            lblSecondRowData[4].Text = "Current Stock Level : ";
            MenuItem mEditStock = new MenuItem("Edit");
            mEditStock.Click += new EventHandler(mEditStock_Click);
            lblSecondRowData[4].ContextMenu.MenuItems.Add(mEditStock);
            lblSecondRowData[5].Text = "Average Daily Sales : ";
            lblSecondRowData[6].Text = "Number of Day's Stock : ";
            lblSecondRowData[7].Text = "Penultimately Sold : ";
            lblSecondRowData[8].Text = "Profit : ";
            lblSecondRowData[9].Text = "Margin (%) : ";
            lblSecondRowData[10].Text = "Last Cost : ";
            MenuItem mEditLastCost = new MenuItem("Edit");
            mEditLastCost.Click += new EventHandler(mEditLastCost_Click);
            lblSecondRowData[10].ContextMenu.MenuItems.Add(mEditLastCost);
            if (Convert.ToInt32(sMainStock[5]) != 5)
                lblSecondRowData[11].Text = "Out Of Stock : ";
            else
                lblSecondRowData[11].Text = "Parent Barcode : ";
            MenuItem mViewParent = new MenuItem("View Parent Item Information");
            mViewParent.Click += new EventHandler(mViewParent_Click);
            lblSecondRowData[11].ContextMenu.MenuItems.Add(mViewParent);

            lblSecondRowStat[0].Text = FormatMoneyForDisplay(Convert.ToDecimal(sMainStock[2]));
            if (sStockStat[4].Length == 6)
            {
                string sDate = sStockStat[4][0].ToString() + sStockStat[4][1].ToString() + "/" + sStockStat[4][2].ToString() + sStockStat[4][3].ToString() + "/" + sStockStat[4][4].ToString() + sStockStat[4][5].ToString();
                lblSecondRowStat[1].Text = sDate;
            }
            else
            {
                lblSecondRowStat[1].Text = "Unknown";
            }
            if (sStockStat[40].Length != 6)
            {
                lblSecondRowStat[3].Text = "Unknown";
            }
            else
            {
                string sDate = sStockStat[40][0].ToString() + sStockStat[40][1].ToString() + "/" + sStockStat[40][2].ToString() + sStockStat[40][3].ToString() + "/" + sStockStat[40][4].ToString() + sStockStat[40][5].ToString();
                lblSecondRowStat[3].Text = sDate;
            }
            if (sStockStat[41].Length != 6)
            {
                lblSecondRowStat[7].Text = "Unknown";
            }
            else
            {
                string sDate = sStockStat[41][0].ToString() + sStockStat[41][1].ToString() + "/" + sStockStat[41][2].ToString() + sStockStat[41][3].ToString() + "/" + sStockStat[41][4].ToString() + sStockStat[41][5].ToString();
                lblSecondRowStat[7].Text = sDate;
            }
            MenuItem mAveCost = new MenuItem("Edit");
            lblSecondRowStat[2].Text = FormatMoneyForDisplay(Convert.ToDecimal(sStockStat[1]));
            lblSecondRowStat[2].ContextMenu.MenuItems.Add(mAveCost);
            mAveCost.Click += new EventHandler(mAveCost_Click);
            lblSecondRowStat[4].Text = FormatMoneyForDisplay(Convert.ToDecimal(sStockStat[36]));
            lblSecondRowStat[4].ContextMenu.MenuItems.Add(mEditStock);
            lblSecondRowStat[5].Text = Math.Round(Convert.ToDecimal(sStockStat[2]), 3).ToString();
            if (Convert.ToDecimal(sMainStock[2]) <= 0 || Convert.ToDecimal(sStockStat[2]) <= 0)
                lblSecondRowStat[6].Text = "0";
            else
                lblSecondRowStat[6].Text = FormatMoneyForDisplay(Convert.ToDecimal(sStockStat[36]) / Convert.ToDecimal(sStockStat[2]));
            decimal dVATRate = (sEngine.GetVATRateFromCode(sMainStock[0]) / 100) + 1;
            decimal dNet = Convert.ToDecimal(sMainStock[2]) / dVATRate;
            decimal dProfit = dNet - Convert.ToDecimal(sStockStat[1]);
            if (sMainStock[5] == "6")
            {
                dNet = (Convert.ToDecimal(sMainStock[2]) - Convert.ToDecimal(sStockStat[1])) / dVATRate;
                dProfit = dNet;
                dNet += Convert.ToDecimal(sStockStat[1]);
            }
            lblSecondRowStat[8].Text = FormatMoneyForDisplay(dProfit);
            decimal dMargin = 0;
            if (Convert.ToDecimal(sStockStat[1]) <= 0 && sMainStock[5] != "2")
                lblSecondRowStat[9].Text = "";
            else if (sMainStock[5] != "2")
            {
                dMargin = (100 / Convert.ToDecimal(sStockStat[1])) * dProfit;
                lblSecondRowStat[9].Text = FormatMoneyForDisplay(dMargin);
            }
            else
            {
                lblSecondRowData[9].Text = "Target Margin (%) : ";
                lblSecondRowStat[9].Text = FormatMoneyForDisplay(sStockStat[39]);
            }
            lblSecondRowStat[10].Text = FormatMoneyForDisplay(sMainStock[8]);
            lblSecondRowStat[10].ContextMenu.MenuItems.Add(mEditLastCost);
            lblSecondRowStat[11].AutoSize = true;
            if (Convert.ToInt32(sMainStock[5]) == 5)
            {
                lblSecondRowStat[11].Text = sMainStock[7];
            }
            else
            {
                lblSecondRowStat[11].Text = sEngine.GetOutOfStockLength(sBarcode, sShopCode) + "% of the time";
            }
            lblSecondRowStat[11].ContextMenu.MenuItems.Add(mViewParent);
            
 
            lblColumnHeaders = new Label[5];
            nLeft = 250;
            for (int i = 0; i < lblColumnHeaders.Length; i++)
            {
                lblColumnHeaders[i] = new Label();
                lblColumnHeaders[i].AutoSize = false;
                lblColumnHeaders[i].Size = new Size(100, 50);
                lblColumnHeaders[i].Location = new Point(nLeft, 300);
                this.Controls.Add(lblColumnHeaders[i]);
                lblColumnHeaders[i].BackColor = Color.Transparent;
                nLeft += 150;
            }
            lblColumnHeaders[0].Text = "Current Daily Sales";
            lblColumnHeaders[1].Text = "Week To Date Sales";
            lblColumnHeaders[2].Text = "Month To Date Sales";
            lblColumnHeaders[3].Text = "Year To Date Sales";
            lblColumnHeaders[4].Text = "Last Year's Sales";

            lblRowTitles = new Label[6];
            nHeight = 360;
            for (int i = 0; i < lblRowTitles.Length; i++)
            {
                lblRowTitles[i] = new Label();
                lblRowTitles[i].AutoSize = true;
                lblRowTitles[i].Location = new Point(10, nHeight);
                this.Controls.Add(lblRowTitles[i]);
                lblRowTitles[i].BackColor = Color.Transparent;
                nHeight += 35;
            }
            lblRowTitles[0].Text = "Quantity Sold";
            lblRowTitles[1].Text = "Gross Sales";
            lblRowTitles[2].Text = "Net Sales";
            lblRowTitles[3].Text = "Cost Of Goods Sold";
            lblRowTitles[4].Text = "Profit";
            lblRowTitles[5].Text = "Profit (%)";

            lblStats = new Label[5, 6]; // Column, Rown
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 6; y++)
                {
                    lblStats[x, y] = new Label();
                    lblStats[x, y].AutoSize = false;
                    lblStats[x, y].Size = new Size(lblColumnHeaders[x].Width, lblRowTitles[y].Height);
                    lblStats[x, y].Location = new Point(lblColumnHeaders[x].Left, lblRowTitles[y].Top);
                    lblStats[x, y].Text = "0.00";
                    lblStats[x, y].TextAlign = ContentAlignment.MiddleRight;
                    this.Controls.Add(lblStats[x, y]);
                    lblStats[x,y].BackColor = Color.Transparent;

                    if (y >= 0 && y < 4)
                    {
                        MenuItem cEditStats = new MenuItem("Edit");
                        cEditStats.Click += new EventHandler(EditStatsClick);
                        cEditStats.Name = x.ToString() + "," + y.ToString();
                        lblStats[x, y].ContextMenu.MenuItems.Add(cEditStats);
                    }
                }
            }
            //Quantities
            lblStats[0, 0].Text = FormatMoneyForDisplay(sStockStat[5]);
            lblStats[1,0].Text = FormatMoneyForDisplay(sStockStat[9]);
            lblStats[2,0].Text = FormatMoneyForDisplay(sStockStat[13]);
            lblStats[3,0].Text = FormatMoneyForDisplay(sStockStat[17]);
            lblStats[4,0].Text = FormatMoneyForDisplay(sStockStat[25]);

            //Gross Sales
            lblStats[0,1].Text = FormatMoneyForDisplay(sStockStat[6]);
            lblStats[1, 1].Text = FormatMoneyForDisplay(sStockStat[10]);
            lblStats[2,1].Text = FormatMoneyForDisplay(sStockStat[14]);
            lblStats[3,1].Text = FormatMoneyForDisplay(sStockStat[18]);
            lblStats[4,1].Text = FormatMoneyForDisplay(sStockStat[26]);

            //Net Sales
            lblStats[0, 2].Text = FormatMoneyForDisplay(sStockStat[7]);
            lblStats[1, 2].Text = FormatMoneyForDisplay(sStockStat[11]);
            lblStats[2, 2].Text = FormatMoneyForDisplay(sStockStat[15]);
            lblStats[3, 2].Text = FormatMoneyForDisplay(sStockStat[19]);
            lblStats[4, 2].Text = FormatMoneyForDisplay(sStockStat[27]);

            //COGS
            lblStats[0, 3].Text = FormatMoneyForDisplay(sStockStat[8]);
            lblStats[1, 3].Text = FormatMoneyForDisplay(sStockStat[12]);
            lblStats[2, 3].Text = FormatMoneyForDisplay(sStockStat[16]);
            lblStats[3, 3].Text = FormatMoneyForDisplay(sStockStat[20]);
            lblStats[4, 3].Text = FormatMoneyForDisplay(sStockStat[28]);

            //Profit
            for (int x = 0; x < 4; x++)
            {
                decimal dNetSales = Convert.ToDecimal(sStockStat[(4 * x) + 7]);
                decimal dCOGS = Convert.ToDecimal(sStockStat[8 + (x * 4)]);
                decimal dProfitAmount = dNetSales - dCOGS;
                decimal dProfitPercent = 0;
                if (dNetSales != 0)
                    dProfitPercent = (100 / dNetSales) * dProfitAmount;
                lblStats[x, 4].Text = FormatMoneyForDisplay(dProfitAmount);
                lblStats[x, 5].Text = FormatMoneyForDisplay(dProfitPercent);
            }
            // LYProfit
            decimal dLYNetSales = Convert.ToDecimal(sStockStat[27]);
            decimal dLYCOGS = Convert.ToDecimal(sStockStat[28]);
            decimal dLYProfitAmount = dLYNetSales - dLYCOGS;
            decimal dLYProfitPercent = 0;
            if (dLYNetSales != 0)
                dLYProfitPercent = (100 / dLYNetSales) * dLYProfitAmount;
            lblStats[4, 4].Text = FormatMoneyForDisplay(dLYProfitAmount);
            lblStats[4, 5].Text = FormatMoneyForDisplay(dLYProfitPercent);

            lblSecondHeaders = new Label[4];
            nLeft = 250;
            for (int i = 0; i < lblSecondHeaders.Length; i++)
            {
                lblSecondHeaders[i] = new Label();
                lblSecondHeaders[i].AutoSize = false;
                lblSecondHeaders[i].Size = new Size(100, 50);
                lblSecondHeaders[i].Location = new Point(nLeft, 575);
                this.Controls.Add(lblSecondHeaders[i]);
                lblSecondHeaders[i].BackColor = Color.Transparent;
                nLeft += 150;
            }
            lblSecondHeaders[0].Text = "Opening Stock";
            lblSecondHeaders[1].Text = "Delivered";
            lblSecondHeaders[2].Text = "Sold";
            lblSecondHeaders[3].Text = "Q.I.S";

            lblSecondRowTitles = new Label[2];
            nHeight = 625;
            for (int i = 0; i < lblSecondRowTitles.Length; i++)
            {
                lblSecondRowTitles[i] = new Label();
                lblSecondRowTitles[i].AutoSize = true;
                lblSecondRowTitles[i].Location = new Point(10, nHeight);
                nHeight += 35;
                this.Controls.Add(lblSecondRowTitles[i]);
                lblSecondRowTitles[i].BackColor = Color.Transparent;
            }
            lblSecondRowTitles[0].Text = "Y.T.D Units";
            lblSecondRowTitles[1].Text = "Cost";

            lblSecondStats = new Label[4, 2];
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    lblSecondStats[x, y] = new Label();
                    lblSecondStats[x, y].AutoSize = false;
                    lblSecondStats[x, y].Location = new Point(lblSecondHeaders[x].Left, lblSecondRowTitles[y].Top);
                    lblSecondStats[x, y].Size = new Size(lblSecondHeaders[x].Width, lblSecondRowTitles[y].Height);
                    lblSecondStats[x, y].Text = "0.00";
                    lblSecondStats[x, y].TextAlign = ContentAlignment.MiddleRight;
                    this.Controls.Add(lblSecondStats[x, y]);
                    lblSecondStats[x, y].BackColor = Color.Transparent;
                }
            }
            lblSecondStats[0, 0].Text = FormatMoneyForDisplay(sStockStat[21]);
            lblSecondStats[1, 0].Text = FormatMoneyForDisplay(sStockStat[23]);
            lblSecondStats[2, 0].Text = FormatMoneyForDisplay(sStockStat[17]);
            lblSecondStats[3, 0].Text = FormatMoneyForDisplay(sStockStat[36]);
            lblSecondStats[0, 1].Text = FormatMoneyForDisplay(sStockStat[22]);
            lblSecondStats[1, 1].Text = FormatMoneyForDisplay(sStockStat[24]);
            lblSecondStats[2, 1].Text = FormatMoneyForDisplay(sStockStat[20]);
            lblSecondStats[3, 1].Text = FormatMoneyForDisplay(Convert.ToDecimal(sStockStat[1]) * Convert.ToDecimal(sStockStat[36]));
        }

        void mAveCost_Click(object sender, EventArgs e)
        {
            frmSingleInputBox fsAvecost = new frmSingleInputBox("Enter the new average cost...", ref sEngine);
            fsAvecost.ShowDialog();
            if (fsAvecost.Response != "$NONE")
            {
                sEngine.EditAverageCostOfItem(sBarcode, sShopCode, fsAvecost.Response);
                ShowStatsAboutProduct();
            }
        }

        void mViewParent_Click(object sender, EventArgs e)
        {
            if (lblSecondRowStat[11].Text == "")
            {
                MessageBox.Show("This item doesn't have a parent!");
            }
            else
            {
                sBarcode = lblSecondRowStat[11].Text;
                ShowStatsAboutProduct();
            }
        }

        void EditStatsClick(object sender, EventArgs e)
        {
            MenuItem s = (MenuItem)sender;
            int nX = Convert.ToInt32(s.Name.Split(',')[0]);
            int nY = Convert.ToInt32(s.Name.Split(',')[1]);
            string sDBaseFieldName = sTopFNames[nX] + sSideFNames[nY];

            frmSingleInputBox fsiGetNewData = new frmSingleInputBox("Enter the new data (field " + sDBaseFieldName + "):", ref sEngine);
            fsiGetNewData.ShowDialog();
            if (fsiGetNewData.Response != "$NONE")
            {
                try
                {
                    Convert.ToDecimal(fsiGetNewData.Response);
                    sEngine.ChangeStockStaField(sBarcode, sDBaseFieldName, fsiGetNewData.Response, sShopCode);
                    ShowStatsAboutProduct();
                }
                catch
                {
                    MessageBox.Show("Invalid data entered. The record has not had its data changed.");
                }
            }
        }

        void mEditLastCost_Click(object sender, EventArgs e)
        {
            decimal dLastCost = Convert.ToDecimal(sEngine.GetMainStockInfo(sBarcode)[8]);
            frmSingleInputBox fsiGetLastCost = new frmSingleInputBox("Enter the last cost :", ref sEngine);
            fsiGetLastCost.tbResponse.Text = FormatMoneyForDisplay(dLastCost);
            fsiGetLastCost.ShowDialog();
            if (fsiGetLastCost.Response != "$NONE")
            {
                bool bOk = false;
                try
                {
                    Convert.ToDecimal(fsiGetLastCost.Response);
                    bOk = true;
                }
                catch { ;}
                if (bOk)
                {
                    sEngine.ChangeLastCostOfItem(sBarcode, Convert.ToDecimal(fsiGetLastCost.Response));
                    ShowStatsAboutProduct();
                }
            }
        }

        void frmDetailedItemEnquiry_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(sBarcode);
        }

        void mEditStock_Click(object sender, EventArgs e)
        {
            decimal dCurrentStockLevel = Convert.ToDecimal(sEngine.GetItemStockStaRecord(sBarcode, sShopCode)[36]);
            frmSingleInputBox fsGetNewStocklevel = new frmSingleInputBox("Enter the new stock level :", ref sEngine);
            fsGetNewStocklevel.ShowDialog();
            if (fsGetNewStocklevel.Response != "$NONE")
            {
                frmSingleInputBox fGetPassword = new frmSingleInputBox("Enter the administrator password : ", ref sEngine);
                fGetPassword.tbResponse.PasswordChar = ' ';
                fGetPassword.ShowDialog();
                if (fGetPassword.Response != "$NONE" && fGetPassword.Response.ToUpper() == sEngine.GetPasswords(2).ToUpper())
                {
                    if (sEngine.GetMainStockInfo(sBarcode)[5] == "1")
                    {
                        if (Convert.ToDecimal(fsGetNewStocklevel.Response) > dCurrentStockLevel)
                        {
                            sEngine.TransferStockItem("BH", sShopCode, sBarcode, Convert.ToDecimal(fsGetNewStocklevel.Response) - dCurrentStockLevel, false);
                        }
                        else
                        {
                            sEngine.TransferStockItem(sShopCode, "BH", sBarcode, dCurrentStockLevel - Convert.ToDecimal(fsGetNewStocklevel.Response), false);
                        }
                    }
                    else if (sEngine.GetMainStockInfo(sBarcode)[5] == "6")
                    {
                        sEngine.ReceiveComissionItem(sBarcode, (Convert.ToDecimal(fsGetNewStocklevel.Response) - dCurrentStockLevel).ToString(), sShopCode);
                    }
                    ShowStatsAboutProduct();
                    if (MessageBox.Show("Would you like to upload the stock level change to all tills?", "Upload now?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        sEngine.CopyWaitingFilesToTills();
                }
            }
        }

        void ShowStatsAboutProduct()
        {
            sStockStat = sEngine.GetItemStockStaRecord(sBarcode, sShopCode);
            sMainStock = sEngine.GetMainStockInfo(sBarcode);
            FillInBlanks(sStockStat);
            FillInBlanks(sMainStock);

            if (sMainStock[5] == "1")
                lblStockCode.Text = "Stock Item : ";
            else if (sMainStock[5] == "2")
                lblStockCode.Text = "Department Item : ";
            else if (sMainStock[5] == "3")
                lblStockCode.Text = "Non-Stock Item : ";
            else if (sMainStock[5] == "4")
                lblStockCode.Text = "Multi-Item Item : ";
            else if (sMainStock[5] == "5")
                lblStockCode.Text = "Child Item : ";
            else if (sMainStock[5] == "6")
                lblStockCode.Text = "Commission Item : ";
            lblStockCode.Text += sBarcode;

            lblStockDesc.Text = sMainStock[1];

            lblStockShopName.Text = "Shop : " + sStockStat[35] + " - " + sEngine.GetShopNameFromCode(sStockStat[35]);

            lblCategory.Text = "Category : " + sEngine.GetCategoryDesc(sMainStock[4]) + " (" + sMainStock[4] + ")";
            sEngine.LastCategoryCode = sMainStock[4];
            sItemCategory = sMainStock[4];

            lblUnits.Text = "Minimum Order : " + sStockStat[37];

            /*lblSecondRowStat[0].Text = FormatMoneyForDisplay(Convert.ToDecimal(sMainStock[2]));
            if (sStockStat[4].Length == 6)
            {
                string sDate = sStockStat[4][0].ToString() + sStockStat[4][1].ToString() + "/" + sStockStat[4][2].ToString() + sStockStat[4][3].ToString() + "/" + sStockStat[4][4].ToString() + sStockStat[4][5].ToString();
                lblSecondRowStat[1].Text = sDate;
            }
            else
            {
                lblSecondRowStat[1].Text = "Unknown";
            }
            lblSecondRowStat[2].Text = FormatMoneyForDisplay(Convert.ToDecimal(sStockStat[1]));
            lblSecondRowStat[3].Text = FormatMoneyForDisplay(Convert.ToDecimal(sStockStat[36]));
            lblSecondRowStat[4].Text = Math.Round(Convert.ToDecimal(sStockStat[2]), 3).ToString();
            if (Convert.ToDecimal(sMainStock[2]) <= 0 || Convert.ToDecimal(sStockStat[2]) <= 0)
                lblSecondRowStat[5].Text = "0";
            else
                lblSecondRowStat[5].Text = FormatMoneyForDisplay(Convert.ToDecimal(sStockStat[36]) / Convert.ToDecimal(sStockStat[2]));
            decimal dVATRate = (sEngine.GetVATRateFromCode(sMainStock[0]) / 100) + 1;
            decimal dNet = Convert.ToDecimal(sMainStock[2]) / dVATRate;
            decimal dProfit = dNet - Convert.ToDecimal(sStockStat[1]);
            lblSecondRowStat[6].Text = FormatMoneyForDisplay(dProfit);
            decimal dMargin = 0;
            if (Convert.ToDecimal(sStockStat[1]) <= 0)
                lblSecondRowStat[7].Text = "";
            else
            {
                dMargin = (100 / Convert.ToDecimal(sStockStat[1])) * dProfit;
                lblSecondRowStat[7].Text = FormatMoneyForDisplay(dMargin);
            }
            lblSecondRowStat[8].Text = FormatMoneyForDisplay(sMainStock[8]);*/
            if (Convert.ToInt32(sMainStock[5]) != 5)
            {
                lblSecondRowData[11].Text = "Out Of Stock : ";
            }
            else
            {
                lblSecondRowData[11].Text = "Parent Barcode : ";
            }
            lblSecondRowStat[0].Text = FormatMoneyForDisplay(Convert.ToDecimal(sMainStock[2]));
            if (sStockStat[4].Length == 6)
            {
                string sDate = sStockStat[4][0].ToString() + sStockStat[4][1].ToString() + "/" + sStockStat[4][2].ToString() + sStockStat[4][3].ToString() + "/" + sStockStat[4][4].ToString() + sStockStat[4][5].ToString();
                lblSecondRowStat[1].Text = sDate;
            }
            else
            {
                lblSecondRowStat[1].Text = "Unknown";
            }
            if (sStockStat[40].Length != 6)
            {
                lblSecondRowStat[3].Text = "Unknown";
            }
            else
            {
                string sDate = sStockStat[40][0].ToString() + sStockStat[40][1].ToString() + "/" + sStockStat[40][2].ToString() + sStockStat[40][3].ToString() + "/" + sStockStat[40][4].ToString() + sStockStat[40][5].ToString();
                lblSecondRowStat[3].Text = sDate;
            }
            if (sStockStat[41].Length != 6)
            {
                lblSecondRowStat[7].Text = "Unknown";
            }
            else
            {
                string sDate = sStockStat[41][0].ToString() + sStockStat[41][1].ToString() + "/" + sStockStat[41][2].ToString() + sStockStat[41][3].ToString() + "/" + sStockStat[41][4].ToString() + sStockStat[41][5].ToString();
                lblSecondRowStat[7].Text = sDate;
            }
            lblSecondRowStat[2].Text = FormatMoneyForDisplay(Convert.ToDecimal(sStockStat[1]));
            lblSecondRowStat[4].Text = FormatMoneyForDisplay(Convert.ToDecimal(sStockStat[36]));
            lblSecondRowStat[5].Text = Math.Round(Convert.ToDecimal(sStockStat[2]), 3).ToString();
            if (Convert.ToDecimal(sMainStock[2]) <= 0 || Convert.ToDecimal(sStockStat[2]) <= 0)
                lblSecondRowStat[6].Text = "0";
            else
                lblSecondRowStat[6].Text = FormatMoneyForDisplay(Convert.ToDecimal(sStockStat[36]) / Convert.ToDecimal(sStockStat[2]));
            decimal dVATRate = (sEngine.GetVATRateFromCode(sMainStock[0]) / 100) + 1;
            decimal dNet = Convert.ToDecimal(sMainStock[2]) / dVATRate;
            decimal dProfit = dNet - Convert.ToDecimal(sStockStat[1]);
            if (sMainStock[5] == "6")
            {
                dNet = (Convert.ToDecimal(sMainStock[2]) - Convert.ToDecimal(sStockStat[1])) / dVATRate;
                dProfit = dNet;
                dNet += Convert.ToDecimal(sStockStat[1]);
            }
            lblSecondRowStat[8].Text = FormatMoneyForDisplay(dProfit);
            decimal dMargin = 0;
            lblSecondRowData[9].Text = "Margin (%) : ";
            if (Convert.ToDecimal(sStockStat[1]) <= 0 && sMainStock[5] != "2")
                lblSecondRowStat[9].Text = "";
            else if (sMainStock[5] != "2")
            {
                dMargin = (100 / Convert.ToDecimal(sStockStat[1])) * dProfit;
                lblSecondRowStat[9].Text = FormatMoneyForDisplay(dMargin);
            }
            else
            {
                lblSecondRowData[9].Text = "Target Margin (%) : ";
                lblSecondRowStat[9].Text = FormatMoneyForDisplay(sStockStat[39]);
            }
            lblSecondRowStat[10].Text = FormatMoneyForDisplay(sMainStock[8]);
            if (Convert.ToInt32(sMainStock[5]) == 5)
            {
                lblSecondRowStat[11].Text = sMainStock[7];
            }
            else
            {
                lblSecondRowStat[11].Text = sEngine.GetOutOfStockLength(sBarcode, sShopCode) + "% of the time";
            }
            //Quantities
            lblStats[0, 0].Text = FormatMoneyForDisplay(sStockStat[5]);
            lblStats[1, 0].Text = FormatMoneyForDisplay(sStockStat[9]);
            lblStats[2, 0].Text = FormatMoneyForDisplay(sStockStat[13]);
            lblStats[3, 0].Text = FormatMoneyForDisplay(sStockStat[17]);
            lblStats[4, 0].Text = FormatMoneyForDisplay(sStockStat[25]);

            //Gross Sales
            lblStats[0, 1].Text = FormatMoneyForDisplay(sStockStat[6]);
            lblStats[1, 1].Text = FormatMoneyForDisplay(sStockStat[10]);
            lblStats[2, 1].Text = FormatMoneyForDisplay(sStockStat[14]);
            lblStats[3, 1].Text = FormatMoneyForDisplay(sStockStat[18]);
            lblStats[4, 1].Text = FormatMoneyForDisplay(sStockStat[26]);

            //Net Sales
            lblStats[0, 2].Text = FormatMoneyForDisplay(sStockStat[7]);
            lblStats[1, 2].Text = FormatMoneyForDisplay(sStockStat[11]);
            lblStats[2, 2].Text = FormatMoneyForDisplay(sStockStat[15]);
            lblStats[3, 2].Text = FormatMoneyForDisplay(sStockStat[19]);
            lblStats[4, 2].Text = FormatMoneyForDisplay(sStockStat[27]);

            //COGS
            lblStats[0, 3].Text = FormatMoneyForDisplay(sStockStat[8]);
            lblStats[1, 3].Text = FormatMoneyForDisplay(sStockStat[12]);
            lblStats[2, 3].Text = FormatMoneyForDisplay(sStockStat[16]);
            lblStats[3, 3].Text = FormatMoneyForDisplay(sStockStat[20]);
            lblStats[4, 3].Text = FormatMoneyForDisplay(sStockStat[28]);

            //Profit
            for (int x = 0; x < 4; x++)
            {
                decimal dNetSales = Convert.ToDecimal(sStockStat[(4 * x) + 7]);
                decimal dCOGS = Convert.ToDecimal(sStockStat[8 + (x * 4)]);
                decimal dProfitAmount = dNetSales - dCOGS;
                decimal dProfitPercent = 0;
                if (dNetSales != 0)
                    dProfitPercent = (100 / dNetSales) * dProfitAmount;
                lblStats[x, 4].Text = FormatMoneyForDisplay(dProfitAmount);
                lblStats[x, 5].Text = FormatMoneyForDisplay(dProfitPercent);
            }
            // LYProfit
            decimal dLYNetSales = Convert.ToDecimal(sStockStat[27]);
            decimal dLYCOGS = Convert.ToDecimal(sStockStat[28]);
            decimal dLYProfitAmount = dLYNetSales - dLYCOGS;
            decimal dLYProfitPercent = 0;
            if (dLYNetSales != 0)
                dLYProfitPercent = (100 / dLYNetSales) * dLYProfitAmount;
            lblStats[4, 4].Text = FormatMoneyForDisplay(dLYProfitAmount);
            lblStats[4, 5].Text = FormatMoneyForDisplay(dLYProfitPercent);

            lblSecondStats[0, 0].Text = FormatMoneyForDisplay(sStockStat[21]);
            lblSecondStats[1, 0].Text = FormatMoneyForDisplay(sStockStat[23]);
            lblSecondStats[2, 0].Text = FormatMoneyForDisplay(sStockStat[17]);
            lblSecondStats[3, 0].Text = FormatMoneyForDisplay(sStockStat[36]);
            lblSecondStats[0, 1].Text = FormatMoneyForDisplay(sStockStat[22]);
            lblSecondStats[1, 1].Text = FormatMoneyForDisplay(sStockStat[24]);
            lblSecondStats[2, 1].Text = FormatMoneyForDisplay(sStockStat[20]);
            lblSecondStats[3, 1].Text = FormatMoneyForDisplay(Convert.ToDecimal(sStockStat[1]) * Convert.ToDecimal(sStockStat[36]));
        }

        void frmDetailedItemEnquiry_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (!bJustEscd)
                    this.Close();
                else
                    bJustEscd = false;
            }
            else
            {
                switch (InputTextBox("INPUT").Text.ToUpper())
                {
                    case "A":
                        frmGetBarcode fgb = new frmGetBarcode(ref sEngine);
                        fgb.LastCategory = sItemCategory;
                        fgb.ShowDialog();
                        if (fgb.Barcode != "" && fgb.Barcode != null)
                        {
                            sBarcode = fgb.Barcode;
                            ShowStatsAboutProduct();
                            fgb.Dispose();
                        }
                        break;
                    case "E":
                        frmAddEditItem faei = new frmAddEditItem(ref sEngine);
                        faei.EditingBarcode = sBarcode;
                        faei.ShowDialog();
                        faei.Dispose();
                        this.ShowStatsAboutProduct();
                        bJustEscd = true;
                        break;
                    case "Q":
                        this.Close();
                        break;
                    case "O":
                        frmOrdersWithItemIn fowii = new frmOrdersWithItemIn(ref sEngine, sBarcode);
                        fowii.ShowDialog();
                        this.ShowStatsAboutProduct();
                        bJustEscd = true;
                        break;
                    case "I":
                        if (ForAnOrder)
                        {
                            ItemAdded = true;
                            this.Close();
                        }
                        break;
                    case "R":
                        if (sMainStock[5] == "1")
                        {
                            sEngine.AddSuggestedOrderItem(sBarcode, sShopCode);
                            MessageBox.Show("Item has been suggested!");
                        }
                        else
                        {
                            MessageBox.Show("Sorry, only type 1 items can be ordered!");
                        }
                        break;
                    case "S":
                        frmSuppliersForItem fsfi = new frmSuppliersForItem(ref sEngine, sBarcode);
                        fsfi.ShowDialog();
                        bJustEscd = true;
                        break;
                    case "V":
                        frmListOfSales flos = new frmListOfSales(ref sEngine, sBarcode);
                        flos.ShowDialog();
                        bJustEscd = true;
                        break;

                }
                InputTextBox("INPUT").Text = "";
            }
        }

    }
}
