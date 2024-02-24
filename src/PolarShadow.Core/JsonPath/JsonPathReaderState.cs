using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PolarShadow.Core
{
    public struct JsonPathReaderState
    {
        public JsonPathReaderState(int position, JsonPathTokenType tokenType, bool inExpression, bool inExpressionFilter)
        {
            this.Position = position;
            this.TokenType = tokenType;
            this.InExpression = inExpression;
            this.InExpressionFilter = inExpressionFilter;
        }
        internal int Position;
        internal JsonPathTokenType TokenType;
        internal bool InExpression;
        internal bool InExpressionFilter;
    }
}
