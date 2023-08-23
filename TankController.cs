using UnityEngine;
using Photon.Pun;

public class TankController : MonoBehaviourPunCallbacks
{
    [SerializeField] private float tankMoveSpeed;
    [SerializeField] private float tankRotationSpeed;

    [SerializeField] private Animator leftTrackAnimator;
    [SerializeField] private Animator rightTrackAnimator;

    [SerializeField] private GameObject gun;
    [SerializeField] private Animator gunAnimator;
    [SerializeField] private float gunRotationSpeed;

    [SerializeField] private GameObject shell;
    [SerializeField] private Transform shellSpawn;

    [SerializeField] private GameObject reloadSpinner;

    [SerializeField] private Animator flashAnimator;
    
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip reloadSound;

    private bool _isReloading = false;
    private float _reloadTimer;
    
    private Rigidbody2D _rigidbody;
    private AudioSource _audioSource;
    private float _tankMoveAmount;
    private Camera _camera;

    private bool _playTrackAnim;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _audioSource = GetComponent<AudioSource>();
        _camera = Camera.main;
    }

    private void Update()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }

        CameraController.Instance.AttachCamera(transform);

        _tankMoveAmount = Input.GetAxisRaw("Vertical");
        _playTrackAnim = Mathf.Abs(_tankMoveAmount) >= 0.01f ? true : false;
        RotateTank();
        
        if (_playTrackAnim)
        {
            PlayTrackAnimation();
        }
        else
        {
            StopTrackAnimation();
        }

        RotateGun();

        if (_isReloading)
        {
            _reloadTimer -= Time.deltaTime;
            _reloadTimer = Mathf.Clamp(_reloadTimer, 0.0f, reloadSound.length);

            if (_reloadTimer == 0.0f)
            {
                _isReloading = false;
                reloadSpinner.SetActive(false);
            }
        }

        if (Input.GetMouseButtonDown(0) && !_isReloading)
        {
            gunAnimator.SetBool("shoot", true);
            flashAnimator.Play("Flash",  -1, 0f);
            _audioSource.PlayOneShot(fireSound);
            var spawnedShell = PhotonNetwork.Instantiate(shell.name, shellSpawn.position, gun.transform.rotation);
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), spawnedShell.GetComponent<Collider2D>());

            _reloadTimer = reloadSound.length + 0.3f;
            _audioSource.PlayOneShot(reloadSound);
            _isReloading = true;
            reloadSpinner.SetActive(true);
        }
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }

        MoveTank();
    }

    private void MoveTank()
    {
        _tankMoveAmount *= tankMoveSpeed * Time.fixedDeltaTime;
        _rigidbody.velocity = transform.up * _tankMoveAmount;
    }

    private void RotateTank()
    {
        var horizontalInput = Input.GetAxisRaw("Horizontal");
        var rotationAmount = horizontalInput * tankRotationSpeed * Time.deltaTime;
        transform.Rotate(0f, 0f, -rotationAmount);

        if (!_playTrackAnim)
        {
            _playTrackAnim = Mathf.Abs(rotationAmount) >= 0.01f ? true : false;
        }
    }

    private void PlayTrackAnimation()
    {
        leftTrackAnimator.SetFloat("speed", 1f);
        rightTrackAnimator.SetFloat("speed", 1f);
    }

    private void StopTrackAnimation()
    {
        leftTrackAnimator.SetFloat("speed", 0f);
        rightTrackAnimator.SetFloat("speed", 0f);
    }

    private void RotateGun()
    {
        var gunScreenPosition = _camera.WorldToScreenPoint(gun.transform.position);

        var targetDirection = (Vector2)(Input.mousePosition - gunScreenPosition);
        var rotationAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;

        var targetRotation = Quaternion.Euler(0f, 0f, rotationAngle - 90f);
        var rotationSpeed = gunRotationSpeed * Time.deltaTime;

        gun.transform.rotation = Quaternion.RotateTowards(gun.transform.rotation, targetRotation, rotationSpeed);
    }
}
