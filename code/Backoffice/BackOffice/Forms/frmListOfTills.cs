using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmListOfTills : ScalableForm
    {

        StockEngine sEngine;
        CListBox lbCode;
        CListBox lbName;
        public string sSelectedTillCode = "NULL";

        public frmListOfTills(ref StockEngine se, string sShopCode)
        {
            sEngine = se;
            AddMessage("CODE", "Till Code", new Point(10, 10));
            AddMessage("NAME", "Till Name", new Point(100, 10));
            this.AllowScaling = false;
            this.Size = new Size(280, 100);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Text = "Select A Till";

            lbCode = new CListBox();
            lbCode.Location = new Point(10, BelowLastControl);
            lbCode.Size = new Size(90, lbCode.ItemHeight);
            lbCode.BorderStyle = BorderStyle.None;
            this.Controls.Add(lbCode);

            lbName = new CListBox();
            lbName.Location = new Point(100, lbCode.Top);
            lbName.BorderStyle = BorderStyle.None;
            lbName.Size = new Size(150, lbName.ItemHeight);
            this.Controls.Add(lbName);

            string[] sTillCodes = sEngine.GetListOfTillCodes(sShopCode);
            for (int i = 0; i < sEngine.NumberOfTills(sShopCode); i++)
            {
                string[] sTillData = sEngine.GetTillData(sTillCodes[i]);
                lbCode.Items.Add(sTillData[0]);
                lbName.Items.Add(sTillData[1]);
                lbCode.Height += lbCode.ItemHeight;
                lbName.Height += lbName.ItemHeight;
                this.Height += lbName.ItemHeight;
            }

            lbName.Focus();
            lbName.SelectedIndexChanged += new EventHandler(lbName_SelectedIndexChanged);
            lbName.KeyDown += new KeyEventHandler(lbName_KeyDown);
            lbCode.KeyDown +=new KeyEventHandler(lbName_KeyDown);
            lbCode.SelectedIndexChanged += new EventHandler(lbCode_SelectedIndexChanged);

            if (lbName.Items.Count >= 1)
                lbName.SelectedIndex = 0;

            this.Text = "Select A Till";
        }

        void lbCode_SelectedIndexChanged(object sender, EventArgs e)
        {
            lbName.SelectedIndex = lbCode.SelectedIndex;
        }

        void lbName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                sSelectedTillCode = lbCode.Items[lbCode.SelectedIndex].ToString();
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        void lbName_SelectedIndexChanged(object sender, EventArgs e)
        {
            lbCode.SelectedIndex = lbName.SelectedIndex;
        }
    }
}
