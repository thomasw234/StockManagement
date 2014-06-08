using System;
using System.Collections.Generic;
using System.Text;
using TillEngine;

namespace GTill
{
    /// <summary>
    /// Works out various statistics about the day's sales
    /// </summary>
    class Statistics
    {
        /// <summary>
        /// Gets the name of the staff member who has had the highest value transaction of the day
        /// </summary>
        /// <param name="tEngine">A reference to the TillEngine</param>
        /// <returns>The name of a member of staff</returns>
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
                return sStaffWithHighest + " - " + tEngine.CurrencySymbol + TillEngine.TillEngine.FormatMoneyForDisplay(fHighest);
            
        }

        /// <summary>
        /// Gets the name of the staff member who has sold the highest quantity of items
        /// </summary>
        /// <param name="tEngine">A reference to the TillEngine</param>
        /// <returns>The name of the staff member who has sold the highest quantity of items</returns>
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
                    return tEngine.GetStaffName(nMaxSlot) + " - " + tEngine.CurrencySymbol + TillEngine.TillEngine.FormatMoneyForDisplay(fAmountSold[nMaxSlot]) + " (" + TillEngine.TillEngine.FormatMoneyForDisplay(fPercentage) + "% of total)";
                }
                else
                    return "No transactions";
            }
            catch
            {
                return "No transactions";
            }
        }

        public static float[] GetAmountsThatStaffHaveSold(ref TillEngine.TillEngine te)
        {
            float[] fAmounts = new float[100];
            for (int i = 0; i < 100; i++)
            {
                fAmounts[i] = WorkOutHowMuchStaffMemberHasSold(i, ref te);
            }
            return fAmounts;
        }

        /// <summary>
        /// Works out how much a member of staff has sold (by price)
        /// </summary>
        /// <param name="nStaffMemberNumber">The staff number</param>
        /// <param name="tEngine">A reference to the TillEngine</param>
        /// <returns>How much a staff member has sold</returns>
        private static float WorkOutHowMuchStaffMemberHasSold(int nStaffMemberNumber, ref TillEngine.TillEngine tEngine)
        {
            string[] sTransactionNumbers = tEngine.GetListOfTransactionNumbers();
            float fHighest = 0.0f;

            string sStaffWithHighest = "";
            for (int i = 0; i < sTransactionNumbers.Length; i++)
            {
                string[,] sTransactionInfo = tEngine.GetTransactionInfo(sTransactionNumbers[i]);
                int nItemsInTransaction = Convert.ToInt32(sTransactionInfo[0, 0]);
                int nOfPaymentMethods = Convert.ToInt32(sTransactionInfo[0, 1]);
                //if (sTransactionInfo[0, 3] == "SALE")
                //{
                    float fAmount = 0.0f;
                    for (int x = 1; x <= nItemsInTransaction; x++)
                    {
                        fAmount += (float)Convert.ToDecimal(sTransactionInfo[x, 2]);
                        fAmount = tEngine.fFixFloatError(fAmount);
                    }
                    for (int x = nItemsInTransaction; x <= nItemsInTransaction + nOfPaymentMethods; x++)
                    {
                        if (sTransactionInfo[x, 0] == "DEPO")
                        {
                            fAmount -= (float)Convert.ToDecimal(sTransactionInfo[x, 1]);
                            fAmount = tEngine.fFixFloatError(fAmount);
                        }
                    }
                        sStaffWithHighest = tEngine.ReturnSensibleDateTimeString(sTransactionInfo[0, 2])[1];
                        if (tEngine.GetStaffName(nStaffMemberNumber).ToUpper() == sStaffWithHighest.ToUpper())
                            fHighest += fAmount;
                    
                //}
            }
            if (fHighest < 0)
                fHighest = 0;
            return tEngine.fFixFloatError(fHighest);
        }

        /// <summary>
        /// Works out the name of the staff member who has sold the highest quantity of items
        /// </summary>
        /// <param name="tEngine">A reference to the TillEngine</param>
        /// <returns>The name of the staff member who has sold the highest quantity of items</returns>
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

        /// <summary>
        /// Works out how many items the member of staff has sold
        /// </summary>
        /// <param name="nStaffMemberNumber">The staff member's number</param>
        /// <param name="tEngine">A reference to the TillEngine</param>
        /// <returns>How many items the member of staff has sold</returns>
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
