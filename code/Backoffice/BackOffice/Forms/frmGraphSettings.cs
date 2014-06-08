using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;
using System.IO;
using BackOffice.Database_Engine;

namespace BackOffice
{
    class DataPoint : IComparable
    {
        public decimal dAmount = 0;
        public DateTime dtWhen = new DateTime();

        public DataPoint(decimal dAmount, DateTime dtWhen)
        {
            this.dAmount = dAmount;
            this.dtWhen = dtWhen;
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            return -1 * ((DataPoint)obj).dtWhen.CompareTo(dtWhen);
        }

        #endregion
    }
    class GraphItem
    {
        public enum Type { Barcode, Staff, Category };
        public enum Parameter { Gross, Net, Profit, QuantitySold};
        private Color cGraphingColour;
        private frmGraphSettings.Resolutions rResolution;
        private Parameter pParam;
        private Bitmap bmpGraph;
        private Type tType;
        private List<DataPoint> dpNodes;
        private string sFieldParam; // ie Barcode, Category code
        private string sFieldDesc;

        public GraphItem(Type tType, ref Bitmap bmpGraph, frmGraphSettings.Resolutions rResolution, Color cGraphingColour, Parameter pParam, string sFieldParam, string sFieldDesc)
        {
            dpNodes = new List<DataPoint>();
            this.cGraphingColour = cGraphingColour;
            this.rResolution = rResolution;
            this.bmpGraph = bmpGraph;
            this.tType = tType;
            this.pParam = pParam;
            this.sFieldParam = sFieldParam;
            this.sFieldDesc = sFieldDesc;
        }

        public Type ItemType
        {
            get{
                return tType;
            }
        }

        public string Code
        {
            get
            {
                return sFieldParam;
            }
        }

        public string CodeDesc
        {
            get
            {
                return sFieldDesc;
            }
        }

        public string ParameterDesc
        {
            get
            {
                switch (pParam)
                {
                    case Parameter.Gross:
                        return "Gross Sales";
                        break;
                    case Parameter.Net:
                        return "Net Sales";
                        break;
                    case Parameter.Profit:
                        return "Profit";
                        break;
                    case Parameter.QuantitySold:
                        return "Quantity Sold";
                        break;
                }
                return pParam.ToString();
            }
        }

        public Color Colour
        {
            get
            {
                return cGraphingColour;
            }
        }

        private int GetFieldToSearch()
        {
            int nFieldToSearch = 0;
            switch (pParam)
            {
                case Parameter.Gross:
                    switch (this.rResolution)
                    {
                        case frmGraphSettings.Resolutions.Daily:
                            nFieldToSearch = 6;
                            break;
                        case frmGraphSettings.Resolutions.Monthly:
                            nFieldToSearch = 14;
                            break;
                        case frmGraphSettings.Resolutions.Weekly:
                            nFieldToSearch = 10;
                            break;
                        case frmGraphSettings.Resolutions.Yearly:
                            nFieldToSearch = 18;
                            break;
                    }
                    break;
                case Parameter.Net:
                    switch (this.rResolution)
                    {
                        case frmGraphSettings.Resolutions.Daily:
                            nFieldToSearch = 7;
                            break;
                        case frmGraphSettings.Resolutions.Monthly:
                            nFieldToSearch = 15;
                            break;
                        case frmGraphSettings.Resolutions.Weekly:
                            nFieldToSearch = 11;
                            break;
                        case frmGraphSettings.Resolutions.Yearly:
                            nFieldToSearch = 19;
                            break;
                    }
                    break;
                case Parameter.Profit:
                    switch (this.rResolution)
                    {
                        case frmGraphSettings.Resolutions.Daily:
                            nFieldToSearch = 8;
                            break;
                        case frmGraphSettings.Resolutions.Monthly:
                            nFieldToSearch = 16;
                            break;
                        case frmGraphSettings.Resolutions.Weekly:
                            nFieldToSearch = 12;
                            break;
                        case frmGraphSettings.Resolutions.Yearly:
                            nFieldToSearch = 20;
                            break;
                    }
                    break;
                case Parameter.QuantitySold:
                    switch (this.rResolution)
                    {
                        case frmGraphSettings.Resolutions.Daily:
                            nFieldToSearch = 5;
                            break;
                        case frmGraphSettings.Resolutions.Monthly:
                            nFieldToSearch = 13;
                            break;
                        case frmGraphSettings.Resolutions.Weekly:
                            nFieldToSearch = 9;
                            break;
                        case frmGraphSettings.Resolutions.Yearly:
                            nFieldToSearch = 17;
                            break;
                    }
                    break;
            }
            return nFieldToSearch;
        }

