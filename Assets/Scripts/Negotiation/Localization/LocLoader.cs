using System;
using System.IO;

namespace UntitledGame.Negotiation.Localization
{
    public static class LocLoader
    {
        /// <summary>
        /// Loads all *.json under {locRoot}/{language}/ into Localization.Current.
        /// locRoot example: .../Assets/Resources/Loc
        /// </summary>
        public static void LoadLanguage(string locRoot, string languageCode)
        {
            Localization.Current.LoadLanguageFolder(locRoot, languageCode);
        }

        public static string FindDefaultLocRoot()
        {
            // Playtest: bin/.../net8.0 → repo Assets/Resources/Loc
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            for (var i = 0; i < 8 && dir != null; i++, dir = dir.Parent)
            {
                var candidate = Path.Combine(dir.FullName, "Assets", "Resources", "Loc");
                if (Directory.Exists(candidate))
                    return candidate;
            }

            throw new DirectoryNotFoundException(
                "Could not find Assets/Resources/Loc. Set LocLoader path explicitly.");
        }
    }
}
