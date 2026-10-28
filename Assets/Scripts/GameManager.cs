using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    [SerializeField] private Ball ball;
    [SerializeField] private Paddle playerPaddle;
    [SerializeField] private Paddle computerPaddle;
    [SerializeField] private Text playerScoreText;
    [SerializeField] private Text computerScoreText;

    private int playerScore;
    private int computerScore;

    private bool isRoundActive = false;

    private void Start()
    {
        NewGame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) {
            NewGame();
        }

        if ( Input.GetMouseButtonDown(0) && !isRoundActive ) {
            ThrowBall();
        }
    }

    public void NewGame()
    {
        SetPlayerScore(0);
        SetComputerScore(0);
        playerPaddle.ResetPosition();
        computerPaddle.ResetPosition();
        ThrowBall();
    }


    private void EndRound()
    {
        isRoundActive = false;
        ball.ResetPosition();
    }

    public void ThrowBall()
    {
        isRoundActive = true;
        ball.AddStartingForce();
    }

    public void OnPlayerScored()
    {
        SetPlayerScore(playerScore + 1);
        computerPaddle.GetComponent<ComputerPaddle>().speed *= 1.5f; // Increase computer paddle speed
        EndRound();
    }

    public void OnComputerScored()
    {
        SetComputerScore(computerScore + 1);
        EndRound();
    }

    private void SetPlayerScore(int score)
    {
        playerScore = score;
        playerScoreText.text = score.ToString();
    }

    private void SetComputerScore(int score)
    {
        computerScore = score;
        computerScoreText.text = score.ToString();
    }

}
