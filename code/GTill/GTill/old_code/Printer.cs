using System;
using DBFDetailsViewer;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using GTill;

namespace TillEngine
{
    public partial class TillEngine
    {
        // An extension of the till engine, just so it's more convenient to program
        // Contains all the printer code
        
        frmReceiptDisplay frdShowReceipt;
        private void InitialiseFakePrinter()
        {
            frdShowReceipt = new frmReceiptDisplay();
            frdShowReceipt.Show();
        }
        private void SendLineToPrinter(string sText)
        {
            string[] sToAdd = { sText };
            SendLinesToPrinter(sToAdd);
        }
        private void SendLinesToPrinter(string[] sLines)
        {
            while (sPrinterBuffer.Length - nBufferPos < sLines.Length)
                ExpandPrinterBufferSize();

            for (int i = nBufferPos; i < sLines.Length + nBufferPos; i++)
            {
                sPrinterBuffer[i] = sLines[i - nBufferPos];
            }

            nBufferPos += sLines.Length;
        }
        private void EmptyPrinterBuffer()
        {
            TextWriter writeReceipt;
            try
            {
                if (File.Exists(sOutGNGFolderLocation + "\\receipts.txt"))
                    File.Copy(sOutGNGFolderLocation + "\\receipts.txt", sOutGNGFolderLocation + "\\prev_receipts.txt", true);
                writeReceipt = new StreamWriter(sOutGNGFolderLocation + "\\receipts.txt", true);
                foreach (string s in sPrinterBuffer)
                {
                    if (s != null)
                        writeReceipt.WriteLine(s);
                }
                writeReceipt.Close();
            }
            catch
            {
                ErrorHandler.LogError("An error occured trying to save a copy of the receipt. Selecting continue here should cause no problems, but receipts.txt will be incorrect");
                throw new NotSupportedException("An error occured trying to save a copy of the receipt. Selecting continue here should cause no problems, but receipts.txt will be incorrect");
                // Could not write to receipt file!
            }
            if (bPrinterEnabled)
            {
                if (GTill.Properties.Settings.Default.bUseVirtualPrinter)
                {
                    if (frdShowReceipt == null)
                        InitialiseFakePrinter();
                    int nBufferUsed = 0;
                    for (int i = 0; i < sPrinterBuffer.Length; i++)
                    {
                        if (sPrinterBuffer[i] == null)
                        {
                            nBufferUsed = i;
                            break;
                        }
                    }
                    string[] sToSend = new string[nBufferUsed];
                    for (int i = 0; i < nBufferUsed; i++)
                    {
                        sToSend[i] = sPrinterBuffer[i];
                    }
                    frdShowReceipt.AddLines(sToSend);
                    sPrinterBuffer = new string[50];
                    nBufferPos = 0;
                }
                else
                {
                    int nBufferUsed = 0;
                    for (int i = 0; i < sPrinterBuffer.Length; i++)
                    {
                        if (sPrinterBuffer[i] == null)
                        {
                            nBufferUsed = i;
                            break;
                        }
                    }
                    IntPtr ptr = CreateFile(sPortName, GENERIC_WRITE, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
                    if (ptr.ToInt32() == -1)
                    {
                        // Error! Invalid Handle Value
                        ErrorHandler.LogError("Error whilst sending data to " + sPortName + ". Disable the printer or enable the virtual printer as a temporary solution.");
                        throw new NotSupportedException("Error whilst sending data to " + sPortName + ". Disable the printer or enable the virtual printer as a temporary solution.");
                    }
                    else
                    {
                        fsOutput = new FileStream(ptr, FileAccess.Write);
                        for (int i = 0; i < nBufferUsed; i++)
                        {
                            bBuffer = Encoding.ASCII.GetBytes(sPrinterBuffer[i] + "\n");
                            fsOutput.Write(bBuffer, 0, bBuffer.Length);
                        }
                        fsOutput.Close();
                        sPrinterBuffer = new string[50];
                        nBufferPos = 0;
                    }
                }
            }
            else
            {
                sPrinterBuffer = new string[50];
                nBufferPos = 0;
            }
        }
        // End
        
        
        uint GENERIC_WRITE = 0x40000000, OPEN_EXISTING = 3;
        string sPortName = GTill.Properties.Settings.Default.sPrinterOutputPort;
        int nPrinterWidth = GTill.Properties.Settings.Default.nPrinterCharWidth;
        const char cReceiptBreaker = '-';
        FileStream fsOutput;
        Byte[] bBuffer = new Byte[2048];
        string[] sPrinterBuffer = new string[50];
        int nBufferPos = 0;

        [DllImport("kernel32.dll", SetLastError=true)]
        static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
        
        public void TogglePrinterStatus()
        {
            bPrinterEnabled = !bPrinterEnabled;
        }

        public bool PrinterEnabled
        {
            get
            {
                return bPrinterEnabled;
            }
        }

        private void ExpandPrinterBufferSize()
        {
            string[] sOldBuffer = sPrinterBuffer;
            sPrinterBuffer = new string[sOldBuffer.Length + 50];
            for (int i = 0; i < sOldBuffer.Length; i++)
            {
                sPrinterBuffer[i] = sOldBuffer[i];
            }
        }

        private string CentralisePrinterText(string sToCentralise)
        {
            string sToGoOnBeginning = "";
            string sToGoOnEnd = "";
            int nEitherSide = (nPrinterWidth - sToCentralise.Length) / 2;
            for (int i = 0; i < nEitherSide; i++)
            {
                sToGoOnBeginning += " ";
                sToGoOnEnd += " ";
            }
            while (sToGoOnEnd.Length + sToCentralise.Length + sToGoOnBeginning.Length < nPrinterWidth)
                sToGoOnEnd += " ";
            return sToGoOnBeginning + sToCentralise + sToGoOnEnd;
        }

        private string RightAlignStringOnExistingString(string sExisting, string sToRightAlign)
        {
            int nSpacesNeeded = nPrinterWidth - sExisting.Length - sToRightAlign.Length;
            for (int i = 0; i < nSpacesNeeded; i++)
            {
                sExisting += " ";
            }
            sExisting += sToRightAlign;
            return sExisting;
        }

        private string RightAlignWholeString(string sExisting)
        {
            while (sExisting.Length < nPrinterWidth)
                sExisting = " " + sExisting;
            return sExisting;
        }

        public void PrintReceiptHeader()
        {
            string[] sToPrint = new string[5];
            for (int i = 0; i < 3; i++)
            {
                sToPrint[i] = tDetails.GetRecordFrom(i)[0].TrimEnd('\0').TrimEnd(' ');
                sToPrint[i] = CentralisePrinterText(sToPrint[i]);
            }
            sToPrint[3] = "TEL:" + tDetails.GetRecordFrom(12)[0].TrimEnd('\0').Trim(' ');
            sToPrint[3] = RightAlignStringOnExistingString(sToPrint[3], "VAT NO:" + tDetails.GetRecordFrom(3)[0].Trim('\0').Trim(' '));
            for (int i = 0; i < nPrinterWidth; i++)
                sToPrint[4] += cReceiptBreaker;
            SendLinesToPrinter(sToPrint);
        }

        public void PrintReceiptFooter(string sUserName, string sDateTime, string sTransactionNumber)
        {
            string[] sLines = new string[6];
            for (int i = 0; i < nPrinterWidth; i++)
            {
                sLines[0] += cReceiptBreaker;
            }
            sLines[1] = sUserName + ", " + sDateTime + " " + sTransactionNumber;
            sLines[1] = CentralisePrinterText(sLines[1]);
            for (int i = 2; i < sLines.Length-1; i++)
            {
                sLines[i] = CentralisePrinterText(tDetails.GetRecordFrom(i + 33)[0].TrimEnd(' '));
            }
            sLines[5] = "";
            SendLinesToPrinter(sLines);
            AddReceiptSpacingToBuffer();
        }

        public void PrintReceiptDescAndPriceTitles()
        {
            string[] sToPrint = new string[2];
            sToPrint[0] = "DESCRIPTION";
            sToPrint[0] = RightAlignStringOnExistingString(sToPrint[0], "PRICE");
            for (int i = 0; i < nPrinterWidth; i++)
            {
                sToPrint[1] += cReceiptBreaker;
            }
            SendLinesToPrinter(sToPrint);
        }

        public void PrintItem(Item iItem)
        {
            string sDescLine = iItem.GetDescription();
            string sQuantityLine = "";
            string sDiscountLine = "";
            bool bMultipleQuantities = false, bDiscount = false;
            if (iItem.GetQuantity() != 1)
                bMultipleQuantities = true;
            if (FixFloatError(iItem.GetQuantity() * iItem.GetGrossAmount()) > iItem.GetAmount())
                bDiscount = true;
            if (bMultipleQuantities)
            {
                sQuantityLine = "QUANTITY : " + iItem.GetQuantity().ToString() + " @ " + FormatMoneyForDisplay(iItem.GetGrossAmount());
                string sFormattedMoney = "";
                if (!bDiscount)
                {
                    sFormattedMoney = FormatMoneyForDisplay(iItem.GetAmount());
                    while (sFormattedMoney.Length < 7) // Allows upto 9999.99
                        sFormattedMoney = " " + sFormattedMoney;
                }
                else
                {
                    sFormattedMoney = "        ";
                }
                    sQuantityLine = RightAlignWholeString(sQuantityLine + " " + sFormattedMoney);
            }
            if (FixFloatError(iItem.GetQuantity() * iItem.GetGrossAmount()) > iItem.GetAmount())
                sDiscountLine = RightAlignWholeString("DISCOUNT : " + (FormatMoneyForDisplay(FixFloatError((iItem.GetGrossAmount() * iItem.GetQuantity()) - iItem.GetAmount())) + "     " + FormatMoneyForDisplay(iItem.GetAmount())).ToString());
            if (sQuantityLine == "" && sDiscountLine == "")
            {
                sDescLine = RightAlignStringOnExistingString(sDescLine, FormatMoneyForDisplay(iItem.GetAmount()));
                SendLineToPrinter(sDescLine);
            }
            else
            {
                SendLineToPrinter(sDescLine);
                if (sQuantityLine != "")
                {
                    SendLineToPrinter(sQuantityLine);
                }
                if (sDiscountLine != "")
                {
                    SendLineToPrinter(sDiscountLine);
                }
            }
        }
        public void PrintItem(string[] sItemInfo)
        {
            // Item array in format { Item code, Item Description, Price Paid, Discount, Quantity }
            string sDescLine = sItemInfo[1];
            string sQuantityLine = "";
            string sDiscountLine = "";
            float fDiscountAmount = FixFloatError((float)Convert.ToDecimal(sItemInfo[3]));
            float fPricePaid = FixFloatError((float)Convert.ToDecimal(sItemInfo[2]));
            int nQuantity = Convert.ToInt32(sItemInfo[4]);
            float fIndividualPricePaid = FixFloatError(fPricePaid / nQuantity);
            float fDiscountPerItem = FixFloatError(fDiscountAmount / nQuantity);
            float fGrossPerItem = FixFloatError(fIndividualPricePaid + fDiscountPerItem);
            bool bMultipleQuantities = false, bDiscount = false;
            if (nQuantity != 1)
                bMultipleQuantities = true;
            if (fDiscountAmount > 0.0f)
                bDiscount = true;
            if (bMultipleQuantities)
            {
                sQuantityLine = "QUANTITY : " + nQuantity.ToString() + " @ " + FormatMoneyForDisplay(fGrossPerItem);
                string sFormattedMoney = "";
                if (!bDiscount)
                {
                    sFormattedMoney = FormatMoneyForDisplay(fPricePaid);
                    while (sFormattedMoney.Length < 7) // Allows upto 9999.99
                        sFormattedMoney = " " + sFormattedMoney;
                }
                else
                {
                    sFormattedMoney = "        ";
                }
                sQuantityLine = RightAlignWholeString(sQuantityLine + " " + sFormattedMoney);
            }
            if (fDiscountAmount > 0.0f)
                sDiscountLine = RightAlignWholeString("DISCOUNT : " + (FormatMoneyForDisplay(fDiscountAmount)) + "     " + FormatMoneyForDisplay(fPricePaid).ToString());
            if (sQuantityLine == "" && sDiscountLine == "")
            {
                sDescLine = RightAlignStringOnExistingString(sDescLine, FormatMoneyForDisplay(fPricePaid));
                SendLineToPrinter(sDescLine);
            }
            else
            {
                SendLineToPrinter(sDescLine);
                if (sQuantityLine != "")
                {
                    SendLineToPrinter(sQuantityLine);
                }
                if (sDiscountLine != "")
                {
                    SendLineToPrinter(sDiscountLine);
                }
            }
        }

        public void PrintPaymentMethod(PaymentMethod pmMethod)
        {
            string sPaymentLine =  GetPaymentDescription(pmMethod.GetPMType()).ToUpper();
            if (sPaymentLine == "CASH")
                sPaymentLine += " TENDERED";
            sPaymentLine = RightAlignStringOnExistingString(sPaymentLine, FormatMoneyForDisplay(pmMethod.Excess));
            SendLineToPrinter(sPaymentLine);
        }

        public void PrintTotalDueSummary(int nOfItems, float fTotalDue)
        {
            SendLineToPrinter(RightAlignWholeString("--------"));
            string sTotalLine = "";
            if (nOfItems == 1)
                sTotalLine += "1 ITEM";
            else
                sTotalLine += nOfItems.ToString() + " ITEMS";
            string sTotalDueSection = "TOTAL DUE   ";
            string sFormattedMoney = FormatMoneyForDisplay(fTotalDue);
            while (sFormattedMoney.Length < 8)
                sFormattedMoney = " " + sFormattedMoney;
            sTotalDueSection += sFormattedMoney;
            sTotalLine = RightAlignStringOnExistingString(sTotalLine, sTotalDueSection);
            SendLineToPrinter(sTotalLine);
            SendLineToPrinter(RightAlignWholeString("========"));
        }

        public string GetPaymentDescription(string sPaymentCode)
        {
            string[] sCreditCards = GetCreditCards();
            sPaymentCode = sPaymentCode.TrimEnd('\0');
            switch (sPaymentCode)
            {
                case "CASH":
                    return "Cash";
                    break;
                case "CRCD":
                    return "Credit Card";
                    break;
                case "CRCD1":
                    return "Credit Card (" + sCreditCards[0] + ")";
                    break;
                case "CRCD2":
                    return "Credit Card (" + sCreditCards[1] + ")";
                    break;
                case "CRCD3":
                    return "Credit Card (" + sCreditCards[2] + ")";
                    break;
                case "CRCD4":
                    return "Credit Card (" + sCreditCards[3] + ")";
                    break;
                case "CRCD5":
                    return "Credit Card (" + sCreditCards[4] + ")";
                    break;
                case "CRCD6":
                    return "Credit Card (" + sCreditCards[5] + ")";
                    break;
                case "CRCD7":
                    return "Credit Card (" + sCreditCards[6] + ")";
                    break;
                case "CRCD8":
                    return "Credit Card (" + sCreditCards[7] + ")";
                    break;
                case "CRCD9":
                    return "Credit Card (" + sCreditCards[8] + ")";
                    break;
                case "CHEQ":
                    return "Cheque";
                    break;
                case "DEPO":
                    return "Deposit Paid";
                    break;
                case "VOUC":
                    return "Voucher";
                    break;
                case "ACNT":
                    return "";
                    break;
            }
            string[] sSplit = sPaymentCode.Split(',');
            if (sSplit[0] == "CHRG")
            {
                // Charged to account
                return "CHARGED TO A/C " + sSplit[1];
            }
            return "!! ** URGENT CODE BUG ** !!";
        }

        private void PrintSignOnDottedLine()
        {
            SendLineToPrinter("");
            SendLineToPrinter("SIGNED ..............................");
        }

        public float GetVATRate(int nVATRate)
        {
            float fVat = (float)Convert.ToDecimal(tDetails.GetRecordFrom(26 + nVATRate)[0]);
            fVat = FixFloatError(fVat);
            return fVat;
        }

        public void PrintVAT()
        {
            SendLineToPrinter("V.A.T. INCLUDED");
            // Do for 0% VAT First
            float fTransactionTotal = tCurrentTransation.GetTotalAmount();
            string sCurrentVAT = "Z0";
            float fCurrentVATRate = GetVATRate(0);
            float fCurrentVATGross = tCurrentTransation.GetAmountForVATRate(sCurrentVAT);
            sCurrentVAT = "X0";
            fCurrentVATGross += tCurrentTransation.GetAmountForVATRate(sCurrentVAT);
            float fVATAmount = FixFloatError(fCurrentVATGross - (fCurrentVATGross / (1 + (fCurrentVATRate / 100))));
            if (fCurrentVATGross != 0.0f)
                PrintVATRate(fCurrentVATGross, fCurrentVATRate, fVATAmount);

            // Now do for 15% VAT (or 17.5%, or 20%, whatever the government decides to do in January...)
            sCurrentVAT = "I1";
            fCurrentVATRate = GetVATRate(1);
            fCurrentVATGross = FixFloatError(tCurrentTransation.GetAmountForVATRate(sCurrentVAT) + tCurrentTransation.GetAmountForVATRate("E1"));
            fVATAmount = FixFloatError(fCurrentVATGross - (fCurrentVATGross / (1 + (fCurrentVATRate / 100))));
            if (fCurrentVATGross != 0.0f)
                PrintVATRate(fCurrentVATGross, fCurrentVATRate, fVATAmount);
        }
        public void PrintVAT(float fNoVATAmount, float fNormalVATAmount)
        {
            SendLineToPrinter("V.A.T. INCLUDED");
            // Do for 0% VAT First
            float fTransactionTotal = FixFloatError(fNoVATAmount + fNormalVATAmount);
            float fCurrentVATRate = GetVATRate(0);
            float fCurrentVATGross = fNoVATAmount;
            float fVATAmount = FixFloatError(fCurrentVATGross - (fCurrentVATGross / (1 + (fCurrentVATRate / 100))));
            if (fCurrentVATGross != 0.0f)
                PrintVATRate(fCurrentVATGross, fCurrentVATRate, fVATAmount);

            // Now do for 15% VAT (or 17.5%, or 20%, whatever the government decides to do in January 2010...)
            fCurrentVATRate = GetVATRate(1);
            fCurrentVATGross = fNormalVATAmount;
            fVATAmount = FixFloatError(fCurrentVATGross - (fCurrentVATGross / (1 + (fCurrentVATRate / 100))));
            if (fCurrentVATGross != 0.0f)
                PrintVATRate(fCurrentVATGross, fCurrentVATRate, fVATAmount);
        }

        private void PrintVATRate(float fGross, float fVATRate, float fVATAmount)
        {
            string sVATLine = "       ";
            sVATLine += FormatMoneyForDisplay(fGross);
            sVATLine += " @ " + FormatMoneyForDisplay(fVATRate);
            sVATLine += " % V.A.T. = ";
            sVATLine = RightAlignStringOnExistingString(sVATLine, FormatMoneyForDisplay(fVATAmount));
            SendLineToPrinter(sVATLine);
        }

        public void PrintChangeDue()
        {
            string sChangeDue = "CHANGE";
            sChangeDue = RightAlignStringOnExistingString(sChangeDue, FormatMoneyForDisplay(GetChangeDue()));
            SendLineToPrinter(sChangeDue);
        }
        public void PrintChangeDue(float fAmountDue)
        {
            string sChangeDue = "CHANGE";
            sChangeDue = RightAlignStringOnExistingString(sChangeDue, FormatMoneyForDisplay(fAmountDue));
            SendLineToPrinter(sChangeDue);
        }

        public void ReprintTransactionReceipt(int nTransactionNumber)
        {
            string[,] sTransactionInfo = GetTransactionInfo(nTransactionNumber.ToString());
            // First array thingy in format { NumberOfItems, NumberOfPaymentMethods, TransactionDateTime, SpecialTransaction }
            // Item array in format { Item code, Item Description, Price Paid, Discount, Quantity }
            // Payment method array format { PaymentCode, Amount, Blank, Blank, Blank }
            //
            // SpecialTransactions can be CASHPAIDOUT, SPECIFICREFUND, VOID
            int nOfItems = Convert.ToInt32(sTransactionInfo[0, 0]);
            int nOfItemIncMulQty = 0;
            int nOfPaymentMethods = Convert.ToInt32(sTransactionInfo[0, 1]);
            string sDateTime = sTransactionInfo[0, 2];
            string sSpecialTransaction = sTransactionInfo[0, 3];
            if (sSpecialTransaction == "CASHPAIDOUT")
            {
                string[] sInfo = ReturnSensibleDateTimeString(sDateTime.TrimEnd('\0'));
                PrintCashPaidOut(FixFloatError(-(float)Convert.ToDecimal(sTransactionInfo[1, 1])), sInfo[0], sInfo[1], nTransactionNumber);
            }
            else if (sSpecialTransaction == "SPECIFICREFUND")
            {
                string sItemDesc = sTransactionInfo[1, 1];
                float fAmountRefunded = FixFloatError(-(float)Convert.ToDecimal(sTransactionInfo[2, 1]));
                PaymentMethod pm = new PaymentMethod();
                pm.SetPaymentMethod(sTransactionInfo[2, 0], 0.0f, fAmountRefunded);
                int nQuantityRefunded = -Convert.ToInt32(sTransactionInfo[1, 4]);
                PrintSpecificRefund(sItemDesc, fAmountRefunded, pm, nQuantityRefunded, true);
            }
            else if (sSpecialTransaction == "GENERALREFUND")
            {
                float fGeneralRefundAmount = FixFloatError(-(float)Convert.ToDecimal(sTransactionInfo[1, 1]));
                PaymentMethod pm = new PaymentMethod();
                pm.SetPaymentMethod(sTransactionInfo[1, 0], 0.0f, fGeneralRefundAmount);
                PrintGeneralRefund(pm, true);
            }
            else
            {
                PrintReceiptDescAndPriceTitles();
                float fTotalDue = 0.0f;
                // Normal tranasaction
                for (int i = 1; i <= nOfItems; i++)
                {
                    string[] sItemInfo = { sTransactionInfo[i, 0], sTransactionInfo[i, 1], sTransactionInfo[i, 2], sTransactionInfo[i, 3], sTransactionInfo[i, 4] };
                    PrintItem(sItemInfo);
                    nOfItemIncMulQty += Convert.ToInt32(sTransactionInfo[i, 4]);
                    fTotalDue = FixFloatError(fTotalDue + (float)Convert.ToDecimal(sTransactionInfo[i, 2]));
                }
                PrintTotalDueSummary(nOfItemIncMulQty, fTotalDue);
                float fTotalPaid = 0.0f;
                for (int i = nOfItems + 1; i <= nOfItems + nOfPaymentMethods; i++)
                {
                    fTotalPaid += FixFloatError((float)Convert.ToDecimal(sTransactionInfo[i, 1]));
                }
                float fExcess = 0.0f;
                bool bChargedToAccountReprint = false;
                if (fTotalPaid > fTotalDue)
                    fExcess = FixFloatError(fTotalPaid - fTotalDue);
                for (int i = nOfItems + 1; i <= nOfItems + nOfPaymentMethods; i++)
                {
                    PaymentMethod pmTemp = new PaymentMethod();
                    float fAmount = FixFloatError((float)Convert.ToDecimal(sTransactionInfo[i, 1]));
                    if (sTransactionInfo[i, 0] != "CASH")
                        pmTemp.SetPaymentMethod(sTransactionInfo[i, 0], fAmount, 0.0f);
                    else
                        pmTemp.SetPaymentMethod(sTransactionInfo[i, 0], FixFloatError(fAmount - fExcess), fExcess);
                    if (sTransactionInfo[i, 0].StartsWith("CHRG"))
                        bChargedToAccountReprint = true;
                    PrintPaymentMethod(pmTemp);
                }
                if (bChargedToAccountReprint)
                    PrintSignOnDottedLine();
                PrintChangeDue(fExcess);
                // Now work out the V.A.T.
                float fNoVat = 0.0f, fNormalVAT = 0.0f;
                for (int i = 1; i <= nOfItems; i++)
                {
                    Item iItem = new Item(tStock.GetRecordFrom(sTransactionInfo[i, 0], 0));
                    if (iItem.GetVATRate() == "X0" || iItem.GetVATRate() == "Z0")
                        fNoVat += FixFloatError((float)Convert.ToDecimal(sTransactionInfo[i, 2]));
                    else
                        fNormalVAT += FixFloatError((float)Convert.ToDecimal(sTransactionInfo[i, 2]));
                }
                PrintVAT(fNoVat, fNormalVAT);
                PrintReprintReceiptNote();
                string[] sFooterData = ReturnSensibleDateTimeString(sDateTime);
                PrintReceiptFooter(sFooterData[1], sFooterData[0], nTransactionNumber.ToString());
                PrintReceiptHeader();
                EmptyPrinterBuffer();
            }
        }

        private void PrintReprintReceiptNote()
        {
            string sBreaker = "";
            for (int i = 0; i < nPrinterWidth; i++)
            {
                sBreaker += cReceiptBreaker;
            }
            SendLineToPrinter(sBreaker);
            SendLineToPrinter(CentralisePrinterText("REPRINT RECEIPT"));
        }

        public string[] ReturnSensibleDateTimeString(string sInput)
        {
            string sDate = "";
            string sTime = "";
            string sUserID = "";
            if (sInput.Contains("/"))
            {
                for (int i = 0; i < 8; i++)
                {
                    sDate += sInput[i].ToString();
                }
                for (int i = 8; i < 13; i++)
                {
                    sTime += sInput[i].ToString();
                }
                for (int i = 16; i < sInput.Length; i++)
                {
                    if (sInput[i] != ' ')
                        sUserID += sInput[i].ToString();
                }
                sUserID = sUserID.TrimEnd('\0');
            }
            else
            {
                sDate = sInput[4].ToString() + sInput[5].ToString() + "/" + sInput[2].ToString() + sInput[3].ToString() + "/"
                    + sInput[0].ToString() + sInput[1].ToString();
                sTime = sInput[6].ToString() + sInput[7].ToString() + ":" + sInput[8].ToString() + sInput[9].ToString();
                sUserID = sInput[10].ToString() + sInput[11].ToString();
                if (sUserID.StartsWith("0"))
                    sUserID = sUserID[1].ToString();
            }

            int nStaffNum = Convert.ToInt32(sUserID);
            string sStaffName = GetStaffName(nStaffNum);

            string[] sToReturn = new string[2];
            sToReturn[0] = sDate + " " + sTime;
            sToReturn[1] = sStaffName;
            return sToReturn;
        }

        public void PrintCashPaidOut(float fAmountPaidOut)
        {
            string[] sToSend = new string[2];
            SendLineToPrinter(CentralisePrinterText("CASH PAID OUT"));
            PrintBreaker();
            sToSend[0] = RightAlignStringOnExistingString("CASH", FormatMoneyForDisplay(fAmountPaidOut));
            sToSend[1] = "";
            SendLinesToPrinter(sToSend);
            PrintSignOnDottedLine();
            PrintReceiptFooter(GetCurrentStaffName(), DateTimeForReceiptFooter(), tCurrentTransation.TransactionNumber.ToString());
            PrintReceiptHeader();
            EmptyPrinterBuffer();
        }
        public void PrintCashPaidOut(float fAmountPaidOut, string sDateTime, string sStaffName, int nTransactionNumber)
        {
            // For reprint receipt
            SendLineToPrinter(CentralisePrinterText("CASH PAID OUT"));
            PrintBreaker();
            string[] sToSend = new string[2];
            sToSend[0] = RightAlignStringOnExistingString("CASH", FormatMoneyForDisplay(fAmountPaidOut));
            sToSend[1] = "";
            SendLinesToPrinter(sToSend);
            PrintSignOnDottedLine();
            PrintReprintReceiptNote();
            PrintReceiptFooter(sStaffName, sDateTime, nTransactionNumber.ToString());
            PrintReceiptHeader();
            EmptyPrinterBuffer();
        }


        public void PrintSpecificRefund(string sItemDesc, float fAmountRefunded, PaymentMethod pmRefundMethod, int nQuantity, bool bReprintReceipt)
        {
            // Item array in format { Item code, Item Description, Price Paid, Discount, Quantity }
            string[] sItemInfo = { "NULL", sItemDesc, FormatMoneyForDisplay(-fAmountRefunded), "0.00", nQuantity.ToString() };
            PrintReceiptDescAndPriceTitles();
            SendLineToPrinter(CentralisePrinterText("SPECIFIC REFUND"));
            PrintBreaker();
            PrintItem(sItemInfo);
            PrintPaymentMethod(pmRefundMethod);
            if (bReprintReceipt)
                PrintReprintReceiptNote();
            PrintReceiptFooter(GetCurrentStaffName(), DateTimeForReceiptFooter(), tCurrentTransation.TransactionNumber.ToString());
            PrintReceiptHeader();
            EmptyPrinterBuffer();
        }

        private void PrintBreaker()
        {
            string sToPrint = "";
            for (int i = 0; i < nPrinterWidth; i++)
            {
                sToPrint += cReceiptBreaker;
            }
            SendLineToPrinter(sToPrint);
        }

        public void PrintGeneralRefund(PaymentMethod pmPayMethod, bool bReceiptReprint)
        {
            PrintReceiptDescAndPriceTitles();
            SendLineToPrinter(CentralisePrinterText("GENERAL REFUND"));
            PrintBreaker();
            PrintPaymentMethod(pmPayMethod);
            SendLineToPrinter("");
            PrintSignOnDottedLine();
            if (bReceiptReprint)
                PrintReprintReceiptNote();
            PrintReceiptFooter(GetCurrentStaffName(), DateTimeForReceiptFooter(), tCurrentTransation.TransactionNumber.ToString());
            PrintReceiptHeader();
            EmptyPrinterBuffer();
        }

        public void PrintRegisterReport()
        {
            // Used for cashing up etc

            SendLineToPrinter(CentralisePrinterText("REGISTER REPORT   No: " + GetNextTransactionNumber().ToString()));
            string sHeaderLine = tsCurrentTillSettings.GetTillName() + " for " + DateTimeForReceiptFooter();
            SendLineToPrinter(CentralisePrinterText(sHeaderLine));
            PrintBreaker();
            // First Section
            string[] sStartRecord = tRepData.GetRecordFrom("START", 1);
            string[] sEndRecord = tRepData.GetRecordFrom("END", 1);
            string[] sItemsRecord = tRepData.GetRecordFrom("NOITEM", 1);
            string[] sTransRecord = tRepData.GetRecordFrom("NOTRAN", 1);

            SendLineToPrinter(RightAlignStringOnExistingString("START", FixFloatError((float)Convert.ToDecimal(sStartRecord[3]) * 100).ToString()));
            SendLineToPrinter(RightAlignStringOnExistingString("END", FixFloatError((float)Convert.ToDecimal(sEndRecord[3]) * 100).ToString()));
            SendLineToPrinter(RightAlignStringOnExistingString("ITEMS", Convert.ToInt32(sItemsRecord[2]).ToString()));
            SendLineToPrinter(RightAlignStringOnExistingString("TRANS", Convert.ToInt32(sTransRecord[2]).ToString()));

            //Second Section (Money in Till)
            PrintBreaker();
            SendLineToPrinter(RightAlignStringOnExistingString("MONEY IN TILL", "AMOUNT"));
            PrintBreaker();
            // Could use GetAmountOfMoneyInTill here, but it doesn't tell me quantities

            float fTotalMoneyInTill = 0.0f;
            int nNumberOfPaymentsInTill = 0;
            if (tRepData.SearchForRecord("CASH", "REPCODE"))
            {
                string[] sCashRecord = tRepData.GetRecordFrom("CASH", 1);
                SendLineToPrinter(RightAlignStringOnExistingString("CASH        " + sCashRecord[2].TrimEnd('\0'), FormatMoneyForDisplay((float)Convert.ToDecimal(sCashRecord[3]))));
                fTotalMoneyInTill += fTotalMoneyInTill = (float)Convert.ToDecimal(sCashRecord[3]);
                nNumberOfPaymentsInTill += Convert.ToInt32(sCashRecord[2]);
            }
            if (tRepData.SearchForRecord("CRCD", "REPCODE"))
            {
                string[] sCCardRecord = tRepData.GetRecordFrom("CRCD", 1);
                fTotalMoneyInTill += (float)Convert.ToDecimal(sCCardRecord[3]);
                nNumberOfPaymentsInTill += Convert.ToInt32(sCCardRecord[2]);
                SendLineToPrinter(RightAlignStringOnExistingString("CRSLP       " + sCCardRecord[2].TrimEnd('\0'), FormatMoneyForDisplay((float)Convert.ToDecimal(sCCardRecord[3]))));
            }
            if (tRepData.SearchForRecord("CHEQ", "REPCODE"))
            {
                string[] sChequeRecord = tRepData.GetRecordFrom("CHEQ", 1);
                SendLineToPrinter(RightAlignStringOnExistingString("CHEQUES     " + sChequeRecord[2].TrimEnd('\0'), FormatMoneyForDisplay((float)Convert.ToDecimal(sChequeRecord[3]))));
                fTotalMoneyInTill += (float)Convert.ToDecimal(sChequeRecord[3]);
                nNumberOfPaymentsInTill += Convert.ToInt32(sChequeRecord[2]);
            }
            if (tRepData.SearchForRecord("VOUC", "REPCODE"))
            {
                string[] sVoucherRecord = tRepData.GetRecordFrom("VOUC", 1);
                SendLineToPrinter(RightAlignStringOnExistingString("VOUCHERS    " + sVoucherRecord[2].TrimEnd('\0'), FormatMoneyForDisplay((float)Convert.ToDecimal(sVoucherRecord[3]))));
                fTotalMoneyInTill += (float)Convert.ToDecimal(sVoucherRecord[3]);
                nNumberOfPaymentsInTill += Convert.ToInt32(sVoucherRecord[2]);
            }
            PrintBreaker();
            SendLineToPrinter(RightAlignStringOnExistingString("TOTAL       " + nNumberOfPaymentsInTill.ToString(), FormatMoneyForDisplay(fTotalMoneyInTill)));

            // Third Section, Credit card slip analysis

            PrintBreaker();
            SendLineToPrinter("CREDIT CARD SLIP ANALYSIS");
            PrintBreaker();
            string[] sPossibleCards = GetCreditCards();
            float fTotalAmountCreditCard = 0.0f;
            int nTotalNumberOfCardTransactions = 0;
            for (int i = 0; i < sPossibleCards.Length; i++)
            {
                if (tRepData.SearchForRecord("CRD" + (i+1).ToString(), "REPCODE"))
                {
                    string[] sCardData = tRepData.GetRecordFrom("CRD" + (i+1).ToString(), 1);
                    int nOfTimesUsed = Convert.ToInt32(sCardData[2]);
                    fTotalAmountCreditCard = FixFloatError(fTotalAmountCreditCard + (float)Convert.ToDecimal(sCardData[3].TrimEnd('\0')));
                    string sAmount = FormatMoneyForDisplay((float)Convert.ToDecimal(sCardData[3].TrimEnd('\0')));
                    string sToSend = sPossibleCards[i];
                    while (sToSend.Length + nOfTimesUsed.ToString().Length < 13)
                        sToSend += " ";
                    sToSend += nOfTimesUsed.ToString();
                    nTotalNumberOfCardTransactions += nOfTimesUsed;
                    SendLineToPrinter(RightAlignStringOnExistingString(sToSend, sAmount));
                }
                else
                {
                    int nOfTimesUsed = 0;
                    string sAmount = "0.00";
                    string sToSend = sPossibleCards[i];
                    while (sToSend.Length + nOfTimesUsed.ToString().Length < 13)
                        sToSend += " ";
                    sToSend += nOfTimesUsed.ToString();
                    SendLineToPrinter(RightAlignStringOnExistingString(sToSend, sAmount));
                }
            }
            PrintBreaker();
            SendLineToPrinter(RightAlignStringOnExistingString("TOTAL       " + nTotalNumberOfCardTransactions.ToString(), FormatMoneyForDisplay(fTotalAmountCreditCard)));

            // Fourth Section, Sales for V.A.T.
            PrintBreaker();
            SendLineToPrinter("SALES FOR V.A.T.");
            PrintBreaker();
            float fTotalAmountOfVATGross = 0.0f;
            string[] sCharge, sGeneralRefund, sReceivedOnAc;
            if (tRepData.SearchForRecord("STOCK", "REPCODE"))
            { // The first two use incorrect variable names, but it was quicker like this :)
                sCharge = tRepData.GetRecordFrom("STOCK", 1);
                SendLineToPrinter(RightAlignStringOnExistingString("STOCK       " + sCharge[2].TrimEnd('\0').Trim(), FormatMoneyForDisplay(FixFloatError((float)Convert.ToDecimal(sCharge[3])))));
                fTotalAmountOfVATGross = FixFloatError(fTotalAmountOfVATGross + (float)Convert.ToDecimal(sCharge[3]));
            }
            if (tRepData.SearchForRecord("NSTCK", "REPCODE"))
            {
                sCharge = tRepData.GetRecordFrom("NSTCK", 1);
                SendLineToPrinter(RightAlignStringOnExistingString("N. STOCK    " + sCharge[2].TrimEnd('\0').Trim(), FormatMoneyForDisplay(FixFloatError((float)Convert.ToDecimal(sCharge[3])))));
                fTotalAmountOfVATGross = FixFloatError(fTotalAmountOfVATGross + (float)Convert.ToDecimal(sCharge[3]));
            }
            if (tRepData.SearchForRecord("CHRG", "REPCODE"))
            {
                sCharge = tRepData.GetRecordFrom("CHRG", 1);
                SendLineToPrinter(RightAlignStringOnExistingString("CHARGE      " + sCharge[2].TrimEnd('\0'), FormatMoneyForDisplay(FixFloatError((float)Convert.ToDecimal(sCharge[3])))));
                fTotalAmountOfVATGross = FixFloatError(fTotalAmountOfVATGross + (float)Convert.ToDecimal(sCharge[3]));
            }
            if (tRepData.SearchForRecord("GREF", "REPCODE"))
            {
                sGeneralRefund = tRepData.GetRecordFrom("GREF", 1);
                SendLineToPrinter(RightAlignStringOnExistingString("G.REFUND    " + sGeneralRefund[2].TrimEnd('\0'), FormatMoneyForDisplay(FixFloatError((float)Convert.ToDecimal(sGeneralRefund[3])))));
                fTotalAmountOfVATGross = FixFloatError(fTotalAmountOfVATGross + (float)Convert.ToDecimal(sGeneralRefund[3]));
            }
            if (tRepData.SearchForRecord("RONA", "REPCODE"))
            {
                sReceivedOnAc = tRepData.GetRecordFrom("RONA", 1);
                SendLineToPrinter(RightAlignStringOnExistingString("RC ON AC    " + sReceivedOnAc[2].TrimEnd('\0'), FormatMoneyForDisplay(FixFloatError((float)Convert.ToDecimal(sReceivedOnAc[3])))));
                fTotalAmountOfVATGross = FixFloatError(fTotalAmountOfVATGross + (float)Convert.ToDecimal(sReceivedOnAc[3]));
            }
            if (tRepData.SearchForRecord("DEPO", "REPCODE"))
            {
                sReceivedOnAc = tRepData.GetRecordFrom("DEP", 1);
                SendLineToPrinter(RightAlignStringOnExistingString("DEPOSIT     " + sReceivedOnAc[2].TrimEnd('\0'), FormatMoneyForDisplay(-FixFloatError((float)Convert.ToDecimal(sReceivedOnAc[3])))));
                fTotalAmountOfVATGross = FixFloatError(fTotalAmountOfVATGross - (float)Convert.ToDecimal(sReceivedOnAc[3]));
            }
            PrintBreaker();
            float fTotalVATGross = 0.0f;
            if (tRepData.SearchForRecord("VAT00", "REPCODE"))
            {
                string[] sVATData = tRepData.GetRecordFrom("VAT00", 1);
                float fVATRate = GetVATRate(0);
                float fVATAmount = FixFloatError((float)Convert.ToDecimal(sVATData[3]));
                fTotalVATGross = FixFloatError(fTotalVATGross + fVATAmount);
                string sToPrint = "V.A.T Exempt";
                SendLineToPrinter(RightAlignStringOnExistingString(sToPrint, FormatMoneyForDisplay(FixFloatError(fVATAmount))));
            }
            if (tRepData.SearchForRecord("VAT01", "REPCODE"))
            {
                string[] sVATData = tRepData.GetRecordFrom("VAT01", 1);
                float fCurrentVATRate = GetVATRate(1);
                float fCurrentVATGross = FixFloatError((float)Convert.ToDecimal(sVATData[3]));
                fTotalVATGross = FixFloatError(fTotalVATGross + fCurrentVATGross);
                float fVATAmount = FixFloatError(fCurrentVATGross - (fCurrentVATGross / FixFloatError(1 + (fCurrentVATRate / 100))));
                string sToPrint = "V.A.T @ " + FormatMoneyForDisplay(fCurrentVATRate) + "%      " + FormatMoneyForDisplay(fVATAmount);
                SendLineToPrinter(RightAlignStringOnExistingString(sToPrint, FormatMoneyForDisplay(fCurrentVATGross)));
            }
            if (tRepData.SearchForRecord("VAT02", "REPCODE"))
            {
                string[] sVATData = tRepData.GetRecordFrom("VAT02", 1);
                float fCurrentVATRate = GetVATRate(0);
                float fCurrentVATGross = FixFloatError((float)Convert.ToDecimal(sVATData[3]));
                fTotalVATGross = FixFloatError(fTotalVATGross + fCurrentVATGross);
                float fVATAmount = FixFloatError(fCurrentVATGross - (fCurrentVATGross / FixFloatError(1 + (fCurrentVATRate / 100))));
                string sToPrint = "V.A.T (Z0) @ " + FormatMoneyForDisplay(fCurrentVATRate) + "%      " + FormatMoneyForDisplay(fVATAmount);
                SendLineToPrinter(RightAlignStringOnExistingString(sToPrint, FormatMoneyForDisplay(fCurrentVATGross)));
            }
            PrintBreaker();
            SendLineToPrinter(RightAlignStringOnExistingString("TOTAL", FormatMoneyForDisplay(fTotalVATGross)));
            PrintBreaker();

            // Fith section - Expenditure

            SendLineToPrinter("EXPENDITURE");
            PrintBreaker();
            float fTotalExpenditure = 0.0f;
            if (tRepData.SearchForRecord("GREF", "REPCODE"))
            {
                string[] sGeneralRefundData = tRepData.GetRecordFrom("GREF", 1);
                string sToPrint = "G.REFUND    " + sGeneralRefundData[2];
                float fAmountRefunded = FixFloatError((float)Convert.ToDecimal(sGeneralRefundData[3]));
                fTotalExpenditure += fAmountRefunded;
                SendLineToPrinter(RightAlignStringOnExistingString(sToPrint, FormatMoneyForDisplay(fAmountRefunded)));
            }
            if (tRepData.SearchForRecord("SREF", "REPCODE"))
            {
                string[] sGeneralRefundData = tRepData.GetRecordFrom("SREF", 1);
                string sToPrint = "S.REFUND    " + sGeneralRefundData[2];
                float fAmountRefunded = FixFloatError((float)Convert.ToDecimal(sGeneralRefundData[3]));
                fTotalExpenditure += fAmountRefunded;
                SendLineToPrinter(RightAlignStringOnExistingString(sToPrint, FormatMoneyForDisplay(fAmountRefunded)));
            }

            PrintBreaker();
            fTotalExpenditure = FixFloatError(fTotalExpenditure);
            if (fTotalExpenditure != 0.0f)
            {
                SendLineToPrinter(RightAlignStringOnExistingString("TOTAL", FormatMoneyForDisplay(fTotalExpenditure)));
                PrintBreaker();
            }
            
            // Sixth section - no title??

            int nOfTimesTillDrawOpened = 0;
            if (tRepData.SearchForRecord("NOSA", "REPCODE"))
            {
                nOfTimesTillDrawOpened = Convert.ToInt32(tRepData.GetRecordFrom("NOSA", 1)[2]);
            }
            SendLineToPrinter("NO SALES    " + nOfTimesTillDrawOpened.ToString());
            SendLineToPrinter("");
            int nOfChargeToAccounts = 0;
            string[,] sChargeToAccountDetails = tRepData.SearchAndGetAllMatchingRecords(0, "CA", ref nOfChargeToAccounts);
            for (int i = 0; i < nOfChargeToAccounts; i++)
            {
                string sToPrint = "";
                if (i == 0)
                    sToPrint = "CHRG ACC    ";
                else
                    sToPrint = "            ";
                string sRepCodeData = sChargeToAccountDetails[i, 1].TrimEnd('\0');
                string sAccountCode = "", sTransactionNumber = "";
                for (int x = 0; x < 6; x++)
                {
                    sAccountCode += sRepCodeData[x];
                }
                for (int x = 6; x < sRepCodeData.Length; x++)
                {
                    sTransactionNumber += sRepCodeData[x];
                }
                sToPrint += sAccountCode.TrimEnd(' ') + "(" + sAccountCode.TrimEnd(' ') + ")";
                float fAmountChargedToAcc = FixFloatError((float)Convert.ToDecimal(sChargeToAccountDetails[i, 3]));
                sToPrint = RightAlignStringOnExistingString(sToPrint, FormatMoneyForDisplay(fAmountChargedToAcc));
                SendLineToPrinter(sToPrint);
            }

            sChargeToAccountDetails = tRepData.SearchAndGetAllMatchingRecords(0, "RA", ref nOfChargeToAccounts);
            for (int i = 0; i < nOfChargeToAccounts; i++)
            {
                string sToPrint = "";
                if (i == 0)
                {
                    sToPrint = "RC ON AC    ";
                    SendLineToPrinter("");
                }
                else
                    sToPrint = "            ";
                string sRepCodeData = sChargeToAccountDetails[i, 1].TrimEnd('\0');
                string sAccountCode = "", sTransactionNumber = "";
                for (int x = 0; x < 6; x++)
                {
                    sAccountCode += sRepCodeData[x];
                }
                for (int x = 6; x < sRepCodeData.Length; x++)
                {
                    sTransactionNumber += sRepCodeData[x];
                }
                sToPrint += sAccountCode.TrimEnd(' ') + "(" + sAccountCode.TrimEnd(' ') + ")";
                float fAmountChargedToAcc = FixFloatError((float)Convert.ToDecimal(sChargeToAccountDetails[i, 3]));
                sToPrint = RightAlignStringOnExistingString(sToPrint, FormatMoneyForDisplay(fAmountChargedToAcc));
                SendLineToPrinter(sToPrint);
            }
            if (tRepData.SearchForRecord("CAPO", "REPCODE"))
            {
                string[] sMoneyPaidOut = tRepData.GetRecordFrom("CAPO", 1);
                string sToPrint = "PAID OUT    " + sMoneyPaidOut[2];
                float fAmountPaidOut = FixFloatError(-(float)Convert.ToDecimal(sMoneyPaidOut[3]));
                fTotalExpenditure += fAmountPaidOut;
                SendLineToPrinter(RightAlignStringOnExistingString(sToPrint, FormatMoneyForDisplay(fAmountPaidOut)));
            }
            else
            {
                string sToPrint = "PAID OUT    0";
                SendLineToPrinter(RightAlignStringOnExistingString(sToPrint, "0.00"));
            }

            // Seventh section - Declared
            PrintBreaker();
            SendLineToPrinter("DECLARED");
            PrintBreaker();

            float fFloatAmount = FixFloatError((float)Convert.ToDecimal(tRepData.GetRecordFrom("FLOT", 1)[3]));
            SendLineToPrinter(RightAlignStringOnExistingString("FLOAT", FormatMoneyForDisplay(fFloatAmount)));

            if (tRepData.SearchForRecord("CASH", "REPCODE"))
            {
                float fCashAmount = FixFloatError((float)Convert.ToDecimal(tRepData.GetRecordFrom("CASH", 1)[3]));
                SendLineToPrinter(RightAlignStringOnExistingString("CASH", FormatMoneyForDisplay(fCashAmount)));
            }
            if (tRepData.SearchForRecord("CHEQUES", "REPCODE"))
            {
                float fCashAmount = FixFloatError((float)Convert.ToDecimal(tRepData.GetRecordFrom("CHEQ", 1)[3]));
                SendLineToPrinter(RightAlignStringOnExistingString("CHQS", FormatMoneyForDisplay(fCashAmount)));
            }

            string[] sCreditCards = GetCreditCards();
            for (int i = 0; i < sCreditCards.Length; i++)
            {
                float fCardAmount = 0.0f;
                if (tRepData.SearchForRecord("CRD" + (i + 1).ToString(), "REPCODE"))
                {
                    fCardAmount = FixFloatError((float)Convert.ToDecimal(tRepData.GetRecordFrom("CRD" + (i + 1).ToString(), 1)[3]));
                }
                SendLineToPrinter(RightAlignStringOnExistingString(sCreditCards[i], FormatMoneyForDisplay(fCardAmount)));
            }

            if (tRepData.SearchForRecord("VOUC", "REPCODE"))
            {
                float fVoucherAmount = FixFloatError((float)Convert.ToDecimal(tRepData.GetRecordFrom("VOUC", 1)[3]));
                SendLineToPrinter(RightAlignStringOnExistingString("VOUCHERS", FormatMoneyForDisplay(fVoucherAmount)));
            }
            PrintBreaker();
            SendLineToPrinter("END OF REPORT");
            PrintBreaker();

            for (int i = 0; i < GTill.Properties.Settings.Default.nLinesAfterReigsterReport; i++)
            {
                AddReceiptSpacingToBuffer();
            }

            PrintReceiptHeader();
            EmptyPrinterBuffer();

        }

        private string DateTimeForReceiptFooter()
        {
            string sMinute = DateTime.Now.Minute.ToString();
            if (DateTime.Now.Minute < 10)
                sMinute = "0" + sMinute;
            string sToReturn = DateTime.Now.Day + "/" + DateTime.Now.Month + "/" + DateTime.Now.Year
                + " " + DateTime.Now.Hour + ":" + sMinute;
            return sToReturn;
        }

        private void PrintTransactionVoidReceipt(int nTransactionNumber, bool bReprintReceipt)
        {
            SendLineToPrinter(CentralisePrinterText("TRANSACTION " + nTransactionNumber.ToString() + " VOID"));
            if (bReprintReceipt)
                PrintReprintReceiptNote();
            PrintReceiptFooter(GetCurrentStaffName(), DateTimeForReceiptFooter(), nTransactionNumber.ToString());
            PrintReceiptHeader();
            EmptyPrinterBuffer();
        }

        public void OpenTillDrawer(bool bNoSale)
        {
            if (!GTill.Properties.Settings.Default.bUseVirtualPrinter)
            {
                char[] cBell = { (char)7 };
                IntPtr ptr = CreateFile(sPortName, GENERIC_WRITE, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
                if (ptr.ToInt32() == -1)
                {
                    // Error! Invalid Handle Value
                    ErrorHandler.LogError("Invalid Handle Value in OpenTillDrawer procedure");
                    throw new NotSupportedException("Invalid Handle Value in OpenTillDrawer procedure");
                }
                else
                {
                    fsOutput = new FileStream(ptr, FileAccess.Write);
                    byte[] drawerBuffer;
                    drawerBuffer = Encoding.ASCII.GetBytes(cBell);
                    fsOutput.Write(drawerBuffer, 0, drawerBuffer.Length);
                    fsOutput.Close();

                    if (bNoSale)
                    {
                        // Update the NOSA record in RepData

                        int nRecordLocation = 0;
                        if (!tRepData.SearchForRecord("NOSA", 1, ref nRecordLocation))
                        {
                            string[] sNoSale = { "CR", "NOSA", "0", "0", "0" };
                            tRepData.AddRecord(sNoSale);
                            tRepData.SearchForRecord("NOSA", 1, ref nRecordLocation);
                        }

                        int nCurrentNoSales = Convert.ToInt32(tRepData.GetRecordFrom(nRecordLocation)[2]);
                        nCurrentNoSales++;
                        tRepData.EditRecordData(nRecordLocation, 2, nCurrentNoSales.ToString());
                        tRepData.SaveToFile("REPDATA.DBF");
                    }
                }
            }
        }

        private void AddReceiptSpacingToBuffer()
        {
            for (int i = 0; i < GTill.Properties.Settings.Default.nSpacesBetweenReceipts; i++)
            {
                SendLineToPrinter(" ");
            }
        }
    }
}