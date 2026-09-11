# ITalent.Games.Tiled

## Setup

- Create a console app
- Reference this package
- Edit the project file, and change `Exe` to `WinExe`, if you don't want a console window to open:

```diff
- <OutputType>Exe</OutputType>
+ <OutputType>WinExe</OutputType>
```

- Apply the template code below

## `Program.cs`

```csharp
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace MyGameProject;

public static class Program
{
  private static void Main()
  {
    var nativeWindowSettings = new NativeWindowSettings()
    {
      ClientSize = new Vector2i(800, 600),
      Title = "The TiledGame Template",
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
```

## `MyGame.cs`

```csharp
using OpenTK.Windowing.Desktop;
using ITalent.Games.Tiled;

namespace MyGameProject;

internal class MyGame(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
    : TileGameWindow(gameWindowSettings, nativeWindowSettings)
{
  private MapRenderLayer _blockLayer = new(MyGameTileSettings);

  protected override void OnLoad()
  {
    AddLayer(_blockLayer);
    base.OnLoad();
  }

  static MapRenderLayerSettings MyGameTileSettings => new()
  {
    TilesetPath = "Resources/tileset.png",
    TilesetSize = new(4, 4),
    TilemapSize = new(9, 9),
    Align = AlignmentMode.CenterContain
  };
}
```

## `Resources\tileset.png`

This example assumes a tileset image is present, which has 4x4 tiles in it. For examples, see the ITalent.Games repository.
