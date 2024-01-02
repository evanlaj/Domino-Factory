using System.Collections.Generic;
using UnityEngine;
using Utils;
using Models;
using System;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int life = 3;

    // Domino Request Management

    private static readonly float initialDominoRequestDuration = 42f;
    private static readonly float minDominoRequestDuration = 34f;

    public float dominoRequestDuration = initialDominoRequestDuration;

    private static readonly float timeToReachMinimumRequestDuration = 900f;

    private static readonly float minUpperBound = 20f;
    private static readonly float initialUpperBound = 32f;
    private static readonly float minLowerBound = 8f;
    private static readonly float initialLowerBound = 16f;

    private static readonly float timeToReachMinimumDelayBetweenRequests = 600f;

    private float delayBetweenRequestsLowerBound = initialLowerBound;
    private float delayBetweenRequestsUpperBound = initialUpperBound;

    // UI Management

    [SerializeField] private GameObject requestPrefab;

    // [SerializeField] private GameObject HudCanvas;

    private List<GameObject> hudRequestList;

    private List<TetrisPlayer> playerList;

    public List<DominoRequest> dominoRequestList;

    float timeSinceLastBlockRequest;
    float timeBeforeNextBlockRequest;

    HearthManager hearthManager;
    private int score;

    private bool gameInProgress = true;

    //structure contenants les evenements ainsi que les timers 
    [Serializable]
    public struct FactoryEvent
    {
        public float timer;
        //Todo: essayer de passer par une reference de script plutot que via un prefab
        public GameObject eventToStart;
        public bool loop;
    }

    [SerializeField] private FactoryEvent[] events;

    // METHODS

    void Start()
    {
        //Todo : spawn players

        // hearthManager = FindObjectOfType<HearthManager>().GetComponent<HearthManager>();

        // HudCanvas = GameObject.Find("HUD");

        timeSinceLastBlockRequest = 0f;

        dominoRequestList = new List<DominoRequest>();
        hudRequestList = new List<GameObject>();

        playerList = GetRandomPlayers();


        foreach (FactoryEvent f in events)
        {
            StartCoroutine(StartEvent(f));
        }
    }


    private IEnumerator StartEvent(FactoryEvent factoryEvent)
    {
        yield return new WaitForSeconds(factoryEvent.timer);
        factoryEvent.eventToStart.GetComponent<IEvent>().StartEvent();

        if (factoryEvent.loop)
            StartCoroutine(StartEvent(factoryEvent));
    }

    void FixedUpdate()
    {
        if (gameInProgress)
        {
            // if(hearthManager.GetCurrentScore() != score) 
            //   hearthManager.ScoreChanged(score);
            DecreaseDominoRequestTimeList();
            CheckForNewDominoRequest();
            DeleteUnsuccessfulDominoRequests();

            CalculateNewDurations();
        }
    }

    private void DecreaseDominoRequestTimeList()
    {
        for (var i = 0; i < dominoRequestList.Count; i++)
            dominoRequestList[i].RemainingTime -= Time.fixedDeltaTime;
    }

    private void CheckForNewDominoRequest()
    {
        timeSinceLastBlockRequest += Time.fixedDeltaTime;

        // Skip if there are already 10 requests or no players left
        if (dominoRequestList.Count >= 10 || playerList.Count == 0)
        {
            timeSinceLastBlockRequest = 0f;
            return;
        }

        if (timeBeforeNextBlockRequest < timeSinceLastBlockRequest || dominoRequestList.Count == 0)
            AddRandomDominoRequest();
    }

    private void DeleteUnsuccessfulDominoRequests()
    {
        for (var i = dominoRequestList.Count - 1; i > -1; i--)
        {
            if (dominoRequestList[i].RemainingTime < 0)
            {
                life--;
                // hearthManager.LifeChanged(life);
                if (life <= 0)
                {
                    EndLevel();
                    return;
                }

                DeleteDominoRequest(i);
            }
        }
    }

    void EndLevel()
    {
        gameInProgress = false;
        for (var i = dominoRequestList.Count - 1; i > -1; i--)
        {
            DeleteDominoRequest(i);
        }
        EndMenuManager menu = FindObjectOfType<EndMenuManager>().GetComponent<EndMenuManager>();
        menu.EndLevel(score);

    }

    public void DeleteDominoRequest(int i)
    {
        var player = dominoRequestList[i].Player;
        playerList.Add(player);

        dominoRequestList.RemoveAt(i);

        Destroy(hudRequestList[i]);
        hudRequestList.RemoveAt(i);

        for (var j = i; j < hudRequestList.Count; j++)
        {
            var requestRectTransform = hudRequestList[j].GetComponent<RectTransform>();
            requestRectTransform.anchoredPosition = new Vector2(56 + 166 * j, 0);
        }
    }

    private void AddRandomDominoRequest()
    {
        timeSinceLastBlockRequest = 0f;
        timeBeforeNextBlockRequest = UnityEngine.Random.Range(delayBetweenRequestsLowerBound, delayBetweenRequestsUpperBound);

        var playerIndex = UnityEngine.Random.Range(0, playerList.Count);
        var player = playerList[playerIndex];
        playerList.RemoveAt(playerIndex);

        var dominoRequest = new DominoRequest()
        {
            Blocks = DominoUtils.GetRandomValidDomino(),
            Color = DominoUtils.GetRandomColor(),
            Player = player,
            InitialDuration = dominoRequestDuration,
            RemainingTime = dominoRequestDuration,
        };

        dominoRequestList.Add(dominoRequest);
        AddDominoRequestToHUD(dominoRequest);
    }

    private void AddDominoRequestToHUD(DominoRequest dominoRequest)
    {
        var requestGameObject = Instantiate(requestPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        // requestGameObject.transform.SetParent(HudCanvas.transform, false);

        var requestRectTransform = requestGameObject.GetComponent<RectTransform>();
        requestRectTransform.anchoredPosition = new Vector2(56 + 166 * (dominoRequestList.Count - 1), 0);

        var requestBehavior = requestGameObject.GetComponent<RequestBehavior>();
        requestBehavior.SetDominoRequest(dominoRequest);

        hudRequestList.Add(requestGameObject);
    }

    private List<TetrisPlayer> GetRandomPlayers()
    {
        var playerList = new List<TetrisPlayer>();

        for (var i = 0; i < 10; i++)
        {
            var player = new TetrisPlayer()
            {
                Name = RequestUtils.GetRandomPlayerName(),
                Age = RequestUtils.GetRandomPlayerAge(),
            };

            playerList.Add(player);
        }

        return playerList;
    }

    private void CalculateNewDurations()
    {
        var dominoRequestDelta = (initialDominoRequestDuration - minDominoRequestDuration) * Time.fixedDeltaTime / timeToReachMinimumRequestDuration;
        dominoRequestDuration -= dominoRequestDelta;

        var delayBetweenRequestsDelta = (initialUpperBound - minUpperBound) * Time.fixedDeltaTime / timeToReachMinimumDelayBetweenRequests;
        delayBetweenRequestsUpperBound -= delayBetweenRequestsDelta;

        delayBetweenRequestsDelta = (initialLowerBound - minLowerBound) * Time.fixedDeltaTime / timeToReachMinimumDelayBetweenRequests;
        delayBetweenRequestsLowerBound -= delayBetweenRequestsDelta;
    }

    public void GainScore(float result)
    {
        score += 60 + (int)Mathf.Floor(180 * result);
    }

    public void LooseScore()
    {
        score -= 40;
    }

}