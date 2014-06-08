using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Text;

namespace BackOffice
{
    class frmListOfShops : ScalableForm
    {
        StockEngine sEngine;
        CListBox lbShopName;
        string[] sListOfCodes;
        public string SelectedShopCode = "$NONE";

        public frmListOfShops(ref StockEngine se)
        {
            sEngine = se;
            SetupForm();
        }

        void SetupForm()
        {
            AddMessage("SHOP", "Select the shop : ", new System.Drawing.Point(10, 10));
            this.AllowScaling = false;
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.Size = new System.Drawing.Size(340, 75);
            lbShopName = new CListBox();
            lbShopName.Location = new System.Drawing.Point(10, 40);
            lbShopName.Size = new System.Drawing.Size(300, 0);
            this.Controls.Add(lbShopName);
            sListOfCodes = sEngine.GetListOfShopCodes();
            for (int i = 0; i < sEngine.NumberOfShops; i++)
            {
                lbShopName.Items.Add(sEngine.GetShopNameFromCode(sListOfCodes[i]));
            }
            lbShopName.Height = lbShopName.ItemHeight * (sEngine.NumberOfShops + 1);
            this.Height += (lbShopName.ItemHeight * (sEngine.NumberOfShops + 1));
            lbShopName.SelectedIndex = 0;
            lbShopName.KeyDown += new KeyEventHandler(lbShopName_KeyDown);
            this.Shown += new EventHandler(frmListOfShops_Shown);
            this.Text = "Select A Shop";
        }

        void frmListOfShops_Shown(object sender, EventArgs e)
        {
            if (sListOfCodes.Length == 1)
            {
                SelectedShopCode = sListOfCodes[0];
                this.Close();
            }
        }

        void lbShopName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectedShopCode = sListOfCodes[lbShopName.SelectedIndex];
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
                this.Close();
        }
    }
}
