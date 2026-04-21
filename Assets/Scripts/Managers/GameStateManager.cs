using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameStateManager : MonoBehaviour
{
    public enum State
    {
        Active,
        Inactive
    }
    public State currentState;

    [Header("Puzzle Image")]
    public Texture2D image; 

    [Header("UI Connections")]
    public TextMeshProUGUI timer;

    private float elapsedTime = 0f;

    public void StartGame()
    {
        // Runs once a new game is started.
        currentState = State.Active;
        elapsedTime = 0;
        Debug.Log($"Game Started, State set to: {currentState}");
    }
    public void Update()
    {
        // Anything that updates as the game plays should be put into here.
        if (currentState == State.Active)
        {
            // Anything requiring an active game should be put in here.
            elapsedTime += Time.deltaTime;
        }
    }
}

