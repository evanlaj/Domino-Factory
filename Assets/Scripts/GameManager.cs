using Enums;
using Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using static GameManager;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int life = 3;

    // Block Sprite Management
    [SerializeField] private Sprite defaultBlockSprite;
    [SerializeField] private Sprite redBlockSprite;
    [SerializeField] private Sprite greenBlockSprite;
    [SerializeField] private Sprite blueBlockSprite;
    [SerializeField] private Sprite yellowBlockSprite;
    [SerializeField] private Sprite cyanBlockSprite;
    [SerializeField] private Sprite purpleBlockSprite;
    [SerializeField] private Sprite blackBlockSprite;
    [SerializeField] private Sprite lightBlueBlockSprite;
    [SerializeField] private Sprite lightRedBlockSprite;

    private static readonly int spriteSize = 512;
    private static readonly int blockPixelSize = spriteSize / 32;
    private static readonly int blockSizeY = 6 * blockPixelSize;
    private static readonly int blockSizeX = 7 * blockPixelSize;

    private static readonly int blockSideSize = 5 * blockPixelSize;
    private static readonly int fullBlockHeight = blockSizeY + blockSideSize;

    // private Color blueOutline = new Color(27 / 255f, 33 / 255f, 114 / 255f, 1);
    // private Color lightBlueOutline = new Color(51 / 255f, 57 / 255f, 132 / 255f, 1);
    // private Color redOutline = new Color(82 / 255f, 1 / 255f, 1 / 255f, 1);
    // private Color lightRedOutline = new Color(117 / 255f, 11 / 255f, 11 / 255f, 1);
    // private Color defaultOutline = new Color(24 / 255f, 24 / 255f, 24 / 255f, 1);
    // private Color blackOutline = Color.black;

    // Domino Request Management

    private static readonly float timeToReachMinimumRequestDuration = 180;


    //Dictionnaire contenants les �v�nements ainsi que les timers 
    [Serializable]
    public struct FactoryEvent
    {
        public float timer;
        //Todo: essayer de passer � une r�f�rence de script plutot que via un prefab
        public GameObject eventToStart;
        public bool loop;
    }
    private static readonly float minUpperBound = 20f;
    private static readonly float initialUpperBound = 32f;
    private static readonly float minLowerBound = 8f;
    private static readonly float initialLowerBound = 16f;


    private static readonly float initialDominoRequestDuration = 40f;
    private static readonly float minDominoRequestDuration = 30f;

    [SerializeField] private FactoryEvent[] events;

    private static readonly float timeToReachMinimumDelayBetweenRequests = 180;

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

    // METHODS

    void Start()
    {
        hearthManager = FindObjectOfType<HearthManager>().GetComponent<HearthManager>();

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
                CameraShake.Instance.MediumShake();
                hearthManager.LifeChanged(life);
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

    public Sprite GenerateDominoSprite(Domino domino)
    {
        var minArea = DominoUtils.GetMinimumDominoArea(domino);

        int dominoPixelHeight = (minArea.Blocks.Length + 1) * blockSizeY;
        int dominoPixelWidth = minArea.Blocks[0].Length * blockSizeX;

        int dominoPaddingLeft = (int)Mathf.Floor((spriteSize - dominoPixelWidth) / 2);
        int dominoPaddingBottom = (int)Mathf.Floor((spriteSize - dominoPixelHeight) / 2);

        Resources.UnloadUnusedAssets();
        Color transparent = new Color(0, 0, 0, 0);

        var newTexture = new Texture2D(spriteSize, spriteSize);

        // INIT BACKGROUND

        for (var x = 0; x < spriteSize; x++)
            for (var y = 0; y < spriteSize; y++)
                newTexture.SetPixel(x, y, transparent);

        // DRAW BLOCKS

        // for (var x = 0; x < dominoPixelWidth; x++)
        // {
        //     for (var y = 0; y < dominoPixelHeight; y++)
        //     {
        //         int blockXPos = (int) Mathf.Floor(x / blockSizeX);
        //         int blockYPos = (int) Mathf.Floor(y / blockSizeY);

        //         int xPosInsideCell = x % blockSizeX;
        //         int yPosInsideCell = y % blockSizeY;

        //         Sprite blockSprite;
        //         Color pixelColor = transparent;


        //         // Draw bottom side
        //         if (
        //           blockYPos == minArea.Blocks.Length ||
        //           blockXPos == minArea.Blocks[blockYPos].Length ||
        //           !minArea.Blocks[blockYPos][blockXPos].Exists
        //         )
        //         {
        //             var previousBlockYPos = blockYPos - 1;

        //             if (
        //               previousBlockYPos < 0 ||
        //               !minArea.Blocks[previousBlockYPos][blockXPos].Exists ||
        //               yPosInsideCell >= 5 * blockPixelSize
        //             ) continue;

        //             blockSprite = GetSpriteFromColor(minArea.Blocks[previousBlockYPos][blockXPos].Color);

        //             pixelColor = blockSprite.texture.GetPixel(xPosInsideCell, fullBlockHeight - blockSizeY - 1 - yPosInsideCell);
        //             if(pixelColor != transparent)
        //               newTexture.SetPixel(x + dominoPaddingLeft, spriteSize - 1 - dominoPaddingBottom - y, pixelColor);

        //             continue;
        //         }


        //         // Draw bottom side behind current block
        //         if(yPosInsideCell <= 1 * blockPixelSize) {
        //             var previousBlockYPos = blockYPos - 1;

        //             if (
        //               previousBlockYPos < 0 ||
        //               !minArea.Blocks[previousBlockYPos][blockXPos].Exists
        //             ) {}
        //             else {
        //               blockSprite = GetSpriteFromColor(minArea.Blocks[previousBlockYPos][blockXPos].Color);

        //               pixelColor = blockSprite.texture.GetPixel(xPosInsideCell, fullBlockHeight - blockSizeY - 1 - yPosInsideCell);
        //               if(pixelColor != transparent)
        //                 newTexture.SetPixel(x + dominoPaddingLeft, spriteSize - 1 - dominoPaddingBottom - y, pixelColor);
        //             }

        //         }

        //         blockSprite = GetSpriteFromColor(minArea.Blocks[blockYPos][blockXPos].Color);

        //         pixelColor = blockSprite.texture.GetPixel(xPosInsideCell, fullBlockHeight - 1 - yPosInsideCell);
        //         if(pixelColor != transparent) 
        //           newTexture.SetPixel(x + dominoPaddingLeft, spriteSize - 1 - dominoPaddingBottom - y, pixelColor);
        //     }
        // }

        for (var blockY = 0; blockY < minArea.Blocks.Length; blockY++)
        {
            for (var blockX = 0; blockX < minArea.Blocks[blockY].Length; blockX++)
            {

                if (!minArea.Blocks[blockY][blockX].Exists) continue;

                var blockSprite = GetSpriteFromColor(minArea.Blocks[blockY][blockX].Color);

                for (var x = 0; x < blockSizeX; x++)
                {
                    for (var y = 0; y < blockSizeY; y++)
                    {
                        var pixelColor = blockSprite.texture.GetPixel(x, fullBlockHeight - 1 - y);
                        if (pixelColor.a == 0) continue;

                        if (pixelColor.a == 1)
                            newTexture.SetPixel(x + dominoPaddingLeft + blockX * blockSizeX, spriteSize - 1 - dominoPaddingBottom - blockY * blockSizeY - y, pixelColor);
                        else
                        { // if the texture is semi-transparent, we need to merge the colors instead of replacing them
                            var mergedColor = Color.Lerp(newTexture.GetPixel(x + dominoPaddingLeft + blockX * blockSizeX, spriteSize - 1 - dominoPaddingBottom - blockY * blockSizeY - y), pixelColor, pixelColor.a);
                            newTexture.SetPixel(x + dominoPaddingLeft + blockX * blockSizeX, spriteSize - 1 - dominoPaddingBottom - blockY * blockSizeY - y, mergedColor);
                        }
                    }
                }

                for (var x = 0; x < blockSizeX; x++)
                {
                    for (var y = 0; y < blockSideSize; y++)
                    {
                        var pixelColor = blockSprite.texture.GetPixel(x, fullBlockHeight - blockSizeY - 1 - y);
                        if (pixelColor.a != 0)
                            newTexture.SetPixel(x + dominoPaddingLeft + blockX * blockSizeX, spriteSize - 1 - dominoPaddingBottom - (blockY + 1) * blockSizeY - y, pixelColor);
                    }
                }
            }
        }

        // CONFIG TEXTURE & SPRITE

        newTexture.filterMode = FilterMode.Point;
        newTexture.wrapMode = TextureWrapMode.Clamp;

        newTexture.Apply();

        var finalSprite = Sprite.Create(newTexture, new Rect(0, 0, spriteSize, spriteSize), new Vector2(0.5f, 0.5f), spriteSize);
        finalSprite.name = "DominoSprite";
        return finalSprite;
    }

    private Sprite GetSpriteFromColor(BlockColor color)
    {
        switch (color)
        {
            case BlockColor.Red:
                Debug.Log("red");
                return redBlockSprite;
            case BlockColor.Green:
                Debug.Log("green");
                return greenBlockSprite;
            case BlockColor.Blue:
                Debug.Log("blue");
                return blueBlockSprite;
            case BlockColor.Purple:
                Debug.Log("purple");
                return purpleBlockSprite;
            case BlockColor.Yellow:
                Debug.Log("yellow");
                return yellowBlockSprite;
            case BlockColor.Cyan:
                Debug.Log("cyan");
                return cyanBlockSprite;
            case BlockColor.Failed:
                Debug.Log("black");
                return blackBlockSprite;
            default:
                Debug.Log("white");
                return defaultBlockSprite;
        }
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
