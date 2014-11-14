using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Text;
using System.Drawing;

namespace BackOffice
{
    class frmSalesReportType : ScalableForm
    {
        CListBox lbOptions;
        public SalesReportType sType;
        public bool OptionSelected = false;

        public frmSalesReportType()
        {
            this.AllowScaling = false;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Size = new Size(240, 90);
            this.Text = "Show on the report...";
            lbOptions = new CListBox();
            lbOptions.Items.Add("All Sold Stock & Category Totals");
            lbOptions.Items.Add("Category Totals Only");
            this.Controls.Add(lbOptions);
            lbOptions.Location = new Point(10, 10);
            lbOptions.Size = new Size(this.ClientSize.Width - 10 - lbOptions.Left, this.ClientSize.Height - 20);
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
                        sType = SalesReportType.AllStock;
                        break;
                    case 1:
                        sType = SalesReportType.CatTotalsAllShops;
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
