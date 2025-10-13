# CrabGameSequencedDropGameMode
A BepInEx mod for Crab Game that adds the Sequenced Drop custom game mode.

Depends on [CustomGameModes](https://github.com/lammas321/CrabGameCustomGameModes)

- The blocks will drop in a sequence
- You must survive for the entire sequence
- Be respectful, no pushing >=(

## How does this game mode work?
Everyone is given a revolver with unlimited (2,147,483,647) ammo, which can be used to either hit other players, boost yourself around, or jump higher from its recoil.

The game mode will pick a random sequence from your SequencedDropSequences folder (must be in the same folder as the SequencedDropGameMode.dll file) and then play out that sequence.
Each sequence will drop blocks in a predetermined pattern (with random rotation and flipping to have some variety).

When a sequence ends, it will continue picking random sequences and playing them faster and faster until either time runs out, or the game has determined it can no longer play any sequences without hitting the maximum height, in which case, the game will congratulate you for reaching the top.
If only one player is remaining, it will lower the time remaining to 10 seconds to end the game sooner.

## Where can I get some sequences?
There are a couple available in this repository, as well as a .zip of them in the releases for this mod.

## How can I make my own sequences?
Sequences are just plain text files that are interpretted by Sequenced Drop.
All you have to do is make your own text file and follow the same formatting that the other sequences use.

It's mostly self explanatory, but can take some practice/messing around to get used to.
When making a more complicated or large sequence, I recommend trying to make it play in steps, and plan ahead for how it'll go.
For my sequences, I built them out in Minecraft at their different steps to help me during their creation.


## Fork Version


can now add "Difficulty" and "Madeby" on sequenced map

ex:

Name=Advanced Quad Spiral

MinPlayers=1

MaxPlayers=-1

Difficulty=Insane

Madeby=Slash and Dash

