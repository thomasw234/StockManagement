using System;
using System.IO;

namespace DBFDetailsViewer
{
    public class Record
    {
        string[] fieldData;
        int nOfFields;
        bool bChanged = true;
        byte[] recordData;
        bool bDeleted = false;

        public Record (int nOFields)
        {
            fieldData = new string[nOFields];
            nOfFields = nOFields;
        }
        public Record (string[] dataToPutInField, int[] nFieldLengths)
        {
            fieldData = dataToPutInField;
            nOfFields = fieldData.Length;
            recordData = ConvertRecordToBytes(nFieldLengths);
            bChanged = false;
            TrimEndingZeros();
        }
        public Record (string commaSeperatedDataToPutInField)
        {
            fieldData = commaSeperatedDataToPutInField.Split(',');
            nOfFields = fieldData.Length;
            TrimEndingZeros();
        }
        public Record (string CombinedString, char splitChar)
        {
            fieldData = CombinedString.Split(splitChar);
            nOfFields = fieldData.Length;
            TrimEndingZeros();
        }

        public bool CheckForSearchData(string sToSearchFor)
        {
            foreach (string field in fieldData)
            {
                if (field.ToUpper().Contains(sToSearchFor))
                    return true;
            }
            return false;
        }
        public bool CheckForSearchData(string sToSearchFor, int nFieldNum)
        {
            if (fieldData[nFieldNum].ToUpper().Contains(sToSearchFor.ToUpper()))
                return true;
            return false;
        }
        public bool CheckForSearchData(string sToSearchFor, int nFieldNum, bool bExactSearch)
        {
            if (bExactSearch)
            {
                if (fieldData[nFieldNum].ToUpper().TrimEnd(' ') == sToSearchFor.ToUpper().TrimEnd(' '))
                    return true;
            }
            return false;
        }

        public string[] GetRecordContents()
        {
            return fieldData;
        }
        public string GetRecordContents(int fieldNum)
        {
            return fieldData[fieldNum];
        }

        public int NumberOfFields()
        {
            return nOfFields;
        }

        public bool ContainsAnyData()
        {
            for (int i = 0; i < fieldData.Length; i++)
            {
                if (fieldData[i] != "" && fieldData[i] != null)
                    return true;
            }
            return false;
        }
        public bool ContainsAnyData(int nFieldNum)
        {
            if (fieldData[nFieldNum] != "" && fieldData[nFieldNum] != null)
                return true;
            return false;
        }

        public void PutDataInRecord(string[] dataToEnter)
        {
            bChanged = true;
            fieldData = dataToEnter;
            TrimEndingZeros();
        }
        public void PutDataInRecord(string dataToEnter, int nFieldNum)
        {
            bChanged = true;
            fieldData[nFieldNum] = dataToEnter; //  Caused problems
            TrimEndingZeros();
        }

        private void TrimEndingZeros()
        {
            if (fieldData == null)
            {
                GTill.ErrorHandler.LogError("TrimEndZeros has been called when fieldData is null! Code error.");
                throw new NotSupportedException("TrimEndZeros has been called when fieldData is null! Code error.");
            }
            else
            {
                for (int i = 0; i < fieldData.Length; i++)
                {
                    fieldData[i] = fieldData[i].TrimEnd('\0');
                }
            }
        }

        public byte[] ConvertRecordToBytes(int[] fieldLengths)
        {
            if (!bChanged)
            {
                return recordData;
            }
            else
            {
                int sumOfFieldLengths = 0;
                int posInArray = 1;
                for (int i = 0; i < fieldLengths.Length; i++)
                {
                    sumOfFieldLengths += fieldLengths[i];
                }
                sumOfFieldLengths++;
                byte[] toReturn = new byte[sumOfFieldLengths];
                if (!bDeleted)
                    toReturn[0] = (byte)32;
                else
                    toReturn[0] = (byte)42;
                for (int i = 0; i < fieldData.Length; i++)
                {
                    bool endReached = false;
                    for (int x = 0; x < fieldLengths[i]; x++)
                    {
                        if (x == fieldData[i].Length)
                            endReached = true;
                        if (!endReached)
                        {
                            toReturn[posInArray] = (byte)fieldData[i][x];
                        }
                        else
                            toReturn[posInArray] = (byte)0;
                        posInArray++;
                    }
                }
                //toReturn[sumOfFieldLengths - 1] = 0x20;
                return toReturn;
            }
        }

