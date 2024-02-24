using System;
using System.Collections.Generic;
using System.Text;

namespace PolarShadow.Core
{
    public partial struct NameSlotReader
    {
        private bool ReadParameterEnd()
        {
            _segmentStart = _segmentEnd = _index;
            while (CanRead())
            {
                if (IsParameterChar(_buffer[_index]))
                {
                    _index++;
                }
                else
                {
                    _tokenType = NameSlotTokenType.Parameter;
                    _segmentEnd = --_index;
                    return true;
                }
            }

            return false;
        }

        private bool IsParameterStart(byte ch)
        {
            return ch >= NameSlotConstants.A && ch <= NameSlotConstants.Z
                || ch >= NameSlotConstants.a && ch <= NameSlotConstants.z
                || ch == NameSlotConstants._;
        }
        private bool IsParameterChar(byte ch)
        {
            return ch >= NameSlotConstants.A && ch <= NameSlotConstants.Z
                || ch >= NameSlotConstants.a && ch <= NameSlotConstants.z
                || ch == NameSlotConstants._
                || ch >= NameSlotConstants.Num0 && ch <= NameSlotConstants.Num9;
        }
    }
}
