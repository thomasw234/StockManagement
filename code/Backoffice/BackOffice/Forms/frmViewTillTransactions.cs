using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmViewTillTransactions : ScalableForm
    {
        StockEngine sEngine;
        StockEngine sOtherEngine;
        CListBox lbTills;
        CListBox lbDays;
        CListBox lbSalesDate;
        CListBox lbTakings;
        string[] sTillCodes;
        bool bAlternateEngine = false;

        public frmViewTillTransactions(ref StockEngine se)
        {
            this.SurroundListBoxes = true;
            this.Size = new Size(580, 290);
            sEngine = se;
            sTillCodes = new string[0];
            lbTills = new CListBox();
            lbTills.Location = new Point(10, 31);
            lbTills.Size = new Size(200, 200);
            lbTills.BorderStyle = BorderStyle.None;
            lbTills.KeyDown += new KeyEventHandler(lbTills_KeyDown);
            lbTills.GotFocus += new EventHandler(lbTills_GotFocus);
            this.Controls.Add(lbTills);
            AddMessage("TILLINST", "Select A Till", new Point(10, 10));

            lbDays = new CListBox();
            lbDays.Location = new Point(250, 31);
            lbDays.Size = new Size(100, 200);
            lbDays.BorderStyle = BorderStyle.None;
            lbDays.SelectedIndexChanged += new EventHandler(lbSelectedChanged);
            lbDays.KeyDown += new KeyEventHandler(lbKeyDown);
            this.Controls.Add(lbDays);
            AddMessage("DAYS", "Day", new Point(250, 10));

            lbSalesDate = new CListBox();
            lbSalesDate.Location = new Point(350, 31);
            lbSalesDate.Size = new Size(100, 200);
            lbSalesDate.BorderStyle = BorderStyle.None;
            lbSalesDate.SelectedIndexChanged +=new EventHandler(lbSelectedChanged);
            lbSalesDate.KeyDown +=new KeyEventHandler(lbKeyDown);
            this.Controls.Add(lbSalesDate);
            AddMessage("SALESDATE", "Sales Date", new Point(350, 10));

            lbTakings = new CListBox();
            lbTakings.Location = new Point(450, 31);
            lbTakings.Size = new Size(this.ClientSize.Width - 10 - lbTakings.Left, 200);
            lbTakings.BorderStyle = BorderStyle.None;
            lbTakings.SelectedIndexChanged +=new EventHandler(lbSelectedChanged);
            lbTakings.RightToLeft = RightToLeft.Yes;
            lbTakings.KeyDown +=new KeyEventHandler(lbKeyDown);
            this.Controls.Add(lbTakings);
            AddMessage("TAKINGS", "Takings", new Point(450, 10));

            AddMessage("INST", "Press Enter to view transactions, or F5 to load up a previous week's transactions.", new Point(10, 230));

            string[] sShopCodes = sEngine.GetListOfShopCodes();
            for (int i = 0; i < sShopCodes.Length; i++)
            {
                string[] sTillCode = sEngine.GetListOfTillCodes(sShopCodes[i]);
                for (int x = 0; x < sTillCode.Length; x++)
                {
                    lbTills.Items.Add(sEngine.GetShopNameFromCode(sShopCodes[i]) + " - " + sTillCode[x]);
                    Array.Resize<string>(ref sTillCodes, sTillCodes.Length + 1);
                    sTillCodes[sTillCodes.Length - 1] = sTillCode[x];
                }
            }
            if (lbTills.Items.Count > 0)
                lbTills.SelectedIndex = 0;
            lbTills.Focus();

            this.AllowScaling = false;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Text = "View Till Transactions";
            this.VisibleChanged += frmViewTillTransactions_VisibleChanged;
        }

        void frmViewTillTransactions_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                if (lbTills.Items.Count > 0)
                {
                    DisplaySalesInfo();
                    lbDays.Focus();
                }
            }
        }

        void lbTills_GotFocus(object sender, EventArgs e)
        {
            if (lbTills.Items.Count > 0)
            {
                lbDays.Focus();
            }
        }

        void lbKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && ((ListBox)sender).SelectedIndex > -1)
            {
                string sDate = lbSalesDate.Items[((ListBox)sender).SelectedIndex].ToString();
                if (sDate != "N/A")
                {
                    sDate = sDate.Replace("/", "");
                    if (!bAlternateEngine)
                    {
                        frmTillTransactions ftt = new frmTillTransactions(ref sEngine, Convert.ToInt32(sTillCodes[lbTills.SelectedIndex]), sDate);
                        ftt.ShowDialog();
                    }
                    else
                    {
                        frmTillTransactions ftt = new frmTillTransactions(ref sOtherEngine, Convert.ToInt32(sTillCodes[lbTills.SelectedIndex]), sDate);
                        ftt.ShowDialog();
                    }
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                if (lbTills.Items.Count > 1)
                {
                    lbTills.Focus();
                    lbDays.SelectedIndex = -1;
                }
                else
                {
                    this.Close();
                }
            }
            else if (e.KeyCode == Keys.F5)
            {
                // Change Date
                frmReportDates frd = new frmReportDates(StockEngine.Period.Weekly);
                frd.ShowDialog();
                if (frd.SelectedFolder != "$NONE")
                {
                    sOtherEngine = new StockEngine(frd.SelectedFolder);
                    bAlternateEngine = true;
                    DisplaySalesInfo();
                    lbDays.Focus();
                }
            }
        }

        void lbSelectedChanged(object sender, EventArgs e)
        {
            lbDays.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbSalesDate.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbTakings.SelectedIndex = ((ListBox)sender).SelectedIndex;
        }

        void DisplaySalesInfo()
        {
            if (lbTills.SelectedIndex > -1)
            {
                lbDays.Items.Clear();
                lbSalesDate.Items.Clear();
                lbTakings.Items.Clear();
                string[] sDays = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
                lbDays.Items.AddRange(sDays);
                for (int i = 0; i < 7; i++)
                {
                    if (!bAlternateEngine)
                        lbSalesDate.Items.Add(sEngine.GetCollectionDate(((i + 1) % 7) + 1, sTillCodes[lbTills.SelectedIndex]));
                    else
                        lbSalesDate.Items.Add(sOtherEngine.GetCollectionDate(((i + 1) % 7) + 1, sTillCodes[lbTills.SelectedIndex]));
                    if (lbSalesDate.Items[i].ToString() != "N/A")
                    {
                        try
                        {
                            if (!bAlternateEngine)
                                lbTakings.Items.Add(FormatMoneyForDisplay(sEngine.GetTakingsForDay(sEngine.GetCollectionDate(((i + 1) % 7) + 1, sTillCodes[lbTills.SelectedIndex]).Replace("/", ""), Convert.ToInt32(sTillCodes[lbTills.SelectedIndex]))));
                            else
                                lbTakings.Items.Add(FormatMoneyForDisplay(sOtherEngine.GetTakingsForDay(sOtherEngine.GetCollectionDate(((i + 1) % 7) + 1, sTillCodes[lbTills.SelectedIndex]).Replace("/", ""), Convert.ToInt32(sTillCodes[lbTills.SelectedIndex]))));
                        }
                        catch
                        {
                            lbTakings.Items.Add("");
                        }
                    }
                    else
                        lbTakings.Items.Add("");

                }
                lbDays.SelectedIndex = 0;
            }
        }

        void lbTills_KeyDown(object sender, KeyEventArgs e)
        {
            DisplaySalesInfo();
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                lbDays.Focus();
            }
        }
    }
}
