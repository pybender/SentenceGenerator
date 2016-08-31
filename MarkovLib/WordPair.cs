using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkovLib
{
    /// <summary>
    /// A pair of words used in order-2 sentence generation.
    /// The second word in the pair may be null.
    /// </summary>
    public struct WordPair
    {
        public WordPair(string first) : this(first, null)
        {
        }

        public WordPair(string first, string second)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            First = first;
            Second = second;
        }

        /// <summary>
        /// The first word in the pair. Will not be <c>null</c>.
        /// </summary>
        public readonly string First;

        /// <summary>
        /// The second word in the pair. May be <c>null</c>.
        /// </summary>
        public readonly string Second;

        public override bool Equals(object obj)
        {
            if (!(obj is WordPair))
                return false;
            return this == (WordPair)obj;
        }

        public static bool operator ==(WordPair lhs, WordPair rhs)
        {
            var comparer = StringComparer.InvariantCultureIgnoreCase;
            return comparer.Equals(lhs.First, rhs.First) && comparer.Equals(lhs.Second, rhs.Second);
        }

        public static bool operator !=(WordPair lhs, WordPair rhs)
        {
            return !(lhs == rhs);
        }

        public override int GetHashCode()
        {
            var comparer = StringComparer.InvariantCultureIgnoreCase;
            var result = 27;
            result = result * 13 + comparer.GetHashCode(First);
            result = result * 13 + (Second != null ? comparer.GetHashCode(Second) : 0);
            return result;
        }

        public override string ToString()
        {
            return Second != null ? $"(\"{First}\", \"{Second}\")" : $"(\"{First}\")";
        }
    }
}
