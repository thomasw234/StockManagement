using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Text;
using System.Drawing;

namespace BackOffice
{
    class frmSingleInputBox : ScalableForm
    {
        public string Response = "$NONE";
        public string ProductLookupHelp = "";
        public bool GettingCategory = false;
        Label lblQuestion;
        public TextBox tbResponse;
        StockEngine sEngine;
        Button bOK;

        // Used when entering parent code and a partial barcode search is required
        public string ShopCode {get; set;}

        public frmSingleInputBox(string sQuestion, ref StockEngine se)
        {
            sEngine = se;
            this.AllowScaling = false;
            this.Size = new Size(510, 110);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            lblQuestion = new Label();
            lblQuestion.Location = new Point(10, 10);
            lblQuestion.Text = sQuestion;
            lblQuestion.AutoSize = true;
            this.Controls.Add(lblQuestion);

            tbResponse = new TextBox();
            tbResponse.Location = new Point(10, BelowLastControl);
            tbResponse.Size = new Size(400, 25);
            this.Controls.Add(tbResponse);
            tbResponse.KeyDown += new KeyEventHandler(tbResponse_KeyDown);

            bOK = new Button();
            bOK.Location = new Point(414, tbResponse.Top);
            bOK.Size = new Size(65, 23);
            bOK.Text = "OK";
            this.Controls.Add(bOK);
            bOK.Click += new EventHandler(bOK_Click);
        }

        void bOK_Click(object sender, EventArgs e)
        {
            if (tbResponse.Text == "")
            {
                Response = "$NONE";
            }
            else
            {
                Response = tbResponse.Text;
            }
            this.Close();
        }

        void bOK_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (tbResponse.Text == "")
                {
                    Response = "$NONE";
                }
                else
                {
                    Response = tbResponse.Text;
                }
                this.Close();
            }
        }

        void tbResponse_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (tbResponse.Text == "")
                {
                    Response = "$NONE";
                }
                else
                {
                    Response = tbResponse.Text; 
                    
                    if (!GettingCategory && lblQuestion.Text.Contains("parent"))
                    {
                        if (this.ShopCode != null && sEngine.GetItemStockStaRecord(tbResponse.Text, this.ShopCode).Length < 3) // ie Item doesn't exist
                        {
                            frmSearchForItemV2 fsfi = new frmSearchForItemV2(ref sEngine);
                            fsfi.CheckForPartialBarcodeFromScanner(tbResponse.Text, false);
                            fsfi.ShowDialog();
                            if (fsfi.GetItemBarcode() != "NONE_SELECTED")
                            {
                                tbResponse.Text = fsfi.GetItemBarcode();
                                Response = tbResponse.Text;
                            }
                            else
                            {
                                Response = "$NONE";
                            }
                        }
                    }
                }
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                tbResponse.Text = "";
                Response = "$NONE";
                this.Close();
            }
            else if (e.KeyCode == Keys.F5)
            {
                // Parent one means that when looking for a parent item, categories will appear
                // Request from Jim
                if (!GettingCategory && !lblQuestion.Text.Contains("parent"))
                {
                    frmSearchForItemV2 fsfi = new frmSearchForItemV2(ref sEngine);
                    if (ProductLookupHelp != "")
                    {
                        fsfi.CheckForPartialBarcodeFromScanner(ProductLookupHelp, false);
                    }
                    fsfi.ShowDialog();
                    if (fsfi.GetItemBarcode() != "NONE_SELECTED")
                    {
                        tbResponse.Text = fsfi.GetItemBarcode();
                        Response = tbResponse.Text;
                        this.Close();
                    }
                }
                else
                {
                    frmCategorySelect fCat = new frmCategorySelect(ref sEngine);
                    fCat.ShowDialog();
                    if (fCat.SelectedItemCategory != "$NULL")
                    {
                        frmSearchForItemV2 fsfi = new frmSearchForItemV2(ref sEngine);
                        fsfi.SetSearchTerm("CAT:" + fCat.SelectedItemCategory);
                        fsfi.ShowDialog();
                        if (!fsfi.GetItemBarcode().Equals("NONE_SELECTED"))
                        {
                            tbResponse.Text =  fsfi.GetItemBarcode();
                        }
                        else
                        {
                            tbResponse.Text = "";
                        }
                    }
                }
            }
        }
    }
}
