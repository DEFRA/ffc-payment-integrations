namespace FFC.Payment.Integrations.Function.Helpers
{
    /// <summary>
    /// Helper methods
    /// </summary>
	public static class FfcHelper
	{
        /// <summary>
        /// Extract the 4-digit year from the filename in the url provided
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
		public static string ExtractYear(string url)
        {
            var filename = ExtractFilename(url);
            var filenameElements = filename.Split("_");
            return filenameElements[3];
        }

        /// <summary>
        /// Extract the filename from the url provided
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string ExtractFilename(string url)
        {
            return url.Substring(url.LastIndexOf("/")+1);
        }

        /// <summary>
        /// Escape double quotes to prevent errors when posting strings
        /// </summary>
        /// <param name="inStr"></param>
        /// <returns></returns>
        public static string EscapeDoubleQuotes(string inStr)
        {
            return inStr == null ? inStr : inStr.Replace("\"", "'");
        }
    }
}

