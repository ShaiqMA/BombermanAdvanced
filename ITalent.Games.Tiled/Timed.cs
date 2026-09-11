namespace ITalent.Games.Tiled;

public class Timed(double interval)
{
  public double interval = interval;
  public double lastTick = Time.Total;

  public bool IsOver => Time.Total > lastTick + interval;
  public double Current => Time.Total - lastTick;
  public void Reset() => lastTick = Time.Total;
}