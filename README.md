# touchpad-peace
Application which will suppress unwanted touchpad activity while you type in your windows laptop.

# How does it work
Build the application, and run it.   When running, it will do the following:  After you type something (any typed key)
the application will suppress mouse clicks until the mouse is moved for 0.2 seconds.  How does this help?  If you are
typing using your laptop and you accidentally hit the touchpad with your thumb, the unintended mouse click you 
generated will not jump your cursor to some other location, create unwanted selections, etc.
Are there any drawbacks?  A minor one:  If you click after typing without moving the mouse at all, the click will be
suppressed.  But you will find the **for almost all the cases** you move the mouse first prior to clicking.  The 
annoyance of having to move the mouse for the cases where you don't is nothing but a fraction of the annoyance 
produced by unwanted mouse activity when you type.

## The following applies ONLY IF YOU HAVE AN *EXTERNAL* TOUCHSCREEN MONITOR
Notice that laptop touchscreens are NOT considered "EXTERNAL", so the following does not apply for those casea:  Due to 
limitations in Windows API calls, TouchpadPeace will not be able to distinguish activity from the external touchscreen
monitor and the touchpad.   I recommend you DO NOT USE TOUCHPADPEACE if this is your situation.   Again, if your laptop
has an integrated touchscreen, THIS DOES NOT APPLY, and you can use touchpadpeace on that device just fine.

## How to run automatically every time you log in
Read the information in file "HowToAutoRunAfterLogin.txt" which is part of the Visual Studio Project.
