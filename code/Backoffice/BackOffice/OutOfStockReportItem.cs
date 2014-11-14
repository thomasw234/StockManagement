using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice
{
    class OutOfStockReportItem : IComparable
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

                decimal dPercentage = (100 / dTotal) * dOut;
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

        public int CompareTo(object obj)
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
}
