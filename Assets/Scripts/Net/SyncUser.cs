﻿using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

[RequireComponent(typeof(PhotonView))]
public class SyncUser : MonoBehaviourPunCallbacks //Этот вариант лучше, его можно будет потом не так сложно переделать под множество игроков.
{
    [Header("Префабы для создания чужого юзера")]
    [SerializeField] private GameObject PlayerPrefab;
    [SerializeField] private GameObject HeadsetPrefab;
    [SerializeField] private GameObject[] HandsPrefabs;

    [Header("Локальный плеер")]
    [SerializeField] private Transform Player;

    [SerializeField] private Transform Headset;
    [SerializeField] private Transform[] Hands;

    private List<GamePerson> Players = new List<GamePerson>();
    private bool isMaster;
    private PhotonView photonView;
    void Start()
    {
        isMaster = PhotonNetwork.IsMasterClient;
        photonView = gameObject.GetComponent<PhotonView>();
    }

    void FixedUpdate()
    {
        if (PhotonNetwork.InRoom)
        {
            photonView.RPC("SyncCapsulePos", RpcTarget.Others, Player.position.x, Player.position.y, Player.position.z, PhotonNetwork.LocalPlayer.ActorNumber);
            photonView.RPC("SyncCapsuleRotation", RpcTarget.Others, Player.rotation.x, Player.rotation.y, Player.rotation.z, Player.rotation.w, PhotonNetwork.LocalPlayer.ActorNumber);

            photonView.RPC("SyncHandsPos", RpcTarget.Others, Hands[0].position.x, Hands[0].position.y, Hands[0].position.z, Hands[1].position.x, Hands[1].position.y, Hands[1].position.z, PhotonNetwork.LocalPlayer.ActorNumber);
            photonView.RPC("SyncHandsRot", RpcTarget.Others, Hands[0].rotation.x, Hands[0].rotation.y, Hands[0].rotation.z, Hands[0].rotation.w, Hands[1].rotation.x, Hands[1].rotation.y, Hands[1].rotation.z, Hands[1].rotation.w, PhotonNetwork.LocalPlayer.ActorNumber);

            photonView.RPC("SyncHeadsetPosition", RpcTarget.Others, Headset.position.x, Headset.position.y, Headset.position.z, PhotonNetwork.LocalPlayer.ActorNumber);
            photonView.RPC("SyncHeadsetRotation", RpcTarget.Others, Headset.rotation.x, Headset.rotation.y, Headset.rotation.z, Headset.rotation.w, PhotonNetwork.LocalPlayer.ActorNumber);

        }
    }

    [PunRPC] private void SyncCapsulePos(float x, float y, float z, int id)
    {
        id = (PhotonNetwork.LocalPlayer.ActorNumber < id) ? id - 2 : id - 1; //Сделано так, ибо мы пропукаем номер того, кто отправляет создание своего персонажа у всех.
        Players[id].SetBodyPosition(x, y, z);
    }

    [PunRPC] private void SyncCapsuleRotation(float x, float y, float z, float w, int id)
    {
        id = (PhotonNetwork.LocalPlayer.ActorNumber < id) ? id - 2 : id - 1;
        Players[id].SetBodyRotation(new Quaternion(x, y, z, w));
    }

    [PunRPC] private void SyncHandsPos(float xL, float yL, float zL, float xR, float yR, float zR, int id)
    {
        id = (PhotonNetwork.LocalPlayer.ActorNumber < id) ? id - 2 : id - 1;
        Players[id].SetHandPosition(new Vector3(xL, yL, zL), false);
        Players[id].SetHandPosition(new Vector3(xR, yR, zR), true);
    }

    [PunRPC] private void SyncHandsRot(float xL, float yL, float zL, float wL, float xR, float yR, float zR, float wR, int id)
    {
        id = (PhotonNetwork.LocalPlayer.ActorNumber < id) ? id - 2 : id - 1;
        Players[id].SetHandRotation(new Quaternion(xL, yL, zL, wL), false);
        Players[id].SetHandRotation(new Quaternion(xR, yR, zR, wR), true);
    }

