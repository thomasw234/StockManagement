using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using TillEngine;

namespace GTill
{
    class frmCategorySelect : Form
    {
        TillEngine.TillEngine tEngine;
        string sCurrentCategory = "";
        string[] sCurrentlyDisplayedCategoryCodes;
        public string SelectedCategory = "NONE_SELECTED";
        ListBox lbCategories;

        public frmCategorySelect(ref TillEngine.TillEngine te)
        {
            tEngine = te;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = GTill.Properties.Settings.Default.cFrmBackColour;
            this.ForeColor = GTill.Properties.Settings.Default.cFrmForeColour;
            this.Size = new Size(640, 480);

            lbCategories = new ListBox();
            lbCategories.Location = new Point(15, 15);
            lbCategories.Size = new Size(this.Width - 30, this.Height - 30);
            lbCategories.Font = new Font(GTill.Properties.Settings.Default.sFontName, 16.0f);
            lbCategories.BorderStyle = BorderStyle.None;
            lbCategories.KeyDown += new KeyEventHandler(lbCategories_KeyDown);
            this.Controls.Add(lbCategories);

            UpdateListBoxWithCategories();
        }

        void lbCategories_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                sCurrentCategory = sCurrentlyDisplayedCategoryCodes[lbCategories.SelectedIndex];
                UpdateListBoxWithCategories();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                string sPrevCat = "";
                sCurrentCategory = sCurrentCategory.TrimEnd(' ');
                if (sCurrentCategory.Length == 0)
                    this.Close();
                else
                {
                    for (int i = 0; i < sCurrentCategory.Length - 2; i++)
                    {
                        sPrevCat += sCurrentCategory[i].ToString();
                    }
                    sCurrentCategory = sPrevCat;
                    UpdateListBoxWithCategories();
                }
            }
        }

        /// <summary>
        /// Gets the next level of categories and displays them
        /// </summary>
        void UpdateListBoxWithCategories()
        {
            lbCategories.Items.Clear();
            string[] sToDisplay = tEngine.GetCategoryCodesBelowCurrent(sCurrentCategory);
            sCurrentlyDisplayedCategoryCodes = new string[sToDisplay.Length];
            Array.Copy(sToDisplay, sCurrentlyDisplayedCategoryCodes, sToDisplay.Length);
            for (int i = 0; i < sToDisplay.Length; i++)
            {
                sToDisplay[i] = tEngine.sGetCategoryDescription(sToDisplay[i]);
                lbCategories.Items.Add(sToDisplay[i]);
            }
            if (lbCategories.Items.Count != 0)
                lbCategories.SelectedIndex = 0;
            else
            {
                // Products Reached
                SelectedCategory = sCurrentCategory.TrimEnd(' ');
                this.Close();
            }
        } 
    }
}
