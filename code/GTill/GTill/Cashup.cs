using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DBFDetailsViewerV2;

namespace TillEngine
{
    /// <summary>
    /// An extension of the TillEngine, just more convenient in a different file
    /// Cashes up the till
    /// </summary>
    public partial class TillEngine
    {
        /// <summary>
        /// The day number as seen in the OUTGNG Directory. i.e. REPDATA7.DBF is Saturday
        /// </summary>
        /// <returns>The day number based on System.DateTime.Now</returns>
        private int DayNumber(string sDateToday)
        {
            string[] sDate = sDateToday.Split('/');
            DateTime dtCashup = new DateTime(Convert.ToInt32(sDate[2]), Convert.ToInt32(sDate[1]), Convert.ToInt32(sDate[0]));
            switch (dtCashup.DayOfWeek.ToString().ToUpper())
            {
                case "SUNDAY":
                    return 1;
                    break;
                case "MONDAY":
                    return 2;
                    break;
                case "TUESDAY":
                    return 3;
                    break;
                case "WEDNESDAY":
                    return 4;
                    break;
                case "THURSDAY":
                    return 5;
                    break;
                case "FRIDAY":
                    return 6;
                    break;
                case "SATURDAY":
                    return 7;
                    break;
            }
            return 0;
        }

        /// <summary>
        /// The location of the OUTGNG folder
        /// </summary>
        string sOutGNGFolderLocation = GTill.Properties.Settings.Default.sOUTGNGDir;
        /// <summary>
        /// The location of REPDATA
        /// </summary>
        string sRepDataFileLoc = GTill.Properties.Settings.Default.sRepDataLoc;
        /// <summary>
        /// The location of TDATA
        /// </summary>
        string sTDataFileLoc = GTill.Properties.Settings.Default.sTDataLoc;
        /// <summary>
        /// The location of THDR
        /// </summary>
        string sTHdrFileLoc = GTill.Properties.Settings.Default.sTHdrLoc;

        private bool IsItSunday
        {
            get
            {
                return (DateTime.Now.DayOfWeek == DayOfWeek.Sunday);
            }
        }

