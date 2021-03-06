﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SocketIO;

using SeizeCommand.Attack;
using SeizeCommand.Health;
using SeizeCommand.Movement;
using SeizeCommand.Interactions.Interactors;
using SeizeCommand.Utility;

namespace SeizeCommand.Networking
{
    public class NetworkManager : SocketIOComponent
    {
        [Header("Network Client")]
        [SerializeField] private Transform networkContainer;

        [SerializeField] private GameObject player;

        private Dictionary<string, NetworkIdentity> serverObjects;

        public static string ClientID { get; private set; }

        public override void Start()
        {
            base.Start();
            Initialize();
            SetupEvents();
        }

        public override void Update()
        {
            base.Update();
        }

        public void Play(InputField inputField)
        {
            string ipAddress = inputField.text;
            url = "ws://" + ipAddress + ":52300/socket.io/?EIO=4&transport=websocket";
            CreateWebSocket();
            Connect();
        }

        public void DeactivateMainMenu(GameObject maniMenuPanel)
        {
            maniMenuPanel.SetActive(false);
        }

        public void Respawn()
        {
            Emit("respawn");
        }

        private void Initialize()
        {
            serverObjects = new Dictionary<string, NetworkIdentity>();
        }

        private void SetupEvents()
        {
            On("open", (E) => {
                Debug.Log("Connection Made to the Server");
            });

            On("register", (E) => {
                ClientID = E.data["id"].ToString().Trim('"');

                Debug.LogFormat("Our Client's ID ({0})", ClientID);
            });

            On("spawn", (E) => {
                string id = E.data["id"].ToString().Trim('"');
                float x = E.data["position"]["x"].f;
                float y = E.data["position"]["y"].f;
                float rotation = E.data["rotation"].f;

                Vector3 position = new Vector3(x, y, 0);

                GameObject g = Instantiate(player, position, Quaternion.Euler(0, 0, rotation), networkContainer);
                g.name = string.Format("Player ({0})", id);

                NetworkIdentity ni = g.GetComponent<NetworkIdentity>();
                ni.SetControllerID(id);
                ni.SetSocketReference(this);

                serverObjects.Add(id, ni);

                if(ni.IsLocalPlayer)
                {
                    Camera mainCamera = Camera.main;
                    CameraFollowPlayer cameraFollowPlayer = mainCamera.GetComponent<CameraFollowPlayer>();
                    cameraFollowPlayer.SetFollowPlayer(ni.transform);
                }
                else
                {
                    CircleCollider2D coll = g.GetComponent<CircleCollider2D>();
                    coll.isTrigger = true;
                }
            });

            On("respawn", (E) => {
                string id = E.data["id"].ToString().Trim('"');
                float x = E.data["position"]["x"].f;
                float y = E.data["position"]["y"].f;
                float rotation = E.data["rotation"].f;

                GameObject g = serverObjects[id].gameObject;
                g.SetActive(true);

                g.transform.position = new Vector3(x, y, 0);
                g.transform.rotation = Quaternion.Euler(0, 0, rotation);
            });

            On("disconnected", (E) => {
                string id = E.data["id"].ToString().Trim('"');

                GameObject g = serverObjects[id].gameObject;
                Destroy(g); //Remove from game
                serverObjects.Remove(id);   //Remove from memory
            });

            On("move", (E) => {
                string id = E.data["id"].ToString().Trim('"');
                float x = E.data["position"]["x"].f;
                float y = E.data["position"]["y"].f;
                float timeSent = E.data["timeSent"].f;

                NetworkIdentity ni = serverObjects[id];
                NetworkPlayerMovement movement = ni.GetComponent<NetworkPlayerMovement>();
                Vector3 position = new Vector3(x, y, 0);

                movement.CorrectPosition(timeSent, position);
            });

            On("collisionMove", (E) => {
                string id = E.data["id"].ToString().Trim('"');
                float x = E.data["position"]["x"].f;
                float y = E.data["position"]["y"].f;

                NetworkIdentity ni = serverObjects[id];
                Vector3 position = new Vector3(x, y, 0);

                ni.transform.position = position;
            });

            On("aim", (E) => {
                string id = E.data["id"].ToString().Trim('"');
                float rotation = E.data["rotation"].f;

                NetworkIdentity ni = serverObjects[id];
                ni.transform.rotation = Quaternion.Euler(0, 0, rotation);
            });

            On("seatMove", (E) => {
                string id = E.data["id"].ToString().Trim('"');
                float x = E.data["position"]["x"].f;
                float y = E.data["position"]["y"].f;
                float rotation = E.data["rotation"].f;

                NetworkIdentity ni = serverObjects[id];
                Vector3 position = new Vector3(x, y, 0);

                ni.transform.position = position;
                ni.transform.rotation = Quaternion.Euler(0, 0, rotation);
            });

            On("attack", (E) => {
                string id = E.data["id"].ToString().Trim('"');

                NetworkIdentity ni = serverObjects[id];
                NetworkAttackManager attack = ni.GetComponent<NetworkAttackManager>();
                
                attack.InduceAttack();
            });

            On("interact", (E) => {
                string id = E.data["id"].ToString().Trim('"');

                NetworkIdentity ni = serverObjects[id];
                NetworkInteractor interactor = ni.GetComponentInChildren<NetworkInteractor>();

                interactor.InduceInteract();
            });

            On("takeDamage", (E) => {
                string id = E.data["id"].ToString().Trim('"');
                float damage = E.data["damage"].f;

                NetworkIdentity ni = serverObjects[id];
                NetworkHealthManager healthManager = ni.GetComponent<NetworkHealthManager>();

                healthManager.InduceDamage(damage);
            });
        }
    }

    [Serializable]
    public class Player
    {
        public string id;
        public Position position;
        public float rotation;
    }

    [Serializable]
    public class Position
    {
        public float x;
        public float y;
    }

    [Serializable]
    public class Move
    {
        public Position clientInputs;
        public float speed;
        public float deltaTime;
        public float timeSent;
    }

    [Serializable]
    public class CollisionMove
    {
        public Position clientInputs;
        public Position clientPosition;
        public float speed;
        public float deltaTime;
    }

    [Serializable]
    public class Aim
    {
        public float rotation;
    }

    // This Serializable Class is used for messages that involve Taking and Leaving Seats
    [Serializable]
    public class SeatMove
    {
        public Position position;
        public float rotation;
    }

    [Serializable]
    public class Damage
    {
        public string senderID;
        public string receiverID;
        public float damage;
    }
}
