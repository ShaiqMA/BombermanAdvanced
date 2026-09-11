using ITalent.Games.Tiled;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace ITalent.Games.TopDownGame;

internal class Game : TileGameWindow
{
  private readonly MapLayer _blockLayer = new();
  private readonly ActorRenderLayer _spriteLayer = new();
  private readonly ActorCollisionLayer _collisionLayer = new();
  private readonly TextLayer _textLayer = new();
  private readonly ActorBehaviour _actorBehaviour = new();
  private readonly Camera _camera;

  public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
  {
    _camera = new(_blockLayer);
  }

  protected override void OnLoad()
  {
    _textLayer.Init();
    _blockLayer.GenerateMap();
    _actorBehaviour.Initialize(_blockLayer.Tiles, _spriteLayer, _collisionLayer);

    AddLayer(_blockLayer);
    AddLayer(_spriteLayer);
    AddLayer(_textLayer);

    base.OnLoad();

    GL.ClearColor(new Color4(234, 165, 108, 255));
  }

  protected override void OnUpdateFrame(FrameEventArgs e)
  {
    base.OnUpdateFrame(e);

    _actorBehaviour.Update(KeyboardState);
    _camera.Update(KeyboardState, ClientSize, _actorBehaviour.PlayerPosition);
    _spriteLayer.Update(_camera.TileSize, ClientSize, _blockLayer.MapOffset);
    _textLayer.Update(_camera.TileSize);
  }
}
