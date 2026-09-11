using ITalent.Games.Tiled;
using OpenTK.Mathematics;

namespace ITalent.Games.TopDownGame;

/// <summary>
/// ActorLayer interface towards Actor
/// </summary>
internal interface IActor_ActorRenderLayer
{
  void RegisterActor(Actor actor);
  void RemoveActor(Actor actor);
}

internal class ActorRenderLayer() : SpriteRenderLayer(Settings), IActor_ActorRenderLayer
{
  private List<Actor> actors = new();

  public void RegisterActor(Actor actor)
  {
    //Debug.Assert(!actorDict.ContainsKey(actor.tilePosition));

    // TODO: render layers
    if (actor.frame == SpriteID.BLOOD) actors.Insert(0, actor);
    else actors.Add(actor);
  }

  public void RemoveActor(Actor actor)
  {
    actors.Remove(actor);
  }

  public void UpdateInstanceArrays()
  {
    SpriteCount = actors.Count;
    for (int i = 0; i < actors.Count; ++i)
    {
      Positions[i] = actors[i].AnimatedPosition;
      Flips[i] = actors[i].Flip;
      Scales[i] = actors[i].scale;
      SpriteIndices[i] = actors[i].frame;
    }
  }

  public void Update(float tileSize,
    Vector2i clientSize, Vector2 mapOffset)
  {
    for (int i = 0; i < actors.Count; ++i)
    {
      actors[i].Update();
    }
    UpdateInstanceArrays();
    UpdatePositions();
    UpdateSpriteFlips();
    UpdateSpriteScales();
    UpdateSpriteIndices();
    UpdateMapScaleOffset(tileSize, clientSize, mapOffset);
  }

  private void UpdateMapScaleOffset(float tileSize, Vector2i clientSize, Vector2 mapOffset)
  {
    // uitgangspunts: sprite gaat van (-1, -1) tot (1,1)
    // en neemt daarmee het hele scherm in beslag
    MapOffset = Vector2.Zero;
    MapScale = Vector2.One;

    // zorg dat de sprite origin linksonder in beeld is

    MapOffset.X = -1f;
    MapOffset.Y = -1f;

    // scale de sprite zodat de scaling matched met de map scale

    MapScale.X = tileSize / clientSize.X;
    MapScale.Y = tileSize / clientSize.Y;

    // offset om de sprite te corrigeren van center naar linksonder positie

    MapOffset.X += MapScale.X;
    MapOffset.Y += MapScale.Y;

    // offset om de sprite op de map je plaatsen. x2 want 1 scherm is 2 units in sprite space

    MapOffset.X -= 2f * MapScale.X * mapOffset.X;
    MapOffset.Y -= 2f * MapScale.Y * mapOffset.Y;

    // verkortte versie:

    MapOffset.X = MapScale.X * (1f - 2f * mapOffset.X) - 1f;
    MapOffset.Y = MapScale.Y * (1f - 2f * mapOffset.Y) - 1f;
  }

  static SpriteRenderLayerSettings Settings => new()
  {
    TilesetPath = "Resources/tilemap_packed.png",
    TilesetSize = new(12, 12)
  };
}
