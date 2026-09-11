using OpenTK.Mathematics;

namespace ITalent.Games.Tiled;

public enum FlipType
{
  None, Horizontal, Vertical, Both, Rot90
}

/// <summary>
/// Moving sprite
/// </summary>
public class TiledSprite
{
  public Vector2i TilePosition; // position in relation to map
  public int Frame; // which sprite to show
  public Vector2 AnimatedPosition;

  public float moveSpeed = 1f;
  public bool FlipHorizontal;
  public bool FlipVertical;
  public int Rot90;
  public FlipType FlipType;
  public bool MirrorVertical;
  public int Rot90Offset;
  public float scale = 1f;
  public bool IsRemoved; // delete the behaviour/controller

  public Vector2i PerceptedPosition => moveProgress < .5f ? prevPosition : TilePosition;

  private Vector2i prevPosition; // position in relation to map
  private float moveProgress; // 0 .. 1

  public TiledSprite(int frame, Vector2i position)
  {
    Frame = frame;
    prevPosition = TilePosition = position;
  }

  public int Flip
  {
    get
    {
      int flip = 0;
      if (FlipHorizontal) flip ^= 1; // H bit
      if (MirrorVertical ^ FlipVertical) flip ^= 2; // V bit
      var rot90 = (Rot90Offset + Rot90) % 4;
      if (rot90 == 1 || rot90 == 3) flip ^= 4; // rot90 bit
      if (rot90 == 2 || rot90 == 3) flip ^= 3; // flip V+H bits after rotation
      return flip;
    }
  }

  public bool AllowMove => moveProgress > 1f;

  public void Update()
  {
    moveProgress += moveSpeed * Time.DeltaF;
    if (moveProgress <= 1f)
    {
      AnimatedPosition.X = TiledMathF.Lerp(prevPosition.X, TilePosition.X, moveProgress);
      AnimatedPosition.Y = TiledMathF.Lerp(prevPosition.Y, TilePosition.Y, moveProgress);
    }
    //else
    //{
    //  AnimatedPosition = TilePosition;
    //  prevPosition = TilePosition;
    //}
  }

  public void Move(Vector2i p)
  {
    AnimatedPosition = TilePosition;
    prevPosition = TilePosition;
    TilePosition = p;
    moveProgress = 0f;
  }

  public void FaceDirection(Vector2i delta)
  {
    if (FlipType == FlipType.Horizontal || FlipType == FlipType.Both)
    {
      FlipHorizontal = delta.X < 0;
    }
    if (FlipType == FlipType.Rot90 && !MirrorVertical)
    {
      FlipHorizontal = !FlipHorizontal;
    }
    if (FlipType == FlipType.Vertical || FlipType == FlipType.Both)
    {
      FlipVertical = delta.Y > 0; // spiders are allready flipped, so we are flipping it again
    }
    if (FlipType == FlipType.Rot90)
    {
      Rot90 = delta.X > 0 ? 1 : delta.Y < 0 ? 2 : delta.X < 0 ? 3 : 0;
    }
  }
}