using ITalent.Games.Tiled;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace ITalent.Games.TopDownGame;

internal enum FlipType
{
  None, Horizontal, Vertical, Both, Rot90
}

/// <summary>
/// Moving sprite
/// </summary>
internal class Actor
{
  public Vector2i tilePosition; // position in relation to map
  public int frame; // which sprite to show
  public float moveSpeed = 1f;
  public Vector2 AnimatedPosition;
  public bool FlipHorizontal;
  public bool FlipVertical;
  public int Rot90;
  public FlipType FlipType;
  public bool MirrorVertical;
  public float scale = 1f;
  public bool IsRemoved; // delete the behaviour/controller

  private Vector2i prevPosition; // position in relation to map
  private float moveProgress; // 0 .. 1

  public Actor(int frame, Vector2i position)
  {
    this.frame = frame;
    prevPosition = tilePosition = position;
  }

  public int Flip
  {
    get
    {
      int flip = 0;
      if (FlipHorizontal) flip ^= 1; // H bit
      if (MirrorVertical ^ FlipVertical) flip ^= 2; // V bit
      if (Rot90 == 1 || Rot90 == 3) flip ^= 4; // rot90 bit
      if (Rot90 == 2 || Rot90 == 3) flip ^= 3; // flip V+H bits after rotation
      return flip;
    }
  }

  public bool AllowMove => moveProgress > 1f;

  public void Update()
  {
    moveProgress += moveSpeed * Time.DeltaF;
    if (moveProgress <= 1f)
    {
      AnimatedPosition.X = TiledMathF.Lerp(prevPosition.X, tilePosition.X, moveProgress);
      AnimatedPosition.Y = TiledMathF.Lerp(prevPosition.Y, tilePosition.Y, moveProgress);
    }
  }

  public void NoMove()
  {
    AnimatedPosition = tilePosition;
    prevPosition = tilePosition;
    moveProgress = 0f;
  }

  public void Move(Vector2i p)
  {
    MoveTo(p);
  }

  /// <summary>
  /// Animate to current (tilePosition) position
  /// as if coming from p, without actually
  /// every occupying p
  /// </summary>
  public void MoveFrom(Vector2i p)
  {
    prevPosition = p;
    moveProgress = 0f;
    AnimatedPosition = p;
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

  private void MoveTo(Vector2i newPosition)
  {
    Debug.Assert(newPosition != tilePosition);

    AnimatedPosition = tilePosition;
    prevPosition = tilePosition;
    tilePosition = newPosition;
    moveProgress = 0;
  }
}