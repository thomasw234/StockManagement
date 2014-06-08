using System;
using System.Collections.Generic;
using System.Text;
using BackOffice.Database_Engine;
using System.IO;
using System.Drawing;
using System.Drawing.Printing;
using System.Net;
using System.Globalization;
using BackOffice.Objects.Items;
using System.Threading;
using System.Threading.Tasks;

namespace BackOffice
{
    class Till
    {
        public int Number;
        public string TillName;
        public string[] ReceiptFooter;
        public string FileLocation;
        public string CollectionMap;
        public string ShopCode;
        public string CollectedMap;
        public string LastCollection;

        public Till(string[] sTillData)
        {
            Number = Convert.ToInt32(Math.Round(Convert.ToDecimal(sTillData[0])));
            TillName = sTillData[1];
            ReceiptFooter = new string[3];
            ReceiptFooter[0] = sTillData[3];
            ReceiptFooter[1] = sTillData[4];
            ReceiptFooter[2] = sTillData[5];
            FileLocation = sTillData[2];
            CollectionMap = sTillData[8];
            ShopCode = sTillData[7];
            CollectedMap = sTillData[6];
            LastCollection = sTillData[9];
        }

        public void GetSalesDataFromTill(string sSalesDate)
        {
            // Sales date in format DDMMYY
            int nYear = Convert.ToInt32(sSalesDate[4].ToString() + sSalesDate[5].ToString());
            int nMonth = Convert.ToInt32(sSalesDate[2].ToString() + sSalesDate[3].ToString());
            int nDay = Convert.ToInt32(sSalesDate[0].ToString() + sSalesDate[1].ToString());
            DateTime d = new DateTime(nYear, nMonth, nDay);
            switch (d.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    nDay = 1;
                    break;
                case DayOfWeek.Monday:
                    nDay = 2;
                    break;
                case DayOfWeek.Tuesday:
                    nDay = 3;
                    break;
                case DayOfWeek.Wednesday:
                    nDay = 4;
                    break;
                case DayOfWeek.Thursday:
                    nDay = 5;
                    break;
                case DayOfWeek.Friday:
                    nDay = 6;
                    break;
                case DayOfWeek.Saturday:
                    nDay = 7;
                    break;
            }
            File.Copy(FileLocation + "\\OUTGNG\\REPDATA" + nDay.ToString() + ".DBF", "TILL" + Number.ToString() +  "\\INGNG\\" + "REPDATA" + nDay.ToString() + ".DBF", true);
            File.Copy(FileLocation + "\\OUTGNG\\TDATA" + nDay.ToString() + ".DBF", "TILL" + Number.ToString() + "\\INGNG\\" + "TDATA" + nDay.ToString() + ".DBF", true);
            File.Copy(FileLocation + "\\OUTGNG\\THDR" + nDay.ToString() + ".DBF", "TILL" + Number.ToString() + "\\INGNG\\" + "THDR" + nDay.ToString() + ".DBF", true);
            MarkTillAsCollected(nDay, sSalesDate);
        }

        private void MarkTillAsCollected(int nDayOfWeek, string sSalesDate)
        {
            string sCollectionMap = "";
            for (int i = 0; i < 7; i++)
            {
                if (i == nDayOfWeek - 2)
                    sCollectionMap += "Y";
                else
                    sCollectionMap += CollectedMap[i];
            }
            CollectedMap = sCollectionMap;
            LastCollection = sSalesDate;
        }

        public void SaveTillChanges(ref Table tTill)
        {
            string[] sTillData = new string[10];
            sTillData[0] = Number.ToString();
            sTillData[1] = TillName;
            sTillData[2] = FileLocation;
            sTillData[3] = ReceiptFooter[0];
            sTillData[4] = ReceiptFooter[1];
            sTillData[5] = ReceiptFooter[2];
            sTillData[6] = CollectedMap;
            sTillData[7] = ShopCode;
            sTillData[8] = CollectionMap;
            sTillData[9] = LastCollection;
            int nRecNum = -1;
            tTill.SearchForRecord(TillName, 1, ref nRecNum);
            tTill.DeleteRecord(nRecNum);
            tTill.AddRecord(sTillData);
            tTill.SaveToFile("TILL.DBF");
        }
    }

    public class Item
    {
        /// <summary>
        /// The description of the item
        /// </summary>
        string sItemDesc;
        /// <summary>
        /// The item's barcode
        /// </summary>
        string sBarcode;
        /// <summary>
        /// The gross price of the item
        /// </summary>
        decimal fGrossAmnt;
        /// <summary>
        /// The final amount of the item
        /// </summary>
        decimal fFinalAmount;
        /// <summary>
        /// The category of V.A.T. that the item is in
        /// </summary>
        string sVATCategory;
        /// <summary>
        /// Whether or not this is a stock item
        /// </summary>
        bool bStockItem;
        /// <summary>
        /// The product category that the item's in (stock, department etc)
        /// </summary>
        int nCategory;
        /// <summary>
        /// The quantity of item
        /// </summary>
        decimal nQuantity;
        /// <summary>
        /// The category that the item is in (user defined categories)
        /// </summary>
        string sCategory;
        /// <summary>
        /// Whether or not the item is discontinued, NOT DISCOUNTED!!
        /// </summary>
        public bool bDiscontinued;
        /// <summary>
        /// Whether or not the record was found with the item's details
        /// </summary>
        bool bItemExists;
        /// <summary>
        /// The V.A.T. code of the item (I1, Z0 etc)
        /// </summary>
        string sVATCode;
        /// <summary>
        /// The current stock level of the item
        /// </summary>
        decimal nCurrentStockLevel;
        /// <summary>
        /// Whether or not the item's price has been discounted
        /// </summary>
        bool bDiscounted;
        /// <summary>
        /// If there is a parent item, then the barcode of it
        /// </summary>
        public string ParentBarcode;
        /// <summary>
        /// The code of the shop that the item's in
        /// </summary>
        public string ShopCode;

        /// <summary>
        /// Intilailises the item
        /// </summary>
        /// <param name="sRecordContents">The contents of the STOCK database record for this item</param>
        public Item(string[] sRecordContents, string[] SSTCKRecord)
        {
            if (sRecordContents[0] == null)
            {
                bItemExists = false;
            }
            else
            {
                sBarcode = sRecordContents[0];
                sItemDesc = sRecordContents[1];
                nCategory = Convert.ToInt32(sRecordContents[5]);
                fGrossAmnt = (decimal)Convert.ToDecimal(sRecordContents[2]);
                fGrossAmnt = (decimal)Math.Round((decimal)fGrossAmnt, 2);
                fFinalAmount = fGrossAmnt;
                sVATCategory = sRecordContents[3];
                sCategory = sRecordContents[4];
                if (nCategory == 1 || nCategory == 5)
                    bStockItem = true;
                else
                    bStockItem = false;
                if (SSTCKRecord[36] == "")
                    nCurrentStockLevel = 0;
                else
                    nCurrentStockLevel = Convert.ToDecimal(SSTCKRecord[36]);
                if (sRecordContents[5].StartsWith("ZZ"))
                    bDiscontinued = true;
                else
                    bDiscontinued = false;

                bItemExists = true;
                bDiscounted = false;
                ParentBarcode = sRecordContents[7];
                ShopCode = SSTCKRecord[35];
            }


        }

        /// <summary>
        /// The quantity of the item
        /// </summary>
        public decimal Quantity
        {
            get
            {
                return nQuantity;
            }
            set
            {
                nQuantity = value;
            }
        }

        /// <summary>
        /// The amount of the item
        /// </summary>
        public decimal Amount
        {
            get
            {
                return fFinalAmount;
            }
        }

        /// <summary>
        /// The gross amount (before discounts etc)
        /// </summary>
        public decimal GrossAmount
        {
            get
            {
                return fGrossAmnt;
            }
            set
            {
                fGrossAmnt = value;
                fFinalAmount = fGrossAmnt;
            }
        }

        /// <summary>
        /// The barcode of the item
        /// </summary>
        public string Barcode
        {
            get
            {
                return sBarcode;
            }
        }

        /// <summary>
        /// Whether or not the item is stock
        /// </summary>
        public bool IsItemStock
        {
            get
            {
                return bStockItem;
            }
        }

        /// <summary>
        /// The V.A.T. category
        /// </summary>
        public string VATRate
        {
            get
            {
                return sVATCategory;
            }
        }

        /// <summary>
        /// The description of the item
        /// </summary>
        public string Description
        {
            get
            {
                return sItemDesc;
            }
            set
            {
                if (nCategory == 4)
                    sItemDesc = value;
            }
        }

        /*
        public bool GetIsDiscontinued()
        {
            return bDiscontinued;
        }*/

        /// <summary>
        /// The current stock level of the item
        /// </summary>
        public decimal StockLevel
        {
            get
            {
                if (nCurrentStockLevel == null)
                    return 0;
                else
                    return nCurrentStockLevel;
            }
            set
            {
                nCurrentStockLevel = value;
            }
        }

        /// <summary>
        /// Set the price of the item
        /// </summary>
        /// <param name="fGrossAmount">The new price</param>
        public void SetPrice(decimal fGrossAmount)
        {
            if (fGrossAmnt == 0)
            {
                if (nCategory == 2 || nCategory == 4)
                {
                    fGrossAmnt = fGrossAmount;
                    fFinalAmount = fGrossAmnt;
                }
            }
            else
            {
                fFinalAmount = fGrossAmount;
            }
        }

        /// <summary>
        /// Gets the category of the item
        /// </summary>
        public int ItemCategory
        {
            get
            {
                return nCategory;
            }
        }

        /// <summary>
        /// Discounts an amount from the price
        /// </summary>
        /// <param name="fAmount">The amount to discount</param>
        public void DiscountAmountFromNet(decimal fAmount)
        {
            fFinalAmount -= fAmount;
            bDiscounted = true;
        }

        /// <summary>
        /// Whether or not the item has been discounted
        /// </summary>
        public bool Discounted
        {
            get
            {
                return bDiscounted;
            }
        }

        public string CodeCategory
        {
            get
            {
                return sCategory;
            }
        }
    }

    public class StockEngine : IDisposable
    {
        Table tAccStat;
        Table tEmails;
        Table tCatGroupHeader;
        Table tCatGroupData;
        Table tCategory;
        Table tCommissioners;
        Table tCommItems;
        Table tCustomer;
        Table tMultiHeader;
        Table tMultiData;
        Table tOffers;
        Table tOrder;
        Table tOrderLine;
        Table tOrderSuggestions;
        Table tSalesData;
        Table tSalesIndex;
        Table tSerials;
        Table tSettings;
        Table tShop;
        Table tStaff;
        Table tStock;
        Table tStockLength;
        Table tStockStats;
        Table tSupplier;
        Table tSupplierIndex;
        Table tTills;
        Table tTotalSales;
        Table tVATRates;

        Till[] Till;

        string sTDir = "";

        public StockEngine()
        {
            LoadAllTables();
            CheckForUpdate(false);
            SetupTills();
            CheckForVersionChange();
            Task.Run(() => { tStockStats.ProperLoad(); });
            Task.Run(() => { tStock.ProperLoad(); });
        }
        public StockEngine(string sTableDir)
        {
            sTDir = sTableDir + "\\";

            // Hopefully this will work
            FileManagementEngine.UncompressDirectory(sTDir);

            LoadAllTables();
            SetupTills();

        }

        public void CheckForUpdate(bool bForced)
        {
            Random r = new Random(); // Used to prevent caching!
            frmProgressBar fp = new frmProgressBar("Checking For Update");
            fp.pb.Maximum = 5;
            try
            {
                if (!UpdateCheckNeeded() && !bForced)
                    return;
                fp.Show();
                if (!Directory.Exists("Update"))
                    Directory.CreateDirectory("Update");
                WebClient w = new WebClient();

                if (File.Exists("Update\\onlineBuildNum.txt"))
                    File.Delete("Update\\onlineBuildNum.txt");
               
                w.DownloadFile("http://www.thomaswormald.co.uk/backoffice/backoffice/updates/buildNum.txt?r=" + r.Next(0, 99999).ToString(), "Update\\onlineBuildNum.txt");
                fp.pb.Value = 1;
                TextReader t = new StreamReader("Update\\onlineBuildNum.txt");
                string sDownloaded = t.ReadLine();
                string[] sSplit = sDownloaded.Split(' ');
                fp.pb.Value = 2;
                TextReader tr = new StreamReader("buildNum.txt");
                string sRunning = tr.ReadLine();
                string[] sRunningSplit = sRunning.Split(' ');
                DateTime dtRunning = DateTime.Parse(sRunningSplit[1] + " " + sRunningSplit[2]);

                DateTime dt = DateTime.Parse(sSplit[1] + " " + sSplit[2]);
                fp.pb.Value = 3;
                if (DateTime.Compare(dtRunning, dt) < 0) // Update available
                {
                    if (File.Exists("Update\\BackOfficeUpdate.exe"))
                        File.Delete("Update\\BackOfficeUpdate.exe");
                    fp.pb.Value = 4;
                    w.DownloadFile("http://www.thomaswormald.co.uk/backoffice/backoffice/updates/BackOffice.exe?r=" + r.Next(0, 99999).ToString(), "Update\\BackOfficeUpdate.exe");
                    w.DownloadFile("http://www.thomaswormald.co.uk/backoffice/backoffice/updates/changeLog.txt?r=" + r.Next(0, 999999), "Update\\changeLog.txt");
                    w.DownloadFile("http://www.thomaswormald.co.uk/backoffice/backoffice/updates/GTill.exe?r=" + r.Next(0, 999999), "Update\\GTill.exe");
                }
                fp.pb.Value = 5;
                t.Close();
                w.Dispose();
                fp.Close();
            }
            catch
            {
                ;
            }
            finally
            {
                fp.Close();
            }
        }
        public bool UpdateAvailable()
        {
            if (File.Exists("Update\\BackOfficeUpdate.exe"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void InstallUpdate()
        {
            UpdateTillSoftware();
            System.Diagnostics.Process.Start("BackOfficeUpdater.exe");
            System.Windows.Forms.Application.ExitThread();
        
        }

        private bool UpdateCheckNeeded()
        {
            string lastChecked = this.LastCheckedForUpdates;
            string todayDate = DateTime.Now.Date.Day.ToString() + "/" + DateTime.Now.Date.Month.ToString() + "/" + DateTime.Now.Year.ToString();
            if (lastChecked != todayDate)
            {
                this.LastCheckedForUpdates = todayDate;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void LoadAllTables()
        {
            if (!Directory.Exists("OffersReceipt"))
            {
                Directory.CreateDirectory("OffersReceipt");
            }
            frmProgressBar fp = new frmProgressBar("Loading Tables");
            fp.pb.Maximum = 26;
            fp.pb.Value = 0;
            //fp.Show();
            fp.Text = "Loading Accounts";
            tAccStat = new Table(sTDir + "ACCSTAT.DBF");
            fp.pb.Value = 1;
            fp.Text = "Loading E-Mails";
            tEmails = new Table(sTDir + "EMAILS.DBF");
            if (tEmails.ReturnFieldNames().Length < 7)
            {
                FileStream fs = new FileStream(sTDir + "EMAILS.DBF", FileMode.OpenOrCreate);
                fs.Write(Properties.Resources.EMAILS, 0, Properties.Resources.EMAILS.Length);
                fs.Close();

                tEmails = new Table(sTDir + "EMAILS.DBF");
            }
            fp.pb.Value = 2;
            fp.Text = "Loading Categories";
            tCatGroupData = new Table(sTDir + "CATGRPDA.DBF");
            fp.pb.Value = 3;
            tCatGroupHeader = new Table(sTDir + "CATGPHDR.DBF");
            fp.pb.Value = 4;
            tCategory = new Table(sTDir + "CATEGORY.DBF");
            fp.pb.Value = 5;
            tCategory.SortTable();
            fp.Text = "Loading Commission Data";
            if (!File.Exists(sTDir + "COMMPPL.DBF"))
            {
                FileStream fs = new FileStream(sTDir + "COMMPPL.DBF", FileMode.OpenOrCreate);
                fs.Write(Properties.Resources.COMMPPL, 0, Properties.Resources.COMMPPL.Length);
                fs.Close();
            }
            tCommissioners = new Table(sTDir + "COMMPPL.DBF");
            if (tCommissioners.ReturnFieldNames().Length != 2)
            {
                FileStream fs = new FileStream(sTDir + "COMMPPL.DBF", FileMode.OpenOrCreate);
                fs.Write(Properties.Resources.COMMPPL, 0, Properties.Resources.COMMPPL.Length);
                fs.Close();
                tCommissioners = new Table(sTDir + "COMMPPL.DBF");
            }
            fp.pb.Value = 6;
            if (!File.Exists(sTDir + "COMMITEM.DBF"))
            {
                FileStream fs = new FileStream(sTDir + "COMMITEM.DBF", FileMode.OpenOrCreate);
                fs.Write(Properties.Resources.COMMITEM, 0, Properties.Resources.COMMITEM.Length);
                fs.Close();
                tCommissioners = new Table(sTDir + "COMMITEM.DBF");
            }
            tCommItems = new Table(sTDir + "COMMITEM.DBF");
            if (tCommItems.ReturnFieldNames().Length != 11)
            {
                FileStream fs = new FileStream(sTDir + "COMMITEM.DBF", FileMode.OpenOrCreate);
                fs.Write(Properties.Resources.COMMITEM, 0, Properties.Resources.COMMITEM.Length);
                fs.Close();
                tCommissioners = new Table(sTDir + "COMMITEM.DBF");
            }
            fp.pb.Value = 8;
            fp.Text = "Loading Type 6 Items";
            tMultiData = new Table(sTDir + "MULTIDAT.DBF");
            fp.pb.Value = 9;
            tMultiHeader = new Table(sTDir + "MULTIHDR.DBF");
            fp.pb.Value = 10;
            fp.Text = "Loading Orders";
            tOrder = new Table(sTDir + "ORDER.DBF");
            fp.pb.Value = 11;
            tOrderLine = new Table(sTDir + "ORDERLIN.DBF");
            fp.pb.Value = 12;
            tOrderLine.SortTable();
            tOrderSuggestions = new Table("ORDERSUG.DBF");
            fp.pb.Value = 13;
            fp.Text = "Loading Settings";
            tSettings = new Table(sTDir + "SETTINGS.DBF");
            fp.pb.Value = 14;
            fp.Text = "Loading Staff";
            tStaff = new Table(sTDir + "STAFF.DBF");
            fp.pb.Value = 15;
            fp.Text = "Loading Stock";
            tStock = new Table(sTDir + "MAINSTOC.DBF");
            fp.pb.Value = 16;
            tStockStats = new Table(sTDir + "STOCKSTA.DBF");
            fp.pb.Value = 17;
            fp.Text = "Loading Suppliers";
            tSupplier = new Table(sTDir + "SUPPLIER.DBF");
            fp.pb.Value = 18;
            fp.Text = "Loading Till Data";
            tTills = new Table(sTDir + "TILL.DBF");
            fp.pb.Value = 19;
            fp.Text = "Loading Weekly Sales Summary";
            tTotalSales = new Table(sTDir + "TOTSALES.DBF");
            fp.pb.Value = 20;
            fp.Text = "Loading VAT Rates";
            tVATRates = new Table(sTDir + "VAT.DBF");
            fp.pb.Value = 21;
            fp.Text = "Loading Shops";
            tShop = new Table(sTDir + "SHOP.DBF");
            fp.pb.Value = 22;
            fp.Text = "Loading Supplier Index";
            tSupplierIndex = new Table(sTDir + "SUPINDEX.DBF");
            fp.pb.Value = 23;
            fp.Text = "Loading Serials";
            if (!File.Exists(sTDir + "SERIALS.DBF"))
            {
                FileStream s = new FileStream(sTDir + "SERIALS.DBF", FileMode.CreateNew);
                s.Write(Properties.Resources.SERIALS, 0, Properties.Resources.SERIALS.Length);
                s.Close();
            }
            tSerials = new Table(sTDir + "SERIALS.DBF");

            if (!File.Exists(sTDir + "OFFERS.DBF"))
            {
                FileStream s = new FileStream(sTDir + "OFFERS.DBF", FileMode.CreateNew);
                s.Write(Properties.Resources.OFFERS, 0, Properties.Resources.OFFERS.Length);
                s.Close();
            }
            fp.Text = "Loading Offers";
            tOffers = new Table(sTDir + "OFFERS.DBF");
            fp.pb.Value = 24;

            if (!File.Exists(sTDir + "SALESIND.DBF") && sTDir == "")
            {
                FileStream s = new FileStream(sTDir + "SALESIND.DBF", FileMode.CreateNew);
                s.Write(Properties.Resources.SALESIND, 0, Properties.Resources.SALESIND.Length);
                s.Close();
                BuildSalesIndex();
            }

            fp.Text = "Loading Sales Index";
            tSalesIndex = new Table(sTDir + "SALESIND.DBF");
            fp.pb.Value = 25;

            if (!File.Exists(sTDir + "STOCKLEN.DBF"))
            {
                FileStream s = new FileStream(sTDir + "STOCKLEN.DBF", FileMode.CreateNew);
                s.Write(Properties.Resources.STOCKLEN, 0, Properties.Resources.STOCKLEN.Length);
                s.Close();
            }
            
            fp.Text = "Loading Out Of Stock Data";
            tStockLength = new Table(sTDir + "STOCKLEN.DBF");
            fp.pb.Value = 26;

            if (tStockLength.ReturnFieldNames()[2].TrimEnd('\0') != "TOTALDAYS")
            {
                FileStream s = new FileStream(sTDir + "STOCKLEN.DBF", FileMode.Create);
                s.Write(Properties.Resources.STOCKLEN, 0, Properties.Resources.STOCKLEN.Length);
                s.Close();
                tStockLength = new Table(sTDir + "STOCKLEN.DBF");
            }
            //fp.Close();

            // Threading Test
            /*
            Console.WriteLine("Creating Thread...");
            Thread stockStaThread = new Thread(new ThreadStart(tStockStats.LoadTable));
            Console.WriteLine("Thread Created. Starting...");
            stockStaThread.Start();
            Console.WriteLine("Waiting for start...");
            while (!stockStaThread.IsAlive) ;
            Console.WriteLine("Started");*/
        }


        /// <summary>
        /// Builds up the STOCKLEN.DBF database from new
        /// </summary>
        public void BuildStockLengthDatabase()
        {
            frmProgressBar fp = new frmProgressBar();
            fp.pb.Maximum = 366;
            fp.pb.Value = 0;

            fp.Show();

            // Get a list of items to search for
            int nOfRecords = -1;
            string[] sBarcodes = tStock.SearchAndGetAllMatchingRecords(5, "1", ref nOfRecords, true, 0);

            // Start at a year ago
            DateTime dtCurrent = DateTime.Now.AddYears(-1);
            for (; !dtCurrent.Date.Equals(DateTime.Now); dtCurrent.AddDays(1))
            {
                // Check that the stocksta file exists in the archive for the current date
                string folderFormat = "Archive\\Daily\\" + dtCurrent.Year.ToString() + ".";
                if (dtCurrent.Month < 10)
                    folderFormat += "0";
                folderFormat += dtCurrent.Month.ToString() + ".";
                if (dtCurrent.Day < 10)
                    folderFormat += "0";
                folderFormat += dtCurrent.Day;

                FileManagementEngine.UncompressDirectory(folderFormat);

                if (!File.Exists(folderFormat + "\\STOCKSTA.DBF"))
                {
                    continue;
                }

                // Does exist, so search for the items in the database

                // Uncompress First
                // Hopefully this will work
                FileManagementEngine.UncompressDirectory(folderFormat);

                Table tArchivedStockSta = new Table(folderFormat + "\\STOCKSTA.DBF");
                tArchivedStockSta.indexingEnabled = false;
                for (int i = 0; i < sBarcodes.Length; i++)
                {
                    int nRecNum = -1;
                    tArchivedStockSta.SearchForRecord(sBarcodes[i], 0, ref nRecNum);
                    if (nRecNum != -1)
                    {
                        string[] sRecordContents = tArchivedStockSta.GetRecordFrom(nRecNum);

                        // Item exists, so check and see if it's in stocklen.dbf
                        nRecNum = -1;
                        tStockLength.SearchForRecord(sBarcodes[i], 0, ref nRecNum);
                        if (nRecNum == -1)
                        {
                            // Create a new record
                            string dtFrom = dtCurrent.Day.ToString();
                            if (dtCurrent.Day < 10)
                                dtFrom = "0" + dtFrom;

                            if (dtCurrent.Month < 10)
                                dtFrom += "0";
                            dtFrom += dtCurrent.Month.ToString();

                            if (dtCurrent.AddYears(-2000).Year < 10)
                                dtFrom += "0";
                            dtFrom += dtCurrent.Year.ToString()[2].ToString() + dtCurrent.Year.ToString()[3].ToString();

                            string[] toAdd = { sBarcodes[i], sRecordContents[35], dtFrom, "0" };
                            tStockLength.AddRecord(toAdd);
                        }

                        // Now that the record has been created (or it already existed), update it
                        nRecNum = -1;
                        tStockLength.SearchForRecord(sBarcodes[i], 0, ref nRecNum);

                        decimal dQIS = 0;
                        try
                        {
                            dQIS = Convert.ToDecimal(sRecordContents[36]);
                        }
                        catch
                        {
                            ; // Do nothing, assume that the QIS is 0
                        }

                        if (dQIS == 0)
                        {
                            decimal dCurrentOutOfStockPeriod = Convert.ToDecimal(tStockLength.GetRecordFrom(nRecNum)[3]);
                            dCurrentOutOfStockPeriod++;
                            tStockLength.EditRecordData(nRecNum, 3, dCurrentOutOfStockPeriod.ToString());
                        }
                    }
                }
                tStockLength.SaveToFile("STOCKLEN.DBF");
                fp.pb.Value++;
            }
            fp.Close();
            tStockLength.SaveToFile("STOCKLEN.DBF");


        }

        private void SetupTills()
        {
            Till = new Till[tTills.NumberOfRecords];
            for (int i = 0; i < Till.Length; i++)
            {
                Till[i] = new Till(tTills.GetRecordFrom(i));
            }
        }

        public bool NeedsSettingUp()
        {
            if (tSettings.NumberOfRecords == 0)
            {
                return true;
            }
            else
                return false;
        }

        public void UpdateVATRates(string[] sVATCodes, string[] sVATNames, decimal[] dVATRates)
        {
            while (tVATRates.NumberOfRecords > 0)
                tVATRates.DeleteRecord(0);
            for (int i = 0; i < sVATNames.Length; i++)
            {
                string[] sToAdd = { sVATCodes[i], sVATNames[i], dVATRates[i].ToString() };
                tVATRates.AddRecord(sToAdd);
            }
            tVATRates.SaveToFile("VAT.DBF");
            foreach (Till t in Till)
            {
                GenerateVATFileForTill(t.Number);
            }
        }

        public void UpdateCreditCardNames(string[] sCardNames)
        {
            int nRecNum = 0;
            if (tSettings.SearchForRecord("NumberOfCards", 0, ref nRecNum))
            {
                int nOfCardsCurrently = Convert.ToInt32(tSettings.GetRecordFrom("NumberOfCards", 0, true)[1]);
                for (int i = 0; i < nOfCardsCurrently; i++)
                {
                    string sSettingName = "CRD";
                    if (i < 10)
                        sSettingName += "0";
                    sSettingName += i.ToString();
                    int nRecordNum = 0;
                    if (tSettings.SearchForRecord(sSettingName, 0, ref nRecordNum))
                        tSettings.DeleteRecord(nRecordNum);
                    tSettings.EditRecordData(nRecNum, 1, sCardNames.Length.ToString());
                }
            }
            else
            {
                string[] sToAdd = { "NumberOfCards", sCardNames.Length.ToString() };
                tSettings.AddRecord(sToAdd);
            }
            for (int i = 0; i < sCardNames.Length; i++)
            {
                string sCardNo = "CRD";
                if (i < 10)
                    sCardNo += "0";
                sCardNo += i.ToString();
                string[] sToAdd = { sCardNo, sCardNames[i] };
                tSettings.AddRecord(sToAdd);
            }
            tSettings.SaveToFile("SETTINGS.DBF");
        }

        public string LastCheckedForUpdates
        {
            get
            {
                if (tSettings.SearchForRecord("LastUpdate", "SETTINGNAM"))
                    return tSettings.GetRecordFrom("LastUpdate", 0)[1];
                else
                    return "";
            }
            set
            {
                int nRecordNumber = 0;
                if (tSettings.SearchForRecord("LastUpdate", 0, ref nRecordNumber))
                {
                    tSettings.EditRecordData(nRecordNumber, 1, value);
                }
                else
                {
                    string[] sToAdd = { "LastUpdate", value };
                    tSettings.AddRecord(sToAdd);
                }
                tSettings.SaveToFile("SETTINGS.DBF");
            }


        }

        public string CompanyName
        {
            get
            {
                if (tSettings.SearchForRecord("CompanyName", "SETTINGNAM"))
                    return tSettings.GetRecordFrom("CompanyName", 0)[1];
                else
                    return "";
            }
            set
            {
                int nRecordNumber = 0;
                if (tSettings.SearchForRecord("CompanyName", 0, ref nRecordNumber))
                {
                    tSettings.EditRecordData(nRecordNumber, 1, value);
                }
                else
                {
                    string[] sToAdd = { "CompanyName", value };
                    tSettings.AddRecord(sToAdd);
                }
                tSettings.SaveToFile("SETTINGS.DBF");
            }
        }

        public int DiscountThresholdOnTill
        {
            get
            {
                if (tSettings.SearchForRecord("DiscountThreshold", "SETTINGNAM"))
                    return Convert.ToInt32(tSettings.GetRecordFrom("DiscountThreshold", 0)[1]);
                else return 0;
            }
            set
            {
                int nRecordNumber = 0;
                if (tSettings.SearchForRecord("DiscountThreshold", 0, ref nRecordNumber))
                {
                    tSettings.EditRecordData(nRecordNumber, 1, value.ToString());
                }
                else
                {
                    string[] sToAdd = { "DiscountThreshold", value.ToString() };
                    tSettings.AddRecord(sToAdd);
                }
                tSettings.SaveToFile("SETTINGS.DBF");
                foreach (Till t in Till)
                {
                    GenerateSettingsForTill(t.Number);
                }
            }
        }

        public string VATNumber
        {
            get
            {
                if (tSettings.SearchForRecord("VATNumber", "SETTINGNAM"))
                    return tSettings.GetRecordFrom("VATNumber", 0)[1];
                else
                    return "";
            }
            set
            {
                int nRecordNumber = 0;
                if (tSettings.SearchForRecord("VATNumber", 0, ref nRecordNumber))
                    tSettings.EditRecordData(nRecordNumber, 1, value);
                else
                {
                    string[] sToAdd = { "VATNumber", value };
                    tSettings.AddRecord(sToAdd);
                }
                tSettings.SaveToFile("SETTINGS.DBF");
            }
        }

        public int NumberOfCards
        {
            get
            {
                if (tSettings.SearchForRecord("NumberOfCards", "SETTINGNAM"))
                    return Convert.ToInt32(tSettings.GetRecordFrom("NumberOfCards", 0)[1]);
                else
                    return 0;
            }
            set
            {
                int nRecordNumber = 0;
                if (tSettings.SearchForRecord("NumberOfCards", 0, ref nRecordNumber))
                    tSettings.EditRecordData(nRecordNumber, 1, value.ToString());
                else
                {
                    string[] sToAdd = { "NumberOfCards", value.ToString() };
                    tSettings.AddRecord(sToAdd);
                }
                tSettings.SaveToFile("SETTINGS.DBF");
            }
        }

        public int NumberOfVATRates
        {
            get
            {
                return tVATRates.NumberOfRecords;
            }
        }

        public string[] GetShopAddress(string sCode)
        {
            string[] sAddress = new string[4];
            sAddress[0] = tSettings.GetRecordFrom("CompanyName", 0, true)[1];
            for (int i = 1; i < sAddress.Length; i++)
            {
                if (tSettings.SearchForRecord(sCode + "Address" + (i + 1).ToString(), "SETTINGNAM"))
                    sAddress[i] = tSettings.GetRecordFrom(sCode + "Address" + (i + 1).ToString(), 0)[1];
                else
                    sAddress[i] = "";
            }
            return sAddress;
        }

        public void SetShopAddress(string sCode, string[] value)
        {
            if (value.Length != 4)
                return;
            for (int i = 1; i < 4; i++)
            {
                int nRecordNum = 0;
                if (tSettings.SearchForRecord("Address" + i.ToString(), 0, ref nRecordNum))
                    tSettings.EditRecordData(nRecordNum, 1, value[i]);
                else
                {
                    string[] sToAdd = { "Address" + i.ToString(), value[i] };
                    tSettings.AddRecord(sToAdd);
                }
            }
            tSettings.SaveToFile("SETTINGS.DBF");
        }
        

        public string[,] VATRates
        {
            get
            {
                string[,] sToReturn = new string[tVATRates.NumberOfRecords, 3];
                for (int i = 0; i < tVATRates.NumberOfRecords; i++)
                {
                    sToReturn[i, 0] = tVATRates.GetRecordFrom(i)[0];
                    sToReturn[i, 1] = tVATRates.GetRecordFrom(i)[1];
                    sToReturn[i, 2] = tVATRates.GetRecordFrom(i)[2];
                }
                return sToReturn;
            }
        }

        public int NumberOfTills(string sShopCode)
        {
            int nCount = 0;
            for (int i = 0; i < Till.Length; i++)
            {
                if (Till[i].ShopCode == sShopCode)
                    nCount++;
            }
            return nCount;
        }

        public void AddTill(string sTillCode, string sTillName, string[] sReceiptMessage, string sLocation, string sCollectionMap, string sShopCode)
        {
            string[] sToAdd = { sTillCode, sTillName, sLocation, sReceiptMessage[0], sReceiptMessage[1], sReceiptMessage[2], "NNNNNNN", sShopCode, sCollectionMap, GetDDMMYYDate() };
            if (tTills.SearchForRecord(sTillCode, "NUMBER"))
                EditTillData(sToAdd);
            else
            {
                tTills.AddRecord(sToAdd);
                tTills.SaveToFile("TILL.DBF");
                Array.Resize<Till>(ref Till, Till.Length + 1);
                Till[Till.Length - 1] = new Till(tTills.GetRecordFrom(sTillCode, 0));
            }
        }

        public void AddShop(string sShopCode, string sShopName, string[] sAddress)
        {
            string[] sToAdd = { sShopCode, sShopName };
            int nRecNum = 0;
            if (tShop.SearchForRecord(sShopCode, 0, ref nRecNum))
            {
                tShop.EditRecordData(nRecNum, 1, sShopName);
            }
            else
            {
                tShop.AddRecord(sToAdd);
            }
            tShop.SaveToFile("SHOP.DBF");
            for (int i = 0; i < sAddress.Length; i++)
            {
                if (tSettings.SearchForRecord(sShopCode + "Address" + (i + 1).ToString(), 0, ref nRecNum))
                {
                    tSettings.EditRecordData(nRecNum, 1, sAddress[i]);
                }
                else
                {
                    string[] sAddToAdd = { sShopCode + "Address" + (i + 1).ToString(), sAddress[i] };
                    tSettings.AddRecord(sAddToAdd);
                }
            }
            tSettings.SaveToFile("SETTINGS.DBF");
        }

        private void EditTillData(string[] sToAdd)
        {
            int nRecNum = 0;
            tTills.SearchForRecord(sToAdd[0], 0, ref nRecNum);
            tTills.DeleteRecord(nRecNum);
            tTills.AddRecord(sToAdd);
            tTills.SaveToFile("TILL.DBF");
            SetupTills();
            GenerateSettingsForTill(Convert.ToInt32(sToAdd[0]));
        }

        public void RemoveTill(string sCode)
        {
            int nRecNum = 0;
            if (tTills.SearchForRecord(sCode, 0, ref nRecNum))
                tTills.DeleteRecord(nRecNum);
            tTills.SaveToFile("TILL.DBF");
            SetupTills();
        }

        public string[] GetListOfTillCodes(string sShopCode)
        {
            string[] sToReturn = new string[NumberOfTills(sShopCode)];
            int nSkipped = 0;
            for (int i = 0; i < sToReturn.Length; i++)
            {
                if (tTills.GetRecordFrom(i)[7].ToUpper() == sShopCode.ToUpper())
                    sToReturn[i - nSkipped] = tTills.GetRecordFrom(i)[0];
                else
                    nSkipped++;
            }
            return sToReturn;
        }

        public string[] GetTillData(string sTillCode)
        {
            int nRecordNum = 0;
            if (tTills.SearchForRecord(sTillCode, 0, ref nRecordNum))
                return tTills.GetRecordFrom(nRecordNum);
            else
                return new string[5];
        }

        public void AddSupplier(string[] sSupplierDetails)
        {
            int nRecNum = 0;
            if (tSupplier.SearchForRecord(sSupplierDetails[0], 0, ref nRecNum))
            {
                tSupplier.DeleteRecord(nRecNum);
            }
            tSupplier.AddRecord(sSupplierDetails);
            tSupplier.SaveToFile("SUPPLIER.DBF");
        }

        public string[] GetSupplierDetails(string sSupplierCode)
        {
            int nRecNo = 0;
            if (tSupplier.SearchForRecord(sSupplierCode, 0, ref nRecNo))
            {
                return tSupplier.GetRecordFrom(nRecNo);
            }
            else
                return new string[13];
        }

        public string[] GetListOfSuppliers()
        {
            int nDel = 0;
            string[] sToReturn = new string[tSupplier.NumberOfRecords];
            for (int i = 0; i < tSupplier.NumberOfRecords; i++)
            {
                if (tSupplier.GetRecordFrom(i)[15].ToUpper() != "YES")
                {
                    sToReturn[i-nDel] = tSupplier.GetRecordFrom(i)[0];
                }
                else
                {
                    Array.Resize<string>(ref sToReturn, sToReturn.Length - 1);
                    nDel++;
                }
            }
            return sToReturn;
        }

        public string[] GetListOfCategories(string sParentCategory)
        {
            string[] sPossibleCategories = tCategory.SearchAndGetAllMatchingRecords(0, sParentCategory);
            int nToRemove = 0;
            for (int i = 0; i < sPossibleCategories.Length; i++)
            {
                if (sPossibleCategories[i].Length != sParentCategory.Length + 2)
                {
                    sPossibleCategories[i] = "$REMOVE";
                    nToRemove++;
                }
                else if (!sPossibleCategories[i].StartsWith(sParentCategory))
                {
                    sPossibleCategories[i] = "$REMOVE";
                    nToRemove++;
                }
            }
            string[] sToReturn = new string[sPossibleCategories.Length - nToRemove];
            int nRemoved = 0;
            for (int i = 0; i < sPossibleCategories.Length; i++)
            {
                if (sPossibleCategories[i] != "$REMOVE")
                {
                    sToReturn[i - nRemoved] = sPossibleCategories[i];
                }
                else
                    nRemoved++;
            }
            return sToReturn;
        }

        public string GetCategoryDesc(string sCode)
        {
            try
            {
                return tCategory.GetRecordFrom(sCode, 0, true)[1];
            }
            catch
            {
                return "";
            }
        }

        public void UpdateShopInfo(string[] sShopNames, string[] sShopCodes)
        {
            while (tShop.NumberOfRecords > 0)
                tShop.DeleteRecord(0);
            for (int i = 0; i < sShopNames.Length; i++)
            {
                string[] sToAdd = { sShopNames[i], sShopCodes[i] };
                tShop.AddRecord(sToAdd);
            }
            tShop.SaveToFile("SHOP.DBF");
        }

        public string[] GetListOfShopCodes()
        {
            string[] sCodes = new string[tShop.NumberOfRecords];
            for (int i = 0; i < sCodes.Length; i++)
            {
                sCodes[i] = tShop.GetRecordFrom(i)[0];
            }
            return sCodes;
        }

        public string GetShopNameFromCode(string sCode)
        {
            try
            {
                return tShop.GetRecordFrom(sCode, 0)[1];
            }
            catch
            {
                return "";
            }
        }

        public int NumberOfShops
        {
            get
            {
                return tShop.NumberOfRecords;
            }
        }

        /*public string[] GetStockStatInfo(string sBarcode)
        {
            return tStockStats.GetRecordFrom(sBarcode, 0);
        }*/

        public string[] GetMainStockInfo(string sBarcode)
        {
            return tStock.GetRecordFrom(sBarcode, 0, true);
        }

        public decimal GetVATRateFromCode(string sCode)
        {
            try
            {
                return Convert.ToDecimal(tVATRates.GetRecordFrom(GetMainStockInfo(sCode)[3], 0)[2]);
            }
            catch
            {
                return -1;
            }
        }

        public decimal GetVATRateFromVATCode(string sVATCode)
        {
            try
            {
                if (sVATCode == "")
                    throw new NotSupportedException("No VAT Rate given");
                return Convert.ToDecimal(tVATRates.GetRecordFrom(sVATCode, 0)[2]);
            }
            catch
            {
                return -1;
            }
        }

        // Taken from GTill
        public string[,] sGetAccordingToPartialBarcode(string sBarcode, ref int nOfResults)
        {
            return tStock.SearchAndGetAllMatchingRecords(0, sBarcode, ref nOfResults);
        }

        // Taken from GTill
        public string[,] sGetAccordingToPartialDescription(string sDesc, ref int nOfResults)
        {
            return tStock.SearchAndGetAllMatchingRecords(1, sDesc.Split(' '), ref nOfResults);
        }

        /*public string[] GetItemRecordContents(string sBarcode)
        {
            return tStock.GetRecordFrom(sBarcode, 0, true);
        }*/

        public string[] GetItemStockStaRecord(string sBarcode, string sShopCode)
        {
            int nRecNum = tStockStats.GetRecordNumberFromTwoFields(sBarcode, 0, sShopCode, 35);
            if (nRecNum != -1)
                return tStockStats.GetRecordFrom(nRecNum);
            else
                return new string[1];
        }

        // Taken from GTill
        public string[] CheckIfItemHasChildren(string sBarcode)
        {
            int nOfRecs = 0;
            /*string[] sChildren =*/ return tStock.SearchAndGetAllMatchingRecords(7, sBarcode, ref nOfRecs, true, 0);
            /*string[] stoReturn = new string[nOfRecs];
            for (int i = 0; i < nOfRecs; i++)
            {
                stoReturn[i] = sChildren[i, 0].TrimEnd(' ');
            }
            return stoReturn;*/
        }

        // Taken from GTill
        public string[] GetCodesOfItemsInCategory(string sCategory, bool bAcceptSubCatCodes)
        {
            if (!bAcceptSubCatCodes)
            {
                int nOfResults = 0;
                string[] sSearch = tStock.SearchAndGetAllMatchingRecords(4, sCategory, ref nOfResults, true, 0);
                /*
                */
                return sSearch;
            }
            else
            {
                int nOfResults = 0;
                string[,] sSearch = tStock.SearchAndGetAllMatchingRecords(4, sCategory, ref nOfResults);
                int nMissed = 0;
                int nToMiss = 0;
                bool[] bMissOut = new bool[nOfResults];
                for (int i = 0; i < bMissOut.Length; i++)
                    bMissOut[i] = false;
                for (int i = 0; i < nOfResults; i++)
                {
                    // Trying to cut out on items showing up that aren't in this category. ie AP shows items from CRAP
                    if (!sSearch[i, 4].StartsWith(sCategory))
                    {
                        bMissOut[i] = true;
                        nToMiss++;
                    }
                }

                string[] sResults = new string[nOfResults];
                for (int i = 0; i < nOfResults; i++)
                {
                    sResults[i] = sSearch[i, 0];
                }
                return sResults;
            }
        }

        public Item[] GetItemsInCategory(string sCategory, string sShopCode)
        {
            int nOfRecords = 0;
            string[,] sItems = tStock.SearchAndGetAllMatchingRecords(4, sCategory, ref nOfRecords, true);
            Item[] iItems = new Item[nOfRecords];
            string[] sStockStats = new string[41];
            string[] sMainStock = new string[9];
            for (int i = 0; i < nOfRecords; i++)
            {
                int nRecNum = tStockStats.GetRecordNumberFromTwoFields(sItems[i,0], 0, sShopCode, 35);
                for (int x = 0; x < 9; x++)
                {
                    sMainStock[x] = sItems[i,x];
                }
                sStockStats = tStockStats.GetRecordFrom(nRecNum);
                iItems[i] = new Item(sMainStock, sStockStats);
                iItems[i].StockLevel = Convert.ToDecimal(sStockStats[36]);
            }
            return iItems;
        }
        // Taken from GTill
        public decimal GetItemStockLevel(string sBarcode)
        {
            string[] sInfo = tStockStats.GetRecordFrom(sBarcode, 0, true);
            decimal nToReturn = -1024;
            if (sInfo[0] != null && sInfo[36] != "")
                nToReturn = Convert.ToDecimal(sInfo[36].TrimStart(' ').Replace(".00", ""));
            return nToReturn;
        }

        public int DayNumberOfWeek(string sSalesDate)
        {
            // Sales date in format DDMMYY
            int nYear = Convert.ToInt32(sSalesDate[4].ToString() + sSalesDate[5].ToString());
            int nMonth = Convert.ToInt32(sSalesDate[2].ToString() + sSalesDate[3].ToString());
            int nDay = Convert.ToInt32(sSalesDate[0].ToString() + sSalesDate[1].ToString());
            DateTime d = new DateTime(nYear, nMonth, nDay);

            switch (d.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    nDay = 1;
                    break;
                case DayOfWeek.Monday:
                    nDay = 2;
                    break;
                case DayOfWeek.Tuesday:
                    nDay = 3;
                    break;
                case DayOfWeek.Wednesday:
                    nDay = 4;
                    break;
                case DayOfWeek.Thursday:
                    nDay = 5;
                    break;
                case DayOfWeek.Friday:
                    nDay = 6;
                    break;
                case DayOfWeek.Saturday:
                    nDay = 7;
                    break;
            }
            return nDay;
        }

        public bool CollectDataFromTills(string sSalesDate, bool bUpdateDaily)
        {

            if (DateTime.Now.DayOfWeek.Equals(DayOfWeek.Sunday))
            {
                if (System.Windows.Forms.MessageBox.Show("Today is Sunday, skip collection and just shutdown till?", "Sunday", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    return true;
                }
            }
            frmProgressBar fp = new frmProgressBar("Collecting");
            fp.Show();
            // sSalesDate in format DDMMYY
            bool bFinishedOK = true;
            bool bAllTillsCollected = true;
            string[] sCodesOfItemsSold = new string[0];
            decimal[] nQuantitiesOfItemsSold = new decimal[0];
            decimal[] dGrossSalePrice = new decimal[0];
            string[] sDepartmentCodes = new string[0];
            // Get the items sold by downloading the tills' data

            EndOfPeriod(Period.Daily);

            foreach (Till t in Till)
            {
                if (!Directory.Exists("TILL" + t.Number.ToString()))
                {
                    Directory.CreateDirectory("TILL" + t.Number.ToString());
                    Directory.CreateDirectory("TILL" + t.Number.ToString() + "\\INGNG");
                    Directory.CreateDirectory("TILL" + t.Number.ToString() + "\\OUTGNG");
                }
                int dayOfWeek = DayNumberOfWeek(sSalesDate);
                if (dayOfWeek - 2 < 0)
                {
                    dayOfWeek += 7;
                }
                fp.Text = "Checking Collection Map";
                // Checks to see if previous days have been collected
                for (int i = 0; i < dayOfWeek - 2; i++)
                {
                    if (t.CollectedMap[i] == 'N' && t.CollectionMap[i] == 'Y')
                    {
                            switch (System.Windows.Forms.MessageBox.Show("Days before today still need to be collected! Will you be collecting these at a later time? If not, they will be marked as collected. Cancel will halt the collection process.", "Collection", System.Windows.Forms.MessageBoxButtons.YesNoCancel))
                            {
                                case System.Windows.Forms.DialogResult.No:
                                    string sNewCollMap = "";
                                    for (int z = 0; z < 7; z++)
                                    {
                                        if (z != i)
                                            sNewCollMap += t.CollectedMap[z].ToString();
                                        else
                                            sNewCollMap += "Y";
                                    }
                                    t.CollectedMap = sNewCollMap;
                                    break;

                                case System.Windows.Forms.DialogResult.Cancel:
                                    return false;
                                    break;

                            }
                        
                    }
                }
                t.SaveTillChanges(ref tTills);
                
                if (t.CollectedMap[dayOfWeek - 2] == 'Y')
                {
                    bFinishedOK = false;
                    if (System.Windows.Forms.MessageBox.Show("Till " + t.Number.ToString() + " has already has its data collected. Re-collect? This will affect weekly, monthly, yearly totals as they will be duplicated if it really has already been collected.", "Re-Collect?", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                    {
                        continue;
                    }
                }
                try
                {
                    fp.Text = "Copying Sales Data From Till";
                    t.GetSalesDataFromTill(sSalesDate);
                }
                catch
                {
                    bFinishedOK = false;
                    bAllTillsCollected = false;
                    continue;
                }
                Table tRepData = new Table(sTDir + "TILL" + t.Number.ToString() + "\\INGNG\\" + "REPDATA" + DayNumberOfWeek(sSalesDate).ToString() + ".DBF");
                Table tTData = new Table(sTDir + "TILL" + t.Number.ToString() + "\\INGNG\\" + "TDATA" + DayNumberOfWeek(sSalesDate).ToString() + ".DBF");
                Table tTHDR = new Table(sTDir + "TILL" + t.Number.ToString() + "\\INGNG\\" + "THDR" + DayNumberOfWeek(sSalesDate).ToString() + ".DBF");
                if (tRepData.GetRecordFrom(0)[1].Replace("/", "").ToUpper() != sSalesDate)
                {
                    bFinishedOK = false;
                    bAllTillsCollected = false;
                    continue;
                }
                // Update Collection Map
                fp.Text = "Updating Collection Map";
                t.LastCollection = sSalesDate;
                t.SaveTillChanges(ref tTills);
                // Go through each item in TData adding it to the array
                fp.Text = "Arranging Sales Data";
                for (int i = 0; i < tTData.NumberOfRecords; i++)
                {
                    string[] sTDataRecord = tTData.GetRecordFrom(i);
                    bool bFound = false;
                    int nLocation = 0;
                    for (int x = 0; x < sCodesOfItemsSold.Length; x++)
                    {
                        if (sCodesOfItemsSold[x] == sTDataRecord[3] && sDepartmentCodes[x] == t.ShopCode)
                        {
                            bFound = true;
                            nLocation = x;
                            break;
                        }
                    }
                    // Check if the line has been voided
                    if (sTDataRecord[2].Trim().Equals("1"))
                    {
                        continue;
                    }
                    if (bFound)
                    {
                        nQuantitiesOfItemsSold[nLocation] += Convert.ToDecimal(sTDataRecord[5]);
                        dGrossSalePrice[nLocation] += (Convert.ToDecimal(sTDataRecord[6]) - Convert.ToDecimal(sTDataRecord[8]));
                    }
                    else
                    {
                        Array.Resize<string>(ref sCodesOfItemsSold, sCodesOfItemsSold.Length + 1);
                        Array.Resize<decimal>(ref nQuantitiesOfItemsSold, nQuantitiesOfItemsSold.Length + 1);
                        Array.Resize<decimal>(ref dGrossSalePrice, dGrossSalePrice.Length + 1);
                        Array.Resize<string>(ref sDepartmentCodes, sDepartmentCodes.Length + 1);
                        sCodesOfItemsSold[sCodesOfItemsSold.Length - 1] = sTDataRecord[3];
                        nQuantitiesOfItemsSold[nQuantitiesOfItemsSold.Length - 1] = Convert.ToDecimal(sTDataRecord[5]);
                        dGrossSalePrice[dGrossSalePrice.Length - 1] = Math.Round(((Convert.ToDecimal(sTDataRecord[6]) / nQuantitiesOfItemsSold[dGrossSalePrice.Length - 1]) - (Convert.ToDecimal(sTDataRecord[8]) / nQuantitiesOfItemsSold[dGrossSalePrice.Length - 1])) * nQuantitiesOfItemsSold[dGrossSalePrice.Length - 1], 2);
                        sDepartmentCodes[sDepartmentCodes.Length - 1] = t.ShopCode;
                    }
                }
                // Check for Deposit Paid
                int nDepRecLoc = 0;
                if (tRepData.SearchForRecord("DEP", 1, ref nDepRecLoc))
                {
                    bool bAlreadyExists = false;
                    int nDepLoc = 0;
                    for (int i = 0; i < sCodesOfItemsSold.Length; i++)
                    {
                        if (sCodesOfItemsSold[i] == "DEP")
                        {
                            bAlreadyExists = true;
                            nDepLoc = i;
                            break;
                        }
                    }
                    if (!bAlreadyExists)
                    { 
                        Array.Resize<string>(ref sCodesOfItemsSold, sCodesOfItemsSold.Length + 1);
                        Array.Resize<decimal>(ref nQuantitiesOfItemsSold, nQuantitiesOfItemsSold.Length + 1);
                        Array.Resize<decimal>(ref dGrossSalePrice, dGrossSalePrice.Length + 1);
                        Array.Resize<string>(ref sDepartmentCodes, sDepartmentCodes.Length + 1);
                        nDepLoc = sCodesOfItemsSold.Length - 1;
                    }
                    sCodesOfItemsSold[nDepLoc] = "DEP";
                    nQuantitiesOfItemsSold[nDepLoc] = Convert.ToDecimal(tRepData.GetRecordFrom(nDepRecLoc)[2]);
                    dGrossSalePrice[nDepLoc] = (Convert.ToDecimal(tRepData.GetRecordFrom(nDepRecLoc)[3]));
                    sDepartmentCodes[nDepLoc] = t.ShopCode;
                }
                // Go through and swap child barcodes with parent ones
                fp.Text = "Processing Child Barcodes";
                for (int i = 0; i < sCodesOfItemsSold.Length; i++)
                {
                    if (GetMainStockInfo(sCodesOfItemsSold[i])[5] == "5")
                    {
                        nQuantitiesOfItemsSold[i] /= Convert.ToDecimal(GetItemStockStaRecord(sCodesOfItemsSold[i], GetTillShopCode(t.Number))[38]);
                        sCodesOfItemsSold[i] = GetMainStockInfo(sCodesOfItemsSold[i])[7];
                    }
                }
                // Collect offers and reset data on the till
                CollectOffers(t.FileLocation);
            }
            fp.pb.Value = 50;
            // Now that the 3 arrays have every item that has been sold, put the details into StockStats
            fp.Text = "Processing Sales";
            bool bProcessedOK = true;
            for (int i = 0; i < sCodesOfItemsSold.Length; i++)
            {
                bProcessedOK = ProcessSoldItem(sCodesOfItemsSold[i], nQuantitiesOfItemsSold[i], dGrossSalePrice[i], sDepartmentCodes[i], bUpdateDaily, sSalesDate);
                if (!bProcessedOK)
                    bFinishedOK = false;
            }
            fp.pb.Value = 90;
            for (int i = 0; i < tShop.NumberOfRecords; i++)
            {
                ProcessAveSales(tShop.GetRecordFrom(i)[0]);
            }
            tStockStats.SaveToFile("STOCKSTA.DBF");
            CopyWaitingFilesToTills();
            GetSuggestedItemsFromTills();
            CollectEmailsFromTills();
            UpdateStockLengthDatabase();
            if (bFinishedOK)
                UpdateTotalSales();
            if (DayNumberOfWeek(sSalesDate) == 7)
            {
                bool bAllCollected = true;
                for (int i = 0; i < Till.Length; i++)
                {
                    if (Till[i].CollectedMap != Till[i].CollectionMap)
                    {
                        bAllCollected = false;
                        // Give a reason for not collecting!
                        TextWriter tw = new StreamWriter("collectionLog.txt", true);
                        tw.WriteLine(DateTime.Now.ToString() + " - Not collecting as collection map for till" + i.ToString() + " is " + Till[i].CollectedMap + " and the collection map is " + Till[i].CollectionMap);
                        tw.Close();

                        System.Windows.Forms.MessageBox.Show("Not offering an end-of-week despite it being due. A log has been created with a reason (see Backoffice folder collectionLog.txt)");
                    }
                }
                if (bAllCollected)
                {
                    if (System.Windows.Forms.MessageBox.Show("Would you like to do an end-of-week?", "End of week?", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        EndOfPeriod(Period.Weekly);
                    }
                }
            }
            int dDay = Convert.ToInt32(sSalesDate[0].ToString() + sSalesDate[1].ToString());
            int dMonth = Convert.ToInt32(sSalesDate[2].ToString() + sSalesDate[3].ToString());
            int dYear = Convert.ToInt32("20" + sSalesDate[4].ToString() + sSalesDate[5].ToString());
            DateTime dtCollectionDate = new DateTime(dYear, dMonth, dDay);
            int nMonth = dtCollectionDate.Month;
            int nYear = dtCollectionDate.Year;
            if (dtCollectionDate.AddDays(GetDaysUntilNextCollection(DayNumberOfWeek(sSalesDate), ref Till)).Month != nMonth)
            {
                if (System.Windows.Forms.MessageBox.Show("Would you like to do an end-of-month?", "End of month?", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    EndOfPeriod(Period.Monthly);
                }
            }
            if (dtCollectionDate.AddDays(GetDaysUntilNextCollection(DayNumberOfWeek(sSalesDate), ref Till)).Year != nYear)
            {
                if (System.Windows.Forms.MessageBox.Show("Would you like to do an end-of-year?", "End of year?", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    EndOfPeriod(Period.Yearly);
                }
            }
            fp.Close();
            FileManagementEngine.CompressWholeArchive();
            return bFinishedOK;
        }

        private int GetDaysUntilNextCollection(int nDayOfWeek, ref Till[] tTills)
        {
            int nMin = 8;
            for (int i = 0; i < tTills.Length; i++)
            {
                int nDaysUntilNextColl = 1;
                while (tTills[i].CollectionMap[(nDayOfWeek + 5 + nDaysUntilNextColl) % 7] == 'N')
                    nDaysUntilNextColl++;
                if (nDaysUntilNextColl < nMin)
                    nMin = nDaysUntilNextColl;
            }
            return nMin;
        }

        /// <summary>
        /// Process a SINGLE commission item that has sold
        /// </summary>
        /// <param name="sBarcode">The barcode</param>
        /// <param name="dSellingPrice">The price sold for</param>
        /// <param name="sCollectionDate">The date collected</param>
        private void ProcessCommissionItem(string sBarcode, decimal dSellingPrice, string sCollectionDate, decimal dVAT, decimal dProfit)
        {
            // Get the supplier's code
            string sCommCode = tStock.GetRecordFrom(sBarcode, 0, true)[6];
            //int nRecNum = tCommItems.GetRecordNumberFromTwoFields(sBarcode, 1, sCommCode, 0);
            int nRecNum = -1;
            string[] sContents = tCommItems.GetRecordFrom(sBarcode, 1, 0, ref nRecNum);
            while (sContents.Length <= 1 || sContents[5] != "")
            {
                if (nRecNum >= tCommItems.NumberOfRecords)
                {
                    nRecNum = -1;
                    break;
                }
                sContents = tCommItems.GetRecordFrom(sBarcode, 1, nRecNum, ref nRecNum);
                nRecNum++;
            }
            if (nRecNum != -1)
            {
                string[] sRec = sContents;
                decimal dCommAmnt = Convert.ToDecimal(tStock.GetRecordFrom(sBarcode, 0, true)[8]);
                // Delete the record and then add this to replace it:
                string[] sToAdd = { sCommCode, sBarcode, sContents[2], dCommAmnt.ToString(), dSellingPrice.ToString(), "Y", sContents[6], sCollectionDate, dVAT.ToString(), FormatMoneyForDisplay(Convert.ToDecimal(sContents[9]) + dProfit), sContents[10] };
                tCommItems.DeleteRecord(nRecNum);
                tCommItems.AddRecord(sToAdd);
            }
            tCommItems.SaveToFile("COMMITEM.DBF");
        }

        /// <summary>
        /// Marks a certain number of commission items as paid for, and adjusts the STOCKSTA profit record etc. accordingly
        /// </summary>
        /// <param name="sBarcode">The barcode of the item to pay for</param>
        /// <param name="dQty">The quantity of the item to pay for</param>
        /// <param name="dTotalAmount">The total amount paid to the artist</param>
        /// <returns>True if a success, false otherwise</returns>
        public bool MarkCommissionItemAsPaid(string sBarcode, decimal dQty, decimal dTotalAmount, string sShopCode)
        {
            // Make sure that there are enough items in COMMITEM to mark as paid
            decimal dQtyStillToFind = dQty;
            for (int i = 0; i < tCommItems.NumberOfRecords && dQtyStillToFind > 0; i++)
            {
                if (tCommItems.GetRecordFrom(i)[1].ToUpper() == sBarcode.ToUpper())
                {
                    if (tCommItems.GetRecordFrom(i)[10] == "" || Convert.ToDecimal(tCommItems.GetRecordFrom(i)[10]) == 0)
                    {
                        dQtyStillToFind -= 1;
                    }
                }
            }
            if (dQtyStillToFind > 0)
            {
                // There aren't enough items to be marked as paid in the database!
                return false;
            }

            // There must be enough there to pay for
            decimal dSingleAmount = Math.Round(dTotalAmount / dQty, 2);

            dQtyStillToFind = dQty;
            for (int i = 0; i < tCommItems.NumberOfRecords && dQtyStillToFind > 0; i++)
            {
                if (tCommItems.GetRecordFrom(i)[1].ToUpper() == sBarcode.ToUpper())
                {
                    if (tCommItems.GetRecordFrom(i)[10] == "" || Convert.ToDecimal(tCommItems.GetRecordFrom(i)[10]) == 0)
                    {
                        decimal dDifference = dSingleAmount - Convert.ToDecimal(tCommItems.GetRecordFrom(i)[3]);

                        // Edit the commitem record
                        tCommItems.EditRecordData(i, 10, FormatMoneyForDisplay(dSingleAmount));
                        // And the profit record
                        tCommItems.EditRecordData(i, 9, FormatMoneyForDisplay(Convert.ToDecimal(tCommItems.GetRecordFrom(i)[9]) - dDifference));

                        // Edit the stocksta data
                        // Edit D,W,M,Y COGS
                        int nRecNum = tStockStats.GetRecordNumberFromTwoFields(sBarcode, 0, sShopCode, 35);
                        if (nRecNum != -1)
                        {
                            decimal dCurrent = Convert.ToDecimal(tStockStats.GetRecordFrom(nRecNum)[8]);
                            tStockStats.EditRecordData(nRecNum, 8, (dCurrent + dDifference).ToString());

                            dCurrent = Convert.ToDecimal(tStockStats.GetRecordFrom(nRecNum)[12]);
                            tStockStats.EditRecordData(nRecNum, 12, (dCurrent + dDifference).ToString());

                            dCurrent = Convert.ToDecimal(tStockStats.GetRecordFrom(nRecNum)[16]);
                            tStockStats.EditRecordData(nRecNum, 16, (dCurrent + (dDifference)).ToString());

                            dCurrent = Convert.ToDecimal(tStockStats.GetRecordFrom(nRecNum)[20]);
                            tStockStats.EditRecordData(nRecNum, 20, (dCurrent + (dDifference)).ToString());
                        }
                        dQtyStillToFind -= 1;
                    }
                }
            }
            tStockStats.SaveToFile("STOCKSTA.DBF");
            tCommissioners.SaveToFile("COMMPPL.DBF");
            return true;
        }

        private bool ProcessSoldItem(string sBarcode, decimal dQuantitySold, decimal dGrossAmount, string sShopCode, bool bDoDaily, string sCollectionDate)
        {
            sBarcode = sBarcode.ToUpper();
            dQuantitySold = Math.Round(dQuantitySold, 3);
            int nCodesRecordNum = tStockStats.GetRecordNumberFromTwoFields(sBarcode, 0, sShopCode, tStockStats.FieldNumber("SHOPCODE"));
            if (nCodesRecordNum == -1)
            {
                return false;
            }
            string[] sOldStockStaRecord = tStockStats.GetRecordFrom(nCodesRecordNum);
            for (int x = 0; x < sOldStockStaRecord.Length; x++)
            {
                if (sOldStockStaRecord[x] == "")
                    sOldStockStaRecord[x] = "0";
            }
            string[] sNewStockStaRecord = new string[tStockStats.ReturnFieldNames().Length];
            sNewStockStaRecord[0] = sBarcode;
            sNewStockStaRecord[1] = sOldStockStaRecord[1];
            decimal dCurrentAveDailySales = Convert.ToDecimal(sOldStockStaRecord[2]);

            // Average Daily Sales
            if (dCurrentAveDailySales == 0)
            {
                sNewStockStaRecord[2] = dQuantitySold.ToString();
            }
            else
            {
                decimal dFraction = 2.0m / 20.0m;
                decimal dFirstPart = dQuantitySold * dFraction;
                decimal dSecondPart = dCurrentAveDailySales * (1.0m - dFraction);
                decimal dAnswer = dFirstPart + dSecondPart;
                string sToAdd = Math.Round(dAnswer, 3, MidpointRounding.AwayFromZero).ToString();
                sNewStockStaRecord[2] = sToAdd;
            }
            sNewStockStaRecord[3] = sOldStockStaRecord[3];
            sNewStockStaRecord[4] = sOldStockStaRecord[4];
            // Calculate Net Price
            decimal dVATRate = 1 + (GetVATRateFromCode(sBarcode) / 100);
            decimal dNetPrice = dGrossAmount / dVATRate;
            decimal dTypeTwoCost = 0;
            if (GetMainStockInfo(sBarcode)[5] == "6")
            {
                dNetPrice = (dGrossAmount - Convert.ToDecimal(sOldStockStaRecord[1])) / dVATRate;

                // Added more recently. The net price is gross - vat, but since vat is only paid on profit
                // surely once the vat has been subtracted, the cost should be added again?

                dNetPrice += Convert.ToDecimal(sOldStockStaRecord[1]);
            }
            else if (GetMainStockInfo(sBarcode)[5] == "2")
            {
                // Calculate the Daily COGS
                decimal dMarginVal = Convert.ToDecimal(sOldStockStaRecord[39]);
                decimal dProfitVal = dNetPrice * (dMarginVal / 100.0m);
                decimal dCostVal = (dNetPrice - dProfitVal);
                sNewStockStaRecord[8] = (Math.Round(dCostVal, 2, MidpointRounding.AwayFromZero)).ToString();
                dTypeTwoCost = Math.Round(dCostVal,2, MidpointRounding.AwayFromZero);
            }
            dNetPrice = Math.Round(dNetPrice, 2);
            if (bDoDaily)
            {
                sNewStockStaRecord[5] = dQuantitySold.ToString();
                sNewStockStaRecord[6] = dGrossAmount.ToString();
                sNewStockStaRecord[7] = dNetPrice.ToString();
                if (GetMainStockInfo(sBarcode)[5] != "2")
                    sNewStockStaRecord[8] = ((Convert.ToDecimal(sOldStockStaRecord[1]) * dQuantitySold) + Convert.ToDecimal(sOldStockStaRecord[8])).ToString();
            }
            else
            {
                sNewStockStaRecord[5] = sOldStockStaRecord[5];
                sNewStockStaRecord[6] = sOldStockStaRecord[6];
                sNewStockStaRecord[7] = sOldStockStaRecord[7];
                sNewStockStaRecord[8] = sOldStockStaRecord[8];
            }
            sNewStockStaRecord[9] = (Convert.ToDecimal(sOldStockStaRecord[9]) + dQuantitySold).ToString();
            sNewStockStaRecord[10] = (Convert.ToDecimal(sOldStockStaRecord[10]) + dGrossAmount).ToString();
            sNewStockStaRecord[11] = (Convert.ToDecimal(sOldStockStaRecord[11]) + dNetPrice).ToString();
            if (GetMainStockInfo(sBarcode)[5] != "2")
                sNewStockStaRecord[12] = (Convert.ToDecimal(sOldStockStaRecord[12]) + (Convert.ToDecimal(sOldStockStaRecord[1]) * dQuantitySold)).ToString();
            else
                sNewStockStaRecord[12] = (Convert.ToDecimal(sOldStockStaRecord[12]) + dTypeTwoCost).ToString();
            sNewStockStaRecord[13] = (Convert.ToDecimal(sOldStockStaRecord[13]) + dQuantitySold).ToString();
            sNewStockStaRecord[14] = (Convert.ToDecimal(sOldStockStaRecord[14]) + dGrossAmount).ToString();
            sNewStockStaRecord[15] = (Convert.ToDecimal(sOldStockStaRecord[15]) + dNetPrice).ToString();
           if (GetMainStockInfo(sBarcode)[5] != "2")
                sNewStockStaRecord[16] = (Convert.ToDecimal(sOldStockStaRecord[16]) + (Convert.ToDecimal(sOldStockStaRecord[1]) * dQuantitySold)).ToString();
           else
               sNewStockStaRecord[16] = (Convert.ToDecimal(sOldStockStaRecord[16]) + dTypeTwoCost).ToString();
            sNewStockStaRecord[17] = (Convert.ToDecimal(sOldStockStaRecord[17]) + dQuantitySold).ToString();
            sNewStockStaRecord[18] = (Convert.ToDecimal(sOldStockStaRecord[18]) + dGrossAmount).ToString();
            sNewStockStaRecord[19] = (Convert.ToDecimal(sOldStockStaRecord[19]) + dNetPrice).ToString();
            if (GetMainStockInfo(sBarcode)[5] != "2")
                sNewStockStaRecord[20] = (Convert.ToDecimal(sOldStockStaRecord[20]) + (Convert.ToDecimal(sOldStockStaRecord[1]) * dQuantitySold)).ToString();
            else
                sNewStockStaRecord[20] = (Convert.ToDecimal(sOldStockStaRecord[20]) + dTypeTwoCost).ToString();
            sNewStockStaRecord[21] = sOldStockStaRecord[21];
            sNewStockStaRecord[22] = sOldStockStaRecord[22];
            sNewStockStaRecord[23] = sOldStockStaRecord[23];
            sNewStockStaRecord[24] = sOldStockStaRecord[24];
            sNewStockStaRecord[25] = sOldStockStaRecord[25];
            sNewStockStaRecord[26] = sOldStockStaRecord[26];
            sNewStockStaRecord[27] = sOldStockStaRecord[27];
            sNewStockStaRecord[28] = sOldStockStaRecord[28];
            sNewStockStaRecord[29] = (Convert.ToDecimal(sOldStockStaRecord[29]) + dQuantitySold).ToString();
            sNewStockStaRecord[30] = (Convert.ToDecimal(sOldStockStaRecord[30]) + dGrossAmount).ToString();
            sNewStockStaRecord[31] = (Convert.ToDecimal(sOldStockStaRecord[31]) + dNetPrice).ToString();
            sNewStockStaRecord[32] = (Convert.ToDecimal(sOldStockStaRecord[32]) + (Convert.ToDecimal(sOldStockStaRecord[1]) * dQuantitySold)).ToString();
            sNewStockStaRecord[33] = sOldStockStaRecord[33];
            sNewStockStaRecord[34] = sOldStockStaRecord[34];
            sNewStockStaRecord[35] = sOldStockStaRecord[35];
            if (GetMainStockInfo(sBarcode)[5] == "1" || GetMainStockInfo(sBarcode)[5] == "6")
            {
                sNewStockStaRecord[36] = (Convert.ToDecimal(sOldStockStaRecord[36]) - dQuantitySold).ToString();
            }
            else
            {
                sNewStockStaRecord[36] = "0";
            }
            if (GetMainStockInfo(sBarcode)[5] == "6")
            {
                decimal dProfit = dNetPrice - Convert.ToDecimal(sOldStockStaRecord[1]);
                decimal dVAT = Math.Round(dGrossAmount - dNetPrice, 2);
                // Process Item is one at a time for commission, so loop through until they're all sold
                for (int i = 0; i < dQuantitySold; i++)
                {
                    ProcessCommissionItem(sBarcode, dGrossAmount, sCollectionDate, dVAT, dProfit);
                }
            }
            sNewStockStaRecord[37] = sOldStockStaRecord[37];
            sNewStockStaRecord[38] = sOldStockStaRecord[38];
            sNewStockStaRecord[39] = sOldStockStaRecord[39];
            sNewStockStaRecord[40] = sCollectionDate;
            sNewStockStaRecord[41] = sOldStockStaRecord[40];
            for (int x = 0; x < sOldStockStaRecord.Length; x++)
            {
                tStockStats.EditRecordData(nCodesRecordNum, x, sNewStockStaRecord[x]);
            }
            for (int i = 0; i < Till.Length; i++)
            {
                decimal dQIS = 0;
                if (GetMainStockInfo(sBarcode)[5] == "1" || GetMainStockInfo(sBarcode)[5] == "6")
                {
                    dQIS = (Convert.ToDecimal(sOldStockStaRecord[36]) - dQuantitySold);
                }
                else
                {
                    dQIS = 0;
                }
                AlterStockLevelOnTill(sBarcode, dQIS, Till[i].Number);
            }
            return true;
        }

        private void ProcessAveSales(string sShopCode)
        {
            for (int i = 0; i < tStockStats.NumberOfRecords; i++)
            {
                string[] sStockStaInfo = tStockStats.GetRecordFrom(i);
                if (sStockStaInfo[35] == sShopCode)
                {
                    decimal dCurrentAveDailySales = Convert.ToDecimal(sStockStaInfo[2]);
                    decimal dQuantitySold = Convert.ToDecimal(sStockStaInfo[5]);
                    if (dQuantitySold == 0)
                    {
                        // Average Daily Sales
                        if (dCurrentAveDailySales == 0)
                        {
                           sStockStaInfo[2] = dQuantitySold.ToString();
                        }
                        else
                        {
                            decimal dFraction = 2.0m / 20.0m;
                            decimal dFirstPart = dQuantitySold * dFraction;
                            decimal dSecondPart = dCurrentAveDailySales * (1.0m - dFraction);
                            decimal dAnswer = dFirstPart + dSecondPart;
                            string sToAdd = Math.Round(dAnswer, 3, MidpointRounding.AwayFromZero).ToString();
                            sStockStaInfo[2] = sToAdd;
                        }
                        tStockStats.EditRecordData(i, 2, sStockStaInfo[2]);
                    }
                }
            }
            tStockStats.SaveToFile("STOCKSTA.DBF");
        }

        private string GetItemDueDate(string sBarcode, string sShopCode)
        {
            //DDMMYY
            for (int i = tOrderLine.NumberOfRecords - 1; i >= 0; i -= 1)
            {
                string[] sOrderLineRec = tOrderLine.GetRecordFrom(i);
                //if (tOrderLine.GetRecordFrom(i)[2] == sBarcode && tOrder.GetRecordFrom(tOrderLine.GetRecordFrom(i)[0], 0, true).Length > 1 && tOrder.GetRecordFrom(tOrderLine.GetRecordFrom(i)[0], 0, true)[6] == sShopCode && (Convert.ToDecimal(tOrderLine.GetRecordFrom(i)[3]) - Convert.ToDecimal(tOrderLine.GetRecordFrom(i)[4])) != 0)
                if (String.Compare(sOrderLineRec[2], sBarcode, true) == 0 && tOrder.GetRecordFrom(sOrderLineRec[0], 0, true).Length > 1 && tOrder.GetRecordFrom(sOrderLineRec[0], 0, true)[6] == sShopCode && (Convert.ToDecimal(sOrderLineRec[3]) - Convert.ToDecimal(tOrderLine.GetRecordFrom(i)[4])) != 0)
                {
                    return tOrder.GetRecordFrom(tOrderLine.GetRecordFrom(i)[0], 0, true)[4];
                }
            }
            try
            {
                return GetItemStockStaRecord(sBarcode, sShopCode)[4];
            }
            catch
            {
                return "";
            }
        }

        private void AlterStockLevelOnTill(string sBarcode, decimal dNewStockLevel, int nTillNum)
        {
            int nTill = -1;
            for (int i = 0; i < Till.Length; i++)
            {
                if (Till[i].Number == nTillNum)
                {
                    nTill = Till[i].Number;
                    break;
                }
            }
            if (nTill == -1)
                return;
            if (!File.Exists("TILL" + nTill.ToString() + "\\OUTGNG\\STKLEVEL.DBF"))
            {
                FileStream fsWriter = new FileStream("TILL" + nTill.ToString() + "\\OUTGNG\\STKLEVEL.DBF", FileMode.Create);
                fsWriter.Write(BackOffice.Properties.Resources.TILLSTKLEVEL, 0, BackOffice.Properties.Resources.TILLSTKLEVEL.Length);
                fsWriter.Close();
            }
            Table tStockLvl = new Table(sTDir + "TILL" + nTill.ToString() + "\\OUTGNG\\STKLEVEL.DBF");
            string[] sStockStaData = GetItemStockStaRecord(sBarcode, GetTillShopCode(nTillNum));
            string[] sMainStockData = GetMainStockInfo(sBarcode);
            string[] sToAddToStkLevel = {sBarcode, sStockStaData[tStockStats.FieldNumber("SHOPCODE")], 
                                                sStockStaData[tStockStats.FieldNumber("QIS")], sMainStockData[tStock.FieldNumber("CATEGORY")], sStockStaData[3], GetItemDueDate(sBarcode, GetTillShopCode(nTill))};
            tStockLvl.AddRecord(sToAddToStkLevel);
            tStockLvl.SaveToFile("TILL" + nTill.ToString() + "\\OUTGNG\\STKLEVEL.DBF");
        }

        public enum ReportType { SalesReport, StockLevelReport, OrderInfo, StockValuationReport, ReprintReceipt, ComissionReport, OutStandingItems, CommissionSummaryReport, OutOfStockLengthReport };
        ReportType rCurrentlyPrinting;
        public enum SalesReportType { AllStock, CatAndStockTotals, StockTotals, CatTotalsAllShops, StockAllShops };
        public enum ReportOrderedBy { CodeAlphabetical, DescAlphabetical, GrossSales, NetSales, Profit, ProfitPercent, QuantitySold, RRP, QIS };
        public enum Period {Daily, Weekly, Monthly, Yearly, Other};
        private class SaleReportItem : IComparable
        {
            public decimal dQuantitySold = 0;
            public decimal dGrossSales = 0;
            public decimal dNetSales = 0;
            public decimal dStockLevel = 0;
            public decimal dProfitAmount = 0;
            public decimal dProfitPercent = 0;
            public string sBarcode = "";
            public string sDescription = "";
            private decimal dCOGS = 0;
            ReportOrderedBy Order;
            public SaleReportItem(string Barcode, ref Table tStockStats, ref Table tMainStock, ref Table tCommission, Period pPeriod, ReportOrderedBy rOrder)
            {
                sBarcode = Barcode;
                string sPeriodCode = "";
                switch (pPeriod)
                {
                    case Period.Daily:
                        sPeriodCode = "D";
                        break;
                    case Period.Weekly:
                        sPeriodCode = "W";
                        break;
                    case Period.Monthly:
                        sPeriodCode = "M";
                        break;
                    case Period.Yearly:
                        sPeriodCode = "Y";
                        break;
                }
                string[] sStockStatsRecord = tStockStats.GetRecordFrom(Barcode, 0, true);
                string[] sMainStockRecord = tMainStock.GetRecordFrom(Barcode, 0, true);
                dQuantitySold = Convert.ToDecimal(sStockStatsRecord[tStockStats.FieldNumber(sPeriodCode + "QSOLD")]);
                dGrossSales = Convert.ToDecimal(sStockStatsRecord[tStockStats.FieldNumber(sPeriodCode + "GSALES")]);
                dNetSales = Convert.ToDecimal(sStockStatsRecord[tStockStats.FieldNumber(sPeriodCode + "NSALES")]);
                dStockLevel = Convert.ToDecimal(sStockStatsRecord[tStockStats.FieldNumber("QIS")]);
                dCOGS = Convert.ToDecimal(sStockStatsRecord[tStockStats.FieldNumber(sPeriodCode + "COGS")]);
                sDescription = sMainStockRecord[tMainStock.FieldNumber("DESCRIPTIO")];
                dProfitAmount = dNetSales - dCOGS;
                if (dCOGS == 0)
                {
                    dProfitPercent = 100;
                }
                else
                {
                    if (dNetSales == 0)
                    {
                        dProfitPercent = -100;
                    }
                    else
                    {
                        dProfitPercent = (100 / dNetSales) * dProfitAmount;
                    }
                }
                Order = rOrder;
            }

            public int CompareTo(object obj)
            {
                SaleReportItem sOtherItem = (SaleReportItem)obj;
                switch (Order)
                {
                    case ReportOrderedBy.CodeAlphabetical:
                        return string.Compare(sBarcode, sOtherItem.sBarcode, true);
                        break;
                    case ReportOrderedBy.DescAlphabetical:
                        return string.Compare(sDescription, sOtherItem.sDescription, true);
                        break;
                    case ReportOrderedBy.GrossSales:
                        if (dGrossSales < sOtherItem.dGrossSales)
                            return 1;
                        else if (dGrossSales == sOtherItem.dGrossSales)
                            return 0;
                        else
                            return -1;
                        break;
                    case ReportOrderedBy.NetSales:
                        if (dNetSales < sOtherItem.dNetSales)
                            return 1;
                        else if (dNetSales == sOtherItem.dNetSales)
                            return 0;
                        else
                            return -1;
                        break;
                    case ReportOrderedBy.Profit:
                        if (dProfitAmount < sOtherItem.dProfitAmount)
                            return 1;
                        else if (dProfitAmount == sOtherItem.dProfitAmount)
                            return 0;
                        else
                            return -1;
                        break;
                    case ReportOrderedBy.ProfitPercent:
                        if (dProfitPercent < sOtherItem.dProfitPercent)
                            return 1;
                        else if (dProfitPercent == sOtherItem.dProfitPercent)
                            return 0;
                        else
                            return -1;
                        break;
                    case ReportOrderedBy.QuantitySold:
                        if (dQuantitySold < sOtherItem.dQuantitySold)
                            return 1;
                        else if (dQuantitySold == sOtherItem.dQuantitySold)
                            return 0;
                        else
                            return -1;
                        break;
                }
                return 0;
            }
        }
        struct SalesItemContainer
        {
            public SaleReportItem[] sItems;
            public string CatCode;
            public string ShopCode;
            public bool AnySoldFromCat;
        }
        public void SalesReportToFile(string sStartCat, string sEndCat, SalesReportType ReportType, Period SalesReportPeriod, ReportOrderedBy rOrder)
        {
            sStartCat = sStartCat.ToUpper();
            sEndCat = sEndCat.ToUpper();
            SalesItemContainer[] sContainer = new SalesItemContainer[tShop.NumberOfRecords * tCategory.NumberOfRecords];
            frmProgressBar frmProgress = new frmProgressBar("Generating Sales Report");
            frmProgress.Show();
            TextWriter tWriter = new StreamWriter("REPORT.TXT");
            tWriter.WriteLine("-------------------------");
            switch (SalesReportPeriod)
            {
                case Period.Daily:
                    string sLastDate = GetLastCollectionDate();
                    tWriter.WriteLine("Daily Sales Report for " + sLastDate[0].ToString()  + sLastDate[1].ToString() + "/" + sLastDate[2].ToString() + sLastDate[3].ToString() + "/" + sLastDate[4].ToString() + sLastDate[5].ToString());
                    break;
                case Period.Weekly:
                    string sWeekComm = GetWeekCommencingDate();
                    tWriter.WriteLine("Weekly Sales Report for the week commencing " + sWeekComm);
                    break;
                case Period.Monthly:
                    tWriter.WriteLine("Monthly Sales Report for " + GetMonthDate());
                    break;
                case Period.Yearly:
                    string sLastCollection = GetLastCollectionDate();
                    string sYear = sLastCollection[4].ToString() + sLastCollection[5].ToString();
                    tWriter.WriteLine("Yearly Sales Report for " + "20" + sYear);
                    break;
            }
            tWriter.WriteLine("-------------------------");
            tWriter.WriteLine("-----------------------------------------------------------------------------------------------------------------------------");
            tWriter.WriteLine("Barcode       Description                    Stock     Quantity  Gross          Net         Profit    Profit(%)  Relative    Relative");
            tWriter.WriteLine("                                             Level     Sold      Sales          Sales                            Profit (%)  Sales (%)");
            tWriter.WriteLine("-----------------------------------------------------------------------------------------------------------------------------");
            bool bFoundCategory = false;
            bool bFinishedAllCats = false;
            frmProgress.pb.Maximum = tShop.NumberOfRecords * tCategory.NumberOfRecords;
            for (int i = 0; i < tShop.NumberOfRecords; i++) // Each Shop
            {
                string sShopStartLine = "Shop : " + tShop.GetRecordFrom(i)[0] + " - " + tShop.GetRecordFrom(i)[1];
                string sDashedLine = "";
                while (sDashedLine.Length < sShopStartLine.Length)
                    sDashedLine += "-";
                tWriter.WriteLine(sDashedLine);
                tWriter.WriteLine(sShopStartLine);
                tWriter.WriteLine(sDashedLine);
                string[] sItemsSoldToday = this.GetListOfSoldBarcodes(tShop.GetRecordFrom(i)[0], SalesReportPeriod);
                for (int q = 0; q < sItemsSoldToday.Length; q++)
                {
                    sItemsSoldToday[q] = sItemsSoldToday[q].ToUpper();
                }
                for (int p = 0; p < Till.Length; p++)
                {
                    if (Till[p].LastCollection != GetLastCollectionDate())
                        tWriter.WriteLine("Till " + Till[p].Number.ToString() + " (from " + GetShopNameFromCode(Till[p].ShopCode) + " shop) hasn't been collected so data from it is not displayed!");
                }
                // Find the shop totals
                #region ShopTotals
                decimal dShopTotalProfitAmount = 0;
                decimal dShopTotalProfitPercent = 0;
                decimal dShopTotalNetAmount = 0;
                decimal dNumOfItemsSold = 0;
                for (int x = 0; x < tCategory.NumberOfRecords; x++) // Each Category
                {
                    decimal dProgress = (((tCategory.NumberOfRecords * i) + x));
                    frmProgress.pb.Value = Convert.ToInt32(dProgress);
                    bool bAnySoldFromCat = false;
                    string[] sCatRec = tCategory.GetRecordFrom(x);
                    SaleReportItem[] sReportItems = new SaleReportItem[0];
                    if (!bFoundCategory && sCatRec[0] == sStartCat)
                        bFoundCategory = true;
                    if (bFoundCategory && !bFinishedAllCats)
                    {
                        int nOfRecords = 0;
                        // Get the barcodes of all the items in this category
                        string[] sItems = tStock.SearchAndGetAllMatchingRecords(4, sCatRec[0], ref nOfRecords, true,0);
                        for (int y = 0; y < nOfRecords; y++)
                        {
                            // Make all the barcodes uppercase
                            sItems[y] = sItems[y].ToUpper();
                            bool bSoldToday = false;
                            for (int t = 0; t < sItemsSoldToday.Length; t++)
                            {
                                if (sItemsSoldToday[t] == sItems[y])
                                {
                                    bSoldToday = true;
                                    break;
                                }
                            }
                            if (bSoldToday)
                            {
                                bAnySoldFromCat = true;
                                string[] sStockStaInfo = tStockStats.GetRecordFrom(sItems[y], 0, true);
                                string[] sMainStockInfo = tStock.GetRecordFrom(sItems[y], 0, true);
                                Array.Resize<SaleReportItem>(ref sReportItems, sReportItems.Length + 1);
                                sReportItems[sReportItems.Length - 1] = new SaleReportItem(sItems[y], ref tStockStats, ref tStock, ref tCommItems, SalesReportPeriod, rOrder);
                            }
                        }

                        for (int t = 0; t < sReportItems.Length; t++)
                        {
                            dShopTotalNetAmount += sReportItems[t].dNetSales;
                            dShopTotalProfitAmount += sReportItems[t].dProfitAmount;
                            dShopTotalProfitPercent += sReportItems[t].dProfitPercent;
                            dNumOfItemsSold += sReportItems[t].dQuantitySold;
                        }
                    }
                    else
                    {
                        continue;
                    }
                    if (sCatRec[0] == sEndCat)
                        bFinishedAllCats = true;
                    sContainer[(tShop.NumberOfRecords * i) + x] = new SalesItemContainer();
                    sContainer[(tShop.NumberOfRecords * i) + x].CatCode = sCatRec[0];
                    sContainer[(tShop.NumberOfRecords * i) + x].ShopCode = tShop.GetRecordFrom(i)[0];
                    sContainer[(tShop.NumberOfRecords * i) + x].sItems = sReportItems;
                    sContainer[(tShop.NumberOfRecords * i) + x].AnySoldFromCat = bAnySoldFromCat;
                }
                #endregion
                decimal dShopQtyTotal = 0;
                decimal dShopGrossTotal = 0;
                decimal dShopNetTotal = 0;
                decimal dShopProfitTotal = 0;
                int nOfCategories = 0;
                bFinishedAllCats = false;
                #region EachCat
                for (int x = 0; x < tCategory.NumberOfRecords; x++) // Each Category
                {
                    string[] sCatRec = tCategory.GetRecordFrom(x);
                    //decimal dProgress = (((tCategory.NumberOfRecords * i) + x) / 2) + (tCategory.NumberOfRecords / 2);
                    //frmProgress.pb.Value = Convert.ToInt32(dProgress);
                    SaleReportItem[] sReportItems = new SaleReportItem[0];
                    bool bAnySoldFromCat = false;
                    
                    for (int z = 0; z < sContainer.Length; z++)
                    {
                        if (sContainer[z].CatCode == sCatRec[0] && sContainer[z].ShopCode == tShop.GetRecordFrom(i)[0])
                        {
                            sReportItems = sContainer[z].sItems;
                            bAnySoldFromCat = sContainer[z].AnySoldFromCat;
                        }
                    }
                    
                    if (!bFoundCategory && sCatRec[0] == sStartCat)
                        bFoundCategory = true;
                    if (bFoundCategory && !bFinishedAllCats)
                    {
                        nOfCategories++;
                        decimal dSoldTotal = 0;
                        decimal dGrossTotal = 0;
                        decimal dNetTotal = 0;
                        decimal dProfitTotal = 0;
                        decimal dProfitPercentTotal = 0;
                        /*
                        int nOfRecords = 0;
                        string[] sItems = tStock.SearchAndGetAllMatchingRecords(4, sCatRec[0], ref nOfRecords, true,0);
                        for (int y = 0; y < nOfRecords; y++)
                        {
                            bool bSoldToday = false;
                            for (int t = 0; t < sItemsSoldToday.Length; t++)
                            {
                                if (sItemsSoldToday[t] == sItems[y])
                                {
                                    bSoldToday = true;
                                    break;
                                }
                            }
                            if (bSoldToday)
                            {
                                bAnySoldFromCat = true;
                                string[] sStockStaInfo = tStockStats.GetRecordFrom(sItems[y], 0, true);
                                string[] sMainStockInfo = tStock.GetRecordFrom(sItems[y], 0, true);
                                Array.Resize<SaleReportItem>(ref sReportItems, sReportItems.Length + 1);
                                sReportItems[sReportItems.Length - 1] = new SaleReportItem(sItems[y], ref tStockStats, ref tStock, ref tCommItems, SalesReportPeriod, rOrder);
                            }
                        }
                    */
                        Array.Sort(sReportItems);
                        decimal dTotalProfitAmount = 0;
                        decimal dTotalGrossAmount = 0;
                        decimal dTotalNetAmount = 0;
                        decimal dTotalProfitPercent = 0;
                        for (int t = 0; t < sReportItems.Length; t++)
                        {
                            dTotalProfitAmount += sReportItems[t].dProfitAmount;
                            dTotalGrossAmount += sReportItems[t].dGrossSales;
                            dTotalNetAmount += sReportItems[t].dNetSales;
                            dTotalProfitPercent += sReportItems[t].dProfitPercent;
                        }
                        for (int t = 0; t < sReportItems.Length; t++)
                        {
                            string sBarcode = sReportItems[t].sBarcode;
                            while (sBarcode.Length < 13)
                                sBarcode += " ";
                            string sDescription = sReportItems[t].sDescription;
                            while (sDescription.Length < 30)
                                sDescription += " ";
                            string sStockLevel = FormatMoneyForDisplay(sReportItems[t].dStockLevel);
                            while (sStockLevel.Length < 10)
                                sStockLevel = " " + sStockLevel;
                            string sQuantitySold = FormatMoneyForDisplay(sReportItems[t].dQuantitySold);
                            while (sQuantitySold.Length < 8)
                                sQuantitySold = " " + sQuantitySold;
                            string sGrossSales = FormatMoneyForDisplay(sReportItems[t].dGrossSales);
                            while (sGrossSales.Length < 9)
                                sGrossSales = " " + sGrossSales;
                            string sNetSales = FormatMoneyForDisplay(sReportItems[t].dNetSales);
                            while (sNetSales.Length < 11)
                                sNetSales = " " + sNetSales;
                            string sProfit = FormatMoneyForDisplay(sReportItems[t].dProfitAmount);
                            while (sProfit.Length < 12)
                                sProfit = " " + sProfit;
                            string sProfitPercent = FormatMoneyForDisplay(sReportItems[t].dProfitPercent);
                            while (sProfitPercent.Length < 9)
                                sProfitPercent = " " + sProfitPercent;
                            // Work out relative amounts
                            decimal dProfitRelativePercent = 0;
                            if (dTotalProfitAmount != 0)
                                dProfitRelativePercent = (100 / dTotalProfitAmount) * sReportItems[t].dProfitAmount;
                            string sProfitRelative = FormatMoneyForDisplay(dProfitRelativePercent);
                            while (sProfitRelative.Length < 10)
                                sProfitRelative = " " + sProfitRelative;
                            decimal dAmountRelativePercent = 0;
                            if (dTotalNetAmount != 0)
                                dAmountRelativePercent = (100 / dTotalNetAmount) * sReportItems[t].dNetSales;
                            string sAmountRelative = FormatMoneyForDisplay(dAmountRelativePercent);
                            while (sAmountRelative.Length < 9)
                                sAmountRelative = " " + sAmountRelative;
                            dSoldTotal += sReportItems[t].dQuantitySold;
                            dGrossTotal += sReportItems[t].dGrossSales;
                            dNetTotal += sReportItems[t].dNetSales;
                            dProfitTotal += sReportItems[t].dProfitAmount;
                            dShopGrossTotal += sReportItems[t].dGrossSales;
                            dShopQtyTotal += sReportItems[t].dQuantitySold;
                            dShopNetTotal += sReportItems[t].dNetSales;
                            dShopProfitTotal += sReportItems[t].dProfitAmount;
                            dProfitPercentTotal += sReportItems[t].dProfitPercent;
                            if (ReportType == SalesReportType.AllStock)
                                tWriter.WriteLine(sBarcode + " " + sDescription + " " + sStockLevel + "  " + sQuantitySold + "   " + sGrossSales + " " + sNetSales + " " + sProfit + " " + sProfitPercent + "  " + sProfitRelative + "  " + sAmountRelative);
                            
                        }
                        if (bAnySoldFromCat)
                        {
                            string sLineAdd = "";
                            tWriter.WriteLine("-----------------------------------------------------------------------------------------------------------------------------");
                            string sEndOfCatLine = sCatRec[1];
                            if (ReportType == SalesReportType.AllStock)
                                sEndOfCatLine = "Totals For " + sEndOfCatLine;
                            while (sEndOfCatLine.Length < 55)
                                sEndOfCatLine += " ";
                            sLineAdd = FormatMoneyForDisplay(dSoldTotal);
                            while (sEndOfCatLine.Length + sLineAdd.Length < 65)
                                sEndOfCatLine += " ";
                            sEndOfCatLine += sLineAdd;
                            sLineAdd = FormatMoneyForDisplay(dGrossTotal);
                            while (sEndOfCatLine.Length + sLineAdd.Length < 77)
                                sEndOfCatLine += " ";
                            sEndOfCatLine += sLineAdd;
                            sLineAdd = FormatMoneyForDisplay(dNetTotal);
                            while (sEndOfCatLine.Length + sLineAdd.Length < 89)
                                sEndOfCatLine += " ";
                            sEndOfCatLine += sLineAdd;
                            sLineAdd = FormatMoneyForDisplay(dProfitTotal);
                            while (sEndOfCatLine.Length + sLineAdd.Length < 102)
                                sEndOfCatLine += " ";
                            sEndOfCatLine += sLineAdd;
                            // Average Profit
                            string sAverageProfit = "0.00";
                            if (dNetTotal != 0)
                                sAverageProfit = FormatMoneyForDisplay((100 / dNetTotal) * dProfitTotal);
                            while (sAverageProfit.Length < 9)
                                sAverageProfit = " " + sAverageProfit;
                            sEndOfCatLine += " " + sAverageProfit;
                            // Relative Totals
                            string sProfitRelative = "";
                            if (dShopTotalProfitAmount != 0)
                                sProfitRelative = FormatMoneyForDisplay((100 / dShopTotalProfitAmount) * dTotalProfitAmount);
                            else
                                sProfitRelative = "0";
                            while (sProfitRelative.Length < 10)
                                sProfitRelative = " " + sProfitRelative;
                            string sAmountRelative = "";
                            if (dShopTotalNetAmount != 0)
                                sAmountRelative = FormatMoneyForDisplay((100 / dShopTotalNetAmount) * dTotalNetAmount);
                            while (sAmountRelative.Length < 9)
                                sAmountRelative = " " + sAmountRelative;
                            sEndOfCatLine += "  " + sProfitRelative + "  " + sAmountRelative;
                            tWriter.WriteLine(sEndOfCatLine);
                            tWriter.WriteLine("-----------------------------------------------------------------------------------------------------------------------------");
                            tWriter.WriteLine();
                        }
                        if (sCatRec[0] == sEndCat)
                            bFinishedAllCats = true;
                    }
                    else
                    {
                        continue;
                    }
                }
                #endregion
                tWriter.WriteLine("-----------------------------------------------------------------------------------------------------------------------------");
                string sEndOfShopLine = "Totals For Shop " + tShop.GetRecordFrom(i)[0] + " - " + tShop.GetRecordFrom(i)[1];
                while (sEndOfShopLine.Length < 55)
                    sEndOfShopLine += " ";
                while (sEndOfShopLine.Length + FormatMoneyForDisplay(dShopQtyTotal).Length < 65)
                    sEndOfShopLine += " ";
                sEndOfShopLine += FormatMoneyForDisplay(dShopQtyTotal);
                while (sEndOfShopLine.Length + FormatMoneyForDisplay(dShopGrossTotal).Length < 77)
                    sEndOfShopLine += " ";
                sEndOfShopLine += FormatMoneyForDisplay(dShopGrossTotal);
                while (sEndOfShopLine.Length + FormatMoneyForDisplay(dShopNetTotal).Length < 89)
                    sEndOfShopLine += " ";
                sEndOfShopLine += FormatMoneyForDisplay(dShopNetTotal);
                while (sEndOfShopLine.Length + FormatMoneyForDisplay(dShopProfitTotal).Length < 102)
                    sEndOfShopLine += " ";
                sEndOfShopLine += FormatMoneyForDisplay(dShopProfitTotal);
                if (dNumOfItemsSold > 0)
                {
                    while (sEndOfShopLine.Length + FormatMoneyForDisplay((100 / dShopNetTotal) * dShopProfitTotal).Length < 112)
                        sEndOfShopLine += " ";
                    sEndOfShopLine += FormatMoneyForDisplay((100 / dShopNetTotal) * dShopProfitTotal);
                }
                else
                {
                    while (sEndOfShopLine.Length < 112)
                        sEndOfShopLine += " ";
                }
                tWriter.WriteLine(sEndOfShopLine);
                tWriter.WriteLine("-----------------------------------------------------------------------------------------------------------------------------");
                tWriter.WriteLine();
            }
            tWriter.Close();
            frmProgress.Close();
        }
        public void SalesReportToPrinter(string sStartCat, string sEndCat, SalesReportType SReportType, Period SalesReportPeriod, ReportOrderedBy rOrder)
        {
            nLineLastPrinted = 7;
            nPrinterPage = 1;
            string sDate = GetLastCollectionDate();
            string sTitleDate = sDate[0].ToString() + sDate[1].ToString() + "/" + sDate[2].ToString() + sDate[3].ToString() + "/" + sDate[4].ToString() + sDate[5].ToString();
            switch (SalesReportPeriod)
            {
                case Period.Daily:
                    sReportTitle = "Daily Sales Report for " + sTitleDate;
                    break;
                case Period.Monthly:
                    sReportTitle = "Monthly Sales Report for " + GetMonthDate();
                    break;
                case Period.Weekly:
                    sReportTitle = "Weekly Sales Report for Week Commencing " + GetWeekCommencingDate();
                    break;
                case Period.Yearly:

                    string sLastCollection = GetLastCollectionDate();
                    string sYear = sLastCollection[4].ToString() + sLastCollection[5].ToString();
                    sReportTitle = "Yearly Sales Report for " + "20" + sYear;
                    break;
            }
            rCurrentlyPrinting = ReportType.SalesReport;
            SalesReportToFile(sStartCat, sEndCat, SReportType, SalesReportPeriod, rOrder);
            PrinterSettings pSettings = new PrinterSettings();
            pSettings.PrinterName = this.PrinterToUse;
            PrintDocument pPrinter = new PrintDocument();
            pPrinter.PrinterSettings = pSettings;
            pPrinter.DocumentName = "Daily Sales Report";
            pPrinter.PrintPage += new PrintPageEventHandler(ReportPrintPage);
            pPrinter.Print();
        }
        public void SalesReportToSpreadsheet(string sStartCat, string sEndCat, SalesReportType ReportType, Period SalesReportPeriod, ReportOrderedBy rOrder)
        {
            SalesItemContainer[] sContainer = new SalesItemContainer[tShop.NumberOfRecords * tCategory.NumberOfRecords];
            frmProgressBar frmProgress = new frmProgressBar("Generating Sales Spreadsheet");
            frmProgress.Show();
            TextWriter tWriter = new StreamWriter("REPORT.CSV");
            switch (SalesReportPeriod)
            {
                case Period.Daily:
                    string sLastDate = GetLastCollectionDate();
                    tWriter.WriteLine("Daily Sales Report for " + sLastDate[0].ToString() + sLastDate[1].ToString() + "/" + sLastDate[2].ToString() + sLastDate[3].ToString() + "/" + sLastDate[4].ToString() + sLastDate[5].ToString());
                    break;
                case Period.Weekly:
                    string sWeekComm = GetWeekCommencingDate();
                    tWriter.WriteLine("Weekly Sales Report for the week commencing " + sWeekComm);
                    break;
                case Period.Monthly:
                    tWriter.WriteLine("Monthly Sales Report for " + GetMonthDate());
                    break;
                case Period.Yearly:
                    string sLastCollection = GetLastCollectionDate();
                    string sYear = sLastCollection[4].ToString() + sLastCollection[5].ToString();
                    tWriter.WriteLine("Yearly Sales Report for " + "20" + sYear);
                    break;
            }tWriter.WriteLine("Barcode,Description,Stock Level,Quantity Sold,Gross Sales,Net Sales,Profit,Profit(%),Relative Profit(%),Relative Sales(%)");
            bool bFoundCategory = false;
            bool bFinishedAllCats = false;
            frmProgress.pb.Maximum = tShop.NumberOfRecords * tCategory.NumberOfRecords;
            for (int i = 0; i < tShop.NumberOfRecords; i++) // Each Shop
            {
                string sShopStartLine = "Shop : " + tShop.GetRecordFrom(i)[0] + " - " + tShop.GetRecordFrom(i)[1];
                tWriter.WriteLine(sShopStartLine);
                string[] sItemsSoldToday = this.GetListOfSoldBarcodes(tShop.GetRecordFrom(i)[0], SalesReportPeriod);
                for (int p = 0; p < Till.Length; p++)
                {
                    if (Till[p].LastCollection != GetLastCollectionDate())
                        tWriter.WriteLine("Till " + Till[p].Number.ToString() + " (from " + GetShopNameFromCode(Till[p].ShopCode) + " shop) hasn't been collected so data from it is not displayed!");
                }
                // Find the shop totals
                #region ShopTotals
                decimal dShopTotalProfitAmount = 0;
                decimal dShopTotalProfitPercent = 0;
                decimal dShopTotalNetAmount = 0;
                decimal dNumOfItemsSold = 0;
                for (int x = 0; x < tCategory.NumberOfRecords; x++) // Each Category
                {
                    decimal dProgress = (((tCategory.NumberOfRecords * i) + x));
                    frmProgress.pb.Value = Convert.ToInt32(dProgress);
                    bool bAnySoldFromCat = false;
                    string[] sCatRec = tCategory.GetRecordFrom(x);
                    SaleReportItem[] sReportItems = new SaleReportItem[0];
                    if (!bFoundCategory && sCatRec[0] == sStartCat)
                        bFoundCategory = true;
                    if (bFoundCategory && !bFinishedAllCats)
                    {
                        int nOfRecords = 0;
                        string[] sItems = tStock.SearchAndGetAllMatchingRecords(4, sCatRec[0], ref nOfRecords, true, 0);
                        for (int y = 0; y < nOfRecords; y++)
                        {
                            bool bSoldToday = false;
                            for (int t = 0; t < sItemsSoldToday.Length; t++)
                            {
                                if (sItemsSoldToday[t] == sItems[y])
                                {
                                    bSoldToday = true;
                                    break;
                                }
                            }
                            if (bSoldToday)
                            {
                                bAnySoldFromCat = true;
                                string[] sStockStaInfo = tStockStats.GetRecordFrom(sItems[y], 0, true);
                                string[] sMainStockInfo = tStock.GetRecordFrom(sItems[y], 0, true);
                                Array.Resize<SaleReportItem>(ref sReportItems, sReportItems.Length + 1);
                                sReportItems[sReportItems.Length - 1] = new SaleReportItem(sItems[y], ref tStockStats, ref tStock, ref tCommItems, SalesReportPeriod, rOrder);
                            }
                        }

                        for (int t = 0; t < sReportItems.Length; t++)
                        {
                            dShopTotalNetAmount += sReportItems[t].dNetSales;
                            dShopTotalProfitAmount += sReportItems[t].dProfitAmount;
                            dShopTotalProfitPercent += sReportItems[t].dProfitPercent;
                            dNumOfItemsSold += sReportItems[t].dQuantitySold;
                        }
                    }
                    else
                    {
                        continue;
                    }
                    if (sCatRec[0] == sEndCat)
                        bFinishedAllCats = true;
                    sContainer[(tShop.NumberOfRecords * i) + x] = new SalesItemContainer();
                    sContainer[(tShop.NumberOfRecords * i) + x].CatCode = sCatRec[0];
                    sContainer[(tShop.NumberOfRecords * i) + x].ShopCode = tShop.GetRecordFrom(i)[0];
                    sContainer[(tShop.NumberOfRecords * i) + x].sItems = sReportItems;
                    sContainer[(tShop.NumberOfRecords * i) + x].AnySoldFromCat = bAnySoldFromCat;
                }
                #endregion
                decimal dShopQtyTotal = 0;
                decimal dShopGrossTotal = 0;
                decimal dShopNetTotal = 0;
                decimal dShopProfitTotal = 0;
                int nOfCategories = 0;
                bFinishedAllCats = false;
                #region EachCat
                for (int x = 0; x < tCategory.NumberOfRecords; x++) // Each Category
                {
                    string[] sCatRec = tCategory.GetRecordFrom(x);
                    //decimal dProgress = (((tCategory.NumberOfRecords * i) + x) / 2) + (tCategory.NumberOfRecords / 2);
                    //frmProgress.pb.Value = Convert.ToInt32(dProgress);
                    SaleReportItem[] sReportItems = new SaleReportItem[0];
                    bool bAnySoldFromCat = false;

                    for (int z = 0; z < sContainer.Length; z++)
                    {
                        if (sContainer[z].CatCode == sCatRec[0] && sContainer[z].ShopCode == tShop.GetRecordFrom(i)[0])
                        {
                            sReportItems = sContainer[z].sItems;
                            bAnySoldFromCat = sContainer[z].AnySoldFromCat;
                        }
                    }

                    if (!bFoundCategory && sCatRec[0] == sStartCat)
                        bFoundCategory = true;
                    if (bFoundCategory && !bFinishedAllCats)
                    {
                        nOfCategories++;
                        decimal dSoldTotal = 0;
                        decimal dGrossTotal = 0;
                        decimal dNetTotal = 0;
                        decimal dProfitTotal = 0;
                        decimal dProfitPercentTotal = 0;
                        /*
                        int nOfRecords = 0;
                        string[] sItems = tStock.SearchAndGetAllMatchingRecords(4, sCatRec[0], ref nOfRecords, true,0);
                        for (int y = 0; y < nOfRecords; y++)
                        {
                            bool bSoldToday = false;
                            for (int t = 0; t < sItemsSoldToday.Length; t++)
                            {
                                if (sItemsSoldToday[t] == sItems[y])
                                {
                                    bSoldToday = true;
                                    break;
                                }
                            }
                            if (bSoldToday)
                            {
                                bAnySoldFromCat = true;
                                string[] sStockStaInfo = tStockStats.GetRecordFrom(sItems[y], 0, true);
                                string[] sMainStockInfo = tStock.GetRecordFrom(sItems[y], 0, true);
                                Array.Resize<SaleReportItem>(ref sReportItems, sReportItems.Length + 1);
                                sReportItems[sReportItems.Length - 1] = new SaleReportItem(sItems[y], ref tStockStats, ref tStock, ref tCommItems, SalesReportPeriod, rOrder);
                            }
                        }
                    */
                        Array.Sort(sReportItems);
                        decimal dTotalProfitAmount = 0;
                        decimal dTotalGrossAmount = 0;
                        decimal dTotalNetAmount = 0;
                        decimal dTotalProfitPercent = 0;
                        for (int t = 0; t < sReportItems.Length; t++)
                        {
                            dTotalProfitAmount += sReportItems[t].dProfitAmount;
                            dTotalGrossAmount += sReportItems[t].dGrossSales;
                            dTotalNetAmount += sReportItems[t].dNetSales;
                            dTotalProfitPercent += sReportItems[t].dProfitPercent;
                        }
                        for (int t = 0; t < sReportItems.Length; t++)
                        {
                            string sBarcode = "\"" + sReportItems[t].sBarcode + "\"";
                            string sDescription = "\"" + sReportItems[t].sDescription + "\"";
                            string sStockLevel = FormatMoneyForDisplay(sReportItems[t].dStockLevel);
                            string sQuantitySold = FormatMoneyForDisplay(sReportItems[t].dQuantitySold);
                            string sGrossSales = FormatMoneyForDisplay(sReportItems[t].dGrossSales);
                            string sNetSales = FormatMoneyForDisplay(sReportItems[t].dNetSales);
                            string sProfit = FormatMoneyForDisplay(sReportItems[t].dProfitAmount);
                            string sProfitPercent = FormatMoneyForDisplay(sReportItems[t].dProfitPercent);
                            // Work out relative amounts
                            decimal dProfitRelativePercent = 0;
                            if (dTotalProfitAmount != 0)
                                dProfitRelativePercent = (100 / dTotalProfitAmount) * sReportItems[t].dProfitAmount;
                            string sProfitRelative = FormatMoneyForDisplay(dProfitRelativePercent);
                            decimal dAmountRelativePercent = 0;
                            if (dTotalNetAmount != 0)
                                dAmountRelativePercent = (100 / dTotalNetAmount) * sReportItems[t].dNetSales;
                            string sAmountRelative = FormatMoneyForDisplay(dAmountRelativePercent);
                            dSoldTotal += sReportItems[t].dQuantitySold;
                            dGrossTotal += sReportItems[t].dGrossSales;
                            dNetTotal += sReportItems[t].dNetSales;
                            dProfitTotal += sReportItems[t].dProfitAmount;
                            dShopGrossTotal += sReportItems[t].dGrossSales;
                            dShopQtyTotal += sReportItems[t].dQuantitySold;
                            dShopNetTotal += sReportItems[t].dNetSales;
                            dShopProfitTotal += sReportItems[t].dProfitAmount;
                            dProfitPercentTotal += sReportItems[t].dProfitPercent;
                            if (ReportType == SalesReportType.AllStock)
                                tWriter.WriteLine(sBarcode + "," + sDescription + "," + sStockLevel + "," + sQuantitySold + "," + sGrossSales + "," + sNetSales + "," + sProfit + "," + sProfitPercent + "," + sProfitRelative + "," + sAmountRelative);

                        }
                        if (bAnySoldFromCat)
                        {
                            string sLineAdd = "";
                            string sEndOfCatLine = sCatRec[1];
                            if (ReportType == SalesReportType.AllStock)
                                sEndOfCatLine = "\"Totals For " + sEndOfCatLine + "\"";
                            sLineAdd = ",," + FormatMoneyForDisplay(dSoldTotal);
                            sEndOfCatLine += "," + sLineAdd;
                            sLineAdd = FormatMoneyForDisplay(dGrossTotal);
                            sEndOfCatLine += "," + sLineAdd;
                            sLineAdd = FormatMoneyForDisplay(dNetTotal);
                            sEndOfCatLine += "," + sLineAdd;
                            sLineAdd = FormatMoneyForDisplay(dProfitTotal);
                            sEndOfCatLine += "," + sLineAdd;
                            // Average Profit
                            string sAverageProfit = "0.00";
                            if (dNetTotal != 0)
                                sAverageProfit = FormatMoneyForDisplay((100 / dNetTotal) * dProfitTotal);
                            sEndOfCatLine += "," + sAverageProfit;
                            // Relative Totals
                            string sProfitRelative = FormatMoneyForDisplay((100 / dShopTotalProfitAmount) * dTotalProfitAmount);
                            string sAmountRelative = FormatMoneyForDisplay((100 / dShopTotalNetAmount) * dTotalNetAmount);
                            sEndOfCatLine += "," + sProfitRelative + "," + sAmountRelative;
                            tWriter.WriteLine(sEndOfCatLine);
                            tWriter.WriteLine();
                        }
                        if (sCatRec[0] == sEndCat)
                            bFinishedAllCats = true;
                    }
                    else
                    {
                        continue;
                    }
                }
                #endregion
                string sEndOfShopLine = "Totals For Shop " + tShop.GetRecordFrom(i)[0] + " - " + tShop.GetRecordFrom(i)[1] + ",,";
                sEndOfShopLine += "," + FormatMoneyForDisplay(dShopQtyTotal);
                sEndOfShopLine += "," + FormatMoneyForDisplay(dShopGrossTotal);
                sEndOfShopLine += "," + FormatMoneyForDisplay(dShopNetTotal);
                sEndOfShopLine += "," + FormatMoneyForDisplay(dShopProfitTotal);
                if (dNumOfItemsSold > 0)
                {
                    sEndOfShopLine += "," + FormatMoneyForDisplay((100 / dShopNetTotal) * dShopProfitTotal);
                }
                tWriter.WriteLine(sEndOfShopLine);
            }
            tWriter.Close();
            frmProgress.Close();
            try
            {
                System.Diagnostics.Process.Start("EXCEL", "REPORT.CSV");
            }
            catch
            {
                ;
            }
        }
        
        public class StockReportItem : IComparable
        {
            public decimal[] dStockLvls;
            public decimal dRRP;
            public decimal dAveSales;
            public string sBarcode;
            public string sDescription;
            ReportOrderedBy rOrder;

            public decimal[] dStockLevels
            {
                get
                {
                    return dStockLvls;
                }
                set
                {
                    dStockLvls = value;
                }
            }

            public bool AllZero
            {
                get
                {
                    bool bAllZero = true;
                    for (int i = 0; i < dStockLevels.Length; i++)
                    {
                        if (dStockLevels[i] != 0)
                            bAllZero = false;
                    }
                    return bAllZero;
                }
            }

            public StockReportItem(ReportOrderedBy r)
            {
                rOrder = r;
            }

            public int CompareTo(object obj)
            {
                StockReportItem sOtherItem = (StockReportItem)obj;
                switch (rOrder)
                {
                    case ReportOrderedBy.QIS:
                        decimal dMaxThis = dStockLevels[0];
                        for (int i = 0; i < dStockLevels.Length; i++)
                        {
                            if (dMaxThis < dStockLevels[i])
                                dMaxThis = dStockLevels[i];
                        }
                        decimal dMaxOther = sOtherItem.dStockLevels[0];
                        for (int i = 0; i < sOtherItem.dStockLevels.Length; i++)
                        {
                            if (dMaxOther < sOtherItem.dStockLevels[i])
                                dMaxOther = sOtherItem.dStockLevels[i];
                        }
                        if (dMaxThis < dMaxOther)
                            return 1;
                        else if (dMaxOther == dMaxThis)
                            return 0;
                        else
                            return -1;
                        break;
                    case ReportOrderedBy.CodeAlphabetical:
                        return String.Compare(sBarcode, sOtherItem.sBarcode, true);
                        break;
                    case ReportOrderedBy.DescAlphabetical:
                        return String.Compare(sDescription, sOtherItem.sDescription, true);
                        break;
                }
                return 0;
            }
        }
        public void StockReportToFile(string[] sCategories, ReportOrderedBy rOrder, string sCatGroupName, bool bIncludeZeroItems)
        {
            for (int i = 0; i < sCategories.Length; i++)
            {
                sCategories[i] = sCategories[i].ToUpper();
            }
            frmProgressBar fProgress = new frmProgressBar("Generating Stock Report");
            fProgress.Show();
            TextWriter tWriter = new StreamWriter("REPORT.TXT");
            string sTitle = "Stock Level Report For ";
            if (sCategories.Length == 1)
                sTitle += GetCategoryDesc(sCategories[0]);
            else
                sTitle += sCatGroupName;
            tWriter.WriteLine("------------------------------------------------");
            tWriter.WriteLine(sTitle);
            tWriter.WriteLine("------------------------------------------------");
            string sPageHeader = "Barcode       Description                    ";
            string[] sShopCodes = GetListOfShopCodes();
            for (int i = 0; i < tShop.NumberOfRecords; i++)
            {
                string sShopName = tShop.GetRecordFrom(i)[1];
                while (sShopName.Length < 11)
                    sShopName = " " + sShopName;
                sPageHeader += sShopName;
            }
            sPageHeader += "  RRP       Ave/Sales";
            string sBreaker = "-";
            while (sBreaker.Length < sPageHeader.Length)
                sBreaker += "-";
            tWriter.WriteLine(sBreaker);
            tWriter.WriteLine(sPageHeader);
            tWriter.WriteLine(sBreaker);
            fProgress.pb.Maximum = sCategories.Length * tStock.NumberOfRecords;
            for (int z = 0; z < sCategories.Length; z++)
            {
                tWriter.WriteLine("--------------------");
                tWriter.WriteLine(GetCategoryDesc(sCategories[z]));
                tWriter.WriteLine("--------------------");
                StockReportItem[] sItems = new StockReportItem[0];
                string[] sMainstockData;
                for (int x = 0; x < tStock.NumberOfRecords; x++)
                {
                    fProgress.pb.Value = (z * tStock.NumberOfRecords) + x;
                    sMainstockData = tStock.GetRecordFrom(x);
                    if (sMainstockData[tStock.FieldNumber("CATEGORY")].StartsWith(sCategories[z]))
                    {
                        Array.Resize<StockReportItem>(ref sItems, sItems.Length + 1);
                        sItems[sItems.Length - 1] = new StockReportItem(rOrder);
                        sItems[sItems.Length - 1].dStockLevels = new decimal[sShopCodes.Length];
                        sItems[sItems.Length - 1].sBarcode = sMainstockData[0];
                        sItems[sItems.Length - 1].sDescription = sMainstockData[1];
                        sItems[sItems.Length - 1].dRRP = Convert.ToDecimal(sMainstockData[2]);
                        int nOfResults = 0;
                        string[,] sResults = tStockStats.SearchAndGetAllMatchingRecords(0, sMainstockData[0], ref nOfResults, true);
                        for (int i = 0; i < nOfResults; i++)
                        {
                            int nShopNum = 0;
                            for (int p = 0; p < sShopCodes.Length; p++)
                            {
                                if (sResults[i, tStockStats.FieldNumber("SHOPCODE")] == sShopCodes[p])
                                {
                                    nShopNum = p;
                                    break;
                                }
                            }
                            sItems[sItems.Length - 1].dStockLevels[nShopNum] = Convert.ToDecimal(sResults[i, tStockStats.FieldNumber("QIS")]);
                            
                            sItems[sItems.Length - 1].dAveSales = Convert.ToDecimal(sResults[i, 2]);
                        }
                    }
                }
                Array.Sort(sItems);
                for (int i = 0; i < sItems.Length; i++)
                {
                    string sBarcode = sItems[i].sBarcode;
                    while (sBarcode.Length < 13)
                        sBarcode += " ";
                    string sDesc = sItems[i].sDescription;
                    while (sDesc.Length < 30)
                        sDesc += " ";
                    string[] sStockLevels = new string[sShopCodes.Length];
                    for (int x = 0; x < sShopCodes.Length; x++)
                    {
                        sStockLevels[x] = FormatMoneyForDisplay(sItems[i].dStockLevels[x]);
                        while (sStockLevels[x].Length < 10)
                            sStockLevels[x] = " " + sStockLevels[x];
                    }
                    string sLine = sBarcode + " " + sDesc + " ";
                    for (int x = 0; x < sShopCodes.Length; x++)
                    {
                        sLine += sStockLevels[x] + " ";
                    }
                    int nLength = sLine.Length;
                    sLine += "  " + FormatMoneyForDisplay(sItems[i].dRRP);
                    while (sLine.Length - nLength < 12)
                        sLine += " ";
                    sLine += sItems[i].dAveSales.ToString();
                   
                    if (bIncludeZeroItems || (!bIncludeZeroItems && !sItems[i].AllZero))
                        tWriter.WriteLine(sLine);
                }
                tWriter.WriteLine("");
            }
            fProgress.Close();
            tWriter.Close();
        }
        public void StockReportToPrinter(string[] sCategories,ReportOrderedBy rOrder, string sCatGroupName, bool bIncludeZeroItems)
        {
            nLineLastPrinted = 6;
            nPrinterPage = 1;
            if (sCategories.Length == 1)
            {
                sReportTitle = "Stock Level Report For " + GetCategoryDesc(sCategories[0]);
            }
            else
            {
                sReportTitle = "Stock Level Report For " + sCatGroupName;
            }
            rCurrentlyPrinting = ReportType.StockLevelReport;
            StockReportToFile(sCategories, rOrder, sCatGroupName, bIncludeZeroItems);
            PrinterSettings pSettings = new PrinterSettings();
            pSettings.PrinterName = this.PrinterToUse;
            PrintDocument pPrinter = new PrintDocument();
            pPrinter.PrinterSettings = pSettings;
            pPrinter.DocumentName = "Stock Level Report";
            pPrinter.PrintPage += new PrintPageEventHandler(ReportPrintPage);
            pPrinter.Print();
        }

        public void StockReportToPrinter(string[] sBarcodes, ReportOrderedBy rOrder, bool bIncludeZeroItems)
        {
            nLineLastPrinted = 6;
            nPrinterPage = 1;
            sReportTitle = "Stock Level Report";
            rCurrentlyPrinting = ReportType.StockLevelReport;
            StockReportToFile(sBarcodes, rOrder, bIncludeZeroItems);
            PrinterSettings pSettings = new PrinterSettings();
            pSettings.PrinterName = this.PrinterToUse;
            PrintDocument pPrinter = new PrintDocument();
            pPrinter.PrinterSettings = pSettings;
            pPrinter.DocumentName = "Stock Level Report";
            pPrinter.PrintPage += new PrintPageEventHandler(ReportPrintPage);
            pPrinter.Print();
        }

        public void StockReportToFile(string[] sBarcodes, ReportOrderedBy rOrder, bool bIncludeZeroItems)
        {
            frmProgressBar fProgress = new frmProgressBar("Generating Stock Report");
            fProgress.Show();
            TextWriter tWriter = new StreamWriter("REPORT.TXT");
            string sTitle = "Stock Level Report";
            tWriter.WriteLine("------------------------------------------------");
            tWriter.WriteLine(sTitle);
            tWriter.WriteLine("------------------------------------------------");
            string sPageHeader = "Barcode       Description                   ";
            string[] sShopCodes = GetListOfShopCodes();
            for (int i = 0; i < tShop.NumberOfRecords; i++)
            {
                string sShopName = tShop.GetRecordFrom(i)[1];
                while (sShopName.Length < 11)
                    sShopName = " " + sShopName;
                sPageHeader += sShopName;
            }
            string sPrice = "Price";
            while (sPrice.Length < 11)
            {
                sPrice = " " + sPrice;
            }
            sPageHeader += sPrice;
            string sBreaker = "-";
            while (sBreaker.Length < sPageHeader.Length)
                sBreaker += "-";
            tWriter.WriteLine(sBreaker);
            tWriter.WriteLine(sPageHeader);
            tWriter.WriteLine(sBreaker);
            fProgress.pb.Maximum = sBarcodes.Length;
            StockReportItem[] sItems = new StockReportItem[0];
            for (int z = 0; z < sBarcodes.Length; z++)
            {
                string[] sMainstockData;
                fProgress.pb.Value = z;

                sMainstockData = GetMainStockInfo(sBarcodes[z]);

                Array.Resize<StockReportItem>(ref sItems, sItems.Length + 1);
                sItems[sItems.Length - 1] = new StockReportItem(rOrder);
                sItems[sItems.Length - 1].dStockLevels = new decimal[sShopCodes.Length];
                sItems[sItems.Length - 1].sBarcode = sBarcodes[z];
                sItems[sItems.Length - 1].sDescription = sMainstockData[1];
                sItems[sItems.Length - 1].dRRP = Convert.ToDecimal(sMainstockData[2]);

                int nOfResults = 0;
                string[,] sResults = tStockStats.SearchAndGetAllMatchingRecords(0, sMainstockData[0], ref nOfResults, true);
                for (int i = 0; i < nOfResults; i++)
                {
                    int nShopNum = 0;
                    for (int p = 0; p < sShopCodes.Length; p++)
                    {
                        if (sResults[i, tStockStats.FieldNumber("SHOPCODE")] == sShopCodes[p])
                        {
                            nShopNum = p;
                            break;
                        }
                    }
                    sItems[sItems.Length - 1].dStockLevels[nShopNum] = Convert.ToDecimal(sResults[i, tStockStats.FieldNumber("QIS")]);
                }
                /* sMainstockData = tStock.GetRecordFrom(x);
                 if (sMainstockData[tStock.FieldNumber("CATEGORY")].StartsWith(sCategories[z]))
                 {
                     Array.Resize<StockReportItem>(ref sItems, sItems.Length + 1);
                     sItems[sItems.Length - 1] = new StockReportItem(rOrder);
                     sItems[sItems.Length - 1].dStockLevels = new decimal[sShopCodes.Length];
                     sItems[sItems.Length - 1].sBarcode = sMainstockData[0];
                     sItems[sItems.Length - 1].sDescription = sMainstockData[1];
                     int nOfResults = 0;
                     string[,] sResults = tStockStats.SearchAndGetAllMatchingRecords(0, sMainstockData[0], ref nOfResults);
                     for (int i = 0; i < nOfResults; i++)
                     {
                         int nShopNum = 0;
                         for (int p = 0; p < sShopCodes.Length; p++)
                         {
                             if (sResults[i, tStockStats.FieldNumber("SHOPCODE")] == sShopCodes[p])
                             {
                                 nShopNum = p;
                                 break;
                             }
                         }
                         sItems[sItems.Length - 1].dStockLevels[nShopNum] = Convert.ToDecimal(sResults[i, tStockStats.FieldNumber("QIS")]);
                     }
                 }*/
            }
            Array.Sort(sItems);
            for (int i = 0; i < sItems.Length; i++)
            {
                string sBarcode = sItems[i].sBarcode;
                while (sBarcode.Length < 13)
                    sBarcode += " ";
                string sDesc = sItems[i].sDescription;
                while (sDesc.Length < 30)
                    sDesc += " ";
                string[] sStockLevels = new string[sShopCodes.Length];
                for (int x = 0; x < sShopCodes.Length; x++)
                {
                    sStockLevels[x] = FormatMoneyForDisplay(sItems[i].dStockLevels[x]);
                    while (sStockLevels[x].Length < 10)
                        sStockLevels[x] = " " + sStockLevels[x];
                }
                string sLine = sBarcode + " " + sDesc;
                for (int x = 0; x < sShopCodes.Length; x++)
                {
                    sLine += sStockLevels[x] + " ";
                }
                string sPriceofItem = sItems[i].dRRP.ToString();
                while (sPriceofItem.Length < 11)
                    sPriceofItem = " " + sPriceofItem;
                sLine += sPriceofItem;
                if (bIncludeZeroItems || (!bIncludeZeroItems && !sItems[i].AllZero))
                    tWriter.WriteLine(sLine);
            }
            tWriter.WriteLine("");

            fProgress.Close();
            tWriter.Close();
        }

        private int nLineLastPrinted = 7;
        private int nPrinterPage = 1;
        private string sReportTitle = "Un-named Report";
        void ReportPrintPage(object sender, PrintPageEventArgs e)
        {
            Font fFont = new Font("Consolas", PrinterFontSize);
            TextReader tGetReport = new StreamReader("REPORT.TXT");
            string[] sReport = tGetReport.ReadToEnd().Split('\n');
            int nLastLine = sReport.Length - 1;
            while (sReport[nLastLine].TrimEnd('\r') == "")
                nLastLine -= 1;
            tGetReport.Close();
            int nCurrentTop = 30;
            int nLeft = 0;
            Color cBoxColour = Color.LightGray;
            int nBoxWidth = 0;
            bool bFinished = false;
            if (rCurrentlyPrinting == ReportType.SalesReport)
            {
                nBoxWidth = Convert.ToInt32(e.Graphics.MeasureString("                                             Level     Sold         Sales       Sales                            Profit (%)   Sales (%)", fFont).Width);
                e.Graphics.DrawString(sReportTitle, new Font("Arial", 14.0f), new SolidBrush(Color.Black), new PointF((e.PageBounds.Width / 2) - Convert.ToInt32((e.Graphics.MeasureString(sReportTitle, new Font("Arial", 14.0f)).Width / 2)), (float)nCurrentTop));
                nCurrentTop += Convert.ToInt32((new Font("Arial", 14.0f)).GetHeight());
                e.Graphics.DrawString("Page " + nPrinterPage.ToString(), new Font("Arial", 12.0f), new SolidBrush(Color.Black), new PointF((e.PageBounds.Width) - Convert.ToInt32((e.Graphics.MeasureString("Page " + nPrinterPage.ToString(), new Font("Arial", 12.0f)).Width) * 2), (float)nCurrentTop));
                nCurrentTop += Convert.ToInt32((new Font("Arial", 12.0f)).GetHeight());
                e.Graphics.DrawString(sReport[8], new Font("Arial", 12.0f), new SolidBrush(Color.Black), new PointF(nLeft, (float)nCurrentTop));
                nCurrentTop += Convert.ToInt32((new Font("Arial", 12.0f)).GetHeight());
                
                e.Graphics.FillRectangle(new SolidBrush(cBoxColour), new Rectangle(nLeft, nCurrentTop, nBoxWidth, Convert.ToInt32(fFont.GetHeight() * 2)));
                e.Graphics.DrawString("Barcode       Description                    Stock     Quantity     Gross       Net         Profit    Profit(%)  Relative     Relative", fFont, new SolidBrush(Color.Black), new PointF(nLeft, nCurrentTop));
                nCurrentTop += Convert.ToInt32(fFont.GetHeight());
                e.Graphics.DrawString("                                             Level     Sold         Sales       Sales                            Profit (%)   Sales (%)", fFont, new SolidBrush(Color.Black), new PointF(nLeft, nCurrentTop));
                nCurrentTop += Convert.ToInt32(fFont.GetHeight());
            }
            else if (rCurrentlyPrinting == ReportType.OrderInfo)
            {
                int nAdd = 0;
                if (GotEmailSupportAddress())
                    nAdd++;
                fFont = new Font("Consolas", 12.0f);
                nBoxWidth = Convert.ToInt32(e.Graphics.MeasureString(sReport[12 + nAdd], fFont).Width);
                e.Graphics.DrawString(DateTime.Now.ToString(), new Font("Arial", 12.0f), new SolidBrush(Color.Black), new PointF(0, (float)nCurrentTop));
                e.Graphics.DrawString(sReportTitle, new Font("Arial", 12.0f), new SolidBrush(Color.Black), new PointF((e.PageBounds.Width / 2) - Convert.ToInt32((e.Graphics.MeasureString(sReportTitle, new Font("Arial", 12.0f)).Width / 2)), (float)nCurrentTop));
                e.Graphics.DrawString("Page " + nPrinterPage.ToString(), new Font("Arial", 12.0f), new SolidBrush(Color.Black), new PointF((e.PageBounds.Width) - Convert.ToInt32((e.Graphics.MeasureString("Page " + nPrinterPage.ToString(), new Font("Arial", 12.0f)).Width) * 2), (float)nCurrentTop));
                nCurrentTop += Convert.ToInt32((new Font("Arial", 12.0f)).GetHeight());
                if (nPrinterPage > 1)
                {
                    e.Graphics.FillRectangle(new SolidBrush(cBoxColour), new Rectangle(nLeft, nCurrentTop, nBoxWidth, Convert.ToInt32(fFont.GetHeight())));
                    e.Graphics.DrawString(sReport[14], fFont, new SolidBrush(Color.Black), new PointF(0, nCurrentTop));
                    nCurrentTop += Convert.ToInt32(fFont.GetHeight());
                }
            }
            else if (rCurrentlyPrinting == ReportType.StockValuationReport)
            {
                nBoxWidth = Convert.ToInt32(e.Graphics.MeasureString(sReport[0], fFont).Width);
                e.Graphics.DrawString(DateTime.Now.ToString(), new Font("Arial", 12.0f), new SolidBrush(Color.Black), new PointF(0, (float)nCurrentTop));
                e.Graphics.DrawString(sReportTitle, new Font("Arial", 12.0f), new SolidBrush(Color.Black), new PointF((e.PageBounds.Width / 2) - Convert.ToInt32((e.Graphics.MeasureString(sReportTitle, new Font("Arial", 12.0f)).Width / 2)), (float)nCurrentTop));
                e.Graphics.DrawString("Page " + nPrinterPage.ToString(), new Font("Arial", 12.0f), new SolidBrush(Color.Black), new PointF((e.PageBounds.Width) - Convert.ToInt32((e.Graphics.MeasureString("Page " + nPrinterPage.ToString(), new Font("Arial", 12.0f)).Width) * 2), (float)nCurrentTop));
                nCurrentTop += Convert.ToInt32((new Font("Arial", 12.0f)).GetHeight());
                e.Graphics.FillRectangle(new SolidBrush(cBoxColour), new Rectangle(nLeft, nCurrentTop, nBoxWidth, Convert.ToInt32(fFont.GetHeight()) * 2));
                e.Graphics.DrawString(sReport[1], fFont, new SolidBrush(Color.Black), new PointF(0, nCurrentTop));
                nCurrentTop += Convert.ToInt32(fFont.GetHeight());
                e.Graphics.DrawString(sReport[2], fFont, new SolidBrush(Color.Black), new PointF(0, nCurrentTop));
                nCurrentTop += Convert.ToInt32(fFont.GetHeight());
            }
            else if (rCurrentlyPrinting == ReportType.ReprintReceipt)
            {
                nBoxWidth = Convert.ToInt32(e.Graphics.MeasureString("--------------------------------------------------", fFont).Width);
            }
            else if (rCurrentlyPrinting == ReportType.ComissionReport)
            {
                nBoxWidth = Convert.ToInt32(e.Graphics.MeasureString(sReport[2], fFont).Width);
                e.Graphics.DrawString(sReportTitle, new Font("Arial", 14.0f), new SolidBrush(Color.Black), new PointF((e.PageBounds.Width / 2) - Convert.ToInt32((e.Graphics.MeasureString(sReportTitle, new Font("Arial", 14.0f)).Width / 2)), (float)nCurrentTop));
                nCurrentTop += Convert.ToInt32((new Font("Arial", 14.0f)).GetHeight());
            }
            else if (rCurrentlyPrinting == ReportType.CommissionSummaryReport)
            {
                nBoxWidth = Convert.ToInt32(e.Graphics.MeasureString(sReport[4], fFont).Width);
                e.Graphics.DrawString(sReportTitle, new Font("Arial", 14.0f), new SolidBrush(Color.Black), new PointF((e.PageBounds.Width / 2) - Convert.ToInt32((e.Graphics.MeasureString(sReportTitle, new Font("Arial", 14.0f)).Width / 2)), (float)nCurrentTop));
                nCurrentTop += Convert.ToInt32((new Font("Arial", 14.0f)).GetHeight());
                e.Graphics.FillRectangle(new SolidBrush(cBoxColour), new Rectangle(nLeft, nCurrentTop, nBoxWidth, Convert.ToInt32(fFont.GetHeight())));
                e.Graphics.DrawString(sReport[2], fFont, new SolidBrush(Color.Black), new PointF(nLeft, nCurrentTop));
                nCurrentTop += Convert.ToInt32(fFont.GetHeight());
            }
            else
            {
                nBoxWidth = Convert.ToInt32(e.Graphics.MeasureString(sReport[4], fFont).Width);
                e.Graphics.DrawString(sReportTitle, new Font("Arial", 14.0f), new SolidBrush(Color.Black), new PointF((e.PageBounds.Width / 2) - Convert.ToInt32((e.Graphics.MeasureString(sReportTitle, new Font("Arial", 14.0f)).Width / 2)), (float)nCurrentTop));
                nCurrentTop += Convert.ToInt32((new Font("Arial", 14.0f)).GetHeight());
                e.Graphics.FillRectangle(new SolidBrush(cBoxColour), new Rectangle(nLeft, nCurrentTop, nBoxWidth, Convert.ToInt32(fFont.GetHeight())));
                e.Graphics.DrawString(sReport[4], fFont, new SolidBrush(Color.Black), new PointF(nLeft, nCurrentTop));
                nCurrentTop += Convert.ToInt32(fFont.GetHeight());
            }
            for (int i = nLineLastPrinted; i < sReport.Length && (nCurrentTop + fFont.Height) < e.PageBounds.Height - BottomBoundSize; i++)
            {
                if (sReport[i].StartsWith("-") && (i + 2) < sReport.Length)
                {
                    e.Graphics.FillRectangle(new SolidBrush(cBoxColour), new Rectangle(nLeft, nCurrentTop, nBoxWidth, Convert.ToInt32(fFont.GetHeight(e.Graphics))));
                    e.Graphics.DrawString(sReport[i + 1], fFont, new SolidBrush(Color.Black), new PointF(nLeft, nCurrentTop));
                    if (i == 3)
                    {
                        nCurrentTop += Convert.ToInt32(fFont.GetHeight());
                        e.Graphics.FillRectangle(new SolidBrush(cBoxColour), new Rectangle(nLeft, nCurrentTop, nBoxWidth, Convert.ToInt32(fFont.GetHeight(e.Graphics))));
                    
                        e.Graphics.DrawString(sReport[i + 2], fFont, new SolidBrush(Color.Black), new PointF(nLeft, nCurrentTop));
                        i += 3;
                    }
                    else
                        i += 2;
                    nCurrentTop += 10;
                }
                else
                {
                    e.Graphics.DrawString(sReport[i], fFont, new SolidBrush(Color.Black), new PointF(nLeft, nCurrentTop));
                }
                nCurrentTop += Convert.ToInt32(fFont.GetHeight());
                nLineLastPrinted = i + 1;
                if (i == nLastLine)
                    bFinished = true;
            }

            // Added because if the report is empty then it will get into an infinite loop on this here
            if (nLineLastPrinted == sReport.Length)
                bFinished = true;

            nPrinterPage++;
            e.HasMorePages = !bFinished;
        }

        public int BottomBoundSize
        {

            get
            {
                int nRecNum = -1;
                if (!tSettings.SearchForRecord("PRINTERBORDER", 0, ref nRecNum))
                {
                    string[] sToAdd = { "PRINTERBORDER", "20" };
                    tSettings.AddRecord(sToAdd);
                    tSettings.SaveToFile("SETTINGS.DBF");
                }
                return Convert.ToInt32(tSettings.GetRecordFrom("PRINTERBORDER", 0)[1]);
            }
            set
            {
                int nFieldNum = -1;
                if (!tSettings.SearchForRecord("PRINTERBORDER", 0, ref nFieldNum))
                {
                    string[] sToAdd = { "PRINTERBORDER", "20" };
                    tSettings.AddRecord(sToAdd);
                    tSettings.SaveToFile("SETTINGS.DBF");
                }
                tSettings.SearchForRecord("PRINTERBORDER", 0, ref nFieldNum);
                if (nFieldNum != -1)
                {
                    tSettings.EditRecordData(nFieldNum, 1, value.ToString());
                    tSettings.SaveToFile("SETTINGS.DBF");
                }
            }
        }
        public float PrinterFontSize
        {
            get
            {
                int nLoc = -1;
                tSettings.SearchForRecord("PFONTSIZE", 0, ref nLoc);
                if (nLoc != -1)
                {
                    return (float)Convert.ToDecimal(tSettings.GetRecordFrom(nLoc)[1]);
                }
                else
                {
                    return 7.5f;
                }
            }
            set
            {
                int nLoc = -1;
                tSettings.SearchForRecord("PFONTSIZE", 0, ref nLoc);
                if (nLoc != -1)
                {
                    tSettings.EditRecordData(nLoc, 1, value.ToString());
                }
                else
                {
                    string[] sToAdd = { "PFONTSIZE", value.ToString() };
                    tSettings.AddRecord(sToAdd);
                }
                tSettings.SaveToFile("SETTINGS.DBF");
            }
        }

        private string[] GetListOfSoldBarcodes(string sShopCode, Period pPeriod)
        {
            string[] sCodesToReturn = new string[0];
            string sPeriodCode = "";
            switch (pPeriod)
            {
                case Period.Daily:
                    sPeriodCode = "D";
                    break;
                case Period.Weekly:
                    sPeriodCode = "W";
                    break;
                case Period.Monthly:
                    sPeriodCode = "M";
                    break;
                case Period.Yearly:
                    sPeriodCode = "Y";
                    break;
            }

            // Go through every record in StockStats
            for (int i = 0; i < tStockStats.NumberOfRecords; i++)
            {
                string[] sStockStaRecord = tStockStats.GetRecordFrom(i);
                if (Convert.ToDecimal(sStockStaRecord[tStockStats.FieldNumber(sPeriodCode + "GSALES")]) != 0 && sStockStaRecord[tStockStats.FieldNumber("SHOPCODE")] == sShopCode || sStockStaRecord[0].ToUpper() == "DEP")
                {
                    Array.Resize<string>(ref sCodesToReturn, sCodesToReturn.Length + 1);
                    sCodesToReturn[sCodesToReturn.Length - 1] = tStockStats.GetRecordFrom(i)[0];
                }
                else if (Convert.ToDecimal(sStockStaRecord[tStockStats.FieldNumber(sPeriodCode + "COGS")]) != 0 && sStockStaRecord[tStockStats.FieldNumber("SHOPCODE")] == sShopCode)
                {
                    Array.Resize<string>(ref sCodesToReturn, sCodesToReturn.Length + 1);
                    sCodesToReturn[sCodesToReturn.Length - 1] = tStockStats.GetRecordFrom(i)[0];
                }
            }
            return sCodesToReturn;
        }

        public string FormatMoneyForDisplay(decimal dAmount)
        {
            dAmount = Math.Round(dAmount, 2, MidpointRounding.AwayFromZero);
            string[] sSplitUp = dAmount.ToString().Split('.');
            if (sSplitUp.Length == 1)
            {
                string[] temp = new string[2];
                temp[0] = sSplitUp[0];
                temp[1] = "00";
                sSplitUp = temp;
            }
            while (sSplitUp[1].Length < 2)
                sSplitUp[1] += "0";

            string toReturn = sSplitUp[0] + "." + sSplitUp[1];

            return toReturn;
        }
        public string FormatMoneyForDisplay(string sAmount)
        {
            decimal dAmount = Convert.ToDecimal(sAmount);
            dAmount = Math.Round(dAmount, 2, MidpointRounding.AwayFromZero);
            string[] sSplitUp = dAmount.ToString().Split('.');
            if (sSplitUp.Length == 1)
            {
                string[] temp = new string[2];
                temp[0] = sSplitUp[0];
                temp[1] = "00";
                sSplitUp = temp;
            }
            while (sSplitUp[1].Length < 2)
                sSplitUp[1] += "0";

            string toReturn = sSplitUp[0] + "." + sSplitUp[1];

            return toReturn;
        }

        public string GetLastCollectionDate()
        {
            DateTime dLatest = new DateTime(2000, 1, 1);
            foreach (Till t in Till)
            {
                string sLastCollection = t.LastCollection;
                DateTime dCollectionDate = new DateTime(2000 + Convert.ToInt32(sLastCollection[4].ToString() + sLastCollection[5].ToString()), Convert.ToInt32(sLastCollection[2].ToString() + sLastCollection[3].ToString()), Convert.ToInt32(sLastCollection[0].ToString() + sLastCollection[1].ToString()));
                if (DateTime.Compare(dLatest, dCollectionDate) < 0)
                    dLatest = dCollectionDate;
            }
            string sDay = dLatest.Day.ToString();
            while (sDay.Length < 2)
                sDay = "0" + sDay;
            string sMonth = dLatest.Month.ToString();
            while (sMonth.Length < 2)
                sMonth = "0" + sMonth;
            string sYear = dLatest.Year.ToString()[2].ToString() + dLatest.Year.ToString()[3].ToString();
            return sDay + sMonth + sYear;
        }

        /*
        private void ZeroDailySales(string sDepartmentCode)
        {
            Console.WriteLine("Zeroing Daily Sales for " + sDepartmentCode);
            for (int i = 0; i < tStock.NumberOfRecords; i++)
            {
                if (tStockStats.GetRecordFrom(i)[tStockStats.FieldNumber("SHOPCODE")] == sDepartmentCode)
                {
                    tStockStats.EditRecordData(i, tStockStats.FieldNumber("DGSALES"), "0");
                    tStockStats.EditRecordData(i, tStockStats.FieldNumber("DNSALES"), "0");
                    tStockStats.EditRecordData(i, tStockStats.FieldNumber("DQSOLD"), "0");
                    tStockStats.EditRecordData(i, tStockStats.FieldNumber("DCOGS"), "0");
                }
            }
            tStockStats.SaveToFile("STOCKSTA.DBF");
        }*/

        public string GetDDMMYYDate()
        {
            string sDateTime = DateTime.Now.ToString();
            string sToReturn = sDateTime[0].ToString() + sDateTime[1].ToString() + sDateTime[3].ToString() + sDateTime[4].ToString() + sDateTime[8].ToString() + sDateTime[9].ToString();
            return sToReturn;
        }

        public string GetFirstCategoryCode()
        {
            tCategory.SortTable();
            return tCategory.GetRecordFrom(0)[0];
        }

        public string GetLastCategoryCode()
        {
            tCategory.SortTable();
            return tCategory.GetRecordFrom(tCategory.NumberOfRecords - 1)[0];
        }

        public string[] ListOfCards
        {
            get
            {
                string[] sToReturn = new string[NumberOfCards];
                for (int i = 0; i < sToReturn.Length; i++)
                {
                    string sCardNo = i.ToString();
                    while (sCardNo.Length < 2)
                        sCardNo = "0" + sCardNo;
                    sToReturn[i] = tSettings.GetRecordFrom("CRD" + sCardNo, 0)[1];
                }
                return sToReturn;
            }
            set
            {
                int nCardNo = 0;
                string sCardNumber = "CRD00";
                int nRecNum = 0;
                while (tSettings.SearchForRecord(sCardNumber, 0, ref nRecNum))
                {
                    tSettings.DeleteRecord(nRecNum);
                    nCardNo++;
                    sCardNumber = nCardNo.ToString();
                    while (sCardNumber.Length < 2)
                        sCardNumber = "0" + sCardNumber;
                    sCardNumber = "CRD" + sCardNumber;
                }
                for (int i = 0; i < value.Length; i++)
                {
                    string sCRDNum = i.ToString();
                    while (sCRDNum.Length < 2)
                        sCRDNum = "0" + sCRDNum;
                    sCRDNum = "CRD" + sCRDNum;
                    string[] sToadd = { sCRDNum, value[i] };
                    tSettings.AddRecord(sToadd);
                }
                tSettings.SearchForRecord("NumberOfCards", 0, ref nRecNum);
                tSettings.EditRecordData(nRecNum, 1, value.Length.ToString());
                tSettings.SaveToFile("SETTINGS.DBF");
                for (int i = 0; i < Till.Length; i++)
                {
                    GenerateSettingsForTill(Till[i].Number);
                }
            }
        }

        /// <summary>
        /// Add category 1/3 item
        /// </summary>
        /// <param name="sShopCode">The shop code</param>
        /// <param name="sBarcode">The barcode</param>
        /// <param name="sDescription">The desc</param>
        /// <param name="sType">The item type (1/3!!!)</param>
        /// <param name="sCategory">The item category</param>
        /// <param name="sRRP">The RRP</param>
        /// <param name="sVATCode">The VAT Code</param>
        /// <param name="sMinQTY">The minimum order units</param>
        public void AddEditItem(string sShopCode, string sBarcode, string sDescription, string sType, string sCategory, string sRRP, string sVATCode, string sMinQTY,  string sAveCost)
        {
            if (sType != "1" && sType != "3")
            {
                throw new NotImplementedException("BAD PROGRAMMING ALERT");
            }
            int nRecNo = tStockStats.GetRecordNumberFromTwoFields(sBarcode, 0, sShopCode, 35);
            string[] sStockStaInfo = new string[0];
            string[] sExistingMainstock = new string[0];

            if (nRecNo != -1)
            {
                // Delete existing record
                sStockStaInfo = tStockStats.GetRecordFrom(nRecNo);
                tStockStats.DeleteRecord(nRecNo);
            }
            if (tStock.SearchForRecord(sBarcode, 0, ref nRecNo))
            {
                // Delete existing record
                sExistingMainstock = tStock.GetRecordFrom(nRecNo);
                tStock.DeleteRecord(nRecNo);
            }
            string[] sStockStats = new string[tStockStats.ReturnFieldNames().Length];
            sStockStats[0] = sBarcode;
            if (sStockStaInfo.Length == 0)
            {
                for (int i = 1; i < sStockStats.Length; i++)
                {
                    sStockStats[i] = "0";
                }
            }
            else
            {
                for (int i = 1; i < sStockStaInfo.Length; i++)
                {
                    sStockStats[i] = sStockStaInfo[i];
                }
            }
            sStockStats[tStockStats.FieldNumber("SHOPCODE")] = sShopCode;
            sStockStats[tStockStats.FieldNumber("MINORDQTY")] = sMinQTY;
            sStockStats[1] = sAveCost;
            tStockStats.AddRecord(sStockStats);

            string[] sMainStock = new string[tStock.ReturnFieldNames().Length];
            sMainStock[0] = sBarcode;
            sMainStock[1] = sDescription;
            sMainStock[2] = sRRP;
            sMainStock[3] = sVATCode;
            sMainStock[4] = sCategory;
            sMainStock[5] = sType;
            if (sExistingMainstock.Length == 0)
            {
                sMainStock[6] = "";
            }
            else
            {
                sMainStock[6] = sExistingMainstock[6];
            }
            sMainStock[7] = "";
            if (sExistingMainstock.Length > 0)
                sMainStock[8] = sExistingMainstock[8];
            else
                sMainStock[8] = "0.00";
            tStock.AddRecord(sMainStock);
            tStock.SaveToFile("MAINSTOC.DBF");
            tStockStats.SaveToFile("STOCKSTA.DBF");
            AddItemToTillDownload(sBarcode, sShopCode);
        }
        public void AddEditItem(string sShopCode, string sBarcode, string sDescription, string sType, string sCategory, string sVATCode, string sMinQTY, decimal sAveCost, decimal dTargetMargin)
        {
            if (sType != "2")
            {
                throw new NotImplementedException("BAD PROGRAMMING ALERT");
            }
            int nRecNo = tStockStats.GetRecordNumberFromTwoFields(sBarcode, 0, sShopCode, 35);
            string[] sExistingStockSta = new string[0];
            if (nRecNo != -1)
            {
                // Delete existing record
                sExistingStockSta = tStockStats.GetRecordFrom(nRecNo);
                tStockStats.DeleteRecord(nRecNo);
            }
            string[] sExistingMainStock = new string[0];
            if (tStock.SearchForRecord(sBarcode, 0, ref nRecNo))
            {
                // Delete existing record
                sExistingMainStock = tStock.GetRecordFrom(nRecNo);
                tStock.DeleteRecord(nRecNo);
            }
            string[] sStockStats = new string[tStockStats.ReturnFieldNames().Length];
            sStockStats[0] = sBarcode;
            sStockStats[1] = sAveCost.ToString();
            if (sExistingStockSta.Length == 0)
            {
                for (int i = 2; i < sStockStats.Length; i++)
                {
                    sStockStats[i] = "0";
                }
            }
            else
            {
                for (int i = 2; i < sStockStats.Length; i++)
                {
                    sStockStats[i] = sExistingStockSta[i];
                }
            }
            sStockStats[tStockStats.FieldNumber("SHOPCODE")] = sShopCode;
            sStockStats[tStockStats.FieldNumber("MINORDQTY")] = sMinQTY;
            sStockStats[39] = FormatMoneyForDisplay(dTargetMargin);
            tStockStats.AddRecord(sStockStats);

            string[] sMainStock = new string[tStock.ReturnFieldNames().Length];
            sMainStock[0] = sBarcode;
            sMainStock[1] = sDescription;
            sMainStock[2] = "0.00";
            sMainStock[3] = sVATCode;
            sMainStock[4] = sCategory;
            sMainStock[5] = sType;
            if (sExistingMainStock.Length == 0)
            {
                sMainStock[6] = "";
            }
            else
            {
                sMainStock[6] = sExistingMainStock[6];
            }
            sMainStock[7] = "";
            sMainStock[8] = "0.00";
            tStock.AddRecord(sMainStock);
            tStock.SaveToFile("MAINSTOC.DBF");
            tStockStats.SaveToFile("STOCKSTA.DBF");
            AddItemToTillDownload(sBarcode, sShopCode);
        }
        public void AddEditItem(string sShopCode, string sBarcode, string sDescription, string sType, string sCategory, string sRRP, string sVATCode, string sMinQTY, string sParentItem, string sCPartQTY)
        {
            int nRecNo = tStockStats.GetRecordNumberFromTwoFields(sBarcode, 0, sShopCode, 35);
            string[] sExistingStockStats = new string[0];
            if (nRecNo != -1)
            {
                // Delete existing record
                sExistingStockStats = tStockStats.GetRecordFrom(nRecNo);
                tStockStats.DeleteRecord(nRecNo);
            }
            string[] sExistingMainStock = new string[0];
            if (tStock.SearchForRecord(sBarcode, 0, ref nRecNo))
            {
                // Delete existing record
                sExistingMainStock = tStock.GetRecordFrom(nRecNo);
                tStock.DeleteRecord(nRecNo);
            }
            string[] sStockStats = new string[tStockStats.ReturnFieldNames().Length];
            sStockStats[0] = sBarcode;
            if (sExistingStockStats.Length == 0)
            {
                for (int i = 1; i < sStockStats.Length; i++)
                {
                    sStockStats[i] = "0";
                }
            }
            else
            {
                for (int i = 1; i < sStockStats.Length; i++)
                {
                    sStockStats[i] = sExistingStockStats[i];
                }
            }
            sStockStats[tStockStats.FieldNumber("SHOPCODE")] = sShopCode;
            sStockStats[tStockStats.FieldNumber("MINORDQTY")] = sMinQTY;
            sStockStats[tStockStats.FieldNumber("CPARTQTY")] = sCPartQTY;
            tStockStats.AddRecord(sStockStats);

            string[] sMainStock = new string[tStock.ReturnFieldNames().Length];
            sMainStock[0] = sBarcode;
            sMainStock[1] = sDescription;
            sMainStock[2] = sRRP;
            sMainStock[3] = sVATCode;
            sMainStock[4] = sCategory;
            sMainStock[5] = sType;
            if (sExistingMainStock.Length == 0)
            {
                sMainStock[6] = "";
            }
            else
            {
                sMainStock[6] = sExistingMainStock[6];
            }
            sMainStock[7] = sParentItem;
            sMainStock[8] = "0.00";
            tStock.AddRecord(sMainStock);
            tStock.SaveToFile("MAINSTOC.DBF");
            tStockStats.SaveToFile("STOCKSTA.DBF");
            AddItemToTillDownload(sBarcode, sShopCode);
        }
        public void AddEditItem(string sShopCode, string sBarcode, string sDescription, string sType, string sCategory, string sRRP, string sVATCode, string sMinQTY, string sCommCode, decimal dCommAmount, string sRecQty)
        {
            if (sType != "6")
            {
                throw new NotImplementedException("BAD PROGRAMMING ALERT");
            }
            int nRecNo = tStockStats.GetRecordNumberFromTwoFields(sBarcode, 0, sShopCode, 35);
            string[] sStockStaInfo = new string[0];
            string[] sExistingMainstock = new string[0];

            if (nRecNo != -1)
            {
                // Delete existing record
                sStockStaInfo = tStockStats.GetRecordFrom(nRecNo);
                tStockStats.DeleteRecord(nRecNo);
            }
            if (tStock.SearchForRecord(sBarcode, 0, ref nRecNo))
            {
                // Delete existing record
                sExistingMainstock = tStock.GetRecordFrom(nRecNo);
                tStock.DeleteRecord(nRecNo);
            }
            string[] sStockStats = new string[tStockStats.ReturnFieldNames().Length];
            sStockStats[0] = sBarcode;
            if (sStockStaInfo.Length == 0)
            {
                for (int i = 1; i < sStockStats.Length; i++)
                {
                    sStockStats[i] = "0";
                }
            }
            else
            {
                for (int i = 1; i < sStockStaInfo.Length; i++)
                {
                    sStockStats[i] = sStockStaInfo[i];
                }
            }
            // Average cost set to the amount paid to the artist
            sStockStats[1] = dCommAmount.ToString();
            sStockStats[tStockStats.FieldNumber("SHOPCODE")] = sShopCode;
            sStockStats[tStockStats.FieldNumber("MINORDQTY")] = sMinQTY;
            tStockStats.AddRecord(sStockStats);

            string[] sMainStock = new string[tStock.ReturnFieldNames().Length];
            sMainStock[0] = sBarcode;
            sMainStock[1] = sDescription;
            sMainStock[2] = sRRP;
            sMainStock[3] = sVATCode;
            sMainStock[4] = sCategory;
            sMainStock[5] = sType;
            // Supplier set to the artist code
            sMainStock[6] = sCommCode;
            sMainStock[7] = "";
            // Last cost set to amount paid to the artist
            sMainStock[8] = dCommAmount.ToString();
            tStock.AddRecord(sMainStock);
            tStock.SaveToFile("MAINSTOC.DBF");
            tStockStats.SaveToFile("STOCKSTA.DBF");
            //AddItemForCommissioner(sBarcode, sCommCode);
            AddItemToTillDownload(sBarcode, sShopCode);

            // Now receieve the correct quantity
            this.ReceiveComissionItem(sBarcode, sRecQty, sShopCode);
        }
        public void RemoveSuppliersForItem(string sBarcode)
        {
            int nRecNo = 0;
            while (tSupplierIndex.SearchForRecord(sBarcode, 0, ref nRecNo))
                tSupplierIndex.DeleteRecord(nRecNo);
        }
        public void RemoveSuppliersForItem(string sBarcode, string sSupCode)
        {
            int nRecNo = tSupplierIndex.GetRecordNumberFromTwoFields(sBarcode, 0, sSupCode, 1);
            if (nRecNo != -1)
                tSupplierIndex.DeleteRecord(nRecNo);
        }

        
        public void AddSupplierForItem(string sBarcode, string sSupplierCode, string sCost, string sSupRef)
        {
            /*
            int nRecNo = tSupplierIndex.GetRecordNumberFromTwoFields(sBarcode, 0, sSupplierCode, 1);
            if (nRecNo != -1)
            {
                // Delete existing record
                tSupplierIndex.DeleteRecord(nRecNo);
            }*/
            string[] sSupIndex = new string[tSupplierIndex.ReturnFieldNames().Length];
            sSupIndex[0] = sBarcode;
            sSupIndex[1] = sSupplierCode;
            sSupIndex[2] = sSupRef;
            sSupIndex[3] = sCost;
            string sDateTimeNow = DateTime.Now.Year.ToString();
            string sMonth = DateTime.Now.Month.ToString();
            while (sMonth.Length < 2)
                sMonth = "0" + sMonth;
            string sDay = DateTime.Now.Day.ToString();
            while (sDay.Length < 2)
                sDay = "0" + sDay;
            sDateTimeNow += sMonth + sDay;
            sSupIndex[4] = sDateTimeNow;
            tSupplierIndex.AddRecord(sSupIndex);
            tSupplierIndex.SaveToFile("SUPINDEX.DBF");
        }

        public string[,] GetListOfSuppliersForItem(string sBarcode, ref int nOfResults)
        {
            return tSupplierIndex.SearchAndGetAllMatchingRecords(0, sBarcode, ref nOfResults, true);
        }

        public void AddMultiItemItem(string sBarcode, string sDesc, string sShopCode, string[] sSubBarcodes, decimal[] dQuantities, decimal[] dAmountPerItem)
        {
            // Delete existing item first
            if (DoesMultiItemExist(sBarcode, sShopCode))
            {
                int nRecLoc = tMultiHeader.GetRecordNumberFromTwoFields(sBarcode, 0, sShopCode, 3);
                tMultiHeader.DeleteRecord(nRecLoc);

                while (tMultiData.SearchForRecord(sBarcode, 0, ref nRecLoc))
                {
                    tMultiData.DeleteRecord(nRecLoc);
                }
            }

            string[] sHeader = { sBarcode, sSubBarcodes.Length.ToString(), sDesc, sShopCode };
            tMultiHeader.AddRecord(sHeader);

            for (int i = 0; i < sSubBarcodes.Length; i++)
            {
                string[] sData = { sBarcode, i.ToString(), sSubBarcodes[i], dQuantities[i].ToString(), dAmountPerItem[i].ToString() };
                tMultiData.AddRecord(sData);
            }
            tMultiData.SaveToFile("MULTIDAT.DBF");
            tMultiHeader.SaveToFile("MULTIHDR.DBF");

            foreach (Till t in Till)
            {
                GenerateMultiBarcodeItemsForTill(t.Number);
            }
        }

        public bool DoesMultiItemExist(string sBarcode, string sShopCode)
        {
            int nFieldNum = tMultiHeader.GetRecordNumberFromTwoFields(sBarcode, 0, sShopCode, 3);
            if (nFieldNum != -1)
                return true;
            else
                return false;
        }

        public void GetMultiItemInfo(string sShopCode, string sBarcode, ref string sDesc, ref string[] sSubBarcodes, ref decimal[] dQuantities, ref decimal[] dRRP)
        {
            int nRecNum = tMultiHeader.GetRecordNumberFromTwoFields(sBarcode, 0, sShopCode, 3);
            string[] sHeaderData = tMultiHeader.GetRecordFrom(nRecNum);
            int nOfItems = Convert.ToInt32(sHeaderData[1]);
            sDesc = sHeaderData[2];
            sSubBarcodes = new string[nOfItems];
            dQuantities = new decimal[nOfItems];
            dRRP = new decimal[nOfItems];
            for (int i = 0; i < nOfItems; i++)
            {
                nRecNum = tMultiData.GetRecordNumberFromTwoFields(sBarcode, 0, i.ToString(),1);
                string[] sInfo = tMultiData.GetRecordFrom(nRecNum);
                sSubBarcodes[i] = sInfo[2];
                dQuantities[i] = Convert.ToDecimal(sInfo[3]);
                dRRP[i] = Convert.ToDecimal(sInfo[4]);
            }
        }

        public string[] GetListOfCategoryGroupNames()
        {
            string[] sToReturn = new string[tCatGroupHeader.NumberOfRecords];
            for (int i = 0; i < tCatGroupHeader.NumberOfRecords; i++)
            {
                sToReturn[i] = tCatGroupHeader.GetRecordFrom(i)[1];
            }
            return sToReturn;
        }

        public string[] GetListOfCatGroupCategories(string sCatGroupName)
        {
            int nCatGroupNum = Convert.ToInt32(tCatGroupHeader.GetRecordFrom(sCatGroupName, 1)[0]);
            int nOfSubGroups = Convert.ToInt32(tCatGroupHeader.GetRecordFrom(sCatGroupName, 1)[2]);
            string[] sToReturn = new string[nOfSubGroups];
            for (int i = 0; i < nOfSubGroups; i++)
            {
                int nFieldNum = tCatGroupData.GetRecordNumberFromTwoFields(nCatGroupNum.ToString(), 0, i.ToString(), 1);
                sToReturn[i] = tCatGroupData.GetRecordFrom(nFieldNum)[2];
            }
            return sToReturn;
        }

        public void AddEditCategoryGroup(string sDesc, string[] sListOfCodes)
        {
            // search for item in catgrouphdr
            int nRecNum = -1;
            tCatGroupHeader.SearchForRecord(sDesc, 1, ref nRecNum);
            int nCatNum = -1;
            if (nRecNum != -1)
            {
                // Delete existing
                int nRecNum2 = 0;
                nCatNum = Convert.ToInt32(tCatGroupHeader.GetRecordFrom(nRecNum)[0]);
                tCatGroupHeader.DeleteRecord(nCatNum);
                while (tCatGroupData.SearchForRecord(nCatNum.ToString(), 0, ref nRecNum2))
                    tCatGroupData.DeleteRecord(nRecNum2);
            }
            else
            {
                // find next available number
                for (int i = 0; i < tCatGroupHeader.NumberOfRecords; i++)
                {
                    if (Convert.ToInt32(tCatGroupHeader.GetRecordFrom(i)[0]) > nCatNum)
                        nCatNum = Convert.ToInt32(tCatGroupHeader.GetRecordFrom(i)[0]);
                }
                nCatNum++;
            }
            string[] sHeader = { nCatNum.ToString(), sDesc, sListOfCodes.Length.ToString() };
            tCatGroupHeader.AddRecord(sHeader);
            for (int i = 0; i < sListOfCodes.Length; i++)
            {
                string[] sToAdd = { nCatNum.ToString(), i.ToString(), sListOfCodes[i] };
                tCatGroupData.AddRecord(sToAdd);
            }
            tCatGroupHeader.SaveToFile("CATGPHDR.DBF");
            tCatGroupData.SaveToFile("CATGRPDA.DBF");
        }

        public void DeleteCategoryGroup(string sDesc)
        {
            int nRecNum = 0;
            tCatGroupHeader.SearchForRecord(sDesc, 1, ref nRecNum);
            int nCatNum = Convert.ToInt32(tCatGroupHeader.GetRecordFrom(sDesc, 1, true)[0]);
            tCatGroupHeader.DeleteRecord(nRecNum);
            while (tCatGroupData.SearchForRecord(nCatNum.ToString(), 0, ref nRecNum))
                tCatGroupData.DeleteRecord(nRecNum);
            tCatGroupHeader.SaveToFile("CATGPHDR.DBF");
            tCatGroupData.SaveToFile("CATGRPDA.DBF");
        }

        public void AddItemToTillDownload(string sBarcode, string sShopCode)
        {
            string[] sToAdd = { sBarcode };
            AddItemsToTillDownload(sToAdd, sShopCode);
        }
        public void AddItemsToTillDownload(string[] sBarcodes, string sShopCode)
        {
            frmProgressBar fp = new frmProgressBar("Adding Items To Download");
            fp.pb.Maximum = sBarcodes.Length * Till.Length;
            if (sBarcodes.Length > 5)
            {
                fp.Show();
            }
            for (int t = 0; t < Till.Length; t++)
            {
                if (Till[t].ShopCode == sShopCode)
                {
                    if (!Directory.Exists("TILL" + Till[t].Number.ToString()))
                    {
                        Directory.CreateDirectory("TILL" + Till[t].Number.ToString());
                    }
                    if (!Directory.Exists("TILL" + Till[t].Number.ToString() + "\\OUTGNG"))
                    {
                        Directory.CreateDirectory("TILL" + Till[t].Number.ToString() + "\\OUTGNG");
                    }
                    if (!File.Exists("TILL" + Till[t].Number.ToString() + "\\OUTGNG\\STOCK.DBF"))
                    {
                        FileStream fsWriter = new FileStream("TILL" + Till[t].Number.ToString() + "\\OUTGNG\\STOCK.DBF", FileMode.Create);
                        fsWriter.Write(BackOffice.Properties.Resources.TILLSTOCK, 0, BackOffice.Properties.Resources.TILLSTOCK.Length);
                        fsWriter.Close();
                    }
                    if (!File.Exists("TILL" + Till[t].Number.ToString() + "\\OUTGNG\\STKLEVEL.DBF"))
                    {
                        FileStream fsWriter = new FileStream("TILL" + Till[t].Number.ToString() + "\\OUTGNG\\STKLEVEL.DBF", FileMode.Create);
                        fsWriter.Write(BackOffice.Properties.Resources.TILLSTKLEVEL, 0, BackOffice.Properties.Resources.TILLSTKLEVEL.Length);
                        fsWriter.Close();
                    }

                    Table tNewStock = new Table(sTDir + "TILL" + Till[t].Number.ToString() + "\\OUTGNG\\STOCK.DBF");
                    Table tStockLvl = new Table(sTDir + "TILL" + Till[t].Number.ToString() + "\\OUTGNG\\STKLEVEL.DBF");
                    for (int i = 0; i < sBarcodes.Length; i++)
                    {
                        fp.pb.Value = (t * sBarcodes.Length) + i;
                        int nRecNo = tStockStats.GetRecordNumberFromTwoFields(sBarcodes[i], 0, sShopCode, tStockStats.FieldNumber("SHOPCODE"));
                        string[] sStockStaData = tStockStats.GetRecordFrom(nRecNo);
                        string[] sMainStockData = tStock.GetRecordFrom(sBarcodes[i], 0, true);
                        string[] sToAddToStock = {sStockStaData[0], sMainStockData[1], sStockStaData[tStockStats.FieldNumber("SHOPCODE")],
                                             sMainStockData[tStock.FieldNumber("ITEMTYPE")], sMainStockData[tStock.FieldNumber("SELLINGPRI")],
                                             sMainStockData[tStock.FieldNumber("SELLINGPRI")], sMainStockData[tStock.FieldNumber("VATCATEGOR")],
                                             sMainStockData[tStock.FieldNumber("PARENTITEM")], "0", sMainStockData[tStock.FieldNumber("CATEGORY")]};
                        string[] sToAddToStkLevel = {sBarcodes[i], sStockStaData[tStockStats.FieldNumber("SHOPCODE")], 
                                                sStockStaData[tStockStats.FieldNumber("QIS")], sMainStockData[tStock.FieldNumber("CATEGORY")], sStockStaData[3], GetItemDueDate(sBarcodes[i], sShopCode)};
                        tNewStock.AddRecord(sToAddToStock);
                        tStockLvl.AddRecord(sToAddToStkLevel);

                        if (sMainStockData[5] == "6")
                        {
                            // Commission Item
                            if (!File.Exists("TILL" + Till[t].Number.ToString() + "\\OUTGNG\\COMMISSI.DBF"))
                            {
                                FileStream fsWriter = new FileStream("TILL" + Till[t].Number.ToString() + "\\OUTGNG\\COMMISSI.DBF", FileMode.Create);
                                fsWriter.Write(BackOffice.Properties.Resources.TILLCOMM, 0, BackOffice.Properties.Resources.TILLCOMM.Length);
                                fsWriter.Close();
                            }
                            Table tCommission = new Table("TILL" + Till[t].Number.ToString() + "\\OUTGNG\\COMMISSI.DBF");
                            nRecNo = 0;
                            if (!tCommission.SearchForRecord(sBarcodes[i], 0, ref nRecNo))
                            {
                                string[] sToAdd = { sBarcodes[i], sMainStockData[8] };
                                tCommission.AddRecord(sToAdd);
                            }
                            else
                            {
                                tCommission.EditRecordData(nRecNo, 1, sMainStockData[8]);
                            }
                            tCommission.SaveToFile("TILL" + Till[t].Number.ToString() + "\\OUTGNG\\COMMISSI.DBF");
                        }
                    }
                    tNewStock.SaveToFile("TILL" + Till[t].Number.ToString() + "\\OUTGNG\\STOCK.DBF");
                    tStockLvl.SaveToFile("TILL" + Till[t].Number.ToString() + "\\OUTGNG\\STKLEVEL.DBF");
                }
            }
            fp.Close();
        }

        public void UpdateStockLevelOnTill(string sBarcode, string sShopCode)
        {
            for (int t = 0; t < Till.Length; t++)
            {
                if (Till[t].ShopCode == sShopCode)
                {
                    if (!Directory.Exists("TILL" + Till[t].Number.ToString()))
                    {
                        Directory.CreateDirectory("TILL" + Till[t].Number.ToString());
                    }
                    if (!Directory.Exists("TILL" + Till[t].Number.ToString() + "\\OUTGNG"))
                    {
                        Directory.CreateDirectory("TILL" + Till[t].Number.ToString() + "\\OUTGNG");
                    }
                    if (!File.Exists("TILL" + Till[t].Number.ToString() + "\\OUTGNG\\STKLEVEL.DBF"))
                    {
                        FileStream fsWriter = new FileStream("TILL" + Till[t].Number.ToString() + "\\OUTGNG\\STKLEVEL.DBF", FileMode.Create);
                        fsWriter.Write(BackOffice.Properties.Resources.TILLSTKLEVEL, 0, BackOffice.Properties.Resources.TILLSTKLEVEL.Length);
                        fsWriter.Close();
                    }

                     Table tStockLvl = new Table(sTDir + "TILL" + Till[t].Number.ToString() + "\\OUTGNG\\STKLEVEL.DBF");

                    int nRecNo = tStockStats.GetRecordNumberFromTwoFields(sBarcode, 0, sShopCode, tStockStats.FieldNumber("SHOPCODE"));
                    string[] sStockStaData = tStockStats.GetRecordFrom(nRecNo);
                    string[] sMainStockData = tStock.GetRecordFrom(sBarcode, 0, true);
                    string[] sToAddToStkLevel = {sBarcode, sStockStaData[tStockStats.FieldNumber("SHOPCODE")], 
                                                sStockStaData[tStockStats.FieldNumber("QIS")], sMainStockData[tStock.FieldNumber("CATEGORY")]};
                    tStockLvl.AddRecord(sToAddToStkLevel);
                    tStockLvl.SaveToFile("TILL" + Till[t].Number.ToString() + "\\OUTGNG\\STKLEVEL.DBF");
                }
            }
        }

        public void TotalDownloadToTill(int nTillNumber)
        {
            // Stock First
            frmProgressBar fp = new frmProgressBar("Total Data Upload");
            fp.pb.Maximum = tStock.NumberOfRecords;
            fp.Show();

            if (!Directory.Exists("TILL" + nTillNumber.ToString()))
            {
                Directory.CreateDirectory("TILL" + nTillNumber.ToString());
            }
            if (!Directory.Exists("TILL" + nTillNumber.ToString() + "\\OUTGNG"))
            {
                Directory.CreateDirectory("TILL" + nTillNumber.ToString() + "\\OUTGNG");
            }
            if (!File.Exists("TILL" + nTillNumber.ToString() + "\\OUTGNG\\STOCK.DBF"))
            {
                FileStream fsWriter = new FileStream("TILL" + nTillNumber.ToString() + "\\OUTGNG\\STOCK.DBF", FileMode.Create);
                fsWriter.Write(BackOffice.Properties.Resources.TILLSTOCK, 0, BackOffice.Properties.Resources.TILLSTOCK.Length);
                fsWriter.Close();
            }
            if (!File.Exists("TILL" + nTillNumber.ToString() + "\\OUTGNG\\STKLEVEL.DBF"))
            {
                FileStream fsWriter = new FileStream("TILL" + nTillNumber.ToString() + "\\OUTGNG\\STKLEVEL.DBF", FileMode.Create);
                fsWriter.Write(BackOffice.Properties.Resources.TILLSTKLEVEL, 0, BackOffice.Properties.Resources.TILLSTKLEVEL.Length);
                fsWriter.Close();
            }
            
            Table tNewStock = new Table(sTDir + "TILL" + nTillNumber.ToString() + "\\OUTGNG\\STOCK.DBF");
            Table tStockLvl = new Table(sTDir + "TILL" + nTillNumber.ToString() + "\\OUTGNG\\STKLEVEL.DBF");
            string[] sStockStaData = new string[tStockStats.ReturnFieldNames().Length];
            string[] sMainStockData = new string[tStock.ReturnFieldNames().Length];
            string sBarcode = "";
            string sShopCode = "";
            for (int i = 0; i < tStockStats.NumberOfRecords; i++)
            {
                fp.pb.Value = i;

                sStockStaData = tStockStats.GetRecordFrom(i);
                if (sStockStaData[35] == Till[nTillNumber - 1].ShopCode)
                {
                    sMainStockData = tStock.GetRecordFrom(sStockStaData[0], 0, true);
                    sBarcode = sMainStockData[0];
                    sShopCode = sStockStaData[35];
                    if (sMainStockData.Length > 1)
                    {
                        int nRecNo = tStockStats.GetRecordNumberFromTwoFields(sBarcode, 0, sShopCode, 35);
                        string[] sToAddToStock = {sStockStaData[0], sMainStockData[1], sStockStaData[35],
                                             sMainStockData[5], sMainStockData[2],
                                             sMainStockData[2], sMainStockData[3],
                                             sMainStockData[7], "0", sMainStockData[4]};
                        string[] sToAddToStkLevel = {sBarcode, sStockStaData[35], 
                                                sStockStaData[36], sMainStockData[4], sStockStaData[3], GetItemDueDate(sBarcode, sShopCode)};
                        tNewStock.AddRecord(sToAddToStock);
                        tStockLvl.AddRecord(sToAddToStkLevel);
                    }
                }
            }
            tNewStock.SaveToFile("TILL" + nTillNumber.ToString() + "\\OUTGNG\\STOCK.DBF");
            tStockLvl.SaveToFile("TILL" + nTillNumber.ToString() + "\\OUTGNG\\STKLEVEL.DBF");
            
            // Now the settings file

            GenerateCustomerAccountFileForTill(nTillNumber);

            GenerateComissionFileForTill(nTillNumber);

            GenerateSettingsForTill(nTillNumber);

            GenerateStaffFileForTill(nTillNumber);

            GenerateCategoryFileForTill(nTillNumber);

            GenerateMultiBarcodeItemsForTill(nTillNumber);

            GenerateVATFileForTill(nTillNumber);

            GenerateOffersFileForTill(nTillNumber);
            
            fp.Close();
        }

        public void GenerateVATFileForTill(int nTillNumber)
        {
            File.Copy("VAT.DBF", "TILL" + nTillNumber.ToString() + "\\OUTGNG\\VAT.DBF", true);
        }

        public void GenerateMultiBarcodeItemsForTill(int nTillNumber)
        {
            File.Copy("MULTIDAT.DBF", "TILL" + nTillNumber.ToString() + "\\OUTGNG\\MULTIDAT.DBF", true);
            File.Copy("MULTIHDR.DBF", "TILL" + nTillNumber.ToString() + "\\OUTGNG\\MULTIHDR.DBF", true);
        }

        public void GenerateComissionFileForTill(int nTillNumber)
        {
            if (!File.Exists("TILL" + nTillNumber.ToString() + "\\OUTGNG\\COMMISSI.DBF"))
            {
                FileStream fsWriter = new FileStream("TILL" + nTillNumber.ToString() + "\\OUTGNG\\COMMISSI.DBF", FileMode.Create);
                fsWriter.Write(BackOffice.Properties.Resources.TILLCOMM, 0, BackOffice.Properties.Resources.TILLCOMM.Length);
                fsWriter.Close();
            }
            Table tTillComm = new Table("TILL" + nTillNumber.ToString() + "\\OUTGNG\\COMMISSI.DBF");
            for (int i = 0; i < tStock.NumberOfRecords; i++)
            {
                if (tStock.GetRecordFrom(i)[5] == "6")
                {
                    string[] sToAdd = { tStock.GetRecordFrom(i)[0], tStock.GetRecordFrom(i)[8] };
                    tTillComm.AddRecord(sToAdd);
                }
            }
            tTillComm.SaveToFile("TILL" + nTillNumber.ToString() + "\\OUTGNG\\COMMISSI.DBF");
        }

        public void GenerateCustomerAccountFileForTill(int nTillNum)
        {
            if (!File.Exists("TILl" + nTillNum.ToString() + "\\OUTGNG\\ACCSTAT.DBF"))
            {
                FileStream fsWriter = new FileStream("TILL" + nTillNum.ToString() + "\\OUTGNG\\ACCSTAT.DBF", FileMode.Create);
                fsWriter.Write(BackOffice.Properties.Resources.TILLACCSTAT, 0, BackOffice.Properties.Resources.TILLACCSTAT.Length);
                fsWriter.Close();
            }
            Table tTillAccStat = new Table("TILL" + nTillNum.ToString() + "\\OUTGNG\\ACCSTAT.DBF");
            for (int i = 0; i < tAccStat.NumberOfRecords; i++)
            {
                string[] sCurrent = tAccStat.GetRecordFrom(i);
                string[] sToAdd = { sCurrent[0], sCurrent[1], sCurrent[2], sCurrent[3], sCurrent[4], sCurrent[5], sCurrent[6] };
                tTillAccStat.AddRecord(sToAdd);
            }
            tTillAccStat.SaveToFile("TILl" + nTillNum.ToString() + "\\OUTGNG\\ACCSTAT.DBF");
        }

        public void GenerateCategoryFileForTill(int nTillNumber)
        {
            FileStream fsWriter4 = new FileStream("TILL" + nTillNumber.ToString() + "\\OUTGNG\\TILLCAT.DBF", FileMode.Create);
            fsWriter4.Write(BackOffice.Properties.Resources.TILLCAT, 0, BackOffice.Properties.Resources.TILLCAT.Length);
            fsWriter4.Close();

            File.Copy("CATEGORY.DBF", "TILL" + nTillNumber.ToString() + "\\OUTGNG\\TILLCAT.DBF", true);
        }

        public void GenerateOffersFileForTill(int nTillNumber)
        {
            File.Copy("OFFERS.DBF", "TILL" + nTillNumber.ToString() + "\\OUTGNG\\OFFERS.DBF", true);
            if (!Directory.Exists("TILL" + nTillNumber.ToString() + "\\OUTGNG\\RECEIPTS"))
            {
                Directory.CreateDirectory("TILL" + nTillNumber.ToString() + "\\OUTGNG\\RECEIPTS");
            }
            for (int i = 0; i < Directory.GetFiles("OffersReceipt").Length; i++)
            {
                File.Copy(Directory.GetFiles("OffersReceipt")[i], "TILL" + nTillNumber.ToString() + "\\OUTGNG\\RECEIPTS\\" + Directory.GetFiles("OffersReceipt")[i].Split('\\')[Directory.GetFiles("OffersReceipt")[i].Split('\\').Length - 1], true);
            }
        }

        public void GenerateStaffFileForTill(int nTillNumber)
        {
            FileStream fsWriter3 = new FileStream("TILL" + nTillNumber.ToString() + "\\OUTGNG\\STAFF.DBF", FileMode.Create);
            fsWriter3.Write(BackOffice.Properties.Resources.TILLSTAFF, 0, BackOffice.Properties.Resources.TILLSTAFF.Length);
            fsWriter3.Close();
            Table tNewStaff = new Table(sTDir + "TILL" + nTillNumber.ToString() + "\\OUTGNG\\STAFF.DBF");
            while (tNewStaff.NumberOfRecords > 0)
                tNewStaff.DeleteRecord(0);
            for (int i = 0; i < tStaff.NumberOfRecords; i++)
            {
                string[] sStaff = tStaff.GetRecordFrom(i);
                if (sStaff[1] == Till[nTillNumber - 1].ShopCode)
                {
                    string[] sNewStaff = { sStaff[0], sStaff[2], "N" };
                    tNewStaff.AddRecord(sNewStaff);
                }
                else
                {
                    string[] sNewStaff = { (i + 1).ToString(), "", "N" };
                    tNewStaff.AddRecord(sNewStaff);
                }
            }
            tNewStaff.SaveToFile("TILL" + nTillNumber.ToString() + "\\OUTGNG\\STAFF.DBF");
        }

        public void GenerateSettingsForTill(int nTillNumber)
        {
            FileStream fsWriter2 = new FileStream("TILL" + nTillNumber.ToString() + "\\OUTGNG\\DETAILS.DBF", FileMode.Create);
            fsWriter2.Write(BackOffice.Properties.Resources.TILLDETAILS, 0, BackOffice.Properties.Resources.TILLDETAILS.Length);
            fsWriter2.Close();
            Table tNewDetails = new Table(sTDir + "TILL" + nTillNumber.ToString() + "\\OUTGNG\\DETAILS.DBF");
            string[] sSettings = new string[40];
            string[] sShopAdd = GetShopAddress(Till[nTillNumber - 1].ShopCode);
            sSettings[0] = sShopAdd[0];
            sSettings[1] = sShopAdd[1];
            sSettings[2] = sShopAdd[2];
            sSettings[3] = tSettings.GetRecordFrom("VATNumber", 0, true)[1];
            sSettings[4] = "N/A";
            sSettings[5] = tSettings.GetRecordFrom("Passwords", 0, true)[1];
            sSettings[6] = Till[nTillNumber - 1].TillName;
            while (sSettings[6].Length < 6)
            {
                sSettings[6] += " ";
            }
            sSettings[6] += Till[nTillNumber - 1].ShopCode;
            while (sSettings[6].Length < 9)
            {
                sSettings[6] += " ";
            }
            sSettings[6] += this.GetShopNameFromCode(Till[nTillNumber - 1].ShopCode);
            sSettings[7] = "£";
            sSettings[8] = "N/A";
            if (tSettings.SearchForRecord("DiscountThreshold", "SETTINGNAM"))
            {
                sSettings[8] = tSettings.GetRecordFrom("DiscountThreshold", 0)[1];
            }
            sSettings[9] = "N/A";
            sSettings[10] = "N/A";
            sSettings[11] = "N/A";
            sSettings[12] = tSettings.GetRecordFrom(Till[nTillNumber - 1].ShopCode + "PhoneNumber", 0, true)[1];
            sSettings[13] = "N/A";
            sSettings[14] = "N/A";
            sSettings[15] = "99";
            int nOfCards = Convert.ToInt32(tSettings.GetRecordFrom("NumberOfCards", 0, true)[1]);
            sSettings[16] = nOfCards.ToString();
            for (int i = 0; i < nOfCards && i < 9; i++)
            {
                string sCardSetting = "CRD0" + i.ToString();
                sSettings[17 + i] = tSettings.GetRecordFrom(sCardSetting, 0, true)[1];
            }
            for (int i = nOfCards; i < 9; i++)
            {
                sSettings[17 + i] = "";
            }
            for (int i = 0; i < 9; i++)
            {
                int nRecNum = 0;
                if (tSettings.SearchForRecord("CRD" + (i).ToString() + "DISC", 0, ref nRecNum))
                {
                    sSettings[26 + i] = tSettings.GetRecordFrom(nRecNum)[1];
                }
                else
                {
                    sSettings[26 + i] = "0.00";
                }
                //sSettings[26 + i] = tVATRates.GetRecordFrom(i)[2];
            }
            /*for (int i = tVATRates.NumberOfRecords; i < 9; i++)
            {
                sSettings[26 + i] = "0.00";
            }*/
            string[] sReceiptFooter = Till[nTillNumber - 1].ReceiptFooter;
            sSettings[35] = sReceiptFooter[0];
            sSettings[36] = sReceiptFooter[1];
            sSettings[37] = sReceiptFooter[2];
            sSettings[38] = "N/A";
            sSettings[39] = "N/A";
            for (int i = 0; i < sSettings.Length; i++)
            {
                string[] sToadd = { sSettings[i] };
                tNewDetails.AddRecord(sToadd);
            }
            tNewDetails.SaveToFile("TILL" + nTillNumber.ToString() + "\\OUTGNG\\DETAILS.DBF");
        }

        public bool CopyWaitingFilesToTills()
        {
            bool bAllOk = true;
            for (int i = 0; i < Till.Length; i++)
            {
                if (Directory.Exists(Till[i].FileLocation + "\\INGNG") && Directory.Exists("TILL" + Till[i].Number.ToString() + "\\OUTGNG"))
                {
                    string[] sFiles = Directory.GetFiles("TILL" + Till[i].Number.ToString() + "\\OUTGNG");
                    for (int z = 0; z < sFiles.Length; z++)
                    {
                        File.Copy(sFiles[z], Till[i].FileLocation + "\\INGNG\\" + sFiles[z].Split('\\')[sFiles[z].Split('\\').Length - 1], true);
                        File.Delete(sFiles[z]);
                    }
                    if (Directory.Exists("TILL" + Till[i].Number.ToString() + "\\OUTGNG\\RECEIPTS"))
                    {
                        sFiles = Directory.GetFiles("TILL" + Till[i].Number.ToString() + "\\OUTGNG\\RECEIPTS");
                        if (!Directory.Exists(Till[i].FileLocation + "\\INGNG\\OffersReceipt"))
                        {
                            Directory.CreateDirectory(Till[i].FileLocation + "\\INGNG\\OffersReceipt");
                        }
                        for (int z = 0; z < sFiles.Length; z++)
                        {
                            File.Copy(sFiles[z], Till[i].FileLocation + "\\INGNG\\OffersReceipt\\" + sFiles[z].Split('\\')[sFiles[z].Split('\\').Length - 1], true);
                            File.Delete(sFiles[z]);
                        }
                    }
                }
                else
                {
                    bAllOk = false;
                }
            }
            return bAllOk;
        }

        public string GetPasswords(int nPasswordLevel)
        {
            string sPasswordRecordContents = tSettings.GetRecordFrom("Passwords", 0, true)[1];
            string sDecrypted = "";
            for (int i = 0; i < sPasswordRecordContents.TrimEnd(' ').Length; i += 6)
            {
                sDecrypted += ((char)(sPasswordRecordContents[i] - 128)).ToString();
                sDecrypted += ((char)(sPasswordRecordContents[i + 1] - 123)).ToString();
                sDecrypted += ((char)(sPasswordRecordContents[i + 2] - 120)).ToString();
                sDecrypted += ((char)(sPasswordRecordContents[i + 3] - 121)).ToString();
                sDecrypted += ((char)(sPasswordRecordContents[i + 4] - 122)).ToString();
                sDecrypted += ((char)(sPasswordRecordContents[i + 5] - 124)).ToString();
                sDecrypted += ",";
            }
            string[] sPasswords = sDecrypted.Split(',');
            for (int i = 0; i < sPasswords.Length; i++)
            {
                sPasswords[i] = sPasswords[i].TrimEnd(' ');

                // Ensures that all random characters at the end are removed
                // Restricts passwords to between 0 and Z on the ASCII table
                for (int x = 0; x < sPasswords[i].Length; x++)
                {
                    if (sPasswords[i][x] < '0' || sPasswords[i][x] > 'Z')
                    {
                        sPasswords[i] = sPasswords[i].Substring(0, x);
                        break;
                    }
                }
            }
            return sPasswords[nPasswordLevel - 1];
        }

        public void SetPassword(int nLevel, string sPasswordRecordContents)
        {
            string[] sDecrypted = new string[3];
            sDecrypted[0] = "";
            sDecrypted[1] = "";
            sDecrypted[2] = "";
            string[] sPasswords = new string[3];
            for (int i = 0; i < 3; i++)
            {
                sPasswords[i] = GetPasswords(i + 1);
            }
            sPasswords[nLevel - 1] = sPasswordRecordContents;
            for (int i = 0; i < 3; i++)
            {
                while (sPasswords[i].Length < 6)
                    sPasswords[i] += " ";
            }
            for (int i = 0; i < 3; i++)
            {
                sDecrypted[i] += ((char)(sPasswords[i][0] + 128)).ToString();
                sDecrypted[i] += ((char)(sPasswords[i][1] + 123)).ToString();
                sDecrypted[i] += ((char)(sPasswords[i][2] + 120)).ToString();
                sDecrypted[i] += ((char)(sPasswords[i][3] + 121)).ToString();
                sDecrypted[i] += ((char)(sPasswords[i][4] + 122)).ToString();
                sDecrypted[i] += ((char)(sPasswords[i][5] + 124)).ToString();
            }
            string sToAdd = sDecrypted[0] + sDecrypted[1] + sDecrypted[2];
            int nRecNum = 0;
            tSettings.SearchForRecord("Passwords", 0, ref nRecNum);
            tSettings.EditRecordData(nRecNum, 1, sToAdd);
            tSettings.SaveToFile("SETTINGS.DBF");
        }

        public void GenerateDetailsForAllTills()
        {
            for (int i = 0; i < Till.Length; i++)
            {
                GenerateSettingsForTill(Till[i].Number);
            }
        }

        public void GenerateOffersForAllTills()
        {
            for (int i = 0; i < Till.Length; i++)
            {
                GenerateOffersFileForTill(Till[i].Number);
            }
        }

        public string[] GetAllCategoryCodes()
        {
            string[] sToReturn = new string[tCategory.NumberOfRecords];
            for (int i = 0; i < sToReturn.Length; i++)
            {
                sToReturn[i] = tCategory.GetRecordFrom(i)[0];
            }
            return sToReturn;
        }

        public void EditCategoryDetails(string sOriginalCode, string sNewCode, string sDesc)
        {
            int nRecNo = -1;
            tCategory.SearchForRecord(sOriginalCode, 0, ref nRecNo);
            tCategory.EditRecordData(nRecNo, 0, sNewCode);
            tCategory.EditRecordData(nRecNo, 1, sDesc);
            tCategory.SaveToFile("CATEGORY.DBF");
            frmProgressBar fProgress = new frmProgressBar("Changing Item Categories");
            fProgress.pb.Maximum = tStock.NumberOfRecords;
            fProgress.Show();
            for (int i = 0; i < tStock.NumberOfRecords; i++)
            {
                fProgress.pb.Value = i;
                if (tStock.GetRecordFrom(i)[4] == sOriginalCode)
                {
                    tStock.EditRecordData(i, 4, sNewCode);
                    int nOfRecords = 0;
                    string[,] sBarcodes = tStockStats.SearchAndGetAllMatchingRecords(0, tStock.GetRecordFrom(i)[0],ref nOfRecords, true);
                    for (int z = 0; z < nOfRecords; z++)
                    {
                        AddItemToTillDownload(sBarcodes[z,0], sBarcodes[z, tStockStats.FieldNumber("SHOPCODE")]);
                    }
                }
            }
            tStock.SaveToFile("MAINSTOC.DBF");
            fProgress.Close();
            CopyWaitingFilesToTills();
        }

        public void AddCategory(string sCode, string sDesc)
        {
            string[] sToAdd = { sCode, sDesc };
            tCategory.AddRecord(sToAdd);
            tCategory.SaveToFile("CATEGORY.DBF");
            for (int i = 0; i < Till.Length; i++)
            {
                GenerateCategoryFileForTill(Till[i].Number);
            }
            CopyWaitingFilesToTills();
        }

        public int LastOrderNumber
        {
            get
            {
                tOrder.SortTable();
                return Convert.ToInt32(tOrder.GetRecordFrom(tOrder.NumberOfRecords - 1)[0]);
            }
        }

        public decimal GetItemCostBySupplier(string sBarcode, string sSupCode)
        {
            try
            {
                int nRecNum = tSupplierIndex.GetRecordNumberFromTwoFields(sBarcode, 0, sSupCode, 1);
                if (nRecNum != -1)
                    return Convert.ToDecimal(tSupplierIndex.GetRecordFrom(nRecNum)[3]);
                else
                    return 0;
            }
            catch
            {
                return 0;
            }
        }

        public decimal GetItemAverageCost(string sBarcode)
        {
            try
            {
                return Convert.ToDecimal(tStockStats.GetRecordFrom(sBarcode, 0)[1]);
            }
            catch
            {
                return 0;
            }
        }

        public void AddEditOrderHeader(string sSupplier, string sOrderNum, string sSupRef, string sNotes, string sDueDate, string sDateRaised, string sShopCode)
        {
            this.FindMissingOrderLines();
            int nRecNum = 0;
            if (tOrder.SearchForRecord(sOrderNum, 0, ref nRecNum))
                tOrder.DeleteRecord(nRecNum);
            string[] sToAdd = { sOrderNum, sSupplier, sSupRef, sNotes, sDueDate, sDateRaised, sShopCode };
            tOrder.AddRecord(sToAdd);
            tOrder.SaveToFile("ORDER.DBF");
            this.FindMissingOrderLines();
        }
        public void AddEditOrderHeader(string[] sHeaderData)
        {
            this.FindMissingOrderLines();
            int nRecNum = 0;
            if (tOrder.SearchForRecord(sHeaderData[0], 0, ref nRecNum))
                tOrder.DeleteRecord(nRecNum);
            tOrder.AddRecord(sHeaderData);
            tOrder.SaveToFile("ORDER.DBF");
            this.FindMissingOrderLines();
        }

        public void AddEditOrderData(string[] sBarcodes, string[] sQuantities, string[] sReceived, string[] sCostPrice, string sOrderNumber)
        {
            this.FindMissingOrderLines();
            frmProgressBar fp = new frmProgressBar("Adding Barcodes To Order");
            fp.pb.Maximum = sBarcodes.Length + Till.Length;
            fp.Show();
            sOrderNumber = sOrderNumber.TrimStart(' ');
            int nRecNum = 0;
            for (int i = 0; i < sBarcodes.Length; i++)
            {
                nRecNum = tOrderLine.GetRecordNumberFromTwoFields(sBarcodes[i], 2, sOrderNumber, 0);
                if (nRecNum != -1)
                {
                    // Test line to fix orders
                    tOrderLine.EditRecordData(nRecNum, 3, sQuantities[i]);
                    // End of test fix
                    /*decimal dCurrentlyOnOrder = Convert.ToDecimal(tOrderLine.GetRecordFrom(nRecNum)[3]) - Convert.ToDecimal(tOrderLine.GetRecordFrom(nRecNum)[4]);
                    int nStockStaLoc = tStockStats.GetRecordNumberFromTwoFields(sBarcodes[i], 0, tOrder.GetRecordFrom(sOrderNumber, 0, true)[6], 35);
                    if (nStockStaLoc != -1)
                    {
                        tStockStats.EditRecordData(nStockStaLoc, 3, (Convert.ToDecimal(tStockStats.GetRecordFrom(nStockStaLoc)[3]) - dCurrentlyOnOrder).ToString());
                    }*/
                    // This block is commented out as it used to adjust the quantity on order of edited items, but this isn't necessary as it's done as the user changes the qty on order
                }
                else
                {
                    decimal dOnorder = Convert.ToDecimal(tStockStats.GetRecordFrom(sBarcodes[i], 0, true)[3]);
                    dOnorder += Convert.ToDecimal(sQuantities[i]);
                    tStockStats.SearchForRecord(sBarcodes[i], 0, ref nRecNum);
                    tStockStats.EditRecordData(nRecNum, 3, dOnorder.ToString());
                }
            }
            while (tOrderLine.SearchForRecord(sOrderNumber, 0, ref nRecNum))
                tOrderLine.DeleteRecord(nRecNum);

            for (int i = 0; i < sBarcodes.Length; i++)
            {
                fp.pb.Value = i;
                if (Convert.ToDecimal(sQuantities[i]) == 0 && Convert.ToDecimal(sReceived[i]) == 0)
                {
                    // Any items that have 0 ordered and 0 received are removed, as there's no point in them being there
                    ;
                }
                else
                {
                    string[] sToAdd = { sOrderNumber, (i + 1).ToString(), sBarcodes[i], sQuantities[i], sReceived[i], sCostPrice[i], "0" };
                    tOrderLine.AddRecord(sToAdd);

                    /*decimal dOnorder = Convert.ToDecimal(tStockStats.GetRecordFrom(sBarcodes[i], 0, true)[3]);
                    dOnorder += Convert.ToDecimal(sQuantities[i]);
                    tStockStats.SearchForRecord(sBarcodes[i], 0, ref nRecNum);
                    tStockStats.EditRecordData(nRecNum, 3, dOnorder.ToString());
                     */
                    // This block was moved into the previous for loop
                }
            }
            string[] sOrderHeader = GetOrderHeader(sOrderNumber.TrimStart(' '));
            for (int i = 0; i < Till.Length; i++)
            {
                fp.pb.Value = sBarcodes.Length + i;
                if (sOrderHeader[6] == Till[i].ShopCode)
                {
                    int nTill = Till[i].Number;
                    if (!File.Exists("TILL" + nTill.ToString() + "\\OUTGNG\\STKLEVEL.DBF"))
                    {
                        FileStream fsWriter = new FileStream("TILL" + nTill.ToString() + "\\OUTGNG\\STKLEVEL.DBF", FileMode.Create);
                        fsWriter.Write(BackOffice.Properties.Resources.TILLSTKLEVEL, 0, BackOffice.Properties.Resources.TILLSTKLEVEL.Length);
                        fsWriter.Close();
                    }
                    Table tStockLvl = new Table(sTDir + "TILL" + nTill.ToString() + "\\OUTGNG\\STKLEVEL.DBF");
                    for (int x = 0; x < sBarcodes.Length; x++)
                    {
                        string[] sStockStaData = GetItemStockStaRecord(sBarcodes[x], Till[i].ShopCode);
                        string[] sMainStockData = GetMainStockInfo(sBarcodes[x]);
                        string[] sToAddToStkLevel = {sBarcodes[x], sStockStaData[35], 
                                                sStockStaData[36], sMainStockData[4], sStockStaData[3], GetItemDueDate(sBarcodes[x], Till[i].ShopCode)};
                        tStockLvl.AddRecord(sToAddToStkLevel);
                    }
                    tStockLvl.SaveToFile("TILL" + nTill.ToString() + "\\OUTGNG\\STKLEVEL.DBF");
                }
            }
            tOrderLine.SaveToFile("ORDERLIN.DBF");
            tStockStats.SaveToFile("STOCKSTA.DBF");
            fp.Close();
            this.FindMissingOrderLines();
        }

        public bool DoesOrderExist(string sOrderNum)
        {
            int n = 0;
            if (tOrder.SearchForRecord(sOrderNum, 0, ref n))
                return true;
            else
                return false;
        }

        public string[] GetOrderHeader(string sOrderNum)
        {
            return tOrder.GetRecordFrom(sOrderNum, 0, true);
        }

        class OrderItem : IComparable
        {

            #region IComparable Members

            public int CompareTo(object obj)
            {
                return -decimal.Compare(((OrderItem)obj).LineNumber, LineNumber);
            }

            #endregion

            public decimal LineNumber;
            public string Barcode;
            public string OrderQty;
            public string Received;
            public string Cost;
            public string InvoiceQty;
        }
        public void GetOrderData(string sOrderNum, ref string[] sBarcodes, ref string[] sOrderQty, ref string[] sReceived, ref string[] sCost)
        {
            this.FindMissingOrderLines();
            int nOfLines = 0;
            string[,] sAllData = tOrderLine.SearchAndGetAllMatchingRecords(0, sOrderNum, ref nOfLines, true);
            sBarcodes = new string[nOfLines];
            sOrderQty = new string[nOfLines];
            sReceived = new string[nOfLines];
            sCost = new string[nOfLines];
            for (int i = 0; i < nOfLines; i++)
            {
                sBarcodes[i] = sAllData[i, 2];
                sOrderQty[i] = sAllData[i, 3];
                sReceived[i] = sAllData[i, 4];
                sCost[i] = sAllData[i, 5];
            }
            OrderItem[] oItems = new OrderItem[nOfLines];
            for (int i = 0; i < nOfLines; i++)
            {
                oItems[i] = new OrderItem();
                oItems[i].Barcode = sBarcodes[i];
                oItems[i].Cost = sCost[i];
                oItems[i].OrderQty = sOrderQty[i];
                oItems[i].Received = sReceived[i];
                oItems[i].LineNumber = Convert.ToDecimal(sAllData[i, 1]);
            }
            Array.Sort(oItems);
            for (int i = 0; i < nOfLines; i++)
            {
                sBarcodes[i] = oItems[i].Barcode;
                sOrderQty[i] = oItems[i].OrderQty;
                sReceived[i] = oItems[i].Received;
                sCost[i] = oItems[i].Cost;
            }
            this.FindMissingOrderLines();
        }

        public void GetOrderData(string sOrderNum, ref string[] sBarcodes, ref string[] sOrderQty, ref string[] sReceived, ref string[] sCost, ref string[] sInvoiceQty)
        {
            this.FindMissingOrderLines();
            int nOfLines = 0;
            string[,] sAllData = tOrderLine.SearchAndGetAllMatchingRecords(0, sOrderNum, ref nOfLines, true);
            sBarcodes = new string[nOfLines];
            sOrderQty = new string[nOfLines];
            sReceived = new string[nOfLines];
            sCost = new string[nOfLines];
            sInvoiceQty = new string[nOfLines];
            for (int i = 0; i < nOfLines; i++)
            {
                sBarcodes[i] = sAllData[i, 2];
                sOrderQty[i] = sAllData[i, 3];
                sReceived[i] = sAllData[i, 4];
                sCost[i] = sAllData[i, 5];
                sInvoiceQty[i] = sAllData[i, 6];
            }
            OrderItem[] oItems = new OrderItem[nOfLines];
            for (int i = 0; i < nOfLines; i++)
            {
                oItems[i] = new OrderItem();
                oItems[i].Barcode = sBarcodes[i];
                oItems[i].Cost = sCost[i];
                oItems[i].OrderQty = sOrderQty[i];
                oItems[i].Received = sReceived[i];
                oItems[i].InvoiceQty = sInvoiceQty[i];
            }
            Array.Sort(oItems);
            for (int i = 0; i < nOfLines; i++)
            {
                sBarcodes[i] = oItems[i].Barcode;
                sOrderQty[i] = oItems[i].OrderQty;
                sReceived[i] = oItems[i].Received;
                sCost[i] = oItems[i].Cost;
                sInvoiceQty[i] = oItems[i].InvoiceQty;
            }
            this.FindMissingOrderLines();
        }

        public void GetOrdersWithItemOutstandingIn(string sBarcode, ref string[] sOrderNums, ref string[] sSupCodes, ref string[] sQuantities)
        {
            this.FindMissingOrderLines();
            int nOfResults = 0;
            string[,] sResults = tOrderLine.SearchAndGetAllMatchingRecords(2, sBarcode, ref nOfResults, true);
            sOrderNums = new string[0];
            sSupCodes = new string[0];
            sQuantities = new string[0];
            for (int i = 0; i < nOfResults; i++)
            {
                if (Convert.ToDecimal(sResults[i, 3]) != Convert.ToDecimal(sResults[i, 4]))
                {
                    decimal dQuantityOnOrder = Convert.ToDecimal(sResults[i, 3]);
                    decimal dQuantityReceived = Convert.ToDecimal(sResults[i, 4]);

                    Array.Resize<string>(ref sOrderNums, sOrderNums.Length + 1);
                    Array.Resize<string>(ref sSupCodes, sSupCodes.Length + 1);
                    Array.Resize<string>(ref sQuantities, sQuantities.Length + 1);

                    sOrderNums[sOrderNums.Length - 1] = sResults[i, 0];
                    string[] sOrderHeader = GetOrderHeader(sOrderNums[sOrderNums.Length - 1]);
                    sSupCodes[sSupCodes.Length - 1] = sOrderHeader[1];
                    sQuantities[sQuantities.Length - 1] = FormatMoneyForDisplay(dQuantityOnOrder - dQuantityReceived);
                }
            }
            this.FindMissingOrderLines();
        }

        public string[] GetListOfOrderNumbers()
        {
            this.FindMissingOrderLines();
            string[] sToReturn = new string[tOrder.NumberOfRecords];
            for (int i = 0; i < tOrder.NumberOfRecords; i++)
            {
                sToReturn[i] = tOrder.GetRecordFrom(i)[0];
            }
            Array.Sort(sToReturn);
            return sToReturn;
            this.FindMissingOrderLines();
        }

        public void ReceiveOrder(string sOrderNum, string[] sBarcodes, string[] sQuantities, string[] sCosts)
        {
            this.FindMissingOrderLines();
            frmProgressBar fp = new frmProgressBar("Receiving Stock");
            fp.pb.Maximum = sBarcodes.Length;
            fp.Show();
            string sShopCode = tOrder.GetRecordFrom(sOrderNum, 0, true)[6];
            for (int i = 0; i < sBarcodes.Length; i++)
            {
                fp.pb.Value = i;
                int nRecLoc = tOrderLine.GetRecordNumberFromTwoFields(sOrderNum, 0, sBarcodes[i], 2);
                if (nRecLoc == -1)
                {
                    throw new NotSupportedException("Error in Receive Order. The barcode wasn't found to be associated with the order. Barcode: " + sBarcodes[i] + " Order Num: " + sOrderNum);
                }
                string[] sExistingRecord = tOrderLine.GetRecordFrom(nRecLoc);
                decimal dReceived = Convert.ToDecimal(sExistingRecord[4]);
                dReceived += Convert.ToDecimal(sQuantities[i]);
                // edit received qty in orderlin
                tOrderLine.EditRecordData(nRecLoc, 4, dReceived.ToString());
                tOrderLine.EditRecordData(nRecLoc, 5, sCosts[i]);
                int nStockStatsRec = tStockStats.GetRecordNumberFromTwoFields(sBarcodes[i], 0, sShopCode, 35);
                if (nStockStatsRec == -1)
                {
                    throw new NotSupportedException("Error in Receive Order. The barcode & shopcode aren't associated in Stockstats");
                }
                string[] sStockStaRec = tStockStats.GetRecordFrom(nStockStatsRec);

                decimal dYearlyDelivered = Convert.ToDecimal(sStockStaRec[23]);
                dYearlyDelivered += Convert.ToDecimal(sQuantities[i]);
                // edit yearly received in stocksta
                tStockStats.EditRecordData(nStockStatsRec, 23, dYearlyDelivered.ToString());
                decimal dQtyOnOrder = Convert.ToDecimal(sStockStaRec[3]);
                dQtyOnOrder -= Convert.ToDecimal(sQuantities[i]);
                // subtract qty on order
                tStockStats.EditRecordData(nStockStatsRec, 3, dQtyOnOrder.ToString());
                tStockStats.EditRecordData(nStockStatsRec, 4, GetDDMMYYDate());
                decimal dYearlyDelCost = Convert.ToDecimal(sStockStaRec[24]);
                //dYearlyDelivered += (Convert.ToDecimal(sQuantities[i]) * Convert.ToDecimal(sCosts[i]));
                // edit yearly cost
                //tStockStats.EditRecordData(nStockStatsRec, 24, dYearlyDelivered.ToString());
                // Commented out above and replaced with code below as it was adding qty + cost instead of just cost. LOL
                dYearlyDelCost = (Convert.ToDecimal(sQuantities[i]) * Convert.ToDecimal(sCosts[i]));
                tStockStats.EditRecordData(nStockStatsRec, 24, dYearlyDelCost.ToString());
                decimal dAverageCost = Convert.ToDecimal(sStockStaRec[1]);
                decimal dQtyInStock = Convert.ToDecimal(sStockStaRec[36]);
                decimal dNewCost = Convert.ToDecimal(sCosts[i]);
                decimal dNewStock = Convert.ToDecimal(sQuantities[i]);
                // work out average cost
                if (dQtyInStock + dNewStock != 0)
                {
                    dAverageCost = Math.Round(((dQtyInStock * dAverageCost) + (dNewCost * dNewStock)) / (dNewStock + dQtyInStock), 2, MidpointRounding.AwayFromZero);
                    tStockStats.EditRecordData(nStockStatsRec, 1, dAverageCost.ToString());
                }
                decimal dNewQIS = dQtyInStock + dNewStock;
                // edit qis
                tStockStats.EditRecordData(nStockStatsRec, 36, dNewQIS.ToString());
                int nRecNum = 0;
                tStock.SearchForRecord(sBarcodes[i], 0, ref nRecNum);
                // edit last cost
                tStock.EditRecordData(nRecNum, 8, sCosts[i]);
                string sSupCode = tOrder.GetRecordFrom(sOrderNum, 0, true)[1];
                nRecLoc = tSupplierIndex.GetRecordNumberFromTwoFields(sBarcodes[i], 0, sSupCode, 1);
                if (nRecLoc != -1)
                {
                    tSupplierIndex.EditRecordData(nRecLoc, 3, sCosts[i]);
                    tSupplierIndex.SaveToFile("SUPINDEX.DBF");
                }
                else
                {
                    string[] sToAdd = { sBarcodes[i], sSupCode.ToUpper(), sBarcodes[i], sCosts[i], "" };
                    tSupplierIndex.AddRecord(sToAdd);
                    tSupplierIndex.SaveToFile("SUPINDEX.DBF");
                }
            }
            fp.Close();
            AddItemsToTillDownload(sBarcodes, sShopCode);
            tOrderLine.SaveToFile("ORDERLIN.DBF");
            tStockStats.SaveToFile("STOCKSTA.DBF");
            tStock.SaveToFile("MAINSTOC.DBF");
            this.FindMissingOrderLines();
        }

        public string[] GetBarcodesOfAllItemsFromSupplier(string sSupCode)
        {
            if (sSupCode != "")
            {
                int nOfRec = 0;
                string[,] sResults = tSupplierIndex.SearchAndGetAllMatchingRecords(1, sSupCode, ref nOfRec, true);
                string[] sToReturn = new string[nOfRec];
                for (int i = 0; i < nOfRec; i++)
                {
                    sToReturn[i] = sResults[i, 0];
                }
                return sToReturn;
            }
            else
            {
                int nOfRec = 0;
                string[,] sResults = tSupplierIndex.SearchAndGetAllMatchingRecords(1, sSupCode, ref nOfRec);
                string[] sToReturn = new string[nOfRec];
                for (int i = 0; i < nOfRec; i++)
                {
                    sToReturn[i] = sResults[i, 0];
                }
                return sToReturn;
            }

        }

        class RequisitionItem : IComparable
        {
            public string sBarcode = "";
            public string sCategoryCode = "";
            public RequisitionItem(string sBCode, string sCatCode)
            {
                sBarcode = sBCode;
                sCategoryCode = sCatCode;
            }

            #region IComparable Members

            public int CompareTo(object obj)
            {
                return String.Compare(sCategoryCode, ((RequisitionItem)obj).sCategoryCode);
            }

            #endregion
        }
        public string[] GetBarcodesOfItemsBySpec(string sSupCode, decimal dMinAveSales, decimal dNOfDays, string sShopCode)
        {
            frmProgressBar fp = new frmProgressBar("Searching For Applicable Items");
            fp.Show();
            string[] sAllCodes = GetBarcodesOfAllItemsFromSupplier(sSupCode);
            fp.pb.Maximum = sAllCodes.Length;
            int nToRemove = 0;
            for (int i = 0; i < sAllCodes.Length; i++)
            {
                fp.pb.Value = i;
                int nRecLoc = tStockStats.GetRecordNumberFromTwoFields(sAllCodes[i], 0, sShopCode, 35);
                if (nRecLoc == -1)
                {
                    sAllCodes[i] = "$REMOVE";
                    nToRemove++;
                }
                else
                {
                    string[] sStockStaData = tStockStats.GetRecordFrom(nRecLoc);
                    decimal dQtyInStock = Convert.ToDecimal(sStockStaData[36]);
                    decimal dAveSales = Convert.ToDecimal(sStockStaData[2]);
                    if (dAveSales < dMinAveSales)
                    {
                        sAllCodes[i] = "$REMOVE";
                        nToRemove++;
                    }
                    else
                    {
                        decimal dNumOfDaysStock = 9999;
                        if (dAveSales != 0)
                            dNumOfDaysStock = dQtyInStock / dAveSales;
                        if (dNumOfDaysStock > dNOfDays)
                        {
                            sAllCodes[i] = "$REMOVE";
                            nToRemove++;
                        }
                        else
                        {
                            string[] sMainStockInfo = GetMainStockInfo(sAllCodes[i]);
                            if (sMainStockInfo[5] != "1")
                            {
                                sAllCodes[i] = "$REMOVE";
                                nToRemove++;
                            }
                        }
                    }
                }
            }
            fp.Close();
            string[] sToReturn = new string[sAllCodes.Length - nToRemove];
            int nSkipped = 0;
            for (int i = 0; i < sAllCodes.Length; i++)
            {
                if (sAllCodes[i] == "$REMOVE")
                {
                    nSkipped++;
                }
                else
                {
                    sToReturn[i - nSkipped] = sAllCodes[i];
                }
            }
            return sToReturn;
        
        }
        public string[] GetBarcodesOfItemsBySpec(decimal dMinAveSales, decimal dNOfDays, string sShopCode, string sCatCode)
        {
            frmProgressBar fp = new frmProgressBar("Searching For Applicable Items");
            fp.Show();
            string[] sAllCodes = GetCodesOfItemsInCategory(sCatCode, true);
            fp.pb.Maximum = sAllCodes.Length;
            int nToRemove = 0;
            for (int i = 0; i < sAllCodes.Length; i++)
            {
                fp.pb.Value = i;
                int nRecLoc = tStockStats.GetRecordNumberFromTwoFields(sAllCodes[i], 0, sShopCode, 35);
                if (nRecLoc == -1)
                {
                    sAllCodes[i] = "$REMOVE";
                    nToRemove++;
                }
                else
                {
                    string[] sStockStaData = tStockStats.GetRecordFrom(nRecLoc);
                    decimal dQtyInStock = Convert.ToDecimal(sStockStaData[36]);
                    decimal dAveSales = Convert.ToDecimal(sStockStaData[2]);
                    if (dAveSales < dMinAveSales)
                    {
                        sAllCodes[i] = "$REMOVE";
                        nToRemove++;
                    }
                    else
                    {
                        decimal dNumOfDaysStock = 9999;
                        if (dAveSales != 0)
                            dNumOfDaysStock = dQtyInStock / dAveSales;
                        if (dNumOfDaysStock > dNOfDays)
                        {
                            sAllCodes[i] = "$REMOVE";
                            nToRemove++;
                        }
                        else
                        {
                            string[] sMainStockInfo = GetMainStockInfo(sAllCodes[i]);
                            if (sMainStockInfo[5] != "1")
                            {
                                sAllCodes[i] = "$REMOVE";
                                nToRemove++;
                            }
                        }
                    }
                }
            }
            fp.Close();
            string[] sToReturn = new string[sAllCodes.Length - nToRemove];
            int nSkipped = 0;
            for (int i = 0; i < sAllCodes.Length; i++)
            {
                if (sAllCodes[i] == "$REMOVE")
                {
                    nSkipped++;
                }
                else
                {
                    sToReturn[i - nSkipped] = sAllCodes[i];
                }
            }
            return sToReturn;

        }

        public void DiscontinueItem(string sBarcode)
        {
            ChangeItemType(sBarcode, 3);
        }

        public void ChangeItemType(string sBarcode, int nNewType)
        {
            int nRecNum = -1;
            tStock.SearchForRecord(sBarcode, 0, ref nRecNum);
            if (nRecNum != -1)
            {
                tStock.EditRecordData(nRecNum, 5, nNewType.ToString());
                tStock.SaveToFile("MAINSTOC.DBF");
            }
        }

        public void ChangeCategoryOfItem(string sBarcode, string sNewCatCode)
        {
            int nRecLoc = -1;
            tStock.SearchForRecord(sBarcode, 0, ref nRecLoc);
            if (nRecLoc != -1)
            {
                tStock.EditRecordData(nRecLoc, 4, sNewCatCode);
                tStock.SaveToFile("MAINSTOC.DBF");
            }
        }
            

        public void OrderDetailsToFile(string sOrderNum)
        {
            this.FindMissingOrderLines();
            sOrderNum = sOrderNum.TrimStart(' ');
            const int nPageWidth = 80;
            TextWriter tWriter = new StreamWriter("REPORT.TXT", false);
            string[] sOrderHeader = tOrder.GetRecordFrom(sOrderNum, 0, true);
            tWriter.WriteLine("Order Number :" + sOrderNum + "/" + sOrderHeader[6]);
            tWriter.WriteLine();
            string[] sOurAdd = GetShopAddress(sOrderHeader[6]);
            string[] sSupDetails = GetSupplierDetails(sOrderHeader[1]);
            for (int i = 0; i < 6; i++)
            {
                string sToWrite = "";
                if (i == 0)
                    sToWrite = "To : ";
                else
                    sToWrite = "     ";
                sToWrite += sSupDetails[i + 1];
                while (sToWrite.Length < 35)
                    sToWrite += " ";
                if (i == 0)
                    sToWrite += "From : " + tSettings.GetRecordFrom("CompanyName", 0, true)[1];
                else
                    sToWrite += "       ";
                if (sOurAdd.Length + 1 > i && i != 0)
                    sToWrite += sOurAdd[i - 1];
                tWriter.WriteLine(sToWrite);
            }
            tWriter.WriteLine();
            string sTel = "Tel : " + sSupDetails[7];
            while (sTel.Length < 36)
                sTel += " ";
            sTel += "Tel : " + tSettings.GetRecordFrom(sOrderHeader[6] + "PhoneNumber", 0)[1];
            string sFax = "Fax : " + sSupDetails[8];
            while (sFax.Length < 23)
                sFax += " ";
            sFax += "Supplier Account : " + sSupDetails[10];
            tWriter.WriteLine(sTel);
            tWriter.WriteLine(sFax);
            int nEmailField = -1;
            if (tSettings.SearchForRecord("EmailAddress", 0, ref nEmailField) && tSettings.GetRecordFrom(nEmailField)[1] != "")
            {
                tWriter.WriteLine("E-Mail " + tSettings.GetRecordFrom(nEmailField)[1] + " with any queries.");
            }
            tWriter.WriteLine();
            string sHeader = "";
            while (sHeader.Length < nPageWidth)
                sHeader += "-";
            tWriter.WriteLine(sHeader);
            tWriter.WriteLine("Item Code      Description                    Quantity");
            tWriter.WriteLine(sHeader);
            int nOfItems = 0;
            string[,] sOrderInfo = tOrderLine.SearchAndGetAllMatchingRecords(0, sOrderNum, ref nOfItems, true);
            for (int i = 0; i < nOfItems; i++)
            {
                string sToPrint = GetItemCodeBySupplier(sOrderInfo[i, 2], sOrderHeader[1]);
                while (sToPrint.Length < 15)
                    sToPrint += " ";
                sToPrint += GetMainStockInfo(sOrderInfo[i, 2])[1];
                while (sToPrint.Length + sOrderInfo[i,3].Length < 54)
                    sToPrint += " ";
                sToPrint += sOrderInfo[i, 3];
                tWriter.WriteLine(sToPrint);
            }
            tWriter.Close();
            this.FindMissingOrderLines();
        }

        public void OrderDetailsToSpreadSheet(string sOrderNum)
        {
            sOrderNum = sOrderNum.TrimStart(' ');
            TextWriter tWriter = new StreamWriter("REPORT.CSV", false);
            string[] sOrderHeader = tOrder.GetRecordFrom(sOrderNum, 0, true);
            tWriter.WriteLine("Order Number :" + sOrderNum + "/" + sOrderHeader[6]);
            tWriter.WriteLine();
            string[] sOurAdd = GetShopAddress(sOrderHeader[6]);
            string[] sSupDetails = GetSupplierDetails(sOrderHeader[1]);
            for (int i = 0; i < 6; i++)
            {
                string sToWrite = "";
                if (i == 0)
                    sToWrite = "To :,";
                else
                    sToWrite = ",";
                sToWrite += sSupDetails[i + 1];
                if (i == 0)
                    sToWrite += ",From : ," + tSettings.GetRecordFrom("CompanyName", 0, true)[1];
                else
                    sToWrite += ",,";
                if (sOurAdd.Length + 1 > i && i != 0)
                    sToWrite += sOurAdd[i - 1];
                tWriter.WriteLine(sToWrite);
            }
            tWriter.WriteLine();
            string sTel = "Tel : " + sSupDetails[7] + ",";
            sTel += "Tel : " + tSettings.GetRecordFrom(sOrderHeader[6] + "PhoneNumber", 0)[1];
            string sFax = "Fax : " + sSupDetails[8] + ",";
            sFax += "Supplier Account : " + sSupDetails[10];
            tWriter.WriteLine(sTel);
            tWriter.WriteLine(sFax);
            int nEmailField = -1;
            if (tSettings.SearchForRecord("EmailAddress", 0, ref nEmailField) && tSettings.GetRecordFrom(nEmailField)[1] != "")
            {
                tWriter.WriteLine("E-Mail " + tSettings.GetRecordFrom(nEmailField)[1] + " with any queries.");
            }
            tWriter.WriteLine();
            tWriter.WriteLine("Item Code,Description,Quantity");
            tWriter.WriteLine();
            int nOfItems = 0;
            string[,] sOrderInfo = tOrderLine.SearchAndGetAllMatchingRecords(0, sOrderNum, ref nOfItems, true);
            for (int i = 0; i < nOfItems; i++)
            {
                string sToPrint = GetItemCodeBySupplier(sOrderInfo[i, 2], sOrderHeader[1]) + ",";
                sToPrint += GetMainStockInfo(sOrderInfo[i, 2])[1] + ",";
                sToPrint += sOrderInfo[i, 3];
                tWriter.WriteLine(sToPrint);
            }
            tWriter.Close();
            try
            {
                System.Diagnostics.Process.Start("EXCEL", "REPORT.CSV");
            }
            catch
            {
                ;
            }
        }

        public void OrderDetailsToFile(string sSupCode, string sShopCode, string[] sItemCodes, string[] sDesc, string[] sOutstanding, string[] sReceived, string sOrderNumber)
        {
            this.FindMissingOrderLines();
            const int nPageWidth = 80;
            TextWriter tWriter = new StreamWriter("REPORT.TXT", false);
            tWriter.WriteLine("Order Number: " + sOrderNumber);
            tWriter.WriteLine();
            string[] sOurAdd = GetShopAddress(sShopCode);
            string[] sSupDetails = GetSupplierDetails(sSupCode);
            for (int i = 0; i < 6; i++)
            {
                string sToWrite = "";
                if (i == 0)
                    sToWrite = "To : ";
                else
                    sToWrite = "     ";
                sToWrite += sSupDetails[i + 1];
                while (sToWrite.Length < 35)
                    sToWrite += " ";
                if (i == 0)
                    sToWrite += "From : " + tSettings.GetRecordFrom("CompanyName", 0, true)[1];
                else
                    sToWrite += "       ";
                if (sOurAdd.Length + 1 > i && i != 0)
                    sToWrite += sOurAdd[i - 1];
                tWriter.WriteLine(sToWrite);
            }
            tWriter.WriteLine();
            string sTel = "Tel : " + sSupDetails[7];
            while (sTel.Length < 36)
                sTel += " ";
            sTel += "Tel : " + tSettings.GetRecordFrom(sShopCode + "PhoneNumber", 0)[1];
            string sFax = "Fax : " + sSupDetails[8];
            while (sFax.Length < 23)
                sFax += " ";
            sFax += "Supplier Account : " + sSupDetails[10];
            tWriter.WriteLine(sTel);
            tWriter.WriteLine(sFax);
            tWriter.WriteLine();
            string sHeader = "";
            while (sHeader.Length < nPageWidth)
                sHeader += "-";
            tWriter.WriteLine(sHeader);
            tWriter.WriteLine("Item Code      Description                         O/S      Rec'd");
            tWriter.WriteLine(sHeader);
            int nOfItems = 0;
            for (int i = 0; i < sItemCodes.Length; i++)
            {
                string sToPrint = sItemCodes[i];
                while (sToPrint.Length < 15)
                    sToPrint += " ";
                sToPrint += sDesc[i];
                while (sToPrint.Length + sOutstanding[i].Length < 54)
                    sToPrint += " ";
                sToPrint += sOutstanding[i];
                while (sToPrint.Length + sReceived[i].Length < 63)
                    sToPrint += " ";
                sToPrint += sReceived[i];
                tWriter.WriteLine(sToPrint);
            }
            tWriter.Close();
            this.FindMissingOrderLines();
        }

        public string GetItemCodeBySupplier(string sBarcode, string sSupCode)
        {
            int nRecNum = tSupplierIndex.GetRecordNumberFromTwoFields(sBarcode, 0, sSupCode, 1);
            if (nRecNum != -1)
            {
                return tSupplierIndex.GetRecordFrom(nRecNum)[2];
            }
            else
                return sBarcode;
        }

        public void OrderDetailsToPrinter(string sOrderNum)
        {
            nLineLastPrinted = 0;
            nPrinterPage = 1;
            sReportTitle = "Purchase Order";
            rCurrentlyPrinting = ReportType.OrderInfo;
            OrderDetailsToFile(sOrderNum);
            PrinterSettings pSettings = new PrinterSettings();
            pSettings.PrinterName = this.PrinterToUse;
            PrintDocument pPrinter = new PrintDocument();
            pPrinter.PrinterSettings = pSettings;
            pPrinter.DocumentName = "Order Number " + sOrderNum;
            pPrinter.PrintPage += new PrintPageEventHandler(ReportPrintPage);
            pPrinter.Print();
        }

        public void OrderDetailsToPrinter(string sSupCode, string sShopCode, string[] sItemCodes, string[] sDesc, string[] sOutstanding, string[] sReceived, string sOrderNum)
        {
            nLineLastPrinted = 0;
            nPrinterPage = 1;
            sReportTitle = "Purchase Order";
            rCurrentlyPrinting = ReportType.OrderInfo;
            OrderDetailsToFile(sSupCode, sShopCode, sItemCodes, sDesc, sOutstanding, sReceived, sOrderNum);
            PrinterSettings pSettings = new PrinterSettings();
            pSettings.PrinterName = this.PrinterToUse;
            PrintDocument pPrinter = new PrintDocument();
            pPrinter.PrinterSettings = pSettings;
            pPrinter.DocumentName = "Printing Order";
            pPrinter.PrintPage += new PrintPageEventHandler(ReportPrintPage);
            pPrinter.Print();
        }

        public void EndOfPeriod(Period pPeriod)
        {
            ArchiveBackOffStuff(pPeriod);
            char cPeriodChar = ' ';
            switch (pPeriod)
            {
                case Period.Daily:
                    cPeriodChar = 'D';
                    break;
                case Period.Monthly:
                    cPeriodChar = 'M';
                    break;
                case Period.Weekly:
                    cPeriodChar = 'W';
                    break;
                case Period.Yearly:
                    cPeriodChar = 'Y';
                    break;
            }
            if (cPeriodChar == 'Y')
            {
                for (int i = 0; i < tStockStats.NumberOfRecords; i++)
                {
                    // Copy Year to Last Year
                    tStockStats.EditRecordData(i, tStockStats.FieldNumber("L" + cPeriodChar.ToString() + "QSOLD"), tStockStats.GetRecordFrom(i)[tStockStats.FieldNumber(cPeriodChar.ToString() + "QSOLD")]);
                    tStockStats.EditRecordData(i, tStockStats.FieldNumber("L" + cPeriodChar.ToString() + "GSALES"), tStockStats.GetRecordFrom(i)[tStockStats.FieldNumber(cPeriodChar.ToString() + "GSALES")]);
                    tStockStats.EditRecordData(i, tStockStats.FieldNumber("L" + cPeriodChar.ToString() + "NSALES"), tStockStats.GetRecordFrom(i)[tStockStats.FieldNumber(cPeriodChar.ToString() + "NSALES")]);
                    tStockStats.EditRecordData(i, tStockStats.FieldNumber("L" + cPeriodChar.ToString() + "COGS"), tStockStats.GetRecordFrom(i)[tStockStats.FieldNumber(cPeriodChar.ToString() + "COGS")]);
                    tStockStats.EditRecordData(i, tStockStats.FieldNumber("YOSLEVEL"), tStockStats.GetRecordFrom(i)[tStockStats.FieldNumber("YDELIVERED")]);
                    tStockStats.EditRecordData(i, tStockStats.FieldNumber("YOSCOST"), tStockStats.GetRecordFrom(i)[tStockStats.FieldNumber("YDELCOST")]);
                }
            }
            for (int i = 0; i < tStockStats.NumberOfRecords; i++)
            {
                tStockStats.EditRecordData(i, tStockStats.FieldNumber(cPeriodChar.ToString() + "QSOLD"), "0.00");
                tStockStats.EditRecordData(i, tStockStats.FieldNumber(cPeriodChar.ToString() + "GSALES"), "0.00");
                tStockStats.EditRecordData(i, tStockStats.FieldNumber(cPeriodChar.ToString() + "NSALES"), "0.00");
                tStockStats.EditRecordData(i, tStockStats.FieldNumber(cPeriodChar.ToString() + "COGS"), "0.00");
            }
            tStockStats.SaveToFile("STOCKSTA.DBF");
            if (pPeriod == Period.Weekly)
            {
                for (int i = 1; i <= 7; i++)
                {
                    for (int x = 0; x < Till.Length; x++)
                    {
                        if (File.Exists("TILL" + Till[x].Number.ToString() + "\\INGNG\\REPDATA" + i.ToString() + ".DBF"))
                            File.Delete("TILL" + Till[x].Number.ToString() + "\\INGNG\\REPDATA" + i.ToString() + ".DBF");
                        if (File.Exists("TILL" + Till[x].Number.ToString() + "\\INGNG\\TDATA" + i.ToString() + ".DBF"))
                            File.Delete("TILL" + Till[x].Number.ToString() + "\\INGNG\\TDATA" + i.ToString() + ".DBF");
                        if (File.Exists("TILL" + Till[x].Number.ToString() + "\\INGNG\\THDR" + i.ToString() + ".DBF"))
                            File.Delete("TILL" + Till[x].Number.ToString() + "\\INGNG\\THDR" + i.ToString() + ".DBF");
                        Till[x].CollectedMap = "NNNNNNN";
                        Till[x].SaveTillChanges(ref tTills);
                    }
                }
                BuildSalesIndex();
            }
        }

        public void UpdateTotalSales()
        {
            decimal dTotalSales = 0;
            foreach (Till t in Till)
            {
                dTotalSales += GetTakingsForDay(GetLastCollectionDate(), t.Number);
            }

            string sOrigDate = GetWeekCommencingDate();
            if (!sOrigDate.Contains("/"))
                sOrigDate = sOrigDate[0].ToString() + sOrigDate[1].ToString() + "/" + sOrigDate[2].ToString() + sOrigDate[3].ToString() + "/20" + sOrigDate[4].ToString() + sOrigDate[5].ToString();
            DateTime dt = DateTime.Parse(sOrigDate);
            dt = dt.AddDays(-1);
            string sNewDate = dt.Year.ToString();
            string sMonth = dt.Month.ToString();
            if (sMonth.Length < 2)
                sMonth = "0" + sMonth;
            string sDay = dt.Day.ToString();
            if (sDay.Length < 2)
                sDay = "0" + sDay;
            sNewDate += sMonth + sDay;

            bool bFound = false;
            for (int i = 0; i < tTotalSales.NumberOfRecords; i++)
            {
                if (tTotalSales.GetRecordFrom(i)[0] == sNewDate)
                {
                    tTotalSales.EditRecordData(i, 3, (Convert.ToDecimal(tTotalSales.GetRecordFrom(i)[3]) + dTotalSales).ToString());
                    bFound = true;
                    break;
                }
            }
            if (!bFound)
            {
                string[] sToAdd = { sNewDate, "0", "0", dTotalSales.ToString() };
                tTotalSales.AddRecord(sToAdd);
            }
            tTotalSales.SaveToFile("TOTSALES.DBF");
        }

       /* void BackupBeforeEndOfPeriod(Period p, string sDate)
        {
            // DDMMYY Date
            string sSaveLoc = "PreCollect\\";
            if (p == Period.Daily)
            {
                sSaveLoc += "Daily\\";
            }
            else if (p == Period.Weekly)
            {
                sSaveLoc += "Weekly\\";
            }
            else if (p == Period.Monthly)
            {
                sSaveLoc += "Monthly\\";
            }
            else if (p == Period.Yearly)
            {
                sSaveLoc += "Yearly\\";
            }
            sSaveLoc += sDate;
            if (!Directory.Exists(sSaveLoc))
                Directory.CreateDirectory(sSaveLoc);
            sSaveLoc += "\\";

            tAccStat.SaveToFile(sSaveLoc + "ACCSTAT.DBF");
            tCategory.SaveToFile(sSaveLoc + "CATEGORY.DBF");
            tCatGroupData.SaveToFile(sSaveLoc + "CATGPHDR.DBF");
            tCatGroupHeader.SaveToFile(sSaveLoc + "CATGRPDA.DBF");
            tCommissioners.SaveToFile(sSaveLoc + "COMMPPL.DBF");
            tCommItems.SaveToFile(sSaveLoc + "COMMISSI.DBF");
            //tCustomer.SaveToFile(sSaveLoc + "CUSTOMER.DBF");
            tMultiData.SaveToFile(sSaveLoc + "MULTIDAT.DBF");
            tMultiHeader.SaveToFile(sSaveLoc + "MULTIHDR.DBF");
            tOrder.SaveToFile(sSaveLoc + "ORDER.DBF");
            tOrderLine.SaveToFile(sSaveLoc + "ORDERLIN.DBF");
            tSettings.SaveToFile(sSaveLoc + "SETTINGS.DBF");
            tShop.SaveToFile(sSaveLoc + "SHOP.DBF");
            tStaff.SaveToFile(sSaveLoc + "STAFF.DBF");
            tStock.SaveToFile(sSaveLoc + "MAINSTOC.DBF");
            tStockStats.SaveToFile(sSaveLoc + "STOCKSTA.DBF");
            tSupplier.SaveToFile(sSaveLoc + "SUPPLIER.DBF");
            tSupplierIndex.SaveToFile(sSaveLoc + "SUPINDEX.DBF");
            tTills.SaveToFile(sSaveLoc + "TILL.DBF");
            tTotalSales.SaveToFile(sSaveLoc + "TOTSALES.DBF");
            tVATRates.SaveToFile(sSaveLoc + "VAT.DBF");
        } */

        /// <summary>
        /// Updates the Stock Length Database, which keeps track of how long items have been out of stock for.
        /// </summary>
        private void UpdateStockLengthDatabase()
        {
            tStock.SortTable();
            tStockLength.SortTable();
            tStockStats.SortTable();
            frmProgressBar fp = new frmProgressBar("Updating Stock Length Database");
            fp.pb.Maximum = tStockStats.NumberOfRecords;
            fp.pb.Value = 0;
            fp.Show();
            
            // Start of the faulty code
            /*
            int nMainStockPos = 0;
            int nStockLengthPos = 0;
            for (int i = 0; i < tStockStats.NumberOfRecords; i++)
            {
                fp.pb.Value = i;
                // Match the main stock and stock stats record first
                while (tStockStats.GetRecordFrom(i)[0].ToUpper() != tStock.GetRecordFrom(nMainStockPos)[0].ToUpper())
                    nMainStockPos++;

                if (tStock.GetRecordFrom(nMainStockPos)[5] == "1")
                {
                    // Is a stock item, so needs to be in stocklength
                    if (nStockLengthPos >= tStockLength.NumberOfRecords || tStockStats.GetRecordFrom(i)[0].ToUpper() != tStockLength.GetRecordFrom(nStockLengthPos)[0].ToUpper())
                    {
                        // Needs to be added
                        string[] sToAdd = { tStockStats.GetRecordFrom(i)[0].ToUpper(), tStockStats.GetRecordFrom(i)[35], "0", "0" };
                        tStockLength.AddRecord(sToAdd);
                        tStockLength.SortTable();
                    }

                    string[] stockLengthRec = tStockLength.GetRecordFrom(nStockLengthPos);

                    // Update the total number of days that the item has been in stock for
                    int nTotalDays = Convert.ToInt32(stockLengthRec[2]);
                    nTotalDays++;
                    tStockLength.EditRecordData(nStockLengthPos, 2, nTotalDays.ToString());

                    // Check to see if the item is out of stock
                    if (Convert.ToDecimal(tStockStats.GetRecordFrom(i)[36]) <= 0)
                    {
                        // Now increase the item's out of stock day count
                        decimal nOutOfStock = Convert.ToDecimal(stockLengthRec[3]);
                        nOutOfStock++;
                        tStockLength.EditRecordData(nStockLengthPos, 3, nOutOfStock.ToString());
                    }

                    nStockLengthPos++;
                }
            }
            */
            // End of the faulty code

            // Temporarily revert to the previous code
            string[] sRecord = null;
            int nRecNum = -1;
            string[] sToAdd = null;
            string[] stockLengthRec = null;
            int nTotalDays = -1;

            for (int i = 0; i < tStockStats.NumberOfRecords; i++)
            {
                fp.pb.Value = i;
                sRecord = tStockStats.GetRecordFrom(i);
                if (tStock.GetRecordFrom(sRecord[0], 0, true)[5] == "1")
                {
                    // Find this item in the StockLength database if possible, or add it otherwise
                    nRecNum = tStockLength.GetRecordNumberFromTwoFields(sRecord[0], 0, sRecord[35], 1);
                    if (nRecNum == -1)
                    {
                        // Need to add the item
                        sToAdd = new string[] { sRecord[0], sRecord[35], "0", "0" };
                        tStockLength.AddRecord(sToAdd);
                        nRecNum = tStockLength.GetRecordNumberFromTwoFields(sRecord[0], 0, sRecord[35], 1);
                    }

                    stockLengthRec = tStockLength.GetRecordFrom(nRecNum);

                    // Update the total number of days that the item has been in stock for
                    nTotalDays = Convert.ToInt32(stockLengthRec[2]);
                    nTotalDays++;
                    tStockLength.EditRecordData(nRecNum, 2, nTotalDays.ToString());

                    // Check to see if the item is out of stock
                    if (Convert.ToDecimal(sRecord[36]) <= 0)
                    {
                        // Now increase the item's out of stock day count
                        int nOutOfStock = Convert.ToInt32(stockLengthRec[3]);
                        nOutOfStock++;
                        tStockLength.EditRecordData(nRecNum, 3, nOutOfStock.ToString());
                    }

                }
            }
            tStockLength.SaveToFile("STOCKLEN.DBF");
            fp.Close();
        }


        public string GetCollectionDate(int nDayOfWeek, string sTillCode)
        {
            try
            {
                Table tRepData = new Table(sTDir + "TILL" + sTillCode + "\\INGNG\\REPDATA" + nDayOfWeek.ToString() + ".DBF");
                return tRepData.GetRecordFrom(0)[1];
            }
            catch
            {
                return "N/A";
            }
        }

        public string[] GetListOfLowestLevelCategories()
        {
            string[] sCats = new string[tCategory.NumberOfRecords];
            for (int i = 0; i < tCategory.NumberOfRecords; i++)
            {
                sCats[i] = tCategory.GetRecordFrom(i)[0];
            }
            int nToRemove = 0;
            for (int i = 0; i < sCats.Length; i++)
            {
                for (int x = 0; x < sCats.Length; x++)
                {
                    if (i != x && sCats[x].StartsWith(sCats[i]) && sCats[x].Length > sCats[i].Length)
                    {
                        sCats[i] = "$REMOVE";
                        nToRemove++;
                        break;
                    }
                }
            }
            string[] sToReturn = new string[sCats.Length - nToRemove];
            int nSkipped = 0;
            for (int i = 0; i < sCats.Length; i++)
            {
                if (sCats[i] == "$REMOVE")
                {
                    nSkipped++;
                }
                else
                {
                    sToReturn[i - nSkipped] = sCats[i];
                }
            }
            return sToReturn;
        }

        decimal NetPriceForItem(string sBarcode)
        {
            decimal dVATRate = GetVATRateFromCode(sBarcode);
            dVATRate /= 100;
            dVATRate += 1;
            decimal dGrossPrice = Convert.ToDecimal(tStock.GetRecordFrom(sBarcode, 0, true)[2]);
            return dGrossPrice / dVATRate;
        }


        public void StockValuationToFile()
        {
            frmProgressBar pb = new frmProgressBar("Generating Stock Valuation");
            pb.Show();
            string[] sCats = GetListOfLowestLevelCategories();
            TextWriter tWriter = new StreamWriter("REPORT.TXT");
            const int nPageWidth = 80;
            string sHeader = "";
            while (sHeader.Length < nPageWidth)
                sHeader += "-";
            tWriter.WriteLine(sHeader);
            tWriter.WriteLine("                                 Value     Value      Net       Gross    ");
            tWriter.WriteLine("Code       Category              @ Cost    @ Repl     Sell       Sell    ");
            tWriter.WriteLine(sHeader);
            decimal dReportCostTotal = 0;
            decimal dReportReplTotal = 0;
            decimal dReportNetTotal = 0;
            decimal dReportGrossTotal = 0;
            for (int i = 0; i < tShop.NumberOfRecords; i++) // Each Shop
            {
                string sShopCode = tShop.GetRecordFrom(i)[0];
                string sShopName = GetShopNameFromCode(sShopCode);
                decimal dShopCostTotal = 0;
                decimal dShopReplTotal = 0;
                decimal dShopNetTotal = 0;
                decimal dShopGrossTotal = 0;
                tWriter.WriteLine(sShopName);
                pb.pb.Maximum = sCats.Length;
                pb.pb.Value = 0;
                for (int x = 0; x < sCats.Length; x++) // Each Category
                {
                    decimal dCatCostTotal = 0;
                    decimal dCatReplTotal = 0;
                    decimal dCatNetTotal = 0;
                    decimal dCatGrossTotal = 0;
                    int nOfResults = 0;
                    string[,] sItems = tStock.SearchAndGetAllMatchingRecords(4, sCats[x], ref nOfResults, true);
                    for (int z = 0; z < nOfResults; z++)
                    {
                        int nRecNum = tStockStats.GetRecordNumberFromTwoFields(sItems[z, 0], 0, sShopCode, 35);
                        if (nRecNum != -1)
                        {
                            string[] sStockSta = tStockStats.GetRecordFrom(nRecNum);
                            dCatCostTotal += (Convert.ToDecimal(sStockSta[1]) * Convert.ToDecimal(sStockSta[36]));
                            dCatReplTotal += (Convert.ToDecimal(sItems[z, 8]) * Convert.ToDecimal(sStockSta[36]));
                            // Calculate Net Price
                            decimal dVATRate = Convert.ToDecimal(tVATRates.GetRecordFrom(sItems[z, 3], 0, true)[2]);
                            decimal dNet = Math.Round(Convert.ToDecimal(sItems[z, 2]) / (1.0m + (dVATRate / 100)), 2);
                            dCatNetTotal += (dNet * Convert.ToDecimal(sStockSta[36]));
                            dCatGrossTotal += (Convert.ToDecimal(sItems[z, 2]) * Convert.ToDecimal(sStockSta[36]));
                        }
                    }
                    string sToWrite = sCats[x];
                    while (sToWrite.Length < 11)
                        sToWrite += " ";
                    sToWrite += GetCategoryDesc(sCats[x]);
                    while (sToWrite.Length + Math.Round(dCatCostTotal).ToString().Length < 42)
                        sToWrite += " ";
                    sToWrite += Math.Round(dCatCostTotal).ToString();
                    while (sToWrite.Length + Math.Round(dCatReplTotal).ToString().Length < 53)
                        sToWrite += " ";
                    sToWrite += Math.Round(dCatReplTotal).ToString();
                    while (sToWrite.Length + Math.Round(dCatNetTotal).ToString().Length < 63)
                        sToWrite += " ";
                    sToWrite += Math.Round(dCatNetTotal).ToString();
                    while (sToWrite.Length + Math.Round(dCatGrossTotal).ToString().Length < 73)
                        sToWrite += " ";
                    sToWrite += Math.Round(dCatGrossTotal).ToString();
                    tWriter.WriteLine(sToWrite);
                    dShopGrossTotal += dCatGrossTotal;
                    dShopNetTotal += dCatNetTotal;
                    dShopCostTotal += dCatCostTotal;
                    dShopReplTotal += dCatReplTotal;
                    pb.pb.Value = x;
                }
                string sToWrite2 = "";
                while (sToWrite2.Length + "Total ".Length + sShopName.Length < 32)
                    sToWrite2 += " ";
                sToWrite2 += "Total " + sShopName + ": ";
                while (sToWrite2.Length + Math.Round(dShopCostTotal).ToString().Length < 42)
                    sToWrite2 += " ";
                sToWrite2 += Math.Round(dShopCostTotal).ToString();
                while (sToWrite2.Length + Math.Round(dShopReplTotal).ToString().Length < 53)
                    sToWrite2 += " ";
                sToWrite2 += Math.Round(dShopReplTotal).ToString();
                while (sToWrite2.Length + Math.Round(dShopNetTotal).ToString().Length < 63)
                    sToWrite2 += " ";
                sToWrite2 += Math.Round(dShopNetTotal).ToString();
                while (sToWrite2.Length + Math.Round(dShopGrossTotal).ToString().Length < 73)
                    sToWrite2 += " ";
                sToWrite2 += Math.Round(dShopGrossTotal).ToString();
                tWriter.WriteLine(sHeader);
                tWriter.WriteLine(sToWrite2);
                tWriter.WriteLine(sHeader);
                dReportCostTotal += dShopCostTotal;
                dReportGrossTotal += dShopGrossTotal;
                dReportNetTotal += dShopNetTotal;
                dReportReplTotal += dShopReplTotal;
            }
            string sToWrite3 = "";
            while (sToWrite3.Length + "Report Total".Length < 32)
                sToWrite3 += " ";
            sToWrite3 += "Report Total " + ": ";
            while (sToWrite3.Length + Math.Round(dReportCostTotal).ToString().Length < 42)
                sToWrite3 += " ";
            sToWrite3 += Math.Round(dReportCostTotal).ToString();
            while (sToWrite3.Length + Math.Round(dReportReplTotal).ToString().Length < 53)
                sToWrite3 += " ";
            sToWrite3 += Math.Round(dReportReplTotal).ToString();
            while (sToWrite3.Length + Math.Round(dReportNetTotal).ToString().Length < 63)
                sToWrite3 += " ";
            sToWrite3 += Math.Round(dReportNetTotal).ToString();
            while (sToWrite3.Length + Math.Round(dReportGrossTotal).ToString().Length < 73)
                sToWrite3 += " ";
            sToWrite3 += Math.Round(dReportGrossTotal).ToString();
            tWriter.WriteLine(sToWrite3);
            pb.Close();
            tWriter.Close();
        }
        public void StockValuationToPrinter()
        {
            StockValuationToFile();
            nLineLastPrinted = 4;
            nPrinterPage = 1;
            sReportTitle = "Stock Valuation Report";
            rCurrentlyPrinting = ReportType.StockValuationReport;
            PrinterSettings pSettings = new PrinterSettings();
            pSettings.PrinterName = this.PrinterToUse;
            PrintDocument pPrinter = new PrintDocument();
            pPrinter.PrinterSettings = pSettings;
            pPrinter.DocumentName = "Stock Valuation Report";
            pPrinter.PrintPage += new PrintPageEventHandler(ReportPrintPage);
            pPrinter.Print();
            
        }

        public decimal GetSalesForWeek(int nWeekNum, int nYear)
        {
            return 0;
        }

        public string[] GetListOfItemsByCatAndSup(string sShopCode, string sCatCode, string sSupCode)
        {
            frmProgressBar fp = new frmProgressBar("Searching For Items");
            int nRecCount = 0;
            string[] sCodes = new string[0];
            if (sSupCode.Length > 0)
            {
                string[] sSupItems = tSupplierIndex.SearchAndGetAllMatchingRecords(1, sSupCode, ref nRecCount, true, 0);
                fp.pb.Maximum = nRecCount;
                fp.Show();
                for (int i = 0; i < nRecCount; i++)
                {
                    string[] sMainStockInfo = tStock.GetRecordFrom(sSupItems[i], 0, true);
                    if (sMainStockInfo.Length > 1 && sMainStockInfo[4].StartsWith(sCatCode))
                    {
                        //string[] sStockStaInfo = tStockStats.GetRecordFrom(sSupItems[i], 0, true);
                        if (tStockStats.GetRecordNumberFromTwoFields(sSupItems[i], 0, sShopCode, 35) != -1)
                        {
                            Array.Resize<string>(ref sCodes, sCodes.Length + 1);
                            sCodes[sCodes.Length - 1] = sSupItems[i];
                        }
                    }

                    fp.pb.Value = i;
                }
                fp.Close();
            }
            else
            {
                string[,] sCatItems = tStock.SearchAndGetAllMatchingRecords(4, sCatCode, ref nRecCount);
                fp.pb.Maximum = nRecCount;
                fp.Show();
                for (int i = 0; i < nRecCount; i++)
                {
                    if (tStockStats.GetRecordNumberFromTwoFields(sCatItems[i,0], 0, sShopCode, 35) != -1 && sCatItems[i,4].StartsWith(sCatCode))
                    {
                        Array.Resize<string>(ref sCodes, sCodes.Length + 1);
                        sCodes[sCodes.Length - 1] = sCatItems[i,0];
                    }
                    fp.pb.Value = i;
                }
                fp.Close();
            }
            Array.Sort(sCodes);
            return sCodes;
        }

        private int DayNumber(string sDateToday)
        {
            DateTime dtCashup = new DateTime(Convert.ToInt32(sDateToday[4].ToString() + sDateToday[5].ToString()),
                Convert.ToInt32(sDateToday[2].ToString() + sDateToday[3].ToString()),
                Convert.ToInt32(sDateToday[0].ToString() + sDateToday[1].ToString()));
            switch (dtCashup.DayOfWeek.ToString().ToUpper())
            {
                case "SUNDAY":
                    return 1;
                    break;
                case "MONDAY":
                    return 2;
                    break;
                case "TUESDAY":
                    return 3;
                    break;
                case "WEDNESDAY":
                    return 4;
                    break;
                case "THURSDAY":
                    return 5;
                    break;
                case "FRIDAY":
                    return 6;
                    break;
                case "SATURDAY":
                    return 7;
                    break;
            }
            return 0;
        }

        public string[] GetListOfTillTransactionNumbers(int nTillNumber, string sDate)
        {
            int nDayNum = DayNumber(sDate);
            Table tRepData = new Table(sTDir + "TILL" + nTillNumber.ToString() + "\\INGNG\\REPDATA" + nDayNum.ToString() + ".DBF");

            decimal fStartPos = Convert.ToDecimal(tRepData.GetRecordFrom("START", 1)[3]);
            fStartPos *= 100;

            decimal fEndPos = Convert.ToDecimal(tRepData.GetRecordFrom("END", 1)[3]);
            fEndPos *= 100;

            int nOfTransactions = Convert.ToInt32(fEndPos - fStartPos) + 1;
            string[] sToReturn = new string[nOfTransactions];

            for (int i = 0; i < nOfTransactions; i++)
            {
                sToReturn[i] = Math.Round(fStartPos + i).ToString();
            }

            return sToReturn;
        }

        public string[,] GetTransactionInfo(string sTransactionNumber, int nTillNum, string sDate)
        {
            // First array element in format { NumberOfItems, NumberOfPaymentMethods, TransactionDateTime, SpecialTransaction }
            // Item array in format { Item code, Item Description, Price Paid, Discount, Quantity }
            // Payment method array format { PaymentCode, Amount, Blank, Blank, Blank }
            //
            // SpecialTransactions can be CASHPAIDOUT, SPECIFICREFUND, VOID

            Table tRepData = new Table(sTDir + "TILL" + nTillNum.ToString() + "\\INGNG\\REPDATA" + DayNumber(sDate).ToString() + ".DBF");
            Table tTData = new Table(sTDir + "TILL" + nTillNum.ToString() + "\\INGNG\\TDATA" + DayNumber(sDate).ToString() + ".DBF");
            Table tTHDR = new Table(sTDir + "TILL" + nTillNum.ToString() + "\\INGNG\\THDR" + DayNumber(sDate).ToString() + ".DBF");

            int nTillArrayPos = -1;
            for (int i = 0; i < Till.Length; i++)
            {
                if (Till[i].Number == nTillNum)
                    nTillArrayPos = i;
            }
            string sShopCode = "";
            if (nTillArrayPos != -1)
                sShopCode = Till[nTillArrayPos].ShopCode;

            int nOfItems = 0;
            string[,] sTDATARecords = tTData.SearchAndGetAllMatchingRecords(0, sTransactionNumber, ref nOfItems);
            int nOfPaymentMethods = 0;
            string[,] sPaymentMethods = tTHDR.SearchAndGetAllMatchingRecords(0, sTransactionNumber, ref nOfPaymentMethods);
            nOfPaymentMethods -= 1;
            string[,] sToReturn = new string[nOfItems + nOfPaymentMethods + 1, 5];
            sToReturn[0, 0] = nOfItems.ToString();
            sToReturn[0, 1] = nOfPaymentMethods.ToString();
            sToReturn[0, 2] = sPaymentMethods[0, 5];
            if (sPaymentMethods[0, 4].TrimEnd(' ') != "SALE")
            {
                switch (sPaymentMethods[0, 4].TrimEnd(' '))
                {
                    case "CAPO":
                        sToReturn[0, 3] = "CASHPAIDOUT";
                        break;
                    case "SREF":
                        sToReturn[0, 3] = "SPECIFICREFUND";
                        break;
                    case "GREF":
                        sToReturn[0, 3] = "GENERALREFUND";
                        break;
                    case "RONA":
                        sToReturn[0, 3] = "RECEIVEDONACCOUNT";
                        break;
                }
            }
            else
            {
                sToReturn[0, 3] = "SALE";
                if (sPaymentMethods[0, 2].TrimEnd('\0') == "1")
                {
                    sToReturn[0, 3] = "VOID";
                    string sUserNumVoided = sPaymentMethods[0, 5].TrimEnd(' ')[sPaymentMethods[0, 5].TrimEnd(' ').Length - 3].ToString() + sPaymentMethods[0, 5][sPaymentMethods[0, 5].TrimEnd(' ').Length - 2].ToString() + sPaymentMethods[0, 5][sPaymentMethods[0, 5].TrimEnd(' ').Length - 1].ToString();
                    sUserNumVoided = sUserNumVoided.TrimStart(' ');
                    int nUserNum = 0;
                    try
                    {
                        nUserNum = Convert.ToInt32(sUserNumVoided);
                    }
                    catch
                    {
                        nUserNum = 0;
                        throw new NotSupportedException("Could not work out user number that voided this transaction. Press continue to user User number 1");
                    }
                    string sUserName = "";
                    if (nUserNum < 100)
                        sUserName = GetStaffName(sShopCode, nUserNum);
                    else
                        sUserName = "???";
                    sToReturn[0, 3] += "," + sUserName;
                }
            }
            float fTotalValueOfTransaction = 0.0f;
            for (int i = 1; i <= nOfItems; i++)
            {
                sToReturn[i, 0] = sTDATARecords[i - 1, 3]; // Barcode
                sToReturn[i, 1] = sTDATARecords[i - 1, 4]; // Description
                sToReturn[i, 2] = (Convert.ToDecimal(sTDATARecords[i - 1, 6]) - Convert.ToDecimal(sTDATARecords[i - 1, 8])).ToString(); // Price Paid
                fTotalValueOfTransaction += (float)Convert.ToDecimal(sToReturn[i, 2]);
                sToReturn[i, 3] = sTDATARecords[i - 1, 8]; // Discount
                sToReturn[i, 4] = sTDATARecords[i - 1, 5]; // Quantity
            }
            float fTotalAmountPaid = 0.0f;
            for (int i = nOfItems + 1; i <= (nOfItems + nOfPaymentMethods); i++)
            {
                sToReturn[i, 0] = sPaymentMethods[i - (nOfItems), 4];
                if (sToReturn[i, 0].StartsWith("CRCD"))
                    sToReturn[i, 0] += sPaymentMethods[i - nOfItems, 5].TrimEnd(' ');
                if (sToReturn[i, 0].StartsWith("CHRG"))
                    sToReturn[i, 0] += "," + sPaymentMethods[i - (nOfItems), 5] + "," + sTransactionNumber;
                sToReturn[i, 1] = sPaymentMethods[i - (nOfItems), 3];
                fTotalAmountPaid += (float)Convert.ToDecimal(sToReturn[i, 1]);
            }
            float fExcess = (fTotalAmountPaid - fTotalValueOfTransaction);
            if (fExcess > 0.0f)
            {
                for (int i = 0; i < nOfPaymentMethods; i++)
                {
                    if (sToReturn[i + 1 + nOfItems, 0].StartsWith("CASH") && (float)Convert.ToDecimal(sToReturn[i + 1 + nOfItems, 1]) > fExcess)
                    {
                        sToReturn[i + 1 + nOfItems, 2] = fExcess.ToString();
                    }
                }
            }
            return sToReturn;
        }

        public string GetStaffName(string sShopCode, int nNumber)
        {
            int nRecNum = tStaff.GetRecordNumberFromTwoFields(nNumber.ToString(), 0, sShopCode, 1);
            if (nRecNum != -1)
                return tStaff.GetRecordFrom(nRecNum)[2];
            else
                return "Unknown";
        }

        public string[] ReturnSensibleDateTimeString(string sInput, string sShopCode)
        {
            string sDate = "";
            string sTime = "";
            string sUserID = "";
            if (sInput.Contains("/"))
            {
                for (int i = 0; i < 8; i++)
                {
                    sDate += sInput[i].ToString();
                }
                for (int i = 8; i < 13; i++)
                {
                    sTime += sInput[i].ToString();
                }
                for (int i = 16; i < sInput.Length; i++)
                {
                    if (sInput[i] != ' ')
                        sUserID += sInput[i].ToString();
                }
                sUserID = sUserID.TrimEnd('\0');
            }
            else
            {
                sDate = sInput[4].ToString() + sInput[5].ToString() + "/" + sInput[2].ToString() + sInput[3].ToString() + "/"
                    + sInput[0].ToString() + sInput[1].ToString();
                sTime = sInput[6].ToString() + sInput[7].ToString() + ":" + sInput[8].ToString() + sInput[9].ToString();
                sUserID = sInput[10].ToString() + sInput[11].ToString();
                if (sUserID.StartsWith("0"))
                    sUserID = sUserID[1].ToString();
            }

            int nStaffNum = Convert.ToInt32(sUserID);
            string sStaffName = GetStaffName(sShopCode, nStaffNum);

            string[] sToReturn = new string[2];
            sToReturn[0] = sDate + " " + sTime;
            sToReturn[1] = sStaffName;
            return sToReturn;
        }

        public string GetTillShopCode(int nTillNum)
        {
            for (int i = 0; i < Till.Length; i++)
            {
                if (Till[i].Number == nTillNum)
                    return Till[i].ShopCode;
            }
            return "";
        }

        public string[] GetCreditCards()
        {
            int nOfCards = Convert.ToInt32(tSettings.GetRecordFrom("NumberOfCards", 0, true)[1]);
            string[] sCards = new string[nOfCards];
            for (int i = 0; i < sCards.Length; i++)
            {
                string sCardNum = i.ToString();
                if (sCardNum.Length < 2)
                    sCardNum = "0" + sCardNum;
                sCards[i] = "CRD" + sCardNum;
                // Now get the card description from SETTINGS
                sCards[i] = tSettings.GetRecordFrom(sCards[i], 0)[1];
            }
            return sCards;
        }

        public string GetPaymentDescription(string sPaymentCode)
        {
            string[] sCreditCards = GetCreditCards();
            sPaymentCode = sPaymentCode.TrimEnd('\0');
            try
            {
                switch (sPaymentCode)
                {
                    case "CASH":
                        return "Cash";
                        break;
                    case "CRCD":
                        return "Credit Card";
                        break;
                    case "CRCD1":
                        return "Credit Card (" + sCreditCards[0] + ")";
                        break;
                    case "CRCD2":
                        return "Credit Card (" + sCreditCards[1] + ")";
                        break;
                    case "CRCD3":
                        return "Credit Card (" + sCreditCards[2] + ")";
                        break;
                    case "CRCD4":
                        return "Credit Card (" + sCreditCards[3] + ")";
                        break;
                    case "CRCD5":
                        return "Credit Card (" + sCreditCards[4] + ")";
                        break;
                    case "CRCD6":
                        return "Credit Card (" + sCreditCards[5] + ")";
                        break;
                    case "CRCD7":
                        return "Credit Card (" + sCreditCards[6] + ")";
                        break;
                    case "CRCD8":
                        return "Credit Card (" + sCreditCards[7] + ")";
                        break;
                    case "CRCD9":
                        return "Credit Card (" + sCreditCards[8] + ")";
                        break;
                    case "CHEQ":
                        return "Cheque";
                        break;
                    case "DEPO":
                        return "Deposit Paid";
                        break;
                    case "VOUC":
                        return "Voucher";
                        break;
                    case "ACNT":
                        return "";
                        break;
                }
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("A payment method was used that is not present on this computer. The method was " + sPaymentCode + ". Please do a full upload");
                throw new ArgumentOutOfRangeException("Payment method " + sPaymentCode + " is not found in SETTINGS.DBF");
            }
            string[] sSplit = sPaymentCode.Split(',');
            if (sSplit[0] == "CHRG")
            {
                // Charged to account
                return "CHARGED TO A/C " + sSplit[1].Trim();
            }
            return "UnknownPayment";
        }

        public decimal GetTakingsForDay(string sDate, int nTill)
        {
            Table tRepData = new Table(sTDir + "TILL" + nTill.ToString() + "\\INGNG\\REPDATA" + DayNumber(sDate) + ".DBF");
            try
            {
                int nRecNum = 0;
                if (tRepData.SearchForRecord("DEPO", 1, ref nRecNum))
                {
                    return Convert.ToDecimal(tRepData.GetRecordFrom("NOTRAN", 1, true)[3]) - Convert.ToDecimal(tRepData.GetRecordFrom("DEPO", 1, true)[3]);
                }
                else
                {
                    return Convert.ToDecimal(tRepData.GetRecordFrom("NOTRAN", 1, true)[3]);
                }
            }
            catch
            {
                return 0;
            }
        }

        public class PaymentMethod
        {
            /// <summary>
            /// The code of the payment method
            /// </summary>
            string sPMName;
            /// <summary>
            /// The amount paid
            /// </summary>
            decimal fAmount;
            /// <summary>
            /// The amount paid without change given
            /// If £6 is due, the customer may give £10. This variable will hold 10, fAmount will hold 6 
            /// </summary>
            decimal fGross = 0.0m;

            /// <summary>
            /// Sets up the payment method
            /// </summary>
            /// <param name="name">The code of the payment method</param>
            /// <param name="flAmount">The amount paid using this payment method</param>
            public void SetPaymentMethod(string name, decimal flAmount)
            {
                sPMName = name;
                fAmount = flAmount;
            }
            /// <summary>
            /// Sets up the payment method
            /// </summary>
            /// <param name="name">The code of the payment method</param>
            /// <param name="flAmount">The final amount paid</param>
            /// <param name="flGross">The gross amount paid (possibly in excess of the amount due)</param>
            public void SetPaymentMethod(string name, decimal flAmount, decimal flGross)
            {
                sPMName = name;
                fAmount = flAmount;
                fGross = flGross + flAmount;
            }

            /// <summary>
            /// The amount paid using this payment method
            /// </summary>
            public decimal Amount
            {
                get
                {
                    return fAmount;
                }
            }

            /// <summary>
            /// The total amount received on this payment method, including excess
            /// </summary>
            public decimal Excess
            {
                get
                {
                    return fGross;
                }
            }

            /// <summary>
            /// Gets the payment code
            /// </summary>
            public string PMType
            {
                get
                {
                    return sPMName;
                }
            }
        }
        public void ReprintTransactionReceipt(int nTransactionNumber, int nTillNum, string sDate)
        {
            ClearReportFile();
            string[,] sTransactionInfo = GetTransactionInfo(nTransactionNumber.ToString(), nTillNum, sDate);
            // First array element in format { NumberOfItems, NumberOfPaymentMethods, TransactionDateTime, SpecialTransaction }
            // Item array in format { Item code, Item Description, Price Paid, Discount, Quantity }
            // Payment method array format { PaymentCode, Amount, Blank, Blank, Blank }
            //
            // SpecialTransactions can be CASHPAIDOUT, SPECIFICREFUND, VOID
            int nOfItems = Convert.ToInt32(sTransactionInfo[0, 0]);
            int nOfItemIncMulQty = 0;
            int nOfPaymentMethods = Convert.ToInt32(sTransactionInfo[0, 1]);
            string sDateTime = sTransactionInfo[0, 2];
            string sSpecialTransaction = sTransactionInfo[0, 3];
            string[] sFooterData = ReturnSensibleDateTimeString(sDateTime, GetTillShopCode(nTillNum));
            if (sSpecialTransaction == "CASHPAIDOUT")
            {
                string[] sInfo = ReturnSensibleDateTimeString(sDateTime.TrimEnd('\0'), GetTillShopCode(nTillNum));
                PrintCashPaidOut(-Convert.ToDecimal(sTransactionInfo[1, 1]), sInfo[0], sInfo[1], nTransactionNumber, nTillNum);
            }
            else if (sSpecialTransaction == "SPECIFICREFUND")
            {
                string sItemDesc = sTransactionInfo[1, 1];
                decimal fAmountRefunded = -Convert.ToDecimal(sTransactionInfo[2, 1]);
                PaymentMethod pm = new PaymentMethod();
                pm.SetPaymentMethod(sTransactionInfo[2, 0], 0.0m, fAmountRefunded);
                int nQuantityRefunded = -Convert.ToInt32(sTransactionInfo[1, 4]);
                PrintSpecificRefund(sItemDesc, fAmountRefunded, pm, nQuantityRefunded, true, nTransactionNumber, nTillNum, sFooterData[1], sFooterData[0]);
            }
            else if (sSpecialTransaction == "GENERALREFUND")
            {
                decimal fGeneralRefundAmount = -Convert.ToDecimal(sTransactionInfo[1, 1]);
                PaymentMethod pm = new PaymentMethod();
                pm.SetPaymentMethod(sTransactionInfo[1, 0], 0.0m, fGeneralRefundAmount);
                PrintGeneralRefund(pm, nTransactionNumber, sFooterData[1], sFooterData[0], nTillNum);
            }
            else if (sSpecialTransaction == "RECEIVEDONACCOUNT")
            {
                PrintReceiptHeader(GetTillShopCode(nTillNum));
                string[] sToSend = { "", CentralisePrinterText("No transaction to reprint."), "", "", CentralisePrinterText("Till re-written by Thomas Wormald.") };
                SendLinesToPrinter(sToSend);
                PrintReceiptFooter(sFooterData[1], sFooterData[0], "", nTillNum);
                EmptyPrinterBuffer();
            }
            else
            {
                PrintReceiptHeader(GetTillShopCode(nTillNum));
                PrintReceiptDescAndPriceTitles();
                decimal fTotalDue = 0.0m;
                // Normal tranasaction
                for (int i = 1; i <= nOfItems; i++)
                {
                    string[] sItemInfo = { sTransactionInfo[i, 0], sTransactionInfo[i, 1], sTransactionInfo[i, 2], sTransactionInfo[i, 3], sTransactionInfo[i, 4] };
                    PrintItem(sItemInfo);
                    nOfItemIncMulQty += Convert.ToInt32(sTransactionInfo[i, 4]);
                    fTotalDue = fTotalDue + Convert.ToDecimal(sTransactionInfo[i, 2]);
                }
                PrintTotalDueSummary(nOfItemIncMulQty, fTotalDue);
                decimal fTotalPaid = 0.0m;
                for (int i = nOfItems + 1; i <= nOfItems + nOfPaymentMethods; i++)
                {
                    fTotalPaid += Convert.ToDecimal(sTransactionInfo[i, 1]);
                }
                decimal fExcess = 0.0m;
                bool bChargedToAccountReprint = false;
                if (fTotalPaid > fTotalDue)
                    fExcess = fTotalPaid - fTotalDue;
                for (int i = nOfItems + 1; i <= nOfItems + nOfPaymentMethods; i++)
                {
                    PaymentMethod pmTemp = new PaymentMethod();
                    decimal fAmount = Convert.ToDecimal(sTransactionInfo[i, 1]);
                    if (sTransactionInfo[i, 0] != "CASH")
                        pmTemp.SetPaymentMethod(sTransactionInfo[i, 0], fAmount, 0.0m);
                    else
                        pmTemp.SetPaymentMethod(sTransactionInfo[i, 0], fAmount - fExcess, fExcess);
                    if (sTransactionInfo[i, 0].StartsWith("CHRG"))
                        bChargedToAccountReprint = true;
                    PrintPaymentMethod(pmTemp);
                }
                if (bChargedToAccountReprint)
                    PrintSignOnDottedLine();
                PrintChangeDue(fExcess);
                // Now work out the V.A.T.
                string[] sCodes = GetVATCodes();
                decimal[] fRates = GetVATRates();
                decimal[] fAmount2 = new decimal[fRates.Length];
                for (int i = 1; i <= nOfItems; i++)
                {
                    Item iItem = new Item(tStock.GetRecordFrom(sTransactionInfo[i, 0], 0, true), tStockStats.GetRecordFrom(sTransactionInfo[i,0], 0, true));
                    for (int x = 0; x < sCodes.Length; x++)
                    {
                        if (sCodes[x] == iItem.VATRate)
                        {
                            fAmount2[x] += Convert.ToDecimal(sTransactionInfo[i, 2]);
                        }
                        break;
                    }
                }
                PrintVAT(fRates, fAmount2);

                PrintReprintReceiptNote();
                PrintReceiptFooter(sFooterData[1], sFooterData[0], nTransactionNumber.ToString(), nTillNum);
                EmptyPrinterBuffer();
            }
        }

        public void PrintVAT(decimal[] fRates, decimal[] fGrossAmounts)
        {
            SendLineToPrinter("V.A.T. INCLUDED");
            for (int i = 0; i < fRates.Length; i++)
            {
                decimal fNet = 1 + (fRates[i] / 100);
                decimal fVATAmount = fGrossAmounts[i] - ( fGrossAmounts[i] / fNet);
                if (fGrossAmounts[i] != 0.00m)
                    PrintVATRate(fGrossAmounts[i], fRates[i], fVATAmount);
            }
        }
        private void PrintVATRate(decimal fGross, decimal fVATRate, decimal fVATAmount)
        {
            string sVATLine = "       ";
            sVATLine += FormatMoneyForDisplay(fGross);
            sVATLine += " @ " + FormatMoneyForDisplay(fVATRate);
            sVATLine += " % V.A.T. = ";
            sVATLine = RightAlignStringOnExistingString(sVATLine, FormatMoneyForDisplay(fVATAmount));
            SendLineToPrinter(sVATLine);
        }

        private const char cReceiptBreaker = '-';

        private void PrintReprintReceiptNote()
        {
            string sBreaker = "";
            for (int i = 0; i < nPrinterWidth; i++)
            {
                sBreaker += cReceiptBreaker;
            }
            SendLineToPrinter(sBreaker);
            SendLineToPrinter(CentralisePrinterText("REPRINT RECEIPT"));
        }

        void SendLinesToPrinter(string[] sLInes)
        {
            TextWriter tWriter = new StreamWriter("REPORT.TXT", true);
            foreach (string Line in sLInes)
                tWriter.WriteLine(Line);
            tWriter.Close();
        }

        void SendLineToPrinter(string sLine)
        {
            string[] sToSend = { sLine };
            SendLinesToPrinter(sToSend);
        }

        void EmptyPrinterBuffer()
        {
            nLineLastPrinted = 0;
            nPrinterPage = 1;
            rCurrentlyPrinting = ReportType.ReprintReceipt;
            sReportTitle = "Reprint Receipt";
            PrinterSettings pSettings = new PrinterSettings();
            pSettings.PrinterName = this.PrinterToUse;
            PrintDocument pPrinter = new PrintDocument();
            pPrinter.PrinterSettings = pSettings;
            pPrinter.DocumentName = "Reprinting Receipt";
            pPrinter.PrintPage += new PrintPageEventHandler(ReportPrintPage);
            pPrinter.Print();
        }

        public void PrintCashPaidOut(decimal fAmountPaidOut, string sDateTime, string sStaffName, int nTransactionNumber, int nTillNum)
        {
            // For reprint receipt
            PrintReceiptHeader(GetTillShopCode(nTillNum));
            SendLineToPrinter(CentralisePrinterText("CASH PAID OUT"));
            PrintBreaker();
            string[] sToSend = new string[2];
            sToSend[0] = RightAlignStringOnExistingString("CASH", FormatMoneyForDisplay(fAmountPaidOut));
            sToSend[1] = "";
            SendLinesToPrinter(sToSend);
            PrintSignOnDottedLine();
            PrintReprintReceiptNote();
            PrintReceiptFooter(sStaffName, sDateTime, nTransactionNumber.ToString(), nTillNum);
            EmptyPrinterBuffer();
        }

        private int nPrinterWidth = 50;

        private void PrintBreaker()
        {
            string sToPrint = "";
            for (int i = 0; i < nPrinterWidth; i++)
            {
                sToPrint += cReceiptBreaker;
            }
            SendLineToPrinter(sToPrint);
        }

        private string CentralisePrinterText(string sToCentralise)
        {
            string sToGoOnBeginning = "";
            string sToGoOnEnd = "";
            int nEitherSide = (nPrinterWidth - sToCentralise.Length) / 2;
            for (int i = 0; i < nEitherSide; i++)
            {
                sToGoOnBeginning += " ";
                sToGoOnEnd += " ";
            }
            while (sToGoOnEnd.Length + sToCentralise.Length + sToGoOnBeginning.Length < nPrinterWidth)
                sToGoOnEnd += " ";
            return sToGoOnBeginning + sToCentralise + sToGoOnEnd;
        }

        public void PrintSpecificRefund(string sItemDesc, decimal fAmountRefunded, PaymentMethod pmRefundMethod, int nQuantity, bool bReprintReceipt, int nTransactionNumber, int nTillNumber, string nStaffNum, string sDate)
        {
            // Item array in format { Item code, Item Description, Price Paid, Discount, Quantity }
            PrintReceiptHeader(GetTillShopCode(nTillNumber));
            string[] sItemInfo = { "NULL", sItemDesc, FormatMoneyForDisplay(-fAmountRefunded), "0.00", nQuantity.ToString() };
            PrintReceiptDescAndPriceTitles();
            SendLineToPrinter(CentralisePrinterText("SPECIFIC REFUND"));
            PrintBreaker();
            PrintItem(sItemInfo);
            PrintPaymentMethod(pmRefundMethod);
            if (bReprintReceipt)
                PrintReprintReceiptNote();
            PrintReceiptFooter(nStaffNum, sDate, nTransactionNumber.ToString(), nTillNumber);
            EmptyPrinterBuffer();
        }

        public void PrintReceiptDescAndPriceTitles()
        {
            string[] sToPrint = new string[2];
            sToPrint[0] = "DESCRIPTION";
            sToPrint[0] = RightAlignStringOnExistingString(sToPrint[0], "PRICE");
            for (int i = 0; i < nPrinterWidth; i++)
            {
                sToPrint[1] += cReceiptBreaker;
            }
            SendLinesToPrinter(sToPrint);
        }

        /// <summary>
        /// Prints the receipt header that is printed at the top of a till receipt
        /// </summary>
        /// <param name="sShopCode">The code of the shop whose header is to be printed</param>
        public void PrintReceiptHeader(string sShopCode)
        {
            string[] sToPrint = new string[5];
            for (int i = 0; i < 3; i++)
            {
                string[] sResult = tSettings.GetRecordFrom(sShopCode + "Address" + (i + 1).ToString(), 0);

                // Check that the address can be found
                if (sResult.Length > 1)
                {
                    sToPrint[i] = sResult[1];
                    sToPrint[i] = CentralisePrinterText(sToPrint[i]);
                }
                    // If it can't, check in the main settings file, as it's likely that this is an archive stockengine
                else
                {
                    Table tRealSettings = new Table("SETTINGS.DBF");
                    sResult = tRealSettings.GetRecordFrom(sShopCode + "Address" + (i + 1).ToString(), 0);
                    if (sResult.Length > 1)
                    {
                        // If it's found, print the current address
                        sToPrint[i] = sResult[1];
                        sToPrint[i] = CentralisePrinterText(sToPrint[i]);
                    }
                        // If it still can't be found, print an error message
                    else
                    {
                        sToPrint[i] = "Address Couldn't Be Found!";
                    }
                }
            }
            sToPrint[3] = "TEL:" + tSettings.GetRecordFrom(sShopCode + "PhoneNumber", 0)[1];
            sToPrint[3] = RightAlignStringOnExistingString(sToPrint[3], "VAT NO:" + tSettings.GetRecordFrom("VATNumber", 0)[1]);
            for (int i = 0; i < nPrinterWidth; i++)
                sToPrint[4] += cReceiptBreaker;
            SendLinesToPrinter(sToPrint);
        }

        private string RightAlignStringOnExistingString(string sExisting, string sToRightAlign)
        {
            int nSpacesNeeded = nPrinterWidth - sExisting.Length - sToRightAlign.Length;
            for (int i = 0; i < nSpacesNeeded; i++)
            {
                sExisting += " ";
            }
            sExisting += sToRightAlign;
            return sExisting;
        }

        public void PrintGeneralRefund(PaymentMethod pmPayMethod, int nTransactionNumber, string sStaffName, string sDate, int nTillNum)
        {
            PrintReceiptHeader(GetTillShopCode(nTillNum));
            PrintReceiptDescAndPriceTitles();
            SendLineToPrinter(CentralisePrinterText("GENERAL REFUND"));
            PrintBreaker();
            PrintPaymentMethod(pmPayMethod);
            SendLineToPrinter("");
            PrintSignOnDottedLine();
            PrintReprintReceiptNote();
            PrintReceiptFooter(sStaffName, sDate, nTransactionNumber.ToString(), nTillNum);
            EmptyPrinterBuffer();
        }

        public void PrintPaymentMethod(PaymentMethod pmMethod)
        {
            string sPaymentLine = GetPaymentDescription(pmMethod.PMType).ToUpper();
            if (sPaymentLine == "CASH")
                sPaymentLine += " TENDERED";
            sPaymentLine = RightAlignStringOnExistingString(sPaymentLine, FormatMoneyForDisplay(pmMethod.Excess));
            SendLineToPrinter(sPaymentLine);
        }

        public void PrintReceiptFooter(string sUserName, string sDateTime, string sTransactionNumber, int nTillNum)
        {
            string[] sLines = new string[6];
            for (int i = 0; i < nPrinterWidth; i++)
            {
                sLines[0] += cReceiptBreaker;
            }
            sLines[1] = sUserName + ", " + sDateTime + " " + sTransactionNumber;
            sLines[1] = CentralisePrinterText(sLines[1]);
            for (int i = 2; i < sLines.Length - 1; i++)
            {
                sLines[i] = CentralisePrinterText(GetTillData(nTillNum.ToString())[i + 1]);
            }
            sLines[5] = "";
            SendLinesToPrinter(sLines);
        }

        public void PrintItem(string[] sItemInfo)
        {
            // Item array in format { Item code, Item Description, Price Paid, Discount, Quantity }
            string sDescLine = sItemInfo[1];
            string sQuantityLine = "";
            string sDiscountLine = "";
            decimal fDiscountAmount = Convert.ToDecimal(sItemInfo[3]);
            decimal fPricePaid = Convert.ToDecimal(sItemInfo[2]);
            int nQuantity = Convert.ToInt32(sItemInfo[4]);
            decimal fIndividualPricePaid = fPricePaid / nQuantity;
            decimal fDiscountPerItem = fDiscountAmount / nQuantity;
            decimal fGrossPerItem = fIndividualPricePaid + fDiscountPerItem;
            bool bMultipleQuantities = false, bDiscount = false;
            if (nQuantity != 1)
                bMultipleQuantities = true;
            if (fDiscountAmount > 0.0m)
                bDiscount = true;
            if (bMultipleQuantities)
            {
                sQuantityLine = "QUANTITY : " + nQuantity.ToString() + " @ " + FormatMoneyForDisplay(fGrossPerItem);
                string sFormattedMoney = "";
                if (!bDiscount)
                {
                    sFormattedMoney = FormatMoneyForDisplay(fPricePaid);
                    while (sFormattedMoney.Length < 7) // Allows upto 9999.99
                        sFormattedMoney = " " + sFormattedMoney;
                }
                else
                {
                    sFormattedMoney = "        ";
                }
                sQuantityLine = RightAlignWholeString(sQuantityLine + " " + sFormattedMoney);
            }
            if (fDiscountAmount > 0.0m)
                sDiscountLine = RightAlignWholeString("DISCOUNT : " + (FormatMoneyForDisplay(fDiscountAmount)) + "     " + FormatMoneyForDisplay(fPricePaid).ToString());
            if (sQuantityLine == "" && sDiscountLine == "")
            {
                sDescLine = RightAlignStringOnExistingString(sDescLine, FormatMoneyForDisplay(fPricePaid));
                SendLineToPrinter(sDescLine);
            }
            else
            {
                SendLineToPrinter(sDescLine);
                if (sQuantityLine != "")
                {
                    SendLineToPrinter(sQuantityLine);
                }
                if (sDiscountLine != "")
                {
                    SendLineToPrinter(sDiscountLine);
                }
            }
        }

        public void PrintTotalDueSummary(int nOfItems, decimal fTotalDue)
        {
            SendLineToPrinter(RightAlignWholeString("--------"));
            string sTotalLine = "";
            if (nOfItems == 1)
                sTotalLine += "1 ITEM";
            else
                sTotalLine += nOfItems.ToString() + " ITEMS";
            string sTotalDueSection = "TOTAL DUE   ";
            string sFormattedMoney = FormatMoneyForDisplay(fTotalDue);
            while (sFormattedMoney.Length < 8)
                sFormattedMoney = " " + sFormattedMoney;
            sTotalDueSection += sFormattedMoney;
            sTotalLine = RightAlignStringOnExistingString(sTotalLine, sTotalDueSection);
            SendLineToPrinter(sTotalLine);
            SendLineToPrinter(RightAlignWholeString("========"));
        }

        private string RightAlignWholeString(string sExisting)
        {
            while (sExisting.Length < nPrinterWidth)
                sExisting = " " + sExisting;
            return sExisting;
        }

        private void PrintSignOnDottedLine()
        {
            SendLineToPrinter("");
            SendLineToPrinter("SIGNED ..............................");
        }

        public void PrintChangeDue(decimal fAmountDue)
        {
            string sChangeDue = "CHANGE";
            sChangeDue = RightAlignStringOnExistingString(sChangeDue, FormatMoneyForDisplay(fAmountDue));
            SendLineToPrinter(sChangeDue);
        }

        string[] GetVATCodes()
        {
            string[] sCodes = new string[tVATRates.NumberOfRecords];
            for (int i = 0; i < sCodes.Length; i++)
            {
                sCodes[i] = tVATRates.GetRecordFrom(i)[0];
            }
            return sCodes;
        }

        decimal[] GetVATRates()
        {
            decimal[] fRates = new decimal[tVATRates.NumberOfRecords];
            for (int i = 0; i < tVATRates.NumberOfRecords; i++)
            {
                fRates[i] = Convert.ToDecimal(tVATRates.GetRecordFrom(i)[2]);
            }
            return fRates;
        }

        void ClearReportFile()
        {
            File.Delete("REPORT.TXT");
        }
        
        void ArchiveBackOffStuff(Period pPeriod)
        {
            if (!Directory.Exists("Archive"))
                Directory.CreateDirectory("Archive");
            string sPeriod = "";
            switch (pPeriod)
            {
                case Period.Daily:
                    sPeriod = "Daily";
                    break;
                case Period.Monthly:
                    sPeriod = "Monthly";
                    break;
                case Period.Weekly:
                    sPeriod = "Weekly";
                    break;
                case Period.Yearly:
                    sPeriod = "Yearly";
                    break;
            }

            if (!Directory.Exists("Archive\\" + sPeriod))
                Directory.CreateDirectory("Archive\\" + sPeriod);
            string sLastColl = GetLastCollectionDate();
            string sDate = "20" + sLastColl[4].ToString() + sLastColl[5].ToString() + "." + sLastColl[2].ToString() + sLastColl[3].ToString() + "." + sLastColl[0].ToString() + sLastColl[1].ToString();
                string sSaveLoc = "Archive\\" + sPeriod + "\\" + sDate + "\\";
            if (!Directory.Exists(sSaveLoc))
                Directory.CreateDirectory(sSaveLoc);
            tAccStat.SaveToFile(sSaveLoc + "ACCSTAT.DBF");
            tCategory.SaveToFile(sSaveLoc + "CATEGORY.DBF");
            tCatGroupData.SaveToFile(sSaveLoc + "CATGRPDA.DBF");
            tCatGroupHeader.SaveToFile(sSaveLoc + "CATGPHDR.DBF");
            tCommissioners.SaveToFile(sSaveLoc + "COMMPPL.DBF");
            tCommItems.SaveToFile(sSaveLoc + "COMMITEM.DBF");
            //tCustomer.SaveToFile(sSaveLoc + "CUSTOMER.DBF");
            tEmails.SaveToFile(sSaveLoc + "EMAILS.DBF");
            tMultiData.SaveToFile(sSaveLoc + "MULTIDAT.DBF");
            tMultiHeader.SaveToFile(sSaveLoc + "MULTIHDR.DBF");
            tOrder.SaveToFile(sSaveLoc + "ORDER.DBF");
            tOrderLine.SaveToFile(sSaveLoc + "ORDERLIN.DBF");
            tOrderSuggestions.SaveToFile(sSaveLoc + "ORDERSUG.DBF");
            tSettings.SaveToFile(sSaveLoc + "SETTINGS.DBF");
            tShop.SaveToFile(sSaveLoc + "SHOP.DBF");
            tStaff.SaveToFile(sSaveLoc + "STAFF.DBF");
            tStock.SaveToFile(sSaveLoc + "MAINSTOC.DBF");
            tStockStats.SaveToFile(sSaveLoc + "STOCKSTA.DBF");
            tSupplier.SaveToFile(sSaveLoc + "SUPPLIER.DBF");
            tSupplierIndex.SaveToFile(sSaveLoc + "SUPINDEX.DBF");
            tTills.SaveToFile(sSaveLoc + "TILL.DBF");
            tTotalSales.SaveToFile(sSaveLoc + "TOTSALES.DBF");
            tVATRates.SaveToFile(sSaveLoc + "VAT.DBF");
            tStockLength.SaveToFile(sSaveLoc + "STOCKLEN.DBF");

            for (int i = 0; i < Till.Length; i++)
            {
                if (!Directory.Exists(sSaveLoc + "TILL" + Till[i].Number.ToString() + "\\INGNG"))
                    Directory.CreateDirectory(sSaveLoc + "TILL" + Till[i].Number.ToString() + "\\INGNG");
                string sTillLoc = sSaveLoc + "TILL" + Till[i].Number.ToString() + "\\INGNG\\";
                string[] sFiles = Directory.GetFiles("TILL" + Till[i].Number.ToString() + "\\INGNG");
                for (int x = 0; x < sFiles.Length; x++)
                {
                    File.Copy(sFiles[x], sTillLoc + sFiles[x].Split('\\')[sFiles[x].Split('\\').Length - 1], true);
                }
            }

            FileManagementEngine.CompressArchiveDirectory(sSaveLoc);
           
        }

        
        public bool DoesParentHaveChildren(string sParentCode)
        {
            for (int i = 0; i < tStock.NumberOfRecords; i++)
            {
                if (tStock.GetRecordFrom(i)[7] == sParentCode)
                    return true;
            }
            return false;
        }
        //38
        public void ChangeChildPricesToMatchParent(string sParentCode, decimal dNewPrice, string sShopCode)
        {
            bool bChanged = false;
            do
            {
                bChanged = false;
                for (int i = 0; i < tStock.NumberOfRecords; i++)
                {
                    if (tStock.GetRecordFrom(i)[7] == sParentCode && Convert.ToDecimal(tStock.GetRecordFrom(i)[2]) != dNewPrice)
                    {
                        if (Convert.ToDecimal(tStockStats.GetRecordFrom(tStock.GetRecordFrom(i)[0], 0)[38]) == 1)
                        {
                            tStock.EditRecordData(i, 2, dNewPrice.ToString());
                            for (int x = 0; x < Till.Length; x++)
                            {
                                AddEditItem(sShopCode, tStock.GetRecordFrom(i)[0], tStock.GetRecordFrom(i)[1], tStock.GetRecordFrom(i)[5], tStock.GetRecordFrom(i)[4], tStock.GetRecordFrom(i)[2], tStock.GetRecordFrom(i)[3], tStockStats.GetRecordFrom(tStock.GetRecordFrom(i)[0], 0, true)[37], sParentCode, tStockStats.GetRecordFrom(tStock.GetRecordFrom(i)[0], 0, true)[38]);
                            }
                        }
                        bChanged = true;
                    }
                }
            } while (bChanged);
            tStock.SaveToFile("MAINSTOC.DBF");
        }

        public bool ChangeParentAndAllOtherChildrenPrices(string sChildCode, decimal dNewPrice, string sShopCode)
        {
            // Check that the child only makes up one of the parent
            if (Convert.ToDecimal(tStockStats.GetRecordFrom(sChildCode, 0)[38]) == 1)
            {
                string sParentCode = tStock.GetRecordFrom(sChildCode, 0, true)[7];
                int nRecNum = -1;
                tStock.SearchForRecord(sParentCode, 0, ref nRecNum);
                if (nRecNum != -1)
                {
                    tStock.EditRecordData(nRecNum, 2, dNewPrice.ToString());
                    ChangeChildPricesToMatchParent(sParentCode, dNewPrice, sShopCode);
                }
                else
                {
                    return false;

                }
            }
            return true;
        }

        public void TransferStockItem(string sSourceShopCode, string sDestShopCode, string sBarcode, decimal dQty, bool bCanOverrideCheck)
        {
            try
            {
                TextWriter tWriter = new StreamWriter("transfers.txt", true);
                tWriter.Write(DateTime.Now.ToString() + ": " + sBarcode + " going from " + sSourceShopCode + " to " + sDestShopCode + ", quantity " + FormatMoneyForDisplay(dQty));
                tWriter.Close();
            }
            catch
            {
                ;
            }
            if (GetMainStockInfo(sBarcode).Length < 5)
            {
                System.Windows.Forms.MessageBox.Show("You can't alter the stock level of this item, as the barcode doesn't exist yet");
                return;
            }
            if (GetMainStockInfo(sBarcode)[5] != "1" && !bCanOverrideCheck)
            {
                System.Windows.Forms.MessageBox.Show("Only type 1 items may be transferred!");
                return;
            }
            tOrderLine.SortTable();
            decimal dLeftToRemove = dQty;
            for (int i = tOrderLine.NumberOfRecords - 1; i >= 0; i -= 1)
            {
                if (dLeftToRemove > 0)
                {
                    if (tOrderLine.GetRecordFrom(i)[2] == sBarcode && tOrder.GetRecordFrom(tOrderLine.GetRecordFrom(i)[0].TrimStart(' '), 0)[6] == sSourceShopCode)
                    {
                        decimal dQtyRecd = Convert.ToDecimal(tOrderLine.GetRecordFrom(i)[4]);
                        if (dLeftToRemove > dQtyRecd)
                        {
                            tOrderLine.EditRecordData(i, 3, "0");
                            tOrderLine.EditRecordData(i, 4, "0");
                            dLeftToRemove -= dQtyRecd;
                            string[] sOrderHeader = GetOrderHeader(tOrderLine.GetRecordFrom(i)[0].TrimStart(' '));
                            int nYear = 2000 + Convert.ToInt32(sOrderHeader[4][4].ToString() + sOrderHeader[4][5]);
                            if (DateTime.Now.Year == nYear)
                            {
                                int nRecNum = tStockStats.GetRecordNumberFromTwoFields(sBarcode, 0, sSourceShopCode, 35);
                                if (nRecNum != -1)
                                {
                                    decimal dYearDel = Convert.ToDecimal(tStockStats.GetRecordFrom(nRecNum)[23]);
                                    decimal dYearCost = Convert.ToDecimal(tStockStats.GetRecordFrom(nRecNum)[24]);
                                    dYearDel -= dQty;
                                    dYearCost -= Convert.ToDecimal(tOrderLine.GetRecordFrom(i)[5]);
                                    tStockStats.EditRecordData(nRecNum, 23, dYearDel.ToString());
                                    tStockStats.EditRecordData(nRecNum, 24, dYearCost.ToString());
                                }
                            }
                        }
                        else
                        {
                            decimal dQtyOrdered = Convert.ToDecimal(tOrderLine.GetRecordFrom(i)[3]);
                            tOrderLine.EditRecordData(i, 3, (dQtyOrdered - dLeftToRemove).ToString());
                            tOrderLine.EditRecordData(i, 4, (dQtyRecd - dLeftToRemove).ToString());
                            dLeftToRemove = 0;
                            string[] sOrderHeader = GetOrderHeader(tOrderLine.GetRecordFrom(i)[0].TrimStart(' '));
                            int nYear = 2000 + Convert.ToInt32(sOrderHeader[4][4].ToString() + sOrderHeader[4][5]);
                            if (DateTime.Now.Year == nYear)
                            {
                                int nRecNum = tStockStats.GetRecordNumberFromTwoFields(sBarcode, 0, sSourceShopCode, 35);
                                if (nRecNum != -1)
                                {
                                    decimal dYearDel = Convert.ToDecimal(tStockStats.GetRecordFrom(nRecNum)[23]);
                                    decimal dYearCost = Convert.ToDecimal(tStockStats.GetRecordFrom(nRecNum)[24]);
                                    dYearDel -= dQty;
                                    dYearCost -= Convert.ToDecimal(tOrderLine.GetRecordFrom(i)[5]);
                                    tStockStats.EditRecordData(nRecNum, 23, dYearDel.ToString());
                                    tStockStats.EditRecordData(nRecNum, 24, dYearCost.ToString());
                                }
                            }
                        }
                    }
                }
                else
                    break;//return;
            }
            int nRecPos = tStockStats.GetRecordNumberFromTwoFields(sBarcode, 0, sSourceShopCode, 35);
            if (nRecPos != -1)
            {
                decimal dQISSource = Convert.ToDecimal(tStockStats.GetRecordFrom(nRecPos)[36]);
                tStockStats.EditRecordData(nRecPos, 36, (dQISSource - dQty).ToString());
            }

            // Add to new shop
            for (int i = tOrderLine.NumberOfRecords - 1; i >= 0; i -= 1)
            {
                if (tOrderLine.GetRecordFrom(i)[2] == sBarcode)
                {
                    if (tOrder.GetRecordFrom(tOrderLine.GetRecordFrom(i)[0], 0)[6] == sDestShopCode)
                    {
                        decimal dQtyOrdered = Convert.ToDecimal(tOrderLine.GetRecordFrom(i)[3]);
                        decimal dQtyReceived = Convert.ToDecimal(tOrderLine.GetRecordFrom(i)[4]);
                        tOrderLine.EditRecordData(i, 3, (dQtyOrdered + dQty).ToString());
                        tOrderLine.EditRecordData(i, 4, (dQtyReceived + dQty).ToString());

                        string[] sOrderHeader = GetOrderHeader(tOrderLine.GetRecordFrom(i)[0].TrimStart(' '));
                        int nYear = 2000 + Convert.ToInt32(sOrderHeader[4][4].ToString() + sOrderHeader[4][5]);
                        if (DateTime.Now.Year == nYear)
                        {
                            int nRecNum = tStockStats.GetRecordNumberFromTwoFields(sBarcode, 0, sDestShopCode, 35);
                            if (nRecNum != -1)
                            {
                                decimal dYearDel = Convert.ToDecimal(tStockStats.GetRecordFrom(nRecNum)[23]);
                                decimal dYearCost = Convert.ToDecimal(tStockStats.GetRecordFrom(nRecNum)[24]);
                                dYearDel += dQty;
                                dYearCost += Convert.ToDecimal(tOrderLine.GetRecordFrom(i)[5]);
                                tStockStats.EditRecordData(nRecNum, 23, dYearDel.ToString());
                                tStockStats.EditRecordData(nRecNum, 24, dYearCost.ToString());
                            }
                        }
                        break;
                    }
                }
            }
            nRecPos = tStockStats.GetRecordNumberFromTwoFields(sBarcode, 0, sDestShopCode, 35);
            if (nRecPos != -1)
            {
                decimal dQISDest = Convert.ToDecimal(tStockStats.GetRecordFrom(nRecPos)[36]);
                tStockStats.EditRecordData(nRecPos, 36, (dQISDest + dQty).ToString());
            }

            if (sDestShopCode.ToUpper() != "BH")
            {
                UpdateStockLevelOnTill(sBarcode, sDestShopCode);
            }
            if (sSourceShopCode.ToUpper() != "BH")
            {
                UpdateStockLevelOnTill(sBarcode, sSourceShopCode);
            }
            tStockStats.SaveToFile("STOCKSTA.DBF");
            tOrderLine.SaveToFile("ORDERLIN.DBF");
        }

        /*public string GetWeekCommencingDate()
        {
            bool bFound = false;
            int nTillNum = 0;
            for (int i = 0; i < Till.Length; i++)
            {
                if (File.Exists(sTDir + "TILL" + Till[i].Number + "\\INGNG\\REPDATA2.DBF"))
                {
                    bFound = true;
                    nTillNum = i;
                    break;
                }
            }
            if (!bFound)
                return GetDDMMYYDate();
            else
            {
                Table tRepData = new Table(sTDir + "TILL" + Till[nTillNum].Number + "\\INGNG\\REPDATA2.DBF");
                return tRepData.GetRecordFrom(0)[1];
            }
            return GetDDMMYYDate();        
        }*/

        /// <summary>
        /// Checks to see if the commissioner is ever used in COMMITEM
        /// Used when the user might want to delete the commissioner
        /// </summary>
        /// <param name="sCommCode">The commissioner's code</param>
        /// <returns></returns>
        public bool CheckIfCommissionerIsUsed(string sCommCode)
        {
            int nRecLOc = -1;
            return tCommItems.SearchForRecord(sCommCode, 0, ref nRecLOc);
        }

        /// <summary>
        /// Deletes the given commissioner from the databases
        /// </summary>
        /// <param name="sCommCode">The commissioner to delete</param>
        public void DeleteCommissioner(string sCommCode)
        {
            int nRecLoc = -1;

            tCommissioners.SearchForRecord(sCommCode, 0, ref nRecLoc);

            if (nRecLoc != -1)
            {
                tCommissioners.DeleteRecord(nRecLoc);

                tCommissioners.SaveToFile("COMMPPL.DBF");
            }
        }

        public string GetWeekCommencingDate()
        {
            bool bFound = false;
            int nTillNum = 0;
            int nEarliestDay = -1;
            for (int x = 1; x <= 7; x++)
            {
                for (int i = 0; i < Till.Length; i++)
                {
                    if (File.Exists(sTDir + "TILL" + Till[i].Number + "\\INGNG\\REPDATA" + x.ToString() + ".DBF"))
                    {
                        bFound = true;
                        nTillNum = i;
                        nEarliestDay = x;
                        break;
                    }
                }
                if (bFound)
                    break;
            }
            if (!bFound)
                return GetDDMMYYDate();
            else
            {
                Table tRepData = new Table(sTDir + "TILL" + Till[nTillNum].Number + "\\INGNG\\REPDATA" + nEarliestDay.ToString() + ".DBF");
                return tRepData.GetRecordFrom(0)[1];
            }
            return GetDDMMYYDate();
        }

        public string GetMonthDate()
        {
            string sDate = GetLastCollectionDate();
            string sMonthNum = sDate[2].ToString() + sDate[3].ToString();
            string sYearNum = "20" + sDate[4].ToString() + sDate[5].ToString();
            string sMonthName = "";
            switch (sMonthNum)
            {
                case "01":
                    sMonthName = "January";
                    break;
                case "02":
                    sMonthName = "February";
                    break;
                case "03":
                    sMonthName = "March";
                    break;
                case "04":
                    sMonthName = "April";
                    break;
                case "05":
                    sMonthName = "May";
                    break;
                case "06":
                    sMonthName = "June";
                    break;
                case "07":
                    sMonthName = "July";
                    break;
                case "08":
                    sMonthName = "August";
                    break;
                case "09":
                    sMonthName = "September";
                    break;
                case "10":
                    sMonthName = "October";
                    break;
                case "11":
                    sMonthName = "November";
                    break;
                case "12":
                    sMonthName = "December";
                    break;
            }
            return sMonthName + " " + sYearNum;
        }

        public void EnterInvoiceCost(string sOrderNum, string[] sBarcodes, decimal[] dQtyInvoiced, decimal[] dInvCosts, decimal[] dOldCosts)
        {
            string[] sHeader = GetOrderHeader(sOrderNum);
            for (int i = 0; i < sBarcodes.Length; i++)
            {
                int nRecNum = tOrderLine.GetRecordNumberFromTwoFields(sBarcodes[i], 2, sOrderNum, 0);
                string[] sData = tOrderLine.GetRecordFrom(nRecNum);
                decimal dQtyAlreadyInvoiced = Convert.ToDecimal(sData[6]);
                tOrderLine.EditRecordData(nRecNum, 5, dInvCosts[i].ToString());
                tOrderLine.EditRecordData(nRecNum, 6, (dQtyAlreadyInvoiced + dQtyInvoiced[i]).ToString());

                nRecNum = tStockStats.GetRecordNumberFromTwoFields(sBarcodes[i], 0, sHeader[6], 35);
                decimal dAveCost = Convert.ToDecimal(tStockStats.GetRecordFrom(nRecNum)[1]);
                decimal dQIS = Convert.ToDecimal(tStockStats.GetRecordFrom(nRecNum)[36]);
                decimal dFullCost = dAveCost * dQIS;
                dFullCost -= (dQtyInvoiced[i] * dOldCosts[i]);
                dFullCost += (dQtyInvoiced[i] * dInvCosts[i]);
                if (dQIS > 0)
                    dAveCost = Math.Round(dFullCost / dQIS, 2, MidpointRounding.AwayFromZero);
                else
                    dAveCost = dFullCost;
                tStockStats.EditRecordData(nRecNum, 1, dAveCost.ToString());

                // Check if YDELIVERED needs to be edited
                string sDelYear = sHeader[4][4].ToString() + sHeader[4][5].ToString();
                string sYearNow = DateTime.Now.Year.ToString()[2].ToString() + DateTime.Now.Year.ToString()[3].ToString();

                if (sDelYear == sYearNow)
                {
                    decimal dDelCost = Convert.ToDecimal(tStockStats.GetRecordFrom(nRecNum)[24]);
                    dDelCost -= (dQtyInvoiced[i] * dOldCosts[i]);
                    dDelCost += (dQtyInvoiced[i] * dInvCosts[i]);
                    tStockStats.EditRecordData(nRecNum, 24, dDelCost.ToString());
                }
                

                tStock.SearchForRecord(sBarcodes[i], 0, ref nRecNum);
                tStock.EditRecordData(nRecNum, 8, dInvCosts[i].ToString());
            }

            tOrderLine.SaveToFile("ORDERLIN.DBF");
            tStockStats.SaveToFile("STOCKSTA.DBF");
            tStock.SaveToFile("MAINSTOC.DBF");
        }

        /// <summary>
        /// Adds or edits a commissioner
        /// </summary>
        /// <param name="sCode">The commissioner's code</param>
        /// <param name="sName">The commissioners name</param>
        public void AddCommissioner(string sCode, string sName)
        {
            int nRecNum = -1;
            if (tCommissioners.SearchForRecord(sCode, 0, ref nRecNum))
            {
                tCommissioners.DeleteRecord(nRecNum);
            }
            string[] sToAdd = { sCode, sName };
            tCommissioners.AddRecord(sToAdd);
            tCommissioners.SaveToFile("COMMPPL.DBF");
        }
        
        public string GetCommissionerName(string sCode)
        {
            try
            {
                return tCommissioners.GetRecordFrom(sCode, 0, true)[1];
            }
            catch
            {
                return "";
            }
        }

        public string[] GetListOfCommissioners()
        {
            string[] sToReturn = new string[tCommissioners.NumberOfRecords];
            for (int i = 0; i < sToReturn.Length; i++)
            {
                sToReturn[i] = tCommissioners.GetRecordFrom(i)[0];
            }
            return sToReturn;
        }

        /// <summary>
        /// Gets the percentage of the time that the given item is out of stock for
        /// </summary>
        /// <param name="sBarcode">The barcode of the ite</param>
        /// <param name="sShopCode">The shop to check at</param>
        /// <returns>A decimal percentage</returns>
        public decimal GetOutOfStockLength(string sBarcode, string sShopCode)
        {
            // Check to see if the barcode exists in stock length
            int nRecLoc = tStockLength.GetRecordNumberFromTwoFields(sBarcode, 0, sShopCode, 1);
            if (nRecLoc == -1)
                return 0;
            else
            {
                // Calculate how long it has been since the item was first recorded
                decimal dTotal = Convert.ToDecimal(tStockLength.GetRecordFrom(nRecLoc)[2]);
                decimal dOutOfStock = Convert.ToDecimal(tStockLength.GetRecordFrom(nRecLoc)[3]);

                // Calculate the percentage of time that the item is out of stock
                decimal dPercentage = (100 / dTotal) * dOutOfStock;

                return Math.Round(dPercentage, 3);
            }
        }

        public int WeekCalc(string sDateInput)
        {
            // Date input in DD/MM/YYYY
            /*string[] sDateSplit = sDateInput.Split('/');
            int nYear = Convert.ToInt32(sDateSplit[2]);
            int nMonth = Convert.ToInt32(sDateSplit[1]);
            int nDay = Convert.ToInt32(sDateSplit[0]);

            bool bYIsALeap = false;
            if ((nYear % 4 == 0 && nYear % 100 != 0) || nYear % 400 == 0)
                bYIsALeap = true;
            else
                bYIsALeap = false;

            bool bYMinusOneIsALeap = false;
            if ((nYear - 1 % 4 == 0 && nYear - 1 % 100 != 0) || nYear - 1 % 400 == 0)
                bYMinusOneIsALeap = true;
            else
                bYMinusOneIsALeap = false;

            int[] nMonthDays = { 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334 };
            int nDayOfYear = nDay + nMonthDays[nMonth - 1];
            if (bYIsALeap && nMonth > 2)
                nDayOfYear++;

            int nYY = (nYear - 1) % 100;
            int nC = (nYear - 1) - nYY;
            int nG = nYY + (nYY / 4);

            int nJanFirstWeekday = 1 + (((((nC / 100) % 4) * 5) + nG) % 7);

            int nH = nDayOfYear + (nJanFirstWeekday - 1);
            int nWeekday = 1 + ((nH - 1) % 7);

            int nYearNumber = 0;
            int nWeekNumber = 0;
            if (nDayOfYear <= (8 - nJanFirstWeekday) && nJanFirstWeekday > 4)
            {
                nYearNumber = nYear - 1;
                if (nJanFirstWeekday == 5 || (nJanFirstWeekday == 6 && bYMinusOneIsALeap))
                {
                    nWeekNumber = 53;
                }
                else
                {
                    nWeekNumber = 52;
                }
            }
            else
            {
                nYearNumber = nYear;
            }

            int I = 0;
            if (nYearNumber == nYear)
            {
                if (bYIsALeap)
                {
                    I = 366;
                }
                else
                {
                    I = 365;
                }
                if (I - nDayOfYear < 4 - nWeekday)
                {
                    nYearNumber = nYear + 1;
                    nWeekNumber = 1;
                }
            }

            int J = 0;
            if (nYearNumber == nYear)
            {
                J = nDayOfYear + (7 - nWeekday) + (nJanFirstWeekday - 1);
                nWeekNumber = J / 7;

                if (nJanFirstWeekday > 4)
                    nWeekNumber -= 1;
            }

            return nWeekNumber;*/

            string[] sDateSplit = sDateInput.Split('/');

            int nYear = Convert.ToInt32(sDateSplit[2]);
            int nMonth = Convert.ToInt32(sDateSplit[1]);
            int nDay = Convert.ToInt32(sDateSplit[0]);

            DateTime dt = new DateTime(nYear, nMonth, nDay);

            CultureInfo ciGetNumber = CultureInfo.CurrentCulture;
            int returnNumber = ciGetNumber.Calendar.GetWeekOfYear(dt, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return returnNumber;

        }

        public decimal GetWeeklySales(int nWeekNum, int nYearToFind, ref string sWeekCommencing)
        {
            tTotalSales.SortTable();
            
            for (int i = 0; i < tTotalSales.NumberOfRecords; i++)
            {
                if (tTotalSales.GetRecordFrom(i)[0].StartsWith(nYearToFind.ToString()))
                {
                    string sDate = tTotalSales.GetRecordFrom(i)[0];
                    int nYear = Convert.ToInt32(sDate[0].ToString() + sDate[1].ToString() + sDate[2].ToString() + sDate[3].ToString());
                    int nMonth = Convert.ToInt32(sDate[4].ToString() + sDate[5].ToString());
                    int nDay = Convert.ToInt32(sDate[6].ToString() + sDate[7].ToString());
                    DateTime dt = new DateTime(nYear, nMonth, nDay);
                    dt = dt.AddDays(1.0d);
                    sDate = dt.Day.ToString() + "/" + dt.Month.ToString() + "/" + dt.Year.ToString();
                    if (WeekCalc(sDate) == nWeekNum)
                    {
                        sWeekCommencing = tTotalSales.GetRecordFrom(i)[0];
                        return Convert.ToDecimal(tTotalSales.GetRecordFrom(i)[3]);
                    }
                }
            }
            return 0;
        }

        /*public void CommissionSummaryReportToFile()
        {
            string[] sComCodes = GetListOfCommissioners();
            TextWriter tWriter = new StreamWriter("REPORT.TXT");
            tWriter.WriteLine("------------------------");
            tWriter.WriteLine("Commission Sales Summary");
            tWriter.WriteLine("------------------------");
            tWriter.WriteLine();
            tWriter.WriteLine("----------------------------------------------------------------------------------");
            tWriter.WriteLine("Item Barcode  Item Description             Qty Sold  Price Sold At  Commission Due");
            tWriter.WriteLine("----------------------------------------------------------------------------------");
            foreach (string sComCode in sComCodes)
            {
                if (GetCommissionerAmountOwed(sComCode) != 0)
                {
                    int nOfRecords = 0;
                    decimal dAmountOwed = 0;
                    string[,] sInfo = tCommItems.SearchAndGetAllMatchingRecords(0, sComCode, ref nOfRecords, true);
                    for (int i = 0; i < nOfRecords; i++)
                    {
                        string sBarcode = sInfo[i, 1];
                        decimal dQtySold = Convert.ToDecimal(sInfo[i, 2]);
                        decimal dAmount = Convert.ToDecimal(sInfo[i, 3]);
                        decimal dGross = Convert.ToDecimal(sInfo[i, 4]);
                        string sLine = sBarcode;
                        while (sLine.Length < 14)
                            sLine += " ";
                        sLine += GetMainStockInfo(sBarcode)[1];
                        while (sLine.Length < 45)
                            sLine += " ";
                        while (sLine.Length + FormatMoneyForDisplay(dQtySold).Length < 51)
                            sLine += " ";
                        sLine += FormatMoneyForDisplay(dQtySold);
                        while (sLine.Length + FormatMoneyForDisplay(dGross).Length < 66)
                            sLine += " ";
                        sLine += FormatMoneyForDisplay(dGross);
                        while (sLine.Length + FormatMoneyForDisplay(dAmount).Length < 81)
                            sLine += " ";
                        sLine += FormatMoneyForDisplay(dAmount);
                        dAmountOwed += dAmount;
                        tWriter.WriteLine(sLine);
                    }
                    tWriter.WriteLine("------------------------------------------------------------------");
                    string sToWrite = "Total Due :";
                    while (sToWrite.Length + FormatMoneyForDisplay(dAmountOwed).Length < 81)
                        sToWrite += " ";
                    sToWrite += FormatMoneyForDisplay(dAmountOwed);
                    tWriter.WriteLine(sToWrite);
                    tWriter.WriteLine("------------------------------------------------------------------");
                }
            }
            tWriter.Close();
        }*/

        private class CommissionReportItem : IComparable
        {
            private string[] sRecordInfo;

            public CommissionReportItem(string[] sRecordInfo)
            {
                this.sRecordInfo = sRecordInfo;
            }

            public string[] GetRecordInfo()
            {
                return sRecordInfo;
            }

            public DateTime GetDateTimeSold()
            {
                string sStartDate = sRecordInfo[7];
                DateTime dtThisDate = new DateTime(2000 + Convert.ToInt32(sStartDate[4].ToString() + sStartDate[5].ToString()),
                                            Convert.ToInt32(sStartDate[2].ToString() + sStartDate[3].ToString()),
                                            Convert.ToInt32(sStartDate[0].ToString() + sStartDate[1].ToString()));
                return dtThisDate;
            }


            #region IComparable Members

            public int CompareTo(object obj)
            {
                return DateTime.Compare(this.GetDateTimeSold(), ((CommissionReportItem)obj).GetDateTimeSold());
            }

            #endregion
        }

        /// <summary>
        /// Prints a summary of commission sales (type 6 items) between the specified dates
        /// </summary>
        /// <param name="sStartDate">The start date (DDMMYY) inclusive</param>
        /// <param name="sEndDate">The end date (DDMMYY) inclusive</param>
        public void CommissionSummaryReportToFile(string sStartDate, string sEndDate)
        {
            // Convert the start and end dates to DateTimes for easy comparison
            DateTime dtStart = new DateTime(2000 + Convert.ToInt32(sStartDate[4].ToString() + sStartDate[5].ToString()),
                                            Convert.ToInt32(sStartDate[2].ToString() + sStartDate[3].ToString()),
                                            Convert.ToInt32(sStartDate[0].ToString() + sStartDate[1].ToString()));
            DateTime dtEnd = new DateTime(2000 + Convert.ToInt32(sEndDate[4].ToString() + sEndDate[5].ToString()),
                                            Convert.ToInt32(sEndDate[2].ToString() + sEndDate[3].ToString()),
                                            Convert.ToInt32(sEndDate[0].ToString() + sEndDate[1].ToString()));

            // Create a list of all records, and sort into date sold order
            List<CommissionReportItem> lItems = new List<CommissionReportItem>();
            for (int i = 0; i < tCommItems.NumberOfRecords; i++)
            {
                if (tCommItems.GetRecordFrom(i)[7].Length == 6)
                {
                    lItems.Add(new CommissionReportItem(tCommItems.GetRecordFrom(i)));
                }
            }

            lItems.Sort();

            // Now, go through the list and remove any items that are out of range

            for (int i = 0; i < lItems.Count; i++)
            {
                // Check if the sold date is before the start date for this report
                if (DateTime.Compare(lItems[i].GetDateTimeSold(), dtStart) < 0)
                {
                    lItems.RemoveAt(i);
                    i--;
                }
                    // Check if the sold date is after the end date for this report
                else if (DateTime.Compare(lItems[i].GetDateTimeSold(), dtEnd) > 0)
                {
                    lItems.RemoveAt(i);
                    i--;
                }
            }

            // Now we have a list of valid items, sort them again into ascending date sold order
            lItems.Sort();

            // Start the report
            TextWriter tWriter = new StreamWriter("REPORT.TXT");
            tWriter.WriteLine("Commission Sales Summary between " + dtStart.ToShortDateString() + " and " + dtEnd.ToShortDateString());

            tWriter.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------------------");
            tWriter.WriteLine("Date         Item Description                Barcode        Artist                         Sold For      Cost    Profit       VAT  Gross Profit");
            tWriter.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------------------");

            // The totals to add up
            decimal dTillTotal = 0;
            decimal dCostTotal = 0;
            decimal dNetProfitTotal = 0;
            decimal dVATTotal = 0;
            
            // Now go through, and print out the report to a file

            for (int i = 0; i < lItems.Count; i++)
            {
                string sLine = lItems[i].GetDateTimeSold().ToShortDateString();
                while (sLine.Length < 13)
                    sLine += " ";

                string[] sItemRecord = tStock.GetRecordFrom(lItems[i].GetRecordInfo()[1], 0, true);
                if (sItemRecord.Length < 2)
                {
                    sLine += "MAINSTOC Record Missing";
                }
                else
                {
                    sLine += sItemRecord[1];
                }

                while (sLine.Length < 45)
                    sLine += " ";

                sLine += lItems[i].GetRecordInfo()[1];
                while (sLine.Length < 60)
                    sLine += " ";

                // Artist
                string sArtistName = GetCommissionerName(lItems[i].GetRecordInfo()[0]);
                sLine += sArtistName;
                while (sLine.Length < 90)
                    sLine += " ";

                // Sold For
                string sMoneyToAdd = FormatMoneyForDisplay(lItems[i].GetRecordInfo()[4]);
                dTillTotal += Convert.ToDecimal(lItems[i].GetRecordInfo()[4]);
                while (sMoneyToAdd.Length < 9)
                    sMoneyToAdd = " " + sMoneyToAdd;
                sLine += sMoneyToAdd + " ";

                // Cost
                decimal dCostForThis = Convert.ToDecimal(lItems[i].GetRecordInfo()[3]);
                if (dCostForThis < Convert.ToDecimal(lItems[i].GetRecordInfo()[10]))
                    dCostForThis = Convert.ToDecimal(lItems[i].GetRecordInfo()[10]);
                sMoneyToAdd = FormatMoneyForDisplay(dCostForThis);
                dCostTotal += Convert.ToDecimal(dCostForThis);
                while (sMoneyToAdd.Length < 9)
                    sMoneyToAdd = " " + sMoneyToAdd;
                sLine += sMoneyToAdd + " ";

                // Profit
                sMoneyToAdd = FormatMoneyForDisplay(lItems[i].GetRecordInfo()[9]);
                dNetProfitTotal += Convert.ToDecimal(lItems[i].GetRecordInfo()[9]);
                while (sMoneyToAdd.Length < 9)
                    sMoneyToAdd = " " + sMoneyToAdd;
                sLine += sMoneyToAdd + " ";

                // VAT
                sMoneyToAdd = FormatMoneyForDisplay(lItems[i].GetRecordInfo()[8]);
                dVATTotal += Convert.ToDecimal(lItems[i].GetRecordInfo()[8]);
                while (sMoneyToAdd.Length < 9)
                    sMoneyToAdd = " " + sMoneyToAdd;
                sLine += sMoneyToAdd + " ";

                // Gross Profit
                sMoneyToAdd = FormatMoneyForDisplay(Convert.ToDecimal(lItems[i].GetRecordInfo()[8]) + Convert.ToDecimal(lItems[i].GetRecordInfo()[9]));
                while (sMoneyToAdd.Length < 13)
                    sMoneyToAdd = " " + sMoneyToAdd;
                sLine += sMoneyToAdd;

                tWriter.WriteLine(sLine);
            }

            tWriter.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------------------");
            string sTotals = "Totals: ";

            string sSoldFor = FormatMoneyForDisplay(dTillTotal);
            while (sTotals.Length + sSoldFor.Length < 99)
                sTotals = " " + sTotals;
            sTotals += sSoldFor + " ";

            string sCost = FormatMoneyForDisplay(dCostTotal);
            while (sCost.Length < 9)
                sCost = " " + sCost;
            sTotals += sCost + " ";

            string sProfit = FormatMoneyForDisplay(dNetProfitTotal);
            while (sProfit.Length < 9)
                sProfit = " " + sProfit;
            sTotals += sProfit + " ";

            string sVAT = FormatMoneyForDisplay(dVATTotal);
            while (sVAT.Length < 9)
                sVAT = " " + sVAT;
            sTotals += sVAT + " ";

            string sGrossProfit = FormatMoneyForDisplay(dVATTotal + dNetProfitTotal);
            while (sGrossProfit.Length < 13)
                sGrossProfit = " " + sGrossProfit;
            sTotals += sGrossProfit;

            tWriter.WriteLine(sTotals);
            tWriter.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------------------");
            tWriter.Close();
        }

        public void CommissionSummaryReportToPrinter(string sStartDate, string sEndDate)
        {
            CommissionSummaryReportToFile(sStartDate, sEndDate);
            sReportTitle = "Commission Sales Summary between " + sStartDate + " and " + sEndDate; 
            nLineLastPrinted = 4;
            nPrinterPage = 1;
            rCurrentlyPrinting = ReportType.CommissionSummaryReport;
            PrinterSettings pSettings = new PrinterSettings();
            pSettings.PrinterName = this.PrinterToUse;
            PrintDocument pPrinter = new PrintDocument();
            pPrinter.PrinterSettings = pSettings;
            pPrinter.DefaultPageSettings.Landscape = true;
            pPrinter.DocumentName = "Commission Sales Summary";
            pPrinter.PrintPage += new PrintPageEventHandler(ReportPrintPage);
            pPrinter.Print();
        }

        /// <summary>
        /// Prints a report detailing the sales & returns of items from a specific commissioner/artist
        /// </summary>
        /// <param name="sComCode">The code of the artist</param>
        /// <param name="sStartDate">The date to start from (DDMMYY)</param>
        /// <param name="sEndDate">The date to end searching (DDMMYY)</param>
        /// <param name="bArtistPresent">If the artist is present, then Price sold for and profit will be removed</param>
        public void ComissionReportToFile(string sComCode, string sStartDate, string sEndDate, bool bArtistPresent)
        {
            TextWriter tWriter = new StreamWriter("REPORT.TXT");
            tWriter.WriteLine(GetCommissionerName(sComCode));
            tWriter.WriteLine();
            if (!bArtistPresent)
            {
                tWriter.WriteLine("------------------------------------------------------------------------------------------------------");
                tWriter.WriteLine("Item Barcode  Item Description             Item Number  Status   Price Sold At  Commission Due  Profit");
                tWriter.WriteLine("------------------------------------------------------------------------------------------------------");
            }
            else
            {
                tWriter.WriteLine("-------------------------------------------------------------------------------");
                tWriter.WriteLine("Item Barcode  Item Description             Item Number  Status   Commission Due");
                tWriter.WriteLine("-------------------------------------------------------------------------------");
            }
            int nOfRecords = 0;
            decimal dAmountOwed = 0;
            string[,] sInfo = tCommItems.SearchAndGetAllMatchingRecords(0, sComCode, ref nOfRecords, true);

            // Get a list of all of the barcodes, and sort them into alphabetical order
            List<string> lBarcodes = new List<string>();
            for (int i = 0; i < nOfRecords; i++)
            {
                if (!lBarcodes.Contains(sInfo[i, 1]))
                    lBarcodes.Add(sInfo[i, 1]);
            }
            lBarcodes.Sort();

            // Get 2 DateTimes, the start and end dates
            DateTime dtStart = new DateTime(2000 + Convert.ToInt32(sStartDate[4].ToString() + sStartDate[5].ToString()),
                                            Convert.ToInt32(sStartDate[2].ToString() + sStartDate[3].ToString()),
                                            Convert.ToInt32(sStartDate[0].ToString() + sStartDate[1].ToString()));
            DateTime dtEnd = new DateTime(2000 + Convert.ToInt32(sEndDate[4].ToString() + sEndDate[5].ToString()),
                                            Convert.ToInt32(sEndDate[2].ToString() + sEndDate[3].ToString()),
                                            Convert.ToInt32(sEndDate[0].ToString() + sEndDate[1].ToString()));


            for (int i = 0; i < lBarcodes.Count; i++)
            {
                List<int> lNumbers = new List<Int32>();

                // Now calculate a list of item numbers, accounting for the possibility that the numbers may not start at1
                // 1 and may have sumber numbers inbetween missing
                for (int x = 0; x < nOfRecords; x++)
                {
                    if (sInfo[x, 1] == lBarcodes[i])
                    {
                        DateTime dtDealtDate = new DateTime();
                        if (sInfo[x, 7] != "")
                        {
                            // Now check that this item number was dealt with between the specified dates
                            dtDealtDate = new DateTime(2000 + Convert.ToInt32(sInfo[x, 7][4].ToString() + sInfo[x, 7][5].ToString()),
                                                       Convert.ToInt32(sInfo[x, 7][2].ToString() + sInfo[x, 7][3].ToString()),
                                                       Convert.ToInt32(sInfo[x, 7][0].ToString() + sInfo[x, 7][1].ToString()));
                        }
                        // Check that the start and end dates create a range which the sold/returned date lies in
                        if (sInfo[x, 7] == "" || (DateTime.Compare(dtStart, dtDealtDate) <= 0 && DateTime.Compare(dtDealtDate, dtEnd) <= 0))
                        {
                            // Add to the list of applicable items
                            lNumbers.Add(Convert.ToInt32(sInfo[x, 2]));
                        }
                    }
                }

                lNumbers.Sort();

                // Now print out the barcodes
                for (int x = 0; x < lNumbers.Count; x++)
                {
                    int nRecLoc = tCommItems.GetRecordNumberFromTwoFields(lBarcodes[i], 1, lNumbers[x].ToString(), 2);
                    string[] sRecordInfo = tCommItems.GetRecordFrom(nRecLoc);

                    // Add the barcode
                    string sLine = lBarcodes[i];
                    while (sLine.Length < 14)
                        sLine += " ";

                    // Add the description
                    sLine += tStock.GetRecordFrom(lBarcodes[i], 0, true)[1];
                    while (sLine.Length < 43)
                        sLine += " ";

                    // Add the item number
                    while (sLine.Length + lNumbers[x].ToString().Length < 54)
                        sLine += " ";
                    sLine += lNumbers[x].ToString() + "  ";

                    // Add the status
                    switch (sRecordInfo[5])
                    {
                        case "Y":
                            sLine += "Sold";
                            break;
                        case "N":
                            sLine += "Returned";
                            break;
                        case " ":
                            sLine += "In Stock";
                            break;
                        case "":
                            sLine += "In Stock";
                            break;
                    }
                    while (sLine.Length < 65)
                        sLine += " ";

                    if (!bArtistPresent)
                    {
                        // Add the price sold for
                        string sSoldFor = FormatMoneyForDisplay(sRecordInfo[4]);
                        while (sSoldFor.Length < 13)
                            sSoldFor = " " + sSoldFor;
                        sLine += sSoldFor;
                        while (sLine.Length < 80)
                            sLine += " ";
                    }

                    // Add the commission due to be paid
                    decimal dAmountDue = Convert.ToDecimal(sRecordInfo[3]) - Convert.ToDecimal(sRecordInfo[10]);
                    if (sRecordInfo[10] != "" && Convert.ToDecimal(sRecordInfo[10]) != 0)
                    {
                        dAmountDue = 0;
                    }
                    string sCommissionDue = FormatMoneyForDisplay(dAmountDue);
                    while (sCommissionDue.Length < 14)
                        sCommissionDue = " " + sCommissionDue;
                    sLine += sCommissionDue;
                    if (!bArtistPresent)
                    {
                        while (sLine.Length < 96)
                            sLine += " ";
                    }
                    else
                    {
                        while (sLine.Length < 80)
                            sLine += " ";
                    }

                    if (!bArtistPresent)
                    {
                        // Add the profit
                        string sProfit = FormatMoneyForDisplay(sRecordInfo[9]);
                        while (sProfit.Length < 6)
                            sProfit = " " + sProfit;
                        sLine += sProfit;
                    }

                    tWriter.WriteLine(sLine);

                }
            }

            /* (int i = 0; i < nOfRecords; i++)
            {
                string sBarcode = sInfo[i, 1];
                decimal dQtySold = Convert.ToDecimal(sInfo[i, 2]);
                decimal dAmount = Convert.ToDecimal(sInfo[i, 3]);
                decimal dGross = Convert.ToDecimal(sInfo[i, 4]);
                string sLine = sBarcode;
                while (sLine.Length < 14)
                    sLine += " ";
                sLine += GetMainStockInfo(sBarcode)[1];
                while (sLine.Length < 45)
                    sLine += " ";
                while (sLine.Length + FormatMoneyForDisplay(dQtySold).Length < 51)
                    sLine += " ";
                sLine += FormatMoneyForDisplay(dQtySold);
                while (sLine.Length + FormatMoneyForDisplay(dGross).Length < 66)
                    sLine += " ";
                sLine += FormatMoneyForDisplay(dGross);
                while (sLine.Length + FormatMoneyForDisplay(dAmount).Length < 81)
                    sLine += " ";
                sLine += FormatMoneyForDisplay(dAmount);
                dAmountOwed += dAmount;
                tWriter.WriteLine(sLine);
            }*/
            tWriter.WriteLine("----------------------------------------------------------------------------------------------");
            /*string sToWrite = "Total Due :";
            while (sToWrite.Length + FormatMoneyForDisplay(dAmountOwed).Length < 81)
                sToWrite += " ";
            sToWrite += FormatMoneyForDisplay(dAmountOwed);
            tWriter.WriteLine(sToWrite);*/
            //tWriter.WriteLine("----------------------------------------------------------------------------------------------");
           
            tWriter.Close();
        }
        public void ComissionReportToPrinter(string sComCode, string sStartDate, string sEndDate, bool bArtistPresent)
        {
            ComissionReportToFile(sComCode, sStartDate, sEndDate, bArtistPresent);
            sReportTitle = GetCommissionerName(sComCode);
            nLineLastPrinted = 1;
            nPrinterPage = 1;
            rCurrentlyPrinting = ReportType.ComissionReport;
            PrinterSettings pSettings = new PrinterSettings();

            PrintDocument pPrinter = new PrintDocument();
            pSettings.PrinterName = this.PrinterToUse;
            pPrinter.PrinterSettings = pSettings;
            pPrinter.DocumentName = "Commission Report for " + GetCommissionerName(sComCode);
            pPrinter.PrintPage += new PrintPageEventHandler(ReportPrintPage);
            pPrinter.Print();
        }

        public void ClearComissionDue(string sComCode)
        {
            for (int i = 0; i < tCommItems.NumberOfRecords; i++)
            {
                if (tCommItems.GetRecordFrom(i)[0] == sComCode)
                {
                    tCommItems.EditRecordData(i, 2, "0");
                    tCommItems.EditRecordData(i, 3, "0");
                }
            }
            tCommItems.SaveToFile("COMMITEM.DBF");
        }

        public string GetNextAutoBarcode()
        {
            try
            {
                string sToReturn = "";
                int nAttempts = 0;
                do
                {
                    sToReturn = tSettings.GetRecordFrom("NextBarcode", 0, true)[1];
                    int nNum = Convert.ToInt32(sToReturn.Substring(0, sToReturn.Length - 1));
                    if (nNum > 1000999 || nNum < 1000000)
                        nNum = 1000000;
                    if (nAttempts > 1000)
                    {
                        System.Windows.Forms.MessageBox.Show("There are no auto-barcodes left free. There are only 1000 possible automatic codes available, in the range 1000000x to 1000999x, where x is a check digit");
                        return "";
                    }
                    nNum++;
                    nAttempts++;
                    int nRecNum = 0;
                    tSettings.SearchForRecord("NextBarcode", 0, ref nRecNum);
                    tSettings.EditRecordData(nRecNum, 1, AddCheckDigitToBarcode(nNum.ToString()));
                    tSettings.SaveToFile("SETTINGS.DBF");
                } while (tStock.SearchForRecord(sToReturn, "BARCODE") || sToReturn.Length != 8);
                return sToReturn;
            }
            catch
            {
                return "";
            }
        }

        public void AddQuantityOnOrder(string sBarcode, decimal dQuantity, string sShopCode)
        {
            this.FindMissingOrderLines();
            int nRecNum = tStockStats.GetRecordNumberFromTwoFields(sBarcode, 0, sShopCode, 35);
            string[] sMainStockInfo = tStockStats.GetRecordFrom(nRecNum);
            decimal dCurrentQty = Convert.ToDecimal(sMainStockInfo[3]);
            dCurrentQty += dQuantity;
            tStockStats.EditRecordData(nRecNum, 3, dCurrentQty.ToString());
            tStockStats.SaveToFile("STOCKSTA.DBF");
            this.FindMissingOrderLines();
        }

        public void RemoveQuantityOnOrder(string sBarcode, decimal dQuantity, string sShopCode)
        {
            this.FindMissingOrderLines();
            int nRecNum = tStockStats.GetRecordNumberFromTwoFields(sBarcode, 0, sShopCode, 35);
            string[] sMainStockInfo = tStockStats.GetRecordFrom(nRecNum);
            decimal dCurrentQty = Convert.ToDecimal(sMainStockInfo[3]);
            dCurrentQty -= dQuantity;
            tStockStats.EditRecordData(nRecNum, 3, dCurrentQty.ToString());
            tStockStats.SaveToFile("STOCKSTA.DBF");
            this.FindMissingOrderLines();
        }

        public bool ReceiveComissionItem(string sBarcode, string sQty, string sShopCode)
        {
            // Check that the barcode is actually a commission item
            if (tStock.GetRecordFrom(sBarcode, 0, true).Length > 2 && tStock.GetRecordFrom(sBarcode, 0, true)[5] == "6")
            {
                string sDay = DateTime.Now.Day.ToString();
                while (sDay.Length < 2)
                    sDay = "0" + sDay;
                string sMonth = DateTime.Now.Month.ToString();
                while (sMonth.Length < 2)
                    sMonth = "0" + sMonth;
                string sYear = DateTime.Now.Year.ToString()[2].ToString() + DateTime.Now.Year.ToString()[3].ToString();

                // Done to ensure that 1.00 can be converted to 1 correctly.
                decimal dQty = Convert.ToDecimal(sQty);
                int nQty = Convert.ToInt32(Math.Round(dQty, 0, MidpointRounding.AwayFromZero));

                // Now, go through and add x records to commitem if x items are being received
                for (int i = 0; i < nQty; i++)
                {
                    // Add : Commissioner code from mainstoc, the barcode, item number, artists fee from mainstock
                    //       retail price from mainstock, blank entry to mark that the item hasn't been sold or returned to the artist,
                    //       received date, blank entry for sold date, blank entry for VAT amount paid, blank entry for profit
                    string[] toAdd = { tStock.GetRecordFrom(sBarcode, 0, true)[6],
                                         sBarcode,
                                         (i+1).ToString(),
                                         tStock.GetRecordFrom(sBarcode, 0, true)[8],
                                         tStock.GetRecordFrom(sBarcode, 0, true)[2],
                                         " ",
                                         sDay + sMonth + sYear,
                                         "      ",
                                         "0.00",
                                         "0.00",
                                         "0.00" };
                    tCommItems.AddRecord(toAdd);
                }

                tCommItems.SaveToFile("COMMITEM.DBF");

                // INcrease the QIS by the received amount
                int nRecLoc = tStockStats.GetRecordNumberFromTwoFields(sBarcode, 0, sShopCode, 35);
                decimal dQtyInStock = Convert.ToDecimal(tStockStats.GetRecordFrom(nRecLoc)[36]);
                dQtyInStock += Convert.ToDecimal(sQty);
                tStockStats.EditRecordData(nRecLoc, 36, dQtyInStock.ToString());
                tStockStats.EditRecordData(nRecLoc, 4, sDay + sMonth + sYear);
                tStockStats.SaveToFile("STOCKSTA.DBF");

                // Upload changes to till
                UpdateStockLevelOnTill(sBarcode, sShopCode);
                return true;
            }
            else
            {
                // Item wasn't a type 6
                return false;
            }
        }

        /// <summary>
        /// Tries to return the specified number of items
        /// </summary>
        /// <param name="sBarcode">The product to try and return</param>
        /// <param name="dQtyToReturn">The quantity to attempt to return</param>
        /// <param name="sShopCode">The shop to return the items from</param>
        /// <returns>The number that were actually returned</returns>
        public decimal ReturnCommissionItems(string sBarcode, decimal dQtyToReturn, string sShopCode)
        {
            decimal dReturned = 0;
            for (int x = (int)dQtyToReturn; x > 0; x--)
            {
                int nLowestRecNum = -1;
                int nLowestNumber = 999999;
                for (int i = 0; i < tCommItems.NumberOfRecords; i++)
                {
                    if (tCommItems.GetRecordFrom(i)[1] == sBarcode)
                    {
                        int nItemNo = Convert.ToInt32(tCommItems.GetRecordFrom(i)[2]);
                        // Check that the item hasn't already been returned
                        if (tCommItems.GetRecordFrom(i)[5].Trim() == "" && nItemNo < nLowestNumber)
                        {
                            nLowestNumber = nItemNo;
                            nLowestRecNum = i;
                        }
                    }
                }
                if (nLowestRecNum != -1)
                {
                    // Modify the record to show that the item has been returned
                    dReturned++;
                    string sDate = DateTime.Now.Day.ToString();
                    if (sDate.Length < 2)
                        sDate = "0" + sDate;
                    string sMonth = DateTime.Now.Month.ToString();
                    if (sMonth.Length < 2)
                        sMonth = "0" + sMonth;
                    string sYear = DateTime.Now.Year.ToString()[2].ToString() + DateTime.Now.Year.ToString()[3].ToString();
                    sDate += sMonth + sYear;
                    tCommItems.EditRecordData(nLowestRecNum, 5, "N");
                    tCommItems.EditRecordData(nLowestRecNum, 6, sDate);
                    tCommItems.EditRecordData(nLowestRecNum, 4, "0.00");

                    // Decrease the QIS by the received amount
                    int nRecLoc = tStockStats.GetRecordNumberFromTwoFields(sBarcode, 0, sShopCode, 35);
                    decimal dQtyInStock = Convert.ToDecimal(tStockStats.GetRecordFrom(nRecLoc)[36]);
                    dQtyInStock -= 1;
                    tStockStats.EditRecordData(nRecLoc, 36, dQtyInStock.ToString());
                }
            }
            tCommItems.SaveToFile("COMMITEM.DBF");
            tStockStats.SaveToFile("STOCKSTA.DBF");
            return dReturned;
        }
            

        public string[,] GetSuggestedItemsForOrder(string sSupCode, string sShopCode, ref int nOfResults)
        {
            string sSugRecs = "";
            for (int i = 0; i < tOrderSuggestions.NumberOfRecords; i++)
            {
                string[] sSug = tOrderSuggestions.GetRecordFrom(i);
                if (sSug[1] == sSupCode && sSug[3] == sShopCode)
                {
                    sSugRecs += i.ToString() + ",";
                }
            }
            sSugRecs = sSugRecs.TrimEnd(',');
            string[,] sToReturn = new string[sSugRecs.Split(',').Length, 2];
            string[] sRecNums = sSugRecs.Split(',');
            nOfResults = sRecNums.Length;
            for (int i = 0; i < sRecNums.Length; i++)
            {
                int nRecNum = Convert.ToInt32(sRecNums[i]);
                string[] sRecData = tOrderSuggestions.GetRecordFrom(nRecNum);
                sToReturn[i, 0] = sRecData[0];
                sToReturn[i, 1] = sRecData[2];
            }
            return sToReturn;
        }

        public void RemoveSuggestedOrderItem(string sBarcode, string sShopCode)
        {
            this.FindMissingOrderLines();
            for (int i = 0; i < tOrderSuggestions.NumberOfRecords; i++)
            {
                if (tOrderSuggestions.GetRecordFrom(i)[0] == sBarcode && tOrderSuggestions.GetRecordFrom(i)[3] == sShopCode)
                {
                    tOrderSuggestions.DeleteRecord(i);
                    i = -1;
                }
            }
            tOrderSuggestions.SaveToFile("ORDERSUG.DBF");
            this.FindMissingOrderLines();
        }

        public void AddSuggestedOrderItem(string sBarcode, string sShopCode)
        {
            this.FindMissingOrderLines();
            if (GetMainStockInfo(sBarcode)[5] == "5")
            {
                sBarcode = GetMainStockInfo(sBarcode)[7];
            }
            int nOfResults = 0;
            string[,] sSuppliers = GetListOfSuppliersForItem(sBarcode, ref nOfResults);
            string sDay = DateTime.Now.Day.ToString();
            while (sDay.Length < 2)
                sDay = "0" + sDay;
            string sMonth = DateTime.Now.Month.ToString();
            while (sMonth.Length < 2)
                sMonth = "0" + sMonth;
            string sYear = DateTime.Now.Year.ToString()[2].ToString() + DateTime.Now.Year.ToString()[3].ToString();
            string sDate = sDay + sMonth + sYear;
            for (int i = 0; i < nOfResults; i++)
            {
                string[] sToAdd = { sBarcode, sSuppliers[i, 1], sDate, sShopCode };
                tOrderSuggestions.AddRecord(sToAdd);
            }
            tOrderSuggestions.SaveToFile("ORDERSUG.DBF");
            this.FindMissingOrderLines();
        }
        public void AddSuggestedOrderItem(string sBarcode, string sShopCode, string sDate)
        {
            this.FindMissingOrderLines();
            if (GetMainStockInfo(sBarcode)[5] == "5")
            {
                sBarcode = GetMainStockInfo(sBarcode)[7];
            }
            int nOfResults = 0;
            string[,] sSuppliers = GetListOfSuppliersForItem(sBarcode, ref nOfResults);
            for (int i = 0; i < nOfResults; i++)
            {
                string[] sToAdd = { sBarcode, sSuppliers[i, 1], sDate, sShopCode };
                tOrderSuggestions.AddRecord(sToAdd);
            }
            tOrderSuggestions.SaveToFile("ORDERSUG.DBF");
            this.FindMissingOrderLines();
        }
        public bool AnySuggestedItemsForSupplier(string sSupCode, string sShopCode)
        {
            this.FindMissingOrderLines();
            GetSuggestedItemsFromTills();
            for (int i = 0; i < tOrderSuggestions.NumberOfRecords; i++)
            {
                if (tOrderSuggestions.GetRecordFrom(i)[1] == sSupCode && tOrderSuggestions.GetRecordFrom(i)[3] == sShopCode)
                {
                    return true;
                }
            }
            return false;
            this.FindMissingOrderLines();
        }

        public void GetSuggestedItemsFromTills()
        {
            for (int i = 0; i < Till.Length; i++)
            {
                if (File.Exists(Till[i].FileLocation + "\\TILL\\TORDERSU.DBF"))
                {
                    Table tOrderSugs = new Table(Till[i].FileLocation + "\\TILL\\TORDERSU.DBF");
                    for (int x = 0; x < tOrderSugs.NumberOfRecords; x++)
                    {
                        string[] sRecord = tOrderSugs.GetRecordFrom(x);
                        AddSuggestedOrderItem(sRecord[0], Till[i].ShopCode, sRecord[1]);
                        tOrderSugs.DeleteRecord(x);
                        x = -1;
                    }
                    tOrderSugs.SaveToFile(Till[i].FileLocation + "\\TILL\\TORDERSU.DBF");
                }
            }
        }

        public string[] GetListOfAccountCodes()
        {
            string[] sToReturn = new string[tAccStat.NumberOfRecords];
            for (int i = 0; i < sToReturn.Length; i++)
            {
                sToReturn[i] = tAccStat.GetRecordFrom(i)[0];
            }
            Array.Sort(sToReturn);
            return sToReturn;
        }

        public string GetAccountName(string sAccCode)
        {
            string[] sToReturn = tAccStat.GetRecordFrom(sAccCode, 0, true);
            if (sToReturn.Length > 1)
                return sToReturn[2];
            else
                return "";
        }

        public string[] GetAccountRecord(string sAccCode)
        {
            return tAccStat.GetRecordFrom(sAccCode, 0, true);
        }

        public void AddEditAccountRecord(string[] sRecData)
        {
            for (int i = 0; i < tAccStat.NumberOfRecords; i++)
            {
                if (tAccStat.GetRecordFrom(i)[0] == sRecData[0])
                    tAccStat.DeleteRecord(i);
            }
            tAccStat.AddRecord(sRecData);
            tAccStat.SaveToFile("ACCSTAT.DBF");
        }

        public void AddItemToOrder(string sOrderNum, string sBarcode, string dOrderQty, string dRecQty, string sCostPrice)
        {
            this.FindMissingOrderLines();
            string[] sHeader = GetOrderHeader(sOrderNum);
            string[] sBarcodes = new string[0];
            string[] sCost = new string[0];
            string[] sOrderQty = new string[0];
            string[] sRecQty = new string[0];
            GetOrderData(sOrderNum, ref sBarcodes, ref sOrderQty, ref sRecQty, ref sCost);
            Array.Resize<string>(ref sBarcodes, sBarcodes.Length + 1);
            Array.Resize<string>(ref sCost, sCost.Length + 1);
            Array.Resize<string>(ref sOrderQty, sOrderQty.Length + 1);
            Array.Resize<string>(ref sRecQty, sRecQty.Length + 1);
            sBarcodes[sBarcodes.Length - 1] = sBarcode;
            sCost[sCost.Length - 1] = sCostPrice;
            sOrderQty[sOrderQty.Length - 1] = dOrderQty;
            sRecQty[sRecQty.Length - 1] = dRecQty;
            AddEditOrderData(sBarcodes, sOrderQty, sRecQty, sCost, sOrderNum);
            this.FindMissingOrderLines();
        }

        public string[] GetListOfCardDiscs()
        {
            int nOfCards = GetCreditCards().Length;
            string[] sDisc = new string[nOfCards];
            for (int i = 0; i < nOfCards; i++)
            {
                int nRecNum = 0;
                if (tSettings.SearchForRecord("CRD" + (i).ToString() + "DISC", 0, ref nRecNum))
                {
                    sDisc[i] = tSettings.GetRecordFrom(nRecNum)[1];
                }
                else
                {
                    sDisc[i] = "0.00";
                }
            }
            return sDisc;
        }

        public void SetListOfCardDiscs(string[] sList)
        {
            for (int i = 0; i < sList.Length; i++)
            {
                int nRecNum = 0;
                if (tSettings.SearchForRecord("CRD" + (i).ToString() + "DISC", 0, ref nRecNum))
                {
                    tSettings.DeleteRecord(nRecNum);
                }
            }
            for (int i = 0; i < sList.Length; i++)
            {
                if (Convert.ToDecimal(sList[i]) != 0)
                {
                    string[] sToAdd = { "CRD" + (i).ToString() + "DISC", sList[i] };
                    tSettings.AddRecord(sToAdd);
                }
            }
            tSettings.SaveToFile("SETTINGS.DBF");

            for (int i = 0; i < Till.Length; i++)
            {
                GenerateSettingsForTill(Till[i].Number);
            }
        }

        /*public decimal GetCommissionerAmountOwed(string sCommCode)
        {
            int nOfRecs = 0;
            string[] sAmounts = tCommItems.SearchAndGetAllMatchingRecords(0, sCommCode, ref nOfRecs, true, 3);
            decimal dAmountOwed = 0;
            for (int i = 0; i < nOfRecs; i++)
            {
                try
                {
                    dAmountOwed += Convert.ToDecimal(sAmounts[i]);
                }
                catch
                {
                    ;
                }
            }
            return dAmountOwed;
        }*/

        public string[] GetComissionerRecordOfItem(string sBarcode)
        {
            int nRecNum = -1;
            if (tCommItems.SearchForRecord(sBarcode.ToUpper(), 1, ref nRecNum))
            {
                return tCommItems.GetRecordFrom(nRecNum);
            }
            else
                return new string[0];
        }

        public string[] GetListOfStaffMembers(string sShopCode)
        {
            string[] sToReturn = new string[tStaff.NumberOfRecords];
            int nSkipped = 0;
            for (int i = 0; i < tStaff.NumberOfRecords; i++)
            {
                if (tStaff.GetRecordFrom(i)[1] == sShopCode)
                    sToReturn[i - nSkipped] = tStaff.GetRecordFrom(i)[2];
                else
                    nSkipped++;
            }
            Array.Resize<string>(ref sToReturn, tStaff.NumberOfRecords - nSkipped);
            return sToReturn;
        }

        public void SaveListOfStaffMembers(string[] sNames, string sShopCode)
        {
            for (int i = 0; i < sNames.Length; i++)
            {
                int nRecNum = tStaff.GetRecordNumberFromTwoFields((i + 1).ToString(), 0, sShopCode, 1);
                if (nRecNum != -1)
                    tStaff.DeleteRecord(nRecNum);
                string[] sToAdd = { (i + 1).ToString(), sShopCode, sNames[i] };
                tStaff.AddRecord(sToAdd);
            }
            tStaff.SaveToFile("STAFF.DBF");
            foreach (Till t in Till)
            {
                GenerateStaffFileForTill(t.Number);
            }
        }

        public void FixOnOrderQuantities()
        {
            int nCorrected = 0;
            frmProgressBar fp = new frmProgressBar("Correcting Records");
            fp.pb.Maximum = tStockStats.NumberOfRecords;
            fp.Show();
            for (int i = 0; i < tStockStats.NumberOfRecords; i++)
            {
                fp.pb.Value = i;
                System.Windows.Forms.Application.DoEvents();
                string[] sStockSta = tStockStats.GetRecordFrom(i);
                string[] sOrderNums = new string[0];
                string[] sSupCodes = new string[0];
                string[] sQuantities = new string[0];
                GetOrdersWithItemOutstandingIn(sStockSta[0], ref sOrderNums, ref sSupCodes, ref sQuantities);
                decimal dQtyOnOrder = 0;
                for (int x = 0; x < sOrderNums.Length; x++)
                {
                    string[] sOrderHeader = GetOrderHeader(sOrderNums[x]);
                    if (sOrderHeader[6] == sStockSta[35])
                    {
                        dQtyOnOrder += Convert.ToDecimal(sQuantities[x]);
                    }
                }
                decimal dCurrentOnOrder = Convert.ToDecimal(tStockStats.GetRecordFrom(i)[3]);
                if (dCurrentOnOrder != dQtyOnOrder)
                {
                    nCorrected++;
                    tStockStats.EditRecordData(i, 3, dQtyOnOrder.ToString());
                    fp.Text = sStockSta[0] + " fixed";
                }
            }
            tStockStats.SaveToFile("STOCKSTA.DBF");
            fp.Close();
            System.Windows.Forms.MessageBox.Show("Corrected " + nCorrected.ToString() + " records!");
        }

        public void SendCommandsToTill(string[] sCommands)
        {
            TextWriter tWriter = new StreamWriter("COMMANDS.TXT", false);
            for (int i = 0; i < sCommands.Length; i++)
            {
                tWriter.WriteLine(sCommands[i]);
            }
            tWriter.Close();
            foreach (Till t in Till)
            {
                if (Directory.Exists(t.FileLocation + "\\TILL"))
                {
                    File.Copy("COMMANDS.TXT", t.FileLocation + "\\TILL\\COMMANDS.TXT", true);
                }
            }
        }

        public void SendCommandToTill(string sCommand)
        {
            string[] sToSend = { sCommand };
            SendCommandsToTill(sToSend);
        }

        string sLastCatCode = "";
        public string LastCategoryCode
        {
            get
            {
                return sLastCatCode;
            }
            set
            {
                sLastCatCode = value;
            }
        }

        public void CollectEmailsFromTills()
        {
            for (int i = 0; i < Till.Length; i++)
            {
                if (File.Exists(Till[i].FileLocation + "\\TILL\\EMAILS.DBF"))
                {
                    Table tTillEmail = new Table(Till[i].FileLocation + "\\TILL\\EMAILS.DBF");
                    while (tTillEmail.NumberOfRecords > 0)
                    {
                        tEmails.AddRecord(tTillEmail.GetRecordFrom(0));
                        tTillEmail.DeleteRecord(0);
                    }
                    tTillEmail.SaveToFile(Till[i].FileLocation + "\\TILL\\EMAILS.DBF");
                }
            }
            tEmails.SaveToFile(sTDir + "EMAILS.DBF");
        }

        public void GetEmailAddresses(ref string[] sTitles, ref string[] sForeNames, ref string[] sSurNames, ref string[] sEmails, ref string[] sDates)
        {
            sTitles = new string[tEmails.NumberOfRecords];
            sForeNames = new string[tEmails.NumberOfRecords];
            sSurNames = new string[tEmails.NumberOfRecords];
            sEmails = new string[tEmails.NumberOfRecords];
            sDates = new string[tEmails.NumberOfRecords];

            for (int i = 0; i < tEmails.NumberOfRecords; i++)
            {
                string[] sRec = tEmails.GetRecordFrom(i);
                sTitles[i] = sRec[0];
                sForeNames[i] = sRec[1];
                sSurNames[i] = sRec[2];
                sEmails[i] = sRec[3];
                sDates[i] = sRec[4];
            }
        }

        public enum SortOrder {Barcode, QIS, OutOfStock, AvgSales};
        private class OutOfStockReportItem : IComparable
        {
            string[] stockStaRecord;
            string[] mainStockRecord;
            string[] stockLengthRecord;
            SortOrder sortOrder;

            public OutOfStockReportItem(string[] stockStaRecord, string[] mainStockRecord, string[] stockLengthRecord)
            {
                this.stockStaRecord = stockStaRecord;
                this.mainStockRecord = mainStockRecord;
                this.stockLengthRecord = stockLengthRecord;
            }

            public SortOrder SortOrder
            {
                get
                {
                    return this.sortOrder;
                }
                set
                {
                    this.sortOrder = value;
                }
            }

            public string Barcode
            {
                get
                {
                    return stockStaRecord[0];
                }
            }

            public decimal QIS
            {
                get
                {
                    return Convert.ToDecimal(stockStaRecord[36]);
                }
            }

            public decimal AverageSales
            {
                get
                {
                    return Convert.ToDecimal(stockStaRecord[2]);
                }
            }

            public decimal PercentageOutOfStock
            {
                get
                {
                    decimal dTotal = Convert.ToDecimal(stockLengthRecord[2]);
                    decimal dOut = Convert.ToDecimal(stockLengthRecord[3]);

                    decimal dPercentage = (100/dTotal) * dOut;
                    return Math.Round(dPercentage, 2);
                }
            }

            public string Description
            {
                get
                {
                    return mainStockRecord[1];
                }
            }
        
            public int  CompareTo(object obj)
            {
                OutOfStockReportItem iItem = (OutOfStockReportItem)obj;
                switch (this.sortOrder)
                {
                    case SortOrder.Barcode:
                        return String.Compare(iItem.Barcode, this.Barcode);
                        break;
                    case SortOrder.AvgSales:
                        return Decimal.Compare(iItem.AverageSales, this.AverageSales);
                        break;
                    case SortOrder.OutOfStock:
                        return Decimal.Compare(iItem.PercentageOutOfStock, this.PercentageOutOfStock);
                        break;
                    case SortOrder.QIS:
                        return Decimal.Compare(iItem.QIS, this.QIS);
                        break;
                }
                return 0;
            }

        }

        public void OutOfStockReportToPrinter(string sCategoryCode, string sShopCode, SortOrder order)
        {
            OutOfStockReportToFile(sCategoryCode, sShopCode, order);
            nLineLastPrinted = 6;
            nPrinterPage = 1;
            sReportTitle = "Out Of Stock Length Report for Items in Category " + GetCategoryDesc(sCategoryCode);
            rCurrentlyPrinting = ReportType.OutOfStockLengthReport;
            PrinterSettings pSettings = new PrinterSettings();
            pSettings.PrinterName = this.PrinterToUse;
            PrintDocument pPrinter = new PrintDocument();
            pPrinter.PrinterSettings = pSettings;
            pPrinter.DocumentName = "Out Of Stock Length Report for Items in Category " + GetCategoryDesc(sCategoryCode);
            pPrinter.PrintPage += new PrintPageEventHandler(ReportPrintPage);
            pPrinter.Print();
        }

        public void OutOfStockReportToFile(string sCategoryCode, string sShopCode, SortOrder order)
        {
            TextWriter tWriter = new StreamWriter("REPORT.TXT", false);
            string sDash = "-";
            while (sDash.Length < ("Out Of Stock Length Report for Items in Category " + GetCategoryDesc(sCategoryCode)).ToString().Length)
                sDash += "-";
            tWriter.WriteLine(sDash);
            tWriter.WriteLine("Out Of Stock Length Report for Items in Category " + GetCategoryDesc(sCategoryCode));
            tWriter.WriteLine(sDash);

            tWriter.WriteLine("-------------------------------------------------------------------------------------------");
            tWriter.WriteLine("Barcode        Description                     Q.I.S    Out Of Stock      Average Sales/Day");
            tWriter.WriteLine("-------------------------------------------------------------------------------------------");

            string[] sBarcodes = this.GetCodesOfItemsInCategory(sCategoryCode, true);
            int nToSkip = 0;
            for (int i = 0; i < sBarcodes.Length; i++)
            {
                if (tStock.GetRecordFrom(sBarcodes[i], 0, true)[5] != "1")
                {
                    sBarcodes[i] = null;
                    nToSkip++;
                }
            }

            int nSkipped = 0;

            OutOfStockReportItem[] oItems = new OutOfStockReportItem[sBarcodes.Length - nToSkip];
            for (int i = 0; i < sBarcodes.Length; i++)
            {
                if (sBarcodes[i] == null)
                {
                    nSkipped++;
                    continue;
                }
                int nStockStats = tStockStats.GetRecordNumberFromTwoFields(sBarcodes[i], 0, sShopCode, 35);
                if (nStockStats == -1)
                    throw new Exception("Trying to find " + sBarcodes[i] + " in stockstats (shop " + sShopCode + ", but it's not there!");

                int nStockLength = tStockLength.GetRecordNumberFromTwoFields(sBarcodes[i], 0, sShopCode, 1);
                 oItems[i-nSkipped] = new OutOfStockReportItem(tStockStats.GetRecordFrom(nStockStats), tStock.GetRecordFrom(sBarcodes[i], 0, true), tStockLength.GetRecordFrom(nStockLength));
                oItems[i-nSkipped].SortOrder = order;
            }

            Array.Sort(oItems);

            for (int i = 0; i < oItems.Length; i++)
            {
                string sToWrite = oItems[i].Barcode;
                while (sToWrite.Length < 15)
                    sToWrite = sToWrite + " ";

                sToWrite += oItems[i].Description;
                while (sToWrite.Length < 47)
                    sToWrite += " ";

                while (sToWrite.Length + Math.Round(oItems[i].QIS, 2).ToString().Length < 56)
                    sToWrite += " ";
                sToWrite += Math.Round(oItems[i].QIS, 2).ToString();

                while (sToWrite.Length + Math.Round(oItems[i].PercentageOutOfStock, 2).ToString().Length < 73)
                    sToWrite += " ";
                sToWrite += Math.Round(oItems[i].PercentageOutOfStock, 2).ToString() + "%";

                while (sToWrite.Length + Math.Round(oItems[i].AverageSales, 3).ToString().Length < 90)
                    sToWrite += " ";
                sToWrite += Math.Round(oItems[i].AverageSales, 3).ToString();

                tWriter.WriteLine(sToWrite);
            }

            tWriter.Close();
        }



        public void OutStandingItemsToFile(string sSupCode)
        {
            frmProgressBar fp = new frmProgressBar("Creating Outstanding Report");
            fp.pb.Maximum = tOrder.NumberOfRecords;
            fp.Show();
            tOrder.SortTable();
            TextWriter tWriter = new StreamWriter("REPORT.TXT", false);
            tWriter.WriteLine("----------------------------------------------------------");
            tWriter.WriteLine("Items Outstanding From " + GetSupplierDetails(sSupCode)[1]);
            tWriter.WriteLine("----------------------------------------------------------");
            tWriter.WriteLine("------------------------------------------------------------------------------");
            tWriter.WriteLine("Barcode        Description                     Ordered   Outstanding     Value");
            tWriter.WriteLine("------------------------------------------------------------------------------");
            for (int x = tOrder.NumberOfRecords - 1; x >= 0; x -= 1)
            {
                fp.pb.Value = tOrder.NumberOfRecords - x;
                decimal dTotal = 0;
                if (tOrder.GetRecordFrom(x)[1].ToUpper() == sSupCode.ToUpper())
                {
                    string sOrderNum = tOrder.GetRecordFrom(x)[0];
                    int nOfLines = 0;
                    bool bWrittenTitle = false;
                    string[,] sResults = tOrderLine.SearchAndGetAllMatchingRecords(0, sOrderNum, ref nOfLines, true);
                    for (int i = 0; i < nOfLines; i++)
                    {
                        if (GetMainStockInfo(sResults[i, 2]).Length > 1)
                        {
                            decimal dQtyOrdered = Convert.ToDecimal(sResults[i, 3]);
                            decimal dQtyReceieved = Convert.ToDecimal(sResults[i, 4]);
                            if (dQtyOrdered - dQtyReceieved > 0)
                            {
                                if (!bWrittenTitle)
                                {
                                    tWriter.WriteLine("-------------------------------------------");
                                    string sDate = tOrder.GetRecordFrom(x)[5];
                                    string sFormatted = sDate[0].ToString() + sDate[1].ToString() + "/" + sDate[2].ToString() + sDate[3].ToString() + "/" + sDate[4].ToString() + sDate[5].ToString();
                                    tWriter.WriteLine("Order Number " + tOrder.GetRecordFrom(x)[0] + ". Ordered : " + sFormatted);
                                    tWriter.WriteLine("--------------------------------------------");
                                    bWrittenTitle = true;
                                }
                                string sToWrite = sResults[i, 2];
                                while (sToWrite.Length < 15)
                                    sToWrite += " ";
                                sToWrite += GetMainStockInfo(sResults[i, 2])[1];
                                while (sToWrite.Length < 47)
                                    sToWrite += " ";
                                while (sToWrite.Length + FormatMoneyForDisplay(dQtyOrdered).Length < 54)
                                    sToWrite += " ";
                                sToWrite += FormatMoneyForDisplay(dQtyOrdered);
                                while (sToWrite.Length + FormatMoneyForDisplay(dQtyOrdered - dQtyReceieved).Length < 68)
                                    sToWrite += " ";
                                sToWrite += FormatMoneyForDisplay(dQtyOrdered - dQtyReceieved);
                                decimal dCost = Convert.ToDecimal(sResults[i, 5]) * (dQtyOrdered - dQtyReceieved);
                                while (sToWrite.Length + FormatMoneyForDisplay(dCost).Length < 78)
                                    sToWrite += " ";
                                sToWrite += FormatMoneyForDisplay(dCost);
                                dTotal += dCost;
                                tWriter.WriteLine(sToWrite);
                            }
                        }
                    }
                    if (dTotal > 0)
                    {
                        string sTotalLine = "";
                        while (sTotalLine.Length + "Total : ".Length + FormatMoneyForDisplay(dTotal).Length < 78)
                            sTotalLine += " ";
                        sTotalLine += "Total : " + FormatMoneyForDisplay(dTotal);
                        tWriter.WriteLine("------------------------------------------------------------------------------");
                        tWriter.WriteLine(sTotalLine);
                        tWriter.WriteLine("------------------------------------------------------------------------------");
                        tWriter.WriteLine(""); 
                    }
                }
            }
            fp.Close();
            tWriter.WriteLine("End Of Report");
            tWriter.Close();
        }

        public void OutStandingItemsToPrinter(string sSupCode)
        {
            OutStandingItemsToFile(sSupCode);
            nLineLastPrinted = 6;
            nPrinterPage = 1;
            sReportTitle = "Outstanding items from " + GetSupplierDetails(sSupCode)[1];
            rCurrentlyPrinting = ReportType.OutStandingItems;
            PrinterSettings pSettings = new PrinterSettings();
            pSettings.PrinterName = this.PrinterToUse;
            PrintDocument pPrinter = new PrintDocument();
            pPrinter.PrinterSettings = pSettings;
            pPrinter.DocumentName = "Outstanding Items from " + GetSupplierDetails(sSupCode)[1];
            pPrinter.PrintPage += new PrintPageEventHandler(ReportPrintPage);
            pPrinter.Print();
        }

        public void MarkCommissionerAsPaid(string sCommCode)
        {
            for (int i = 0; i < tCommItems.NumberOfRecords; i++)
            {
                if (tCommItems.GetRecordFrom(i)[0] == sCommCode)
                {
                    tCommItems.EditRecordData(i, 2, "0");
                    tCommItems.EditRecordData(i, 3, "0");
                    tCommItems.EditRecordData(i, 4, "0");
                }
            }
            tCommItems.SaveToFile("COMMITEM.DBF");
        }

        public void UpdateTillSoftware()
        {
            if (File.Exists("Update\\GTill.exe"))
            {
                for (int i = 0; i < Till.Length; i++)
                {
                    try
                    {
                        File.Copy("UPDATE\\GTILL.EXE", Till[i].FileLocation + "\\TILL\\GTillUpdate.EXE", true);
                        File.Copy("UPDATE\\onlineBuildNum.txt", Till[i].FileLocation + "\\TILL\\buildNum.txt", true);
                    }
                    catch
                    {
                        System.Windows.Forms.MessageBox.Show("Can't access Till " + Till[i].Number.ToString() + ", so it won't be updated this time.");
                    }
                }
                File.Delete("Update\\GTILL.EXE");
                SendCommandToTill("UpdateSoftware");
            }
        }

       

        public bool[] TillsConnected(ref int[] sCodes)
        {
            bool[] bToReturn = new bool[Till.Length];
            sCodes = new int[Till.Length];
            for (int i = 0; i < Till.Length; i++)
            {
                if (Directory.Exists(Till[i].FileLocation))
                {
                    bToReturn[i] = true;
                }
                else
                {
                    bToReturn[i] = false;
                }
                sCodes[i] = Till[i].Number;
            }
            return bToReturn;
        }

        public string[] GetListOfTypeFourItems(string sShopCode)
        {
            int nOfRecords = 0;
            return tMultiHeader.SearchAndGetAllMatchingRecords(3, sShopCode, ref nOfRecords, true, 0);
        }

        public void RemoveBlankBarcode()
        {
            for (int i = 0; i < tStockStats.NumberOfRecords; i++)
            {
                if (tStockStats.GetRecordFrom(i)[0] == "")
                {
                    tStockStats.DeleteRecord(i);
                    i -= 1;
                }
            }
        }

        private string AddCheckDigitToBarcode(string sBarcode)
        {
            int nOddSum = 0;
            int nEvenSum = 0;
            for (int i = 0; i < sBarcode.Length; i += 2)
            {
                nOddSum += Convert.ToInt32(sBarcode[i]);
            }
            for (int i = 1; i < sBarcode.Length; i += 2)
            {
                nEvenSum += Convert.ToInt32(sBarcode[i]);
            }
            nOddSum *= 3;
            nOddSum += nEvenSum;
            nOddSum = 10 - (nOddSum % 10);
            return sBarcode + nOddSum.ToString()[nOddSum.ToString().Length - 1];
        }

        public bool GotEmailSupportAddress()
        {
            int nRecNum = 0;
            return tSettings.SearchForRecord("EmailAddress", 0, ref nRecNum);
        }

        public string GetEmailSupportAddress()
        {
            if (!GotEmailSupportAddress())
            {
                return "";
            }
            else
            {
                return tSettings.GetRecordFrom("EmailAddress", 0, true)[1];
            }
        }

        public void SetEmailSupportAddress(string sNewAddress)
        {
            if (!GotEmailSupportAddress())
            {
                string[] sToAdd = { "EmailAddress", sNewAddress };
                tSettings.AddRecord(sToAdd);
                tSettings.SaveToFile("SETTINGS.DBF");
            }
            else
            {
                int nRecNum = 0;
                tSettings.SearchForRecord("EmailAddress", 0, ref nRecNum);
                tSettings.EditRecordData(nRecNum, 1, sNewAddress);
                tSettings.SaveToFile("SETTINGS.DBF");
            }
        }

        public string PrinterToUse
        {
            get
            {
                string[] setting = tSettings.GetRecordFrom("PRINTER", 0, true);
                bool defaultExists = false;
                if (setting.Length > 1)
                {
                    for (int i = 0; i < PrinterSettings.InstalledPrinters.Count; i++)
                    {
                        if (setting[1].Equals(PrinterSettings.InstalledPrinters[i]))
                        {
                            defaultExists = true;
                            return setting[1];
                        }
                    }
                }
                
                if (!defaultExists)
                {
                    PrinterSettings pSettings = new PrinterSettings();
                    for (int i = 0; i < PrinterSettings.InstalledPrinters.Count; i++)
                    {
                        pSettings.PrinterName = PrinterSettings.InstalledPrinters[i];
                        if (pSettings.IsDefaultPrinter)
                            return pSettings.PrinterName;
                    }
                }
                return "Unknown";
            }

            set
            {
                int settingLoc = -1;
                tSettings.SearchForRecord("PRINTER", 0, ref settingLoc);
                if (settingLoc != -1)
                {
                    tSettings.DeleteRecord(settingLoc);
                }

                string[] newSetting = { "PRINTER", value };
                tSettings.AddRecord(newSetting);
                tSettings.SaveToFile("SETTINGS.DBF");
            }
        }

        public void ChangeItemsVATCodes(string sOldCode, string sNewCode)
        {
            for (int i = 0; i < tStock.NumberOfRecords; i++)
            {
                if (tStock.GetRecordFrom(i)[3].Equals(sOldCode.ToUpper()))
                {
                    tStock.EditRecordData(i, 3, sNewCode.ToUpper());
                }
            }
            tStock.SaveToFile("MAINSTOC.DBF");
        }

        public void ChangeLastCostOfItem(string sBarcode, decimal dNewAmount)
        {
            int nRecLoc = -1;
            tStock.SearchForRecord(sBarcode, 0, ref nRecLoc);
            if (nRecLoc != -1)
            {
                tStock.EditRecordData(nRecLoc, 8, dNewAmount.ToString());
            }
            tStock.SaveToFile("MAINSTOC.DBF");
        }

        public void ChangeVATOnItems(string sVATCode, decimal dNewVATRate)
        {
            decimal dOldVATRate = GetVATRateFromVATCode(sVATCode);
            dOldVATRate /= 100;
            dOldVATRate += 1;
            dNewVATRate /= 100;
            dNewVATRate += 1;
            frmProgressBar fpBar = new frmProgressBar("Changing VAT Rate");
            fpBar.Show();
            fpBar.pb.Maximum = tStock.NumberOfRecords;
            for (int i = 0; i < tStock.NumberOfRecords; i++)
            {
                if (tStock.GetRecordFrom(i)[3].Equals(sVATCode))
                {
                    decimal dCurrentPrice = Convert.ToDecimal(tStock.GetRecordFrom(i)[2]);
                    dCurrentPrice /= dOldVATRate;
                    dCurrentPrice *= dNewVATRate;
                    tStock.EditRecordData(i, 2, Math.Round(dCurrentPrice,2).ToString());
                }
                fpBar.pb.Value= i;
            }
            tStock.SaveToFile("MAINSTOC.DBF");
            fpBar.Close();
        }

        public void ChangeStockStaField(string sBarcode, string sFieldName, string sData, string sShopCode)
        {
            int nFieldNum = tStockStats.FieldNumber(sFieldName);
            int nRecNum = tStockStats.GetRecordNumberFromTwoFields(sBarcode, 0, sShopCode, 35);
            tStockStats.EditRecordData(nRecNum, nFieldNum, sData);
            tStockStats.SaveToFile("STOCKSTA.DBF");
        }

        public void FixEndOfYearBug()
        {
            string sFolderName = Directory.GetDirectories("Archive\\Yearly")[0].ToString();
            Table tOldYear = new Table(sFolderName + "\\STOCKSTA.DBF");
            for (int i = 0; i < tOldYear.NumberOfRecords; i++)
            {
                int nLoc = tStockStats.GetRecordNumberFromTwoFields(tOldYear.GetRecordFrom(i)[0], 0, tOldYear.GetRecordFrom(i)[tOldYear.FieldNumber("SHOPCODE")], tOldYear.FieldNumber("SHOPCODE"));
                tStockStats.EditRecordData(nLoc, 25, tOldYear.GetRecordFrom(i)[17]);
                tStockStats.EditRecordData(nLoc, 26, tOldYear.GetRecordFrom(i)[18]);
                tStockStats.EditRecordData(nLoc, 27, tOldYear.GetRecordFrom(i)[19]);
                tStockStats.EditRecordData(nLoc, 28, tOldYear.GetRecordFrom(i)[20]);
            }
            tStockStats.SaveToFile("STOCKSTA.DBF");
        }

        public void FixAverageCostZeroBug(string sStockStaFile, string sMainStockFile)
        {
            Table tStockStaToFix = new Table(sStockStaFile);
            Table tMainStockToFix = new Table(sMainStockFile);

            for (int i = 0; i < tStockStaToFix.NumberOfRecords; i++)
            {
                if (Convert.ToDecimal(tStockStaToFix.GetRecordFrom(i)[1]) == 0 && Convert.ToDecimal(tMainStockToFix.GetRecordFrom(tStockStaToFix.GetRecordFrom(i)[0], 0, true)[8]) != 0)
                {
                    tStockStaToFix.EditRecordData(i, 1, tMainStockToFix.GetRecordFrom(tStockStaToFix.GetRecordFrom(i)[0], 0, true)[8]);
                }
            }
            tStockStaToFix.SaveToFile(sStockStaFile);
        }

        public void FixCOGSZeroBug(string sStockStaFile)
        {
            Table tStockStaToFix = new Table(sStockStaFile);

            for (int i = 0; i < tStockStaToFix.NumberOfRecords; i++)
            {
                // Daily
                if (Convert.ToDecimal(tStockStaToFix.GetRecordFrom(i)[1]) != 0 && Convert.ToDecimal(tStockStaToFix.GetRecordFrom(i)[5]) != 0 && Convert.ToDecimal(tStockStaToFix.GetRecordFrom(i)[8]) == 0)
                {
                    // Faulty Item
                    tStockStaToFix.EditRecordData(i, 8, (Convert.ToDecimal(tStockStaToFix.GetRecordFrom(i)[1]) * Convert.ToDecimal(tStockStaToFix.GetRecordFrom(i)[5])).ToString());
                }
                // Weekly
                if (Convert.ToDecimal(tStockStaToFix.GetRecordFrom(i)[1]) != 0 && Convert.ToDecimal(tStockStaToFix.GetRecordFrom(i)[9]) != 0 && Convert.ToDecimal(tStockStaToFix.GetRecordFrom(i)[12]) == 0)
                {
                    // Faulty Item
                    tStockStaToFix.EditRecordData(i, 12, (Convert.ToDecimal(tStockStaToFix.GetRecordFrom(i)[1]) * Convert.ToDecimal(tStockStaToFix.GetRecordFrom(i)[9])).ToString());
                }
                // Monthly
                if (Convert.ToDecimal(tStockStaToFix.GetRecordFrom(i)[1]) != 0 && Convert.ToDecimal(tStockStaToFix.GetRecordFrom(i)[13]) != 0 && Convert.ToDecimal(tStockStaToFix.GetRecordFrom(i)[16]) == 0)
                {
                    // Faulty Item
                    tStockStaToFix.EditRecordData(i, 16, (Convert.ToDecimal(tStockStaToFix.GetRecordFrom(i)[1]) * Convert.ToDecimal(tStockStaToFix.GetRecordFrom(i)[13])).ToString());
                }
                // Yearly
                if (Convert.ToDecimal(tStockStaToFix.GetRecordFrom(i)[1]) != 0 && Convert.ToDecimal(tStockStaToFix.GetRecordFrom(i)[17]) != 0 && Convert.ToDecimal(tStockStaToFix.GetRecordFrom(i)[20]) == 0)
                {
                    // Faulty Item
                    tStockStaToFix.EditRecordData(i, 20, (Convert.ToDecimal(tStockStaToFix.GetRecordFrom(i)[1]) * Convert.ToDecimal(tStockStaToFix.GetRecordFrom(i)[17])).ToString());
                }
            }

            tStockStaToFix.SaveToFile(sStockStaFile);
        }

        public void FixAverageCostZero()
        {
            // Let's do this

            // First, get a list of all items whose average cost is zero when it shouldn't be
            List<String> sFaultyItems = new List<string>();

            for (int i = 0; i < tStockStats.NumberOfRecords; i++)
            {
                // Check first if the Average Cost equals zero, and if it does, check if the last cost in mainstock
                // doesn't equal zero
                if (Convert.ToDecimal(tStockStats.GetRecordFrom(i)[1]) == 0 &&
                    Convert.ToDecimal(tStock.GetRecordFrom(tStockStats.GetRecordFrom(i)[0], 0, true)[8]) != 0)
                {
                    // Discrepancy found
                    // Add to list of faulty barcodes
                    sFaultyItems.Add(tStockStats.GetRecordFrom(i)[0]);
                    Console.WriteLine(tStockStats.GetRecordFrom(i)[0]);
                }
                
            }

            // Now we've got a list of faulty barcodes

            // Unfortunately, not all are faulty from this particular bug, and should be left faulty
            // This can be checked by looking at the first Archive of 2011, and seeing if the average cost was 0 then
            // If it was, then remove the code from the list of faulty ones

            Table tFirstOfTheYear = new Table("Archive\\Daily\\2011.01.04\\STOCKSTA.DBF");

            for (int i = 0; i < sFaultyItems.Count; i++)
            {
                int nRecNum = -1;
                if (tFirstOfTheYear.SearchForRecord(sFaultyItems[i], 0, ref nRecNum))
                {
                    if (Convert.ToDecimal(tFirstOfTheYear.GetRecordFrom(nRecNum)[1]) == 0)
                    {
                        tStockStats.SearchForRecord(sFaultyItems[i], 0, ref nRecNum);
                        tStockStats.EditRecordData(nRecNum, 1, tStock.GetRecordFrom(sFaultyItems[i], 0, true)[8]);
                        Console.WriteLine("Removing " + sFaultyItems[i]);
                        sFaultyItems.Remove(sFaultyItems[i]);
                        i--;
                    }
                }
            }

            // This doesn't remove enough, check to see if an item has been received this year
            // If not, set the ave cost to last cost

            for (int i = 0; i < sFaultyItems.Count; i++)
            {
                int nRecNum = -1;
                tStockStats.SearchForRecord(sFaultyItems[i], 0, ref nRecNum);
                decimal dYearlyQtdSold = Convert.ToDecimal(tStockStats.GetRecordFrom(nRecNum)[23]);
                if (dYearlyQtdSold == 0)
                {
                    tStockStats.EditRecordData(nRecNum, 1, tStock.GetRecordFrom(sFaultyItems[i], 0, true)[8]);
                    Console.WriteLine("Removing " + sFaultyItems[i]);
                    sFaultyItems.RemoveAt(i);
                    i--;
                }
            }

            // Remove even more (and pray it doesn't leave many)
            // Check if the YCOGS = YSOLD * LASTCOST, if so, set the average to last cost and remove it

            for (int i = 0; i < sFaultyItems.Count; i++)
            {
                int nRecNum = -1;
                tStockStats.SearchForRecord(sFaultyItems[i], 0, ref nRecNum);
                if (Convert.ToDecimal(tStockStats.GetRecordFrom(nRecNum)[17]) * Convert.ToDecimal(tStock.GetRecordFrom(sFaultyItems[i], 0, true)[8]) == Convert.ToDecimal(tStockStats.GetRecordFrom(nRecNum)[20]))
                {
                    tStockStats.EditRecordData(nRecNum, 1, tStock.GetRecordFrom(sFaultyItems[i], 0, true)[8]);
                    tStockStats.SaveToFile("STOCKSTA.DBF");
                    Console.WriteLine("Fixing & Removing " + sFaultyItems[i]);
                    sFaultyItems.RemoveAt(i);
                    i--;
                }
            }

            TextWriter tWriter = new StreamWriter("FAULTY.TXT");
            for (int i = 0; i < sFaultyItems.Count; i++)
            {
                tWriter.WriteLine(sFaultyItems[i]);
            }
            tWriter.Close();

           

            Console.WriteLine("That leaves... {0} items to remove!", sFaultyItems.Count);

            Console.WriteLine("Press any key to continue");
            Console.ReadLine();
            tStockStats.SaveToFile("STOCKSTA.DBF");

            // Got to find the date that the average was changed to 0, and re-calculate the average from there on
            // Holy hell

            for (int i = 0; i < sFaultyItems.Count; i++)
            {
                Console.WriteLine("Starting on " + sFaultyItems[i]);
                DateTime dtCurrent = DateTime.Now.AddDays(-1);

                int nOfDaysMissed = 0;

                string sDateBeforeChanged = "";
                DateTime dtDateBeforeChanged = new DateTime(1, 1, 1);
                decimal dAverageCost = 0;
                decimal dQuantityReceived = 0;
                decimal dYearlyDelCost = 0;

                decimal dYearlyCOGS = 0;

                // If 8 days have passed with no archive, assume that there are no more folders
                while (nOfDaysMissed < 8)
                {
                    // Compose the string for the folder name to look for
                    string sDirToSearchFor = "Archive\\Daily\\" + dtCurrent.Year.ToString() + ".";
                    if (dtCurrent.Month < 10)
                        sDirToSearchFor += "0";

                    sDirToSearchFor += dtCurrent.Month.ToString() + ".";

                    if (dtCurrent.Day < 10)
                        sDirToSearchFor += "0";

                    sDirToSearchFor += dtCurrent.Day.ToString();

                    //Console.WriteLine("Searching " + sDirToSearchFor);

                    // Check to see if the directory exists
                    if (!Directory.Exists(sDirToSearchFor))
                    {
                        // If it doesn't, add it to the count and try subtracting another day
                        nOfDaysMissed++;
                        dtCurrent = dtCurrent.AddDays(-1);
                    }
                    else
                    {
                        // Reset the counter as a day was found
                        nOfDaysMissed = 0;
                        //Console.Write("Checking " + sDirToSearchFor + " for " + sFaultyItems[i] + " change");

                        // Load the stocksta table from that day
                        Table tTempStockSta = new Table(sDirToSearchFor + "\\STOCKSTA.DBF");
                        int nRecNum = -1;
                        // Check to see if the item actually exists
                        if (tTempStockSta.SearchForRecord(sFaultyItems[i], 0, ref nRecNum))
                        {
                            // Item exists
                            // Check to see if average is 0.00
                            if (Convert.ToDecimal(tTempStockSta.GetRecordFrom(nRecNum)[1]) != 0)
                            {
                                Console.WriteLine("Found the date that {0} changed, it was {1}", sFaultyItems[i], sDirToSearchFor);
                                // The average wasn't 0, record the date, average cost, quantity received to date this year, and stop searching
                                sDateBeforeChanged = sDirToSearchFor;
                                dtDateBeforeChanged = dtCurrent;
                                dAverageCost = Convert.ToDecimal(tTempStockSta.GetRecordFrom(nRecNum)[1]);
                                dQuantityReceived = Convert.ToDecimal(tTempStockSta.GetRecordFrom(nRecNum)[23]);
                                dYearlyCOGS = Convert.ToDecimal(tTempStockSta.GetRecordFrom(nRecNum)[20]);
                                dYearlyDelCost = Convert.ToDecimal(tTempStockSta.GetRecordFrom(nRecNum)[24]);
                                break;
                            }
                            else
                            {
                                // The average was still 0, continue searching
                                //Console.WriteLine("... no");
                            }
                        }
                        else
                        {
                            // Got a problem - the item exists, its average was always 0, but the last cost wasn't
                            //throw new Exception("Average has always been 0, despite last cost not being");
                            Console.WriteLine("Average cost for {0} has always been 0?", sFaultyItems[i]);
                            nOfDaysMissed = 10;
                        }
                        // Subtract a day, and continue the search!
                        dtCurrent = dtCurrent.AddDays(-1);
                    }
                }

                // Check that the date was found
                if (sDateBeforeChanged != "")
                {
                    Console.WriteLine("Checking for increases");
                    // Now go forward through the archive, checking the YDELIVERED field for increases

                    int nDaysMissed = 0;

                    while (nDaysMissed < 8)
                    {
                        dtDateBeforeChanged = dtDateBeforeChanged.AddDays(1);


                        // Compose the string for the folder name to look for
                        string sDirToSearchFor = "Archive\\Daily\\" + dtDateBeforeChanged.Year.ToString() + ".";
                        if (dtDateBeforeChanged.Month < 10)
                            sDirToSearchFor += "0";

                        sDirToSearchFor += dtDateBeforeChanged.Month.ToString() + ".";

                        if (dtDateBeforeChanged.Day < 10)
                            sDirToSearchFor += "0";

                        sDirToSearchFor += dtDateBeforeChanged.Day.ToString();

                        // Check to see if the directory exists
                        if (!Directory.Exists(sDirToSearchFor))
                        {
                            // The directory doesn't exist, add one to the counter and try again
                            nDaysMissed++;
                            dtDateBeforeChanged = dtDateBeforeChanged.AddDays(1);
                            continue;
                        }
                        else
                        {
                            // Does exist, reset the counter
                            nDaysMissed = 0;

                            // Load the table
                            Table tTemp = new Table(sDirToSearchFor + "\\STOCKSTA.DBF");
                            
                            // Check that the item exists 
                            int nRecNum = -1;
                            if (tTemp.SearchForRecord(sFaultyItems[i], 0, ref nRecNum))
                            {
                                // Item found
                                // Check the quantity received against the previous day's quantity

                                decimal dTodayQtyRecd = Convert.ToDecimal(tTemp.GetRecordFrom(nRecNum)[23]);

                                if (dTodayQtyRecd != dQuantityReceived)
                                {
                                    Console.WriteLine("Quantities aren't the same");
                                    // Some items have been received, work out the new average
                                    
                                    // Work out how many were received today
                                    decimal dQtyReceived = dTodayQtyRecd - dQuantityReceived;

                                    // Get their cost from the last cost field in mainstock
                                    decimal dLastCost = 0;
                                    Table tMainStockTemp = new Table(sDirToSearchFor + "\\MAINSTOC.DBF");
                                    int nMSRecNum = -1;
                                    if (tMainStockTemp.SearchForRecord(sFaultyItems[i], 0, ref nMSRecNum))
                                    {
                                        // The item was found in mainstock
                                        dLastCost = Convert.ToDecimal(tMainStockTemp.GetRecordFrom(nMSRecNum)[8]);
                                    }
                                    else
                                    {
                                        // Why wasn't it found?? Shouldn't ever occur
                                        throw new Exception("Barcode in StockSta but not Mainstock!?");
                                    }

                                    // Work out the qty in stock before receiving new stock
                                    decimal dQtyInStock = Convert.ToDecimal(tTemp.GetRecordFrom(nRecNum)[36]);
                                    dQtyInStock -= dQtyReceived;

                                    decimal dPrevAverage = dAverageCost;
                                    // Work out the new average cost
                                    dAverageCost = Math.Round(((dQtyInStock * dAverageCost) + (dLastCost * dQtyReceived)) / (dQtyReceived + dQtyInStock), 2, MidpointRounding.AwayFromZero);

                                    // Record the new average cost to the database
                                    tTemp.EditRecordData(nRecNum, 1, dAverageCost.ToString());

                                    // Need to calculate the yearly delivery cost and record it
                                    dYearlyDelCost += Math.Round(dLastCost * dQtyReceived, 2);
                                    tTemp.EditRecordData(nRecNum, 24, dYearlyDelCost.ToString());

                                    

                                    Console.WriteLine("Some {0} were recevied - Previous Average {1}, Previous QIS {2}, New QIS {3}, New Average {4}", sFaultyItems[i], dPrevAverage.ToString(), dQtyInStock.ToString(), (dQtyInStock + dQtyReceived).ToString(), dAverageCost.ToString());
                                    dQuantityReceived = dTodayQtyRecd;
                                }

                                // Now check if any have sold, and if so, increase the YCOGS by the average * qty sold
                                decimal dQtySold = Convert.ToDecimal(tTemp.GetRecordFrom(nRecNum)[5]);
                                dYearlyCOGS += Math.Round(dQtySold * dAverageCost, 2);
                                tTemp.EditRecordData(nRecNum, 20, dYearlyCOGS.ToString());

                                Console.WriteLine("{0}x {1} sold on {2}, YCOGS increased by {3} to {4}", dQtySold.ToString(), sFaultyItems[i], dtDateBeforeChanged.ToString(), (dQtySold * dAverageCost).ToString(), dYearlyCOGS.ToString());



                                // Save the table
                                tTemp.SaveToFile(sDirToSearchFor + "\\STOCKSTA.DBF");
                            }
                            else
                            {
                                // Item not found
                                throw new Exception("Some bad programming going on here");
                            }
                            
                        }

                    }

                    // Now that all of the archives have been updated, update the current STOCKSTA

                    int nRecLoc = -1;
                    tStockStats.SearchForRecord(sFaultyItems[i], 0, ref nRecLoc);
                    if (nRecLoc != -1)
                    {
                        tStockStats.EditRecordData(nRecLoc, 1, dAverageCost.ToString());
                        tStockStats.EditRecordData(nRecLoc, 20, dYearlyCOGS.ToString());
                        tStockStats.EditRecordData(nRecLoc, 24, dYearlyDelCost.ToString());
                    }
                    tStockStats.SaveToFile("STOCKSTA.DBF");
                }
            }
        }

        public void EditAverageCostOfItem(string sBarcode, string sShopCode, string sNewAveCost)
        {
            int nRecNum = tStockStats.GetRecordNumberFromTwoFields(sBarcode, 0, sShopCode, 35);
            if (nRecNum != -1)
            {
                tStockStats.EditRecordData(nRecNum, 1, sNewAveCost);
                tStockStats.SaveToFile("STOCKSTA.DBF");
            }
        }

        public string GetLastVersion
        {
            get
            {
                if (tSettings.SearchForRecord("LASTVER", "SETTINGNAM"))
                {
                    return tSettings.GetRecordFrom("LASTVER", 0)[1];
                }
                else
                {
                    return "";
                }
            }
            set
            {
                int nRecNum = 0;
                tSettings.SearchForRecord("LASTVER", 0, ref nRecNum);
                if (nRecNum != -1)
                {
                    tSettings.DeleteRecord(nRecNum);
                }
                string[] sToadd = { "LASTVER", value };
                tSettings.AddRecord(sToadd);
                tSettings.SaveToFile("SETTINGS.DBF");
            }
        }

        void CheckForVersionChange()
        {
            if (File.Exists("buildNum.txt"))
            {
                TextReader tReader = new StreamReader("buildNum.txt");
                string current = tReader.ReadLine();
                string version = current.Split(' ')[1];
                tReader.Close();

                if (version != this.GetLastVersion)
                {
                    // Do something
                    this.GetLastVersion = version;
                }
            }
        }

        public void FixIncorrectPriceEntry(ref StockEngine sEngine)
        {
            // Goes through each price making sure that it can be converted to a decimal

            for (int i = 0; i < tStock.NumberOfRecords; i++)
            {
                try
                {
                    Convert.ToDecimal(tStock.GetRecordFrom(i)[2]);
                }
                catch
                {
                    frmSingleInputBox fsi = new frmSingleInputBox("Price for " + tStock.GetRecordFrom(i)[0] + " - " + tStock.GetRecordFrom(i)[1] + " is " + tStock.GetRecordFrom(i)[2] + " which is invalid. Enter the correct price:", ref sEngine);
                    fsi.Width = fsi.Width + 150;
                    fsi.ShowDialog();
                    if (fsi.Response != "$NONE")
                    {
                        tStock.EditRecordData(i, 2, fsi.Response);
                    }
                }
            }
            tStock.SaveToFile("MAINSTOC.DBF");
            System.Windows.Forms.MessageBox.Show("Finished!");
        }

        public bool AllowDatabaseSaves
        {
            get
            {
                bool bAllowed = false;
                if (tStock.PreventFromSaving)
                    bAllowed = true;
                if (tStockStats.PreventFromSaving)
                    bAllowed = true;
                return bAllowed;
            }
            set
            {
                tStock.PreventFromSaving = value;
                tStockStats.PreventFromSaving = value;
            }
        }

        public void SaveStockDatabases()
        {
            tStockStats.PreventFromSaving = false;
            tStock.PreventFromSaving = false;
            tStock.SaveToFile("MAINSTOC.DBF");
            tStockStats.SaveToFile("STOCKSTA.DBF");
        }

        public void AddSerialNumber(string sBarcode, string sSerial)
        {
            string[] sToAdd = { sBarcode, sSerial, "NO" };
            tSerials.AddRecord(sToAdd);
            tSerials.SaveToFile("SERIALS.DBF");
        }

        public void MigrateToNewSupplierDatabase()
        {
            
            frmProgressBar fp = new frmProgressBar("Migrating Database");
            
            Table tOldSupp = new Table("SUPPLIER.DBF");
            if (tOldSupp.GetRecordFrom(0).Length == 16)
            {
                return;
            }
            fp.pb.Value = 0;
            fp.pb.Maximum = tOldSupp.NumberOfRecords;
            FileStream fs = new FileStream("SUPPLIER.DBF", FileMode.OpenOrCreate);
            fs.Write(Properties.Resources.NEWSUPPL, 0, Properties.Resources.NEWSUPPL.Length);
            fs.Close();
            fp.Show();
            Table tNewSupp = new Table("SUPPLIER.DBF");
            for (int i = 0; i < tOldSupp.NumberOfRecords; i++)
            {
                fp.pb.Value = i;
                string[] sOldRec = tOldSupp.GetRecordFrom(i);
                string[] sNewRec = new string[sOldRec.Length + 1];
                for (int x = 0; x < sOldRec.Length; x++)
                {
                    sNewRec[x] = sOldRec[x];
                }
                sNewRec[sOldRec.Length] = "NO";
                tNewSupp.AddRecord(sNewRec);
            }
            tNewSupp.SaveToFile("SUPPLIER.DBF");
            fp.Close();
        }

        public void MarkSupplierAsDeleted(string sSupCode)
        {
            int nLoc = -1;
            tSupplier.SearchForRecord(sSupCode, 0, ref nLoc);
            if (nLoc != -1)
            {
                tSupplier.EditRecordData(nLoc, 15, "YES");
                tSupplier.SaveToFile("SUPPLIER.DBF");
            }
        }

        public string[] GetStoredTransactionFromTill(int nTillNum)
        {
            int nTillPos = -1;
            for (int i = 0; i < Till.Length; i++)
            {
                if (Till[i].Number == nTillNum)
                {
                    nTillPos = i;
                    break;
                }
            }
            if (nTillPos == -1)
                throw new NotSupportedException("Unknown Till Number");
            System.Windows.Forms.Form fWaiting = new System.Windows.Forms.Form();
            fWaiting.Size = new Size(200, 70);
            fWaiting.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            fWaiting.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            System.Windows.Forms.Label lblWaiting = new System.Windows.Forms.Label();
            lblWaiting.Location = new Point(10, 10);
            lblWaiting.AutoSize = true;
            lblWaiting.Font = new Font("Franklin Gothic Medium", 12.0f);
            lblWaiting.Text = "Waiting...";
            fWaiting.Controls.Add(lblWaiting);
            fWaiting.Show();
            //SendCommandToTill("DumpTransaction");
            int nOfTries = 0;
            while (!File.Exists(Till[nTillPos].FileLocation + "\\TILL\\dumped"))
            {
                System.Threading.Thread.Sleep(1000);
                nOfTries++;
                lblWaiting.Text += ".";
                fWaiting.Refresh();
                System.Windows.Forms.Application.DoEvents();
                if (nOfTries > 10)
                {
                    if (System.Windows.Forms.MessageBox.Show("Timed out. Continue waiting?", "Timed out", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                    {
                        throw new TimeoutException("Timed out whilst waiting for the till to dump the transaction");
                    }
                    else
                    {
                        nOfTries = 0;
                    }
                }
            }
            System.Threading.Thread.Sleep(7000);
            TextReader tReader = new StreamReader(Till[nTillPos].FileLocation + "\\TILL\\output.txt");
            string[] sToReturn = tReader.ReadToEnd().Split('\n');
            tReader.Close();
            File.Delete(Till[nTillPos].FileLocation + "\\TILL\\output.txt");
            File.Delete(Till[nTillPos].FileLocation + "\\TILL\\dumped");
            fWaiting.Close();
            return sToReturn;
        }


        public void CreateAnOffer(string sOfferCode, string sDesc, string sTypeSixBarcode, string sReceiptLoc)
        {
            int nRecLoc = -1;
            tOffers.SearchForRecord(sOfferCode, 0, ref nRecLoc);
            if (nRecLoc != -1)
            {
                int nPrinted = Convert.ToInt32(tOffers.GetRecordFrom(nRecLoc)[4]);
                int nReturned = Convert.ToInt32(tOffers.GetRecordFrom(nRecLoc)[5]);
                tOffers.DeleteRecord(nRecLoc);
            }
            string[] sToAdd = { sOfferCode, sDesc, sTypeSixBarcode, sReceiptLoc, "0", "0" };
            tOffers.AddRecord(sToAdd);
            tOffers.SaveToFile("OFFERS.DBF");
        }

        public string[] GetListOfOfferNumbers()
        {
            string[] sList = new string[tOffers.NumberOfRecords];
            for (int i = 0; i < sList.Length; i++)
            {
                sList[i] = tOffers.GetRecordFrom(i)[0];
            }
            return sList;
        }

        public string[] GetDetailsOfOffer(string sOfferNum)
        {
            int nRecNum = -1;
            tOffers.SearchForRecord(sOfferNum, 0, ref nRecNum);
            if (nRecNum != -1)
            {
                return tOffers.GetRecordFrom(nRecNum);
            }
            else
                return new string[1];
        }

        public void SaveOffersReceipt(string sBarcode, string sReceipt)
        {
            TextWriter tWriter = new StreamWriter("OffersReceipt\\" + sBarcode + ".txt", false);
            tWriter.Write(sReceipt);
            tWriter.Close();
        }

        public string LoadOffersReceipt(string sBarcode)
        {
            if (File.Exists("OffersReceipt\\" + sBarcode + ".txt"))
            {
                TextReader tReader = new StreamReader("OffersReceipt\\" + sBarcode + ".txt");
                string sToReturn = tReader.ReadToEnd();
                tReader.Close();
                return sToReturn;
            }
            else
            {
                return "";
            }
        }

        public void RunTillSoftware()
        {
            SendCommandToTill("TempCloseSoftware");
            while (!File.Exists(Till[0].FileLocation + "\\TILL\\done"))
            {
                System.Threading.Thread.Sleep(500);
            }
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = Till[0].FileLocation + "\\TILL\\GTill.exe";
            p.StartInfo.WorkingDirectory = Till[0].FileLocation + "\\TILL";
            p.Start();
            while (!p.HasExited)
            {
                System.Threading.Thread.Sleep(500);
            }
            TextWriter tWriter = new StreamWriter(Till[0].FileLocation + "\\TILL\\alsodone");
            tWriter.WriteLine("");
            tWriter.Close();
        }

        private void CollectOffers(string sTillLoc)
        {
            Table tTillOffers = new Table(sTillLoc + "\\TILL\\OFFERS.DBF");
            for (int i = 0; i < tTillOffers.NumberOfRecords; i++)
            {
                int nPrinted = Convert.ToInt32(tTillOffers.GetRecordFrom(i)[4]);
                int nReturned = Convert.ToInt32(tTillOffers.GetRecordFrom(i)[5]);

                int nRecLoc = -1;
                tOffers.SearchForRecord(tTillOffers.GetRecordFrom(i)[0], 0, ref nRecLoc);
                if (nRecLoc != -1)
                {
                    int nCurrentPrinted = Convert.ToInt32(tOffers.GetRecordFrom(nRecLoc)[4]);
                    int nCurrentReturned = Convert.ToInt32(tOffers.GetRecordFrom(nRecLoc)[5]);

                    tOffers.EditRecordData(nRecLoc, 4, (nCurrentPrinted + nPrinted).ToString());
                    tOffers.EditRecordData(nRecLoc, 5, (nCurrentReturned + nReturned).ToString());

                    tTillOffers.EditRecordData(i, 4, "0");
                    tTillOffers.EditRecordData(i, 5, "0");
                }
            }
            tTillOffers.SaveToFile(sTillLoc + "\\TILL\\OFFERS.DBF");
            tOffers.SaveToFile("OFFERS.DBF");
        }

        public void BuildSalesIndex()
        {
            frmProgressBar fp = new frmProgressBar("Deleting Previous Index");
            fp.Show();
            string[] sFoldersToIndex = Directory.GetDirectories("Archive\\Weekly");
            fp.pb.Maximum = sFoldersToIndex.Length;

            // Hopefully this will work
            //FileManagementEngine.UncompressDirectory(sTDir);

            FileStream s = new FileStream(sTDir + "SALESIND.DBF", FileMode.Create);
            s.Write(Properties.Resources.SALESIND, 0, Properties.Resources.SALESIND.Length);
            s.Close();

            tSalesIndex = new Table("SALESIND.DBF");
            for (int i = 0; i < sFoldersToIndex.Length; i++)
            {
                try
                {
                    // Hopefully this will work
                    FileManagementEngine.UncompressDirectory(sFoldersToIndex[i]);

                }
                catch (Ionic.Zip.BadReadException)
                {
                    // One of the compressed files is dodgy, delete it
                    Directory.Delete(sFoldersToIndex[i], true);
                }
                fp.pb.Value = i;
                fp.Text = "Indexing " + sFoldersToIndex[i].Split('\\')[sFoldersToIndex[i].Split('\\').Length - 1];
                string[] sFilesToAdd = Directory.GetFiles(sFoldersToIndex[i] + "\\TILL1\\INGNG", "TDATA*.DBF");
                for (int x = 0; x < sFilesToAdd.Length; x++)
                {
                    Table tData = new Table(sFilesToAdd[x]);
                    for (int y = 0; y < tData.NumberOfRecords; y++)
                    {
                        string sBarcode = tData.GetRecordFrom(y)[3];
                        string sTranNo = tData.GetRecordFrom(y)[0];
                        string sWeek = sFoldersToIndex[i].Split('\\')[sFoldersToIndex[i].Split('\\').Length - 1];
                        string[] sToAdd = { sTranNo, sBarcode, sWeek };
                        tSalesIndex.AddRecord(sToAdd);
                    }
                }

                FileManagementEngine.CompressArchiveDirectory(sFoldersToIndex[i]);
            }
            tSalesIndex.SaveToFile("SALESIND.DBF");
            fp.Close();
        }

        public string[] GetWeeksOfItemSold(string sBarcode, ref string[] sTranNos)
        {
            int n = 0;
            string[,] sResults = tSalesIndex.SearchAndGetAllMatchingRecords(1, sBarcode, ref n, true);
            sTranNos = new string[n];
            string[] sSaleDate = new string[n];
            for (int i = 0; i < n; i++)
            {
                sTranNos[i] = sResults[i, 0];
                sSaleDate[i] = sResults[i, 2];
            }
            return sSaleDate;

        }

        public string WorkOutDateOfSale(string sTranNum, string sFolder)
        {
            // Hopefully this will work
            try
            {
                FileManagementEngine.UncompressDirectory("Archive\\Weekly\\" + sFolder);

                string[] sTDATAFiles = Directory.GetFiles("Archive\\Weekly\\" + sFolder + "\\TILL1\\INGNG", "TDATA*.DBF");
                for (int i = 0; i < sTDATAFiles.Length; i++)
                {
                    int n = 0;
                    Table t = new Table(sTDATAFiles[i]);
                    if (t.SearchForRecord(sTranNum, 0, ref n))
                    {
                        return sTDATAFiles[i].Split('\\')[sTDATAFiles[i].Split('\\').Length - 1][5].ToString();
                    }
                }
            }
            catch
            {
                return "?";
            }
            return "ERROR";
        }

        public decimal GetSensibleDecimal(string sInput)
        {
            if (sInput == "")
                return 0;
            // If they've used the default order qty field and it's like 6PACK, try and just get the 6
            string sAcceptable = "";
            for (int i = 0; i < sInput.Length; i++)
            {
                // Go through each character and see if it can be converted into a decimal. If it can, keep it. First one that can't breaks the checker.
                try
                {
                    Convert.ToDecimal((sInput[i]).ToString());
                    sAcceptable += sInput[i].ToString();
                }
                catch
                {
                    if (sInput[i] == '.')
                        sAcceptable += ".";
                    else
                        break;
                }
            }
            decimal dQtyToOrder = Convert.ToDecimal(sAcceptable);
            return dQtyToOrder;
        }

        string[] lastMissingOrderLine = new string[0];

        public string[] FindMissingOrderLines()
        {
            List<string> missingOrderLines = new List<string>();

            int fieldNum = 0;
            for (int i = 0; i < tOrderLine.NumberOfRecords; i++)
            {
                string orderNum = tOrderLine.GetRecordFrom(i)[0];

                if (!missingOrderLines.Contains(orderNum))
                {
                    if (!tOrder.SearchForRecord(orderNum, 0, ref fieldNum))
                    {
                        missingOrderLines.Add(orderNum);
                    }
                }
            }

            if (lastMissingOrderLine.Length > 0 && lastMissingOrderLine.Length < missingOrderLines.Count)
                throw new Exception("Order numbers have just been corrupted!!!! Please do a full upload and include a description of what you were doing at the time.");

            lastMissingOrderLine = missingOrderLines.ToArray();
            return lastMissingOrderLine;
        }

        public void FixMinimumOrderQuantities()
        {
            for (int i = 0; i < tStockStats.NumberOfRecords; i++)
            {
                try
                {
                    Convert.ToDecimal(tStockStats.GetRecordFrom(i)[37]);
                }
                catch
                {
                    if (tStockStats.GetRecordFrom(i)[37] != "")
                    {
                        tStockStats.EditRecordData(i, 37, "1");
                    }
                }
            }
            tStockStats.SaveToFile("STOCKSTA.DBF");
        }


        /*public MasterItem getItemFromBarcode(string sBarcode)
        {

        }*/

        public void DisposeOfTables()
        {
            Console.WriteLine("Tables being disposed");
            tAccStat.Dispose();
            tEmails.Dispose();
            tCatGroupHeader.Dispose();
            tCatGroupData.Dispose();
            tCategory.Dispose();
            tCommissioners.Dispose();
            tCommItems.Dispose();
            tMultiHeader.Dispose();
            tMultiData.Dispose();
            tOffers.Dispose();
            tOrder.Dispose();
            tOrderLine.Dispose();
            tOrderSuggestions.Dispose();
            tSalesIndex.Dispose();
            tSerials.Dispose();
            tSettings.Dispose();
            tShop.Dispose();
            tStaff.Dispose();
            tStock.Dispose();
            tStockLength.Dispose();
            tStockStats.Dispose();
            tSupplier.Dispose();
            tSupplierIndex.Dispose();
            tTills.Dispose();
            tTotalSales.Dispose();
            tVATRates.Dispose();
        }

        public void CompareWithArchivedDay(string archiveFolder, out int newItems, out int itemsChanged)
        {
            itemsChanged = 0;
            newItems = 0;
            try
            {
                FileManagementEngine.UncompressDirectory(archiveFolder);
                Table tbStock = new Table(archiveFolder + "\\MAINSTOC.DBF");
                Table tbStockSta = new Table(archiveFolder + "\\STOCKSTA.DBF");

                newItems = this.tStockStats.NumberOfRecords - tbStockSta.NumberOfRecords;

                for (int i = 0; i < tbStock.NumberOfRecords; i++)
                {
                    int nRecLoc = -1;
                    tStock.SearchForRecord(tbStock.GetRecordFrom(i)[0], 0, ref nRecLoc);
                    if (nRecLoc == -1)
                        continue;
                    string[] oldRecord = tbStock.GetRecordFrom(i);
                    string[] newRecord = tStock.GetRecordFrom(i);
                    for (int y = 0; y < oldRecord.Length; y++)
                    {
                        if (oldRecord[y] != newRecord[y])
                            itemsChanged++;
                    }
                }
            }
            catch
            {
                // Error while trying to do this
            }

            FileManagementEngine.CompressArchiveDirectory(archiveFolder);
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            tAccStat.Dispose();
            tEmails.Dispose();
            tCatGroupHeader.Dispose();
            tCatGroupData.Dispose();
            tCategory.Dispose();
            tCommissioners.Dispose();
            tCommItems.Dispose();
            tCustomer.Dispose();
            tMultiHeader.Dispose();
            tMultiData.Dispose();
            tOffers.Dispose();
            tOrder.Dispose();
            tOrderLine.Dispose();
            tOrderSuggestions.Dispose();
            tSalesData.Dispose();
            tSalesIndex.Dispose();
            tSerials.Dispose();
            tSettings.Dispose();
            tShop.Dispose();
            tStaff.Dispose();
            tStock.Dispose();
            tStockLength.Dispose();
            tStockStats.Dispose();
            tSupplier.Dispose();
            tSupplierIndex.Dispose();
            tTills.Dispose();
            tTotalSales.Dispose();
            tVATRates.Dispose();
        }

        #endregion


    }



}