        public void ReceiveStockStaTable(Table tStockStats, string sWorkingDir)
        {
            int nSubFieldForProfit = -1;
            DateTime dt = DateTime.Parse(sWorkingDir.Split('\\')[sWorkingDir.Split('\\').Length - 1]);
            if (this.tType == Type.Barcode)
            {
                int nFieldToSearch = GetFieldToSearch();
                decimal dAmount = Convert.ToDecimal(tStockStats.GetRecordFrom(sFieldParam, 0, true)[nFieldToSearch]);
                if (pParam == Parameter.Profit)
                    dAmount = Convert.ToDecimal(tStockStats.GetRecordFrom(sFieldParam, 0, true)[nFieldToSearch + nSubFieldForProfit]) - dAmount;
                dpNodes.Add(new DataPoint(dAmount, dt));
            }
            else if (tType == Type.Category)
            {
                int nFieldToSearch = GetFieldToSearch();
                Table tMainStock = new Table(sWorkingDir + "\\MAINSTOC.DBF");
                int nOfRecords = 0;
                string[] sBarcodes = tMainStock.SearchAndGetAllMatchingRecords(4, sFieldParam, ref nOfRecords, true, 0);
                decimal dAmount = 0;
                for (int i = 0; i < nOfRecords; i++)
                {
                    if (pParam != Parameter.Profit)
                        dAmount += Convert.ToDecimal(tStockStats.GetRecordFrom(sBarcodes[i], 0, true)[nFieldToSearch]);
                    else
                        dAmount += Convert.ToDecimal(tStockStats.GetRecordFrom(sBarcodes[i], 0, true)[nFieldToSearch + nSubFieldForProfit]);
                }
                dpNodes.Add(new DataPoint(dAmount, dt));
            }
            // To Do:
            // Category, Staff
        }

        public void SortNodesChronologically()
        {
            dpNodes.Sort();
        }

        public decimal HighestNodeValue
        {
            get
            {
                decimal dCurrent = 0;
                for (int i = 0; i < dpNodes.Count; i++)
                {
                    if (dpNodes[i].dAmount > dCurrent)
                        dCurrent = dpNodes[i].dAmount;
                }
                return dCurrent;
            }
        }

        public DateTime FirstDate
        {
            get
            {
                SortNodesChronologically();
                return dpNodes[0].dtWhen;
            }
        }

        public DateTime LastDate
        {
            get
            {
                SortNodesChronologically();
                return dpNodes[dpNodes.Count - 1].dtWhen;
            }
        }

        public void DrawItem(decimal dXScale, decimal dYScale)
        {
            Graphics g = Graphics.FromImage(bmpGraph);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            int nX = 0, nY = 0;
            if (dpNodes.Count > 0)
            {
                nY = bmpGraph.Height - Convert.ToInt32(dpNodes[0].dAmount * dYScale) - 50;
                nX = 50;
            }
            int nOldX = 0, nOldY = 0;
            int nCurrentWritingX = 0;
            for (int i = 1; i < dpNodes.Count; i++)
            {
                nOldX = nX;
                nOldY = nY;
                nY = bmpGraph.Height - Convert.ToInt32(dpNodes[i].dAmount * dYScale) - 50;
                TimeSpan t = dpNodes[0].dtWhen.Subtract(dpNodes[i].dtWhen);
                nX = Convert.ToInt32(50 + (t.Days * dXScale));
                g.DrawLine(new Pen(cGraphingColour, 2.0f), new Point(nOldX, nOldY), new Point(nX, nY));
                if (nX > nCurrentWritingX)
                {
                    g.DrawString(dpNodes[i].dtWhen.ToShortDateString(), new Font("Arial", 9.0f), new SolidBrush(Color.Black), new PointF(nX, bmpGraph.Height - 50));
                    nCurrentWritingX = nX + Convert.ToInt32(g.MeasureString(dpNodes[i].dtWhen.ToShortDateString(), new Font("Arial", 9.0f)).Width);
                }
            }
        }
    }

