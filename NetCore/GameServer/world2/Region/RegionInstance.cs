using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DOL.GS;

namespace GS.World2;

public class RegionInstance
{
	public readonly RegionTemplate Template;
	public readonly bool IsPermanent;

	public int PriorityLevel
	{
		get
		{
			lock (_players) return _players.Count;
		}
	}

	private readonly List<GamePlayer> _players = new(8);
	private readonly List<GameNPC> _npcs = new(256);
	private readonly List<GameObject> _objects = new(256);

	private readonly ConcurrentBag<GamePlayer> _playersToAdd = new ();
	private ConcurrentBag<GamePlayer> _playersToRemove = new ();

	private readonly ConcurrentBag<GameNPC> _npcsToAdd = new ();
	private readonly ConcurrentBag<GameNPC> _npcsToRemove = new ();

	private readonly ConcurrentBag<GameObject> _objectsToAdd = new ();
	private readonly ConcurrentBag<GameObject> _objectsToRemove = new ();

	public RegionInstance(RegionTemplate template, bool isPermanent)
	{
		Template = template;
		IsPermanent = isPermanent;
		if (isPermanent)
			_players = new(256);
	}

	public void UpdatePlayers(float deltaTime)
	{
		var playersToRemove = Interlocked.Exchange(ref _playersToRemove, new ConcurrentBag<GamePlayer>());
		while (_playersToRemove.TryTake(out var player))
			_players.Remove(player);
		while (_playersToAdd.TryTake(out var player))
			_players.Add(player);

		Parallel.ForEach(_players, player =>
		{
			if (player.Velocity.LengthSquared() > 0)
				player.Position += player.Velocity * deltaTime;
		});
	}

	public void UpdateNPCs(float deltaTime)
	{
		while (_npcsToRemove.TryTake(out var npc))
			_npcs.Remove(npc);
		while (_npcsToAdd.TryTake(out var npc))
			_npcs.Add(npc);

		Parallel.ForEach(_npcs, npc =>
		{
			if (npc.Velocity.LengthSquared() > 0)
				npc.Position += npc.Velocity * deltaTime;
		});
	}

	public void SendNetworkUpdates(float deltaTime)
	{
		while (_objectsToRemove.TryTake(out var obj))
			_objects.Remove(obj);
		while (_objectsToAdd.TryTake(out var obj))
			_objects.Add(obj);

		Parallel.ForEach(_players, player =>
		{
			// TODO
		});
	}

	public void AddPlayer(GamePlayer player)
	{
		_playersToAdd.Add(player);
	}

	public void RemovePlayer(GamePlayer player)
	{
		_playersToRemove.Add(player);
	}

	public void AddNpc(GameNPC npc)
	{
		_npcsToAdd.Add(npc);
	}

	public void RemoveNpc(GameNPC npc)
	{
		_npcsToRemove.Add(npc);
	}

	public void AddObject(GameObject gameObject)
	{
		Debug.Assert(gameObject is not GamePlayer);
		Debug.Assert(gameObject is not GameNPC);
		_objectsToAdd.Add(gameObject);
	}

	public void RemoveObject(GameObject gameObject)
	{
		Debug.Assert(gameObject is not GamePlayer);
		Debug.Assert(gameObject is not GameNPC);
		_objectsToRemove.Add(gameObject);
	}
}