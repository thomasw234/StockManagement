using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackOffice.Objects.Items
{
    public class ChildItem : MasterItem
    {
        private MasterItem _parentItem;

        public ChildItem(string barcode)
        {

            this.Statistics = _parentItem.Statistics;
        }
    }
}
