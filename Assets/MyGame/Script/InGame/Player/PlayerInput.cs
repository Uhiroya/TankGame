using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private InputReceiver _inputReceiver;
    [SerializeField] private AudioSource _audioSource;
    private float _inputHorizontal;
    private float _inputMoveHorizontal;

    private float _inputMoveVertical;
    

    private void Update()
    {
        _inputMoveVertical = Input.GetAxisRaw("Vertical");
        _inputMoveHorizontal = Input.GetAxisRaw("Horizontal");
        _inputHorizontal = Input.GetAxisRaw("Horizontal2");
        if (Input.GetButtonDown("Fire1")) _inputReceiver.InputFire();
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
        _inputReceiver.InputMove(_inputMoveVertical);
        if (_inputMoveVertical >= 0)
            _inputReceiver.InputTurn(_inputMoveHorizontal);
        else
            _inputReceiver.InputTurn(-_inputMoveHorizontal);
        _inputReceiver.InputBarrelTurn(_inputHorizontal);
    }

    public void StopTankAudio()
    {
        _audioSource.Pause();
    }
}