        public void MarkAsDeleted()
        {
            bDeleted = true;
        }
    }

    public class Table
    {
        Record[] rRecords;
        string[] sFieldNames;
        string sFileName;
        HeaderData tHeader;
        VisibleHeaderData vHeader;
        string[,] sVisTable;

        public Table(string[] sFields, int[] sFieldLengths, HeaderData.FieldTypes[] fft)
        {
            rRecords = new Record[1];
            rRecords[0] = new Record(sFields.Length);
            vHeader = new VisibleHeaderData();
            sFieldNames = sFields;
            tHeader = new HeaderData(sFields, sFieldLengths, ref vHeader, fft);
        }
        /*public Table(int nOfFields, int nOfRecords)
        {
            rRecords = new Record[nOfRecords];
            for (int i = 0; i < rRecords.Length; i++)
            {
                rRecords[i] = new Record(nOfFields);
            }
            sFieldNames = new string[nOfFields];
        }*/
        /*public Table(string[,] sTable, int nOfRecords, int nOfFields)
        {
            rRecords = new Record[nOfRecords];
            for (int i = 0; i < rRecords.Length; i++)
            {
                string[] currentRecord = new string[nOfFields];
                for (int x = 0; x < nOfFields; x++)
                {
                    currentRecord[x] = sTable[i, x];
                }
                rRecords[i] = new Record(currentRecord);
            }
            sFieldNames = new string[nOfFields];
        }*/
        public Table(string dBaseFileName)
        {            
            vHeader = new VisibleHeaderData();
            byte[] bData = GetFileData(dBaseFileName);
            tHeader = new HeaderData(bData, ref vHeader);
            rRecords = new Record[vHeader.nOfRecords];
            sFieldNames = vHeader.sFieldNames;
            string[,] sTable = new string[vHeader.nOfRecords, vHeader.nOfFields];
            ReadContentsOfFields(ref sTable, bData, vHeader);
            sFileName = dBaseFileName;

            for (int i = 0; i < rRecords.Length; i++)
            {
                string[] toShoveInRecord = new string[vHeader.nOfFields];
                for (int x = 0; x < vHeader.nOfFields; x++)
                {
                    toShoveInRecord[x] = sTable[i, x];
                }
                rRecords[i] = new Record(toShoveInRecord, vHeader.nFieldLength);
            }
            sVisTable = sTable;
            
        }
        public Table(string dBaseFileName, ref Table tToWatch)
        {
            vHeader = new VisibleHeaderData();
            byte[] bData = GetFileData(dBaseFileName);
            tHeader = new HeaderData(bData, ref vHeader, ref tToWatch);
            rRecords = new Record[vHeader.nOfRecords];
            sFieldNames = vHeader.sFieldNames;
            string[,] sTable = new string[vHeader.nOfRecords, vHeader.nOfFields];
            ReadContentsOfFields(ref sTable, bData, vHeader);
            sFileName = dBaseFileName;

            for (int i = 0; i < rRecords.Length; i++)
            {
                string[] toShoveInRecord = new string[vHeader.nOfFields];
                for (int x = 0; x < vHeader.nOfFields; x++)
                {
                    toShoveInRecord[x] = sTable[i, x];
                }
                rRecords[i] = new Record(toShoveInRecord, vHeader.nFieldLength);
            }

        }

        public int NumberOfRecords
        {
            get
            {
                return rRecords.Length;
            }
        }
        