        /// <summary>
        /// Cashes up the till
        /// </summary>
        public void CashUp()
        {
            try
            {
                // Open till drawer
                OpenTillDrawer(false);
                //ApplyCreditCardDiscs();

                // Check what to do if it's a Sunday
                if (IsItSunday && System.Windows.Forms.MessageBox.Show("Today is Sunday. Should I skip cashing up? (If you're unsure, choose yes)", "Sunday", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    return;
                }

                string sDateToday = tRepData.GetRecordFrom(0)[1].TrimEnd('\0');
                tRepData.EditRecordData(0, 2, "2"); //  Change the date REPQTY to 2
                // Sort out START and END Records
                string[] sEnd = tRepData.GetRecordFrom("END", 1);
                float fStartValue = FixFloatError((float)Convert.ToDecimal(sEnd[3].TrimEnd('\0')) + 0.01f);

                // Add SALES Record
                /*
                string[] sNoTran = tRepData.GetRecordFrom("NOTRAN", 1);
                string[] sSales = new string[sNoTran.Length];
                Array.Copy(sNoTran, sSales, sNoTran.Length);
                sSales[1] = "SALES";
                sSales[4] = "";
                tRepData.AddRecord(sSales);
                // Sales record added
                tRepData.SaveToFile(sRepDataFileLoc);*/

                // Fixed SALES record code

                int nOfSalesRecords = 0;
                string[,] sSales = tTHDR.SearchAndGetAllMatchingRecords(4, "SALE", ref nOfSalesRecords);
                float fSalesAmount = 0.0f;
                for (int i = 0; i < nOfSalesRecords; i++)
                {
                    fSalesAmount += (float)Convert.ToDecimal(sSales[i, 3]);
                    fSalesAmount = TillEngine.FixFloatError(fSalesAmount);
                }
                string[] sToAdd = { "CR", "SALES", nOfSalesRecords.ToString(), fSalesAmount.ToString(), "" };
                tRepData.AddRecord(sToAdd);

                // Remove removed transactions records

                int nTranNum = 0;
                do
                {
                    nTranNum = 0;
                    tRepData.SearchForRecord("RE", 0, ref nTranNum);
                    if (nTranNum != 0)
                    {
                        tRepData.DeleteRecord(nTranNum);
                    }
                }
                while (nTranNum != 0);
                tRepData.SaveToFile(sRepDataFileLoc);

                if (!Directory.Exists(sOutGNGFolderLocation))
                {
                    try
                    {
                        Directory.CreateDirectory(sOutGNGFolderLocation);
                    }
                    catch
                    {
                        GTill.ErrorHandler.LogError("The directory " + sOutGNGFolderLocation + " was not found, and could not be created. Please create this folder, or run this program with administrator priviliges (Windows Vista & 7)");
                        throw new NotSupportedException("The directory " + sOutGNGFolderLocation + " was not found, and could not be created. Please create this folder, or run this program with administrator priviliges (Windows Vista & 7)");
                    }
                }

                File.Copy(sRepDataFileLoc, sOutGNGFolderLocation + "\\" + sRepDataFileLoc.Replace(".DBF", "") + DayNumber(sDateToday).ToString() + ".DBF", true);
                File.Copy(sOutGNGFolderLocation + "\\" + sRepDataFileLoc.Replace(".DBF", "") + DayNumber(sDateToday).ToString() + ".DBF", sOutGNGFolderLocation + "\\" + sRepDataFileLoc, true);
                File.Copy(sTDataFileLoc, sOutGNGFolderLocation + "\\" + sTDataFileLoc.Replace(".DBF", "") + DayNumber(sDateToday).ToString() + ".DBF", true);
                File.Copy(sTHdrFileLoc, sOutGNGFolderLocation + "\\" + sTHdrFileLoc.Replace(".DBF", "") + DayNumber(sDateToday).ToString() + ".DBF", true);

                // Write the blank files
                FileStream fsWriter = new FileStream(sRepDataFileLoc, FileMode.Create);
                fsWriter.Write(GTill.Properties.Resources.BLANK_REPDATA, 0, GTill.Properties.Resources.BLANK_REPDATA.Length);
                fsWriter.Close();
                fsWriter = new FileStream(sTDataFileLoc, FileMode.Create);
                fsWriter.Write(GTill.Properties.Resources.BLANK_TDATA, 0, GTill.Properties.Resources.BLANK_TDATA.Length);
                fsWriter.Close();
                fsWriter = new FileStream(sTHdrFileLoc, FileMode.Create);
                fsWriter.Write(GTill.Properties.Resources.BLANK_THDR, 0, GTill.Properties.Resources.BLANK_THDR.Length);
                fsWriter.Close();

                // Reload the blank files
                tRepData = new Table(sRepDataFileLoc);
                tTData = new Table(sTDataFileLoc);
                tTHDR = new Table(sTHdrFileLoc);

                tRepData.EditRecordData(0, 2, "4"); // Change the REPQTY to 4, to show that the till has been cashed up
                tRepData.EditRecordData(0, 1, sDateToday);
                int nStartRecordLocation = 0;
                tRepData.SearchForRecord("START", 1, ref nStartRecordLocation);
                tRepData.EditRecordData(nStartRecordLocation, 3, FormatMoneyForDisplay(fStartValue));
                tRepData.SearchForRecord("END", 1, ref nStartRecordLocation);
                tRepData.EditRecordData(nStartRecordLocation, 3, FormatMoneyForDisplay(FixFloatError(fStartValue - 0.01f)));
                // Change the IDENT record. Default is MODELS, but only Models uses that
                tRepData.SearchForRecord("IDENT MODELS", 1, ref nStartRecordLocation);
                string[] sIdent = tRepData.GetRecordFrom("IDENT MODELS", 1);
                sIdent[1] = "IDENT " + TillName.ToUpper();
                tRepData.EditRecordData(nStartRecordLocation, 1, sIdent[1]);
                tRepData.SaveToFile(sRepDataFileLoc);
                tTData.SaveToFile(sTDataFileLoc);
                tTHDR.SaveToFile(sTHdrFileLoc);
            }
            catch (Exception ex)
            {
                GTill.ErrorHandler.LogError(ex.ToString());
                System.Windows.Forms.MessageBox.Show("Error caught! Please try cashing up again");
            }
        }

        string[] GetListOfCardDiscounts()
        {
            string[] sToReturn = new string[9];
            for (int i = 0; i < 9; i++)
            {
                sToReturn[i] = tDetails.GetRecordFrom(26 + i)[0];
            }
            return sToReturn;
        }

        public void ApplyCreditCardDiscs()
        {
            // First array element in format { NumberOfItems, NumberOfPaymentMethods, TransactionDateTime, SpecialTransaction }
            // Item array in format { Item code, Item Description, Price Paid, Discount, Quantity }
            // Payment method array format { PaymentCode, Amount, Blank, Blank, Blank }
            //
            // SpecialTransactions can be CASHPAIDOUT, SPECIFICREFUND, VOID

            string[] sListOfDiscs = GetListOfCardDiscounts();
            string[] sListOfTranNums = this.GetListOfTransactionNumbers();
            for (int t = 0; t < sListOfTranNums.Length; t++)
            {
                int nOfItems = 0;
                int nOfPayments = 0;
                string[,] sTransactionInfo = this.GetTransactionInfo(sListOfTranNums[t]);
                nOfItems = Convert.ToInt32(sTransactionInfo[0, 0]);
                nOfPayments = Convert.ToInt32(sTransactionInfo[0, 1]);
                for (int x = 0; x < nOfPayments; x++)
                {
                    if (sTransactionInfo[1 + nOfItems + x, 0].StartsWith("CRCD"))
                    {
                        for (int i = 1; i <= nOfItems; i++)
                        {
                            decimal dPercentToDiscount = Convert.ToDecimal(sListOfDiscs[Convert.ToInt32(sTransactionInfo[1 + nOfItems + x, 0][4].ToString()) - 1]);
                            decimal dAmountToDeduct = Math.Round(Convert.ToDecimal(sTransactionInfo[1 + nOfItems + x, 1]) - (Convert.ToDecimal(sTransactionInfo[1 + nOfItems + x,1]) * (1.0m - (dPercentToDiscount / 100))), 2);

                            int nRecNum = 0;
                            tRepData.SearchForRecord(sTransactionInfo[i, 0], 1, ref nRecNum);
                            decimal dCurrentAmount = Convert.ToDecimal(tRepData.GetRecordFrom(nRecNum)[3]);
                            dCurrentAmount -= dAmountToDeduct;
                            tRepData.EditRecordData(nRecNum, 3, dCurrentAmount.ToString());

                            if (GetItemAsItemClass(sTransactionInfo[i, 0]).ItemCategory != 2  && GetItemAsItemClass(sTransactionInfo[i, 0]).ItemCategory != 6)
                            {
                                tRepData.SearchForRecord("STOCK", 1, ref nRecNum);
                                decimal dCurrentStock = Convert.ToDecimal(tRepData.GetRecordFrom(nRecNum)[3]);
                                dCurrentStock -= dAmountToDeduct;
                                tRepData.EditRecordData(nRecNum, 3, dCurrentStock.ToString());
                            }
                            else
                            {
                                tRepData.SearchForRecord("NSTCK", 1, ref nRecNum);
                                decimal dCurrentStock = Convert.ToDecimal(tRepData.GetRecordFrom(nRecNum)[3]);
                                dCurrentStock -= dAmountToDeduct;
                                tRepData.EditRecordData(nRecNum, 3, dCurrentStock.ToString());
                            }

                            tRepData.SearchForRecord("CRCD", 1, ref nRecNum);
                            decimal dCRCDAmnt = Convert.ToDecimal(tRepData.GetRecordFrom(nRecNum)[3]);
                            dCRCDAmnt -= dAmountToDeduct;
                            tRepData.EditRecordData(nRecNum, 3, dCRCDAmnt.ToString());

                            tRepData.SearchForRecord("CRD" + (Convert.ToInt32(sTransactionInfo[1 + nOfItems + x, 0][4].ToString())).ToString(), 1, ref nRecNum);
                            decimal dCRDAmnt = Convert.ToDecimal(tRepData.GetRecordFrom(nRecNum)[3]);
                            dCRDAmnt -= dAmountToDeduct;
                            tRepData.EditRecordData(nRecNum, 3, dCRDAmnt.ToString());

                            tRepData.SearchForRecord("VAT" + GetItemAsItemClass(sTransactionInfo[i,0]).VATRate, 1, ref nRecNum);
                            decimal dCurrentVATRate = Convert.ToDecimal(tRepData.GetRecordFrom(nRecNum)[3]);
                            dCurrentVATRate -= dAmountToDeduct;
                            tRepData.EditRecordData(nRecNum, 3, dCurrentVATRate.ToString());

                            while (sListOfTranNums[t].Length < 6)
                                sListOfTranNums[t] = "0" + sListOfTranNums[t];
                            nRecNum = tTData.GetRecordNumberFromTwoFields(sListOfTranNums[t], 0, sTransactionInfo[i, 0], 3);
                            decimal dCurrentDisc = Convert.ToDecimal(tTData.GetRecordFrom(nRecNum)[8]);
                            dCurrentDisc += dAmountToDeduct;
                            tTData.EditRecordData(nRecNum, 8, dCurrentDisc.ToString());
                        }
                    }
                }
                tRepData.SaveToFile("REPDATA.DBF");
                tTData.SaveToFile("TDATA.DBF");
            }
        }

        
    }
}
