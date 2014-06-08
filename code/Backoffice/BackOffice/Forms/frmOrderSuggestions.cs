using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmOrderSuggestions : ScalableForm
    {
        CListBox lbBarcode;
        CListBox lbDesc;
        CListBox lbSugDate;
        CListBox lbIncluding;
        StockEngine sEngine;
        public string[] BarcodesToInclude;
        string sShopCode;

        public frmOrderSuggestions(ref StockEngine se, string sShpCode, string sSupCode)
        {
            this.AllowScaling = false;
            sEngine = se;
            sShopCode = sShpCode;
            AddMessage("INST", "Staff have suggested that the following items be included in this order :", new Point(10, 10));
            AddMessage("INST2", "(Press enter on each item to include it in the order. Y = include, N = delete, '' = ignore)", new Point(10, BelowLastControl));

            AddMessage("BARCODE", "Barcode", new Point(10, BelowLastControl));
            lbBarcode = new CListBox();
            lbBarcode.Location = new Point(10, BelowLastControl);
            lbBarcode.Size = new Size(150, 300);
            lbBarcode.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
            lbBarcode.KeyDown += new KeyEventHandler(lbKeyDown);
            lbBarcode.BorderStyle = BorderStyle.None;
            this.Controls.Add(lbBarcode);

            AddMessage("DESC", "Description", new Point(160, MessageLabel("BARCODE").Top));
            lbDesc = new CListBox();
            lbDesc.Location = new Point(160, lbBarcode.Top);
            lbDesc.Size = new Size(200, 300);
            lbDesc.BorderStyle = BorderStyle.None;
            lbDesc.SelectedIndexChanged +=new EventHandler(lbSelectedChanged);
            lbDesc.KeyDown +=new KeyEventHandler(lbKeyDown);
            this.Controls.Add(lbDesc);

            AddMessage("SUGDATE", "Date Suggested", new Point(360, MessageLabel("BARCODE").Top));
            lbSugDate = new CListBox();
            lbSugDate.Location = new Point(360, lbBarcode.Top);
            lbSugDate.Size = new Size(150, 300);
            lbSugDate.BorderStyle = BorderStyle.None;
            lbSugDate.SelectedIndexChanged +=new EventHandler(lbSelectedChanged);
            lbSugDate.KeyDown +=new KeyEventHandler(lbKeyDown);
            this.Controls.Add(lbSugDate);

            AddMessage("INC", "Including", new Point(470, MessageLabel("BARCODE").Top));
            lbIncluding = new CListBox();
            lbIncluding.Location = new Point(510, lbBarcode.Top);
            lbIncluding.Size = new Size(30, 300);
            lbIncluding.BorderStyle = BorderStyle.None;
            lbIncluding.SelectedIndexChanged +=new EventHandler(lbSelectedChanged);
            lbIncluding.KeyDown +=new KeyEventHandler(lbKeyDown);
            this.Controls.Add(lbIncluding);

            int nOfResults = 0;
            string[,] sSugs = sEngine.GetSuggestedItemsForOrder(sSupCode, sShopCode, ref nOfResults);
            for (int i = 0; i < nOfResults; i++)
            {
                lbBarcode.Items.Add(sSugs[i, 0]);
                lbDesc.Items.Add(sEngine.GetMainStockInfo(sSugs[i, 0])[1]);
                string sDate = sSugs[i, 1][0].ToString() + sSugs[i, 1][1].ToString() + "/" + sSugs[i, 1][2].ToString() + sSugs[i, 1][3].ToString() + "/" + sSugs[i, 1][4].ToString() + sSugs[i, 1][5].ToString();
                lbSugDate.Items.Add(sDate);
                lbIncluding.Items.Add("");
            }
            lbBarcode.SelectedIndex = 0;

            this.Size = new Size(550, 400);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.VisibleChanged += new EventHandler(frmOrderSuggestions_VisibleChanged);

            this.Text = "Order Suggestions";
        }

        void frmOrderSuggestions_VisibleChanged(object sender, EventArgs e)
        {
            if (lbBarcode.Items.Count == 0)
            {
                this.Close();
            }
        }

        void lbKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (lbIncluding.Items[((ListBox)sender).SelectedIndex].ToString() == "")
                {
                    lbIncluding.Items[((ListBox)sender).SelectedIndex] = "Y";
                }
                else if (lbIncluding.Items[((ListBox)sender).SelectedIndex].ToString() == "Y")
                {
                    lbIncluding.Items[((ListBox)sender).SelectedIndex] = "N";
                }
                else if (lbIncluding.Items[((ListBox)sender).SelectedIndex].ToString() == "N")
                {
                    lbIncluding.Items[((ListBox)sender).SelectedIndex] = "";
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Save();
            }
            else if (e.KeyCode == Keys.Y)
            {
                lbIncluding.Items[((ListBox)sender).SelectedIndex] = "Y";
                if (((ListBox)sender).SelectedIndex + 1 != ((ListBox)sender).Items.Count)
                {
                    ((ListBox)sender).SelectedIndex++;
                }
                else
                    Save();
            }
            else if (e.KeyCode == Keys.N)
            {
                lbIncluding.Items[((ListBox)sender).SelectedIndex] = "N";
                if (((ListBox)sender).SelectedIndex + 1 != ((ListBox)sender).Items.Count)
                {
                    ((ListBox)sender).SelectedIndex++;
                }
                else
                    Save();
            }
            else if (e.KeyCode == Keys.Space)
            {
                lbIncluding.Items[((ListBox)sender).SelectedIndex] = "";
                if (((ListBox)sender).SelectedIndex + 1 != ((ListBox)sender).Items.Count)
                {
                    ((ListBox)sender).SelectedIndex++;
                }
                else
                    Save();
            }
        }

        void Save()
        {
            BarcodesToInclude = new string[0];
            for (int i = 0; i < lbBarcode.Items.Count; i++)
            {
                if (lbIncluding.Items[i].ToString() == "Y")
                {
                    Array.Resize<string>(ref BarcodesToInclude, BarcodesToInclude.Length + 1);
                    BarcodesToInclude[BarcodesToInclude.Length - 1] = lbBarcode.Items[i].ToString();
                    sEngine.RemoveSuggestedOrderItem(lbBarcode.Items[i].ToString(), sShopCode);
                }
                else if (lbIncluding.Items[i].ToString() == "N")
                {
                    sEngine.RemoveSuggestedOrderItem(lbBarcode.Items[i].ToString(), sShopCode);
                }
            }
            this.Close();
        }

        void lbSelectedChanged(object sender, EventArgs e)
        {
            lbBarcode.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbDesc.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbSugDate.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbIncluding.SelectedIndex = ((ListBox)sender).SelectedIndex;
        }

        public void RemoveSuggestion(string sBarcode)
        {
            for (int i = 0; i < lbBarcode.Items.Count; i++)
            {
                if (lbBarcode.Items[i].ToString().ToUpper() == sBarcode.ToUpper())
                {
                    lbBarcode.Items.RemoveAt(i);
                    lbDesc.Items.RemoveAt(i);
                    lbIncluding.Items.RemoveAt(i);
                    lbSugDate.Items.RemoveAt(i);
                    i -= 1;
                }
            }
            sEngine.RemoveSuggestedOrderItem(sBarcode, sShopCode);
        }
    }
}
