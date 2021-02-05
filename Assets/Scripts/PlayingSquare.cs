using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayingSquare : MonoBehaviour
{
    public GameManager.SquareType squareType;
    public int destinationSquare;

    public bool isSpecialSquare;

    public Vector3[] playerPositions;

    // Start is called before the first frame update
    void Start()
    {
        playerPositions = new Vector3[4];

        // Positions of individual player's tokens on a given square
        playerPositions[0] = new Vector3(GetComponent<Renderer>().bounds.size.x / -4f, GetComponent<Renderer>().bounds.size.y /  4f, -0.1f);
        playerPositions[1] = new Vector3(GetComponent<Renderer>().bounds.size.x /  4f, GetComponent<Renderer>().bounds.size.y /  4f, -0.1f);
        playerPositions[2] = new Vector3(GetComponent<Renderer>().bounds.size.x / -4f, GetComponent<Renderer>().bounds.size.y / -4f, -0.1f);
        playerPositions[3] = new Vector3(GetComponent<Renderer>().bounds.size.x /  4f, GetComponent<Renderer>().bounds.size.y / -4f, -0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
