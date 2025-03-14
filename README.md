# The Cinderella City Project - Unreal Edition

This project was originally conceived as an attempt to port the original Cinderella City Project from Unity to Unreal Engine. Due to technical constraints, the scope has been reduced to a single anchor store, Penney's from the 60s/70s era. 

## Editing the Simulation

This game was created using Unreal Engine 5.5. This requires the Epic Games Launcher and an Epic account - see the [official page](https://www.unrealengine.com/en-US/download) for details.

The CinderellaCityUnreal.uproject file in the root of this repo should open in Unreal with a sufficiently powerful computer, but so far I have not tested this on a "clean" install. The only reason I'd expect this not to work is if there are cached files outside the repo or critical files that were inadvertently gitignored.

Open the level anchor-demo (the only level in the project) if for some reason it does not default to it.

## Playing the Simulation

There are build artifacts in the repo, although they haven't been tested on a machine outside of my development machine. If the whole repo is cloned, then presumably the .exe file in Build/Windows/CinderellaCityUnreal.exe should play on Windows.

The controls are listed in-game on the screen. Press the escape key to quit.

## Other Details

More specifics about the objectives and work done in the project are provided in the [Project Details](./project-details.md).