using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmListOfItemTypes : ScalableForm
    {
        CListBox lbItemType;
        int CategorySelected = -1;

        public frmListOfItemTypes()
        {
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.AllowScaling = false;
            this.Text = "Item Type";
            this.Size = new Size(170, 190);

            AddMessage("INST", "Select an Item Type", new Point(10, 10));

            lbItemType = new CListBox();
            lbItemType.Location = new Point(10, BelowLastControl);
            lbItemType.Size = new Size(this.ClientSize.Width - 20, this.ClientSize.Height - 10 - lbItemType.Top);
            lbItemType.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(lbItemType);

            lbItemType.Items.Add("1. Stock Item");
            lbItemType.Items.Add("2. Department Item");
            lbItemType.Items.Add("3. Non-Stock Item");
            lbItemType.Items.Add("4. Multi-item Item");
            lbItemType.Items.Add("5. Child of Stock Item");
            lbItemType.Items.Add("6. Commission Item");

            lbItemType.KeyDown += new KeyEventHandler(lbItemType_KeyDown);
            lbItemType.SelectedIndex = 0;
        }

        void lbItemType_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CategorySelected = lbItemType.SelectedIndex + 1;
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        public int SelectedItemType
        {
            get
            {
                return CategorySelected;
            }
            set
            {
                lbItemType.SelectedIndex = value - 1;
            }
        }
    }
}
