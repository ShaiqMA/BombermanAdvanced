using OpenTK;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenALTests;

class Program
{
  static unsafe void Main(string[] args)
  {
    //Initialize
    var device = ALC.OpenDevice(null);
    var context = ALC.CreateContext(device, (int*)null);

    ALC.MakeContextCurrent(context);

    var version = AL.Get(ALGetString.Version);
    var vendor = AL.Get(ALGetString.Vendor);
    var renderer = AL.Get(ALGetString.Renderer);
    Console.WriteLine(version);
    Console.WriteLine(vendor);
    Console.WriteLine(renderer);

    //Process

    int sampleFreq = 44100;
    double dt = 2 * Math.PI / sampleFreq;
    var dataCount = 100;
    double amp = 0.5;

#if false
    for (int freq = 440; freq < 10000; freq += 100)
    {
      int source;
      int buffers;
      AL.GenBuffers(1, out buffers);
      AL.GenSources(1, out source);

      var sinData = new short[dataCount];
      for (int i = 0;
      i < sinData.Length; ++i)
      {
        sinData[i] = (short)(amp * short.MaxValue * Math.Sin(i * dt * freq));
      }

      AL.BufferData(buffers, ALFormat.Mono16, sinData, sinData.Length, sampleFreq);
      AL.Source(source, ALSourcei.Buffer, buffers);
      AL.Source(source, ALSourceb.Looping, true);

      AL.SourcePlay(source);
      Thread.Sleep(100);
    }
    Console.WriteLine("fin");
    Console.ReadKey();

    ///Dispose
    if (context != ContextHandle.Zero)
    {
      ALC.MakeContextCurrent(ContextHandle.Zero);
      ALC.DestroyContext(context);
    }
    context = ContextHandle.Zero;

    if (device != IntPtr.Zero)
    {
      ALC.CloseDevice(device);
    }
    device = IntPtr.Zero;
#endif
  }
}
