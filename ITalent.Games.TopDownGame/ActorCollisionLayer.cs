using OpenTK.Mathematics;
using System.Diagnostics;

namespace ITalent.Games.TopDownGame;

internal class ActorCollisionLayer
{
  private readonly Dictionary<Vector2i, Actor> actorDict = new();

  public void RegisterActor(Actor actor)
  {
    Debug.Assert(!actorDict.ContainsKey(actor.tilePosition));
    actorDict.Add(actor.tilePosition, actor);
  }

  public void RemoveActor(Actor actor)
  {
    if (actorDict.TryGetValue(actor.tilePosition, out var other) && other == actor)
    {
      actorDict.Remove(actor.tilePosition);
    }
  }

  public bool Empty(Vector2i pos) => !actorDict.ContainsKey(pos);
  public Actor? ActorAt(Vector2i pos) => actorDict.TryGetValue(pos, out var actor) ? actor : null;

  public void Move(Vector2i from, Vector2i to)
  {
    Debug.Assert(from != to);
    Debug.Assert(actorDict[from].tilePosition == from);

    actorDict.Add(to, actorDict[from]);
    actorDict.Remove(from);
  }
}
