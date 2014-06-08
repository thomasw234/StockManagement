using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;
using System.Drawing.Printing;

namespace BackOffice
{
    class frmReportSelect : ScalableForm
    {
        StockEngine sEngine;
        StockEngine.Period rtSelectedReport;
        Button bGo;
        public string skipSettings = "N";

        public frmReportSelect(StockEngine.Period rType, ref StockEngine se)
        {
            sEngine = se;
            rtSelectedReport = rType;
            this.AllowScaling = false;
            this.Size = new Size(820, 233);
            SetupForm();
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.VisibleChanged += frmReportSelect_VisibleChanged;
        }

        void frmReportSelect_VisibleChanged(object sender, EventArgs e)
        {
            if (skipSettings != "N")
            {
                this.CreateReport(StockEngine.SalesReportType.AllStock, StockEngine.ReportOrderedBy.CodeAlphabetical, skipSettings);
                skipSettings = "N";
            }
        }

        void SetupForm()
        {
            AddInputControl("DATE", "Report Date", new Point(10, 10), 300, "Press F5 to see a list of possible dates, or leave blank for the latest collection date.");
            InputTextBox("DATE").KeyDown += new KeyEventHandler(DateKeyDown);
            AddInputControl("STARTCAT", "Enter the category to start at:", new Point(10, BelowLastControl), 300, "Press F5 to select a category");
            AddInputControl("ENDCAT", "Enter the category to end at:", new Point(10, BelowLastControl), 300, "Press F5 to select a category");
            AddInputControl("SORP", "<S>creen, <P>rinter or Spreadshee<T>", new Point(10, BelowLastControl), 300);
            InputTextBox("STARTCAT").Text = sEngine.GetFirstCategoryCode();
            InputTextBox("ENDCAT").Text = sEngine.GetLastCategoryCode();
            InputTextBox("SORP").Text = "S";
            InputTextBox("SORP").KeyDown += new KeyEventHandler(frmRerportSelectSORP_KeyDown);
            InputTextBox("SORP").GotFocus += sorpGotFocus;
            InputTextBox("STARTCAT").KeyDown += new KeyEventHandler(frmReportSelectSTARTCAT_KeyDown);
            InputTextBox("ENDCAT").KeyDown += new KeyEventHandler(frmReportSelectENDCAT_KeyDown);

            AlignInputTextBoxes();

            this.Text = "Report Options";

            bGo = new Button();
            bGo.Location = new Point(10, BelowLastControl);
            bGo.Size = new Size(100, this.ClientSize.Height - 10 - bGo.Top);
            bGo.Text = "Done";
            this.Controls.Add(bGo);
            bGo.Click += new EventHandler(bGo_Click);
        }

        void sorpGotFocus(object sender, EventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        void bGo_Click(object sender, EventArgs e)
        {
            CreateReport();
        }

        void DateKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                frmReportDates frd = new frmReportDates(rtSelectedReport);
                frd.ShowDialog();
                if (frd.SelectedFolder != "$NONE")
                {
                    InputTextBox("DATE").Text = frd.SelectedFolder;
                    InputTextBox("STARTCAT").Focus();
                }
                else
                {
                    InputTextBox("DATE").Text = "";
                    InputTextBox("DATE").Focus();
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        void frmReportSelectENDCAT_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                frmCategorySelect fcSelect = new frmCategorySelect(ref sEngine);
                fcSelect.ShowDialog();
                if (fcSelect.SelectedItemCategory != "$NULL")
                    InputTextBox("ENDCAT").Text = fcSelect.SelectedItemCategory;
                fcSelect.Dispose();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                InputTextBox("STARTCAT").Focus();
            }
        }

