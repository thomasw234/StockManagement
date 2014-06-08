using System;
using System.IO;
using System.Text;

namespace DBFDetailsViewerV2
{

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
