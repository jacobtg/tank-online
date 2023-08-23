using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public enum TankColor
{
    Bronze = 0,
    Gold = 1,
    Turquoise = 2,
    Blue = 3,
    NoFreeColor
}

public class MatchManager : MonoBehaviourPunCallbacks
{
    public static MatchManager Instance { get; private set; }

    public TankColor _myTankColor;
    private Dictionary<TankColor, bool> _tankColors = new Dictionary<TankColor, bool>()
    {
        { TankColor.Bronze, false },
        { TankColor.Gold, false },
        { TankColor.Turquoise, false },
        { TankColor.Blue, false }
    };

    private Dictionary<int, int[]> leaderboard = new Dictionary<int, int[]>();
    private List<KeyValuePair<string, float>> KDLeaderboard;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Master client needs a tank too :)
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                _myTankColor = GetFreeTankColor();
            }
            leaderboard[PhotonNetwork.LocalPlayer.ActorNumber] = new int[] { 1, 1 };
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            leaderboard[newPlayer.ActorNumber] = new int[] { 1, 1 };
            photonView.RPC("RPC_GetTankColorOnJoin", newPlayer, GetFreeTankColor());
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            _tankColors[TankColor.Bronze] = false;
            _tankColors[TankColor.Gold] = false;
            _tankColors[TankColor.Turquoise] = false;
            _tankColors[TankColor.Blue] = false;

            photonView.RPC("RPC_RequestColorReport", RpcTarget.AllViaServer);
        }
    }
    
    public override void OnLeftRoom()
    {
        Destroy(gameObject);
    }
    
    [PunRPC]
    private void RPC_GetTankColorOnJoin(TankColor color)
    {
        print("Got " + color + " in RPC_GetTankColorOnJoin");
        _myTankColor = color;
    }

    [PunRPC]
    private void RPC_ReportColorToMaster(TankColor color)
    {
        // Set color as taken
        _tankColors[color] = true;
    }
    
    [PunRPC]
    private void RPC_RequestColorReport()
    {
        ReportColorOnRequest(GameManager.Instance.AssignedTankColor);
    }

    public TankColor GetFreeTankColor()
    {
        var freeTankColor = TankColor.NoFreeColor; 

        foreach (var tankColor in _tankColors)
        {
            // Color is free
            if (tankColor.Value == false)
            {
                freeTankColor = tankColor.Key;
                break;
            }
        }

        if (freeTankColor != TankColor.NoFreeColor)
        {
            Debug.Log("Taking " + freeTankColor);
            _tankColors[freeTankColor] = true;
        }

        return freeTankColor;
    }

    public void SetTankColorOnGameManager()
    {
        GameManager.Instance.AssignedTankColor = _myTankColor;
        print(_myTankColor);
    }

    public void ReportColorOnRequest(TankColor color)
    {
        photonView.RPC("RPC_ReportColorToMaster", RpcTarget.MasterClient, color);
    }
    
    public void ReportKillAndDeath(Player killingPlayer, Player dyingPlayer)
    {
        photonView.RPC("RPC_AddKillTo", RpcTarget.MasterClient, killingPlayer);
        photonView.RPC("RPC_AddDeathTo", RpcTarget.MasterClient, dyingPlayer);
        photonView.RPC("RPC_CommandRespawn", dyingPlayer);
    }

    [PunRPC]
    private void RPC_AddKillTo(Player player)
    {
        leaderboard[player.ActorNumber][0] += 1;
        photonView.RPC("RPC_UpdateLeaderboard", RpcTarget.AllViaServer, leaderboard);
    }

    [PunRPC]
    private void RPC_AddDeathTo(Player player)
    {
        leaderboard[player.ActorNumber][1] += 1;
        photonView.RPC("RPC_UpdateLeaderboard", RpcTarget.AllViaServer, leaderboard);
    }

    [PunRPC]
    private void RPC_CommandRespawn()
    {
        GameManager.Instance.Respawn();
    }

    [PunRPC]
    private void RPC_UpdateLeaderboard(Dictionary<int, int[]> updatedLeaderboard)
    {
        leaderboard = updatedLeaderboard;
        var newKDLeaderboard = new List<KeyValuePair<string, float>>();
        foreach (var playerStats in leaderboard)
        {
            var playerNickname = PhotonNetwork.CurrentRoom.Players[playerStats.Key].NickName;
            
            var kills = playerStats.Value[0];
            var deaths = playerStats.Value[1];
            
            float kd = (float)kills / deaths;

            newKDLeaderboard.Add(new KeyValuePair<string, float>(playerNickname, kd));
        }

        KDLeaderboard = newKDLeaderboard.OrderByDescending(x => x.Value).ToList();
        
        GameManager.Instance.UpdateLeaderboardUI(KDLeaderboard);
    }

    public void MatchSceneLoad()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_UpdateLeaderboard", RpcTarget.AllViaServer, leaderboard);
        }

        GameManager.Instance.StartMatchEndTimer();
    }

    public string GetWinnerName()
    {
        return KDLeaderboard[0].Key;
    }
}
