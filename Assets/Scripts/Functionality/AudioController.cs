using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioSource bg_adudio;
    [SerializeField] internal AudioSource audioPlayer_wl;
    [SerializeField] internal AudioSource audioPlayer_button;
    [SerializeField] internal AudioSource audioSpin_button;
    [SerializeField] private AudioClip[] clips;


    private void Start()
    {
        if (bg_adudio) bg_adudio.Play();
        audioPlayer_button.clip = clips[clips.Length-1];
        audioSpin_button.clip = clips[clips.Length-2];
    }

    internal void CheckFocusFunction(bool focus, bool IsSpinning)
    {
        if (!focus)
        {
            bg_adudio.Pause();
            audioPlayer_wl.Pause();
            audioPlayer_button.Pause();
        }
        else
        {
            if (!bg_adudio.mute) bg_adudio.UnPause();
            if (IsSpinning)
            {
                if (!audioPlayer_wl.mute) audioPlayer_wl.UnPause();
            }
            else
            {
                StopWLAaudio();
            }
            if (!audioPlayer_button.mute) audioPlayer_button.UnPause();

        }
    }



    internal void PlayWLAudio(string type)
    {
        audioPlayer_wl.loop = false;
        int index = 0;
        switch (type)
        {
            case "spin":
                index = 0;
                audioPlayer_wl.loop = true;
                break;
            case "win":
                index = 1;
                break;
            case "megaWin":
                index = 2;
                break;
        }
        StopWLAaudio();
        audioPlayer_wl.clip = clips[index];
        audioPlayer_wl.Play();

    }

 

    internal void PlayButtonAudio()
    {
        audioPlayer_button.Play();
    }

    internal void PlaySpinButtonAudio()
    {
        audioSpin_button.Play();
    }

    internal void StopWLAaudio()
    {
        audioPlayer_wl.Stop();
        audioPlayer_wl.loop = false;
    }



    internal void StopBgAudio()
    {
        bg_adudio.Stop();
    }

    internal void ChangeVol ( float value,string type)
    {
        switch (type)
        {
            case "bg":
                bg_adudio.mute = (value<0.1);
                bg_adudio.volume=value;
                break;
            case "button":
                audioPlayer_button.mute=(value<0.1);
                audioSpin_button.volume=value;
                break;
            case "wl":
                audioPlayer_wl.mute=(value<0.1);
                audioPlayer_wl.volume=value;
                break;
            case "all":
                audioPlayer_wl.mute = (value<0.1);
                bg_adudio.mute = (value<0.1);
                audioPlayer_button.mute = (value<0.1);
                audioSpin_button.mute = (value<0.1);

                audioPlayer_wl.volume = (value);
                bg_adudio.volume = (value);
                audioPlayer_button.volume = (value);
                audioSpin_button.volume = (value);

                break;
        }
    }

}
