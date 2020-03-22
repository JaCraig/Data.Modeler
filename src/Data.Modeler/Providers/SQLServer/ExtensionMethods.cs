using System.Text.RegularExpressions;

namespace Data.Modeler.Providers.SQLServer
{
    internal static class ExtensionMethods
    {
        /// <summary>
        /// The comment regex
        /// </summary>
        private static readonly Regex CommentRegex = new Regex("-- (.*)", RegexOptions.Compiled);

        /// <summary>
        /// The connection regex
        /// </summary>
        private static readonly Regex ConnectionRegex = new Regex("Initial Catalog=(.*?;)", RegexOptions.Compiled);

        /// <summary>
        /// Removes the comments.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The text with comments removed.</returns>
        public static string RemoveComments(this string text) => CommentRegex.Replace(text, string.Empty);

        /// <summary>
        /// Removes the initial catalog.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The text without the initial catalog.</returns>
        public static string RemoveInitialCatalog(this string text) => ConnectionRegex.Replace(text, string.Empty);
    }
}