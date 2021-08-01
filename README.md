**!Disclaimer! I'm just having fun here, this repo is public so feel free to use it, but do not expect support or complete reliability. It is still in development and I'm mostly testing things depending on my needs. If you're looking for a proper way to animate your texts, there are probably some cheap solutions on the asset store, way better than this.**

# TextMeshPro_Extension

**A Unity extension for text mesh pro. Use and create custom tags for special effects on your texts.**

All tags are supposed to work with each other (so you can make a wave with the jitter effect for example).

All tags are supposed to scale with the font size.

Tags currently in the project :
  - Teletype
  - Wave
  - Jitter

## How to setup your Unity text game object

Simply add the `TextMeshProExtended` script to a game object and reference the `TextMeshPro` component you want to animate in the editor.
You can now use custom tags in the text box of the `TextMeshProExtended` component.

*I'm aware that it is not handy in the current state as you need to write without preview, and also need to copy paste your current texts inside the component. It will change eventually.*


## How to use

In your text, include your custom tags as follows : `<tagName integerParameter=12 stringParamater=value>`

Extra spaces are supported so if you prefer to put space around equal signs or whatever, it should work just fine.

Parameters are not necessary, skipped parameters are assigned a default value. (I'm looking into a way of changing the default values outside of the code)

To close your tag, a classic : `</tagName>` will do. You can also close the last opened tag with : `</>`. I'm considering allowing multiple closure in a single tag but it doesn't work yet. Something like `<//>` for example.

If tags aren't closed, they are considered animating from the opening tag to the end of the text.


## Included tags

### Teletype (`<teletype>`)

Make your text appear over time. It is the most important tag and works a bit differently than others.

Usually, tags are animated all together. However, the teletype tags are animated one after the other (in the order of appearance of the opening tag in the text). So you can setup different parameters inside your sentence (for example to write "..." more slowly than the rest of the text).

If `Is Default Teletyped` is selected in the component, the text will consider that there is an opening tag at the begining of the sentence, like this : `<teletype>`.
You can then close it manually inside the sentence if you need to.

#### Parameters

  - `speed` (float) : The number of characters displayed by second.
  - `isFading` (bool) : If the characters appear fading in or not (color from alpha 0 to 1).
  - `spacing` (int) : Only used if `isFading` is true. The number of characters that are fading simultaneously (for example, if spacing=5, when the first character will be completely visible (alpha=1), the 6th character will start appearing).
  - `delay` (float) : Pause before the animation (in seconds).


### Wave (`<wave>`)

Pretty straightforward, the animated text will wave.

#### Parameters

  - `period` (float) : The time for a character to make a full up and down cycle, the higher the slower the animation will be.
  - `amplitude` (float) : The height of the wave, an amplitude of 1 means one character's height difference from top to bottom.
  - `spacing` (int) : The distance (in characters) between 2 peaks. The higher the smoother the curve will be.
  - `offset` (float) : The offset int time for the Sin function. It honestly feels completely useless, I'll probably remove it sometime.


### Jitter (`<jitter>`)

Animate the characters in a chaotic way, useful for representing anger or distorted voice.

#### Parameters

  - `intensity` (float) : How far the characters are going to go. An intensity of 1 will make the characters touch each other. An intensity above 1 can make the reading painful.
  - `speed` (float) : The number of times the characters are going to change direction by second. High values recommended (above 10-20).



## Scripts

  - `SetText` : Changes the text to be parsed. it **does not** refresh the animation nor the text itself.
  - `AnimateText` : Parses the text and starts the animation.
  - `DisplayAllText` : Instantly end all teletype tags.
