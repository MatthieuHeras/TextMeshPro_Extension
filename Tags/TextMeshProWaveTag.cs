﻿using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProExtension.Tags {
    public class TextMeshProWaveTag :
     TextMeshProTag {
        // The necessary time for a full cycle. The higher the longer the animation will be
        private float period;
        
        // The number of characters to have a full period. The higher the smoother the curve will be.
        private int spacing;
        
        // The height of the wave, set it to 1 to have a one character height difference from top to bottom.
        private float amplitude;
        
        // The offset with which the curve starts. Set it to ...
        private float offset;


        // internal variables
        private float frequency;
        private float actualAmplitude;
        private float actualSpacing;
        
        private TextMeshProWaveTag(TMP_Text rawTextRef, int startIndex)
            : base(rawTextRef, "wave", startIndex) {
            actualAmplitude = 1f;
            frequency = 1f;
            actualSpacing = 2f;
        }

        public static bool TryGenerate(out TextMeshProWaveTag tag, TMP_Text rawTextRef, Dictionary<string, string> parameters, int startIndex) {
            tag = new TextMeshProWaveTag(rawTextRef, startIndex) {
                period = 2f,
                amplitude = 1f,
                spacing = 5,
                offset = 0f
            };

            foreach (KeyValuePair<string, string> parameter in parameters) {
                switch (parameter.Key) {
                    case "period" when TextParser.IsFloatRegex(out float periodValue, parameter.Value):
                        tag.period = periodValue;
                        break;
                    case "spacing" when TextParser.IsIntRegex(out int spacingValue, parameter.Value):
                        tag.spacing = spacingValue;
                        break;
                    case "amplitude" when TextParser.IsFloatRegex(out float amplitudeValue, parameter.Value):
                        tag.amplitude = amplitudeValue;
                        break;
                    case "offset" when TextParser.IsFloatRegex(out float offsetValue, parameter.Value):
                        tag.offset = offsetValue;
                        break;
                    default:
                        return false;
                }
            }
            return true;
        }

        public override void UpdateText(float time) {
            TMP_TextInfo textInfo = rawText.textInfo;
            for (int i = 0; i < textInfo.meshInfo.Length; i++) {

                for (int j = startIndex * 4; j < endIndex * 4; j += 4) {
                    Vector3 waveOffset = Vector3.up * (Mathf.Sin((offset + time) * frequency + j * 0.25f * actualSpacing) * actualAmplitude);

                    textInfo.meshInfo[i].vertices[j + 0] += waveOffset;
                    textInfo.meshInfo[i].vertices[j + 1] += waveOffset;
                    textInfo.meshInfo[i].vertices[j + 2] += waveOffset;
                    textInfo.meshInfo[i].vertices[j + 3] += waveOffset;
                }
            }
        }

        public override void Initialize() {
            // To have a one character height difference, the amplitude must be ~0.3 for a font size of 1
            actualAmplitude = rawText.fontSize * amplitude * 0.3f;
            frequency = 2f * Mathf.PI / period;
            actualSpacing = 2f * Mathf.PI / spacing;
        }
    }
}