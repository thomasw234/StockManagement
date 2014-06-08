using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.WormaldForms;
using System.Drawing;
using System.Data;
using System.Text;

namespace BackOffice
{
    class frmBatchEditItems : Form
    {
        DataGridView dGrid;
        DataTable dTable;
        StockEngine sEngine;
        string[] sCodes;
        string sShopCode;
        string sSpecifiedSupplierCode = "";
        List<string> sModifiedCodes;

        public frmBatchEditItems(ref StockEngine se)
        {
            this.VisibleChanged += new EventHandler(frmBatchEditItems_VisibleChanged);
            sEngine = se;
            SetupForm();
            this.SizeChanged += new EventHandler(frmBatchEditItems_SizeChanged);
            if (GetItemsInfo())
            {
                for (int i = 0; i < sCodes.Length; i++)
                {
                    AddItemRow(sCodes[i]);
                }
            }
            //SizeColumnsToContent(dGrid, -1);
            for (int i = 0; i < dGrid.Columns.Count; i++)
            {
                dGrid.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            dGrid.GotFocus += new EventHandler(dGrid_GotFocus);
            dGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.LightBlue;
            
        }

        void frmBatchEditItems_VisibleChanged(object sender, EventArgs e)
        {
            if (dGrid.Rows.Count == 0 && this.Visible)
            {
                bClosing = true;
                MessageBox.Show("No items fit the criteria given");
                this.Close();
            }
        }

        void dGrid_GotFocus(object sender, EventArgs e)
        {
            dGrid.Columns[0].ReadOnly = true;
            for (int i = 0; i < dGrid.Rows.Count; i++)
            {
                if (dGrid.Rows[i].Cells[2].Value.ToString() != null && dGrid.Rows[i].Cells[2].Value.ToString() != "5")
                {
                    dGrid.Rows[i].Cells[11].ReadOnly = true;
                    dGrid.Rows[i].Cells[12].ReadOnly = true;
                }
                if (dGrid.Rows[i].Cells[2].Value.ToString() != "1")
                {
                    dGrid.Rows[i].Cells[7].ReadOnly = true;
                }
                dGrid.CellEndEdit += new DataGridViewCellEventHandler(dGrid_CellEndEdit);
                dGrid.KeyDown +=new KeyEventHandler(dGrid_KeyDown);
            }
        }

        void dGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 2 && dGrid.Rows[e.RowIndex].Cells[2].Value.ToString().Equals("5"))
            {
                dGrid.Rows[e.RowIndex].Cells[11].ReadOnly = false;
                dGrid.Rows[e.RowIndex].Cells[12].ReadOnly = false;
            }
            else if (e.ColumnIndex == 2)
            {
                dGrid.Rows[e.RowIndex].Cells[11].ReadOnly = true;
                dGrid.Rows[e.RowIndex].Cells[11].Value = "";
                dGrid.Rows[e.RowIndex].Cells[12].Value = "0";
                dGrid.Rows[e.RowIndex].Cells[12].ReadOnly = true;
            }

            if (e.ColumnIndex == 2 && dGrid.Rows[e.RowIndex].Cells[2].Value.ToString().Equals("1"))
            {
                dGrid.Rows[e.RowIndex].Cells[7].ReadOnly = false;
            }
            else if (e.ColumnIndex == 2 && !dGrid.Rows[e.RowIndex].Cells[2].Value.ToString().Equals("1"))
            {
                dGrid.Rows[e.RowIndex].Cells[7].ReadOnly = true;
            }

        }

        void frmBatchEditItems_SizeChanged(object sender, EventArgs e)
        {
            dGrid.Size = new Size(this.Width 
                
                
                , this.Height - 60);
        }

