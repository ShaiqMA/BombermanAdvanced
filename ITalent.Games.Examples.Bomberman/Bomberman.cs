using ITalent.Games.Tiled;
using OpenTK.Graphics.GL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using TextWriter = ITalent.Games.Tiled.TextWriter;

namespace ITalent.Games.Examples.Bomberman;

internal class Bomberman : TileGameWindow
{
  private readonly MapRenderLayer _blockLayer = new(BombermanTileSettings);
  private readonly MapRenderLayer _textLayer = new(TextTileSettings);
  private readonly MapCollision _collision;
  private readonly TiledSpriteRenderLayer _spriteLayer = new(BombermanTiledSpriteSettings);
  private readonly Camera _camera;
  private readonly GameState _state = new();
  private IScreen? _screen;
  private readonly UpdateContext _context = new();

  public TiledSpriteRenderLayer SpriteLayer => _spriteLayer;
  public MapRenderLayer TextLayer => _textLayer;
  public UpdateContext Context => _context;
  public Camera Camera => _camera;

  public Bomberman(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
  {
    _collision = new(_blockLayer.Tiles, [0, 1]);
    _camera = new(new(9, 9), new(19, 13), _blockLayer);
  }

  public void SwitchScreen<T>() where T : IScreen, new()
  {
    _screen = new T();
    _screen.Enter(this);
  }

  protected override void OnLoad()
  {
    base.OnLoad();

    // init context
    _context.Collision = _collision;
    _context.State = _state;
    _context.Map = _blockLayer;

    SwitchScreen<TitleScreen>();
  }

  protected override void OnUpdateFrame(FrameEventArgs e)
  {
    base.OnUpdateFrame(e);
    _screen?.Update();
  }

  static MapRenderLayerSettings BombermanTileSettings => new()
  {
    TilesetPath = "Resources/bomberman.png",
    TilesetSize = new(8, 4),
    TilemapSize = new(13, 21),
  };

  static MapRenderLayerSettings TextTileSettings => new()
  {
    TilesetPath = "Resources/oldschool_9x16.png",
    TilesetSize = new(32, 8),
    TilemapSize = new(80, 30),
    Align = AlignmentMode.Stretch,
    IsGreyscaleAlpha = true
  };

  static TiledSpriteRenderLayerSettings BombermanTiledSpriteSettings => new()
  {
    TilesetPath = "Resources/bomberman.png",
    TilesetSize = new(8, 4),
  };
}
 
interface IScreen
{
  void Enter(Bomberman window);
  void Update();
}

class PlayGameScreen : IScreen
{
  private Bomberman window;
  private UpdateContext _context;

  public void Enter(Bomberman window)
  {
    this.window = window;
    _context = window.Context;
    var state = _context.State;

    //window.BackgroundColor = Color4.Teal;
    window.ReplaceLayers(_context.Map, window.SpriteLayer, window.TextLayer);
    //_context.Map.AlignCenterContain(window.ClientSize);
  }

  public void Update()
  {
    _context.Keyboard = window.KeyboardState;

    var state = _context.State;
    state.Update(_context);
    state.UpdateSprites(window.SpriteLayer);

    window.Camera.Update(window.ClientSize, state.Player.AnimatedPosition);
    window.SpriteLayer.Update(
      _context.Map.GetTileSize(window.ClientSize),
      window.ClientSize,
      _context.Map.MapOffset);

    var textWriter = new TextWriter(window.TextLayer.Tiles);
    textWriter.Clear();
    textWriter.CursorPos = new(1, 1);
    textWriter.Write($"Score: 00000    Bombs: {state.MaxBombCount}   Power: {state.BombRange}   Lives: {state.Lives}");

    if( window.KeyboardState.IsKeyPressed(Keys.End))
    {
      window.SwitchScreen<LevelCompleteScreen>();
      return;
    }

    if (state.Player.IsRemoved)
    {
      state.Lives--;
      state.Player.IsRemoved = false;
      state.ResetStage();
      state.UpdateSprites(window.SpriteLayer);
      window.SwitchScreen<PlayerDeathScreen>();
    }
    else if (state.IsLevelComplete)
    {
      window.SwitchScreen<LevelCompleteScreen>();
    }
  }
}

class TitleScreen : IScreen
{
  private Bomberman window;

