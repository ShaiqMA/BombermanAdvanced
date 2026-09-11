using ITalent.Games.Tiled;
using OpenTK.Mathematics;

namespace ITalent.Games.TopDownGame.Actors;

internal class Effect(Actor actor) : IActorController
{
  public Actor Actor => actor;
  private float growProgress; // 0 .. 1

  public static Effect Create(int spriteID, Vector2i pos, float duration)
  {
    return new(new(spriteID, pos) { FlipType = FlipType.None });
  }

  public void Update()
  {
    if (actor.frame == SpriteID.BLOOD)
    {
      if (growProgress > 3f) actor.IsRemoved = true;
    }
    else
    {
      actor.scale = 2f * (1f - growProgress);
      if (growProgress > 1f) actor.IsRemoved = true;
    }
    growProgress += Time.DeltaF;
  }
}
