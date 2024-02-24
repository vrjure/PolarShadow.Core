using System;
using System.Collections.Generic;
using System.Text;

namespace PolarShadow.Core
{
    public enum JsonPathTokenType
    {
        None = 0,
        /// <summary>
        /// $
        /// </summary>
        Root,
        /// <summary>
        /// @
        /// </summary>
        Current,
        /// <summary>
        /// *
        /// </summary>
        Wildcard,
        /// <summary>
        /// ..
        /// </summary>
        DeepScan,
        /// <summary>
        /// .
        /// </summary>
        Child,
        PropertyName,
        String,
        StartFilter,
        EndFilter,
        Number,
        Regex,
        /// <summary>
        /// [start:end]
        /// </summary>
        Slice,
        /// <summary>
        /// [?(<expression>)]
        /// </summary>
        StartExpression,
        /// <summary>
        /// [?(<expression>)]
        /// </summary>
        EndExpression,
        /// <summary>
        /// ==
        /// </summary>
        Equal,
        /// <summary>
        /// <
        /// </summary>
        LessThan,
        /// <summary>
        /// >
        /// </summary>
        GreaterThan,
        /// <summary>
        /// <=
        /// </summary>
        LessThanOrEqual,
        /// <summary>
        /// >=
        /// </summary>
        GreaterThanOrEqual,
        /// <summary>
        /// !=
        /// </summary>
        NotEqual,
        /// <summary>
        /// =~
        /// </summary>
        Matches,
        In,
        NIn,
        Subsetof,
        Anyof,
        Noneof,
        Size,
        Empty
    }
}
