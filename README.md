# NieR:Automata™ Music Player Experiment
A very hacky and probably unoptimized player built with [NAudio](https://github.com/naudio/NAudio) for BGM extracted from NieR:Automata™.

Plays and loops different streams together (vocals, instruments, and 8-bit versions).

## Extracting the BGM
1. You'll need [NME2](https://github.com/TypeA2/NME2)
2. Get all the BGM_X_XXX.wsp files from `NieRAutomata\data\sound\stream` and put them into a folder
3. Open a prompt where NME2 is and enter
   ```
   nme <path_to_BGM_files> -ac vorbis -aq 500k -sf fltp -p BGM*.wsp
   ```
4. Use [names.txt](names.txt) as a guide for the output files.

## Beats info
The [beats.txt](beats.txt) file contains the BPM and the looping points (in samples) of the tracks that I have analyzed with Audacity. I'll try to find the real looping information from the game (or anywhere for that matter) but for now, this should do.

## TODO
- [X] ~~A proper UI~~
- [X] ~~A proper fade effect~~
- [X] ~~Getting rid of the occasional subtle clicking when looping~~
