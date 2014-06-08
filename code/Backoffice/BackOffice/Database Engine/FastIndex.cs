using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace BackOffice.Database_Engine
{
    class FastIndex
    {
        // A dictionary for every field. Each dictionary is <fieldContent, recordNumber>
        Dictionary<string, int>[] dictionaryList;

        public FastIndex(Table table)
        {
            dictionaryList = new Dictionary<string, int>[table.ReturnFieldNames().Length];
            for (int i = 0; i < dictionaryList.Length; i++)
            {
                dictionaryList[i] = new Dictionary<string, int>(table.NumberOfRecords, StringComparer.OrdinalIgnoreCase);
            }
                        int result = -1;

            for (int i = 0; i < table.NumberOfRecords; i++)
            {
                string[] recordContents = table.GetRecordFrom(i);
                for (int x = 0; x < recordContents.Length; x++)
                {
                    // Add as long as the key doesn't already exist
                    // If it does exist, then delete the current one so that duplicates aren't lost when searching
                    if (!dictionaryList[x].ContainsKey(recordContents[x]))
                        dictionaryList[x].Add(recordContents[x], i);
                    else
                    {
                        dictionaryList[x].TryGetValue(recordContents[x], out result);
                        if (result != -1)
                        {
                            dictionaryList[x].Remove(recordContents[x]);
                            dictionaryList[x].Add(recordContents[x], -1);
                        }
                    }
                }
            }
        }

        public int getIndex(string searchTerm, int field)
        {
            int result = -1;
            if (dictionaryList[field].TryGetValue(searchTerm, out result))
                return result;
            else return -1;
        }

    }
}
