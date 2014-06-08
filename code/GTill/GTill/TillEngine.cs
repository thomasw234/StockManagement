using System;
using DBFDetailsViewerV2;
using System.IO;

namespace TillEngine
{
    /// <summary>
    /// An item object
    /// </summary>
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
        float fGrossAmnt;
        /// <summary>
        /// The final amount of the item
        /// </summary>
        float fFinalAmount;
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
        int nQuantity;
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
        float nCurrentStockLevel;
        /// <summary>
        /// Whether or not the item's price has been discounted
        /// </summary>
        bool bDiscounted;
        /// <summary>
        /// If there is a parent item, then the barcode of it
        /// </summary>
        public string ParentBarcode;

        public float fQuantityOnOrder;
        public string sDueDate;

        /// <summary>
        /// Intilailises the item
        /// </summary>
        /// <param name="sRecordContents">The contents of the STOCK database record for this item</param>
        public Item(string[] sRecordContents)
        {
            if (sRecordContents[0] == null)
            {
                bItemExists = false;
            }
            else
            {
                sBarcode = sRecordContents[0];
                sItemDesc = sRecordContents[1];
                nCategory = Convert.ToInt32(sRecordContents[3]);
                fGrossAmnt = (float)Convert.ToDecimal(sRecordContents[4]);
                fGrossAmnt = (float)Math.Round((decimal)fGrossAmnt, 2);
                fFinalAmount = fGrossAmnt;
                sVATCategory = sRecordContents[6];
                sCategory = sRecordContents[9];
                if (nCategory == 1 || nCategory == 5)
                    bStockItem = true;
                else
                    bStockItem = false;
                nQuantity = 0;
                if (sRecordContents[9].StartsWith("ZZ"))
                    bDiscontinued = true;
                else
                    bDiscontinued = false;

                bItemExists = true;
                bDiscounted = false;
                ParentBarcode = sRecordContents[7];
            }


        }

        /// <summary>
        /// Initialises the item with stock level
        /// </summary>
        /// <param name="sRecordContents">The record contents of the STOCK database for this item</param>
        /// <param name="sSTKLevelContents">The record contents of the STKLEVEL database for this item</param>
        public Item(string[] sRecordContents, string[] sSTKLevelContents)
        {
            if (sRecordContents[0] == null)
            {
                bItemExists = false;
            }
            else
            {
                sBarcode = sRecordContents[0];
                sItemDesc = sRecordContents[1];
                nCategory = Convert.ToInt32(sRecordContents[3]);
                fGrossAmnt = (float)Convert.ToDecimal(sRecordContents[4]);
                fGrossAmnt = (float)Math.Round((decimal)fGrossAmnt, 2);
                fFinalAmount = fGrossAmnt;
                sVATCategory = sRecordContents[6];
                sCategory = sRecordContents[9];
                if (fGrossAmnt == 0.0f)
                    bStockItem = false;
                nQuantity = 0;
                if (sRecordContents[9].StartsWith("ZZ"))
                    bDiscontinued = true;
                else
                    bDiscontinued = false;

                bItemExists = true;
                bDiscounted = false;
                ParentBarcode = sRecordContents[7];
                nCurrentStockLevel = Convert.ToInt32((float)Convert.ToDecimal(sSTKLevelContents[2]));
                fQuantityOnOrder = (float)Convert.ToDecimal(sSTKLevelContents[4]);
                if (sSTKLevelContents[5].Length > 1)
                    sDueDate = sSTKLevelContents[5][0].ToString() + sSTKLevelContents[5][1].ToString() + "/" + sSTKLevelContents[5][2].ToString() + sSTKLevelContents[5][3].ToString() + "/" + sSTKLevelContents[5][4].ToString() + sSTKLevelContents[5][5].ToString();
                else
                    sDueDate = "Unknown";
            }
        }

        /// <summary>
        /// The quantity of the item
        /// </summary>
        public int Quantity
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
        public float Amount
        {
            get
            {
                fFinalAmount = TillEngine.FixFloatError(fFinalAmount);
                return fFinalAmount;
            }
        }

