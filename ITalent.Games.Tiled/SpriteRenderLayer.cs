using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace ITalent.Games.Tiled;

/// <summary>
/// Work in progress!!
/// </summary>
public class SpriteRenderLayer(SpriteRenderLayerSettings Settings) : IRenderLayer
{
  // TODO:
  //public class Sprite
  //{
  //  public Vector2 Position;
  //  public int Index; // which frame from the sheet
  //  public bool FlipHorizontal;
  //  public bool FlipVertical;
  //  public int Rotate90;
  //  public float scale = 1f;
  //}

  private readonly float[] _vertices =
  {
      // Position         Texture coordinates
       1.0f,  1.0f, 1.0f, 1f, // top right
       1.0f, -1.0f, 1.0f, 0f, // bottom right
      -1.0f, -1.0f, 0.0f, 0f, // bottom left
      -1.0f,  1.0f, 0.0f, 1f  // top left
  };

  private readonly uint[] _indices =
  {
      0, 1, 3,
      1, 2, 3
  };

  private int _elementBufferObject; // the sprite quad indices
  private int _vertexBufferObject;  // the sprite quad vertices
  private int _vertexArrayObject;   // unclear wtf this does but its needed
  private int _positionBufferObject; // the sprite position, per instance
  private int _spriteIndexBufferObject; // the sprite index, per instance
  private int _spriteScaleBufferObject;
  private int _spriteFlipBufferObject; // the sprite flip/rot90, per instance -- note: only 3 bits are used
  private readonly Shader _shader = new();
  private Texture _spritesTexture;
  // NOTE: this could be a Half2
  protected Vector2[] _positions = [];
  // NOTE: sprite flips and indices can easily be combined into one 32-bit or even 16-bit int
  private int[] _spriteIndices = [];
  private int[] _spriteFlips = [];
  private float[] _spriteScales = [];

  public Vector2[] Positions => _positions;
  public int[] SpriteIndices => _spriteIndices;
  public int[] Flips => _spriteFlips;
  public float[] Scales => _spriteScales;

  private int _spriteCount = 0;

  public int SpriteCount
  {
    get => _spriteCount;
    set
    {
      //if (value > _spriteCount)
      {
        Array.Resize(ref _positions, value);
        Array.Resize(ref _spriteIndices, value);
        Array.Resize(ref _spriteFlips, value);
        Array.Resize(ref _spriteScales, value);
      }
      _spriteCount = value;
    }
  }

  public int SpriteBrushCount => Settings.TilesetSize.X * Settings.TilesetSize.Y;

  // TODO
  public Vector2 MapOffset = Vector2.Zero;
  public Vector2 MapScale = Vector2.One;

  public void Load()
  {
    // https://learnopengl.com/In-Practice/2D-Game/Rendering-Sprites
    // https://learnopengl.com/In-Practice/2D-Game/Particles
    // https://learnopengl.com/Advanced-OpenGL/Instancing
    // https://wikis.khronos.org/opengl/Buffer_Object#Buffer_Object_Usage

    // TODO: remove these defaults

    //for (int i = 0; i < _positions.Length; ++i)
    //{
    //  _positions[i] = new(
    //      2.2f * (i % 10),
    //      2.2f * (i / 10)
    //  );
    //  _spriteIndices[i] = i % 4;
    //}

    _vertexArrayObject = GL.GenVertexArray();
    GL.BindVertexArray(_vertexArrayObject);

    _vertexBufferObject = GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
    GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

    _elementBufferObject = GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
    GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

    // The shaders have been modified to include the texture coordinates, check them out after finishing the OnLoad function.
    _shader.Load("Shaders/sprites.vert", "Shaders/sprites.frag");
    _shader.Use();

    // Because there's now 5 floats between the start of the first vertex and the start of the second,
    // we modify the stride from 3 * sizeof(float) to 5 * sizeof(float).
    // This will now pass the new vertex array to the buffer.
    var vertexLocation = _shader.GetAttribLocation("aPos");
    GL.EnableVertexAttribArray(vertexLocation);
    GL.VertexAttribPointer(vertexLocation, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

    // Next, we also setup texture coordinates. It works in much the same way.
    // We add an offset of 3, since the texture coordinates comes after the position data.
    // We also change the amount of data to 2 because there's only 2 floats for texture coordinates.
    var texCoordLocation = _shader.GetAttribLocation("aTexCoord");
    GL.EnableVertexAttribArray(texCoordLocation);
    GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));


    // position array 'aOffset'
    _positionBufferObject = InitializeInstancedParameter(2, _positions, 2 * sizeof(float), 2, VertexAttribPointerType.Float);

    // sprite index array 'aIndex'
    _spriteIndexBufferObject = InitializeInstancedParameter(3, _spriteIndices, sizeof(int), 1, VertexAttribPointerType.Int);

    // sprite index array 'aScale'
    _spriteScaleBufferObject = InitializeInstancedParameter(4, _spriteScales, sizeof(float), 1, VertexAttribPointerType.Float);
    
    // sprite index array 'aFlip'
    //    3 bits:
    //      0 = normal
    //      1 = flip horizontal
    //      2 = flip vertical
    //      4 = rotate 90 CW
    //      3 = rotate 180 CW
    //      7 = rotate 270 CW
    _spriteFlipBufferObject = InitializeInstancedParameter(5, _spriteFlips, sizeof(int), 1, VertexAttribPointerType.Int);

