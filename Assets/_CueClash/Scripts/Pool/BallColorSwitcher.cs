using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallColorSwitcher : MonoBehaviour
{
    [SerializeField] private int ballNumber;
    [Header("Ball Meshes")]
    [SerializeField] private MeshFilter cueBallMeshFilter;
    [SerializeField] private MeshRenderer cueBallRenderer;
    [SerializeField] private Mesh cueBallMesh;
    [SerializeField] private Mesh _8BallMesh;
    [SerializeField] private Mesh fullBallMesh;
    [SerializeField] private Mesh halfBallMesh;
    [Header("Ball Colors")]
    [SerializeField] private Color cueBallColor = Color.white;
    [SerializeField] private Color _8BallColor = new Color(0.016f, 0.016f, 0.016f);
    [SerializeField] private Color[] ballColors = new Color[]
    {
        new Color(0.973f, 0.661f, 0.134f),
        new Color(0.095f, 0.136f, 0.288f),
        new Color(0.930f, 0.115f, 0.132f),
        new Color(0.231f, 0.122f, 0.456f),
        new Color(1f, 0.250f, 0.130f),
        new Color(0.047f, 0.326f, 0.090f),
        new Color(0.367f, 0.135f, 0.028f)
    };

    private void Awake()
    {
        SetBall(ballNumber);
    }

    public void SetBall(int number)
    {
        ballNumber = number;
        switch (ballNumber)
        {
            case 0:
                cueBallRenderer.material.color = cueBallColor;
                cueBallMeshFilter.mesh = cueBallMesh;
                break;
            case 8:
                cueBallRenderer.material.color = _8BallColor;
                cueBallMeshFilter.mesh = _8BallMesh;
                break;
            default:
                if (ballNumber > 8)
                {
                    cueBallRenderer.material.color = ballColors[ballNumber - 9];
                    cueBallMeshFilter.mesh = halfBallMesh;
                }
                else
                {
                    cueBallRenderer.material.color = ballColors[ballNumber - 1];
                    cueBallMeshFilter.mesh = fullBallMesh;
                }
                break;
        }
    }
}
