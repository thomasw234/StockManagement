using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackOffice.Objects.Items
{
    public class StockItem : MasterItem
    {
        private decimal _stockLevel;

        public StockItem()
        {
            this.ItemType = 1;
        }

        public decimal StockLevel
        {
            get
            {
                return _stockLevel;
            }
            set
            {
                _stockLevel = value;
            }
        }
    }
}
