using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Text;

namespace BackOffice
{
    class frmAddEditItem : ScalableForm
    {
        StockEngine sEngine;
        bool bItemTypeSelectOpen = false;
        bool bCategorySelectOpen = false;
        bool bVATSelectOpen = false;
        bool bSelectShopOpen = false;
        decimal dCatTwoItemCost = 0;
        decimal dCatTwoItemMargin = 0;
        TextBox[] tbSupCode;
        TextBox[] tbSupCost;
        TextBox[] tbSupProdCode;
        string sParentCode = "";
        decimal dChildPart = 0;
        bool bEditingItem = false;
        string sCurrentBarcodeShowing = "";
        decimal dOriginalPrice = 0;
        decimal dOriginalStockLevel = 0;
        bool bAskedAboutClosing = false;
        int nOriginalItemType = 1;
        Button bSave;

        public string AddingBarcode
        {
            set
            {
                if (value == null)
                    return;
                EditingItem = false;
                InputTextBox("BARCODE").Text = value;
                InputTextBox("DESCRIPTION").Focus();
            }
        }

        public string EditingBarcode
        {
            set
            {
                if (value == null)
                    return;
                EditingItem = true;
                InputTextBox("BARCODE").Text = value;
            }
        }

        public bool EditingItem
        {
            get
            {

                return bEditingItem;
            }
            set
            {
                bEditingItem = value;
                if (bEditingItem)
                {
                    this.Text = "Edit Item";
                }
                else
                {
                    this.Text = "Add Item";
                }
            }
        }

        public frmAddEditItem(ref StockEngine se)
        {
            sEngine = se;
            AllowScaling = false;
            SetupForm();
            //this.WindowState = FormWindowState.Maximized;
            this.Size = new Size(725, 740);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Text = "Add / Edit Item";
            this.FormClosing += new FormClosingEventHandler(frmAddEditItem_FormClosing);
            this.KeyDown += new KeyEventHandler(frmAddEditItem_KeyDown);
        }

        void frmAddEditItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.S && e.Alt)
            {

                if (!CheckAllFieldsAreAcceptable())
                {
                    MessageBox.Show("One or more fields are invalid, please correct this", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    AddItem();
                    bAskedAboutClosing = true;
                    this.Close();
                }

            }

        }

