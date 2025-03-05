using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public int currentSongIndex;

    public List<AudioSource> musicSourceList = new List<AudioSource>();

    public const float DEFAULT_VOLUME = 0.15f;
    public const float FADE_TIME = 0.69f;

    #region Singleton

    public static MusicManager singleton;

    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
        }
        else Destroy(this.gameObject);
    }

    #endregion

    /// <summary>
    /// Fades old song out and fades the new song in
    /// </summary>
    /// <param name="songIndex"></param>
    public static void TryChangeSong(int songIndex)
    {
        if (songIndex == singleton.currentSongIndex) return;
        if (songIndex >= singleton.musicSourceList.Count) return;

        AudioSource prevAudioSource = singleton.musicSourceList[singleton.currentSongIndex];
        singleton.StartCoroutine(singleton.FadeToVolumeOverTimeCoroutine(prevAudioSource,0));

        singleton.currentSongIndex = songIndex;
        AudioSource currentAudioSource = singleton.musicSourceList[singleton.currentSongIndex];
        singleton.StartCoroutine(singleton.FadeToVolumeOverTimeCoroutine(currentAudioSource, DEFAULT_VOLUME));
    }

    public IEnumerator FadeToVolumeOverTimeCoroutine(AudioSource audioSource, float newVolume)
    {
        float startingVolume = audioSource.volume;
        float timePassed = 0;
        while (timePassed < FADE_TIME)
        {
            float lerpedVolume = Mathf.Lerp(startingVolume, newVolume, timePassed / FADE_TIME);
            audioSource.volume = lerpedVolume;
            timePassed += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = newVolume;  
    }
}