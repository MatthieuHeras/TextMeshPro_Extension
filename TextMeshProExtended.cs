using System;
using System.Collections;
using System.Collections.Generic;
using Submodules.TextMeshPro_Extension.Tags;
using TMPro;
using UnityEngine;

namespace Submodules.TextMeshPro_Extension {
    /// <summary> Component that animates a TextMeshPro component with custom tags. </summary>
    public class TextMeshProExtended : MonoBehaviour {
        public event Action OnEndTyping = delegate {  };
        
        [SerializeField] private bool isDefaultTeletyped = true;
        [TextArea]
        [SerializeField] private string text = default;
        [SerializeField] private TMP_Text tmpText = default;
        
        private List<TextMeshProTag> tags;
        private List<TextMeshProTeletypeTag> teletypeTags;

        private float startTime;
        private float teletypeStartTime;

        private Coroutine currentAnimationCoroutine;

        public void SetText(string newText) {
            text = newText;
        }

        public void AnimateText() {
            InitializeAnimation();

            foreach (TextMeshProTag textMeshProTag in tags) {
                textMeshProTag.Initialize();
            }
            
            foreach (TextMeshProTag textMeshProTag in teletypeTags) {
                textMeshProTag.Initialize();
            }
            
            if (currentAnimationCoroutine != null)
                StopCoroutine(currentAnimationCoroutine);
            currentAnimationCoroutine = StartCoroutine(UpdateTags());
        }

        private void InitializeAnimation() {
            ParsedText parsedText = TextParser.ParseText(text, tmpText, isDefaultTeletyped);
            tmpText.text = parsedText.RawText;
            tags = parsedText.Tags;

            teletypeTags = new List<TextMeshProTeletypeTag>();
            for (int tagIndex = tags.Count - 1; tagIndex >= 0; tagIndex--) {
                if (tags[tagIndex] is TextMeshProTeletypeTag teletypeTag) {
                    teletypeTags.Insert(0, teletypeTag);
                    tags.RemoveAt(tagIndex);
                }
            }

            if (teletypeTags.Count == 0)
                OnEndTyping();
            else
                teletypeTags[0].OnEndTeletype += DecrementTeletypeTags;

            startTime = Time.time;
            teletypeStartTime = startTime;

            tmpText.ForceMeshUpdate();
        }

        private void DecrementTeletypeTags() {
            teletypeTags[0].OnEndTeletype -= DecrementTeletypeTags;
            teletypeTags.RemoveAt(0);

            if (teletypeTags.Count == 0) {
                OnEndTyping();
                return;
            }

            teletypeStartTime = Time.time;
            teletypeTags[0].OnEndTeletype += DecrementTeletypeTags;
        }

        private IEnumerator UpdateTags() {
            while (true) {
                UpdateTeletypeTags();
                UpdateOtherTags();
                
                tmpText.UpdateVertexData();
                yield return null;
            }
        }

        private void UpdateOtherTags() {
            foreach (TextMeshProTag textMeshProTag in tags) {
                textMeshProTag.UpdateText(Time.time - startTime);
            }
        }

        private void UpdateTeletypeTags() {
            if (teletypeTags.Count == 0)
                return;
            
            teletypeTags[0].UpdateText(Time.time - teletypeStartTime);
        }

        public void DisplayAllText() {
            foreach (TextMeshProTag TMPTag in tags) {
                if (TMPTag is TextMeshProTeletypeTag teletypeTag)
                    teletypeTag.EndTeletype();
            }
        }

        private void OnDestroy() {
            if (currentAnimationCoroutine != null)
                StopCoroutine(currentAnimationCoroutine);
        }
    }
}
