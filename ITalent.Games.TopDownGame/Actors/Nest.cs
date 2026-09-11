using ITalent.Games.Tiled;
using OpenTK.Mathematics;

namespace ITalent.Games.TopDownGame.Actors;

internal class Nest(Actor actor, ActorCollisionLayer layer, INest_ActorBehaviour parent) : IActorController
{
  public Actor Actor => actor;
  private float growProgress; // 0 .. 1

  public static Nest Create(ActorCollisionLayer layer, INest_ActorBehaviour parent, Vector2i pos)
  {
    return new(new(SpriteID.NEST, pos) { FlipType = FlipType.None }, layer, parent);
  }

  public void Update(ByteGrid map)
  {
    growProgress += Time.DeltaF;
    Actor.scale = .8f + .5f * growProgress;
    if (growProgress > 1f)
    {
      // spawn spider
      growProgress -= 1f;
      var options = ActorBehaviour.Offsets(1)
        .Select(uv => actor.tilePosition + uv)
        .Where(p => map.IsInBounds(p) && !TileID.IsWall(map[p.X, p.Y]) && layer.Empty(p)).ToList();
      if (options.Count > 0)
      {
        var position = options[Random.Shared.Next(options.Count)];
        parent.HatchSpider(position);
      }
    }
  }
}
