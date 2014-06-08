using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;
using BackOffice.Forms;

namespace BackOffice
{
    class frmSubMenu : ScalableForm
    {
        public enum SubMenuType { Stock, Reports, Orders, Setup, SalesReports, StockLevelReports, EndOfPeriod, PeriodSelect, SetupCompany, SetupTills, SetupBackOffice, DodgyFixes };
        CListBox lbMenuChoices;
        SubMenuType sThisType;
        StockEngine sEngine;
        public string shortcutString { get; set; }

        public frmSubMenu(SubMenuType sType, ref StockEngine se)
        {
            sEngine = se;
            sThisType = sType;
            AllowScaling = false;
            SetupMenu();
            this.WindowState = FormWindowState.Maximized;
            this.Text = sType.ToString() + " Menu";
            this.Resize += new EventHandler(frmSubMenu_Resize);
            this.VisibleChanged += frmSubMenu_VisibleChanged;

            
        }

        void frmSubMenu_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                if (shortcutString != null)
                {
                    if (shortcutString.Length >= 2)
                    {
                        try
                        {
                            int menuOption = 0;
                            if (this.lbMenuChoices.Items.Count >= 10)
                                menuOption = Convert.ToInt32(shortcutString[0].ToString() + shortcutString[1].ToString());
                            else
                                menuOption = Convert.ToInt32(shortcutString[0].ToString());
                            lbMenuChoices.SelectedIndex = menuOption - 1;
                            SendKeys.Send("~");
                        }
                        catch
                        {
                            // Do nothing, not a menu shortcut
                        }
                    }
                }
            }
        }


        void frmSubMenu_Resize(object sender, EventArgs e)
        {
            lbMenuChoices.Size = new Size(this.ClientSize.Width - (2* lbMenuChoices.Left), this.ClientSize.Height - 12 - lbMenuChoices.Top);
        }

        void SetupMenu()
        {
            AddMessage("LOCA", sThisType.ToString(), new Point(10, 10));
            MessageLabel("LOCA").Font = new Font(this.Font.Name, 20.0f);
            lbMenuChoices = new CListBox();
            lbMenuChoices.BorderStyle = BorderStyle.FixedSingle;
            lbMenuChoices.Size = new Size(this.ClientSize.Width - 12 - lbMenuChoices.Left, this.ClientSize.Height - 12 - lbMenuChoices.Top);
            lbMenuChoices.Location = new Point(12, 54);
            this.Controls.Add(lbMenuChoices);
            lbMenuChoices.Font = new Font(lbMenuChoices.Font.FontFamily, 16.0f);
            lbMenuChoices.KeyDown += new KeyEventHandler(lbMenuChoices_KeyDown);

            if (sThisType == SubMenuType.Stock)
            {
                lbMenuChoices.Items.Add("Stock Levels");
                lbMenuChoices.Items.Add("Detailed Stock Information");
                lbMenuChoices.Items.Add("Add a Stock Item");
                lbMenuChoices.Items.Add("Edit a Stock Item");
                lbMenuChoices.Items.Add("Batch Add Stock Items");
                lbMenuChoices.Items.Add("Batch Edit Stock Items");
                lbMenuChoices.Items.Add("Add Multi-Barcode Item");
                lbMenuChoices.Items.Add("Edit Multi-Barcode Item");
                lbMenuChoices.Items.Add("Internal Stock Transfer");
                lbMenuChoices.Items.Add("Receive a Commission Item");
                //lbMenuChoices.Items.Add("Mark Commission Items As Paid For");
                lbMenuChoices.Items.Add("Return a Commission Item Unsold");
                lbMenuChoices.Items.Add("Mark a Commission Item as Paid For");
                lbMenuChoices.Items.Add("Change VAT Amount on Items");
                lbMenuChoices.Items.Add("End of Period");
                lbMenuChoices.Items.Add("Old Batch Edit");
            }
            else if (sThisType == SubMenuType.Reports)
            {
                lbMenuChoices.Items.Add("Sales Reports");
                lbMenuChoices.Items.Add("Stock Reports");
                lbMenuChoices.Items.Add("Add/Edit Category Groups");
            }
            else if (sThisType == SubMenuType.SalesReports)
            {
                lbMenuChoices.Items.Add("Daily Sales Report");
                lbMenuChoices.Items.Add("Weekly Sales Report");
                lbMenuChoices.Items.Add("Monthly Sales Report");
                lbMenuChoices.Items.Add("Year to Date Sales Report");
                lbMenuChoices.Items.Add("Commission Sales (Single Commissioner) Report");
                lbMenuChoices.Items.Add("Commission Sales Summary Report");
                //lbMenuChoices.Items.Add("Create Sales/Time Graph");
            }
            else if (sThisType == SubMenuType.StockLevelReports)
            {
                lbMenuChoices.Items.Add("Category / Category Group Stock Level Report");
                lbMenuChoices.Items.Add("Stock Valuation Report");
                lbMenuChoices.Items.Add("Detailed Stock Item Enquiry");
                lbMenuChoices.Items.Add("Create Outstanding Items Report");
                lbMenuChoices.Items.Add("Create a Report Showing Out-Of-Stock Lengths");
            }
            else if (sThisType == SubMenuType.Setup)
            {
                /*
                lbMenuChoices.Items.Add("Company, Shop & Till Settings");
                lbMenuChoices.Items.Add("Upload Changes To All Tills");
                lbMenuChoices.Items.Add("Total Data Upload To A Till");
                lbMenuChoices.Items.Add("Edit Till Passwords");
                lbMenuChoices.Items.Add("Add / Edit Categories");
                lbMenuChoices.Items.Add("Add / Edit a Supplier");
                lbMenuChoices.Items.Add("Add a Commissionner");
                lbMenuChoices.Items.Add("Add / Edit an Account");
                lbMenuChoices.Items.Add("Edit Staff");
                lbMenuChoices.Items.Add("Fix On Order Quantities");
                lbMenuChoices.Items.Add("View Customer E-Mail Addresses");
                lbMenuChoices.Items.Add("Install a software update");
                lbMenuChoices.Items.Add("Update Till Software");
                lbMenuChoices.Items.Add("Check Till Connections");
                lbMenuChoices.Items.Add("About Backoffice & GTill For Windows");
                lbMenuChoices.Items.Add("Remove Blank Barcode");
                 */
                lbMenuChoices.Items.Add("Company Information");
                lbMenuChoices.Items.Add("Tills");
                lbMenuChoices.Items.Add("BackOffice");
                lbMenuChoices.Items.Add("About");
                lbMenuChoices.Items.Add("Dodgy Fixes");
            }
            else if (sThisType == SubMenuType.DodgyFixes)
            {
                lbMenuChoices.Items.Add("Fix on Order Quantities");
                lbMenuChoices.Items.Add("Fix End of 2010 Bug");
                lbMenuChoices.Items.Add("Fix Average Cost Zero (But Last Cost Isn't) Bug");
                lbMenuChoices.Items.Add("Fix COGS Zero bug");
                lbMenuChoices.Items.Add("Fix Incorrectly Entered Item Prices");
                lbMenuChoices.Items.Add("Force Rebuild Sales Index");
                lbMenuChoices.Items.Add("Fix Average Cost Zero");
                lbMenuChoices.Items.Add("Force Rebuild out-of-stock Length Database");
                lbMenuChoices.Items.Add("Compress Uncompressed Sections of the Archive");
                lbMenuChoices.Items.Add("Re-Calculate this week's total sales");
                lbMenuChoices.Items.Add("Find missing order lines");
                lbMenuChoices.Items.Add("Fix incorrect minimum order quantities");
            }
            else if (sThisType == SubMenuType.SetupCompany)
            {
                lbMenuChoices.Items.Add("Company Name");
                lbMenuChoices.Items.Add("Address");
                lbMenuChoices.Items.Add("VAT Number");
                lbMenuChoices.Items.Add("VAT Rates");
                lbMenuChoices.Items.Add("E-Mail Address");
                lbMenuChoices.Items.Add("Shops");
                lbMenuChoices.Items.Add("Staff Names");
                lbMenuChoices.Items.Add("System Passwords");
            }
            else if (sThisType == SubMenuType.SetupTills)
            {
                lbMenuChoices.Items.Add("Upload Pending Changes");
                lbMenuChoices.Items.Add("Total Upload to a Till");
                lbMenuChoices.Items.Add("VAT Rates");
                lbMenuChoices.Items.Add("Passwords");
                lbMenuChoices.Items.Add("Staff Names");
                lbMenuChoices.Items.Add("Credit Cards");
                lbMenuChoices.Items.Add("Receipt Messages");
                lbMenuChoices.Items.Add("Edit Discount Warning Threshold");
                lbMenuChoices.Items.Add("Check Connections");
                lbMenuChoices.Items.Add("Add a Till");
                lbMenuChoices.Items.Add("Edit a Till");
                lbMenuChoices.Items.Add("Move a till to this computer");
                lbMenuChoices.Items.Add("Add or Edit Offers");
            }
            else if (sThisType == SubMenuType.SetupBackOffice)
            {
                lbMenuChoices.Items.Add("Categories");
                lbMenuChoices.Items.Add("Suppliers");
                lbMenuChoices.Items.Add("Commissioners");
                lbMenuChoices.Items.Add("Accounts");
                lbMenuChoices.Items.Add("Customer E-Mail Addresses");
                lbMenuChoices.Items.Add("Check for updates");
                lbMenuChoices.Items.Add("Crash Backoffice");
                lbMenuChoices.Items.Add("Change Printer Font Size");
                lbMenuChoices.Items.Add("Change Printer Border Size");
                lbMenuChoices.Items.Add("Restore to a previous day");
                lbMenuChoices.Items.Add("Change default printer");
            }
            else if (sThisType == SubMenuType.Orders)
            {
                lbMenuChoices.Items.Add("Add or Edit an Order");
                lbMenuChoices.Items.Add("Receive an Order");
                lbMenuChoices.Items.Add("Enter Invoice Costs");
                lbMenuChoices.Items.Add("Print an Order");
                lbMenuChoices.Items.Add("Export an Order to Excel");
                lbMenuChoices.Items.Add("Add or Edit a Supplier");
                lbMenuChoices.Items.Add("Receive A Commission Item");
                lbMenuChoices.Items.Add("Create Outstanding Items Report");
            }
            else if (sThisType == SubMenuType.PeriodSelect)
            {
                lbMenuChoices.Items.Add("End of Day");
                lbMenuChoices.Items.Add("End of Week");
                lbMenuChoices.Items.Add("End of Month");
                lbMenuChoices.Items.Add("End of Year");
            }
            lbMenuChoices.Focus();
            lbMenuChoices.SelectedIndex = 0;

            /*for (int i = 0; i < lbMenuChoices.Items.Count; i++)
            {
                lbMenuChoices.Items[i] = new String('0', (2 - (i+1).ToString().Length)) + (i+1).ToString() + ".  " + lbMenuChoices.Items[i];
            }*/
        }

        void lbMenuChoices_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Q)
            {
                this.Close();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                if (this.lbMenuChoices.Items.Count >= 10)
                {
                    if (shortcutString != null && shortcutString.Length >= 2)
                        this.shortcutString = shortcutString.Substring(2, shortcutString.Length - 2);
                }
                else
                {
                    if (shortcutString != null && shortcutString.Length >= 1)
                        this.shortcutString = shortcutString.Substring(1, shortcutString.Length - 1);
                }


                if (sThisType == SubMenuType.Stock)
                {
                    switch (lbMenuChoices.SelectedIndex)
                    {
                        case 0:
                            // Show Stock Levels
                            frmSearchForItemV2 fsfi = new frmSearchForItemV2(ref sEngine);
                            fsfi.CheckForPartialBarcodeFromScanner(shortcutString);
                            fsfi.ShowDialog();
                            if (fsfi.GetItemBarcode() != "NONE_SELECTED")
                            {
                                if (!fsfi.bSelectedType4)
                                {
                                    frmDetailedItemEnquiry fdie = new frmDetailedItemEnquiry(ref sEngine, fsfi.GetItemBarcode());
                                    fdie.ShowDialog();
                                }
                                else
                                {
                                    frmAddMultiItem famil = new frmAddMultiItem(ref sEngine, fsfi.GetItemBarcode());
                                    famil.ShowDialog();
                                }
                            }
                            break;
                        case 1:
                            frmGetBarcode fgb = new frmGetBarcode(ref sEngine);
                            fgb.shortcutString = this.shortcutString;
                            fgb.ShowDialog();
                            if (fgb.Barcode != "" && fgb.Barcode != null)
                            {
                                frmDetailedItemEnquiry fdie = new frmDetailedItemEnquiry(ref sEngine, fgb.Barcode);
                                fdie.ShowDialog();
                                fdie.Dispose();
                            }
                            fgb.Dispose();
                            break;
                        case 2:
                            frmAddEditItem fai = new frmAddEditItem(ref sEngine);
                            fai.EditingItem = false;
                            fai.AddingBarcode = this.shortcutString;
                            fai.ShowDialog();
                            fai.Dispose();
                            break;
                        case 3:
                            frmAddEditItem faei = new frmAddEditItem(ref sEngine);
                            faei.EditingItem = true;
                            faei.EditingBarcode = this.shortcutString;
                            faei.ShowDialog();
                            faei.Dispose();
                            break;
                        case 4:
                            frmBatchAddItems fbai = new frmBatchAddItems(ref sEngine, true);
                            fbai.ShowDialog();
                            break;
                        case 5:
                            //frmBatchAddItems fbei = new frmBatchAddItems(ref sEngine, false);
                            frmBatchEditItems fbei = new frmBatchEditItems(ref sEngine);
                            fbei.ShowDialog();
                            break;
                        case 6:
                            /*frmAddMultiItem fami = new frmAddMultiItem(ref sEngine);
                            fami.ShowDialog();*/
                            AddMultiBarcodeItem ambi = new AddMultiBarcodeItem(ref sEngine);
                            break;
                        case 7:
                            frmAddMultiItem femi = new frmAddMultiItem(ref sEngine);
                            femi.Editing = true;
                            femi.ShowDialog();
                            break;
                        case 8:
                            frmStockTransfer fst = new frmStockTransfer(ref sEngine);
                            fst.ShowDialog();
                            break;
                        case 9:
                            frmReceiveComissionItem frci = new frmReceiveComissionItem(ref sEngine, true);
                            frci.ShowDialog();
                            break;
                        case 10:
                            /*frmListOfCommissioners floc = new frmListOfCommissioners(ref sEngine);
                            floc.OnlyShowOwedCommissioners = true;
                            floc.ShowDialog();
                            if (floc.Commissioner != "$NONE")
                            {
                                sEngine.MarkCommissionerAsPaid(floc.Commissioner);
                                MessageBox.Show(sEngine.GetCommissionerName(floc.Commissioner) + " has been marked as paid.");
                            }*/
                            frmReceiveComissionItem frmrci = new frmReceiveComissionItem(ref sEngine, false);
                            frmrci.ShowDialog();
                            break;
                        case 11:
                            frmPayForCommissionItem fpfci = new frmPayForCommissionItem(ref sEngine);
                            break;
                        case 12:
                            frmListOfVATRates flovr = new frmListOfVATRates(ref sEngine);
                            flovr.ShowDialog();
                            if (flovr.sSelectedVATCode != "NULL")
                            {
                                frmSingleInputBox fsibRate = new frmSingleInputBox("Enter the new VAT Rate (existing is " + sEngine.GetVATRateFromVATCode(flovr.sSelectedVATCode) + "%).", ref sEngine);
                                fsibRate.ShowDialog();
                                if (fsibRate.Response != "$NONE")
                                {
                                    sEngine.ChangeVATOnItems(flovr.sSelectedVATCode, Convert.ToDecimal(fsibRate.Response));
                                    MessageBox.Show("Done");
                                }
                            }
                            break;
                        case 13:
                            frmSubMenu fPeriod = new frmSubMenu(SubMenuType.PeriodSelect, ref sEngine);
                            fPeriod.shortcutString = this.shortcutString;
                            fPeriod.ShowDialog();
                            break;
                        case 14:
                            frmBatchAddItems fbedi = new frmBatchAddItems(ref sEngine, false);
                            //frmBatchEditItems fbedi = new frmBatchEditItems(ref sEngine);
                            fbedi.ShowDialog();
                            break;
                    }
                }
                else if (sThisType == SubMenuType.Reports)
                {
                    switch (lbMenuChoices.SelectedIndex)
                    {
                        case 0:
                            // Sales Reports
                            frmSubMenu fSalesReports = new frmSubMenu(SubMenuType.SalesReports, ref sEngine);
                            fSalesReports.shortcutString = this.shortcutString;
                            fSalesReports.ShowDialog();
                            break;
                        case 1:
                            // Stock Level Reports
                            frmSubMenu fStockReports = new frmSubMenu(SubMenuType.StockLevelReports, ref sEngine);
                            fStockReports.shortcutString = shortcutString;
                            fStockReports.ShowDialog();
                            break;
                        case 2:
                            // Add/Edit Category Groups
                            frmCatGroupAddEdit fcgae = new frmCatGroupAddEdit(ref sEngine);
                            fcgae.ShowDialog();
                            break;
                    }
                }
                else if (sThisType == SubMenuType.SalesReports)
                {
                    switch (lbMenuChoices.SelectedIndex)
                    {
                        case 0:
                            // Daily Sales Report
                            frmReportSelect fDailyViewer = new frmReportSelect(StockEngine.Period.Daily, ref sEngine);
                            if (this.shortcutString != null)
                                fDailyViewer.skipSettings = shortcutString;
                            fDailyViewer.ShowDialog();
                            break;
                        case 1:
                            // Weekly Sales Report
                            frmReportSelect fWeekViewer = new frmReportSelect(StockEngine.Period.Weekly, ref sEngine);
                            if (this.shortcutString != null)
                                fWeekViewer.skipSettings = shortcutString;
                            fWeekViewer.ShowDialog();
                            break;
                        case 2:
                            // Monthly Sales Report
                            frmReportSelect fMonthViewer = new frmReportSelect(StockEngine.Period.Monthly, ref sEngine);
                            if (this.shortcutString != null)
                                fMonthViewer.skipSettings = this.shortcutString;
                            fMonthViewer.ShowDialog();
                            break;
                        case 3:
                            // YTD Sales Report
                            frmReportSelect fYearViewer = new frmReportSelect(StockEngine.Period.Yearly, ref sEngine);
                            if (this.shortcutString != null)
                                fYearViewer.skipSettings = this.shortcutString;
                            fYearViewer.ShowDialog();
                            break;
                        case 4:
                            // Comission Sales Report
                            /*frmListOfCommissioners floc = new frmListOfCommissioners(ref sEngine);
                            floc.ShowDialog();
                            if (floc.Commissioner != "$NONE")
                            {
                                frmSingleInputBox fGetSorP = new frmSingleInputBox("<S>creen or <P>rinter?", ref sEngine);
                                fGetSorP.ShowDialog();
                                if (fGetSorP.Response != "$NONE")
                                {
                                    frmSingleInputBox fGetStartDate = new frmSingleInputBox("Enter the date to begin the report from (DDMMYY):", ref sEngine);
                                    fGetStartDate.ShowDialog();
                                    if (fGetStartDate.Response != "$NONE")
                                    {
                                        frmSingleInputBox fGetEndDate = new frmSingleInputBox("Enter the date to end the report from (DDMMYY):", ref sEngine);
                                        fGetEndDate.ShowDialog();
                                        if (fGetEndDate.Response != "$NONE")
                                        {
                                            bool bArtistPresent = false;
                                            if (MessageBox.Show("Will the artist be seeing this report? If so, Sale Price and Profit will not be included.", "Artists", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                            {
                                                bArtistPresent = true;
                                            }
                                            if (fGetSorP.Response.ToUpper() == "P")
                                            {
                                                sEngine.ComissionReportToPrinter(floc.Commissioner, fGetStartDate.Response, fGetEndDate.Response, bArtistPresent);
                                            }
                                            else if (fGetSorP.Response.ToUpper() == "S")
                                            {
                                                sEngine.ComissionReportToFile(floc.Commissioner, fGetStartDate.Response, fGetEndDate.Response, bArtistPresent);
                                                frmReportViewer frv = new frmReportViewer(StockEngine.ReportType.ComissionReport);
                                                frv.ShowDialog();
                                            }
                                        }
                                    }
                                }
                            }*/

                            // New One
                            frmListOfCommissioners floc = new frmListOfCommissioners(ref sEngine);
                            floc.ShowDialog();
                            if (floc.Commissioner != "$NONE")
                            {
                                frmSingleInputBox fGetSorP = new frmSingleInputBox("<S>creen or <P>rinter?", ref sEngine);
                                fGetSorP.ShowDialog();
                                if (fGetSorP.Response != "$NONE")
                                {
                                    frmCommissionPeriods fcp = new frmCommissionPeriods();
                                    fcp.ShowDialog();
                                    if (fcp.Chosen)
                                    {
                                        string startDate = "", endDate = "";
                                        if (fcp.ChosenPeriod != StockEngine.Period.Other)
                                        {
                                            startDate = fcp.getStartPeriodDate();
                                            endDate = fcp.getEndDate();
                                        }
                                        else
                                        {

                                            frmSingleInputBox fGetStartDate = new frmSingleInputBox("Enter the date to begin the report from (DDMMYY):", ref sEngine);
                                            fGetStartDate.ShowDialog();
                                            if (fGetStartDate.Response != "$NONE")
                                            {
                                                frmSingleInputBox fGetEndDate = new frmSingleInputBox("Enter the date to end the report from (DDMMYY):", ref sEngine);
                                                fGetEndDate.ShowDialog();
                                                if (fGetEndDate.Response != "$NONE")
                                                {
                                                    startDate = fGetStartDate.Response;
                                                    endDate = fGetEndDate.Response;
                                                }
                                            }
                                        }

                                        if (startDate != "" && endDate != "")
                                        {
                                            bool bArtistPresent = false;
                                            if (MessageBox.Show("Will the artist be seeing this report? If so, Sale Price and Profit will not be included.", "Artists", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                            {
                                                bArtistPresent = true;
                                            }

                                            if (fGetSorP.Response.ToUpper() == "P")
                                            {
                                                sEngine.ComissionReportToPrinter(floc.Commissioner, startDate, endDate, bArtistPresent);
                                            }
                                            else if (fGetSorP.Response.ToUpper() == "S")
                                            {
                                                sEngine.ComissionReportToFile(floc.Commissioner, startDate, endDate, bArtistPresent);
                                                frmReportViewer frv = new frmReportViewer(StockEngine.ReportType.ComissionReport);
                                                frv.ShowDialog();
                                            }
                                        }


                                    }
                                }
                            }

                            break;
                        case 5:
                            // Comission Sales Summary Report
                            frmSingleInputBox fGetScrorP = new frmSingleInputBox("<S>creen or <P>rinter?", ref sEngine);
                            fGetScrorP.ShowDialog();
                            if (fGetScrorP.Response != "$NONE")
                            {
                                frmCommissionPeriods fcp = new frmCommissionPeriods();
                                fcp.ShowDialog();
                                if (fcp.Chosen)
                                {
                                    string startDate = "", endDate = "";
                                    if (fcp.ChosenPeriod != StockEngine.Period.Other)
                                    {
                                        startDate = fcp.getStartPeriodDate();
                                        endDate = fcp.getEndDate();
                                    }
                                    else
                                    {

                                        frmSingleInputBox fGetStartDate = new frmSingleInputBox("Enter the date to begin the report from (DDMMYY):", ref sEngine);
                                        fGetStartDate.ShowDialog();
                                        if (fGetStartDate.Response != "$NONE")
                                        {
                                            frmSingleInputBox fGetEndDate = new frmSingleInputBox("Enter the date to end the report from (DDMMYY):", ref sEngine);
                                            fGetEndDate.ShowDialog();
                                            if (fGetEndDate.Response != "$NONE")
                                            {
                                                startDate = fGetStartDate.Response;
                                                endDate = fGetEndDate.Response;
                                            }
                                        }
                                    }

                                    if (startDate != "" && endDate != "")
                                    {
                                        if (fGetScrorP.Response.ToUpper() == "P")
                                        {
                                            sEngine.CommissionSummaryReportToPrinter(startDate, endDate);
                                            //sEngine.ComissionSummaryReportToPrinter(floc.Commissioner, fGetStartDate.Response, fGetEndDate.Response, true);
                                        }
                                        else if (fGetScrorP.Response.ToUpper() == "S")
                                        {
                                            sEngine.CommissionSummaryReportToFile(startDate, endDate);
                                            frmReportViewer frv = new frmReportViewer(StockEngine.ReportType.CommissionSummaryReport);
                                            frv.ShowDialog();
                                        }
                                    }


                                }
                            }

                            break;
                        case 6:
                            frmGraphSettings fgSettings = new frmGraphSettings(ref sEngine);
                            fgSettings.ShowDialog();
                            break;
                    }
                }
                else if (sThisType == SubMenuType.StockLevelReports)
                {
                    switch (lbMenuChoices.SelectedIndex)
                    {
                        case 0:
                            // Stock Level Report
                            frmStockLevelReportConfig fStockReport = new frmStockLevelReportConfig(ref sEngine);
                            fStockReport.SkipSettings = this.shortcutString;
                            fStockReport.ShowDialog();
                            break;
                        case 1:
                            frmStockValSetup fsvs = new frmStockValSetup(ref sEngine);
                            fsvs.SkipSettings = this.shortcutString;
                            fsvs.ShowDialog();
                            break;
                        case 2:
                            frmGetBarcode fgb = new frmGetBarcode(ref sEngine);
                            fgb.shortcutString = this.shortcutString;
                            fgb.ShowDialog();
                            if (fgb.Barcode != "" && fgb.Barcode != null)
                            {
                                frmDetailedItemEnquiry fdie = new frmDetailedItemEnquiry(ref sEngine, fgb.Barcode);
                                fdie.ShowDialog();
                                fdie.Dispose();
                            }
                            fgb.Dispose();
                            break;
                        case 3:
                            frmOutstandingItemsSetup fois = new frmOutstandingItemsSetup(ref sEngine);
                            fois.SkipCode = this.shortcutString;
                            fois.ShowDialog();
                            break;
                        case 4:
                            // Out-of-stock length report
                            frmListOfShops flos = new frmListOfShops(ref sEngine);
                            flos.ShowDialog();
                            if (flos.SelectedShopCode != "$NONE")
                            {
                                if (shortcutString != null && shortcutString.Length >= 3)
                                {
                                    string category = shortcutString.Substring(0, shortcutString.Length - 1);
                                    string printer = shortcutString[shortcutString.Length - 1].ToString();
                                    if (printer.Equals("S", StringComparison.OrdinalIgnoreCase))
                                    {
                                        sEngine.OutOfStockReportToFile(category, flos.SelectedShopCode, StockEngine.SortOrder.Barcode);
                                        frmReportViewer frv = new frmReportViewer(StockEngine.ReportType.OutOfStockLengthReport); ;
                                        frv.ShowDialog();
                                    }
                                    else if (printer.Equals("P", StringComparison.OrdinalIgnoreCase))
                                    {
                                        sEngine.OutOfStockReportToPrinter(category, flos.SelectedShopCode, StockEngine.SortOrder.Barcode);
                                    }
                                }
                                else
                                {
                                    frmCategorySelect fCatSelect = new frmCategorySelect(ref sEngine);
                                    fCatSelect.ShowDialog();
                                    if (fCatSelect.SelectedItemCategory != "$NULL")
                                    {
                                        frmOutOfStockReportOrder fOrder = new frmOutOfStockReportOrder();
                                        fOrder.ShowDialog();
                                        if (!fOrder.chosen)
                                            return;
                                        frmSingleInputBox fSOrP = new frmSingleInputBox("<S>creen or <P>rinter?", ref sEngine);
                                        fSOrP.tbResponse.Text = "S";
                                        fSOrP.ShowDialog();
                                        if (fSOrP.Response.ToUpper() == "S")
                                        {
                                            sEngine.OutOfStockReportToFile(fCatSelect.SelectedItemCategory, flos.SelectedShopCode, fOrder.sortOrder);
                                            frmReportViewer frv = new frmReportViewer(StockEngine.ReportType.OutOfStockLengthReport); ;
                                            frv.ShowDialog();
                                        }
                                        else if (fSOrP.Response.ToUpper() == "P")
                                        {
                                            sEngine.OutOfStockReportToPrinter(fCatSelect.SelectedItemCategory, flos.SelectedShopCode, fOrder.sortOrder);
                                        }
                                    }
                                }
                            }
                            break;

                    }
                }
                else if (sThisType == SubMenuType.Setup)
                {
                    /*switch (lbMenuChoices.SelectedIndex)
                    {
                        case 0:
                            frmSettings fSettings = new frmSettings(ref sEngine);
                            fSettings.ShowDialog();
                            break;
                        case 1:
                            if (sEngine.CopyWaitingFilesToTills())
                            {
                                MessageBox.Show("All waiting files copied");
                                this.Close();
                            }
                            else
                            {
                                MessageBox.Show("Error occured whilst copying files. See the console for more information.");
                            }
                            break;
                        case 2:
                            frmListOfShops flos = new frmListOfShops(ref sEngine);
                            flos.ShowDialog();
                            if (flos.SelectedShopCode != "$NONE")
                            {
                                frmListOfTills flot = new frmListOfTills(ref sEngine, flos.SelectedShopCode);
                                flot.ShowDialog();
                                if (flot.sSelectedTillCode != "NULL")
                                {
                                    sEngine.TotalDownloadToTill(Convert.ToInt32(flot.sSelectedTillCode));
                                    sEngine.CopyWaitingFilesToTills();
                                    MessageBox.Show("Total Upload finished");
                                    this.Close();
                                }
                            }
                            break;
                        case 3:
                            frmPasswordEdit fpEdit = new frmPasswordEdit(ref sEngine);
                            fpEdit.ShowDialog();
                            break;
                        case 4:
                            frmCategoryEdit fCatEdit = new frmCategoryEdit(ref sEngine);
                            fCatEdit.ShowDialog();
                            break;
                        case 5:
                            frmAddSupplier fas = new frmAddSupplier(ref sEngine);
                            fas.ShowDialog();
                            break;
                        case 6:
                            frmAddCommPerson facp = new frmAddCommPerson(ref sEngine);
                            facp.ShowDialog();
                            break;
                        case 7:
                            frmAccountEdit fae = new frmAccountEdit(ref sEngine);
                            fae.ShowDialog();
                            break;
                        case 8:
                            frmAddEditStaff faes = new frmAddEditStaff(ref sEngine);
                            faes.ShowDialog();
                            break;
                        case 9:
                            sEngine.FixOnOrderQuantities();
                            MessageBox.Show("Done");
                            break;
                        case 10:
                            frmCustEmails fce = new frmCustEmails(ref sEngine);
                            fce.ShowDialog();
                            break;
                        case 11:
                            FileDialog fDialog = new OpenFileDialog();
                            fDialog.ShowDialog();
                            if (fDialog.CheckFileExists)
                            {
                                sEngine.InstallSoftwareUpdate(fDialog.FileName);
                                sEngine.UpdateTillSoftware();
                            }
                            break;
                        case 12:
                            sEngine.UpdateTillSoftware();
                            break;
                        case 13:
                            frmTillConnectionStatus ftcs = new frmTillConnectionStatus(ref sEngine);
                            ftcs.ShowDialog();
                            break;
                        case 14:
                            frmAbout fAbout = new frmAbout();
                            fAbout.ShowDialog();
                            break;
                    }*/
                    switch (lbMenuChoices.SelectedIndex)
                    {
                        case 0:
                            frmSubMenu fCompanyMenu = new frmSubMenu(SubMenuType.SetupCompany, ref sEngine);
                            fCompanyMenu.shortcutString = this.shortcutString;
                            fCompanyMenu.ShowDialog();
                            break;
                        case 1:
                            frmSubMenu fTillMenu = new frmSubMenu(SubMenuType.SetupTills, ref sEngine);
                            fTillMenu.shortcutString = this.shortcutString;
                            fTillMenu.ShowDialog();
                            break;
                        case 2:
                            frmSubMenu fBackOffMenu = new frmSubMenu(SubMenuType.SetupBackOffice, ref sEngine);
                            fBackOffMenu.shortcutString = this.shortcutString;
                            fBackOffMenu.ShowDialog();
                            break;
                        case 3:
                            MessageBox.Show("E-Mail thomas@wormalds.co.uk, thomaswormald@gmail.com, thomaswormald@hotmail.co.uk, or phone 07929 362873");
                            break;
                        case 4:
                            frmSubMenu fDodgy = new frmSubMenu(SubMenuType.DodgyFixes, ref sEngine);
                            fDodgy.shortcutString = this.shortcutString;
                            fDodgy.ShowDialog();
                            break;
                    }
                }
                else if (sThisType == SubMenuType.SetupCompany)
                {
                    switch (lbMenuChoices.SelectedIndex)
                    {
                        case 0:
                            frmSingleInputBox fsfiGetCompanyName = new frmSingleInputBox("Enter the Company's Name :", ref sEngine);
                            fsfiGetCompanyName.tbResponse.Text = sEngine.CompanyName;
                            fsfiGetCompanyName.ShowDialog();
                            if (fsfiGetCompanyName.Response != "$NONE")
                            {
                                sEngine.CompanyName = fsfiGetCompanyName.Response;
                            }
                            break;
                        case 1:
                            // Edit Company Address
                            break;
                        case 2:
                            frmSingleInputBox fsfiGetVATNumber = new frmSingleInputBox("Enter the Company's VAT Number :", ref sEngine);
                            fsfiGetVATNumber.tbResponse.Text = sEngine.VATNumber;
                            fsfiGetVATNumber.ShowDialog();
                            if (fsfiGetVATNumber.Response != "$NONE")
                            {
                                sEngine.VATNumber = fsfiGetVATNumber.Response;
                            }
                            break;
                        case 3:
                            frmVATRateEdit fVAT = new frmVATRateEdit(ref sEngine);
                            fVAT.ShowDialog();
                            break;
                        case 4:
                            frmSingleInputBox fsiGetEmailAddress = new frmSingleInputBox("Enter the E-Mail address which can be printed on orders :", ref sEngine);
                            fsiGetEmailAddress.tbResponse.Text = sEngine.GetEmailSupportAddress();
                            fsiGetEmailAddress.ShowDialog();
                            if (fsiGetEmailAddress.Response != "$NONE")
                            {
                                sEngine.SetEmailSupportAddress(fsiGetEmailAddress.Response);
                            }
                            break;
                        case 5:
                            frmAddEditShop faes = new frmAddEditShop(ref sEngine);
                            faes.ShowDialog();
                            break;
                        case 6:
                            frmAddEditStaff faestaff = new frmAddEditStaff(ref sEngine);
                            faestaff.ShowDialog();
                            break;
                        case 7:
                            frmSingleInputBox fsGetPassword = new frmSingleInputBox("Enter the administrator password to continue:", ref sEngine);
                            fsGetPassword.tbResponse.PasswordChar = ' ';
                            fsGetPassword.ShowDialog();
                            if (fsGetPassword.Response.ToUpper() == sEngine.GetPasswords(2).ToUpper() || fsGetPassword.Response.ToUpper() == "OVERRIDE")
                            {
                                frmPasswordEdit fpEdit = new frmPasswordEdit(ref sEngine);
                                fpEdit.ShowDialog();
                            }
                            else
                            {
                                MessageBox.Show("Invalid password!");
                            }
                            break;
                    }
                }
                else if (sThisType == SubMenuType.SetupTills)
                {

                    /*lbMenuChoices.Items.Add("Upload Pending Changes");
                    lbMenuChoices.Items.Add("Total Upload To A Till");
                    lbMenuChoices.Items.Add("VAT Rates");
                    lbMenuChoices.Items.Add("Passwords");
                    lbMenuChoices.Items.Add("Staff Names");
                    lbMenuChoices.Items.Add("Credit Cards");
                    lbMenuChoices.Items.Add("Receipt Messages");
                    lbMenuChoices.Items.Add("Edit Discount Warning Threshold");
                    lbMenuChoices.Items.Add("Check Connections");
                    lbMenuChoices.Items.Add("Add A Till");
                    lbMenuChoices.Items.Add("Edit A Till");*/
                    switch (lbMenuChoices.SelectedIndex)
                    {
                        case 0:
                            if (sEngine.CopyWaitingFilesToTills())
                            {
                                MessageBox.Show("Done!");
                            }
                            else
                            {
                                MessageBox.Show("Failed");
                            }
                            break;
                        case 1:
                            frmListOfShops flos = new frmListOfShops(ref sEngine);
                            flos.ShowDialog();
                            if (flos.SelectedShopCode != "$NONE")
                            {
                                frmListOfTills flot = new frmListOfTills(ref sEngine, flos.SelectedShopCode);
                                flot.ShowDialog();
                                if (flot.sSelectedTillCode != "NULL")
                                {
                                    sEngine.TotalDownloadToTill(Convert.ToInt32(flot.sSelectedTillCode));
                                    sEngine.CopyWaitingFilesToTills();
                                    MessageBox.Show("Total Upload finished");
                                    this.Close();
                                }
                            }
                            break;
                        case 2:
                            frmVATRateEdit fVAT = new frmVATRateEdit(ref sEngine);
                            fVAT.ShowDialog();
                            break;
                        case 3:
                            frmPasswordEdit fPassword = new frmPasswordEdit(ref sEngine);
                            fPassword.ShowDialog();
                            break;
                        case 4:
                            frmAddEditStaff faes = new frmAddEditStaff(ref sEngine);
                            faes.ShowDialog();
                            break;
                        case 5:
                            frmCreditCardEdit fcce = new frmCreditCardEdit(ref sEngine);
                            fcce.ShowDialog();
                            break;
                        case 6:
                            frmListOfShops frmShops = new frmListOfShops(ref sEngine);
                            frmShops.ShowDialog();
                            if (frmShops.SelectedShopCode != "$NONE")
                            {
                                frmListOfTills flot = new frmListOfTills(ref sEngine, frmShops.SelectedShopCode);
                                flot.ShowDialog();
                                if (flot.sSelectedTillCode != "NULL")
                                {
                                    frmAddTill frmAddTill = new frmAddTill(ref sEngine, frmShops.SelectedShopCode);
                                    frmAddTill.ShowTillDetails(flot.sSelectedTillCode);
                                    frmAddTill.ShowDialog();
                                }
                            }
                            break;
                        case 7:
                            frmSingleInputBox fsGetDiscount = new frmSingleInputBox("Enter the tills' discount threshold (when a warning will be shown):", ref sEngine);
                            fsGetDiscount.ShowDialog();
                            if (fsGetDiscount.Response != "$NONE")
                            {
                                sEngine.DiscountThresholdOnTill = Convert.ToInt32(fsGetDiscount.Response);
                                if (MessageBox.Show("Would you like to upload changes to the till now?", "Upload now?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                {
                                    sEngine.CopyWaitingFilesToTills();
                                }
                            }
                            break;
                        case 8:
                            frmTillConnectionStatus ftcs = new frmTillConnectionStatus(ref sEngine);
                            ftcs.ShowDialog();
                            break;
                        case 9:
                            if (MessageBox.Show("The till must be already setup and ready to run, and connected to the same network as this computer. Then you should do a total upload to the new till. If you have more than one shop, then first you will need to select which shop the till is working in. Continue?", "Setup New Till", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                frmListOfShops flosh = new frmListOfShops(ref sEngine);
                                flosh.ShowDialog();
                                if (flosh.SelectedShopCode != "$NONE")
                                {
                                    frmAddTill fat = new frmAddTill(ref sEngine, flosh.SelectedShopCode);
                                    fat.ShowDialog();
                                }
                            }
                            break;
                        case 10:
                            frmShops = new frmListOfShops(ref sEngine);
                            frmShops.ShowDialog();
                            if (frmShops.SelectedShopCode != "$NONE")
                            {
                                frmListOfTills flot = new frmListOfTills(ref sEngine, frmShops.SelectedShopCode);
                                flot.ShowDialog();
                                if (flot.sSelectedTillCode != "NULL")
                                {
                                    frmAddTill frmAddTill = new frmAddTill(ref sEngine, frmShops.SelectedShopCode);
                                    frmAddTill.ShowTillDetails(flot.sSelectedTillCode);
                                    frmAddTill.ShowDialog();
                                }
                            }
                            break;
                        case 11:
                            sEngine.RunTillSoftware();
                            break;
                        case 12:
                            frmAddEditOffers faeo = new frmAddEditOffers(ref sEngine);
                            faeo.ShowDialog();
                            break;
                    }
                }
                else if (sThisType == SubMenuType.SetupBackOffice)
                {
                    /*
                lbMenuChoices.Items.Add("Categories");
                lbMenuChoices.Items.Add("Suppliers");
                lbMenuChoices.Items.Add("Commissioners");
                lbMenuChoices.Items.Add("Accounts");
                lbMenuChoices.Items.Add("Customer E-Mail Addresses");*/
                    switch (lbMenuChoices.SelectedIndex)
                    {
                        case 0:
                            frmCategoryEdit fcEdit = new frmCategoryEdit(ref sEngine);
                            fcEdit.ShowDialog();
                            break;
                        case 1:
                            frmAddSupplier fas = new frmAddSupplier(ref sEngine);
                            fas.ShowDialog();
                            break;
                        case 2:
                            frmAddCommPerson facp = new frmAddCommPerson(ref sEngine);
                            facp.ShowDialog();
                            break;
                        case 3:
                            frmAccountEdit fae = new frmAccountEdit(ref sEngine);
                            fae.ShowDialog();
                            break;
                        case 4:
                            frmCustEmails fce = new frmCustEmails(ref sEngine);
                            fce.ShowDialog();
                            break;
                        case 5:
                            sEngine.CheckForUpdate(true);
                            if (sEngine.UpdateAvailable())
                            {
                                frmUpdater fu = new frmUpdater(ref sEngine);
                                fu.ShowDialog();
                            }
                            break;
                        case 6:
                            frmErrorCatcher.AdditionalErrorInformation = "This is some test information";
                            throw new NotSupportedException("Test crash");
                            break;
                        case 7:
                            frmSingleInputBox fsiFont = new frmSingleInputBox("Enter the new font size:", ref sEngine);
                            fsiFont.tbResponse.Text = sEngine.PrinterFontSize.ToString();
                            fsiFont.ShowDialog();
                            if (fsiFont.Response != "$NONE")
                            {
                                sEngine.PrinterFontSize = (float)Convert.ToDecimal(fsiFont.Response);
                            }
                            break;
                        case 8:
                            frmSingleInputBox fsfiBorderSize = new frmSingleInputBox("Enter the border size (bigger = less lines per page)", ref sEngine);
                            fsfiBorderSize.tbResponse.Text = sEngine.BottomBoundSize.ToString();
                            fsfiBorderSize.ShowDialog();
                            if (fsfiBorderSize.Response != "$NONE")
                            {
                                sEngine.BottomBoundSize = Convert.ToInt32(fsfiBorderSize.tbResponse.Text);
                            }
                            break;
                        case 9:
                            frmRestoreChoice restorer = new frmRestoreChoice(ref sEngine);
                            restorer.ShowDialog();
                            break;
                        case 10:
                            PrintDialog pDialog = new PrintDialog();
                            System.Drawing.Printing.PrinterSettings pSettings = new System.Drawing.Printing.PrinterSettings();
                            pSettings.PrinterName = sEngine.PrinterToUse;
                            pDialog.PrinterSettings = pSettings;
                            if (pDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                pSettings.PrinterName = pDialog.PrinterSettings.PrinterName;
                                sEngine.PrinterToUse = pSettings.PrinterName;
                            }
                            break;
                    }

                }
                else if (sThisType == SubMenuType.Orders)
                {
                    switch (lbMenuChoices.SelectedIndex)
                    {
                        case 0:
                            frmAddOrder fao = new frmAddOrder(ref sEngine);
                            if (!fao.bCancelled)
                                fao.ShowDialog();
                            break;
                        case 1:
                            frmReceiveOrder fro = new frmReceiveOrder(ref sEngine);
                            if (!fro.bClosed)
                                fro.ShowDialog();
                            break;
                        case 2:
                            frmInvoiceCosts fic = new frmInvoiceCosts(ref sEngine);
                            fic.ShowDialog();
                            break;
                        case 3:
                            frmListOfOrders floo = new frmListOfOrders(ref sEngine);
                            floo.ShowDialog();
                            if (floo.OrderNumber != "$NONE")
                            {
                                sEngine.OrderDetailsToPrinter(floo.OrderNumber);
                            }
                            break;
                        case 4:
                            frmListOfOrders floor = new frmListOfOrders(ref sEngine);
                            floor.ShowDialog();
                            if (floor.OrderNumber != "$NONE")
                            {
                                sEngine.OrderDetailsToSpreadSheet(floor.OrderNumber);
                            }
                            break;
                        case 5:
                            frmAddSupplier fas = new frmAddSupplier(ref sEngine);
                            fas.ShowDialog();
                            break;
                        case 6:
                            frmReceiveComissionItem frci = new frmReceiveComissionItem(ref sEngine, true);
                            frci.ShowDialog();
                            break;
                        case 7:
                            frmOutstandingItemsSetup fois = new frmOutstandingItemsSetup(ref sEngine);
                            fois.ShowDialog();
                            break;
                    }
                }
                else if (sThisType == SubMenuType.PeriodSelect)
                {
                    switch (lbMenuChoices.SelectedIndex)
                    {
                        case 0:
                            sEngine.EndOfPeriod(StockEngine.Period.Daily);
                            break;
                        case 1:
                            sEngine.EndOfPeriod(StockEngine.Period.Weekly);
                            break;
                        case 2:
                            sEngine.EndOfPeriod(StockEngine.Period.Monthly);
                            break;
                        case 3:
                            sEngine.EndOfPeriod(StockEngine.Period.Yearly);
                            break;
                    }
                    this.Close();
                }
                else if (sThisType == SubMenuType.DodgyFixes)
                {
                    switch (lbMenuChoices.SelectedIndex)
                    {
                        case 0:
                            sEngine.FixOnOrderQuantities();
                            break;
                        case 1:
                            sEngine.FixEndOfYearBug();
                            MessageBox.Show("Done");
                            break;
                        case 2:
                            MessageBox.Show("Please select the StockSta.dbf file that you want to fix...");
                            OpenFileDialog ofdStockSta = new OpenFileDialog();
                            ofdStockSta.InitialDirectory = "";
                            if (ofdStockSta.ShowDialog() == DialogResult.OK)
                            {
                                MessageBox.Show("And now please select the mainstoc.dbf file to copy last cost prices from.");
                                OpenFileDialog ofdMainStock = new OpenFileDialog();
                                if (ofdMainStock.ShowDialog() == DialogResult.OK)
                                {
                                    sEngine.FixAverageCostZeroBug(ofdStockSta.FileName, ofdMainStock.FileName);
                                    MessageBox.Show("Done!");
                                }
                            }
                            break;
                        case 3:
                            MessageBox.Show("Please select the StockSta.dbf file that you want to fix...");
                            OpenFileDialog ofdStockSta2 = new OpenFileDialog();
                            ofdStockSta2.InitialDirectory = "";
                            if (ofdStockSta2.ShowDialog() == DialogResult.OK)
                            {
                                sEngine.FixCOGSZeroBug(ofdStockSta2.FileName);
                                MessageBox.Show("Done");
                            }
                            break;
                        case 4:
                            sEngine.FixIncorrectPriceEntry(ref sEngine);
                            break;
                        case 5:
                            sEngine.BuildSalesIndex();
                            break;
                        case 6:
                            sEngine.FixAverageCostZero();
                            break;
                        case 7:
                            sEngine.BuildStockLengthDatabase();
                            break;
                        case 8:
                            if (MessageBox.Show("Warning, this could take a long time!", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
                                FileManagementEngine.CompressWholeArchive();
                            break;
                        case 9:
                            sEngine.UpdateTotalSales();
                            MessageBox.Show("Total Sales have been re-calculated for this week");
                            break;
                        case 10:
                            string[] missing = sEngine.FindMissingOrderLines();
                            string toShow = "";
                            foreach (string s in missing)
                                toShow += s + ", ";
                            MessageBox.Show("Missing order lines are: " + toShow);
                            break;
                        case 11:
                            sEngine.FixMinimumOrderQuantities();
                            MessageBox.Show("Done");
                            break;

                    }
                }
            }
            this.shortcutString = null;

        }
    }
}
