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
