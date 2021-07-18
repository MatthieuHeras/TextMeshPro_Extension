using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Submodules.TextMeshPro_Extension.Tags {
    public class TextMeshProJitterTag : TextMeshProTag {
        public override string TagName => TextMeshProTagFactory.Constants.JitterTagName;
        
        private float intensity;

        private float speed;

        private float threshold;
        private float period;
        private List<Vector3> jitterVectors;

        private TextMeshProJitterTag(TMP_Text rawTextRef, int startIndex)
            : base(rawTextRef, startIndex) {
        }

        public static bool TryGenerate(out TextMeshProJitterTag tag, TMP_Text rawTextRef, Dictionary<string, string> parameters, int startIndex) {
            tag = new TextMeshProJitterTag(rawTextRef, startIndex) {
                intensity = 1f,
                speed = 1f
            };

            foreach (KeyValuePair<string, string> parameter in parameters) {
                switch (parameter.Key) {
                    case "intensity" when TextParser.IsFloatRegex(out float intensityValue, parameter.Value):
                        tag.intensity = intensityValue;
                        break;
                    case "speed" when TextParser.IsFloatRegex(out float speedValue, parameter.Value):
                        tag.speed = speedValue;
                        break;
                    default:
                        return false;
                }
            }
            return true;
        }

        public override void UpdateText(float time) {
            TMP_TextInfo textInfo = rawText.textInfo;
            if (time >= threshold) {
                threshold += period;

                for (int i = 0; i < endIndex - startIndex; i++) {
                    Vector3 jitterVector = new Vector3(Random.Range(-intensity, intensity), Random.Range(-intensity, intensity));
                    jitterVectors[i] = jitterVector;
                }
            }

            for (int i = 0; i < textInfo.meshInfo.Length; i++) {
                int k = 0;
                for (int j = startIndex * 4; j < endIndex * 4; j += 4, k++) {
                    textInfo.meshInfo[i].vertices[j + 0] += jitterVectors[k];
                    textInfo.meshInfo[i].vertices[j + 1] += jitterVectors[k];
                    textInfo.meshInfo[i].vertices[j + 2] += jitterVectors[k];
                    textInfo.meshInfo[i].vertices[j + 3] += jitterVectors[k];
                }
            }
        }

        public override void Initialize() {
            period = 1 / speed;
            threshold = 0;
            jitterVectors = new List<Vector3>();
            for (int i = 0; i < endIndex - startIndex; i++)
                jitterVectors.Add(Vector3.zero);
        }
    }
}