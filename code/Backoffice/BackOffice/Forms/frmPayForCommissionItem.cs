using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackOffice
{
    class frmPayForCommissionItem
    {
        public frmPayForCommissionItem(ref StockEngine sEngine)
        {
            // Get the barcode of the commission item
            frmSingleInputBox fsfiGetCode = new frmSingleInputBox("Enter the Barcode of the commission item. Press F5 to look up:", ref sEngine);
            fsfiGetCode.ShowDialog();
            if (fsfiGetCode.Response != "$NONE")
            {
                frmSingleInputBox fsfiGetQtySold = new frmSingleInputBox("Enter the quantity of items that you are paying for:", ref sEngine);
                fsfiGetQtySold.ShowDialog();
                if (fsfiGetQtySold.Response != "$NONE")
                {
                    decimal dQtyPaid = -1;
                    try
                    {
                        dQtyPaid = Convert.ToDecimal(fsfiGetQtySold.Response);
                    }
                    catch
                    {
                        System.Windows.Forms.MessageBox.Show("Invalid number entered.");
                        return;
                    }
                    frmSingleInputBox fGetAmount = new frmSingleInputBox("Enter the total amount that you paid for the items:", ref sEngine);
                    fGetAmount.ShowDialog();
                    if (fGetAmount.Response != "$NONE")
                    {
                        decimal dAmountPaid = -1;
                        try
                        {
                            dAmountPaid = Convert.ToDecimal(fGetAmount.Response);
                        }
                        catch
                        {
                            System.Windows.Forms.MessageBox.Show("Invalid amount entered.");
                            return;
                        }
                        frmListOfShops flos = new frmListOfShops(ref sEngine);
                        flos.ShowDialog();
                        if (flos.SelectedShopCode != "$NONE")
                        {
                            // Code here to straighten out the stock files
                            if (sEngine.MarkCommissionItemAsPaid(fsfiGetCode.Response, dQtyPaid, dAmountPaid, flos.SelectedShopCode))
                                System.Windows.Forms.MessageBox.Show(dQtyPaid.ToString() + " " + sEngine.GetMainStockInfo(fsfiGetCode.Response)[1] + " paid for at a total of " + dAmountPaid.ToString());
                            else
                                System.Windows.Forms.MessageBox.Show("Unfortunately the items couldn't be paid for. This could be because the total you've ever received is less than the total you're trying to pay for");
                        }
                        else
                            return;
                    }
                }
            
            }
        }
    }
}
