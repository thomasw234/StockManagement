using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackOffice.Objects.Items
{
    public class DepartmentItem : MasterItem
    {
        private decimal _targetMargin;

        public DepartmentItem()
        {
            this.ItemType = 2;
        }

        public decimal TargetMargin
        {
            get
            {
                return _targetMargin;
            }
            set
            {
                _targetMargin = value;
            }
        }
    }
}
