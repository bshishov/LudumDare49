using UI.Scripts;
using UIF.Scripts.Transitions;
using UnityEngine;
using Utils;

[RequireComponent(typeof(FrameManager))]
public class GameManager : Singleton<GameManager>
{
    [Header("UI")]
    public FrameData LoginFrame;
    public FrameData GameFrame;
    public FrameData LeagueDivisionFrame;
    
    public BaseTransition LoginTransition;
    public BaseTransition GameTransition;
    public BaseTransition LeagueDivisionTransition;
        
    private FrameManager _frameManager;

    private void Start()
    {
        _frameManager = GetComponent<FrameManager>();
        
        // Todo: subscribe to connection
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