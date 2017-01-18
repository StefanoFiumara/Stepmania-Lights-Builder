# Stepmania Lights Builder
A small program that automatically adds a lights-cabinet chart to all stepfiles in a stepmania directory, for use with dance machines running SM5
---
A pet peeve that I've had with my ITG Cabinet ever since I switched over to using Stepmania 5 is that SM5 doesn't automatically generate light patterns for charts that don't have them predefined. Instead, all the cabinet lights turn on during a song for each beat, after some time it becomes incredibly annoying and blinding. I was actually a big fan of the way the original ITG software (or even openITG) handled creating light patterns for charts that didn't include them.

This program will crawl through your stepmania songs directory and automatically generate a chart for the **lights-cabinet** style, which is what Stepmania 5 sends to its lights driver. The program chooses a reference chart to base the lights chart off of, it goes in this order:

1. Single - Hard
2. Single - Challenge
3. Single - Highest Available
4. Double - Hard
5. Double - Challenge
6. Double - Highest Available

The basic tap notes are mapped 1-1 with the four marquee lights at the top of the cabinet, while the bass lights only respond to quarter notes, holds, or jumps.

You can see a video of the automatically generated light patterns for one of my charts [Here](http://www.youtube.com/watch?v=zrA6GCFxqMA).