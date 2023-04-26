using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    [SerializeField] private Ball[] balls;
    private Dictionary<Ball, Vector3> ballPositions = new Dictionary<Ball, Vector3>();
    private List<Ball> player1SinkedBalls = new List<Ball>();
    private List<Ball> player2SinkedBalls = new List<Ball>();

    public void SaveBallPositions()
    {
        player1SinkedBalls.Clear();
        player2SinkedBalls.Clear();
        foreach (Ball ball in balls)
        {
            ballPositions.Add(ball, ball.transform.position);
        }
    }

    public void AddSinkedBall(Ball ball, int player)
    {
        if (player == 1)
        {
            player1SinkedBalls.Add(ball);
        }
        else
        {
            player2SinkedBalls.Add(ball);
        }
    }
}
