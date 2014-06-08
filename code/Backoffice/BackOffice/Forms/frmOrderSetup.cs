using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;

namespace BackOffice
{
    class frmOrderSetup : ScalableForm
    {
        StockEngine sEngine;
        bool bShopSelectOpen = false;
        bool bFinished = false;
        public bool OrderExists = false;
        public bool OrderIsOk = false;
        Button bOK;
        public bool bRequisitionOrder
        {
            get
            {
                if (InputTextBox("REQ").Text.ToUpper() == "Y")
                    return true;
                else
                    return false;
            }
        }

        public string[] OrderHeaderRecord
        {
            get
            {
                if (bFinished)
                {
                    string[] sHeader = new string[7];
                    sHeader[0] = InputTextBox("ORDERNUM").Text;
                    sHeader[1] = InputTextBox("SUPCODE").Text;
                    sHeader[2] = InputTextBox("SUPREF").Text;
                    sHeader[3] = InputTextBox("NOTES").Text;
                    sHeader[4] = InputTextBox("DUE").Text;
                    sHeader[5] = sEngine.GetDDMMYYDate();
                    sHeader[6] = InputTextBox("SHOPCODE").Text;
                    return sHeader;
                }
                else
                {
                    return new string[0];
                }
            }
        }

        public frmOrderSetup(ref StockEngine se)
        {
            sEngine = se;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.AllowScaling = false;
            this.Size = new System.Drawing.Size(800, 400);
            this.VisibleChanged += new EventHandler(frmOrderSetup_VisibleChanged);
            this.KeyDown += new KeyEventHandler(frmOrderSetup_KeyDown);

            AddMessage("INST", "Please enter header information about the order here :", new Point(10, 20));
            AddInputControl("SHOPCODE", "Shop Code :", new Point(10, BelowLastControl), 250);
            InputTextBox("SHOPCODE").GotFocus += new EventHandler(ShopCodeGotFocus);
            AddInputControl("ORDERNUM", "Order Number :", new Point(10, BelowLastControl), 250, "Previous order was number " + sEngine.LastOrderNumber.ToString() + ", press F5 (or double click in the box) for a list of orders");
            InputTextBox("ORDERNUM").Text = (sEngine.LastOrderNumber + 1).ToString();
            InputTextBox("ORDERNUM").DoubleClick += new EventHandler(frmOrderSetupOrderNum_DoubleClick);
            InputTextBox("ORDERNUM").KeyDown += new KeyEventHandler(OrderNumKeyDown);
            AddInputControl("SUPCODE", "Supplier Code :", new Point(10, BelowLastControl), 250, "Press F5 (or double click in the box) to select a supplier.");
            InputTextBox("SUPCODE").KeyDown += new KeyEventHandler(SupCodeKeyDown);
            InputTextBox("SUPCODE").GotFocus += new EventHandler(SupCodeGotFocus);
            InputTextBox("SUPCODE").AutoCompleteMode = AutoCompleteMode.Append;
            InputTextBox("SUPCODE").AutoCompleteSource = AutoCompleteSource.CustomSource;
            InputTextBox("SUPCODE").AutoCompleteCustomSource.AddRange(sEngine.GetListOfSuppliers());
            InputTextBox("SUPCODE").DoubleClick += new EventHandler(SupplierCodeDoubleClick);
            AddInputControl("SUPREF", "Supplier Ref:", new Point(10, BelowLastControl), 250, "Enter the reference number for this order that the supplier uses");
            AddInputControl("NOTES", "Notes :", new Point(10, BelowLastControl), 350, "Any notes about the order");
            AddInputControl("REQ", "Is this a requisition order?", new Point(10, BelowLastControl), 250, "Y/N");
            InputTextBox("REQ").Text = "N";
            InputTextBox("REQ").GotFocus += new EventHandler(ReqGotFocus);
            AddInputControl("DUE", "Due Date :", new Point(10, BelowLastControl), 250, "The expected delivery date of this order");
            InputTextBox("DUE").KeyDown += new KeyEventHandler(DueKeyDown);
            DateTime dtNow = DateTime.Now;
            dtNow = dtNow.AddDays(7);
            string sDay = dtNow.Day.ToString();
            while (sDay.Length < 2)
                sDay = "0" + sDay;
            string sMonth = dtNow.Month.ToString();
            while (sMonth.Length < 2)
                sMonth = "0" + sMonth;
            string sYear = dtNow.Year.ToString()[2].ToString() + dtNow.Year.ToString()[3].ToString();
            InputTextBox("DUE").Text = sDay + sMonth + sYear;
            AddMessage("RAISED", "Date Raised : " + sEngine.GetDDMMYYDate(), new Point(10, BelowLastControl));

            this.Text = "Order Setup";

            bOK = new Button();
            bOK.Size = new Size(100, 40);
            bOK.Location = new Point(10, this.ClientSize.Height - 10 - bOK.Top - bOK.Height);
            bOK.Text = "OK";
            bOK.Click += new EventHandler(bOK_Click);
            this.Controls.Add(bOK);
            this.AlignInputTextBoxes();
        }