        public void AddRecord(string[] sRecord)
        {
            bool found = false;
            for (int i = 0; i < rRecords.Length; i++)
            {
                if (!rRecords[i].ContainsAnyData())
                {
                    rRecords[i].PutDataInRecord(sRecord);
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                Record[] temp = new Record[rRecords.Length];
                Array.Copy(rRecords, temp, rRecords.Length);
                rRecords = new Record[temp.Length + 1];
                for (int i = 0; i < temp.Length; i++)
                {
                    rRecords[i] = temp[i];
                }
                rRecords[temp.Length] = new Record(sRecord, vHeader.nFieldLength);
            }
            tHeader.ChangeNumberOfRecords(1, ref vHeader);
        }

        public void EditRecordData(int nRecordNum, int nFieldNum, string newData)
        {
            rRecords[nRecordNum].PutDataInRecord(newData, nFieldNum);
        }

        public bool SearchForRecord(string toSearchFor, int fieldToSearch, ref int nFieldNum)
        {
            for (int i = 0; i < rRecords.Length; i++)
            {
                if (rRecords[i].CheckForSearchData(toSearchFor, fieldToSearch, true))
                {
                    nFieldNum = i;
                    return true;
                }
            }
            return false;
        }
        public bool SearchForRecord(string toSearchFor, string fieldName)
        {
            int nFieldNum = 0;
            for (int i = 0; i < sFieldNames.Length; i++)
            {
                if (sFieldNames[i].ToUpper().Contains(fieldName.ToUpper()))
                    return SearchForRecord(toSearchFor, i, ref nFieldNum);
            }
            return false;
        }

        public string[] GetRecordFrom(string toSearchFor, int fieldToSearch)
        {
            for (int i = 0; i < rRecords.Length; i++)
            {
                if (rRecords[i].CheckForSearchData(toSearchFor, fieldToSearch))
                    return rRecords[i].GetRecordContents();
            }
            return (new Record(1).GetRecordContents());
        }
        public string[] GetRecordFrom(int nRecordNumber)
        {
            return rRecords[nRecordNumber].GetRecordContents();
        }
        public string[] GetRecordFrom(string toSearchFor, int fieldToSearch, bool bExactSearch)
        {
            for (int i = 0; i < rRecords.Length; i++)
            {
                if (rRecords[i].CheckForSearchData(toSearchFor, fieldToSearch, bExactSearch))
                    return rRecords[i].GetRecordContents();
            }
            return (new Record(1).GetRecordContents());
        }
        public string[] GetRecordFrom(string toSearchFor, int fieldToSearch, int nStartPos, ref int nRecordLocation)
        {
            for (int i = nStartPos; i < rRecords.Length; i++)
            {
                if (rRecords[i].CheckForSearchData(toSearchFor, fieldToSearch))
                {
                    nRecordLocation = i;
                    return rRecords[i].GetRecordContents();
                }
            }
            return (new Record(1).GetRecordContents());
        }

        public Record GetRecordFrom(string toSearchFor, int fieldToSearch, int nStartPos, ref int nRecordLocation, ref bool bFoundNoMore)
        {
            for (int i = nStartPos; i < rRecords.Length; i++)
            {
                if (rRecords[i].CheckForSearchData(toSearchFor, fieldToSearch))
                {
                    nRecordLocation = i;
                    bFoundNoMore = false;
                    return rRecords[i];
                }
            }
            bFoundNoMore = true;
            return (new Record(1));
        }
        public string[] GetRecordFrom(string toSearchFor, string fieldName)
        {
            for (int i = 0; i < sFieldNames.Length; i++)
            {
                if (sFieldNames[i].ToUpper().Contains(fieldName.ToUpper()))
                    return GetRecordFrom(toSearchFor, i);
            }
            return (new Record(1).GetRecordContents());
        }

        public string[] ReturnFieldNames()
        {
            return sFieldNames;
        }

        public void SaveToFile(string fileName)
        {
            byte[] header = GenerateHeaderData();
            header = nConverter.AddByteToEndofAnotherByte(header, GenerateFieldData());
            //header[header.Length - 1] = 0x1A;
            byte[] b = {0x1A};
            header = nConverter.AddByteToEndofAnotherByte(header, b);
            FileStream fsW = new FileStream(fileName, FileMode.Create);
            fsW.Write(header, 0, header.Length);
            fsW.Close();
        
        }
        public byte[] FileContents()
        {
            byte[] header = GenerateHeaderData();
            header = nConverter.AddByteToEndofAnotherByte(header, GenerateFieldData());
            header[header.Length - 1] = 0x1A;
            return header;
        }

        public string[,] GetTableStringArray()
        {
            return sVisTable;
        }

        public VisibleHeaderData GetVHD()
        {
            return vHeader;
        }


        public string[] SearchAndGetAllMatchingRecords(int nField, string sToSearchFor)
        {
            sToSearchFor = sToSearchFor.TrimEnd(' ');
            int nCurrentField = 0;
            int nFindLocation = 0;
            Record[] rToReturn = new Record[0];
            bool bFoundNoMore = false;
            while (!bFoundNoMore)
            {
                Record rTemp = GetRecordFrom(sToSearchFor, nField, nCurrentField, ref nFindLocation, ref bFoundNoMore);
                nCurrentField = nFindLocation+1;
                if (!bFoundNoMore)
                    rToReturn = rIncreaseRecordArray(rToReturn, rTemp);
            }
            string[] sToReturn = new string[rToReturn.Length];
            for (int i = 0; i < rToReturn.Length; i++)
            {
                sToReturn[i] = rToReturn[i].GetRecordContents(nField);
            }
            return sToReturn;
        }
        public string[,] SearchAndGetAllMatchingRecords(int nField, string sToSearchFor, ref int nOfRecords)
        {
            sToSearchFor = sToSearchFor.TrimEnd(' ');
            int nCurrentField = 0;
            int nFindLocation = 0;
            Record[] rToReturn = new Record[0];
            bool bFoundNoMore = false;
            while (!bFoundNoMore)
            {
                Record rTemp = GetRecordFrom(sToSearchFor, nField, nCurrentField, ref nFindLocation, ref bFoundNoMore);
                nCurrentField = nFindLocation + 1;
                if (!bFoundNoMore)
                    rToReturn = rIncreaseRecordArray(rToReturn, rTemp);
            }
            string[,] sToReturn = new string[rToReturn.Length,sFieldNames.Length];
            for (int i = 0; i < rToReturn.Length; i++)
            {
                string[] sContents = rToReturn[i].GetRecordContents();
                for (int z = 0; z < sContents.Length; z++)
                {
                    sToReturn[i, z] = sContents[z];
                }
            }
            nOfRecords = rToReturn.Length;
            return sToReturn;
        }
        public string[,] SearchAndGetAllMatchingRecords(int nField, string[] sToSearchFor, ref int nOfRecords)
        {
            for (int i = 0; i < sToSearchFor.Length; i++)
            {
                sToSearchFor[i] = sToSearchFor[i].TrimEnd(' ');
            }
            int nCurrentField = 0;
            int nFindLocation = 0;
            Record[] rToReturn = new Record[0];
            bool bFoundNoMore = false;
            while (!bFoundNoMore)
            {
                Record rTemp = GetRecordFrom(sToSearchFor[0], nField, nCurrentField, ref nFindLocation, ref bFoundNoMore);
                bool bContainsAllSearchTerms = true;
                if (!bFoundNoMore)
                {
                    foreach (string sTerm in sToSearchFor)
                    {
                        if (!rTemp.GetRecordContents()[nField].Contains(sTerm.ToUpper()))
                            bContainsAllSearchTerms = false;
                    }
                }
                nCurrentField = nFindLocation + 1;
                if (!bFoundNoMore && bContainsAllSearchTerms)
                    rToReturn = rIncreaseRecordArray(rToReturn, rTemp);
            }
            string[,] sToReturn = new string[rToReturn.Length, sFieldNames.Length];
            for (int i = 0; i < rToReturn.Length; i++)
            {
                string[] sContents = rToReturn[i].GetRecordContents();
                for (int z = 0; z < sContents.Length; z++)
                {
                    sToReturn[i, z] = sContents[z];
                }
            }
            nOfRecords = rToReturn.Length;
            return sToReturn;
        }

        public void DeleteRecord(int nRecordNumber)
        {
            Record[] rTemp = rRecords;
            rRecords = new Record[rTemp.Length - 1];
            int nDiff = 0;
            for (int i = 0; i < rTemp.Length; i++)
            {
                if (i == nRecordNumber)
                    nDiff++;
                else
                    rRecords[i - nDiff] = rTemp[i];
            }
            tHeader.ChangeNumberOfRecords(-1, ref vHeader);
        }

        // HIDDEN FROM PUBLIC VIEW

        static Record[] rIncreaseRecordArray(Record[] rCurrentArray, Record rNewRecord)
        {
            Record[] rTemp = rCurrentArray;
            rCurrentArray = new Record[rTemp.Length + 1];
            for (int i = 0; i < rTemp.Length; i++)
            {
                rCurrentArray[i] = rTemp[i];
            }
            rCurrentArray[rTemp.Length] = rNewRecord;
            return rCurrentArray;
        }

        static byte[] GetFileData(string sFileName)
        {
            FileStream fs = new FileStream(sFileName, FileMode.Open);
            byte[] toReturn = new byte[fs.Length];
            fs.Read(toReturn, 0, (int)fs.Length);
            fs.Close();
            return toReturn;
        }
        static void ReadContentsOfFields(ref string[,] sTable, byte[] dBaseFile, VisibleHeaderData vhd)
        {
            
            for (int nRn = 0; nRn < vhd.nOfRecords; nRn++)
            {
                for (int nFn = 0; nFn < vhd.nOfFields; nFn++)
                {
                    string fieldContent = "";
                    int position = vhd.nFieldDataStartsAt;
                    position += (nRn * vhd.nRecordSize);
                    for (int i = 0; i < nFn; i++)
                        position += vhd.nFieldLength[i];
                    for (int fPos = 0; fPos < vhd.nFieldLength[nFn]; fPos++)
                    {
                        fieldContent += (char)dBaseFile[position];
                        position++;
                    }
                    sTable[nRn, nFn] = fieldContent;
                    fieldContent = "";
                }
            }

        }
        public byte[] GenerateHeaderData()
        {
            return tHeader.ReturnHeaderData(vHeader);
        }
        public byte[] GenerateFieldData()
        {
            byte[] toReturn = new byte[vHeader.nOfRecords * vHeader.nRecordSize];
            int currentArrayPos = 0;
            for (int i = 0; i < rRecords.Length; i++)
            {
                byte[] temp = rRecords[i].ConvertRecordToBytes(vHeader.nFieldLength);
                for (int x = 0; x < temp.Length; x++)
                {
                    toReturn[currentArrayPos + x] = temp[x];
                }
                currentArrayPos += vHeader.nRecordSize;
            }

            return toReturn;
        }
    }

