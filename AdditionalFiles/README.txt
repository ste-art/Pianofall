Pianofall v0.4.1

Main page: https://bitbucket.org/steart/pianofall/wiki/Home
Issues tracker (send bugs, feature requests etc): https://bitbucket.org/steart/pianofall/issues
Email for other questions: steart.ru@gmail.com

Controls:
  1. Camera.
     Arrows - move.
     Right mouse button - rotate.
     Middle mouse button - pan.
     Shift + Right mouse button (up/down) - move forwards/backwards

  2. Other.
     Left mouse button - drag objects
     Escape - show / hide menu.

  3. Keyboard:
 _______________________________________________________________________________________________________________________________________________________________________________________________________________
|  | |  |  | | | |  |  | | | | | |  |  | | | |  |  | | | | | |  |  | | | |  |  | | | | | |  |  | | | |  |  | | | | | |  |  | | | |  |  | | | | | |  |  | | | |  |  | | | | | |  |  | | | |  |  | | | | | |  |   |
|  | |  |  | | | |  |  | | | | | |  |  | | | |  |  | | | | | |  |  | | | |  |  | | | | | |  |  | | | |  |  | | | | | |  |  | | | |  |  | | | | | |  |  | | | |  |  | | | | | |  |  | | | |  |  | | | | | |  |   |
|  | |  |  | | | |  |  | | | | | |  |  | | | |  |  | | | | | |  |  | | | |  |  | | |1| |2|  |  |4| |5|  |  |7| |8| |9|  |  |-| |=|  |  | | | | | |  |  | | | |  |  | | | | | |  |  | | | |  |  | | | | | |  |   |
|  | |  |  | | | |  |  | | | | | |  |  | | | |  |  | | |A| |S|  |  |F| |G|  |  |J| |K| |L|  |  | | | |  |  | | | | | |  |  | | | |  |  | | | | | |  |  | | | |  |  | | | | | |  |  | | | |  |  | | | | | |  |   |
|  |_|  |  |_| |_|  |  |_| |_| |_|  |  |_| |_|  |  |_| |_| |_|  |  |_| |_|  |  |_| |_| |_|  |  |_| |_|  |  |_| |_| |_|  |  |_| |_|  |  |_| |_| |_|  |  |_| |_|  |  |_| |_| |_|  |  |_| |_|  |  |_| |_| |_|  |   |
|   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |
|   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | Q | W | E | R | T | Y | U | I | O | P | [ | ] |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |
|   |   |   |   |   |   |   |   |   |   |   |   |   |   | Z | X | C | V | B | N | M | , | . | / |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |
|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|
    Shift - Transpose octave up.
    Ctrl - Transpose octave down.

  4. Custom.
     You can assign custom controls in options menu.
     Action that highlighted blue require a value (like gravity or position)
     and only assingable to MIDI device controls.
     Event based actions are highlighted green (like pressing all notes or blast)
     and can be assigned to any button or midi key or midi control.
     


