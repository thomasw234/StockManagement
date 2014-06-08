using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackOffice.Objects
{
    public abstract class Item
    {
        private Barcode _barcode;
        private Description _description;
        private VATRate _VATRate;

        public string Barcode
        {
            get
            {
                return _barcode._Barcode;
            }
        }

        public string Description
        {
            get
            {
                return _description._Description;
            }
            set
            {
                _description._Description = value;
            }
        }

        public string VATRate
        {
            get
            {
                return _VATRate.VATRateCode;
            }
        }
    }
}
