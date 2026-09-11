# ITalent.Games.Tiled

## Description

`ITalent.Games.Tiled` is a class library for making old-school arcade-style (8-bit/16-bit) 2D games very easily.
It is very light weight, platform independent and extremely fast. The only dependency it has is OpenGL, which has been available on every platform since at least 1990.

## Install

- Clone, fork or download the `ITalent.Games.Tiled` class library.
- The projects that are using audio have a dependency on Foster.Audio, which is a git submodule from a different repository. To add that, you need to run those code once on the root folder of the solution:

		git submodule update --init --recursive

## Create a project

- Create a new Console Project
- Open the .csproj file and change it from Exe to WinExe type:

```diff
- <OutputType>Exe</OutputType>
+ <OutputType>WinExe</OutputType>
```

- Reference the `ITalent.Games.Tiled` class library.

To get a project working, we'll need a little bit of boiler plate code. I advise to have a look at the examples, and just copy one of those as a starting point.
At first you start with two classes. The program class is the entry point to your application. It opens the main game window.

### Program class

- Copy the `Program.cs` file from the `ITalent.Games.Examples` project and change the name to your game (`MyGame`in this example):

		using OpenTK.Mathematics;
		using OpenTK.Windowing.Common;
		using OpenTK.Windowing.Desktop;

		namespace ITalent.Games.Examples;

		public static class Program
		{
		  private static void Main()
		  {
			var nativeWindowSettings = new NativeWindowSettings()
			{
			  ClientSize = new Vector2i(800, 600),
			  Title = "My Game",
			  // This is needed to run on macos
			  Flags = ContextFlags.ForwardCompatible,
			  Vsync = VSyncMode.On
			};
			using (var window = new MyGame(GameWindowSettings.Default, nativeWindowSettings))
			{
			  window.Run();
			}
		  }
		}

### Game Window class

This class defines the game's main window. It inherits from `TileGameWindow`. This allows you to override `OnLoad` and `OnUpdate`, and it gives access to properties like `ClientSize`.

- Copy the HelloWorld class from the `ITalent.Games.Examples` project and change the name to your game (`MyGame`in this example):

		using OpenTK.Windowing.Desktop;
		using ITalent.Games.Tiled;

		namespace ITalent.Games.Examples;

		internal class MyGame(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
			: TileGameWindow(gameWindowSettings, nativeWindowSettings)
		{
		  protected override void OnLoad()
		  {
			// add layers
			
			base.OnLoad();
			
			// initialize layers
		  }
		}

## Example projects

Some games can be deceptively difficult to build. On the internet, you can find lists of good projects at your level. Here's some suggestions:

Easy:
- etch-a-sketch
- snake
- ninja-writer
- space invaders

Medium:
- Boulder-dash
- Tetris
- Zelda

Hard:
- Super mario

## Basic architecture

You inherit the `TileGameWindow`
- override OnLoad to initialize
- override OnUpdate for implement input and animation

Your game consists of layers (`TileLayer`). Each has its own spritemap and tilemap. Each layer has its own scaling mode. These settings are defined in the layer's `TileGameSettings`. 

    Warning: currently the tilemaps width needs to be a multple of four (4)!


You can also manually do scaling and panning, see `ITalent.Games.Examples.Demo`

## Moving sprites

This layer type is not yet implemented. It will be used for things like the main character, the enemies, bullits and effects.

## Moving tilemaps

This was never an option on old 8-bit systems, but there are every little technical drawbacks of implementing something like this for platforms, moving map elements and such.

## Dynamically changing tilemaps

Used typically for something like editing or destroying the map. This is possible, but it needs an example and some wrapper code.

## Overlay text

This is possible and demonstrated in the tetris and demo examples. Technically, it is just the same as a normal tilemap. There is a TextWriter class, however, to help you write text on screen using strings.

## Sound

This feature needs to be ported from `OpenTK.OpenAL`.
- Provide free licence arcade game sounds and tunes

## TODO

- [ ] ~~make (0, 0) top left corner (instead of bottom left)~~
- [x] scaling
- [x] overlay text
- [x] moving sprites
- [x] tetris
- [x] hello world
- [x] sound
- [ ] vector layer
- [x] large scrolling map demo
- [ ] colored text
- [x] text-effects like drop-shadow
