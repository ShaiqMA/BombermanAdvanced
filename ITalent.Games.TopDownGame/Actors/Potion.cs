using ITalent.Games.Tiled;
using OpenTK.Mathematics;

namespace ITalent.Games.TopDownGame.Actors;

internal class Potion(Actor actor) : IActorController
{
  public Actor Actor => actor;

  public static Potion Create(Vector2i pos)
  {
    return new(new(SpriteID.POTION, pos) { FlipType = FlipType.None });
  }

  public void Update()
  {
    Actor.scale = (float)(1.0 + .2 * Math.Cos(5.0 * Time.Total));
  }
}
