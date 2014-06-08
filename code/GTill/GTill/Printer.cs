using System;
using DBFDetailsViewerV2;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using GTill;

namespace TillEngine
{
    /// <summary>
    ///An extension of the till engine, just so it's more convenient to program
    /// Contains all the printer code
    /// </summary>
    public partial class TillEngine
    {
        /// <summary>
        /// The virtual printer
        /// </summary>
        frmReceiptDisplay frdShowReceipt;

        /// <summary>
        /// Initialises the virtual printer
        /// </summary>
        private void InitialiseFakePrinter()
        {
            frdShowReceipt = new frmReceiptDisplay();
            frdShowReceipt.Show();
        }
        /// <summary>
        /// Sends a line of text to the printer
        /// </summary>
        /// <param name="sText">The text to send</param>
        private void SendLineToPrinter(string sText)
        {
            string[] sToAdd = { sText };
            SendLinesToPrinter(sToAdd);
        }
        /// <summary>
        /// Sends an array of lines of text to the printer
        /// </summary>
        /// <param name="sLines">The lines of text to send</param>
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
        /// <summary>
        /// Empties the printer buffer, and actually sends the data in the buffer to the printer
        /// </summary>
        public void EmptyPrinterBuffer()
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
                    try
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
                            bool bSuccessFullPrint = true;
                            do
                            {
                                try
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
                                catch
                                {
                                    bSuccessFullPrint = false;
                                    if (System.Windows.Forms.MessageBox.Show("An error has occured whilst printed. Check that the printer is online and has paper. Do you want to try and print again?", "Error", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                                    {
                                        bSuccessFullPrint = true;
                                    }
                                }
                            } while (!bSuccessFullPrint);
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.LogError("Error whilst trying to print - " + ex.Message.ToString());
                        System.Windows.Forms.MessageBox.Show("Error whilst trying to print! Please ensure that the printer is switched on, and online.");

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
        
        /// <summary>
        /// Variables for the P/Invoke to send data to the parallel port
        /// </summary>
        uint GENERIC_WRITE = 0x40000000, OPEN_EXISTING = 3;
        /// <summary>
        /// The name of the port to send data to
        /// </summary>
        string sPortName = GTill.Properties.Settings.Default.sPrinterOutputPort;
        /// <summary>
        /// The maximum width of a string that the printer can handle
        /// </summary>
        int nPrinterWidth = GTill.Properties.Settings.Default.nPrinterCharWidth;
        /// <summary>
        /// The character to use to break up sections of a receipt
        /// </summary>
        const char cReceiptBreaker = '-';
        /// <summary>
        /// A FileStream to output data to the real parallel port
        /// </summary>
        FileStream fsOutput;
        /// <summary>
        /// A buffer of data that needs to be sent to the printer
        /// </summary>
        Byte[] bBuffer = new Byte[2048];
        /// <summary>
        /// The printer buffer, which contains lines of text ready to send to the printer
        /// </summary>
        string[] sPrinterBuffer = new string[50];
        /// <summary>
        /// The position in the buffer of the next item to print
        /// </summary>
        int nBufferPos = 0;

        /// <summary>
        /// The P/Invoke to a Windows feature to send data to the parallel port
        /// </summary>
        [DllImport("kernel32.dll", SetLastError=true)]
        static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
        
        /// <summary>
        /// Enables or disables the printer
        /// </summary>
        public void TogglePrinterStatus()
        {
            bPrinterEnabled = !bPrinterEnabled;
        }

        /// <summary>
        /// Gets whether or not the printer is enabled
        /// </summary>
        public bool PrinterEnabled
        {
            get
            {
                return bPrinterEnabled;
            }
        }

        public void PrintTestBarcode(string sInput)
        {
            IntPtr ptr = CreateFile(sPortName, GENERIC_WRITE, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            if (ptr.ToInt32() == -1)
            {
                // Error! Invalid Handle Value
                ErrorHandler.LogError("Error whilst sending data to " + sPortName + ". Disable the printer or enable the virtual printer as a temporary solution.");
                throw new NotSupportedException("Error whilst sending data to " + sPortName + ". Disable the printer or enable the virtual printer as a temporary solution.");
            }
            else
            {
                string[] hexSplit = sInput.Trim().Split(' ');
                byte[] bToPrint = new byte[hexSplit.Length];
                for (int i = 0; i < bToPrint.Length; i++)
                {
                    bToPrint[i] = (byte)Convert.ToInt32(hexSplit[i], 16);
                }
                fsOutput = new FileStream(ptr, FileAccess.Write);
                fsOutput.Write(bToPrint, 0, bToPrint.Length);

                fsOutput.Close();
                System.Windows.Forms.MessageBox.Show("Done - " + sInput);
            }
        }

        /// <summary>
        /// Prints a barcode on a Star TSP-200 thermal printer
        /// </summary>
        /// <param name="sBarcode">The barcode to generate</param>
        public byte[] PrintBarcode(string sBarcode, bool bReturnBytesInstead)
        {
            if ((bPrinterEnabled/* && !GTill.Properties.Settings.Default.bUseVirtualPrinter*/) || bReturnBytesInstead)
            {

                if (!bReturnBytesInstead)
                {
                    // Get the details of the item first
                    Item iItem = GetItemAsItemClass(sBarcode);

                    // Print the item details
                    SendLineToPrinter("");

                    SendLineToPrinter(CentralisePrinterText(iItem.Description + " - " + FormatMoneyForDisplay(iItem.Amount)));

                    SendLineToPrinter("");

                    EmptyPrinterBuffer();
                }

                // Add all bytes to a list to start with
                System.Collections.Generic.List<Byte> bToPrint = new System.Collections.Generic.List<byte>();

                bToPrint.Add(0x1B); // Required
                bToPrint.Add(0x62); // Required

                // Now set to to EAN-8 (0x32) or EAN-13 (0x33)
                if (sBarcode.Length == 7 || sBarcode.Length == 8)
                {
                    bToPrint.Add(0x32);
                }
                else
                {
                    bToPrint.Add(0x33);
                }

                // Set to 0x32 which means print the numbers under the barcode and to a line feed
                bToPrint.Add(0x32);

                // Set to 0x32 which means that the width of the barcode is the middle setting
                bToPrint.Add(0x32);

                // Set barcode height to 90 (out of 00-FF in hex)
                bToPrint.Add(0x90);

                // Convert the barcode into hex and add it
                // Doesn't matter whether or not the check digit is there - the printer will add it if it isn't.
                for (int i = 0; i < sBarcode.Length; i++)
                {
                    int nNum = 0;
                    try
                    {
                        nNum = Convert.ToInt32(sBarcode[i].ToString());
                    }
                    catch
                    {
                        System.Windows.Forms.MessageBox.Show("Can't print the barcode as it contains non-numerical characters.");
                        return new byte[0];
                    }
                    nNum += 30;
                    bToPrint.Add((byte)Convert.ToInt32(nNum.ToString(), 16));
                }

                // Add the final character, 1E which signals the end of the barcode
                bToPrint.Add(0x1E);

                if (bReturnBytesInstead)
                    return bToPrint.ToArray();

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
                    fsOutput.Write(bToPrint.ToArray(), 0, bToPrint.Count);
                    fsOutput.Close();
                }

                // Add a space at the end
                SendLineToPrinter("");
                SendLineToPrinter("");

                // Print the receipt header ready for the next receipt
                PrintReceiptHeader();
                EmptyPrinterBuffer();

                return bToPrint.ToArray();
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Either the printer is disabled or you are using a virtual printer. Turn the printer on (option at the top of this menu), or go to the settings (Admin Menu -> Configure GTill) to switch off the virtual printer.");
            }
            return null;
        }

        /// <summary>
        /// Expands the size of the printer buffer incase 50 lines isn't enough
        /// </summary>
        private void ExpandPrinterBufferSize()
        {
            string[] sOldBuffer = sPrinterBuffer;
            sPrinterBuffer = new string[sOldBuffer.Length + 50];
            for (int i = 0; i < sOldBuffer.Length; i++)
            {
                sPrinterBuffer[i] = sOldBuffer[i];
            }
        }

        /// <summary>
        /// Centralises text to send to the printer
        /// </summary>
        /// <param name="sToCentralise">The text to centralise</param>
        /// <returns>Text with padding at either side so it's centralised</returns>
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

        /// <summary>
        /// Puts text on the right hand side of an existing string with already on
        /// </summary>
        /// <param name="sExisting">The existing string</param>
        /// <param name="sToRightAlign">The string to add</param>
        /// <returns>The resulting string</returns>
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

        /// <summary>
        /// Right aligns the whole string
        /// </summary>
        /// <param name="sExisting">The string to right align</param>
        /// <returns>The right alinged string</returns>
        private string RightAlignWholeString(string sExisting)
        {
            while (sExisting.Length < nPrinterWidth)
                sExisting = " " + sExisting;
            return sExisting;
        }

        /// <summary>
        /// Prints a receipt header
        /// </summary>
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

        /// <summary>
        /// Prints a receipt footer
        /// </summary>
        /// <param name="sUserName">The name of the current user</param>
        /// <param name="sDateTime">The date and time to print</param>
        /// <param name="sTransactionNumber">The transaction number</param>
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

        /// <summary>
        /// Prints the column titles on the receipt
        /// </summary>
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

        /// <summary>
        /// Prints an item and its price on the receipt
        /// </summary>
        /// <param name="iItem">The item object to print</param>
        public void PrintItem(Item iItem)
        {
            string sDescLine = iItem.Description;
            string sQuantityLine = "";
            string sDiscountLine = "";
            bool bMultipleQuantities = false, bDiscount = false;
            if (iItem.Quantity != 1)
                bMultipleQuantities = true;
            if (FixFloatError(iItem.Quantity * iItem.GrossAmount) > iItem.Amount)
                bDiscount = true;
            if (bMultipleQuantities)
            {
                sQuantityLine = "QUANTITY : " + iItem.Quantity.ToString() + " @ " + FormatMoneyForDisplay(iItem.Amount / iItem.Quantity);
                string sFormattedMoney = "";
                if (!bDiscount)
                {
                    sFormattedMoney = FormatMoneyForDisplay(iItem.Amount);
                    while (sFormattedMoney.Length < 7) // Allows upto 9999.99
                        sFormattedMoney = " " + sFormattedMoney;
                }
                else
                {
                    sFormattedMoney = "        ";
                }
                    sQuantityLine = RightAlignWholeString(sQuantityLine + " " + sFormattedMoney);
            }
            if (FixFloatError(iItem.Quantity * iItem.GrossAmount) > iItem.Amount)
                sDiscountLine = RightAlignWholeString("DISCOUNT : " + (FormatMoneyForDisplay(FixFloatError((iItem.GrossAmount * iItem.Quantity) - iItem.Amount)) + "     " + FormatMoneyForDisplay(iItem.Amount)).ToString());
            if (sQuantityLine == "" && sDiscountLine == "")
            {
                sDescLine = RightAlignStringOnExistingString(sDescLine, FormatMoneyForDisplay(iItem.Amount));
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
        /// <summary>
        /// Prints and item and its price on a receipt
        /// </summary>
        /// <param name="sItemInfo">An array of data with info about the item</param>
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
                sQuantityLine = "QUANTITY : " + nQuantity.ToString() + " @ " + FormatMoneyForDisplay(fPricePaid / nQuantity);
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

        /// <summary>
        /// Prints a payment method
        /// </summary>
        /// <param name="pmMethod">A payment method object</param>
        public void PrintPaymentMethod(PaymentMethod pmMethod)
        {
            string sPaymentLine =  GetPaymentDescription(pmMethod.PMType).ToUpper();
            if (sPaymentLine == "CASH")
                sPaymentLine += " TENDERED";
            sPaymentLine = RightAlignStringOnExistingString(sPaymentLine, FormatMoneyForDisplay(pmMethod.Excess));
            SendLineToPrinter(sPaymentLine);
        }

        /// <summary>
        /// Prints a summary of the total amount due
        /// </summary>
        /// <param name="nOfItems">The number of items in the transaction</param>
        /// <param name="fTotalDue">The total amount that is due</param>
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

        /// <summary>
        /// Gets the description of a payment based on its code
        /// </summary>
        /// <param name="sPaymentCode">The code of the payment</param>
        /// <returns>The description of the payment method</returns>
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
                return "CHARGED TO A/C " + sSplit[1].Trim();
            }
            // The code didn't match any descriptions
            ErrorHandler.LogError("Couldn't find the description for " + sPaymentCode + " in the printer section");
            return " * Error *";
        }

        /// <summary>
        /// Prints a line on which the customer must sign
        /// </summary>
        private void PrintSignOnDottedLine()
        {
            SendLineToPrinter("");
            SendLineToPrinter("SIGNED ..............................");
        }

        /// <summary>
        /// Gets the V.A.T. rate
        /// </summary>
        /// <param name="nVATRate">The V.A.T. rate to get</param>
        /// <returns>The V.A.T. rate</returns>
        public float GetVATRate(int nVATRate)
        {
            if (!bDemoMode)
            {
                float fVat = (float)Convert.ToDecimal(tDetails.GetRecordFrom(26 + nVATRate)[0]);
                fVat = FixFloatError(fVat);
                return fVat;
            }
            else
            {
                frmSingleInputBox fsiVAT = new frmSingleInputBox("Enter the VAT Rate to use:");
                fsiVAT.ShowDialog();
                return (float)Convert.ToDecimal(fsiVAT.Response);
            }
        }

        public float GetVATRate(string sCode)
        {
            if (!bDemoMode)
            {
                float fVAT = (float)Convert.ToDecimal(tVAT.GetRecordFrom(sCode, 0, true)[2]);
                fVAT = FixFloatError(fVAT);
                return fVAT;
            }
            else
            {
                frmSingleInputBox fsiVAT = new frmSingleInputBox("Enter the VAT Rate to use:");
                fsiVAT.ShowDialog();
                return (float)Convert.ToDecimal(fsiVAT.Response);
            }
        }

        /// <summary>
        /// Prints the amount of V.A.T. that was included in the transaction
        /// </summary>
        public void PrintVAT()
        {
            SendLineToPrinter("V.A.T. INCLUDED");
            if (tVAT != null)
            {
                for (int i = 0; i < tVAT.NumberOfRecords; i++)
                {
                    float fTransactionTotal = tCurrentTransation.TotalAmount;

                    float fCurrentVATRate = (float)Convert.ToDecimal(tVAT.GetRecordFrom(i)[2]);

                    if (bDemoMode)
                    {
                        frmSingleInputBox fsiGetVAT = new frmSingleInputBox("Enter the VAT Rate to display");
                        fsiGetVAT.ShowDialog();
                        fCurrentVATRate = (float)Convert.ToDecimal(fsiGetVAT.Response);
                    }
                    float fCurrentVATGross = tCurrentTransation.GetAmountForVATRate(tVAT.GetRecordFrom(i)[0], true);
                    float fVATAmount = FixFloatError(fCurrentVATGross - (fCurrentVATGross / (1 + (fCurrentVATRate / 100))));
                    if (fCurrentVATGross != 0.0f)
                        PrintVATRate(fCurrentVATGross, fCurrentVATRate, fVATAmount);
                }
            }
            else
            {
                // Do for 0% VAT First
                float fTransactionTotal = tCurrentTransation.TotalAmount;
                string sCurrentVAT = "Z0";
                float fCurrentVATRate = GetVATRate(0);
                float fCurrentVATGross = tCurrentTransation.GetAmountForVATRate(sCurrentVAT, true);
                sCurrentVAT = "X0";
                fCurrentVATGross += tCurrentTransation.GetAmountForVATRate(sCurrentVAT, true);
                float fVATAmount = FixFloatError(fCurrentVATGross - (fCurrentVATGross / (1 + (fCurrentVATRate / 100))));
                if (fCurrentVATGross != 0.0f)
                    PrintVATRate(fCurrentVATGross, fCurrentVATRate, fVATAmount);

                // Now do for 17.5% V.A.T. (or whatever the current V.A.T. rate is)
                sCurrentVAT = "I1";
                fCurrentVATRate = GetVATRate(1);
                fCurrentVATGross = FixFloatError(tCurrentTransation.GetAmountForVATRate(sCurrentVAT, true) + tCurrentTransation.GetAmountForVATRate("E1", true));
                fVATAmount = FixFloatError(fCurrentVATGross - (fCurrentVATGross / (1 + (fCurrentVATRate / 100))));
                if (fCurrentVATGross != 0.0f)
                    PrintVATRate(fCurrentVATGross, fCurrentVATRate, fVATAmount);
            }
        }
        public void PrintVAT(float[] fRates, float[] fGrossAmounts)
        {
            SendLineToPrinter("V.A.T. INCLUDED");
            for (int i = 0; i < fRates.Length; i++)
            {
                float fNet = 1 + (fRates[i] / 100);
                float fVATAmount = fGrossAmounts[i] - (fGrossAmounts[i] / fNet);
                fVATAmount = FixFloatError(fVATAmount);
                if (fGrossAmounts[i] != 0)
                    PrintVATRate(fGrossAmounts[i], fRates[i], fVATAmount);
            }
        }
        /*
        /// <summary>
        /// Prints the amount of V.A.T. that was included
        /// </summary>
        /// <param name="fNoVATAmount">The amount of zero rated V.A.T.</param>
        /// <param name="fNormalVATAmount">The amount of normal V.A.T.</param>
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
        }*/
        /// <summary>
        /// Prints the amount of V.A.T. that was included
        /// </summary>
        /// <param name="fGross">The gross amount of the vat-able products</param>
        /// <param name="fVATRate">The V.A.T. rate that is being printed</param>
        /// <param name="fVATAmount">The amount of V.A.T. for the transaction</param>
        private void PrintVATRate(float fGross, float fVATRate, float fVATAmount)
        {
            string sVATLine = "       ";
            sVATLine += FormatMoneyForDisplay(fGross);
            sVATLine += " @ " + FormatMoneyForDisplay(fVATRate);
            sVATLine += " % V.A.T. = ";
            sVATLine = RightAlignStringOnExistingString(sVATLine, FormatMoneyForDisplay(fVATAmount));
            SendLineToPrinter(sVATLine);
        }

        /// <summary>
        /// Prints the amount of change that is due
        /// </summary>
        public void PrintChangeDue()
        {
            string sChangeDue = "CHANGE";
            sChangeDue = RightAlignStringOnExistingString(sChangeDue, FormatMoneyForDisplay(GetChangeDue()));
            SendLineToPrinter(sChangeDue);
        }
        /// <summary>
        /// Prints the amount of change that is due
        /// </summary>
        /// <param name="fAmountDue">The amount of change due</param>
        public void PrintChangeDue(float fAmountDue)
        {
            string sChangeDue = "CHANGE";
            sChangeDue = RightAlignStringOnExistingString(sChangeDue, FormatMoneyForDisplay(fAmountDue));
            SendLineToPrinter(sChangeDue);
        }

        /// <summary>
        /// Reprints a receipt from a previous transaction
        /// </summary>
        /// <param name="nTransactionNumber">The transaction number to print the receipt from</param>
        public void ReprintTransactionReceipt(int nTransactionNumber)
        {
            string[,] sTransactionInfo = GetTransactionInfo(nTransactionNumber.ToString());
            // First array element in format { NumberOfItems, NumberOfPaymentMethods, TransactionDateTime, SpecialTransaction }
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
                PrintGeneralRefund(pm, nTransactionNumber);
            }
            else if (sSpecialTransaction == "RECEIVEDONACCOUNT")
            {
                string[] sToSend = { "", CentralisePrinterText("No transaction to reprint."), "", "", CentralisePrinterText("Till re-written by Thomas Wormald.") };
                SendLinesToPrinter(sToSend);
                PrintReceiptFooter("Thomas", DateTimeForReceiptFooter(), "");
                PrintReceiptHeader();
                EmptyPrinterBuffer();
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
                if (tVAT == null)
                {
                    float fNoVat = 0.0f, fNormalVAT = 0.0f;
                    for (int i = 1; i <= nOfItems; i++)
                    {
                        Item iItem = new Item(tStock.GetRecordFrom(sTransactionInfo[i, 0], 0));
                        if (iItem.VATRate == "X0" || iItem.VATRate == "Z0")
                            fNoVat += FixFloatError((float)Convert.ToDecimal(sTransactionInfo[i, 2]));
                        else
                            fNormalVAT += FixFloatError((float)Convert.ToDecimal(sTransactionInfo[i, 2]));
                    }
                    float[] fRates = new float[2];
                    fRates[0] = GetVATRate("I1");
                    fRates[1] = 0;
                    float[] fAmount = new float[2];
                    fAmount[0] = fNormalVAT;
                    fAmount[1] = fNoVat;
                    PrintVAT(fRates, fAmount);
                }
                else
                {
                    string[] sCodes = GetVATCodes();
                    float[] fRates = GetVATRates();
                    float[] fAmount = new float[fRates.Length];
                    for (int i = 1; i <= nOfItems; i++)
                    {
                        Item iItem = new Item(tStock.GetRecordFrom(sTransactionInfo[i, 0], 0));
                        for (int x = 0; x < sCodes.Length; x++)
                        {
                            if (iItem.ItemCategory != 6)
                            {
                                if (sCodes[x] == iItem.VATRate)
                                {
                                    fAmount[x] += FixFloatError((float)Convert.ToDecimal(sTransactionInfo[i, 2]));
                                    break;
                                }
                            }
                            else
                            {
                                if (sCodes[x] == "X0")
                                    fAmount[x] += FixFloatError((float)Convert.ToDecimal(sTransactionInfo[i, 2]));
                            }
                        }
                    }
                    PrintVAT(fRates, fAmount);
                }
                PrintReprintReceiptNote();
                string[] sFooterData = ReturnSensibleDateTimeString(sDateTime);
                PrintReceiptFooter(sFooterData[1], sFooterData[0], nTransactionNumber.ToString());
                PrintReceiptHeader();
                EmptyPrinterBuffer();
            }
        }

        string[] GetVATCodes()
        {
            string[] sCodes = new string[tVAT.NumberOfRecords];
            for (int i = 0; i < sCodes.Length; i++)
            {
                sCodes[i] = tVAT.GetRecordFrom(i)[0];
            }
            return sCodes;
        }

        float[] GetVATRates()
        {
            float[] fRates = new float[tVAT.NumberOfRecords];
            for (int i = 0; i < tVAT.NumberOfRecords; i++)
            {
                fRates[i] = FixFloatError((float)Convert.ToDecimal(tVAT.GetRecordFrom(i)[2]));
            }
            return fRates;
        }

        /// <summary>
        /// Prints a note saying that the receipt is a reprint
        /// </summary>
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

        /// <summary>
        /// Formats the date and time from tHeader
        /// </summary>
        /// <param name="sInput">The date and time as found in tHeader</param>
        /// <returns>A nicely formatted date and time string</returns>
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

        /// <summary>
        /// Prints out a receipt for when cash is paid out
        /// </summary>
        /// <param name="fAmountPaidOut"></param>
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
        /// <summary>
        /// Prints a receipt for when cash is paid out
        /// </summary>
        /// <param name="fAmountPaidOut">The amount paid out</param>
        /// <param name="sDateTime">The date and time which it was paid out</param>
        /// <param name="sStaffName">The name of the staff member that paid the money out</param>
        /// <param name="nTransactionNumber">The transaction number of the cash paid out</param>
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

        public void PrintReceivedOnAccount(string sAccountCode, float fAmountReceived, string sPaymentMethod)
        {
            SendLineToPrinter(CentralisePrinterText("RECEIVED ON ACCOUNT"));
            PrintBreaker();
            SendLineToPrinter("Account: " + sGetAccountDetailsFromCode(sAccountCode)[2] + " (" + sAccountCode + ")");
            SendLineToPrinter("Amount Received : " + FormatMoneyForDisplay(fAmountReceived));
            SendLineToPrinter("Payment Method : " + GetPaymentDescription(sPaymentMethod));
            PrintBreaker();
            PrintReceiptFooter(GetCurrentStaffName(), DateTimeForReceiptFooter(), "N/A");
            PrintReceiptHeader();
            EmptyPrinterBuffer();
        }

        /// <summary>
        /// Prints a receipt for a specific refund
        /// </summary>
        /// <param name="sItemDesc">The description of the item being refunded</param>
        /// <param name="fAmountRefunded">The amount that has been refunded</param>
        /// <param name="pmRefundMethod">The payment method object used to refund</param>
        /// <param name="nQuantity">The quantity of the item being refunded</param>
        /// <param name="bReprintReceipt">Whether or not this is a reprint receipt</param>
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

        /// <summary>
        /// Prints a receipt breaker to split up the receipt into different sections
        /// </summary>
        private void PrintBreaker()
        {
            string sToPrint = "";
            for (int i = 0; i < nPrinterWidth; i++)
            {
                sToPrint += cReceiptBreaker;
            }
            SendLineToPrinter(sToPrint);
        }

        /// <summary>
        /// Prints a receipt for a general refund
        /// </summary>
        /// <param name="pmPayMethod">The payment method to use</param>
        /// <param name="bReceiptReprint">Whether or not this is a reprint receipt</param>
        public void PrintGeneralRefund(PaymentMethod pmPayMethod)
        {
            PrintReceiptDescAndPriceTitles();
            SendLineToPrinter(CentralisePrinterText("GENERAL REFUND"));
            PrintBreaker();
            PrintPaymentMethod(pmPayMethod);
            SendLineToPrinter("");
            PrintSignOnDottedLine();
            PrintReceiptFooter(GetCurrentStaffName(), DateTimeForReceiptFooter(), tCurrentTransation.TransactionNumber.ToString());
            PrintReceiptHeader();
            EmptyPrinterBuffer();
        }
        public void PrintGeneralRefund(PaymentMethod pmPayMethod, int nTransactionNumber)
        {
            PrintReceiptDescAndPriceTitles();
            SendLineToPrinter(CentralisePrinterText("GENERAL REFUND"));
            PrintBreaker();
            PrintPaymentMethod(pmPayMethod);
            SendLineToPrinter("");
            PrintSignOnDottedLine();
            PrintReprintReceiptNote();
            PrintReceiptFooter(GetCurrentStaffName(), DateTimeForReceiptFooter(), nTransactionNumber.ToString());
            PrintReceiptHeader();
            EmptyPrinterBuffer();
        }

        /// <summary>
        /// Prints a register report showing inforamtion about the money taken
        /// </summary>
        public void PrintRegisterReport()
        {
            // Used for cashing up etc

            SendLineToPrinter(CentralisePrinterText("REGISTER REPORT   No: " + GetNextTransactionNumber().ToString()));
            string sHeaderLine = tsCurrentTillSettings.TillName + " for " + DateTime.Now.DayOfWeek.ToString() + " "+  DateTimeForReceiptFooter();
            SendLineToPrinter(CentralisePrinterText(sHeaderLine));
            string sSalesDate = "SALES DATE : " + tRepData.GetRecordFrom(0)[1];
            SendLineToPrinter(CentralisePrinterText(sSalesDate));
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
                string sToPrint = "CASH";
                while (sToPrint.Length + sCashRecord[2].TrimEnd('\0').Length < 14)
                    sToPrint += " ";
                SendLineToPrinter(RightAlignStringOnExistingString(sToPrint + sCashRecord[2].TrimEnd('\0'), FormatMoneyForDisplay((float)Convert.ToDecimal(sCashRecord[3]))));
                fTotalMoneyInTill += fTotalMoneyInTill = (float)Convert.ToDecimal(sCashRecord[3]);
                nNumberOfPaymentsInTill += Convert.ToInt32(sCashRecord[2]);
            }
            if (tRepData.SearchForRecord("CRCD", "REPCODE"))
            {
                string[] sCCardRecord = tRepData.GetRecordFrom("CRCD", 1);
                fTotalMoneyInTill += (float)Convert.ToDecimal(sCCardRecord[3]);
                nNumberOfPaymentsInTill += Convert.ToInt32(sCCardRecord[2]);
                string sToPrint = "CRSLP";
                while (sToPrint.Length + sCCardRecord[2].TrimEnd('\0').Length < 14)
                    sToPrint += " ";
                float fCreditCardAmount = (float)Convert.ToDecimal(sCCardRecord[3]);
                // Find out which card is BACS, if any
                int nCrdNum = 0;
                for (int i = 17; i < 26; i++)
                {
                    if (tDetails.GetRecordFrom(i)[0].TrimEnd('\0') == "BACS")
                    {
                        nCrdNum = i - 16;
                    }
                }
                float fBacs = 0.0f;
                int nOfBacs = 0;
                if (nCrdNum != 0)
                {
                    int nBacsLoc = 0;
                    if (tRepData.SearchForRecord("CRD" + nCrdNum.ToString(), 1, ref nBacsLoc))
                    {
                        float fBacsAmount = (float)Convert.ToDecimal(tRepData.GetRecordFrom(nBacsLoc)[3]);
                        fBacsAmount = FixFloatError(fBacsAmount);
                        nOfBacs += Convert.ToInt32(tRepData.GetRecordFrom(nBacsLoc)[2]);
                        string sBacsLine = "BACS";
                        while (sBacsLine.Length + nOfBacs.ToString().Length < 14)
                            sBacsLine += " ";
                        sBacsLine += nOfBacs.ToString();
                        SendLineToPrinter(RightAlignStringOnExistingString(sBacsLine, FormatMoneyForDisplay(fBacsAmount)));
                        fBacs += fBacsAmount;
                    }
                }
                SendLineToPrinter(RightAlignStringOnExistingString(sToPrint + (Convert.ToInt32(sCCardRecord[2].TrimEnd('\0')) - nOfBacs).ToString(), FormatMoneyForDisplay((float)Convert.ToDecimal(sCCardRecord[3]) - fBacs)));
            }
            if (tRepData.SearchForRecord("CHEQ", "REPCODE"))
            {
                string[] sChequeRecord = tRepData.GetRecordFrom("CHEQ", 1);
                string sToSend = "CHEQUES";
                while (sToSend.Length + sChequeRecord[2].TrimEnd('\0').Length < 14)
                    sToSend += " ";
                sToSend += sChequeRecord[2].TrimEnd('\0');

                SendLineToPrinter(RightAlignStringOnExistingString(sToSend, FormatMoneyForDisplay((float)Convert.ToDecimal(sChequeRecord[3]))));
                fTotalMoneyInTill += (float)Convert.ToDecimal(sChequeRecord[3]);
                nNumberOfPaymentsInTill += Convert.ToInt32(sChequeRecord[2]);
            }
            if (tRepData.SearchForRecord("VOUC", "REPCODE"))
            {
                string[] sVoucherRecord = tRepData.GetRecordFrom("VOUC", 1);
                string sToPrint = "VOUCHERS";
                while (sToPrint.Length + sVoucherRecord[2].Length < 14)
                    sToPrint += " ";
                SendLineToPrinter(RightAlignStringOnExistingString(sToPrint + sVoucherRecord[2].TrimEnd('\0'), FormatMoneyForDisplay((float)Convert.ToDecimal(sVoucherRecord[3]))));
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
                    while (sToSend.Length + nOfTimesUsed.ToString().Length < 14)
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
            string sTotalCard = "TOTAL";
            while (sTotalCard.Length + nTotalNumberOfCardTransactions.ToString().Length < 14)
                sTotalCard += " ";
            sTotalCard += nTotalNumberOfCardTransactions.ToString();
            SendLineToPrinter(RightAlignStringOnExistingString(sTotalCard, FormatMoneyForDisplay(fTotalAmountCreditCard)));

            // Fourth Section, Sales for V.A.T.
            PrintBreaker();
            SendLineToPrinter("SALES FOR V.A.T.");
            PrintBreaker();
            float fTotalAmountOfVATGross = 0.0f;
            string[] sCharge, sGeneralRefund, sReceivedOnAc;
            if (tRepData.SearchForRecord("STOCK", "REPCODE"))
            { // The first two use incorrect variable names, but it was quicker like this :)
                sCharge = tRepData.GetRecordFrom("STOCK", 1);
                string sToPrint = "STOCK";
                while (sToPrint.Length + sCharge[2].TrimEnd('\0').Trim().Length < 14)
                    sToPrint += " ";
                sToPrint += sCharge[2].TrimEnd('\0').Trim();
                SendLineToPrinter(RightAlignStringOnExistingString(sToPrint, FormatMoneyForDisplay(FixFloatError((float)Convert.ToDecimal(sCharge[3])))));
                fTotalAmountOfVATGross = FixFloatError(fTotalAmountOfVATGross + (float)Convert.ToDecimal(sCharge[3]));
            }
            if (tRepData.SearchForRecord("NSTCK", "REPCODE"))
            {
                sCharge = tRepData.GetRecordFrom("NSTCK", 1);
                string sToPrint = "N. STOCK";
                while (sToPrint.Length + sCharge[2].TrimEnd('\0').Trim().Length < 14)
                    sToPrint += " ";
                sToPrint += sCharge[2].TrimEnd('\0').Trim();
                SendLineToPrinter(RightAlignStringOnExistingString(sToPrint, FormatMoneyForDisplay(FixFloatError((float)Convert.ToDecimal(sCharge[3])))));
                fTotalAmountOfVATGross = FixFloatError(fTotalAmountOfVATGross + (float)Convert.ToDecimal(sCharge[3]));
            }
            if (tRepData.SearchForRecord("CHRG", "REPCODE"))
            {
                sCharge = tRepData.GetRecordFrom("CHRG", 1);
                string sToPrint = "CHARGE";
                while (sToPrint.Length + sCharge[2].TrimEnd('\0').Trim().Length < 14)
                    sToPrint += " ";
                sToPrint += sCharge[2].TrimEnd('\0').Trim();
                SendLineToPrinter(RightAlignStringOnExistingString(sToPrint, FormatMoneyForDisplay(FixFloatError((float)Convert.ToDecimal(sCharge[3])))));
                fTotalAmountOfVATGross = FixFloatError(fTotalAmountOfVATGross + (float)Convert.ToDecimal(sCharge[3]));
            }
            if (tRepData.SearchForRecord("GREF", "REPCODE"))
            {
                sGeneralRefund = tRepData.GetRecordFrom("GREF", 1);
                string sToPrint = "G.REFUND";
                while (sToPrint.Length + sGeneralRefund[2].TrimEnd('\0').Trim().Length < 14)
                    sToPrint += " ";
                sToPrint += sGeneralRefund[2].TrimEnd('\0').Trim();
                SendLineToPrinter(RightAlignStringOnExistingString(sToPrint, FormatMoneyForDisplay(FixFloatError((float)Convert.ToDecimal(sGeneralRefund[3])))));
                fTotalAmountOfVATGross = FixFloatError(fTotalAmountOfVATGross + (float)Convert.ToDecimal(sGeneralRefund[3]));
            }
            if (tRepData.SearchForRecord("RONA", "REPCODE"))
            {
                sReceivedOnAc = tRepData.GetRecordFrom("RONA", 1);
                string sToPrint = "RC ON AC";
                while (sToPrint.Length + sReceivedOnAc[2].TrimEnd('\0').Trim().Length < 14)
                    sToPrint += " ";
                sToPrint += sReceivedOnAc[2].TrimEnd('\0').Trim();
                SendLineToPrinter(RightAlignStringOnExistingString(sToPrint, FormatMoneyForDisplay(FixFloatError((float)Convert.ToDecimal(sReceivedOnAc[3])))));
                fTotalAmountOfVATGross = FixFloatError(fTotalAmountOfVATGross + (float)Convert.ToDecimal(sReceivedOnAc[3]));
            }
            if (tRepData.SearchForRecord("DEPO", "REPCODE"))
            {
                sReceivedOnAc = tRepData.GetRecordFrom("DEP", 1);
                string sToPrint = "DEPOSIT";
                while (sToPrint.Length + sReceivedOnAc[2].TrimEnd('\0').Trim().Length < 14)
                    sToPrint += " ";
                sToPrint += sReceivedOnAc[2].TrimEnd('\0');
                SendLineToPrinter(RightAlignStringOnExistingString(sToPrint, FormatMoneyForDisplay(-FixFloatError((float)Convert.ToDecimal(sReceivedOnAc[3])))));
                fTotalAmountOfVATGross = FixFloatError(fTotalAmountOfVATGross - (float)Convert.ToDecimal(sReceivedOnAc[3]));
            }
            PrintBreaker();
            if (tVAT == null)
            {
                SendLineToPrinter("[Static VAT data - VAT.DBF is null]");
                float fTotalVATGross = 0.0f;
                if (tRepData.SearchForRecord("VAT01", "REPCODE"))
                {
                    string[] sVATData = tRepData.GetRecordFrom("VAT01", 1);
                    float fCurrentVATRate = GetVATRate(1);
                    float fCurrentVATGross = FixFloatError((float)Convert.ToDecimal(sVATData[3]));
                    fTotalVATGross = FixFloatError(fTotalVATGross + fCurrentVATGross);
                    float fVATAmount = FixFloatError(fCurrentVATGross - (fCurrentVATGross / (1 + (fCurrentVATRate / 100))));
                    string sToPrint = "V.A.T @ " + FormatMoneyForDisplay(fCurrentVATRate) + "%";
                    while (sToPrint.Length + FormatMoneyForDisplay(fVATAmount).Length < 30)
                        sToPrint += " ";
                    sToPrint += FormatMoneyForDisplay(fVATAmount);
                    SendLineToPrinter(RightAlignStringOnExistingString(sToPrint, FormatMoneyForDisplay(fCurrentVATGross)));
                }
                if (tRepData.SearchForRecord("VAT02", "REPCODE"))
                {
                    string[] sVATData = tRepData.GetRecordFrom("VAT02", 1);
                    float fCurrentVATRate = GetVATRate(0);
                    float fCurrentVATGross = FixFloatError((float)Convert.ToDecimal(sVATData[3]));
                    fTotalVATGross = FixFloatError(fTotalVATGross + fCurrentVATGross);
                    float fVATAmount = FixFloatError(fCurrentVATGross - (fCurrentVATGross / (1 + (fCurrentVATRate / 100))));
                    string sToPrint = "V.A.T (Z0) @ " + FormatMoneyForDisplay(fCurrentVATRate) + "%";
                    while (sToPrint.Length + FormatMoneyForDisplay(fVATAmount).Length < 30)
                        sToPrint += " ";
                    sToPrint += FormatMoneyForDisplay(fVATAmount);
                    SendLineToPrinter(RightAlignStringOnExistingString(sToPrint, FormatMoneyForDisplay(fCurrentVATGross)));
                }
                if (tRepData.SearchForRecord("VAT00", "REPCODE"))
                {
                    string[] sVATData = tRepData.GetRecordFrom("VAT00", 1);
                    float fVATRate = GetVATRate(0);
                    float fVATAmount = FixFloatError((float)Convert.ToDecimal(sVATData[3]));
                    fTotalVATGross = FixFloatError(fTotalVATGross + fVATAmount);
                    string sToPrint = "V.A.T Exempt";
                    SendLineToPrinter(RightAlignStringOnExistingString(sToPrint, FormatMoneyForDisplay(FixFloatError(fVATAmount))));
                }
                PrintBreaker();
                SendLineToPrinter(RightAlignStringOnExistingString("TOTAL", FormatMoneyForDisplay(fTotalVATGross)));
                PrintBreaker();
            }
            else
            {
                float fTotalVAT = 0;
                //SendLineToPrinter("[Dynamic VAT - VAT.DBF loaded]");
                string[] sCodes = GetVATCodes();
                float[] fRates = GetVATRates();
                if (sCodes[0] == "I1" && sCodes[1] == "X0" && sCodes[2] == "Z0")
                {
                    sCodes[1] = "Z0";
                    sCodes[2] = "X0";
                
                }
                for (int i = 0; i < sCodes.Length; i++)
                {
                    string[] sVATData = tRepData.GetRecordFrom("VAT" + sCodes[i], 1, true);
                    int nVATRecLoc = 0;
                    tVAT.SearchForRecord(sCodes[i], 0, ref nVATRecLoc);
                    if (sVATData[0] != null && sVATData[0] != "")
                    {
                        float fAmount = FixFloatError((float)Convert.ToDecimal(sVATData[3]));
                        float fVATRate = GetVATRate(sCodes[i]);
                        float fVATAmount = FixFloatError(fAmount - (fAmount / (1 + (fVATRate / 100))));
                        fTotalVAT = FixFloatError(fTotalVAT + fAmount);
                        string sToPrint = tVAT.GetRecordFrom(nVATRecLoc)[1] + "(" + FormatMoneyForDisplay((float)Convert.ToDecimal(tVAT.GetRecordFrom(nVATRecLoc)[2])) + "%)";
                        while (sToPrint.Length + FormatMoneyForDisplay(fVATAmount).Length < 30)
                            sToPrint += " ";
                        sToPrint += FormatMoneyForDisplay(fVATAmount);
                        SendLineToPrinter(RightAlignStringOnExistingString(sToPrint, FormatMoneyForDisplay(FixFloatError(fAmount))));
                    }
                }
                PrintBreaker();
                SendLineToPrinter(RightAlignStringOnExistingString("TOTAL", FormatMoneyForDisplay(fTotalVAT)));
                PrintBreaker();
            }


            // Fith section - Expenditure

            SendLineToPrinter("EXPENDITURE");
            PrintBreaker();
            float fTotalExpenditure = 0.0f;
            if (tRepData.SearchForRecord("GREF", "REPCODE"))
            {
                string[] sGeneralRefundData = tRepData.GetRecordFrom("GREF", 1);
                string sToPrint = "G.REFUND";
                while (sToPrint.Length + sGeneralRefundData[2].Length < 14)
                    sToPrint += " ";
                sToPrint += sGeneralRefundData[2];
                float fAmountRefunded = FixFloatError((float)Convert.ToDecimal(sGeneralRefundData[3]));
                fTotalExpenditure += fAmountRefunded;
                SendLineToPrinter(RightAlignStringOnExistingString(sToPrint, FormatMoneyForDisplay(fAmountRefunded)));
            }
            if (tRepData.SearchForRecord("SREF", "REPCODE"))
            {
                string[] sGeneralRefundData = tRepData.GetRecordFrom("SREF", 1);
                string sToPrint = "S.REFUND";
                while (sToPrint.Length + +sGeneralRefundData[2].Length < 14)
                    sToPrint += " ";
                sToPrint += sGeneralRefundData[2];
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
            string sPrintable = "NO SALES";
            while (sPrintable.Length + nOfTimesTillDrawOpened.ToString().Length < 14)
                sPrintable += " ";
            sPrintable += nOfTimesTillDrawOpened.ToString();
            SendLineToPrinter(sPrintable);
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
                string sToPrint = "PAID OUT";
                while (sToPrint.Length + sMoneyPaidOut[2].Length < 14)
                    sToPrint += " ";
                sToPrint += sMoneyPaidOut[2];
                float fAmountPaidOut = FixFloatError(-(float)Convert.ToDecimal(sMoneyPaidOut[3]));
                fTotalExpenditure += fAmountPaidOut;
                SendLineToPrinter(RightAlignStringOnExistingString(sToPrint, FormatMoneyForDisplay(fAmountPaidOut)));
            }
            else
            {
                string sToPrint = "PAID OUT     0";
                SendLineToPrinter(RightAlignStringOnExistingString(sToPrint, "0.00"));
            }

            int nOfVoidRecords = 0;
            string[,] sVoidedTransactions = tTHDR.SearchAndGetAllMatchingRecords(2, "1", ref nOfVoidRecords);
            if (nOfVoidRecords != 0)
            {
                SendLineToPrinter("VOIDED TRANSACTIONS : ");
            }
            for (int i = 0; i < nOfVoidRecords; i++)
            {
                string sStaffNum = sVoidedTransactions[i,5][sVoidedTransactions[i, 5].TrimEnd(' ').Length - 2].ToString() + sVoidedTransactions[i,5][(sVoidedTransactions[i, 5].TrimEnd(' ').Length - 1)].ToString();
                int nStaffNum = Convert.ToInt32(sStaffNum);
                string sStaffName = GetStaffName(nStaffNum);
                string sLine = sVoidedTransactions[i, 0].TrimEnd(' ') + " by " + sStaffName;
                SendLineToPrinter(RightAlignStringOnExistingString(sLine, sVoidedTransactions[i, 3]));
            }
            int nOfRemovedTrans = 0;
            string[,] sRemovedTrans = tRepData.SearchAndGetAllMatchingRecords(0, "RE", ref nOfRemovedTrans);
            if (nOfRemovedTrans != 0)
            {
                SendLineToPrinter("REMOVED TRANSACTIONS : ");
            }
            for (int i = 0; i < nOfRemovedTrans; i++)
            {
                string sStaffName = GetStaffName(Convert.ToInt32(sRemovedTrans[i, 2]));
                string sTransactionNumber = sRemovedTrans[i, 1].TrimEnd(' ');
                float fTransactionValue = (float)Convert.ToDecimal(sRemovedTrans[i,3]);
                string sLine = sTransactionNumber + " by " + sStaffName;
                SendLineToPrinter(RightAlignStringOnExistingString(sLine, FormatMoneyForDisplay(fTransactionValue)));
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
            if (tRepData.SearchForRecord("CHEQ", "REPCODE"))
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

        /// <summary>
        /// Works out a string for the receipt footer based on the current date and time
        /// </summary>
        /// <returns>A formatted string with the date and time now</returns>
        private string DateTimeForReceiptFooter()
        {
            if (!bDemoMode)
            {
                string sMinute = DateTime.Now.Minute.ToString();
                if (DateTime.Now.Minute < 10)
                    sMinute = "0" + sMinute;
                string sToReturn = DateTime.Now.Day + "/" + DateTime.Now.Month + "/" + DateTime.Now.Year
                    + " " + DateTime.Now.Hour + ":" + sMinute;
                return sToReturn;
            }
            else
            {
                frmDateInput fDate = new frmDateInput(new System.Drawing.Size(800, 600));
                fDate.Visible = false;
                fDate.ShowDialog();
                return fDate.DateTimeInput;
            }
        }

        /// <summary>
        /// Prints a receipt of a transaction that has been marked as VOID
        /// </summary>
        /// <param name="nTransactionNumber">The transaction number that has been marked as VOID</param>
        /// <param name="bReprintReceipt">Whether or not this is a reprint receipt</param>
        private void PrintTransactionVoidReceipt(int nTransactionNumber, bool bReprintReceipt)
        {
            SendLineToPrinter(CentralisePrinterText("TRANSACTION " + nTransactionNumber.ToString() + " VOID"));
            if (bReprintReceipt)
                PrintReprintReceiptNote();
            PrintReceiptFooter(GetCurrentStaffName(), DateTimeForReceiptFooter(), nTransactionNumber.ToString());
            PrintReceiptHeader();
            EmptyPrinterBuffer();
        }

        /// <summary>
        /// Opens the till drawer by sending a bell character to the printer
        /// </summary>
        /// <param name="bNoSale">Whether of not this is because of the no-sale key being pressed</param>
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

        /// <summary>
        /// Sends a blank line to the printer
        /// </summary>
        private void AddReceiptSpacingToBuffer()
        {
            for (int i = 0; i < GTill.Properties.Settings.Default.nSpacesBetweenReceipts; i++)
            {
                SendLineToPrinter(" ");
            }
        }

        public void PrintOffer(string sBarcode)
        {
            int nRecNum = -1;
            tOffers.SearchForRecord(sBarcode, 0, ref nRecNum);
            if (nRecNum == -1)
                return;
            string sReceiptLoc = tOffers.GetRecordFrom(nRecNum)[3];
            string sReceipt = "";

            TextReader tReader = new StreamReader("OffersReceipt\\" + sReceiptLoc);
            sReceipt = tReader.ReadToEnd();
            tReader.Close();

            sReceipt = sReceipt.Replace("<Underline>", ((char)0x1B).ToString() + ((char)0x2D).ToString() + ((char)0x31).ToString());
            sReceipt = sReceipt.Replace("</Underline>", ((char)0x1B).ToString() + ((char)0x2D).ToString() + ((char)0x30).ToString());

            sReceipt = sReceipt.Replace("<DoubleWidth>", ((char)0x0E).ToString());
            sReceipt = sReceipt.Replace("</DoubleWidth>", ((char)0x14).ToString());

            sReceipt = sReceipt.Replace("<DoubleHeight>", ((char)0x1B).ToString() + ((char)0x0E).ToString());
            sReceipt = sReceipt.Replace("</DoubleHeight>", ((char)0x1B).ToString() + ((char)0x14).ToString());

            sReceipt = sReceipt.Replace("<Highlight>", ((char)0x1B).ToString() + ((char)0x34).ToString());
            sReceipt = sReceipt.Replace("</Highlight>", ((char)0x1B).ToString() + ((char)0x35).ToString());

            sReceipt = sReceipt.Replace("<Emphasised>", ((char)0x1B).ToString() + ((char)0x45).ToString());
            sReceipt = sReceipt.Replace("</Emphasised>", ((char)0x1B).ToString() + ((char)0x46).ToString());

            int nBarcodeLoc = sReceipt.IndexOf("<Barcode>");
            if (nBarcodeLoc != -1)
            {
                int nEndOfBarcode = sReceipt.IndexOf("</Barcode>");
                if (nEndOfBarcode != -1)
                {
                    nBarcodeLoc += 9;
                    string sOfferBarcode = sReceipt.Substring(nBarcodeLoc, nEndOfBarcode - nBarcodeLoc);
                    byte[] bToAdd = PrintBarcode(sOfferBarcode, true);
                    string sBarcodeHex = "";
                    for (int i = 0; i < bToAdd.Length; i++)
                    {
                        sBarcodeHex += ((char)bToAdd[i]).ToString();
                    }

                    sReceipt = sReceipt.Replace("<Barcode>" + sOfferBarcode + "</Barcode>", sBarcodeHex);
                }
            }

            while (sReceipt.Contains("<Centre>"))
            {
                int nCentLoc = sReceipt.IndexOf("<Centre>");
                int nEndLoc = sReceipt.IndexOf("</Centre>");
                nCentLoc += 8;
                string sToCentre = sReceipt.Substring(nCentLoc, nEndLoc - nCentLoc);
                sReceipt = sReceipt.Remove(nCentLoc - 8, 8 + sToCentre.Length + 9);
                nCentLoc -= 8;
                bool bDouble = false;
                for (int i = (nCentLoc) - 1; i >= 0; i-=1)
                {
                    if (sReceipt[i] == ((char)0x14)) // End Double Width
                    {
                        bDouble = false;
                        break;
                    }
                    else if (sReceipt[i] == ((char)0x0E))
                    {
                        bDouble = true;
                        break;
                    }
                }
                int nTextWidth = sToCentre.Length;
                if (bDouble)
                    nTextWidth *= 2;

                int nSpaceLoc = 0;
                while (nTextWidth - nSpaceLoc > nPrinterWidth)
                    nSpaceLoc += nPrinterWidth;

                if (bDouble)
                    nSpaceLoc /= 2;
                int nLeftToCentre = sToCentre.Substring(nSpaceLoc, sToCentre.Length - nSpaceLoc).Length;
                if (bDouble)
                {
                    nLeftToCentre *= 2;
                    nSpaceLoc *= 2;
                }
                int nPadding = nPrinterWidth - nLeftToCentre;
                nPadding /= 2;

                string sToInsert = "";
                for (int i = 0; i < nPadding; i++)
                {
                    sToInsert += " ";
                }

                if (bDouble)
                    nSpaceLoc /= 2;

                sToCentre = sToCentre.Insert(nSpaceLoc, sToInsert);

                if (bDouble)
                    nSpaceLoc *= 2;

                sReceipt = sReceipt.Insert(nCentLoc - 8, sToCentre);

                                

            }


            SendLineToPrinter(sReceipt);
            SendLineToPrinter("");
            SendLineToPrinter("");

            int nPrinted = Convert.ToInt32(tOffers.GetRecordFrom(nRecNum)[4]);
            tOffers.EditRecordData(nRecNum, 4, (nPrinted + 1).ToString());
            tOffers.SaveToFile("OFFERS.DBF");

        }


        /*public void PrintOffers()
        {
            if (File.Exists("offers.txt"))
            {
                TextReader tOfferReader = new StreamReader("offers.txt");
                string[] sOffers = tOfferReader.ReadToEnd().Split('\n');*/
                
    }
}