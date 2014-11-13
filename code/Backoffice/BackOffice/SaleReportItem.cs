using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackOffice.Database_Engine;

namespace BackOffice
{
    class SaleReportItem : IComparable
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
}