        /// <summary>
        /// The gross amount (before discounts etc)
        /// </summary>
        public float GrossAmount
        {
            get
            {
                fGrossAmnt = TillEngine.FixFloatError(fGrossAmnt);
                return fGrossAmnt;
            }
            set
            {
                fGrossAmnt = TillEngine.FixFloatError(value);
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
        public float StockLevel
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
        public void SetPrice(float fGrossAmount)
        {
            if (fGrossAmnt == 0.0f)
            {
                if (nCategory == 2 || nCategory == 4)
                {
                    fGrossAmnt = TillEngine.FixFloatError(fGrossAmount);
                    fFinalAmount = fGrossAmnt;
                }
            }
            else
            {
                fFinalAmount = TillEngine.FixFloatError(fGrossAmount);
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
        public void DiscountAmountFromNet(float fAmount)
        {
            fFinalAmount -= fAmount;
            fFinalAmount = TillEngine.FixFloatError(fFinalAmount);
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

    /// <summary>
    /// Contains details about payment methods
    /// </summary>
    public class PaymentMethod
    {
        /// <summary>
        /// The code of the payment method
        /// </summary>
        string sPMName;
        /// <summary>
        /// The amount paid
        /// </summary>
        float fAmount;
        /// <summary>
        /// The amount paid without change given
        /// If £6 is due, the customer may give £10. This variable will hold 10, fAmount will hold 6 
        /// </summary>
        float fGross = 0.0f;

        /// <summary>
        /// Sets up the payment method
        /// </summary>
        /// <param name="name">The code of the payment method</param>
        /// <param name="flAmount">The amount paid using this payment method</param>
        public void SetPaymentMethod(string name, float flAmount)
        {
            sPMName = name;
            fAmount = flAmount;
        }
        /// <summary>
        /// Sets up the payment method
        /// </summary>
        /// <param name="name">The code of the payment method</param>
        /// <param name="flAmount">The final amount paid</param>
        /// <param name="flGross">The gross amount paid (possibly in excess of the amount due)</param>
        public void SetPaymentMethod(string name, float flAmount, float flGross)
        {
            sPMName = name;
            fAmount = flAmount;
            fGross = TillEngine.FixFloatError(flGross + flAmount);
        }

        /// <summary>
        /// The amount paid using this payment method
        /// </summary>
        public float Amount
        {
            get
            {
                return fAmount;
            }
        }

        /// <summary>
        /// The total amount received on this payment method, including excess
        /// </summary>
        public float Excess
        {
            get
            {
                return fGross;
            }
        }

        /// <summary>
        /// Gets the payment code
        /// </summary>
        public string PMType
        {
            get
            {
                return sPMName;
            }
        }
    }

    /// <summary>
    /// A transaction
    /// </summary>
    class Transaction
    {
        /// <summary>
        /// An array of items
        /// </summary>
        Item[] iItems;
        /// <summary>
        /// An array of payment methods
        /// </summary>
        PaymentMethod[] pmPayMethod;
        /// <summary>
        /// Whether or not the transaction has been paid for
        /// </summary>
        bool bAllPaidFor = false;
        /// <summary>
        /// The current till settings
        /// </summary>
        TillSettings currentSettings;
        /// <summary>
        /// The current transaction number
        /// </summary>
        int nTransactionNumber = 0;
        /// <summary>
        /// Whether or not the last item was successfully added
        /// </summary>
        bool bLastItemAddedSuccessfully;
        /// <summary>
        /// The amount of excess paid (or change due)
        /// </summary>
        float fExcessPaid = 0.0f;
        /// <summary>
        /// The main V.A.T. rate
        /// </summary>
        float fMainVATRate = 0.0f;

        int nDiscountThreshold = 0;

        Table tCommission;

        /// <summary>
        /// Initialises the transaction
        /// </summary>
        /// <param name="ts">The till settings</param>
        /// <param name="nTransactionNum">The transaction number</param>
        public Transaction(TillSettings ts, int nTransactionNum, ref Table tComm, int nDiscountThreshold)
        {
            tCommission = tComm;
            iItems = new Item[0];
            pmPayMethod = new PaymentMethod[0];
            currentSettings = ts;
            nTransactionNumber = nTransactionNum;
            this.nDiscountThreshold = nDiscountThreshold;
        }

        /// <summary>
        /// Gets the payment methods of the transaction
        /// </summary>
        public PaymentMethod[] PaymentMethods
        {
            get
            {
                return pmPayMethod;
            }
        }

        /// <summary>
        /// Gets the transaction number
        /// </summary>
        public int TransactionNumber
        {
            get
            {
                return nTransactionNumber;
            }
            set
            {
                nTransactionNumber = value;
            }
        }

        /// <summary>
        /// Gets of sets the main V.A.T. rate
        /// </summary>
        public float VATRate
        {
            get
            {
                return fMainVATRate;
            }
            set
            {
                fMainVATRate = value;
            }
        }

        /// <summary>
        /// Adds an item to the transaction
        /// </summary>
        /// <param name="sBarcode">The barcode of the item to add</param>
        /// <param name="StockDotDBF">A reference to the STOCK database</param>
        public void AddItemToTransaction(string sBarcode, ref Table StockDotDBF)
        {
            string[] sRecordContents = StockDotDBF.GetRecordFrom(sBarcode.ToUpper(), 0, true);
            if (sRecordContents.Length > 1)
            {
                Item[] currentItem = iItems;
                iItems = new Item[currentItem.Length + 1];
                for (int i = 0; i < currentItem.Length; i++)
                {
                    iItems[i] = currentItem[i];
                }
                iItems[iItems.Length - 1] = new Item(sRecordContents);
                iItems[iItems.Length - 1].Quantity = 1;
                if (iItems[iItems.Length - 1].VATRate == "E1")
                {
                    float fVATRate = fMainVATRate; // Need to add on VAT
                    fVATRate = TillEngine.FixFloatError((fVATRate / 100) + 1.0f);
                    iItems[iItems.Length - 1].GrossAmount = (TillEngine.FixFloatError(iItems[iItems.Length - 1].GrossAmount * fVATRate));
                }
                bLastItemAddedSuccessfully = true;
            }
            else
                bLastItemAddedSuccessfully = false;
        }
        
        /// <summary>
        /// Adds a payment method to the transaction
        /// </summary>
        /// <param name="sName">The code of the payment method</param>
        /// <param name="fAMNT">The amount</param>
        public void AddPaymentMethod(string sName, float fAMNT)
        {
            AddPaymentMethod(sName, fAMNT, 0.0f);
        }
        /// <summary>
        /// Adds a payment method to the transaction
        /// </summary>
        /// <param name="sName">The code of the payment method</param>
        /// <param name="fAMNT">The amount</param>
        /// <param name="fExcess">The excess</param>
        public void AddPaymentMethod(string sName, float fAMNT, float fExcess)
        {
            float fTotalAmount = 0.0f;
            PaymentMethod[] temp = pmPayMethod;
            pmPayMethod = new PaymentMethod[temp.Length + 1];
            for (int i = 0; i < temp.Length; i++)
            {
                pmPayMethod[i] = temp[i];
                fTotalAmount += pmPayMethod[i].Amount;
            }
            pmPayMethod[temp.Length] = new PaymentMethod();
            pmPayMethod[temp.Length].SetPaymentMethod(sName, fAMNT, fExcess);
            fTotalAmount += pmPayMethod[temp.Length].Amount;

            float fTotalDue = 0.0f;

            for (int i = 0; i < iItems.Length; i++)
            {
                fTotalDue += iItems[i].Amount;
            }
            fTotalAmount = TillEngine.FixFloatError(fTotalAmount);
            fTotalDue = TillEngine.FixFloatError(fTotalDue);

            if (fTotalAmount >= fTotalDue)
            {
                bAllPaidFor = true;
                if (GetStillDue() <= 0.0f)
                {
                    pmPayMethod[temp.Length].SetPaymentMethod(sName, TillEngine.FixFloatError(fAMNT + GetStillDue()));
                    fExcessPaid = TillEngine.FixFloatError(fAMNT - pmPayMethod[temp.Length].Amount);
                }
            }
            else
                bAllPaidFor = false;
        }

        /// <summary>
        /// Makes a quick backup of the databases
        /// </summary>
        /// <param name="repData">A reference to the REPDATA database</param>
        /// <param name="tData">A reference to the TDATA database</param>
        /// <param name="tHdr">A reference to the THDR database</param>
        private void BackupDatabases(ref Table repData, ref Table tData, ref Table tHdr)
        {
            repData.SaveToFile("REPDATA_PREV.DBF");
            tData.SaveToFile("TDATA_PREV.DBF");
            tHdr.SaveToFile("THDR_PREV.DBF");
        }

        /// <summary>
        /// Saves the transaction to the databases
        /// </summary>
        /// <param name="repData">A reference to the REPDATA database</param>
        /// <param name="tData">A reference to the TDATA database</param>
        /// <param name="tHdr">A reference to the THDR database</param>
        public void SaveTransaction(ref Table repData, ref Table tData, ref Table tHdr, ref Table tVAT, ref Table tCommission) 
        {
            // Make a backup first
            BackupDatabases(ref repData, ref tData, ref tHdr);
            string sChargeToAccountCode = "";
            float fCat01VATPaid = 0.0f;
            // REPDATA.DBF :
            foreach (Item item in iItems)
            {
                // Make a note for debugging that the user is putting in DEP (Deposit)
                if (item.Barcode.StartsWith("DEP"))
                {
                    GTill.ErrorHandler.LogMessage("(Transaction SaveTransaction) : Saving item with barcode DEP, amount " + item.Amount.ToString() + ", quantity " + item.Quantity.ToString());
                }
                // Go through each item and add it to REPDATA
                int nRecordNum = 0;
                bool recordAlreadyExists = repData.SearchForRecord(item.Barcode, 1, ref nRecordNum);
                if (recordAlreadyExists)
                {
                    // Get the current details about this item from REPDATA
                    string[] currentContents = repData.GetRecordFrom(item.Barcode, 1, true);

                    int nCurrentQTY = Convert.ToInt32(currentContents[2]);
                    int nNewQTY = nCurrentQTY + item.Quantity;

                    float fCurrentAmount = (float)Convert.ToDecimal(currentContents[3]);
                    fCurrentAmount += item.Amount; // Increase the amount for this product
                    fCurrentAmount = TillEngine.FixFloatError(fCurrentAmount);

                    float fGrossAmount = (float)Convert.ToDecimal(currentContents[4]);

                    currentContents[2] = nNewQTY.ToString();
                    currentContents[3] = fCurrentAmount.ToString();
                    currentContents[4] = TillEngine.FixFloatError(fGrossAmount + (item.GrossAmount * item.Quantity)).ToString();

                    for (int i = 2; i < 5; i++)
                    {
                        // Update REPDATA with the modified item details
                        repData.EditRecordData(nRecordNum, i, currentContents[i]);
                    }
                }
                else
                {
                    // Add information about the item to REPDATA
                    string[] toAdd = new string[5];
                    toAdd[0] = "ST";
                    toAdd[1] = item.Barcode;
                    toAdd[2] = item.Quantity.ToString();
                    toAdd[3] = item.Amount.ToString();
                    toAdd[4] = (item.GrossAmount * item.Quantity).ToString();

                    repData.AddRecord(toAdd);
                }

                if (item.IsItemStock) // Add data to the STOCK record in REPDATA
                {
                    repData.SearchForRecord("STOCK", 1, ref nRecordNum);
                    string[] currentContents = repData.GetRecordFrom("STOCK", 1);

                    int nCurrentQuantity = Convert.ToInt32(currentContents[2]);
                    nCurrentQuantity += item.Quantity;

                    float fCurrentAmnt = (float)Convert.ToDecimal(currentContents[3]);
                    fCurrentAmnt += item.Amount;
                    fCurrentAmnt = TillEngine.FixFloatError(fCurrentAmnt);

                    currentContents[2] = nCurrentQuantity.ToString();
                    currentContents[3] = fCurrentAmnt.ToString();

                    repData.EditRecordData(nRecordNum, 2, currentContents[2]);
                    repData.EditRecordData(nRecordNum, 3, currentContents[3]);
                }
                else // Add data to the NSTCK record in REPDATA
                {
                    repData.SearchForRecord("NSTCK", 1, ref nRecordNum);
                    string[] currentContents = repData.GetRecordFrom("NSTCK", 1);

                    int nCurrentQuantity = Convert.ToInt32(currentContents[2]);
                    nCurrentQuantity += item.Quantity;

                    float fCurrentAmnt = (float)Convert.ToDecimal(currentContents[3]);
                    fCurrentAmnt += item.Amount;
                    fCurrentAmnt = TillEngine.FixFloatError(fCurrentAmnt);

                    currentContents[2] = nCurrentQuantity.ToString();
                    currentContents[3] = fCurrentAmnt.ToString();

                    repData.EditRecordData(nRecordNum, 2, currentContents[2]);
                    repData.EditRecordData(nRecordNum, 3, currentContents[3]);
                }

                int nNITEMPos = 0;
                repData.SearchForRecord("NOITEM", 1, ref nNITEMPos); // Search for and update the Number of items record (NOITEM) in REPDATA
                int nOfItem = Convert.ToInt32(repData.GetRecordFrom("NOITEM", 1)[2]);
                nOfItem += item.Quantity;
                float fCurrentItemAmount = (float)Convert.ToDecimal(repData.GetRecordFrom("NOITEM", 1)[3]);
                fCurrentItemAmount += item.Amount;
                fCurrentItemAmount = TillEngine.FixFloatError(fCurrentItemAmount);

                repData.EditRecordData(nNITEMPos, 2, nOfItem.ToString());
                repData.EditRecordData(nNITEMPos, 3, fCurrentItemAmount.ToString());

                // Work out the V.A.T. record to update, and then update it
                if (tVAT == null)
                {
                    int nVATPOS = 0;
                    string sVATNum = "VAT";
                    if (item.VATRate == "X0")
                    {
                        sVATNum += "00";
                    }
                    else if (item.VATRate == "Z0")
                    {
                        sVATNum += "02";
                    }
                    else if (item.VATRate == "I1" || item.VATRate == "E1")
                    {
                        sVATNum += "01";
                        fCat01VATPaid += item.Amount;
                        fCat01VATPaid = TillEngine.FixFloatError(fCat01VATPaid);
                    }
                    if (!repData.SearchForRecord(sVATNum, 1, ref nVATPOS))
                    {
                        string[] toAdd = { "CR", sVATNum, "0", "0", "0" };
                        repData.AddRecord(toAdd);
                        repData.SearchForRecord(sVATNum, 1, ref nVATPOS);
                    }
                    float fCurrentVAT = (float)Convert.ToDecimal(repData.GetRecordFrom(sVATNum, 1)[3]);
                    fCurrentVAT += item.Amount;
                    fCurrentVAT = TillEngine.FixFloatError(fCurrentVAT);
                    repData.EditRecordData(nVATPOS, 3, fCurrentVAT.ToString());
                }
                else
                {
                    int nVatRecNum = -1;
                    if (!repData.SearchForRecord("VAT" + item.VATRate, 1, ref nVatRecNum))
                    {
                        string[] sToAdd = { "CR", "VAT" + item.VATRate, "0", "0", "0" };
                        repData.AddRecord(sToAdd);
                        repData.SearchForRecord("VAT" + item.VATRate, 1, ref nVatRecNum);
                    }
                    float fCurrentAmount = (float)Convert.ToDecimal(repData.GetRecordFrom(nVatRecNum)[3]);
                    if (item.ItemCategory != 6)
                    {
                        fCurrentAmount += item.Amount;
                        fCurrentAmount = TillEngine.FixFloatError(fCurrentAmount);
                        repData.EditRecordData(nVatRecNum, 3, fCurrentAmount.ToString());
                    }
                    else
                    {
                        float fCostPrice = (float)Convert.ToDecimal(tCommission.GetRecordFrom(item.Barcode, 0, true)[1]);
                        fCurrentAmount += (item.Amount - fCostPrice);
                        fCurrentAmount = TillEngine.FixFloatError(fCurrentAmount);
                        repData.EditRecordData(nVatRecNum, 3, fCurrentAmount.ToString());

                        float fNoVAT = fCostPrice;
                        fNoVAT = TillEngine.FixFloatError(fNoVAT);
                        // Commission VAT Code is X0 here
                        nVatRecNum = -1;
                        if (!tVAT.SearchForRecord("VATX0", 0, ref nVatRecNum))
                        {
                            string[] sToAdd = { "CR", "VATX0", "0", "0", "0" };
                            repData.AddRecord(sToAdd);
                            repData.SearchForRecord("VATX0", 1, ref nVatRecNum);
                        }
                        fCurrentAmount = (float)Convert.ToDecimal(repData.GetRecordFrom(nVatRecNum)[3]);
                        fCurrentAmount += fCostPrice;
                        repData.EditRecordData(nVatRecNum, 3, fCurrentAmount.ToString());
                    }

                }
            }

            foreach (PaymentMethod paymentMethod in pmPayMethod)
            {
                // Go through each payment method and add it to REPDATA
                // This is the generic add, there are some specifics for certain payment methods as seen below this section

                int nRecordPos = 0;
                if (paymentMethod.PMType.Split(',').Length == 2)
                {
                    sChargeToAccountCode = paymentMethod.PMType.Split(',')[1];
                    paymentMethod.SetPaymentMethod(paymentMethod.PMType.Split(',')[0], paymentMethod.Amount);
                }
                if (!repData.SearchForRecord(paymentMethod.PMType.Split(',')[0], 1, ref nRecordPos))
                {
                    string[] sCheque = { "CR", paymentMethod.PMType, "0", "0", "0" };
                    if (sCheque[1].StartsWith("CRCD"))
                    {
                        sCheque[1] = "CRCD";
                        if (!repData.SearchForRecord("CRCD", 1, ref nRecordPos))
                        {
                            repData.AddRecord(sCheque);
                            repData.SearchForRecord("CRCD", 1, ref nRecordPos);
                        }
                    }
                    else
                        repData.AddRecord(sCheque);
                    if (sCheque[1] != "CRCD")
                        repData.SearchForRecord(paymentMethod.PMType, 1, ref nRecordPos);
                    else
                        repData.SearchForRecord("CRCD", 1, ref nRecordPos);
                }

                string[] chequeFields = repData.GetRecordFrom(paymentMethod.PMType, 1);
                if (paymentMethod.PMType.StartsWith("CRCD"))
                    chequeFields = repData.GetRecordFrom("CRCD", 1);
                int nQuantity = Convert.ToInt32(chequeFields[2]);
                nQuantity++;
                float fAmount = (float)Convert.ToDecimal(chequeFields[3].TrimEnd('\0'));
                fAmount += paymentMethod.Amount;
                fAmount = TillEngine.FixFloatError(fAmount);

                chequeFields[2] = nQuantity.ToString();
                chequeFields[3] = fAmount.ToString();

                repData.EditRecordData(nRecordPos, 2, chequeFields[2]);
                repData.EditRecordData(nRecordPos, 3, chequeFields[3]);

                // End of Generic
                
                // Specialises now for credit/debit cards first
                if (paymentMethod.PMType.StartsWith("CRCD"))
                {
                    int nCRDRecordPos = 0;
                    int nCreditCardType = Convert.ToInt32(paymentMethod.PMType[4].ToString());
                    string sCRDFieldName = "CRD" + nCreditCardType.ToString();
                    if (!repData.SearchForRecord(sCRDFieldName, 1, ref nCRDRecordPos))
                    {
                        string[] sToAdd = { "CR", sCRDFieldName, "0", "0", "0" };
                        repData.AddRecord(sToAdd);
                        repData.SearchForRecord(sCRDFieldName, 1, ref nCRDRecordPos);
                    }

                    string[] crdFields = repData.GetRecordFrom(sCRDFieldName, 1);
                    int nCRDQuantity = Convert.ToInt32(crdFields[2]);
                    nCRDQuantity += 1;
                    float fCRDAmount = (float)Convert.ToDecimal(crdFields[3]);
                    fCRDAmount += paymentMethod.Amount;
                    fCRDAmount = TillEngine.FixFloatError(fCRDAmount);

                    crdFields[2] = nCRDQuantity.ToString();
                    crdFields[3] = fCRDAmount.ToString();

                    repData.EditRecordData(nCRDRecordPos, 2, crdFields[2]);
                    repData.EditRecordData(nCRDRecordPos, 3, crdFields[3]);
                }
                    // Now specialises for deposit paid
                else if (paymentMethod.PMType == "DEPO")
                {
                    // Log that deposit paid has been used
                    GTill.ErrorHandler.LogMessage("(Transaction - SaveTransaction) : Staff number " + currentSettings.StaffNumber.ToString() + " is using deposit paid for transaction number " + nTransactionNumber.ToString() + ", amount " + paymentMethod.Amount);

                    int nDEPRecordNum = 0;
                    bool recordDEPAlreadyExists = repData.SearchForRecord("DEP", 1, ref nDEPRecordNum);
                    if (recordDEPAlreadyExists)
                    {
                        // Subtract amount from DEP record
                        string[] currentContents = repData.GetRecordFrom("DEP", 1, true);

                        int nCurrentQTY = Convert.ToInt32(currentContents[2]);
                        int nNewQTY = nCurrentQTY + 1;

                        float fCurrentAmount = (float)Convert.ToDecimal(currentContents[3]);
                        fCurrentAmount -= paymentMethod.Amount;

                        currentContents[2] = nNewQTY.ToString();
                        currentContents[3] = fCurrentAmount.ToString();
                        currentContents[4] = "0";

                        for (int i = 2; i < 5; i++)
                        {
                            repData.EditRecordData(nDEPRecordNum, i, currentContents[i]);
                        }
                    }
                    else
                    {
                        string[] toAdd = new string[5];
                        toAdd[0] = "ST";
                        toAdd[1] = "DEP";
                        toAdd[2] = "1";
                        toAdd[3] = (paymentMethod.Amount * -1).ToString();
                        toAdd[4] = "0";

                        repData.AddRecord(toAdd);
                    }

                    // Remove NSTCK
                    nDEPRecordNum = 0;
                    recordDEPAlreadyExists = repData.SearchForRecord("NSTCK", 1, ref nDEPRecordNum);
                    if (recordDEPAlreadyExists)
                    {
                        // Subtract amount from DEP record
                        string[] currentContents = repData.GetRecordFrom("NSTCK", 1, true);

                        int nCurrentQTY = Convert.ToInt32(currentContents[2]);
                        int nNewQTY = nCurrentQTY - 1;

                        float fCurrentAmount = (float)Convert.ToDecimal(currentContents[3]);
                        fCurrentAmount -= paymentMethod.Amount;

                        currentContents[2] = nNewQTY.ToString();
                        currentContents[3] = fCurrentAmount.ToString();
                        currentContents[4] = "0";

                        for (int i = 2; i < 5; i++)
                        {
                            repData.EditRecordData(nDEPRecordNum, i, currentContents[i]);
                        }
                    }
                    else
                    {
                        string[] toAdd = new string[5];
                        toAdd[0] = "CR";
                        toAdd[1] = "NSTCK";
                        toAdd[2] = "-1";
                        toAdd[3] = (paymentMethod.Amount * -1).ToString();
                        toAdd[4] = "0";

                        repData.AddRecord(toAdd);
                    }

                    // Remove any applicable V.A.T. that was paid

                    float fAmountOfVATToRemove = paymentMethod.Amount;

                    if (tVAT == null)
                    {
                        int nRecordPosition = 0;
                        if (!repData.SearchForRecord("VAT01", 1, ref nRecordPosition))
                        {
                            string[] sVAT01 = { "CR", "VAT01", "0", "0.00", "0.00" };
                            repData.AddRecord(sVAT01);
                            repData.SearchForRecord("VAT01", 1, ref nRecordPosition);
                        }

                        string[] sCurrentData = repData.GetRecordFrom("VAT01", 1);

                        float fCurrent = (float)Convert.ToDecimal(sCurrentData[3]);
                        fCurrent = TillEngine.FixFloatError(fCurrent);

                        fCurrent -= fAmountOfVATToRemove;
                        fCurrent = TillEngine.FixFloatError(fCurrent);

                        repData.EditRecordData(nRecordPosition, 3, fCurrent.ToString());
                    }
                    else
                    {
                        int nRecordPosition = 0;
                        if (!repData.SearchForRecord("VATI1", 1, ref nRecordPosition))
                        {
                            string[] sVAT01 = { "CR", "VATI1", "0", "0.00", "0.00" };
                            repData.AddRecord(sVAT01);
                            repData.SearchForRecord("VATI1", 1, ref nRecordPosition);
                        }

                        string[] sCurrentData = repData.GetRecordFrom("VATI1", 1);

                        float fCurrent = (float)Convert.ToDecimal(sCurrentData[3]);
                        fCurrent = TillEngine.FixFloatError(fCurrent);

                        fCurrent -= fAmountOfVATToRemove;
                        fCurrent = TillEngine.FixFloatError(fCurrent);

                        repData.EditRecordData(nRecordPosition, 3, fCurrent.ToString());
                    }
                }
                    // Specialises for charge to account
                else if (paymentMethod.PMType == "CHRG")
                {
                    // Remove the V.A.T. that was added, as V.A.T. is only paid upon receipt of money!

                    float fToRemove = GetAmountForVATRate("I1", false) + GetAmountForVATRate("E1", false);
                    fToRemove = TillEngine.FixFloatError(fToRemove);

                    if (fToRemove > paymentMethod.Amount)
                    {
                        fToRemove = paymentMethod.Amount;
                    }

                    if (!repData.SearchForRecord("VAT01", "REPCODE"))
                    {
                        string[] sToAdd = { "CR", "VAT01", "0", "0.00", "0.00" };
                        repData.AddRecord(sToAdd);
                    }

                    string[] sCurrentData = repData.GetRecordFrom("VAT01", 1);

                    int nRecordPosition = 0;
                    repData.SearchForRecord("VAT01", 1, ref nRecordPosition);

                    float fCurrent = (float)Convert.ToDecimal(sCurrentData[3]);
                    fCurrent = TillEngine.FixFloatError(fCurrent);

                    fCurrent -= fToRemove;
                    fCurrent = TillEngine.FixFloatError(fCurrent);

                    repData.EditRecordData(nRecordPosition, 3, fCurrent.ToString());

                    if (fCurrent <= 0.0f)
                        repData.DeleteRecord(nRecordPosition);

                    string[] sNewRecord = { "CA", "", "1", paymentMethod.Amount.ToString(), "0" };
                    string sAccountCodeToRecord = sChargeToAccountCode;
                    while (sAccountCodeToRecord.Length < 6)
                        sAccountCodeToRecord = sAccountCodeToRecord + " ";
                    string sTransNum = nTransactionNumber.ToString();
                    while (sTransNum.Length < 6)
                        sTransNum = "0" + sTransNum;
                    sNewRecord[1] = sAccountCodeToRecord + sTransNum;
                    repData.AddRecord(sNewRecord);
                    paymentMethod.SetPaymentMethod(paymentMethod.PMType+ "," + sChargeToAccountCode, paymentMethod.Amount);
                }

            }

            // Increase the number of transaction that has taken place
            int nNOTRANLoc = 0;
            repData.SearchForRecord("NOTRAN", 1, ref nNOTRANLoc);
            int fTranCount = Convert.ToInt32(repData.GetRecordFrom("NOTRAN", 1)[2]);
            fTranCount += 1;
            float fTranValue = (float)Convert.ToDecimal(repData.GetRecordFrom("NOTRAN", 1)[3]);
            fTranValue += TotalAmount;
            fTranValue = TillEngine.FixFloatError(fTranValue);
            repData.EditRecordData(nNOTRANLoc, 2, fTranCount.ToString());
            repData.EditRecordData(nNOTRANLoc, 3, fTranValue.ToString());

            // Increase the number of the last transaction of the day
            repData.SearchForRecord("END", 1, ref nNOTRANLoc);
            float fEnd = (float)Convert.ToDecimal(repData.GetRecordFrom("END", 1)[3]);
            fEnd += 0.01f;
            nTransactionNumber = Convert.ToInt32(fEnd * 100.0f);
            repData.EditRecordData(nNOTRANLoc, 3, fEnd.ToString());

            repData.SaveToFile(GTill.Properties.Settings.Default.sRepDataLoc);



            /*
             * End of REPDATA.DBF
             * 
             * Start of TDATA.DBF
             */
            
            // Add each item to tData
            for (int i = 0; i < iItems.Length; i++)
            {
                string[] toAdd = new string[10];

                string tNum = nTransactionNumber.ToString();
                while (tNum.Length < 6)
                    tNum = "0" + tNum;
                toAdd[0] = tNum;

                tNum = (i + 1).ToString();
                while (tNum.Length < 2)
                    tNum = "0" + tNum;
                toAdd[1] = tNum;

                toAdd[2] = "0";

                toAdd[3] = iItems[i].Barcode.TrimEnd(' ');

                toAdd[4] = iItems[i].Description.TrimEnd(' ');

                toAdd[5] = iItems[i].Quantity.ToString();

                toAdd[6] = TillEngine.FixFloatError(iItems[i].GrossAmount * iItems[i].Quantity).ToString();

                if (iItems[i].Discounted && iItems[i].ItemCategory == 1)
                    toAdd[7] = "1";
                else
                    toAdd[7] = "0";

                toAdd[8] = TillEngine.FixFloatError((iItems[i].GrossAmount * iItems[i].Quantity) - iItems[i].Amount).ToString();

                string sVATCode = iItems[i].VATRate;

                toAdd[9] = sVATCode;

                tData.AddRecord(toAdd);
            }

            tData.SaveToFile(GTill.Properties.Settings.Default.sTDataLoc);

            /*
             * End of TDATA.DBF
             * 
             * Start of THDR.DBF
             */
            // Add general information about the transaction to tHdr
            string[] nextRec = new string[9];
            nextRec[0] = nTransactionNumber.ToString();
            while (nextRec[0].Length < 6)
                nextRec[0] = "0" + nextRec[0];

            nextRec[1] = "00";
            nextRec[2] = "0";
            nextRec[3] = TotalAmount.ToString();
            nextRec[4] = "SALE";
            string sMonth = DateTime.Now.Month.ToString();
            if (sMonth.Length == 1)
                sMonth = "0" + sMonth;
            string sDay = DateTime.Now.Day.ToString();
            if (sDay.Length == 1)
                sDay = "0" + sDay;
            string sHour = DateTime.Now.Hour.ToString();
            while (sHour.Length < 2)
                sHour = "0" + sHour;
            string sMinute = DateTime.Now.Minute.ToString();
            while (sMinute.Length < 2)
                sMinute = "0" + sMinute;
            string sCurrentUser = currentSettings.StaffNumber.ToString();
            while (sCurrentUser.Length < 2)
                sCurrentUser = "0" + sCurrentUser;
            string sDescription = DateTime.Now.Year.ToString()[2].ToString() + DateTime.Now.Year.ToString()[3].ToString()
                                    + sMonth + sDay + sHour + sMinute + sCurrentUser;
            nextRec[5] = sDescription;

            nextRec[6] = "";
            nextRec[7] = "";
            nextRec[8] = "";

            tHdr.AddRecord(nextRec);

            // Add information about each of the payment methods to tHdr
            for (int i = 0; i < pmPayMethod.Length; i++)
            {
                nextRec = new string[9];
                nextRec[0] = nTransactionNumber.ToString();
                while (nextRec[0].Length < 6)
                    nextRec[0] = "0" + nextRec[0];
                string sLineNum = (i + 1).ToString();
                while (sLineNum.Length < 2)
                    sLineNum = "0" + sLineNum;
                nextRec[1] = sLineNum;
                nextRec[2] = "0";
                nextRec[3] = pmPayMethod[i].Amount.ToString();
                if (i == pmPayMethod.Length - 1)
                {
                    nextRec[3] = TillEngine.FixFloatError(pmPayMethod[i].Amount + fExcessPaid).ToString();
                }
                string sPaymethod = "", sDesc = "";
                if (pmPayMethod[i].PMType.StartsWith("CRCD"))
                {
                    sPaymethod = "CRCD";
                    sDesc = pmPayMethod[i].PMType[4].ToString();
                }
                else if (pmPayMethod[i].PMType == "CASH")
                    sPaymethod = "CASH";
                else if (pmPayMethod[i].PMType == "CHEQ")
                    sPaymethod = "CHEQ";
                else if (pmPayMethod[i].PMType == "VOUC")
                {
                    sPaymethod = "VOUC";
                    sDesc = sDescription;
                }
                else if (pmPayMethod[i].PMType == "DEPO")
                {
                    sPaymethod = "DEPO";
                    sDesc = sDescription;
                }
                else if (pmPayMethod[i].PMType.StartsWith("CHRG"))
                {
                    sPaymethod = "CHRG";
                    sDesc = sChargeToAccountCode;
                }

                nextRec[4] = sPaymethod;
                nextRec[5] = sDesc;
                nextRec[6] = "";
                nextRec[7] = "";
                nextRec[8] = "";

                tHdr.AddRecord(nextRec);
            }

            tHdr.SaveToFile(GTill.Properties.Settings.Default.sTHdrLoc);
        }

        /// <summary>
        /// Gets the total amount of the items in the transaction
        /// </summary>
        public float TotalAmount
        {
            get
            {
                float fTotal = 0.0f;
                foreach (Item i in iItems)
                    fTotal += i.Amount;
                return TillEngine.FixFloatError(fTotal);
            }
        }

        /// <summary>
        /// Gets the number of items in the transaction
        /// </summary>
        public int NumberOfItems
        {
            get
            {
                return iItems.Length;
            }
        }

        /// <summary>
        /// Gets the total number of items in the transaction (includes multiple quantities)
        /// </summary>
        public int NumberOfItemsPurchased
        {
            get
            {
                // This differs from the above, because it includes multiple quantities
                int nToReturn = 0;
                for (int i = 0; i < iItems.Length; i++)
                {
                    nToReturn += iItems[i].Quantity;
                }
                return nToReturn;
            }
        }

        /// <summary>
        /// Gets the item object from the iItems array
        /// </summary>
        /// <param name="posInItemArray">The item to get</param>
        /// <returns>The item from iItems</returns>
        public Item GetItemInTransaction(int posInItemArray)
        {
            return iItems[posInItemArray];
        }

        /// <summary>
        /// Gets whether or not the last item was successfully added (correct barcode)
        /// </summary>
        public bool ItemAddedSuccessfully
        {
            get
            {
                return bLastItemAddedSuccessfully;
            }
        }

        /// <summary>
        /// Sets the price of the last item
        /// </summary>
        /// <param name="fPrice">The price to set the last item at</param>
        public void SetLastItemPrice(float fPrice)
        {
            iItems[iItems.Length - 1].SetPrice(fPrice);
        }

        /// <summary>
        /// Sets the description of the last item
        /// </summary>
        /// <param name="sDesc">The description to set the last item to</param>
        public void SetLastItemDescription(string sDesc)
        {
            iItems[iItems.Length - 1].Description = sDesc;
        }

        /// <summary>
        /// Sets the quantity of the last item
        /// </summary>
        /// <param name="nNewQuantity">The quantity of the last item</param>
        public void SetLastItemQuantity(int nNewQuantity)
        {
            iItems[iItems.Length - 1].Quantity = nNewQuantity;
        }

        /// <summary>
        /// Gets the quantity of the last item
        /// </summary>
        /// <returns>The quantity of the last item added</returns>
        public int GetLastItemQuantity()
        {
            return iItems[iItems.Length - 1].Quantity;
        }

        /// <summary>
        /// Gets the net price of the last item added
        /// </summary>
        /// <returns>The net price if the last item added</returns>
        public float GetLastItemNetPrice()
        {
            return iItems[iItems.Length - 1].Amount;
        }

        /// <summary>
        /// Returns a 2D array of the payment methods in the transaction
        /// </summary>
        /// <returns>The payment methods in the transaction</returns>
        public string[,] GetPaymentMethods()
        {
            string[,] sToReturn = new string[pmPayMethod.Length + 1, 2];
            sToReturn[0, 0] = pmPayMethod.Length.ToString();
            for (int i = 0; i < pmPayMethod.Length; i++)
            {
                sToReturn[i + 1, 0] = pmPayMethod[i].PMType;
                sToReturn[i + 1, 1] = TillEngine.FormatMoneyForDisplay(pmPayMethod[i].Amount);
            }
            return sToReturn;
        }

        /// <summary>
        /// Gets the amount that is still due to be paid
        /// </summary>
        /// <returns>The amount still due</returns>
        public float GetStillDue()
        {
            float fTotalAmount = TotalAmount;
            for (int i = 0; i < pmPayMethod.Length; i++)
            {
                fTotalAmount -= pmPayMethod[i].Amount;
            }
            fTotalAmount = TillEngine.FixFloatError(fTotalAmount);
            if (fTotalAmount <= 0.0f)
                bAllPaidFor = true;
            else
                bAllPaidFor = false;

            return fTotalAmount;
        }

        /// <summary>
        /// Gets whether or not everything has been paid for
        /// </summary>
        public bool AllPaidFor
        {
            get
            {
                return bAllPaidFor;
            }
        }

        /// <summary>
        /// Gets the amount of change that's due
        /// </summary>
        public float ChangeDue
        {
            get
            {
                return fExcessPaid;
            }
        }

        /// <summary>
        /// Removes an item from the transaction
        /// </summary>
        /// <param name="nLineNumber">The 'line number', which is iItems array position + 1</param>
        public void DeleteLine(int nLineNumber)
        {
            nLineNumber -= 1;
            Item[] iTemp = iItems;
            int nDiff = 0;
            iItems = new Item[iTemp.Length - 1];
            for (int i = 0; i < iTemp.Length; i++)
            {
                if (i == nLineNumber)
                    nDiff++;
                else
                    iItems[i - nDiff] = iTemp[i];
            }
        }

        /// <summary>
        /// Discounts a set amount from the item
        /// </summary>
        /// <param name="nItemNumber">The item number (not line number!) to discount</param>
        /// <param name="fAmount">The amount to discount</param>
        public void DiscountAmountFromItem(int nItemNumber, float fAmount)
        {
            if (iItems[nItemNumber].GrossAmount * ((float)nDiscountThreshold / 100) > iItems[nItemNumber].Amount - fAmount)
            {
                GTill.frmDiscountWarning fWarning = new GTill.frmDiscountWarning(iItems[nItemNumber].Description);
                fWarning.ShowDialog();
                if (fWarning.Continue)
                {
                    iItems[nItemNumber].DiscountAmountFromNet(fAmount);
                }

            }
            else
            {
                iItems[nItemNumber].DiscountAmountFromNet(fAmount);
            }

        }

        public void SwapItemWithLast(int nItem)
        {
            if (nItem < iItems.Length && nItem >= 0)
            {
                Item i = iItems[nItem];
                iItems[nItem] = iItems[iItems.Length - 1];
                iItems[iItems.Length - 1] = i;
            }
        }

        /// <summary>
        /// Clears all payment methods, incase the user decides to add another item
        /// </summary>
        public void ClearAllPayments()
        {
            pmPayMethod = new PaymentMethod[0];
            bAllPaidFor = false;
        }

        /// <summary>
        /// Discounts a certain percentage from the price of the item
        /// </summary>
        /// <param name="nItemNumber">The item number (not line number!) to discount</param>
        /// <param name="nPercent">The percentage to discount</param>
        public void DiscountPercentageFromItem(int nItemNumber, float nPercent)
        {
            float fCurrentItemAmount = iItems[nItemNumber].Amount;
            float fPercent = (fCurrentItemAmount / 100) * nPercent;
            fPercent = (float)Math.Round((decimal)fPercent, 2);
            DiscountAmountFromItem(nItemNumber, fPercent);
        }
        
        /// <summary>
        /// Discounts a certain amount from the whole transaction
        /// </summary>
        /// <param name="fAmount">The amount to discount</param>
        public void DiscountAmountFromWholeTransaction(float fAmount)
        {
            float fTargetTotal = TillEngine.FixFloatError(TotalAmount - fAmount);
            float fCurrentTotal = TotalAmount;

            for (int i = 0; i < iItems.Length; i++)
            {
                float fCurrentAmount = iItems[i].Amount;
                float fAmountToDiscount = (fCurrentAmount / fCurrentTotal) * fAmount;
                fAmountToDiscount = TillEngine.FixFloatError(fAmountToDiscount);
                DiscountAmountFromItem(i, fAmountToDiscount);
                //iItems[i].DiscountAmountFromNet(fAmountToDiscount);
            }

            float fActualTotal = TotalAmount;
            if (fActualTotal != fTargetTotal)
            {
                iItems[iItems.Length - 1].DiscountAmountFromNet(TillEngine.FixFloatError(fActualTotal - fTargetTotal));
            }
        }

        /// <summary>
        /// Discounts a certain percentage from the whole transaction
        /// </summary>
        /// <param name="nPercent">The percentage to discount</param>
        public void DiscountPercentageFromWholeTransaction(float nPercent)
        {
            float fCurrentAmount = TotalAmount;
            float fAmountToDiscount = (fCurrentAmount / 100) * nPercent;
            fAmountToDiscount = TillEngine.FixFloatError(fAmountToDiscount);

            DiscountAmountFromWholeTransaction(fAmountToDiscount);
        }

        /// <summary>
        /// Gets the amount from all items in the transaction that is applicable at the given V.A.T. rate
        /// </summary>
        /// <param name="sVATRate">The V.A.T. rate code</param>
        /// <returns>The amount of items which are under the given V.A.T. rate</returns>
        public float GetAmountForVATRate(string sVATRate, bool bForReceipt)
        {
            float fTotal = 0.0f;
            for (int i = 0; i < iItems.Length; i++)
            {
                if (iItems[i].ItemCategory != 6)
                {
                    if (iItems[i].VATRate == sVATRate)
                        fTotal += iItems[i].Amount;
                }
                else
                {
                    if (!bForReceipt)
                    {
                        float fCost = (float)Convert.ToDecimal(tCommission.GetRecordFrom(iItems[i].Barcode, 0, true)[1]);
                        if (iItems[i].VATRate == sVATRate)
                            fTotal += (iItems[i].Amount - fCost);
                        else if (sVATRate == "X0")
                            fTotal += fCost;
                    }
                    else
                    {
                        if (sVATRate == "X0")
                        {
                            fTotal += iItems[i].Amount;
                        }
                    }
                }
            }
            return TillEngine.FixFloatError(fTotal);
        }
    }

    /// <summary>
    /// Contains settings about the till in general, and also about the current transaction
    /// </summary>
    class TillSettings
    {
        /// <summary>
        /// The name of the shop
        /// </summary>
        string sShopName;
        /// <summary>
        /// The address of the shop
        /// </summary>
        string[] sAddress;
        /// <summary>
        /// The telephone number
        /// </summary>
        string sTelNumber;
        /// <summary>
        /// The V.A.T. number
        /// </summary>
        string sVATNumber;
        /// <summary>
        /// The end of receipt (footer) message
        /// </summary>
        string[] sEndOfReceiptMessage;
        /// <summary>
        /// Whether or not discount is allowed
        /// </summary>
        bool bDiscountAllowed;
        /// <summary>
        /// Whether or not change can be given from cheques
        /// </summary>
        bool bChangeFromCheques;
        /// <summary>
        /// The number of credit cards
        /// </summary>
        int nNumberOfCreditCards;
        /// <summary>
        /// A list of credit cards
        /// </summary>
        string[] sCreditCards;
        /// <summary>
        /// The currency character
        /// </summary>
        char cCurrencyChar;
        /// <summary>
        /// V.A.T. rates
        /// </summary>
        float[] fVATRates;
        /// <summary>
        /// The user that's currently logged in
        /// </summary>
        int nCurrentUser;
        /// <summary>
        /// The name of the till
        /// </summary>
        string sTillName;
        /// <summary>
        /// The till ID
        /// </summary>
        string sTillID;
        /// <summary>
        /// The department name
        /// </summary>
        string sDeptName;
        /// <summary>
        /// Whether or not to auto-lowercase item descriptions
        /// </summary>
        bool bAutoLowercase;

        /// <summary>
        /// Initialises the till settings
        /// </summary>
        /// <param name="tTillSettings">A referece to the DETAILS database</param>
        public TillSettings(Table tTillSettings)
        {
            string[] sSettings = new string[tTillSettings.NumberOfRecords];
            for (int i = 0; i < sSettings.Length; i++)
            {
                sSettings[i] = tTillSettings.GetRecordFrom(i)[0];
                sSettings[i] = sSettings[i].TrimEnd(' ');
            }

            sShopName = sSettings[0];

            sAddress = new string[2];
            sAddress[0] = sSettings[1];
            sAddress[1] = sSettings[2];

            sVATNumber = sSettings[3];

            cCurrencyChar = sSettings[7][0];
            if (cCurrencyChar == (char)156)
                cCurrencyChar = '£';

            bDiscountAllowed = true; // ConvertSettingsBoolean(sSettings[8]);

            bChangeFromCheques = false; // ConvertSettingsBoolean(sSettings[10]);

            sTelNumber = sSettings[12];

            sCreditCards = new string[Convert.ToInt32(sSettings[16])];
            nNumberOfCreditCards = Convert.ToInt32(sSettings[16]);
            for (int i = 17; i < 17 + sCreditCards.Length; i++)
            {
                sCreditCards[i - 17] = sSettings[i].Split(' ')[0];
            }

            fVATRates = new float[9];
            for (int i = 26; i < 35; i++)
            {
                fVATRates[i - 26] = (float)Convert.ToDecimal(sSettings[i]);
            }

            sEndOfReceiptMessage = new string[3];
            sEndOfReceiptMessage[0] = sSettings[35];
            sEndOfReceiptMessage[1] = sSettings[36];
            sEndOfReceiptMessage[2] = sSettings[37];

            string sTemp = sSettings[6].TrimEnd(' ');
            for (int i = 0; i < 6; i++)
            {
                sTillName += sTemp[i];
            }
            for (int i = 6; i < 9; i++)
            {
                sTillID += sTemp[i];
            }
            for (int i = 9; i < sTemp.Length; i++)
            {
                sDeptName += sTemp[i];
            }
            bAutoLowercase = GTill.Properties.Settings.Default.bAutoLowercaseItems;
        }

        /// <summary>
        /// Gets the V.A.T. rate
        /// </summary>
        /// <param name="VATNum">The V.A.T. rate number</param>
        /// <returns>The V.A.T. rate</returns>
        public float GetVATRate(int VATNum)
        {
            return fVATRates[VATNum];
        }

        /// <summary>
        /// Gets or sets the current staff number
        /// </summary>
        public int StaffNumber
        {
            get
            {
                return nCurrentUser;
            }
            set
            {
                nCurrentUser = value;
            }
        }

        /// <summary>
        /// Gets the shop's name
        /// </summary>
        public string ShopName
        {
            get
            {
                return sShopName;
            }
        }

        /// <summary>
        /// Gets the name of this till
        /// </summary>
        public string TillName
        {
            get
            {
                return sTillName;
            }
        }

        /// <summary>
        /// Gets the currency symbol
        /// </summary>
        public char CurrenySymbol
        {
            get
            {
                return cCurrencyChar;
            }
        }

        /// <summary>
        /// Gets a list of credit cards
        /// </summary>
        public string[] CreditCards
        {
            get
            {
                return sCreditCards;
            }
        }

        /// <summary>
        /// A procedure to decrypt passwords
        /// </summary>
        /// <param name="dBaseData">The encrypted password</param>
        /// <returns>A decrypted password</returns>
        string[] DecryptPasswords(string dBaseData)
        {
            dBaseData = dBaseData.TrimEnd(' ');
            string[] sPasswords = new string[3];
            int nPos = 0;
            for (int i = 0; i < dBaseData.Length; i++)
            {
                if ((i % 6) == 0 && i != 0)
                    nPos++;
                sPasswords[nPos] += dBaseData[i];
            }
            string[] sDecrypted = new string[3];
            for (int i = 0; i < sPasswords.Length; i++)
            {
                sDecrypted[i] += (char)((int)sPasswords[i][0] - 128);
                sDecrypted[i] += (char)((int)sPasswords[i][1] - 123);
                sDecrypted[i] += (char)((int)sPasswords[i][2] - 120);
                sDecrypted[i] += (char)((int)sPasswords[i][3] - 121);
                sDecrypted[i] += (char)((int)sPasswords[i][4] - 122);
                sDecrypted[i] += (char)((int)sPasswords[i][5] - 124);
                if (sDecrypted[i].EndsWith(((char)215).ToString()))
                    sDecrypted[i] = sDecrypted[i].TrimEnd((char)215);
            }
            return sDecrypted;
        }
        /// <summary>
        /// Converts Y to true, and N to false
        /// </summary>
        /// <param name="sSetting">The Y or N</param>
        /// <returns>True for Y, False for N</returns>
        bool ConvertSettingsBoolean(string sSetting)
        {
            if (sSetting[0] == 'Y')
                return true;
            else
                return false;
        }
        public bool AutoLowercase
        {
            get
            {
                return bAutoLowercase;
            }
            set
            {
                bAutoLowercase = value;
            }
        }
    }

    /// <summary>
    /// Preserves information about a transaction for when the user stores the transaction for later
    /// </summary>
    class StoredTransaction
    {
        /// <summary>
        /// The transaction that is being stored
        /// </summary>
        Transaction actualTransaction;
        /// <summary>
        /// The tillsettings from when the transaction was saved
        /// </summary>
        TillSettings ts;
        /// <summary>
        /// The number of the user that stored the transaction
        /// </summary>
        int nUserNumber;

        /// <summary>
        /// Initialises the stored transaction
        /// </summary>
        /// <param name="t">The transaction to store</param>
        /// <param name="tillS">The TillSettings to store</param>
        /// <param name="nNum">The staff number that stored this</param>
        public StoredTransaction(Transaction t, TillSettings tillS, int nNum)
        {
            actualTransaction = t;
            ts = tillS;
            nUserNumber = nNum;
        }

        /// <summary>
        /// Gets the user that stored this transaction
        /// </summary>
        public int UserNumber
        {
            get
            {
                return nUserNumber;
            }
        }

        /// <summary>
        /// Gets the saved transaction
        /// </summary>
        public Transaction SavedTranaction
        {
            get
            {
                return actualTransaction;
            }
        }

        /// <summary>
        /// Gets the saved TillSettings
        /// </summary>
        public TillSettings SavedTillSettings
        {
            get
            {
                return ts;
            }
        }
    }

    /// <summary>
    /// The TillEngine interfaces the user forms with the underlying code to save transactions etc
    /// </summary>
    public partial class TillEngine
    {
        /// <summary>
        /// The details database, which stores settings for the till
        /// </summary>
        Table tDetails;
        Table tMultiHeader;
        Table tMultiData;
        /// <summary>
        /// The RepData database, which stores information about the day's transactions
        /// </summary>
        Table tRepData;
        /// <summary>
        /// The staff database, which stores information about staff
        /// </summary>
        Table tStaff;
        /// <summary>
        /// The stock level database, which stores stock levels
        /// </summary>
        Table tStkLevel;
        /// <summary>
        /// The stock database, which stores information about stock items
        /// </summary>
        Table tStock;
        /// <summary>
        /// The ticket data database, which stores information about transactions
        /// </summary>
        Table tTData;
        /// <summary>
        /// The ticket header database, which stores information about payment methods
        /// </summary>
        Table tTHDR;
        /// <summary>
        /// The accounts database, which stores information about accounts
        /// </summary>
        Table tAccStat;
        /// <summary>
        /// The key presets database, which stores information about function key presets
        /// </summary>
        Table tPresets;
        /// <summary>
        /// The customer record database, which stores information about customers
        /// </summary>
        Table tCustRec;
        /// <summary>
        /// The VAT Rates table as used by the new system
        /// </summary>
        Table tVAT;

        /// <summary>
        /// The table with commission rates
        /// </summary>
        Table tCommission;

        /// <summary>
        /// The Table with order suggestions from staff. Cleared when backoffice collects this table
        /// </summary>
        Table tOrderSug;

        /// <summary>
        /// The TillSettings for this till, stores settings and other info
        /// </summary>
        TillSettings tsCurrentTillSettings;

        /// <summary>
        /// The current transaction
        /// </summary>
        Transaction tCurrentTransation;

        /// <summary>
        /// The Till Category table
        /// </summary>
        Table tTillCat;

        /// <summary>
        /// Stores Customers' Email Addresses
        /// </summary>
        Table tEmails;

        /// <summary>
        /// Stores a list of offers, with the number printed and the number returned
        /// </summary>
        Table tOffers;

        /// <summary>
        /// An array of stored transactions
        /// </summary>
        StoredTransaction[] tStoredTransaction;

        /// <summary>
        /// Whether or not the printer is enabled
        /// </summary>
        bool bPrinterEnabled = true;

        /// <summary>
        /// Loads a database table
        /// </summary>
        /// <param name="sTableName">The name of the table to load</param>
        /// <returns>True if loaded with no problems</returns>
        public bool LoadTable(string sTableName)
        {
            try
            {
                if (!sTableName.Contains(".DBF"))
                {
                    switch (sTableName)
                    {
                        case "DETAILS":
                            tDetails = new Table(GTill.Properties.Settings.Default.sDetailsLoc);
                            // Fix grammatical errors (LORDS --> LORD'S)
                            tDetails.EditRecordData(0, 0, tDetails.GetRecordFrom(0)[0].Replace("LORDS", "LORD'S"));
                            tsCurrentTillSettings = new TillSettings(tDetails);
                            break;
                        case "MULTIHEADER":
                            if (File.Exists("MULTIHDR.DBF"))
                                tMultiHeader = new Table("MULTIHDR.DBF");
                            break;
                        case "MULTIDATA":
                            if (File.Exists("MULTIDAT.DBF"))
                                tMultiData = new Table("MULTIDAT.DBF");
                            break;
                        case "REPDATA":
                            tRepData = new Table(GTill.Properties.Settings.Default.sRepDataLoc);
                            break;
                        case "STAFF":
                            tStaff = new Table(GTill.Properties.Settings.Default.sStaffLoc);
                            break;
                        case "STKLEVEL":
                            tStkLevel = new Table(GTill.Properties.Settings.Default.sStkLevelLoc);
                            break;
                        case "STOCK":
                            tStock = new Table(GTill.Properties.Settings.Default.sStockLoc);
                            break;
                        case "TDATA":
                            tTData = new Table(GTill.Properties.Settings.Default.sTDataLoc);
                            break;
                        case "THDR":
                            tTHDR = new Table(GTill.Properties.Settings.Default.sTHdrLoc);
                            break;
                        case "ACCSTAT":
                            tAccStat = new Table(GTill.Properties.Settings.Default.sAccStatLoc);
                            break;
                        case "PRESETS":
                            tPresets = new Table(GTill.Properties.Settings.Default.sPresetsLoc);
                            break;
                        case "CUSTREC":
                            tCustRec = new Table(GTill.Properties.Settings.Default.sCustRecLoc);
                            break;
                        case "TILLCAT":
                            tTillCat = new Table(GTill.Properties.Settings.Default.sTillCatLoc);
                            tTillCat.SortTable();
                            break;
                        case "VAT":
                            if (File.Exists("VAT.DBF"))
                                tVAT = new Table("VAT.DBF");
                            break;
                        case "COMMISSION":
                            if (File.Exists("COMMISSI.DBF"))
                                tCommission = new Table("COMMISSI.DBF");
                            break;
                        case "TORDERSUG":
                            if (File.Exists("TORDERSU.DBF"))
                                tOrderSug = new Table("TORDERSU.DBF");
                            break;
                        case "EMAIL":
                            
                            tEmails = new Table("EMAILS.DBF");
                            if (tEmails.ReturnFieldNames().Length < 7)
                            {
                                FileStream fs = new FileStream("EMAILS.DBF", FileMode.OpenOrCreate);
                                fs.Write(GTill.Properties.Resources.EMAILS, 0, GTill.Properties.Resources.EMAILS.Length);
                                fs.Close();

                                tEmails = new Table("EMAILS.DBF");
                            }
                            break;
                        case "OFFERS":
                            tOffers = new Table("OFFERS.DBF");
                            break;

                    }
                }
                else
                {
                    if (sTableName.Contains("REPDATA"))
                        tRepData = new Table(sTableName);
                    else if (sTableName.Contains("THDR"))
                        tTHDR = new Table(sTableName);
                    else if (sTableName.Contains("TDATA"))
                        tTData = new Table(sTableName);
                }
                return true;
            }
            catch (Exception ex)
            {
                ex.ToString();
                return false;
            }
        }

        /// <summary>
        /// Loads just the required tables to print a register report retrospectively
        /// </summary>
        public void LoadTablesForJustRegisterReport()
        {
            tRepData = new Table(System.Windows.Forms.Application.ExecutablePath.Replace("\\GTill.exe", "") + "\\REPDATA.DBF");
            tTData = new Table(System.Windows.Forms.Application.ExecutablePath.Replace("\\GTill.exe", "") + "\\TDATA.DBF");
            tTHDR = new Table(System.Windows.Forms.Application.ExecutablePath.Replace("\\GTill.exe", "") + "\\THDR.DBF");
            tDetails = new Table(System.Windows.Forms.Application.ExecutablePath.Replace("\\GTill.exe", "") + "\\DETAILS.DBF");
            tVAT = new Table(System.Windows.Forms.Application.ExecutablePath.Replace("\\GTill.exe", "") + "\\VAT.DBF");
            tsCurrentTillSettings = new TillSettings(tDetails);
        }

        /// <summary>
        /// Gets the name of the shop
        /// </summary>
        public string ShopName
        {
            get
            {
                return tsCurrentTillSettings.ShopName;
            }
        }

        /// <summary>
        /// Gets the name of the current staff member
        /// </summary>
        /// <returns>The name of the current staff member</returns>
        public string GetCurrentStaffName()
        {
            int nCurrentStaff = tsCurrentTillSettings.StaffNumber;
            return GetStaffName(nCurrentStaff);
        }

        /// <summary>
        /// Gets or sets the number of the current staff member
        /// </summary>
        public int CurrentStaffNumber
        {
            get
            {
                return tsCurrentTillSettings.StaffNumber;
            }
            set
            {
                tsCurrentTillSettings.StaffNumber = value;
            }
        }

        /// <summary>
        /// Gets the name of any staff member
        /// </summary>
        /// <param name="nStaffNumber">The staff number</param>
        /// <returns>The staff's name</returns>
        public string GetStaffName(int nStaffNumber)
        {
            if (nStaffNumber == 0)
                return CorrectAllUppercase(tsCurrentTillSettings.TillName);
            string[] sData = tStaff.GetRecordFrom(nStaffNumber.ToString(), 0);
            return CorrectAllUppercase(sData[1].TrimEnd(' '));
        }

        /// <summary>
        /// Corrects all uppercase if enabled
        /// </summary>
        /// <param name="sInput">The string to correct</param>
        /// <returns>The corrected string</returns>
        private string CorrectAllUppercase(string sInput)
        {
            if (sInput.Length > 0)
            {
                string sCorrected = sInput[0].ToString().ToUpper();
                for (int i = 1; i < sInput.Length; i++)
                {
                    sCorrected += sInput[i].ToString().ToLower();
                }
                return sCorrected;
            }
            else
                return sInput;
        }

        /// <summary>
        /// Gets the next transaction number
        /// </summary>
        /// <returns>The next transaction number</returns>
        public int GetNextTransactionNumber()
        {
            string[] sData = tRepData.GetRecordFrom("END", 1);
            float fCurrent = (float)Convert.ToDecimal(sData[3]);
            fCurrent *= 100;
            int nCurrent = Convert.ToInt32(fCurrent);
            nCurrent++;
            return nCurrent;
        }

        /// <summary>
        /// Gets the till name
        /// </summary>
        public string TillName
        {
            get
            {
                return CorrectAllUppercase(tsCurrentTillSettings.TillName);
            }
        }

        /// <summary>
        /// Gets an item as a class // TODO: REMOVE THIS!!
        /// </summary>
        /// <param name="sBarcode">The barcode of the item to get the information about</param>
        /// <returns>An item class</returns>
        public Item GetItemAsItemClass(string sBarcode)
        {
            string[] sDetails = tStock.GetRecordFrom(sBarcode, 0, true);
            string[] sSTKLevel;
            if (sDetails.Length == 1)
                return null;
            if (sDetails[0].TrimEnd(' ').Length > 0)
            {
                // sDetails[0] below and above was previously sDetails[7]. This is the parent barcode, not product barcode!
                sSTKLevel = tStkLevel.GetRecordFrom(sDetails[0].TrimEnd(' '), 0, true);
                if (sSTKLevel.Length == 1)
                {
                    Item sDetailsToReturn = new Item(sDetails);
                    sDetailsToReturn.StockLevel = -1024;
                    return sDetailsToReturn;
                }
            }
            else
            {
                sSTKLevel = tStkLevel.GetRecordFrom(sDetails[0].TrimEnd(' '), 0, true);
                if (sSTKLevel.Length == 1)
                    return (new Item(sDetails));
            }
            return (new Item(sDetails, sSTKLevel));
        }

        /// <summary>
        /// Gets the currency symbol to use
        /// </summary>
        public char CurrencySymbol
        {
            get
            {
                return tsCurrentTillSettings.CurrenySymbol;
            }
        }

        /// <summary>
        /// Sets up a new transaction
        /// </summary>
        public void SetupNewTransaction()
        {
            if (!LoadTranscationFromStore(tsCurrentTillSettings.StaffNumber))
            {
                tCurrentTransation = new Transaction(tsCurrentTillSettings, GetNextTransactionNumber(), ref tCommission, DiscountPercentageThreshold);
                //tCurrentTransation.VATRate = GetVATRate(1);
            }
        }

        /// <summary>
        /// Gets the number of items in the current transaction
        /// </summary>
        /// <returns>Number of 'lines' in the transaction</returns>
        public int GetNumberOfItemsInCurrentTransaction()
        {
            if (tCurrentTransation == null)
                return 0;
            else
                return tCurrentTransation.NumberOfItems;
        }

        /// <summary>
        /// Gets the total number of item in the current transaction (includes multiple quantities)
        /// </summary>
        /// <returns>Number of items in the transaction</returns>
        public int GetNumberOfTotalItemsInCurrentTransaction()
        {
            // Differs from the above because it also counts quantities

            return tCurrentTransation.NumberOfItemsPurchased;
        }

        /// <summary>
        /// Gets the total amount in the transaction
        /// </summary>
        /// <returns>The total amount in the transaction</returns>
        public float GetTotalAmountInTransaction()
        {
            return TillEngine.FixFloatError(tCurrentTransation.TotalAmount);
        }

        /// <summary>
        /// Adds an item to the transaction
        /// </summary>
        /// <param name="sBarcode">The barcode of the item to add to the transaction</param>
        public void AddItemToTransaction(string sBarcode)
        {
            tCurrentTransation.AddItemToTransaction(sBarcode, ref tStock);
            if (!tCurrentTransation.ItemAddedSuccessfully && tMultiData != null && tMultiHeader != null)
            {
                // Check for type 6 multi item
                int nRecNum = 0;
                if (tMultiHeader.SearchForRecord(sBarcode, 0, ref nRecNum))
                {
                    string[] sHeaderInfo = tMultiHeader.GetRecordFrom(nRecNum);
                    int nOfItems = Convert.ToInt32(sHeaderInfo[1]);
                    for (int i = 0; i < nOfItems; i++)
                    {
                        nRecNum = tMultiData.GetRecordNumberFromTwoFields(sBarcode, 0, i.ToString(), 1);
                        string[] sItemData = tMultiData.GetRecordFrom(nRecNum);
                        tCurrentTransation.AddItemToTransaction(sItemData[2], ref tStock);
                        float fPrice = (float)Convert.ToDecimal(sItemData[4]) * (float)Convert.ToDecimal(sItemData[3]);
                        fPrice = FixFloatError(fPrice);
                        tCurrentTransation.SetLastItemPrice(fPrice);
                        tCurrentTransation.SetLastItemQuantity((int)Math.Round(Convert.ToDecimal(sItemData[3])));
                    }
                }
            }

        }

        /// <summary>
        /// Increases the quantity of the last item added to the transaction
        /// </summary>
        public void RepeatLastItem()
        {
            this.SetLastItemQuantity(tCurrentTransation.GetLastItemQuantity() + 1);
        }

        /// <summary>
        /// Gets the item class of the last item added to the transaction
        /// </summary>
        /// <returns>The last item added to the transaction</returns>
        public Item GetItemJustAdded()
        {
            return tCurrentTransation.GetItemInTransaction(tCurrentTransation.NumberOfItems - 1);
        }

        /// <summary>
        /// Whether or not the last item was successfully added (if the barcode was correct)
        /// </summary>
        /// <returns>True if the item was successfully added</returns>
        public bool WasItemAddSuccessful()
        {
            return tCurrentTransation.ItemAddedSuccessfully;
        }

        /// <summary>
        /// Sets the price of the last item added to the transaction
        /// </summary>
        /// <param name="fPrice">The item's new price</param>
        public void SetLastItemPrice(float fPrice)
        {
            tCurrentTransation.SetLastItemPrice(fPrice);
        }

        /// <summary>
        /// Sets the description of the last item added to the transaction
        /// </summary>
        /// <param name="sDesc">The description of the last item</param>
        public void SetLastItemDescription(string sDesc)
        {
            tCurrentTransation.SetLastItemDescription(sDesc);
        }

        /// <summary>
        /// Gets a list of credit cards
        /// </summary>
        /// <returns>A list of credit cards</returns>
        public string[] GetCreditCards()
        {
            return tsCurrentTillSettings.CreditCards;
        }

        /// <summary>
        /// Gets a 2D array of items to display in the transaction
        /// </summary>
        /// <returns>Items to display in the transaction</returns>
        public string[,] GetItemsToDisplay()
        {
            string[,] sToReturn = new string[tCurrentTransation.NumberOfItems + 1, 4];
            sToReturn[0, 0] = tCurrentTransation.NumberOfItems.ToString();

            for (int i = 1; i <= tCurrentTransation.NumberOfItems; i++)
            {
                Item iCurrentItem = tCurrentTransation.GetItemInTransaction(i - 1);
                sToReturn[i, 0] = CorrectMultiWordUpperCase(iCurrentItem.Description);
                sToReturn[i, 1] = iCurrentItem.Amount.ToString();
                sToReturn[i, 2] = iCurrentItem.GrossAmount.ToString();
                sToReturn[i, 3] = iCurrentItem.Quantity.ToString();
            }
            return sToReturn;
        }

        /// <summary>
        /// Sets the quantity of the last item added to the transaction
        /// </summary>
        /// <param name="nNewQuantity">The new quantity of the last item</param>
        public void SetLastItemQuantity(int nNewQuantity)
        {
            tCurrentTransation.SetLastItemPrice((tCurrentTransation.GetLastItemNetPrice() / tCurrentTransation.GetLastItemQuantity()) * nNewQuantity);
            tCurrentTransation.SetLastItemQuantity(nNewQuantity);
        }

        /// <summary>
        /// Multiplys the quantity of the last item
        /// </summary>
        /// <param name="nMultiplyFactor">The number to multiply the current quantity by</param>
        public void MultiplyLastItemQuantity(int nMultiplyFactor)
        {
            int nCurrentQuantity = tCurrentTransation.GetLastItemQuantity();
            nCurrentQuantity *= nMultiplyFactor;
            tCurrentTransation.SetLastItemQuantity(nCurrentQuantity);
            tCurrentTransation.SetLastItemPrice(tCurrentTransation.GetLastItemNetPrice() * (float)nMultiplyFactor);
        }

        /// <summary>
        /// Works out a float value from an input string
        /// </summary>
        /// <param name="sAmount"></param>
        /// <returns></returns>
        public static float fFormattedMoneyString(string sAmount)
        {
            /*
             * Possible Money Input Formats:
             * 
             * 1500 = £15.00
             * 15   = £00.15
             * 15.  = £15.00
             * .15  = £00.15
             * 
             */
            bool bNegative = false;
            if (sAmount.StartsWith("-"))
            {
                bNegative = true;
                sAmount = sAmount.TrimStart('-');
            }
            if (!sAmount.Contains("."))
            {
                float fTemp = (float)Convert.ToDecimal(sAmount);
                if (Math.Abs(fTemp) < 100 && Math.Abs(fTemp) > 9)
                {
                    sAmount = "0." + sAmount;
                }
                else if (Math.Abs(fTemp) < 100 && Math.Abs(fTemp) < 10)
                {
                    sAmount = "0.0" + sAmount;
                }
                else
                {
                    string sPence = sAmount[sAmount.Length - 2].ToString() + sAmount[sAmount.Length - 1].ToString();
                    sAmount = sAmount.Remove(sAmount.Length - 2);
                    sAmount += ".";
                    sAmount += sPence;
                }
            }

            if (sAmount.Contains("E-"))
                return 0.0f;
            else if (!bNegative)
                return (float)Convert.ToDecimal(sAmount);
            else
                return -(float)Convert.ToDecimal(sAmount);
        }

        /// <summary>
        /// Formats money for display, so 1.5 becomes 1.50
        /// </summary>
        /// <param name="fAmount">The number to format</param>
        /// <returns>A formatted for money string</returns>
        public static string FormatMoneyForDisplay(float fAmount)
        {
            fAmount = (float)Math.Round(fAmount, 2);
            string[] sSplitUp = fAmount.ToString().Split('.');
            if (sSplitUp.Length == 1)
            {
                string[] temp = new string[2];
                temp[0] = sSplitUp[0];
                temp[1] = "00";
                sSplitUp = temp;
            }
            while (sSplitUp[1].Length < 2)
                sSplitUp[1] += "0";

            string toReturn = sSplitUp[0] + "." + sSplitUp[1];

            return toReturn;
        }

        /// <summary>
        /// Adds a payment method to the transaction
        /// </summary>
        /// <param name="sPaymentType">The code of the payment method</param>
        /// <param name="fPaymentAmount">The amount of the payment</param>
        public void AddPayment(string sPaymentType, float fPaymentAmount)
        {
            tCurrentTransation.AddPaymentMethod(sPaymentType, fPaymentAmount);
        }
        /// <summary>
        /// Adds a payment to the transaction
        /// </summary>
        /// <param name="sPaymentType">The code of the payment method</param>
        /// <param name="fPaymentAmount">The amount of the payment</param>
        /// <param name="fExcess">The amount in excess of the amount due</param>
        public void AddPayment(string sPaymentType, float fPaymentAmount, float fExcess)
        {
            tCurrentTransation.AddPaymentMethod(sPaymentType, fPaymentAmount, fExcess);
        }

        /// <summary>
        /// Gets all the payment methods that have been used
        /// </summary>
        /// <returns>All the payment methods in a 2D array (code, amount)</returns>
        public string[,] GetPaymentMethods()
        {
            return tCurrentTransation.GetPaymentMethods();
        }

        /// <summary>
        /// Gets how much money is still due to be paid
        /// </summary>
        /// <returns>The amount due</returns>
        public float GetAmountStillDue()
        {
            return TillEngine.FixFloatError(tCurrentTransation.GetStillDue());
        }

        /// <summary>
        /// Whether or not the transaction has been fully paid for
        /// </summary>
        /// <returns>True if all paid</returns>
        public bool GetAllPaidFor()
        {
            return tCurrentTransation.AllPaidFor;
        }

        /// <summary>
        /// Gets the amount of change due
        /// </summary>
        /// <returns>The amount of change due</returns>
        public float GetChangeDue()
        {
            if (tCurrentTransation != null)
                return TillEngine.FixFloatError(tCurrentTransation.ChangeDue);
            else
                return 0;
        }

        /// <summary>
        /// Saves the transaction, opens the till drawer and then prints a receipt
        /// </summary>
        public void SaveTransaction()
        {
            if (!bDemoMode)
            {
                tCurrentTransation.SaveTransaction(ref tRepData, ref tTData, ref tTHDR, ref tVAT, ref tCommission);
            }
            if (bPrinterEnabled)
            {
                OpenTillDrawer(false);
                PrintReceiptDescAndPriceTitles();
                for (int i = 0; i < tCurrentTransation.NumberOfItems; i++)
                {
                    PrintItem(tCurrentTransation.GetItemInTransaction(i));
                }
                PrintTotalDueSummary(tCurrentTransation.NumberOfItemsPurchased, tCurrentTransation.TotalAmount);
                foreach (PaymentMethod pmPay in tCurrentTransation.PaymentMethods)
                {
                    PrintPaymentMethod(pmPay);
                }
                PrintChangeDue();
                PrintVAT();
                if (!bDemoMode)
                {
                    PrintReceiptFooter(GetCurrentStaffName(), DateTimeForReceiptFooter(), tCurrentTransation.TransactionNumber.ToString());
                }
                else
                {
                    GTill.frmSingleInputBox fsi = new GTill.frmSingleInputBox("Enter the transaction number:");
                    fsi.ShowDialog();
                    PrintReceiptFooter(GetCurrentStaffName(), DateTimeForReceiptFooter(), fsi.Response);
                    
                }
                    PrintReceiptHeader();
                EmptyPrinterBuffer();
            }
        }

        /// <summary>
        /// Fixes the floating point precision error that sometimes occurs. So 7.9999999999999 becomes 8.00
        /// </summary>
        /// <param name="fOriginal">The potentially faulty floating point number</param>
        /// <returns>A number rounded to 2 d.p</returns>
        public float fFixFloatError(float fOriginal)
        {
            return (float)Math.Round((double)fOriginal, 2);
        }

        /// <summary>
        /// Fixes the floating point precision error that sometimes occurs. So 7.9999999999999 becomes 8.00
        /// Static so that the process using this function needn't have an instance of the TillEngine
        /// 
        /// 8/6/14 UPDATE: I want to pour hot solder on past me's eyes
        /// </summary>
        /// <param name="fOriginal">The potentially faulty floating point number</param>
        /// <returns>A number rounded to 2 d.p</returns>
        public static float FixFloatError(float fOriginal)
        {
            return (float)Math.Round((double)fOriginal, 2);
        }
           
        /// <summary>
        /// Stores the transaction for later so that someone else can use the till
        /// </summary>
        public void StoreTransactionForLater()
        {
            if (tStoredTransaction == null)
            {
                tStoredTransaction = new StoredTransaction[0];
            }

            StoredTransaction[] tTemp = tStoredTransaction;
            tStoredTransaction = new StoredTransaction[tTemp.Length + 1];
            for (int i = 0; i < tTemp.Length; i++)
            {
                tStoredTransaction[i] = tTemp[i];
            }
            tStoredTransaction[tTemp.Length] = new StoredTransaction(tCurrentTransation, tsCurrentTillSettings, tsCurrentTillSettings.StaffNumber);

        }

        /// <summary>
        /// Loads the transaction back from memory so that the user can resume using the till
        /// </summary>
        /// <param name="nStaffNumber">The staff member who wants to resume</param>
        /// <returns>True if there was a transaction to be restored</returns>
        public bool LoadTranscationFromStore(int nStaffNumber)
        {
            if (tStoredTransaction == null)
                return false;
            else
            {
                for (int i = 0; i < tStoredTransaction.Length; i++)
                {
                    if (tStoredTransaction[i].UserNumber == nStaffNumber)
                    {
                        tCurrentTransation = tStoredTransaction[i].SavedTranaction;
                        tsCurrentTillSettings = tStoredTransaction[i].SavedTillSettings;

                        StoredTransaction[] tTemp = tStoredTransaction;
                        tStoredTransaction = new StoredTransaction[tTemp.Length - 1];
                        int nDiff = 0;
                        for (int x = 0; x < tTemp.Length; x++)
                        {
                            if (x == i)
                                nDiff++;
                            else
                                tStoredTransaction[x - nDiff] = tTemp[x];
                        }

                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Deletes a line from the transaction
        /// </summary>
        /// <param name="nLineNum"></param>
        public void DeleteLine(int nLineNum)
        {
            tCurrentTransation.DeleteLine(nLineNum);
        }

        /// <summary>
        /// Discounts a fixed amount from the price of the last item added to the transaction
        /// </summary>
        /// <param name="fAmount">The amount to discount</param>
        public void DiscountFixedAmountFromLastItem(float fAmount)
        {
            int nLastItem = tCurrentTransation.NumberOfItems - 1;
            tCurrentTransation.DiscountAmountFromItem(nLastItem, fAmount);
        }

        public void SwapItemWithLast(int nItemNumber)
        {
            tCurrentTransation.SwapItemWithLast(nItemNumber);
        }

        /// <summary>
        /// Discounts a fixed percentagr from the price of the last item added to the transaction
        /// </summary>
        /// <param name="nPercent">The percentage to discount</param>
        public void DiscountPercentageFromLastItem(float nPercent)
        {
            int nLastItem = tCurrentTransation.NumberOfItems - 1;
            tCurrentTransation.DiscountPercentageFromItem(nLastItem, nPercent);
        }

        /// <summary>
        /// Discounts a fixed amount from the whole transaction
        /// </summary>
        /// <param name="fAmount">The total amount to discount</param>
        public void DiscountFixedAmountFromWholeTransaction(float fAmount)
        {
            tCurrentTransation.DiscountAmountFromWholeTransaction(fAmount);
        }

        /// <summary>
        /// Discounts a fixed percentage from the whole transaction
        /// </summary>
        /// <param name="nPercent">The total percentage to discount</param>
        public void DiscountPercentageFromWholeTransaction(float nPercent)
        {
            tCurrentTransation.DiscountPercentageFromWholeTransaction(nPercent);
        }

        /// <summary>
        /// Clears all payment methods added to the transaction
        /// </summary>
        public void ClearPaymentMethods()
        {
            tCurrentTransation.ClearAllPayments();
        }

        /// <summary>
        /// Whether or not to allow change from cheques (feature from the old till that was carried forward)
        /// </summary>
        /// <returns>True if allowed</returns>
        public bool bAllowChangeFromCheques()
        {
            string[] sAllowed = tDetails.GetRecordFrom(10);
            if (sAllowed[0].Contains("N"))
                return false;
            else
                return true;
        }

        /// <summary>
        /// Gets details about an item for an item lookup
        /// </summary>
        /// <param name="sBarcode">The barcode of the item to lookup</param>
        /// <returns>Returns string array in format Description, Price and Stock Level</returns>
        public string[] GetItemDetailsForLookup(string sBarcode)
        {
            string[] s = new string[5];
            Item i = GetItemAsItemClass(sBarcode);
            if (i != null)
            {
                s[0] = i.Description;
                s[1] = FormatMoneyForDisplay(i.GrossAmount);
                s[2] = i.StockLevel.ToString();
                s[3] = i.fQuantityOnOrder.ToString();
                s[4] = i.sDueDate;
                if (i.StockLevel == -1024)
                    s[2] = "Not found";
            }
            else
                return null;
            return s;
        }

        /// <summary>
        /// Fixes the uppercase of item descriptions if enabled by the user
        /// </summary>
        /// <param name="sToCorrect">THE STRING IN CAPITAL LETTERS</param>
        /// <returns>A string with corrected capitals</returns>
        public string CorrectMultiWordUpperCase(string sToCorrect)
        {
            if (tsCurrentTillSettings.AutoLowercase)
            {
                string[] sKeyWords = { "PER", "OR", "AND" };
                string[] sReplacementWords = { "Per", "or", "and" };
                string[] sWord = sToCorrect.Split(' ');
                for (int i = 0; i < sWord.Length; i++)
                {
                    bool bContainsCharacters = false;
                    for (int x = 0; x < sWord[i].Length; x++)
                    {
                        if ((sWord[i][x] < 65 || sWord[i][x] > 90) && sWord[i][x] != 39 && sWord[i][x] != 44)
                            bContainsCharacters = true;
                    }
                    for (int x = 0; x < sKeyWords.Length; x++)
                    {
                        if (sWord[i].ToUpper() == sKeyWords[x].ToUpper())
                        {
                            sWord[i] = sReplacementWords[x];
                            bContainsCharacters = true;
                        }
                    }
                    if (!bContainsCharacters && sWord[i].Length > 3)
                        sWord[i] = CorrectAllUppercase(sWord[i]);
                }

                string sToReturn = "";
                foreach (string s in sWord)
                    sToReturn += s + " ";
                sToReturn = sToReturn.TrimEnd(' ');
                return sToReturn;
            }
            else
                return sToCorrect;
        }

        /// <summary>
        /// Removes a payment method from REPDATA
        /// </summary>
        /// <param name="sPaymentMethodCode">The code of the payment method to use</param>
        /// <param name="fAmount">The amount to be removed</param>
        /// <param name="bRefund">Whether or not this is due to a refund</param>
        private void RemovePaymentMethodFromRepData(string sPaymentMethodCode, float fAmount, bool bRefund)
        {
            // Pass Charge to account in format 'CHRG,CompanyCode,TransactionNumber'
            if (sPaymentMethodCode == "CASH" || sPaymentMethodCode == "VOUC" || sPaymentMethodCode == "CHEQ")
            {
                int nRecordPos = 0;
                if (!tRepData.SearchForRecord(sPaymentMethodCode, 1, ref nRecordPos))
                {
                    string[] sNew = { "ST", sPaymentMethodCode, "0", "0", "0" };
                    tRepData.AddRecord(sNew);
                    tRepData.SearchForRecord(sPaymentMethodCode, 1, ref nRecordPos);
                }
                tRepData.SearchForRecord(sPaymentMethodCode, 1, ref nRecordPos);
                string[] sCurrentData = tRepData.GetRecordFrom(sPaymentMethodCode, 1);
                float fCurrentAmount = FixFloatError((float)Convert.ToDecimal(sCurrentData[3]));
                float fNewAmount = FixFloatError(fCurrentAmount - fAmount);
                int nCurrentQuantity = Convert.ToInt32(sCurrentData[2]);
                int nNewQuantity = nCurrentQuantity - 1;
                if (bRefund)
                    nNewQuantity += 2;
                tRepData.EditRecordData(nRecordPos, 2, nNewQuantity.ToString());
                tRepData.EditRecordData(nRecordPos, 3, fNewAmount.ToString());
                if (nNewQuantity == 0)
                    tRepData.DeleteRecord(nRecordPos);
            }
            else if (sPaymentMethodCode.StartsWith("CRCD"))
            {
                // First, remove the amount from CRCD
                int nRecordPos = 0;
                if (!tRepData.SearchForRecord("CRCD", 1, ref nRecordPos))
                {
                    string[] sNew = { "ST", "CRCD", "0", "0", "0" };
                    tRepData.AddRecord(sNew);
                    tRepData.SearchForRecord("CRCD", 1, ref nRecordPos);
                }
                tRepData.SearchForRecord("CRCD", 1, ref nRecordPos);
                string[] sCurrentData = tRepData.GetRecordFrom("CRCD", 1);
                float fCurrentAmount = FixFloatError((float)Convert.ToDecimal(sCurrentData[3]));
                float fNewAmount = FixFloatError(fCurrentAmount - fAmount);
                int nCurrentQuantity = Convert.ToInt32(sCurrentData[2]);
                int nNewQuantity = nCurrentQuantity - 1;
                if (bRefund)
                    nNewQuantity+=2;
                tRepData.EditRecordData(nRecordPos, 2, nNewQuantity.ToString());
                tRepData.EditRecordData(nRecordPos, 3, fNewAmount.ToString());
                if (nNewQuantity == 0)
                    tRepData.DeleteRecord(nRecordPos);

                // Then remove from CRD1, CRD2 or whatever it isf
                sPaymentMethodCode = sPaymentMethodCode.Replace("CRCD", "CRD");
                nRecordPos = 0;
                if (!tRepData.SearchForRecord(sPaymentMethodCode, 1, ref nRecordPos))
                {
                    string[] sNew = { "ST", sPaymentMethodCode, "0", "0", "0" };
                    tRepData.AddRecord(sNew);
                    tRepData.SearchForRecord(sPaymentMethodCode, 1, ref nRecordPos);
                }
                tRepData.SearchForRecord(sPaymentMethodCode, 1, ref nRecordPos);
                sCurrentData = tRepData.GetRecordFrom(sPaymentMethodCode, 1);
                fCurrentAmount = FixFloatError((float)Convert.ToDecimal(sCurrentData[3]));
                fNewAmount = FixFloatError(fCurrentAmount - fAmount);
                nCurrentQuantity = Convert.ToInt32(sCurrentData[2]);
                nNewQuantity = nCurrentQuantity - 1;
                if (bRefund)
                    nNewQuantity += 2;
                tRepData.EditRecordData(nRecordPos, 2, nNewQuantity.ToString());
                tRepData.EditRecordData(nRecordPos, 3, fNewAmount.ToString());
                if (nNewQuantity == 0)
                    tRepData.DeleteRecord(nRecordPos);
            }
            else if (sPaymentMethodCode == "DEPO")
            {
                int nRecordPos = 0;
                if (!tRepData.SearchForRecord(sPaymentMethodCode, 1, ref nRecordPos))
                {
                    string[] sNew = { "ST", sPaymentMethodCode, "0", "0", "0" };
                    tRepData.AddRecord(sNew);
                    tRepData.SearchForRecord(sPaymentMethodCode, 1, ref nRecordPos);
                }
                tRepData.SearchForRecord(sPaymentMethodCode, 1, ref nRecordPos);
                string[] sCurrentData = tRepData.GetRecordFrom(sPaymentMethodCode, 1);
                float fCurrentAmount = FixFloatError((float)Convert.ToDecimal(sCurrentData[3]));
                float fNewAmount = FixFloatError(fCurrentAmount - fAmount);
                int nCurrentQuantity = Convert.ToInt32(sCurrentData[2]);
                int nNewQuantity = nCurrentQuantity - 1;
                if (bRefund)
                    nNewQuantity+=2;
                tRepData.EditRecordData(nRecordPos, 2, nNewQuantity.ToString());
                tRepData.EditRecordData(nRecordPos, 3, fNewAmount.ToString());
                if (nNewQuantity == 0)
                    tRepData.DeleteRecord(nRecordPos);

                // Alter ST,DEP record

                nRecordPos = 0;
                if (!tRepData.SearchForRecord("DEP", 1, ref nRecordPos))
                {
                    string[] sToAdd = { "ST", "DEP", "0", "0", "0" };
                    tRepData.AddRecord(sToAdd);
                    tRepData.SearchForRecord("DEP", 1, ref nRecordPos);
                }
                sCurrentData = tRepData.GetRecordFrom(nRecordPos);
                fCurrentAmount = FixFloatError((float)Convert.ToDecimal(sCurrentData[3]));
                fNewAmount = FixFloatError(fCurrentAmount + fAmount);
                nCurrentQuantity = Convert.ToInt32(sCurrentData[2]);
                nNewQuantity = nCurrentQuantity - 1;
                tRepData.EditRecordData(nRecordPos, 2, nNewQuantity.ToString());
                tRepData.EditRecordData(nRecordPos, 3, fNewAmount.ToString());
                //if (nNewQuantity == 0)
                //    tRepData.DeleteRecord(nRecordPos);
            }
            else if (sPaymentMethodCode.StartsWith("CHRG"))
            {
                string[] sChargeInfo = sPaymentMethodCode.Split(',');
                sChargeInfo[1] = sChargeInfo[1].TrimEnd(' ');
                while (sChargeInfo[1].Length < 6)
                    sChargeInfo[1] = sChargeInfo[1] + " ";
                sChargeInfo[2] = sChargeInfo[2].TrimEnd(' ');
                while (sChargeInfo[2].Length < 6)
                    sChargeInfo[2] = "0" + sChargeInfo[2];
                string sRepCodeEntry = sChargeInfo[1] + sChargeInfo[2];
                int nSTRecordPos = 0;
                if(tRepData.SearchForRecord(sRepCodeEntry, 1, ref nSTRecordPos))
                    tRepData.DeleteRecord(nSTRecordPos);
                if (bRefund)
                {
                    string[] sToReAdd = { "CA", sRepCodeEntry, "0", "0", "0" };
                    tRepData.AddRecord(sToReAdd);
                }

                // Now remove from the CHRG record
                int nRecordPos = 0;
                if (!tRepData.SearchForRecord("CHRG", 1, ref nRecordPos))
                {
                    string[] sNew = { "ST", "CHRG", "0", "0", "0" };
                    tRepData.AddRecord(sNew);
                    tRepData.SearchForRecord("CHRG", 1, ref nRecordPos);
                }
                tRepData.SearchForRecord("CHRG", 1, ref nRecordPos);
                string[] sCurrentData = tRepData.GetRecordFrom("CHRG", 1);
                float fCurrentAmount = FixFloatError((float)Convert.ToDecimal(sCurrentData[3]));
                float fNewAmount = FixFloatError(fCurrentAmount - fAmount);
                int nCurrentQuantity = Convert.ToInt32(sCurrentData[2]);
                int nNewQuantity = nCurrentQuantity - 1;
                if (bRefund)
                    nNewQuantity += 2;
                tRepData.EditRecordData(nRecordPos, 2, nNewQuantity.ToString());
                tRepData.EditRecordData(nRecordPos, 3, fNewAmount.ToString());
                if (nNewQuantity == 0)
                    tRepData.DeleteRecord(nRecordPos);
            }
        }

        /// <summary>
        /// Removes V.A.T. from REPDATA
        /// </summary>
        /// <param name="nCategoryNumber">The V.A.T. category</param>
        /// <param name="fAmount">The amount to remove</param>
        /// <param name="bRefund">Whether or not this is due to a refund</param>
        private void RemoveVAT(int nCategoryNumber, float fAmount, bool bRefund)
        {
            string sRepCode = "VAT";
            if (nCategoryNumber < 10)
                sRepCode += "0";
            sRepCode += nCategoryNumber.ToString();
            int nRecordPos = 0;
            if (!tRepData.SearchForRecord(sRepCode, 1, ref nRecordPos))
            {
                string[] sNew = { "CR", sRepCode, "0", "0", "0" };
                tRepData.AddRecord(sNew);
                tRepData.SearchForRecord(sRepCode, 1, ref nRecordPos);
            }

            string[] sCurrentContents = tRepData.GetRecordFrom(nRecordPos);
            float fCurrentAmount = FixFloatError((float)Convert.ToDecimal(sCurrentContents[3]));
            float fNewAmount = FixFloatError(fCurrentAmount - fAmount);
            sCurrentContents[3] = fNewAmount.ToString();
            if (bRefund)
            {
                int nCurrentQuantity = Convert.ToInt32(sCurrentContents[2]);
                nCurrentQuantity++;
                tRepData.EditRecordData(nRecordPos, 2, nCurrentQuantity.ToString());
            }
            if (fNewAmount != 0.0f || bRefund)
                tRepData.EditRecordData(nRecordPos, 3, fNewAmount.ToString());
            else
                tRepData.DeleteRecord(nRecordPos);
        }
        private void RemoveVAT(string sVATCode, float fAmount, bool bRefund)
        {
            string sRepCode = "VAT" + sVATCode;
            int nRecordPos = 0;
            if (!tRepData.SearchForRecord(sRepCode, 1, ref nRecordPos))
            {
                string[] sNew = { "CR", sRepCode, "0", "0", "0" };
                tRepData.AddRecord(sNew);
                tRepData.SearchForRecord(sRepCode, 1, ref nRecordPos);
            }

            string[] sCurrentContents = tRepData.GetRecordFrom(nRecordPos);
            float fCurrentAmount = FixFloatError((float)Convert.ToDecimal(sCurrentContents[3]));
            float fNewAmount = FixFloatError(fCurrentAmount - fAmount);
            sCurrentContents[3] = fNewAmount.ToString();
            if (bRefund)
            {
                int nCurrentQuantity = Convert.ToInt32(sCurrentContents[2]);
                nCurrentQuantity++;
                tRepData.EditRecordData(nRecordPos, 2, nCurrentQuantity.ToString());
            }
            if (fNewAmount != 0.0f || bRefund)
                tRepData.EditRecordData(nRecordPos, 3, fNewAmount.ToString());
            else
                tRepData.DeleteRecord(nRecordPos);
        }

        /// <summary>
        /// Removes an item from REPDATA
        /// </summary>
        /// <param name="sBarcode">The barcode of the item to remove</param>
        /// <param name="fNetAmount">The net amount of the item</param>
        /// <param name="fDiscount">The amount that was discounted from the item</param>
        /// <param name="nQuantity">The quantity to remove</param>
        /// <param name="bRefund">Whether or not this is due to a refund</param>
        private void RemoveItem(string sBarcode, float fNetAmount, float fDiscount, int nQuantity, bool bRefund)
        {
            int nRecordPos = 0;
            if (!tRepData.SearchForRecord(sBarcode.TrimEnd('\0'), 1, ref nRecordPos))
            {
                string[] sNew = { "ST", sBarcode, "0", "0", "0" };
                tRepData.AddRecord(sNew);
                tRepData.SearchForRecord(sBarcode, 1, ref nRecordPos);
            }

            string[] sCurrentContents = tRepData.GetRecordFrom(nRecordPos);

            int nCurrentQty = Convert.ToInt32(sCurrentContents[2]);
            int nNewQty = nCurrentQty - nQuantity;

            float fCurrentAmount = FixFloatError((float)Convert.ToDecimal(sCurrentContents[3]));
            float fNewNet = FixFloatError(fCurrentAmount - fNetAmount);

            float fCurrentGross = FixFloatError((float)Convert.ToDecimal(sCurrentContents[4]));
            float fNewGross = FixFloatError(fCurrentGross - (fNetAmount + fDiscount));

            tRepData.EditRecordData(nRecordPos, 2, nNewQty.ToString());
            tRepData.EditRecordData(nRecordPos, 3, fNewNet.ToString());
            if (!bRefund)
                tRepData.EditRecordData(nRecordPos, 4, fNewGross.ToString());

            if (nNewQty == 0 && !bRefund)
                tRepData.DeleteRecord(nRecordPos);

            // Sort out NSTCK, STOCK, NOITEM

            Item iTemp = new Item(tStock.GetRecordFrom(sBarcode.TrimEnd('\0'), 0, true));
            string sCode = "";
            if (iTemp.IsItemStock)
                sCode = "STOCK";
            else
                sCode = "NSTCK";

            tRepData.SearchForRecord(sCode, 1, ref nRecordPos);
            sCurrentContents = tRepData.GetRecordFrom(sCode, 1);
            fCurrentAmount = FixFloatError((float)Convert.ToDecimal(sCurrentContents[3]));
            fCurrentAmount -= fNetAmount;
            fCurrentAmount = FixFloatError(fCurrentAmount);
            nCurrentQty = Convert.ToInt32(sCurrentContents[2]);
            nCurrentQty -= nQuantity;
            tRepData.EditRecordData(nRecordPos, 3, fCurrentAmount.ToString());
            tRepData.EditRecordData(nRecordPos, 2, nCurrentQty.ToString());

            if (!bRefund)
            {
                sCurrentContents = tRepData.GetRecordFrom("NOITEM", 1);
                tRepData.SearchForRecord("NOITEM", 1, ref nRecordPos);
                fCurrentAmount = FixFloatError((float)Convert.ToDecimal(sCurrentContents[3]));
                nCurrentQty = Convert.ToInt32(sCurrentContents[2]);
                nCurrentQty -= nQuantity;
                fCurrentAmount = FixFloatError(fCurrentAmount - fNetAmount);
                tRepData.EditRecordData(nRecordPos, 2, nCurrentQty.ToString());
                tRepData.EditRecordData(nRecordPos, 3, fCurrentAmount.ToString());
            }

        }

        /// <summary>
        /// Works out the amount of the transaction that is applicable to the chosen V.A.T. category
        /// </summary>
        /// <param name="sTransactionInfo">The details of the transaction</param>
        /// <param name="nCategory">The V.A.T. category to find out for</param>
        /// <returns>The amount of the transaction that is applicable to the chosen V.A.T. category</returns>
        private float WorkOutVATOnTransaction(string[,] sTransactionInfo, int nCategory)
        {
            int nOfItems = Convert.ToInt32(sTransactionInfo[0, 0]);
            int nOfPaymentMethods = Convert.ToInt32(sTransactionInfo[0, 1]);

            float fVat00 = 0.0f, fVat01 = 0.0f, fVat02 = 0.0f;
            for (int i = 1; i <= nOfItems; i++)
            {
                Item item = new Item(tStock.GetRecordFrom(sTransactionInfo[i, 0], 0));
                string sVATCode = item.VATRate;
                switch (sVATCode)
                {
                    case "Z0":
                        fVat02 = FixFloatError(fVat02 + (float)Convert.ToDecimal(sTransactionInfo[i, 2]));
                        break;
                    case "X0":
                        fVat00 = FixFloatError(fVat00 + (float)Convert.ToDecimal(sTransactionInfo[i, 2]));
                        break;
                    case "I1":
                        fVat01 = FixFloatError(fVat01 + (float)Convert.ToDecimal(sTransactionInfo[i, 2]));
                        break;
                    case "E1":
                        fVat01 = FixFloatError(fVat01 + (float)Convert.ToDecimal(sTransactionInfo[i, 2]));
                        break;
                }
            }
            for (int i = nOfItems + 1; i <= nOfItems + nOfPaymentMethods; i++)
            {
                if (sTransactionInfo[i, 0] == "DEPO")
                {
                    fVat01 = FixFloatError(fVat01 - (float)Convert.ToDecimal(sTransactionInfo[i, 1]));
                }
                else if (sTransactionInfo[i, 0].StartsWith("CHRG") && nCategory == 1)
                {
                    float fChrgAmount = FixFloatError((float)Convert.ToDecimal(sTransactionInfo[i, 1]));
                    if (fChrgAmount > fVat01)
                        fVat01 = 0.0f;
                    else
                        fVat01 = FixFloatError(fVat01 - fChrgAmount);
                }
            }
            if (nCategory == 0)
                return fVat00;
            else if (nCategory == 1)
                return fVat01;
            else if (nCategory == 2)
                return fVat02;
            else
                return 0.0f;
        }
        private float WorkOutVATOnTransaction(string[,] sTransactionInfo, string sVATCode, bool bReceipt)
        {
            int nOfItems = Convert.ToInt32(sTransactionInfo[0, 0]);
            int nOfPaymentMethods = Convert.ToInt32(sTransactionInfo[0, 1]);

            float fVat = 0;
            for (int i = 1; i <= nOfItems; i++)
            {
                Item item = new Item(tStock.GetRecordFrom(sTransactionInfo[i, 0], 0));
                if (item.ItemCategory != 6)
                {
                    if (item.VATRate == sVATCode)
                    {
                        fVat = FixFloatError(fVat + (float)Convert.ToDecimal(sTransactionInfo[i, 2]));
                    }
                }
                else
                {
                    if (!bReceipt)
                    {
                        float fCost = (float)Convert.ToDecimal(tCommission.GetRecordFrom(item.Barcode, 0, true)[1]);
                        if (item.VATRate == sVATCode)
                        {
                            fVat = FixFloatError(fVat + ((float)Convert.ToDecimal(sTransactionInfo[i, 2]) - fCost));
                        }
                        else if (sVATCode == "X0")
                        {
                            fVat = FixFloatError(fVat + fCost);
                        }
                    }
                    else
                    {
                        if (sVATCode == "X0")
                        {
                            fVat = FixFloatError(fVat + (float)Convert.ToDecimal(sTransactionInfo[i, 2]));
                        }
                    }
                }
            }
            for (int i = nOfItems + 1; i <= nOfItems + nOfPaymentMethods; i++)
            {
                if (sTransactionInfo[i, 0] == "DEPO" && bIsVATRateGreaterThanZero(sVATCode))
                {
                    fVat = FixFloatError(fVat - (float)Convert.ToDecimal(sTransactionInfo[i, 1]));
                }
                else if (sTransactionInfo[i, 0].StartsWith("CHRG") && bIsVATRateGreaterThanZero(sVATCode))
                {
                    float fChrgAmount = FixFloatError((float)Convert.ToDecimal(sTransactionInfo[i, 1]));
                    if (fChrgAmount > fVat)
                        fVat = 0.0f;
                    else
                        fVat = FixFloatError(fVat - fChrgAmount);
                }
            }

            return fVat;
        }

        /// <summary>
        /// Removes a transaction from the databases
        /// </summary>
        /// <param name="nTransactionNumber">The transaction number to remove</param>
        public
        
        bool bIsVATRateGreaterThanZero(string sVATCode)
        {
            string[] sCodes = GetVATCodes();
            float[] fRates = GetVATRates();
            for (int i = 0; i < sCodes.Length; i++)
            {
                if (sVATCode == sCodes[i])
                {
                    if (fRates[i] > 0)
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }

        public void RemoveTransactionFromDatabases(int nTransactionNumber)
        {
            string[,] sTransactionInfo = GetTransactionInfo(nTransactionNumber.ToString());
            int nOfItems = Convert.ToInt32(sTransactionInfo[0, 0]);
            int nOfPaymentMethods = Convert.ToInt32(sTransactionInfo[0, 1]);
            // First array element in format { NumberOfItems, NumberOfPaymentMethods, TransactionDateTime, SpecialTransaction }
            // Item array in format { Item code, Item Description, Price Paid, Discount, Quantity }
            // Payment method array format { PaymentCode, Amount, Blank, Blank, Blank }
            //
            // SpecialTransactions can be CASHPAIDOUT, SPECIFICREFUND
            
            // Start of RepData

            // I had to split this part down into smaller chunks, as it was huge, complicated and didn't
            // work properly before!
            float fTotalValueOfTransaction = 0.0f;
            for (int i = 1; i <= nOfItems; i++)
            {
                RemoveItem(sTransactionInfo[i, 0], FixFloatError((float)Convert.ToDecimal(sTransactionInfo[i, 2])), FixFloatError((float)Convert.ToDecimal(sTransactionInfo[i, 3])), Convert.ToInt32(sTransactionInfo[i, 4]), false);
                fTotalValueOfTransaction = FixFloatError(fTotalValueOfTransaction + (float)Convert.ToDecimal(sTransactionInfo[i, 2]));
            }
            
            for (int i = nOfItems + 1; i <= nOfItems + nOfPaymentMethods; i++)
            {
                if (sTransactionInfo[i, 0].StartsWith("CHRG"))
                    RemovePaymentMethodFromRepData(GetChargeToAccountTransactionInfo(nTransactionNumber.ToString()), FixFloatError((float)Convert.ToDecimal(sTransactionInfo[i, 1])), false);
                else
                {
                    float fAmountWithoutExcess = (float)Convert.ToDecimal(sTransactionInfo[i, 1]);
                    if (sTransactionInfo[i, 0].StartsWith("CASH"))
                    {
                        if (sTransactionInfo[i, 2] != null)
                        {
                            fAmountWithoutExcess -= (float)Convert.ToDecimal(sTransactionInfo[i, 2]);
                            fAmountWithoutExcess = FixFloatError(fAmountWithoutExcess);
                        }
                    }
                    RemovePaymentMethodFromRepData(sTransactionInfo[i, 0], fAmountWithoutExcess, false);
                }
            }
            if (tVAT != null)
            {
                string[] sCodes = GetVATCodes();
                for (int i = 0; i < sCodes.Length; i++)
                {
                    RemoveVAT(sCodes[i], WorkOutVATOnTransaction(sTransactionInfo, sCodes[i], false), false);
                }
            }
            else
            {
                RemoveVAT(0, WorkOutVATOnTransaction(sTransactionInfo, 0), false);
                RemoveVAT(1, WorkOutVATOnTransaction(sTransactionInfo, 1), false);
                RemoveVAT(2, WorkOutVATOnTransaction(sTransactionInfo, 2), false);
            }

            // Sort out transaction END in Repdata

            int nEndPos = 0;
            tRepData.SearchForRecord("END", 1, ref nEndPos);

            string[] sCurrentEndData = tRepData.GetRecordFrom(nEndPos);
            float fCurrentEnd = FixFloatError((float)Convert.ToDecimal(sCurrentEndData[3]));
            fCurrentEnd -= 0.01f; // Move end position back one transaction
            fCurrentEnd = FixFloatError(fCurrentEnd);
            tRepData.EditRecordData(nEndPos, 3, fCurrentEnd.ToString());

            // Sort out NoTran record
            int nNoTranRecordPos = 0;
            tRepData.SearchForRecord("NOTRAN", 1, ref nNoTranRecordPos);
            string[] sNoTranCurrentData = tRepData.GetRecordFrom(nNoTranRecordPos);

            int nNoTranCurrentQuantity = Convert.ToInt32(sNoTranCurrentData[2]);
            int nNoTranNewQuantity = nNoTranCurrentQuantity - 1;

            float fNoTranCurrentAmount = FixFloatError((float)Convert.ToDecimal(sNoTranCurrentData[3].TrimEnd('\0')));
            float fNewTransactionAmount = FixFloatError(fNoTranCurrentAmount - fTotalValueOfTransaction);

            tRepData.EditRecordData(nNoTranRecordPos, 2, nNoTranNewQuantity.ToString());
            tRepData.EditRecordData(nNoTranRecordPos, 3, fNewTransactionAmount.ToString());

            // Sort out charge to account numbers in REPDATA

            int nCurrentPos = 0;
            string[] sChargeData = tRepData.GetRecordFrom("CA", 0, nCurrentPos, ref nCurrentPos);
            while (sChargeData.Length >= 2)
            {
                string sCurrentTransactionNumber = "";
                for (int i = sChargeData[1].Length-1; i >= sChargeData[1].Length - 6; i -= 1)
                {
                    sCurrentTransactionNumber = sChargeData[1][i].ToString() + sCurrentTransactionNumber;
                }
                int nTranNum = Convert.ToInt32(sCurrentTransactionNumber);
                if (nTranNum > nTransactionNumber) // If the transaction was entered after the one being removed
                {
                    nTranNum -= 1;
                    string sNewTransactionNumber = nTranNum.ToString();
                    while (sNewTransactionNumber.Length < 6)
                        sNewTransactionNumber = "0" + sNewTransactionNumber;
                    sChargeData[1] = sChargeData[1].Replace(sCurrentTransactionNumber, sNewTransactionNumber);
                    tRepData.EditRecordData(nCurrentPos, 1, sChargeData[1]);
                    nCurrentPos++;
                    sChargeData = tRepData.GetRecordFrom("CA", 0, nCurrentPos, ref nCurrentPos);
                }
            }

            // Add / Update record saying how much has been removed today

            string[] sRemoveAdd = { "RE", nTransactionNumber.ToString(), tsCurrentTillSettings.StaffNumber.ToString(), FormatMoneyForDisplay(fTotalValueOfTransaction), "0.00" };
            tRepData.AddRecord(sRemoveAdd);

            tRepData.SaveToFile(GTill.Properties.Settings.Default.sRepDataLoc);

            // End of RepData

            // Start of TData

            bool bRecordFound = true;
            string sTransactionNumber = nTransactionNumber.ToString();
            while (sTransactionNumber.Length < 6)
                sTransactionNumber = "0" + sTransactionNumber;
            do
            {
                int nRecordPos = 0;
                bRecordFound = tTData.SearchForRecord(sTransactionNumber, 0, ref nRecordPos);
                if (bRecordFound)
                    tTData.DeleteRecord(nRecordPos);
            }
            while (bRecordFound);

            // Edit Transaction Numbers in TData

            int nOfRecordsInTData = 0;
            string[,] sTDataContents = tTData.SearchAndGetAllMatchingRecords(0, "", ref nOfRecordsInTData);

            for (int i = 0; i < nOfRecordsInTData; i++)
            {
                int nCurrentTransactionNumber = Convert.ToInt32(sTDataContents[i, 0]);
                if (nCurrentTransactionNumber > nTransactionNumber)
                {
                    nCurrentTransactionNumber -= 1;
                    string sNewNum = nCurrentTransactionNumber.ToString();
                    while (sNewNum.Length < 6)
                        sNewNum = "0" + sNewNum;
                    tTData.EditRecordData(i, 0, sNewNum);
                }
            }

            tTData.SaveToFile(GTill.Properties.Settings.Default.sTDataLoc);

            // End of TDATA

            // Start of THDR

            bRecordFound = true;
            do
            {
                int nRecordPos = 0;
                bRecordFound = tTHDR.SearchForRecord(sTransactionNumber, 0, ref nRecordPos);
                if (bRecordFound)
                    tTHDR.DeleteRecord(nRecordPos);
            }
            while (bRecordFound);

            // Edit transaction numbers in THDR

            int nOfRecordsInTHdr = 0;
            string[,] sTHdrContents = tTHDR.SearchAndGetAllMatchingRecords(0, "", ref nOfRecordsInTHdr);

            for (int i = 0; i < nOfRecordsInTHdr; i++)
            {
                int nCurrentTransactionNumber = Convert.ToInt32(sTHdrContents[i, 0]);
                if (nCurrentTransactionNumber > nTransactionNumber)
                {
                    nCurrentTransactionNumber -= 1;
                    string sNewNum = nCurrentTransactionNumber.ToString();
                    while (sNewNum.Length < 6)
                        sNewNum = "0" + sNewNum;
                    tTHDR.EditRecordData(i, 0, sNewNum);
                }
            }

            tTHDR.SaveToFile(GTill.Properties.Settings.Default.sTHdrLoc);
            
            // End of THDR
        }

        // For the Lookup Dialog

        /// <summary>
        /// Gets all the items in a user-defined category
        /// </summary>
        /// <param name="sCategory">The category's code</param>
        /// <param name="nOfResults">A reference to the number of results</param>
        /// <returns>An array of records from STOCK of products from the chosen category</returns>
        public string[,] sGetAllInCategory(string sCategory, ref int nOfResults)
        {
            sCategory = sCategory.TrimEnd(' ');
            string[,] sPossibilities = tStock.SearchAndGetAllMatchingRecords(9, sCategory, ref nOfResults);
            return sPossibilities;
        }

        /// <summary>
        /// Gets a list of items that contain the partial barcode entered
        /// </summary>
        /// <param name="sBarcode">The partial barcode to use to search</param>
        /// <param name="nOfResults">A reference to store the number of results</param>
        /// <returns>The results of the partial barcode search as record arrays from the STOCK database</returns>
        public string[,] sGetAccordingToPartialBarcode(string sBarcode, ref int nOfResults)
        {
            string[,] sPossibilities = tStock.SearchAndGetAllMatchingRecords(0, sBarcode, ref nOfResults);
            return sPossibilities;
        }

        /// <summary>
        /// Gets the stock level of an item
        /// </summary>
        /// <param name="sBarcode">The barcode of the item</param>
        /// <returns>The stock level of the specified item</returns>
        public float GetItemStockLevel(string sBarcode)
        {
            string[] sInfo = tStkLevel.GetRecordFrom(sBarcode, 0, true);
            float nToReturn = -1024;
            if (sInfo[0] != null)
                nToReturn = (float)Convert.ToDecimal(sInfo[2].TrimStart(' '));
            return nToReturn;
        }

        /// <summary>
        /// Gets a list of items that contain the partial description entered
        /// </summary>
        /// <param name="sDesc">The description keywords of the item to be searched for</param>
        /// <param name="nOfResults">A reference to store the number of results found</param>
        /// <returns>The results in a 2D array of records from the STOCK database</returns>
        public string[,] sGetAccordingToPartialDescription(string sDesc, ref int nOfResults)
        {
            return tStock.SearchAndGetAllMatchingRecords(1, sDesc.Split(' '), ref nOfResults);
        }

        /// <summary>
        /// Gets the barcode associated with the function keys on the keyboard
        /// </summary>
        /// <param name="sFKeyCode">The key code "F12" = F12, "SF12" = Shift Key + F12</param>
        /// <returns>The barcode if any is associated</returns>
        public string sBarcodeFromFunctionKey(string sFKeyCode)
        {
            switch (sFKeyCode)
            {
                case "F1":
                    return tPresets.GetRecordFrom(0)[0];
                case "F2":
                    return tPresets.GetRecordFrom(1)[0];
                    break;
                case "F3":
                    return tPresets.GetRecordFrom(2)[0];
                    break;
                case "F4":
                    return tPresets.GetRecordFrom(3)[0];
                    break;
                case "F5":
                    return tPresets.GetRecordFrom(4)[0];
                    break;
                case "F6":
                    return tPresets.GetRecordFrom(5)[0];
                    break;
                case "F7":
                    return tPresets.GetRecordFrom(6)[0];
                    break;
                case "F8":
                    return tPresets.GetRecordFrom(7)[0];
                    break;
                case "F9":
                    return tPresets.GetRecordFrom(8)[0];
                    break;
                case "F10":
                    return tPresets.GetRecordFrom(9)[0];
                    break;
                case "F11":
                    return tPresets.GetRecordFrom(10)[0];
                    break;
                case "F12":
                    return tPresets.GetRecordFrom(11)[0];
                    break;
                case "SF1":
                    return tPresets.GetRecordFrom(12)[0];
                    break;
                case "SF2":
                    return tPresets.GetRecordFrom(13)[0];
                    break;
                case "SF3":
                    return tPresets.GetRecordFrom(14)[0];
                    break;
                case "SF4":
                    return tPresets.GetRecordFrom(15)[0];
                    break;
                case "SF5":
                    return tPresets.GetRecordFrom(16)[0];
                    break;
                case "SF6":
                    return tPresets.GetRecordFrom(17)[0];
                    break;
                case "SF7":
                    return tPresets.GetRecordFrom(18)[0];
                    break;
                case "SF8":
                    return tPresets.GetRecordFrom(19)[0];
                    break;
                case "SF9":
                    return tPresets.GetRecordFrom(20)[0];
                    break;
                case "SF10":
                    return tPresets.GetRecordFrom(21)[0];
                    break;
                case "SF11":
                    return tPresets.GetRecordFrom(22)[0];
                    break;
                case "SF12":
                    return tPresets.GetRecordFrom(23)[0];
                    break;
            }
            return "";
        }

        /// <summary>
        /// Edits the preset function keys
        /// </summary>
        /// <param name="sKeyToEdit">The key to edit</param>
        /// <param name="sNewBarcode">The new barcode of that key</param>
        public void EditPresetKeys(string sKeyToEdit, string sNewBarcode)
        {
            int nRecordToEdit = 0;
            switch (sKeyToEdit)
            {
                case "F1":
                    nRecordToEdit = 0;
                    break;
                case "F2":
                    nRecordToEdit = 1;
                    break;
                case "F3":
                    nRecordToEdit = 2;
                    break;
                case "F4":
                    nRecordToEdit = 3;
                    break;
                case "F5":
                    nRecordToEdit = 4;
                    break;
                case "F6":
                    nRecordToEdit = 5;
                    break;
                case "F7":
                    nRecordToEdit = 6;
                    break;
                case "F8":
                    nRecordToEdit = 7;
                    break;
                case "F9":
                    nRecordToEdit = 8;
                    break;
                case "F10":
                    nRecordToEdit = 9;
                    break;
                case "F11":
                    nRecordToEdit = 10;
                    break;
                case "F12":
                    nRecordToEdit = 11;
                    break;
                case "SF1":
                    nRecordToEdit = 12;
                    break;
                case "SF2":
                    nRecordToEdit = 13;
                    break;
                case "SF3":
                    nRecordToEdit = 14;
                    break;
                case "SF4":
                    nRecordToEdit = 15;
                    break;
                case "SF5":
                    nRecordToEdit = 16;
                    break;
                case "SF6":
                    nRecordToEdit = 17;
                    break;
                case "SF7":
                    nRecordToEdit = 18;
                    break;
                case "SF8":
                    nRecordToEdit = 19;
                    break;
                case "SF9":
                    nRecordToEdit = 20;
                    break;
                case "SF10":
                    nRecordToEdit = 21;
                    break;
                case "SF11":
                    nRecordToEdit = 22;
                    break;
                case "SF12":
                    nRecordToEdit = 23;
                    break;
            }

            tPresets.EditRecordData(nRecordToEdit, 0, sNewBarcode);
            tPresets.SaveToFile(GTill.Properties.Settings.Default.sPresetsLoc);
        }


        // For the Account Selection Dialog

        /// <summary>
        /// Gets a list of accounts
        /// </summary>
        /// <param name="nOfResults">A reference to store the number of results of the search</param>
        /// <returns>All the accounts</returns>
        public string[,] sGetAccounts(ref int nOfResults)
        {
            return tAccStat.SearchAndGetAllMatchingRecords(1, "B",ref nOfResults);
        }

        /// <summary>
        /// Gets the details of accounts
        /// </summary>
        /// <param name="sCode">The code of the account</param>
        /// <returns>Details about the account including address, name etc</returns>
        public string[] sGetAccountDetailsFromCode(string sCode)
        {
            return tAccStat.GetRecordFrom(sCode, 0);
        }

        /// <summary>
        /// Gets account details from the description of the account
        /// </summary>
        /// <param name="sCode">The description of the account</param>
        /// <returns>Details about the account including address, code etc</returns>
        public string[] sGetAccountDetailsFromDesc(string sCode)
        {
            return tAccStat.GetRecordFrom(sCode.TrimEnd(' '),2);
        }

        // For the Main Menu

        /// <summary>
        /// Pays cash out from the till
        /// </summary>
        /// <param name="sAmount">The amount to pay out</param>
        public void PayCashOut(string sAmount)
        {
            float nCurrentTransactionNumber = (float)Convert.ToDecimal(tRepData.GetRecordFrom("END", 1)[3]);
            nCurrentTransactionNumber+=0.01f;
            int nRecordPos = 0;
            tRepData.SearchForRecord("END", 1, ref nRecordPos);
            float fNewTransNum = nCurrentTransactionNumber;
            fNewTransNum = TillEngine.FixFloatError(fNewTransNum);
            tRepData.EditRecordData(nRecordPos, 3, fNewTransNum.ToString());

            float fAmountToPayOut = TillEngine.FixFloatError(TillEngine.fFormattedMoneyString(sAmount) * -1);
            if (!tRepData.SearchForRecord("CAPO", 1, ref nRecordPos))
            {
                string[] sToAdd = { "CR", "CAPO", "0", "0", "" };
                tRepData.AddRecord(sToAdd);
                tRepData.SearchForRecord("CAPO", 1, ref nRecordPos);
            }

            string[] sCurrentContents = tRepData.GetRecordFrom(nRecordPos);
            float fCurrentAmount = (float)Convert.ToDouble(sCurrentContents[3]);
            int nCurrentQty = Convert.ToInt32(sCurrentContents[2]);
            nCurrentQty++;
            fCurrentAmount += fAmountToPayOut;
            fCurrentAmount = TillEngine.FixFloatError(fCurrentAmount);
            sCurrentContents[3] = fCurrentAmount.ToString();
            sCurrentContents[2] = nCurrentQty.ToString();
            tRepData.EditRecordData(nRecordPos, 3, sCurrentContents[3]);
            tRepData.EditRecordData(nRecordPos, 2, sCurrentContents[2]);

            if (!tRepData.SearchForRecord("NOTRAN", 1, ref nRecordPos))
            {
                string[] sToAdd = { "CR", "NOTRAN", "0", "0", "0" };
                tRepData.AddRecord(sToAdd);
                tRepData.SearchForRecord("NOTRAN", 1, ref nRecordPos);
            }

            sCurrentContents = tRepData.GetRecordFrom(nRecordPos);
            fCurrentAmount = (float)Convert.ToDouble(sCurrentContents[3]);
            fCurrentAmount += fAmountToPayOut;
            fCurrentAmount = TillEngine.FixFloatError(fCurrentAmount);
            nCurrentQty = Convert.ToInt32(sCurrentContents[2]);
            nCurrentQty++;
            tRepData.EditRecordData(nRecordPos, 2, nCurrentQty.ToString());
            tRepData.EditRecordData(nRecordPos, 3, fCurrentAmount.ToString());

            if (!tRepData.SearchForRecord("CASH", 1, ref nRecordPos))
            {
                string[] sToAdd = { "CR", "CASH", "0", "0", "" };
                tRepData.AddRecord(sToAdd);
                tRepData.SearchForRecord("CASH", 1, ref nRecordPos);
            }

            sCurrentContents = tRepData.GetRecordFrom(nRecordPos);
            fCurrentAmount = (float)Convert.ToDouble(sCurrentContents[3]);
            nCurrentQty = Convert.ToInt32(sCurrentContents[2]);
            nCurrentQty++;
            fCurrentAmount += fAmountToPayOut;
            fCurrentAmount = TillEngine.FixFloatError(fCurrentAmount);
            sCurrentContents[3] = fCurrentAmount.ToString();
            sCurrentContents[2] = nCurrentQty.ToString();
            tRepData.EditRecordData(nRecordPos, 3, sCurrentContents[3]);
            tRepData.EditRecordData(nRecordPos, 2, sCurrentContents[2]);
            tRepData.SaveToFile(GTill.Properties.Settings.Default.sRepDataLoc);

            string sTransNum = FixFloatError(nCurrentTransactionNumber * 100).ToString();
            while (sTransNum.Length < 6)
                sTransNum = "0" + sTransNum;
            
            string[] sThdrLine = { sTransNum, "00", "0", fAmountToPayOut.ToString(), "CAPO", sNewerDateTimeString() };
            tTHDR.AddRecord(sThdrLine);
            string[] sThdrLine2 = { sTransNum, "01", "", fAmountToPayOut.ToString(), "CASH", "" };
            tTHDR.AddRecord(sThdrLine2);
            tTHDR.SaveToFile(GTill.Properties.Settings.Default.sTHdrLoc);

            PrintCashPaidOut(-fAmountToPayOut);
        }

        /// <summary>
        /// Gets the newer (or alternative) date string for the databases (used for refunds)
        /// </summary>
        /// <returns>A string containing information about the date and time</returns>
        private string sNewerDateTimeString()
        {
            string sDay = DateTime.Now.Date.Day.ToString();
            while (sDay.Length < 2)
                sDay = "0" + sDay;

            string sMonth = DateTime.Now.Month.ToString();
            while (sMonth.Length < 2)
                sMonth = "0" + sMonth;

            string sYear = DateTime.Now.Year.ToString();
            sYear = sYear[2].ToString() + sYear[3].ToString();

            string sHour = DateTime.Now.Hour.ToString();
            while (sHour.Length < 2)
                sHour = "0" + sHour;

            string sMinute = DateTime.Now.Minute.ToString();
            while (sMinute.Length < 2)
                sMinute = "0" + sMinute;

            string sSecond = DateTime.Now.Second.ToString();
            while (sSecond.Length < 2)
                sSecond = "0" + sSecond;

            string sCurrentUser = tsCurrentTillSettings.StaffNumber.ToString();
            while (sCurrentUser.Length < 3)
                sCurrentUser = " " + sCurrentUser;

            string sResult = sDay + "/" + sMonth + "/" + sYear + sHour + ":" + sMinute + ":" + sSecond + sCurrentUser;
            return sResult;
        }

        /// <summary>
        /// Decrypts and returns the chosen password
        /// </summary>
        /// <param name="nPasswordLevel">The password level, 0 = Admin, 1 = Staff</param>
        /// <returns>The decrypted password</returns>
        public string GetPasswords(int nPasswordLevel)
        {
            string sPasswordRecordContents = tDetails.GetRecordFrom(5)[0];
            string sDecrypted = "";
            for (int i = 0; i < sPasswordRecordContents.TrimEnd(' ').Length; i += 6)
            {
                sDecrypted += ((char)(sPasswordRecordContents[i] - 128)).ToString();
                sDecrypted += ((char)(sPasswordRecordContents[i + 1] - 123)).ToString();
                sDecrypted += ((char)(sPasswordRecordContents[i + 2] - 120)).ToString();
                sDecrypted += ((char)(sPasswordRecordContents[i + 3] - 121)).ToString();
                sDecrypted += ((char)(sPasswordRecordContents[i + 4] - 122)).ToString();
                sDecrypted += ((char)(sPasswordRecordContents[i + 5] - 124)).ToString();
                sDecrypted += ",";
            }
            string[] sPasswords = sDecrypted.Split(',');
            for (int i = 0; i < sPasswords.Length; i++)
            {
                sPasswords[i] = sPasswords[i].TrimEnd(' ');

                // Ensures that all random characters at the end are removed
                // Restricts passwords to between 0 and Z on the ASCII table
                for (int x = 0; x < sPasswords[i].Length; x++)
                {
                    if (sPasswords[i][x] < '0' || sPasswords[i][x] > 'Z')
                    {
                        sPasswords[i] = sPasswords[i].Substring(0, x);
                        break;
                    }
                }
            }
            return sPasswords[nPasswordLevel];
        }

        // For the Lookup Transactions Form

        /// <summary>
        /// Gets information about a transaction including items, payment methods, discount etc
        /// </summary>
        /// <param name="sTransactionNumber">The transaction number</param>
        /// <returns>A 2D array containing details about the item</returns>
        public string[,]  GetTransactionInfo(string sTransactionNumber)
        {
            // First array element in format { NumberOfItems, NumberOfPaymentMethods, TransactionDateTime, SpecialTransaction }
            // Item array in format { Item code, Item Description, Price Paid, Discount, Quantity }
            // Payment method array format { PaymentCode, Amount, Blank, Blank, Blank }
            //
            // SpecialTransactions can be CASHPAIDOUT, SPECIFICREFUND, VOID
            int nOfItems = 0;
            string[,] sTDATARecords = tTData.SearchAndGetAllMatchingRecords(0, sTransactionNumber, ref nOfItems);
            int nOfPaymentMethods = 0;
            string[,] sPaymentMethods = tTHDR.SearchAndGetAllMatchingRecords(0, sTransactionNumber, ref nOfPaymentMethods);
            nOfPaymentMethods -= 1;
            string[,] sToReturn = new string[nOfItems + nOfPaymentMethods + 1,5];
            sToReturn[0, 0] = nOfItems.ToString();
            sToReturn[0, 1] = nOfPaymentMethods.ToString();
            sToReturn[0, 2] = sPaymentMethods[0, 5];
            if (sPaymentMethods[0, 4].TrimEnd(' ') != "SALE")
            {
                switch (sPaymentMethods[0, 4].TrimEnd(' '))
                {
                    case "CAPO":
                        sToReturn[0, 3] = "CASHPAIDOUT";
                        break;
                    case "SREF":
                        sToReturn[0, 3] = "SPECIFICREFUND";
                        break;
                    case "GREF":
                        sToReturn[0, 3] = "GENERALREFUND";
                        break;
                    case "RONA":
                        sToReturn[0, 3] = "RECEIVEDONACCOUNT";
                        break;
                }
            }
            else
            {
                sToReturn[0, 3] = "SALE";
                if (sPaymentMethods[0, 2].TrimEnd('\0') == "1")
                {
                    sToReturn[0, 3] = "VOID";
                    string sUserNumVoided = sPaymentMethods[0, 5].TrimEnd(' ')[sPaymentMethods[0, 5].TrimEnd(' ').Length - 3].ToString() + sPaymentMethods[0, 5][sPaymentMethods[0, 5].TrimEnd(' ').Length - 2].ToString() + sPaymentMethods[0, 5][sPaymentMethods[0, 5].TrimEnd(' ').Length - 1].ToString();
                    sUserNumVoided = sUserNumVoided.TrimStart(' ');
                    int nUserNum = 0;
                    try
                    {
                        nUserNum = Convert.ToInt32(sUserNumVoided);
                    }
                    catch
                    {
                        nUserNum = 0;
                        GTill.ErrorHandler.LogError("Could not work out user number that voided this transaction. Press continue to use User number 1");
                        throw new NotSupportedException("Could not work out user number that voided this transaction. Press continue to use User number 1");
                    }
                    string sUserName = "";
                    if (nUserNum < 100)
                        sUserName = GetStaffName(nUserNum);
                    else
                        sUserName = "???";
                    sToReturn[0, 3] += "," + sUserName;
                }
            }
            float fTotalValueOfTransaction = 0.0f;
            for (int i = 1; i <= nOfItems; i++)
            {
                sToReturn[i, 0] = sTDATARecords[i - 1, 3]; // Barcode
                sToReturn[i, 1] = sTDATARecords[i - 1, 4]; // Description
                sToReturn[i, 2] = FixFloatError((float)Convert.ToDecimal(sTDATARecords[i - 1, 6]) - (float)Convert.ToDecimal(sTDATARecords[i-1,8])).ToString(); // Price Paid
                fTotalValueOfTransaction += (float)Convert.ToDecimal(sToReturn[i, 2]);
                sToReturn[i, 3] = sTDATARecords[i - 1, 8]; // Discount
                sToReturn[i, 4] = sTDATARecords[i - 1, 5]; // Quantity
            }
            float fTotalAmountPaid = 0.0f;
            for (int i = nOfItems + 1; i <= (nOfItems + nOfPaymentMethods); i++)
            {
                sToReturn[i, 0] = sPaymentMethods[i - (nOfItems), 4];
                if (sToReturn[i, 0].StartsWith("CRCD"))
                    sToReturn[i, 0] += sPaymentMethods[i - nOfItems, 5].TrimEnd(' ');
                if (sToReturn[i, 0].StartsWith("CHRG"))
                    sToReturn[i, 0] += "," + sPaymentMethods[i - (nOfItems), 5] + "," + sTransactionNumber;
                sToReturn[i, 1] = sPaymentMethods[i - (nOfItems), 3];
                fTotalAmountPaid += (float)Convert.ToDecimal(sToReturn[i, 1]);
            }
            fTotalValueOfTransaction = FixFloatError(fTotalValueOfTransaction);
            fTotalAmountPaid = FixFloatError(fTotalAmountPaid);
            float fExcess = FixFloatError(fTotalAmountPaid - fTotalValueOfTransaction);
            if (fExcess > 0.0f)
            {
                for (int i = 0; i < nOfPaymentMethods; i++)
                {
                    if (sToReturn[i + 1 + nOfItems, 0].StartsWith("CASH") && (float)Convert.ToDecimal(sToReturn[i + 1 + nOfItems, 1]) > fExcess)
                    {
                        sToReturn[i + 1 + nOfItems, 2] = fExcess.ToString();
                    }
                }
            }
            return sToReturn;
        }

        /// <summary>
        /// Gets a list of all the transactions from the current day
        /// </summary>
        /// <returns>A list of transaction numbers</returns>
        public string[] GetListOfTransactionNumbers()
        {
            float fStartPos = (float)Convert.ToDecimal(tRepData.GetRecordFrom("START", 1)[3]);
            fStartPos *= 100.0f;
            fStartPos = FixFloatError(fStartPos);

            float fEndPos = (float)Convert.ToDecimal(tRepData.GetRecordFrom("END", 1)[3]);
            fEndPos *= 100.0f;
            fEndPos = FixFloatError(fEndPos);

            int nOfTransactions = Convert.ToInt32(fEndPos - fStartPos)+1;
            string[] sToReturn = new string[nOfTransactions];

            for (int i = 0; i < nOfTransactions; i++)
            {
                sToReturn[i] = (fStartPos + (float)i).ToString();
            }

            return sToReturn;
        }

        // For the Money In Till Form

        /// <summary>
        /// Gets the amount of money in the till
        /// </summary>
        /// 
        /// <returns>
        /// Cash
        /// Credit Cards
        /// Cheques
        /// Vouchers
        /// </returns>
        public float[] GetAmountOfMoneyInTill()
        {
            float[] fToReturn = new float[4];
            for (int i = 0; i < fToReturn.Length; i++)
            {
                fToReturn[i] = 0.0f;
            }
            int nFieldNum = 0;
            if (tRepData.SearchForRecord("CASH", 1, ref nFieldNum))
                fToReturn[0] = FixFloatError((float)Convert.ToDecimal(tRepData.GetRecordFrom(nFieldNum)[3]));
            if (tRepData.SearchForRecord("CRCD", 1, ref nFieldNum))
                fToReturn[1] = FixFloatError((float)Convert.ToDecimal(tRepData.GetRecordFrom(nFieldNum)[3]));
            if (tRepData.SearchForRecord("CHEQ", 1, ref nFieldNum))
                fToReturn[2] = FixFloatError((float)Convert.ToDecimal(tRepData.GetRecordFrom(nFieldNum)[3]));
            if (tRepData.SearchForRecord("VOUC", 1, ref nFieldNum))
                fToReturn[3] = FixFloatError((float)Convert.ToDecimal(tRepData.GetRecordFrom(nFieldNum)[3]));

            return fToReturn;
        }

        // For Marking a transaction void

        /// <summary>
        /// Makes an item in REPDATA void
        /// </summary>
        /// <param name="sBarcode">The barcode of the item to make void</param>
        /// <param name="fNetAmount">The net amount of the item</param>
        /// <param name="fDiscount">The amount of discount that was applied to the item</param>
        /// <param name="nQuantity">The quantity of the item</param>
        private void VoidItem(string sBarcode, float fNetAmount, float fDiscount, int nQuantity)
        {
            int nRecordPos = 0;
            float fNewGross = 0.0f;
            if (!tRepData.SearchForRecord(sBarcode.TrimEnd('\0'), 1, ref nRecordPos))
            {
                string[] sNew = { "ST", sBarcode, "0", "0", "0" };
                tRepData.AddRecord(sNew);
                tRepData.SearchForRecord(sBarcode, 1, ref nRecordPos);
            }

            string[] sCurrentContents = tRepData.GetRecordFrom(nRecordPos);

            int nCurrentQty = Convert.ToInt32(sCurrentContents[2]);
            int nNewQty = nCurrentQty - nQuantity;

            float fCurrentAmount = FixFloatError((float)Convert.ToDecimal(sCurrentContents[3]));
            float fNewNet = FixFloatError(fCurrentAmount - fNetAmount);

            if ((nNewQty == 0 && fNewGross != 0.0f))
            {
                GTill.ErrorHandler.LogError("Whilst voiding " + sBarcode + ", the new quantity would be 0, but the RepGross field would be " + FormatMoneyForDisplay(fNewNet) + "! I can carry on, but there may be a bug in the software. To carry on, press continue.");
                throw new NotSupportedException("Whilst voiding " + sBarcode + ", the new quantity would be 0, but the RepGross field would be " + FormatMoneyForDisplay(fNewNet) +"! I can carry on, but there may be a bug in the software. To carry on, press continue.");
            }

            tRepData.EditRecordData(nRecordPos, 2, nNewQty.ToString());
            tRepData.EditRecordData(nRecordPos, 3, fNewNet.ToString());

            // Sort out NSTCK, STOCK, NOITEM

            Item iTemp = new Item(tStock.GetRecordFrom(sBarcode.TrimEnd('\0'), 0));
            string sCode = "";
            if (iTemp.IsItemStock)
                sCode = "STOCK";
            else
                sCode = "NSTCK";

            tRepData.SearchForRecord(sCode, 1, ref nRecordPos);
            sCurrentContents = tRepData.GetRecordFrom(sCode, 1);
            fCurrentAmount = FixFloatError((float)Convert.ToDecimal(sCurrentContents[3]));
            fCurrentAmount -= fNetAmount;
            fCurrentAmount = FixFloatError(fCurrentAmount);
            nCurrentQty = Convert.ToInt32(sCurrentContents[2]);
            nCurrentQty -= nQuantity;
            tRepData.EditRecordData(nRecordPos, 3, fCurrentAmount.ToString());
            tRepData.EditRecordData(nRecordPos, 2, nCurrentQty.ToString());

            sCurrentContents = tRepData.GetRecordFrom("NOITEM", 1);
            tRepData.SearchForRecord("NOITEM", 1, ref nRecordPos);
            fCurrentAmount = FixFloatError((float)Convert.ToDecimal(sCurrentContents[3]));
            nCurrentQty = Convert.ToInt32(sCurrentContents[2]);
            nCurrentQty -= nQuantity;
            fCurrentAmount = FixFloatError(fCurrentAmount - fNetAmount);
            tRepData.EditRecordData(nRecordPos, 2, nCurrentQty.ToString());
            tRepData.EditRecordData(nRecordPos, 3, fCurrentAmount.ToString());

        }

        /// <summary>
        /// Marks V.A.T. as void
        /// </summary>
        /// <param name="nCategoryNumber">The V.A.T. category to mark as void</param>
        /// <param name="fAmount">The amount to mark as void</param>
        private void VoidVAT(int nCategoryNumber, float fAmount)
        {
            string sRepCode = "VAT";
            if (nCategoryNumber < 10)
                sRepCode += "0";
            sRepCode += nCategoryNumber.ToString();
            int nRecordPos = 0;
            if (!tRepData.SearchForRecord(sRepCode, 1, ref nRecordPos))
            {
                string[] sNew = { "CR", sRepCode, "0", "0", "0" };
                tRepData.AddRecord(sNew);
                tRepData.SearchForRecord(sRepCode, 1, ref nRecordPos);
            }

            string[] sCurrentContents = tRepData.GetRecordFrom(nRecordPos);
            float fCurrentAmount = FixFloatError((float)Convert.ToDecimal(sCurrentContents[3]));
            float fNewAmount = FixFloatError(fCurrentAmount - fAmount);
            sCurrentContents[3] = fNewAmount.ToString();
            tRepData.EditRecordData(nRecordPos, 3, fNewAmount.ToString());
        }
        private void VoidVAT(string sVATCode, float fAmount)
        {
            string sRepCode = "VAT" + sVATCode;
            int nRecordPos = 0;
            if (!tRepData.SearchForRecord(sRepCode, 1, ref nRecordPos))
            {
                string[] sNew = { "CR", sRepCode, "0", "0", "0" };
                tRepData.AddRecord(sNew);
                tRepData.SearchForRecord(sRepCode, 1, ref nRecordPos);
            }

            string[] sCurrentContents = tRepData.GetRecordFrom(nRecordPos);
            float fCurrentAmount = FixFloatError((float)Convert.ToDecimal(sCurrentContents[3]));
            float fNewAmount = FixFloatError(fCurrentAmount - fAmount);
            sCurrentContents[3] = fNewAmount.ToString();
            tRepData.EditRecordData(nRecordPos, 3, fNewAmount.ToString());
        }

        /// <summary>
        /// Marks a transaction as VOID
        /// </summary>
        /// <param name="nTransactionNumber">The transaction number</param>
        /// <param name="nStaffNumber">The staff member marking the transaction as VOID</param>
        public void VoidTransaction(int nTransactionNumber, int nStaffNumber)
        {
            string[,] sTransactionInfo = GetTransactionInfo(nTransactionNumber.ToString());
            int nOfItems = Convert.ToInt32(sTransactionInfo[0, 0]);
            int nOfPaymentMethods = Convert.ToInt32(sTransactionInfo[0, 1]);
            // First array element in format { NumberOfItems, NumberOfPaymentMethods, TransactionDateTime, SpecialTransaction }
            // Item array in format { Item code, Item Description, Price Paid, Discount, Quantity }
            // Payment method array format { PaymentCode, Amount, Blank, Blank, Blank }
            //
            // SpecialTransactions can be CASHPAIDOUT, SPECIFICREFUND

            // Start of RepData

            float fTotalValueOfTransaction = 0.0f;
            for (int i = 1; i <= nOfItems; i++)
            {
                VoidItem(sTransactionInfo[i, 0], FixFloatError((float)Convert.ToDecimal(sTransactionInfo[i, 2])), FixFloatError((float)Convert.ToDecimal(sTransactionInfo[i, 3])), Convert.ToInt32(sTransactionInfo[i, 4]));
                fTotalValueOfTransaction = FixFloatError(fTotalValueOfTransaction + (float)Convert.ToDecimal(sTransactionInfo[i, 2]));
            }
            for (int i = nOfItems + 1; i <= nOfItems + nOfPaymentMethods; i++)
            {
                float fAmountWithoutExcess = (float)Convert.ToDecimal(sTransactionInfo[i, 1]);
                if (sTransactionInfo[i, 0].StartsWith("CASH"))
                {
                    if (sTransactionInfo[i, 2] != null)
                    {
                        fAmountWithoutExcess -= (float)Convert.ToDecimal(sTransactionInfo[i, 2]);
                    }
                }
                RemovePaymentMethodFromRepData(sTransactionInfo[i, 0], FixFloatError((float)Convert.ToDecimal(fAmountWithoutExcess)), false);
            }
            if (tVAT == null)
            {
                VoidVAT(0, WorkOutVATOnTransaction(sTransactionInfo, 0));
                VoidVAT(1, WorkOutVATOnTransaction(sTransactionInfo, 1));
                VoidVAT(2, WorkOutVATOnTransaction(sTransactionInfo, 2));
            }
            else
            {
                string[] sVATCodes = GetVATCodes();
                for (int i = 0; i < sVATCodes.Length; i++)
                {
                    VoidVAT(sVATCodes[i], WorkOutVATOnTransaction(sTransactionInfo, sVATCodes[i], false));
                }
            }

            // Sort out NoTran record
            int nNoTranRecordPos = 0;
            tRepData.SearchForRecord("NOTRAN", 1, ref nNoTranRecordPos);
            string[] sNoTranCurrentData = tRepData.GetRecordFrom(nNoTranRecordPos);

            int nNoTranCurrentQuantity = Convert.ToInt32(sNoTranCurrentData[2]);
            int nNoTranNewQuantity = nNoTranCurrentQuantity - 1;

            float fNoTranCurrentAmount = FixFloatError((float)Convert.ToDecimal(sNoTranCurrentData[3].TrimEnd('\0')));
            float fNewTransactionAmount = FixFloatError(fNoTranCurrentAmount - fTotalValueOfTransaction);

            tRepData.EditRecordData(nNoTranRecordPos, 2, nNoTranNewQuantity.ToString());
            tRepData.EditRecordData(nNoTranRecordPos, 3, fNewTransactionAmount.ToString());

            tRepData.SaveToFile(GTill.Properties.Settings.Default.sRepDataLoc);

            // End of RepData

            // Start of TData

            string sTransactionNumber = nTransactionNumber.ToString();
            while (sTransactionNumber.Length < 6)
                sTransactionNumber = "0" + sTransactionNumber;
            int nRecordPos = 0;
            string[] sCurrentData;
            do
            {
                sCurrentData = tTData.GetRecordFrom(sTransactionNumber, 0, nRecordPos, ref nRecordPos);
                if (sCurrentData[0] != null)
                {
                    sCurrentData[2] = "1";
                    tTData.EditRecordData(nRecordPos, 2, sCurrentData[2]);
                    nRecordPos++;
                }
            } while (sCurrentData[0] != null);

            tTData.SaveToFile(GTill.Properties.Settings.Default.sTDataLoc);

            // Start of THDR

            sCurrentData = null;
            nRecordPos = 0;
            bool bSorted = false;
            string sStaffNum = nStaffNumber.ToString();
            while (sStaffNum.Length < 3)
                sStaffNum = " " + sStaffNum;
            do
            {
                sCurrentData = tTHDR.GetRecordFrom(sTransactionNumber, 0, nRecordPos, ref nRecordPos);
                if (sCurrentData[4].TrimEnd('\0') == "SALE")
                {
                    sCurrentData[2] = "1";
                    sCurrentData[5] = sCurrentData[5].TrimEnd(' ') + sStaffNum;
                    tTHDR.EditRecordData(nRecordPos, 2, sCurrentData[2]);
                    tTHDR.EditRecordData(nRecordPos, 5, sCurrentData[5]);
                    bSorted = true;
                }
                else
                {
                    nRecordPos++;
                }
            } while (!bSorted);

            tTHDR.SaveToFile(GTill.Properties.Settings.Default.sTHdrLoc);

            // End of THDR
            // Now print a receipt

            PrintTransactionVoidReceipt(nTransactionNumber, false);
        }

        // For the Refund Form

        /// <summary>
        /// Refunds a payment method
        /// </summary>
        /// <param name="sPaymentMethod">The code of the payment method</param>
        /// <param name="fAmount">The amount to refund</param>
        private void RefundPaymentMethod(string sPaymentMethod, float fAmount)
        {
            RemovePaymentMethodFromRepData(sPaymentMethod, fAmount, true);
        }

        /// <summary>
        /// Refunds an item
        /// </summary>
        /// <param name="sBarcode">The barcode of the item to refund</param>
        /// <param name="fAmount">The price of the item</param>
        /// <param name="nQuantity">The quantity of the item</param>
        /// <param name="sPaymentMethod">The code of the payment method</param>
        public void RefundItem(string sBarcode, float fAmount, int nQuantity, string sPaymentMethod)
        {
            // Increase Transaction Number in RepData
            int nNOTRANLoc = 0;
            tRepData.SearchForRecord("END", 1, ref nNOTRANLoc);
            float fEnd = (float)Convert.ToDecimal(tRepData.GetRecordFrom("END", 1)[3]);
            fEnd += 0.01f;
            int nTransactionNumber = Convert.ToInt32(fEnd * 100.0f);
            tRepData.EditRecordData(nNOTRANLoc, 3, fEnd.ToString());
            string sTransactionNumber = nTransactionNumber.ToString();
            while (sTransactionNumber.Length < 6)
                sTransactionNumber = "0" + sTransactionNumber;
            Item i = new Item(tStock.GetRecordFrom(sBarcode, 0, true));
            string sVatCode = i.VATRate;
            int nVatRate = 0;
            switch (sVatCode)
            {
                case "Z0":
                    nVatRate = 2;
                    break;
                case "X0":
                    nVatRate = 0;
                    break;
                case "E1":
                    nVatRate = 1;
                    break;
                case "I1":
                    nVatRate = 1;
                    break;
            }
            float fIndividualItemAmount = FixFloatError(fAmount / nQuantity);
            if (sPaymentMethod.StartsWith("CHRG"))
                sPaymentMethod += "," + sTransactionNumber;
            for (int n = 0; n < nQuantity; n++)
            {
                RemoveItem(sBarcode, fIndividualItemAmount, 0.0f, 1, true);
                if (tVAT == null)
                {
                    if (sPaymentMethod != "DEPO")
                        RemoveVAT(nVatRate, fIndividualItemAmount, true);
                }
                else
                {
                    if (sPaymentMethod != "DEPO")
                    {
                        if (i.ItemCategory != 6)
                        {
                            RemoveVAT(sVatCode, fIndividualItemAmount, true);
                        }
                        else
                        {
                            float fCost = (float)Convert.ToDecimal(tCommission.GetRecordFrom(i.Barcode, 0, true)[1]);
                            RemoveVAT(sVatCode, i.Amount - fCost, true);
                            RemoveVAT("X0", fCost, true);
                        }
                    }
                }
                RemovePaymentMethodFromRepData(sPaymentMethod, fIndividualItemAmount, true);
            }

            // Now add to TDATA the refund
            string[] sTDataRecord = { 
                                        sTransactionNumber,
                                        "01",
                                        "0",
                                        sBarcode,
                                        i.Description,
                                        (nQuantity * -1).ToString(),
                                        FixFloatError(fAmount * -1).ToString(),
                                        "1",
                                        "0",
                                        i.VATRate
                                    };
            tTData.AddRecord(sTDataRecord);

            // Now add to THdr Database
            string sCRDType = "";
            if (sPaymentMethod.StartsWith("CRCD"))
            {
                sCRDType = sPaymentMethod[4].ToString();
                sPaymentMethod = "CRCD";
            }
            else if (sPaymentMethod.StartsWith("CHRG"))
            {
                sCRDType = sPaymentMethod.Split(',')[1];
                sPaymentMethod = "CHRG";
            }
            else if (sPaymentMethod == "DEPO")
            {
                //sCRDType = sNewerDateTimeString();
            }

            string[] sTHdrRecord = {
                                       sTransactionNumber,
                                       "00",
                                       "0",
                                       FixFloatError(fAmount * -1).ToString(),
                                       "SREF",
                                       sNewerDateTimeString(),
                                       "",
                                       "",
                                       ""
                                   };
            string[] sSecondTHdrRecord = {
                                             sTransactionNumber,
                                             "01",
                                             "",
                                             FixFloatError(fAmount * -1).ToString(),
                                             sPaymentMethod,
                                             sCRDType,
                                             "",
                                             "",
                                             ""
                                         };

            tTHDR.AddRecord(sTHdrRecord);
            tTHDR.AddRecord(sSecondTHdrRecord);

            // Add SREF / Append it in REPDATA

            int nSRefFieldNum = 0;
            if (!tRepData.SearchForRecord("SREF", 1, ref nSRefFieldNum))
            {
                string[] sToAdd = {
                                      "CR",
                                      "SREF",
                                      "0",
                                      "0",
                                      ""
                                  };
                tRepData.AddRecord(sToAdd);
                tRepData.SearchForRecord("SREF", 1, ref nSRefFieldNum);
            }

            string[] sCurrentContents = tRepData.GetRecordFrom(nSRefFieldNum);
            int nCurrentQty = Convert.ToInt32(sCurrentContents[2]);
            nCurrentQty += nQuantity;
            float fCurrentAmount = FixFloatError((float)Convert.ToDecimal(sCurrentContents[3]));
            float fNewAmount = FixFloatError(fCurrentAmount - fAmount);
            tRepData.EditRecordData(nSRefFieldNum, 2, nCurrentQty.ToString());
            tRepData.EditRecordData(nSRefFieldNum, 3, fNewAmount.ToString());

            // Edit NOTRAN in RepData

            nNOTRANLoc = 0;
            tRepData.SearchForRecord("NOTRAN", 1, ref nNOTRANLoc);
            int fTranCount = Convert.ToInt32(tRepData.GetRecordFrom("NOTRAN", 1)[2]);
            fTranCount += nQuantity;
            float fTranValue = (float)Convert.ToDecimal(tRepData.GetRecordFrom("NOTRAN", 1)[3]);
            fTranValue += FixFloatError(fAmount * -1);
            fTranValue = FixFloatError(fTranValue);
            tRepData.EditRecordData(nNOTRANLoc, 2, fTranCount.ToString());
            tRepData.EditRecordData(nNOTRANLoc, 3, fTranValue.ToString());

            tRepData.SaveToFile(GTill.Properties.Settings.Default.sRepDataLoc);
            tTHDR.SaveToFile(GTill.Properties.Settings.Default.sTHdrLoc);
            tTData.SaveToFile(GTill.Properties.Settings.Default.sTDataLoc);

            PaymentMethod pmRefund = new PaymentMethod();
            if (sPaymentMethod == "CHRG")
                sPaymentMethod += "," + sCRDType;
            pmRefund.SetPaymentMethod(sPaymentMethod, 0.0f, fAmount);
            OpenTillDrawer(false);
            PrintSpecificRefund(i.Description, fAmount, pmRefund, nQuantity,false);
        }

        /// <summary>
        /// A general refund
        /// </summary>
        /// <param name="fAmount">The amount to refund</param>
        /// <param name="sPaymentMethod">The code of the payment method</param>
        public void RefundGeneral(float fAmount, string sPaymentMethod)
        {
            // Increase Transaction Number in RepData

            int nNOTRANLoc = 0;
            tRepData.SearchForRecord("END", 1, ref nNOTRANLoc);
            float fEnd = (float)Convert.ToDecimal(tRepData.GetRecordFrom("END", 1)[3]);
            fEnd += 0.01f;
            int nTransactionNumber = Convert.ToInt32(fEnd * 100.0f);
            tRepData.EditRecordData(nNOTRANLoc, 3, fEnd.ToString());
            string sTransactionNumber = nTransactionNumber.ToString();
            while (sTransactionNumber.Length < 6)
                sTransactionNumber = "0" + sTransactionNumber;
            if (sPaymentMethod.StartsWith("CHRG"))
                sPaymentMethod += "," + sTransactionNumber;
            if (sPaymentMethod != "DEPO")
                RemoveVAT(1, fAmount, true);
            RemovePaymentMethodFromRepData(sPaymentMethod, fAmount, true);
            // Now add to THdr Database
            string sCRDType = "";
            if (sPaymentMethod.StartsWith("CRCD"))
            {
                sCRDType = sPaymentMethod[4].ToString();
                sPaymentMethod = "CRCD";
            }
            else if (sPaymentMethod.StartsWith("CHRG"))
            {
                sCRDType = sPaymentMethod.Split(',')[1];
                sPaymentMethod = "CHRG";
            }
            else if (sPaymentMethod == "DEPO")
            {
                //sCRDType = sNewerDateTimeString();
            }

            string[] sTHdrRecord = {
                                       sTransactionNumber,
                                       "00",
                                       "0",
                                       FixFloatError(fAmount * -1).ToString(),
                                       "GREF",
                                       sNewerDateTimeString(),
                                       "",
                                       "",
                                       ""
                                   };
            string[] sSecondTHdrRecord = {
                                             sTransactionNumber,
                                             "01",
                                             "",
                                             FixFloatError(fAmount * -1).ToString(),
                                             sPaymentMethod,
                                             sCRDType,
                                             "",
                                             "",
                                             ""
                                         };

            tTHDR.AddRecord(sTHdrRecord);
            tTHDR.AddRecord(sSecondTHdrRecord);

            int nSRefFieldNum = 0;
            if (!tRepData.SearchForRecord("GREF", 1, ref nSRefFieldNum))
            {
                string[] sToAdd = {
                                      "CR",
                                      "GREF",
                                      "0",
                                      "0",
                                      ""
                                  };
                tRepData.AddRecord(sToAdd);
                tRepData.SearchForRecord("GREF", 1, ref nSRefFieldNum);
            }

            string[] sCurrentContents = tRepData.GetRecordFrom(nSRefFieldNum);
            int nCurrentQty = Convert.ToInt32(sCurrentContents[2]);
            nCurrentQty += 1;
            float fCurrentAmount = FixFloatError((float)Convert.ToDecimal(sCurrentContents[3]));
            float fNewAmount = FixFloatError(fCurrentAmount - fAmount);
            tRepData.EditRecordData(nSRefFieldNum, 2, nCurrentQty.ToString());
            tRepData.EditRecordData(nSRefFieldNum, 3, fNewAmount.ToString());

            // Edit NOTRAN in RepData

            nNOTRANLoc = 0;
            tRepData.SearchForRecord("NOTRAN", 1, ref nNOTRANLoc);
            int fTranCount = Convert.ToInt32(tRepData.GetRecordFrom("NOTRAN", 1)[2]);
            fTranCount += 1;
            float fTranValue = (float)Convert.ToDecimal(tRepData.GetRecordFrom("NOTRAN", 1)[3]);
            fTranValue += FixFloatError(fAmount * -1);
            fTranValue = FixFloatError(fTranValue);
            tRepData.EditRecordData(nNOTRANLoc, 2, fTranCount.ToString());
            tRepData.EditRecordData(nNOTRANLoc, 3, fTranValue.ToString());

            tRepData.SaveToFile(GTill.Properties.Settings.Default.sRepDataLoc);
            tTHDR.SaveToFile(GTill.Properties.Settings.Default.sTHdrLoc);
            tTData.SaveToFile(GTill.Properties.Settings.Default.sTDataLoc);

            PaymentMethod pmPayMethod = new PaymentMethod();
            if (sPaymentMethod == "CHRG")
                sPaymentMethod += "," + sCRDType;
            pmPayMethod.SetPaymentMethod(sPaymentMethod, 0.0f, fAmount);
            PrintGeneralRefund(pmPayMethod);
            OpenTillDrawer(false);

        }

        /// <summary>
        /// Gets information about the charge to account for a specific transaction
        /// </summary>
        /// <param name="sTransactionNumber"></param>
        /// <returns>A comma seperated array in the format CHRG,AccountCode,TransactionNumber</returns>
        private string GetChargeToAccountTransactionInfo(string sTransactionNumber)
        {
            string[] sRecordData = tRepData.GetRecordFrom(sTransactionNumber, 1);
            string sAccountCode = "";
            for (int i = 0; i < 6; i++)
                sAccountCode += sRecordData[1][i].ToString();
            string sToReturn = "CHRG," + sAccountCode + "," + sTransactionNumber;
            return sToReturn;
        }

        // For the received on account form

        /// <summary>
        /// Receives money on an account
        /// </summary>
        /// <param name="sAccountCode">The code of the account to receive money on</param>
        /// <param name="fAmountToReceive">The amount to receive</param>
        /// <param name="sPaymentMethod">The payment method code to receive the payment on</param>
        public void ReceiveMoneyOnAccount(string sAccountCode, float fAmountToReceive, string sPaymentMethod)
        {
            // Sort out RepData first

            string sTransactionNumber = GetNextTransactionNumber().ToString();
            while (sTransactionNumber.Length < 6)
                sTransactionNumber = "0" + sTransactionNumber;
            while (sAccountCode.Length < 6)
                sAccountCode = sAccountCode + " ";
            string sCombinedRepCode = sAccountCode + sTransactionNumber;
            string[] sToAdd = {"RA",
                                sCombinedRepCode,
                                "1",
                                fAmountToReceive.ToString(),
                                "",
                              };
            tRepData.AddRecord(sToAdd);
            int nPaymentRecordPos = 0;
            if (!tRepData.SearchForRecord(sPaymentMethod, 1, ref nPaymentRecordPos))
            {
                string[] sPaymentMethodToAdd = { "CR",
                                                   sPaymentMethod,
                                                   "0",
                                                   "0",
                                                   ""
                                               };
                tRepData.AddRecord(sPaymentMethodToAdd);
                tRepData.SearchForRecord(sPaymentMethod, 1, ref nPaymentRecordPos);
            }
            string[] sCurrentPaymentData = tRepData.GetRecordFrom(nPaymentRecordPos);
            float fCurrentAmount = (float)Convert.ToDecimal(sCurrentPaymentData[3]);
            fCurrentAmount = FixFloatError(fCurrentAmount + fAmountToReceive);
            int nCurrentQty = Convert.ToInt32(sCurrentPaymentData[2]);
            nCurrentQty++;
            tRepData.EditRecordData(nPaymentRecordPos, 2, nCurrentQty.ToString());
            tRepData.EditRecordData(nPaymentRecordPos, 3, fAmountToReceive.ToString());

            if (!tRepData.SearchForRecord("RONA", 1, ref nPaymentRecordPos))
            {
                string[] sPaymentMethodToAdd = { "CR",
                                                   "RONA",
                                                   "0",
                                                   "0",
                                                   ""
                                               };
                tRepData.AddRecord(sPaymentMethodToAdd);
                tRepData.SearchForRecord("RONA", 1, ref nPaymentRecordPos);
            }
            sCurrentPaymentData = tRepData.GetRecordFrom(nPaymentRecordPos);
            fCurrentAmount = (float)Convert.ToDecimal(sCurrentPaymentData[3]);
            fCurrentAmount = FixFloatError(fCurrentAmount + fAmountToReceive);
            nCurrentQty = Convert.ToInt32(sCurrentPaymentData[2]);
            nCurrentQty++;
            tRepData.EditRecordData(nPaymentRecordPos, 2, nCurrentQty.ToString());
            tRepData.EditRecordData(nPaymentRecordPos, 3, fAmountToReceive.ToString());

            if (!tRepData.SearchForRecord("VAT01", 1, ref nPaymentRecordPos))
            {
                string[] sPaymentMethodToAdd = { "CR",
                                                   "VAT01",
                                                   "0",
                                                   "0",
                                                   ""
                                               };
                tRepData.AddRecord(sPaymentMethodToAdd);
                tRepData.SearchForRecord("VAT01", 1, ref nPaymentRecordPos);
            }
            sCurrentPaymentData = tRepData.GetRecordFrom(nPaymentRecordPos);
            fCurrentAmount = (float)Convert.ToDecimal(sCurrentPaymentData[3]);
            fCurrentAmount = FixFloatError(fCurrentAmount + fAmountToReceive);
            nCurrentQty = Convert.ToInt32(sCurrentPaymentData[2]);
            nCurrentQty++;
            tRepData.EditRecordData(nPaymentRecordPos, 2, nCurrentQty.ToString());
            tRepData.EditRecordData(nPaymentRecordPos, 3, fCurrentAmount.ToString());

            int nNOTRANLoc = 0;
            tRepData.SearchForRecord("NOTRAN", 1, ref nNOTRANLoc);
            int fTranCount = Convert.ToInt32(tRepData.GetRecordFrom("NOTRAN", 1)[2]);
            fTranCount += 1;
            float fTranValue = (float)Convert.ToDecimal(tRepData.GetRecordFrom("NOTRAN", 1)[3]);
            fTranValue += fAmountToReceive;
            fTranValue = TillEngine.FixFloatError(fTranValue);
            tRepData.EditRecordData(nNOTRANLoc, 2, fTranCount.ToString());
            tRepData.EditRecordData(nNOTRANLoc, 3, fTranValue.ToString());

            tRepData.SearchForRecord("END", 1, ref nNOTRANLoc);
            float fEnd = (float)Convert.ToDecimal(tRepData.GetRecordFrom("END", 1)[3]);
            fEnd += 0.01f;
            int nTransactionNumber = Convert.ToInt32(fEnd * 100.0f);
            tRepData.EditRecordData(nNOTRANLoc, 3, fEnd.ToString());

            // End of RepData

            // Start of THdr

            string[] sLine00 = { sTransactionNumber, "00", "0", fAmountToReceive.ToString(), "RONA", sNewerDateTimeString(), "", "", "" };
            string[] sLine01 = { sTransactionNumber, "01", "", fAmountToReceive.ToString(), sPaymentMethod, "", "", "", "" };
            string[] sLine02 = { sTransactionNumber, "02", "", "0", "ACNT", sAccountCode, "", "", "" };

            tTHDR.AddRecord(sLine00);
            tTHDR.AddRecord(sLine01);
            tTHDR.AddRecord(sLine02);

            tRepData.SaveToFile(GTill.Properties.Settings.Default.sRepDataLoc);
            tTHDR.SaveToFile(GTill.Properties.Settings.Default.sTHdrLoc);
        }

        /// <summary>
        /// Sets the date in REPDATA
        /// </summary>
        /// <param name="sDateTime">The date and time in dd/mm/yyyy format</param>
        public void SetDateInRepData(string sDateTime)
        {
            string[] sDate = sDateTime.Split('/');
            string sNewDate = sDate[0] + "/" + sDate[1] + "/" + sDate[2][2].ToString() + sDate[2][3].ToString();
            tRepData.EditRecordData(0, 1, sNewDate);
            tRepData.EditRecordData(0, 2, "0");
            tRepData.SaveToFile(GTill.Properties.Settings.Default.sRepDataLoc);
        }

        /// <summary>
        /// Gets whether or not REPDATA needs the date setting
        /// </summary>
        /// <returns>True if the date is needed</returns>
        public bool RepdataNeedsDate()
        {
            if (Convert.ToInt32(tRepData.GetRecordFrom(0)[2]) == 4)
                return true;
            else
                return false;
        }
        
        /// <summary>
        /// Adds or amends items in a database (mainly used for adding new items to STOCK and SSTCK
        /// </summary>
        /// <param name="tToUpdate">The table to update</param>
        /// <param name="tSource">The soruce table</param>
        /// <param name="nIndexFieldNumber">The index field</param>
        private void AddOrAmend(ref Table tToUpdate, Table tSource, int nIndexFieldNumber)
        {
            for (int i = 0; i < tSource.NumberOfRecords; i++)
            {
                string[] sNewData = tSource.GetRecordFrom(i);
                int nRecordNum = 0;
                if (tToUpdate.SearchForRecord(sNewData[nIndexFieldNumber], nIndexFieldNumber, ref nRecordNum))
                {
                    tToUpdate.DeleteRecord(nRecordNum);
                }
                tToUpdate.AddRecord(sNewData);
            }
        }

        /// <summary>
        /// Adds or amends items in a database (mainly used for adding new items to STOCK and SSTCK) but misses given fields
        /// </summary>
        /// <param name="tToUpdate">The table to update</param>
        /// <param name="tSource">The soruce table</param>
        /// <param name="nIndexFieldNumber">The index field</param>
        private void AddOrAmend(ref Table tToUpdate, Table tSource, int nIndexFieldNumber, int[] nFieldsToMiss)
        {
            for (int i = 0; i < tSource.NumberOfRecords; i++)
            {
                string[] sNewData = tSource.GetRecordFrom(i);
                string[] sOldData = new string[0];
                int nRecordNum = 0;
                if (tToUpdate.SearchForRecord(sNewData[nIndexFieldNumber], nIndexFieldNumber, ref nRecordNum))
                {
                    sOldData = tToUpdate.GetRecordFrom(nRecordNum);
                    tToUpdate.DeleteRecord(nRecordNum);
                }
                tToUpdate.AddRecord(sNewData);
                if (sOldData.Length > 0)
                {
                    for (int z = 0; z < nFieldsToMiss.Length; z++)
                    {
                        tToUpdate.EditRecordData(nRecordNum, nFieldsToMiss[z], sOldData[nFieldsToMiss[z]]);
                    }
                }

            }
        }

        /// <summary>
        /// Checks for files to process
        /// </summary>
        /// <returns></returns>
        public bool AnyFilesToBeProcessed()
        {
            if (Directory.Exists(GTill.Properties.Settings.Default.sINGNGDir))
            {
                if (Directory.GetFiles(GTill.Properties.Settings.Default.sINGNGDir).Length > 0)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        /// <summary>
        /// Processes files from the backoffice computer
        /// </summary>
        /// <returns>True if the software needs to restart</returns>
        public bool ProcessFilesInINGNG()
        {
            string sINGNGDir = GTill.Properties.Settings.Default.sINGNGDir;
            string[] sFiles = Directory.GetFiles(sINGNGDir);
            bool bNeedToReboot = false;
            for (int i = 0; i < sFiles.Length; i++)
            {
                sFiles[i] = sFiles[i].Split('\\')[sFiles[i].Split('\\').Length - 1];
                if (File.ReadAllBytes(sINGNGDir + "\\" + sFiles[i]).Length != 0)
                {

                    bool bSorted = false;
                    if (sFiles[i].ToUpper().EndsWith(".FLG"))
                        bSorted = true;
                    switch (sFiles[i].ToUpper())
                    {
                        case "STKLEVEL.DBF":
                            Table tTemp = new Table(sINGNGDir + "\\STKLEVEL.DBF");
                            AddOrAmend(ref tStkLevel, tTemp, 0);
                            tStkLevel.SaveToFile(GTill.Properties.Settings.Default.sStkLevelLoc);
                            bSorted = true;
                            break;
                        case "STOCK.DBF":
                            Table tStockTemp = new Table(sINGNGDir + "\\STOCK.DBF");
                            AddOrAmend(ref tStock, tStockTemp, 0);
                            tStock.SaveToFile(GTill.Properties.Settings.Default.sStockLoc);
                            bSorted = true;
                            break;
                        case "OFFERS.DBF":
                            Table tOffersTemp = new Table(sINGNGDir + "\\OFFERS.DBF");
                            // Doesn't update records 4 and 5 because that would lose the number printed/returned for today
                            int[] nToMiss = {4, 5};
                            AddOrAmend(ref tOffers, tOffersTemp, 0, nToMiss);
                            tOffers.SaveToFile("OFFERS.DBF");
                            bSorted = true;
                            break;
                        case "ACCSTAT.DBF":
                            Table tTempAcc = new Table(sINGNGDir + "\\ACCSTAT.DBF");
                            AddOrAmend(ref tAccStat, tTempAcc, 0);
                            tAccStat.SaveToFile(GTill.Properties.Settings.Default.sAccStatLoc);
                            bSorted = true;
                            break;
                        case "DETAILS.DBF":
                            File.Copy(sINGNGDir + "\\DETAILS.DBF", GTill.Properties.Settings.Default.sDetailsLoc, true);
                            tDetails = new Table(GTill.Properties.Settings.Default.sDetailsLoc);
                            bSorted = true;
                            bNeedToReboot = true;
                            break;
                        case "STAFF.DBF":
                            File.Copy(sINGNGDir + "\\STAFF.DBF", GTill.Properties.Settings.Default.sStaffLoc, true);
                            tStaff = new Table(GTill.Properties.Settings.Default.sStaffLoc);
                            bSorted = true;
                            bNeedToReboot = true;
                            break;
                        case "TILLCAT.DBF":
                            File.Copy(sINGNGDir + "\\TILLCAT.DBF", GTill.Properties.Settings.Default.sTillCatLoc, true);
                            tTillCat = new Table(GTill.Properties.Settings.Default.sTillCatLoc);
                            bSorted = true;
                            break; // TILL CAT IS ONLY LOADED WHEN FRMSEARCHFORITEM IS LOADED, TODO : CHANGE THIS
                        case "COMMISSI.DBF":
                            Table tTempComm = new Table(sINGNGDir + "\\COMMISSI.DBF");
                            AddOrAmend(ref tCommission, tTempComm, 0);
                            tTempComm.SaveToFile("COMMISSI.DBF");
                            bSorted = true;
                            break;
                        case "VAT.DBF":
                            File.Copy(sINGNGDir + "\\VAT.DBF", "VAT.DBF", true);
                            tVAT = new Table("VAT.DBF");
                            bSorted = true;
                            bNeedToReboot = true;
                            break;
                        case "MULTIDAT.DBF":
                            File.Copy(sINGNGDir + "\\MULTIDAT.DBF", "MULTIDAT.DBF", true);
                            tMultiData = new Table("MULTIDAT.DBF");
                            bSorted = true;
                            break;
                        case "MULTIHDR.DBF":
                            File.Copy(sINGNGDir + "\\MULTIHDR.DBF", "MULTIHDR.DBF", true);
                            tMultiHeader = new Table("MULTIHDR.DBF");
                            bSorted = true;
                            break;
                    }
                    if (bSorted)
                        File.Delete(sINGNGDir + "\\" + sFiles[i]);
                    else
                    {
                        GTill.ErrorHandler.LogError("I don't know what to do with " + sFiles[i] + " in the INGNG directory. Press continue to ignore.");
                        throw new NotImplementedException("I don't know what to do with " + sFiles[i] + " in the INGNG directory. Press continue to ignore.");
                    }
                }
            }
            sFiles = Directory.GetFiles(sINGNGDir);
            if (sFiles.Length != 0)
            {
                for (int z = 0; z < sFiles.Length; z++)
                {
                    Directory.CreateDirectory("Quarantine");
                    File.Copy(sFiles[z], "Quarantine\\" + sFiles[z].Split('\\')[sFiles[z].Split('\\').Length - 1], true);
                    File.Delete(sFiles[z]);
                }
            }
            if (Directory.Exists(sINGNGDir + "\\OffersReceipt"))
            {
                sFiles = Directory.GetFiles(sINGNGDir + "\\OffersReceipt");
                if (!Directory.Exists("OffersReceipt"))
                {
                    Directory.CreateDirectory("OffersReceipt");
                }
                for (int i = 0; i < sFiles.Length; i++)
                {
                    File.Copy(sFiles[i], "OffersReceipt\\" + sFiles[i].Split('\\')[sFiles[i].Split('\\').Length - 1], true);
                    File.Delete(sFiles[i]);
                }
            }
            return bNeedToReboot;
        }

        /// <summary>
        /// Reprints the receipt of the last transaction if possible
        /// </summary>
        public void ReprintLastReceipt()
        {
            string[] sTransactionNumbers = GetListOfTransactionNumbers();
            if (sTransactionNumbers.Length == 0)
            {
                string[] sToSend = { "", CentralisePrinterText("No transaction to reprint."), "", "", CentralisePrinterText("Till re-written by Thomas Wormald.") };
                SendLinesToPrinter(sToSend);
                PrintReceiptFooter("Thomas", DateTimeForReceiptFooter(), "");
                PrintReceiptHeader();
                EmptyPrinterBuffer();
            }
            else
            {
                ReprintTransactionReceipt(Convert.ToInt32(sTransactionNumbers[sTransactionNumbers.Length - 1]));
            }
        }

        /// <summary>
        /// Gets the day number of today from repdata
        /// </summary>
        /// <returns>Today's day number 1 = Sunday, 2 = Monday etc</returns>
        public int GetDayNumberFromRepData()
        {
            string sDateToday = tRepData.GetRecordFrom(0)[1].TrimEnd('\0');
            return DayNumber(sDateToday);
        }

        /// <summary>
        /// Collects files from the MS-DOS drive
        /// </summary>
        public void CollectFilesFromDosDrive()
        {
            if (GTill.Properties.Settings.Default.bUsingDosInsteadOfFloppy)
            {
                string sCashupLoc = GTill.Properties.Settings.Default.sFloppyLocation;
                string[] sFilesInLoc;
                if (Directory.Exists(sCashupLoc + "\\INGNG"))
                {
                    sFilesInLoc = Directory.GetFiles(sCashupLoc + "\\INGNG");
                    for (int i = 0; i < sFilesInLoc.Length; i++)
                    {
                        File.Copy(sFilesInLoc[i], GTill.Properties.Settings.Default.sINGNGDir + "\\" + sFilesInLoc[i].Split('\\')[sFilesInLoc[i].Split('\\').Length - 1], true);
                        File.Delete(sFilesInLoc[i]);
                    }
                }
                else
                {
                    GTill.ErrorHandler.LogError("The DOS partition could not be read from, so the files from INGNG can't be copied. To disable reading from DOS partition set bUsingDosInsteadOfFloppy to false");
                    throw new NotSupportedException("The DOS partition could not be read from, so the files from INGNG can't be copied. To disable reading from DOS partition set bUsingDosInsteadOfFloppy to false");
                }
            }
            if (GTill.Properties.Settings.Default.bAutoSwapBootFiles)
            {
                /// boot1.ini = Windows XP as default
                /// boot2.ini = MS-DOS as default
                /// boot.ini is the one that is read by the bootloader
                try
                {
                    File.SetAttributes("C:\\BOOT.INI", ~(FileAttributes.ReadOnly | FileAttributes.System | FileAttributes.Hidden));
                    File.SetAttributes("C:\\BOOT1.INI", ~(FileAttributes.ReadOnly | FileAttributes.System | FileAttributes.Hidden));
                    File.Copy("C:\\BOOT1.INI", "C:\\BOOT.INI", true);
                    File.SetAttributes("C:\\BOOT.INI", (FileAttributes.ReadOnly | FileAttributes.System | FileAttributes.Hidden));
                    File.SetAttributes("C:\\BOOT1.INI", (FileAttributes.ReadOnly | FileAttributes.System | FileAttributes.Hidden));
                }
                catch
                {
                    GTill.ErrorHandler.LogError("Could not swap boot.ini files around. To disable this, set bAutoSwapBootFiles to false");
                    throw new NotSupportedException("Could not swap boot.ini files around. To disable this, set bAutoSwapBootFiles to false");
                }
            }
        }

        /// <summary>
        /// Gets the category description from its code
        /// </summary>
        /// <param name="sCategoryCode">The code of the category to get the description for</param>
        /// <returns></returns>
        public string GetCategoryDescription(string sCategoryCode)
        {
            sCategoryCode = sCategoryCode.TrimEnd(' ');
            string[] sResults = tTillCat.GetRecordFrom(sCategoryCode, 0, true);
            return sResults[1];
        }

        /// <summary>
        /// Gets the codes of categories below the current category
        /// </summary>
        /// <param name="sSelectedCategoryCode">The code of the category to search below</param>
        /// <returns></returns>
        public string[] GetCategoryCodesBelowCurrent(string sSelectedCategoryCode)
        {
            string[] sPotentials = tTillCat.SearchAndGetAllMatchingRecords(0, sSelectedCategoryCode);
            for (int i = 0; i < sPotentials.Length; i++)
            {
                if (sPotentials[i].TrimEnd(' ').Length != sSelectedCategoryCode.TrimEnd(' ').Length + 2)
                    sPotentials[i] = null;
                else if (!sPotentials[i].StartsWith(sSelectedCategoryCode.TrimEnd(' ')))
                    sPotentials[i] = null;
            }
            sPotentials = sShortenStringArray(sPotentials);
            return sPotentials;
        }

        /// <summary>
        /// Removes null elements from a string array
        /// </summary>
        /// <param name="sOriginal">The string array from which to remove elements</param>
        /// <returns></returns>
        private string[] sShortenStringArray(string[] sOriginal)
        {
            int nNullCount = 0;
            for (int i = 0; i < sOriginal.Length; i++)
            {
                if (sOriginal[i] == null)
                    nNullCount++;
            }

            int nDiff = 0;
            string[] sToReturn = new string[sOriginal.Length - nNullCount];

            for (int i = 0; i < sOriginal.Length; i++)
            {
                if (sOriginal[i] == null)
                    nDiff++;
                else
                    sToReturn[i - nDiff] = sOriginal[i];
            }
            return sToReturn;
        }

        public string[] GetItemRecordContents(string sBarcode)
        {
            return tStock.GetRecordFrom(sBarcode, 0, true);
        }

        public string[] CheckIfItemHasChildren(string sBarcode)
        {
            int nOfRecs = 0;
            string[,] sChildren = tStock.SearchAndGetAllMatchingRecords(7, sBarcode, ref nOfRecs, true);
            string[] stoReturn = new string[nOfRecs];
            for (int i = 0; i < nOfRecs; i++)
            {
                stoReturn[i] = sChildren[i, 0].TrimEnd(' ');
            }
            return stoReturn;
        }

        public string[] GetCodesOfItemsInCategory(string sCategory)
        {
            int nOfResults = 0;
            string[,] sSearch = tStock.SearchAndGetAllMatchingRecords(9, sCategory, ref nOfResults);
            int nMissed = 0;
            int nToMiss = 0;
            bool[] bMissOut = new bool[nOfResults];
            for (int i = 0; i < bMissOut.Length; i++)
                bMissOut[i] = false;
            for (int i = 0; i < nOfResults; i++)
            {
                // Trying to cut out on items showing up that aren't in this category. ie AP shows items from CRAP
                if (sSearch[i, 9].TrimEnd(' ') != sCategory.TrimEnd(' '))
                {
                    bMissOut[i] = true;
                    nToMiss++;
                }
            }
            string[] sResults = new string[nOfResults - nToMiss];
            for (int i = 0; i < nOfResults; i++)
            {
                if (bMissOut[i] == false)
                {
                    sResults[i - nMissed] = sSearch[i, 0].TrimEnd(' ');
                }
                else
                    nMissed++;
            }
            return sResults;
        }

        /// <summary>
        /// Gets the category description from its code
        /// </summary>
        /// <param name="sCategoryCode">The code of the category to get the description for</param>
        /// <returns></returns>
        public string sGetCategoryDescription(string sCategoryCode)
        {
            sCategoryCode = sCategoryCode.TrimEnd(' ');
            string[] sResults = tTillCat.GetRecordFrom(sCategoryCode, 0, true);
            return sResults[1];
        }

        bool bDemoMode = false;
        public bool DemoMode
        {
            get
            {
                return bDemoMode;
            }
            set
            {
                bDemoMode = value;
            }
        }

        public void PrintStockLevelsOfCategory(string sCatCode)
        {
            SendLineToPrinter("Code         Description                 QIS");
            for (int i = 0; i < tStock.NumberOfRecords; i++)
            {
                if (tStock.GetRecordFrom(i)[9].StartsWith(sCatCode))
                {
                    string sCode = tStock.GetRecordFrom(i)[0];
                    string sDesc = "";
                    for (int x = 0; x < 25 && x < tStock.GetRecordFrom(i)[1].Length; x++)
                    {
                        sDesc = tStock.GetRecordFrom(i)[1][tStock.GetRecordFrom(i)[1].Length - x - 1] + sDesc;
                    }
                    string sStockLevel = tStkLevel.GetRecordFrom(tStock.GetRecordFrom(i)[0], 0)[2];
                    SendLineToPrinter(RightAlignStringOnExistingString(sCode + sDesc, sStockLevel));
                }
            }
            EmptyPrinterBuffer();
        }

        public void AddOrderSuggestion(string sBarcode)
        {
            bool bFound = false;
            for (int i = 0; i < tOrderSug.NumberOfRecords; i++)
            {
                if (tOrderSug.GetRecordFrom(i)[0].ToUpper() == sBarcode.ToUpper())
                {
                    bFound = true;
                    break;
                }
            }
            if (!bFound)
            {
                string sDay = DateTime.Now.Day.ToString();
                while (sDay.Length < 2)
                    sDay = "0" + sDay;
                string sMonth = DateTime.Now.Month.ToString();
                while (sMonth.Length < 2)
                    sMonth = "0" + sMonth;
                string sYear = DateTime.Now.Year.ToString()[2].ToString() + DateTime.Now.Year.ToString()[3].ToString();
                string sDate = sDay + sMonth + sYear;
                string[] sToAdd = { sBarcode, sDate };
                tOrderSug.AddRecord(sToAdd);
                tOrderSug.SaveToFile("TORDERSU.DBF");
            }
        }

        public void ProcessCommands()
        {
            if (File.Exists("COMMANDS.TXT"))
            {
                TextReader tReader = new StreamReader("COMMANDS.TXT");
                string[] sLines = tReader.ReadToEnd().Split('\n');
                tReader.Close();
                for (int i = 0; i < sLines.Length; i++)
                {
                    switch (sLines[i].TrimEnd('\r'))
                    {
                        case "ShutDown":
                            GTill.ShutdownCode.ShutdownComputer();
                            break;
                        case "Restart":
                            GTill.ShutdownCode.RebootComputer();
                            break;
                        case "OpenTillDrawer":
                            OpenTillDrawer(false);
                            break;
                        case "UpdateSoftware":
                            System.Diagnostics.Process.Start("GTillUpdater.exe");
                            System.Windows.Forms.Application.ExitThread();
                            break;
                        case "DumpTransaction":
                            DumpTransactionToFile();
                            break;
                        case "TempCloseSoftware":
                            System.Diagnostics.Process.Start("GTillTempCloser.exe");
                            System.Windows.Forms.Application.ExitThread();
                            break;

                    }
                }
                File.Delete("COMMANDS.TXT");
            }
        }

        public void DumpTransactionToFile()
        {
            TextWriter tWriter = new StreamWriter("output.txt", false);
            bool bFound = false;
            int i = 0;
            for (i = 0; i < tStoredTransaction.Length; i++)
            {
                if (tStoredTransaction[i].UserNumber == 0)
                {
                    bFound = true;
                    break;
                }
            }
            if (bFound)
            {
                Transaction t = tStoredTransaction[i].SavedTranaction;
                for (int x = 0; x < t.NumberOfItems; x++)
                {
                    Item it = t.GetItemInTransaction(x);
                    tWriter.WriteLine(it.Barcode + "," + it.Quantity + "," + it.Amount);
                }
                tWriter.Close();
            }
            TextWriter tt = new StreamWriter("dumped", false);
            tt.Close();
        }

        public void AddEmail(string sTitle, string sForename, string sSurName, string sEmailAddress, string sAddress, string sItem)
        {
            string sDay = DateTime.Now.Day.ToString();
            while (sDay.Length < 2)
                sDay = "0" + sDay;
            string sMonth = DateTime.Now.Month.ToString();
            while (sMonth.Length < 2)
                sMonth = "0" + sMonth;
            string sYear = DateTime.Now.Year.ToString()[2].ToString() + DateTime.Now.Year.ToString()[3].ToString();
            string sDate = sDay + sMonth + sYear;
            string[] sToAdd = { sTitle, sForename, sSurName, sEmailAddress, sDate, sAddress, sItem };
            tEmails.AddRecord(sToAdd);
            tEmails.SaveToFile("EMAILS.DBF");
        }

        public int DiscountPercentageThreshold
        {
            get
            {
                if (tDetails.GetRecordFrom(8)[0] != "N/A")
                {
                    return Convert.ToInt32(tDetails.GetRecordFrom(8)[0]);
                }
                else
                {
                    return 0;
                }
            }
        }

        public string[] GetListOfOfferCodes()
        {
            string[] sToReturn = new string[tOffers.NumberOfRecords];
            for (int i = 0; i < sToReturn.Length; i++)
            {
                sToReturn[i] = tOffers.GetRecordFrom(i)[0];
            }
            return sToReturn;
        }

        public bool OfferExists(string sCode)
        {
            int n = 0;
            return tOffers.SearchForRecord(sCode, 0, ref n);
        }

        public string GetOfferDesc(string sOfferCode)
        {
            int nRecNum = -1;
            tOffers.SearchForRecord(sOfferCode, 0, ref nRecNum);
            if (nRecNum != -1)
            {
                return tOffers.GetRecordFrom(nRecNum)[1];
            }
            else
            {
                return "Offer Code Not Found!";
            }
        }

        public void RecordOfferReturned(string sOfferCode)
        {
            int nRec = -1;
            tOffers.SearchForRecord(sOfferCode, 0, ref nRec);
            if (nRec != -1)
            {
                int nCount = Convert.ToInt32(tOffers.GetRecordFrom(nRec)[5]);
                nCount++;
                tOffers.EditRecordData(nRec, 5, nCount.ToString());
                tOffers.SaveToFile("OFFERS.DBF");
            }
        }

    }
}

