using System;
using System.Collections.Generic;
using System.Text;

namespace BackOffice.Objects
{
    public class VATRate
    {
        private string vatRateCode = String.Empty;

        /// <summary>
        /// Sets up a new instance of VAT
        /// </summary>
        /// <param name="vatRateCode">The code of the VAT Rate</param>
        /// <param name="vatRate">The Rate of VAT</param>
        public VATRate(string vatRateCode)
        {
            this.vatRateCode = vatRateCode;
        }

        /// <summary>
        /// Accessor for the VAT Rate
        /// </summary>
        public string VATRateCode
        {
            get
            {
                return vatRateCode;
            }
            set
            {
                vatRateCode = value;
            }
        }
    }
}
