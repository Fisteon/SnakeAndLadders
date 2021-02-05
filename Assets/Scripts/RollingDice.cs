using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingDice : MonoBehaviour
{
    public Camera diceCamera;
    public int dieResult;

    public bool currentlyRolling;

    // Start is called before the first frame update
    void Start()
    {
        currentlyRolling = false;
        dieResult = -1;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.GetComponent<Rigidbody>().velocity.magnitude == 0)
        {
            dieResult = GetDieResult();
            if (dieResult != -1) diceCamera.enabled = false;
            currentlyRolling = false;
        }
    }

    public void RollTheDie()
    {
        diceCamera.enabled = true;
        currentlyRolling = true;

        this.transform.localPosition = new Vector3(0, 1.5f, 0);
        int directionX = Random.Range(0, 2) == 0 ? -1 : 1;
        int directionZ = Random.Range(0, 2) == 0 ? -1 : 1;
        GetComponent<Rigidbody>().velocity = new Vector3(0f, 0.01f, 0f);

        GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(500f, 1000f) * directionX, 0f, Random.Range(500f, 1000f) * directionZ));
        GetComponent<Rigidbody>().AddTorque(new Vector3(Random.Range(1f, 3f), Random.Range(1f, 3f), Random.Range(1f, 3f)), ForceMode.Impulse);
        
    }

    // Checks if which one of the cube's sides is looking "up"
    int GetDieResult()
    {
        if (Vector3.Distance(this.transform.up, Vector3.up) < 0.1f)
        {
            return 6;
        }
        else if (Vector3.Distance(this.transform.up, Vector3.down) < 0.1f)
        {
            return 1;
        }
        else if (Vector3.Distance(this.transform.forward, Vector3.up) < 0.1f)
        {
            return 5;
        }
        else if (Vector3.Distance(this.transform.forward, Vector3.down) < 0.1f)
        {
            return 2;
        }
        else if (Vector3.Distance(this.transform.right, Vector3.up) < 0.1f)
        {
            return 4;
        }
        else if (Vector3.Distance(this.transform.right, Vector3.down) < 0.1f)
        {
            return 3;
        }
        else
        {
            Debug.Log("Die wasn't straight");
            return -1;
        }
        /*
         * This doesn't work if there's a slightest mismatch, so the "approximate"
         * version is used instead, it should always be correct
         * 
        if (this.transform.up == Vector3.up)
        {
            //Debug.Log("6");
            return 6;
        }
        else if (this.transform.up == Vector3.down)
        {
            //Debug.Log("1");
            return 1;
        }
        else if (this.transform.forward == Vector3.up)
        {
            //Debug.Log("5");
            return 5;
        }
        else if (this.transform.forward == Vector3.down)
        {
            //Debug.Log("2");
            return 2;
        }
        else if (this.transform.right == Vector3.up)
        {
            //Debug.Log("4");
            return 4;
        }
        else if (this.transform.right == Vector3.down)
        {
            //Debug.Log("3");
            return 3;
        }
        else
        {
            Debug.Log("ERROR ROLLING DICE!");
            return -1;
        }*/
    }
}
