using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PoolManager : NetworkSingleton<PoolManager>
{
    [SerializeField] private Ball[] balls;
    private int numberOfHits = 0, currentPlayerFault = -1; // Fault: -1 default, 0 false, 1 true
    private bool whiteBallStruck = false, p1sinked = false, p2sinked = false;
    [SerializeField] private InfoText infoText;
    public int currentPoolPlayer = 1;
    private readonly Dictionary<Ball, Vector3> ballPositions = new();
    private readonly List<Ball> player1SinkedBalls = new();
    private readonly List<Ball> player2SinkedBalls = new();
    private float recentlyStruck = 0;

    private readonly float maxDurationOfBattle = 120;
    private float battleTimer = 0;
    bool isFight = false;

    public override void Awake()
    {
        base.Awake();
        // TODO: Figure out where to put this
        SaveBallPositions();
    }

    // Decide whose turn it is based on if the current player has sunk a ball. TODO: disable hitting balls until none are moving
    private void Update()
    {
        if (recentlyStruck > 0)
        {
            recentlyStruck -= Time.deltaTime;
        }
        if (whiteBallStruck && !BallsMoving() && recentlyStruck <= 0)
        {
            print("End of current hit");
            whiteBallStruck = false;
            PlaceFallenBalls();

            // Keep going if the current player has sunk a ball and didn't commit a fault, otherwise swap
            if (((currentPoolPlayer == 1 && p1sinked) || (currentPoolPlayer == 2 && p2sinked)) && (currentPlayerFault == 0))
            {
                SwapPlayer();
            }

            if (currentPlayerFault == 1)
            {
                Fault(currentPoolPlayer);
            }

            currentPlayerFault = -1;
        }

        if (IsServer && !isFight && !BallsMoving() && numberOfHits > 2)
        {
            //swap to fighting
            isFight = true;
            battleTimer = maxDurationOfBattle;
            StartFightClientRpc();
        }

        if (IsServer && isFight)
        {
            battleTimer -= Time.deltaTime;
            infoText.timeLeft.Value = (int)battleTimer;
            if (battleTimer < 0)
            {
                StopFightClientRpc();
            }
        }
    }

    public void HitBall(Ball ball)
    {
        p1sinked = false;
        p2sinked = false;

        IncrementNumberOfHits(ball);
    }

    public void IncrementNumberOfHits(Ball ball)
    {
        numberOfHits++;
        if (IsServer)
        {
            infoText.shotsLeft.Value = 3 - numberOfHits;
        }

        if (ball.ballNumber == 0)
        {
            whiteBallStruck = true;
            recentlyStruck = 1;
        }
        else
        {
            currentPlayerFault = 1;
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

    [ClientRpc]
    private void StopFightClientRpc()
    {
        for (int i = 0; i < LevelManager.Instance.players.Count; i++)
        {
            LevelManager.Instance.players[i].SwitchToBilliard();
        }

        numberOfHits = 0;
    }

    public void SaveBallPositions()
    {
        player1SinkedBalls.Clear();
        player2SinkedBalls.Clear();
        foreach (Ball ball in balls)
        {
            ballPositions.Add(ball, ball.transform.position);
        }
    }

    // Check if the current player has thrown balls off the table and fix it. Also, FAULT! This also fixes white ball in pocket issues.
    public void PlaceFallenBalls()
    {
        Vector3 fallen;
        foreach (Ball ball in balls)
        {
            fallen = ball.transform.position;
            if ((fallen.y < 1 || (fallen.z < -2.5 || fallen.z > 2.5) || (fallen.x < -1.5 || fallen.x > 1.5)) && ball.sinked == false)
            {
                currentPlayerFault = 1;
                Vector3 pos = ballPositions[ball];
                pos.y += 1;
                ball.transform.position = pos;
            }
        }

        print("Place fallen balls");
    }

    public void AddSinkedBall(Ball ball)
    {
        if (ball.ballNumber > 0 && ball.ballNumber < 8)
        {
            ball.sinked = true;
            player1SinkedBalls.Add(ball);
            return;
        }
        if (ball.ballNumber > 8 && ball.ballNumber < 16)
        {
            ball.sinked = true;
            player2SinkedBalls.Add(ball);
            return;
        }

        if (ball.ballNumber == 0)
        {
            currentPlayerFault = 1;
        }

        //TODO: supreme showdown
        if (ball.ballNumber == 8)
        {
            if (currentPoolPlayer == 1)
            {
                if (CountSinkedBalls(1) == 7)
                {
                    //TODO: final battle

                }
                else
                {
                    //TODO: mega fault, ending the game here would not be fun

                }
            }
            else
            {
                if (CountSinkedBalls(2) == 7)
                {
                    //TODO: final battle

                }
                else
                {
                    //TODO: mega fault, ending the game here would not be fun

                }
            }
        }
    }

    // Puts the balls back to their original positions, with y += 1 in case a new ball is at that position.
    public void PutBallsBackForPlayer(int player)
    {
        if (player == 1)
        {
            foreach (Ball ball in player1SinkedBalls)
            {
                Vector3 pos = ballPositions[ball];
                pos.y += 1;
                ball.transform.position = pos;
                ball.sinked = false;
            }
        }
        else
        {
            foreach (Ball ball in player2SinkedBalls)
            {
                Vector3 pos = ballPositions[ball];
                pos.y += 1;
                ball.transform.position = pos;
                ball.sinked = false;
            }
        }
    }

    public bool BallsMoving()
    {
        bool stillMoving = false;

        foreach (Ball ball in balls)
        {

            if (ball.GetComponent<Rigidbody>().velocity != Vector3.zero)
            {
                stillMoving = true;

                // detect if the player has hit one of his balls first
                if (currentPlayerFault == -1)
                {
                    if (ball.ballNumber != 0)
                    {
                        if ((currentPoolPlayer == 1 && (ball.ballNumber > 0 && ball.ballNumber < 8)) ||
                            (currentPoolPlayer == 2 && (ball.ballNumber > 8 && ball.ballNumber < 16)) ||
                            (currentPoolPlayer == 1 && ball.ballNumber == 8 && player1SinkedBalls.Count == 7) ||
                            (currentPoolPlayer == 2 && ball.ballNumber == 8 && player2SinkedBalls.Count == 7))
                        {
                            currentPlayerFault = 0;
                        }
                        else currentPlayerFault = 1;
                    }
                }
            }
        }

        return stillMoving;
    }
    // TODO: Enable hitting balls for the current player
    public void SwapPlayer()
    {
        currentPoolPlayer = currentPoolPlayer == 1 ? 2 : 1;
    }

    // Current player has commited a fault. Do not call this directly, set fault to 1.
    public void Fault(int player)
    {
        PutBallsBackForPlayer(player);
        SwapPlayer();
        LevelManager.Instance.players[3 - player].AddBullet(0);
    }

    // Returns the number of balls that have been sinked by the player
    public int CountSinkedBalls(int player)
    {
        int aux = 0;

        if (player == 1)
        {
            foreach (Ball ball in balls)
            {
                if (ball.ballNumber > 0 && ball.ballNumber < 8 && ball.sinked == true)
                {
                    aux++;
                }
            }
        }
        else
        {
            foreach (Ball ball in balls)
            {
                if (ball.ballNumber > 8 && ball.ballNumber < 16 && ball.sinked == true)
                {
                    aux++;
                }
            }
        }

        return aux;
    }
}
