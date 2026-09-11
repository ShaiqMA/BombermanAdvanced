using ITalent.Games.Tiled;
using ITalent.Games.TopDownGame.Actors;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ITalent.Games.TopDownGame;

internal interface IPlayer_ActorBehaviour
{
  void Shoot(Vector2i pos, Vector2i delta);
}

internal interface INest_ActorBehaviour
{
  void HatchSpider(Vector2i pos);
}

internal class ActorBehaviour : IPlayer_ActorBehaviour, INest_ActorBehaviour
{
  private const int POTIONCOUNT = 1000;
  private const int SPIDERCOUNT = 4000;
  private const int RATCOUNT = 4000;
  private const int NESTCOUNT = 20;

  private Player player;
  private List<Rat> rats = new(RATCOUNT);
  private List<Spider> spiders = new(SPIDERCOUNT);
  private List<Potion> potions = new(POTIONCOUNT);
  private List<Nest> nests = new(NESTCOUNT);
  private List<Projectile> projectiles = new();
  private List<Effect> effects = new();
  private ByteGrid map;
  private ActorRenderLayer actorList;
  private ActorCollisionLayer actorCollisions;

  public Vector2i PlayerPosition => player.Actor.tilePosition;

  public void Initialize(ByteGrid map, ActorRenderLayer actorList, ActorCollisionLayer actorCollisions)
  {
    this.map = map;
    this.actorList = actorList;
    this.actorCollisions = actorCollisions;

    // player
    player = Player.Create(actorCollisions, this, new(0, 0));
    RegisterActor(player.Actor, true);

    // spider spawners
    foreach (var pos in FindRandomPositions(NESTCOUNT))
    {
      var nest = Nest.Create(actorCollisions, this, pos);
      nests.Add(nest);
      RegisterActor(nest.Actor, true);
    }

    // prey
    foreach (var pos in FindRandomPositions(RATCOUNT))
    {
      var rat = Rat.Create(actorCollisions, pos);
      rats.Add(rat);
      RegisterActor(rat.Actor, true);
    }

    // spiders
    foreach (var pos in FindRandomPositions(SPIDERCOUNT))
    {
      var spider = Spider.Create(actorCollisions, pos);
      spiders.Add(spider);
      RegisterActor(spider.Actor, true);
    }

    // potions
    foreach (var pos in FindRandomPositions(POTIONCOUNT))
    {
      var potion = Potion.Create(pos);
      potions.Add(potion);
      RegisterActor(potion.Actor, true);
    }

    actorList.UpdateInstanceArrays();
  }

  private IEnumerable<Vector2i> FindRandomPositions(int count)
  {
    Vector2i position = new();
    while (count > 0)
    {
      position.X = Random.Shared.Next(map.Size.X);
      position.Y = Random.Shared.Next(map.Size.Y);
      if (actorCollisions.Empty(position))
      {
        yield return position;
        --count;
      }
    }
  }

  public static IEnumerable<Vector2i> Offsets(int radius)
  {
    var uv = new Vector2i();
    for (uv.X = -radius; uv.X <= radius; ++uv.X)
    {
      for (uv.Y = -radius; uv.Y <= radius; ++uv.Y)
      {
        if (uv.X != 0 || uv.Y != 0)
        {
          yield return uv;
        }
      }
    }
  }

  public static Vector2i RandomMove()
  {
    switch (Random.Shared.Next() % 4)
    {
      case 0: return new(1, 0);
      case 1: return new(0, 1);
      case 2: return new(-1, 0);
      default: return new(0, -1);
    }
  }

  private void RemoveActors<T>(List<T> list) where T : IActorController
  {
    for (int i = 0; i < list.Count; ++i)
    {
      if (list[i].Actor.IsRemoved)
      {
        actorList.RemoveActor(list[i].Actor);
        actorCollisions.RemoveActor(list[i].Actor);
        list.RemoveAt(i--);
      }
    }
  }

  public void Update(KeyboardState input)
  {
    player.Update(map, input);

    // spawn
    List<(int, Vector2i)> spawnEffects = [];
    List<Vector2i> spawnPotions = spiders.Where(s => s.Actor.IsRemoved).Select(s => s.Actor.tilePosition).ToList();
    List<Vector2i> explosions = []; // projectiles.Where(p => p.lastHitResult == Projectile.HitResult.Explode).Select(p => p.Actor.tilePosition).ToList();

    // die effects
    spawnEffects.AddRange(rats.Where(n => n.Actor.IsRemoved).Select(n => (SpriteID.BLOOD, n.Actor.tilePosition)));
    spawnEffects.AddRange(spiders.Where(n => n.Actor.IsRemoved).Select(n => (SpriteID.BLOOD, n.Actor.tilePosition)));
    spawnEffects.AddRange(nests.Where(n => n.Actor.IsRemoved).Select(n => (SpriteID.FX_POOF, n.Actor.tilePosition)));

    // fireball effects
    foreach (var projectile in projectiles)
    {
      if (projectile.lastHitResult == Projectile.HitResult.Explode)
      {
        explosions.Add(projectile.Actor.tilePosition);
        spawnEffects.Add((SpriteID.FX_STAR, projectile.Actor.tilePosition));
        projectile.lastHitResult = Projectile.HitResult.NoHit;
      }
      else if (projectile.lastHitResult == Projectile.HitResult.Hit)
      {
        spawnEffects.Add((SpriteID.FX_SPAT, projectile.Actor.tilePosition));
      }
    }

    // remove
    RemoveActors(nests);
    RemoveActors(rats);
    RemoveActors(spiders);
    RemoveActors(potions);
    RemoveActors(projectiles);
    RemoveActors(effects);

    // update
    foreach (var nest in nests) nest.Update(map);
    foreach (var rat in rats) rat.Update(map);
    foreach (var spiders in spiders) spiders.Update(map);
    foreach (var potion in potions) potion.Update();
    foreach (var projectile in projectiles) projectile.Update(map);
    foreach (var effect in effects) effect.Update();

    // spawn 
    foreach (var p in spawnPotions.Where(p => actorCollisions.Empty(p)))
    {
      var potion = Potion.Create(p);
      potions.Add(potion);
      RegisterActor(potion.Actor, true);
    }
    foreach (var p in explosions)
    {
      Shoot(p, new(1, 0));
      Shoot(p, new(-1, 0));
      Shoot(p, new(0, 1));
      Shoot(p, new(0, -1));
    }
    foreach (var (idx, p) in spawnEffects)
    {
      var effect = Effect.Create(idx, p, 1f);
      effects.Add(effect);
      RegisterActor(effect.Actor, false);
    }
  }

  public void Shoot(Vector2i pos, Vector2i delta)
  {
    var projectile = Projectile.CreateFireball(actorCollisions, pos, delta);
    projectiles.Add(projectile);
    RegisterActor(projectile.Actor, false);
  }

  public void HatchSpider(Vector2i pos)
  {
    var spider = Spider.Create(actorCollisions, pos);
    spiders.Add(spider);
    RegisterActor(spider.Actor, true);
  }

  private void RegisterActor(Actor actor, bool collision)
  {
    // TODO
    actorList.RegisterActor(actor);
    if (collision) actorCollisions.RegisterActor(actor);
  }
}
