using ITalent.Games.Tiled;
using OpenTK.Mathematics;

namespace ITalent.Games.TopDownGame;

internal class EffectsBehaviour
{
  private ByteGrid map;
  private SpriteRenderLayer layer;

  public void Initialize(ByteGrid map, SpriteRenderLayer layer)
  {
    this.map = map;
    this.layer = layer;

    //layer.UpdateInstanceArrays();
  }

  public void LaunchProjectile(Vector2i position, Vector2i delta, float speed)
  {

  }

  public void Update()
  {
  }
}
