using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System;
using System.IO;

namespace ITalent.Games.Tiled;

public class MapRenderLayer(MapRenderLayerSettings Settings) : IRenderLayer
{
  // Because we're adding a texture, we modify the vertex array to include texture coordinates.
  // Texture coordinates range from 0.0 to 1.0, with (0.0, 0.0) representing the bottom left, and (1.0, 1.0) representing the top right.
  // The new layout is three floats to create a vertex, then two floats to create the coordinates.
  private readonly float[] _vertices =
  {
      // Position         Texture coordinates
       1.0f,  1.0f, 0.0f, 1.0f, 1f, // top right
       1.0f, -1.0f, 0.0f, 1.0f, 0f, // bottom right
      -1.0f, -1.0f, 0.0f, 0.0f, 0f, // bottom left
      -1.0f,  1.0f, 0.0f, 0.0f, 1f  // top left
  };

  private readonly uint[] _indices =
  {
      0, 1, 3,
      1, 2, 3
  };

  private int _elementBufferObject;
  private int _vertexBufferObject;
  private int _vertexArrayObject;
  private readonly Shader _shader = new();

  // these should not be null ... do i need an internal 'LoadedTileLayer' class to express this cleanly? weird
  private Texture _spritesheetTexture;
  private Texture _tilemapTexture;
  private ByteGrid _tiles = new(Settings.TilemapSize.X, Settings.TilemapSize.Y);

  /// <summary>
  /// -1 * how many tiles is the visible map away from the bottom left corner
  /// </summary>
  public Vector2 MapOffset = Vector2.Zero;
  /// <summary>
  /// How many tiles wide and high is the client size of the window
  /// </summary>
  protected Vector2 MapScale = Vector2.One;

  public Vector4 Color = Vector4.One;

  public ByteGrid Tiles => _tiles;

  public void Load()
  {
    if (!File.Exists(Settings.TilesetPath))
    {
      throw new FileNotFoundException($"MapRenderLayer: invalid value for Settings.TilesetPath. The file \"{Settings.TilesetPath}\" does not exist in the output folder");
    }

    _vertexArrayObject = GL.GenVertexArray();
    GL.BindVertexArray(_vertexArrayObject);

    _vertexBufferObject = GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
    GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

    _elementBufferObject = GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
    GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

    // The shaders have been modified to include the texture coordinates, check them out after finishing the OnLoad function.
    if (Settings.IsText) _shader.Load("Shaders/font.vert", "Shaders/font.frag");
    else _shader.Load("Shaders/tiles.vert", "Shaders/tiles.frag");
    _shader.Use();

    // Because there's now 5 floats between the start of the first vertex and the start of the second,
    // we modify the stride from 3 * sizeof(float) to 5 * sizeof(float).
    // This will now pass the new vertex array to the buffer.
    var vertexLocation = _shader.GetAttribLocation("aPosition");
    GL.EnableVertexAttribArray(vertexLocation);
    GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

    // Next, we also setup texture coordinates. It works in much the same way.
    // We add an offset of 3, since the texture coordinates comes after the position data.
    // We also change the amount of data to 2 because there's only 2 floats for texture coordinates.
    var texCoordLocation = _shader.GetAttribLocation("aTexCoord");
    GL.EnableVertexAttribArray(texCoordLocation);
    GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

    _shader.SetInt2("mapSize", _tiles.Size);
    _shader.SetInt2("sheetSize", Settings.TilesetSize);
    _shader.SetInt("isGreyScaleAlpha", Settings.IsGreyscaleAlpha ? 1 : 0);

    // okay ... blijkbaar is dit moeilijk. geen enkel werkend voorbeeld op internet te vinden, alleen
    // halve stackoverflow antwoorden zonder context
    // https://stackoverflow.com/questions/7954927/passing-a-list-of-values-to-fragment-shader
    // https://stackoverflow.com/questions/73176393/how-to-render-instances-using-the-new-opergl-opentk-apis
    // https://github.com/Rabbid76/c_sharp_opengl
    // https://vis.uni-jena.de/Lecture/ComputerGraphics2/Lec5_AdvancedDataGLSL.pdf
    // https://learnopengl.com/Advanced-OpenGL/Instancing
    // misschien is het beter om het te benaderen als particles met drawmeshinstanced
    //      https://www.opengl-tutorial.org/intermediate-tutorials/billboards-particles/particles-instancing/
    // of misschien als 1D texture via texelFetch zoals hier onder "1D Textures":
    //      https://stackoverflow.com/questions/7954927/passing-a-list-of-values-to-fragment-shader
    // of gewoon een 2D texture als array:
    //      https://stackoverflow.com/questions/4854584/glteximage2d-multiple-images
    // maar kan ik niet elk frame een complete mesh uploaden?

    //var tileArrayLocation = _shader.GetAttribLocation("aSpriteIndex");
    //GL.EnableVertexAttribArray(tileArrayLocation);

    //GL.VertexAttribFormat(tileArrayLocation, 4, VertexAttribType.Float, false, ix * vec4Size * sizeof(float));
    //GL.VertexArrayAttribBinding(VAO, tileArrayLocation, 1); //All use the same buffer binding

    //GL.VertexAttribPointer(tileArrayLocation, 2, VertexAttribPointerType.Float, false, sizeof(float), 3 * sizeof(float));

    _spritesheetTexture = Texture.LoadFromFile(Settings.TilesetPath, Settings.SmoothTextures);
    //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
    //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
    _spritesheetTexture.Use(TextureUnit.Texture0);

    {
      var size = _tiles.Size;
      _tilemapTexture = CreateTexture(size.X, size.Y, _tiles.Bytes);
      //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
      //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
      _tilemapTexture.Use(TextureUnit.Texture1);
    }

    // Next, we must setup the samplers in the shaders to use the right textures.
    // The int we send to the uniform indicates which texture unit the sampler should use.
    _shader.SetInt("texture0", 0);
    _shader.SetInt("texture1", 1);

    //FitMapToScreen();
  }

