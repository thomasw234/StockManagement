using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackOffice.Objects
{
    public class Barcode
    {
        // Internal storage of the item's barcode
        private string _itemBarcode = String.Empty;

        /// <summary>
        /// Initialises a new barcode
        /// </summary>
        /// <param name="barcode">The barcode to use</param>
        public Barcode(string barcode)
        {
            // Check that the barcode is a valid length
            if (barcode.Length > 12 || barcode.Length < 1)
            {
                throw new ArgumentException("Barcode " + barcode + " is an invalid length. Must be between 1 and 12 characters");
            }

            _itemBarcode = barcode.ToUpper();
        }

        /// <summary>
        /// Returns the barcode
        /// </summary>
        public string _Barcode
        {
            get
            {
                return _itemBarcode;
            }
        }

        public string ToString()
        {
            return _itemBarcode;
        }
    }
}
