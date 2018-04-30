using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bouncy : MonoBehaviour {
    
    public float speed = 0.5f;
    float xVelocity = 0;
    float yVelocity = 0;
    float zVelocity = 0;
    public bool selected = false;
    Vector3 previousPos;
	// Use this for initialization
	void Start () {
        xVelocity = Random.Range(0, speed);
        yVelocity = Mathf.Sqrt(speed * speed - xVelocity * xVelocity);
        GetComponent<Rigidbody>().velocity = new Vector3(xVelocity, yVelocity, zVelocity);
	}
	
	// Update is called once per frame
	void Update () {
    }
    void LateUpdate()
    {
        previousPos = GetComponent<Transform>().position;
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Ball" || other.gameObject.tag == "Target")
        {
            var previous = GetComponent<Rigidbody>().velocity;
            GetComponent<Rigidbody>().velocity = new Vector3(-previous.x, -previous.y, previous.z);
        }
        else if (other.gameObject.tag == "Top" || other.gameObject.tag == "Bottom")
        {
            var previous = GetComponent<Rigidbody>().velocity;
            GetComponent<Rigidbody>().velocity = new Vector3(previous.x, -previous.y, previous.z);
        }
        else if (other.gameObject.tag == "Left" || other.gameObject.tag == "Right")
        {
            var previous = GetComponent<Rigidbody>().velocity;
            GetComponent<Rigidbody>().velocity = new Vector3(-previous.x, previous.y, previous.z);
            
        }
        GetComponent<Transform>().position = previousPos;

    }

    
}
