using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice
{
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
}
