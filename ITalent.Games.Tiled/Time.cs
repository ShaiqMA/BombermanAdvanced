using System;
using OpenTK.Windowing.Common;

namespace ITalent.Games.Tiled;

public static class Time
{
  public static double Total { private set; get; }
  public static double Delta { private set; get; }
  public static float DeltaF => (float)Delta;
  public static int Fps => (int) Math.Round(1.0 / Time.Delta);

  public static void OnUpdateFrame(FrameEventArgs e)
  {
    Delta = e.Time;
    Total += Delta;
  }
}
