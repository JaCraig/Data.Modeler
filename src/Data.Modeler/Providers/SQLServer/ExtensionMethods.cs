using System.Text.RegularExpressions;

namespace Data.Modeler.Providers.SQLServer
{
    internal static partial class ExtensionMethods
    {
        /// <summary>
        /// The comment regex
        /// </summary>
        private static readonly Regex _CommentRegex = CreateCommentRegex();

        /// <summary>
        /// The connection regex
        /// </summary>
        private static readonly Regex _ConnectionRegex = CreateConnectionRegex();

        /// <summary>
        /// Removes the comments.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The text with comments removed.</returns>
        public static string RemoveComments(this string text) => _CommentRegex.Replace(text, string.Empty);

        /// <summary>
        /// Removes the initial catalog.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The text without the initial catalog.</returns>
        public static string RemoveInitialCatalog(this string text) => _ConnectionRegex.Replace(text, string.Empty);

        /// <summary>
        /// Creates a compiled regular expression for parsing comments that start with "-- ".
        /// </summary>
        /// <remarks>
        /// Generated at compile time by the <see cref="GeneratedRegexAttribute"/> for improved
        /// startup performance and reduced runtime overhead.
        /// </remarks>
        /// <returns>
        /// A regular expression that matches "-- " followed by any text and captures the text after
        /// the prefix.
        /// </returns>
        [GeneratedRegex("-- (.*)", RegexOptions.Compiled)]
        private static partial Regex CreateCommentRegex();

        /// <summary>
        /// Creates a source-generated regular expression that matches the `Initial Catalog` segment
        /// in a connection string.
        /// </summary>
        /// <remarks>
        /// Uses `[GeneratedRegex]` to generate the expression at compile time for improved
        /// performance and reduced runtime overhead.
        /// </remarks>
        /// <returns>
        /// A compiled `Regex` that matches `Initial Catalog=` followed by the shortest sequence
        /// ending with `;`, captured as a group.
        /// </returns>
        [GeneratedRegex("Initial Catalog=(.*?;)", RegexOptions.Compiled)]
        private static partial Regex CreateConnectionRegex();
    }
}