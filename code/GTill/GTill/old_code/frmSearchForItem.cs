using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Design;
using DBFDetailsViewerV2;
using TillEngine;

namespace GTill
{
    class frmSearchForItem : Form
    {
        public enum FormState { CategoryLookup, BarcodeSearch, DescSearch };
        Color cFrmForeColour, cFrmBackColour;
        Label lblTitle;
        Label lblInstruction;
        TextBox tbInput;
        Label[] lblModes;
        Table tTillCat;
        FormState fsCurrentFormState;
        TillEngine.TillEngine tEngine;
        bool bLoadedFormBefore = false;
        string sFontName;

        // Category Lookup

        ListBox lbCategories;
        string sCurrentCategory;
        string[] sCurrentlyDisplayedCategoryCodes;
        ListBox[] lbItemDisplay;
        Label[] lblListBoxDesc;
        string sBarcodeOfProductSelected;

        // End of Category Lookup

        // Barcode Search

        ListBox[] lbBCodeProducts;
        Label[] lblBCodeListBoxDesc;

        // End of Barcode Search

        // Description Search

        TextBox tbDescInput;
        ListBox[] lbDescProducts;
        Label[] lblDescListBoxDesc;

        // End of Description Search

        public frmSearchForItem(ref TillEngine.TillEngine te)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            cFrmBackColour = Properties.Settings.Default.cFrmForeColour;
            cFrmForeColour = Properties.Settings.Default.cFrmBackColour;
            this.BackColor = cFrmBackColour;
            this.ForeColor = cFrmForeColour;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(640, 480);
            sFontName = Properties.Settings.Default.sFontName;
            tTillCat = new Table("TILLCAT.DBF");
            DrawForm(FormState.CategoryLookup);
            fsCurrentFormState = FormState.CategoryLookup;
            tEngine = te;
            this.KeyDown += new KeyEventHandler(frm_KeyDown);
        }

        void frm_KeyDown(object sender, KeyEventArgs e)
        {
            if (fsCurrentFormState == FormState.CategoryLookup)
                DrawForm(FormState.BarcodeSearch);
            else if (fsCurrentFormState == FormState.BarcodeSearch)
                DrawForm(FormState.DescSearch);
            else if (fsCurrentFormState == FormState.DescSearch)
                DrawForm(FormState.CategoryLookup);
        }

