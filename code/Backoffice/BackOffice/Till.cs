using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackOffice.Database_Engine;

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
            File.Copy(FileLocation + "\\OUTGNG\\REPDATA" + nDay.ToString() + ".DBF", "TILL" + Number.ToString() + "\\INGNG\\" + "REPDATA" + nDay.ToString() + ".DBF", true);
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
}
