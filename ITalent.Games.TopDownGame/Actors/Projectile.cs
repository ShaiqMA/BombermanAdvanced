using ITalent.Games.Tiled;
using OpenTK.Mathematics;

namespace ITalent.Games.TopDownGame.Actors;

internal class Projectile(Actor actor, ActorCollisionLayer layer) : IActorController
{
  public HitResult lastHitResult;
  public Vector2i delta { get; init; }
  public Actor Actor => actor;

  public static Projectile CreateFireball(ActorCollisionLayer layer, Vector2i pos, Vector2i delta)
  {
    Actor actor = new(SpriteID.FIREBALL, pos + delta) { moveSpeed = 6f, FlipType = FlipType.Rot90 };
    actor.MoveFrom(pos);
    actor.FaceDirection(delta);
    return new(actor, layer) { delta = delta };
  }

  public void Update(ByteGrid map)
  {
    if (!actor.AllowMove) return;

    UpdateCollision(map, actor.tilePosition);
    if (lastHitResult != HitResult.NoHit)
    {
      // die
      actor.IsRemoved = true;
      return;
    }

    // move
    var p = actor.tilePosition + delta;
    actor.Move(p);
  }

  public void UpdateCollision(ByteGrid map, Vector2i p)
  {
    lastHitResult = CheckCollision(layer, map, p);
  }

  /// <returns>true if something was hit and projectile died</returns>
  private static HitResult CheckCollision(ActorCollisionLayer layer, ByteGrid map, Vector2i p)
  {
    if (!map.IsInBounds(p)) return HitResult.Fizzle;

    // check collision at pos+delta
    if (TileID.IsWall(map[p.X, p.Y]))
    {
      // fizzle: todo: show some effect
      return HitResult.Hit;
    }
    var other = layer.ActorAt(p);
    if (other != null)
    {
      if (other.frame == SpriteID.WIZARD) return HitResult.Fizzle;

      // kill
      other.IsRemoved = true;
      layer.RemoveActor(other); // TODO: hitpoints
      return other.frame == SpriteID.POTION ? HitResult.Explode : HitResult.Hit;
    }
    return HitResult.NoHit;
  }

  public enum HitResult { NoHit, Fizzle, Hit, Explode }
}