        void frmReportSelectSTARTCAT_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                frmCategorySelect fcSelect = new frmCategorySelect(ref sEngine);
                fcSelect.ShowDialog();
                if (fcSelect.SelectedItemCategory != "$NULL")
                InputTextBox("STARTCAT").Text = fcSelect.SelectedItemCategory;
                fcSelect.Dispose();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                InputTextBox("DATE").Focus();
            }
        }

        void frmRerportSelectSORP_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CreateReport();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                InputTextBox("ENDCAT").Focus();
            }
            else if (e.Alt && e.KeyCode == Keys.G)
            {
                CreateReport();
            }
            else
                InputTextBox("SORP").Text = "";
        }

        private void CreateReport(StockEngine.SalesReportType sType, StockEngine.ReportOrderedBy rOrder, string output)
        {
            // Do Report
            if (output.Equals("S", StringComparison.OrdinalIgnoreCase))
            {
                if (InputTextBox("DATE").Text != "" && System.IO.Directory.Exists(InputTextBox("DATE").Text))
                {
                    StockEngine sOldEngine = new StockEngine(InputTextBox("DATE").Text);
                    sOldEngine.SalesReportToFile(InputTextBox("STARTCAT").Text, InputTextBox("ENDCAT").Text, sType, rtSelectedReport, rOrder);
                    string sTitle = "";
                    switch (rtSelectedReport)
                    {
                        case StockEngine.Period.Daily:
                            string sDate = sOldEngine.GetLastCollectionDate();
                            string sTitleDate = sDate[0].ToString() + sDate[1].ToString() + "/" + sDate[2].ToString() + sDate[3].ToString() + "/" + sDate[4].ToString() + sDate[5].ToString();
                            sTitle = "Daily Sales Report for " + sTitleDate;
                            break;
                        case StockEngine.Period.Weekly:
                            sTitle = "Weekly Sales Report for Week Commencing " + sEngine.GetWeekCommencingDate();
                            break;
                        case StockEngine.Period.Monthly:
                            sTitle = "Monthly Sales Report for " + sEngine.GetMonthDate();
                            break;
                        case StockEngine.Period.Yearly:
                            string sLastCollection = sEngine.GetLastCollectionDate();
                            string sYear = sLastCollection[4].ToString() + sLastCollection[5].ToString();
                            sTitle = "Yearly Sales Report for " + "20" + sYear;
                            break;
                    }
                    frmReportViewer fViewer = new frmReportViewer(StockEngine.ReportType.SalesReport);
                    fViewer.ShowDialog();
                    this.Close();
                }
                else
                {
                    sEngine.SalesReportToFile(InputTextBox("STARTCAT").Text, InputTextBox("ENDCAT").Text, sType, rtSelectedReport, rOrder);
                    string sTitle = "";
                    switch (rtSelectedReport)
                    {
                        case StockEngine.Period.Daily:
                            string sDate = sEngine.GetLastCollectionDate();
                            string sTitleDate = sDate[0].ToString() + sDate[1].ToString() + "/" + sDate[2].ToString() + sDate[3].ToString() + "/" + sDate[4].ToString() + sDate[5].ToString();
                            sTitle = "Daily Sales Report for " + sTitleDate;
                            break;
                        case StockEngine.Period.Weekly:
                            sTitle = "Weekly Sales Report for Week Commencing " + sEngine.GetWeekCommencingDate();
                            break;
                        case StockEngine.Period.Monthly:
                            sTitle = "Monthly Sales Report for " + sEngine.GetMonthDate();
                            break;
                        case StockEngine.Period.Yearly:
                            string sLastCollection = sEngine.GetLastCollectionDate();
                            string sYear = sLastCollection[4].ToString() + sLastCollection[5].ToString();
                            sTitle = "Yearly Sales Report for " + "20" + sYear;
                            break;
                    }
                    frmReportViewer fViewer = new frmReportViewer(StockEngine.ReportType.SalesReport);
                    fViewer.ShowDialog();
                    this.Close();
                }
            }
            else if (output.Equals("P", StringComparison.OrdinalIgnoreCase))
            {
                if (InputTextBox("DATE").Text != "" && System.IO.Directory.Exists(InputTextBox("DATE").Text))
                {
                    StockEngine sOldEngine = new StockEngine(InputTextBox("DATE").Text);
                    sOldEngine.SalesReportToFile(InputTextBox("STARTCAT").Text, InputTextBox("ENDCAT").Text, sType, rtSelectedReport, rOrder);
                    string sTitle = "";
                    switch (rtSelectedReport)
                    {
                        case StockEngine.Period.Daily:
                            string sDate = sOldEngine.GetLastCollectionDate();
                            string sTitleDate = sDate[0].ToString() + sDate[1].ToString() + "/" + sDate[2].ToString() + sDate[3].ToString() + "/" + sDate[4].ToString() + sDate[5].ToString();
                            sTitle = "Daily Sales Report for " + sTitleDate;
                            break;
                        case StockEngine.Period.Weekly:
                            sTitle = "Weekly Sales Report for Week Commencing " + sEngine.GetWeekCommencingDate();
                            break;
                        case StockEngine.Period.Monthly:
                            sTitle = "Monthly Sales Report for " + sEngine.GetMonthDate();
                            break;
                        case StockEngine.Period.Yearly:
                            string sLastCollection = sEngine.GetLastCollectionDate();
                            string sYear = sLastCollection[4].ToString() + sLastCollection[5].ToString();
                            sTitle = "Yearly Sales Report for " + "20" + sYear;
                            break;
                    }
                    sOldEngine.SalesReportToPrinter(InputTextBox("STARTCAT").Text, InputTextBox("ENDCAT").Text, sType, rtSelectedReport, rOrder);
                    this.Close();
                }
                else
                {
                    sEngine.SalesReportToFile(InputTextBox("STARTCAT").Text, InputTextBox("ENDCAT").Text, sType, rtSelectedReport, rOrder);
                    string sTitle = "";
                    switch (rtSelectedReport)
                    {
                        case StockEngine.Period.Daily:
                            string sDate = sEngine.GetLastCollectionDate();
                            string sTitleDate = sDate[0].ToString() + sDate[1].ToString() + "/" + sDate[2].ToString() + sDate[3].ToString() + "/" + sDate[4].ToString() + sDate[5].ToString();
                            sTitle = "Daily Sales Report for " + sTitleDate;
                            break;
                        case StockEngine.Period.Weekly:
                            sTitle = "Weekly Sales Report for Week Commencing " + sEngine.GetWeekCommencingDate();
                            break;
                        case StockEngine.Period.Monthly:
                            sTitle = "Monthly Sales Report for " + sEngine.GetMonthDate();
                            break;
                        case StockEngine.Period.Yearly:
                            string sLastCollection = sEngine.GetLastCollectionDate();
                            string sYear = sLastCollection[4].ToString() + sLastCollection[5].ToString();
                            sTitle = "Yearly Sales Report for " + "20" + sYear;
                            break;
                    }

                    sEngine.SalesReportToPrinter(InputTextBox("STARTCAT").Text, InputTextBox("ENDCAT").Text, sType, rtSelectedReport, rOrder);
                   
                }
            }
            else if (output.Equals("T", StringComparison.OrdinalIgnoreCase))
            {
                if (InputTextBox("DATE").Text != "" && System.IO.Directory.Exists(InputTextBox("DATE").Text))
                {
                    StockEngine sOldEngine = new StockEngine(InputTextBox("DATE").Text);
                    sOldEngine.SalesReportToSpreadsheet(InputTextBox("STARTCAT").Text, InputTextBox("ENDCAT").Text, sType, rtSelectedReport, rOrder);
                    string sTitle = "";
                    switch (rtSelectedReport)
                    {
                        case StockEngine.Period.Daily:
                            string sDate = sOldEngine.GetLastCollectionDate();
                            string sTitleDate = sDate[0].ToString() + sDate[1].ToString() + "/" + sDate[2].ToString() + sDate[3].ToString() + "/" + sDate[4].ToString() + sDate[5].ToString();
                            sTitle = "Daily Sales Report for " + sTitleDate;
                            break;
                        case StockEngine.Period.Weekly:
                            sTitle = "Weekly Sales Report for Week Commencing " + sEngine.GetWeekCommencingDate();
                            break;
                        case StockEngine.Period.Monthly:
                            sTitle = "Monthly Sales Report for " + sEngine.GetMonthDate();
                            break;
                        case StockEngine.Period.Yearly:
                            string sLastCollection = sEngine.GetLastCollectionDate();
                            string sYear = sLastCollection[4].ToString() + sLastCollection[5].ToString();
                            sTitle = "Yearly Sales Report for " + "20" + sYear;
                            break;
                    }
                    this.Close();
                }
                else
                {
                    sEngine.SalesReportToSpreadsheet(InputTextBox("STARTCAT").Text, InputTextBox("ENDCAT").Text, sType, rtSelectedReport, rOrder);
                    string sTitle = "";
                    switch (rtSelectedReport)
                    {
                        case StockEngine.Period.Daily:
                            string sDate = sEngine.GetLastCollectionDate();
                            string sTitleDate = sDate[0].ToString() + sDate[1].ToString() + "/" + sDate[2].ToString() + sDate[3].ToString() + "/" + sDate[4].ToString() + sDate[5].ToString();
                            sTitle = "Daily Sales Report for " + sTitleDate;
                            break;
                        case StockEngine.Period.Weekly:
                            sTitle = "Weekly Sales Report for Week Commencing " + sEngine.GetWeekCommencingDate();
                            break;
                        case StockEngine.Period.Monthly:
                            sTitle = "Monthly Sales Report for " + sEngine.GetMonthDate();
                            break;
                        case StockEngine.Period.Yearly:
                            string sLastCollection = sEngine.GetLastCollectionDate();
                            string sYear = sLastCollection[4].ToString() + sLastCollection[5].ToString();
                            sTitle = "Yearly Sales Report for " + "20" + sYear;
                            break;
                    }
                }
            }
            this.Close();
        }

        void CreateReport()
        {
            // Get Layout
            frmSalesReportType fType = new frmSalesReportType();
            fType.ShowDialog();
            if (!fType.OptionSelected)
            {
                InputTextBox("SORP").Focus();
                return;
            }
            StockEngine.SalesReportType sType = fType.sType;
            // Get Order
            StockEngine.ReportOrderedBy rOrder = StockEngine.ReportOrderedBy.DescAlphabetical;
            if (sType == StockEngine.SalesReportType.AllStock)
            {
                frmSalesReportOrder fOrder = new frmSalesReportOrder();
                fOrder.ShowDialog();
                if (!fOrder.OptionSelected)
                {
                    InputTextBox("SORP").Focus();
                    return;
                }
                rOrder = fOrder.SelectedOrder;
            }

            this.CreateReport(sType, rOrder, InputTextBox("SORP").Text);
            
        }


    }
}
