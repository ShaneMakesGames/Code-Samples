using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemPauseComponent : MonoBehaviour, IEventSubscriber
{
    public List<ParticleSystem> particleSystems;
    public bool pauseOnHitstop;

    public void Awake()
    {
        if (pauseOnHitstop)
        {
            EventManager.Subscribe(EventType.HITSTOP_STARTED, this);
            EventManager.Subscribe(EventType.HITSTOP_ENDED, this);
        }
        EventManager.Subscribe(EventType.GAME_PAUSED, this);
        EventManager.Subscribe(EventType.GAME_UNPAUSED, this);
    }

    void IEventSubscriber.EventReceived(EventType eventType)
    {
        if (eventType == EventType.HITSTOP_STARTED || eventType == EventType.GAME_PAUSED)
        {
            foreach (ParticleSystem ps in particleSystems)
            {
                ps.Pause(true);
            }
        }
        else if (eventType == EventType.HITSTOP_ENDED || eventType == EventType.GAME_UNPAUSED)
        {
            foreach (ParticleSystem ps in particleSystems)
            {
                ps.Play();
            }
        }
    }

    private void OnDestroy()
    {
        EventManager.Unsubscribe(EventType.HITSTOP_STARTED, this);
        EventManager.Unsubscribe(EventType.HITSTOP_ENDED, this);
        EventManager.Unsubscribe(EventType.GAME_PAUSED, this);
        EventManager.Unsubscribe(EventType.GAME_UNPAUSED, this);
    }
}