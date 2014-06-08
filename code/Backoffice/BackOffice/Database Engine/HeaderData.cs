using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackOffice.Database_Engine
{
    /// <summary>
    /// A class which contains the header data of a dBase III format table
    /// </summary>
    public class HeaderData
    {
        /// <summary>
        /// The possible field data types in a dBase file
        /// </summary>
        public enum FieldTypes { Character, Currency, Numeric, Float, Date, DateTime, Double, Integer, Logical, Memo, General, CharacterBin, MemoBin, Picture };

        /// <summary>
        /// Whether or not the file is a valid dBase III file
        /// </summary>
        public bool validDbaseFile;
        /// <summary>
        /// The last modified date in format YY/MM/DD
        /// </summary>
        public string lastDateModified;
        /// <summary>
        /// The record count
        /// </summary>
        public int nRecordCount;
        /// <summary>
        /// The position in the byte array where the field data starts
        /// </summary>
        public int nFieldDataStartsAt;
        /// <summary>
        /// The number of records in the table
        /// </summary>
        public int nOfRecords = 0;
        /// <summary>
        /// The size (in bytes) of an individual record
        /// </summary>
        public int nRecordSize;
        /// <summary>
        /// The number of fields in the table
        /// </summary>
        public int nOfFields;
        /// <summary>
        /// The length (in bytes) of each field
        /// </summary>
        public int[] nFieldLength;
        /// <summary>
        /// The name of each field
        /// </summary>
        public string[] sFieldNames;
        /// <summary>
        /// The data type of each field
        /// </summary>
        public FieldTypes[] fFieldTypes;

        /// <summary>
        /// Initialises the Header Data
        /// </summary>
        /// <param name="sFieldName">The field names</param>
        /// <param name="nFieldLegnths">The field lengths</param>
        /// <param name="fft">The field types</param>
        public HeaderData(string[] sFieldName, int[] nFieldLegnths, FieldTypes[] fft)
        {
            validDbaseFile = true;
            lastDateModified = "TODO!!";
            nRecordCount = 1;
            nFieldDataStartsAt = 162;
            nOfRecords = 1;
            int recordSize = 0;

            foreach (int value in nFieldLegnths)
                recordSize += value;

            nRecordSize = recordSize + 1;
            nOfFields = sFieldName.Length;
            nFieldLength = nFieldLegnths;
            sFieldNames = sFieldName;
            fFieldTypes = fft;
        }
        /// <summary>
        /// Initialises Header Data from a dBase III format file
        /// </summary>
        /// <param name="dBFileData">The array of bytes which is the dBase III format file's contents</param>
        public HeaderData(byte[] dBFileData)
        {
            ReadHeader(dBFileData);
        }

        /// <summary>
        /// Reads the header from a dBase III format file
        /// </summary>
        /// <param name="dbaseFile">The dBase III format file's contents</param>
        private void ReadHeader(byte[] dbaseFile)
        {
            int nPos = 1;

            lastDateModified = dbaseFile[1] + "/" + dbaseFile[2] + "/" + dbaseFile[3];
            nPos += 3;

            byte[] recordCount = { dbaseFile[4], dbaseFile[5], dbaseFile[6], dbaseFile[7] };
            nPos = 8;
            nOfRecords = nConverter.MergeBytesToDenary(recordCount);

            byte[] headerSize = { dbaseFile[8], dbaseFile[9] };
            nPos += 2;
            nFieldDataStartsAt = nConverter.MergeBytesToDenary(headerSize) + 1;

            byte[] bytesInRecord = { dbaseFile[10], dbaseFile[11] };
            nPos += 2;
            nRecordSize = nConverter.MergeBytesToDenary(bytesInRecord);

            nPos = 32;
            // Move onto the field descriptor array

            // Find the end
            nOfFields = 0;
            for (int i = nPos; i < nFieldDataStartsAt; i++)
            {
                if (dbaseFile[i] == 13 && (i % 32) == 0)
                {
                    nOfFields = i - nPos;
                    break;
                }
            }
            nOfFields /= 32;

            nFieldLength = new int[nOfFields];
            sFieldNames = new string[nOfFields];
            fFieldTypes = new FieldTypes[nOfFields];

            string[,] sTable = new string[nOfRecords, nOfFields];

            int nPos1 = 0;
            for (int i = nPos; i < (nPos + (nOfFields * 32)); i += 32)
            {
                string sName = "";
                for (int x = i; x < i + 11; x++)
                {
                    sName += (char)dbaseFile[x];
                }
                // Work out the field name
                sFieldNames[nPos1] = sName;

                char fieldType = (char)dbaseFile[i + 11];
                switch (fieldType)
                {
                    case 'C':
                        fFieldTypes[nPos1] = FieldTypes.Character;
                        break;
                    case 'D':
                        fFieldTypes[nPos1] = FieldTypes.Date;
                        break;
                    case 'N':
                        fFieldTypes[nPos1] = FieldTypes.Numeric;
                        break;
                    case 'L':
                        fFieldTypes[nPos1] = FieldTypes.Logical;
                        break;
                    case 'M':
                        fFieldTypes[nPos1] = FieldTypes.Memo;
                        break;
                }
                nFieldLength[nPos1] = dbaseFile[i + 16];
                nPos1++;
            }

        }

        /// <summary>
        /// Calculates the header data for this table in a dBase III structure
        /// </summary>
        /// <returns>An array of bytes which represesnt the header for a dBase III format table</returns>
        public byte[] ReturnHeaderData()
        {
            int nHeaderSize = 32 + (nOfFields * 32) + 2;
            byte[] headerBytes = new byte[nHeaderSize];

            headerBytes[0] = 3;
            // Make a valid dBase III File

            string dateTime = DateTime.Now.Year.ToString()[2].ToString() + DateTime.Now.Year.ToString()[3].ToString();
            string monthNow = DateTime.Now.Month.ToString();
            if (monthNow.Length == 1)
                monthNow = "0" + monthNow;
            string dayNow = DateTime.Now.Day.ToString();
            if (dayNow.Length == 1)
                dayNow = "0" + dayNow;
            // Set the last modified date

            int nM = Convert.ToInt32(monthNow);
            int nD = Convert.ToInt32(dayNow);
            int nY = Convert.ToInt32(dateTime);

            headerBytes[1] = (byte)nY;
            headerBytes[2] = (byte)nM;
            headerBytes[3] = (byte)nD;

            headerBytes[4] = nConverter.SplitDenaryToBytes(nOfRecords, 4)[0];
            headerBytes[5] = nConverter.SplitDenaryToBytes(nOfRecords, 4)[1];
            headerBytes[6] = nConverter.SplitDenaryToBytes(nOfRecords, 4)[2];
            headerBytes[7] = nConverter.SplitDenaryToBytes(nOfRecords, 4)[3];
            // Number of records

            headerBytes[8] = nConverter.SplitDenaryToBytes(nHeaderSize, 2)[0];
            headerBytes[9] = nConverter.SplitDenaryToBytes(nHeaderSize, 2)[1];
            // Position of first data record

            headerBytes[10] = nConverter.SplitDenaryToBytes(nRecordSize, 2)[0];
            headerBytes[11] = nConverter.SplitDenaryToBytes(nRecordSize, 2)[1];
            // Record size

            for (int i = 12; i <= 27; i++)
            {
                headerBytes[i] = 0;
            }
            // Reserved

            headerBytes[28] = 0;
            // Table flags

            headerBytes[29] = 0;
            // Code page marks

            headerBytes[30] = 0; headerBytes[31] = 0;
            // Reserved

            // Field Sub-Records :

            int bPos = 32;
            for (int i = 0; i < nOfFields; i++)
            {
                byte[] fieldName = nConverter.ConvertStringToByteArray(sFieldNames[i]);
                for (int x = 0; x < fieldName.Length; x++)
                {
                    headerBytes[bPos + x] = fieldName[x];
                }
                bPos += 11; // Move bPos to 11

                switch (fFieldTypes[i])
                {
                    case (FieldTypes.Character):
                        headerBytes[bPos] = nConverter.ConvertStringToByteArray("C")[0];
                        break;
                    case (FieldTypes.CharacterBin):
                        headerBytes[bPos] = nConverter.ConvertStringToByteArray("C")[0];
                        break;
                    case (FieldTypes.Currency):
                        headerBytes[bPos] = nConverter.ConvertStringToByteArray("Y")[0];
                        break;
                    case (FieldTypes.Date):
                        headerBytes[bPos] = nConverter.ConvertStringToByteArray("D")[0];
                        break;
                    case (FieldTypes.DateTime):
                        headerBytes[bPos] = nConverter.ConvertStringToByteArray("T")[0];
                        break;
                    case (FieldTypes.Double):
                        headerBytes[bPos] = nConverter.ConvertStringToByteArray("B")[0];
                        break;
                    case (FieldTypes.Float):
                        headerBytes[bPos] = nConverter.ConvertStringToByteArray("F")[0];
                        break;
                    case (FieldTypes.General):
                        headerBytes[bPos] = nConverter.ConvertStringToByteArray("G")[0];
                        break;
                    case (FieldTypes.Integer):
                        headerBytes[bPos] = nConverter.ConvertStringToByteArray("I")[0];
                        break;
                    case (FieldTypes.Logical):
                        headerBytes[bPos] = nConverter.ConvertStringToByteArray("L")[0];
                        break;
                    case (FieldTypes.Memo):
                        headerBytes[bPos] = nConverter.ConvertStringToByteArray("M")[0];
                        break;
                    case (FieldTypes.MemoBin):
                        headerBytes[bPos] = nConverter.ConvertStringToByteArray("M")[0];
                        break;
                    case (FieldTypes.Numeric):
                        headerBytes[bPos] = nConverter.ConvertStringToByteArray("N")[0];
                        break;
                    case (FieldTypes.Picture):
                        headerBytes[bPos] = nConverter.ConvertStringToByteArray("P")[0];
                        break;
                }
                bPos++;

                headerBytes[bPos] = 0; headerBytes[bPos + 1] = 0; headerBytes[bPos + 2] = 0; headerBytes[bPos + 3] = 0;
                bPos += 4;
                // The memory location, not useful!

                headerBytes[bPos] = (byte)nFieldLength[i];
                // Set the field length

                bPos++;

                headerBytes[bPos] = 0;
                // Field decimal count

                for (int x = bPos; x <= bPos + 14; x++)
                {
                    headerBytes[x] = 0;
                }
                // Fill in the blanks

                bPos += 3;
                headerBytes[bPos] = 0;

                bPos += 12;

            }

            headerBytes[nHeaderSize - 2] = (byte)0x0D;
            headerBytes[nHeaderSize - 1] = (byte)0x00;

            return headerBytes;
        }

        /// <summary>
        /// Changes the number of records in the table
        /// </summary>
        /// <param name="nToChangeBy">The number to change by</param>
        public void ChangeNumberOfRecords(int nToChangeBy)
        {
            nOfRecords += nToChangeBy;
        }

    }
}
