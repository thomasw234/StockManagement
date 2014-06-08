using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmListOfCategoryGroups : ScalableForm
    {
        StockEngine sEngine;
        CListBox lbListOfCats;
        public string SelectedCategoryGroup = "$NONE";

        public frmListOfCategoryGroups(ref StockEngine se)
        {
            sEngine = se;
            this.AllowScaling = false;
            this.Text = "Select A Category Group";
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Size = new Size(220, 450);

            lbListOfCats = new CListBox();
            lbListOfCats.Location = new Point(10, 10);
            lbListOfCats.Size = new Size(this.ClientSize.Width - 20, this.ClientSize.Height - 20);
            lbListOfCats.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(lbListOfCats);
            lbListOfCats.Items.AddRange(sEngine.GetListOfCategoryGroupNames());
            lbListOfCats.KeyDown += new KeyEventHandler(lbListOfCats_KeyDown);
            lbListOfCats.SelectedIndex = 0;

        }

        void lbListOfCats_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectedCategoryGroup = lbListOfCats.Items[lbListOfCats.SelectedIndex].ToString();
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}
