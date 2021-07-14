using TMPro;

namespace Submodules.TextMeshPro_Extension.Tags {
    /// <summary>Base class for custom tags.</summary>
    public abstract class TextMeshProTag {
        public string TagName { get; }

        protected readonly TMP_Text rawText;
        protected readonly int startIndex;
        protected int endIndex;

        protected TextMeshProTag(TMP_Text rawTextRef, string tagName, int startIndex) {
            rawText = rawTextRef;
            TagName = tagName;
            this.startIndex = startIndex;
            endIndex = startIndex;
        }

        public void SetLastIndex(int endIndex) {
            this.endIndex = endIndex;
        }

        /// <summary>Update the animation of the text from the startIndex to the endIndex.</summary>
        /// <param name="time">The time since the beginning of the animation.</param>
        public abstract void UpdateText(float time);

        /// <summary>Initialize the tag.</summary>
        public abstract void Initialize();
    }
}
