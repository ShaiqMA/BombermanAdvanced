using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ITalent.Games.Tiled;

public class TileGameWindow(GameWindowSettings gameWindowSettings,NativeWindowSettings nativeWindowSettings)
      : GameWindow(gameWindowSettings, nativeWindowSettings)
{
  private readonly List<IRenderLayer> _layers = [];

  public void AddLayer(IRenderLayer layer)
  {
    _layers.Add(layer);
  }

  public void ReplaceLayers(params IRenderLayer[] layers)
  {
    _layers.Clear();
    _layers.AddRange(layers);
    foreach (var layer in _layers)
    {
      layer.Load();
      layer.Align(ClientSize);
    }
  }

  public Color4 BackgroundColor
  {
    set { GL.ClearColor(value); }
  }

  protected override void OnLoad()
  {
    base.OnLoad();

    GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

    foreach (var layer in _layers)
    {
      layer.Load();
      layer.Align(ClientSize);
    }
  }

  protected override void OnRenderFrame(FrameEventArgs e)
  {
    base.OnRenderFrame(e);

    GL.Clear(ClearBufferMask.ColorBufferBit);

    foreach (var layer in _layers)
    {
      layer.RenderFrame(e);
    }

    SwapBuffers();
  }

  protected override void OnUpdateFrame(FrameEventArgs e)
  {
    base.OnUpdateFrame(e);

    Time.OnUpdateFrame(e);

    var input = KeyboardState;

    if (input.IsKeyDown(Keys.Escape))
    {
      Close();
    }
    if (input.IsKeyPressed(Keys.F11))
    {
      WindowState = IsFullscreen ? WindowState.Normal : WindowState.Fullscreen;
      VSync = nativeWindowSettings.Vsync;
    }
  }

  protected override void OnResize(ResizeEventArgs e)
  {
    base.OnResize(e);

    foreach (var layer in _layers)
    {
      layer.Align(ClientSize);
    }

    GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
  }
}
