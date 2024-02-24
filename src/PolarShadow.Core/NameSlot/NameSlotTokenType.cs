using System;
using System.Collections.Generic;
using System.Text;

namespace PolarShadow.Core
{
    public enum NameSlotTokenType
    {
        None = 0,
        /// <summary>
        /// {
        /// </summary>
        Start,
        /// <summary>
        /// }
        /// </summary>
        End,
        /// <summary>
        /// :
        /// </summary>
        Format,
        /// <summary>
        /// :/ ... /
        /// </summary>
        Match,
        /// <summary>
        /// 参数 [_][A-Z a-z]
        /// </summary>
        Parameter,
        /// <summary>
        /// 文本
        /// </summary>
        Text,
        /// <summary>
        /// 条件 ?
        /// </summary>
        Condition,
        /// <summary>
        /// ?: 中的:
        /// </summary>
        ConditionElse,
        /// <summary>
        /// 条件操作符 > < = >= <= == !=
        /// </summary>
        ConditionOperator,
        /// <summary>
        /// ''字符串
        /// </summary>
        String,
        /// <summary>
        /// :[,]
        /// </summary>
        SubString

    }
}
