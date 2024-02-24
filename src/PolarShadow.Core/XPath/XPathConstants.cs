using System;
using System.Collections.Generic;
using System.Text;

namespace PolarShadow.Core
{
    internal static class XPathConstants
    {
        public const byte Step = (byte)'/';
        public const byte A = (byte)'A';
        public const byte Z = (byte)'Z';
        public const byte a = (byte)'a';
        public const byte z = (byte)'z';
        public const byte LeftBrack = (byte)'[';
        public const byte RightBrack = (byte)']';
        public const byte At = (byte)'@';
        public const byte Space = (byte)' ';
        public const byte Dot = (byte)'.';

        public static ReadOnlySpan<byte> EndChars => new byte[]
        { (byte)' ', (byte)'}'};
    }
}
