using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float speed = 10f;
    public float airspeed = 0.2f;
    public float jumppower = 50f;
    public float secondjumppower = 60f;
    public float charHeight = 1;

    public GameObject attackThingy;

    public bool direction = false; //false = left, true = right

    private bool jumped = false;

    // Check if is touching the ground
    bool isGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, charHeight + 0.1f);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        float x;
        //Take inputs for movement
        if (isGrounded())
        {
            jumped = false;
            x = speed * Input.GetAxis("Horizontal") * Time.deltaTime;
        }
        else
            x = airspeed * Input.GetAxis("Horizontal") * Time.deltaTime;
        float y = 0;
        //Check for jumps!
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded())
            {
                y = jumppower;
            } else if (jumped == false)
            {
                jumped = true;
                y = secondjumppower;
            }
        }
        if (x<0)
        {
            direction = false;
        } else if (x>0)
        {
            direction = true;
        }
        if (direction)
        {
            this.transform.localEulerAngles = new Vector3(0, 0, 0);
        }
        else
        {
            this.transform.localEulerAngles = new Vector3(0, 180, 0);
            x *= -1;
        }



        this.transform.Translate(x, 0, 0f);
        this.GetComponent<Rigidbody>().AddForce(0, y, 0, ForceMode.Impulse);

        //Check for attacks
        if (Input.GetButton("Fire1"))
        {
            attackThingy.SetActive(true);
        } else
            attackThingy.SetActive(false);

    }

}
