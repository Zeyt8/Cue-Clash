using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    [SerializeField] private Ball[] _balls;
    private Dictionary<Ball, Vector3> _ballPositions = new Dictionary<Ball, Vector3>();
    private List<Ball> _player1SinkedBalls = new List<Ball>();
    private List<Ball> _player2SinkedBalls = new List<Ball>();

    public void SaveBallPositions()
    {
        _player1SinkedBalls.Clear();
        _player2SinkedBalls.Clear();
        foreach (Ball ball in _balls)
        {
            _ballPositions.Add(ball, ball.transform.position);
        }
    }

    public void AddSinkedBall(Ball ball, int player)
    {
        if (player == 1)
        {
            _player1SinkedBalls.Add(ball);
        }
        else
        {
            _player2SinkedBalls.Add(ball);
        }
    }
}
