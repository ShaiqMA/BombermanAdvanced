using ITalent.Games.Tiled;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace ITalent.Games.TopDownGame.Actors;

internal class Rat(Actor actor, ActorCollisionLayer layer) : IActorController
{
  public int Health = 1;
  public Actor Actor => actor;

  public static Rat Create(ActorCollisionLayer layer, Vector2i pos)
  {
    return new(new(SpriteID.PREY, pos) { moveSpeed = 5f, FlipType = FlipType.Horizontal }, layer);
  }

  public void Update(ByteGrid map)
  {
    Debug.Assert(layer.ActorAt(actor.tilePosition) == actor);

    if (!actor.AllowMove) return;

    // try move away from spiders
    var moveBias = new Vector2i();
    foreach (var uv in ActorBehaviour.Offsets(2))
    {
      var q = actor.tilePosition + uv;
      var neighbour = layer.ActorAt(q);
      if (neighbour != null && neighbour.frame == SpriteID.SPIDER)
      {
        moveBias -= uv;
      }
    }
    Vector2i delta = new();
    if (moveBias.X == 0 && moveBias.Y == 0)
    {
      delta = ActorBehaviour.RandomMove();
    }
    else if (MathHelper.Abs(moveBias.X) > MathHelper.Abs(moveBias.Y))
    {
      delta.X = moveBias.X < 0 ? -1 : 1;
    }
    else
    {
      delta.Y = moveBias.Y < 0 ? -1 : 1;
    }
    var p = actor.tilePosition + delta;
    if (map.IsInBounds(p) && !TileID.IsWall(map[p.X, p.Y]) && layer.Empty(p))
    {
      actor.FaceDirection(delta);
      Debug.Assert(layer.ActorAt(actor.tilePosition) == actor);
      Debug.Assert(layer.ActorAt(p) == null);
      layer.Move(actor.tilePosition, p);
      actor.Move(p);
    }
  }
}
