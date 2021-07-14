using System.Collections.Generic;
using Submodules.TextMeshPro_Extension.Tags;

namespace Submodules.TextMeshPro_Extension {
    public struct ParsedText {
        public string RawText { get; private set; }
        public List<TextMeshProTag> Tags { get; private set; }

        public ParsedText(string rawText, List<TextMeshProTag> tags) {
            RawText = rawText;
            Tags = tags;
        }
    }
}
