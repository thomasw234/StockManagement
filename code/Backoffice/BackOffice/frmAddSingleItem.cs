using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmAddSingleItem : ScalableForm
    {
        StockEngine sEngine;

        public frmAddSingleItem(ref StockEngine se)
        {
            sEngine = se;

            AddInputControl("BARCODE", "Item Barcode : ", new Point(10, 10), 500, "Enter the barcode of the item");
            AddInputControl("DESCRIPTION", "Item Description : ", new Point(10, BelowLastControl), 500, "Enter the description of the item");
            AddInputControl("RRP", "Price : ", new Point(10, BelowLastControl), 500, "Enter the selling price of the item");
        }
    }
}
