using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Text;

namespace BackOffice
{
    class frmListOfSales : ScalableForm
    {
        StockEngine sEngine;
        CListBox lbSales;
        string[] sSalesDate;
        string[] sTranNo;
        string sBarcode;

        public frmListOfSales(ref StockEngine sEngine, string sBarcode)
        {
            this.sEngine = sEngine;
            this.AllowScaling = false;
            this.sBarcode = sBarcode;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Size = new System.Drawing.Size(275, 420);
            this.SurroundListBoxes = true;

            AddMessage("INST", "Sales Date  -  Transaction Number", new System.Drawing.Point(10, 10));

            lbSales = new CListBox();
            lbSales.Location = new System.Drawing.Point(10, BelowLastControl);
            lbSales.Size = new System.Drawing.Size(this.ClientSize.Width - 20, 300);
            lbSales.BorderStyle = BorderStyle.None;
            this.Controls.Add(lbSales);

            AddMessage("NOTE", "Only sales before this week are shown", new System.Drawing.Point(10, 340));
            AddMessage("NOTE2", "Press Enter to view the transaction", new System.Drawing.Point(10, 360));

            sTranNo = new string[0];
            sSalesDate = sEngine.GetWeeksOfItemSold(sBarcode, ref sTranNo);

            frmProgressBar fp = new frmProgressBar("Getting Sales Dates");
            fp.pb.Maximum = sSalesDate.Length;
            fp.Show();
            for (int i = 0; i < sSalesDate.Length; i++)
            {
                fp.pb.Value = i;
                string sDay = sEngine.WorkOutDateOfSale(sTranNo[i], sSalesDate[i]);
                BackOffice.Database_Engine.Table t = new BackOffice.Database_Engine.Table("Archive\\Weekly\\" + sSalesDate[i] + "\\TILL1\\INGNG\\REPDATA" + sDay + ".DBF");
                string sDate = t.GetRecordFrom(0)[1];
                lbSales.Items.Add(sDate + " - " + sTranNo[i]);
            }
            if (lbSales.Items.Count > 0)
                lbSales.SelectedIndex = 0;
            fp.Close();
            lbSales.KeyDown += new KeyEventHandler(lbSales_KeyDown);
        }

        void lbSales_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                if (lbSales.Items.Count > 0)
                {
                    string sDay = sEngine.WorkOutDateOfSale(sTranNo[lbSales.SelectedIndex], sSalesDate[lbSales.SelectedIndex]);
                    BackOffice.Database_Engine.Table t = new BackOffice.Database_Engine.Table("Archive\\Weekly\\" + sSalesDate[lbSales.SelectedIndex] + "\\TILL1\\INGNG\\REPDATA" + sDay + ".DBF");
                    string sDate = t.GetRecordFrom(0)[1];
                    if (sDay == "ERROR")
                    {
                        throw new NotImplementedException("Barcode : " + sBarcode + ", TranNo : " + sTranNo[lbSales.SelectedIndex]);
                    }
                    StockEngine s = new StockEngine("Archive\\Weekly\\" + sSalesDate[lbSales.SelectedIndex]);
                    frmTillTransactions ftt = new frmTillTransactions(ref s, 1, sDate.Replace("/", ""));
                    ftt.Show();
                    ftt.SelectATransaction(sTranNo[lbSales.SelectedIndex]);
                }
            }
        }


    }
}
