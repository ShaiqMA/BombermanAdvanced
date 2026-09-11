using ITalent.Games.Tiled;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ITalent.Games.Examples.Sprites;

/// <summary>
/// Demonstrates moving sprites.
/// You can change the SpriteCount at line 27. Try 1000. Then try 10000!
/// </summary>
/// <param name="gameWindowSettings"></param>
/// <param name="nativeWindowSettings"></param>
internal class Sprites(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
    : TileGameWindow(gameWindowSettings, nativeWindowSettings)
{
  private float tileSize = 64f; // width and height of a tile on screen, in pixels
  private const float PANSPEED = 1f; // Cursor keys
  private const float ZOOMSPEED = 0.4f; // PgUp / PgDn, <0, 1>
  private float time = 0f;

  private SpriteRenderLayer _spriteLayer = new(SpritesSettings);

  protected override void OnLoad()
  {
    _spriteLayer.SpriteCount = 1024;
    for (int i = 0; i < _spriteLayer.Positions.Length; ++i)
    {
      _spriteLayer.Positions[i] = GridPos(i, 0f);
      _spriteLayer.SpriteIndices[i] = i % 3;
      _spriteLayer.Scales[i] = 1f;
    }
    AddLayer(_spriteLayer);
    base.OnLoad();
  }

  protected override void OnUpdateFrame(FrameEventArgs e)
  {
    base.OnUpdateFrame(e);

    time += (float)e.Time;
    if (time > 10f) time -= 10f;
    if (time < 5f)
    {
      // slowly lerp towards a rotating spiral
      for (int i = 0; i < _spriteLayer.Positions.Length; ++i)
      {
        var p = _spriteLayer.Positions[i];
        var q = SpiralPos(i, time);
        _spriteLayer.Positions[i] = .99f * p + .01f * q;
      }
    }
    else
    {
      // slowly lerp towards grid
      for (int i = 0; i < _spriteLayer.Positions.Length; ++i)
      {
        var p = _spriteLayer.Positions[i];
        var q = GridPos(i, time);
        _spriteLayer.Positions[i] = .99f * p + .01f * q;
      }
    }

    
    _spriteLayer.UpdatePositions();

    // randomly change one sprite
    int index = Random.Shared.Next() % _spriteLayer.SpriteIndices.Length;
    int value = Random.Shared.Next() % _spriteLayer.SpriteBrushCount;
    _spriteLayer.SpriteIndices[index] = value;
    _spriteLayer.UpdateSpriteIndices();

    ZoomAndPan();
  }

  private Vector2 SpiralPos(int i, float time)
  {
    float angle = MathF.Tau * MathF.Sqrt(i) + time;
    //float angle = MathF.Tau / 24f * i + time;
    float radius = 1f + 1f * MathF.Sqrt(i);
    return new Vector2(
      radius * (float)Math.Sin(angle),
      radius * (float)Math.Cos(angle)
    );
  }

  private Vector2 GridPos(int i, float time)
  {
    int r = (int)Math.Sqrt(_spriteLayer.SpriteCount + 0.5);
    return new(
          2.2f * ((i % r) - .5f * r),
          2.2f * ((i / r) - .5f * r)
      );
  }

  private void ZoomAndPan()
  {
    // process input
    var input = KeyboardState;

    // zoom map from input
    if (input.IsKeyDown(Keys.PageUp))
    {
      //e.Time;
      tileSize *= 1f + ZOOMSPEED * Time.DeltaF;
    }
    if (input.IsKeyDown(Keys.PageDown))
    {
      //e.Time;
      tileSize *= 1f - ZOOMSPEED * Time.DeltaF;
    }

    // pan map from input
    float panStep = -Time.DeltaF * PANSPEED;
    if (input.IsKeyDown(Keys.Left))
    {
      _spriteLayer.MapOffset.X -= panStep;
    }
    if (input.IsKeyDown(Keys.Right))
    {
      _spriteLayer.MapOffset.X += panStep;
    }
    if (input.IsKeyDown(Keys.Up))
    {
      _spriteLayer.MapOffset.Y += panStep;
    }
    if (input.IsKeyDown(Keys.Down))
    {
      _spriteLayer.MapOffset.Y -= panStep;
    }

    // scale so that a 1x1 sprite is tileSize x tileSize on screen
    //_spriteLayer.MapScale.X = tileSize / ClientSize.X;
    //_spriteLayer.MapScale.Y = tileSize / ClientSize.Y;
    _spriteLayer.ScaleToTilesize(tileSize, ClientSize);
    // mapscale: which portion of the screen is a tile?
  }

  static SpriteRenderLayerSettings SpritesSettings => new()
  {
    TilesetPath = "Resources/tetris.png",
    TilesetSize = new(4, 4),
    Align = AlignmentMode.CenterContain
  };
}
