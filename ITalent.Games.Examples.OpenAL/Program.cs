using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace ITalent.Games.Examples.OpenAL;

public static class Program
{
  private static void Main()
  {
    ALTest();
    //AudioTest("Resources\\snare-trimmed.wav");
    return;

    var nativeWindowSettings = new NativeWindowSettings()
    {
      ClientSize = new Vector2i(800, 600),
      Title = "Exploring Sound via OpenAL",
      // This is needed to run on macos
      Flags = ContextFlags.ForwardCompatible,
      Vsync = VSyncMode.On
    };
    using (var window = new MyGame(GameWindowSettings.Default, nativeWindowSettings))
    {
      window.Run();
    }
  }

  private static void ALTest()
  {
    // https://www.openal.org/documentation/openal-1.1-specification.pdf page 15

    var device = ALC.OpenDevice(null);
    if( device != null)
    {
      var context = ALC.CreateContext(device, []);
      if( context != null )
      {
        ALC.MakeContextCurrent(context);

      }
    }

    //var device = new ALDevice();
    //if (ALC.IsExtensionPresent(device, "ALC_ENUMERATION_EXT"))
    //{
    //  // Enumeration Extension Found
    //  Console.WriteLine("Found");
    //}
  }

#if false

  private static void AudioTest(string filename)
  {
    using (ALContext context = new())
    {
      int buffer = AL.GenBuffer();
      int source = AL.GenSource();
      int state;

      int channels, bits_per_sample, sample_rate;
      byte[] sound_data = WavFile.LoadWave(File.Open(filename, FileMode.Open), out channels, out bits_per_sample, out sample_rate);
      AL.BufferData(buffer, WavFile.GetSoundFormat(channels, bits_per_sample), sound_data, sound_data.Length, sample_rate);

      AL.Source(source, ALSourcei.Buffer, buffer);
      AL.SourcePlay(source);

      Trace.Write("Playing");

      // Query the source to find out when it stops playing.
      do
      {
        Thread.Sleep(250);
        Trace.Write(".");
        AL.GetSource(source, ALGetSourcei.SourceState, out state);
      }
      while ((ALSourceState)state == ALSourceState.Playing);

      Trace.WriteLine("");

      AL.SourceStop(source);
      AL.DeleteSource(source);
      AL.DeleteBuffer(buffer);
    }

  }
#endif
}