    public class HeaderData
    {
        public enum FieldTypes { Character, Currency, Numeric, Float, Date, DateTime, Double, Integer, Logical, Memo, General, CharacterBin, MemoBin, Picture };

        static bool validDbaseFile;
        static string lastDateModified; // YY/MM/DD
        static int nRecordCount;
        static int nFieldDataStartsAt;
        static int nOfRecords = 0;
        static int nRecordSize;
        static int nOfFields;
        static int[] nFieldLength;
        static string[] sFieldNames;
        static FieldTypes[] fFieldTypes;

        public HeaderData(string[] sFieldName, int[] nFieldLegnths, ref VisibleHeaderData vhd, FieldTypes[] fft)
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
        public HeaderData(byte[] dBFileData, ref VisibleHeaderData vhd)
        {
            ReadHeader(dBFileData);

            UpdateVisibleHeader(ref vhd);
        }
        public HeaderData(byte[] dBFileData, ref VisibleHeaderData vhd, ref Table tToWatch)
        {
            ReadHeader(dBFileData, ref tToWatch);

            UpdateVisibleHeader(ref vhd);
        } // For debugging purposes!

        static void ReadHeader(byte[] dbaseFile)
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
            //for (int i = nPos; i < nConverter.MergeBytesToDenary(headerSize); i++)
            {
                if (dbaseFile[i] == 13 && (i % 32) == 0)// nConverter.MergeBytesToDenary(headerSize) - i < 32)
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
        static void ReadHeader(byte[] dbaseFile, ref Table tToWatch)
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
            for (int i = nPos; i < nConverter.MergeBytesToDenary(headerSize); i++)
            {
                if (dbaseFile[i] == 13 && nConverter.MergeBytesToDenary(headerSize) - i < 32)
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


        } // For debugging purposes!

        public byte[] ReturnHeaderData(VisibleHeaderData vhd)
        {
            int nHeaderSize = 32 + (vhd.nOfFields * 32) + 2;
            byte[] headerBytes = new byte[nHeaderSize];

            headerBytes[0] = 3;
            // Make a valid dBase File

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

            headerBytes[4] = nConverter.SplitDenaryToBytes(vhd.nOfRecords, 4)[0];
            headerBytes[5] = nConverter.SplitDenaryToBytes(vhd.nOfRecords, 4)[1];
            headerBytes[6] = nConverter.SplitDenaryToBytes(vhd.nOfRecords, 4)[2];
            headerBytes[7] = nConverter.SplitDenaryToBytes(vhd.nOfRecords, 4)[3];
            // Number of records

            headerBytes[8] = nConverter.SplitDenaryToBytes(nHeaderSize, 2)[0];
            headerBytes[9] = nConverter.SplitDenaryToBytes(nHeaderSize, 2)[1];
            // Position of first data record

            headerBytes[10] = nConverter.SplitDenaryToBytes(vhd.nRecordSize, 2)[0];
            headerBytes[11] = nConverter.SplitDenaryToBytes(vhd.nRecordSize, 2)[1];
            // Record size

            for (int i = 12; i <= 27; i++)
            {
                headerBytes[i] = 0;
            }
            // Reserved

            headerBytes[28] = 0;
            // Table flags?

            headerBytes[29] = 0;
            // Code page marks

            headerBytes[30] = 0; headerBytes[31] = 0;
            // Reserved

            // Field Sub-Records :

            int bPos = 32;
            for (int i = 0; i < vhd.nOfFields; i++)
            {
                byte[] fieldName = nConverter.ConvertStringToByteArray(vhd.sFieldNames[i]);
                for (int x = 0; x < fieldName.Length; x++)
                {
                    headerBytes[bPos + x] = fieldName[x];
                }
                bPos += 11; // Move bPos to 11

                switch (vhd.fFieldTypes[i])
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

                headerBytes[bPos] = (byte)vhd.nFieldLength[i];
                // Set the field length

                bPos++;

                headerBytes[bPos] = 0;
                // Field decimal count? May need looking at later.

                for (int x = bPos; x <= bPos + 14; x++)
                {
                    headerBytes[x] = 0;
                }
                // Fill in the blanks

                bPos += 3;
                headerBytes[bPos] = 0; // Was set to one before...?

                bPos += 12;

            }

            headerBytes[nHeaderSize - 2] = (byte)0x0D;
            headerBytes[nHeaderSize - 1] = (byte)0x00;

            return headerBytes;
        }

        static void UpdateVisibleHeader(ref VisibleHeaderData vhd)
        {
            vhd.validDbaseFile = validDbaseFile;
            vhd.lastDateModified = lastDateModified;
            vhd.nRecordCount = nRecordCount;
            vhd.nFieldDataStartsAt = nFieldDataStartsAt;
            vhd.nOfRecords = nOfRecords;
            vhd.nRecordSize = nRecordSize;
            vhd.nOfFields = nOfFields;
            vhd.nFieldLength = nFieldLength;
            vhd.sFieldNames = sFieldNames;
            vhd.fFieldTypes = fFieldTypes;
        }

        public void ChangeNumberOfRecords(int nToChangeBy, ref VisibleHeaderData vhd)
        {
            nOfRecords += nToChangeBy;
            vhd.nOfRecords += nToChangeBy;
        }

    }

