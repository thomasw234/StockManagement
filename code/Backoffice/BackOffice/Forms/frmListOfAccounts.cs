using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Forms.WormaldForms;
using System.Text;

namespace BackOffice
{
    class frmListOfAccounts : ScalableForm
    {
        StockEngine sEngine;
        CListBox lbAccCode;
        CListBox lbAccName;
        public string AccountCode = "$NONE";

        public frmListOfAccounts(ref StockEngine se)
        {
            this.AllowScaling = false;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            sEngine = se;

            AddMessage("CODE", "Code", new Point(10, 10));
            AddMessage("NAME", "Name", new Point(100, 10));

            lbAccCode = new CListBox();
            lbAccCode.Location = new Point(10, BelowLastControl);
            lbAccCode.Size = new Size(90, 400);
            lbAccCode.BorderStyle = BorderStyle.None;
            lbAccCode.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
            lbAccCode.KeyDown += new KeyEventHandler(lbKeyDown);
            this.Controls.Add(lbAccCode);

            lbAccName = new CListBox();
            lbAccName.Location = new Point(100, lbAccCode.Top);
            lbAccName.Size = new Size(300, 400);
            lbAccName.BorderStyle = BorderStyle.None;
            lbAccName.SelectedIndexChanged +=new EventHandler(lbSelectedChanged);
            lbAccName.KeyDown +=new KeyEventHandler(lbKeyDown);
            this.Controls.Add(lbAccName);

            this.Size = new Size(450, 500);

            string[] sAccounts = sEngine.GetListOfAccountCodes();
            for (int i = 0; i < sAccounts.Length; i++)
            {
                lbAccCode.Items.Add(sAccounts[i]);
                lbAccName.Items.Add(sEngine.GetAccountName(sAccounts[i]));
            }
            if (lbAccCode.Items.Count > 0)
                lbAccCode.SelectedIndex = 0;

            this.Text = "Select An Account";
        }

        void lbKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                AccountCode = lbAccCode.Items[((ListBox)sender).SelectedIndex].ToString();
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        void lbSelectedChanged(object sender, EventArgs e)
        {
            lbAccCode.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbAccName.SelectedIndex = ((ListBox)sender).SelectedIndex;
        }
    }
}