        public void DrawForm(FormState fs)
        {
            fsCurrentFormState = fs;
            if (!bLoadedFormBefore)
            {
                lblTitle = new Label();
                lblTitle.BackColor = cFrmBackColour;
                lblTitle.ForeColor = cFrmForeColour;
                lblTitle.Font = new Font(sFontName, 18.0f);
                lblTitle.Text = "Product Lookup";
                lblTitle.Left = 0;
                lblTitle.Top = 0;
                lblTitle.AutoSize = true;
                int nLblHeight = lblTitle.Height;
                lblTitle.AutoSize = false;
                lblTitle.Width = this.Width;
                lblTitle.Height = 30;
                lblTitle.TextAlign = ContentAlignment.MiddleCenter;
                this.Controls.Add(lblTitle);

                lblModes = new Label[3];
                for (int i = 0; i < 3; i++)
                {
                    lblModes[i] = new Label();
                    lblModes[i].AutoSize = true;
                    lblModes[i].BackColor = cFrmBackColour;
                    lblModes[i].ForeColor = cFrmForeColour;
                    lblModes[i].Font = new Font(sFontName, 14.0f);
                    lblModes[i].Top = 40;
                    this.Controls.Add(lblModes[i]);
                }
                lblModes[0].Text = "Category";
                lblModes[1].Text = "Barcode";
                lblModes[2].Text = "Description";
                lblModes[1].Left = (this.Width / 2) - (lblModes[1].Width / 2);
                lblModes[0].Left = lblModes[1].Left - lblModes[0].Width - 10;
                lblModes[2].Left = lblModes[1].Left + lblModes[1].Width + 10;

                lblInstruction = new Label();
                lblInstruction.BackColor = cFrmBackColour;
                lblInstruction.ForeColor = cFrmForeColour;
                lblInstruction.Left = 0;
                lblInstruction.Width = this.Width;
                lblInstruction.Top = lblModes[0].Top + lblModes[0].Height + 10;
                lblInstruction.Height = 60;
                lblInstruction.AutoSize = false;
                lblInstruction.Font = new Font(sFontName, 14.0f);
                lblInstruction.Text = "Select the category that the product is in, or press RIGHT ARROW to change mode";
                lblInstruction.TextAlign = ContentAlignment.MiddleCenter;
                this.Controls.Add(lblInstruction);
                bLoadedFormBefore = true;
            }

            // Clear up from previous states

            try
            {
                this.Controls.Remove(lbCategories);
                lbCategories.Dispose();
                for (int i = 0; i < lbItemDisplay.Length; i++)
                {
                    this.Controls.Remove(lbItemDisplay[i]);
                    lbItemDisplay[i].Dispose();
                    this.Controls.Remove(lblListBoxDesc[i]);
                    lblListBoxDesc[i].Dispose();
                }
                this.Controls.Remove(tbInput);
                tbInput.Dispose();
                for (int i = 0; i < lbBCodeProducts.Length; i++)
                {
                    this.Controls.Remove(lbBCodeProducts[i]);
                    lbBCodeProducts[i].Dispose();
                    this.Controls.Remove(lblBCodeListBoxDesc[i]);
                    lblBCodeListBoxDesc[i].Dispose();
                }
                this.Controls.Remove(tbDescInput);
                tbDescInput.Dispose();
                for (int i = 0; i < lbDescProducts.Length; i++)
                {
                    this.Controls.Remove(lbDescProducts[i]);
                    lbDescProducts[i].Dispose();
                    this.Controls.Remove(lblDescListBoxDesc[i]);
                    lblBCodeListBoxDesc[i].Dispose();
                }
            }
            catch
            {
                ;
            }

            if (fs == FormState.CategoryLookup)
            {
                lbCategories = new ListBox();
                lbCategories.Font = new Font(sFontName, 14.0f);
                sCurrentCategory = "";
                lbCategories.Top = lblInstruction.Top + lblInstruction.Height + 10;
                lbCategories.Left = 0;
                lbCategories.Width = this.Width;
                lbCategories.Height = this.Height - lbCategories.Top;
                lbCategories.BorderStyle = BorderStyle.FixedSingle;
                lbCategories.KeyDown += new KeyEventHandler(lbCategories_KeyDown);
                lbCategories.BackColor = cFrmBackColour;
                lbCategories.ForeColor = cFrmForeColour;
                this.Controls.Add(lbCategories);
                UpdateListBoxWithCategories();
                lbCategories.SelectedIndex = 0;
                lblModes[0].BackColor = cFrmForeColour;
                lblModes[0].ForeColor = cFrmBackColour;
                lblModes[1].BackColor = cFrmBackColour;
                lblModes[1].ForeColor = cFrmForeColour;
                lblModes[2].BackColor = cFrmBackColour;
                lblModes[2].ForeColor = cFrmForeColour;

                lbItemDisplay = new ListBox[4];
                lblListBoxDesc = new Label[4];
                for (int i = 0; i < lbItemDisplay.Length; i++)
                {
                    lbItemDisplay[i] = new ListBox();
                    lbItemDisplay[i].Font = new Font(sFontName, 14.0f);
                    lbItemDisplay[i].BackColor = cFrmBackColour;
                    lbItemDisplay[i].ForeColor = cFrmForeColour;
                    lbItemDisplay[i].BorderStyle = BorderStyle.None;
                    lbItemDisplay[i].Items.Add("Test");
                    lbItemDisplay[i].Top = lbCategories.Top + 35;
                    lbItemDisplay[i].Height = lbCategories.Height - (lbItemDisplay[i].Top - lbCategories.Top);
                    lbItemDisplay[i].ScrollAlwaysVisible = true;
                    lbItemDisplay[i].SelectedIndexChanged += new EventHandler(frmSearchForItem_SelectedIndexChanged);
                    lbItemDisplay[i].KeyDown += new KeyEventHandler(frmSearchForItem_KeyDown);
                    this.Controls.Add(lbItemDisplay[i]);

                    lblListBoxDesc[i] = new Label();
                    lblListBoxDesc[i].Top = lbItemDisplay[i].Top - 35;
                    lblListBoxDesc[i].BackColor = cFrmBackColour;
                    lblListBoxDesc[i].ForeColor = cFrmForeColour;
                    lblListBoxDesc[i].Font = new Font(sFontName, 16.0f);
                    lblListBoxDesc[i].AutoSize = true;
                    this.Controls.Add(lblListBoxDesc[i]);
                }
                lbItemDisplay[0].Left = 0;
                lbItemDisplay[1].Left = 150;
                lbItemDisplay[2].Left = 490;
                lbItemDisplay[3].Left = 600;
                lbItemDisplay[0].Width = 640;
                lbItemDisplay[1].Width = 560;
                lbItemDisplay[2].Width = 190;
                lbItemDisplay[3].Width = 190;
                lblListBoxDesc[0].Text = "Barcode";
                lblListBoxDesc[1].Text = "Description";
                lblListBoxDesc[2].Text = "Price";
                lblListBoxDesc[3].Text = "Stock";
                lblListBoxDesc[0].Left = 0;
                lblListBoxDesc[1].Left = lbItemDisplay[1].Left;
                lblListBoxDesc[2].Left = lbItemDisplay[2].Left;
                lblListBoxDesc[3].Left = 560;
                lbCategories.Focus();


                /*
                 * lbBCodeProducts[0].Left = 0;
                lbBCodeProducts[1].Left = 150;
                lbBCodeProducts[2].Left = 490;
                lbBCodeProducts[3].Left = 600;
                lbBCodeProducts[0].Width = 640;
                lbBCodeProducts[1].Width = 560;
                lbBCodeProducts[2].Width = 190;
                lbBCodeProducts[3].Width = 190;
                lblBCodeListBoxDesc[0].Left = 0;
                lblBCodeListBoxDesc[1].Left = 150;
                lblBCodeListBoxDesc[2].Left = 470;
                lblBCodeListBoxDesc[3].Left = 560;
                lblBCodeListBoxDesc[0].Text = "Barcode";
                lblBCodeListBoxDesc[1].Text = "Description";
                lblBCodeListBoxDesc[2].Text = "Price";
                lblBCodeListBoxDesc[3].Text = "Stock";
                lbBCodeProducts[1].BringToFront();
                lbBCodeProducts[2].BringToFront();
                lbBCodeProducts[3].BringToFront();
                 */
            }
            else if (fs == FormState.BarcodeSearch)
            {
                tbInput = new TextBox();
                tbInput.BorderStyle = BorderStyle.FixedSingle;
                tbInput.BackColor = cFrmBackColour;
                tbInput.ForeColor = cFrmForeColour;
                tbInput.Width = this.Width;
                tbInput.Left = 0;
                tbInput.Font = new Font(sFontName, 16.0f);
                tbInput.Top = lblInstruction.Top + lblInstruction.Height + 5;
                tbInput.KeyDown += new KeyEventHandler(tbInput_KeyDown);
                this.Controls.Add(tbInput);
                tbInput.Focus();
                tbInput.TextAlign = HorizontalAlignment.Center;

                lblInstruction.Text = "Enter a part of the barcode to search for possible products, or press RIGHT ARROW to change mode.";
                lblModes[0].BackColor = cFrmBackColour;
                lblModes[0].ForeColor = cFrmForeColour;
                lblModes[1].BackColor = cFrmForeColour;
                lblModes[1].ForeColor = cFrmBackColour;
                lblModes[2].BackColor = cFrmBackColour;
                lblModes[2].ForeColor = cFrmForeColour;

                lbBCodeProducts = new ListBox[4];
                lblBCodeListBoxDesc = new Label[4];
                for (int i = 0; i < lbBCodeProducts.Length; i++)
                {
                    lbBCodeProducts[i] = new ListBox();
                    lbBCodeProducts[i].Font = new Font(sFontName, 14.0f);
                    lbBCodeProducts[i].BackColor = cFrmBackColour;
                    lbBCodeProducts[i].ForeColor = cFrmForeColour;
                    lbBCodeProducts[i].ScrollAlwaysVisible = true;
                    lbBCodeProducts[i].SelectedIndexChanged += new EventHandler(lbBCodeP_SelectedIndexChanged);
                    lbBCodeProducts[i].Top = tbInput.Top + tbInput.Height + 35;
                    lbBCodeProducts[i].Height = this.Height - lbBCodeProducts[i].Top;
                    lbBCodeProducts[i].BorderStyle = BorderStyle.None;
                    lbBCodeProducts[i].KeyDown += new KeyEventHandler(lbBCode_KeyDown);
                    this.Controls.Add(lbBCodeProducts[i]);

                    lblBCodeListBoxDesc[i] = new Label();
                    lblBCodeListBoxDesc[i].Font = new Font(sFontName, 16.0f);
                    lblBCodeListBoxDesc[i].BackColor = cFrmBackColour;
                    lblBCodeListBoxDesc[i].ForeColor = cFrmForeColour;
                    lblBCodeListBoxDesc[i].AutoSize = true;
                    lblBCodeListBoxDesc[i].Top = lbBCodeProducts[i].Top - lblBCodeListBoxDesc[i].Height - 3;
                    this.Controls.Add(lblBCodeListBoxDesc[i]);
                }
                lbBCodeProducts[0].Left = 0;
                lbBCodeProducts[1].Left = 150;
                lbBCodeProducts[2].Left = 490;
                lbBCodeProducts[3].Left = 600;
                lbBCodeProducts[0].Width = 640;
                lbBCodeProducts[1].Width = 560;
                lbBCodeProducts[2].Width = 190;
                lbBCodeProducts[3].Width = 190;
                lblBCodeListBoxDesc[0].Left = 0;
                lblBCodeListBoxDesc[1].Left = 150;
                lblBCodeListBoxDesc[2].Left = 470;
                lblBCodeListBoxDesc[3].Left = 560;
                lblBCodeListBoxDesc[0].Text = "Barcode";
                lblBCodeListBoxDesc[1].Text = "Description";
                lblBCodeListBoxDesc[2].Text = "Price";
                lblBCodeListBoxDesc[3].Text = "Stock";
                lbBCodeProducts[1].BringToFront();
                lbBCodeProducts[2].BringToFront();
                lbBCodeProducts[3].BringToFront();
            }
            else if (fs == FormState.DescSearch)
            {
                lblInstruction.Text = "Enter part of the description of the product and press ENTER, or press RIGHT ARROW to change mode.";
                lblModes[0].BackColor = cFrmBackColour;
                lblModes[0].ForeColor = cFrmForeColour;
                lblModes[1].BackColor = cFrmBackColour;
                lblModes[1].ForeColor = cFrmForeColour;
                lblModes[2].BackColor = cFrmForeColour;
                lblModes[2].ForeColor = cFrmBackColour;

                tbDescInput = new TextBox();
                tbDescInput.BackColor = cFrmBackColour;
                tbDescInput.ForeColor = cFrmForeColour;
                tbDescInput.Font = new Font(sFontName, 16.0f);
                tbDescInput.Width = this.Width;
                tbDescInput.Left = 0;
                tbDescInput.Top = lblInstruction.Top + lblInstruction.Height + 5;
                tbDescInput.TextAlign = HorizontalAlignment.Center;
                tbDescInput.BorderStyle = BorderStyle.FixedSingle;
                tbDescInput.KeyDown += new KeyEventHandler(tbDescInput_KeyDown);
                this.Controls.Add(tbDescInput);
                tbDescInput.Focus();

                lblDescListBoxDesc = new Label[4];
                lbDescProducts = new ListBox[4];

                for (int i = 0; i < lbDescProducts.Length; i++)
                {
                    lbDescProducts[i] = new ListBox();
                    lbDescProducts[i].BackColor = cFrmBackColour;
                    lbDescProducts[i].ForeColor = cFrmForeColour;
                    lbDescProducts[i].Font = new Font(sFontName, 14.0f);
                    lbDescProducts[i].Top = tbDescInput.Top + tbDescInput.Height + 35;
                    lbDescProducts[i].Height = this.Height - lbDescProducts[i].Top;
                    lbDescProducts[i].BorderStyle = BorderStyle.None;
                    lbDescProducts[i].ScrollAlwaysVisible = true;
                    lbDescProducts[i].KeyDown += new KeyEventHandler(lbDescProducts_KeyDown);
                    lbDescProducts[i].SelectedIndexChanged += new EventHandler(lbDescProducts_SIChanged);
                    this.Controls.Add(lbDescProducts[i]);

                    lblDescListBoxDesc[i] = new Label();
                    lblDescListBoxDesc[i].BackColor = cFrmBackColour;
                    lblDescListBoxDesc[i].ForeColor = cFrmForeColour;
                    lblDescListBoxDesc[i].Font = new Font(sFontName, 16.0f);
                    lblDescListBoxDesc[i].AutoSize = true;
                    lblDescListBoxDesc[i].Top = lbDescProducts[i].Top - lblDescListBoxDesc[i].Height - 3;
                    this.Controls.Add(lblDescListBoxDesc[i]);
                }
                lbDescProducts[0].Left = 0;
                lbDescProducts[1].Left = 150;
                lbDescProducts[2].Left = 490;
                lbDescProducts[3].Left = 600;
                lbDescProducts[0].Width = 640;
                lbDescProducts[1].Width = 560;
                lbDescProducts[2].Width = 190;
                lbDescProducts[2].Width = 190;
                lblDescListBoxDesc[0].Left = 0;
                lblDescListBoxDesc[1].Left = 150;
                lblDescListBoxDesc[2].Left = 470;
                lblDescListBoxDesc[3].Left = 560;
                lblDescListBoxDesc[0].Text = "Barcode";
                lblDescListBoxDesc[1].Text = "Description";
                lblDescListBoxDesc[2].Text = "Price";
                lblDescListBoxDesc[3].Text = "Stock";
                lbDescProducts[1].BringToFront();
                lbDescProducts[2].BringToFront();
                lbDescProducts[3].BringToFront();
            }
        }