    public class VisibleHeaderData
    {
        public bool validDbaseFile;
        public string lastDateModified; // YY/MM/DD
        public int nRecordCount;
        public int nFieldDataStartsAt;
        public int nOfRecords;
        public int nRecordSize;
        public int nOfFields;
        public int[] nFieldLength;
        public string[] sFieldNames;
        public HeaderData.FieldTypes[] fFieldTypes;
    }

    public class QuickTable
    {
        HeaderData tHeader;
        VisibleHeaderData vHeader;
        byte[] bFileContents;
        string sFileName;

        public QuickTable(string dbfName)
        {
            sFileName = dbfName;
            bFileContents = LoadDataFromFile(sFileName);
            tHeader = new HeaderData(bFileContents, ref vHeader);
        }

        static byte[] LoadDataFromFile(string dbfFileName)
        {
            FileStream fs = new FileStream(dbfFileName, FileMode.Open);
            byte[] contents = new byte[fs.Length];
            fs.Read(contents, 0, (int)fs.Length);
            fs.Close();
            return contents;
        }

    }

    class nConverter
    {
        private static string ConvertBinaryToHex(string binaryInput)
        {
            try
            {
                int x = 0;
                string hexadecimalcharacters = "0123456789ABCDEF";
                string hexAnswer = "", tempHex = "", decimalHex = "";
                for (int i = binaryInput.Length - 1; i >= 0; i -= 1)
                {
                    if (x < 4)
                    {
                        tempHex = binaryInput[i].ToString() + tempHex;
                        x++;
                    }
                    if ((x == 4) || (i == 0))
                    {
                        if (tempHex.Length != 4)
                        {
                            tempHex = "0" + tempHex;
                        }
                        int temp = ConvertBinaryToDenary(tempHex);
                        // Now get the hex equivalent
                        hexAnswer = hexadecimalcharacters[temp] + hexAnswer;
                        x = 0;
                        tempHex = "";
                    }
                }
                return hexAnswer;
            }
            catch
            {
                return "Error!";
            }
        }

