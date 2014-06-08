using System;
using System.IO;

namespace DBFDetailsViewerV2
{
    /// <summary>
    /// A Table is made up of an array of Records. A record has field data.
    /// </summary>
    public class Record
    {
        /// <summary>
        /// The data which is contained in each field
        /// </summary>
        public string[] fieldData;
        /// <summary>
        /// The number of fields in the record
        /// </summary>
        int nOfFields;
        /// <summary>
        /// Whether or not the record has changed since it was loaded
        /// </summary>
        bool bChanged = true;
        /// <summary>
        /// The record data as a byte array
        /// </summary>
        byte[] recordData;
        /// <summary>
        /// Whether the record has been marked as deleted or not
        /// </summary>
        bool bDeleted = false;

        /// <summary>
        /// Initialises the record
        /// </summary>
        /// <param name="nOFields">The number of fields</param>
        public Record (int nOFields)
        {
            fieldData = new string[nOFields];
            nOfFields = nOFields;
        }
        /// <summary>
        /// Initialises the record
        /// </summary>
        /// <param name="dataToPutInField">The data to go in fields</param>
        /// <param name="nFieldLengths">The length of each field (number of characters)</param>
        public Record (string[] dataToPutInField, int[] nFieldLengths)
        {
            fieldData = dataToPutInField;
            nOfFields = fieldData.Length;
            recordData = ConvertRecordToBytes(nFieldLengths);
            bChanged = false;
            TrimEndingZeros();
        }
        /// <summary>
        /// Initialises the record
        /// </summary>
        /// <param name="commaSeperatedDataToPutInField">A comma seperated string with the field data</param>
        public Record (string commaSeperatedDataToPutInField)
        {
            fieldData = commaSeperatedDataToPutInField.Split(',');
            nOfFields = fieldData.Length;
            TrimEndingZeros();
        }
        /// <summary>
        /// Initialises the record
        /// </summary>
        /// <param name="CombinedString">A character seperated string containing the field data</param>
        /// <param name="splitChar">The character that splits the fields</param>
        public Record (string CombinedString, char splitChar)
        {
            fieldData = CombinedString.Split(splitChar);
            nOfFields = fieldData.Length;
            TrimEndingZeros();
        }

