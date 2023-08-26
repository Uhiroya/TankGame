using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTankManager : MonoBehaviour , IStart , IPause , ITankData
{
    public TankData TankData;
    PlayerInputManager _playerInputManager;
    void Awake()
    {
        _playerInputManager = GetComponent<PlayerInputManager>();
        //_playerInputManager.enabled = false;
    }

    void OnEnable()
    {
        AddPauseScript();
    }

    void OnDisable()
    {
        
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Active()
    {
        _playerInputManager.enabled = true;
    }
    public TankData GetTankData()
    {
        return TankData;
    }
    public void Pause()
    {
        Debug.Log("‚æ‚Î‚ê‚Ä‚ç‚Ÿ");
    }

    public void Resume()
    {
        throw new System.NotImplementedException();
    }

    public void AddPauseScript()
    {
        PauseManager.PauseScripts.Add(this as IPause);
    }
}
