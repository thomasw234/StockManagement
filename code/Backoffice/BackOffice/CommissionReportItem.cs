using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice
{
    class CommissionReportItem : IComparable
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
}
