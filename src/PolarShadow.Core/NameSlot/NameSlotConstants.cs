using System;
using System.Collections.Generic;
using System.Text;

namespace PolarShadow.Core
{
    internal static class NameSlotConstants
    {
        public const byte a = (byte)'a';
        public const byte z = (byte)'z';
        public const byte A = (byte)'A';
        public const byte Z = (byte)'Z';
        public const byte _ = (byte)'_';
        public const byte Num0 = (byte)'0';
        public const byte Num9 = (byte)'9';
        public const byte Add = (byte)'+';
        public const byte Minus = (byte)'-';
        public const byte Multiply = (byte)'*';
        public const byte Divide = (byte)'/';
        public const byte Space = (byte)' ';
        public const byte SlotStart = (byte)'{';
        public const byte SlotEnd = (byte)'}';
        public const byte JsonPathRoot = (byte)'$';
        public const byte Colon = (byte)':';
        public const byte At = (byte)'@';
        public const byte Tilde = (byte)'~';
        public const byte Apostrophe = (byte)'\'';
        public const byte LeftBrackets = (byte)'[';
        public const byte RightBrackets = (byte)']';

        public const byte Equal = (byte)'=';
        public const byte Not = (byte)'!';
        public const byte LessThan = (byte)'<';
        public const byte GreaterThan = (byte)'>';
        public const byte Question = (byte)'?';

        public static ReadOnlySpan<byte> StartEnd => new byte[]
        {
            (byte)'{', (byte)'}'
        };

        public static ReadOnlySpan<byte> XPathEndChars => new byte[]
        { (byte)' ', (byte)'}', (byte)':', (byte)'>', (byte)'<', (byte)'=', (byte)'!'};

        public static ReadOnlySpan<byte> ConditionExpressionStartChars => new byte[]
        {
           Equal, Not, LessThan, GreaterThan, Question
        };

        public static ReadOnlySpan<byte> RegexModifyChars => new byte[] { (byte)'g', (byte)'i', (byte)'m', (byte)'s' };
        public static ReadOnlySpan<byte> NumberFormatCommonChars => new byte[] 
        { 
            (byte)'C', (byte)'c',(byte)'E', (byte)'e',
            (byte)'F', (byte)'f', (byte)'G', (byte)'g',
            (byte)'N', (byte)'n', (byte)'P', (byte)'p',
        };

        public static ReadOnlySpan<byte> NumberFormatIntegralChars => new byte[]
        {
            (byte)'B', (byte)'b',
            (byte)'D', (byte)'d',
            (byte)'X', (byte)'x'
        };

        public static ReadOnlySpan<byte> NumberFormatR => new byte[]
        {
            (byte)'R', (byte)'r'
        };

        public static ReadOnlySpan<byte> UrlEncode => new byte[]
        {
            (byte)'u', (byte)'r', (byte)'l', (byte)'E', (byte)'n', (byte)'c', (byte)'o', (byte)'d',(byte)'e'
        };

        public static ReadOnlySpan<byte> TrimChars => new byte[]
        {
            (byte)'T',(byte)'r',(byte)'i',(byte)'m'
        };
    }
}
