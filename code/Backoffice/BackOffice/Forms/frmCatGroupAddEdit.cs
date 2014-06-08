using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmCatGroupAddEdit : ScalableForm
    {
        StockEngine sEngine;
        CListBox lbListOfCatGroups;
        CListBox lbListOfCatsInGroup;
        string[] sCodesOnDisplay;

        public frmCatGroupAddEdit(ref StockEngine se)
        {
            sEngine = se;

            AddMessage("INST", "Insert to add a category group, Shift & Delete to delete a category group. Enter to edit the category group with the same keys.", new Point(10, 10));

            lbListOfCatGroups = new CListBox();
            lbListOfCatGroups.Location = new Point(10, BelowLastControl);
            lbListOfCatGroups.Size = new Size(500, this.Height - 60);
            lbListOfCatGroups.SelectedIndexChanged += new EventHandler(lbListOfCatGroups_SelectedIndexChanged);
            lbListOfCatGroups.KeyDown += new KeyEventHandler(lbListOfCatGroups_KeyDown);
            this.Controls.Add(lbListOfCatGroups);

            lbListOfCatsInGroup = new CListBox();
            lbListOfCatsInGroup.Location = new Point(520, lbListOfCatGroups.Top);
            lbListOfCatsInGroup.KeyDown += new KeyEventHandler(lbListOfCatsInGroup_KeyDown);
            lbListOfCatsInGroup.Size = new Size(500, this.Height - 60);
            this.Controls.Add(lbListOfCatsInGroup);

            string[] sItems = sEngine.GetListOfCategoryGroupNames();
            lbListOfCatGroups.Items.AddRange(sItems);
            lbListOfCatGroups.SelectedIndex = 0;
            this.WindowState = FormWindowState.Maximized;

            this.Text = "Add / Edit Category Groups";
        }

        void lbListOfCatsInGroup_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && e.Shift)
            {
                for (int i = lbListOfCatsInGroup.SelectedIndex; i < sCodesOnDisplay.Length - 1; i++)
                {
                    sCodesOnDisplay[i] = sCodesOnDisplay[i + 1];
                }
                Array.Resize<string>(ref sCodesOnDisplay, sCodesOnDisplay.Length - 1);
                lbListOfCatsInGroup.Items.RemoveAt(lbListOfCatsInGroup.SelectedIndex);
                if (lbListOfCatGroups.Items.Count > 0)
                    lbListOfCatGroups.SelectedIndex = 0;
                else
                {
                    sEngine.DeleteCategoryGroup(lbListOfCatGroups.Items[lbListOfCatGroups.SelectedIndex].ToString());
                    lbListOfCatGroups.Items.RemoveAt(lbListOfCatGroups.SelectedIndex);
                    lbListOfCatGroups.SelectedIndex = 0;
                    lbListOfCatGroups.Focus();
                }
                sEngine.AddEditCategoryGroup(lbListOfCatGroups.Items[lbListOfCatGroups.SelectedIndex].ToString(), sCodesOnDisplay);
            }
            else if (e.KeyCode == Keys.Insert)
            {
                frmSingleInputBox fGetCat = new frmSingleInputBox("Enter the category code, of press F5 to choose a category. Leave blank to cancel.", ref sEngine);
                fGetCat.GettingCategory = true;
                if (fGetCat.Response != "$NONE")
                {
                    Array.Resize<string>(ref sCodesOnDisplay, sCodesOnDisplay.Length + 1);
                    sCodesOnDisplay[sCodesOnDisplay.Length - 1] = fGetCat.Response;
                    sEngine.AddEditCategoryGroup(lbListOfCatGroups.Items[lbListOfCatGroups.SelectedIndex].ToString(), sCodesOnDisplay);
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                lbListOfCatGroups.Focus();
            }
        }

        void lbListOfCatGroups_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                lbListOfCatsInGroup.Focus();
                if (lbListOfCatsInGroup.Items.Count > 0)
                    lbListOfCatsInGroup.SelectedIndex = 0;
            }
            else if (e.KeyCode == Keys.Insert)
            {
                // Add New Item
                frmSingleInputBox fGetDesc = new frmSingleInputBox("Please enter the description of the category group :", ref sEngine);
                fGetDesc.ShowDialog();
                lbListOfCatsInGroup.Items.Clear();
                string sDesc = "";
                if (fGetDesc.Response != "$NONE")
                {
                    sDesc = fGetDesc.Response;

                    frmSingleInputBox fGetCats = new frmSingleInputBox("Enter the category code, or press F5 to choose a category. Leave blank to finish.", ref sEngine);
                    fGetCats.GettingCategory = true;
                    fGetCats.ShowDialog();
                    string[] sCats = new string[0];
                    while (fGetCats.Response != "$NONE")
                    {
                        Array.Resize<string>(ref sCats, sCats.Length + 1);
                        sCats[sCats.Length - 1] = fGetCats.Response;
                        lbListOfCatsInGroup.Items.Add(sEngine.GetCategoryDesc(fGetCats.Response));
                        fGetCats.Response = "$NONE";
                        fGetCats.tbResponse.Text = "";
                        fGetCats.ShowDialog();
                    }
                    if (MessageBox.Show("Save category group?", "Save", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        sEngine.AddEditCategoryGroup(sDesc, sCats);
                        lbListOfCatGroups.Items.Add(sDesc);
                        lbListOfCatsInGroup.SelectedIndex = lbListOfCatsInGroup.Items.Count - 1;
                    }
                    else
                    {
                        lbListOfCatGroups.SelectedIndex = 0;
                    }
                }
            }
            else if (e.KeyCode == Keys.Delete && e.Shift)
            {
                sEngine.DeleteCategoryGroup(lbListOfCatGroups.Items[lbListOfCatGroups.SelectedIndex].ToString());
                lbListOfCatGroups.Items.RemoveAt(lbListOfCatGroups.SelectedIndex);
                lbListOfCatGroups.SelectedIndex = 0;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        void lbListOfCatGroups_SelectedIndexChanged(object sender, EventArgs e)
        {
            lbListOfCatsInGroup.Items.Clear();
            if (lbListOfCatGroups.SelectedIndex == -1)
                lbListOfCatGroups.SelectedIndex = 0;
            sCodesOnDisplay = sEngine.GetListOfCatGroupCategories(lbListOfCatGroups.Items[lbListOfCatGroups.SelectedIndex].ToString());
            for (int i = 0; i < sCodesOnDisplay.Length; i++)
            {
                lbListOfCatsInGroup.Items.Add(sEngine.GetCategoryDesc(sCodesOnDisplay[i]));
            }
        }


    }
}