        private static int ConvertBinaryToDenary(string binaryInput)
        {
            try
            {
                char[] sDigits = binaryInput.ToCharArray();
                int[] nDigits = new int[sDigits.Length];
                for (int i = 0; i < sDigits.Length; i++)
                {
                    nDigits[i] = Convert.ToInt32(sDigits[i].ToString());
                }
                int currentBase = 1, finalnum = 0;
                for (int i = nDigits.Length - 1; i >= 0; i -= 1)
                {
                    if (nDigits[i] == 1)
                    {
                        finalnum += currentBase;
                    }
                    currentBase *= 2;
                }
                return finalnum;
            }
            catch
            {
                return 0;
            }
        }

        public static string ConvertDenaryToBinary(int denaryInput)
        {
            try
            {
                string reply = "";
                int currentDenary = denaryInput, currentBase = 1;
                while (currentBase < currentDenary)
                {
                    currentBase *= 2;
                }
                while (currentBase != 0)
                {
                    if (currentDenary >= currentBase)
                    {
                        reply += "1";
                        currentDenary -= currentBase;
                    }
                    else
                    {
                        reply += "0";
                    }
                    if (currentBase != 1)
                        currentBase /= 2;
                    else
                        currentBase = 0;
                }
                reply = reply.TrimStart('0');
                return reply;
            }
            catch
            {
                return "Error!";
            }
        }

