using System.Collections.Generic;
using TMPro;

namespace Submodules.TextMeshPro_Extension.Tags {
    /// <summary>
    /// Factory that returns a TextMeshProTag depending on the name
    /// </summary>
    public static class TextMeshProTagFactory {


        public static TextMeshProTag CreateTag(string tagName, TMP_Text tmpText, Dictionary<string, string> parameters, int startIndex) {
            switch (tagName) {
                case "wave":
                    if (TextMeshProWaveTag.TryGenerate(out TextMeshProWaveTag waveTag, tmpText, parameters, startIndex))
                        return waveTag;
                    break;

                case "teletype":
                    if (TextMeshProTeletypeTag.TryGenerate(out TextMeshProTeletypeTag teletypeTag, tmpText, parameters, startIndex))
                        return teletypeTag;
                    break;

                case "jitter":
                    if (TextMeshProJitterTag.TryGenerate(out TextMeshProJitterTag jitterTag, tmpText, parameters, startIndex))
                        return jitterTag;
                    break;

                default:
                    return null;
            }

            return null;
        }
    }
}