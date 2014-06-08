using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmSuppliersForItem : ScalableForm
    {
        StockEngine sEngine;
        CListBox lbSupName;
        CListBox lbSupCost;
        CListBox lbSupCode;
        public frmSuppliersForItem(ref StockEngine se, string Barcode)
        {
            sEngine = se;
            this.AllowScaling = false;
            this.Size = new Size(400, 240);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Text = "Suppliers for " + sEngine.GetMainStockInfo(Barcode)[1];
            this.SurroundListBoxes = true;

            int nOfResults = 0;
            string[,] sSuppliers = sEngine.GetListOfSuppliersForItem(Barcode, ref nOfResults);

            AddMessage("NAME", "Supplier Name", new Point(10, 10));
            lbSupName = new CListBox();
            lbSupName.Location = new Point(10, 35);
            lbSupName.Size = new Size(200, this.ClientSize.Height - lbSupName.Top - 10);
            lbSupName.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
            lbSupName.BorderStyle = BorderStyle.None;
            lbSupName.KeyDown += new KeyEventHandler(lbKeyDown);
            this.Controls.Add(lbSupName);

            AddMessage("COST", "Last Cost", new Point(210, 10));
            lbSupCost = new CListBox();
            lbSupCost.Location = new Point(210, 35);
            lbSupCost.Size = new Size(75, this.ClientSize.Height - lbSupName.Top - 10);
            lbSupCost.SelectedIndexChanged +=new EventHandler(lbSelectedChanged);
            lbSupCost.RightToLeft = RightToLeft.Yes;
            lbSupCost.BorderStyle = BorderStyle.None;
            lbSupCost.KeyDown +=new KeyEventHandler(lbKeyDown);
            this.Controls.Add(lbSupCost);

            AddMessage("CODE", "Supplier Code", new Point(285, 10));
            lbSupCode = new CListBox();
            lbSupCode.Location = new Point(285, 35);
            lbSupCode.Size = new Size(this.ClientSize.Width - lbSupCode.Left - 10, this.ClientSize.Height - lbSupName.Top - 10);
            lbSupCode.SelectedIndexChanged +=new EventHandler(lbSelectedChanged);
            lbSupCode.BorderStyle = BorderStyle.None;
            lbSupCode.KeyDown +=new KeyEventHandler(lbKeyDown);
            this.Controls.Add(lbSupCode);

            for (int i = 0; i < nOfResults; i++)
            {
                lbSupName.Items.Add(sEngine.GetSupplierDetails(sSuppliers[i, 1])[1]);
                lbSupCost.Items.Add(FormatMoneyForDisplay(sSuppliers[i, 3]));
                lbSupCode.Items.Add(sSuppliers[i, 2]);
            }

            if (nOfResults > 0)
                lbSupName.SelectedIndex = 0;
        }

        void lbKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Enter)
            {
                this.Close();
            }
        }

        void lbSelectedChanged(object sender, EventArgs e)
        {
            lbSupName.SelectedIndex = ((CListBox)sender).SelectedIndex;
            lbSupCost.SelectedIndex = ((CListBox)sender).SelectedIndex;
            lbSupCode.SelectedIndex = ((CListBox)sender).SelectedIndex;
        }
    }
}
