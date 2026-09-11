using OpenTK.Windowing.Desktop;
using ITalent.Games.Tiled;

namespace ITalent.Games.Examples.HelloWorld;

internal class HelloWorld(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
    : TileGameWindow(gameWindowSettings, nativeWindowSettings)
{
  private MapRenderLayer _blockLayer = new(HelloWorldTileSettings);

  protected override void OnLoad()
  {
    AddLayer(_blockLayer);
    base.OnLoad();

    _blockLayer.Tiles.Fill(12); // rode blokjes
    _blockLayer.Tiles.SetTile(4, 2, 9); // blauw blokje
    for( int i = 0; i < _blockLayer.Tiles.Size.X && i < _blockLayer.Tiles.Size.Y; ++i)
    {
      _blockLayer.Tiles.SetTile(i, i, 15); // groen blokje
    }
  }

  static MapRenderLayerSettings HelloWorldTileSettings => new()
  {
    TilesetPath = "Resources/tetris.png",
    TilesetSize = new(4, 4),
    TilemapSize = new(9, 9),
    Align = AlignmentMode.CenterContain
  };
}
