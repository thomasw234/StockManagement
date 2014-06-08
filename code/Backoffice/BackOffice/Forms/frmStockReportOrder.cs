using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Text;
using System.Drawing;

namespace BackOffice
{
    class frmStockReportOrder : ScalableForm
    {
        CListBox lbOptions;
        public StockEngine.ReportOrderedBy SelectedOrder;
        public bool OptionSelected = false;


        public frmStockReportOrder()
        {
            this.AllowScaling = false;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Size = new Size(220, 105);
            this.Text = "Report Sort Order";
            lbOptions = new CListBox();
            lbOptions.Items.Add("Barcode Alphabetically");
            lbOptions.Items.Add("Description Alphabetically");
            lbOptions.Items.Add("Quantity In stock Descending");
            this.Controls.Add(lbOptions);
            lbOptions.Location = new Point(10, 10);
            lbOptions.Size = new Size(this.ClientSize.Width - this.lbOptions.Left - 10 , this.ClientSize.Height - this.lbOptions.Top - 10);
            lbOptions.BorderStyle = BorderStyle.None;
            lbOptions.KeyDown += new KeyEventHandler(lbOptions_KeyDown);
            lbOptions.SelectedIndex = 1;
            this.SurroundListBoxes = true;
        }

        void lbOptions_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                switch (lbOptions.SelectedIndex)
                {
                    case 0:
                        SelectedOrder = StockEngine.ReportOrderedBy.CodeAlphabetical;
                        break;
                    case 1:
                        SelectedOrder = StockEngine.ReportOrderedBy.DescAlphabetical;
                        break;
                    case 2:
                        SelectedOrder = StockEngine.ReportOrderedBy.QIS;
                        break;
                }
                OptionSelected = true;
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}
