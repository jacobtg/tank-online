using UnityEngine;
using Photon.Pun;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private float health = 100.0f;

    private void Update()
    {
        if (photonView.IsMine)
        {
            if (health <= 0f)
            {
                GameManager.Instance.LeaveRoom();
            }
        }
    }
 
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        if (!other.CompareTag("Enemy"))
        {
            return;
        }

        health -= 100.0f;
    }
}
