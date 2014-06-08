using System;
using DBFDetailsViewerV2;
using System.IO;

namespace TillEngine
{
    public class Item
    {
        string sItemDesc;
        string sBarcode;
        string sParentBarcode;
        float fGrossAmnt;
        float fFinalAmount;
        float fVATAmnt;
        string sVATCategory;
        bool bStockItem;
        int nCategory;
        int nQuantity;
        string sDepartment;
        string sCategory;
        bool bDiscontinued;
        bool bItemExists;
        string sVATCode;
        int nCurrentStockLevel;
        bool bDiscounted;
        float fExcessPaid = 0.0f;

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
                sDepartment = sRecordContents[2];
                nCategory = Convert.ToInt32(sRecordContents[3]);
                fGrossAmnt = (float)Convert.ToDecimal(sRecordContents[4]);
                fGrossAmnt = (float)Math.Round((decimal)fGrossAmnt, 2);
                fFinalAmount = fGrossAmnt;
                sVATCategory = sRecordContents[6];
                sCategory = sRecordContents[9];
                if (nCategory == 1)
                    bStockItem = true;
                else
                    bStockItem = false;
                nQuantity = 0;
                sParentBarcode = sRecordContents[7];
                if (sRecordContents[9].StartsWith("ZZ"))
                    bDiscontinued = true;
                else
                    bDiscontinued = false;

