using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameObject board;
    public RollingDice dice;

    [Space(10)]
    [Header("Players")]
    public int numberOfPlayers;
    public PlayerManager player;
    List<PlayerManager> players;

    public int currentPlayer;

    // Main Menu UI elements
    [Header("Main Menu")]
    [Space(10)]
    public Canvas canvasMainMenu;
    public Button buttonNewGame;
    public Button buttonContinue;
    public Button buttonExit;
    public Button buttonStartGame;
    public Button buttonBack;
    public Toggle toggleBoardRotation;
    public Slider sliderPlayerNumber;
    public Text textPlayerNumber;
    
    // Ingame UI elements
    [Space(10)]
    [Header("UI Elements")]
    public bool boardRotation;
    public RectTransform container;
    [Space(5)]
    public Button buttonRoll;
    public Button buttonUp;
    public Button buttonDown;
    public TextMeshProUGUI textNotification;
    public TextMeshProUGUI textGameOver;

    // Sprites to change the Roll button with, during the game
    [Space(5)]
    public List<Sprite> diceButtonNumbersSprites;
    public Sprite defaultButtonSprite;

    bool gameOver;

    public enum SquareType{
        Normal,
        SkipTurn,
        DoubleTurn,
        Snake,
        Ladder,
        LandingSquare
    };

    enum PlayerColorNames
    {
        Red,
        Yellow,
        Blue,
        Green
    };
    Color[] playerColors = { Color.red, Color.yellow, Color.blue, Color.green };

    public List<PlayingSquare> squares;

    private static GameManager _GameManager;
    public static GameManager GM
    {
        get
        {
            if (_GameManager == null)
                _GameManager = FindObjectOfType(typeof(GameManager)) as GameManager;
            if (_GameManager == null)
                _GameManager = new GameObject("GameMaster").AddComponent(typeof(GameManager)) as GameManager;
            return _GameManager;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        gameOver = false;
        players = new List<PlayerManager>();
        squares = new List<PlayingSquare>();

        board.gameObject.SetActive(true);

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !dice.currentlyRolling)
        {
            dice.RollTheDie();
        }
    }

    void NewGame()
    {
        // Cleanup
        for (int i = 0; i < squares.Count; i++)
        {
            squares[i].isSpecialSquare = false;
            squares[i].destinationSquare = -1;
            squares[i].squareType = SquareType.Normal;
        }

        for (int i = 0; i < players.Count; i++)
        {
            Destroy(players[i].gameObject);
        }
        players.Clear();

        for (int i = 0; i < board.transform.GetChild(1).childCount; i++)
        {
            Destroy(board.transform.GetChild(1).GetChild(i).gameObject);
        }

        buttonContinue.interactable = true;

        board.GetComponent<Boardinitialization>().InitializeBoard();

        for (int i = 0; i < numberOfPlayers; i++)
        {
            players.Add(Instantiate(player, new Vector3(-6f, -4f + 0.5f * i, 0f), Quaternion.identity));
            players[i].GetComponent<SpriteRenderer>().color = playerColors[i];
        }

        currentPlayer = 0;

        StartCoroutine(StartPlayerTurn());

        canvasMainMenu.gameObject.SetActive(false);
    }

    IEnumerator StartPlayerTurn()
    {
        // Who's turn is it - notification
        textNotification.text = "It's <color=#" + ColorUtility.ToHtmlStringRGB(playerColors[currentPlayer]) + ">" + 
            ((PlayerColorNames)currentPlayer).ToString() + " player's</color> turn!";

        // Reset roll button
        buttonRoll.transform.GetComponentInChildren<Text>().enabled = true;
        buttonRoll.transform.GetComponentInChildren<Text>().text = "ROLL";
        buttonRoll.interactable = true;
        buttonRoll.GetComponent<Image>().sprite = defaultButtonSprite;

        // UI/Board rotation
        if (boardRotation == true)
        {
            board.transform.rotation = Quaternion.Euler(0f, 0f, 90f * currentPlayer);
            container.rotation = Quaternion.Euler(0f, 0f, 90f * currentPlayer);
            textNotification.transform.rotation = Quaternion.Euler(0f, 0f, 90f * currentPlayer);
        }

        if (players[currentPlayer].transform.parent != board.transform.GetChild(1))
        {
            players[currentPlayer].transform.parent = board.transform.GetChild(1);
        }

        if (players[currentPlayer].skipTurn == true)
        {
            // Player misses a turn - notification
            textNotification.text = "<color=#" + ColorUtility.ToHtmlStringRGB(playerColors[currentPlayer]) + ">" +
            ((PlayerColorNames)currentPlayer).ToString() + " player</color> misses a turn!";
            buttonRoll.interactable = false;
            players[currentPlayer].skipTurn = false;

            yield return new WaitForSeconds(2f);

            currentPlayer = (currentPlayer + 1) % numberOfPlayers;

            StartCoroutine(StartPlayerTurn());
        }
    }

    IEnumerator DiceRoll()
    {
        buttonRoll.interactable = false;
        buttonRoll.GetComponentInChildren<Text>().text = "Rolling...";

        dice.RollTheDie();

        // Making sure the dice has started rolling before waiting for it to settle
        yield return new WaitForSeconds(0.15f);
        yield return new WaitUntil(() => dice.currentlyRolling == false);

        // If it lands on an edge, near the walls
        if (dice.dieResult == -1)
        {
            buttonRoll.interactable = true;
            buttonRoll.GetComponentInChildren<Text>().text = "ROLL\n(again)";
            textNotification.text = "How (un)lucky!\nRepeat your roll!";
            yield break;
        }

        buttonRoll.GetComponent<Image>().sprite = diceButtonNumbersSprites[dice.dieResult - 1];
        buttonRoll.transform.GetComponentInChildren<Text>().enabled = false;

        textNotification.text = "Choose direction!";
        EnableDirectionButtons();
    }

    IEnumerator MovePlayer(int direction)
    {
        int landingPosition = players[currentPlayer].currentPosition + dice.dieResult * direction;

        if (direction == 1)
        {
            for (int i = players[currentPlayer].currentPosition; i < landingPosition; i++)
            {
                if (i == -1) i++;
                if (landingPosition == 0)
                {
                    players[currentPlayer].transform.position = squares[0].transform.position + squares[0].playerPositions[currentPlayer];
                    break;
                }
                float elapsed = 0f;
                float duration = 0.2f;

                // Animate player token movement over time, square by square 
                // (in order for it to properly follow the path it should take)
                while (elapsed < duration)
                {
                    players[currentPlayer].transform.position = Vector3.Lerp(squares[i].transform.position + squares[i].playerPositions[currentPlayer],
                                                                             squares[i + 1].transform.position + squares[i + 1].playerPositions[currentPlayer],
                                                                             elapsed / duration);
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }
        }        
        else
        {
            for (int i = players[currentPlayer].currentPosition; i > landingPosition; i--)
            {
                float elapsed = 0f;
                float duration = 0.2f;

                while (elapsed < duration)
                {
                    players[currentPlayer].transform.position = Vector3.Lerp(squares[i].transform.position + squares[i].playerPositions[currentPlayer],
                                                                             squares[i - 1].transform.position + squares[i - 1].playerPositions[currentPlayer],
                                                                             elapsed / duration);
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }
        }

        players[currentPlayer].currentPosition += dice.dieResult * direction;

        // Check the type of the square player landed on, and act accordingly
        switch (squares[players[currentPlayer].currentPosition].squareType)
        {
            case SquareType.Normal:
                break;

            case SquareType.DoubleTurn:
                StartCoroutine(StartPlayerTurn());
                yield break;

            case SquareType.SkipTurn:
                players[currentPlayer].skipTurn = true;
                break;

            // Find the ladder/snake that starts on the current square, then animate
            // player token's movement along the spline of the SpriteShape
            case SquareType.Ladder:
                SpriteShapeController ladder = GameObject.Find("SnakeLadder_" + players[currentPlayer].currentPosition.ToString()).GetComponent<SpriteShapeController>();
                Vector3 start = board.transform.rotation * ladder.spline.GetPosition(1) ;
                Vector3 end = board.transform.rotation * ladder.spline.GetPosition(0);

                float elapsed = 0f;
                float duration = 1.5f;

                while (elapsed < duration)
                {
                    players[currentPlayer].transform.position = Vector3.Lerp(start, end, elapsed / duration);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                players[currentPlayer].currentPosition = squares[players[currentPlayer].currentPosition].destinationSquare;
                players[currentPlayer].transform.position = squares[players[currentPlayer].currentPosition].transform.position + squares[players[currentPlayer].currentPosition].playerPositions[currentPlayer];
                break;

            case SquareType.Snake:
                SpriteShapeController snake = GameObject.Find("SnakeLadder_" + players[currentPlayer].currentPosition.ToString()).GetComponent<SpriteShapeController>();
                start = snake.spline.GetPosition(1);
                end = snake.spline.GetPosition(0);

                elapsed = 0f;
                duration = 1.5f;

                while (elapsed < duration)
                {
                    players[currentPlayer].transform.position = Vector3.Lerp(start, end, elapsed / duration);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                players[currentPlayer].currentPosition = squares[players[currentPlayer].currentPosition].destinationSquare;
                players[currentPlayer].transform.position = squares[players[currentPlayer].currentPosition].transform.position + squares[players[currentPlayer].currentPosition].playerPositions[currentPlayer];
                break;
        }

        // If a player reached the end (square 100) congratulate the winner
        // and return to Main Menu after a celebratory pause
        if (IsGameOver())
        {
            textGameOver.gameObject.SetActive(true);
            textGameOver.text = "Congratulations!\n" +
                                "<color=#" + ColorUtility.ToHtmlStringRGB(playerColors[currentPlayer]) + ">" +
                                 ((PlayerColorNames)currentPlayer).ToString() + " player</color> wins!";
            yield return new WaitForSeconds(8f);
            textGameOver.gameObject.SetActive(true);
            UIButtonPress_MainMenu();
            buttonContinue.interactable = false;
            yield break;
        }

        currentPlayer = (currentPlayer + 1) % numberOfPlayers;
        StartCoroutine(StartPlayerTurn());
    }

    bool IsGameOver()
    {
        return (players[currentPlayer].currentPosition == 99);
    }

    void EnableDirectionButtons()
    {
        if ((players[currentPlayer].currentPosition + dice.dieResult) <= 99)
        {
            buttonUp.interactable = true;
        }
        if ((players[currentPlayer].currentPosition - dice.dieResult) >= 0)
        {
            buttonDown.interactable = true;
        }
    }

    #region MainMenuInteraction

    // Ingame UI
    public void UIButtonPress_Roll()
    {
        StartCoroutine(DiceRoll());
    }

    public void UIButtonPress_Direction(int direction)
    {
        StartCoroutine(MovePlayer(direction));
        buttonUp.interactable = false;
        buttonDown.interactable = false;
    }

    public void UIButtonPress_MainMenu()
    {
        canvasMainMenu.gameObject.SetActive(true);
    }

    // Main Menu UI
    public void UIButtonPress_Continue()
    {
        canvasMainMenu.gameObject.SetActive(false);
    }

    public void UIButtonPress_NewGame()
    {
        ToggleMainMenu_Options(false);
    }

    public void UIButtonPress_PlayerSlider()
    {
        textPlayerNumber.text = sliderPlayerNumber.value.ToString();
    }

    public void UIButtonPress_ToggleRotation()
    {
        boardRotation = toggleBoardRotation.isOn;
    }

    public void UIButtonPress_Exit()
    {
        Application.Quit();
    }

    public void UIButtonPress_StartGame()
    {
        ToggleMainMenu_Options(true);
        numberOfPlayers = (int)sliderPlayerNumber.value;
        NewGame();
    }

    public void UIButtonPress_Back()
    {
        ToggleMainMenu_Options(true);
    }

    void ToggleMainMenu_Options(bool menuState)
    {
        buttonContinue.gameObject.SetActive(menuState);
        buttonNewGame.gameObject.SetActive(menuState);
        buttonExit.gameObject.SetActive(menuState);
        toggleBoardRotation.gameObject.SetActive(menuState);
        
        sliderPlayerNumber.gameObject.SetActive(!menuState);
        buttonStartGame.gameObject.SetActive(!menuState);
        buttonBack.gameObject.SetActive(!menuState);
    }

    #endregion
}
