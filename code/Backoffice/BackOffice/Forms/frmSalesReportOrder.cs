using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Text;
using System.Drawing;

namespace BackOffice
{
    class frmSalesReportOrder : ScalableForm
    {
        CListBox lbOptions;
        public ReportOrderedBy SelectedOrder;
        public bool OptionSelected = false;

        public frmSalesReportOrder()
        {
            this.AllowScaling = false;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Size = new Size(220, 180);
            this.Text = "Sort Report By...";
            lbOptions = new CListBox();
            lbOptions.Items.Add("Description Alphabetically");
            lbOptions.Items.Add("Quantity Sold Descending");
            lbOptions.Items.Add("Barcode Alphabetically");
            lbOptions.Items.Add("Gross Sales Descending");
            lbOptions.Items.Add("Net Sales Descending");
            lbOptions.Items.Add("Profit Descending");
            lbOptions.Items.Add("Profit Percent Descending");
            this.Controls.Add(lbOptions);
            lbOptions.Location = new Point(10, 10);
            lbOptions.Size = new Size(this.ClientSize.Width - 20, this.ClientSize.Height - 20);
            lbOptions.BorderStyle = BorderStyle.FixedSingle;
            lbOptions.KeyDown += new KeyEventHandler(lbOptions_KeyDown);
            lbOptions.SelectedIndex = 0;
        }

        void lbOptions_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                switch (lbOptions.SelectedIndex)
                {
                    case 0:
                        SelectedOrder = ReportOrderedBy.DescAlphabetical;
                        break;
                    case 1:
                        SelectedOrder = ReportOrderedBy.QuantitySold;
                        break;
                    case 2:
                        SelectedOrder = ReportOrderedBy.CodeAlphabetical;
                        break;
                    case 3:
                        SelectedOrder = ReportOrderedBy.GrossSales;
                        break;
                    case 4:
                        SelectedOrder = ReportOrderedBy.NetSales;
                        break;
                    case 5:
                        SelectedOrder = ReportOrderedBy.Profit;
                        break;
                    case 6:
                        SelectedOrder = ReportOrderedBy.ProfitPercent;
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
