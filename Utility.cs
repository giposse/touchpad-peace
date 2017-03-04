using System;
using System.Collections.Generic;
using System.Text;
// using System.Threading.Tasks;

namespace TouchpadPeace2010
{
    internal static class Utility
    {
        public static byte[] Base64toByteArray(string base64String)
        {
            string base64Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789/+";
            int len = base64String.Length;
            int outputLength = 0;  // dummy value to calm the compiler

            // When the string has 1 more char than a multiple of 3, 
            // the base 64 string will have 2 more characters.  The converted
            // length will be ((len - 2) * 3 / 4) + 1 which solves to 
            // (3 * len - 2 ) / 4;
            // If the string has 2 more characters than a multiple of 3, the base
            // 64 string has 3 more characters (for the 2 ending), and the converted
            // length is ((len - 3) * 3 / 4) + 2 = (3 * len - 1) / 4;
            switch (len % 4)
            {
                case 0:
                case 1:
                    outputLength = 3 * len / 4;
                    break;
                case 2:
                    outputLength = (3 * len - 2) / 4;
                    break;
                case 3:
                    outputLength = (3 * len - 1) / 4;
                    break;
                default:
                    break;
            }

            byte[] returnValue = new byte[outputLength];
            int iByte = 0;
            int iOut = 0;
            int phase = 0;
            byte currentByte = 0;
            byte lastByte = 0;
            byte workByte;
            while (iByte < len)
            {
                char ch = base64String[iByte++];
                currentByte = (byte)base64Characters.IndexOf(ch);
                switch (phase)
                {
                    case 0:
                        lastByte = (byte)(currentByte << 2);
                        break;
                    case 1:
                        workByte = (byte)(lastByte | ((currentByte & 0x30) >> 4));
                        returnValue[iOut++] = workByte;
                        lastByte = (byte)((currentByte & 0x0F) << 4);
                        break;
                    case 2:
                        workByte = (byte)(lastByte | ((currentByte & 0x3C) >> 2));
                        returnValue[iOut++] = workByte;
                        lastByte = (byte)((currentByte & 0x03) << 6);
                        break;
                    case 3:
                        workByte = (byte)(lastByte | currentByte);
                        returnValue[iOut++] = workByte;
                        break;
                }

                phase = (phase + 1) & 0x03;   // equivalent to (phase + 1) % 4
            }

            return returnValue;
        }

        public static string ByteArrayToBase64(byte[] bufferIn)
        {
            int bufferSize = bufferIn.Length;
            string base64Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789/+";
            int outputLength = bufferSize / 3 * 4;
            switch (bufferSize % 3)
            {
                case 1: outputLength += 2; break;  // one char converts into 2 characters
                case 2: outputLength += 3; break;  // two chars convert to 3 chars
                default:
                    break;
            }

            int iByte = 0;
            int phase = 0;
            byte currentByte;
            byte lastByte = 0;
            byte index = 0;  // dummy assignment to calm the compiler
            StringBuilder returnValue = new StringBuilder(outputLength);
            while (iByte < bufferSize)
            {
                currentByte = bufferIn[iByte++];
                switch (phase)
                {
                    case 0:
                        index = (byte)((currentByte & 0xFC) >> 2);  // first 6 bits of the byte xxxx xxyy
                        lastByte = (byte)(currentByte << 6);
                        break;
                    case 1:
                        // first 4 bits of byte YYYY ZZZZ to be YY YYYY
                        index = (byte)((((currentByte & 0xF0) >> 2) | lastByte) >> 2);
                        lastByte = (byte)(currentByte << 4);
                        break;
                    case 2:
                        // first 2 bits of ZZAA AAAA to be ZZZZ  ZZ
                        index = (byte)((((currentByte & 0xC0) >> 4) | lastByte) >> 2);
                        lastByte = (byte)(currentByte & 0x3F);
                        break;
                }

                returnValue.Append(base64Characters[(int)index]);
                if (phase == 2)   // AA  AAAA
                    returnValue.Append(base64Characters[(int)lastByte]);

                phase = (phase + 1) % 3;
            }

            if (returnValue.Length < outputLength)
            {
                index = (byte)(lastByte >> 2);
                returnValue.Append(base64Characters[index]);
            }

            return returnValue.ToString();
        }
    }
}