  public void Enter(Bomberman window)
  {
    this.window = window;
    TextLayout textLayout = new(window.TextLayer);

    //window.BackgroundColor = Color4.LightCoral;
    //window.TextLayer.Color = (Vector4)Color4.DarkOliveGreen;

    window.ReplaceLayers(window.TextLayer);
    textLayout.CenterText("Bomberman\n\nPress SPACE to start\n\n\n [1] Choose Stage");
  }

  public void Update()
  {
    if (window.KeyboardState.IsKeyPressed(Keys.Space))
    {
            window.Context.State.Reset();
            window.SwitchScreen<LevelStartScreen>();
      
      window.Context.State.InitStage(window.Context.Map);
      
    }
    else if (window.KeyboardState.IsKeyPressed(Keys.D1))
    {
      window.SwitchScreen<StageScreen>();
    }
  }
}

//class voor ene nieuwe scherm waar je stage kunt selecteren
class StageScreen : IScreen
{
    private Bomberman window;

    private int optionIndex = 0;
    //opties in de keuzemenu
    private string[] options = ["stage 1" , "stage 2" , "stage 3" , "stage 4" , "stage 5" , "stage 6" , "stage 7" , "stage 8" , "stage 9" , "stage 10"];
    public void Enter(Bomberman window)
    {
        this.window = window;    

        window.ReplaceLayers(window.TextLayer);
    }
    public void Update()
    {
        //zorgt voor navigatie met pijlen
        if (window.KeyboardState.IsKeyPressed(Keys.Down) && optionIndex < options.Length -1)
       {
            optionIndex++;
        }
        if (window.KeyboardState.IsKeyPressed(Keys.Up) && optionIndex > 0)
       {
            optionIndex--;
       }
        //kijkt bij enter welke keuze je hebt geklikt om de juiste stage te starten
        if (window.KeyboardState.IsKeyPressed(Keys.Enter))
        {
            window.Context.State.Reset();
            //window.Context.State.StageSelect(2);
            Level();
            StageStarter();
            return;
        }




        StringBuilder menu = new();
        menu.Append("Please slect: \n\n");
        for (int i = 0; i < options.Length; ++i)
        {
            if (i == optionIndex) menu.Append($"-> {options[i]} <-\n");
            else menu.Append($"{options[i]}\n");
        }
        menu.Append("\n Press Enter tot return");

        
        TextLayout textLayout = new(window.TextLayer);
        textLayout.CenterText(menu.ToString());
    }

    public void Exit() { }
    private void StageStarter()
    {
        ;
        window.Context.State.InitStage(window.Context.Map);
        window.SwitchScreen<LevelStartScreen>();
    }

    private void Level()
    {
        for (int i = 0; i < options.Length; i++)
        {

            if (window.KeyboardState.IsKeyPressed(Keys.Enter) && optionIndex == i)
            {
                int stage = i;
                window.Context.State.StageSelect(StageNumber: stage);

            }
        }
    }
}


class PlayerDeathScreen : IScreen
{
  private Bomberman window;
  private Timed _cooldown = new(2.0);

  public void Enter(Bomberman window)
  {
    this.window = window;

    TextLayout textLayout = new(window.TextLayer);
    textLayout.CenterText($"You died!\n{window.Context.State.Lives} Lives left\n\nStage{window.Context.State.Stage}");

    window.Context.State.Player.Move(new(1, 1));
  }

