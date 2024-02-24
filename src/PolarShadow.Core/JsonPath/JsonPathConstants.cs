using System;
using System.Collections.Generic;
using System.Text;

namespace PolarShadow.Core
{
    internal static class JsonPathConstants
    {
        public const byte Root = (byte)'$';
        public const byte SelectCurrent = (byte)'@';
        public const byte Wildcard = (byte)'*';
        public const byte Dot = (byte)'.';
        public const byte Space = (byte)' ';
        public const byte StartFilter = (byte)'[';
        public const byte EndFilter = (byte)']';
        public const byte ArraySeparator = (byte)',';
        public const byte ExpressionStart = (byte)'?';
        public const byte Tab = (byte)'\t';
        public const byte Return = (byte)'\n';
        public const byte LeftBracket = (byte)'(';
        public const byte RightBracket = (byte)')';
        public const byte SingleQuote = (byte)'\'';
        public const byte Comma = (byte)',';
        public const byte Colon = (byte)':';
        public const byte RegexStart = (byte)'/';

        public const byte Equal = (byte)'=';
        public const byte Not = (byte)'!';
        public const byte LessThan = (byte)'<';
        public const byte GreaterThan = (byte)'>';
        public const byte MatchRegex = (byte)'~';
        public const byte Minus = (byte)'-';

        public const byte Num0 = (byte)'0';
        public const byte Num9 = (byte)'9';

        public const byte A = (byte)'A';
        public const byte Z = (byte)'Z';
        public const byte a = (byte)'a';
        public const byte z = (byte)'z';
        public const byte UnderLine = (byte)'_';

        public static ReadOnlySpan<byte> SkipChars => new byte[] { Space, Tab, Return };
        public static ReadOnlySpan<byte> PropertyEndChars => new byte[] { Dot, Space, StartFilter, ArraySeparator, Tab, Return, SingleQuote, RightBracket };
        public static ReadOnlySpan<byte> OperatorStartChars => new byte[] { Equal, Not, LessThan, GreaterThan};
        public static ReadOnlySpan<byte> OperatorChars => new byte[] { Equal, Not, LessThan, GreaterThan, MatchRegex};

        public static ReadOnlySpan<byte> InChars => new byte[] { (byte)'i', (byte)'n' };
        public static ReadOnlySpan<byte> NInChars => new byte[] { (byte)'n', (byte)'i', (byte)'n' };
        public static ReadOnlySpan<byte> SubsetOfChars => new byte[] { (byte)'s', (byte)'u', (byte)'b', (byte)'s', (byte)'e', (byte)'t', (byte)'o', (byte)'f' };
        public static ReadOnlySpan<byte> AnyOfChars => new byte[] { (byte)'a', (byte)'n', (byte)'y', (byte)'o', (byte)'f' };
        public static ReadOnlySpan<byte> NoneOfChars => new byte[] { (byte)'n', (byte)'o', (byte)'n', (byte)'e', (byte)'o', (byte)'f' };
        public static ReadOnlySpan<byte> SizeChars => new byte[] { (byte)'s', (byte)'i', (byte)'z', (byte)'e' };
        public static ReadOnlySpan<byte> EmptyChars => new byte[] { (byte)'e', (byte)'m', (byte)'p', (byte)'t', (byte)'y' };

        public static ReadOnlySpan<byte> RegexModifyChars => new byte[] { (byte)'g', (byte)'i', (byte)'m', (byte)'s' };

    }
}
