using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BackOffice
{
    public partial class frmOffersReceptDesigner : Form
    {
        string sBarcode;
        StockEngine sEngine;

        public frmOffersReceptDesigner(string sBarcode, ref StockEngine sEngine)
        {
            InitializeComponent();
            this.sBarcode = sBarcode;
            this.sEngine = sEngine;
            textBox1.Text = sEngine.LoadOffersReceipt(sBarcode);
        }

        private void AddTag(string sTag, int nSelStart)
        {
            textBox1.Text = textBox1.Text.Insert(textBox1.SelectionStart, "<" + sTag + ">" + "</" + sTag + ">");
            textBox1.Focus();
            textBox1.SelectionStart = nSelStart + ("<" + sTag + ">").Length;
            textBox1.SelectionLength = 0;
        }

        private void btnUnderline_Click(object sender, EventArgs e)
        {
            AddTag("Underline", textBox1.SelectionStart);
        }

        private void btnHighlight_Click(object sender, EventArgs e)
        {
            AddTag("Highlight", textBox1.SelectionStart);
        }

        private void btnEmphasised_Click(object sender, EventArgs e)
        {
            AddTag("Emphasised", textBox1.SelectionStart);
        }

        private void btnBarcode_Click(object sender, EventArgs e)
        {
            textBox1.Text = textBox1.Text.Insert(textBox1.SelectionStart, "<Barcode>" + sBarcode + "</Barcode>");
        }

        private void btnDoubleWidth_Click(object sender, EventArgs e)
        {
            AddTag("DoubleWidth", textBox1.SelectionStart);
        }

        private void btnDoubleHeight_Click(object sender, EventArgs e)
        {
            AddTag("DoubleHeight", textBox1.SelectionStart);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            sEngine.SaveOffersReceipt(sBarcode, textBox1.Text);
            this.Close();
        }

        private void btnCentre_Click(object sender, EventArgs e)
        {
            AddTag("Central", textBox1.SelectionStart);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form frmShowPic = new Form();
            frmShowPic.Size = new Size(800, 500);
            frmShowPic.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            frmShowPic.BackgroundImage = Properties.Resources.receiptSample;
            frmShowPic.StartPosition = FormStartPosition.CenterScreen;
            frmShowPic.ShowDialog();
        }
    }
}