  public void Update()
  {
    if (_cooldown.IsOver)
    {
      if (window.Context.State.Lives > 0)
      {
        window.SwitchScreen<LevelStartScreen>();
      }
      else
      {
        window.SwitchScreen<GameOverScreen>();
      }
      return;
    }
    window.Camera.Update(window.ClientSize, window.Context.State.Player.AnimatedPosition);
    window.SpriteLayer.Update(
      window.Context.Map.GetTileSize(window.ClientSize),
      window.ClientSize,
      window.Context.Map.MapOffset);
  }
}

class LevelStartScreen : IScreen
{
  private Bomberman window;
  private Timed _cooldown = new(2.0);

  public void Enter(Bomberman window)
  {
    this.window = window;
    TextLayout textLayout = new(window.TextLayer);

    //window.BackgroundColor = Color4.DarkOliveGreen;
    //window.TextLayer.Color = (Vector4)Color4.WhiteSmoke;

    //window.ReplaceLayers(window.TextLayer);
    textLayout.CenterText($"Stage {window.Context.State.Stage}");
  }

  public void Update()
  {
    if (_cooldown.IsOver)
    {
      window.SwitchScreen<PlayGameScreen>();
    }
  }
}

class LevelCompleteScreen : IScreen
{
  private Bomberman window;

  public void Enter(Bomberman window)
  {
    this.window = window;
    TextLayout textLayout = new(window.TextLayer);

    //window.BackgroundColor = Color4.LightCoral;
    //window.TextLayer.Color = (Vector4)Color4.DarkOliveGreen;

    window.ReplaceLayers(window.TextLayer);
    textLayout.CenterText($"Stage {window.Context.State.Stage} completed!\n\nPress SPACE");
  }

  public void Update()
  {
    if (window.KeyboardState.IsKeyPressed(Keys.Space))
    {
      window.Context.State.NextLevel();
      window.Context.State.InitStage(window.Context.Map);
      window.SwitchScreen<LevelStartScreen>();
    }
  }
}

class GameOverScreen : IScreen
{
  private Bomberman window;

  public void Enter(Bomberman window)
  {
    this.window = window;
    TextLayout textLayout = new(window.TextLayer);

    //window.BackgroundColor = Color4.Red;
    //window.TextLayer.Color = (Vector4)Color4.White;

    window.ReplaceLayers(window.TextLayer);
    textLayout.CenterText($"Game over!\n\nStage: {window.Context.State.Stage}\n\nPress SPACE");
  }

  public void Update()
  {
    if (window.KeyboardState.IsKeyPressed(Keys.Space))
    {
      window.SwitchScreen<TitleScreen>();
    }
  }
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
    for (int i = 0; i < lines.Length; ++i)
    {
      _textWriter.CursorPos = new((layer.Tiles.Size.X - lines[i].Length) / 2, i + y);
      _textWriter.Write(lines[i]);
    }
  }
}

class MapCollision(ByteGrid map, byte[] walls)
{
  public bool IsWall(int x, int y)
  {
    return !map.IsInBounds(x, y) || walls.Contains(map[x, y]);
  }

  public bool IsWall(Vector2i pos)
  {
    return !map.IsInBounds(pos) || walls.Contains(map[pos.X, pos.Y]);
  }
}

class UpdateContext
{
  public KeyboardState Keyboard;
  public GameState State;
  public MapCollision Collision;
  public MapRenderLayer Map;
}

enum TreasureId { None, Range = 16, Bomb = 17,  ExtraLife = 26 , ExitDoor = 19 }

class GameState
{
  public int Lives;
  public int Stage;
  public int MaxBombCount;
  public int BombCount;
  public int BombRange;
  public Player Player;
  public List<Bomb> Bombs = [];
  public List<Fire> Fires = [];
  public List<Enemy> Enemies = [];
  public List<Treasure> Treasures = [];
  public Dictionary<Vector2i, TreasureId> TreasureLocations = [];

  private Random _random;
  private bool _needUpdate = true;
  private Timed _bombCooldown = new(.1);
  private static readonly Vector2i[] DIRECTIONS = [new(1, 0), new(-1, 0), new(0, 1), new(0, -1)];

