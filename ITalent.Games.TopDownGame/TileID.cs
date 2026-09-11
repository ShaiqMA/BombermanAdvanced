namespace ITalent.Games.TopDownGame;

internal class TileID
{
  internal const byte CASTLE = 10 * 12;
  internal const byte WALL = 7 * 12 + 4;

  public static bool IsWall(byte tileId)
  {
    switch (tileId)
    {
      //case CASTLE:
      case 6 * 12 + 9:
      case 6 * 12 + 11:
      case 10 * 12 + 4:
      case 10 * 12 + 5:
      case TileID.WALL:
      case 8 * 12 + 2:
      case 9 * 12 + 3:
      case 9 * 12 + 1:
      case 4 + 12 * 8: // bars
      case 5 + 12 * 8: // flag
        return true;
    }
    return false;
  }
}
