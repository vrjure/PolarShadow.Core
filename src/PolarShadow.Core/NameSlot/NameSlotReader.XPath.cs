using System;
using System.Collections.Generic;
using System.Text;

namespace PolarShadow.Core
{
    public partial struct NameSlotReader
    {
        private bool ReadToXPathEnd()
        {
            _segmentStart = _segmentEnd = _index;
            if (XPathSimpleReader.TryReadToEnd(_buffer.Slice(_segmentStart), out int consume))
            {
                _index = _segmentEnd = _segmentStart + consume - 1;
                return true;
            }
            return false;
        }
    }
}
