using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static AudioClip sound;

    private AudioSource source;

    private void Start()
    {
        source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (sound != null)
        {
            source.PlayOneShot(sound, 0.2f);
            sound = null;
        }
    }
}