        void lbDescProducts_SIChanged(object sender, EventArgs e)
        {
            ListBox lb = (ListBox)sender;
            for (int i = 0; i < lbDescProducts.Length; i++)
            {
                lbDescProducts[i].SelectedIndex = lb.SelectedIndex;
            }
        }

        void lbDescProducts_KeyDown(object sender, KeyEventArgs e)
        {
            ListBox lbSender = (ListBox)sender;
            if (e.KeyCode == Keys.Right)
            {
                if (fsCurrentFormState == FormState.CategoryLookup)
                    DrawForm(FormState.BarcodeSearch);
                else if (fsCurrentFormState == FormState.BarcodeSearch)
                    DrawForm(FormState.DescSearch);
                else if (fsCurrentFormState == FormState.DescSearch)
                    DrawForm(FormState.CategoryLookup);
            }
            else if (e.KeyCode == Keys.Up && lbSender.SelectedIndex == 0)
            {
                tbDescInput.Focus();
            }
            else if (e.KeyCode == Keys.Down && lbSender.SelectedIndex == lbSender.Items.Count - 1)
            {
                tbDescInput.Focus();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                tbDescInput.Focus();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                sBarcodeOfProductSelected = lbDescProducts[0].Items[lbDescProducts[0].SelectedIndex].ToString();
                this.Close();
            }
        }