  public bool DropBomb()
  {
    var p = Player.PerceptedPosition;
    if (BombCount < MaxBombCount && _bombCooldown.IsOver && BombAt(p) == null)
    {
      Debug.WriteLine($"Drop bomb {Time.Total}");
      Bombs.Add(new Bomb(p));
      BombCount++;
      _bombCooldown.Reset();
      _needUpdate = true;
      return true;
    }
    return false;
  }

  internal void UpdateSprites(TiledSpriteRenderLayer spriteLayer)
  {
    if (_needUpdate)
    {
      _needUpdate = false;
      spriteLayer.Sprites.Clear();
      spriteLayer.Sprites.AddRange(Treasures);
      spriteLayer.Sprites.AddRange(Enemies);
      spriteLayer.Sprites.Add(Player);
      spriteLayer.Sprites.AddRange(Bombs);
      spriteLayer.Sprites.AddRange(Fires);
    }
  }

  public void Explode(Bomb bomb, UpdateContext context)
  {
    bomb.IsRemoved = true;
    //Bombs.Remove(bomb);
    Fires.Add(new Fire(6, bomb.TilePosition));
    CheckFireKill(bomb.TilePosition);
    foreach (var delta in DIRECTIONS)
    {
      for (int i = 1; i <= BombRange; ++i)
      {
        var p = bomb.TilePosition + i * delta;
        // walls block fire
        if (context.Collision.IsWall(p))
        {
          // soft walls break
          if (context.Map.Tiles.GetTile(p) == 0)
          {
            context.Map.Tiles.SetTile(p, 2);
            Fires.Add(new Fire(15, p));
            if (TreasureLocations.ContainsKey(p))
            {
              Treasures.Add(new Treasure((int)TreasureLocations[p], p));
            }
          }
          break;
        }
        // fire blocks fire
        if (IsFire(p)) break;
        // check chain reaction
        {
          var other = BombAt(p);
          if (other != null && !other.IsRemoved)
          {
            Explode(other, context);
            break;
          }
        }
        CheckFireKill(p);
        // propagate fire
        var fire = new Fire(i == BombRange ? 14 : 7, p);
        if (i < BombRange) fire.Rot90Offset = 1;
        fire.FaceDirection(delta);
        Fires.Add(fire);
      }
    }
    BombCount--;
    _needUpdate = true;
  }

  public void CheckPlayerKill()
  {
    var p = Player.TilePosition;
    var other = EnemyAt(p);
    if (other != null && !other.IsRemoved && (Player.AnimatedPosition - other.AnimatedPosition).LengthSquared < .5f)
    {
      Fires.Add(new Fire(18, p));
      Player.IsRemoved = true;
    }
  }

  private void CheckFireKill(Vector2i p)
  {
    // check kill enemy
    var other = EnemyAt(p);
    if (other != null && !other.IsRemoved)
    {
      Fires.Add(new Fire(18, p));
      other.IsRemoved = true;
    }
    // check kill player
    if (Player.TilePosition == p)
    {
      Fires.Add(new Fire(18, p));
      Player.IsRemoved = true;
    }
  }

  public Bomb? BombAt(Vector2i p) => Bombs.FirstOrDefault(bomb => bomb.TilePosition == p);

  public Enemy? EnemyAt(Vector2i p) => Enemies.FirstOrDefault(enemy => enemy.TilePosition == p);

  private bool IsFire(Vector2i p) => Fires.Any(fire => fire.TilePosition == p);

  internal void RemoveFire(Fire fire)
  {
    fire.IsRemoved = true;
    //Fires.Remove(fire);
    _needUpdate = true;
  }

  private List<Vector2i> PickRandomSpots(Random random, ByteGrid grid, int count, Func<KeyValuePair<Vector2i, byte>, bool> selector)
  {
    return grid.Where(selector)
          .Select(p => (p.Key, random.Next()))
          .OrderBy(t => t.Item2)
          .Select(t => t.Key)
          .Take(count).ToList();
  }

