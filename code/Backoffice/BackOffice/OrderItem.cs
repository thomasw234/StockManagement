using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice
{
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
}