        void tbDescInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && tbDescInput.Text.Length > 2)
            {
                lbDescProducts[0].Items.Clear();
                lbDescProducts[1].Items.Clear();
                lbDescProducts[2].Items.Clear();
                lbDescProducts[3].Items.Clear();
                int nOfResults = 0;
                string[,] sPossibilities = tEngine.sGetAccordingToPartialDescription(tbDescInput.Text, ref nOfResults);
                for (int i = 0; i < nOfResults; i++)
                {
                    lbDescProducts[0].Items.Add(sPossibilities[i, 0]);
                    lbDescProducts[1].Items.Add(sPossibilities[i, 1]);
                    lbDescProducts[2].Items.Add(TillEngine.TillEngine.FormatMoneyForDisplay((float)Convert.ToDecimal(sPossibilities[i, 4])));
                    lbDescProducts[3].Items.Add(tEngine.GetItemStockLevel(sPossibilities[i,0]));
                }
                if (lbDescProducts[0].Items.Count > 0)
                {
                    lbDescProducts[0].SelectedIndex = 0;
                    lbDescProducts[0].Focus();
                }
            }
            else if (e.KeyCode == Keys.Enter && tbDescInput.Text.Length <= 2)
            {
                MessageBox.Show("Please enter at least 3 characters to search with.");
            }
            else if (e.KeyCode == Keys.Escape)
            {
                sBarcodeOfProductSelected = "NONE_SELECTED";
                this.Close();
            }
            else if (e.KeyCode == Keys.Down && lbDescProducts[0].Items.Count > 0)
            {
                lbDescProducts[0].SelectedIndex = 0;
                lbDescProducts[0].Focus();
            }
            else if (e.KeyCode == Keys.Up && lbDescProducts[0].Items.Count > 0)
            {
                lbDescProducts[0].SelectedIndex = lbDescProducts[0].Items.Count - 1;
                lbDescProducts[0].Focus();
            }
            else if (e.KeyCode == Keys.Right)
            {
                 if (fsCurrentFormState == FormState.CategoryLookup)
                    DrawForm(FormState.BarcodeSearch);
                else if (fsCurrentFormState == FormState.BarcodeSearch)
                    DrawForm(FormState.DescSearch);
                else if (fsCurrentFormState == FormState.DescSearch)
                    DrawForm(FormState.CategoryLookup);
            }
        }

        void lbBCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && lblInstruction.Text.StartsWith("The barcode"))
            {
                sBarcodeOfProductSelected = "NONE_SELECTED";
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                tbInput.Focus();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                sBarcodeOfProductSelected = lbBCodeProducts[0].Items[lbBCodeProducts[0].SelectedIndex].ToString();
                this.Close();
            }
            else if (e.KeyCode == Keys.Up && lbBCodeProducts[0].SelectedIndex == 0)
            {
                tbInput.Focus();
            }
            else if (e.KeyCode == Keys.Down && lbBCodeProducts[0].SelectedIndex == lbBCodeProducts[0].Items.Count - 1)
            {
                tbInput.Focus();
            }
            else if (e.KeyCode == Keys.Right)
            {
                if (fsCurrentFormState == FormState.CategoryLookup)
                    DrawForm(FormState.BarcodeSearch);
                else if (fsCurrentFormState == FormState.BarcodeSearch)
                    DrawForm(FormState.DescSearch);
                else if (fsCurrentFormState == FormState.DescSearch)
                    DrawForm(FormState.CategoryLookup);
            }
        }

        void tbInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && tbInput.Text.Length > 2)
            {
                string sToSearchFor = tbInput.Text;
                int nOfItems = 0;
                lbBCodeProducts[0].Items.Clear();
                lbBCodeProducts[1].Items.Clear();
                lbBCodeProducts[2].Items.Clear();
                string[,] sResults = tEngine.sGetAccordingToPartialBarcode(sToSearchFor, ref nOfItems);
                for (int i = 0; i < nOfItems; i++)
                {
                    lbBCodeProducts[0].Items.Add(sResults[i, 0]);
                    lbBCodeProducts[1].Items.Add(sResults[i, 1]);
                    lbBCodeProducts[2].Items.Add(TillEngine.TillEngine.FormatMoneyForDisplay((float)Convert.ToDecimal(sResults[i, 4])));
                    int nStockLevel = tEngine.GetItemStockLevel(sResults[i, 0]);
                    lbBCodeProducts[3].Items.Add(nStockLevel.ToString());
               }
                if (lbBCodeProducts[0].Items.Count > 0)
                {
                    lbBCodeProducts[0].SelectedIndex = 0;
                    lbBCodeProducts[0].Focus();
                }
            }
            else if (tbInput.Text.Length <= 2 && e.KeyCode == Keys.Enter)
            {
                MessageBox.Show("Please enter at least 3 characters to search with.");
            }
            else if (e.KeyCode == Keys.Escape)
            {
                sBarcodeOfProductSelected = "NONE_SELECTED";
                this.Close();
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (lbBCodeProducts[0].Items.Count > 0)
                {
                    lbBCodeProducts[0].SelectedIndex = 0;
                    lbBCodeProducts[0].Focus();
                }
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (lbBCodeProducts[0].Items.Count > 0)
                {
                    lbBCodeProducts[0].SelectedIndex = lbBCodeProducts[0].Items.Count - 1;
                    lbBCodeProducts[0].Focus();
                }
            }
            else if (e.KeyCode == Keys.Right)
            {
                if (fsCurrentFormState == FormState.CategoryLookup)
                    DrawForm(FormState.BarcodeSearch);
                else if (fsCurrentFormState == FormState.BarcodeSearch)
                    DrawForm(FormState.DescSearch);
                else if (fsCurrentFormState == FormState.DescSearch)
                    DrawForm(FormState.CategoryLookup);
            }

        }

        void lbBCodeP_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox lb = (ListBox)sender;
            for (int i = 0; i < lbBCodeProducts.Length; i++)
            {
                lbBCodeProducts[i].SelectedIndex = lb.SelectedIndex;
            }
        }

        void frmSearchForItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && lbItemDisplay[0].Items.Count > 0)
            {
                sBarcodeOfProductSelected = lbItemDisplay[0].Items[lbItemDisplay[0].SelectedIndex].ToString().TrimEnd(' ');
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                lbCategories.BringToFront();
                lbCategories.Visible = true;
                lbCategories.Focus();
                lbCategories_KeyDown(sender, e);
            }
            else if (e.KeyCode == Keys.Right)
            {
                if (fsCurrentFormState == FormState.CategoryLookup)
                    DrawForm(FormState.BarcodeSearch);
                else if (fsCurrentFormState == FormState.BarcodeSearch)
                    DrawForm(FormState.DescSearch);
                else if (fsCurrentFormState == FormState.DescSearch)
                    DrawForm(FormState.CategoryLookup);
            }
        }

        void frmSearchForItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox lb = (ListBox)sender;
            for (int i = 0; i < 4; i++)
            {
                lbItemDisplay[i].SelectedIndex = lb.SelectedIndex;
            }
        }

        void lbCategories_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                sCurrentCategory = sCurrentlyDisplayedCategoryCodes[lbCategories.SelectedIndex];
                UpdateListBoxWithCategories();
            }
            else if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Left)
            {
                if (sCurrentCategory == "")
                    this.Close();
                else
                {
                    sCurrentCategory = sCurrentCategory.TrimEnd(' ');
                    string sTemp = sCurrentCategory;
                    sCurrentCategory = "";
                    for (int i = 0; i < sTemp.Length - 2; i++)
                    {
                        sCurrentCategory += sTemp[i];
                    }
                    UpdateListBoxWithCategories();
                }
            }
            else if (e.KeyCode == Keys.Right)
            {
                if (fsCurrentFormState == FormState.CategoryLookup)
                    DrawForm(FormState.BarcodeSearch);
                else if (fsCurrentFormState == FormState.BarcodeSearch)
                    DrawForm(FormState.DescSearch);
                else if (fsCurrentFormState == FormState.DescSearch)
                    DrawForm(FormState.CategoryLookup);
            }
        }

        void UpdateListBoxWithCategories()
        {
            lbCategories.Items.Clear();
            string[] sToDisplay = GetCategoryCodesBelowCurrent(sCurrentCategory);
            sCurrentlyDisplayedCategoryCodes = new string[sToDisplay.Length];
            Array.Copy(sToDisplay, sCurrentlyDisplayedCategoryCodes, sToDisplay.Length);
            for (int i = 0; i < sToDisplay.Length; i++)
            {
                sToDisplay[i] = sGetCategoryDescription(sToDisplay[i]);
                lbCategories.Items.Add(sToDisplay[i]);
            }
            if (lbCategories.Items.Count != 0)
                lbCategories.SelectedIndex = 0;
            else
            {
                // Products Reached
                int nOfResults = 0;
                string[,] sProductsToDisplay = tEngine.sGetAllInCategory(sCurrentCategory, ref nOfResults);
                DisplayItemsFromCategoryLookup(sProductsToDisplay, nOfResults);
            }
        }            

        string sGetCategoryDescription(string sCategoryCode)
        {
            sCategoryCode = sCategoryCode.TrimEnd(' ');
            string[] sResults = tTillCat.GetRecordFrom(sCategoryCode, 0, true);
            return sResults[1];
        }

        string[] GetCategoryCodesBelowCurrent(string sSelectedCategoryCode)
        {
            string[] sPotentials = tTillCat.SearchAndGetAllMatchingRecords(0, sSelectedCategoryCode);
            for (int i = 0; i < sPotentials.Length; i++)
            {
                if (sPotentials[i].TrimEnd(' ').Length != sSelectedCategoryCode.TrimEnd(' ').Length + 2)
                    sPotentials[i] = null;
                else if (!sPotentials[i].StartsWith(sSelectedCategoryCode.TrimEnd(' ')))
                    sPotentials[i] = null;
            }
            sPotentials = sShortenStringArray(sPotentials);
            return sPotentials;
        }

        string[] sShortenStringArray(string[] sOriginal)
        {
            // Removes null elements from the array

            int nNullCount = 0;
            for (int i = 0; i < sOriginal.Length; i++)
            {
                if (sOriginal[i] == null)
                    nNullCount++;
            }

            int nDiff = 0;
            string[] sToReturn = new string[sOriginal.Length - nNullCount];

            for (int i = 0; i < sOriginal.Length; i++)
            {
                if (sOriginal[i] == null)
                    nDiff++;
                else
                    sToReturn[i - nDiff] = sOriginal[i];
            }
            return sToReturn;
        }

        void DisplayItemsFromCategoryLookup(string[,] sItemsToDisplay, int nOfItems)
        {
            lbCategories.Visible = false;
            lbItemDisplay[0].Items.Clear();
            lbItemDisplay[1].Items.Clear();
            lbItemDisplay[1].BringToFront();
            lbItemDisplay[2].Items.Clear();
            lbItemDisplay[2].BringToFront();
            lbItemDisplay[3].BringToFront();
            lbItemDisplay[3].Items.Clear();
            for (int i = 0; i < nOfItems; i++)
            {
                lbItemDisplay[0].Items.Add(sItemsToDisplay[i, 0].TrimEnd(' '));
                lbItemDisplay[1].Items.Add(sItemsToDisplay[i, 1].TrimEnd(' '));
                lbItemDisplay[2].Items.Add(TillEngine.TillEngine.FormatMoneyForDisplay((float)Convert.ToDecimal(sItemsToDisplay[i, 4].TrimEnd(' '))));
                int nStockLevel = tEngine.GetItemStockLevel(sItemsToDisplay[i,0]);
                lbItemDisplay[3].Items.Add(nStockLevel.ToString());
            }
            if (lbItemDisplay[0].Items.Count > 0)
                lbItemDisplay[0].SelectedIndex = 0;
        }

        public string GetItemBarcode()
        {
            if (sBarcodeOfProductSelected == null || sBarcodeOfProductSelected == "")
                return "NONE_SELECTED";
            else
                return sBarcodeOfProductSelected;
        }

        public void CheckForPartialBarcodeFromScanner(string sScannerInput)
        {
            DrawForm(FormState.BarcodeSearch);
            tbInput.Text = sScannerInput;
            string sToSearchFor = tbInput.Text;
            int nOfItems = 0;
            lbBCodeProducts[0].Items.Clear();
            lbBCodeProducts[1].Items.Clear();
            lbBCodeProducts[2].Items.Clear();
            lbBCodeProducts[3].Items.Clear();
            string[,] sResults = tEngine.sGetAccordingToPartialBarcode(sToSearchFor, ref nOfItems);
            for (int i = 0; i < nOfItems; i++)
            {
                lbBCodeProducts[0].Items.Add(sResults[i, 0]);
                lbBCodeProducts[1].Items.Add(sResults[i, 1]);
                lbBCodeProducts[2].Items.Add(TillEngine.TillEngine.FormatMoneyForDisplay((float)Convert.ToDecimal(sResults[i, 4])));
                lbBCodeProducts[3].Items.Add(tEngine.GetItemStockLevel(sResults[i, 0]));
            }
            if (lbBCodeProducts[0].Items.Count > 0)
            {
                lbBCodeProducts[0].SelectedIndex = 0;
                this.Show();
                lbBCodeProducts[0].Focus();
                lblInstruction.Text = "The barcode was not recognised. The following items are possibly what you were scanning, if so, select the correct one and press ENTER, otherwise, press ESCAPE";
                lblInstruction.Font = new Font(sFontName, 12.0f);
                lblInstruction.BackColor = cFrmForeColour;
                lblInstruction.ForeColor = cFrmBackColour;
            }
            else
            {
                sBarcodeOfProductSelected = "NONE_SELECTED";
                this.Close();
            }
        }
    }
}
