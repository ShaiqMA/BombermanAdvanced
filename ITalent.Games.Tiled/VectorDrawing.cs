using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace ITalent.Games.Tiled;

public class VectorDrawing
{
  private readonly List<IElement> _elements = [];

  public bool IsEmpty => _elements.Count == 0;
  public bool IsChanged { get; set; }

  public void Line(params Vector2[] points)
  {
    _elements.Add(new _Line(points));
    IsChanged = true;
  }

  public void Circle(Vector2 center, float radius)
  {
    _elements.Add(new _Circle(center, radius));
    IsChanged = true;
  }

  public void Rect(Vector2 center, Vector2 size)
  {
    _elements.Add(new _Rect(center, size));
    IsChanged = true;
  }

  public void RectBL(Vector2 bl, Vector2 size)
  {
    _elements.Add(_Rect.BottomLeft(bl, size));
    IsChanged = true;
  }

  public void Clear()
  {
    _elements.Clear();
    IsChanged = true;
  }

  public (float[] vertices, uint[] indices) ToMesh()
  {
    List<float> vertexList = [];
    List<uint> indexList = [];
    foreach (var el in _elements)
    {
      el.Output(vertexList, indexList);
    }
    return (vertexList.ToArray(), indexList.ToArray());
  }

  interface IElement
  {
    void Output(List<float> vertexList, List<uint> indexList);
  }

  class _Line(params Vector2[] points) : IElement
  {
    public void Output(List<float> vertexList, List<uint> indexList)
    {
      bool first = true;
      foreach (var p in points)
      {
        if (!first)
        {
          // connect to previous vertex
          uint index = (uint)(vertexList.Count / 2);
          indexList.Add(index - 1);
          indexList.Add(index);
        }
        first = false;

        vertexList.Add(p.X);
        vertexList.Add(p.Y);
      }
    }
  }

  class _Circle(Vector2 center, float radius) : IElement
  {
    public void Output(List<float> vertexList, List<uint> indexList)
    {
      uint count = 32;
      uint firstIndex = (uint)(vertexList.Count / 2);
      for (uint i = 0; i < count; ++i)
      {
        if (i == 0)
        {
          // connect first to last
          indexList.Add(firstIndex + i);
          indexList.Add(firstIndex + count - 1);
        }
        else
        {
          // connect others to previous
          indexList.Add(firstIndex + i);
          indexList.Add(firstIndex + i - 1);
        }
        float angle = MathF.Tau * (i / (float)(count - 1));
        vertexList.Add(center.X + radius * MathF.Cos(angle));
        vertexList.Add(center.Y + radius * MathF.Sin(angle));
      }
    }
  }

  class _Rect(Vector2 center, Vector2 size) : IElement
  {
    public static _Rect BottomLeft(Vector2 bl, Vector2 size) => new(bl + .5f * size, size);

    public void Output(List<float> vertexList, List<uint> indexList)
    {
      uint firstIndex = (uint)(vertexList.Count / 2);
      indexList.Add(firstIndex + 0);
      indexList.Add(firstIndex + 1);
      indexList.Add(firstIndex + 1);
      indexList.Add(firstIndex + 2);
      indexList.Add(firstIndex + 2);
      indexList.Add(firstIndex + 3);
      indexList.Add(firstIndex + 3);
      indexList.Add(firstIndex + 0);

      var half = 0.5f * size;
      vertexList.Add(center.X - half.X);
      vertexList.Add(center.Y - half.Y);
      vertexList.Add(center.X + half.X);
      vertexList.Add(center.Y - half.Y);
      vertexList.Add(center.X + half.X);
      vertexList.Add(center.Y + half.Y);
      vertexList.Add(center.X - half.X);
      vertexList.Add(center.Y + half.Y);
    }
  }
}
