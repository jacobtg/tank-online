using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchWinner : MonoBehaviour
{
    public static MatchWinner Instance { get; private set; }

    public string winnerName;

    private void Awake()
    {
        Instance = this;
    }
}
