using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmListOfSuppliers : ScalableForm
    {
        StockEngine sEngine;
        CListBox lbCode;
        CListBox lbName;
        public string sSelectedSupplierCode = "NULL";

        public frmListOfSuppliers(ref StockEngine se)
        {
            sEngine = se;
            AddMessage("CODE", "Supplier Code", new Point(10, 10));
            AddMessage("NAME", "Supplier Name", new Point(100, 10));
            AddMessage("EDIT", "Press Insert To Add / Edit A Supplier, Shift+Del to Delete", new Point(10, BelowLastControl));
            this.AllowScaling = false;
            this.SurroundListBoxes = true;
            this.Size = new Size(400, 100);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Text = "Select A Supplier";

            lbCode = new CListBox();
            lbCode.Height = 5;
            lbCode.Location = new Point(10, BelowLastControl);
            lbCode.BorderStyle = BorderStyle.None;
            this.Controls.Add(lbCode);

            lbName = new CListBox();
            lbName.Height = 5;
            lbName.Location = new Point(100, lbCode.Top);
            lbName.BorderStyle = BorderStyle.None;
            this.Controls.Add(lbName);

            string[] sTillCodes = sEngine.GetListOfSuppliers();
            Array.Sort(sTillCodes);
            for (int i = 0; i < sTillCodes.Length; i++)
            {
                string[] sTillData = sEngine.GetSupplierDetails(sTillCodes[i]);
                lbCode.Items.Add(sTillData[0]);
                lbName.Items.Add(sTillData[1]);
                if (this.Height < 700)
                {
                    lbCode.Height += lbCode.ItemHeight;
                    lbName.Height += lbName.ItemHeight;
                    this.Height += lbName.ItemHeight;
                }
            }
            lbCode.Size = new Size(90, this.ClientSize.Height - 10 - lbCode.Top);
            lbName.Size = new Size(this.ClientSize.Width - 10 - lbName.Left, this.ClientSize.Height - 10 - lbName.Top);

            lbName.Focus();
            lbName.SelectedIndexChanged += new EventHandler(lbName_SelectedIndexChanged);
            lbName.KeyDown += new KeyEventHandler(lbName_KeyDown);
            lbCode.KeyDown +=new KeyEventHandler(lbName_KeyDown);
            lbCode.SelectedIndexChanged += new EventHandler(lbCode_SelectedIndexChanged);

            if (lbName.Items.Count >= 1)
                lbName.SelectedIndex = 0;

            this.Text = "Select A Supplier";
        }

        void lbCode_SelectedIndexChanged(object sender, EventArgs e)
        {
            lbName.SelectedIndex = lbCode.SelectedIndex;
        }

        void lbName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                sSelectedSupplierCode = lbCode.Items[lbCode.SelectedIndex].ToString();
                this.Close();
            }
            else if (e.KeyCode == Keys.Insert)
            {
                frmAddSupplier faes = new frmAddSupplier(ref sEngine);
                faes.ShowDialog();
                string[] sTillCodes = sEngine.GetListOfSuppliers();
                Array.Sort(sTillCodes);
                lbCode.Items.Clear();
                lbName.Items.Clear();
                for (int i = 0; i < sTillCodes.Length; i++)
                {
                    string[] sTillData = sEngine.GetSupplierDetails(sTillCodes[i]);
                    lbCode.Items.Add(sTillData[0]);
                    lbName.Items.Add(sTillData[1]);
                }
                lbCode.SelectedIndex = 0;
            }
            else if (e.KeyCode == Keys.Delete && e.Shift)
            {
                sEngine.MarkSupplierAsDeleted(lbCode.Items[lbCode.SelectedIndex].ToString());
                int n = lbCode.SelectedIndex;
                lbCode.Items.RemoveAt(n);
                lbName.Items.RemoveAt(n);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        void lbName_SelectedIndexChanged(object sender, EventArgs e)
        {
            lbCode.SelectedIndex = lbName.SelectedIndex;
        }
    }
}
