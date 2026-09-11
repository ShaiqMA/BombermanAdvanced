using ITalent.Games.Tiled;
using NAudio.Wave;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Diagnostics;
using System.Timers;

namespace ITalent.Games.NAudio;

internal class MyGame(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
    : TileGameWindow(gameWindowSettings, nativeWindowSettings)
{
  private MapRenderLayer _blockLayer = new(HelloWorldTileSettings);
  private IWavePlayer wavePlayer;
  private AudioFileReader audioFileReader;

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

    wavePlayer = LoadSound("C:\\Users\\Stijn\\source\\repos\\NAudio\\SampleData\\Drums\\snare-trimmed.wav");
  }

  private IWavePlayer LoadSound(string filename)
  {
    var wavePlayer = new WaveOutEvent();
    audioFileReader = new AudioFileReader(filename);
    audioFileReader.Volume = 1f;
    wavePlayer.Init(audioFileReader);
    return wavePlayer;
  }

  protected override void OnUpdateFrame(FrameEventArgs e)
  {
    base.OnUpdateFrame(e);
    if (KeyboardState.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Space))
    {
      audioFileReader.Position = 0;
      wavePlayer.Play();
    }

    // werkt, maar ik word er niet blij van.
    // misschien deze: https://github.com/MrBrixican/Foster.Audio/blob/main/Samples/AudioVisualizer/Program.cs
    // of misschien deze: https://www.ambiera.com/irrklang/tutorial-memoryplayback-csharp.html
  }

  private void BeginPlayback(string filename)
  {
    Debug.Assert(wavePlayer == null);
    wavePlayer = new WaveOutEvent();
    audioFileReader = new AudioFileReader(filename);
    audioFileReader.Volume = 1f;
    wavePlayer.Init(audioFileReader);
    wavePlayer.PlaybackStopped += OnPlaybackStopped;
    wavePlayer.Play();
    return;

    // Source - https://stackoverflow.com/a/54670829
    // Posted by dbraillon, modified by community. See post 'Timeline' for change history
    // Retrieved 2026-06-02, License - CC BY-SA 4.0

    using (var waveOut = new WaveOutEvent())
    using (var wavReader = new WaveFileReader(filename))
    {
      waveOut.Init(wavReader);
      waveOut.Volume = 1f;
      waveOut.Play();
    }

  }

  //private IWavePlayer CreateWavePlayer()
  //{
  //  return new WaveOut();
  //}

  void OnPlaybackStopped(object sender, StoppedEventArgs e)
  {
    Debug.WriteLine("Playback stopped.");
  }

  static MapRenderLayerSettings HelloWorldTileSettings => new()
  {
    TilesetPath = "Resources/tetris.png",
    TilesetSize = new(4, 4),
    TilemapSize = new(9, 9),
    Align = AlignmentMode.CenterContain
  };
}
