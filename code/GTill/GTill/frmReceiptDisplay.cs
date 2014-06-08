using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GTill
{
    public partial class frmReceiptDisplay : Form
    {
        public frmReceiptDisplay()
        {
            InitializeComponent();
        }

        public void Clear()
        {
            listBox1.Items.Clear();
        }

        public void AddLines(string[] sLines)
        {
            listBox1.Items.AddRange(sLines);
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
        }

        public void AddLine(string sLine)
        {
            listBox1.Items.Add(sLine);
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
        }

        private void frmReceiptDisplay_Load(object sender, EventArgs e)
        {
        }
    }
}
