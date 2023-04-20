using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallColorSwitcher : MonoBehaviour
{
    [SerializeField] private int _ballNumber;
    [Header("Ball Meshes")]
    [SerializeField] private MeshFilter _cueBallMeshFilter;
    [SerializeField] private MeshRenderer _cueBallRenderer;
    [SerializeField] private Mesh _cueBallMesh;
    [SerializeField] private Mesh _8BallMesh;
    [SerializeField] private Mesh _fullBallMesh;
    [SerializeField] private Mesh _halfBallMesh;
    [Header("Ball Colors")]
    [SerializeField] private Color _cueBallColor = Color.white;
    [SerializeField] private Color _8BallColor = new Color(0.016f, 0.016f, 0.016f);
    [SerializeField] private Color[] _ballColors = new Color[]
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
        SetBall(_ballNumber);
    }

    public void SetBall(int number)
    {
        _ballNumber = number;
        switch (_ballNumber)
        {
            case 0:
                _cueBallRenderer.material.color = _cueBallColor;
                _cueBallMeshFilter.mesh = _cueBallMesh;
                break;
            case 8:
                _cueBallRenderer.material.color = _8BallColor;
                _cueBallMeshFilter.mesh = _8BallMesh;
                break;
            default:
                if (_ballNumber > 8)
                {
                    _cueBallRenderer.material.color = _ballColors[_ballNumber - 9];
                    _cueBallMeshFilter.mesh = _halfBallMesh;
                }
                else
                {
                    _cueBallRenderer.material.color = _ballColors[_ballNumber - 1];
                    _cueBallMeshFilter.mesh = _fullBallMesh;
                }
                break;
        }
    }
}
