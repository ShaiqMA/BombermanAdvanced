using OpenTK.Mathematics;

namespace ITalent.Games.Tiled;

public class Camera(Vector2i _minVisibleTileCount, Vector2i _maxVisibleTileCount, MapRenderLayer _blockLayer)
{
  private float tileSize = 64f; // width and height of a tile on screen, in pixels

  public float TileSize => tileSize;

  public void Update(Vector2i ClientSize, Vector2 playerPosition)
  {
    float minTilesize = MathHelper.Min(
      (float)ClientSize.X / _minVisibleTileCount.X,
      (float)ClientSize.Y / _minVisibleTileCount.Y
    );
    float maxTilesize = MathHelper.Max(
      (float)ClientSize.X / _maxVisibleTileCount.X,
      (float)ClientSize.Y / _maxVisibleTileCount.Y
    );

    float mapAspect = (float)_blockLayer.Tiles.Size.Y / _blockLayer.Tiles.Size.X;
    float clientAspect = (float)ClientSize.Y / ClientSize.X;
    if (mapAspect > clientAspect)
    {
      // map is higher, width is leading, scroll vertically
      tileSize = (float)ClientSize.X / _blockLayer.Tiles.Size.X;
    }
    else
    {
      // map is wider, height is leading, scroll horizontally
      tileSize = (float)ClientSize.Y / _blockLayer.Tiles.Size.Y;
    }

    tileSize = MathHelper.Max(maxTilesize, tileSize);
    tileSize = MathHelper.Min(minTilesize, tileSize);

    _blockLayer.ScaleToTilesize(tileSize, ClientSize);

    PanToFollowPlayer(ClientSize, playerPosition);
    LimitPanningToEdges(ClientSize);
  }

  private void PanToFollowPlayer(Vector2i ClientSize, Vector2 playerPosition)
  {
    var idealMapPosition = playerPosition;
    idealMapPosition.X += -.5f * ClientSize.X / tileSize + .5f;
    idealMapPosition.Y += -.5f * ClientSize.Y / tileSize + .5f;

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
  }

  private void LimitPanningToEdges(Vector2i ClientSize)
  {
    float maxX = _blockLayer.Tiles.Size.X - ClientSize.X / tileSize;
    float maxY = _blockLayer.Tiles.Size.Y - ClientSize.Y / tileSize;
    if (maxX < 0f) maxX = 0f; // -.5f * maxX;
    if (maxY < 0f) maxY = 0f; // -.5f * maxY;
    _blockLayer.MapOffset.X = Clamp(_blockLayer.MapOffset.X, 0f, maxX);
    _blockLayer.MapOffset.Y = Clamp(_blockLayer.MapOffset.Y, 0f, maxY);
  }

  private static float Clamp(float x, float min, float max) => x < min ? min : x > max ? max : x;
}
