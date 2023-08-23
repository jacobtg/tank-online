using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    private Transform target;
    private Camera cam;

    [SerializeField] private Vector3 offset;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    public void AttachCamera(Transform target)
    {
        this.target = target;
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
        else
        {
            transform.position = GameObject.Find("Environment").GetComponent<Transform>().position + offset;
        }
    }
}
