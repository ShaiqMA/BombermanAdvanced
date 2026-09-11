using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ITalent.Games.Tiled;

public static class Input
{
  public static Vector2i WASD(KeyboardState input)
  {
    if (input.IsKeyDown(Keys.W)) return new(0, 1);
    else if (input.IsKeyDown(Keys.A)) return new(-1, 0);
    else if (input.IsKeyDown(Keys.S)) return new(0, -1);
    else if (input.IsKeyDown(Keys.D)) return new(1, 0);
    else return Vector2i.Zero;
  }
}
