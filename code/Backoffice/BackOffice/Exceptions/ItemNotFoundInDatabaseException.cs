using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Exceptions
{
    public class ItemNotFoundInDatabaseException : Exception
    {
        public ItemNotFoundInDatabaseException(string Barcode)
            : base(
                "A lookup was tried on an item with barcode " + Barcode + " but it could not be found in the database")

        {

        }
    }
}
