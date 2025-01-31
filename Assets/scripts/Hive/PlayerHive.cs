using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class PlayerHive
{
    private readonly Dictionary<int, GameObject> _players = new();

    private readonly Dictionary<int, List<Vector3>> playerPath = new();

    public List<GameObject> Players()
    {
        return _players.Values.ToList();
    }

    public void AddPlayer(GameObject player)
    {
        Assert.IsFalse(_players.ContainsKey(player.GetInstanceID()), "cannot have duplicate players");

        playerPath.TryAdd(player.GetInstanceID(), new());
        _players.TryAdd(player.GetInstanceID(), player);
    }

    public void AddPlayer(IEnumerable<GameObject> players)
    {
        foreach (GameObject player in players)
        {
            Assert.IsFalse(_players.ContainsKey(player.GetInstanceID()), "cannot have duplicate players");
            Assert.IsNotNull(player.GetComponent<DemoPlayer>(), "player does not have demo player component");

            playerPath.TryAdd(player.GetInstanceID(), new());
            _players.TryAdd(player.GetInstanceID(), player);
        }
    }

    public bool HasPlayer(int playerId) => _players.ContainsKey(playerId);

    public bool HasPlayer(GameObject player) => _players.ContainsKey(player.GetInstanceID());

    public List<Vector3> GetPath(int playerId)
    {
        Assert.IsTrue(_players.ContainsKey(playerId), $"unexpected player {playerId}");

        playerPath.TryGetValue(playerId, out List<Vector3> val);

        Assert.IsNotNull(val, "path was not initialized for player ");
        return val;
    }

    public void AddPath(int playerId, Vector3 path)
    {
        Assert.IsTrue(_players.ContainsKey(playerId), $"unexpected player {playerId}");

        List<Vector3> currentPath = GetPath(playerId);

        currentPath.Add(path);

        playerPath.TryAdd(playerId, currentPath);
    }
}
