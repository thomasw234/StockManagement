using System;
using System.Collections.Generic;
using System.Text;
using DBFDetailsViewerV2;
using System.IO;

namespace BackOffice
{
    class Till
    {
        string sCode;
        string sTillName;
        string[] sReceiptLines;

        public Till(string[] sRecordData)
        {
            sCode = sRecordData[0];
            sTillName = sRecordData[1];
            sReceiptLines = new string[3];
            for (int i = 0; i < 3; i++)
            {
                sReceiptLines[i] = sRecordData[i + 2];
            }
        }

    }

    class BackEngine
    {
        Table tOrder;
        Table tOrderLines;
        Table tSettings;
        Table tStaff;
        Table tStock;
        Table tStockStats;
        Table tSupplier;
        Table tTills;

        Till[] tlTills;

        public BackEngine()
        {
            ;
        }

        public void LoadTable(string sTableToLoad)
        {
            switch (sTableToLoad)
            {
                case "ORDER":
                    tOrder = new Table("ORDER.DBF");
                    break;
                case "ORDERLIN":
                    tOrderLines = new Table("ORDERLIN.DBF");
                    break;
                case "STAFF":
                    tStaff = new Table("STAFF.DBF");
                    break;
                case "STOCK":
                    tStock = new Table("STOCK.DBF");
                    break;
                case "STOCKSTA":
                    tStockStats = new Table("DAILY.DBF");
                    break;
                case "SUPPLIER.DBF":
                    tSupplier = new Table("CATEGORY.DBF");
                    break;
                case "SETTINGS":
                    tSettings = new Table("SETTINGS.DBF");
                    break;
            }
        }

        public string CompanyName
        {
            get
            {
                return tDetails.GetRecordFrom(0)[0];
            }
            set
            {
                tDetails.EditRecordData(0, 0, value);
            }
        }

        public string[] TillCodes
        {
            get
            {
                string[] sToReturn = new string[tTillLoc.NumberOfRecords];
                for (int i = 0; i < sToReturn.Length; i++)
                {
                    sToReturn[i] = tTillLoc.GetRecordFrom(i)[0];
                }
                return sToReturn;
            }
        }

        public void CollectDailySalesData(string sDateToCollect)
        {
            // Get Files from the tills
            foreach (Till till in tlTills)
            {
                till.CollectTillData(sDateToCollect);
            }
        }

        public static int DayNumber(string sDateToday)
        {
            string[] sDate = sDateToday.Split('/');
            DateTime dtCashup = new DateTime(Convert.ToInt32("20" + sDate[2]), Convert.ToInt32(sDate[1]), Convert.ToInt32(sDate[0]));
            switch (dtCashup.DayOfWeek.ToString().ToUpper())
            {
                case "SUNDAY":
                    return 1;
                case "MONDAY":
                    return 2;
                case "TUESDAY":
                    return 3;
                case "WEDNESDAY":
                    return 4;
                case "THURSDAY":
                    return 5;
                case "FRIDAY":
                    return 6;
                case "SATURDAY":
                    return 7;
            }
            return 0;
        }

        public static string RealTillLoc(string sTillLocLoc)
        {
            Table tOverRide = new Table("LOCOVERR.DBF");
            if (tOverRide.SearchForRecord(sTillLocLoc, "TILLLOCLOC"))
            {
                return tOverRide.GetRecordFrom(sTillLocLoc, 0)[1];
            }
            else
            {
                return sTillLocLoc;
            }
        }

        public static string RealStoreLoc(string sTillLocLoc)
        {
            Table tOverRide = new Table("LOCOVERR.DBF");
            if (tOverRide.SearchForRecord(sTillLocLoc, "TILLLOCLOC"))
            {
                return tOverRide.GetRecordFrom(sTillLocLoc, 0)[2];
            }
            else
            {
                return sTillLocLoc;
            }
        }
    }
}
