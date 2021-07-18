using TMPro;

namespace Submodules.TextMeshPro_Extension.Tags {
    /// <summary> Base class for custom tags. </summary>
    public abstract class TextMeshProTag {
        public abstract string TagName { get; }

        protected readonly TMP_Text rawText;
        protected readonly int startIndex;
        protected int endIndex;

        protected TextMeshProTag(TMP_Text rawTextRef, int startIndex) {
            rawText = rawTextRef;
            this.startIndex = startIndex;
            endIndex = startIndex;
        }

        /// <summary> Set the index of th last character that is going to be animated. </summary>
        public void SetEndIndex(int endIndex) {
            this.endIndex = endIndex;
        }

        /// <summary> Update the animation of the text from the startIndex to the endIndex. </summary>
        /// <param name="time"> The time since the beginning of the animation. </param>
        public abstract void UpdateText(float time);

        /// <summary> Initialize the tag. </summary>
        public abstract void Initialize();
    }
}
