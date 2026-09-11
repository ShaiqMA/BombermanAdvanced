using ITalent.Games.Tiled;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;

namespace ITalent.Games.TopDownGame.Actors;

internal class Player(Actor actor, ActorCollisionLayer layer, IPlayer_ActorBehaviour parent) : IActorController
{
  public int Health = 1;
  public Actor Actor => actor;
  private Vector2i direction = new(1, 0);

  public static Player Create(ActorCollisionLayer layer, IPlayer_ActorBehaviour parent, Vector2i pos)
  {
    return new(new(SpriteID.WIZARD, pos) { moveSpeed = 4f, FlipType = FlipType.Horizontal }, layer, parent);
  }

  public void Update(ByteGrid map, KeyboardState keys)
  {
    if (!actor.AllowMove) return;

    var delta = Input.WASD(keys);
    if (delta != Vector2i.Zero) direction = delta;
    if (keys.IsKeyDown(Keys.Space))
    {
      parent.Shoot(actor.tilePosition, direction);
      actor.NoMove();
      return;
    }

    if (delta == Vector2i.Zero) return;

    var p = Actor.tilePosition + delta;
    actor.FaceDirection(delta);
    if (map.IsInBounds(p) && !TileID.IsWall(map[p.X, p.Y]))
    {
      var other = layer.ActorAt(p);
      if (other != null && other.frame == SpriteID.POTION)
      {
        // pickup potion
        other.IsRemoved = true;
        layer.RemoveActor(other);
        other = null;
      }
      if (other == null)
      {
        Debug.Assert(layer.ActorAt(actor.tilePosition) == actor);
        Debug.Assert(layer.ActorAt(p) == null);
        layer.Move(actor.tilePosition, p);
        actor.Move(p);
      }
    }
  }
}
