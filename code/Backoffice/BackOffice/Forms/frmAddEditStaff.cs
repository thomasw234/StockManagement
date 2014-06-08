using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmAddEditStaff : ScalableForm
    {
        StockEngine sEngine;
        CListBox lbStaff;
        CListBox lbNumbers;
        string sShopCode;

        public frmAddEditStaff(ref StockEngine se)
        {
            sEngine = se;

            this.AllowScaling = false;
            this.Size = new Size(300, 360);

            frmListOfShops flos = new frmListOfShops(ref sEngine);
            flos.ShowDialog();
            if (flos.SelectedShopCode != "$NONE")
            {
                sShopCode = flos.SelectedShopCode;
            }

            AddMessage("NUM", "ID Number", new Point(10, 10));
            lbNumbers = new CListBox();
            lbNumbers.Location = new Point(10, 40);
            lbNumbers.Size = new Size(75, this.ClientSize.Height - 10 - lbNumbers.Top);
            lbNumbers.BorderStyle = BorderStyle.None;
            lbNumbers.KeyDown += new KeyEventHandler(lbKeyDown);
            lbNumbers.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
            this.Controls.Add(lbNumbers);

            AddMessage("NAME", "Staff Name", new Point(85, 10));
            lbStaff = new CListBox();
            lbStaff.Location = new Point(85, 40);
            lbStaff.Size = new Size(200, this.ClientSize.Height - 10 - lbNumbers.Top);
            lbStaff.BorderStyle = BorderStyle.None;
            lbStaff.KeyDown += new KeyEventHandler(lbKeyDown);
            lbStaff.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
            this.Controls.Add(lbStaff);

            this.VisibleChanged += new EventHandler(frmAddEditStaff_VisibleChanged);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Text = "Edit Staff";

        }

        void frmAddEditStaff_VisibleChanged(object sender, EventArgs e)
        {
            lbNumbers.Items.Clear();
            lbStaff.Items.Clear();
            if (sShopCode != "$NONE")
            {
                string[] sStaff = sEngine.GetListOfStaffMembers(sShopCode);
                for (int i = 0; i < sStaff.Length; i++)
                {
                    lbNumbers.Items.Add((i + 1).ToString());
                    lbStaff.Items.Add(sStaff[i]);
                }
            }
        }

        void lbSelectedChanged(object sender, EventArgs e)
        {
            lbStaff.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbNumbers.SelectedIndex = ((ListBox)sender).SelectedIndex;
        }

        void lbKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                frmSingleInputBox fsiGetName = new frmSingleInputBox("Enter the name of staff member " + lbNumbers.Items[((ListBox)sender).SelectedIndex].ToString(), ref sEngine);
                fsiGetName.tbResponse.Text = lbStaff.Items[((ListBox)sender).SelectedIndex].ToString();
                fsiGetName.ShowDialog();
                if (fsiGetName.Response != "$NONE")
                {
                    lbStaff.Items[((ListBox)sender).SelectedIndex] = fsiGetName.Response;
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                if (MessageBox.Show("Would you like to save the changes?", "Save Changes?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    string[] sToAdd = new string[lbStaff.Items.Count];
                    for (int i = 0; i < sToAdd.Length; i++)
                    {
                        sToAdd[i] = lbStaff.Items[i].ToString();
                    }
                    sEngine.SaveListOfStaffMembers(sToAdd, sShopCode);
                    if (MessageBox.Show("Upload changes to all tills now?", "Upload?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        sEngine.CopyWaitingFilesToTills();
                    }
                    this.Close();
                }
                else
                    this.Close();
            }
        }
    }
}