        /// <summary>
        /// Checks to see if the string to search for exists
        /// </summary>
        /// <param name="sToSearchFor">The string to search for</param>
        /// <returns></returns>
        public bool CheckForSearchData(string sToSearchFor)
        {
            foreach (string field in fieldData)
            {
                if (field.ToUpper().Contains(sToSearchFor))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Searches the field nFieldNum for sToSearchFor
        /// </summary>
        /// <param name="sToSearchFor">The string to search for</param>
        /// <param name="nFieldNum">The field to search in</param>
        /// <returns></returns>
        public bool CheckForSearchData(string sToSearchFor, int nFieldNum)
        {
            if (fieldData[nFieldNum].ToUpper().Contains(sToSearchFor.ToUpper()))
                return true;
            return false;
        }
        /// <summary>
        /// Searches for data in the specified field
        /// </summary>
        /// <param name="sToSearchFor">The data to search for</param>
        /// <param name="nFieldNum">The field to search in</param>
        /// <param name="bExactSearch">Whether or not to do an exact search. E.g. searching for "THOM" if field data is "THOMAS" would return false</param>
        /// <returns></returns>
        public bool CheckForSearchData(string sToSearchFor, int nFieldNum, bool bExactSearch)
        {
            if (bExactSearch)
            {
                if (fieldData[nFieldNum].ToUpper().TrimEnd(' ') == sToSearchFor.ToUpper().TrimEnd(' '))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the contents of the record
        /// </summary>
        /// <returns>The contents of the record in a string array</returns>
        public string[] GetRecordContents()
        {
            string[] sToReurn = new string[fieldData.Length];
            Array.Copy(fieldData, sToReurn, fieldData.Length);
                for (int i = 0; i < sToReurn.Length; i++)
                {
                    if (sToReurn[i] != null)
                        sToReurn[i] = sToReurn[i].TrimStart(' ');
                }
            
            return sToReurn;
        }
        /// <summary>
        /// Gets the contents of the specified field in the record
        /// </summary>
        /// <param name="fieldNum">The field to get the contents of</param>
        /// <returns>The contents of the specified field</returns>
        public string GetRecordContents(int fieldNum)
        {
            return fieldData[fieldNum].TrimStart(' ');
        }

        /// <summary>
        /// The number of fields in this record
        /// </summary>
        /// <returns>The number of fields</returns>
        public int NumberOfFields
        {
            get
            {
                return nOfFields;
            }
        }

        /// <summary>
        /// Checks to see whether the record is occupied
        /// </summary>
        /// <returns>True if there is any data</returns>
        public bool ContainsAnyData()
        {
            for (int i = 0; i < fieldData.Length; i++)
            {
                if (fieldData[i] != "" && fieldData[i] != null)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Checks a field to see if there is any data in it
        /// </summary>
        /// <param name="nFieldNum">The field to check</param>
        /// <returns>True if there is any data in the field</returns>
        public bool ContainsAnyData(int nFieldNum)
        {
            if (fieldData[nFieldNum] != "" && fieldData[nFieldNum] != null)
                return true;
            return false;
        }

        /// <summary>
        /// Puts the data passed into the record
        /// </summary>
        /// <param name="dataToEnter">The data to put in the record</param>
        public void PutDataInRecord(string[] dataToEnter, ref HeaderData tHeader)
        {
            bChanged = true;
            fieldData = dataToEnter;
            for (int i = 0; i < fieldData.Length; i++)
            {
                if (tHeader.fFieldTypes[i] == HeaderData.FieldTypes.Numeric)
                {
                    while (fieldData[i].Length != tHeader.nFieldLength[i])
                    {
                        fieldData[i] = " " + fieldData[i];
                    }
                }
            }
            TrimEndingZeros();
        }
        /// <summary>
        /// Puts data in a single field of the record
        /// </summary>
        /// <param name="dataToEnter">The data to enter</param>
        /// <param name="nFieldNum">The field number to enter the data into</param>
        public void PutDataInRecord(string dataToEnter, int nFieldNum, ref HeaderData tHeader)
        {
            bChanged = true;
            if (tHeader.fFieldTypes[nFieldNum] == HeaderData.FieldTypes.Numeric)
            {
                while (dataToEnter.Length != tHeader.nFieldLength[nFieldNum])
                {
                    dataToEnter = " " + dataToEnter;
                }
            }
            fieldData[nFieldNum] = dataToEnter; //  Caused problems
            TrimEndingZeros();
        }

        /// <summary>
        /// Removes the ending \0's that are added when a record is loaded for some reason
        /// </summary>
        private void TrimEndingZeros()
        {
            if (fieldData == null)
            {
                GTill.ErrorHandler.LogError("TrimEndZeros has been called when fieldData is null! Non-fatal, press Continue to carry on using the till. (Check the dBase file isn't corrupted)");
                throw new NotSupportedException("TrimEndZeros has been called when fieldData is null! Non-fatal, press Continue to carry on using the till.");
            }
            else
            {
                for (int i = 0; i < fieldData.Length; i++)
                {
                    fieldData[i] = fieldData[i].TrimEnd('\0');
                }
            }
        }

        /// <summary>
        /// Converts the record data into bytes ready to be saved to a file
        /// </summary>
        /// <param name="fieldLengths"></param>
        /// <returns></returns>
        public byte[] ConvertRecordToBytes(int[] fieldLengths)
        {
            if (!bChanged) // No point re-calculating the save data if the record hasn't changed, so returns the data that was loaded originally
                // This significantly speeds up the save process
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
                return toReturn;
            }
        }

        /// <summary>
        /// Marks the record as being deleted
        /// </summary>
        public void MarkAsDeleted()
        {
            bDeleted = true;
        }
    }

    /// <summary>
    /// A Table contains records and the table header data.
    /// </summary>
    public class Table
    {
        /// <summary>
        /// The records contained in the table
        /// </summary>
        Record[] rRecords;
        /// <summary>
        /// The field names of the table
        /// </summary>
        string[] sFieldNames;
        /// <summary>
        /// The file name of the record
        /// </summary>
        string sFileName;
        /// <summary>
        /// The header data of the record
        /// </summary>
        HeaderData tHeader;
        /// <summary>
        /// Whether or not the table has been sorted using QuickSort
        /// </summary>
        public bool bTableIsSorted = false;

        /// <summary>
        /// Initialises the table
        /// </summary>
        /// <param name="sFields">The field data</param>
        /// <param name="sFieldLengths">The length of each field</param>
        /// <param name="fft">The data type of each field</param>
        public Table(string[] sFields, int[] sFieldLengths, HeaderData.FieldTypes[] fft)
        {
            rRecords = new Record[1];
            rRecords[0] = new Record(sFields.Length);
            sFieldNames = sFields;
            tHeader = new HeaderData(sFields, sFieldLengths, fft);
        }
        /// <summary>
        /// Initialises the table
        /// </summary>
        /// <param name="dBaseFileName">The file name (including relative location) of the dBase III file to open</param>
        public Table(string dBaseFileName)
        { 
            byte[] bData = GetFileData(dBaseFileName);
            tHeader = new HeaderData(bData);
            rRecords = new Record[tHeader.nOfRecords];
            sFieldNames = tHeader.sFieldNames;
            string[,] sTable = new string[tHeader.nOfRecords, tHeader.nOfFields];
            ReadContentsOfFields(ref sTable, bData, tHeader);
            sFileName = dBaseFileName;

            for (int i = 0; i < rRecords.Length; i++)
            {
                string[] toShoveInRecord = new string[tHeader.nOfFields];
                for (int x = 0; x < tHeader.nOfFields; x++)
                {
                    toShoveInRecord[x] = sTable[i, x];
                }
                rRecords[i] = new Record(toShoveInRecord, tHeader.nFieldLength);
            }
        }

        /// <summary>
        /// Gets the number of records
        /// </summary>
        public int NumberOfRecords
        {
            get
            {
                return rRecords.Length;
            }
        }
        
        /// <summary>
        /// Adds a record to the table
        /// </summary>
        /// <param name="sRecord">The field data for the new record</param>
        public void AddRecord(string[] sRecord)
        {
            bool found = false;
            // Search for an empty record first
            for (int i = 0; i < rRecords.Length; i++)
            {
                if (!rRecords[i].ContainsAnyData())
                {
                    rRecords[i].PutDataInRecord(sRecord, ref tHeader);
                    found = true;
                    break;
                }
            }
            if (!found) // If no empty records, increase the length of the record array by one, and then insert the new data
            {
                Record[] temp = new Record[rRecords.Length];
                Array.Copy(rRecords, temp, rRecords.Length);
                rRecords = new Record[temp.Length + 1];
                for (int i = 0; i < temp.Length; i++)
                {
                    rRecords[i] = temp[i];
                }
                rRecords[temp.Length] = new Record(sRecord, tHeader.nFieldLength);
            }
            tHeader.ChangeNumberOfRecords(1);
            bTableIsSorted = false;
        }

        /// <summary>
        /// Edit the data in one field of one record
        /// </summary>
        /// <param name="nRecordNum">The record number to modify</param>
        /// <param name="nFieldNum">The field number to modify</param>
        /// <param name="newData">The data to put into the specified field</param>
        public void EditRecordData(int nRecordNum, int nFieldNum, string newData)
        {
            rRecords[nRecordNum].PutDataInRecord(newData, nFieldNum, ref tHeader);
            bTableIsSorted = false;
        }

        /// <summary>
        /// Searches to see if a record exists
        /// </summary>
        /// <param name="toSearchFor">The data to search for</param>
        /// <param name="fieldToSearch">The field number of every record to search in</param>
        /// <param name="nFieldNum">The field number that is currently being searched</param>
        /// <returns>True if found, and if found then nFieldNum is the location of the found record</returns>
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
        /// <summary>
        /// Searches for the existence of a record
        /// </summary>
        /// <param name="toSearchFor">The data to search for</param>
        /// <param name="fieldName">The name of the field to look in</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the contents of a record
        /// </summary>
        /// <param name="toSearchFor">The data to search for</param>
        /// <param name="fieldToSearch">The field number to search in</param>
        /// <returns>The contents of a record if found, otherwise returns a string array length 1, with the first element null.</returns>
        public string[] GetRecordFrom(string toSearchFor, int fieldToSearch)
        {
            for (int i = 0; i < rRecords.Length; i++)
            {
                if (rRecords[i].CheckForSearchData(toSearchFor, fieldToSearch))
                    return rRecords[i].GetRecordContents();
            }
            return (new Record(1).GetRecordContents());
        }
        /// <summary>
        /// Gets the contents of a record
        /// </summary>
        /// <param name="nRecordNumber">The number of the record to get the data from</param>
        /// <returns>The contents of the specified record</returns>
        public string[] GetRecordFrom(int nRecordNumber)
        {
            return rRecords[nRecordNumber].GetRecordContents();
        }
        /// <summary>
        /// Gets the contents of a record
        /// </summary>
        /// <param name="toSearchFor">The data to search for</param>
        /// <param name="fieldToSearch">The field number to search in</param>
        /// <param name="bExactSearch">Whether or not to do an exact search. E.g. searching for "THOM" if field data is "THOMAS" would return false</param>
        /// <returns>The contents of the record if found, otherwise returns a string array length 1, with the first element null.</returns>
        public string[] GetRecordFrom(string toSearchFor, int fieldToSearch, bool bExactSearch)
        {
            for (int i = 0; i < rRecords.Length; i++)
            {
                if (rRecords[i].CheckForSearchData(toSearchFor, fieldToSearch, bExactSearch))
                    return rRecords[i].GetRecordContents();
            }
            return (new Record(1).GetRecordContents());
        }

        /// <summary>
        /// Gets the contents of a record
        /// </summary>
        /// <param name="toSearchFor">The data to search for</param>
        /// <param name="fieldToSearch">The field number to search in</param>
        /// <param name="nStartPos">The record number to start searching</param>
        /// <param name="nRecordLocation">The record that is currently being searched</param>
        /// <returns>The contents of the record if found, and nRecordLocation contains the record number that is being returned</returns>
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
        /// <summary>
        /// Gets a record
        /// </summary>
        /// <param name="toSearchFor">The data to search for</param>
        /// <param name="fieldToSearch">The field number to search in</param>
        /// <param name="nStartPos">The start position of the search</param>
        /// <param name="nRecordLocation">The record that is currently being searched</param>
        /// <param name="bFoundNoMore">Will become false if all records have been searched and no more matches are found</param>
        /// <returns>A record that matches the search data</returns>
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
        /// <summary>
        /// Gets the contents of a record
        /// </summary>
        /// <param name="toSearchFor">The data to search for</param>
        /// <param name="fieldName">The name of the field to search in</param>
        /// <returns>The contents of the matching record if there is one, or an empty string array otherwise</returns>
        public string[] GetRecordFrom(string toSearchFor, string fieldName)
        {
            for (int i = 0; i < sFieldNames.Length; i++)
            {
                if (sFieldNames[i].ToUpper().Contains(fieldName.ToUpper()))
                    return GetRecordFrom(toSearchFor, i);
            }
            return (new Record(1).GetRecordContents());
        }

        /// <summary>
        /// Gets the field names of the current table
        /// </summary>
        /// <returns>The field names of the current table</returns>
        public string[] ReturnFieldNames()
        {
            return sFieldNames;
        }

        /// <summary>
        /// Saves to a dBase III format file
        /// </summary>
        /// <param name="fileName">The name (and relative path) of the file to save to</param>
        public void SaveToFile(string fileName)
        {
            byte[] header = GenerateHeaderData(); // Work out the byte structure of the header
            header = nConverter.AddByteToEndofAnotherByte(header, GenerateFieldData()); // Add on the bytes of the field data
            byte[] b = {0x1A};
            header = nConverter.AddByteToEndofAnotherByte(header, b); // Add the byte that signals the end of the file
            FileStream fsW = new FileStream(fileName, FileMode.Create);
            fsW.Write(header, 0, header.Length); // Write the file
            fsW.Close();
        
        }
        /// <summary>
        /// Generates the file contents
        /// </summary>
        /// <returns>An array of bytes which is the file contents</returns>
        public byte[] FileContents()
        {
            byte[] header = GenerateHeaderData();
            header = nConverter.AddByteToEndofAnotherByte(header, GenerateFieldData());
            header[header.Length - 1] = 0x1A;
            return header;
        }
        
        /// <summary>
        /// Gets all matching records based on a search term
        /// </summary>
        /// <param name="nField">The field number to search in</param>
        /// <param name="sToSearchFor">The data to search for</param>
        /// <returns>An array. Each element is another record, and the elements are split up with commas</returns>
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
        /// <summary>
        /// Gets all matching records based on a search term
        /// </summary>
        /// <param name="nField">The field number to search in</param>
        /// <param name="sToSearchFor">The data to search for</param>
        /// <param name="nOfRecords">The number of first dimensions</param>
        /// <returns>A 2D array of the records contents. The first dimension is a record</returns>
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
        /// <summary>
        /// Gets all matching records based on a few search terms
        /// </summary>
        /// <param name="nField">The field number to search in</param>
        /// <param name="sToSearchFor">The terms to search using</param>
        /// <param name="nOfRecords">The number of records that were found</param>
        /// <returns>A 2D array of record contents</returns>
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

        /// <summary>
        /// Deletes a record from the table
        /// </summary>
        /// <param name="nRecordNumber">The record number to delete</param>
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
            tHeader.ChangeNumberOfRecords(-1);
            bTableIsSorted = false;
        }

        // HIDDEN FROM PUBLIC VIEW

        /// <summary>
        /// Increases the number of records in the table
        /// </summary>
        /// <param name="rCurrentArray">The current record array</param>
        /// <param name="rNewRecord">The new record to add</param>
        /// <returns>A record array with the new record added to the end</returns>
        private Record[] rIncreaseRecordArray(Record[] rCurrentArray, Record rNewRecord)
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

        /// <summary>
        /// Gets the contents of a file
        /// </summary>
        /// <param name="sFileName">The file to open and read</param>
        /// <returns>A byte array of the file contents</returns>
        private byte[] GetFileData(string sFileName)
        {
            FileStream fs = new FileStream(sFileName, FileMode.Open);
            byte[] toReturn = new byte[fs.Length];
            fs.Read(toReturn, 0, (int)fs.Length);
            fs.Close();
            return toReturn;
        }
        /// <summary>
        /// Reads the byte array and converts it into a 2D string array of records
        /// </summary>
        /// <param name="sTable">The table to read the data into</param>
        /// <param name="dBaseFile">The bytes of the field data (not the header bytes!)</param>
        /// <param name="thd">The header data of the table</param>
        private void ReadContentsOfFields(ref string[,] sTable, byte[] dBaseFile, HeaderData thd)
        {
            
            for (int nRn = 0; nRn < thd.nOfRecords; nRn++)
            {
                for (int nFn = 0; nFn < thd.nOfFields; nFn++)
                {
                    string fieldContent = "";
                    int position = thd.nFieldDataStartsAt;
                    position += (nRn * thd.nRecordSize);
                    for (int i = 0; i < nFn; i++)
                        position += thd.nFieldLength[i];
                    for (int fPos = 0; fPos < thd.nFieldLength[nFn]; fPos++)
                    {
                        fieldContent += (char)dBaseFile[position];
                        position++;
                    }
                    sTable[nRn, nFn] = fieldContent;
                    fieldContent = "";
                }
            }

        }
        /// <summary>
        /// Generates the byte array containing the header data for the table in dBase III format
        /// </summary>
        /// <returns>A byte array of dBase III format header data</returns>
        public byte[] GenerateHeaderData()
        {
            return tHeader.ReturnHeaderData();
        }
        /// <summary>
        /// Generates the field data in dBase III format
        /// </summary>
        /// <returns>A byte array of field data</returns>
        public byte[] GenerateFieldData()
        {
            byte[] toReturn = new byte[tHeader.nOfRecords * tHeader.nRecordSize];
            int currentArrayPos = 0;
            for (int i = 0; i < rRecords.Length; i++)
            {
                byte[] temp = rRecords[i].ConvertRecordToBytes(tHeader.nFieldLength);
                for (int x = 0; x < temp.Length; x++)
                {
                    toReturn[currentArrayPos + x] = temp[x];
                }
                currentArrayPos += tHeader.nRecordSize;
            }

            return toReturn;
        }
    }

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

    /// <summary>
    /// A class with static functions to convert and merge various data formats. Used when reading/writing dBase III files
    /// </summary>
    class nConverter
    {
        /// <summary>
        /// Converts a binary string to a hexadecimal string
        /// </summary>
        /// <param name="binaryInput">The binary string to convert</param>
        /// <returns>The binary string converted to hexadecimal</returns>
        private static string ConvertBinaryToHex(string binaryInput)
        {
            try
            {
                int x = 0;
                string hexadecimalcharacters = "0123456789ABCDEF";
                string hexAnswer = "", tempHex = "";
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

        /// <summary>
        /// Converts a binary string to a decimal integer
        /// </summary>
        /// <param name="binaryInput">The binary to convert to decimal</param>
        /// <returns>The integer result</returns>
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

        /// <summary>
        /// Converts an integer into a binary string
        /// </summary>
        /// <param name="denaryInput">The integer to convert</param>
        /// <returns>The binary result</returns>
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

        /// <summary>
        /// Converts a hexadecimal string into a binary string
        /// </summary>
        /// <param name="hexInput">The hexadecimal to convert</param>
        /// <returns>The binary result</returns>
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

        /// <summary>
        /// Converts an integer into a hexadecimal string
        /// </summary>
        /// <param name="denaryInput">The integer to convert</param>
        /// <returns>The hexadecimal result</returns>
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

        /// <summary>
        /// Merges an array of bytes, and converts to an integer value
        /// </summary>
        /// <param name="bToMerge">The bytes to merge</param>
        /// <returns>The integer result</returns>
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

        /// <summary>
        /// Works out the bytes that represent an integer value, so a 32bit integer would take up 4 bytes.
        /// </summary>
        /// <param name="dInput">The integer to convert</param>
        /// <param name="nOfBytesToFill">The numer of bytes that should be filled (adds padding if too few bytes are used)</param>
        /// <returns>An array of bytes which represents the input integer</returns>
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

        /// <summary>
        /// Converts a string into an array of bytes
        /// </summary>
        /// <param name="toConvert">The string to convert</param>
        /// <returns>The byte array which represents the input string</returns>
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

        /// <summary>
        /// Merges 2 arrays of bytes into one array of bytes
        /// </summary>
        /// <param name="bOriginal">The array that will be at the front</param>
        /// <param name="bToAddOn">The array that will be at the rear</param>
        /// <returns>The merged array of bytes</returns>
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
