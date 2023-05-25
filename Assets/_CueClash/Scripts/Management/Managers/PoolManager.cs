using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PoolManager : NetworkSingleton<PoolManager>
{
    [SerializeField] private Ball[] balls;
    private int numberOfHits = 0, currentPlayerFault = -1; // Fault: -1 default, 0 false, 1 true
    private bool whiteBallStruck = false, otherBallStruck = false, p1Sinked = false, p2Sinked = false;
    [SerializeField] private InfoText infoText;
    [SerializeField] private BallsMovingUI ballsMovingUI;
    [SerializeField] private TextMeshProUGUI endScreen;
    [SerializeField] private Soundtrack soundtrack;
    public int currentPoolPlayer = 0;
    public float[] damageTaken = new float[2];
    private readonly Dictionary<Ball, Vector3> ballPositions = new();
    private readonly List<Ball> player1SinkedBalls = new();
    private readonly List<Ball> player2SinkedBalls = new();
    private float recentlyStruck = 1;

    private readonly float maxDurationOfBattle = 40;
    private float battleTimer = 0;
    bool isFight = false;
    private bool finalBattle = false;
    public bool ballsMoving;

    public override void Awake()
    {
        base.Awake();
        soundtrack = GetComponent<Soundtrack>();
        // TODO: Figure out where to put this
        SaveBallPositions();
    }

    private void Update()
    {
        if (recentlyStruck > 0)
        {
            recentlyStruck -= Time.deltaTime;
        }
        ballsMoving = BallsMoving();
        if (IsServer)
        {
            ballsMovingUI.isActive.Value = ballsMoving;

            // if player hit the white ball with the stick, do this
            if (whiteBallStruck && !ballsMoving && recentlyStruck <= 0)
            {
                print("End of current hit");
                whiteBallStruck = false;
                PlaceFallenBalls();

                // Keep going if the current player has sunk a ball and didn't commit a fault, otherwise swap
                if (currentPlayerFault != 0)
                {
                    Fault(currentPoolPlayer);
                }
                else if (!(currentPoolPlayer == 0 && p1Sinked) && !(currentPoolPlayer == 1 && p2Sinked))
                {
                    SwapPlayerClientRpc();
                }
                currentPlayerFault = -1;
                infoText.shotsLeft.Value = 5 - numberOfHits;
            }

            // if player didn't hit the white ball with the stick (and therefore fault is not 0), do this instead of the above
            if(!ballsMoving && otherBallStruck && recentlyStruck <= 0)
            {
                otherBallStruck = false;
                PlaceFallenBalls();
                Fault(currentPoolPlayer);
                currentPlayerFault = -1;
                infoText.shotsLeft.Value = 5 - numberOfHits;
            }
            
            if (!isFight && !ballsMoving && recentlyStruck <= 0 && numberOfHits >= 5)
            {
                //swap to fighting
                isFight = true;
                battleTimer = maxDurationOfBattle;
                StartFightClientRpc();
            }

            if (isFight)
            {
                battleTimer -= Time.deltaTime;
                infoText.timeLeft.Value = (int)battleTimer;
                if (battleTimer < 0)
                {
                    StopFightClientRpc();
                }
            }
        }
    }

    public void HitBall(Ball ball)
    {
        p1Sinked = false;
        p2Sinked = false;

        IncrementNumberOfHits(ball);
    }

    public void IncrementNumberOfHits(Ball ball)
    {
        numberOfHits++;
        recentlyStruck = 1;
        if (ball.ballNumber == 0)
        {
            whiteBallStruck = true;
        }
        else
        {
            otherBallStruck = true;
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
            LevelManager.Instance.players[0].AddBulletClientRpc(ball.ballNumber > 8 ? ball.ballNumber - 8 : ball.ballNumber);
        }
        foreach (Ball ball in player2SinkedBalls)
        {
            LevelManager.Instance.players[1].AddBulletClientRpc(ball.ballNumber > 8 ? ball.ballNumber - 8 : ball.ballNumber);
        }
        soundtrack.Pool = false;
    }

    [ClientRpc]
    private void StopFightClientRpc()
    {
        isFight = false;
        for (int i = 0; i < LevelManager.Instance.players.Count; i++)
        {
            LevelManager.Instance.players[i].SwitchToBilliard();
        }

        numberOfHits = 0;

        if (damageTaken[0] > damageTaken[1])
        {
            currentPoolPlayer = 1;
        }
        else if (damageTaken[0] < damageTaken[1])
        {
            currentPoolPlayer = 0;
        }
        else
        {
            currentPoolPlayer = Random.Range(0, 2);
        }

        damageTaken[0] = 0;
        damageTaken[1] = 0;

        infoText.shotsLeft.Value = 5;

        if (finalBattle)
        {
            endScreen.gameObject.SetActive(true);
            endScreen.text = "Player " + (currentPoolPlayer + 1) + " wins!";
            infoText.gameObject.SetActive(false);
            ballsMovingUI.gameObject.SetActive(false);
        }

        soundtrack.Pool = true;
    }

    private void SaveBallPositions()
    {
        player1SinkedBalls.Clear();
        player2SinkedBalls.Clear();
        foreach (Ball ball in balls)
        {
            ballPositions.Add(ball, ball.transform.position);
        }
    }

    // Check if the current player has thrown balls off the table and fix it. Also, FAULT! This also fixes white ball in pocket issues.
    private void PlaceFallenBalls()
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
            p1Sinked = true;
            player1SinkedBalls.Add(ball);
            return;
        }
        if (ball.ballNumber > 8 && ball.ballNumber < 16)
        {
            ball.sinked = true;
            p2Sinked = true;
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
            if (currentPoolPlayer == 0)
            {
                if (CountSinkedBalls(0) == 7)
                {
                    finalBattle = true;
                }
                else
                {
                    //TODO: mega fault, ending the game here would not be fun

                }
            }
            else
            {
                if (CountSinkedBalls(1) == 7)
                {
                    finalBattle = true;
                }
                else
                {
                    //TODO: mega fault, ending the game here would not be fun

                }
            }
        }
    }

    // Puts the balls back to their original positions, with y += 1 in case a new ball is at that position.
    private void PutBallsBackForPlayer(int player)
    {
        if (player == 0)
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

    private bool BallsMoving()
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
                        if ((currentPoolPlayer == 0 && (ball.ballNumber > 0 && ball.ballNumber < 8)) ||
                            (currentPoolPlayer == 1 && (ball.ballNumber > 8 && ball.ballNumber < 16)) ||
                            (currentPoolPlayer == 0 && ball.ballNumber == 8 && CountSinkedBalls(0) == 7) ||
                            (currentPoolPlayer == 1 && ball.ballNumber == 8 && CountSinkedBalls(1) == 7))
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

    [ClientRpc]
    private void SwapPlayerClientRpc()
    {
        currentPoolPlayer = currentPoolPlayer == 0 ? 1 : 0;
    }

    // Current player has commited a fault. Do not call this directly, set fault to 1.
    private void Fault(int player)
    {
        PutBallsBackForPlayer(player);
        SwapPlayerClientRpc();
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { (ulong)LevelManager.Instance.players[currentPoolPlayer].team.Value }
            }
        };
        LevelManager.Instance.players[currentPoolPlayer].AddBulletClientRpc(0, clientRpcParams);
    }

    // Returns the number of balls that have been sinked by the player
    private int CountSinkedBalls(int player)
    {
        int aux = 0;

        if (player == 0)
        {
            foreach (Ball ball in balls)
            {
                if (ball.ballNumber > 0 && ball.ballNumber < 8 && ball.sinked)
                {
                    aux++;
                }
            }
        }
        else
        {
            foreach (Ball ball in balls)
            {
                if (ball.ballNumber > 8 && ball.ballNumber < 16 && ball.sinked)
                {
                    aux++;
                }
            }
        }

        return aux;
    }
}
