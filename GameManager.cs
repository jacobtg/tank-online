using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject bronzeTankPrefab;
    [SerializeField] private GameObject goldTankPrefab;
    [SerializeField] private GameObject turquoiseTankPrefab;
    [SerializeField] private GameObject blueTankPrefab;

    [SerializeField] private GameObject countdownPanel;
    [SerializeField] private TextMeshProUGUI countdownText;

    [SerializeField] private TextMeshProUGUI[] leaderboardPositionsText = new TextMeshProUGUI[4];

    [SerializeField] private TextMeshProUGUI matchTimeLeftText;

    public TankColor AssignedTankColor { get; set; }

    public static GameManager Instance { get; private set; }

    private GameObject _myTank = null;

    private float _matchTimeLeft = 60.0f;

    private bool _matchHasStarted = false;
    private bool _matchHasEnded = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        MatchManager.Instance.SetTankColorOnGameManager();
        MatchManager.Instance.MatchSceneLoad();
        StartCoroutine(StartCountdown());
    }

    private void Update()
    {
        if (_matchHasStarted && !_matchHasEnded)
        {
            _matchTimeLeft -= Time.deltaTime;
            _matchTimeLeft = Mathf.Clamp(_matchTimeLeft, 0.0f, 60.0f);
        
            if (_matchTimeLeft == 0.0f)
            {
                _matchHasEnded = true;
                EndGame();
            }
        }

        matchTimeLeftText.text = $"Time remaining {Mathf.CeilToInt(_matchTimeLeft)}s";
    }

    private IEnumerator StartCountdown()
    {
        countdownPanel.SetActive(true);
        for (int i = 3; i >= 1; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(1);
        }
        countdownPanel.SetActive(false);
        SpawnTank();
    }
    
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }

    public void EndGame()
    {
        PhotonNetwork.LeaveRoom();
        DontDestroyOnLoad(GameObject.Find("Match Winner"));
        MatchWinner.Instance.winnerName = MatchManager.Instance.GetWinnerName();
        SceneManager.LoadScene("End Match");
    }

    public void SetTankColor(TankColor color)
    {
        AssignedTankColor = color;
    }

    public void SpawnTank()
    {
        GameObject tankGO;

        switch (AssignedTankColor)
        {
            case TankColor.Bronze:
                tankGO = PhotonNetwork.Instantiate(bronzeTankPrefab.name, new Vector3(9f, 10f, 0f), bronzeTankPrefab.transform.rotation, 0);
                //tankGO.GetComponentInChildren<Canvas>().GetComponentInChildren<TextMeshProUGUI>().text = PhotonNetwork.NickName;
                break;
            case TankColor.Gold:
                tankGO = PhotonNetwork.Instantiate(goldTankPrefab.name, new Vector3(-7f, 1f, 0f), Quaternion.identity, 0);
                //tankGO.GetComponentInChildren<Canvas>().GetComponentInChildren<TextMeshProUGUI>().text = PhotonNetwork.NickName;
                break;
            case TankColor.Turquoise:
                tankGO = PhotonNetwork.Instantiate(turquoiseTankPrefab.name, new Vector3(9f, -2.5f, 0f), Quaternion.identity, 0);
                //tankGO.GetComponentInChildren<Canvas>().GetComponentInChildren<TextMeshProUGUI>().text = PhotonNetwork.NickName;
                break;
            case TankColor.Blue:
                tankGO = PhotonNetwork.Instantiate(blueTankPrefab.name, new Vector3(-7f, -8f, 0f), Quaternion.identity, 0);
                //tankGO.GetComponentInChildren<Canvas>().GetComponentInChildren<TextMeshProUGUI>().text = PhotonNetwork.NickName;
                break;
            case TankColor.NoFreeColor:
            default:
                Debug.LogError("NoFreeColor or invalid color. No tank spawned.");
                tankGO = null;
                break;
        }

        _myTank = tankGO;
    }

    public void Respawn()
    {
        PhotonNetwork.Destroy(_myTank);
        _myTank = null;
        StartCoroutine(StartCountdown());
    }

    public void UpdateLeaderboardUI(List<KeyValuePair<string, float>> sortedKDLeaderboard)
    {
        for (int i = 0; i < sortedKDLeaderboard.Count; i++)
        {
            var playerName = sortedKDLeaderboard[i].Key;
            var kd = sortedKDLeaderboard[i].Value;
            var text = $"{sortedKDLeaderboard[i].Key} | {String.Format("{0:0.00}", kd)}";
            leaderboardPositionsText[i].text = text;
        }
    }

    public void StartMatchEndTimer()
    {
        _matchHasStarted = true;
    }
}