                bItemExists = true;
                bDiscounted = false;
            }


        }

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
                sDepartment = sRecordContents[2];
                nCategory = Convert.ToInt32(sRecordContents[3]);
                fGrossAmnt = (float)Convert.ToDecimal(sRecordContents[4]);
                fGrossAmnt = (float)Math.Round((decimal)fGrossAmnt, 2);
                fFinalAmount = fGrossAmnt;
                sVATCategory = sRecordContents[6];
                sCategory = sRecordContents[9];
                if (fGrossAmnt == 0.0f)
                    bStockItem = false;
                nQuantity = 0;
                sParentBarcode = sRecordContents[7];
                if (sRecordContents[9].StartsWith("ZZ"))
                    bDiscontinued = true;
                else
                    bDiscontinued = false;

                bItemExists = true;
                bDiscounted = false;

                /* TODO:
                 * Get the VAT Amount
                 */

                nCurrentStockLevel = Convert.ToInt32((float)Convert.ToDecimal(sSTKLevelContents[2]));

            }
        }

        public int GetQuantity()
        {
            return nQuantity;
        }

        public void SetQuantity(int nAmount)
        {
            nQuantity = nAmount;
        }

        public float GetAmount()
        {
            fFinalAmount = TillEngine.FixFloatError(fFinalAmount);
            return fFinalAmount;
        }

        public float GetGrossAmount()
        {
            fGrossAmnt = TillEngine.FixFloatError(fGrossAmnt);
            return fGrossAmnt;
        }

        public void SetGrossAmount(float fToSetTo)
        {
            fGrossAmnt = TillEngine.FixFloatError(fToSetTo);
            fFinalAmount = fGrossAmnt;
        }

        public string GetBarcode()
        {
            return sBarcode;
        }

        public bool GetIsItemStock()
        {
            return bStockItem;
        }

        public string GetVATRate()
        {
            return sVATCategory;
        }

        public string GetDescription()
        {
            return sItemDesc;
        }

        public bool GetIsDiscontinued()
        {
            return bDiscontinued;
        }

        public int GetStockLevel()
        {
            if (nCurrentStockLevel == null)
                return 0;
            else
                return nCurrentStockLevel;
        }

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

        public void SetDescription(string sDesc)
        {
            if (nCategory == 4)
                sItemDesc = sDesc;
        }

        public int GetItemCategory()
        {
            return nCategory;
        }

        public void DiscountAmountFromNet(float fAmount)
        {
            fFinalAmount -= fAmount;
            fFinalAmount = TillEngine.FixFloatError(fFinalAmount);
            bDiscounted = true;
        }

        public bool GetIsDiscounted()
        {
            return bDiscounted;
        }

        public void SetStockLevel(int nToSetTo)
        {
            nCurrentStockLevel = nToSetTo;
        }
    }

    public class PaymentMethod
    {
        string sPMName;
        float fAmount;
        float fGross = 0.0f; // Amount paid without change given

        public void SetPaymentMethod(string name, float flAmount)
        {
            sPMName = name;
            fAmount = flAmount;
        }
        public void SetPaymentMethod(string name, float flAmount, float flGross)
        {
            sPMName = name;
            fAmount = flAmount;
            fGross = TillEngine.FixFloatError(flGross + flAmount);
        }

        public float GetAmount()
        {
            return fAmount;
        }

        public float Excess
        {
            get
            {
                return fGross;
            }
        }

        public string GetPMType()
        {
            return sPMName;
        }
    }

    class Transaction
    {
        Item[] iItems;
        PaymentMethod[] pmPayMethod;
        bool bAllPaidFor = false;
        TillSettings currentSettings;
        int nTransactionNumber = 0;
        bool bLastItemAddedSuccessfully;
        float fExcessPaid = 0.0f;
        float fMainVATRate = 0.0f;

        public Transaction(TillSettings ts, int nTransactionNum)
        {
            iItems = new Item[0];
            pmPayMethod = new PaymentMethod[0];
            currentSettings = ts;
            nTransactionNumber = nTransactionNum;
        }

        public PaymentMethod[] PaymentMethods
        {
            get
            {
                return pmPayMethod;
            }
        }

        public int TransactionNumber
        {
            get
            {
                return nTransactionNumber;
            }
        }

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

        public void SetTransactionNumber(int nToSetTo)
        {
            nTransactionNumber = nToSetTo;
        }

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
                iItems[iItems.Length - 1].SetQuantity(1);
                if (iItems[iItems.Length - 1].GetVATRate() == "E1")
                {
                    float fVATRate = fMainVATRate; // Need to add on VAT
                    fVATRate = TillEngine.FixFloatError((fVATRate / 100) + 1.0f);
                    iItems[iItems.Length - 1].SetGrossAmount(TillEngine.FixFloatError(iItems[iItems.Length - 1].GetGrossAmount() * fVATRate));
                }
                bLastItemAddedSuccessfully = true;
            }
            else
                bLastItemAddedSuccessfully = false;
        }
        

        public void AddPaymentMethod(string sName, float fAMNT)
        {
            AddPaymentMethod(sName, fAMNT, 0.0f);
        }

        public void AddPaymentMethod(string sName, float fAMNT, float fExcess)
        {
            float fTotalAmount = 0.0f;
            PaymentMethod[] temp = pmPayMethod;
            pmPayMethod = new PaymentMethod[temp.Length + 1];
            for (int i = 0; i < temp.Length; i++)
            {
                pmPayMethod[i] = temp[i];
                fTotalAmount += pmPayMethod[i].GetAmount();
            }
            pmPayMethod[temp.Length] = new PaymentMethod();
            pmPayMethod[temp.Length].SetPaymentMethod(sName, fAMNT, fExcess);
            fTotalAmount += pmPayMethod[temp.Length].GetAmount();

            float fTotalDue = 0.0f;

            for (int i = 0; i < iItems.Length; i++)
            {
                fTotalDue += iItems[i].GetAmount();
            }
            fTotalAmount = TillEngine.FixFloatError(fTotalAmount);
            fTotalDue = TillEngine.FixFloatError(fTotalDue);

            if (fTotalAmount >= fTotalDue)
            {
                bAllPaidFor = true;
                if (GetStillDue() <= 0.0f)
                {
                    pmPayMethod[temp.Length].SetPaymentMethod(sName, fAMNT + GetStillDue());
                    fExcessPaid = TillEngine.FixFloatError(fAMNT - pmPayMethod[temp.Length].GetAmount());
                }
            }
            else
                bAllPaidFor = false;
        }

        private void BackupDatabases(ref Table repData, ref Table tData, ref Table tHdr)
        {
            repData.SaveToFile("REPDATA_PREV.DBF");
            tData.SaveToFile("TDATA_PREV.DBF");
            tHdr.SaveToFile("THDR_PREV.DBF");
        }

        public void SaveTransaction(ref Table repData, ref Table tData, ref Table tHdr)
        {
            BackupDatabases(ref repData, ref tData, ref tHdr);
            string sChargeToAccountCode = "";
            float fCat01VATPaid = 0.0f;
            // REPDATA.DBF :
            foreach (Item item in iItems)
            {
                int nRecordNum = 0;
                bool recordAlreadyExists = repData.SearchForRecord(item.GetBarcode(), 1, ref nRecordNum);
                if (recordAlreadyExists)
                {
                    string[] currentContents = repData.GetRecordFrom(item.GetBarcode(), 1, true);

                    int nCurrentQTY = Convert.ToInt32(currentContents[2]);
                    int nNewQTY = nCurrentQTY + item.GetQuantity();

                    float fCurrentAmount = (float)Convert.ToDecimal(currentContents[3]);
                    fCurrentAmount += item.GetAmount();

                    float fGrossAmount = (float)Convert.ToDecimal(currentContents[4]);

                    currentContents[2] = nNewQTY.ToString();
                    currentContents[3] = fCurrentAmount.ToString();
                    currentContents[4] = TillEngine.FixFloatError(fGrossAmount + (item.GetGrossAmount() * item.GetQuantity())).ToString();

                    for (int i = 2; i < 5; i++)
                    {
                        repData.EditRecordData(nRecordNum, i, currentContents[i]);
                    }
                }
                else
                {
                    string[] toAdd = new string[5];
                    toAdd[0] = "ST";
                    toAdd[1] = item.GetBarcode();
                    toAdd[2] = item.GetQuantity().ToString();
                    toAdd[3] = item.GetAmount().ToString();
                    toAdd[4] = (item.GetGrossAmount() * item.GetQuantity()).ToString();

                    repData.AddRecord(toAdd);
                }

                if (item.GetIsItemStock())
                {
                    repData.SearchForRecord("STOCK", 1, ref nRecordNum);
                    string[] currentContents = repData.GetRecordFrom("STOCK", 1);

                    int nCurrentQuantity = Convert.ToInt32(currentContents[2]);
                    nCurrentQuantity += item.GetQuantity();

                    float fCurrentAmnt = (float)Convert.ToDecimal(currentContents[3]);
                    fCurrentAmnt += item.GetAmount();

                    currentContents[2] = nCurrentQuantity.ToString();
                    currentContents[3] = fCurrentAmnt.ToString();

                    repData.EditRecordData(nRecordNum, 2, currentContents[2]);
                    repData.EditRecordData(nRecordNum, 3, currentContents[3]);
                }
                else
                {
                    repData.SearchForRecord("NSTCK", 1, ref nRecordNum);
                    string[] currentContents = repData.GetRecordFrom("NSTCK", 1);

                    int nCurrentQuantity = Convert.ToInt32(currentContents[2]);
                    nCurrentQuantity += item.GetQuantity();

                    float fCurrentAmnt = (float)Convert.ToDecimal(currentContents[3]);
                    fCurrentAmnt += item.GetAmount();

                    currentContents[2] = nCurrentQuantity.ToString();
                    currentContents[3] = fCurrentAmnt.ToString();

                    repData.EditRecordData(nRecordNum, 2, currentContents[2]);
                    repData.EditRecordData(nRecordNum, 3, currentContents[3]);
                }

                int nNITEMPos = 0;
                repData.SearchForRecord("NOITEM", 1, ref nNITEMPos);
                int nOfItem = Convert.ToInt32(repData.GetRecordFrom("NOITEM", 1)[2]);
                nOfItem += item.GetQuantity();
                float fCurrentItemAmount = (float)Convert.ToDecimal(repData.GetRecordFrom("NOITEM", 1)[3]);
                fCurrentItemAmount += item.GetAmount();

                repData.EditRecordData(nNITEMPos, 2, nOfItem.ToString());
                repData.EditRecordData(nNITEMPos, 3, fCurrentItemAmount.ToString());

                int nVATPOS = 0;
                string sVATNum = "VAT";
                if (item.GetVATRate() == "X0")
                {
                    sVATNum += "00";
                }
                else if (item.GetVATRate() == "Z0")
                {
                    sVATNum += "02";
                }
                else if (item.GetVATRate() == "I1" || item.GetVATRate() == "E1")
                {
                    sVATNum += "01";
                    fCat01VATPaid += item.GetAmount();
                    fCat01VATPaid = TillEngine.FixFloatError(fCat01VATPaid);
                }
                if (!repData.SearchForRecord(sVATNum, 1, ref nVATPOS))
                {
                    string[] toAdd = { "CR", sVATNum, "0", "0", "0" };
                    repData.AddRecord(toAdd);
                    repData.SearchForRecord(sVATNum, 1, ref nVATPOS);
                }
                float fCurrentVAT = (float)Convert.ToDecimal(repData.GetRecordFrom(sVATNum, 1)[3]);
                fCurrentVAT += item.GetAmount();
                fCurrentVAT = TillEngine.FixFloatError(fCurrentVAT);
                repData.EditRecordData(nVATPOS, 3, fCurrentVAT.ToString());
            }

            foreach (PaymentMethod paymentMethod in pmPayMethod)
            {
                // Generic REPDATA add:

                int nRecordPos = 0;
                if (paymentMethod.GetPMType().Split(',').Length == 2)
                {
                    sChargeToAccountCode = paymentMethod.GetPMType().Split(',')[1];
                    paymentMethod.SetPaymentMethod(paymentMethod.GetPMType().Split(',')[0], paymentMethod.GetAmount());
                }
                if (!repData.SearchForRecord(paymentMethod.GetPMType().Split(',')[0], 1, ref nRecordPos))
                {
                    string[] sCheque = { "CR", paymentMethod.GetPMType(), "0", "0", "0" };
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
                        repData.SearchForRecord(paymentMethod.GetPMType(), 1, ref nRecordPos);
                    else
                        repData.SearchForRecord("CRCD", 1, ref nRecordPos);
                }

                string[] chequeFields = repData.GetRecordFrom(paymentMethod.GetPMType(), 1);
                if (paymentMethod.GetPMType().StartsWith("CRCD"))
                    chequeFields = repData.GetRecordFrom("CRCD", 1);
                int nQuantity = Convert.ToInt32(chequeFields[2]);
                nQuantity++;
                float fAmount = (float)Convert.ToDecimal(chequeFields[3].TrimEnd('\0'));
                fAmount += paymentMethod.GetAmount();
                fAmount = TillEngine.FixFloatError(fAmount);

                chequeFields[2] = nQuantity.ToString();
                chequeFields[3] = fAmount.ToString();

                repData.EditRecordData(nRecordPos, 2, chequeFields[2]);
                repData.EditRecordData(nRecordPos, 3, chequeFields[3]);

                // End of Generic
                
                if (paymentMethod.GetPMType().StartsWith("CRCD"))
                {
                    int nCRDRecordPos = 0;
                    int nCreditCardType = Convert.ToInt32(paymentMethod.GetPMType()[4].ToString());
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
                    fCRDAmount += paymentMethod.GetAmount();

                    crdFields[2] = nCRDQuantity.ToString();
                    crdFields[3] = fCRDAmount.ToString();

                    repData.EditRecordData(nCRDRecordPos, 2, crdFields[2]);
                    repData.EditRecordData(nCRDRecordPos, 3, crdFields[3]);
                }
                else if (paymentMethod.GetPMType() == "DEPO")
                {
                    int nDEPRecordNum = 0;
                    bool recordDEPAlreadyExists = repData.SearchForRecord("DEP", 1, ref nDEPRecordNum);
                    if (recordDEPAlreadyExists)
                    {
                        string[] currentContents = repData.GetRecordFrom("DEP", 1, true);

                        int nCurrentQTY = Convert.ToInt32(currentContents[2]);
                        int nNewQTY = nCurrentQTY + 1;

                        float fCurrentAmount = (float)Convert.ToDecimal(currentContents[3]);
                        fCurrentAmount -= paymentMethod.GetAmount();

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
                        toAdd[3] = (paymentMethod.GetAmount() * -1).ToString();
                        toAdd[4] = "0";

                        repData.AddRecord(toAdd);
                    }

                    // Remove any applicable VAT that was paid

                    float fAmountOfVATToRemove = paymentMethod.GetAmount();

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

                    /*
                    if (fCurrent <= 0.0f)
                        repData.DeleteRecord(nRecordPosition);
                     */
                
                }
                else if (paymentMethod.GetPMType() == "CHRG")
                {
                    // Remove the VAT that was added!

                    float fToRemove = GetAmountForVATRate("I1") + GetAmountForVATRate("E1");
                    fToRemove = TillEngine.FixFloatError(fToRemove);

                    if (fToRemove > paymentMethod.GetAmount())
                    {
                        fToRemove = paymentMethod.GetAmount();
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

                    string[] sNewRecord = { "CA", "", "1", paymentMethod.GetAmount().ToString(), "0" };
                    string sAccountCodeToRecord = sChargeToAccountCode;
                    while (sAccountCodeToRecord.Length < 6)
                        sAccountCodeToRecord = sAccountCodeToRecord + " ";
                    string sTransNum = nTransactionNumber.ToString();
                    while (sTransNum.Length < 6)
                        sTransNum = "0" + sTransNum;
                    sNewRecord[1] = sAccountCodeToRecord + sTransNum;
                    repData.AddRecord(sNewRecord);
                    paymentMethod.SetPaymentMethod(paymentMethod.GetPMType()+ "," + sChargeToAccountCode, paymentMethod.GetAmount());
                }

            }

            int nNOTRANLoc = 0;
            repData.SearchForRecord("NOTRAN", 1, ref nNOTRANLoc);
            int fTranCount = Convert.ToInt32(repData.GetRecordFrom("NOTRAN", 1)[2]);
            fTranCount += 1;
            float fTranValue = (float)Convert.ToDecimal(repData.GetRecordFrom("NOTRAN", 1)[3]);
            fTranValue += TotalAmount();
            fTranValue = TillEngine.FixFloatError(fTranValue);
            repData.EditRecordData(nNOTRANLoc, 2, fTranCount.ToString());
            repData.EditRecordData(nNOTRANLoc, 3, fTranValue.ToString());

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

                toAdd[3] = iItems[i].GetBarcode().TrimEnd(' ');

                toAdd[4] = iItems[i].GetDescription().TrimEnd(' ');

                toAdd[5] = iItems[i].GetQuantity().ToString();

                toAdd[6] = TillEngine.FixFloatError(iItems[i].GetGrossAmount() * iItems[i].GetQuantity()).ToString();

                if (iItems[i].GetIsDiscounted() && iItems[i].GetItemCategory() == 1)
                    toAdd[7] = "1";
                else
                    toAdd[7] = "0";

                toAdd[8] = TillEngine.FixFloatError((iItems[i].GetGrossAmount() * iItems[i].GetQuantity()) - iItems[i].GetAmount()).ToString();

                string sVATCode = iItems[i].GetVATRate();

                toAdd[9] = sVATCode;

                tData.AddRecord(toAdd);
            }

            tData.SaveToFile(GTill.Properties.Settings.Default.sTDataLoc);

            /*
             * End of TDATA.DBF
             * 
             * Start of THDR.DBF
             */

            string[] nextRec = new string[9];
            nextRec[0] = nTransactionNumber.ToString();
            while (nextRec[0].Length < 6)
                nextRec[0] = "0" + nextRec[0];

            nextRec[1] = "00";
            nextRec[2] = "0";
            nextRec[3] = TotalAmount().ToString();
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
            string sCurrentUser = currentSettings.GetCurrentUserNumber().ToString();
            while (sCurrentUser.Length < 2)
                sCurrentUser = "0" + sCurrentUser;
            string sDescription = DateTime.Now.Year.ToString()[2].ToString() + DateTime.Now.Year.ToString()[3].ToString()
                                    + sMonth + sDay + sHour + sMinute + sCurrentUser;
            nextRec[5] = sDescription;

            nextRec[6] = "";
            nextRec[7] = "";
            nextRec[8] = "";

            tHdr.AddRecord(nextRec);

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
                nextRec[3] = pmPayMethod[i].GetAmount().ToString();
                if (i == pmPayMethod.Length - 1)
                {
                    nextRec[3] = TillEngine.FixFloatError(pmPayMethod[i].GetAmount() + fExcessPaid).ToString();
                }
                string sPaymethod = "", sDesc = "";
                if (pmPayMethod[i].GetPMType().StartsWith("CRCD"))
                {
                    sPaymethod = "CRCD";
                    sDesc = pmPayMethod[i].GetPMType()[4].ToString();
                }
                else if (pmPayMethod[i].GetPMType() == "CASH")
                    sPaymethod = "CASH";
                else if (pmPayMethod[i].GetPMType() == "CHEQ")
                    sPaymethod = "CHEQ";
                else if (pmPayMethod[i].GetPMType() == "VOUC")
                {
                    sPaymethod = "VOUC";
                    sDesc = sDescription;
                }
                else if (pmPayMethod[i].GetPMType() == "DEPO")
                {
                    sPaymethod = "DEPO";
                    sDesc = sDescription;
                }
                else if (pmPayMethod[i].GetPMType().StartsWith("CHRG"))
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

        float TotalAmount()
        {
            float fTotal = 0.0f;
            foreach (Item i in iItems)
                fTotal += i.GetAmount();
            return (float)Math.Round((double)fTotal, 2);
        }

        public int GetNumberOfItemsInTransaction()
        {
            return iItems.Length;
        }

        public int GetTotalNumberOfItemsPurchased()
        {
            // This differs from the above, because it includes multiple quantities
            int nToReturn = 0;
            for (int i = 0; i < iItems.Length; i++)
            {
                nToReturn += iItems[i].GetQuantity();
            }
            return nToReturn;
        }

        public float GetTotalAmount()
        {
            return TotalAmount();
        }

        public Item GetItemInTransaction(int posInItemArray)
        {
            return iItems[posInItemArray];
        }

        public bool GetItemAddStatus()
        {
            return bLastItemAddedSuccessfully;
        }

        public void SetLastItemPrice(float fPrice)
        {
            iItems[iItems.Length - 1].SetPrice(fPrice);
        }

        public void SetLastItemDescription(string sDesc)
        {
            iItems[iItems.Length - 1].SetDescription(sDesc);
        }

        public void SetLastItemQuantity(int nNewQuantity)
        {
            iItems[iItems.Length - 1].SetQuantity(nNewQuantity);
        }

        public int GetLastItemQuantity()
        {
            return iItems[iItems.Length - 1].GetQuantity();
        }

        public float GetLastItemNetPrice()
        {
            return iItems[iItems.Length - 1].GetAmount();
        }

        public string[,] GetPaymentMethods()
        {
            string[,] sToReturn = new string[pmPayMethod.Length + 1, 2];
            sToReturn[0, 0] = pmPayMethod.Length.ToString();
            for (int i = 0; i < pmPayMethod.Length; i++)
            {
                sToReturn[i + 1, 0] = pmPayMethod[i].GetPMType();
                sToReturn[i + 1, 1] = TillEngine.FormatMoneyForDisplay(pmPayMethod[i].GetAmount());
            }
            return sToReturn;
        }

        public float GetStillDue()
        {
            float fTotalAmount = TotalAmount();
            for (int i = 0; i < pmPayMethod.Length; i++)
            {
                fTotalAmount -= pmPayMethod[i].GetAmount();
            }
            fTotalAmount = TillEngine.FixFloatError(fTotalAmount);
            if (fTotalAmount <= 0.0f)
                bAllPaidFor = true;
            else
                bAllPaidFor = false;

            return fTotalAmount;
        }

        public bool HasAllBeenPaidFor()
        {
            return bAllPaidFor;
        }

        public float ChangeDue()
        {
            return fExcessPaid;
        }

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

        public void DiscountAmountFromItem(int nItemNumber, float fAmount)
        {
            iItems[nItemNumber].DiscountAmountFromNet(fAmount);
        }

        public void ClearAllPayments()
        {
            pmPayMethod = new PaymentMethod[0];
            bAllPaidFor = false;
        }

        public void DiscountPercentageFromItem(int nItemNumber, int nPercent)
        {
            float fCurrentItemAmount = iItems[nItemNumber].GetAmount();
            float fPercent = (fCurrentItemAmount / 100) * nPercent;
            fPercent = (float)Math.Round((decimal)fPercent, 2);
            DiscountAmountFromItem(nItemNumber, fPercent);
        }

        public void DiscountAmountFromWholeTransaction(float fAmount)
        {
            float fTargetTotal = TillEngine.FixFloatError(GetTotalAmount() - fAmount);
            float fCurrentTotal = GetTotalAmount();

            for (int i = 0; i < iItems.Length; i++)
            {
                float fCurrentAmount = iItems[i].GetAmount();
                float fAmountToDiscount = (fCurrentAmount / fCurrentTotal) * fAmount;
                fAmountToDiscount = TillEngine.FixFloatError(fAmountToDiscount);
                iItems[i].DiscountAmountFromNet(fAmountToDiscount);
            }

            float fActualTotal = GetTotalAmount();
            if (fActualTotal != fTargetTotal)
            {
                iItems[iItems.Length - 1].DiscountAmountFromNet(TillEngine.FixFloatError(fActualTotal - fTargetTotal));
            }
        }

        public void DiscountPercentageFromWholeTransaction(int nPercent)
        {
            float fCurrentAmount = GetTotalAmount();
            float fAmountToDiscount = (fCurrentAmount / 100) * nPercent;
            fAmountToDiscount = TillEngine.FixFloatError(fAmountToDiscount);

            DiscountAmountFromWholeTransaction(fAmountToDiscount);
        }

        public float GetAmountForVATRate(string sVATRate)
        {
            float fTotal = 0.0f;
            for (int i = 0; i < iItems.Length; i++)
            {
                if (iItems[i].GetVATRate() == sVATRate)
                    fTotal += iItems[i].GetAmount();
            }
            return fTotal;
        }
    }

    class TillSettings
    {
        public string sShopName;
        public string[] sAddress;
        public string sTelNumber;
        public string sVATNumber;
        public string[] sEndOfReceiptMessage;
        public bool bDiscountAllowed;
        public bool bChangeFromCheques;
        public bool bPrintCardNumberOnBackOfCheque;
        public int nMaxMultiplicationAllowed;
        public int nNumberOfCreditCards;
        public string[] sCreditCards;
        public char cCurrencyChar;
        public bool bColourDisplay;
        public float[] fVATRates;
        public int nCurrentUser;
        public string sTopLevelPassword;
        public bool bUsePrinter;
        public string sTillName;
        public string sTillID;
        public string sDeptName;
        public bool bAutoLowercase;

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

            bPrintCardNumberOnBackOfCheque = ConvertSettingsBoolean(sSettings[4]);

            sTopLevelPassword = DecryptPasswords(sSettings[5])[0];

            cCurrencyChar = sSettings[7][0];
            if (cCurrencyChar == (char)156)
                cCurrencyChar = '£';

            bDiscountAllowed = ConvertSettingsBoolean(sSettings[8]);

            bChangeFromCheques = ConvertSettingsBoolean(sSettings[10]);

            sTelNumber = sSettings[12];

            if (sSettings[13][0] == 'C')
                bColourDisplay = true;
            else
                bColourDisplay = false;

            nMaxMultiplicationAllowed = Convert.ToInt32(sSettings[15]);

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

        public float GetVATRate(int VATNum)
        {
            return fVATRates[VATNum];
        }

        public int GetCurrentUserNumber()
        {
            return nCurrentUser;
        }

        public void SetStaffNumber(int nNumber)
        {
            nCurrentUser = nNumber;
        }

        public string GetShopName()
        {
            return sShopName;
        }

        public bool SwitchPrinterStatus()
        {
            bUsePrinter = !bUsePrinter;
            return bUsePrinter;
        }

        public string GetTillName()
        {
            return sTillName;
        }

        public char GetCurrenySymbol()
        {
            return cCurrencyChar;
        }

        public string[] GetCreditCardTypes()
        {
            return sCreditCards;
        }

        static string[] DecryptPasswords(string dBaseData)
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
        static bool ConvertSettingsBoolean(string sSetting)
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

    class StoredTransaction
    {
        // Saves transactions that are stored for later!
        Transaction actualTransaction;
        TillSettings ts;
        int nUserNumber;

        public StoredTransaction(Transaction t, TillSettings tillS, int nNum)
        {
            actualTransaction = t;
            ts = tillS;
            nUserNumber = nNum;
        }

        public int GetUserNumber()
        {
            return nUserNumber;
        }

        public Transaction GetTranaction()
        {
            return actualTransaction;
        }

        public TillSettings GetTillSettings()
        {
            return ts;
        }
    }

    public partial class TillEngine
    {
        Table tDetails;
        Table tRepData;
        Table tStaff;
        Table tStkLevel;
        Table tStock;
        Table tTData;
        Table tTHDR;
        Table tAccStat;
        Table tPresets;
        Table tCustRec;

        TillSettings tsCurrentTillSettings;

        Transaction tCurrentTransation;

        StoredTransaction[] tStoredTransaction;

        bool bPrinterEnabled = true;
        bool bFirstItemToPrint = true;

        public bool LoadTable(string sTableName)
        {
            try
            {
                switch (sTableName)
                {
                    case "DETAILS":
                        tDetails = new Table(GTill.Properties.Settings.Default.sDetailsLoc);
                        // Fix grammatical errors
                        tDetails.EditRecordData(0, 0, tDetails.GetRecordFrom(0)[0].Replace("LORDS", "LORD'S"));
                        tDetails.EditRecordData(0, 0, tDetails.GetRecordFrom(0)[0].TrimEnd(','));
                        tsCurrentTillSettings = new TillSettings(tDetails);
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

                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string sShopName()
        {
            return tsCurrentTillSettings.GetShopName();
        }

        public void ChangeStaffNumber(int nNew)
        {
            bFirstItemToPrint = true;
            tsCurrentTillSettings.SetStaffNumber(nNew);
        }

        public string GetCurrentStaffName()
        {
            int nCurrentStaff = tsCurrentTillSettings.GetCurrentUserNumber();
            return GetStaffName(nCurrentStaff);
        }

        public int GetCurrentStaffNumber()
        {
            return tsCurrentTillSettings.GetCurrentUserNumber();
        }

        public string GetStaffName(int nStaffNumber)
        {
            if (nStaffNumber == 0)
                return CorrectAllUppercase(tsCurrentTillSettings.GetTillName());
            string[] sData = tStaff.GetRecordFrom(nStaffNumber.ToString(), 0);
            return CorrectAllUppercase(sData[1].TrimEnd(' '));
        }

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

        public int GetNextTransactionNumber()
        {
            string[] sData = tRepData.GetRecordFrom("END", 1);
            float fCurrent = (float)Convert.ToDecimal(sData[3]);
            fCurrent *= 100;
            int nCurrent = Convert.ToInt32(fCurrent);
            nCurrent++;
            return nCurrent;
        }

        public string GetTillName()
        {
            return CorrectAllUppercase(tsCurrentTillSettings.GetTillName());
        }

        public bool bSwitchPrinterStatus()
        {
            return tsCurrentTillSettings.SwitchPrinterStatus();
        }

        public Item GetItemAsItemClass(string sBarcode)
        {
            string[] sDetails = tStock.GetRecordFrom(sBarcode, 0, true);
            string[] sSTKLevel;
            if (sDetails.Length == 1)
                return null;
            if (sDetails[7].TrimEnd(' ').Length > 0)
            {
                sSTKLevel = tStkLevel.GetRecordFrom(sDetails[7].TrimEnd(' '), 0);
                if (sSTKLevel.Length == 1)
                {
                    Item sDetailsToReturn = new Item(sDetails);
                    sDetailsToReturn.SetStockLevel(-1024);
                    return sDetailsToReturn;
                }
            }
            else
            {
                sSTKLevel = tStkLevel.GetRecordFrom(sDetails[0].TrimEnd(' '), 0);
                if (sSTKLevel.Length == 1)
                    return (new Item(sDetails));
            }
            return (new Item(sDetails, sSTKLevel));
        }

        public char GetCurrencySymbol()
        {
            return tsCurrentTillSettings.GetCurrenySymbol();
        }

        public void SetupNewTransaction()
        {
            if (!LoadTranscationFromStore(tsCurrentTillSettings.GetCurrentUserNumber()))
            {
                tCurrentTransation = new Transaction(tsCurrentTillSettings, GetNextTransactionNumber());
                tCurrentTransation.VATRate = GetVATRate(1);
            }
        }

        public int GetNumberOfItemsInCurrentTransaction()
        {
            if (tCurrentTransation == null)
                return 0;
            else
                return tCurrentTransation.GetNumberOfItemsInTransaction();
        }

        public int GetNumberOfTotalItemsInCurrentTransaction()
        {
            // Differs from the above because it also counts quantities

            return tCurrentTransation.GetTotalNumberOfItemsPurchased();
        }

        public float GetTotalAmountInTransaction()
        {
            return TillEngine.FixFloatError(tCurrentTransation.GetTotalAmount());
        }

        public void AddItemToTransaction(string sBarcode)
        {
            tCurrentTransation.AddItemToTransaction(sBarcode, ref tStock);
        }

        public void RepeatLastItem()
        {
            this.SetLastItemQuantity(tCurrentTransation.GetLastItemQuantity() + 1);
        }

        public Item GetItemJustAdded()
        {
            return tCurrentTransation.GetItemInTransaction(tCurrentTransation.GetNumberOfItemsInTransaction() - 1);
        }

        public bool WasItemAddSuccessful()
        {
            return tCurrentTransation.GetItemAddStatus();
        }

        public void SetLastItemPrice(float fPrice)
        {
            tCurrentTransation.SetLastItemPrice(fPrice);
        }

        public void SetLastItemDescription(string sDesc)
        {
            tCurrentTransation.SetLastItemDescription(sDesc);
        }

        public string[] GetCreditCards()
        {
            return tsCurrentTillSettings.GetCreditCardTypes();
        }

        public string[,] GetItemsToDisplay()
        {
            string[,] sToReturn = new string[tCurrentTransation.GetNumberOfItemsInTransaction() + 1, 4];
            sToReturn[0, 0] = tCurrentTransation.GetNumberOfItemsInTransaction().ToString();

            for (int i = 1; i <= tCurrentTransation.GetNumberOfItemsInTransaction(); i++)
            {
                Item iCurrentItem = tCurrentTransation.GetItemInTransaction(i - 1);
                sToReturn[i, 0] = CorrectMultiWordUpperCase(iCurrentItem.GetDescription());
                sToReturn[i, 1] = iCurrentItem.GetAmount().ToString();
                sToReturn[i, 2] = iCurrentItem.GetGrossAmount().ToString();
                sToReturn[i, 3] = iCurrentItem.GetQuantity().ToString();
            }
            return sToReturn;
        }

        public void SetLastItemQuantity(int nNewQuantity)
        {
            tCurrentTransation.SetLastItemPrice((tCurrentTransation.GetLastItemNetPrice() / tCurrentTransation.GetLastItemQuantity()) * nNewQuantity);
            tCurrentTransation.SetLastItemQuantity(nNewQuantity);
        }

        public void MultiplyLastItemQuantity(int nMultiplyFactor)
        {
            int nCurrentQuantity = tCurrentTransation.GetLastItemQuantity();
            nCurrentQuantity *= nMultiplyFactor;
            tCurrentTransation.SetLastItemQuantity(nCurrentQuantity);
            tCurrentTransation.SetLastItemPrice(tCurrentTransation.GetLastItemNetPrice() * (float)nMultiplyFactor);
        }

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

        public static string FormatMoneyForDisplay(float fAmount)
        {
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

        public void AddPayment(string sPaymentType, float fPaymentAmount)
        {
            tCurrentTransation.AddPaymentMethod(sPaymentType, fPaymentAmount);
        }
        public void AddPayment(string sPaymentType, float fPaymentAmount, float fExcess)
        {
            tCurrentTransation.AddPaymentMethod(sPaymentType, fPaymentAmount, fExcess);
        }

        public string[,] GetPaymentMethods()
        {
            return tCurrentTransation.GetPaymentMethods();
        }

        public float GetAmountStillDue()
        {
            return TillEngine.FixFloatError(tCurrentTransation.GetStillDue());
        }

        public bool GetAllPaidFor()
        {
            return tCurrentTransation.HasAllBeenPaidFor();
        }

        public float GetChangeDue()
        {
            return TillEngine.FixFloatError(tCurrentTransation.ChangeDue());
        }

        public void SaveTransaction()
        {
            tCurrentTransation.SaveTransaction(ref tRepData, ref tTData, ref tTHDR);
            if (bPrinterEnabled)
            {
                OpenTillDrawer(false);
                PrintReceiptDescAndPriceTitles();
                for (int i = 0; i < tCurrentTransation.GetNumberOfItemsInTransaction(); i++)
                {
                    PrintItem(tCurrentTransation.GetItemInTransaction(i));
                }
                PrintTotalDueSummary(tCurrentTransation.GetTotalNumberOfItemsPurchased(), tCurrentTransation.GetTotalAmount());
                foreach (PaymentMethod pmPay in tCurrentTransation.PaymentMethods)
                {
                    PrintPaymentMethod(pmPay);
                }
                PrintChangeDue();
                PrintVAT();
                PrintReceiptFooter(GetCurrentStaffName(), DateTimeForReceiptFooter(), tCurrentTransation.TransactionNumber.ToString());
                PrintReceiptHeader();
                EmptyPrinterBuffer();
            }
        }

        public float fFixFloatError(float fOriginal)
        {
            return (float)Math.Round((double)fOriginal, 2);
        }

        public static float FixFloatError(float fOriginal)
        {
            return (float)Math.Round((double)fOriginal, 2);
        }

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
            tStoredTransaction[tTemp.Length] = new StoredTransaction(tCurrentTransation, tsCurrentTillSettings, tsCurrentTillSettings.GetCurrentUserNumber());

        }

        public bool LoadTranscationFromStore(int nStaffNumber)
        {
            if (tStoredTransaction == null)
                return false;
            else
            {
                for (int i = 0; i < tStoredTransaction.Length; i++)
                {
                    if (tStoredTransaction[i].GetUserNumber() == nStaffNumber)
                    {
                        tCurrentTransation = tStoredTransaction[i].GetTranaction();
                        tsCurrentTillSettings = tStoredTransaction[i].GetTillSettings();

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

        public void DeleteLine(int nLineNum)
        {
            tCurrentTransation.DeleteLine(nLineNum);
        }

        public void DiscountFixedAmountFromLastItem(float fAmount)
        {
            int nLastItem = tCurrentTransation.GetNumberOfItemsInTransaction() - 1;
            tCurrentTransation.DiscountAmountFromItem(nLastItem, fAmount);
        }

        public void DiscountPercentageFromLastItem(int nPercent)
        {
            int nLastItem = tCurrentTransation.GetNumberOfItemsInTransaction() - 1;
            tCurrentTransation.DiscountPercentageFromItem(nLastItem, nPercent);
        }

        public void DiscountFixedAmountFromWholeTransaction(float fAmount)
        {
            tCurrentTransation.DiscountAmountFromWholeTransaction(fAmount);
        }

        public void DiscountPercentageFromWholeTransaction(int nPercent)
        {
            tCurrentTransation.DiscountPercentageFromWholeTransaction(nPercent);
        }

        public void ClearPaymentMethods()
        {
            tCurrentTransation.ClearAllPayments();
        }

        public bool bAllowChangeFromCheques()
        {
            string[] sAllowed = tDetails.GetRecordFrom(10);
            if (sAllowed[0].Contains("N"))
                return false;
            else
                return true;
        }

        public string[] GetItemDetailsForLookup(string sBarcode)
        {
            // Returns string array in format Description, Price and Stock Level
            string[] s = new string[3];
            Item i = GetItemAsItemClass(sBarcode);
            if (i != null)
            {
                s[0] = i.GetDescription();
                s[1] = FormatMoneyForDisplay(i.GetGrossAmount());
                s[2] = i.GetStockLevel().ToString();
                if (i.GetStockLevel() == -1024)
                    s[2] = "Not found";
            }
            else
                return null;
            return s;
        }

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

                // Then remove from CRD1, CRD2 or whatever it is
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
                fNewAmount = FixFloatError(fCurrentAmount - fAmount);
                nCurrentQuantity = Convert.ToInt32(sCurrentData[2]);
                nNewQuantity = nCurrentQuantity - 1;
                tRepData.EditRecordData(nRecordPos, 2, nNewQuantity.ToString());
                tRepData.EditRecordData(nRecordPos, 3, fNewAmount.ToString());
                if (nNewQuantity == 0)
                    tRepData.DeleteRecord(nRecordPos);
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

            Item iTemp = new Item(tStock.GetRecordFrom(sBarcode.TrimEnd('\0'), 0));
            string sCode = "";
            if (iTemp.GetIsItemStock())
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

        /*private float WorkOutVATOnTransaction(string[,] sTransactionInfo, int nCategory)
        {
            int nOfItems = Convert.ToInt32(sTransactionInfo[0, 0]);
            int nOfPaymentMethods = Convert.ToInt32(sTransactionInfo[0, 1]);

            float fToReturn = 0.0f;
            for (int i = 1; i <= nOfItems; i++)
            {
                Item item = new Item(tStock.GetRecordFrom(sTransactionInfo[i, 0], 0));
                string sVATCode = item.GetVATRate();
                int nVatNum = 0;
                switch (sVATCode)
                {
                    case "Z0":
                        nVatNum = 2;
                        break;
                    case "E1":
                        nVatNum = 1;
                        break;
                    case "I1":
                        nVatNum = 1;
                        break;
                }
                if (nVatNum == nCategory)
                {
                    fToReturn += FixFloatError((float)Convert.ToDecimal(sTransactionInfo[i, 2]));
                    fToReturn = FixFloatError(fToReturn);
                }
            }
            for (int i = nOfItems + 1; i <= nOfItems + nOfPaymentMethods; i++)
            {
                if (sTransactionInfo[i, 0] == "DEPO" || sTransactionInfo[i, 0].StartsWith("CHRG"))
                {
                    fToReturn -= FixFloatError((float)Convert.ToDecimal(sTransactionInfo[i, 1]));
                    fToReturn = FixFloatError(fToReturn);
                    //if (fToReturn < 0.0f)
                      //  fToReturn = 0.0f;
                }
            }

            return fToReturn;
        }*/

        private float WorkOutVATOnTransaction(string[,] sTransactionInfo, int nCategory)
        {
            int nOfItems = Convert.ToInt32(sTransactionInfo[0, 0]);
            int nOfPaymentMethods = Convert.ToInt32(sTransactionInfo[0, 1]);

            float fVat00 = 0.0f, fVat01 = 0.0f, fVat02 = 0.0f;
            for (int i = 1; i <= nOfItems; i++)
            {
                Item item = new Item(tStock.GetRecordFrom(sTransactionInfo[i, 0], 0));
                string sVATCode = item.GetVATRate();
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


        public void RemoveTransactionFromDatabases(int nTransactionNumber)
        {
            string[,] sTransactionInfo = GetTransactionInfo(nTransactionNumber.ToString());
            int nOfItems = Convert.ToInt32(sTransactionInfo[0, 0]);
            int nOfPaymentMethods = Convert.ToInt32(sTransactionInfo[0, 1]);
            // First array thingy in format { NumberOfItems, NumberOfPaymentMethods, TransactionDateTime, SpecialTransaction }
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
                if (sTransactionInfo[i,0].StartsWith("CHRG"))
                    RemovePaymentMethodFromRepData(GetChargeToAccountTransactionInfo(nTransactionNumber.ToString()), FixFloatError((float)Convert.ToDecimal(sTransactionInfo[i, 1])), false);
                else
                    RemovePaymentMethodFromRepData(sTransactionInfo[i, 0], FixFloatError((float)Convert.ToDecimal(sTransactionInfo[i, 1])), false);
            }
            RemoveVAT(0, WorkOutVATOnTransaction(sTransactionInfo, 0), false);
            RemoveVAT(1, WorkOutVATOnTransaction(sTransactionInfo, 1), false);
            RemoveVAT(2, WorkOutVATOnTransaction(sTransactionInfo, 2), false);

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

        public string[,] sGetAllInCategory(string sCategory, ref int nOfResults)
        {
            sCategory = sCategory.TrimEnd(' ');
            string[,] sPossibilities = tStock.SearchAndGetAllMatchingRecords(9, sCategory, ref nOfResults);
            return sPossibilities;
        }

        public string[,] sGetAccordingToPartialBarcode(string sBarcode, ref int nOfResults)
        {
            string[,] sPossibilities = tStock.SearchAndGetAllMatchingRecords(0, sBarcode, ref nOfResults);
            return sPossibilities;
        }

        public int GetItemStockLevel(string sBarcode)
        {
            string[] sInfo = tStkLevel.GetRecordFrom(sBarcode, 0);
            int nToReturn = 0;
            if (sInfo[0] != null)
                nToReturn = Convert.ToInt32(sInfo[2].TrimStart(' ').Replace(".00",""));
            return nToReturn;
        }

        public string[,] sGetAccordingToPartialDescription(string sDesc, ref int nOfResults)
        {
            return tStock.SearchAndGetAllMatchingRecords(1, sDesc.Split(' '), ref nOfResults);
        }

        public string sBarcodeFromFunctionKey(string sFKeyCode)
        {
            switch (sFKeyCode)
            {
                case "F1":
                    return tPresets.GetRecordFrom(0)[0];
                    break;
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

        public string[,] sGetAccounts(ref int nOfResults)
        {
            return tAccStat.SearchAndGetAllMatchingRecords(1, "B",ref nOfResults);
        }

        public string[] sGetAccountDetailsFromCode(string sCode)
        {
            return tAccStat.GetRecordFrom(sCode, 0);
        }

        public string[] sGetAccountDetailsFromDesc(string sCode)
        {
            return tAccStat.GetRecordFrom(sCode.TrimEnd(' '),2);
        }

        // For the Main Menu

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

            string sCurrentUser = tsCurrentTillSettings.GetCurrentUserNumber().ToString();
            while (sCurrentUser.Length < 3)
                sCurrentUser = " " + sCurrentUser;

            string sResult = sDay + "/" + sMonth + "/" + sYear + sHour + ":" + sMinute + ":" + sSecond + sCurrentUser;
            return sResult;
        }

        public string GetPasswords(int nPasswordLevel) // 0 = Admin, 1 = Staff
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
            }
            return sPasswords[nPasswordLevel];
        }

        // For the Lookup Transactions Form

        public string[,]  GetTransactionInfo(string sTransactionNumber)
        {
            // First array thingy in format { NumberOfItems, NumberOfPaymentMethods, TransactionDateTime, SpecialTransaction }
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
                    string sUserNumVoided = sPaymentMethods[0, 5].TrimEnd('\0')[sPaymentMethods[0, 5].TrimEnd('\0').Length - 3].ToString() + sPaymentMethods[0, 5][sPaymentMethods[0, 5].TrimEnd('\0').Length - 2].ToString() + sPaymentMethods[0, 5][sPaymentMethods[0, 5].TrimEnd('\0').Length - 1].ToString();
                    sUserNumVoided = sUserNumVoided.TrimStart(' ');
                    int nUserNum = 0;
                    try
                    {
                        nUserNum = Convert.ToInt32(sUserNumVoided);
                    }
                    catch
                    {
                        nUserNum = 0;
                        GTill.ErrorHandler.LogError("Could not work out user number that voided this transaction. Press continue to user User number 1");
                        throw new NotSupportedException("Could not work out user number that voided this transaction. Press continue to user User number 1");
                    }
                    string sUserName = "";
                    if (nUserNum < 100)
                        sUserName = GetStaffName(nUserNum);
                    else
                        sUserName = "???";
                    sToReturn[0, 3] += "," + sUserName;
                }
            }
            for (int i = 1; i <= nOfItems; i++)
            {
                sToReturn[i, 0] = sTDATARecords[i - 1, 3]; // Barcode
                sToReturn[i, 1] = sTDATARecords[i - 1, 4]; // Description
                sToReturn[i, 2] = FixFloatError((float)Convert.ToDecimal(sTDATARecords[i - 1, 6]) - (float)Convert.ToDecimal(sTDATARecords[i-1,8])).ToString(); // Price Paid
                sToReturn[i, 3] = sTDATARecords[i - 1, 8]; // Discount
                sToReturn[i, 4] = sTDATARecords[i - 1, 5]; // Quantity
            }
            for (int i = nOfItems + 1; i <= (nOfItems + nOfPaymentMethods); i++)
            {
                sToReturn[i, 0] = sPaymentMethods[i - (nOfItems), 4];
                if (sToReturn[i, 0].StartsWith("CRCD"))
                    sToReturn[i, 0] += sPaymentMethods[i - nOfItems, 5].TrimEnd(' ');
                if (sToReturn[i, 0].StartsWith("CHRG"))
                    sToReturn[i, 0] += "," + sPaymentMethods[i - (nOfItems), 5] + "," + sTransactionNumber;
                sToReturn[i, 1] = sPaymentMethods[i - (nOfItems), 3];
            }
            return sToReturn;
        }

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

        public float[] GetAmountOfMoneyInTill()
        {
            // Cash
            // Credit Cards
            // Cheques
            // Vouchers

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
            if (iTemp.GetIsItemStock())
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

        public void VoidTransaction(int nTransactionNumber, int nStaffNumber)
        {
            string[,] sTransactionInfo = GetTransactionInfo(nTransactionNumber.ToString());
            int nOfItems = Convert.ToInt32(sTransactionInfo[0, 0]);
            int nOfPaymentMethods = Convert.ToInt32(sTransactionInfo[0, 1]);
            // First array thingy in format { NumberOfItems, NumberOfPaymentMethods, TransactionDateTime, SpecialTransaction }
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
                RemovePaymentMethodFromRepData(sTransactionInfo[i, 0], FixFloatError((float)Convert.ToDecimal(sTransactionInfo[i, 1])), false);
            }
            VoidVAT(0, WorkOutVATOnTransaction(sTransactionInfo, 0));
            VoidVAT(1, WorkOutVATOnTransaction(sTransactionInfo, 1));
            VoidVAT(2, WorkOutVATOnTransaction(sTransactionInfo, 2));

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
                    sCurrentData[5] = sCurrentData[5] + sStaffNum;
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

        private void RefundPaymentMethod(string sPaymentMethod, float fAmount)
        {
            RemovePaymentMethodFromRepData(sPaymentMethod, fAmount, true);
        }

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
            Item i = new Item(tStock.GetRecordFrom(sBarcode, 0));
            string sVatCode = i.GetVATRate();
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
                if (sPaymentMethod != "DEPO")
                    RemoveVAT(nVatRate, fIndividualItemAmount, true);
                RemovePaymentMethodFromRepData(sPaymentMethod, fIndividualItemAmount, true);
            }

            // Now add to TDATA the refund
            string[] sTDataRecord = { 
                                        sTransactionNumber,
                                        "01",
                                        "0",
                                        sBarcode,
                                        i.GetDescription(),
                                        (nQuantity * -1).ToString(),
                                        FixFloatError(fAmount * -1).ToString(),
                                        "1",
                                        "0",
                                        i.GetVATRate()
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
            PrintSpecificRefund(i.GetDescription(), fAmount, pmRefund, nQuantity,false);
        }

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
            PrintGeneralRefund(pmPayMethod, false);

        }

        private string GetChargeToAccountTransactionInfo(string sTransactionNumber)
        {
            //Returns CHRG,AccountCode,TransactionNumber
            string[] sRecordData = tRepData.GetRecordFrom(sTransactionNumber, 1);
            string sAccountCode = "";
            for (int i = 0; i < 6; i++)
                sAccountCode += sRecordData[1][i].ToString();
            string sToReturn = "CHRG," + sAccountCode + "," + sTransactionNumber;
            return sToReturn;
        }

        // For the received on account form

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
            float fCurrentAmount = TillEngine.fFormattedMoneyString(sCurrentPaymentData[3]);
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
            fCurrentAmount = TillEngine.fFormattedMoneyString(sCurrentPaymentData[3]);
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
            fCurrentAmount = TillEngine.fFormattedMoneyString(sCurrentPaymentData[3]);
            fCurrentAmount = FixFloatError(fCurrentAmount + fAmountToReceive);
            nCurrentQty = Convert.ToInt32(sCurrentPaymentData[2]);
            nCurrentQty++;
            tRepData.EditRecordData(nPaymentRecordPos, 2, nCurrentQty.ToString());
            tRepData.EditRecordData(nPaymentRecordPos, 3, fAmountToReceive.ToString());

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

        public void SetDateInRepData(string sDateTime)
        {
            //sDateTimeInFormat dd/mm/yyyy as from frmDateInput

            string[] sDate = sDateTime.Split('/');
            string sNewDate = sDate[0] + "/" + sDate[1] + "/" + sDate[2][2].ToString() + sDate[2][3].ToString();
            tRepData.EditRecordData(0, 1, sNewDate);
            tRepData.EditRecordData(0, 2, "0");
            tRepData.SaveToFile(GTill.Properties.Settings.Default.sRepDataLoc);
        }

        public bool RepdataNeedsDate()
        {
            if (Convert.ToInt32(tRepData.GetRecordFrom(0)[2]) == 4)
                return true;
            else
                return false;
        }

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

        public void ProcessFilesInINGNG()
        {
            string sINGNGDir = GTill.Properties.Settings.Default.sINGNGDir;
            string[] sFiles = Directory.GetFiles(sINGNGDir);
            for (int i = 0; i < sFiles.Length; i++)
            {
                sFiles[i] = sFiles[i].Split('\\')[sFiles[i].Split('\\').Length - 1];
                if (File.ReadAllBytes(sINGNGDir + "\\" + sFiles[i]).Length != 0)
                {

                    bool bSorted = true;
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
                            break;
                        case "STAFF.DBF":
                            File.Copy(sINGNGDir + "\\STAFF.DBF", GTill.Properties.Settings.Default.sStaffLoc, true);
                            tStaff = new Table(GTill.Properties.Settings.Default.sStaffLoc);
                            bSorted = true;
                            break;
                        case "TILLCAT.DBF":
                            File.Copy(sINGNGDir + "\\TILLCAT.DBF", GTill.Properties.Settings.Default.sTillCatLoc, true);
                            bSorted = true;
                            break; // TILL CAT IS ONLY LOADED WHEN FRMSEARCHFORITEM IS LOADED, TODO : CHANGE THIS

                    }
                    if (bSorted)
                        File.Delete(sINGNGDir + "\\" + sFiles[i]);
                    else
                    {
                        GTill.ErrorHandler.LogError("I don't know what to do with " + sFiles[i] + " in the INGNG directory. Press continue to ignore.");
                        throw new NotImplementedException("I don't know what to do with " + sFiles[i] + " in the INGNG directory. Press continue to ignore.");
                    }
                }
                sFiles = Directory.GetFiles(sINGNGDir);
                if (sFiles.Length != 0)
                {
                    for (int z = 0; z < sFiles.Length; z++)
                    {
                        Directory.CreateDirectory("Quarantine");

                        File.Copy(sFiles[z], "Quarantine\\" + sFiles[z].Split('\\')[sFiles[z].Split('\\').Length - 1], true);
                    }
                }
            }
        }

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

        public int GetDayNumberFromRepData()
        {
            string sDateToday = tRepData.GetRecordFrom(0)[1].TrimEnd('\0');
            return DayNumber(sDateToday);
        }

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
    }
}

