using System;
using System.Collections.Generic;
using System.Text;

namespace PolarShadow.Core
{
    internal struct NameSlotReaderState
    {
        public NameSlotReaderState(int segStart, int segEnd, NameSlotTokenType tokenType)
        {
            SegmentStart = segStart;
            SegmentEnd = segEnd;
            TokenType = tokenType;
        }
        internal int SegmentStart { get; }
        internal int SegmentEnd { get; }
        internal NameSlotTokenType TokenType { get; }
    }
}
