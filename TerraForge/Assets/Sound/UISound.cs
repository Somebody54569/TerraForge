using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISound : MonoBehaviour
{
    public void PlaySoundSFX(string s)
    {
        AudioManager.Instance.PlaySFX(s);
    }
}