    class GraphEngine
    {
        private string sWorkingDir;
        private GraphItem[] gItem;
        private Bitmap iGraph;
        private decimal dYAxisScale, dXAxisScale;
        private frmGraphSettings.Resolutions rResolution;

        public GraphEngine(string sWorkingDir,frmGraphSettings.Resolutions rResolution)
        {
            iGraph = new Bitmap(1920, 1080);
            gItem = new GraphItem[0];
            this.sWorkingDir = sWorkingDir;
            this.rResolution = rResolution;
        }

        public void AddGraphItem(GraphItem.Type tType, Color cGraphingColour, string sFieldParam, GraphItem.Parameter pParam, string sFieldDesc)
        {
            Array.Resize<GraphItem>(ref gItem, gItem.Length + 1);
            gItem[gItem.Length - 1] = new GraphItem(tType, ref iGraph, rResolution, cGraphingColour, pParam, sFieldParam, sFieldDesc);
        }

        public void SaveImage()
        {
            iGraph.Save("Graph.bmp");
        }
        public void SaveImage(string sStage)
        {
            if (!Directory.Exists("GraphImages"))
            {
                Directory.CreateDirectory("GraphImages");
            }
            iGraph.Save("GraphImages\\Graph Stage " + sStage + ".bmp");
        }

        private void CalculateAxesScale()
        {
            decimal dCurrent = 0;
            for (int i = 0; i < gItem.Length; i++)
            {
                if (gItem[i].HighestNodeValue > dCurrent)
                    dCurrent = gItem[i].HighestNodeValue;
            }
            if (dCurrent != 0)
                dYAxisScale = (iGraph.Height - 100) / dCurrent;
            else
                dYAxisScale = 1;

            decimal dMaxScale = 99999;
            for (int i = 0; i < gItem.Length; i++)
            {
                TimeSpan t = gItem[i].FirstDate.Subtract(gItem[i].LastDate);
                if (dMaxScale > (iGraph.Width - 100) / t.Days)
                {
                    dMaxScale = (iGraph.Width - 100) / t.Days;
                }
            }
            dXAxisScale = dMaxScale;                
        }

        private void LoadInData(string[] sDirsToUse)
        {
            frmProgressBar fp = new frmProgressBar("Loading Data");
            fp.pb.Maximum = sDirsToUse.Length;
            fp.pb.Value = 0;
            Form f = new Form();
            f.WindowState = FormWindowState.Maximized;
            PictureBox pb = new PictureBox();
            f.Controls.Add(pb);
            f.Show();
            fp.Show();
            pb.Size = f.Size;
            pb.SizeMode = PictureBoxSizeMode.Zoom;
            for (int i = 0; i < sDirsToUse.Length; i++)
            {
                fp.pb.Value = i;
                using (Table tStockSta = new Table(sWorkingDir + "\\" + sDirsToUse[i] + "\\STOCKSTA.DBF"))
                {
                    for (int x = 0; x < gItem.Length; x++)
                    {
                        if (gItem[x].ItemType == GraphItem.Type.Barcode || gItem[x].ItemType == GraphItem.Type.Category)
                        {
                            gItem[x].ReceiveStockStaTable(tStockSta, sWorkingDir + "\\" + sDirsToUse[i]);
                        }
                    }
                }
                if (i >= 1)
                {
                    CalculateAxesScale();
                    DrawAxes();
                    DrawGraph();
                    SaveImage(i.ToString());
                    pb.Image = Image.FromFile("GraphImages\\Graph Stage " + i.ToString() + ".bmp");
                    Application.DoEvents();
                }
                
            }
            fp.Close();
        }

