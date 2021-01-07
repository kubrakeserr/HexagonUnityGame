using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static AudioClip hexTurnSound, explosionSound, gameOverSound, pointSound, hexExpSound;
    static AudioSource audioSrc;
    
    void Start()
    {
        hexTurnSound = Resources.Load<AudioClip> ("hexTurn");
        explosionSound = Resources.Load<AudioClip> ("explosion");
        gameOverSound = Resources.Load<AudioClip> ("gameOver");
        pointSound = Resources.Load<AudioClip> ("point");
        hexExpSound = Resources.Load<AudioClip> ("hexExp");

        audioSrc = GetComponent<AudioSource>();
    }

    public static void PlaySound (string clip)
    {
        switch(clip)
        {
            case "hexTurn":
                audioSrc.PlayOneShot(hexTurnSound);
                break;
            case "explosion":
                audioSrc.PlayOneShot(explosionSound);
                break;
            case "gameOver":
                audioSrc.PlayOneShot(gameOverSound);
                break;
            case "point":
                audioSrc.PlayOneShot(pointSound);
                break;
            case "hexExp":
                audioSrc.PlayOneShot(hexExpSound);
                break;
        }
    }
}
