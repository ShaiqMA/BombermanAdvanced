using OpenTK.Windowing.Desktop;
using ITalent.Games.Tiled;
using TextWriter = ITalent.Games.Tiled.TextWriter;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Text;

namespace ITalent.Games.Examples.Menu;

/// <summary>
/// Demonstrates the game switching between different states, like menu, play, level complete, gameover,
/// while using the same tile grid and layers.
/// This is set up following a simple version of the State Machine pattern.
/// </summary>
internal class Menu(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
    : TileGameWindow(gameWindowSettings, nativeWindowSettings)
{
  private MapRenderLayer _mapLayer = new(MapTileSettings);
  private MapRenderLayer _textLayer = new(TextTileSettings);
  private IScreen? _screen;
  private readonly GameState _state = new();

  public MapRenderLayer MapLayer => _mapLayer;
  public MapRenderLayer TextLayer => _textLayer;
  public GameState GameState => _state;

  public void SwitchScreen<T>() where T : IScreen, new()
  {
    if (_screen != null) _screen.Exit();
    _screen = new T();
    _screen.Enter(this);
  }

  protected override void OnLoad()
  {
    base.OnLoad();
    SwitchScreen<MenuScreen>();
  }

  protected override void OnUpdateFrame(FrameEventArgs e)
  {
    base.OnUpdateFrame(e);
    _screen?.Update();
  }

  static MapRenderLayerSettings MapTileSettings => new()
  {
    TilesetPath = "Resources/tilemap_packed.png",
    TilesetSize = new(12, 11),
    TilemapSize = new(40, 20),
    Align = AlignmentMode.CenterContain
  };

  static MapRenderLayerSettings TextTileSettings => new()
  {
    TilesetPath = "Resources/oldschool_9x16.png",
    TilesetSize = new(32, 8),
    TilemapSize = new(60, 25),
    Align = AlignmentMode.Stretch,
    IsGreyscaleAlpha = true,
    IsText = true
  };
}

class GameState
{
  public int Stage;
  public int Score;
  public int Lives;

  public void Reset()
  {
    Stage = 1;
    Score = 0;
    Lives = 3;
  }
}

interface IScreen
{
  void Enter(Menu window);
  void Exit();
  void Update();
}

class MenuScreen : IScreen
{
  private Menu window;

  public void Enter(Menu window)
  {
    this.window = window;
    TextLayout textLayout = new(window.TextLayer);

    window.BackgroundColor = Color4.LightCoral;
    window.TextLayer.Color = (Vector4)Color4.DarkOliveGreen;

    window.ReplaceLayers(window.TextLayer);
    textLayout.CenterText("Welcome to Game\n\nPress SPACE to start\n\n\n[O]ptions");
  }

  public void Update()
  {
    if (window.KeyboardState.IsKeyPressed(Keys.Space))
    {
      window.GameState.Reset();
      window.SwitchScreen<LevelStartScreen>();
    }
    if (window.KeyboardState.IsKeyPressed(Keys.O))
    {
      window.SwitchScreen<OptionsScreen>();
    }
  }

  public void Exit() { }
}

class PlayGameScreen : IScreen
{
  private Menu window;

  public void Enter(Menu window)
  {
    this.window = window;
    window.BackgroundColor = Color4.Teal;
    window.ReplaceLayers(window.MapLayer, window.TextLayer);
    window.MapLayer.Tiles.RandomMap();
  }

  public void Update()
  {
    TextWriter w = new(window.TextLayer.Tiles);
    w.Clear();
    w.CursorPos = new(1, 1);
    w.Write($"Score: {window.GameState.Score}    Stage: {window.GameState.Stage}    Lives: {window.GameState.Lives}");
    w.CursorPos = new(1, 2);
    w.Write("Press: [W] to add score, [S] to loose a life, [D] to complete stage");

    // The actual game code goes here
    if (window.KeyboardState.IsKeyPressed(Keys.W))
    {
      window.GameState.Score += 10;
      window.MapLayer.Tiles.RandomMap();
    }
    if (window.KeyboardState.IsKeyPressed(Keys.D))
    {
      window.SwitchScreen<LevelCompleteScreen>();
      return;
    }
    if (window.KeyboardState.IsKeyPressed(Keys.S))
    {
      window.GameState.Lives--;
      window.MapLayer.Tiles.RandomMap();
      if( window.GameState.Lives <= 0 )
      {
        window.SwitchScreen<GameOverScreen>();
        return;
      }
    }
  }

  public void Exit() { }
}

class LevelStartScreen : IScreen
{
  private Menu window;
  private Timed _cooldown = new(2.0);

  public void Enter(Menu window)
  {
    this.window = window;
    TextLayout textLayout = new(window.TextLayer);

    window.BackgroundColor = Color4.DarkOliveGreen;
    window.TextLayer.Color = (Vector4)Color4.WhiteSmoke;

    window.ReplaceLayers(window.TextLayer);
    textLayout.CenterText($"Stage {window.GameState.Stage}");
  }

  public void Update()
  {
    if(_cooldown.IsOver)
    {
      window.SwitchScreen<PlayGameScreen>();
    }
  }

  public void Exit() { }
}

class OptionsScreen : IScreen
{
  private Menu window;
  private int optionIndex = 0;
  private string[] options = ["Easy", "Normal", "Hardcore", "Endless", "Challenge"];

  public void Enter(Menu window)
  {
    this.window = window;



    window.ReplaceLayers(window.TextLayer);
  }

  public void Update()
  {
    if (window.KeyboardState.IsKeyPressed(Keys.Down) && optionIndex < options.Length - 1)
    {
      optionIndex++;
    }
    if (window.KeyboardState.IsKeyPressed(Keys.Up) && optionIndex > 0)
    {
      optionIndex--;
    }
    if (window.KeyboardState.IsKeyPressed(Keys.Enter))
    {
      window.SwitchScreen<MenuScreen>();
      return;
    }
    
    StringBuilder menu = new();
    menu.Append("Please select:\n\n");
    for(int i = 0; i < options.Length;++i)
    {
      if (i == optionIndex) menu.Append($"-> {options[i]} <-\n");
      else menu.Append($"{options[i]}\n");
    }
    menu.Append("\nPress ENTER to return");

    TextLayout textLayout = new(window.TextLayer);
    textLayout.CenterText(menu.ToString());
  }

  public void Exit() { }
}

class LevelCompleteScreen : IScreen
{
  private Menu window;

  public void Enter(Menu window)
  {
    this.window = window;
    TextLayout textLayout = new(window.TextLayer);

    window.BackgroundColor = Color4.LightCoral;
    window.TextLayer.Color = (Vector4)Color4.DarkOliveGreen;

    window.ReplaceLayers(window.TextLayer);
    textLayout.CenterText($"Stage {window.GameState.Stage} completed!\nScore: {window.GameState.Score}\n\nPress SPACE");
  }

  public void Update()
  {
    if (window.KeyboardState.IsKeyPressed(Keys.Space))
    {
      window.GameState.Stage++;
      window.SwitchScreen<LevelStartScreen>();
    }
  }

  public void Exit() { }
}

class GameOverScreen : IScreen
{
  private Menu window;

  public void Enter(Menu window)
  {
    this.window = window;
    TextLayout textLayout = new(window.TextLayer);

    window.BackgroundColor = Color4.Red;
    window.TextLayer.Color = (Vector4)Color4.White;

    window.ReplaceLayers(window.TextLayer);
    textLayout.CenterText($"Game over!\n\nScore: {window.GameState.Score}\nStage: {window.GameState.Stage}\n\nPress SPACE");
  }

  public void Update()
  {
    if (window.KeyboardState.IsKeyPressed(Keys.Space))
    {
      window.SwitchScreen<MenuScreen>();
    }
  }

  public void Exit() { }
}

class TextLayout(MapRenderLayer layer)
{
  private TextWriter _textWriter = new(layer.Tiles);

  public void CenterText(string s)
  {
    _textWriter.Clear();
    string[] lines = s.Split('\n');
    //int w = lines.Max(line => line.Length);
    int y = (layer.Tiles.Size.Y - lines.Length) / 2;
    for(int i = 0; i < lines.Length;++i)
    {
      _textWriter.CursorPos = new((layer.Tiles.Size.X - lines[i].Length) / 2, i + y);
      _textWriter.Write(lines[i]);
    }
  }
}