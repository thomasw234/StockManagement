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

    public partial class frmErrorCatcher : Form
    {
        public static string AdditionalErrorInformation = null;

        string sErrorDesc = "";

        Random r;

        public enum StoreMethod { StoreLocally, StoreServer };

        public StoreMethod storeMethod = StoreMethod.StoreLocally;

        public frmErrorCatcher(string sErrorDesc)
        {
            InitializeComponent();
            this.sErrorDesc = sErrorDesc;
        }

        private void frmErrorCatcher_Load(object sender, EventArgs e)
        {
        }

        public string UserText
        {
            get
            {
                return textBox1.Text;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            storeMethod = StoreMethod.StoreLocally;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            storeMethod = StoreMethod.StoreServer;
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show(sErrorDesc);
        }
    }
}
