using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameScene : MonoBehaviourPunCallbacks, IPunObservable
{
    public Config ConfigData;

    const float FAILED_TAP = float.MinValue;

    public Image stopButton;
    public TextMeshProUGUI stopText;

    public TextMeshProUGUI roundTimeText;
    public TextMeshProUGUI lastTapTimeText;

    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI opponentScoreText;

    public TextMeshProUGUI DELETEME_RNG_SEED;
    public TextMeshProUGUI DELETEME_TIMERTEST;

    int RNGSeed = 0;

    float nextTimer;
    float currentRoundTime;
    float totalTime = 10.0f;//30.0f;
    int roundNumber = 0;

    Dictionary<int, float> playerTimes = new Dictionary<int, float>();
    Dictionary<int, float> opponentTimes = new Dictionary<int, float>();

    int playerPoints = 0;
    int opponentPoints = 0;

    bool isActive;
    private bool hasTapped;

    // Start is called before the first frame update

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            RNGSeed = Random.Range(int.MinValue, int.MaxValue);
            Random.InitState(RNGSeed);

            this.photonView.RPC("syncRNG", RpcTarget.All, RNGSeed);
        }
        isActive = true;
        hasTapped = false;
        stopButton.color = ConfigData.InactiveColor;
        stopText.text = "STOP";


    }

    [PunRPC]
    void syncRNG(int setRNG)
    {
        Debug.Log(string.Format("setting rng to {0}", setRNG.ToString()));
        DELETEME_RNG_SEED.text = setRNG.ToString();
        Random.InitState(setRNG);
        SetNextTime(true);
    }

    // Update is called once per frame
    void Update()
    {
        var deltaTime = Time.deltaTime;
        totalTime -= deltaTime;
        nextTimer -= deltaTime;
        currentRoundTime += deltaTime;

        if (totalTime <= 0.0f)
        {
            EndMatch();
            return;
        }

        roundTimeText.text = Mathf.Max(totalTime, 0.00f).ToString("00.00");

        if (nextTimer <= 0.0f)
        {
            EnableButton();
        }


    }

    private void EnableButton()
    {
        currentRoundTime = 0.0f;
        SetNextTime();
        hasTapped = false;
        stopButton.color = ConfigData.ActiveColor;
        stopText.text = "PRESS";

    }

    private void SetNextTime(bool firstTime = false)
    {
        if (!hasTapped && !firstTime)
        {
            ResolvePlayerRound(true);
        }
        roundNumber++;
        nextTimer = Random.Range(ConfigData.minTurnTime, ConfigData.maxTurnTime);
        DELETEME_TIMERTEST.text = nextTimer.ToString();
    }

    public void TapButton()
    {
        if (!isActive || hasTapped)
        {
            return;
        }

        hasTapped = true;
        stopButton.color = ConfigData.InactiveColor;
        stopText.text = "STOP";

        lastTapTimeText.text = currentRoundTime.ToString("0.00");

        ResolvePlayerRound(false);
    }

    private void ResolvePlayerRound(bool failure)
    {


        if (failure)
        {
            playerTimes[roundNumber - 1] = FAILED_TAP;
            Debug.Log("SETTING FAILED TAP AT POS " + (roundNumber));
            if (opponentTimes.ContainsKey(roundNumber - 1) && opponentTimes[roundNumber - 1] > 0)
            {
                opponentPoints++;
                opponentScoreText.text = opponentPoints.ToString();
            }
            this.photonView.RPC("SyncTaps", RpcTarget.Others, roundNumber, FAILED_TAP);
        }
        else
        {
            playerTimes[roundNumber - 1] = currentRoundTime;
            Debug.Log("SETTING real TAP AT POS " + (roundNumber) + " " + currentRoundTime);
            if (opponentTimes.ContainsKey(roundNumber - 1))
            {
                if (opponentTimes[roundNumber - 1] > currentRoundTime)
                {
                    opponentPoints++;
                    opponentScoreText.text = opponentPoints.ToString();
                }
                else if (currentRoundTime > 0)
                {
                    playerPoints++;
                    playerScoreText.text = playerPoints.ToString();
                }
            }


            this.photonView.RPC("SyncTaps", RpcTarget.Others, roundNumber, currentRoundTime);
        }

    }

    [PunRPC]
    public void SyncTaps(int round, float roundTime)
    {
        opponentTimes[round - 1] = roundTime;

        if (playerTimes.ContainsKey(round - 1))
        {
            if (playerTimes[round - 1] > roundTime)
            {
                playerPoints++;
                playerScoreText.text = playerPoints.ToString();
            }
            else if (roundTime > 0)
            {
                opponentPoints++;
                opponentScoreText.text = opponentPoints.ToString();
            }
        }
    }

    private void EndMatch()
    {
        if (!isActive)
        {
            return;
        }

        stopButton.color = ConfigData.DisabledColor;
        stopText.text = "GAME OVER";
        isActive = false;

        roundTimeText.text = string.Join(System.Environment.NewLine, playerTimes.Values);
        lastTapTimeText.text = string.Join(System.Environment.NewLine, opponentTimes.Values);
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel(0);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
