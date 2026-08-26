using System.ComponentModel;
using System.Globalization;

namespace KHMultiBrowser
{
    /// <summary>
    /// Application settings for grid layout and behavior.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Number of rows in the browser grid (default 3).
        /// </summary>
        [DefaultValue(3)]
        public int Rows { get; set; } = 3;

        /// <summary>
        /// Number of columns in the browser grid (default 3).
        /// </summary>
        [DefaultValue(3)]
        public int Columns { get; set; } = 3;

        /// <summary>
        /// Language code (default: system language or "en").
        /// </summary>
        [DefaultValue("")]
        public string Language { get; set; } = GetDefaultLanguage();

        /// <summary>
        /// Minimum allowed rows.
        /// </summary>
        public static readonly int MinRows = 1;

        /// <summary>
        /// Maximum allowed rows.
        /// </summary>
        public static readonly int MaxRows = 9;

        /// <summary>
        /// Minimum allowed columns.
        /// </summary>
        public static readonly int MinColumns = 1;

        /// <summary>
        /// Maximum allowed columns.
        /// </summary>
        public static readonly int MaxColumns = 9;

        /// <summary>
        /// Validates and constrains the settings within min/max bounds.
        /// </summary>
        public void Validate()
        {
            Rows = System.Math.Clamp(Rows, MinRows, MaxRows);
            Columns = System.Math.Clamp(Columns, MinColumns, MaxColumns);

            if (string.IsNullOrWhiteSpace(Language))
                Language = GetDefaultLanguage();
        }

        private static string GetDefaultLanguage()
        {
            var culture = CultureInfo.CurrentCulture;
            var langCode = culture.TwoLetterISOLanguageName.ToLower();
            return (langCode == "de" || langCode == "en") ? langCode : "en";
        }
    }
}
