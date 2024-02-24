using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Text;

namespace PolarShadow.Core
{
    public ref struct JsonPathReader
    {
        private readonly ReadOnlySpan<byte> _buffer;
        private JsonPathTokenType _tokenType;
        private int _currentPosition;
        private bool _inExpression;
        private bool _inExpressionFilter;
        private int _segmentStart;
        private int _segmentEnd;
        private bool _readFinal;

        public JsonPathReader(string jsonPath) : this(Encoding.UTF8.GetBytes(jsonPath))
        {

        }

        public JsonPathReader(ReadOnlySpan<byte> jsonPath)
        {
            if (jsonPath[0] != '$')
            {
                throw new ArgumentException("json path must start with '$'", nameof(jsonPath));
            }

            _buffer = jsonPath;
            _tokenType = JsonPathTokenType.None;
            _currentPosition = -1;
            _inExpression = _inExpressionFilter = false;
            _segmentStart = _segmentEnd = -1;
            _readFinal = false;
        }

        public JsonPathReader(ReadOnlySpan<byte> buffer, int start) : this(buffer.Slice(start))
        {
            
        }

        public JsonPathReader(ReadOnlySpan<byte> buffer, int start, int length) : this(buffer.Slice(start, length))
        {

        }

        public readonly JsonPathTokenType TokenType => _tokenType;
        public readonly int Position => _currentPosition;

        public JsonPathReaderState State => new JsonPathReaderState(_currentPosition, _tokenType, _inExpression, _inExpressionFilter);

        public void Reset(JsonPathReaderState state)
        {
            _currentPosition = state.Position;
            _tokenType = state.TokenType;
            _inExpression = state.InExpression;
            _inExpressionFilter = state.InExpressionFilter;
            if (_currentPosition < _buffer.Length)
            {
                _readFinal = false;
            }
        }

        public bool IsCompleted => _readFinal;

        public bool Read()
        {
            if (_readFinal)
            {
                throw new InvalidOperationException("read completed");
            }

            if (!TryRead())
            {
                _readFinal = true;
                if (IsEndToken(_tokenType))
                {
                    return false;
                }
                ThrowInvalidDataException();
            }
            return true;
        }

        public int ReadToEnd()
        {
            while (TryRead()) { }

            if (!IsEndToken(_tokenType))
            {
                ThrowInvalidDataException();
            }
            return _currentPosition;
        }

        private bool TryRead()
        {
            _currentPosition++;
            if (ShouldSkipInvaild())
                SkipInvaild();

            if (IsEnd()) return false;

            return _tokenType == JsonPathTokenType.None ? ReadRoot() : ReadNext();
        }

        public ReadOnlySpan<byte> Slice()
        {
            return _buffer.Slice(_segmentStart, _segmentEnd - _segmentStart + 1);
        }

        public string GetString()
        {
            return Encoding.UTF8.GetString(_buffer.Slice(_segmentStart, _segmentEnd - _segmentStart + 1));
        }

        public bool TryGetString(out string value)
        {
            value = string.Empty;
            try
            {
                value = Encoding.UTF8.GetString(_buffer.Slice(_segmentStart, _segmentEnd - _segmentStart + 1));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public int GetInt()
        {
            return int.Parse(GetString());
        }

        public bool TryGetInt(out int value)
        {
            value = 0;
            return TryGetString(out string s) && int.TryParse(s, out value);
        }

        public double GetDouble()
        {
            return double.Parse(GetString());
        }

        public bool TryGetDouble(out double value)
        {
            value = 0d;
            return TryGetString(out string s) && double.TryParse(s, out value);
        }

        public decimal GetDecimal()
        {
            return decimal.Parse(GetString());
        }

        public bool TryGetDecimal(out decimal value)
        {
            value = 0;
            return TryGetString(out string s) && decimal.TryParse(s, out value);
        }

        private bool ReadNext()
        {
            return _tokenType switch
            {
                JsonPathTokenType.Root => ReadRootNext(),
                JsonPathTokenType.Current => ReadSelectCurrentNext(),
                JsonPathTokenType.DeepScan => ReadChildOrDeepScanNext(),
                JsonPathTokenType.Child => ReadChildOrDeepScanNext(),
                JsonPathTokenType.Wildcard => ReadWildcardNext(),
                JsonPathTokenType.PropertyName => ReadPropertyNameNext(),
                JsonPathTokenType.String => ReadStringNext(),
                JsonPathTokenType.StartFilter => ReadStartFilterNext(),
                JsonPathTokenType.EndFilter => ReadEndFilterNext(),
                JsonPathTokenType.Number => ReadNumberNext(),
                JsonPathTokenType.Slice => ReadSliceNext(),
                JsonPathTokenType.StartExpression => ReadStartExpressionNext(),
                JsonPathTokenType.EndExpression => ReadEndExpressionNext(),
                _ => ReadOperateNext()
            };
        }

        private bool ReadRootNext()
        {
            var ch = _buffer[_currentPosition];
            if (ch == JsonPathConstants.StartFilter)
            {
                _tokenType = JsonPathTokenType.StartFilter;
                if (_inExpression)
                {
                    _inExpressionFilter = true;
                }
                return true;
            }
            return TryReadChildOrDeepScan();
        }

        private bool ReadStartFilterNext()
        {
            var ch = _buffer[_currentPosition];
            if (ch == JsonPathConstants.SingleQuote)
            {
                _tokenType = JsonPathTokenType.String;
                return ReadStringEnd();
            }
            else if (IsNumberCharStart(ch))
            {
                _tokenType = JsonPathTokenType.Number;
                return ReadIntegerEnd();
            }
            else if (!_inExpression && ch == JsonPathConstants.Colon)
            {
                _tokenType = JsonPathTokenType.Slice;
                return true;
            }
            else if (!_inExpression && ch == JsonPathConstants.Wildcard)
            {
                _tokenType = JsonPathTokenType.Wildcard;
                return true;
            }

            return TryReadStartExpression();
        }

        private bool ReadEndFilterNext()
        {
            var ch = _buffer[_currentPosition];
            if (ch == JsonPathConstants.Dot && NextCharIs(JsonPathConstants.Dot))
            {
                _tokenType = JsonPathTokenType.DeepScan;
                return true;
            }
            else if (ch == JsonPathConstants.Dot)
            {
                _tokenType = JsonPathTokenType.Child;
                return true;
            }
            else if (ch == JsonPathConstants.StartFilter)
            {
                _tokenType = JsonPathTokenType.StartFilter;
                return true;
            }
            else if (_inExpression && ch == JsonPathConstants.RightBracket)
            {
                _tokenType = JsonPathTokenType.EndExpression;
                return true;
            }
            return false;
        }

        private bool ReadPropertyNameNext()
        {
            var ch = _buffer[_currentPosition];
            if (ch == JsonPathConstants.RightBracket)
            {
                return ReadEndExpression();
            }
            else if (!_inExpression && ch == JsonPathConstants.StartFilter)
            {
                _tokenType = JsonPathTokenType.StartFilter;
                return true;
            }
            else if (_inExpression)
            {
                SkipInvaild();
                return ReadInExpressionPropertyNameEndNext();
            }

            return TryReadChildOrDeepScan();
        }

        private bool ReadInExpressionPropertyNameEndNext()
        {
            var ch = _buffer[_currentPosition];
            if (ch == JsonPathConstants.RightBracket)
            {
                _tokenType = JsonPathTokenType.EndExpression;
                return true;
            }
            if (ch == JsonPathConstants.Equal && NextCharIs(JsonPathConstants.Equal))
            {
                _tokenType = JsonPathTokenType.Equal;
                _currentPosition++;
                return true;
            }
            else if (ch == JsonPathConstants.LessThan && NextCharIs(JsonPathConstants.Equal))
            {
                _tokenType = JsonPathTokenType.LessThanOrEqual;
                _currentPosition++;
                return true;
            }
            else if (ch == JsonPathConstants.GreaterThan && NextCharIs(JsonPathConstants.Equal))
            {
                _tokenType = JsonPathTokenType.GreaterThanOrEqual;
                _currentPosition++;
                return true;
            }
            else if (ch == JsonPathConstants.Not && NextCharIs(JsonPathConstants.Equal))
            {
                _tokenType = JsonPathTokenType.NotEqual;
                _currentPosition++;
                return true;
            }
            else if (ch == JsonPathConstants.Equal && NextCharIs(JsonPathConstants.MatchRegex))
            {
                _tokenType = JsonPathTokenType.Matches;
                _currentPosition++;
                return true;
            }
            else if (ch == JsonPathConstants.LessThan)
            {
                _tokenType = JsonPathTokenType.LessThan;
                return true;
            }
            else if (ch == JsonPathConstants.GreaterThan)
            {
                _tokenType = JsonPathTokenType.GreaterThan;
                return true;
            }
            else if (StartsWithAndEndWithSpace(JsonPathConstants.InChars))
            {
                _tokenType = JsonPathTokenType.In;
                _currentPosition += JsonPathConstants.InChars.Length - 1;
                return true;
            }
            else if (StartsWithAndEndWithSpace(JsonPathConstants.NInChars))
            {
                _tokenType = JsonPathTokenType.NIn;
                _currentPosition += JsonPathConstants.NInChars.Length - 1;
                return true;
            }
            else if (StartsWithAndEndWithSpace(JsonPathConstants.SubsetOfChars))
            {
                _tokenType = JsonPathTokenType.Subsetof;
                _currentPosition += JsonPathConstants.SubsetOfChars.Length - 1;
                return true;
            }
            else if (StartsWithAndEndWithSpace(JsonPathConstants.AnyOfChars))
            {
                _tokenType = JsonPathTokenType.Anyof;
                _currentPosition += JsonPathConstants.AnyOfChars.Length - 1;
                return true;
            }
            else if (StartsWithAndEndWithSpace(JsonPathConstants.NoneOfChars))
            {
                _tokenType = JsonPathTokenType.Noneof;
                _currentPosition += JsonPathConstants.NoneOfChars.Length - 1;
                return true;
            }
            else if (StartsWithAndEndWithSpace(JsonPathConstants.SizeChars))
            {
                _tokenType = JsonPathTokenType.Size;
                _currentPosition += JsonPathConstants.SizeChars.Length - 1;
                return true;
            }
            else if (StartsWith(JsonPathConstants.EmptyChars))
            {
                _tokenType = JsonPathTokenType.Empty;
                _currentPosition += JsonPathConstants.EmptyChars.Length - 1;
                return true;
            }

            return false;
        }

        private bool ReadOperateNext()
        {
            var ch = _buffer[_currentPosition];
            if (ch == JsonPathConstants.Root)
            {
                _tokenType = JsonPathTokenType.Root;
                return true;
            }
            else if (ch == JsonPathConstants.SelectCurrent)
            {
                _tokenType = JsonPathTokenType.Current;
                return true;
            }
            else if (ch == JsonPathConstants.RightBracket)
            {
                 return ReadEndExpression();
            }
            else if (IsNumberCharStart(ch))
            {
                _tokenType = JsonPathTokenType.Number;
                return ReadNumberEnd();
            }
            else if (ch == JsonPathConstants.RegexStart)
            {
                _tokenType = JsonPathTokenType.Regex;
                return ReadRegexEnd();
            }
            else if (ch == JsonPathConstants.SingleQuote)
            {
                _tokenType = JsonPathTokenType.String;
                return ReadStringEnd();
            }

            return false;
        }

        private bool ReadStringNext()
        {
            var ch = _buffer[_currentPosition];
            if (ch == JsonPathConstants.Comma)
            {
                _currentPosition++;
                SkipInvaild();
                if (_buffer[_currentPosition] == JsonPathConstants.SingleQuote)
                {
                    _tokenType = JsonPathTokenType.String;
                    return ReadStringEnd();
                }
            }
            else if (ch == JsonPathConstants.EndFilter)
            {
                _tokenType = JsonPathTokenType.EndFilter;
                if (_inExpressionFilter)
                {
                    _inExpressionFilter = false;
                }
                return true;
            }
            else if (!_inExpressionFilter && ch == JsonPathConstants.RightBracket)
            {
                return ReadEndExpression();
            }

            return false;
        }

        private bool ReadNumberNext()
        {
            var ch = _buffer[_currentPosition];
            if (ch == JsonPathConstants.Comma)
            {
                _currentPosition++;
                SkipInvaild();
                if (IsNumberCharStart(_buffer[_currentPosition]))
                {
                    _tokenType = JsonPathTokenType.Number;
                    return ReadIntegerEnd();
                }
            }
            else if (ch == JsonPathConstants.EndFilter)
            {
                _tokenType = JsonPathTokenType.EndFilter;
                if (_inExpressionFilter)
                {
                    _inExpressionFilter = false;
                }
                return true;
            }
            else if (!_inExpressionFilter && ch == JsonPathConstants.RightBracket)
            {
                return ReadEndExpression();
            }
            else if (ch == JsonPathConstants.Colon)
            {
                _tokenType = JsonPathTokenType.Slice;
                return true;
            }

            return false;
        }

        private bool ReadChildOrDeepScanNext()
        {
            var ch = _buffer[_currentPosition];
            if (ch == JsonPathConstants.Wildcard)
            {
                _tokenType = JsonPathTokenType.Wildcard;
                return true;
            }
            else if (IsPropertyNameCharStart(ch))
            {
                _tokenType = JsonPathTokenType.PropertyName;
                return ReadPropertyNameEnd();
            }
            else if (ch == JsonPathConstants.StartFilter)
            {
                _tokenType = JsonPathTokenType.StartFilter;
                return true;
            }

            return false;
        }

        private bool ReadWildcardNext()
        {
            var ch = _buffer[_currentPosition];
            if (ch == JsonPathConstants.StartFilter)
            {
                _tokenType = JsonPathTokenType.StartFilter;
                return true;
            }
            else if (ch == JsonPathConstants.EndFilter)
            {
                _tokenType = JsonPathTokenType.EndFilter;
                return true;
            }
            return TryReadChildOrDeepScan();
        }

        private bool ReadSliceNext()
        {
            var ch = _buffer[_currentPosition];
            if (IsNumberCharStart(ch))
            {
                _tokenType = JsonPathTokenType.Number;
                return ReadIntegerEnd();
            }
            else if (ch == JsonPathConstants.EndFilter)
            {
                _tokenType = JsonPathTokenType.EndFilter;
                return true;
            }

            return false;
        }

        private bool ReadStartExpressionNext()
        {
            var ch = _buffer[_currentPosition];
            if (ch == JsonPathConstants.SelectCurrent)
            {
                _tokenType = JsonPathTokenType.Current;
                return true;
            }
            else if (ch == JsonPathConstants.Root)
            {
                _tokenType = JsonPathTokenType.Root;
                return true;
            }
            return false;
        }

        private bool ReadEndExpressionNext()
        {
            var ch = _buffer[_currentPosition];
            if (ch == JsonPathConstants.EndFilter)
            {
                _tokenType = JsonPathTokenType.EndFilter;
                return true;
            }

            return false;
        }

        private bool ReadSelectCurrentNext()
        {
            return TryReadChildOrDeepScan();
        }

        private bool NextCharIs(byte ch)
        {
            return NextCharIs(_currentPosition, ch);
        }

        private bool NextCharIs(int start, byte ch)
        {
            var nextIndex = start + 1;
            if (nextIndex >= _buffer.Length)
            {
                return false;
            }
            return _buffer[nextIndex] == ch;
        }

        private bool NextCharIsIn(ReadOnlySpan<byte> chs)
        {
            return NextCharIsIn(_currentPosition, chs);
        }

        private bool NextCharIsIn(int start, ReadOnlySpan<byte> chs)
        {
            var nextIndex = start + 1;
            if (nextIndex >= _buffer.Length)
            {
                return false;
            }

            return chs.IndexOf(_buffer[nextIndex]) > -1;
        }

        private bool ReadPropertyNameEnd()
        {
            _segmentStart = _segmentEnd = _currentPosition;
            _currentPosition++;
            while (!IsEnd() && IsPropertyNameChar(_buffer[_currentPosition]))
            {
                _currentPosition++;
            }
            _segmentEnd = --_currentPosition;
            return _segmentEnd >= _segmentStart;
        }

        private bool ReadStringEnd()
        {
            _currentPosition++;
            _segmentStart = _segmentEnd = _currentPosition;
            while (!IsEnd() && IsPropertyNameChar(_buffer[_currentPosition]))
            {
                _currentPosition++;
            }
            _segmentEnd = _currentPosition - 1;

            if (_buffer[_currentPosition] == JsonPathConstants.SingleQuote && _segmentStart <= _segmentEnd)
            {
                return true;
            }

            return false;
        }

        private bool ReadIntegerEnd()
        {
            _segmentStart = _segmentEnd = _currentPosition;
            _currentPosition++;
            while (!IsEnd() && IsIntegerChar(_buffer[_currentPosition]))
            {
                _currentPosition++;
            }
            _segmentEnd = --_currentPosition;
            if (_segmentStart == _segmentEnd && _buffer[_segmentStart] != JsonPathConstants.Minus
                || _segmentEnd > _segmentStart)
            {
                return true;
            }

            return false;
        }

        private bool ReadNumberEnd()
        {
            _segmentStart = _segmentEnd = _currentPosition;
            _currentPosition++;
            bool hasdot = false;
            while (!IsEnd() && IsNumberChar(_buffer[_currentPosition]))
            {
                if (_buffer[_currentPosition] == JsonPathConstants.Dot)
                {
                    if (!hasdot)
                    {
                        hasdot = true;
                    }
                    else
                    {
                        return false;
                    }
                }
                _currentPosition++;
            }
            _segmentEnd = --_currentPosition;

            if (_segmentStart == _segmentEnd
                && _buffer[_segmentStart] != JsonPathConstants.Minus
                || _segmentEnd > _segmentStart && _buffer[_segmentEnd] != JsonPathConstants.Dot)
            {
                return true;
            }

            return false;
        }

        private bool ReadRegexEnd()
        {
            _segmentStart = _segmentEnd = _currentPosition;
            _currentPosition++;
            while (!IsEnd())
            {
                if (_buffer[_currentPosition] == JsonPathConstants.RegexStart && !NextCharIs(JsonPathConstants.RegexStart))
                {
                    if (NextCharIsIn(JsonPathConstants.RegexModifyChars))
                    {
                        _currentPosition++;
                    }

                    _segmentEnd = _currentPosition;
                    return true;
                }
                _currentPosition++;
            }

            return false;
        }

        private bool TryReadStartExpression()
        {
            var ch = _buffer[_currentPosition];
            if (ch == JsonPathConstants.ExpressionStart && NextCharIs(JsonPathConstants.LeftBracket))
            {
                _currentPosition++;
                _tokenType = JsonPathTokenType.StartExpression;
                _inExpression = true;
                return true;
            }

            return false;
        }

        private bool ReadEndExpression()
        {
            _tokenType = JsonPathTokenType.EndExpression;
            _inExpression = false;
            return true;
        }

        private bool TryReadChildOrDeepScan()
        {
            var ch = _buffer[_currentPosition];
            if (ch == JsonPathConstants.Dot && NextCharIs(JsonPathConstants.Dot))
            {
                _tokenType = JsonPathTokenType.DeepScan;
                _currentPosition++;
                return true;
            }
            else if (ch == JsonPathConstants.Dot)
            {
                _tokenType = JsonPathTokenType.Child;
                return true;
            }
            return false;
        }

        private bool IsPropertyNameCharStart(byte ch)
        {
            return ch >= JsonPathConstants.A && ch <= JsonPathConstants.Z
                || ch >= JsonPathConstants.a && ch <= JsonPathConstants.z
                || ch == JsonPathConstants.UnderLine;
        }

        private bool IsPropertyNameChar(byte ch)
        {
            return ch >= JsonPathConstants.A && ch <= JsonPathConstants.Z
                || ch >= JsonPathConstants.a && ch <= JsonPathConstants.z
                || ch == JsonPathConstants.UnderLine
                || ch >= JsonPathConstants.Num0 && ch <= JsonPathConstants.Num9;
        }

        private bool IsNumberCharStart(byte ch)
        {
            return ch == JsonPathConstants.Minus
                || ch >= JsonPathConstants.Num0 && ch <= JsonPathConstants.Num9;
        }

        private bool IsIntegerChar(byte ch)
        {
            return ch >= JsonPathConstants.Num0 && ch <= JsonPathConstants.Num9;
        }

        private bool IsNumberChar(byte ch)
        {
            return IsIntegerChar(ch) || ch == JsonPathConstants.Dot;
        }

        private bool StartsWith(ReadOnlySpan<byte> buffer)
        {
            return _buffer.Slice(_currentPosition).StartsWith(buffer);
        }

        private bool StartsWithAndEndWithSpace(ReadOnlySpan<byte> buffer)
        {
            return _buffer.Slice(_currentPosition).StartsWith(buffer) && NextCharIs(_currentPosition + buffer.Length, JsonPathConstants.Space);
        }

        private void SkipInvaild()
        {
            while (!IsEnd()
                && _buffer[_currentPosition] == JsonPathConstants.Space)
            {
                _currentPosition++;
            }
        }

        private bool ShouldSkipInvaild()
        {
            return _tokenType == JsonPathTokenType.Equal
                || _tokenType == JsonPathTokenType.GreaterThan
                || _tokenType == JsonPathTokenType.LessThan
                || _tokenType == JsonPathTokenType.GreaterThanOrEqual
                || _tokenType == JsonPathTokenType.LessThanOrEqual
                || _tokenType == JsonPathTokenType.In
                || _tokenType == JsonPathTokenType.NIn
                || _tokenType == JsonPathTokenType.Noneof
                || _tokenType == JsonPathTokenType.Anyof
                || _tokenType == JsonPathTokenType.Empty
                || _tokenType == JsonPathTokenType.EndExpression
                || _tokenType == JsonPathTokenType.Matches
                || _tokenType == JsonPathTokenType.NotEqual
                || _tokenType == JsonPathTokenType.Number
                || _tokenType == JsonPathTokenType.Size
                || _tokenType == JsonPathTokenType.Slice
                || _tokenType == JsonPathTokenType.StartExpression
                || _tokenType == JsonPathTokenType.Subsetof
                || _tokenType == JsonPathTokenType.StartFilter;
        }

        private bool IsEndToken(JsonPathTokenType token)
        {
            return token == JsonPathTokenType.EndFilter
                || token == JsonPathTokenType.PropertyName
                || token == JsonPathTokenType.Wildcard;
        }

        private bool ReadRoot()
        {
            if (_buffer[_currentPosition] == JsonPathConstants.Root)
            {
                _tokenType = JsonPathTokenType.Root;
                return true;
            }
            return false;
        }

        private bool IsEnd()
        {
            return _currentPosition >= _buffer.Length;
        }

        private void ThrowInvalidDataException()
        {
            _readFinal = true;
            throw new InvalidDataException($"Invalid data at {GetString()}");
        }
    }
}