    _shader.SetInt2("sheetSize", Settings.TilesetSize);

    _spritesTexture = Texture.LoadFromFile(Settings.TilesetPath, Settings.SmoothTextures);
    _spritesTexture.Use(TextureUnit.Texture0);

    _shader.SetInt("texture0", 0);
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="VAALocation">this should match the layout (location = ?) in the vertex shader</param>
  /// <returns></returns>
  private int InitializeInstancedParameter<T>(int VAALocation, T[] data, int sizeoftype, int vsize, VertexAttribPointerType vtype) where T : struct
  {
    int bufferObject;
    // We're first going to store the _positions array (from the previous section) in a new buffer object: 
    GL.GenBuffers(1, out bufferObject); // generate 1 buffer
    GL.BindBuffer(BufferTarget.ArrayBuffer, bufferObject); // select it
    GL.BufferData(BufferTarget.ArrayBuffer, sizeoftype * data.Length, data, BufferUsageHint.StreamDraw); //  upload the positions, TODO: use best BufferUsageHint
    GL.BindBuffer(BufferTarget.ArrayBuffer, 0); // unselect it

    // Then we also need to set its vertex attribute pointer and enable the vertex attribute: 
    GL.EnableVertexAttribArray(VAALocation);
    GL.BindBuffer(BufferTarget.ArrayBuffer, bufferObject); // select vertex buffer
    GL.VertexAttribPointer(VAALocation, vsize, vtype, false, sizeoftype, 0); // let the vertex aray attribute point to the selected buffer (i guess)
    GL.BindBuffer(BufferTarget.ArrayBuffer, 0); // unselect vertex buffer
    GL.VertexAttribDivisor(VAALocation, 1); // through some twisted logic, this tells the GPU to treat it as instanced
                                            // What makes this code interesting is the last line where we call glVertexAttribDivisor.
    return bufferObject;
  }

  /// <summary>
  /// Set MapScale, so that a 1x1 sprite is tileSize x tileSize on screen
  /// </summary>
  /// <param name="tileSize">Size of a 1x1 unscaled on screen. In pixels.</param>
  /// <param name="clientSize">Current size of the window client area. In pixels.</param>
  public void ScaleToTilesize(float tileSize, Vector2i clientSize)
  {
    MapScale.X = tileSize / clientSize.X;
    MapScale.Y = tileSize / clientSize.Y;
  }

  public void Align(Vector2i clientSize)
  {
    //throw new NotImplementedException();
  }

  public void UpdatePositions()
  {
    GL.BindBuffer(BufferTarget.ArrayBuffer, _positionBufferObject); // select it
    GL.BufferData(BufferTarget.ArrayBuffer, 2 * sizeof(float) * _positions.Length, _positions, BufferUsageHint.StreamDraw); //  upload the positions
    //GL.BufferSubData(BufferTarget.ArrayBuffer, 0, 2 * sizeof(float) * _positions.Length, _positions); // is this faster/better?
    GL.BindBuffer(BufferTarget.ArrayBuffer, 0); // unselect it
  }

  public void UpdateSpriteIndices()
  {
    GL.BindBuffer(BufferTarget.ArrayBuffer, _spriteIndexBufferObject); // select it
    GL.BufferData(BufferTarget.ArrayBuffer, sizeof(int) * _spriteIndices.Length, _spriteIndices, BufferUsageHint.StreamDraw); //  upload the indices
    //GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(int) * _spriteIndices.Length, _spriteIndices); // is this faster/better?
    GL.BindBuffer(BufferTarget.ArrayBuffer, 0); // unselect it
  }

  public void UpdateSpriteFlips()
  {
    GL.BindBuffer(BufferTarget.ArrayBuffer, _spriteFlipBufferObject); // select it
    GL.BufferData(BufferTarget.ArrayBuffer, sizeof(int) * _spriteFlips.Length, _spriteFlips, BufferUsageHint.StreamDraw); //  upload the indices
    //GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(int) * _spriteFlips.Length, _spriteFlips); // is this faster/better?
    GL.BindBuffer(BufferTarget.ArrayBuffer, 0); // unselect it
  }

  public void UpdateSpriteScales()
  {
    GL.BindBuffer(BufferTarget.ArrayBuffer, _spriteScaleBufferObject); // select it
    GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * _spriteScales.Length, _spriteScales, BufferUsageHint.StreamDraw); //  upload the indices
    //GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * _spriteScales.Length, _spriteScales); // is this faster/better?
    GL.BindBuffer(BufferTarget.ArrayBuffer, 0); // unselect it
  }

  public void RenderFrame(FrameEventArgs e)
  {
    GL.BindVertexArray(_vertexArrayObject);

    _spritesTexture.Use(TextureUnit.Texture0);

    _shader.Use();
    _shader.SetFloat2("mapOffset", MapOffset);
    _shader.SetFloat2("mapScale", MapScale);
    //_shader.SetFloat4("color", Color);

    //GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
    GL.DrawElementsInstanced(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0, _positions.Length);
  }
}
