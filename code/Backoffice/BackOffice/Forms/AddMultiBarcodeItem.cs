using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackOffice
{
    class AddMultiBarcodeItem
    {
        public string Barcode;

        public AddMultiBarcodeItem(ref StockEngine sEngine)
        {

            frmSingleInputBox fsiGetBarcode = new frmSingleInputBox("Enter the barcode for the new multi-barcode item", ref sEngine);
            fsiGetBarcode.ShowDialog();
            if (fsiGetBarcode.Response != "$NONE")
            {
                Barcode = fsiGetBarcode.Response;
                frmSingleInputBox fsiGetDesc = new frmSingleInputBox("Enter the description for the new multi-barcode item", ref sEngine);
                fsiGetDesc.ShowDialog();
                if (fsiGetDesc.Response != "$NONE")
                {
                    frmListOfShops flos = new frmListOfShops(ref sEngine);
                    flos.ShowDialog();
                    frmListOfTills flot = new frmListOfTills(ref sEngine, flos.SelectedShopCode);
                    flot.ShowDialog();
                    System.Windows.Forms.MessageBox.Show("When the till is free, it will temporarily move to this computer. Enter the number 0 as your ID, and enter the transaction as you would like it to appear when you enter " + fsiGetBarcode.Response + " at the till. Then press the space bar and the till program will quit back to this", "Instructions", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                    sEngine.RunTillSoftware();
                    string[] sData = sEngine.GetStoredTransactionFromTill(Convert.ToInt32(flot.sSelectedTillCode));
                    int nOfLines = 0;
                    foreach (string line in sData)
                    {
                        if (line.Contains(','))
                            nOfLines++;
                    }
                    string[] sBarcodes = new string[nOfLines];
                    decimal[] dQuantities = new decimal[nOfLines];
                    decimal[] dAmountPerItem = new decimal[nOfLines];

                    for (int i = 0; i < nOfLines; i++)
                    {
                        string[] sTemp = sData[i].Split(',');
                        sBarcodes[i] = sTemp[0];
                        dQuantities[i] = Convert.ToDecimal(sTemp[1]);
                        dAmountPerItem[i] = Convert.ToDecimal(sTemp[2]) / dQuantities[i];
                    }

                    sEngine.AddMultiItemItem(fsiGetBarcode.Response, fsiGetDesc.Response, flos.SelectedShopCode, sBarcodes, dQuantities, dAmountPerItem);

                    if (System.Windows.Forms.MessageBox.Show("Upload changes to all tills now?", "Upload?", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        sEngine.CopyWaitingFilesToTills();
                    }
                }
                else
                {
                    Barcode = "$NULL";
                }
            }
            else
            {
                Barcode = "$NULL";
            }
        }
    }
}
