using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Submodules.TextMeshPro_Extension.Tags;
using TMPro;
using UnityEngine;

namespace Submodules.TextMeshPro_Extension {
    public static class TextParser {

        #region Constants

        private const char OpeningChar = '<';
        private const char ClosingChar = '>';
        private const char EndingChar = '/';

        private const string VariableNameRegex = @"[A-Za-z_][A-Za-z0-9_]*";
        // examples : "VariableName", "_1", "zaiu125__az", ...

        private const string FloatRegex = @"[0-9]+(\.[0-9]+)?";
        // examples : "1", "1.1", "984.16874", ...

        private const string BoolRegex = @"(true)|(false)";
        // examples : "true", "false".

        private const string IntRegex = @"[0-9]+";
        // examples : "2", "23818318165", ...

        private const string TMProTagParametersRegex = @" ?(([^ ]* ?)?=.*)?";
        // examples : " ", "name=  largeSpace", "=  we  i r d Text", ...

        private const string HexadecimalRegex = @"#[0-9a-fA-F]{6}";
        // examples : "#000000", "#FFAB29", ...

        // All the valid text mesh pro tags
        private static readonly List<string> RichTextTagsNames = new List<string> {
            "align", "alpha", "color", "b", "i", "cspace", "font", "indent", "line-height", "line-indent", "link",
            "lowercase", "uppercase", "smallcaps", "margin", "mark", "mspace", "noparse", "nobr", "page", "pos", "size",
            "space", "sprite", "s", "u", "style", "sub", "sup", "voffset", "width"
        };

        #endregion

        private static int animationCurrentIndex = 0;
        private static string rawText = "";
        private static string outputText = "";
        private static List<TextMeshProTag> tags = new List<TextMeshProTag>();
        private static List<TextMeshProTag> validatedTags = new List<TextMeshProTag>();

        /// <summary>Parse a raw text into tags and the text without them.</summary>
        /// <param name="originalText">The raw text to parse.</param>
        /// <param name="tmpText">The reference to the TMP component.</param>
        /// <returns>The parsed text and its tags.</returns>
        public static ParsedText ParseText(string originalText, TMP_Text tmpText, bool isDefaultTeletyped) {
            // Reset variables
            animationCurrentIndex = 0;
            outputText = "";
            tags = new List<TextMeshProTag>();
            validatedTags = new List<TextMeshProTag>();
            rawText = originalText;

            if (isDefaultTeletyped)
                tags.Add(new TextMeshProTeletypeTag(tmpText, 10, 0, 1, false));
            
            // With each iteration, we delete a part of the raw text and add it (or convert it into a tag) to the output
            while (rawText.Length > 0) {
                // Is it a tag ?
                if (rawText[0] == OpeningChar)
                    HandleTag(tmpText);

                // It's not a tag, find the beginning of one and add the text between
                else
                    HandleNotATag();
            }

            // Close the open tags
            while(tags.Count > 0)
                ValidateTag(tags[0]);

            // Return the parsed text
            return new ParsedText(outputText, validatedTags);
        }

        /// <summary>
        /// Adds a given string to the output text.
        /// Doesn't count the spaces for the animation index since the spaces aren't animated.
        /// </summary>
        /// <param name="addedText">The text to add to the output.</param>
        /// <param name="incrementIndex">If the animation index must be incremented (not for TMP tags for example).</param>
        private static void AddToOutputText(string addedText, bool incrementIndex) {
            outputText += addedText;
            if (incrementIndex)
                foreach (char c in addedText)
                    if (c != ' ')
                        animationCurrentIndex++;
        }


        /// <summary>Finish the initialization of the tag and add it to the tags to return.</summary>
        private static void ValidateTag(TextMeshProTag tag) {
            tag.SetEndIndex(animationCurrentIndex);
            validatedTags.Add(tag);
            tags.Remove(tag);
        }


        /// <summary>Adds a part of the raw text to the output.</summary>
        /// <param name="incrementIndex">If the animation index must be incremented (not for TMP tags for example).</param>
        private static void AddRawTextToOutputText(int length, bool incrementIndex) {
            AddToOutputText(rawText.Substring(0, length), incrementIndex);
            rawText = rawText.Substring(length);
        }