        void frmOrderSetup_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.O && e.Alt)
            {
                bFinished = true;
                this.Close();
            }
        }

        void SupplierCodeDoubleClick(object sender, EventArgs e)
        {
            frmListOfSuppliers flos = new frmListOfSuppliers(ref sEngine);
            flos.ShowDialog();
            if (flos.sSelectedSupplierCode != "NULL")
            {
                InputTextBox("SUPCODE").Text = flos.sSelectedSupplierCode;
                InputTextBox("SUPREF").Focus();
            }
        }

        void frmOrderSetupOrderNum_DoubleClick(object sender, EventArgs e)
        {
            frmListOfOrders flos = new frmListOfOrders(ref sEngine);
            flos.ShowDialog();
            if (flos.OrderNumber != "$NONE")
            {
                InputTextBox("ORDERNUM").Text = flos.OrderNumber;
                if (sEngine.DoesOrderExist(InputTextBox("ORDERNUM").Text))
                {
                    string[] sHeaderData = sEngine.GetOrderHeader(InputTextBox("ORDERNUM").Text);
                    InputTextBox("SUPCODE").Text = sHeaderData[1];
                    InputTextBox("SUPCODE").SelectionStart = InputTextBox("SUPCODE").Text.Length;
                    InputTextBox("SUPREF").Text = sHeaderData[2];
                    InputTextBox("NOTES").Text = sHeaderData[3];
                    InputTextBox("DUE").Text = sHeaderData[4];
                    MessageLabel("RAISED").Text = "Date Raised : " + sHeaderData[5];
                    OrderExists = true;
                    SendKeys.Send("{ENTER}");
                }
                else
                    OrderExists = false;
            }
        }

        void bOK_Click(object sender, EventArgs e)
        {
            bFinished = true;
            this.Close();
        }

        void ReqGotFocus(object sender, EventArgs e)
        {
            InputTextBox("REQ").SelectAll();
        }

        void frmOrderSetup_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible && InputTextBox("ORDERNUM").Text != (sEngine.LastOrderNumber + 1).ToString() && InputTextBox("ORDERNUM").Text != "")
            {
                if (sEngine.DoesOrderExist(InputTextBox("ORDERNUM").Text))
                {
                    string[] sHeaderData = sEngine.GetOrderHeader(InputTextBox("ORDERNUM").Text);
                    InputTextBox("SHOPCODE").Text = sHeaderData[6];
                    InputTextBox("SUPCODE").Text = sHeaderData[1];
                    InputTextBox("SUPCODE").SelectionStart = InputTextBox("SUPCODE").Text.Length;
                    InputTextBox("SUPREF").Text = sHeaderData[2];
                    InputTextBox("NOTES").Text = sHeaderData[3];
                    InputTextBox("DUE").Text = sHeaderData[4];
                    MessageLabel("RAISED").Text = "Date Raised : " + sHeaderData[5];
                    OrderExists = true;
                }
                else
                    OrderExists = false;
                if (!OrderExists)
                {
                    MessageBox.Show("There must be a bug with the software, as the selected order cannot be found!");
                    this.Close();
                }
                else
                {
                    this.Show();
                    if (MessageBox.Show("Is this ok?", "Order Details", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        bFinished = true;
                        OrderIsOk = true;
                        this.Close();
                    }
                    else
                    {
                        this.Close();
                    }
                }
            }
        }

        void SupCodeGotFocus(object sender, EventArgs e)
        {
            InputTextBox("SUPCODE").SelectionStart = InputTextBox("SUPCODE").Text.Length;
        }

        public string OrderNumberToCheck
        {
            set
            {
                InputTextBox("ORDERNUM").Text = value;
            }
        }

        void OrderNumKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (sEngine.DoesOrderExist(InputTextBox("ORDERNUM").Text))
                {
                    string[] sHeaderData = sEngine.GetOrderHeader(InputTextBox("ORDERNUM").Text);
                    InputTextBox("SUPCODE").Text = sHeaderData[1];
                    InputTextBox("SUPCODE").SelectionStart = InputTextBox("SUPCODE").Text.Length;
                    InputTextBox("SUPREF").Text = sHeaderData[2];
                    InputTextBox("NOTES").Text = sHeaderData[3];
                    InputTextBox("DUE").Text = sHeaderData[4];
                    MessageLabel("RAISED").Text = "Date Raised : " + sHeaderData[5];
                    OrderExists = true;
                }
                else
                    OrderExists = false;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
            else if (e.KeyCode == Keys.F5)
            {
                frmListOfOrders flos = new frmListOfOrders(ref sEngine);
                flos.ShowDialog();
                if (flos.OrderNumber != "$NONE")
                {
                    InputTextBox("ORDERNUM").Text = flos.OrderNumber;
                    if (sEngine.DoesOrderExist(InputTextBox("ORDERNUM").Text))
                    {
                        string[] sHeaderData = sEngine.GetOrderHeader(InputTextBox("ORDERNUM").Text);
                        InputTextBox("SUPCODE").Text = sHeaderData[1];
                        InputTextBox("SUPCODE").SelectionStart = InputTextBox("SUPCODE").Text.Length;
                        InputTextBox("SUPREF").Text = sHeaderData[2];
                        InputTextBox("NOTES").Text = sHeaderData[3];
                        InputTextBox("DUE").Text = sHeaderData[4];
                        MessageLabel("RAISED").Text = "Date Raised : " + sHeaderData[5];
                        OrderExists = true;
                        SendKeys.Send("{ENTER}");
                    }
                    else
                        OrderExists = false;
                }
            }
        }

        void DueKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                bFinished = true;
                this.Close();
            }
        }

        void SupCodeKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                frmListOfSuppliers flos = new frmListOfSuppliers(ref sEngine);
                flos.ShowDialog();
                if (flos.sSelectedSupplierCode != "NULL")
                {
                    InputTextBox("SUPCODE").Text = flos.sSelectedSupplierCode;
                    InputTextBox("SUPREF").Focus();
                }
            }
            else if (e.KeyCode == Keys.Enter)
            {
                if (sEngine.GetSupplierDetails(InputTextBox("SUPCODE").Text)[0] == null && InputTextBox("SUPCODE").Text != "")
                {
                    if (MessageBox.Show("Supplier doesn't exist, would you like to add it?", "Add Supplier?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        frmAddSupplier fas = new frmAddSupplier(ref sEngine);
                        fas.ShowDialog();
                    }
                    InputTextBox("SUPCODE").Focus();
                }
                else // THe supplier is recognised, a new order may be created, or a previous one opened
                {
                    // Check to see if the user is trying to use the shift&enter short
                    if (e.Shift)
                    {
                        bFinished = true;
                        this.Close();
                    }
                }
            }
        }

        void ShopCodeGotFocus(object sender, EventArgs e)
        {
            frmListOfShops flos = new frmListOfShops(ref sEngine);
            while (flos.SelectedShopCode == "$NONE" && !bShopSelectOpen)
            {
                bShopSelectOpen = true;
                flos.ShowDialog();
            }
            InputTextBox("SHOPCODE").Text = flos.SelectedShopCode;
            InputTextBox("ORDERNUM").Focus();
        }
    }
}
