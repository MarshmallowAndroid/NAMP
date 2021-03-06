# NieR:Automata™ Music Player
A hacky and probably unoptimized player built with [NAudio](https://github.com/naudio/NAudio) for BGM extracted from NieR:Automata™.

Plays and loops different streams together (vocals, instruments, and 8-bit versions).

## Extracting the BGM
1. You'll need [NME2](https://github.com/TypeA2/NME2).
2. Get all the BGM_X_XXX.wsp files from `NieRAutomata\data\sound\stream` and put them into a folder.
3. Open a prompt where NME2 is and enter:
    ```
    nme <path_to_BGM_files> -ac vorbis -aq 500k -sf fltp -p BGM*.wsp
    ```
4. Use [names.txt](names.txt) as a guide for the output files.

## Mapping info
The [mapping.txt](mapping.txt) file contains the looping points (in samples) of each track, as well as which file corresponds to each track.

## Loop method fixed
There's no more loop clicking during playback due to the new method.\
However, if ever there is any clicking of any sort then it's just bad loop points, which can be fixed by editing the mapping.

## TODO
- Improve UI
- ~~Implement proper fade effect~~
- ~~Get rid of the occasional subtle clicking when looping~~
- Fade when switching songs
- ~~Gimmicky pause/resume sound effect~~
- Support for songs with other versions like Alien Manifestation
- Rework mappings
- Sync tracks when switching between them to avoid sounding "wrong"

## Known issues
- After seeking, the next loop will click.