        /// <summary>Deals with a tag.</summary>
        /// <param name="tmpText">A reference to the TMP component.</param>
        private static void HandleTag(TMP_Text tmpText) {
            // Search the end of the tag (>)
            int endIndex = rawText.IndexOf(ClosingChar);

            // There is no ending char (>) thus no more tags. We add all the remaining text to the rawText
            if (endIndex == -1) {
                AddRawTextToOutputText(rawText.Length, true);
                return;
            }

            // We now evaluate if the tag is valid

            // Store the potential tag, remove the angle brackets (<>)
            string potentialTag = rawText.Substring(1, endIndex - 1);

            // The tag is from TextMeshPro, we add it to the text
            if (IsTMProValid(potentialTag)) {
                AddRawTextToOutputText(endIndex + 1, false);
                return;
            }

            // We now remove the extra spaces int the tag for our own parsing
            potentialTag = potentialTag.Trim();

            // Empty tag "<>"
            if (potentialTag.Length == 0) {
                // We simply remove it from the text
                rawText = rawText.Substring(2);
                Debug.LogWarning("tag invalid : <>");
                return;
            }

            // Closing tag "</...>"
            if (potentialTag[0] == EndingChar)
                HandleClosingTag(potentialTag, endIndex);
            // Opening tag "<...>"
            else
                HandleOpeningTag(potentialTag, endIndex, tmpText);
        }


        /// <summary>Deal with a closing tag "</...>".</summary>
        /// <param name="potentialTag">The content of the tag (without the angle brackets).</param>
        /// <param name="endIndex">The index of the end of the tag, used to add or remove it.</param>
        private static void HandleClosingTag(string potentialTag, int endIndex) {
            // Remove the closing character "/"
            potentialTag = potentialTag.Substring(1);

            // The tag is "</>", validate the last tag and move on
            if (potentialTag.Length == 0) {
                rawText = rawText.Substring(endIndex + 1);
                if (tags.Count > 0)
                    ValidateTag(tags[tags.Count - 1]);
                else
                    Debug.LogWarning("tag is invalid : </>");
                return;
            }

            // Check if the name corresponds to a previously opened tag
            for (int i = tags.Count - 1; i >= 0; i--) {
                // check if the tag name is the same
                if (tags[i].TagName == potentialTag) {
                    ValidateTag(tags[i]);
                    rawText = rawText.Substring(endIndex + 1);
                    return;
                }
            }

            // It corresponds to nothing, we remove the tag from the raw text
            rawText = rawText.Substring(endIndex + 1);
            Debug.LogWarning($"tag invalid : </{potentialTag}>");
        }


