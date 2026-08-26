using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace KHMultiBrowser
{
    /// <summary>
    /// Manages localization strings from JSON resource files.
    /// Singleton pattern for easy access throughout the application.
    /// </summary>
    public class StringResources
    {
        private static StringResources _instance;
        private Dictionary<string, string> _currentLanguage = new();
        private string _currentLanguageCode = "en";

        private StringResources() { }

        public static StringResources Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new StringResources();
                return _instance;
            }
        }

        /// <summary>
        /// Loads a language resource file and sets it as current.
        /// </summary>
        public void LoadLanguage(string languageCode)
        {
            try
            {
                var resourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", $"{languageCode}.json");

                if (!File.Exists(resourcePath))
                {
                    // Fallback to English
                    resourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "en.json");
                }

                if (!File.Exists(resourcePath))
                    return;

                var json = File.ReadAllText(resourcePath);
                _currentLanguage = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
                _currentLanguageCode = languageCode;
            }
            catch
            {
                // Fehler beim Laden ignorieren
                if (_currentLanguage.Count == 0)
                    _currentLanguage = new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Loads the default language based on Windows culture.
        /// </summary>
        public void LoadDefaultLanguage()
        {
            var culture = CultureInfo.CurrentCulture;
            var languageCode = culture.TwoLetterISOLanguageName.ToLower();

            if (languageCode != "de" && languageCode != "en")
                languageCode = "en"; // Default to English

            LoadLanguage(languageCode);
        }

        /// <summary>
        /// Gets a localized string by key.
        /// </summary>
        public string Get(string key)
        {
            if (_currentLanguage.TryGetValue(key, out var value))
                return value;

            return key; // Return key if not found
        }

        /// <summary>
        /// Gets the current language code.
        /// </summary>
        public string CurrentLanguage => _currentLanguageCode;

        /// <summary>
        /// Gets available languages.
        /// </summary>
        public static readonly string[] AvailableLanguages = { "de", "en" };
    }
}
