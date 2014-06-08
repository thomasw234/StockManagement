using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackOffice.Objects.Items
{
    class NonStockItem : MasterItem
    {
        public NonStockItem()
        {
            this.ItemType = 3;
        }
    }
}
