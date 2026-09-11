
using ITalent.Games.Tiled;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using TextWriter = ITalent.Games.Tiled.TextWriter;

namespace ITalent.Games.Examples.Tetris;

// TODO:
//    - fix bug where piece gets stuck in side when trying to rotate into it
//    - high score

internal class Tetris(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
    : TileGameWindow(gameWindowSettings, nativeWindowSettings)
{
  private static readonly Vector2i[][][] BLOCKS = [
    [
      // 2x2 cube 'Smashboy'
      [new(0,0),new(1,-1),new(0,-1),new(1,0)]
    ],
    [
      // 1x4 long 'Hero'
      [new(-1,0),new(0,0),new(1,0),new(2,0)],
      [new(0,1),new(0,0),new(0,-1),new(0,-2)]
    ],
    [
      // Z 'Cleveland Z'
      [new(-1,0),new(0,0),new(0,1),new(-1,-1)],
      [new(-1,0),new(0,0),new(0,-1),new( 1,-1)]
    ],
    [
      // S 'Rhode Island Z'
      [new(0, 0), new(1, 0), new( 0,-1), new(-1,-1)],
      [new(0, 0), new(0,-1), new(-1, 0), new(-1, 1)],
    ],
    [
      // L 'Orange Ricky'
      [new(0,0),new(-1,0),new(1,0),new(1,1)],
      [new(0,0),new(0,-1),new(0, 1),new(1,-1)],
      [new(0,0),new(-1,0),new(1,0),new(-1,-1)],
      [new(0,0),new(0,-1),new(0, 1),new(-1,1)],
    ],
    [
      // J 'Blue Ricky'
      [new(0,0),new(-1,0),new(1,0),new(-1,1)],
      [new(0,0),new(0,-1),new(0, 1),new(-1,-1)],
      [new(0,0),new(-1,0),new(1,0),new(1,-1)],
      [new(0,0),new(0,-1),new(0, 1),new(1,1)],
    ],
    [
      // T 'Teewee'
      [new(0,0),new(-1,0),new(1,0),new(0,1)],
      [new(0,0),new(0,-1),new(1,0),new(0,1)],
      [new(0,0),new(-1,0),new(1,0),new(0,-1)],
      [new(0,0),new(-1,0),new(0,1),new(0,-1)],
    ]
  ];

  private readonly byte[] COLORS = [GREEN, RED, PURPLE, YELLOW, ORANGE, BLUE, CYAN];
  private const byte GROUND = 4;
  private const byte PITBG = 5;
  private const byte SKY = 6;
  private const byte GRASS = 7;
  private const byte CYAN = 8;
  private const byte BLUE = 9;
  private const byte PURPLE = 10;
  private const byte BORDER = 11;
  private const byte RED = 12;
  private const byte ORANGE = 13;
  private const byte YELLOW = 14;
  private const byte GREEN = 15;

  private const int LINESPERLEVEL = 10;

  private MapRenderLayer _blockLayer = new(TetrisTileSettings);
  private MapRenderLayer _textLayer = new(TextTileSettings);
  private TextWriter _textWriter;

  private double _nextFall;
  private double _fallInterval;
  private double _time;

  private bool _allowFall; // prevent dropping multiple pieces in succession with space
  private PieceState _current = new();
  private int _score = 0;
  private int _linesLeft;
  private int _level;

  private enum GameState { Menu, Game };
  private GameState State;

  protected override void OnLoad()
  {
    _textWriter = new(_textLayer.Tiles);

    AddLayer(_blockLayer);
    AddLayer(_textLayer);

    base.OnLoad();

    State = GameState.Menu;

    //ResetGame();
    RenderMenu();
  }

  private void RenderMenu()
  {
    _blockLayer.Tiles.Fill(SKY);
    _blockLayer.Tiles.Fill(0, 0, 20, 4, GROUND);
    _blockLayer.Tiles.Fill(0, 4, 20, 1, GRASS);

    _textWriter.Clear();
    _textWriter.CursorPos = new(3, 3);
    _textWriter.Write("Press SPACE to start");
  }

  protected override void OnUpdateFrame(FrameEventArgs e)
  {
    base.OnUpdateFrame(e);
    _time += e.Time;

    var input = KeyboardState;
    if (State == GameState.Menu)
    {
      _textLayer.Color.Y = _textLayer.Color.Z = (float) (.5 + .5 * Math.Sin(_time * 5.0));

      // process input
      if (input.IsKeyPressed(Keys.Space))
      {
        State = GameState.Game;
        ResetGame();
      }
    }
    else
    {
      var next = _current; // copy, because structs
      var landWhenblocked = false; // if next position is blocked, land piece here?

      // process input

      // check menu
      if (input.IsKeyPressed(Keys.Q))
      {
        // quit game
        State = GameState.Menu;
        RenderMenu();
        return;
      }

      // check dropping with SPACE
      if (_allowFall && input.IsKeyDown(Keys.Space))
      {
        // drop
        next.Position.Y--;
        _nextFall = _fallInterval;
        landWhenblocked = true;
      }
      if (!input.IsKeyDown(Keys.Space))
      {
        _allowFall = true;
      }

      // move from autofall
      _nextFall -= e.Time;
      if (_nextFall < 0.0)
      {
        _nextFall = _fallInterval;
        next.Position.Y--;
        landWhenblocked = true;
      }

      // check landing
      if(landWhenblocked && !IsMoveAllowed(next))
      {
        Land();
        NextPiece();
        if (_linesLeft <= 0)
        {
          NextLevel();
        }
        else if (!IsEmpty(_current))
        {
          GameOver();
          return;
        }
      }

      // check moving
      if (input.IsKeyPressed(Keys.Down))
      {
        // move down
        next.Position.Y--;
        _nextFall = _fallInterval;
        // BUG : only extend time when actually moved down
      }
      if (input.IsKeyPressed(Keys.Left))
      {
        // move left
        next.Position.X--;
      }
      if (input.IsKeyPressed(Keys.Right))
      {
        // move right
        next.Position.X++;
      }
      if (input.IsKeyPressed(Keys.Up))
      {
        // rotate
        next.Rotation = (_current.Rotation + 1) % BLOCKS[_current.Piece].Length;
      }

      // check movement and line completion
      if (IsMoveAllowed(next))
      {
        RenderPiece(_current, PITBG);
        _current = next;
        RenderPiece(_current, COLORS[_current.Piece]);
      }

      RenderGameUI();
    }
  }

  private void RenderGameUI()
  {
    // update UI
    _textLayer.Color.X = _textLayer.Color.Y = _textLayer.Color.Z = 1f;
    _textWriter.Clear();
    _textWriter.CursorPos = new(0, 44);
    _textWriter.Write($"Score: {_score}");
    _textWriter.CursorPos = new(0, 46);
    _textWriter.Write($"{_linesLeft} lines left");
    _textWriter.CursorPos = new(10, 44);
    _textWriter.Write($"Level: {_level}");
  }

  private void ResetGame()
  {
    RenderTetrisMap();

    _score = 0;
    _fallInterval = 0.5;
    _nextFall = _fallInterval;
    _linesLeft = LINESPERLEVEL;
    _level = 1;

    NextPiece();
  }

  private void NextLevel()
  {
    NextPiece();
    RenderTetrisMap();

    _score += 10;
    _fallInterval *= .9;
    _nextFall = _fallInterval;
    _linesLeft = LINESPERLEVEL;
    _level++;
  }

  private void NextPiece()
  {
    _current.Piece = Random.Shared.Next(BLOCKS.Length);
    _current.Rotation = 0;
    _current.Position = new(4, 19);
  }

  private void GameOver()
  {
    ResetGame();
  }

  private void Land()
  {
    _nextFall = _fallInterval;
    _allowFall = false;

    // check lines completed
    int lineCount = 0;
    var ORIGIN = new Vector2i(5, 3);
    for (int y = 0; y < 20; ++y)
    {
      bool isComplete = true;
      for (int x = 0; x < 10; ++x)
      {
        var color = _blockLayer.Tiles.GetTile(ORIGIN.X + x, ORIGIN.Y + y);
        if (color == PITBG)
        {
          isComplete = false;
          break;
        }
      }
      if (isComplete)
      {
        ++lineCount;
        _blockLayer.Tiles.Copy(ORIGIN.X, ORIGIN.Y + y + 1, 10, 20 - y, ORIGIN.X, ORIGIN.Y + y);
        _blockLayer.Tiles.Fill(ORIGIN.X, ORIGIN.Y + 20, 10, 1, PITBG);
        --y;
      }
    }
    _score += lineCount * lineCount;
    _linesLeft -= lineCount;
  }

  private Vector2i[] GetPiecePositions(PieceState piece)
  {
    var result = new Vector2i[4];
    var ORIGIN = new Vector2i(5, 3);
    var points = BLOCKS[piece.Piece][piece.Rotation];
    for (int i = 0; i < 4; ++i)
    {
      result[i] = ORIGIN + piece.Position + points[i];
    }
    return result;
  }

  private bool IsEmpty(PieceState newPos)
  {
    var points = GetPiecePositions(newPos);
    foreach (var point in points)
    {
      var tile = _blockLayer.Tiles.GetTile(point);
      if (tile != PITBG) return false;
    }
    return true;
  }

  private bool IsMoveAllowed(PieceState newPos)
  {
    var ownPoints = GetPiecePositions(_current);
    var points = GetPiecePositions(newPos);
    foreach (var point in points)
    {
      if (ownPoints.Contains(point)) continue; // prevent colliding with self

      var tile = _blockLayer.Tiles.GetTile(point);
      if (tile != PITBG) return false;
    }
    return true;
  }

  private void RenderPiece(PieceState piece, byte color)
  {
    var points = GetPiecePositions(piece);
    foreach (var point in points)
    {
      _blockLayer.Tiles.SetTile(point, color);
    }
  }

  private void RenderTetrisMap()
  {
    // 20 x 22
    // [  4  ][1][    10    ][    5    ]
    // [  4  ][1][    10    ][1][  4   ]
    // [  4  ][1][    10    ][1][  4   ]
    // [  4  ][1][    10    ][1][  4   ]
    // [  4  ][1][    10    ][1][  4   ]
    // [  4  ][1][    10    ][    5    ]
    // [  4  ][1][    10    ][1][  4   ]
    // [  4  ][1][    10    ][1][  4   ]
    // [  4  ][1][    10    ][1][  4   ]
    // [  4  ][1][    10    ][1][  4   ]
    // [  4  ][1][    10    ][1][  4   ]
    // [  4  ][1][    10    ][1][  4   ]
    // [  4  ][       12       ][  4   ]

    _blockLayer.Tiles.Fill(SKY);
    _blockLayer.Tiles.Fill(0, 0, 20, 4, GROUND);
    _blockLayer.Tiles.Fill(0, 4, 20, 1, GRASS);
    _blockLayer.Tiles.Fill(4, 2, 12, 22, BORDER);
    _blockLayer.Tiles.Fill(5, 3, 10, 21, PITBG);
  }

  struct PieceState
  {
    public Vector2i Position;
    public int Piece;
    public int Rotation;

    public PieceState Clone()
    {
      return new PieceState
      {
        Piece = Piece,
        Rotation = Rotation,
        Position = Position
      };
    }
  }

  static MapRenderLayerSettings TetrisTileSettings => new()
  {
    TilesetPath = "Resources/tetris.png",
    TilesetSize = new(4, 4),
    TilemapSize = new(20, 24),
    Align = AlignmentMode.CenterContain
  };

  static MapRenderLayerSettings TextTileSettings => new()
  {
    TilesetPath = "Resources/oldschool_9x16.png",
    TilesetSize = new(32, 8),

    // overlay text over game
    TilemapSize = new(40, 48),
    Align = AlignmentMode.CenterContain,

    // default 8-bit style fullscreen text
    //TilemapSize = new(40, 25),
    //Align = TileGameSettings.AlignmentMode.Stretch,
    IsGreyscaleAlpha = true,
    IsText = true
  };
}
