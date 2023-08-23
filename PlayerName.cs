using UnityEngine;
using TMPro;
using Photon.Pun;

public class PlayerName : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerName;

    private const string playerNamePrefKey = "PlayerName";

    private void Start()
    {
        string defaultName = string.Empty;
        
        TMP_InputField inputField = GetComponent<TMP_InputField>();
        
        if (inputField)
        {
            if (PlayerPrefs.HasKey(playerNamePrefKey))
            {
                defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                inputField.text = defaultName;
            }
        }

        PhotonNetwork.NickName = defaultName;
        playerName.text = PhotonNetwork.NickName;
    }

    public void SetPlayerName(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            Debug.LogError("Player name is null or empty");
            return;
        }

        Debug.Log($"Player name set to {value}");
        PhotonNetwork.NickName = value;
        playerName.text = PhotonNetwork.NickName;

        PlayerPrefs.SetString(playerNamePrefKey, value);
    }
}
