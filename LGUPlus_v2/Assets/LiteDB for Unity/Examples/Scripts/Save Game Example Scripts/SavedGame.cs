using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is a POCO class that helps store save game data in a LiteDB database.
/// </summary>
public class SavedGame
{
    public int Id { get; set; }
    public DateTime SaveTime { get; set; }
    public int KillCount { get; set; }
    public int DeathCount { get; set; }
    public List<Orientation> BadGuys { get; set; }
    public Orientation Hero { get; set; }
}

public class Orientation
{
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
}

