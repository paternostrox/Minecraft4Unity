using UnityEngine;
using System.Collections;

public static class GameController
{

    public const string gameVersion = "0.1.0";

    public static bool IsPaused { get => isPaused; set => isPaused = value; }
    public static bool isPaused;

    //public const int chunkSize = 24;

    //public static int drawRadius = 4; // Mesured in chunks!

    //public static int totalChunks = 129; // Total number of chunks in a sphere of radius drawRadius

}
