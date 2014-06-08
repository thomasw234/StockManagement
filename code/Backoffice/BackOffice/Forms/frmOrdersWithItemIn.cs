using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmOrdersWithItemIn : ScalableForm
    {
        StockEngine sEngine;
        CListBox lbOrderNum;
        CListBox lbSupCode;
        CListBox lbSupName;
        CListBox lbQtyOnOrder;

        public frmOrdersWithItemIn(ref StockEngine se, string sBarcode)
        {
            sEngine = se;
            this.AllowScaling = false;
            this.Size = new Size(500, 200);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.KeyDown += new KeyEventHandler(frmOrdersWithItemIn_KeyDown);
            this.SurroundListBoxes = true;

            lbOrderNum = new CListBox();
            lbOrderNum.Location = new Point(10, 40);
            lbOrderNum.Size = new Size(75, this.ClientSize.Height - 10 - lbOrderNum.Top);
            lbOrderNum.BorderStyle = BorderStyle.None;
            lbOrderNum.KeyDown += new KeyEventHandler(lbOrderNum_KeyDown);
            lbOrderNum.SelectedIndexChanged += new EventHandler(lbOrderNum_SelectedIndexChanged);
            this.Controls.Add(lbOrderNum);
            AddMessage("ORDNUM", "Order Num", new Point(10, 10));

            lbSupCode = new CListBox();
            lbSupCode.Location = new Point(lbOrderNum.Left + lbOrderNum.Width, 40);
            lbSupCode.Size = new Size(100, this.ClientSize.Height - 10 - lbOrderNum.Top);
            lbSupCode.BorderStyle = BorderStyle.None;
            lbSupCode.KeyDown +=new KeyEventHandler(lbOrderNum_KeyDown);
            lbSupCode.SelectedIndexChanged += new EventHandler(lbOrderNum_SelectedIndexChanged);
            this.Controls.Add(lbSupCode);
            AddMessage("SUPCODE", "Sup Code", new Point(lbSupCode.Left, 10));

            lbSupName = new CListBox();
            lbSupName.Location = new Point(lbSupCode.Left + lbSupCode.Width, 40);
            lbSupName.Size = new Size(200, this.ClientSize.Height - 10 - lbOrderNum.Top);
            lbSupName.BorderStyle = BorderStyle.None;
            lbSupName.KeyDown += new KeyEventHandler(lbOrderNum_KeyDown);
            lbSupName.SelectedIndexChanged += new EventHandler(lbOrderNum_SelectedIndexChanged);
            this.Controls.Add(lbSupName);
            AddMessage("SUPNAME", "Supplier Name", new Point(lbSupName.Left, 10));

            lbQtyOnOrder = new CListBox();
            lbQtyOnOrder.Location = new Point(lbSupName.Left + lbSupName.Width, 40);
            lbQtyOnOrder.Size = new Size(this.ClientSize.Width - 10 - lbQtyOnOrder.Left, this.ClientSize.Height - 10 - lbOrderNum.Top);
            lbQtyOnOrder.BorderStyle = BorderStyle.None;
            lbQtyOnOrder.KeyDown += new KeyEventHandler(lbOrderNum_KeyDown);
            lbQtyOnOrder.SelectedIndexChanged += new EventHandler(lbOrderNum_SelectedIndexChanged);
            this.Controls.Add(lbQtyOnOrder);
            AddMessage("QTY", "Outstanding", new Point(lbQtyOnOrder.Left, 10));

            string[] sOrderNums = new string[0];
            string[] sSupCodes = new string[0];
            string[] sQuantities = new string[0];
            sEngine.GetOrdersWithItemOutstandingIn(sBarcode, ref sOrderNums, ref sSupCodes, ref sQuantities);

            lbOrderNum.Items.AddRange(sOrderNums);
            lbSupCode.Items.AddRange(sSupCodes);
            lbQtyOnOrder.Items.AddRange(sQuantities);
            for (int i = 0; i < sSupCodes.Length; i++)
            {
                lbSupName.Items.Add(sEngine.GetSupplierDetails(sSupCodes[i])[1]);
            }
            if (lbSupName.Items.Count > 0)
                lbSupName.SelectedIndex = 0;

            this.Text = "Orders With Item Outstanding";
        }

        void frmOrdersWithItemIn_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close();
        }

        void lbOrderNum_SelectedIndexChanged(object sender, EventArgs e)
        {
            lbOrderNum.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbQtyOnOrder.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbSupCode.SelectedIndex = ((ListBox)sender).SelectedIndex;
            lbSupName.SelectedIndex = ((ListBox)sender).SelectedIndex;
        
        }

        void lbOrderNum_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close();
            else if (e.KeyCode == Keys.Enter)
            {
                if (lbOrderNum.SelectedIndex >= 0)
                {
                    frmAddOrder fao = new frmAddOrder(ref sEngine, lbOrderNum.Items[lbOrderNum.SelectedIndex].ToString());
                    fao.ShowDialog();
                }
                else
                    this.Close();
            }
        }
    }
}
