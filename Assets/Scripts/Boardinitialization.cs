using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

public class Boardinitialization : MonoBehaviour
{
    public Canvas canvas;
    public GameObject _square;
    public Text _squareText;

    [Header("Snakes and ladders settings")]
    [Space(10)]
    public int snakeLadderDensity;
    [Range(25, 50)]
    public int MAX_JUMP_FALL;

    [Header("Sprites")]
    [Space(10)]
    public SpriteShapeController ladder;
    public SpriteShapeController snake;
    [Space(10)]
    public GameObject skipTurn;
    public GameObject doubleRoll;

    Vector2 screenOffset;
    const int LOWER_SNAKESLADDERS_OFFSET = 5;
    const int UPPER_SNAKESLADDERS_OFFSET = 17;

    #region Colors
    Color color_0 = new Color(0.588f, 1f, 1f);
    Color color_16 = new Color(0.588f, 0.588f, 1f);
    Color color_33 = new Color(1f, 0.588f, 1f);
    Color color_50 = new Color(1f, 0.588f, 0.588f);
    Color color_66 = new Color(1f, 1f, 0.588f);
    Color color_83 = new Color(0.588f, 1f, 0.588f);

    Gradient gradient;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        #region GradientPopulation
        gradient = new Gradient();
        GradientColorKey[] colorKey = new GradientColorKey[7];
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[7];
        for (int i = 0; i < 7; i++)
        {
            alphaKey[i].alpha = 1.0f;
            alphaKey[i].time = i / 6f;
        }

        colorKey[0].color = color_0;
        colorKey[0].time = 0f;

        colorKey[1].color = color_16;
        colorKey[1].time = 0.166f;

        colorKey[2].color = color_33;
        colorKey[2].time = 0.333f;

        colorKey[3].color = color_50;
        colorKey[3].time = 0.5f;

        colorKey[4].color = color_66;
        colorKey[4].time = 0.666f;

        colorKey[5].color = color_83;
        colorKey[5].time = 0.833f;

        colorKey[6].color = color_0;
        colorKey[6].time = 1f;

        gradient.SetKeys(colorKey, alphaKey);
        #endregion GradientPopulation

        for (int row = -5; row < 5; row++)
        {
            for (int column = -5; column < 5; column++)
            {
                CreateSquare(row, column);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializeBoard()
    {
        CreateSpecialSquares();
    }

    void CreateSquare(int row, int column)
    {
        // Instantiation and naming
        PlayingSquare square = Instantiate(_square, this.transform.GetChild(0)).GetComponent<PlayingSquare>();
        int squareNumber = (row + 5) * 10 + column + 5;
        square.name = "PlayingSquare_" + squareNumber.ToString();

        // Positioning
        int direction = Mathf.Abs(row) % 2 == 0 ? -1 : 1;
        square.transform.position = new Vector2(
            square.GetComponent<Renderer>().bounds.size.x * direction * (column + (direction == -1 ? 1 : 0)) + square.GetComponent<Renderer>().bounds.size.x / 2f,
            square.GetComponent<Renderer>().bounds.size.y * row + square.GetComponent<Renderer>().bounds.size.y / 2f);

        square.GetComponent<SpriteRenderer>().color = NextColor(squareNumber);

        GameManager.GM.squares.Add(square);
    }

    public void CreateSpecialSquares()
    {
        int startingSquare = 0;
        int destinationSquare;
        int cutoff = 90;
        int safetyCounter = 0; // In case board layout turns out in a way that it's impossible to find a legal landing spot
                               // there will be an infinite loop. SafetyCounter will break the loop after a reasonable amount
                               // of attempts to find the legal square has been made.

        snakeLadderDensity = Random.Range(6, 9);
        MAX_JUMP_FALL = Random.Range(25, 50);

        // Create Ladders
        for (int i = 0; i < snakeLadderDensity; i++) 
        {
            safetyCounter = 0;
            do
            {
                startingSquare = Random.Range(0, 90);
            } while (GameManager.GM.squares[startingSquare].squareType != GameManager.SquareType.Normal && startingSquare != cutoff);

            do
            {
                destinationSquare = startingSquare + Random.Range(10, Mathf.Min(MAX_JUMP_FALL, 100 - startingSquare));
            } while (GameManager.GM.squares[destinationSquare].isSpecialSquare && safetyCounter < 500);

            if (safetyCounter < 500)
            {
                GenerateSnakesAndLadders(ladder, startingSquare, destinationSquare, GameManager.SquareType.Ladder);
            }
        }

        startingSquare = 99;
        cutoff = 11;

        // Create Snakes
        for (int i = 0; i < snakeLadderDensity; i++)
        {
            safetyCounter = 0;
            do
            {
                startingSquare = Random.Range(10, 100);
            } while (GameManager.GM.squares[startingSquare].squareType != GameManager.SquareType.Normal && startingSquare != cutoff);


            if (startingSquare <= cutoff) break;

            do
            {
                destinationSquare = startingSquare - Random.Range(10, Mathf.Min(MAX_JUMP_FALL, startingSquare + 1));
                safetyCounter++;
            } while (GameManager.GM.squares[destinationSquare].isSpecialSquare && safetyCounter < 500);

            if (safetyCounter < 500)
            {
                GenerateSnakesAndLadders(snake, startingSquare, destinationSquare, GameManager.SquareType.Snake);
            }
        }

        // Create Skip Turns
        for (int i = 0; i < 3; i++)
        {
            do
            {
                startingSquare = Random.Range(0, 99);
            } while (GameManager.GM.squares[startingSquare].isSpecialSquare);

            GameManager.GM.squares[startingSquare].squareType = GameManager.SquareType.SkipTurn;
            Instantiate(skipTurn, GameManager.GM.squares[startingSquare].transform.position, Quaternion.identity, this.transform.GetChild(1));
        }

        // Create Double Roll
        for (int i = 0; i < 3; i++)
        {
            do
            {
                startingSquare = Random.Range(0, 99);
            } while (GameManager.GM.squares[startingSquare].isSpecialSquare) ;

            GameManager.GM.squares[startingSquare].squareType = GameManager.SquareType.DoubleTurn;
            Instantiate(doubleRoll, GameManager.GM.squares[startingSquare].transform.position, Quaternion.identity, this.transform.GetChild(1));
        }
    }

    // Creates Ladder and Snake sprites
    // Uses Sprite Shapes to seamlessly extend them from starting to finishing square
    void GenerateSnakesAndLadders(SpriteShapeController snakeLadderSprite, int startingSquare, int destinationSquare, GameManager.SquareType type)
    {
        GameManager.GM.squares[startingSquare].squareType = type;
        GameManager.GM.squares[destinationSquare].squareType = GameManager.SquareType.LandingSquare;

        GameManager.GM.squares[startingSquare].isSpecialSquare = true;
        GameManager.GM.squares[startingSquare].destinationSquare = destinationSquare;
        GameManager.GM.squares[GameManager.GM.squares[startingSquare].destinationSquare].isSpecialSquare = true;

        SpriteShapeController s = Instantiate(snakeLadderSprite, this.transform.GetChild(1));
        s.name = "SnakeLadder_" + startingSquare.ToString();
        s.spline.SetPosition(1, GameManager.GM.squares[startingSquare].transform.position);
        s.spline.SetPosition(0, GameManager.GM.squares[GameManager.GM.squares[startingSquare].destinationSquare].transform.position);
    }

    // Progressively changes background color of the squares
    Color NextColor(int squareNumber)
    {
        return gradient.Evaluate(squareNumber / 100f);
    }
}
