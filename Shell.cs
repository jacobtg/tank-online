using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Shell : MonoBehaviourPunCallbacks
{
    [SerializeField] private AudioClip explosionSound;

    private void Start()
    {
        GetComponent<Rigidbody2D>().velocity = transform.up * 8f;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {   
        print($"Shell shot by {photonView.Owner.NickName} has collided with {col.gameObject.name}");
        var explosion = PhotonNetwork.Instantiate("Explosion", col.contacts[0].point, Quaternion.identity);
        explosion.GetComponent<Animator>().Play("0");
        explosion.GetComponent<AudioSource>().PlayOneShot(explosionSound);

        if (photonView.IsMine && col.gameObject.CompareTag("PlayerTank"))
        {
            MatchManager.Instance.ReportKillAndDeath(PhotonNetwork.LocalPlayer, col.gameObject.GetPhotonView().Owner);
        }

        PhotonNetwork.Destroy(gameObject);
    }
}
