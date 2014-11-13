using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice
{
    public class Item
    {
        /// <summary>
        /// The description of the item
        /// </summary>
        string sItemDesc;
        /// <summary>
        /// The item's barcode
        /// </summary>
        string sBarcode;
        /// <summary>
        /// The gross price of the item
        /// </summary>
        decimal fGrossAmnt;
        /// <summary>
        /// The final amount of the item
        /// </summary>
        decimal fFinalAmount;
        /// <summary>
        /// The category of V.A.T. that the item is in
        /// </summary>
        string sVATCategory;
        /// <summary>
        /// Whether or not this is a stock item
        /// </summary>
        bool bStockItem;
        /// <summary>
        /// The product category that the item's in (stock, department etc)
        /// </summary>
        int nCategory;
        /// <summary>
        /// The quantity of item
        /// </summary>
        decimal nQuantity;
        /// <summary>
        /// The category that the item is in (user defined categories)
        /// </summary>
        string sCategory;
        /// <summary>
        /// Whether or not the item is discontinued, NOT DISCOUNTED!!
        /// </summary>
        public bool bDiscontinued;
        /// <summary>
        /// Whether or not the record was found with the item's details
        /// </summary>
        bool bItemExists;
        /// <summary>
        /// The V.A.T. code of the item (I1, Z0 etc)
        /// </summary>
        string sVATCode;
        /// <summary>
        /// The current stock level of the item
        /// </summary>
        decimal nCurrentStockLevel;
        /// <summary>
        /// Whether or not the item's price has been discounted
        /// </summary>
        bool bDiscounted;
        /// <summary>
        /// If there is a parent item, then the barcode of it
        /// </summary>
        public string ParentBarcode;
        /// <summary>
        /// The code of the shop that the item's in
        /// </summary>
        public string ShopCode;

        /// <summary>
        /// Intilailises the item
        /// </summary>
        /// <param name="sRecordContents">The contents of the STOCK database record for this item</param>
        public Item(string[] sRecordContents, string[] SSTCKRecord)
        {
            if (sRecordContents[0] == null)
            {
                bItemExists = false;
            }
            else
            {
                sBarcode = sRecordContents[0];
                sItemDesc = sRecordContents[1];
                nCategory = Convert.ToInt32(sRecordContents[5]);
                fGrossAmnt = (decimal)Convert.ToDecimal(sRecordContents[2]);
                fGrossAmnt = (decimal)Math.Round((decimal)fGrossAmnt, 2);
                fFinalAmount = fGrossAmnt;
                sVATCategory = sRecordContents[3];
                sCategory = sRecordContents[4];
                if (nCategory == 1 || nCategory == 5)
                    bStockItem = true;
                else
                    bStockItem = false;
                if (SSTCKRecord[36] == "")
                    nCurrentStockLevel = 0;
                else
                    nCurrentStockLevel = Convert.ToDecimal(SSTCKRecord[36]);
                if (sRecordContents[5].StartsWith("ZZ"))
                    bDiscontinued = true;
                else
                    bDiscontinued = false;

                bItemExists = true;
                bDiscounted = false;
                ParentBarcode = sRecordContents[7];
                ShopCode = SSTCKRecord[35];
            }


        }

        /// <summary>
        /// The quantity of the item
        /// </summary>
        public decimal Quantity
        {
            get
            {
                return nQuantity;
            }
            set
            {
                nQuantity = value;
            }
        }

        /// <summary>
        /// The amount of the item
        /// </summary>
        public decimal Amount
        {
            get
            {
                return fFinalAmount;
            }
        }

        /// <summary>
        /// The gross amount (before discounts etc)
        /// </summary>
        public decimal GrossAmount
        {
            get
            {
                return fGrossAmnt;
            }
            set
            {
                fGrossAmnt = value;
                fFinalAmount = fGrossAmnt;
            }
        }

        /// <summary>
        /// The barcode of the item
        /// </summary>
        public string Barcode
        {
            get
            {
                return sBarcode;
            }
        }

        /// <summary>
        /// Whether or not the item is stock
        /// </summary>
        public bool IsItemStock
        {
            get
            {
                return bStockItem;
            }
        }

        /// <summary>
        /// The V.A.T. category
        /// </summary>
        public string VATRate
        {
            get
            {
                return sVATCategory;
            }
        }

        /// <summary>
        /// The description of the item
        /// </summary>
        public string Description
        {
            get
            {
                return sItemDesc;
            }
            set
            {
                if (nCategory == 4)
                    sItemDesc = value;
            }
        }

        /*
        public bool GetIsDiscontinued()
        {
            return bDiscontinued;
        }*/

        /// <summary>
        /// The current stock level of the item
        /// </summary>
        public decimal StockLevel
        {
            get
            {
                if (nCurrentStockLevel == null)
                    return 0;
                else
                    return nCurrentStockLevel;
            }
            set
            {
                nCurrentStockLevel = value;
            }
        }

        /// <summary>
        /// Set the price of the item
        /// </summary>
        /// <param name="fGrossAmount">The new price</param>
        public void SetPrice(decimal fGrossAmount)
        {
            if (fGrossAmnt == 0)
            {
                if (nCategory == 2 || nCategory == 4)
                {
                    fGrossAmnt = fGrossAmount;
                    fFinalAmount = fGrossAmnt;
                }
            }
            else
            {
                fFinalAmount = fGrossAmount;
            }
        }

        /// <summary>
        /// Gets the category of the item
        /// </summary>
        public int ItemCategory
        {
            get
            {
                return nCategory;
            }
        }

        /// <summary>
        /// Discounts an amount from the price
        /// </summary>
        /// <param name="fAmount">The amount to discount</param>
        public void DiscountAmountFromNet(decimal fAmount)
        {
            fFinalAmount -= fAmount;
            bDiscounted = true;
        }

        /// <summary>
        /// Whether or not the item has been discounted
        /// </summary>
        public bool Discounted
        {
            get
            {
                return bDiscounted;
            }
        }

        public string CodeCategory
        {
            get
            {
                return sCategory;
            }
        }
    }
}
