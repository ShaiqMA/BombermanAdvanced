namespace ITalent.Games.Tiled;

public static class TiledMathF
{
  public static float Lerp(float x1, float x2, float t)
  {
    return t * x2 + (1f - t) * x1;
  }

  public static int NextMultipleOf4(int x)
  {
    int r = x / 4 * 4;
    if (r < x)
    {
      r += 4;
    }
    return r;
  }
}