Commandline arguments:
 Flag  Verbose flag     Modes   Description
   -m, --mode           R,P,A   Pianofall mode. Modes: Realtime, Prerender, Audio.
   -f, --file           R,P,A   Midi file path. Can be also passed without a flag.
   -d, --doubles        R,P     Doubles reuction mode. Modes: 0,1,2,3,4 (see below).
   -o, --out, --output    P,A   Output folder.
   -b, --blocks         R,P     Blocks limit.
   -s, --soundfont      R,P,A   Soundfont. Fonts: Piano1, Piano2.
   -v, --volume         R,P,A   Audio volume. Values: 0 - 100.
   -c, --color          R,P     Color config name or path.
 -bgc, --bgcolor        R,P     Background color (#RRGGBB).
  -fc, --floorcolor     R,P     Floor color (#RRGGBB).

If some flag is not present the default value will be used.

Modes:
  1. Realtime.
     Plays selected midi in realtime.
	 Avaliable doubles reduction modes:
	 0 recommended for normal midis,
	 1 recommended for black midis in realtime mode.
	 
  2. Prerender.
     Renders a track frame by frame. 60 fps.
	 Outputs PNG sequence and raw audio file to folder speified by -o flag or OutPath config value.
      ***WARNING!*** Prerender mode DELETES the output folder on each start!
	 Raw audio is 22050 HZ, 2 channels, 32 bit floating point format.
	 If you have at least quad-core CPU you don't have to watch the whole render process.
	 You can freely switch windows, browse internet, watch videos, play games. In short use PC normally.
	 Avaliable doubles reduction modes: 
	 0 recommended for normal midis,
	 1, 
	 2, 
	 3 recommended for <1M notes black midis, 
	 4 recommended for >1M notes midis.
	 To convert image sequence to video file you have to use external video editor or some utility.
	 Most video editors can do this, try your favorite. 
	 If you don't have one or don't want to use an editor, you can try free utility FFmpeg: https://bitbucket.org/steart/pianofall/wiki/Creating_a_video_with_FFmpeg
  
  3. Audio.
     Renders a audio of the selected track.
	 Outputs raw audio file (audio.raw) to folder speified by -o flag or OutPath config value.
	 Raw audio is 22050 HZ, 2 channels, 32 bit floating point format.
	 Ignores -b, -d and color flags.
	 
Double reduction modes.
  0 - Disabled - Each note generates a block.
  1 - Once per frame - A note can generate a block only once per frame.
	  Recommended for black midis in realtime mode
  2 - Once per millisecond - A note can generate a block only once per 1 millisecond.
  3 - Smart: 2 levels - If the current frame generates less than 660 notes, no doulbes reduction is used.
	  If the current frame generates more than 660 notes, a note can generate a block only once per 1 millisecond.
	  Recommended for < 1 Million black midis in prerender mode.
  4 - Smart: 5 levels - If the current frame generates less than 128 notes, no doulbes reduction is used.
	  If the current frame generates more than 128 and less then 256 notes, a note can generate a block only once per 1 millisecond.
	  If the current frame generates more than 256 and less then 384 notes, a note can generate a block only once per 2 milliseconds.
	  If the current frame generates more than 384 and less then 512 notes, a note can generate a block only once per 3 milliseconds.
	  If the current frame generates more than 512 notes, a note can generate a block only once per 4 milliseconds.
	  Recommended for > 1 Million black midis in prerender mode.
	
Soundfonts:
  Piano1 - Keppy's Steinway Piano - Grand Piano (Black MIDIs)
  Piano2 - Arachno SoundFont - Grand Piano.

BackgroundColor: 
  Background color in  RGB (#RRGGBB). 
  Default value is #DCFEFE

FloorColor: 
  Floor color in RGB (#RRGGBB).
  Default value is #101010

FloorPosition, FloorRotation, FloorScale: 
  Allows to move, rotate and scale the floor.
  x, y, and z can be fractional (e.g. 2.5).
  To remove the floor, set position to some big number, like <x>10000</x>.

Color configs:
  Color configs are in JSON format.
  If config file does not exist, it will be generated with default values on pianofall start.
  You can make your own configs based on the defaults by renaming or copying them.

  1. BwColor.
     Simple black and white notes.
         White: white notes color in RGB.
         Black: black notes color in RGB.

  2. RandomHue.
     Each note gets random hue.
     RandomHue.conf settings based on HSV color model, see https://en.wikipedia.org/wiki/HSL_and_HSV
     All HSV (Hue, Saturation, Value) parameters should be between 0.0 and 1.0.
     Saturation and Value parameters are editable for black and white notes separatly.

  3. RainbowColor.
     Notes get rainbow colors.
     RainbowColor.conf settings uses HSV color model, see https://en.wikipedia.org/wiki/HSL_and_HSV
     All HSV (Hue, Saturation, Value) parameters should be between 0.0 and 1.0.
         StartNote - note index represents left corner of the rainbow. All notes less than StartNote will have same color.
         EndNote - note index represents right corner of the rainbow. All notes greater than EndNote will have same color.
         StartHue - Hue of the left corner of the rainbow. See HSV model.
         EndHue - hue of the right corner of the rainbow. See HSV model.
         Saturation and Value are parameters from HSV.

  4. ChannelRGB
     Black and white notes for each MIDI channel get separate color.
     Colors must be written in RGB format (#RRGGBB).
     There are 16 editable channels with two (black and white notes) color each. The config must contain exactly 16 channels.
     Currently engine splits only channels, but not tracks. 
     This means same channels in different tracks in single midi file will always have same color.

  5. TrackChannelRandomHue
     Each channel on each track gets random hue.
     All HSV (Hue, Saturation, Value) parameters should be between 0.0 and 1.0.
     Saturation and Value parameters are editable for black and white notes separatly.
     

  
Commandline usage examples.

Example 1 (Same as "Open with"):
Pianofall.exe C:\nyancat.mid
Starts nyancat.mid playing or rendering using default values taken from the config.

Example 2:
Pianofall.exe -m Realtime -f C:\nyancat.mid -b 500 -c RainbowColor.conf
Plays nyancat.mid in realtime mode using block limit 500, rainbow colors and other default values taken from the config (Double reduction mode, Soundfont, Volume).

Example 3: 
Pianofall.exe --mode Prerender --file C:\SomeBlackMidi.mid --doubles 3 --soundfont piano1 --out C:\ResultFolder
Renders C:\SomeBlackMidi.mid and stores the result in C:\ResultFolder, using double reduction mode 3, piano1 (arachno) soundfont and other default values taken from the config (Block limit, Volume).

Example 4:
Pianofall.exe -m Audio --File C:\SomeBlackMidi.mid -s Piano2 --o C:\ResultFolder -v 50
Renders the audio of C:\SomeBlackMidi.mid to C:\ResultFolder\audio.raw using piano2 (Keppy's) soundfont with 50% audio volume.