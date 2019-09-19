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

## "Verified" working music (little to no clicking during playback)
1. Pascal
2. Amusement Park
3. A Beautiful Song
4. Memories of Dust
5. The Sound of the End

## TODO
- [X] ~~Design proper UI~~
- [X] ~~Implement proper fade effect~~
- [X] ~~Get rid of the occasional subtle clicking when looping~~
- [ ] Fade when switching songs
- [ ] Gimmicky pause/resume sound effect
- [ ] Support for songs with other versions like Alien Manifestation
- [ ] Rework mappings

## Known issues
- Resuming after being paused for quite a while will make the music choppy