  internal void PlaceTreasures(Random random, ByteGrid grid)
  {
    var options = PickRandomSpots(random, grid, 4, p => p.Value == 0);
    Debug.Assert(options.Count == 4);
    TreasureLocations[options[0]] = TreasureId.ExitDoor;
    TreasureLocations[options[1]] = TreasureId.Bomb;
    TreasureLocations[options[2]] = TreasureId.Range;
    TreasureLocations[options[3]] = TreasureId.ExtraLife;

    Debug.WriteLine(string.Join(',', options));
  }

  internal void PlaceEnemies(Random random, ByteGrid grid)
  {
    var options = PickRandomSpots(random, grid, Stage,
        p => p.Value > 1 && (Player.TilePosition - p.Key).ManhattanLength > 3 && p.Key.X < grid.Size.X - 1 && p.Key.Y < grid.Size.Y - 2);
    Debug.Assert(options.Count == Stage);
    for (int i = 0; i < Stage; ++i)
    {
      Enemies.Add(new Enemy(11 + (Stage % 3), options[i]));
      _needUpdate = true;
    }
  }

  internal bool IsLevelComplete => Treasures.Any(t => t.Frame == 20 && t.TilePosition == Player.TilePosition);

  internal void CheckTreasurePickup()
  {
    var p = Player.TilePosition;
    if (TreasureLocations.ContainsKey(p))
    {
      switch (TreasureLocations[p])
      {
        case TreasureId.Bomb:
          MaxBombCount++;
          break;
        case TreasureId.Range:
          BombRange++;
          break;
        case TreasureId.ExtraLife:
                    Lives++;
                    break;
        case TreasureId.ExitDoor:
          // TODO: next level if all enemies are dead
          return;
      }
      Treasures.Find(t => t.TilePosition == p)!.IsRemoved = true;
      TreasureLocations.Remove(p);
      _needUpdate = true;
    }
  }

  internal void Reset()
  {
    Lives = 3;
    Stage = 1;
    MaxBombCount = 1;
    BombCount = 0;
    BombRange = 1;
    Player = null;
    Bombs.Clear();
    Fires.Clear();
    Enemies.Clear();
    Treasures.Clear();
    TreasureLocations.Clear();
    _needUpdate = true;
  }

  internal void NextLevel()
  {
    Stage++;
    BombCount = 0;
    Bombs.Clear();
    Fires.Clear();
    Enemies.Clear();
    Treasures.Clear();
    TreasureLocations.Clear();
    _needUpdate = true;
  }
    internal void StageSelect(int StageNumber)
    {
        Stage = Stage + StageNumber;
    }
    


  internal void ResetStage()
  {
    BombCount = 0;
    Bombs.Clear();
    Fires.Clear();
    Player.Move(new(1, 1));
    foreach (var enemy in Enemies)
    {
      enemy.ResetPosition();
    }
    _needUpdate = true;
  }

  internal void InitStage(MapRenderLayer map)
  {
    _random = new Random(1000 + Stage);
    map.Tiles.Resize(
      9 + 2 * _random.Next(3 + Stage),
      9 + 2 * _random.Next(Stage)
    );

    DrawLevel(map);

    // add player character to sprites
    Player = new();
    PlaceTreasures(_random, map.Tiles);
    PlaceEnemies(_random, map.Tiles);
    _needUpdate = true;
  }

  private void DrawLevel(MapRenderLayer map)
  {
    int w = map.Tiles.Size.X;
    int h = map.Tiles.Size.Y;
    map.Tiles.Fill(2);
    for (int y = 0; y < h; ++y)
    {
      for (int x = 0; x < w; ++x)
      {
        if (x == 0 || y == 0 || y == h - 1 || x == w - 1 || x % 2 == 0 && y % 2 == 0)
        {
          // block
          map.Tiles.SetTile(x, y, 1);
        }
        else if ((x % 2 == 1 || y % 2 == 1) && (x > 2 || y > 2) && _random.Next() % 4 == 0)
        {
          // wall
          map.Tiles.SetTile(x, y, 0);
        }
        else if (_random.Next() % 4 == 0)
        {
          // dust
          map.Tiles.SetTile(x, y, 3);
        }
      }
    }
  }