        void SetupForm()
        {
            this.Size = new Size(1024, 768);
            this.WindowState = FormWindowState.Maximized;
            this.FormClosing += new FormClosingEventHandler(frmBatchEditItems_FormClosing);

            Label lblInst = new Label();
            lblInst.Text = "Press Enter to begin editing a cell, F3 to batch change a column, F5 to choose category, VAT code etc., and ESC to save and quit";
            lblInst.AutoSize = true;
            this.Controls.Add(lblInst);
            lblInst.Location = new Point(0, 0);

            dGrid = new DataGridView();
            dGrid.Size = new Size(this.Width - 20, this.Height - 60);
            this.Controls.Add(dGrid);
            dGrid.Location = new Point(0, 20);

            dTable = new DataTable();
            dGrid.Font = new Font("Franklin Gothic Medium", 9.0f);
           

            DataColumn[] dColumns = new DataColumn[13];
            string[] sNames = { "Barcode", "Description", "Type", "Category", "RRP", "VAT", "Minimum Quantity", "QIS", "Supplier Code", "Cost", "Product Code", "Parent Code", "Qty/Parent" };
            Type[] tTypes = { typeof(String), typeof(String), typeof(String), typeof(String), typeof(Decimal), typeof(String), typeof(String), typeof(String), typeof(String), typeof(Decimal), typeof(String), typeof(String), typeof(Decimal) };
            for (int i = 0; i < dColumns.Length; i++)
            {
                dColumns[i] = new DataColumn(sNames[i], tTypes[i]);
            }
            dTable.Columns.AddRange(dColumns);
            dGrid.DataSource = dTable;
            dGrid.KeyDown += new KeyEventHandler(dGrid_KeyDown);
            dGrid.KeyUp += new KeyEventHandler(dGrid_KeyUp);
            dGrid.CellValueChanged += new DataGridViewCellEventHandler(dGrid_CellValueChanged);
            dGrid.AllowUserToAddRows = false;

            sModifiedCodes = new List<string>();

            this.Text = "Batch Edit Items";
            
        }