        private static string ConvertHexToBinary(string hexInput)
        {
            string[] inHex = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
            string[] inBinary = { "0000", "0001", "0010", "0011", "0100", "0101", "0110", "0111", "1000", "1001", "1010", "1011", "1100", "1101", "1110", "1111" };
            string reply = "";
            foreach (char letter in hexInput)
            {
                int pos = 0;
                while (inHex[pos] != letter.ToString())
                {
                    pos++;
                }
                reply += inBinary[pos];
            }
            return reply;
        }

        public static string ConvertDenaryToHex(int denaryInput)
        {
            string Hex = ConvertBinaryToHex(ConvertDenaryToBinary(denaryInput));
            for (int i = 0; i < Hex.Length; i++)
            {
                if (Hex[i] == '0')
                    Hex.Remove(i, 1);
                else
                    break;
            }
            return Hex;
        }

        public static int MergeBytesToDenary(byte[] bToMerge)
        {
            string hex = "";
            foreach (byte b in bToMerge)
            {
                string sNextHex = ConvertDenaryToHex(b);
                while (sNextHex.Length < 2)
                    sNextHex = "0" + sNextHex;
                hex = sNextHex + hex;
            }
            return ConvertBinaryToDenary(ConvertHexToBinary(hex));
        }

        public static byte[] SplitDenaryToBytes(int dInput, int nOfBytesToFill)
        {
            string binary = ConvertDenaryToBinary(dInput);

            while ((binary.Length % 8) != 0)
                binary = "0" + binary;

            byte[] toReturn = new byte[binary.Length / 8];
            string[] unConverted = new string[binary.Length / 8];

            for (int i = 0; i < (binary.Length / 8); i++)
            {
                unConverted[i] = binary[i*8].ToString();
                unConverted[i] += binary[(i*8) + 1];
                unConverted[i] += binary[(i*8) + 2];
                unConverted[i] += binary[(i*8) + 3];
                unConverted[i] += binary[(i*8) + 4];
                unConverted[i] += binary[(i*8) + 5];
                unConverted[i] += binary[(i*8) + 6];
                unConverted[i] += binary[(i*8) + 7];
            }

            for (int i = 0; i < unConverted.Length; i++)
            {
                toReturn[i] = (byte)ConvertBinaryToDenary(unConverted[i]);
            }

            byte[] flipped = new byte[toReturn.Length];
            for (int i = 0; i < toReturn.Length; i++)
            {
                flipped[i] = toReturn[toReturn.Length - 1 - i];
            }

            if (flipped.Length < nOfBytesToFill)
            {
                byte[] temp = new byte[nOfBytesToFill];
                for (int i = 0; i < flipped.Length; i++)
                {
                    temp[i] = flipped[i];
                }
                for (int i = flipped.Length; i < temp.Length; i++)
                {
                    temp[i] = 0;
                }
                return temp;
            }

            return flipped;

        }

        public static byte[] ConvertStringToByteArray(string toConvert)
        {
            char[] cArray = toConvert.ToCharArray();
            byte[] toReturn = new byte[cArray.Length];

            for (int i = 0; i < cArray.Length; i++)
            {
                toReturn[i] = Convert.ToByte(cArray[i]);
            }

            return toReturn;
        }

        public static byte[] AddByteToEndofAnotherByte(byte[] bOriginal, byte[] bToAddOn)
        {
            byte[] result = new byte[bOriginal.Length + bToAddOn.Length];
            for (int i = 0; i < bOriginal.Length; i++)
            {
                result[i] = bOriginal[i];
            }
            for (int i = bOriginal.Length; i < result.Length; i++)
            {
                result[i] = bToAddOn[i - bOriginal.Length];
            }
            return result;
        }
    }
}