  internal void Update(UpdateContext context)
  {
    Player.Update(context);
    foreach (var enemy in Enemies)
    {
      enemy.Update(context);
    }
    Enemies.RemoveAll(el => el.IsRemoved);
    CheckPlayerKill();
    foreach (var bomb in Bombs)
    {
      bomb.Update(context);
    }
    Bombs.RemoveAll(el => el.IsRemoved);
    foreach (var fire in Fires)
    {
      fire.Update(context);
    }
    Fires.RemoveAll(el => el.IsRemoved);
    Treasures.RemoveAll(el => el.IsRemoved);
    if (Treasures.Count > 0 && Enemies.Count == 0)
    {
      foreach (var item in Treasures.Where(t => t.Frame == 19))
      {
        item.Frame = 20;
      }
    }
  }
}

class Player : TiledSprite
{
  internal Player() : base(Random.Shared.Next(8, 11), new(1, 1))
  {
    FlipType = FlipType.Horizontal;
    moveSpeed = 4f;
  }

  internal void Update(UpdateContext context)
  {
    if (context.Keyboard.IsKeyPressed(Keys.Space))
    {
      context.State.DropBomb();
    }

    if (!AllowMove) return;

    // check move
    var delta = Input.WASD(context.Keyboard);
    if (delta != Vector2i.Zero)
    {
      FaceDirection(delta);
      var p = TilePosition + delta;
      if (!context.Collision.IsWall(p) && context.State.BombAt(p) == null)
      {
        Move(p);
        context.State.CheckTreasurePickup();
      }
    }
  }
}

class Bomb : TiledSprite
{
  private Timed _fuseTimer = new(2.5);

  internal Bomb(Vector2i pos) : base(5, pos)
  {
    _fuseTimer.Reset();
  }

  internal void Update(UpdateContext context)
  {
    if (_fuseTimer.IsOver)
    {
      context.State.Explode(this, context);
    }
    else
    {
      scale = (float)(1.0 + 0.1 * Math.Cos(_fuseTimer.Current * Math.Tau + Math.PI));
    }
  }
}

class Fire : TiledSprite
{
  private Timed _lifetime = new(1.0);

  internal Fire(int frame, Vector2i pos) : base(frame, pos)
  {
    _lifetime.Reset();
    FlipType = FlipType.Rot90;
  }

  internal void Update(UpdateContext context)
  {
    if (_lifetime.IsOver)
    {
      context.State.RemoveFire(this);
    }
  }
}

class Treasure : TiledSprite
{
  internal Treasure(int frame, Vector2i pos) : base(frame, pos) { }
}

class Enemy : TiledSprite
{
  private static readonly Vector2i[] DIRECTIONS = [new(1, 0), new(-1, 0), new(0, 1), new(0, -1)];

  private Vector2i originalPosition;
  private int direction;

  internal Enemy(int frame, Vector2i pos) : base(frame, pos)
  {
    FlipType = FlipType.Horizontal;
    moveSpeed = 2f;
    originalPosition = pos;
    direction = Random.Shared.Next(4);
  }

  internal void Update(UpdateContext context)
  {
    if (!AllowMove) return;

    bool IsWalkable(Vector2i p) => context.Map.Tiles.GetTile(p) > 1 && context.State.BombAt(p) == null;

    // 25% to randomly try to turn left or right
    if (Random.Shared.Next(4) == 0)
    {
      direction = (direction + 2 * Random.Shared.Next(2)) % 4;
    }

    int maxiter = 10;
    while (!IsWalkable(TilePosition + DIRECTIONS[direction]))
    {
      if (maxiter-- < 0) return;
      direction = Random.Shared.Next(4);
    }
    Move(TilePosition + DIRECTIONS[direction]);
  }

  public void ResetPosition()
  {
    Move(originalPosition);
  }
}