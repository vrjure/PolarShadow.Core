using System;
using System.Collections.Generic;
using System.Text;

namespace PolarShadow.Core
{
    public partial struct NameSlotReader
    {
        private bool ReadJsonPathEnd()
        {
            _segmentStart = _index;
            var reader = new JsonPathReader(_buffer, _segmentStart);
            var consume = reader.ReadToEnd();
            _index += consume - 1;
            _segmentEnd = _index;
            return true;
        }
    }
}