        /// <summary>Deal with an opening tag "<...>".</summary>
        /// <param name="potentialTag">The content of the tag (without the angle brackets).</param>
        /// <param name="endIndex">The index of the end of the tag, used to add or remove it.</param>
        /// <param name="tmpText">A reference to the TMP component, used by the tags to animate the text.</param>
        private static void HandleOpeningTag(string potentialTag, int endIndex, TMP_Text tmpText) {
            // Find the index of the end of the tag name
            int endTagNameIndex = potentialTag.IndexOf(' ');
            if (endTagNameIndex == -1) endTagNameIndex = potentialTag.Length;

            // Store the name
            string tagName = potentialTag.Substring(0, endTagNameIndex);

            // The tag name is syntactically invalid
            if (!IsVariableNameRegex(tagName)) {
                // Remove the tag from the raw text
                rawText = rawText.Substring(endIndex + 1);
                Debug.LogWarning($"tag invalid : <{potentialTag}>");
                return;
            }

            // We remove the valid tag's name and the extra spaces
            potentialTag = endTagNameIndex == potentialTag.Length
                ? ""
                : potentialTag.Substring(endTagNameIndex + 1).Trim();

            Dictionary<string, string> tagParameters = new Dictionary<string, string>();

            bool isValidTag = true;

            // We now check if the parameters are valid
            while (potentialTag.Length > 0) {

                // We search for '='
                int equalCharIndex = potentialTag.IndexOf('=');

                // If '=' doesn't exist or has nothing before or after it
                if (equalCharIndex <= 0 || equalCharIndex == potentialTag.Length - 1) {
                    // The tag isn't valid
                    Debug.LogWarning($"tag is invalid : {rawText.Substring(0, endIndex + 1)}");
                    rawText = rawText.Substring(endIndex + 1);
                    isValidTag = false;
                    break;
                }

                // We store the parameter's name and remove the extra spaces
                string parameterName = potentialTag.Substring(0, equalCharIndex).Trim();

                // If the parameter's name isn't valid
                if (!IsVariableNameRegex(parameterName)) {
                    // We remove the tag from the raw text
                    Debug.LogWarning($"tag is invalid : {rawText.Substring(0, endIndex + 1)}");
                    rawText = rawText.Substring(endIndex + 1);
                    isValidTag = false;
                    break;
                }

                // We remove the name, '=' and the extra spaces
                potentialTag = potentialTag.Substring(equalCharIndex + 1).Trim();

                // We now check if the value is valid
                // Find the end of the value
                int endParameter = potentialTag.IndexOf(' ');
                if (endParameter == -1) endParameter = potentialTag.Length;
                // Store the value
                string parameterValue = potentialTag.Substring(0, endParameter);

                // We add it to the parameters
                tagParameters.Add(parameterName, parameterValue);
                potentialTag = potentialTag.Substring(endParameter).Trim();
            }

            // The tag has a valid syntax
            if (isValidTag) {
                // if the tag is valid
                TextMeshProTag testTag = TextMeshProTagFactory.CreateTag(tagName, tmpText, tagParameters, animationCurrentIndex);
                if (testTag != null) {
                    tags.Add(testTag);
                }
                else
                    Debug.LogWarning($"tag is invalid : {rawText.Substring(0, endIndex + 1)}");
                rawText = rawText.Substring(endIndex + 1);
            }
        }

        
        /// <summary>Deals with a non tag. Adds all the raw text to the output until a tag appears.</summary>
        private static void HandleNotATag() {
            int nextTagIndex = rawText.IndexOf(OpeningChar);
            AddRawTextToOutputText(nextTagIndex == -1 ? rawText.Length : nextTagIndex, true);
        }
        
        
        /// <summary>Tells if a given string is a TMP tag.</summary>
        private static bool IsTMProValid(string input) {
            // Is a color tag like "<#FF0129>"
            if (Regex.Match(input, HexadecimalRegex).Length == input.Length)
                return true;

            // Remove the closing character "/"
            if (input.Length != 0 && input[0] == EndingChar)
                input = input.Substring(1);

            // Empty or starting with a space is invalid
            if (input.Length == 0 || input[0] == ' ')
                return false;

            // Search for the first character after the tag's name. It can be a space, a equal or the end of the tag
            int separatorIndex = Mathf.Min(
                input.IndexOf(' ') == -1 ? input.Length : input.IndexOf(' '),
                input.IndexOf('=') == -1 ? input.Length : input.IndexOf('=')
            );

            // We check if the tag's name is valid (as a text mesh pro tag)
            if (!RichTextTagsNames.Contains(input.Substring(0, separatorIndex))) {
                return false;
            }

            // Remove the tag's name
            input = input.Substring(separatorIndex);

            // Returns if the rest of the tag is valid
            return Regex.Match(input, TMProTagParametersRegex).Length == input.Length;
        }

        #region RegexTests

        /// <summary>Tells if a given string looks like a float input and returns the corresponding value.</summary>
        public static bool IsFloatRegex(out float value, string input) {
            if (Regex.Match(input, FloatRegex).Length == input.Length) {
                value = float.Parse(input, CultureInfo.InvariantCulture);
                return true;
            }
            
            value = 0f;
            return false;
        }

        /// <summary>Tells if a given string looks like an int input and returns the corresponding value.</summary>
        public static bool IsIntRegex(out int value, string input) {
            if (Regex.Match(input, IntRegex).Length == input.Length) {
                value = int.Parse(input, CultureInfo.InvariantCulture);
                return true;
            }

            value = 0;
            return false;
        }

        /// <summary>Tells if a given string looks like a boolean input and returns the corresponding value.</summary>
        public static bool IsBoolRegex(out bool value, string input) {
            if (Regex.Match(input, BoolRegex).Length == input.Length) {
                value = input == "true";
                return true;
            }

            value = false;
            return false;
        }
        
        /// <summary>Tells if a given string looks like a variable name.</summary>
        private static bool IsVariableNameRegex(string input) {
            return Regex.Match(input, VariableNameRegex).Length == input.Length;
        }
        
        #endregion
    }
}