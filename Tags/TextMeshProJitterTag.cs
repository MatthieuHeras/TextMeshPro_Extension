using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Submodules.TextMeshPro_Extension.Tags {
    public class TextMeshProJitterTag : TextMeshProTag {
        public override string TagName => TextMeshProTagFactory.Constants.JitterTagName;
        
        // The intensity of the jittering. Above 1, characters will start to mix each others' places.
        private float intensity;
        // The number of times the direction is going to change by second.
        private float speed;

        // Internal variables.
        private float actualIntensity;
        private float threshold;
        private float period;
        private List<Vector3> targetOffsets;
        private List<Vector3> actualTargetOffsets;
        private List<Vector3> currentOffsets;

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
                    // Reset the current offset.
                    currentOffsets[i] -= actualTargetOffsets[i];
                    
                    // change this to a circular random ?
                    // Pick a random offset around the char.
                    Vector3 targetOffset = new Vector3(Random.Range(-actualIntensity, actualIntensity), Random.Range(-actualIntensity, actualIntensity));
                    // Since the char is already offset, we need to remove the current offset. 
                    actualTargetOffsets[i] = targetOffset - targetOffsets[i];
                    targetOffsets[i] = targetOffset;
                }
            }

            for (int j = startIndex * 4, k = 0; j < endIndex * 4; j += 4, k++) {
                // Store the previous offset.
                Vector3 appliedOffset = currentOffsets[k];
                // Compute the wanted offset.
                currentOffsets[k] = Vector3.Lerp(Vector3.zero, actualTargetOffsets[k], (time + period - threshold) / period);
                // Subtract the previous offset to the wanted offset (as we use += and not =).
                appliedOffset = currentOffsets[k] - appliedOffset;

                // Apply the offset to the characters.
                textInfo.meshInfo[0].vertices[j + 0] += appliedOffset;
                textInfo.meshInfo[0].vertices[j + 1] += appliedOffset;
                textInfo.meshInfo[0].vertices[j + 2] += appliedOffset;
                textInfo.meshInfo[0].vertices[j + 3] += appliedOffset;
            }
        }

        public override void Initialize() {
            period = 1 / speed;
            threshold = 0;
            // Arbitrary constant : multiplied by 0.1, an intensity of 1 makes the characters touch each other.
            actualIntensity = intensity * rawText.fontSize * 0.1f;

            int numberOfCharacters = endIndex - startIndex;
            
            targetOffsets = new List<Vector3>(numberOfCharacters);
            actualTargetOffsets = new List<Vector3>(numberOfCharacters);
            currentOffsets = new List<Vector3>(numberOfCharacters);
            
            for (int i = 0; i < numberOfCharacters; i++) {
                targetOffsets.Add(Vector3.zero);
                actualTargetOffsets.Add(Vector3.zero);
                currentOffsets.Add(Vector3.zero);
            }
        }
    }
}