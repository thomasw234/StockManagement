using System;
using System.Collections.Generic;
using BackOffice.Database_Engine;
using System.Text;
using System.IO;

namespace BackOffice
{
    /// <summary>
    /// A static class that generates reports and dumps them in REPORT.TXT
    /// </summary>
    class ReportEngine
    {
        /// <summary>
        /// An object that can be sorted for Out of stock length reports
        /// </summary>
        public class OOSReportItem : IComparable
        {
            public string sBarcode;
            public string sDescription;
            public decimal dQIS;
            public decimal dOOSPercentage;
            public enum OOSOrder { Barcode, Description, QIS, Percentage };
            public OOSOrder order;

            public OOSReportItem(string sBarcode, string sDescription, decimal dQIS, decimal dOOSPercentage, OOSOrder order)
            {
                this.sBarcode = sBarcode;
                this.sDescription = sDescription;
                this.dQIS = dQIS;
                this.dOOSPercentage = dOOSPercentage;
                this.order = order;
            }
        
            public int  CompareTo(object obj)
            {
                switch (order)
                {
                    case OOSOrder.Barcode:
                        return String.Compare(((OOSReportItem)obj).sBarcode, this.sBarcode);
                        break;
                    case OOSOrder.Description:
                        return String.Compare(((OOSReportItem)obj).sDescription, this.sDescription);
                        break;
                    case OOSOrder.Percentage:
                        return Decimal.Compare(((OOSReportItem)obj).dOOSPercentage, this.dOOSPercentage);
                        break;
                    case OOSOrder.QIS:
                        return Decimal.Compare(((OOSReportItem)obj).dQIS, this.dQIS);
                        break;
                }
                return 0;
            }

        }

        public static void OutOfStockLengthReport(Table tStockStats, Table tMainStock, Table tStockLength,
                                                    string[] sBarcodes, string sShopCode, OOSReportItem.OOSOrder order)
        {
            // First, prepare the data for the report

            List<OOSReportItem> itemList = new List<OOSReportItem>();

            for (int i = 0; i < sBarcodes.Length; i++)
            {
                string[] sMainStock = tMainStock.GetRecordFrom(sBarcodes[i], 0, true);
                int nStockStaLoc = tStockStats.GetRecordNumberFromTwoFields(sBarcodes[i], 0, sShopCode, 35);
                string[] sStockStats = tStockStats.GetRecordFrom(nStockStaLoc);
                nStockStaLoc = tStockLength.GetRecordNumberFromTwoFields(sBarcodes[i], 0, sShopCode, 1);

                // If it's not a type 1 item, or it hasn't been added to stocklength, then it won't be included in the report
                if (nStockStaLoc == -1)
                    continue;

                string[] sStockLength = tStockLength.GetRecordFrom(nStockStaLoc);

                // Work out the percentage that it is out of stock
                decimal dTotalDays = Convert.ToDecimal(sStockLength[2]);
                decimal dStockOut = Convert.ToDecimal(sStockLength[3]);
                decimal dPercentage = (100 / dTotalDays) * dStockOut;

                itemList.Add(new OOSReportItem(sBarcodes[i], sMainStock[1], Convert.ToDecimal(sStockStats[36]), dPercentage, order));
            }

            itemList.Sort();

            // Time to create the report, now that the data has been prepared

            TextWriter tWriter = new StreamWriter("REPORT.TXT", false);

            tWriter.WriteLine("-------------------");
            tWriter.WriteLine("Out Of Stock Levels");
            tWriter.WriteLine("-------------------");

            tWriter.WriteLine("------------------------------------------------------------");
            tWriter.WriteLine("Barcode       Description                    Q.I.S  Time OOS");
            tWriter.WriteLine("------------------------------------------------------------");

            for (int i = 0; i < itemList.Count; i++)
            {
                string sOutput = itemList[i].sBarcode;
                while (sOutput.Length < 14)
                    sOutput += " ";

                sOutput += itemList[i].sDescription;
                while (sOutput.Length < 45)
                    sOutput += " ";

                while (sOutput.Length + System.Windows.Forms.WormaldForms.ScalableForm.FormatMoneyForDisplay(itemList[i].dQIS).Length < 51)
                    sOutput += " ";
                sOutput += System.Windows.Forms.WormaldForms.ScalableForm.FormatMoneyForDisplay(itemList[i].dQIS);

                while (sOutput.Length + System.Windows.Forms.WormaldForms.ScalableForm.FormatMoneyForDisplay(itemList[i].dOOSPercentage).Length < 60)
                    sOutput += " ";
                sOutput += System.Windows.Forms.WormaldForms.ScalableForm.FormatMoneyForDisplay(itemList[i].dOOSPercentage);

                tWriter.WriteLine(sOutput);
            }

            tWriter.Close();
        }

    }
}
