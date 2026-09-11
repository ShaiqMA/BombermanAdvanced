using OpenTK.Mathematics;

namespace ITalent.Games.Tiled;

public class TextWriter(ByteGrid tiles)
{
  public Vector2i CursorPos = new();

  public void Clear()
  {
    tiles.Fill(AsciiToIndex(32)); // space
  }

  private byte AsciiToIndex(int ascii)
  {
    // ascii bits: yyyxxxxx, this works because tilemap is 32x8
    //return (byte)((ascii % 32) + (7 - ascii / 32) * 32); // flip y
    return (byte)((ascii & 31) | (7 - (ascii >> 5)) << 5); // same code using bitwise operations
  }

  public void Write(string s)
  {
    for (int i = 0; i < s.Length; i++)
    {
      int ascii = s[i];
      // TODO: handle 10(linefeed), 13(carriage return) and tab
      tiles.SetTile(CursorPos.X, tiles.Size.Y - 1 - CursorPos.Y, AsciiToIndex(ascii));
      CursorPos.X++;
      if (CursorPos.X >= tiles.Size.X)
      {
        CursorPos.X = CursorPos.X % tiles.Size.X;
        CursorPos.Y++;
      }
      if (CursorPos.Y >= tiles.Size.Y)
      {
        // scroll up???
        //CursorPos.Y = Layer.Size.Y - 1; // stop
        CursorPos.Y = 0; // wrap
      }
    }
  }
}