        void frmAddEditItem_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!bAskedAboutClosing)
            {
                EscPressed();
            }
        }

        public void SetupForm()
        {
            AddInputControl("SHOPCODE", "Shop Code : ", new Point(10, 25), 200);
            InputTextBox("SHOPCODE").GotFocus += new EventHandler(frmAddEditItemShopCode_GotFocus);
            if (!bEditingItem)
                AddInputControl("BARCODE", "Item's Barcode : ", new Point(10, BelowLastControl), 300, "Enter the barcode of the new item (F6 for auto-barcode)");
            else
                AddInputControl("BARCODE", "Item's Barcode : ", new Point(10, BelowLastControl), 300, "Enter the barcode of the existing item");
            InputTextBox("BARCODE").MaxCharCount = 13;
            InputTextBox("BARCODE").KeyDown += new KeyEventHandler(tbBarcodeKeyDown);
            InputTextBox("BARCODE").TextChanged += new EventHandler(tbBarcodeTextChanged);
            AddInputControl("DESCRIPTION", "Item's Description : ", new Point(10, BelowLastControl), 300, "Enter a description of the item");
            InputTextBox("DESCRIPTION").MaxCharCount = 30;
            AddInputControl("TYPE", "Item's Type : ", new Point(10, BelowLastControl), 300, "Select the item's type");
            InputTextBox("TYPE").GotFocus += new EventHandler(frmAddEditItemTypeBox_GotFocus);
            InputTextBox("TYPE").KeyDown += new KeyEventHandler(tbTypeKeyDown);
            AddInputControl("CATEGORY", "Item's Category : ", new Point(10, BelowLastControl), 300, "Select the item's category");
            InputTextBox("CATEGORY").GotFocus += new EventHandler(frmAddEditItemCategoryBox_GotFocus);
            InputTextBox("CATEGORY").KeyDown += new KeyEventHandler(tbCategoryKeyDown);
            AddInputControl("£RRP", "Item's Price : ", new Point(10, BelowLastControl), 300, "Enter the retail price of the item");
            InputTextBox("£RRP").KeyDown += new KeyEventHandler(RRPKeyDown);
            AddInputControl("VAT", "Item's V.A.T Type : ", new Point(10, BelowLastControl), 300, "Select the item's V.A.T rate");
            InputTextBox("VAT").GotFocus += new EventHandler(frmAddEditItemVatBox_GotFocus);
            AddInputControl("QIS", "Stock Level : ", new Point(10, BelowLastControl), 300, "Enter the stock level for the item");
            AddInputControl("AVECOST", "Average Cost : ", new Point(10, BelowLastControl), 300, "Enter the average cost of this item");
            InputTextBox("AVECOST").KeyDown += new KeyEventHandler(AveCost_KeyDown);
            AddInputControl("MINQTY", "Minimum Order : ", new Point(10, BelowLastControl), 300, "What's the minimum that you can order? Numerical values only!");
            InputTextBox("MINQTY").KeyDown += new KeyEventHandler(frmAddEditItemMinQTY_KeyDown);
            InputTextBox("MINQTY").MaxCharCount = 6;

            AlignInputTextBoxes();

            AddMessage("SUPCODE", "Suppliers' Codes", new Point(10, BelowLastControl));
            AddMessage("SUPNAME", "Suppliers' Names", new Point(120, lmArray[lmArray.Length - 1].lblMessage.Top));
            AddMessage("SUPCOST", "Suppliers' Costs", new Point(420, lmArray[lmArray.Length - 1].lblMessage.Top));
            AddMessage("SUPREF", "Product Code", new Point(530, lmArray[lmArray.Length - 1].lblMessage.Top));

            tbSupCode = new TextBox[0];
            tbSupCost = new TextBox[0];
            tbSupProdCode = new TextBox[0];

            for (int i = 0; i < ibArray.Length; i++)
            {
                ibArray[i].tbInput.KeyDown += new KeyEventHandler(AllTextboxKeyDown);
            }

            bSave = new Button();
            bSave.Location = new Point(10, this.Height - 100);
            bSave.Size = new Size(100, 30);
            bSave.Text = "Save";
            this.Controls.Add(bSave);
            bSave.Click += new EventHandler(bSave_Click);
        }

        /// <summary>
        /// Occurs when the text changes in the barcode field
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Any arguments</param>
        void tbBarcodeTextChanged(object sender, EventArgs e)
        {
            // If a code has been pasted in that's longer than 13 characters, just trim it down to 13 characters
            if (InputTextBox("BARCODE").Text.Length > 13)
            {
                InputTextBox("BARCODE").Text = InputTextBox("BARCODE").Text.Substring(0, 13);
                MessageBox.Show("The barcode that you entered was too long, and has been automatically trimmed to 13 characters.", "Barcode Too Long", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        void AveCost_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab)
            {
                try
                {
                    FormatMoneyForDisplay(InputTextBox("AVECOST").Text);
                }
                catch
                {
                    MessageBox.Show("Invalid average cost entered, please try again");
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                }
            }
        }

        void tbTypeKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
            }
        }

        void bSave_Click(object sender, EventArgs e)
        {

            if (!CheckAllFieldsAreAcceptable())
            {
                MessageBox.Show("One or more fields are invalid, please correct this", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            {
                AddItem();
                bAskedAboutClosing = true;
                this.Close();
            }
        }

        void RRPKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (((CTextBox)sender).Text != "")
                {
                    try
                    {
                        ((CTextBox)sender).Text = FormatMoneyForDisplay(((CTextBox)sender).Text);
                    }
                    catch
                    {
                        MessageBox.Show("Invalid amount entered!");
                        ((CTextBox)sender).Focus();
                        e.SuppressKeyPress = true;
                    }
                }
                else
                {
                    InputTextBox("£RRP").Focus();
                    e.SuppressKeyPress = true;
                }
            }
        }

        void AllTextboxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                EscPressed();
        }

        void tbCategoryKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                GetCategory();
            }
            else if (e.KeyCode == Keys.Enter && InputTextBox("TYPE").Text == "2")
            {
                InputTextBox("VAT").Focus();
            }
        }

        void GetCategory()
        {
            if (InputTextBox("TYPE").Text != "5")
            {
                frmCategorySelect fCatSelect = new frmCategorySelect(ref sEngine);
                bCategorySelectOpen = true;
                fCatSelect.ShowDialog();
                if (fCatSelect.SelectedItemCategory != "$NULL")
                {
                    InputTextBox("CATEGORY").Text = fCatSelect.SelectedItemCategory;
                    InputTextBox("£RRP").Focus();
                }
                else
                {
                    InputTextBox("TYPE").Focus();
                }
                fCatSelect.Close();
                bCategorySelectOpen = false;
                fCatSelect.Dispose();
            }
            else if (InputTextBox("TYPE").Text == "2")
            {
                InputTextBox("£RRP").Text = "0.00";
                InputTextBox("VAT").Focus();
            }
            else
            {
                InputTextBox("CATEGORY").Text = sEngine.GetMainStockInfo(sParentCode)[4];
                InputTextBox("£RRP").Focus();
            }
        }

        bool bCont = true;
        void GetParentInfo()
        {
            bCont = true;
            if (InputTextBox("TYPE").Text == "5")
            {
                string sToAsk = "Enter the barcode of the parent item (F5 to search for the parent)";
                if (sParentCode != null && sParentCode != "")
                {
                    sToAsk = "Enter the barcode of the parent item (currently " + sEngine.GetMainStockInfo(sParentCode)[1] + ")";
                }
                frmSingleInputBox fsi = new frmSingleInputBox(sToAsk, ref sEngine);
                fsi.tbResponse.Text = sParentCode;
                fsi.ShopCode = InputTextBox("SHOPCODE").Text;
                fsi.ShowDialog();
                if (sEngine.GetMainStockInfo(fsi.Response).Length <= 1 && (fsi.Response == "$NONE" && fsi.Response != "") || sEngine.GetMainStockInfo(fsi.Response)[5] != "1")
                {
                    bCont = false;
                    InputTextBox("TYPE").Focus();
                    if (sEngine.GetMainStockInfo(fsi.Response).Length > 1 && sEngine.GetMainStockInfo(fsi.Response)[5] != "1")
                    {
                        MessageBox.Show("You can't use that barcode as it's not a child item.");
                        InputTextBox("DESCRIPTION").Focus();
                    }

                }
                else
                {
                    sParentCode = fsi.Response;
                    frmSingleInputBox fsPart = new frmSingleInputBox("How many of this item make up the parent item?", ref sEngine);
                    if (dChildPart == 0)
                        dChildPart = 1;
                    fsPart.tbResponse.Text = FormatMoneyForDisplay(dChildPart);
                    fsPart.ShowDialog();
                    if (fsPart.Response == "$NONE")
                    {
                        InputTextBox("TYPE").Focus();
                    }
                    else
                    {
                        try
                        {
                            dChildPart = Convert.ToDecimal(fsPart.Response);
                        }
                        catch
                        {
                            MessageBox.Show("Error! Invalid input for child part info.");
                            InputTextBox("TYPE").Focus();
                        }
                    }
                }
            }
            if (InputTextBox("TYPE").Text != "2" && bCont)
            {
                InputTextBox("£RRP").Focus();
            }
            else
            {
                if (bCont)
                {
                    InputTextBox("£RRP").Text = "0.00";
                    //InputTextBox("VAT").Focus();
                }
            }
        }

        void RemoveAllSupBoxes()
        {
            for (int i = 0; i < tbSupCode.Length; i++)
            {
                RemoveMessage(tbSupCode[i].Text);
                this.Controls.Remove(tbSupCode[i]);
                this.Controls.Remove(tbSupCost[i]);
                this.Controls.Remove(tbSupProdCode[i]);
            }
            tbSupCode = new TextBox[0];
            tbSupCost = new TextBox[0];
            tbSupProdCode = new TextBox[0];
        }

        void EmptyAllTextBoxes()
        {
            InputTextBox("DESCRIPTION").Text = "";
            InputTextBox("TYPE").Text = "";
            InputTextBox("CATEGORY").Text = "";
            InputTextBox("£RRP").Text = "";
            InputTextBox("VAT").Text = "";
            InputTextBox("MINQTY").Text = "";
            dCatTwoItemCost = 0;
            dChildPart = 0;
            sParentCode = "";
        }

        void ReturnBarcodeBoxPressed()
        {
            if (InputTextBox("BARCODE").Text == "")
            {
                bAskedAboutClosing = true;
                this.Close();
            }
            else
            {
                if (sEngine.DoesMultiItemExist(InputTextBox("BARCODE").Text, InputTextBox("SHOPCODE").Text))
                {
                    frmAddMultiItem fami = new frmAddMultiItem(ref sEngine);
                    fami.Barcode = InputTextBox("BARCODE").Text;
                    fami.ShowDialog();
                    bAskedAboutClosing = true;
                    this.Close();
                }
                else
                {
                    // Search to see if item exists
                    string[] sStockSta = sEngine.GetItemStockStaRecord(InputTextBox("BARCODE").Text, InputTextBox("SHOPCODE").Text);
                    if ((sStockSta.Length > 1 && bEditingItem) || (sStockSta.Length > 1 && !bEditingItem))
                    {
                        if (bEditingItem || MessageBox.Show("Product alredy exists! Would you like to edit it?", "Error Adding Item", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            EditingItem = true;
                            RemoveAllSupBoxes();
                            EmptyAllTextBoxes();
                            string[] sMainStock = sEngine.GetMainStockInfo(InputTextBox("BARCODE").Text);
                            InputTextBox("DESCRIPTION").Text = sMainStock[1];
                            InputTextBox("TYPE").Text = sMainStock[5];
                            nOriginalItemType = Convert.ToInt32(sMainStock[5]);
                            InputTextBox("CATEGORY").Text = sMainStock[4];
                            InputTextBox("£RRP").Text = sMainStock[2];
                            dOriginalPrice = Convert.ToDecimal(sMainStock[2]);
                            InputTextBox("VAT").Text = sMainStock[3];
                            InputTextBox("QIS").Text = sStockSta[36];
                            dOriginalStockLevel = Convert.ToDecimal(sStockSta[36]);
                            InputTextBox("AVECOST").Text = FormatMoneyForDisplay(sStockSta[1]);
                            InputTextBox("MINQTY").Text = sStockSta[37];
                            dChildPart = Convert.ToDecimal(sStockSta[38]);
                            sParentCode = sMainStock[7];
                            dCatTwoItemCost = Convert.ToDecimal(sStockSta[1]);
                            dCatTwoItemMargin = Convert.ToDecimal(sStockSta[39]);
                            sCurrentBarcodeShowing = InputTextBox("BARCODE").Text;
                            int nOfSuppliers = 0;
                            if (sMainStock[5] != "6")
                            {
                                string[,] sSuppliers = sEngine.GetListOfSuppliersForItem(sMainStock[0], ref nOfSuppliers);
                                for (int i = 0; i < nOfSuppliers; i++)
                                {
                                    AddSupplierBoxes();
                                    tbSupCode[tbSupCode.Length - 1].Text = sSuppliers[i, 1];
                                    tbSupCost[tbSupCost.Length - 1].Text = FormatMoneyForDisplay(sSuppliers[i, 3]);
                                    tbSupProdCode[tbSupProdCode.Length - 1].Text = sSuppliers[i, 2];
                                }
                            }
                            else
                            {
                                string[] sComPerson = sEngine.GetComissionerRecordOfItem(InputTextBox("BARCODE").Text);
                                AddSupplierBoxes();
                                // If no items have been recevied, then sComPerson will be empty, so don't try and fill in data
                                if (sComPerson.Length > 1)
                                {
                                    tbSupCode[0].Text = sComPerson[0];
                                    tbSupCost[0].Text = FormatMoneyForDisplay(sMainStock[8]);
                                    tbSupProdCode[0].Text = sComPerson[1];
                                }
                            
                            }
                            InputTextBox("DESCRIPTION").Focus();
                        }
                        else
                        {
                            InputTextBox("BARCODE").Text = "";
                            InputTextBox("BARCODE").Focus();
                        }
                    }
                    else if (sStockSta.Length <= 1 && bEditingItem)
                    {
                        if (MessageBox.Show("Item doesn't exist to edit! Would you like to add the item instead?", "Error editing item", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            InputTextBox("DESCRIPTION").Focus();
                            EditingItem = false;
                        }
                        else
                        {
                            InputTextBox("BARCODE").Text = "";
                            InputTextBox("BARCODE").Focus();
                        }
                    }
                }
            }
        }

        void tbBarcodeKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ReturnBarcodeBoxPressed();
            }
            else if (e.KeyCode == Keys.F5)
            {
                frmSearchForItemV2 fsfi = new frmSearchForItemV2(ref sEngine);
                fsfi.ShowDialog();
                if (fsfi.GetItemBarcode() != "NONE_SELECTED")
                {
                    InputTextBox("BARCODE").Text = fsfi.GetItemBarcode();
                    ReturnBarcodeBoxPressed();
                }
            }
            else if (e.KeyCode == Keys.F6 && !bEditingItem)
            {
                ((CTextBox)sender).Text = sEngine.GetNextAutoBarcode();
                InputTextBox("DESCRIPTION").Focus();
            }
            else if (e.KeyCode == Keys.F6 && bEditingItem)
            {
                frmSearchForItemV2 fsfi = new frmSearchForItemV2(ref sEngine);
                fsfi.SetSearchTerm("CAT:" + sEngine.LastCategoryCode);
                fsfi.ShowDialog();
                if (fsfi.GetItemBarcode() != "NONE_SELECTED")
                {
                    InputTextBox("BARCODE").Text = fsfi.GetItemBarcode();
                }
            }
        }

        void frmAddEditItemShopCode_GotFocus(object sender, EventArgs e)
        {
            if (!bSelectShopOpen)
            {
                frmListOfShops flos = new frmListOfShops(ref sEngine);
                bSelectShopOpen = true;
                flos.ShowDialog();
                if (flos.SelectedShopCode != "$NONE")
                {
                    InputTextBox("SHOPCODE").Text = flos.SelectedShopCode;
                    AddMessage("SHOPNAME", sEngine.GetShopNameFromCode(flos.SelectedShopCode), new Point(ibArray[0].tbInput.Left + ibArray[0].tbInput.Width + 10, ibArray[0].tbInput.Top));
                    InputTextBox("BARCODE").Focus();
                }
                else
                {
                    bAskedAboutClosing = true;
                    this.Close();
                }
            }
        }

        void frmAddEditItemMinQTY_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (tbSupCode.Length == 0)
                {
                    AddSupplierBoxes();
                }
                tbSupCode[0].Focus();

            }
        }

        void AddSupplierBoxes()
        {
            Array.Resize<TextBox>(ref tbSupCode, tbSupCode.Length + 1);
            tbSupCode[tbSupCode.Length - 1] = new TextBox();
            if (tbSupCode.Length == 1)
            {
                tbSupCode[tbSupCode.Length - 1].Location = new Point(10, MessageLabel("SUPCODE").Top + MessageLabel("SUPCODE").Height + 10);
            }
            else
                tbSupCode[tbSupCode.Length - 1].Location = new Point(10, BelowLastControl);
            tbSupCode[tbSupCode.Length - 1].Size = new Size(100, Convert.ToInt32(this.Font.GetHeight()));
            this.Controls.Add(tbSupCode[tbSupCode.Length - 1]);
            tbSupCode[tbSupCode.Length - 1].Tag = tbSupCode.Length - 1;
            tbSupCode[tbSupCode.Length - 1].KeyDown += new KeyEventHandler(tbSupCodeKeyDown);
            tbSupCode[tbSupCode.Length - 1].Focus();
            tbSupCode[tbSupCode.Length - 1].AutoCompleteSource = AutoCompleteSource.CustomSource;
            tbSupCode[tbSupCode.Length - 1].AutoCompleteMode = AutoCompleteMode.Append;
            tbSupCode[tbSupCode.Length - 1].AutoCompleteCustomSource.AddRange(sEngine.GetListOfSuppliers());

            Array.Resize<TextBox>(ref tbSupCost, tbSupCost.Length + 1);
            tbSupCost[tbSupCost.Length - 1] = new TextBox();
            tbSupCost[tbSupCost.Length - 1].Location = new Point(420, tbSupCode[tbSupCost.Length - 1].Top);
            tbSupCost[tbSupCost.Length - 1].Size = new Size(100, Convert.ToInt32(this.Font.GetHeight()));
            this.Controls.Add(tbSupCost[tbSupCost.Length - 1]);
            tbSupCost[tbSupCost.Length - 1].Tag = tbSupCost.Length - 1;
            tbSupCost[tbSupCost.Length - 1].Text = InputTextBox("AVECOST").Text;
            tbSupCost[tbSupCost.Length - 1].KeyDown += new KeyEventHandler(tbSupCostKeyDown);

            Array.Resize<TextBox>(ref tbSupProdCode, tbSupProdCode.Length + 1);
            tbSupProdCode[tbSupProdCode.Length - 1] = new TextBox();
            tbSupProdCode[tbSupProdCode.Length - 1].Location = new Point(530, tbSupCost[tbSupProdCode.Length - 1].Top);
            tbSupProdCode[tbSupProdCode.Length - 1].Size = new Size(200, Convert.ToInt32(this.Font.GetHeight()));
            this.Controls.Add(tbSupProdCode[tbSupProdCode.Length - 1]);
            tbSupProdCode[tbSupProdCode.Length - 1].Tag = tbSupProdCode.Length - 1;
            tbSupProdCode[tbSupProdCode.Length - 1].KeyDown += new KeyEventHandler(tbSupProdCodeKeyDown);
        }

        void tbSupCostKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                tbSupProdCode[Convert.ToInt32(((TextBox)sender).Tag)].Focus();
            }
            else if (e.KeyCode == Keys.Escape)
                EscPressed();
            else if (e.Shift && e.KeyCode == Keys.Delete)
                DeleteSupplierRow(Convert.ToInt32(((TextBox)sender).Tag));
        }

        void tbSupCodeKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                if (InputTextBox("TYPE").Text != "6")
                {
                    frmListOfSuppliers flos = new frmListOfSuppliers(ref sEngine);
                    flos.ShowDialog();
                    if (flos.sSelectedSupplierCode != "NULL")
                    {
                        TextBox tbSender = (TextBox)sender;
                        tbSender.Text = flos.sSelectedSupplierCode;
                        for (int i = 0; i < lmArray.Length; i++)
                        {
                            if (lmArray[i].lblMessage.Location == new Point(120, tbSender.Top))
                            {
                                RemoveMessage(lmArray[i].sTag);
                            }
                        }
                        AddMessage(flos.sSelectedSupplierCode, sEngine.GetSupplierDetails(flos.sSelectedSupplierCode)[1], new Point(120, tbSender.Top));
                        tbSupCost[Convert.ToInt32(tbSender.Tag)].Focus();
                    }
                }
                else
                {
                    frmListOfCommissioners floc = new frmListOfCommissioners(ref sEngine);
                    floc.ShowDialog();
                    if (floc.Commissioner != "$NONE")
                    {
                        TextBox tbSender = (TextBox)sender;
                        tbSender.Text = floc.Commissioner;
                        for (int i = 0; i < lmArray.Length; i++)
                        {
                            if (lmArray[i].lblMessage.Location == new Point(120, tbSender.Top))
                            {
                                RemoveMessage(lmArray[i].sTag);
                            }
                        }
                        AddMessage(floc.Commissioner, sEngine.GetCommissionerName(floc.Commissioner), new Point(120, tbSender.Top));
                        tbSupCost[Convert.ToInt32(tbSender.Tag)].Focus();
                    }
                }
            }
            else if (e.KeyCode == Keys.Enter && ((TextBox)sender).Text == "")
            {
                Finished();
            }
            else if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab)
            {
                if (InputTextBox("TYPE").Text != "6")
                {
                    string sSupName = sEngine.GetSupplierDetails(((TextBox)sender).Text)[0];
                    if (sSupName == null)
                    {
                        if (MessageBox.Show("Supplier not found! Create now?", "Unknown Supplier", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            // Create new supplier window
                            frmAddSupplier fAS = new frmAddSupplier(ref sEngine);
                            fAS.ShowDialog();
                            ((TextBox)sender).Text = fAS.supplierCode;
                        }
                        else
                        {
                            ((TextBox)sender).Focus();
                            ((TextBox)sender).SelectAll();
                        }
                    }
                    else
                    {
                        for (int i = 0; i < lmArray.Length; i++)
                        {
                            if (lmArray[i].lblMessage.Location == new Point(120, ((TextBox)sender).Top))
                            {
                                RemoveMessage(lmArray[i].sTag);
                            }
                        }
                        AddMessage(sSupName, sEngine.GetSupplierDetails(sSupName)[1], new Point(120, ((TextBox)sender).Top));
                        tbSupCost[Convert.ToInt32(((TextBox)sender).Tag)].Focus();
                    }
                }
                else
                {
                    TextBox tbSender = ((TextBox)sender);
                    for (int i = 0; i < lmArray.Length; i++)
                    {
                        if (lmArray[i].lblMessage.Location == new Point(120, tbSender.Top))
                        {
                            RemoveMessage(lmArray[i].sTag);
                        }
                    }
                    AddMessage(tbSender.Text, sEngine.GetCommissionerName(tbSender.Text), new Point(120, tbSender.Top));
                    tbSupCost[Convert.ToInt32(tbSender.Tag)].Focus();
                }
            }
            else if (e.KeyCode == Keys.Escape)
                EscPressed();
            else if (e.Shift && e.KeyCode == Keys.Delete)
                DeleteSupplierRow(Convert.ToInt32(((TextBox)sender).Tag));
        }

        void DeleteSupplierRow(int nRow)
        {
            for (int i = nRow; i < tbSupCode.Length - 1; i++)
            {
                tbSupCode[i].Text = tbSupCode[i + 1].Text;
                tbSupCost[i].Text = tbSupCost[i + 1].Text;
                tbSupProdCode[i].Text = tbSupProdCode[i + 1].Text;
            }
            this.Controls.Remove(tbSupCode[tbSupCode.Length - 1]);
            this.Controls.Remove(tbSupCost[tbSupCost.Length - 1]);
            this.Controls.Remove(tbSupProdCode[tbSupProdCode.Length - 1]);
            Array.Resize<TextBox>(ref tbSupCode, tbSupCode.Length - 1);
            Array.Resize<TextBox>(ref tbSupCost, tbSupCost.Length - 1);
            Array.Resize<TextBox>(ref tbSupProdCode, tbSupProdCode.Length - 1);
            InputTextBox("MINQTY").Focus();
        }

        void tbSupProdCodeKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab)
            {
                if (Convert.ToInt32(((TextBox)sender).Tag) == tbSupCode.Length - 1)
                {
                    AddSupplierBoxes();
                }
                else
                {
                    tbSupCode[Convert.ToInt32(((TextBox)sender).Tag) + 1].Focus();
                }
            }
            else if (e.KeyCode == Keys.Escape)
                EscPressed();
            else if (e.Shift && e.KeyCode == Keys.Delete)
                DeleteSupplierRow(Convert.ToInt32(((TextBox)sender).Tag));
        }

        void frmAddEditItemVatBox_GotFocus(object sender, EventArgs e)
        {
            if (!bVATSelectOpen)
            {
                frmListOfVATRates fVat = new frmListOfVATRates(ref sEngine);
                bVATSelectOpen = true;
                fVat.ShowDialog();
                if (fVat.sSelectedVATCode != "NULL")
                {
                    InputTextBox("VAT").Text = fVat.sSelectedVATCode;
                    if (bEditingItem)
                        InputTextBox("QIS").Focus();
                    else
                        InputTextBox("AVECOST").Focus();
                }
                else
                {
                    InputTextBox("£RRP").Focus();
                }
                fVat.Close();
                bVATSelectOpen = false;
                fVat.Dispose();
            }
        }

        void frmAddEditItemCategoryBox_GotFocus(object sender, EventArgs e)
        {
            if (!bCategorySelectOpen && InputTextBox("CATEGORY").Text == "")
            {
                GetCategory();
            }

        }

        void frmAddEditItemTypeBox_GotFocus(object sender, EventArgs e)
        {
            if (!bItemTypeSelectOpen)
            {
                frmListOfItemTypes fItemTypes = new frmListOfItemTypes();
                if (InputTextBox("TYPE").Text != "")
                {
                    try
                    {
                        fItemTypes.SelectedItemType = Convert.ToInt32(InputTextBox("TYPE").Text);

                    }
                    catch
                    {
                        ;
                    }
                }
                bItemTypeSelectOpen = true;
                fItemTypes.ShowDialog();
                if (fItemTypes.SelectedItemType == 4)
                {
                    frmAddMultiItem fami = new frmAddMultiItem(ref sEngine);
                    fami.ShopCode = InputTextBox("SHOPCODE").Text;
                    fami.Barcode = InputTextBox("BARCODE").Text;
                    fami.Description = InputTextBox("DESCRIPTION").Text;
                    fami.ShowDialog();
                    bAskedAboutClosing = true;
                    this.Close();
                }
                if (fItemTypes.SelectedItemType == -1)
                {
                    InputTextBox("DESCRIPTION").Focus();
                }
                else
                {
                    InputTextBox("TYPE").Text = fItemTypes.SelectedItemType.ToString();
                    GetParentInfo();
                    if (fItemTypes.SelectedItemType == 2) // Department
                    {
                        frmSingleInputBox fsiMargin = new frmSingleInputBox("Would you like to enter a target margin for this item? Leave blank (or as 0.00)  if not.", ref sEngine);
                        fsiMargin.tbResponse.Text = FormatMoneyForDisplay(dCatTwoItemMargin);
                        fsiMargin.ShowDialog();
                        if (fsiMargin.Response != "$NONE")
                        {
                            try
                            {
                                dCatTwoItemMargin = Convert.ToDecimal(fsiMargin.Response);
                            }
                            catch
                            {
                                dCatTwoItemMargin = 0;
                            }
                        }

                    }
                    if (Convert.ToInt32(InputTextBox("TYPE").Text) == 3 && nOriginalItemType != 3)
                    {
                        if (MessageBox.Show("Would you like to set the stock level to 0 (as type 3 items don't keep record of stock)?", "Stock Level", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            InputTextBox("QIS").Text = "0";
                        }
                    }
                    // If it's a commission item, then set the QIS to 1, as long as we're not editing
                    // an existing item!
                    if (Convert.ToInt32(InputTextBox("TYPE").Text) == 6 && !bEditingItem)
                    {
                        InputTextBox("QIS").Text = "1";
                    }
                    if (bCont)
                        InputTextBox("CATEGORY").Focus();
                }
                fItemTypes.Close();
                bItemTypeSelectOpen = false;
                fItemTypes.Dispose();
            }
        }

        void Finished()
        {
            switch (MessageBox.Show("Would you like to save the changes?", "Save Changes", MessageBoxButtons.YesNoCancel))
            {
                case DialogResult.Yes:

                    if (!CheckAllFieldsAreAcceptable())
                    {
                        MessageBox.Show("One or more fields are invalid, please correct this", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        AddItem();
                        bAskedAboutClosing = true;
                        this.Close();
                    }
                    break;
                case DialogResult.No:
                    bAskedAboutClosing = true;
                    this.Close();
                    break;
                case DialogResult.Cancel:
                    InputTextBox("BARCODE").Focus();
                    break;
            }
        }

        /// <summary>
        /// Checks that all of the fields entered by the user are acceptable, and shouldn't cause any problems
        /// with the databases
        /// </summary>
        /// <returns>True if all is ok, false otherwise</returns>
        private bool CheckAllFieldsAreAcceptable()
        {
            /*
             * AddInputControl("SHOPCODE", "Shop Code : ", new Point(10, 25), 200);
            InputTextBox("SHOPCODE").GotFocus += new EventHandler(frmAddEditItemShopCode_GotFocus);
            if (!bEditingItem)
                AddInputControl("BARCODE", "Item's Barcode : ", new Point(10, BelowLastControl), 300, "Enter the barcode of the new item (F6 for auto-barcode)");
            else
                AddInputControl("BARCODE", "Item's Barcode : ", new Point(10, BelowLastControl), 300, "Enter the barcode of the existing item");
            InputTextBox("BARCODE").MaxCharCount = 13;
            InputTextBox("BARCODE").KeyDown += new KeyEventHandler(tbBarcodeKeyDown);
            AddInputControl("DESCRIPTION", "Item's Description : ", new Point(10, BelowLastControl), 300, "Enter a description of the item");
            InputTextBox("DESCRIPTION").MaxCharCount = 30;
            AddInputControl("TYPE", "Item's Type : ", new Point(10, BelowLastControl), 300, "Select the item's type");
            InputTextBox("TYPE").GotFocus += new EventHandler(frmAddEditItemTypeBox_GotFocus);
            InputTextBox("TYPE").KeyDown += new KeyEventHandler(tbTypeKeyDown);
            AddInputControl("CATEGORY", "Item's Category : ", new Point(10, BelowLastControl), 300, "Select the item's category");
            InputTextBox("CATEGORY").GotFocus += new EventHandler(frmAddEditItemCategoryBox_GotFocus);
            InputTextBox("CATEGORY").KeyDown += new KeyEventHandler(tbCategoryKeyDown);
            AddInputControl("£RRP", "Item's Price : ", new Point(10, BelowLastControl), 300, "Enter the retail price of the item");
            InputTextBox("£RRP").KeyDown += new KeyEventHandler(RRPKeyDown);
            AddInputControl("VAT", "Item's V.A.T Type : ", new Point(10, BelowLastControl), 300, "Select the item's V.A.T rate");
            InputTextBox("VAT").GotFocus += new EventHandler(frmAddEditItemVatBox_GotFocus);
            AddInputControl("QIS", "Stock Level : ", new Point(10, BelowLastControl), 300, "Enter the stock level for the item");
            AddInputControl("AVECOST", "Average Cost : ", new Point(10, BelowLastControl), 300, "Enter the average cost of this item");
            InputTextBox("AVECOST").KeyDown += new KeyEventHandler(AveCost_KeyDown);
            AddInputControl("MINQTY", "Minimum Order : ", new Point(10, BelowLastControl), 300, "What's the minimum that you can order? Numerical values only!");
            InputTextBox("MINQTY").KeyDown += new KeyEventHandler(frmAddEditItemMinQTY_KeyDown);
            InputTextBox("MINQTY").MaxCharCount = 6;
             */
            try
            {
                // Check that the barcode field is of an acceptable length
                if (InputTextBox("BARCODE").Text.Length == 0 || InputTextBox("BARCODE").Text.Length > 13)
                {
                    return false;
                }

                // Check that the type is valid
                if (Convert.ToInt32(InputTextBox("TYPE").Text) < 1 || Convert.ToInt32(InputTextBox("TYPE").Text) > 6)
                {
                    return false;
                }

                // Check that the category exists
                if (sEngine.GetCategoryDesc(InputTextBox("CATEGORY").Text) == "")
                {
                    return false;
                }

                // Check that the RRP is a valid decimal
                try
                {
                    Convert.ToDecimal(InputTextBox("£RRP").Text);
                }
                catch
                {
                    return false;
                }

                // Check that the VAT is a valid type
                if (sEngine.GetVATRateFromVATCode(InputTextBox("VAT").Text) == -1)
                {
                    return false;
                }

                // Check that the stock level is a valid decimal
                try
                {
                    Convert.ToDecimal(InputTextBox("QIS").Text);
                }
                catch
                {
                    if (InputTextBox("QIS").Text != "")
                    {
                        return false;
                    }
                }

                // Check that the average cost is a valid decimal
                try
                {
                    Convert.ToDecimal(InputTextBox("AVECOST").Text);
                }
                catch
                {
                    if (InputTextBox("AVECOST").Text != "")
                    {
                        return false;
                    }
                }

                try
                {
                    Convert.ToDecimal(InputTextBox("MINQTY").Text);
                }
                catch
                {
                    if (InputTextBox("MINQTY").Text != "")
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        void AddItem()
        {
            if (InputTextBox("BARCODE").Text == sCurrentBarcodeShowing && bEditingItem || !bEditingItem)
            {
                if (InputTextBox("TYPE").Text == "1" || InputTextBox("TYPE").Text == "3")
                {
                    /*// Temporary?
                    decimal dStockLevel = 0;
                    try
                    {
                        dStockLevel = Convert.ToDecimal(InputTextBox("QIS").Text);
                    }
                    catch
                    {
                        ;
                    }
                    if (dStockLevel != dOriginalStockLevel)
                    {
                        frmSingleInputBox fsPassword = new frmSingleInputBox("The administrator password is required to alter the stock level", ref sEngine);
                        fsPassword.tbResponse.PasswordChar = ' ';
                        fsPassword.ShowDialog();
                        if (fsPassword.Response != "$NONE")
                        {
                            if (fsPassword.Response.ToUpper() == sEngine.GetPasswords(2).ToUpper())
                            {
                                if (dStockLevel > dOriginalStockLevel)
                                {
                                    sEngine.TransferStockItem("BH", InputTextBox("SHOPCODE").Text, InputTextBox("BARCODE").Text, (dStockLevel - dOriginalStockLevel), true);
                                }
                                else
                                {
                                    sEngine.TransferStockItem(InputTextBox("SHOPCODE").Text, "BH", InputTextBox("BARCODE").Text, (dOriginalStockLevel - dStockLevel), true);
                                }
                            }
                        } //End Temp
                    }*/
                    sEngine.RemoveSuppliersForItem(InputTextBox("BARCODE").Text);
                    sEngine.AddEditItem(InputTextBox("SHOPCODE").Text, InputTextBox("BARCODE").Text, InputTextBox("DESCRIPTION").Text,
                        InputTextBox("TYPE").Text, InputTextBox("CATEGORY").Text, InputTextBox("£RRP").Text, InputTextBox("VAT").Text,
                        InputTextBox("MINQTY").Text, InputTextBox("AVECOST").Text);

                    // Temporarily moved here to see if it fixes changing stock level of new item
                    decimal dStockLevel = 0;
                    try
                    {
                        dStockLevel = Convert.ToDecimal(InputTextBox("QIS").Text);
                    }
                    catch
                    {
                        ;
                    }
                    if (dStockLevel != dOriginalStockLevel)
                    {
                        frmSingleInputBox fsPassword = new frmSingleInputBox("The administrator password is required to alter the stock level", ref sEngine);
                        fsPassword.tbResponse.PasswordChar = ' ';
                        fsPassword.ShowDialog();
                        if (fsPassword.Response != "$NONE")
                        {
                            if (fsPassword.Response.ToUpper() == sEngine.GetPasswords(2).ToUpper())
                            {
                                if (dStockLevel > dOriginalStockLevel)
                                {
                                    sEngine.TransferStockItem("BH", InputTextBox("SHOPCODE").Text, InputTextBox("BARCODE").Text, (dStockLevel - dOriginalStockLevel), true);
                                }
                                else
                                {
                                    sEngine.TransferStockItem(InputTextBox("SHOPCODE").Text, "BH", InputTextBox("BARCODE").Text, (dOriginalStockLevel - dStockLevel), true);
                                }
                            }
                        } //End Temp
                    }

                    if (Convert.ToDecimal(InputTextBox("£RRP").Text) != dOriginalPrice && dOriginalPrice != 0)
                    {
                        if (sEngine.DoesParentHaveChildren(InputTextBox("BARCODE").Text))
                        {
                            if (MessageBox.Show("This item has children, and its price has changed. Would you like to alter the children items' prices to match?", "Change children prices?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                sEngine.ChangeChildPricesToMatchParent(InputTextBox("BARCODE").Text, Convert.ToDecimal(InputTextBox("£RRP").Text), InputTextBox("SHOPCODE").Text);
                            }
                        }
                    }
                    for (int i = 0; i < tbSupCode.Length; i++)
                    {
                        // Add each supplier
                        if (tbSupCode[i].Text != "")
                        {
                            sEngine.AddSupplierForItem(InputTextBox("BARCODE").Text, tbSupCode[i].Text, tbSupCost[i].Text, tbSupProdCode[i].Text);
                        }
                    } 
                    /*if (dStockLevel != dOriginalStockLevel)
                    {
                        frmSingleInputBox fsPassword = new frmSingleInputBox("The administrator password is required to alter the stock level", ref sEngine);
                        fsPassword.tbResponse.PasswordChar = ' ';
                        fsPassword.ShowDialog();
                        if (fsPassword.Response != "$NONE")
                        {
                            if (fsPassword.Response.ToUpper() == sEngine.GetPasswords(2).ToUpper())
                            {
                                if (dStockLevel > dOriginalStockLevel)
                                {
                                    sEngine.TransferStockItem("BH", InputTextBox("SHOPCODE").Text, InputTextBox("BARCODE").Text, (dStockLevel - dOriginalStockLevel));
                                }
                                else
                                {
                                    sEngine.TransferStockItem(InputTextBox("SHOPCODE").Text, "BH", InputTextBox("BARCODE").Text, (dOriginalStockLevel - dStockLevel));
                                }
                            }
                        }*/
                    
                }
                else if (InputTextBox("TYPE").Text == "2")
                {
                    try
                    {
                        dCatTwoItemCost = Convert.ToDecimal(InputTextBox("AVECOST").Text);
                    }
                    catch
                    {
                        dCatTwoItemCost = 0;
                    }
                    sEngine.RemoveSuppliersForItem(InputTextBox("BARCODE").Text);
                    sEngine.AddEditItem(InputTextBox("SHOPCODE").Text, InputTextBox("BARCODE").Text, InputTextBox("DESCRIPTION").Text, InputTextBox("TYPE").Text,
                        InputTextBox("CATEGORY").Text, InputTextBox("VAT").Text, InputTextBox("MINQTY").Text, dCatTwoItemCost, dCatTwoItemMargin);
                }
                else if (InputTextBox("TYPE").Text == "5")
                {
                    sEngine.RemoveSuppliersForItem(InputTextBox("BARCODE").Text);
                    sEngine.AddEditItem(InputTextBox("SHOPCODE").Text, InputTextBox("BARCODE").Text, InputTextBox("DESCRIPTION").Text,
                        InputTextBox("TYPE").Text, InputTextBox("CATEGORY").Text, InputTextBox("£RRP").Text, InputTextBox("VAT").Text,
                        InputTextBox("MINQTY").Text, sParentCode, dChildPart.ToString());
                    if (MessageBox.Show("Would you like to match parent and other children's prices to this item's new price?", "Change parent & child prices?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        if (sEngine.ChangeParentAndAllOtherChildrenPrices(InputTextBox("BARCODE").Text, Convert.ToDecimal(InputTextBox("£RRP").Text), InputTextBox("SHOPCODE").Text) == false)
                        {
                            MessageBox.Show("Failed to change parent and child prices to match this one. Are you sure that the parent exists?");
                        }
                    }
                }
                else if (InputTextBox("TYPE").Text == "6")
                {
                    // Add or edit the type 6 item to stocksta and mainstock
                    sEngine.AddEditItem(InputTextBox("SHOPCODE").Text, InputTextBox("BARCODE").Text, InputTextBox("DESCRIPTION").Text,
                        InputTextBox("TYPE").Text, InputTextBox("CATEGORY").Text, InputTextBox("£RRP").Text, InputTextBox("VAT").Text,
                        InputTextBox("MINQTY").Text, tbSupCode[0].Text, Convert.ToDecimal(tbSupCost[0].Text), "0");

                    decimal dStockLevel = 1;
                    // Attempt to receive the commission item
                    try
                    {
                        dStockLevel = Convert.ToDecimal(InputTextBox("QIS").Text);
                        if (bEditingItem)
                        {
                            // Subtract the number in stock from the original stock level, to work out how many
                            // to add
                            dStockLevel -= dOriginalStockLevel;
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Stock Level invalid, assuming 0");
                        dStockLevel = 0;
                    }
                    // If addding a new item, or more have been received in stock
                    if (!bEditingItem || dStockLevel > 0)
                    {
                        sEngine.ReceiveComissionItem(InputTextBox("BARCODE").Text, dStockLevel.ToString(), InputTextBox("SHOPCODE").Text);
                    }
                }
                if (MessageBox.Show("Would you like to upload the information to all tills now?", "Upload now?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    sEngine.CopyWaitingFilesToTills();
                }
            }
            else
            {
                MessageBox.Show("Sorry, you can't change the barcode of an item!");
                InputTextBox("BARCODE").Text = sCurrentBarcodeShowing;
            }
        }

        void EscPressed()
        {
            if (bEditingItem)
            {
                if (InputTextBox("BARCODE").Text != "")
                {
                    switch (MessageBox.Show("Save any changes made to this item?", "Save Changes?", MessageBoxButtons.YesNoCancel))
                    {
                        case DialogResult.Yes:

                            if (!CheckAllFieldsAreAcceptable())
                            {
                                MessageBox.Show("One or more fields are invalid, please correct this", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                            {
                                AddItem();
                                bAskedAboutClosing = true;
                                this.Close();
                            }
                            break;
                        case DialogResult.No:
                            bAskedAboutClosing = true;
                            this.Close();
                            break;
                        case DialogResult.Cancel:
                            break;
                    }
                }
                else
                {
                    bAskedAboutClosing = true;
                    this.Close();
                }
            }
            else if (!bEditingItem)
            {
                switch (MessageBox.Show("Save new item?", "Save item?", MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.Yes:

                        if (!CheckAllFieldsAreAcceptable())
                        {
                            MessageBox.Show("One or more fields are invalid, please correct this", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            AddItem();
                            bAskedAboutClosing = true;
                            this.Close();
                        }
                        break;
                    case DialogResult.No:
                        bAskedAboutClosing = true;
                        this.Close();
                        break;
                }
            }
        }
    }
}
