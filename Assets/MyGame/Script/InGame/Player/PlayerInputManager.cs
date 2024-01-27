using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    [SerializeField] private TankController _tankController;
    [SerializeField] private AudioSource _audioSource;
    private float _inputHorizontal;
    private float _inputMoveHorizontal;

    private float _inputMoveVertical;
    

    private void Update()
    {
        _inputMoveVertical = Input.GetAxisRaw("Vertical");
        _inputMoveHorizontal = Input.GetAxisRaw("Horizontal");
        _inputHorizontal = Input.GetAxisRaw("Horizontal2");
        if (Input.GetButtonDown("Fire1")) _tankController.InputFire();
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
        _tankController.InputMove(_inputMoveVertical);
        if (_inputMoveVertical >= 0)
            _tankController.InputTurn(_inputMoveHorizontal);
        else
            _tankController.InputTurn(-_inputMoveHorizontal);
        _tankController.InputBarrelTurn(_inputHorizontal);
    }

    public void StopTankAudio()
    {
        _audioSource.Pause();
    }
}