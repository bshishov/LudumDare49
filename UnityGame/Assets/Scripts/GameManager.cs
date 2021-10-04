using System;
using Audio;
using Network;
using Network.Messages;
using UI.Scripts;
using UIF.Scripts.Transitions;
using UnityEngine;
using Utils;

[RequireComponent(typeof(FrameManager))]
public class GameManager : Singleton<GameManager>
{
    [Header("Music")] public SoundAsset Music;
    
    [Header("UI")]
    public FrameData LoginFrame;
    public FrameData GameFrame;
    public FrameData LeagueDivisionFrame;
    
    public BaseTransition LoginTransition;
    public BaseTransition GameTransition;
    public BaseTransition LeagueDivisionTransition;
        
    private FrameManager _frameManager;


    [Header("Player Data")]
    public PlayerStats PlayerStats;

    private void Start()
    {
        SoundManager.Instance.Play(Music);
        _frameManager = GetComponent<FrameManager>();

        // Todo: subscribe to connection
        Connection.Instance.Connect("ws://unsmith.shishov.me/");

    }

    private void Update()
    {
        /* For debug purposes */
        if (Input.GetKeyDown(KeyCode.F3))
            TransitionToLeagueDivision();
        
        /* For debug purposes */
        if (Input.GetKeyDown(KeyCode.F2))
            TransitionToGame();
    }

    public void TransitionToLogin()
    {
        _frameManager.TransitionTo(LoginFrame, LoginTransition, 0);
    }
        
    public void TransitionToLeagueDivision()
    {
        _frameManager.TransitionTo(LeagueDivisionFrame, LeagueDivisionTransition, 0);
    }
        
    public void TransitionToGame()
    {
        _frameManager.TransitionTo(GameFrame, GameTransition, 0);
    }
}