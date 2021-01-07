using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyME : MonoBehaviour
{
    // Ekrana çıkan puanın yok olması
    public int lifeTime;
    
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    
    void Update()
    {
        
    }
}
