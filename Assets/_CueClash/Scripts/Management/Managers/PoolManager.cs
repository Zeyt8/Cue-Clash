using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PoolManager : NetworkSingleton<PoolManager>
{
    [SerializeField] private Ball[] balls;
    [SerializeField] private int numberOfHits = 0;
    public int currentPoolPlayer = 1;
    private readonly Dictionary<Ball, Vector3> ballPositions = new();
    private readonly List<Ball> player1SinkedBalls = new();
    private readonly List<Ball> player2SinkedBalls = new();

    public void IncrementNumberOfHits(Ball ball)
    {
        numberOfHits++;

        // Fault
        if (ball.ballNumber != 0)
        {
            
        }

        //swap to fighting
        if (numberOfHits > 2)
        {
            if (IsServer)
            {
                StartFightClientRpc();
            }
        }
    }

    [ClientRpc]
    private void StartFightClientRpc()
    {
        for (int i = 0; i < LevelManager.Instance.players.Count; i++)
        {
            LevelManager.Instance.players[i].transform.position = LevelManager.Instance.spawnPoints[i].position;
            LevelManager.Instance.players[i].SwitchToFight();
        }
        foreach (Ball ball in player1SinkedBalls)
        {
            LevelManager.Instance.players[0].AddBullet(ball.ballNumber > 8 ? ball.ballNumber - 8 : ball.ballNumber);
        }
        foreach (Ball ball in player2SinkedBalls)
        {
            // TODO: change this
            if (LevelManager.Instance.players.Count > 1)
                LevelManager.Instance.players[1].AddBullet(ball.ballNumber > 8 ? ball.ballNumber - 8 : ball.ballNumber);
        }
    }
    
    public void SaveBallPositions()
    {
        numberOfHits = 0;
        player1SinkedBalls.Clear();
        player2SinkedBalls.Clear();
        foreach (Ball ball in balls)
        {
            ballPositions.Add(ball, ball.transform.position);
        }
    }

    public void AddSinkedBall(Ball ball)
    {
        if (ball.ballNumber > 0 && ball.ballNumber < 8)
        {
            player1SinkedBalls.Add(ball);
            return;
        }
        if (ball.ballNumber > 8 && ball.ballNumber < 16)
        {
            player2SinkedBalls.Add(ball);
            return;
        }

        //fault fight
        if (ball.ballNumber == 0)
        {

        }

        //supreme showdown
        if (ball.ballNumber == 8)
        {

        }
    }

    // Puts the balls back to their original positions, with y += 1 in case a new ball is at the same position
    public void PutBallsBackForPlayer(int player)
    {
        if (player == 1)
        {
            foreach (Ball ball in player1SinkedBalls)
            {
                ball.transform.position = ballPositions[ball];
            }
        }
        else
        {
            foreach (Ball ball in player2SinkedBalls)
            {
                Vector3 pos = ballPositions[ball];
                pos.y += 1;
                ball.transform.position = pos;
            }
        }
    }
}
