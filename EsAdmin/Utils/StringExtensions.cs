using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EsAdmin.Utils
{
    public static class StringExtensions
    {
        /// <summary>
        /// Represents a very short alias for string.Format method.
        /// </summary>
        public static string F(this string source, params object[] args)
        {
            return string.Format(source, args);
        }


        ///<summary>
        /// Indicates whether String object is <c>null</c> or an Empty string.
        ///</summary>
        public static bool IsNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }
    }
}
