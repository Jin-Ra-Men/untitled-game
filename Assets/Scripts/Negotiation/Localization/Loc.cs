namespace UntitledGame.Negotiation.Localization
{
    /// <summary>Short accessor for Localization.Current.</summary>
    public static class Loc
    {
        public static string T(string key) => Localization.Current.Get(key);

        public static string F(string key, params object[] args) => Localization.Current.Format(key, args);

        public static ChoiceOption Choice(string id, string labelKey, string? hintKey = null) =>
            Localization.Current.Choice(id, labelKey, hintKey);
    }
}