    [PunRPC] private void SyncHeadsetRotation(float x, float y, float z, float w, int id)
    {
        id = (PhotonNetwork.LocalPlayer.ActorNumber < id) ? id - 2 : id - 1;
        Players[id].SetHeadsetRotation(new Quaternion(x, y, z, w));
    }

    [PunRPC] private void SyncHeadsetPosition(float x, float y, float z, int id)
    {
        id = (PhotonNetwork.LocalPlayer.ActorNumber < id) ? id - 2 : id - 1;
        Players[id].SetHeadsetPosition(new Vector3(x, y, z));
    }


    public override void OnJoinedRoom() //Нужно добавить одну капсулу всем клиентам, кроме себя.
    {
        photonView.RPC("CreateCapsule", RpcTarget.OthersBuffered, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC] private void CreateCapsule(int id)
    {
        Players.Add(new GamePerson(PlayerPrefab, HandsPrefabs[0], HandsPrefabs[1], HeadsetPrefab));
        Debug.LogError($"Добавляем капсулу. Кол во игроков в листе {Players.Count}.");
    }


    class GamePerson {
        private GameObject Body;
        private Transform BodyTransform;

        private GameObject LeftHand;
        private Transform LeftHandTransform;
        private Transform BodyLeftHand;

        private GameObject RightHand;
        private Transform RightHandTransform;
        private Transform BodyRightHand;

        //private GameObject Headset;
        private Transform HeadsetTransform;

        public GamePerson(GameObject Body, GameObject LeftHand, GameObject RightHand, GameObject Headset)
        {
            this.Body = Instantiate(Body);
            BodyTransform = this.Body.GetComponent<Transform>();

            this.LeftHand = Instantiate(LeftHand, BodyTransform);
            this.RightHand = Instantiate(RightHand, BodyTransform);
            LeftHandTransform = this.LeftHand.GetComponent<Transform>();
            BodyLeftHand = this.Body.transform.Find("leftHand").gameObject.transform;
            RightHandTransform = this.RightHand.GetComponent<Transform>();
            BodyRightHand = this.Body.transform.Find("rightHand").gameObject.transform;

            //this.Headset = Instantiate(Headset, BodyTransform);
            HeadsetTransform = this.Body.transform.Find("head").gameObject.transform;
        }

        ~GamePerson()
        {
            Destroy(Body);
            Destroy(LeftHand);
            Destroy(RightHand);
        }

        public void SetBodyPosition(float x, float y, float z)
        {
            BodyTransform.position = new Vector3(x, y, z);
        }

        public void SetBodyPosition(Vector3 position)
        {
            BodyTransform.position = position;
        }

        public void SetBodyRotation(float x, float y, float z, float w)
        {
            BodyTransform.rotation = new Quaternion(z, y, z, w);
        }

        public void SetBodyRotation(Quaternion rotation)
        {
            BodyTransform.rotation = rotation;
        }

        public void SetHandPosition(Vector3 position, bool IsRightHand)
        {
            if (IsRightHand)
            {
                BodyRightHand.position = RightHandTransform.position = position;
            }
            else
            {
                BodyLeftHand.position = LeftHandTransform.position = position;
            }
        }

        public void SetHandRotation(Quaternion rotation, bool IsRightHand)
        {
            if (IsRightHand)
            {
                BodyRightHand.rotation = RightHandTransform.rotation = rotation;
            }
            else
            {
                BodyLeftHand.rotation = LeftHandTransform.rotation = rotation;
            }
        }

        public void SetHeadsetRotation(Quaternion rotation)
        {
            HeadsetTransform.rotation = rotation;
        }

        public void SetHeadsetPosition(Vector3 position)
        {
            HeadsetTransform.position = position;
        }
    }
}
