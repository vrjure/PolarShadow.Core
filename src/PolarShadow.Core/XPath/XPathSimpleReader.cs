using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PolarShadow.Core
{
    internal static class XPathSimpleReader
    {
        public static bool TryReadToEnd(ReadOnlySpan<byte> buffer, out int consume)
        {
            var index = consume = 0;
            bool inFilter = false;
            while (index < buffer.Length)
            {
                var ch = buffer[index];
                if (!inFilter && NameSlotConstants.XPathEndChars.IndexOf(ch) > -1)
                {
                    consume = index;
                    return true;
                }
                else if (ch == XPathConstants.LeftBrack)
                {
                    inFilter = true;
                }
                else if (ch == XPathConstants.RightBrack)
                {
                    inFilter = false;
                }

                index++;
            }
            return false;
        }
    }
}
