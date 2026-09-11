using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace ITalent.Games.Examples.Demo;

public static class Program
{
  private static void Main()
  {
    var nativeWindowSettings = new NativeWindowSettings()
    {
      ClientSize = new Vector2i(800, 600),
      Title = "Demo",
      // This is needed to run on macos
      Flags = ContextFlags.ForwardCompatible,
      Vsync = VSyncMode.On
    };
    using (var window = new Demo(GameWindowSettings.Default, nativeWindowSettings))
    {
      window.Run();
    }
  }
}
