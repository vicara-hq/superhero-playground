using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeDestroyer : MonoBehaviour
{
    public GameObject explosionParticleSystem;

    public void Explode()
    {
        var explosionSystem = Instantiate(explosionParticleSystem, transform.position, transform.rotation);
        Destroy(gameObject, 0.5f);
        Destroy(explosionSystem, 2f);
    }
}
