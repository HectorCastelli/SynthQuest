using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float speed = 10f;
    public float airspeed = 0.2f;
    public float jumppower = 50f;
    public float secondjumppower = 60f;
    public float charHeight = 1;

    public bool direction = false; //false = left, true = right

    public bool jumped = false;
    public bool touching = false;

    // Check if is touching the ground
    bool isGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, charHeight/2 + 0.1f);
    }

    // Use this for initialization
    void Start () {
        charHeight = this.transform.localScale.y;
}

    // Update is called once per frame
    void Update()
    {
        float x;
        float y = 0;

        //Check for inputs and move
        x = speed * Input.GetAxis("Horizontal");
        //Adapt for different directions
        if (Input.GetAxis("Horizontal") > 0)
        {
            direction = true;
        } else if (Input.GetAxis("Horizontal") < 0)
        {
            direction = false;
        }
        if (direction)
        {
            this.transform.localEulerAngles = new Vector3(0, 50, 0);
        } else
        {
            this.transform.localEulerAngles = new Vector3(0, 120, 0);
        }
        //Check for jump
        if (isGrounded() || touching) jumped = false;
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded() || touching)
            {
                //regular jump
                y = jumppower;
            }
            else if (!jumped)
            {
                //double jump
                y = secondjumppower;
                jumped = true;
            }
        }

        //Apply movement
        this.GetComponent<Rigidbody>().velocity = new Vector3 (x, this.GetComponent<Rigidbody>().velocity.y, 0);
        this.GetComponent<Rigidbody>().AddForce(new Vector3(0, y, 0), ForceMode.Impulse);

    }

    void OnCollisionStay(Collision col)
    {
        touching = true;
    }

    void OnCollisionExit(Collision col)
    {
        touching = false;
    }
}
