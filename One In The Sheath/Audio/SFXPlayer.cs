using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundState
{
    Playing,
    Paused,
    Finished
}

public class SFXPlayer : MonoBehaviour
{
    public AudioSource source;
    public SoundState soundState;
    public float timePassed;
    public bool isLoop;

    public void PlaySFX(AudioClip sfx, bool randomizePitch)
    {
        sfxCoroutine = StartCoroutine(PlaySFXCoroutine(sfx, randomizePitch));
    }

    public Coroutine sfxCoroutine;
    private IEnumerator PlaySFXCoroutine(AudioClip sfx, bool randomizePitch)
    {
        soundState = SoundState.Playing;
        SFXSystem.singleton.sfxPlayerPool.Remove(this);
        SFXSystem.singleton.activeSFXPlayers.Add(this);

        if (randomizePitch) source.pitch = Random.Range(0.75f, 1.25f);

        source.clip = sfx;
        source.Play();

        timePassed = 0;
        while (timePassed < sfx.length)
        {
            timePassed += Time.deltaTime;
            yield return null;
        }

        CleanUp();
    }

    public void PlaySFXOnLoop(AudioClip sfx, bool randomizePitch)
    {
        isLoop = true;
        soundState = SoundState.Playing;
        SFXSystem.singleton.sfxPlayerPool.Remove(this);
        SFXSystem.singleton.activeSFXPlayers.Add(this);

        if (randomizePitch) source.pitch = Random.Range(0.75f, 1.25f);

        source.clip = sfx;
        source.loop = true;
        source.Play();
    }

    public void PauseSFX()
    {
        if (soundState != SoundState.Playing) return;

        soundState = SoundState.Paused;
        source.Pause();
    }

    public void UnpauseSFX()
    {
        sfxCoroutine = StartCoroutine(ResumeSFXCoroutine());
    }

    private IEnumerator ResumeSFXCoroutine()
    {
        if (soundState != SoundState.Paused) yield break;

        soundState = SoundState.Playing;
        source.UnPause();

        if (isLoop) yield break;

        while (timePassed < source.clip.length)
        {
            timePassed += Time.deltaTime;
            yield return null;
        }

        CleanUp();
    }

    public void CleanUp()
    {
        if (soundState == SoundState.Finished) return;

        source.Stop();
        source.pitch = 1;
        source.loop = false;
        source.clip = null;
        soundState = SoundState.Finished;
        timePassed = 0;
        isLoop = false;
        if (sfxCoroutine != null) StopCoroutine(sfxCoroutine);

        if (!SFXSystem.singleton.sfxPlayerPool.Contains(this)) SFXSystem.singleton.sfxPlayerPool.Add(this);
        if (SFXSystem.singleton.activeSFXPlayers.Contains(this)) SFXSystem.singleton.activeSFXPlayers.Remove(this);
    }
}