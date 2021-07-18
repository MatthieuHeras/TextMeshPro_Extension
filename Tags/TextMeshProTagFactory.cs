using System.Collections.Generic;
using TMPro;

namespace Submodules.TextMeshPro_Extension.Tags {
    /// <summary> Factory that returns a TextMeshProTag depending on the name. </summary>
    public static class TextMeshProTagFactory {


        public static TextMeshProTag CreateTag(string tagName, TMP_Text tmpText, Dictionary<string, string> parameters, int startIndex) {
            switch (tagName) {
                case Constants.WaveTagName:
                    if (TextMeshProWaveTag.TryGenerate(out TextMeshProWaveTag waveTag, tmpText, parameters, startIndex))
                        return waveTag;
                    break;

                case Constants.TeletypeTagName:
                    if (TextMeshProTeletypeTag.TryGenerate(out TextMeshProTeletypeTag teletypeTag, tmpText, parameters, startIndex))
                        return teletypeTag;
                    break;

                case Constants.JitterTagName:
                    if (TextMeshProJitterTag.TryGenerate(out TextMeshProJitterTag jitterTag, tmpText, parameters, startIndex))
                        return jitterTag;
                    break;

                default:
                    return null;
            }

            return null;
        }
        
        // We store this here to make sure there are no doubles.
        public static class Constants {
            // /!\ Keep alphabetical order ! /!\
            public const string JitterTagName = "jitter";
            public const string TeletypeTagName = "teletype";
            public const string WaveTagName = "wave";
        }
    }
}