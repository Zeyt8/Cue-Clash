using JSAM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soundtrack : MonoBehaviour
{
    public bool Pool
    {
        get => _pool;
        set
        {
            _pool = value;
            PlayNext();
        }
    }

    private bool _pool = true;
    private Music lastMusic = 0;
    
    private void Update()
    {
        if (!AudioManager.IsMusicPlaying(lastMusic))
        {
            PlayNext();
        }
    }

    private void PlayNext()
    {
        if (Pool)
        {
            // pool
            Music toPlay = (Music)Random.Range(4, 9);
            while (toPlay == lastMusic)
            {
                toPlay = (Music)Random.Range(4, 9);
            }
            AudioManager.PlayMusic(toPlay);
            lastMusic = toPlay;
        }
        else
        {
            // combat
            Music toPlay = (Music)Random.Range(0, 4);
            while (toPlay == lastMusic)
            {
                toPlay = (Music)Random.Range(0, 4);
            }
            AudioManager.PlayMusic(toPlay);
            lastMusic = toPlay;
        }
    }
}