  public void Resize(int w, int h)
  {
    _tiles.Resize(w, h);
    _shader.Use();
    _shader.SetInt2("mapSize", _tiles.Size);

    _tilemapTexture = CreateTexture(_tiles.Size.X, _tiles.Size.Y, _tiles.Bytes);
    _tilemapTexture.Use(TextureUnit.Texture1);
  }

  /// <summary>
  /// update MapOffset and MapScale to fit the tiles onto the screen,
  /// using a 1:1 pixel ratio
  /// </summary>
  /// <param name="ClientSize">Clientsize of the window in pixels</param>
  public void AlignCenterContain(Vector2i ClientSize)
  {
    float tileSize = Math.Min(
        (float)ClientSize.X / (float)_tiles.Size.X,
        (float)ClientSize.Y / (float)_tiles.Size.Y
    );
    MapScale.X = ClientSize.X / tileSize;
    MapScale.Y = ClientSize.Y / tileSize;
    MapOffset.X = MapOffset.Y = 0f;

    // center
    float mapWidth = _tiles.Size.X * tileSize; // px
    float mapHeight = _tiles.Size.Y * tileSize; // px
    if (mapWidth < ClientSize.X)
      MapOffset.X = -(.5f * (ClientSize.X - mapWidth)) / tileSize;
    if (mapHeight < ClientSize.Y)
      MapOffset.Y = -(.5f * (ClientSize.Y - mapHeight)) / tileSize;
  }

  public float GetTileSize(Vector2i clientSize) => clientSize.X / MapScale.X;

  /// <summary>
  /// Fill up the screen with the entire map,
  /// squashing and stretching it however needed
  /// </summary>
  /// <param name="ClientSize"></param>
  public void AlignStretch(Vector2i ClientSize)
  {
    MapOffset.X = 0f;
    MapOffset.Y = 0f;
    MapScale.X = (float)_tiles.Size.X;
    MapScale.Y = (float)_tiles.Size.Y;
  }

  public void RenderFrame(FrameEventArgs e)
  {
    GL.BindVertexArray(_vertexArrayObject);

    _spritesheetTexture.Use(TextureUnit.Texture0);

    _tilemapTexture.Use(TextureUnit.Texture1);
    if (_tiles.IsChanged)
    {
      var size = _tiles.Size;
      UpdateTexturePixels(_tilemapTexture, size.X, size.Y, _tiles.Bytes);
      _tiles.ClearChanged();
    }

    _shader.Use();
    _shader.SetFloat2("mapOffset", MapOffset);
    _shader.SetFloat2("mapScale", MapScale);
    _shader.SetFloat4("color", Color);

    GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
    //GL.DrawElementsInstanced(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0, 100);
  }

