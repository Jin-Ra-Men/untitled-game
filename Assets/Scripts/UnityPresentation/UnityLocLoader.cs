#if UNITY_5_3_OR_NEWER
using UntitledGame.Negotiation.Localization;
using UnityEngine;

namespace UntitledGame.UnityPresentation
{
    public static class UnityLocLoader
    {
        public static string Language = "ko";

        public static void EnsureLoaded()
        {
            Localization.Current.Clear();
            Localization.Current.SetLanguage(Language);
            var assets = Resources.LoadAll<TextAsset>("Loc/" + Language);
            if (assets == null || assets.Length == 0)
            {
                Debug.LogError("No loc TextAssets under Resources/Loc/" + Language);
                return;
            }

            foreach (var asset in assets)
                Localization.Current.MergeJson(asset.text);
        }
    }
}
#endif
