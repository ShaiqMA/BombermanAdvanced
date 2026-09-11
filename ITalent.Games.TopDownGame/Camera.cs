using ITalent.Games.Tiled;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ITalent.Games.TopDownGame;

internal class Camera(MapLayer _blockLayer)
{
  private float tileSize = 64f; // width and height of a tile on screen, in pixels
  private const float PANSPEED = 1f; // Cursor keys
  private const float ZOOMSPEED = 0.4f; // PgUp / PgDn, <0, 1>

  public float TileSize => tileSize;

  public void Update(KeyboardState input, Vector2i ClientSize, Vector2 playerPosition)
  {
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
    //_blockLayer.MapScale.X = ClientSize.X / tileSize;
    //_blockLayer.MapScale.Y = ClientSize.Y / tileSize;
    _blockLayer.ScaleToTilesize(tileSize, ClientSize);
    // mapscale: how many tiles fit on screen?

    //PanWithCursorKeys(deltaTime, input);
    PanToFollowPlayer(ClientSize, playerPosition);
    LimitPanningToEdges(ClientSize);
  }

  private void PanWithCursorKeys(float deltaTime, KeyboardState input)
  {
    // pan map from input
    float panStep = deltaTime * PANSPEED * tileSize;
    if (input.IsKeyDown(Keys.LeftShift) || input.IsKeyDown(Keys.RightShift))
    {
      panStep *= 10f;
    }
    if (input.IsKeyDown(Keys.LeftControl) || input.IsKeyDown(Keys.RightControl))
    {
      panStep *= .1f;
    }
    if (input.IsKeyDown(Keys.Left))
    {
      _blockLayer.MapOffset.X -= panStep;
    }
    if (input.IsKeyDown(Keys.Right))
    {
      _blockLayer.MapOffset.X += panStep;
    }
    if (input.IsKeyDown(Keys.Up))
    {
      _blockLayer.MapOffset.Y += panStep;
    }
    if (input.IsKeyDown(Keys.Down))
    {
      _blockLayer.MapOffset.Y -= panStep;
    }
  }

  private void PanToFollowPlayer(Vector2i ClientSize, Vector2 playerPosition)
  {
    // pan map to follow player
    var idealMapPosition = playerPosition;
    idealMapPosition.X += -.5f * ClientSize.X / tileSize + .5f;
    idealMapPosition.Y += -.5f * ClientSize.Y / tileSize + .5f;

    //_cameraSpeed

#if true

    // yessss ... achieved smoothness :)

    var delta = idealMapPosition - _blockLayer.MapOffset;
    var len = tileSize * delta.Length;
    if (len > 1f)
    {
      delta *= Time.DeltaF;
      _blockLayer.MapOffset += delta;
    }
    else
    {
      _blockLayer.MapOffset = idealMapPosition;
    }
#else
    // no delay
    _blockLayer.MapOffset.X = idealMapPosition.X;
    _blockLayer.MapOffset.Y = idealMapPosition.Y;
#endif

  }

  private void LimitPanningToEdges(Vector2i ClientSize)
  {
    //float maxX = tileSize * _blockLayer.Size.X - ClientSize.X;
    //float maxY = tileSize * _blockLayer.Size.Y - ClientSize.Y;
    float maxX = _blockLayer.Tiles.Size.X - ClientSize.X / tileSize;
    float maxY = _blockLayer.Tiles.Size.Y - ClientSize.Y / tileSize;
    _blockLayer.MapOffset.X = Clamp(_blockLayer.MapOffset.X, 0f, maxX);
    _blockLayer.MapOffset.Y = Clamp(_blockLayer.MapOffset.Y, 0f, maxY);
  }

  private static float Clamp(float x, float min, float max) => x < min ? min : x > max ? max : x;

}
