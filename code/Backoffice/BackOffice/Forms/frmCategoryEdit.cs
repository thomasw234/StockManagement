using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmCategoryEdit : ScalableForm
    {
        StockEngine sEngine;
        CListBox lbListOfCategories;
        CListBox lbListOfCodes;

        public frmCategoryEdit(ref StockEngine se)
        {
            sEngine = se;

            AllowScaling = false;

            lbListOfCodes = new CListBox();
            lbListOfCodes.Location = new Point(10, 10);
            lbListOfCodes.BorderStyle = BorderStyle.None;
            lbListOfCodes.Size = new Size((this.Width / 3) - 30, this.Height - 200);
            this.Controls.Add(lbListOfCodes);

            lbListOfCategories = new CListBox();
            lbListOfCategories.Location = new Point(lbListOfCodes.Left + lbListOfCodes.Width, 10);
            lbListOfCategories.BorderStyle = BorderStyle.None;
            lbListOfCategories.Size = new Size((this.Width / 2) - 30, this.Height - 200);
            this.Controls.Add(lbListOfCategories);

            AddMessage("INST1", "Insert key to add a category, Enter to edit the selected one, Esc to exit.", new Point(10, BelowLastControl));

            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(815, 640);

            ListAllCategories();
            lbListOfCodes.Focus();
            lbListOfCodes.KeyDown += new KeyEventHandler(lbListOfCodes_KeyDown);
            lbListOfCodes.SelectedIndexChanged += new EventHandler(lbListOfCodes_SelectedIndexChanged);
            lbListOfCodes.SelectedIndex = 0;

            this.Text = "Category Edit";
        }

        void lbListOfCodes_SelectedIndexChanged(object sender, EventArgs e)
        {
            lbListOfCategories.SelectedIndex = lbListOfCodes.SelectedIndex;
        }

        void lbListOfCodes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                frmSingleInputBox fGetCode = new frmSingleInputBox("Please enter the code", ref sEngine);
                string sOriginalCode = lbListOfCodes.Items[lbListOfCodes.SelectedIndex].ToString();
                string sNewCode = "";
                string sDesc = "";
                fGetCode.tbResponse.Text = lbListOfCodes.Items[lbListOfCodes.SelectedIndex].ToString();
                fGetCode.ShowDialog();
                if (fGetCode.Response != "$NONE")
                {
                    sNewCode = fGetCode.Response;
                    frmSingleInputBox fGetDesc = new frmSingleInputBox("Please enter the description", ref sEngine);
                    fGetDesc.tbResponse.Text = lbListOfCategories.Items[lbListOfCodes.SelectedIndex].ToString().TrimStart(' ');
                    fGetDesc.ShowDialog();
                    if (fGetDesc.Response != "$NONE")
                    {
                        sDesc = fGetDesc.Response;
                        sEngine.EditCategoryDetails(sOriginalCode, sNewCode, sDesc);
                        ListAllCategories();
                        lbListOfCodes.SelectedIndex = 0;
                        for (int i = 0; i < lbListOfCodes.Items.Count; i++)
                        {
                            if (lbListOfCodes.Items[i].ToString() == fGetCode.Response)
                            {
                                lbListOfCodes.SelectedIndex = i;
                            }
                        }
                    }
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
            else if (e.KeyCode == Keys.Insert)
            {
                frmSingleInputBox fGetCode = new frmSingleInputBox("Enter the code for the new category:", ref sEngine);
                fGetCode.ShowDialog();
                if (fGetCode.Response != "$NONE")
                {
                    frmSingleInputBox fGetDesc = new frmSingleInputBox("Enter the description for the new category:", ref sEngine);
                    fGetDesc.ShowDialog();
                    if (fGetDesc.Response != "$NONE")
                    {
                        sEngine.AddCategory(fGetCode.Response, fGetDesc.Response);
                        ListAllCategories();
                        lbListOfCodes.SelectedIndex = 0;

                        for (int i = 0; i < lbListOfCodes.Items.Count; i++)
                        {
                            if (lbListOfCodes.Items[i].ToString() == fGetCode.Response)
                            {
                                lbListOfCodes.SelectedIndex = i;
                            }
                        }
                    }
                }
            }
        }

        void ListAllCategories()
        {
            lbListOfCategories.Items.Clear();
            lbListOfCodes.Items.Clear();

            string[] sItems = sEngine.GetAllCategoryCodes();
            Array.Sort<string>(sItems);
            for (int i = 0; i < sItems.Length; i++)
            {
                lbListOfCodes.Items.Add(sItems[i]);
                string sDesc = sEngine.GetCategoryDesc(sItems[i]);
                for (int z = 2; z < sItems[i].Length; z++)
                {
                    sDesc = "  " +sDesc;
                }
                lbListOfCategories.Items.Add(sDesc);
            }
        }
    }
}
