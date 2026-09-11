using OpenTK.Compute.OpenCL;
using OpenTK.Mathematics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace ITalent.Games.Tiled;

public class ByteGrid : IEnumerable<KeyValuePair<Vector2i, byte>>
{
  private byte[] _bytes;
  private int _stride;
  private Vector2i _size = new();
  private bool _isChanged;

  public byte[] Bytes => _bytes;
  //public int Width => _stride;
  //public int Height => _height;
  public bool IsChanged => _isChanged;
  public Vector2i Size => _size;

  public ByteGrid(int w, int h)
  {
    _stride = TiledMathF.NextMultipleOf4(w);
    _bytes = new byte[_stride * h];
    _size.X = w;
    _size.Y = h;
  }

  public void Resize(int w, int h)
  {
    int stride = TiledMathF.NextMultipleOf4(w);
    if( stride == _stride && h == _size.Y )
    {
      _size.X = w;
      return;
    }
    _stride = stride;
    _size.X = w;
    _size.Y = h;
    _bytes = new byte[_stride * h];
    _isChanged = true;
  }

  public void Fill(byte b)
  {
    Array.Fill(_bytes, b);
    _isChanged = true;
  }

  public void Fill(int x, int y, int w, int h, byte b)
  {
    for (int row = 0; row < h; ++row)
    {
      Array.Fill(_bytes, b, x + _stride * (row + y), w);
      //for (int col = 0; col < w; ++col)
      //{
      //  _bytes[col + x + _stride * (row + y)] = b;
      //}
    }
    _isChanged = true;
  }

  public void Copy(int x, int y, int w, int h, int destX, int destY)
  {
    if (destY >= y) throw new NotImplementedException("Can only copy areas downwards");
    for (int row = 0; row < h; ++row)
    {
      // TODO: optimize using Array.Copy()
      for (int col = 0; col < w; ++col)
      {
        _bytes[col + destX + _stride * (row + destY)] = _bytes[col + x + _stride * (row + y)];
      }
    }
    _isChanged = true;
  }

  public int GetPixelIndex(int x, int y) => x + _stride * y;

  public byte this[int x, int y]
  {
    get => _bytes[x + _stride * y];
    set { _bytes[x + _stride * y] = value; _isChanged = true; }
  }

  public byte GetTile(Vector2i p) => GetTile(p.X, p.Y);

  public byte GetTile(int x, int y) => this[x, y];

  public void SetTile(Vector2i p, byte b) => SetTile(p.X, p.Y, b);

  public void SetTile(int x, int y, int b) => SetTile(x, y, (byte)b);

  public void SetTile(int x, int y, byte b)
  {
    this[x, y] = b;
  }

  public bool IsInBounds(Vector2i pos)
  {
    return pos.X >= 0 && pos.Y >= 0 && pos.X < _size.X && pos.Y < _size.Y;
  }

  public bool IsInBounds(int x, int y)
  {
    return x >= 0 && y >= 0 && x < _size.X && y < _size.Y;
  }

  public IEnumerator<KeyValuePair<Vector2i, byte>> GetEnumerator()
  {
    for (int row = 0; row < _size.Y; ++row)
    {
      for (int col = 0; col < _size.X; ++col)
      {
        yield return new(new(col, row), _bytes[col + _stride * row]);
      }
    }
  }

  public void RandomMap()
  {
    Random.Shared.NextBytes(_bytes);
    _isChanged = true;
  }

  public void ClearChanged() { _isChanged = false; }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }

    public void SetTile(int playerX, int playerY, object pLAYER)
    {
        throw new NotImplementedException();
    }
}
