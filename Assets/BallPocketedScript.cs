using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPocketedScript : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject LogicManagerLink;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        print(other.name);
    }

}
