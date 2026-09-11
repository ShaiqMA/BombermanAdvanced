using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace ITalent.Games.Tiled;

public class VectorRenderLayer(VectorRenderLayerSettings Settings) : IRenderLayer
{
  // Because we're adding a texture, we modify the vertex array to include texture coordinates.
  // Texture coordinates range from 0.0 to 1.0, with (0.0, 0.0) representing the bottom left, and (1.0, 1.0) representing the top right.
  // The new layout is three floats to create a vertex, then two floats to create the coordinates.
  private float[] _vertices = [];
  private uint[] _indices = [];

  private int _elementBufferObject;
  private int _vertexBufferObject;
  private int _vertexArrayObject;
  private readonly Shader _shader = new();
  private readonly VectorDrawing _drawing = new();

  public Vector2 MapOffset = Vector2.Zero;
  protected Vector2 MapScale = Vector2.One;

  public VectorDrawing Drawing => _drawing;

  // https://solarianprogrammer.com/2013/05/13/opengl-101-drawing-primitives/

  public void Align(Vector2i clientSize)
  {
  }

  public void Load()
  {
    _vertexArrayObject = GL.GenVertexArray();
    GL.BindVertexArray(_vertexArrayObject);

    _vertexBufferObject = GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
    GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

    _elementBufferObject = GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
    GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

    // The shaders have been modified to include the texture coordinates, check them out after finishing the OnLoad function.
    _shader.Load("Shaders/lines.vert", "Shaders/lines.frag");
    _shader.Use();

    // Because there's now 5 floats between the start of the first vertex and the start of the second,
    // we modify the stride from 3 * sizeof(float) to 5 * sizeof(float).
    // This will now pass the new vertex array to the buffer.
    var vertexLocation = _shader.GetAttribLocation("aPosition");
    GL.EnableVertexAttribArray(vertexLocation);
    GL.VertexAttribPointer(vertexLocation, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
  }

  public void RenderFrame(FrameEventArgs e)
  {
    if (_drawing.IsEmpty) return;

    GL.BindVertexArray(_vertexArrayObject);

    _shader.Use();
    //_shader.SetFloat4("color", Color);

    if(_drawing.IsChanged)
    {
      (_vertices, _indices) = _drawing.ToMesh();

      GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
      GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

      GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
      GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

      // without indices:
      // GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

      _drawing.IsChanged = false;
    }

    GL.DrawElements(PrimitiveType.Lines, _indices.Length, DrawElementsType.UnsignedInt, 0);

    // linestrip without indices:
    //GL.DrawArrays(PrimitiveType.LineStrip, 0, _vertices.Length / 2);
  }

  public void ScaleToTilesize(float tileSize, Vector2i clientSize)
  {
  }
}

public class VectorRenderLayerSettings
{

}