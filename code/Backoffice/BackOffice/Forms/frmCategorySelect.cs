using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmCategorySelect : ScalableForm
    {
        StockEngine sEngine;
        CListBox lbCategories;
        string sCurrentCategory = "";
        string[] sCurrentLevelCategories;
        string SelectedCategory = "$NULL";
        int[] nSelectionLocations = new int[5];

        public frmCategorySelect(ref StockEngine se)
        {
            sEngine = se;

            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Size = new Size(250, 510);
            this.AllowScaling = false;
            this.Text = "Select A Category";

            AddMessage("INST1", "Insert Key to edit categories", new Point(10, 10));
            AddMessage("INST2", "F6 to select the last category", new Point(10, 30));
            AddMessage("INST3", "Space to select the category (any level)", new Point(10, 50));
            lbCategories = new CListBox();
            lbCategories.Location = new Point(10, 70);
            lbCategories.ShowScrollbar = true;
            lbCategories.Size = new Size(this.ClientSize.Width - 20, this.ClientSize.Height - 70);
            lbCategories.BorderStyle = BorderStyle.FixedSingle;
            lbCategories.KeyDown += new KeyEventHandler(lbCategories_KeyDown);
            this.Controls.Add(lbCategories);

            ShowCategories("");
        }

        void lbCategories_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                nSelectionLocations[sCurrentCategory.Length / 2] = lbCategories.SelectedIndex;
                ShowCategories(sCurrentLevelCategories[lbCategories.SelectedIndex]);
            }
            else if (e.KeyCode == Keys.Space)
            {
                SelectedCategory = sCurrentLevelCategories[lbCategories.SelectedIndex];
                this.Close();
            }
            else if (e.KeyCode == Keys.Insert)
            {
                frmCategoryEdit fce = new frmCategoryEdit(ref sEngine);
                fce.ShowDialog();
                ShowCategories(sCurrentCategory);
            }
            else if (e.KeyCode == Keys.F6)
            {
                SelectedCategory = sEngine.LastCategoryCode;
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                if (sCurrentCategory.Length != 0)
                {
                    nSelectionLocations[sCurrentCategory.Length / 2] = 0;
                    string sCategoryAbove = "";
                    for (int i = 0; i < sCurrentCategory.Length - 2; i++)
                    {
                        sCategoryAbove += sCurrentCategory[i].ToString();
                    }
                    ShowCategories(sCategoryAbove);
                }
                else
                    this.Close();
            }
        }

        public void ShowCategories(string sCurrentLevel)
        {
            lbCategories.Items.Clear();
            sCurrentLevelCategories = sEngine.GetListOfCategories(sCurrentLevel);
            if (sCurrentLevelCategories.Length == 0)
            {
                SelectedCategory = sCurrentLevel;
                sEngine.LastCategoryCode = SelectedCategory;
                this.Close();
            }
            sCurrentCategory = sCurrentLevel;
            Array.Sort(sCurrentLevelCategories);
            for (int i = 0; i < sCurrentLevelCategories.Length; i++)
            {
                lbCategories.Items.Add(sEngine.GetCategoryDesc(sCurrentLevelCategories[i]));
            }
            if (lbCategories.Items.Count != 0)
            {
                lbCategories.SelectedIndex = nSelectionLocations[(sCurrentLevel.Length / 2)];
            }
        }

        public void SelectCategoryCode(string sCode)
        {
            lbCategories.SelectedIndex = 0;
            for (int i = 0; i < lbCategories.Items.Count; i++)
            {
                if (sCurrentLevelCategories[i].ToUpper() == sCode.ToUpper())
                {
                    lbCategories.SelectedIndex = i;
                    break;
                }
            }
        }


        public string SelectedItemCategory
        {
            get
            {
                return SelectedCategory;
            }
        }

    }
}
