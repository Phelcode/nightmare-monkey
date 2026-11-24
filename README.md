# Schedule 1 Archipelago Randomizer Setup Guide

## Required Software

- [Archipelago](https://github.com/ArchipelagoMW/Archipelago/releases/latest)
- [The Schedule I apworld](https://github.com/MacH8s/Narcopelago/releases/latest),
- [Thunderstore Mod Manager](https://www.overwolf.com/app/thunderstore-thunderstore_mod_manager)
- [Narcopelago Mod](https://thunderstore.io/c/schedule-i/p/Narcopelago/Narcopelago/)

## How to play

First, you need a room to connect to. For this, you or someone you know has to generate a game.  
This will not be explained here,
but you can check the Archipelago Setup Guide.

You also need to have [Archipelago](https://github.com/ArchipelagoMW/Archipelago/releases/latest) installed
and the [The Schedule I apworld](https://github.com/MacH8s/Narcopelago/releases/latest) installed into Archipelago.

### Install Mod

Install Thunderstore Mod Manager and open it.
Choose Schedule I and make a profile for Archipelago, name it whatever you like.
Search for 'Narcopelago' in the mod search and install it.
Search for 'MelonLoader' in the mod serach and choose version ***0.7.1***.
From there you can launch the game as Modded on the top right and your install has been complete! You must launch the game this way every time you want to play Archipelago.

### Joining Game

Use In-Game UI to connect to server. Once connected, Create a new world and skip the prologue.
Make sure to save as often as you can, and you are able to rejoin. Restart your game if you need to rejoin the world!
If you want to play with friends (Untested): Invite them to your lobby. All of you connect as same archipelago Info, Load into world.

## Switching Rooms
Restart your game to switch rooms. There may be some issues if you don't do so even if it shows things are working.


# Schedule 1

## Where is the options page?

The [player options page for this game](../player-options) contains all the options you need to configure and export a
config file.

## What does randomization do to this game?

 - Sends checks from all missions.
 - Sends and receives all customer checks and items.
 - Customers can be unlocked by receiving them through archipelago when randomize_customers is true in the YAML.
 - checks for samples will be sent no matter the settings and are functional.
 - Dealers will send checks when recruiting regardless of settings.
 - Dealer AP unlock will allow user to then recruit them in game. Check is still possible when having them as a possible contact.
 - Suppliers will not be unlockable if suppliers are randomized and only unlocked through ap items
 - Suppliers give checks for unlocking them when suppliers are not randomized
 - Every Action that would cause cartel influence in a region to drop is a check (x7 per region)
 - Unable to reduce cartel influence naturally and cartel influence items added to pool when randomize_cartel_influence is true
 - Level up rewards are suppressed when randomize_level_up_rewards is true
 - Level up rewards are added to the item pool when randomize_level_up_rewards is true
 - Whenever you'd nomrally get unlocks for leveling up, you get a check regardless of the option
 - Deathlink is sent when a player dies or when they arrested. Recieved deathlink causes player to get sent to the hospital by default. Other options are available.
 - Property and busniesses give checks when purchased
 - Randomized properties or businesses will not be unlocked when purchased if randomization on, properties and/or businesses will be added to the item pool
 - Recipe checks are sent when recipes are learned
 - Cash for trash are sent every 10 trash burned
 - Filler items, including bundles are claimed via the AP phone app
 - Purchasing from Jen Heard and entering keycode into sewer office counts as a check

## Once I'm inside Schedule1, how do I play Schedule1AP

Use In-Game UI to connect to server. Once connected, Create a new world and skip the prologue.
Make sure to save as often as you can, and you are able to rejoin. Restart your game if you need to rejoin the world!
If you want to play with friends (Untested): Invite them to your lobby. All of you connect as same archipelago Info, Load into world.

## Tracker: Use the Pop Tracker!
[Poptracker by Razgriz](https://github.com/RazgrizEast/schedule-1-poptracker-razgriz/releases/latest)

## Multiplayer
Everyone connect using AP as same player/yaml name. 
Known issue: People who aren't host will have fillers within AP app too, so they'll be duplicate.
Suggestion is to not collect fillers if you are not the host.

## A statement on the ownership over Schedule1AP

Schedule I apworld is MIT license. Created by MacH8s

## Building from source (Windows, IL2CPP)

This mod targets the **IL2CPP** version of Schedule I. Its references point at the game's
`Schedule I_Data\Managed` assemblies plus MelonLoader's generated `MelonLoader\Il2CppAssemblies`
and `MelonLoader\net6` folders.

### Prerequisites

- Windows with the [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) (or a newer SDK / Visual Studio 2022 with the .NET 6 targeting pack)
- A Schedule I install with MelonLoader 0.7.x (IL2CPP) installed, and the game launched once with it, so that `<GameDir>\MelonLoader\Il2CppAssemblies` and `<GameDir>\MelonLoader\net6` exist

### Build

Point the build at your game folder using the `ScheduleIPath` property (the default is `F:\Steam games\steamapps\common\Schedule I`):

```powershell
dotnet build Narcopelago.sln -c Release -p:ScheduleIPath="C:\Program Files (x86)\Steam\steamapps\common\Schedule I"
```

You can also set the `ScheduleIPath` environment variable instead of passing `-p:ScheduleIPath=...`, or change the default value in `Narcopelago.csproj`.

The build outputs `bin\Release\net6.0\Narcopelago.dll` together with the `Data` folder (items/locations/regions/victory JSON, synced from the Schedule I apworld). The PostBuild step also copies `Narcopelago.dll`, its NuGet dependency `Archipelago.MultiClient.Net.dll` (required at runtime - without it the mod fails to load AP types and cannot connect), and `Data\` into `<ScheduleIPath>\Mods`; override the destination with `-p:ModsOutputPath=...`.

To play, launch the game modded (Thunderstore Mod Manager or MelonLoader). The mod loads its `Data` folder from next to the DLL, or from `Mods\Narcopelago\Data` / `Mods\Data`.
