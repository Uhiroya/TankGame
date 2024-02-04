using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private TankInputSync _tankInputSync;
    [SerializeField] private AudioSource _audioSource;
    private float _inputHorizontal;
    private float _inputMoveHorizontal;

    private float _inputMoveVertical;
    

    private void Update()
    {
        _inputMoveVertical = Input.GetAxisRaw("Vertical");
        _inputMoveHorizontal = Input.GetAxisRaw("Horizontal");
        _inputHorizontal = Input.GetAxisRaw("Horizontal2");
        if (Input.GetButtonDown("Fire1")) _tankInputSync.InputFire();
        if (_inputMoveVertical == 0f)
        {
            _audioSource.Pause();
        }
        else
        {
            if (!_audioSource.isPlaying) _audioSource.Play();
        }
    }

    private void FixedUpdate()
    {
        _tankInputSync.InputMove(_inputMoveVertical);
        if (_inputMoveVertical >= 0)
            _tankInputSync.InputTurn(_inputMoveHorizontal);
        else
            _tankInputSync.InputTurn(-_inputMoveHorizontal);
        _tankInputSync.InputBarrelTurn(_inputHorizontal);
    }

    public void StopTankAudio()
    {
        _audioSource.Pause();
    }
}