        private void DrawGraph()
        {
            for (int i = 0; i < gItem.Length; i++)
            {
                gItem[i].DrawItem(dXAxisScale, dYAxisScale);
            }
        }

        private void DrawAxes()
        {
            Graphics g = Graphics.FromImage(iGraph);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, 1920, 1080);
            g.DrawLine(new Pen(Color.Black), new Point(50, 50), new Point(50, 1030));
            g.DrawLine(new Pen(Color.Black), new Point(50, 1030), new Point(1870, 1030));

            // Y Axis
            decimal dCurrent = 0;
            for (int i = 0; i < gItem.Length; i++)
            {
                if (gItem[i].HighestNodeValue > dCurrent)
                    dCurrent = gItem[i].HighestNodeValue;
            }
            for (int i = 0; i <= 10; i++)
            {
                g.DrawString((Math.Round((dCurrent / 10 * i), 0).ToString()), new Font("Arial", 9.0f), new SolidBrush(Color.Black), 50 - g.MeasureString((Math.Round((dCurrent / 10 * i), 0).ToString()).ToString(), new Font("Arial", 9.0f)).Width - 5, iGraph.Height - 50 - ((980 / 10) * i) - (g.MeasureString((Math.Round((dCurrent / 10 * i), 0).ToString()), new Font("Arial", 9.0f)).Height / 2));
                g.DrawLine(new Pen(Color.Black), new Point(46, iGraph.Height - ((980 / 10) * i) - 50), new Point(50, iGraph.Height - ((980 / 10) * i) - 50));
            }

            // Draw Key across top

            int xPos = 0;
            for (int i = 0; i < gItem.Length; i++)
            {
                string sToDraw = gItem[i].CodeDesc + " " + gItem[i].ParameterDesc;
                g.DrawString(sToDraw, new Font("Arial", 10.0f), new SolidBrush(gItem[i].Colour), new PointF(xPos, 0));
                xPos += Convert.ToInt32(g.MeasureString(sToDraw, new Font("Arial", 10.0f)).Width);
            }
        }

        private void BlankImage()
        {
            iGraph = new Bitmap(1920, 1080);
        }

