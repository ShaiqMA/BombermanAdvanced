using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace ITalent.Games.Tiled;

public class TiledSpriteRenderLayer : IRenderLayer
{
  private readonly SpriteRenderLayer _spriteLayer;

  public TiledSpriteRenderLayer(TiledSpriteRenderLayerSettings settings)
  {
    _spriteLayer = new(new()
    {
      TilesetPath = settings.TilesetPath,
      TilesetSize = settings.TilesetSize
    });
  }

  public List<TiledSprite> Sprites = [];

  private void UpdateInstanceArrays()
  {
    _spriteLayer.SpriteCount = Sprites.Count;
    for (int i = 0; i < Sprites.Count; ++i)
    {
      _spriteLayer.Positions[i] = Sprites[i].AnimatedPosition;
      _spriteLayer.Flips[i] = Sprites[i].Flip;
      _spriteLayer.Scales[i] = Sprites[i].scale;
      _spriteLayer.SpriteIndices[i] = Sprites[i].Frame;
    }
  }

  public void Update(float tileSize, Vector2i clientSize, Vector2 mapOffset)
  {
    for (int i = 0; i < Sprites.Count; ++i)
    {
      Sprites[i].Update();
    }
    UpdateInstanceArrays();
    _spriteLayer.UpdatePositions();
    _spriteLayer.UpdateSpriteFlips();
    _spriteLayer.UpdateSpriteScales();
    _spriteLayer.UpdateSpriteIndices();
    UpdateMapScaleOffset(tileSize, clientSize, mapOffset);
  }

  private void UpdateMapScaleOffset(float tileSize, Vector2i clientSize, Vector2 mapOffset)
  {
    var MapOffset = Vector2.Zero;
    var MapScale = Vector2.One;
    MapScale.X = tileSize / clientSize.X;
    MapScale.Y = tileSize / clientSize.Y;
    MapOffset.X = MapScale.X * (1f - 2f * mapOffset.X) - 1f;
    MapOffset.Y = MapScale.Y * (1f - 2f * mapOffset.Y) - 1f;
    _spriteLayer.MapOffset = MapOffset;
    _spriteLayer.MapScale = MapScale;
  }

  public void Align(Vector2i clientSize)
  {
    _spriteLayer.Align(clientSize);
  }

  public void Load()
  {
    _spriteLayer.Load();
  }

  public void RenderFrame(FrameEventArgs e)
  {
    _spriteLayer.RenderFrame(e);
  }

  public void ScaleToTilesize(float tileSize, Vector2i clientSize)
  {
    _spriteLayer.ScaleToTilesize(tileSize, clientSize);
  }
}
