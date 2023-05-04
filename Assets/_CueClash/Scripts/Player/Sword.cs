using UnityEngine;

public class Sword : MonoBehaviour
{
    public bool parrying;
    public bool blocking;

    private float timer = 0;
    
    public void Activate()
    {

    }

    public void Deactivate()
    {

    }

    private void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else if (parrying)
        {
            parrying = false;
        }
    }

    public void StartParry()
    {
        parrying = true;
        blocking = true;
        timer = 0.5f;
    }

    public void EndParry()
    {
        blocking = false;
    }
}
