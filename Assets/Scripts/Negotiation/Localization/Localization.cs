using System;
using System.Collections.Generic;
using System.IO;

namespace UntitledGame.Negotiation.Localization
{
    /// <summary>
    /// Flat JSON string tables: { "key": "value", ... }
    /// Episode/UI code uses keys only; never embed player-facing locale text in scripts.
    /// </summary>
    public sealed class Localization
    {
        public static Localization Current { get; } = new Localization();

        private readonly Dictionary<string, string> _table = new Dictionary<string, string>(StringComparer.Ordinal);
        public string Language { get; private set; } = "ko";

        public void Clear() => _table.Clear();

        public void SetLanguage(string languageCode) => Language = languageCode;

        public void LoadFile(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Loc file missing: " + path, path);
            MergeJson(File.ReadAllText(path));
        }

        public void LoadLanguageFolder(string locRoot, string languageCode)
        {
            Language = languageCode;
            _table.Clear();
            var dir = Path.Combine(locRoot, languageCode);
            if (!Directory.Exists(dir))
                throw new DirectoryNotFoundException("Loc folder missing: " + dir);

            foreach (var file in Directory.GetFiles(dir, "*.json"))
                LoadFile(file);
        }

        public void MergeJson(string json)
        {
            foreach (var pair in LocJson.ParseObject(json))
                _table[pair.Key] = pair.Value;
        }

        public string Get(string key)
        {
            if (_table.TryGetValue(key, out var value))
                return value;
            return "[" + key + "]";
        }

        public string Format(string key, params object[] args)
        {
            var format = Get(key);
            try
            {
                return string.Format(format, args);
            }
            catch (FormatException)
            {
                return format;
            }
        }

        public ChoiceOption Choice(string id, string labelKey, string? hintKey = null)
        {
            return new ChoiceOption(
                id,
                Get(labelKey),
                hintKey == null ? null : Get(hintKey));
        }
    }
}
