using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using N2.Core.Exceptions;

namespace N2.Core
{
    /// <summary>
    /// Conditional contract validators.
    /// </summary>
    public static class Contract
    {
        /// <summary>
        /// Validates the item to contain a valid name, starting with a character, followed by only
        /// characters and digits. An underscore is acceptable anywhere.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        [Conditional("DEBUG")]
        [Conditional("CODECONTRACTS")]
        public static void ValidName([NotNull] string? item)
        {
            if (string.IsNullOrWhiteSpace(item))
            {
                throw new ContractException("Value cannot be empty or only whitespace.");
            }

            if (!(IsChar(item[0]) || (item[0] == '_')))
            {
                throw new ContractException("Valid name should start with a character");
            }

            for (int i = 0; i < item.Length; i++)
            {
                char c = item[i];
                if (!(IsChar(c) || IsDigit(c) || (c == '_')))
                {
                    throw new ContractException($"Character at position {i} is not allowed ({c}).");
                }
            }
        }

        /// <summary>
        /// Check if the item is null and throw an ArgumentNullException if the item in not
        /// initialized. The validation is conditional for compilation when 'CODECONTRACTS' is defined.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <param name="nameOfItem">
        /// The name of item.
        /// </param>
        [Conditional("DEBUG")]
        [Conditional("CODECONTRACTS")]
        public static void NotNull([NotNull] object? item, string nameOfItem)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameOfItem);
            }
        }

        /// <summary>
        /// Return true if the value is a character.
        /// </summary>
        /// <param name="c">
        /// The value to evaluate.
        /// </param>
        /// <returns>
        /// A bool.
        /// </returns>
        public static bool IsChar(char c)
        {
            int check = (int)c;
            if (check > 256)
            {
                return false;
            }

            check = check & 0b00100000;
            return check >= 65 && check <= 90;
        }

        /// <summary>
        /// Return true if the value is a digit.
        /// </summary>
        /// <param name="c">
        /// The value to evaluate.
        /// </param>
        /// <returns>
        /// A bool.
        /// </returns>
        public static bool IsDigit(char c)
        {
            int check = (int)c;
            if (check > 256)
            {
                return false;
            }

            return check >= 48 && check <= 57;
        }
    }
}