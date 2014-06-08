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

        string sOutGNGFolderLocation = GTill.Properties.Settings.Default.sOUTGNGDir;
        string sRepDataFileLoc = GTill.Properties.Settings.Default.sRepDataLoc;
        string sTDataFileLoc = GTill.Properties.Settings.Default.sTDataLoc;
        string sTHdrFileLoc = GTill.Properties.Settings.Default.sTHdrLoc;

        /// <summary>
        /// Cashes up the till
        /// </summary>
        public void CashUp()
        {
            string sDateToday = tRepData.GetRecordFrom(0)[1].TrimEnd('\0');
            tRepData.EditRecordData(0, 2, "2"); //  Change the date REPQTY to 2
            // Sort out START and END Records
            string[] sEnd = tRepData.GetRecordFrom("END", 1);
            float fStartValue = FixFloatError((float)Convert.ToDecimal(sEnd[3].TrimEnd('\0')) + 0.01f);
            
            // Add SALES Record
            string[] sNoTran = tRepData.GetRecordFrom("NOTRAN", 1);
            string[] sSales = new string[sNoTran.Length];
            Array.Copy(sNoTran, sSales, sNoTran.Length);
            sSales[1] = "SALES";
            sSales[4] = "";
            tRepData.AddRecord(sSales);
            // Sales record added
            tRepData.SaveToFile(sRepDataFileLoc);

            if (!Directory.Exists(sOutGNGFolderLocation))
            {
                try
                {
                    Directory.CreateDirectory(sOutGNGFolderLocation);
                }
                catch
                {
                    throw new NotSupportedException("The directory " + sOutGNGFolderLocation + " was not found, and could not be created. Please create this folder, or run this program with administrator priviliges (Windows Vista & 7)");
                }
            }

            // Open till drawer
            OpenTillDrawer(false);

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
            sIdent[1] = "IDENT " + GetTillName().ToUpper();
            tRepData.EditRecordData(nStartRecordLocation, 1, sIdent[1]);
            tRepData.SaveToFile(sRepDataFileLoc);
            tTData.SaveToFile(sTDataFileLoc);
            tTHDR.SaveToFile(sTHdrFileLoc);
        }
    }
}
