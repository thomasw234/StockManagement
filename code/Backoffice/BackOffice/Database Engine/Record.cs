using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackOffice.Database_Engine
{
    /// <summary>
    /// A Table is made up of an array of Records. A record has field data.
    /// </summary>
    public class Record : IComparable
    {
        /// <summary>
        /// The data which is contained in each field
        /// </summary>
        private string[] fieldData;
        /// <summary>
        /// The field data, just trimmed already
        /// </summary>
        private string[] trimmedFieldData;
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
        public Record(int nOFields)
        {
            fieldData = new string[nOFields];
            nOfFields = nOFields;
        }
        /// <summary>
        /// Initialises the record
        /// </summary>
        /// <param name="dataToPutInField">The data to go in fields</param>
        /// <param name="nFieldLengths">The length of each field (number of characters)</param>
        public Record(string[] dataToPutInField, int[] nFieldLengths, ref HeaderData tHeader)
        {
            fieldData = dataToPutInField;
            for (int i = 0; i < fieldData.Length; i++)
            {
                if (tHeader.fFieldTypes[i] == HeaderData.FieldTypes.Numeric)
                {
                    if (fieldData[i].TrimEnd(' ') == "")
                    {
                        fieldData[i] = "0";
                    }
                    else
                    {
                        fieldData[i] = fieldData[i].Replace(".00000", "");
                    }
                    while (fieldData[i].Length < tHeader.nFieldLength[i])
                    {
                        fieldData[i] = " " + fieldData[i];
                    }
                }
                else
                {
                    while (fieldData[i].Length < tHeader.nFieldLength[i])
                    {
                        fieldData[i] += " ";
                    }
                }
            }
            nOfFields = fieldData.Length;
            recordData = ConvertRecordToBytes(nFieldLengths);
            bChanged = false;
            TrimEndingZeros();

            trimmedFieldData = new string[fieldData.Length];
            reTrim();
        }
        private void reTrim()
        {
            for (int i = 0; i < fieldData.Length; i++)
            {
                trimmedFieldData[i] = fieldData[i].Trim();
            }
        }

        /// <summary>
        /// Initialises the record
        /// </summary>
        /// <param name="commaSeperatedDataToPutInField">A comma seperated string with the field data</param>
        public Record(string commaSeperatedDataToPutInField, ref HeaderData tHeader)
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
        public Record(string CombinedString, char splitChar, ref HeaderData tHeader)
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
                if (field.Equals(sToSearchFor, StringComparison.OrdinalIgnoreCase))
                //if (String.Compare(sToSearchFor, field, true) == 0)
                    //if (field.ToUpper().Contains(sToSearchFor.ToUpper()))
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
            //if (String.Compare(sToSearchFor, fieldData[nFieldNum], true) == 0) 
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
                if (trimmedFieldData[nFieldNum].Equals(sToSearchFor, StringComparison.OrdinalIgnoreCase))
                    return true;
                //if (String.Compare(fieldData[nFieldNum].TrimEnd(' ').TrimStart(' '), sToSearchFor, true) == 0)
                    //if (fieldData[nFieldNum].ToUpper().TrimEnd(' ').TrimStart(' ').ToUpper() == sToSearchFor.ToUpper().TrimEnd(' ').TrimStart(' ').ToUpper())
                  //  return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the contents of the record
        /// </summary>
        /// <returns>The contents of the record in a string array</returns>
        public string[] GetRecordContents(ref HeaderData tHeader)
        {
            string[] sToReurn = new string[fieldData.Length];
            Array.Copy(fieldData, sToReurn, fieldData.Length);
            for (int i = 0; i < sToReurn.Length; i++)
            {
                if (sToReurn[i] != null && tHeader.fFieldTypes[i] != HeaderData.FieldTypes.Numeric)
                    sToReurn[i] = sToReurn[i].Trim();
                else if (sToReurn[i] != null && tHeader.fFieldTypes[i] == HeaderData.FieldTypes.Numeric)
                    sToReurn[i] = sToReurn[i].TrimStart(' ');
            }

            return sToReurn;
        }
        /// <summary>
        /// Gets the contents of the specified field in the record
        /// </summary>
        /// <param name="fieldNum">The field to get the contents of</param>
        /// <returns>The contents of the specified field</returns>
        public string GetRecordContents(int fieldNum, ref HeaderData tHeader)
        {
            if (tHeader.fFieldTypes[fieldNum] != HeaderData.FieldTypes.Numeric)
                return fieldData[fieldNum].Trim();
            else
                return fieldData[fieldNum];
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
                else
                {
                    while (fieldData[i].Length != tHeader.nFieldLength[i])
                    {
                        fieldData[i] += " ";
                    }
                }
            }
            TrimEndingZeros();
            reTrim();
        }
        /// <summary>
        /// Puts data in a single field of the record
        /// </summary>
        /// <param name="dataToEnter">The data to enter</param>
        /// <param name="nFieldNum">The field number to enter the data into</param>
        public void PutDataInRecord(string dataToEnter, int nFieldNum, ref HeaderData tHeader)
        {
            bChanged = true;
            if (dataToEnter.Length > tHeader.nFieldLength[nFieldNum])
                throw new NotImplementedException("Data is too large to fit in field!");
            if (tHeader.fFieldTypes[nFieldNum] == HeaderData.FieldTypes.Numeric)
            {
                dataToEnter = String.Concat(new string(' ', tHeader.nFieldLength[nFieldNum] - dataToEnter.Length), dataToEnter);
                /*while (dataToEnter.Length != tHeader.nFieldLength[nFieldNum])
                {
                    dataToEnter = " " + dataToEnter;
                }*/
            }
            else
            {
                dataToEnter = String.Concat(dataToEnter, new string(' ', tHeader.nFieldLength[nFieldNum] - dataToEnter.Length));
                /*while (dataToEnter.Length < tHeader.nFieldLength[nFieldNum])
                {
                    dataToEnter += " ";
                }*/
            }
            fieldData[nFieldNum] = dataToEnter; //  Caused problems
            TrimEndingZeros();
            reTrim();
        }

        /// <summary>
        /// Removes the ending \0's that are added when a record is loaded for some reason
        /// </summary>
        private void TrimEndingZeros()
        {
            if (fieldData == null)
            {
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

        int IComparable.CompareTo(object obj)
        {
            Record r = (Record)obj;
            return string.Compare(fieldData[0], r.fieldData[0]);
        }
    }
}
