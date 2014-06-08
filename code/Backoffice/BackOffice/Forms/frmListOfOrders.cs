using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmListOfOrders : ScalableForm
    {
        StockEngine sEngine;

        CListBox lbOrderNum;
        CListBox lbSupplierName;
        CListBox lbOrderDate;
        Button btnAddOrder;
        public string OrderNumber = "$NONE";
        public bool bAllowAnEmptyOrder = false;

        public frmListOfOrders(ref StockEngine se)
        {
            sEngine = se;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.AllowScaling = false;
            this.SurroundListBoxes = true;
            this.Size = new System.Drawing.Size(500, 520);

            lbOrderNum = new CListBox();
            lbOrderNum.Location = new Point(10, 10);
            lbOrderNum.Size = new Size(140, 400);
            lbOrderNum.BorderStyle = BorderStyle.None;
            lbOrderNum.KeyDown += new KeyEventHandler(lbOrderNum_KeyDown);
            lbOrderNum.SelectedIndexChanged += new EventHandler(lbOrderNum_SelectedIndexChanged);
            this.Controls.Add(lbOrderNum);

            lbSupplierName = new CListBox();
            lbSupplierName.Location = new Point(150, 10);
            lbSupplierName.Size = new Size(240, 400);
            lbSupplierName.BorderStyle = BorderStyle.None;
            lbSupplierName.KeyDown +=new KeyEventHandler(lbOrderNum_KeyDown);
            lbSupplierName.SelectedIndexChanged +=new EventHandler(lbOrderNum_SelectedIndexChanged);
            this.Controls.Add(lbSupplierName);

            lbOrderDate = new CListBox();
            lbOrderDate.Location = new Point(390, 10);
            lbOrderDate.Size = new Size(this.ClientSize.Width - 10 - lbOrderDate.Left, 400);
            lbOrderDate.BorderStyle = BorderStyle.None;
            lbOrderDate.KeyDown +=new KeyEventHandler(lbOrderNum_KeyDown);
            lbOrderDate.SelectedIndexChanged +=new EventHandler(lbOrderNum_SelectedIndexChanged);
            this.Controls.Add(lbOrderDate);

            btnAddOrder = new Button();
            btnAddOrder.Size = new Size(200, 50);
            btnAddOrder.Location = new Point(10, this.ClientSize.Height - btnAddOrder.Height - 10);
            btnAddOrder.Text = "Add Empty Order : Press Insert";
            btnAddOrder.Click += new EventHandler(btnAddOrder_Click);

            LoadOrders();

            this.Text = "Select An Order";
        }

        void LoadOrders()
        {
            string[] sOrders = sEngine.GetListOfOrderNumbers();
            for (int i = 0; i < sOrders.Length; i++)
            {
                lbOrderNum.Items.Add(sOrders[i]);
                if (sEngine.GetSupplierDetails(sEngine.GetOrderHeader(sOrders[i])[1])[0] != null)
                {
                    lbSupplierName.Items.Add(sEngine.GetSupplierDetails(sEngine.GetOrderHeader(sOrders[i])[1])[1]);
                }
                else
                {
                    lbSupplierName.Items.Add("");
                }
                string[] SupplierRecord = sEngine.GetOrderHeader(sOrders[i]);
                string sDate = SupplierRecord[5][0].ToString() + SupplierRecord[5][1].ToString() + "/" + SupplierRecord[5][2].ToString() + SupplierRecord[5][3].ToString() + "/" + SupplierRecord[5][4].ToString() + SupplierRecord[5][5].ToString();
                lbOrderDate.Items.Add(sDate);
            }
            lbOrderNum.SelectedIndex = lbOrderNum.Items.Count - 1;
        }

        void btnAddOrder_Click(object sender, EventArgs e)
        {
            AddEmptyOrder();
        }

        private bool AddEmptyOrder()
        {
            frmOrderSetup fOS = new frmOrderSetup(ref sEngine);
            fOS.ShowDialog();
            if (fOS.OrderHeaderRecord.Length != 0)
            {
                if (MessageBox.Show("Add this empty order?", "Empty Order", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    sEngine.AddEditOrderHeader(fOS.OrderHeaderRecord);
                }
                lbOrderNum.Items.Clear();
                lbOrderDate.Items.Clear();
                lbSupplierName.Items.Clear();
                LoadOrders();
                lbOrderNum.Focus();
                return true;
            }
            else
            {
                return false;
            }
        }


        public bool AllowAnEmptyOrder
        {
            get
            {
                return bAllowAnEmptyOrder;
            }
            set
            {
                bAllowAnEmptyOrder = value;
                if (bAllowAnEmptyOrder)
                    this.Controls.Add(btnAddOrder);
            }
        }

        void lbOrderNum_SelectedIndexChanged(object sender, EventArgs e)
        {
            lbOrderNum.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbSupplierName.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbOrderDate.SelectedIndex = ((ListBox)sender).SelectedIndex;
        }

        void lbOrderNum_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                OrderNumber = lbOrderNum.Items[lbOrderNum.SelectedIndex].ToString();
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
                this.Close();
            else if (e.KeyCode == Keys.Insert)
                AddEmptyOrder();

        }
    }
}
