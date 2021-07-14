using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Submodules.TextMeshPro_Extension.Tags {
    public class TextMeshProTeletypeTag : TextMeshProTag {

        #region parameters

        // Number of characters displayed by second
        private float speed;
        private int spacing;
        private bool isFading;
        // Pause before starting the animation
        private float delay;

        #endregion

        public event Action OnEndTeletype = delegate {  };

        private int currentIndex;
        private int currentEndIndex;
        private bool isDone;
        private float period;
        private Color32[] newVertexColors;

        private TextMeshProTeletypeTag(TMP_Text rawTextRef, int startIndex)
            : base(rawTextRef, "teletype", startIndex) {
            isDone = false;
        }

        public TextMeshProTeletypeTag(TMP_Text rawTextRef, float speed, float delay, int spacing, bool isFading)
            : this(rawTextRef, 0) {
            this.speed = speed;
            this.delay = delay;
            this.spacing = spacing;
            this.isFading = isFading;
        }

        public void EndTeletype() {
            isDone = true;
            OnEndTeletype();
        }

        public static bool TryGenerate(out TextMeshProTeletypeTag tag, TMP_Text rawTextRef, Dictionary<string, string> parameters, int startIndex) {
            tag = new TextMeshProTeletypeTag(rawTextRef, startIndex) {
                speed = 10f,
                delay = 0f,
                spacing = 5,
                isFading = true
            };

            foreach (KeyValuePair<string, string> parameter in parameters) {
                switch (parameter.Key) {
                    case "speed" when TextParser.IsFloatRegex(out float speedValue, parameter.Value):
                        tag.speed = speedValue;
                        break;
                    case "spacing" when TextParser.IsIntRegex(out int spacingValue, parameter.Value):
                        tag.spacing = spacingValue;
                        break;
                    case "delay" when TextParser.IsFloatRegex(out float delayValue, parameter.Value):
                        tag.delay = delayValue;
                        break;
                    case "isFading" when TextParser.IsBoolRegex(out bool isFadingValue, parameter.Value):
                        tag.isFading = isFadingValue;
                        break;
                    default:
                        return false;
                }
            }
            return true;
        }

        public override void UpdateText(float time) {
            if (isDone)
                return;

            float timeSinceBeginning = time - delay;
            
            int j = currentIndex - startIndex;
            for (int i = currentIndex * 4; i < currentEndIndex * 4; i += 4, j++) {

                if (newVertexColors[i + 3].a == 255) {
                    TryIncrementCurrentIndexes();
                    continue;
                }

                byte alpha = (byte) (isFading
                    ? 255 * Mathf.Clamp01((timeSinceBeginning - j * period / spacing) / period)
                    : timeSinceBeginning > (j + 1) * period ? 255 : 0);
                
                SetVertexAlpha(i, alpha);
            }

            if (timeSinceBeginning - (endIndex - startIndex) * period > 0)
                EndTeletype();
        }

        public override void Initialize() {
            currentIndex = startIndex;
            currentEndIndex = Mathf.Min(startIndex + spacing, endIndex);
            period = isFading ? spacing / speed : 1 / speed;
            // Get the vertex colors of the mesh used by this text element (character or sprite).
            newVertexColors = rawText.textInfo.meshInfo[0].colors32;

            for (int i = startIndex * 4; i < endIndex * 4; i += 4) {
                SetVertexAlpha(i, 0);
            }
        }

        private void TryIncrementCurrentIndexes() {
            currentIndex = Mathf.Min(endIndex, currentIndex + 1);
            currentEndIndex = Mathf.Min(endIndex, currentEndIndex + 1);
        }

        private void SetVertexAlpha(int i, byte alpha) {
            newVertexColors[i + 0].a = alpha;
            newVertexColors[i + 1].a = alpha;
            newVertexColors[i + 2].a = alpha;
            newVertexColors[i + 3].a = alpha;
        }
    }
}