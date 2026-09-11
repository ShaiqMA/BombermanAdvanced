using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace ITalent.Games.Tiled;

public interface IRenderLayer
{
  void Align(Vector2i clientSize);
  void Load();
  void RenderFrame(FrameEventArgs e);
  void ScaleToTilesize(float tileSize, Vector2i clientSize);
}
