using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace GTill
{
    class frmAddToOrder : Form
    {
        TillEngine.TillEngine tEngine;
        public string Barcode;
        Label lblInst;
        TextBox tbBarcode;

        public frmAddToOrder(ref TillEngine.TillEngine te)
        {
            tEngine = te;
            this.BackColor = GTill.Properties.Settings.Default.cFrmBackColour;
            this.ForeColor = GTill.Properties.Settings.Default.cFrmForeColour;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Width = 500;
            this.Height = 100;

            lblInst = new Label();
            lblInst.Location = new Point(10, 10);
            lblInst.AutoSize = true;
            lblInst.BackColor = this.BackColor;
            lblInst.ForeColor = this.ForeColor;
            lblInst.Font = new Font(GTill.Properties.Settings.Default.sFontName, 12.0f);
            lblInst.Text = "Enter the barcode of the item to order (press look up to search) :";
            this.Controls.Add(lblInst);

            tbBarcode = new TextBox();
            tbBarcode.Location = new Point(10, 40);
            tbBarcode.Width = 200;
            tbBarcode.BorderStyle = BorderStyle.None;
            tbBarcode.BackColor = this.ForeColor;
            tbBarcode.ForeColor = this.BackColor;
            tbBarcode.Font = new Font(GTill.Properties.Settings.Default.sFontName, 12.0f);
            this.Controls.Add(tbBarcode);
            tbBarcode.KeyDown += new KeyEventHandler(tbBarcode_KeyDown);
        }

        void tbBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5 || e.KeyCode == Keys.OemQuestion || e.KeyCode == Keys.Divide)
            {
                frmSearchForItemV2 fsfi = new frmSearchForItemV2(ref tEngine);
                fsfi.ShowDialog();
                if (fsfi.GetItemBarcode() != "NONE_SELECTED")
                {
                    tbBarcode.Text = fsfi.GetItemBarcode();
                    Barcode = tbBarcode.Text;
                    tEngine.AddOrderSuggestion(Barcode);
                    this.Close();
                }
            }
            else if (e.KeyCode == Keys.Enter)
            {
                if (tEngine.GetItemRecordContents(Barcode).Length > 1)
                    tEngine.AddOrderSuggestion(Barcode);
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}
