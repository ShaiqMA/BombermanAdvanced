using ITalent.Games.Tiled;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace ITalent.Games.TopDownGame.Actors;

internal class Spider(Actor actor, ActorCollisionLayer layer) : IActorController
{
  private const float HEALTHDECAY = 1f / 20f;
  private const float MAXHEALTH = 4f;

  public float Health = 2f;
  public Actor Actor => actor;

  public static Spider Create(ActorCollisionLayer layer, Vector2i pos)
  {
    return new(new(SpriteID.SPIDER, pos) { moveSpeed = 3f, FlipType = FlipType.Rot90, MirrorVertical = true }, layer)
    {
      Health = 1.5f + Random.Shared.NextSingle()
    };
  }

  public void Update(ByteGrid map)
  {
    Debug.Assert(layer.ActorAt(actor.tilePosition) == actor);

    Health -= HEALTHDECAY * Time.DeltaF;
    if (Health < 0f)
    {
      // die
      actor.IsRemoved = true;
      //layer.RemoveActor(Actor);
      return;
    }

    if (!actor.AllowMove) return;

    var delta = ActorBehaviour.RandomMove();
    var p = actor.tilePosition + delta;
    if (map.IsInBounds(p) && !TileID.IsWall(map[p.X, p.Y]))
    {
      var other = layer.ActorAt(p);
      if (other != null && other.frame == SpriteID.PREY && Health < MAXHEALTH)
      {
        Debug.WriteLine($"{actor} @ {actor.tilePosition} kills prey @ {p}");
        Debug.Assert(layer.ActorAt(other.tilePosition) == other);

        // kill
        layer.RemoveActor(other);
        other.IsRemoved = true;
        //Debug.Assert(layer.ActorAt(other.tilePosition) == null);
        other = null;
        // eat
        Health += 1f;
      }
      if (other == null)
      {
        actor.FaceDirection(delta);
        Debug.Assert(layer.ActorAt(actor.tilePosition) == actor);
        Debug.Assert(layer.ActorAt(p) == null);
        layer.Move(actor.tilePosition, p);
        actor.Move(p);
      }
    }
    actor.scale = .8f + .1f * Health;
  }
}
