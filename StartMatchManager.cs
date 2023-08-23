using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class StartMatchManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject masterClientPanel;
    [SerializeField] private TextMeshProUGUI masterClientPlayersJoinedText;

    [SerializeField] private GameObject clientPanel;
    [SerializeField] private TextMeshProUGUI clientPlayersJoinedText;

    private void Start()
    {
        UpdatePlayersJoinedText();
        
        if (PhotonNetwork.IsMasterClient)
        {
            masterClientPanel.SetActive(true);
        }
        else
        {
            clientPanel.SetActive(true);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // if (isMasterWaitingForOpponent)
            // {
            //     isMasterWaitingForOpponent = false;
            //     GameManager.Instance.RemoveWaitingForOpponent();
            //     photonView.RPC("RPC_GetTankColorOnJoin", newPlayer, GetFreeTankColor());
            //     photonView.RPC("RPC_StartMatch", RpcTarget.AllViaServer);
            // }
            // else
            // {
            //     photonView.RPC("RPC_GetTankColorOnJoin", newPlayer, GetFreeTankColor());
            // }
        }

        UpdatePlayersJoinedText();    
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayersJoinedText();
    }

    private void UpdatePlayersJoinedText()
    {
        var playersJoinedText = $"{PhotonNetwork.CurrentRoom.PlayerCount}/4 Players";
        masterClientPlayersJoinedText.text = playersJoinedText;
        clientPlayersJoinedText.text = playersJoinedText;
    }    

    public void OnStartMatchClick()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel("Room");
        }
    }
}