        void dGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Add the code to the list of modified codes, just in case it is modified
            bool bAlreadyEdited = false;
            for (int i = 0; i < sModifiedCodes.Count; i++)
            {
                if (dGrid.Rows[e.RowIndex].Cells[0].Value.ToString() == sModifiedCodes[i])
                {
                    bAlreadyEdited = true;
                    break;
                }
            }
            if (!bAlreadyEdited)
            {
                sModifiedCodes.Add(dGrid.Rows[e.RowIndex].Cells[0].Value.ToString());
            }
        }

        void dGrid_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                dGrid.BeginEdit(false);
            }
        }

        void frmBatchEditItems_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!bClosing)
            {
                AskToSave();
            }
        }
        bool bShowingType = false;
        bool bEscapeRegistered = false;
        bool bF5Registered = false;
        bool bF3Registered = false;
        int nColumnEscRegistered = -1;
        int nRowEscRegistered = -1;
        void dGrid_KeyDown(object sender, KeyEventArgs e)
        {
            
            if (e.KeyCode == Keys.F3 && !bF3Registered)
            {
                bF5Registered = false;
                bEscapeRegistered = false;
                bF3Registered = true;
                frmSingleInputBox fsfiGetOriginal = new frmSingleInputBox("Where the " + dGrid.Columns[dGrid.SelectedCells[0].ColumnIndex].Name + " is currently...", ref sEngine);
                fsfiGetOriginal.tbResponse.Text = dGrid.CurrentCell.Value.ToString();
                fsfiGetOriginal.ShowDialog();
                if (fsfiGetOriginal.Response != "$NONE")
                {
                    frmSingleInputBox fsfiGetNew = new frmSingleInputBox("Change the " + dGrid.Columns[dGrid.SelectedCells[0].ColumnIndex].Name + " from " + fsfiGetOriginal.Response + " to...", ref sEngine);
                    fsfiGetNew.ShowDialog();
                    if (fsfiGetNew.Response != "$NONE")
                    {
                        bool bNoneConverted = false;
                        while (!bNoneConverted) // Has to be done otherwise the grid sorts elements as it's changing them and ends up missing some
                        {
                            bNoneConverted = true;
                            for (int i = 0; i < dGrid.Rows.Count; i++)
                            {
                                if (dGrid.SelectedCells[0].ColumnIndex != 4 && dGrid.SelectedCells[0].ColumnIndex != 8 && dGrid.Rows[i].Cells[dGrid.SelectedCells[0].ColumnIndex].FormattedValue.ToString() == fsfiGetOriginal.Response)
                                {
                                    dGrid.Rows[i].Cells[dGrid.SelectedCells[0].ColumnIndex].Value = fsfiGetNew.Response;
                                    bNoneConverted = false;
                                    //i = 0;
                                }
                                else if ((dGrid.SelectedCells[0].ColumnIndex == 4 || dGrid.SelectedCells[0].ColumnIndex == 8) && dGrid.Rows[i].Cells[dGrid.SelectedCells[0].ColumnIndex].FormattedValue.ToString() != "" && Convert.ToDecimal(dGrid.Rows[i].Cells[dGrid.SelectedCells[0].ColumnIndex].FormattedValue.ToString()).Equals(Convert.ToDecimal(fsfiGetOriginal.Response)))
                                {
                                    dGrid.Rows[i].Cells[dGrid.SelectedCells[0].ColumnIndex].Value = ScalableForm.FormatMoneyForDisplay(fsfiGetNew.Response);
                                    bNoneConverted = false;
                                    //i = 0;
                                }
                            }
                        }
                        
                    }
                }
            }
            else if (e.KeyCode == Keys.Escape && ((dGrid.CurrentCell != null && !dGrid.CurrentCell.IsInEditMode && !bEscapeRegistered) || (bEscapeRegistered && (nRowEscRegistered != dGrid.CurrentCell.RowIndex) || (nColumnEscRegistered != dGrid.CurrentCell.ColumnIndex))))// && lastKey != Keys.Escape)
            {
                bEscapeRegistered = true;
                nColumnEscRegistered = dGrid.CurrentCell.ColumnIndex;
                nRowEscRegistered = dGrid.CurrentCell.RowIndex;
                bF5Registered = false;
                bF3Registered = false;
                AskToSave();
            }
            else if (dGrid.CurrentCell.ColumnIndex == 2 && e.KeyCode == Keys.F5 && !bShowingType && !bF5Registered)
            {
                bEscapeRegistered = false;
                bF5Registered = true;
                bF3Registered = false;
                bShowingType = true;
                frmListOfItemTypes flot = new frmListOfItemTypes();
                flot.SelectedItemType = Convert.ToInt32(dGrid.CurrentCell.Value.ToString());
                flot.ShowDialog();
                if (flot.SelectedItemType != -1)
                    dGrid.CurrentCell.Value = flot.SelectedItemType.ToString();
                bShowingType = false;
                e.Handled = true;
            }
            else if (dGrid.CurrentCell.ColumnIndex == 3 && e.KeyCode == Keys.F5 && !bF5Registered)
            {
                bEscapeRegistered = false;
                bF5Registered = true;
                bF3Registered = false;
                frmCategorySelect fcs = new frmCategorySelect(ref sEngine);
                fcs.ShowDialog();
                if (fcs.SelectedItemCategory != "$NULL")
                {
                    dGrid.CurrentCell.Value = fcs.SelectedItemCategory;
                }
            }
            else if (dGrid.CurrentCell.ColumnIndex == 5 && e.KeyCode == Keys.F5 && !bF5Registered)
            {
                bEscapeRegistered = false;
                bF5Registered = true;
                bF3Registered = false;
                frmListOfVATRates flov = new frmListOfVATRates(ref sEngine);
                flov.ShowDialog();
                if (flov.sSelectedVATCode != "NULL")
                    dGrid.CurrentCell.Value = flov.sSelectedVATCode;
            }
            else if (dGrid.CurrentCell.ColumnIndex == 7 && e.KeyCode == Keys.F5 && !bF5Registered)
            {
                bEscapeRegistered = false;
                bF5Registered = true;
                bF3Registered = false;
                frmListOfSuppliers flos = new frmListOfSuppliers(ref sEngine);
                flos.ShowDialog();
                if (flos.sSelectedSupplierCode != "NULL")
                {
                    dGrid.CurrentCell.Value = flos.sSelectedSupplierCode;
                }
            }
            else if (dGrid.CurrentCell.ColumnIndex == 10 && e.KeyCode == Keys.F5 && !bF5Registered)
            {
                bEscapeRegistered = false;
                bF5Registered = true;
                bF3Registered = false;
                frmSearchForItemV2 fsfi = new frmSearchForItemV2(ref sEngine);
                fsfi.ShowDialog();
                if (fsfi.GetItemBarcode() != "NONE_SELECTED")
                {
                    dGrid.CurrentCell.Value = fsfi.GetItemBarcode();
                }
            }
            else if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Right || e.KeyCode == Keys.Up || e.KeyCode == Keys.Left)
            {
                bF3Registered = false;
                bF5Registered = false;
                bEscapeRegistered = false;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
            }
        }
        frmNewBatchEditOptions fbEdit;
        bool GetItemsInfo()
        {
            fbEdit = new frmNewBatchEditOptions(ref sEngine);
            fbEdit.ShowDialog();
            if (fbEdit.bFinished)
            {
                sCodes = sEngine.GetListOfItemsByCatAndSup(fbEdit.ShopCode.ToUpper(), fbEdit.CategoryCode.ToUpper(), fbEdit.SupplierCode.ToUpper());
                sShopCode = fbEdit.ShopCode.ToUpper();
                sSpecifiedSupplierCode = fbEdit.SupplierCode.ToUpper();
            }
            return fbEdit.bFinished;
        }

        bool bClosing = false;
        void AskToSave()
        {
            switch (MessageBox.Show("Would you like to save any changes before you exit?", "Exit Batch Edit", MessageBoxButtons.YesNoCancel))
            {
                case DialogResult.Yes:
                    AddAllItems();
                    bClosing = true;
                    this.Close();
                    break;
                case DialogResult.No:
                    bClosing = true;
                    this.Close();
                    break;
                case DialogResult.Cancel:
                    bClosing = false;
                    break;
            }
        }

        void AddItemRow(string sBarcode)
        {
            string[] sStockStaInfo = sEngine.GetItemStockStaRecord(sBarcode, sShopCode);
            string[] sMainStockInfo = sEngine.GetMainStockInfo(sBarcode);

            string sSupCode = "";
            if (sSpecifiedSupplierCode == "")
            {
                sSupCode = sMainStockInfo[6];
            }
            if (sStockStaInfo[37] == "" || sStockStaInfo[37] == "0.00000")
                sStockStaInfo[37] = "0.00";
            string[] sToAdd = { sStockStaInfo[0], sMainStockInfo[1], sMainStockInfo[5], sMainStockInfo[4], sMainStockInfo[2], sMainStockInfo[3], sStockStaInfo[37], sStockStaInfo[36], sSupCode, sEngine.GetItemCostBySupplier(sBarcode, sSupCode).ToString(), sEngine.GetItemCodeBySupplier(sBarcode, sSupCode), sMainStockInfo[7], sStockStaInfo[38] };
            dTable.Rows.Add(sToAdd);
        }

        public void SizeColumnsToContent(DataGridView dataGrid, int nRowsToScan)
        {
            dataGrid.AutoResizeColumns();
        }

        bool AddAllItems()
        {
            bool bAllok = true;
            int nofItemsOK = 0;
            int nOfItems = dGrid.Rows.Count;
            for (int i = nOfItems - 1; i >= 0; i -= 1)
            {
                if (!CheckRow(i))
                    bAllok = false;
                else
                    nofItemsOK++;
            }
            if (bAllok)
            {
                frmProgressBar fp = new frmProgressBar("Loading Items");
                fp.pb.Maximum = nofItemsOK;
                fp.Show();
                sEngine.AllowDatabaseSaves = false;
                for (int i = 0; i < nofItemsOK; i++)
                {
                    //string[] sNames = { "Barcode", "Description", "Type", "Category", "RRP", "VAT", "Minimum Quantity", "Supplier Code", "Cost", "Product Code" };
                    bool bExists = false;
                    for (int x = 0; x < sModifiedCodes.Count; x++)
                    {
                        if (sModifiedCodes[x] == dGrid.Rows[i].Cells[0].Value.ToString())
                        {
                            bExists = true;
                            break;
                        }
                    }
                    if (bExists)
                    {
                        //string[] sNames = { "Barcode", "Description", "Type", "Category", "RRP", "VAT", "Minimum Quantity", "QIS", "Supplier Code", "Cost", "Product Code", "Parent Code", "Qty/Parent" };
                        string sAverageCost = sEngine.GetItemAverageCost(dGrid.Rows[i].Cells[0].Value.ToString()).ToString();
                        AddItem(dGrid.Rows[i].Cells[2].Value.ToString(), dGrid.Rows[i].Cells[0].Value.ToString(), dGrid.Rows[i].Cells[1].Value.ToString(), dGrid.Rows[i].Cells[3].Value.ToString(), dGrid.Rows[i].Cells[4].Value.ToString(), dGrid.Rows[i].Cells[5].Value.ToString(),
                            dGrid.Rows[i].Cells[6].Value.ToString(), Convert.ToDecimal(dGrid.Rows[i].Cells[9].Value.ToString()),Convert.ToDecimal(dGrid.Rows[i].Cells[7].Value.ToString()), dGrid.Rows[i].Cells[11].Value.ToString(), Convert.ToDecimal(dGrid.Rows[i].Cells[12].Value.ToString()),
                            dGrid.Rows[i].Cells[8].Value.ToString(), Convert.ToDecimal(dGrid.Rows[i].Cells[9].Value.ToString()), dGrid.Rows[i].Cells[10].Value.ToString(), sAverageCost);
                    }
                    fp.pb.Value = i;

                    if (i == nofItemsOK - 1)
                    {
                        sEngine.AllowDatabaseSaves = true;
                        sEngine.SaveStockDatabases();
                    }
                }
                fp.Close();
                if (MessageBox.Show("Upload changes to all tills now?", "Upload", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    sEngine.CopyWaitingFilesToTills();
                }
            }
            return bAllok;
        }

        void GetPassword()
        {
            if (!bGotPassword)
            {
                frmSingleInputBox fGetPassword = new frmSingleInputBox("Enter the administrator password : ", ref sEngine);
                fGetPassword.tbResponse.PasswordChar = ' ';
                fGetPassword.ShowDialog();
                if (fGetPassword.Response != "$NONE" && fGetPassword.Response.ToUpper() == sEngine.GetPasswords(2).ToUpper())
                {
                    bGotPassword = true;
                }
            }
        }

        bool bGotPassword = false;
        void AddItem(string sType, string sBarcode, string sDesc, string sCategory, string sRRP, string sVAT, string sMinQty, decimal dCatTwoItemCost, decimal dQIS, string sParentCode, decimal dCPartQTY, string sSupplier, decimal dSupCost, string sSupCode, string sAveCost)
        {
            string sShopCode = "";
            sShopCode = fbEdit.ShopCode;
            sEngine.RemoveSuppliersForItem(sBarcode, sSupplier);
            if (sType == "1" || sType == "3")
            {
                sEngine.AddEditItem(sShopCode, sBarcode, sDesc,
                    sType, sCategory, sRRP, sVAT,
                    sMinQty, sAveCost);

                decimal dCurrentStockLevel = Convert.ToDecimal(sEngine.GetItemStockStaRecord(sBarcode, sShopCode)[36]);
                if (dQIS > dCurrentStockLevel)
                {
                    GetPassword();
                    sEngine.TransferStockItem("BH", sShopCode, sBarcode, dQIS - dCurrentStockLevel, false);
                }
                else if (dQIS < dCurrentStockLevel)
                {
                    GetPassword();
                    sEngine.TransferStockItem(sShopCode, "BH", sBarcode, dCurrentStockLevel -dQIS, false);
                }
                if (sSupplier != "")
                    sEngine.AddSupplierForItem(sBarcode, sSupplier, dSupCost.ToString(), sSupCode);

            }
            else if (sType == "2")
            {
                try
                {
                    sEngine.AddEditItem(sShopCode, sBarcode, sDesc, sType,
                        sCategory, sVAT, sMinQty, dCatTwoItemCost, Convert.ToDecimal(sEngine.GetItemStockStaRecord(sBarcode, sShopCode)[39])); // Last field should be target margin
                }
                catch
                {
                    sEngine.AddEditItem(sShopCode, sBarcode, sDesc, sType,
                        sCategory, sVAT, sMinQty, dCatTwoItemCost, 0.00m); // Last field should be target margin

                }
            }
            else if (sType == "5")
            {
                sEngine.AddEditItem(sShopCode, sBarcode, sDesc,
                    sType, sCategory, sRRP, sVAT,
                    sMinQty, sParentCode, dCPartQTY.ToString());
            }
            else if (sType == "6")
            {
                sEngine.AddEditItem(sShopCode, sBarcode, sDesc,
                    sType, sCategory, sRRP, sVAT,
                    sMinQty, sSupplier, dSupCost, "0");
            }
        }
        bool CheckRow(int nRow)
        {
            return true;
        }

    }
}
