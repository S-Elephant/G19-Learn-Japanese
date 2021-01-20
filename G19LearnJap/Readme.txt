===============================================================================
About
===============================================================================
This is an open source program for the logitech G19 keyboard to learn the
Japanese Hiragana alphabet. It's written in C#.

===============================================================================
Installation
===============================================================================
1. Download the .NET framework 4.0 (if you haven't already)
http://www.microsoft.com/download/en/details.aspx?id=17851

2. Make sure the G19 Keyboard is plugged in.

3. Extract the PerfMon.zip to any location on disk and run "G19LernJap.exe".


===============================================================================
FAQ / Troubleshooting
===============================================================================

1. To get the tray icon back just delete the "Settings.xml" file in the
PerfMon folder and restart the application.

2. The Configure button in the Logitech profiler might not work with this
application. You can however configure this application from the system tray
or directly from the "Settings.xml" located in the application folder.

3. The button presses are checked every 100ms. For the pause button this might
mean that you have to press it a little longer than just a quick press (because
you might press it within 2 intervals of 100ms).

===============================================================================
Usage
===============================================================================

- Use the LCD up, right, down and left buttons to chose the correct
  translation.
- Use the LCD's "ok" key to switch between Hiragana->Romaji and
  Romaji->Hiragana.
- The numbers in the upper left corner mean:
{correct answers given}/{current answer}/{total # of questions}

===============================================================================
Adding and modifying new content
===============================================================================

<Application Path>/Resources/Letters/<Your chapter name>/<Your *.png files>
<Your chapter name> = Name of what appears in the loading screen
<Your *.png files> = The png files that are being shown on screen. Note that
their filesnames are the translations in Romaji.

This can be used to add other alphabets like the Katakana or just almost any
other alphabet.

Optional:
<Application Path>/Resources/Letters/<Your chapter name>/StaticChoices.xml
StaticChoices.xml adds specific possible (wrong) answers to questions. For
example:
If you want to bind ra, chi and sa together then you add the entry:
    <Key romaji="ra">
      <StaticChoice>chi</StaticChoice>
      <StaticChoice>sa</StaticChoice>
    </Key>

Now whenever the user must translate "ra" the chi and sa are also displayed
as multiple choice options. If you define less than 3 static choices per
hiragana then the empty spots will be filled with random choices from the
same chapter.

To make sure that this also applies to chi and sa just add the attribute:
alsoReverse="true"

Important:
- Make sure that when you create a chapter, the chapter has at least 4
  images.
- Every key is only allowed once in the StaticChoices.xml file. This includes
  'reverses'. For example:
    <Key romaji="ra">
      <StaticChoice>chi</StaticChoice>
      <StaticChoice>sa</StaticChoice>
    </Key>
	<Key romaji="chi">
		<StaticChoice>na</StaticChoice>
	</Key>
	Will throw a warning and the 2nd key entry in this case will be ignored.
- If you put in 4 or more static choices for one Hiragana then only the first
  3 are used (the 1st spot contains the correct answer ofcourse).
===============================================================================
Credits
===============================================================================

See the credits section in the Config Screen.


===============================================================================
Contact
===============================================================================

napoleonite2010@gmail.com (please be aware that it in some cases may take a
while to respond).

===============================================================================
Changelog
===============================================================================

Version 1.0.0
	- First release