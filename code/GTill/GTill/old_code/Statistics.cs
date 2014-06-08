using System;
using System.Collections.Generic;
using System.Text;
using TillEngine;

namespace GTill
{
    class Statistics
    {
        public static string StaffWithBiggestTransaction(ref TillEngine.TillEngine tEngine)
        {
            string[] sTransactionNumbers = tEngine.GetListOfTransactionNumbers();
            float fHighest = 0.0f;
            string sStaffWithHighest = "";
            for (int i = 0; i < sTransactionNumbers.Length; i++)
            {
                string[,] sTransactionInfo = tEngine.GetTransactionInfo(sTransactionNumbers[i]);
                int nItemsInTransaction = Convert.ToInt32(sTransactionInfo[0, 0]);
                if (sTransactionInfo[0, 3] == "SALE")
                {
                    float fAmount = 0.0f;
                    for (int x = 1; x <= nItemsInTransaction; x++)
                    {
                        fAmount += (float)Convert.ToDecimal(sTransactionInfo[x, 2]);
                        fAmount = tEngine.fFixFloatError(fAmount);
                    }
                    if (fAmount > fHighest)
                    {
                        fHighest = fAmount;
                        sStaffWithHighest = tEngine.ReturnSensibleDateTimeString(sTransactionInfo[0, 2])[1];
                    }
                }
            }
            if (fHighest == 0.0f)
                return "No transactions";
            else
                return sStaffWithHighest + " - " + tEngine.GetCurrencySymbol() + TillEngine.TillEngine.FormatMoneyForDisplay(fHighest);
            
        }

        public static string StaffThatHasSoldTheMost(ref TillEngine.TillEngine tEngine)
        {
            float fTotal = 0.0f;
            float[] fAmountSold = new float[10];
            for (int i = 0; i < fAmountSold.Length; i++)
            {
                fAmountSold[i] = WorkOutHowMuchStaffMemberHasSold(i, ref tEngine);
                fTotal += fAmountSold[i];
                fTotal = tEngine.fFixFloatError(fTotal);
            }
            int nMaxSlot = 0;
            for (int i = 0; i < fAmountSold.Length; i++)
            {
                if (fAmountSold[i] > fAmountSold[nMaxSlot])
                    nMaxSlot = i;
            }
            try
            {
                if (fTotal != 0.0f)
                {
                    float fPercentage = (100 / fTotal) * fAmountSold[nMaxSlot];
                    fPercentage = tEngine.fFixFloatError(fPercentage);
                    return tEngine.GetStaffName(nMaxSlot) + " - " + tEngine.GetCurrencySymbol() + TillEngine.TillEngine.FormatMoneyForDisplay(fAmountSold[nMaxSlot]) + " (" + TillEngine.TillEngine.FormatMoneyForDisplay(fPercentage) + "% of total)";
                }
                else
                    return "No transactions";
            }
            catch
            {
                return "No transactions";
            }
        }

        private static float WorkOutHowMuchStaffMemberHasSold(int nStaffMemberNumber, ref TillEngine.TillEngine tEngine)
        {
            string[] sTransactionNumbers = tEngine.GetListOfTransactionNumbers();
            float fHighest = 0.0f;

            string sStaffWithHighest = "";
            for (int i = 0; i < sTransactionNumbers.Length; i++)
            {
                string[,] sTransactionInfo = tEngine.GetTransactionInfo(sTransactionNumbers[i]);
                int nItemsInTransaction = Convert.ToInt32(sTransactionInfo[0, 0]);
                if (sTransactionInfo[0, 3] == "SALE")
                {
                    float fAmount = 0.0f;
                    for (int x = 1; x <= nItemsInTransaction; x++)
                    {
                        fAmount += (float)Convert.ToDecimal(sTransactionInfo[x, 2]);
                        fAmount = tEngine.fFixFloatError(fAmount);
                    }
                        sStaffWithHighest = tEngine.ReturnSensibleDateTimeString(sTransactionInfo[0, 2])[1];
                        if (tEngine.GetStaffName(nStaffMemberNumber).ToUpper() == sStaffWithHighest.ToUpper())
                            fHighest += fAmount;
                    
                }
            }
            return tEngine.fFixFloatError(fHighest);
        }

        public static string StaffThatHasSoldHighestQuantityOfItem(ref TillEngine.TillEngine tEngine)
        {
            int[] nQuantitySold = new int[10];
            int nTotalSold = 0;
            for (int i = 0; i < nQuantitySold.Length; i++)
            {
                nQuantitySold[i] = WorkOutQuantityOfItemStaffMemberHasSold(i, ref tEngine);
                nTotalSold += nQuantitySold[i];
            }
            int nHighest = 0;
            for (int i = 0; i < nQuantitySold.Length; i++)
            {
                if (nQuantitySold[nHighest] < nQuantitySold[i])
                    nHighest = i;
            }
            try
            {
                if (nTotalSold != 0)
                {
                    float fPercentage = (100 / (float)nTotalSold) * nQuantitySold[nHighest];
                    fPercentage = TillEngine.TillEngine.FixFloatError(fPercentage);
                    return tEngine.GetStaffName(nHighest) + " - " + nQuantitySold[nHighest].ToString() + " item(s) (" + TillEngine.TillEngine.FormatMoneyForDisplay(fPercentage) + "% of total)";
                }
                else
                {
                    return "No transactions";
                }
            }
            catch
            {
                return "No transactions";
            }
        }

        private static int WorkOutQuantityOfItemStaffMemberHasSold(int nStaffMemberNumber, ref TillEngine.TillEngine tEngine)
        {
            string[] sTransactionNumbers = tEngine.GetListOfTransactionNumbers();
            int nHighest = 0;

            string sStaffWithHighest = "";
            for (int i = 0; i < sTransactionNumbers.Length; i++)
            {
                string[,] sTransactionInfo = tEngine.GetTransactionInfo(sTransactionNumbers[i]);
                int nItemsInTransaction = Convert.ToInt32(sTransactionInfo[0, 0]);
                if (sTransactionInfo[0, 3] == "SALE")
                {
                    int nCurrentItems = 0;
                    for (int x = 1; x <= nItemsInTransaction; x++)
                    {
                        nCurrentItems += Convert.ToInt32(sTransactionInfo[x, 4]);
                    }
                    sStaffWithHighest = tEngine.ReturnSensibleDateTimeString(sTransactionInfo[0, 2])[1];
                    if (tEngine.GetStaffName(nStaffMemberNumber).ToUpper() == sStaffWithHighest.ToUpper())
                        nHighest += nCurrentItems;
                }
            }
            return nHighest;
        }


    }
}
