﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ShipMovement : NetworkBehaviour {


    public float enginesHP = 1.0f; // % decimal, 0-1 (aka .66 is 66% engine hp)
    public float thrust = 1.5f;
    public float torque = 10f;
    public float topSpeed = 10f;
    public float maxAngularVel = 10f;
    public float RAM_POWER = 12f; //testing some changes lool

    public GameObject childUI;

    public Text nameLabelRef;

    private GameObject ownerObjRef;

    private GameObject camRef;

    private Rigidbody2D rb;




    private bool camSet = false;

    private short updateCount = 0;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody2D>();
        gameObject.layer = 10; // networkships
        //  SHIP HAS BEEN CREATED HERE
        //  LINK TO OWNER
        //if(hasAuthority)
        //    LinkToOwner();
        Quaternion rotation = new Quaternion(0.0f, 0.0f, gameObject.transform.rotation.z, 0.0f);
        childUI.transform.rotation = rotation;




    }
	
	// Update is called once per frame
	void FixedUpdate () {
        updateCount++;
        //Vector3 rotation = new Vector3(gameObject.transform.rotation.x, gameObject.transform.rotation.y, gameObject.transform.rotation.z * -1);
        Quaternion rotation = new Quaternion(0.0f, 0.0f, gameObject.transform.rotation.z, 0.0f);
        childUI.transform.rotation = rotation;

        if (hasAuthority)
        {

            if (!camSet)
            {
                Debug.Log("Updates before setting camera: " + updateCount);
                LinkToOwner();
                camSet = true;
                gameObject.layer = 8; //localShip
                updateNamesInit();

            }

            float angle;
            if (Input.GetKey(KeyCode.W))        //  ----------  FORWARD
            {
                angle = rb.rotation;
                Vector3 dir = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.right;
                rb.AddForce(dir * thrust * enginesHP);
            }

            if (Input.GetKey(KeyCode.S))        //  ----------  BACKWARD
            {
                angle = rb.rotation;
                Vector3 dir = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.right;
                rb.AddForce(-dir * thrust * enginesHP);
            }

            if (Input.GetKey(KeyCode.A))        //  ----------  TURN LEFT
            {
                if (rb.angularVelocity < maxAngularVel)
                    rb.AddTorque(torque * enginesHP);
            }

            if (Input.GetKey(KeyCode.D))        //  ----------  TURN RIGHT
            {
                if (rb.angularVelocity > -maxAngularVel)
                    rb.AddTorque(-torque * enginesHP);
            }


            if (Input.GetKeyDown(KeyCode.R))
            {
                updateNamesInit();
            }

            //if (Input.GetKey(KeyCode.F))        //  ----------  TURN RIGHT
            //{
                
            //    LinkToOwner();
            //}
        }


    }
    
    private void updateNamesInit()
    {
        //Debug.Log(gameObject.name + " is requesting playernames from server");

        ownerObjRef.GetComponent<PlayerConnection>().UpdateNamesInit();
    }

    private void LinkToOwner()
    {
        //Debug.Log("PlayerShip is attempting to link to its owner");


        if (hasAuthority)
        {
            //Debug.Log("This playership has authority, attempting link...");♣

            GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");

            bool playerFound = false;

            for (int i = 0; i < allPlayers.Length && !playerFound; i++) // loop through every player
            {
                PlayerConnection playerObj = allPlayers[i].GetComponent<PlayerConnection>();
                //Debug.Log("Iteration of search loop: " + i);
                if (playerObj.isLocalPlayer)
                {
                    //Debug.Log("Player Found!");
                    playerObj.PlayerShipObj = gameObject;  // player object will look at THIS
                    ownerObjRef = allPlayers[i];           // THIS will look at playerobject
                    playerObj.LinkCameraToObj(gameObject);
                    setDisplayName(ownerObjRef.GetComponent<PlayerConnection>().name);
                    CmdSetLinkOnServer(playerObj.netId.Value);
                    playerFound = true;

                }

            }



            // camera look at this object
        }
        
    }

    // --  The Server's computer will have correct references between player and ship
    [Command]
    public void CmdSetLinkOnServer(uint netId)
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        bool playerFound = false;
        for(int i = 0; i < allPlayers.Length && !playerFound; i++)
        {
            PlayerConnection playerObjRef = allPlayers[i].GetComponent<PlayerConnection>();
            if(playerObjRef.netId.Value == netId)
            {
                playerObjRef.PlayerShipObj = gameObject;
                ownerObjRef = allPlayers[i];
            }
        }

    }

    public void setDisplayName(string name)
    {
        CmdSetDisplayName(name);
    }

    [Command]
    void CmdSetDisplayName(string name)
    {
        nameLabelRef.GetComponent<Text>().text = name;
        RpcSetDisplayName(name);
    }

    [ClientRpc]
    void RpcSetDisplayName(string name)
    {
        nameLabelRef.GetComponent<Text>().text = name;
    }
    


    void DestroyThisShip()
    {
        CmdDestroyShip();       //  destroy this object on Server's computer
        Destroy(gameObject);    //  Destroy this object on THIS computer
    }

    [Command]
    void CmdDestroyShip()
    {
        Destroy(gameObject);
    }
}
