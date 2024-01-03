using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.U2D.Animation;

/// <summary>
/// Cette classe est utilise pour gerer le choix des Personnages, puis la gestions PlayerInput / Device connecte a chaque joueurs.
/// Elle ne seras donc pas detruite aux changement de scene.
/// </summary>
public class PlayerJoinedManager : MonoBehaviour
{
    [SerializeField] GameObject playerChooseCharacterPrefab;
    [SerializeField] GameObject redominoPrefab;

    public List<PlayerInfo> players = new();

    public class PlayerInfo
    {
        public int PlayerIndex;
        public SpriteLibraryAsset SpriteLibrary;
        public InputDevice Device;
        public string ControlScheme;
    }

    //nombre de joeur ayant valider leur choix de personnage
    int playersReady = 0;
    bool isStarted = false;

    PlayerInputActions inputActions;

    List<GameObject> spawns = new();
    List<GameObject> joinMessages = new();

    bool firstKeyboardPlayerJoined = false;
    bool secondKeyboardPlayerJoined = false;

    void Awake()
    {
        //Create a new instance of the PlayerInputActions class to get input from users
        inputActions = new PlayerInputActions();
        inputActions.Player.Action.performed += ctx => Join(ctx);

        spawns.Add(GameObject.Find("SpawnPoint/SpawnP1"));
        spawns.Add(GameObject.Find("SpawnPoint/SpawnP2"));
        spawns.Add(GameObject.Find("SpawnPoint/SpawnP3"));
        spawns.Add(GameObject.Find("SpawnPoint/SpawnP4"));

        joinMessages.Add(GameObject.Find("Canvas/JoinMessage1"));
        joinMessages.Add(GameObject.Find("Canvas/JoinMessage2"));
        joinMessages.Add(GameObject.Find("Canvas/JoinMessage3"));
        joinMessages.Add(GameObject.Find("Canvas/JoinMessage4"));
    }

    void Start()
    {
        //La classe PlayerJoinedManager ne seras pas detruite aux changement de scene.
        DontDestroyOnLoad(gameObject);
    }


    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        inputActions.Player.Enable();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        inputActions.Player.Disable();
    }

    private void Join(InputAction.CallbackContext ctx)
    {
        if (!isStarted && ctx.performed)
        {
            // Determine the control scheme based on the input device by default it's Gamepad
            string controlScheme = "Gamepad";

            if (ctx.control.device is Keyboard keyboard)
            {
                //Todo: a voir si on peut remplacer les constantes par des variables dynamiques
                if (keyboard.spaceKey.isPressed && !firstKeyboardPlayerJoined)
                {
                    controlScheme = "KeyboardMain";
                    firstKeyboardPlayerJoined = true;

                    for (int i = players.Count; i < joinMessages.Count; i++)
                    {
                        joinMessages[i].GetComponent<TextMeshProUGUI>().text = "PRESS A OR L \n (SECOND PLAYER ON KEYBOARD)";
                    }
                }
                else if (keyboard.lKey.isPressed && firstKeyboardPlayerJoined && !secondKeyboardPlayerJoined)
                {
                    controlScheme = "KeyboardDuo";
                    secondKeyboardPlayerJoined = true;

                    for (int i = players.Count; i < joinMessages.Count; i++)
                    {
                        joinMessages[i].GetComponent<TextMeshProUGUI>().text = "PRESS A";
                    }
                }
                else
                {
                    return;
                }

            }
            else if (players.Any(p => p.Device == ctx.control.device))
            {
                return;
            }

            // Instantiate the playerPrefab with the determined control scheme
            var player = PlayerInput.Instantiate(playerChooseCharacterPrefab, controlScheme: controlScheme, pairWithDevice: ctx.control.device);
            player.transform.position = spawns[player.playerIndex].transform.position;
            player.GetComponent<ChooseCharacterIndividual>().manager = this;
            joinMessages[player.playerIndex].SetActive(false);

            players.Add(new PlayerInfo()
            {
                PlayerIndex = player.playerIndex,
                Device = ctx.control.device,
                ControlScheme = controlScheme
            });
        }
    }


    public void PlayerValidate(int playerIndex, SpriteLibraryAsset spriteLibraryChosen)
    {
        players.Find(p => p.PlayerIndex == playerIndex).SpriteLibrary = spriteLibraryChosen;

        playersReady++;
        if (playersReady == players.Count)
        {
            isStarted = true;

            //Trigger Animation before start
            StartCoroutine(StartLevelAfterDelay());
        }
    }

    IEnumerator StartLevelAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        GameObject.FindGameObjectWithTag("LevelLoader").GetComponentInChildren<Animator>().SetTrigger("End");

        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("ChooseLvl");
    }

    #region On load level character and playerInput management

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Todo : voir pour trouver mieux pour filtrer sur uniquement les levels ou l'on doit instancier des joueurs
        if (SceneManager.GetActiveScene().name.Contains("Level"))
        {
            var spawns = GameObject.FindGameObjectsWithTag("Spawns");
            foreach (PlayerInfo playerInfo in players)
            {
                //On instancie un nouveau prefab du joueur lié au bon controller et au bon schema
                var player = PlayerInput.Instantiate(redominoPrefab, controlScheme: playerInfo.ControlScheme, pairWithDevice: playerInfo.Device);
                player.transform.position = spawns[player.playerIndex].transform.position;
                player.GetComponent<SpriteLibrary>().spriteLibraryAsset = playerInfo.SpriteLibrary;
            }
        }
    }
    #endregion

}