        public void Start(string[] sDirs)
        {
            LoadInData(sDirs);
            SaveImage();
            /*CalculateAxesScale();
            DrawAxes();
            DrawGraph();
            SaveImage();*/
        }
    }

    class frmGraphSettings : ScalableForm
    {
        Color[] cPieColours = { Color.Red, Color.Orange, Color.Yellow, Color.LightGreen, Color.Green, Color.LightBlue, Color.Blue, Color.Indigo, Color.Violet };
        ComboBox cResolution;
        ComboBox[] cItemType;
        TextBox[] tbItemParam;
        ComboBox[] cItemProperty;
        Button[] btnItemClr;
        Button[] btnRemove;
        Button btnAddRow;
        Button btnStart;
        DateTimePicker dtStartDate;
        DateTimePicker dtEndDate;
        DateTime[] dAvailableDates;
        public enum Resolutions {Daily, Monthly, Weekly, Yearly};
        StockEngine sEngine;

        public frmGraphSettings(ref StockEngine sEngine)
        {
            this.sEngine = sEngine;
            AllowScaling = false;
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.Size = new System.Drawing.Size(550, 600);
            this.Text = "Graph Settings";
            SetupForm();
        }

        void GetAvailableDates(Resolutions rSelectedResolution)
        {
            string sFolder = "";
            switch (rSelectedResolution)
            {
                case Resolutions.Daily:
                    sFolder = "DAILY";
                    break;
                case Resolutions.Monthly:
                    sFolder = "MONTHLY";
                    break;
                case Resolutions.Weekly:
                    sFolder = "WEEKLY";
                    break;
                case Resolutions.Yearly:
                    sFolder = "YEARLY";
                    break;
            }

            string[] sDates = Directory.GetDirectories("ARCHIVE\\" + sFolder);
            Array.Sort<String>(sDates);
            dAvailableDates = new DateTime[sDates.Length];
            for (int i = 0; i < dAvailableDates.Length; i++)
            {
                dAvailableDates[i] = DateTime.Parse(sDates[i].Split('\\')[sDates[i].Split('\\').Length - 1]);
            }
        }

        void SetupForm()
        {
            AddMessage("RESOLUTION", "Select Graph Resolution : ", new Point(10, 10));
            cResolution = new ComboBox();
            cResolution.Items.Add("Daily");
            cResolution.Items.Add("Weekly");
            cResolution.Items.Add("Monthly");
            cResolution.Items.Add("Yearly");
            cResolution.DropDownStyle = ComboBoxStyle.DropDownList;
            cResolution.Location = new Point(180, 5);
            cResolution.SelectedIndexChanged += new EventHandler(cResolution_SelectedIndexChanged);
            this.Controls.Add(cResolution);

            AddMessage("FROM", "From", new Point(10, BelowLastControl + 5));
            dtStartDate = new DateTimePicker();
            dtStartDate.Location = new Point(60, MessageLabel("FROM").Top - 2);
            this.Controls.Add(dtStartDate);

            AddMessage("TO", " to ", new Point(dtStartDate.Left + dtStartDate.Width + 10, MessageLabel("FROM").Top));
            dtEndDate = new DateTimePicker();
            dtEndDate.Location = new Point(300, dtStartDate.Top);
            this.Controls.Add(dtEndDate);

            cItemType = new ComboBox[0];
            tbItemParam = new TextBox[0];
            cItemProperty = new ComboBox[0];
            btnItemClr = new Button[0];
            btnRemove = new Button[0];

            AddMessage("TYPE", "Type", new Point(10, BelowLastControl + 10));
            AddMessage("CODE", "Code", new Point(138, MessageLabel("TYPE").Top));
            AddMessage("PARAM", "Parameter", new Point(248, MessageLabel("TYPE").Top));
            AddMessage("COLOUR", "Colour", new Point(380, MessageLabel("TYPE").Top));
            AddMessage("DELETE", "Delete", new Point(442, MessageLabel("TYPE").Top));

            btnAddRow = new Button();

            AddItemRow();

            btnAddRow.Location = new Point(100, BelowLastControl);
            btnAddRow.FlatStyle = FlatStyle.Popup;
            btnAddRow.Click += new EventHandler(btnAddRow_Click);
            this.Controls.Add(btnAddRow);
            btnAddRow.AutoSize = true;
            btnAddRow.Text = "Add New Row";

            btnStart = new Button();
            btnStart.AutoSize = true;
            btnStart.Text = "Create Graph";
            this.Controls.Add(btnStart);
            btnStart.Location = new Point(10, this.Height - 40 - btnStart.Height);
            btnStart.Click += new EventHandler(btnStart_Click);

            cResolution.SelectedIndex = 0;
        }

        void btnStart_Click(object sender, EventArgs e)
        {
            
            Resolutions rRes = Resolutions.Daily;
            switch (cResolution.SelectedIndex)
            {
                case 0:
                    rRes = Resolutions.Daily;
                    break;
                case 1:
                    rRes = Resolutions.Weekly;
                    break;
                case 2:
                    rRes = Resolutions.Monthly;
                    break;
                case 3:
                    rRes = Resolutions.Yearly;
                    break;
            }
            GraphEngine gEngine = new GraphEngine("Archive\\" + cResolution.Items[cResolution.SelectedIndex].ToString(), rRes);
            for (int i = 0; i < cItemType.Length; i++)
            {
                GraphItem.Type tType = GraphItem.Type.Barcode;
                switch (cItemType[i].SelectedIndex)
                {
                    case 0:
                        tType = GraphItem.Type.Barcode;
                        break;
                    case 1:
                        tType = GraphItem.Type.Category;
                        break;
                    case 2:
                        tType = GraphItem.Type.Staff;
                        break;
                }
                GraphItem.Parameter gParam = GraphItem.Parameter.Gross;
                switch (cItemProperty[i].SelectedIndex)
                {
                    case 0:
                        gParam = GraphItem.Parameter.Gross;
                        break;
                    case 1:
                        gParam = GraphItem.Parameter.Net;
                        break;
                    case 2:
                        gParam = GraphItem.Parameter.QuantitySold;
                        break;
                    case 3:
                        gParam = GraphItem.Parameter.Profit;
                        break;
                }
                string sFieldDesc = "";

                if (tbItemParam[i].Text == "")
                {
                    MessageBox.Show("No item entered in box " + (i + 1).ToString());
                    return;
                }

                if (tType == GraphItem.Type.Barcode)
                {
                    sFieldDesc = sEngine.GetMainStockInfo(tbItemParam[i].Text)[1];
                }
                else if (tType == GraphItem.Type.Category)
                {
                    sFieldDesc = sEngine.GetCategoryDesc(tbItemParam[i].Text);
                }
                gEngine.AddGraphItem(tType, btnItemClr[i].BackColor, tbItemParam[i].Text, gParam, sFieldDesc);
            }
            gEngine.Start(GenerateListOfFolders());

            SaveFileDialog sFile = new SaveFileDialog();
            sFile.Filter = "Bitmap File|*.bmp";
            if (sFile.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.Copy("graph.bmp", sFile.FileName, true);
            }

            this.Close();
        }

        void btnAddRow_Click(object sender, EventArgs e)
        {
            AddItemRow();
        }

        void AddItemRow()
        {
            int nTop = 0;
            if (cItemType.Length == 0)
                nTop = MessageLabel("TYPE").Top + MessageLabel("TYPE").Height + 10;
            else
                nTop = cItemType[cItemType.Length - 1].Top + cItemType[cItemType.Length - 1].Height + 10;

            Array.Resize<ComboBox>(ref cItemType, cItemType.Length + 1);
            Array.Resize<TextBox>(ref tbItemParam, tbItemParam.Length + 1);
            Array.Resize<ComboBox>(ref cItemProperty, cItemProperty.Length + 1);
            Array.Resize<Button>(ref btnItemClr, btnItemClr.Length + 1);
            Array.Resize<Button>(ref btnRemove, btnRemove.Length + 1);

            cItemType[cItemType.Length - 1] = new ComboBox();
            cItemType[cItemType.Length - 1].Items.Add("Product");
            cItemType[cItemType.Length - 1].Items.Add("Category");
            cItemType[cItemType.Length - 1].SelectedIndex = 0;
            cItemType[cItemType.Length - 1].DropDownStyle = ComboBoxStyle.DropDownList;
            cItemType[cItemType.Length - 1].Location = new Point(10, nTop);
            this.Controls.Add(cItemType[cItemType.Length - 1]);

            tbItemParam[tbItemParam.Length - 1] = new TextBox();
            tbItemParam[tbItemParam.Length - 1].AccessibleDescription = (tbItemParam.Length - 1).ToString();
            tbItemParam[tbItemParam.Length - 1].Location = new Point(cItemType[cItemType.Length - 1].Width + 20, nTop);
            tbItemParam[tbItemParam.Length - 1].KeyDown += new KeyEventHandler(tbItemParamKeyDown);
            this.Controls.Add(tbItemParam[tbItemParam.Length - 1]);

            cItemProperty[cItemProperty.Length - 1] = new ComboBox();
            cItemProperty[cItemProperty.Length - 1].Items.Add("Gross Sales");
            cItemProperty[cItemProperty.Length - 1].Items.Add("Net Sales");
            cItemProperty[cItemProperty.Length - 1].Items.Add("Quantity Sold");
            cItemProperty[cItemProperty.Length - 1].Items.Add("Profit");
            cItemProperty[cItemProperty.Length - 1].DropDownStyle = ComboBoxStyle.DropDownList;
            cItemProperty[cItemProperty.Length - 1].SelectedIndex = 0;
            cItemProperty[cItemProperty.Length - 1].Location = new Point(tbItemParam[tbItemParam.Length - 1].Left + tbItemParam[tbItemParam.Length - 1].Width + 10, nTop);
            this.Controls.Add(cItemProperty[cItemProperty.Length - 1]);

            btnItemClr[btnItemClr.Length - 1] = new Button();
            btnItemClr[btnItemClr.Length - 1].Width = 35;
            btnItemClr[btnItemClr.Length - 1].Location = new Point(cItemProperty[cItemProperty.Length - 1].Left + cItemProperty[cItemProperty.Length - 1].Width + 10, nTop);
            btnItemClr[btnItemClr.Length - 1].Click += new EventHandler(btnItemClrClick);
            btnItemClr[btnItemClr.Length - 1].BackColor = Color.Red;
            if (btnItemClr.Length < cPieColours.Length)
            {
                btnItemClr[btnItemClr.Length - 1].BackColor = cPieColours[btnItemClr.Length - 1];
            }
            btnItemClr[btnItemClr.Length - 1].FlatStyle = FlatStyle.Flat;
            this.Controls.Add(btnItemClr[btnItemClr.Length - 1]);

            btnRemove[btnRemove.Length - 1] = new Button();
            btnRemove[btnRemove.Length - 1].Location = new Point(btnItemClr[btnItemClr.Length - 1].Left + btnItemClr[btnItemClr.Length - 1].Width + 25, nTop);
            btnRemove[btnRemove.Length - 1].Click += new EventHandler(btnRemoveClick);
            btnRemove[btnRemove.Length - 1].Text = "x";
            btnRemove[btnRemove.Length - 1].FlatStyle = FlatStyle.Popup;
            btnRemove[btnRemove.Length - 1].AccessibleDescription = (btnRemove.Length - 1).ToString();
            btnRemove[btnRemove.Length - 1].Width = 20;
            this.Controls.Add(btnRemove[btnRemove.Length - 1]);

            btnAddRow.Top = btnRemove[btnRemove.Length - 1].Top + btnRemove[btnRemove.Length - 1].Height + 10;
        }

        void tbItemParamKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                int nNo = Convert.ToInt32(((TextBox)sender).AccessibleDescription);
                if (cItemType[nNo].SelectedIndex == 0)
                {
                    frmSearchForItemV2 fsfi = new frmSearchForItemV2(ref sEngine);
                    fsfi.ShowDialog();
                    if (fsfi.GetItemBarcode() != "NONE_SELECTED")
                    {
                        ((TextBox)sender).Text = fsfi.GetItemBarcode();
                    }
                }
                else if (cItemType[nNo].SelectedIndex == 1)
                {
                    frmCategorySelect fCats = new frmCategorySelect(ref sEngine);
                    fCats.ShowDialog();
                    if (fCats.SelectedItemCategory != "$NULL")
                    {
                        ((TextBox)sender).Text = fCats.SelectedItemCategory;
                    }
                }
            }
        }

        void btnRemoveClick(object sender, EventArgs e)
        {
            int nRowToRemove = Convert.ToInt32(((Button)sender).AccessibleDescription);
            int nRowTop = ((Button)sender).Top;

            this.Controls.Remove(cItemType[nRowToRemove]);
            this.Controls.Remove(tbItemParam[nRowToRemove]);
            this.Controls.Remove(btnItemClr[nRowToRemove]);
            this.Controls.Remove(btnRemove[nRowToRemove]);
            this.Controls.Remove(cItemProperty[nRowToRemove]);

            for (int i = nRowToRemove + 1; i < cItemType.Length; i++)
            {
                int nTempTop = cItemType[i].Top;
                cItemType[i].Top = nRowTop;
                tbItemParam[i].Top = nRowTop;
                cItemProperty[i].Top = nRowTop;
                btnRemove[i].Top = nRowTop;
                btnRemove[i].AccessibleDescription = (i - 1).ToString();
                btnItemClr[i].Top = nRowTop;
                nRowTop = nTempTop;
                cItemType[i - 1] = cItemType[i];
                tbItemParam[i - 1] = tbItemParam[i];
                cItemProperty[i - 1] = cItemProperty[i];
                btnRemove[i - 1] = btnRemove[i];
                btnItemClr[i - 1] = btnItemClr[i];
            }

            Array.Resize<ComboBox>(ref cItemType, cItemType.Length - 1);
            Array.Resize<TextBox>(ref tbItemParam, tbItemParam.Length - 1);
            Array.Resize<ComboBox>(ref cItemProperty, cItemProperty.Length - 1);
            Array.Resize<Button>(ref btnItemClr, btnItemClr.Length - 1);
            Array.Resize<Button>(ref btnRemove, btnRemove.Length - 1);
            if (btnRemove.Length != 0)
                btnAddRow.Top = btnRemove[btnRemove.Length - 1].Top + btnRemove[btnRemove.Length - 1].Height + 10;
            else
                btnAddRow.Top = MessageLabel("TYPE").Top + MessageLabel("TYPE").Height + 10;
        }

        void btnItemClrClick(object sender, EventArgs e)
        {
            ColorDialog cDialog = new ColorDialog();
            if (cDialog.ShowDialog() == DialogResult.OK)
                ((Button)sender).BackColor = cDialog.Color;
        }


        void cResolution_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cResolution.SelectedIndex)
            {
                case 0:
                    GetAvailableDates(Resolutions.Daily);
                    break;
                case 1:
                    GetAvailableDates(Resolutions.Weekly);
                    break;
                case 2:
                    GetAvailableDates(Resolutions.Monthly);
                    break;
                case 3:
                    GetAvailableDates(Resolutions.Yearly);
                    break;
            }

            if (dAvailableDates.Length == 0)
            {
                MessageBox.Show("No archive data found, you can't create a sales/time graph");
                
                return;
            }

            dtStartDate.MinDate = dAvailableDates[0];
            dtStartDate.MaxDate = dAvailableDates[dAvailableDates.Length - 1];
            dtStartDate.ValueChanged += new EventHandler(dtStartDate_ValueChanged);
            dtStartDate.Value = dAvailableDates[0];

            dtEndDate.MaxDate = dAvailableDates[dAvailableDates.Length - 1];
            
        }

        void dtStartDate_ValueChanged(object sender, EventArgs e)
        {
            bool bFound = false;
            for (int i = 0; i < dAvailableDates.Length; i++)
            {
                if (dAvailableDates[i].CompareTo(dtStartDate.Value) > 0)
                {
                    dtEndDate.MinDate = dAvailableDates[i];
                    bFound = true;
                    break;
                }
            }
            if (!bFound)
            {
                MessageBox.Show("Can't find a date available after the selected start date. Please select another one!");
            }
        }

        string[] GenerateListOfFolders()
        {
            string[] sToReturn = new string[0];
            DateTime dtCurrent = dtStartDate.Value;
            while (dtCurrent.CompareTo(dtEndDate.Value) < 1)
            {
                string sFolder = cResolution.Items[cResolution.SelectedIndex].ToString();
                string sDay = dtCurrent.Day.ToString();
                while (sDay.Length < 2)
                    sDay = "0" + sDay;
                string sMonth = dtCurrent.Month.ToString();
                while (sMonth.Length < 2)
                    sMonth = "0" + sMonth;
                if (Directory.Exists("Archive\\" + sFolder + "\\" + dtCurrent.Year.ToString() + "." + sMonth + "." + sDay))
                {
                    if (File.Exists("Archive\\" + sFolder + "\\" + dtCurrent.Year.ToString() + "." + sMonth + "." + sDay + "\\STOCKSTA.DBF") && File.Exists("Archive\\" + sFolder + "\\" + dtCurrent.Year.ToString() + "." + sMonth + "." + sDay + "\\MAINSTOC.DBF"))
                    {
                        Array.Resize<string>(ref sToReturn, sToReturn.Length + 1);
                        sToReturn[sToReturn.Length - 1] = dtCurrent.Year.ToString() + "." + sMonth + "." + sDay;
                    }
                }
                dtCurrent = dtCurrent.AddDays(1);
            }
            return sToReturn;
        }
    }
}
