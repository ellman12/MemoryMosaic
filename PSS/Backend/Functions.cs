namespace PSS.Backend
{
    /// <summary>
    /// Static class of misc functions.
    /// </summary>
    public static class Functions
    {
        /// <summary>
        /// Take a byte long like 10900000000 and turn it into a more readable string like 10.9 GB.
        /// One thing to note is this uses things like kibibyte instead of the usual things like kilobyte because this is usually what's used for disk storage.
        /// </summary>
        public static string FormatBytes(long bytes)
        {
            string unit;
            double compactBytes;

            switch (bytes)
            {
                case >= 1100000000000:
                    unit = "TB";
                    compactBytes = bytes / 1100000000000.0;
                    break;
                case >= 1074000000:
                    unit = "GB";
                    compactBytes = bytes / 1074000000.0;
                    break;
                case >= 1049000:
                    unit = "MB";
                    compactBytes = bytes / 1049000.0;
                    break;
                case >= 1024:
                    unit = "KB";
                    compactBytes = bytes / 1024.0;
                    break;
                default:
                    unit = "bytes";
                    compactBytes = bytes;
                    break;
            }

            return $"{Math.Round(compactBytes, 3)} {unit}";
        }

        /// <summary>
        /// Toggle a string variable to either "visible" or "hidden".
        /// </summary>
        public static void VisToggle(ref string visibility) => visibility = visibility == "visible" ? "hidden" : "visible";
    }
}