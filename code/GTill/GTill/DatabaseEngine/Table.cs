using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace DBFDetailsViewerV2
{
    /// <summary>
    /// A Table contains records and the table header data.
    /// </summary>
    public class Table : IDisposable
    {
        List<Record> rRecords;
        /// <summary>
        /// The records contained in the table
        /// </summary>
        //Record[] rRecords;
        /// <summary>
        /// The field names of the table
        /// </summary>
        string[] sFieldNames;
        /// <summary>
        /// The file name of the table
        /// </summary>
        string sFileName;
        /// <summary>
        /// The header data of the table
        /// </summary>
        HeaderData tHeader;
        /// <summary>
        /// Whether or not the table has been sorted using QuickSort
        /// </summary>
        public bool bTableIsSorted = false;
        /// <summary>
        /// Whether the table has actually been loaded. If false, then it will be loaded on the first request to use it
        /// </summary>
        public bool bTableIsActuallyLoaded = false;
        /// <summary>
        /// A dictionary for fast looking up fields
        /// </summary>
        FastIndex index;
        bool indexesCurrent = false;
        public bool indexingEnabled = true;

        string dBaseFileName = "";

        /// <summary>
        /// Initialises the table
        /// </summary>
        /// <param name="sFields">The field data</param>
        /// <param name="sFieldLengths">The length of each field</param>
        /// <param name="fft">The data type of each field</param>
        public Table(string[] sFields, int[] sFieldLengths, HeaderData.FieldTypes[] fft)
        {
            //rRecords = new Record[1];
            rRecords = new List<Record>();
            rRecords.Add(new Record(sFields.Length));
            sFieldNames = sFields;
            tHeader = new HeaderData(sFields, sFieldLengths, fft);
        }
        /// <summary>
        /// Initialises the table
        /// </summary>
        /// <param name="dBaseFileName">The file name (including relative location) of the dBase III file to open</param>
        public Table(string dBaseFileName)
        {
            // Removed for experiment
            // LoadTable()
            this.dBaseFileName = dBaseFileName;
        }

        public void ProperLoad()
        {
            if (!bTableIsActuallyLoaded)
                this.LoadTable();
        }

        private void LoadTable()
        {
            byte[] bData = GetFileData(dBaseFileName);
            tHeader = new HeaderData(bData);
            //rRecords = new Record[tHeader.nOfRecords];
            rRecords = new List<Record>(tHeader.nRecordCount * 2);
            sFieldNames = tHeader.sFieldNames;
            string[,] sTable = new string[tHeader.nOfRecords, tHeader.nOfFields];
            ReadContentsOfFields(ref sTable, bData, tHeader);
            sFileName = dBaseFileName;

            for (int i = 0; i < tHeader.nOfRecords; i++)
            {
                string[] toShoveInRecord = new string[tHeader.nOfFields];
                for (int x = 0; x < tHeader.nOfFields; x++)
                {
                    toShoveInRecord[x] = sTable[i, x];
                }
                rRecords.Add(new Record(toShoveInRecord, tHeader.nFieldLength, ref tHeader));
            }

            bTableIsActuallyLoaded = true;
            dtLastIndexUpdate = DateTime.Now;
        }

        /// <summary>
        /// Gets the number of records
        /// </summary>
        public int NumberOfRecords
        {
            get
            {
                if (!bTableIsActuallyLoaded)
                {
                    LoadTable();
                }
                return rRecords.Count;
            }
        }

        /// <summary>
        /// Adds a record to the table
        /// </summary>
        /// <param name="sRecord">The field data for the new record</param>
        public void AddRecord(string[] sRecord)
        {
            if (!bTableIsActuallyLoaded)
            {
                LoadTable();
            }
            bool found = false;

            /*if (!found) // If no empty records, increase the length of the record array by one, and then insert the new data
            {
                Array.Resize<Record>(ref rRecords, rRecords.Length + 1);
                rRecords[rRecords.Length - 1] = new Record(sRecord, tHeader.nFieldLength, ref tHeader);
            }*/
            rRecords.Add(new Record(sRecord, tHeader.nFieldLength, ref tHeader));
            tHeader.ChangeNumberOfRecords(1);
            bTableIsSorted = false;
            indexesCurrent = false;
        }

        /*public void ExpandNumberOfRecords(int nOfNewRecords)
        {
            if (!bTableIsActuallyLoaded)
            {
                LoadTable();
                bTableIsActuallyLoaded = true;
            }
            Array.Resize<Record>(ref rRecords, rRecords.Length + nOfNewRecords);
        }*/

        /// <summary>
        /// Edit the data in one field of one record
        /// </summary>
        /// <param name="nRecordNum">The record number to modify</param>
        /// <param name="nFieldNum">The field number to modify</param>
        /// <param name="newData">The data to put into the specified field</param>
        public void EditRecordData(int nRecordNum, int nFieldNum, string newData)
        {
            if (!bTableIsActuallyLoaded)
            {
                LoadTable();
            }
            rRecords[nRecordNum].PutDataInRecord(newData, nFieldNum, ref tHeader);
            bTableIsSorted = false;
            indexesCurrent = false;
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
            if (!bTableIsActuallyLoaded)
            {
                LoadTable();
            }

            // Try the indexes First
            if (!indexesCurrent)
                this.updateIndexes();

            if (indexesCurrent)
            {
                int result = index.getIndex(toSearchFor, fieldToSearch);
                if (result != -1)
                {
                    nFieldNum = result;
                    return true;
                }
            }

            for (int i = 0; i < rRecords.Count; i++)
            {
                if (rRecords[i].CheckForSearchData(toSearchFor, fieldToSearch, true))
                {
                    nFieldNum = i;
                    return true;
                }
            }
            return false;
            /*}
            else
            {
                int nHalfPos = rRecords.Length / 2;
                bool bMatch = false;
                int nPrevHalfPos = 0;
                while (!bMatch && nHalfPos != nPrevHalfPos)
                {
                    if (rRecords[nHalfPos].CheckForSearchData(toSearchFor, fieldToSearch, true))
                        bMatch = true;
                    else
                    {
                        int nOldHalfPos = nPrevHalfPos;
                        nPrevHalfPos = nHalfPos;
                        if (IsStringLeftOf(rRecords[nHalfPos].fieldData[nFieldNum], toSearchFor))
                            nHalfPos /= 2;
                        else
                            nHalfPos += (nOldHalfPos - nHalfPos) / 2;
                    }
                }

                return bMatch;
            }*/
        }

        /// <summary>
        /// Compares 2 strings to see which comes first in alphabetical order
        /// </summary>
        /// <param name="sMiddle">The middle value</param>
        /// <param name="sComparison">The value to compare. If this value is before the middle value, true will be returned</param>
        /// <returns>True if first var is after second var</returns>
        bool IsStringLeftOf(string sMiddle, string sComparison)
        {
            sMiddle = sMiddle.ToUpper();
            sComparison = sComparison.ToUpper();

            int nMaxLength = sMiddle.Length;
            if (sComparison.Length < nMaxLength)
                nMaxLength = sComparison.Length;

            for (int i = 0; i < nMaxLength; i++)
            {
                if ((int)sMiddle[i] > (int)sComparison[i])
                    return true;
                else if ((int)sMiddle[i] < (int)sComparison[i])
                    return false;
            }

            throw new NotSupportedException("Strings cannot be identical");
        }
        /// <summary>
        /// Searches for the existance of a record
        /// </summary>
        /// <param name="toSearchFor">The data to search for</param>
        /// <param name="fieldName">The name of the field to look in</param>
        /// <returns></returns>
        public bool SearchForRecord(string toSearchFor, string fieldName)
        {
            if (!bTableIsActuallyLoaded)
            {
                LoadTable();
            }
            int nFieldNum = 0;
            for (int i = 0; i < sFieldNames.Length; i++)
            {
                if (String.Compare(fieldName, sFieldNames[i], true) == 0)
                    return SearchForRecord(toSearchFor, i, ref nFieldNum);
            }
            return false;
        }
        /// <summary>
        /// A faster search if you can be sure that the data being searched is exact to the data in the records. Ideal for normal barcode search
        /// </summary>
        /// <param name="sToSearchFor">The data to search for</param>
        /// <param name="fieldToSearch">The field number to search in</param>
        /// <param name="nFieldNum">The record which it was found in</param>
        /// <returns></returns>


        /// <summary>
        /// Gets the contents of a record
        /// </summary>
        /// <param name="toSearchFor">The data to search for</param>
        /// <param name="fieldToSearch">The field number to search in</param>
        /// <returns>The contents of a record if found, otherwise returns a string array length 1, with the first element null.</returns>
        public string[] GetRecordFrom(string toSearchFor, int fieldToSearch)
        {
            if (!bTableIsActuallyLoaded)
            {
                LoadTable();
            }

            // First try the indexes
            if (!indexesCurrent)
                this.updateIndexes();

            if (indexesCurrent)
            {
                int result = index.getIndex(toSearchFor, fieldToSearch);
                if (result != -1)
                    return this.GetRecordFrom(result);
            }


            // Failing that, try every record in the database, yawn...
            for (int i = 0; i < rRecords.Count; i++)
            {
                if (rRecords[i].CheckForSearchData(toSearchFor, fieldToSearch))
                    return rRecords[i].GetRecordContents(ref tHeader);
            }
            return (new Record(1).GetRecordContents(ref tHeader));
        }
        /// <summary>
        /// Gets the contents of a record
        /// </summary>
        /// <param name="nRecordNumber">The number of the record to get the data from</param>
        /// <returns>The contents of the specified record</returns>
        public string[] GetRecordFrom(int nRecordNumber)
        {
            if (!bTableIsActuallyLoaded)
            {
                LoadTable();
            }
            return rRecords[nRecordNumber].GetRecordContents(ref tHeader);
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
            if (!bTableIsActuallyLoaded)
            {
                LoadTable();
            }

            // First, try the indexes
            if (!indexesCurrent)
                this.updateIndexes();

            if (indexesCurrent)
            {
                int result = index.getIndex(toSearchFor, fieldToSearch);
                if (result != -1)
                    return this.GetRecordFrom(result);
            }

            //If it's a multiple entry, then the index won't contain it, so search every field:
            // Arguably this code could be deleted: if it doesn't exist as a single entry then we shouldn't be calling this anyway
            // TODO: Consider this
            for (int i = 0; i < rRecords.Count; i++)
            {
                if (rRecords[i].CheckForSearchData(toSearchFor, fieldToSearch, bExactSearch))
                    return rRecords[i].GetRecordContents(ref tHeader);
            }
            return (new Record(1).GetRecordContents(ref tHeader));
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
            if (!bTableIsActuallyLoaded)
            {
                LoadTable();
            }
            for (int i = nStartPos; i < rRecords.Count; i++)
            {
                if (rRecords[i].CheckForSearchData(toSearchFor, fieldToSearch))
                {
                    nRecordLocation = i;
                    return rRecords[i].GetRecordContents(ref tHeader);
                }
            }
            return (new Record(1).GetRecordContents(ref tHeader));
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
            if (!bTableIsActuallyLoaded)
            {
                LoadTable();
            }
            for (int i = nStartPos; i < rRecords.Count; i++)
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
        /// Gets a record
        /// </summary>
        /// <param name="toSearchFor">The data to search for</param>
        /// <param name="fieldToSearch">The field number to search in</param>
        /// <param name="nStartPos">The start position of the search</param>
        /// <param name="nRecordLocation">The record that is currently being searched</param>
        /// <param name="bFoundNoMore">Will become false if all records have been searched and no more matches are found</param>
        /// <returns>A record that matches the search data</returns>
        public Record GetRecordFrom(string toSearchFor, int fieldToSearch, int nStartPos, ref int nRecordLocation, ref bool bFoundNoMore, bool bExact)
        {
            if (!bTableIsActuallyLoaded)
            {
                LoadTable();
            }
            for (int i = nStartPos; i < rRecords.Count; i++)
            {
                if (rRecords[i].CheckForSearchData(toSearchFor, fieldToSearch, bExact))
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
            if (!bTableIsActuallyLoaded)
            {
                LoadTable();
            }
            for (int i = 0; i < sFieldNames.Length; i++)
            {
                if (String.Compare(sFieldNames[i], fieldName, true) == 0)
                    //if (sFieldNames[i].ToUpper().Contains(fieldName.ToUpper()))
                    return GetRecordFrom(toSearchFor, i);
            }
            return (new Record(1).GetRecordContents(ref tHeader));
        }

        /// <summary>
        /// Checks for record based on two fields
        /// </summary>
        /// <param name="sFieldOne">The first search term</param>
        /// <param name="nFieldOne">The field to search in</param>
        /// <param name="sFieldTwo">The second search term</param>
        /// <param name="nFieldTwo">The second field to search in</param>
        /// <returns></returns>
        public int GetRecordNumberFromTwoFields(string sFieldOne, int nFieldOne, string sFieldTwo, int nFieldTwo)
        {
            if (!bTableIsActuallyLoaded)
            {
                LoadTable();
            }
            // First try the indexes on the first field. Hopefully this will be a primary key, and its location will be found quickly
            if (!indexesCurrent)
                this.updateIndexes();

            if (indexesCurrent)
            {
                int result = index.getIndex(sFieldOne, nFieldOne);
                if (result != -1)
                {
                    if (rRecords[result].CheckForSearchData(sFieldTwo, nFieldTwo, true))
                        return result;
                }

                // Next try the second field, just in case
                result = index.getIndex(sFieldTwo, nFieldTwo);
                if (result != -1)
                {
                    if (rRecords[result].CheckForSearchData(sFieldOne, nFieldOne, true))
                        return result;
                }
            }

            // Never mind, time for the slow way:
            for (int i = 0; i < rRecords.Count; i++)
            {
                if (rRecords[i].CheckForSearchData(sFieldOne, nFieldOne, true) && (rRecords[i].CheckForSearchData(sFieldTwo, nFieldTwo, true)))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Gets the field names of the current table
        /// </summary>
        /// <returns>The field names of the current table</returns>
        public string[] ReturnFieldNames()
        {
            if (!bTableIsActuallyLoaded)
            {
                LoadTable();
            }
            return sFieldNames;
        }

        DateTime dtLastIndexUpdate;
        /// <summary>
        /// This will only update the indexes if it was last updated over 500ms ago
        /// </summary>
        private void updateIndexes()
        {
            if (!indexingEnabled)
                return;
            if (dtLastIndexUpdate != null)
            {
                if (DateTime.Now.Subtract(dtLastIndexUpdate).TotalMilliseconds < 500)
                    return;
            }


            index = new FastIndex(this);
            indexesCurrent = true;
            dtLastIndexUpdate = DateTime.Now;
        }

        /// <summary>
        /// Pretty dangerous way to speedily add/edit several items to the database without having to save each time.
        /// Used in batch edit items
        /// </summary>
        public bool PreventFromSaving = false;

        /// <summary>
        /// Saves to a dBase III format file
        /// </summary>
        /// <param name="fileName">The name (and relative path) of the file to save to</param>
        public void SaveToFile(string fileName)
        {
            if (!bTableIsActuallyLoaded)
            {
                LoadTable();
            }
            if (!PreventFromSaving)
            {
                byte[] header = GenerateHeaderData(); // Work out the byte structure of the header
                header = nConverter.AddByteToEndofAnotherByte(header, GenerateFieldData()); // Add on the bytes of the field data
                byte[] b = { 0x1A };
                header = nConverter.AddByteToEndofAnotherByte(header, b); // Add the byte that signals the end of the file
                FileStream fsW = new FileStream(fileName, FileMode.Create);
                fsW.Write(header, 0, header.Length); // Write the file
                fsW.Close();
            }

        }
        /// <summary>
        /// Generates the file contents
        /// </summary>
        /// <returns>An array of bytes which is the file contents</returns>
        public byte[] FileContents()
        {
            if (!bTableIsActuallyLoaded)
            {
                LoadTable();
            }
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
            if (!bTableIsActuallyLoaded)
            {
                LoadTable();
            }
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
            string[] sToReturn = new string[rToReturn.Length];
            for (int i = 0; i < rToReturn.Length; i++)
            {
                sToReturn[i] = rToReturn[i].GetRecordContents(nField, ref tHeader);
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
            if (!bTableIsActuallyLoaded)
            {
                LoadTable();
            }
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
            string[,] sToReturn = new string[rToReturn.Length, sFieldNames.Length];
            for (int i = 0; i < rToReturn.Length; i++)
            {
                string[] sContents = rToReturn[i].GetRecordContents(ref tHeader);
                for (int z = 0; z < sContents.Length; z++)
                {
                    sToReturn[i, z] = sContents[z];
                }
            }
            nOfRecords = rToReturn.Length;
            return sToReturn;
        }
        public string[,] SearchAndGetAllMatchingRecords(int nField, string sToSearchFor, ref int nOfRecords, bool bExact)
        {
            if (!bTableIsActuallyLoaded)
            {
                LoadTable();
            }
            sToSearchFor = sToSearchFor.TrimEnd(' ');

            // Just on the off-chance that there's only one result, it's worth checking the indexes
            if (!indexesCurrent && bExact)
                updateIndexes();

            if (indexesCurrent && bExact)
            {
                int result = index.getIndex(sToSearchFor, nField);
                if (result != -1)
                {
                    string[] record = this.GetRecordFrom(result);
                    string[,] toReturn = new string[1, record.Length];
                    for (int i = 0; i < record.Length; i++)
                    {
                        toReturn[0, i] = record[i];
                    }
                    nOfRecords = 1;
                    return toReturn;
                }
            }

            int nCurrentField = 0;
            int nFindLocation = 0;
            Record[] rToReturn = new Record[0];
            bool bFoundNoMore = false;
            while (!bFoundNoMore)
            {
                Record rTemp = GetRecordFrom(sToSearchFor, nField, nCurrentField, ref nFindLocation, ref bFoundNoMore, bExact);
                nCurrentField = nFindLocation + 1;
                if (!bFoundNoMore)
                    rToReturn = rIncreaseRecordArray(rToReturn, rTemp);
            }
            string[,] sToReturn = new string[rToReturn.Length, sFieldNames.Length];
            for (int i = 0; i < rToReturn.Length; i++)
            {
                string[] sContents = rToReturn[i].GetRecordContents(ref tHeader);
                for (int z = 0; z < sContents.Length; z++)
                {
                    sToReturn[i, z] = sContents[z];
                }
            }
            nOfRecords = rToReturn.Length;
            return sToReturn;
        }
        public string[] SearchAndGetAllMatchingRecords(int nField, string sToSearchFor, ref int nOfRecords, bool bExact, int nFieldToReturn)
        {
            if (!bTableIsActuallyLoaded)
            {
                LoadTable();
            }
            /*
            sToSearchFor = sToSearchFor.TrimEnd(' ');
            int nCurrentField = 0;
            int nFindLocation = 0;
            Record[] rToReturn = new Record[0];
            bool bFoundNoMore = false;
            while (!bFoundNoMore)
            {
                Record rTemp = GetRecordFrom(sToSearchFor, nField, nCurrentField, ref nFindLocation, ref bFoundNoMore, bExact);
                nCurrentField = nFindLocation + 1;
                if (!bFoundNoMore)
                    rToReturn = rIncreaseRecordArray(rToReturn, rTemp);
            }
            string[] sToReturn = new string[rToReturn.Length];
            for (int i = 0; i < rToReturn.Length; i++)
            {
                sToReturn[i] = rToReturn[i].GetRecordContents(ref tHeader)[nFieldToReturn];
            }
            nOfRecords = rToReturn.Length;
            return sToReturn;
             * */

            sToSearchFor = sToSearchFor.TrimEnd(' ');

            // Just on the off-chance that there's only one result, it's worth checking the indexes
            if (!indexesCurrent && bExact)
                updateIndexes();

            if (indexesCurrent && bExact)
            {
                int result = index.getIndex(sToSearchFor, nField);
                if (result != -1)
                {
                    string[] record = this.GetRecordFrom(result);
                    nOfRecords = 1;

                    return new string[] { this.GetRecordFrom(result)[nFieldToReturn] };
                }
            }

            bool bFoundNoMore = false;
            int nCurrentField = 0;
            int nFindLocation = 0;
            string[] sToReturn = new string[0];
            while (!bFoundNoMore)
            {
                Record rTemp = GetRecordFrom(sToSearchFor, nField, nCurrentField, ref nFindLocation, ref bFoundNoMore, bExact);
                nCurrentField = nFindLocation + 1;
                if (!bFoundNoMore)
                {
                    Array.Resize<string>(ref sToReturn, sToReturn.Length + 1);
                    sToReturn[sToReturn.Length - 1] = rTemp.GetRecordContents(ref tHeader)[nFieldToReturn];
                }
            }
            nOfRecords = sToReturn.Length;
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
            if (!bTableIsActuallyLoaded)
            {
                LoadTable();
            }
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
                        //if (Strin(rTemp.GetRecordContents(ref tHeader)[nField], sTerm, true) == 1)
                        if (!rTemp.GetRecordContents(ref tHeader)[nField].ToUpper().Contains(sTerm.ToUpper()))
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
                string[] sContents = rToReturn[i].GetRecordContents(ref tHeader);
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
            if (!bTableIsActuallyLoaded)
            {
                LoadTable();
            }
            /*Record[] rTemp = rRecords;
            rRecords = new Record[rTemp.Length - 1];
            int nDiff = 0;
            for (int i = 0; i < rTemp.Length; i++)
            {
                if (i == nRecordNumber)
                    nDiff++;
                else
                    rRecords[i - nDiff] = rTemp[i];
            }*/
            rRecords.RemoveAt(nRecordNumber);
            tHeader.ChangeNumberOfRecords(-1);
            indexesCurrent = false;
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
            /*
            Record[] rTemp = rCurrentArray;
            rCurrentArray = new Record[rTemp.Length + 1];
            for (int i = 0; i < rTemp.Length; i++)
            {
                rCurrentArray[i] = rTemp[i];
            }
            rCurrentArray[rTemp.Length] = rNewRecord;
            return rCurrentArray;
             */
            Array.Resize<Record>(ref rCurrentArray, rCurrentArray.Length + 1);
            rCurrentArray[rCurrentArray.Length - 1] = rNewRecord;
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
                    //StringBuilder fieldContent = new StringBuilder();
                    int position = thd.nFieldDataStartsAt;
                    position += (nRn * thd.nRecordSize);
                    for (int i = 0; i < nFn; i++)
                        position += thd.nFieldLength[i];
                    /*for (int fPos = 0; fPos < thd.nFieldLength[nFn]; fPos++)
                    {
                        //fieldContent.Append(dBaseFile[position]);
                        fieldContent += (char)dBaseFile[position];
                        position++;
                    }*/

                    fieldContent = Encoding.Default.GetString(dBaseFile, position, thd.nFieldLength[nFn]);
                    position += thd.nFieldLength[nFn];
                    sTable[nRn, nFn] = fieldContent;
                    //fieldContent = new StringBuilder(null) ;
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
            for (int i = 0; i < rRecords.Count; i++)
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

        public int FieldNumber(string sFieldName)
        {
            if (!bTableIsActuallyLoaded)
            {
                LoadTable();
                bTableIsActuallyLoaded = true;
            }
            for (int i = 0; i < sFieldNames.Length; i++)
            {
                if (String.Compare(sFieldName, sFieldNames[i], true) == 0)
                //if (sFieldName.ToUpper() == sFieldNames[i].ToUpper().TrimEnd('\0'))
                {
                    return i;
                }
            }
            throw new NotImplementedException("Field not found");
            return -1;

        }

        public void SortTable()
        {
            if (!bTableIsActuallyLoaded)
            {
                LoadTable();
                bTableIsActuallyLoaded = true;
            }
            //Array.Sort<Record>(rRecords);
            if (!bTableIsSorted)
                rRecords.Sort();
            bTableIsSorted = true;
        }

        #region IDisposable Members

        public void Dispose()
        {
            ;
        }

        #endregion
    }
}