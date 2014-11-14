using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Text;

namespace BackOffice.Forms
{
    class frmOutOfStockReportOrder : ScalableForm
    {
        public SortOrder sortOrder;
        public bool chosen = false;

        public frmOutOfStockReportOrder()
        {
            setupForm();
        }

        private void setupForm()
        {
            this.AllowScaling = false;

            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Size = new System.Drawing.Size(260, 125);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Choose Report Ordering:";

            ListBox lbChoices = new ListBox();
            lbChoices.Location = new System.Drawing.Point(10, 10);
            lbChoices.Size = new System.Drawing.Size(215, 80);
            lbChoices.Items.Add("Barcode");
            lbChoices.Items.Add("Average Sales");
            lbChoices.Items.Add("Out of Stock Time (as a Percentage)");
            lbChoices.Items.Add("Quantity In Stock");

            this.Controls.Add(lbChoices);

            lbChoices.SelectedIndex = 0;
            lbChoices.KeyDown += new KeyEventHandler(lbChoices_KeyDown);
            lbChoices.MouseClick += new MouseEventHandler(lbChoices_MouseClick);
            lbChoices.BorderStyle = BorderStyle.FixedSingle;
            lbChoices.Size = new System.Drawing.Size(this.ClientSize.Width - 20, this.ClientSize.Height - 20);
        }

        void lbChoices_MouseClick(object sender, MouseEventArgs e)
        {
            setReturnBasedOnSelectedIndex(sender);
            this.Close();
        }

        private void setReturnBasedOnSelectedIndex(Object sender)
        {
            if (((ListBox)sender).SelectedIndex == 0)
            {
                this.sortOrder = SortOrder.Barcode;
            }
            else if (((ListBox)sender).SelectedIndex == 1)
            {
                this.sortOrder = SortOrder.AvgSales;
            }
            else if (((ListBox)sender).SelectedIndex == 2)
            {
                this.sortOrder = SortOrder.OutOfStock;
            }
            else
            {
                this.sortOrder = SortOrder.QIS;
            }
        }

        void lbChoices_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                chosen = true;
                setReturnBasedOnSelectedIndex(sender);
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }


    }
}
