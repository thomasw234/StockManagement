using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice
{
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
}