  private static void UpdateTexturePixels(Texture texture, int width, int height, byte[] data)
  {
    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, width, height, 0, PixelFormat.Red, PixelType.UnsignedByte, data);
  }

  private static Texture CreateTexture(int width, int height, byte[] data)
  {
    // Generate handle
    int handle = GL.GenTexture();

    // Bind the handle
    GL.ActiveTexture(TextureUnit.Texture0);
    GL.BindTexture(TextureTarget.Texture2D, handle);

    /* C++ example

      glTexImage2D(
          GL_TEXTURE_2D, 0, GL_RED,
          dicomImage->GetColumns(), dicomImage->GetRows(),
          0, GL_RED, GL_UNSIGNED_BYTE, pixelArrayPtr);

     */

    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, width, height, 0, PixelFormat.Red, PixelType.UnsignedByte, data);

    // Now that our texture is loaded, we can set a few settings to affect how the image appears on rendering.

    // First, we set the min and mag filter. These are used for when the texture is scaled down and up, respectively.
    // Here, we use Linear for both. This means that OpenGL will try to blend pixels, meaning that textures scaled too far will look blurred.
    // You could also use (amongst other options) Nearest, which just grabs the nearest pixel, which makes the texture look pixelated if scaled too far.
    // NOTE: The default settings for both of these are LinearMipmap. If you leave these as default but don't generate mipmaps,
    // your image will fail to render at all (usually resulting in pure black instead).
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

    // Now, set the wrapping mode. S is for the X axis, and T is for the Y axis.
    // We set this to Repeat so that textures will repeat when wrapped. Not demonstrated here since the texture coordinates exactly match
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

    // Next, generate mipmaps.
    // Mipmaps are smaller copies of the texture, scaled down. Each mipmap level is half the size of the previous one
    // Generated mipmaps go all the way down to just one pixel.
    // OpenGL will automatically switch between mipmaps when an object gets sufficiently far away.
    // This prevents moiré effects, as well as saving on texture bandwidth.
    // Here you can see and read about the morié effect https://en.wikipedia.org/wiki/Moir%C3%A9_pattern
    // Here is an example of mips in action https://en.wikipedia.org/wiki/File:Mipmap_Aliasing_Comparison.png
    //GL.GenerateMipmap(GenerateMipmapTarget.Texture2D); // not needed?

    return new Texture(handle);
  }

  public void Align(Vector2i clientSize)
  {
    switch (Settings.Align)
    {
      case AlignmentMode.CenterContain:
        AlignCenterContain(clientSize);
        break;
      case AlignmentMode.TopLeftContain:
        // TODO
        break;
      case AlignmentMode.Stretch:
        AlignStretch(clientSize);
        break;
    }
  }

  /// <summary>
  /// Set MapScale, so that a 1x1 sprite is tileSize x tileSize on screen
  /// </summary>
  /// <param name="tileSize">Size of a 1x1 unscaled on screen. In pixels.</param>
  /// <param name="clientSize">Current size of the window client area. In pixels.</param>
  public void ScaleToTilesize(float tileSize, Vector2i clientSize)
  {
    MapScale.X = clientSize.X / tileSize;
    MapScale.Y = clientSize.Y / tileSize;
  }

  public void SetBottomLeftOffset(float tileSize, Vector2i clientSize)
  {
    // TODO
  }

  /// <summary>
  /// Given the position of a pixel in the client area (like Window.MousePosition)
  /// and the size of the client area (Window.ClientSize),
  /// returns that position in the tile coordinate space of the grid
  /// </summary>
  public Vector2 PixelToTilePosition(Vector2 position, Vector2i clientSize)
  {
    var x = (position.X / clientSize.X) * MapScale.X + MapOffset.X;
    var y = (position.Y / clientSize.Y) * MapScale.Y + MapOffset.Y;
    y = (_tiles.Size.Y - y);
    return new(x, y);
  }
}
