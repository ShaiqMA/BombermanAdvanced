using OpenTK.Windowing.Desktop;
using ITalent.Games.Tiled;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;

namespace ITalent.Games.Examples.Vector;

internal class VectorWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
    : TileGameWindow(gameWindowSettings, nativeWindowSettings)
{
  private VectorRenderLayer _layer = new(Settings);
  private int _increase = 0;

  protected override void OnLoad()
  {
    AddLayer(_layer);
    base.OnLoad();

    CreateRandomDrawing();
  }

  protected override void OnUpdateFrame(FrameEventArgs e)
  {
    base.OnUpdateFrame(e);
    if (KeyboardState.IsKeyDown(Keys.Space))
    {
      CreateRandomDrawing();
    }
  }

  private void CreateRandomDrawing()
  {
    var drawing = _layer.Drawing;
    drawing.Clear();
    int elementCount = Random.Shared.Next(5, 20) + _increase;
    for (int i = 0; i < elementCount; ++i)
    {
      var r = Random.Shared.NextSingle();
      if (r < .6f) drawing.Circle(RandomPoint(), .05f + .2f * Random.Shared.NextSingle());
      else if (r < .9f) drawing.Rect(RandomPoint(), new(
          .1f + Random.Shared.NextSingle() * .4f,
          .1f + Random.Shared.NextSingle() * .4f
        ));
      else drawing.Line(RandomPoint(), RandomPoint());
    }
    ++_increase;
  }

  private Vector2 RandomPoint() => new(Random.Shared.NextSingle() * 2f - 1f, Random.Shared.NextSingle() * 2f - 1f);

  static VectorRenderLayerSettings Settings => new()
  {
  };
}
