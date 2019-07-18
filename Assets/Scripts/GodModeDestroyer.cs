using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GodModeDestroyer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider collider)
    {
        Debug.Log(collider.gameObject.tag);
        if(collider.gameObject.tag != "Player")
            return;
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Cubes");
        foreach(var gameObject in gameObjects)
        {
            gameObject.GetComponent<CubeDestroyer>().Explode();
        }
    }
}
