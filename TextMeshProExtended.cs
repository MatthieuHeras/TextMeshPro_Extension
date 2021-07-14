using System;
using System.Collections;
using System.Collections.Generic;
using Submodules.TextMeshPro_Extension.Tags;
using TMPro;
using UnityEngine;

namespace Submodules.TextMeshPro_Extension {
    /// <summary>
    /// Component that animates a TextMeshPro component with custom beacons
    /// </summary>
    public class TextMeshProExtended : MonoBehaviour {
        public event Action OnEndTyping = delegate {  };
        
        [SerializeField] private bool isDefaultTeletyped = true;
        [TextArea]
        [SerializeField] private string text = default;
        [SerializeField] private TMP_Text tmpText = default;
        
        private List<TextMeshProTag> tags;
        private List<TextMeshProTeletypeTag> teletypeTags;

        private int teletypeFinishedBuffer;
        private float startTime;

        private Coroutine currentAnimationCoroutine;

        public void SetText(string text) {
            this.text = text;
        }

        public void AnimateText() {
            InitializeAnimation();

            foreach (TextMeshProTag textMeshProTag in tags) {
                textMeshProTag.Initialize();
            }
            if (currentAnimationCoroutine != null)
                StopCoroutine(currentAnimationCoroutine);
            currentAnimationCoroutine = StartCoroutine(UpdateText());
        }

        private void InitializeAnimation() {
            teletypeFinishedBuffer = 0;

            ParsedText parsedText = TextParser.ParseText(text, tmpText, isDefaultTeletyped);
            tmpText.text = parsedText.RawText;
            tags = parsedText.Tags;

            teletypeTags = new List<TextMeshProTeletypeTag>();
            foreach (TextMeshProTag TMPTag in tags) {
                if (TMPTag is TextMeshProTeletypeTag teletypeTag) {
                    teletypeTags.Add(teletypeTag);
                    teletypeTag.OnEndTeletype += IncrementTeletypeBuffer;
                }
            }

            if (teletypeTags.Count == 0)
                OnEndTyping();

            startTime = Time.time;

            tmpText.ForceMeshUpdate();
        }

        private IEnumerator UpdateText() {
            while (true) {
                foreach (TextMeshProTag textMeshProTag in tags) {
                    textMeshProTag.UpdateText(Time.time - startTime);
                }
                tmpText.UpdateVertexData();
                yield return null;
            }
        }

        public void DisplayAllText() {
            foreach (TextMeshProTag TMPTag in tags) {
                if (TMPTag is TextMeshProTeletypeTag teletypeTag)
                    teletypeTag.EndTeletype();
            }
        }

        private void IncrementTeletypeBuffer() {
            teletypeFinishedBuffer++;
            if (teletypeFinishedBuffer == teletypeTags.Count)
                OnEndTyping();
        }

        private void OnDestroy() {
            if (currentAnimationCoroutine != null)
                StopCoroutine(currentAnimationCoroutine);
        }
    }
}
