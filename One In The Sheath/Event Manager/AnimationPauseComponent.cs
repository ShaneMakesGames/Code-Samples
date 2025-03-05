using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationPauseComponent : MonoBehaviour, IEventSubscriber
{
    public List<Animator> anims;
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
            foreach (Animator anim in anims)
            {
                anim.enabled = false;
            }
        }
        else if (eventType == EventType.HITSTOP_ENDED || eventType == EventType.GAME_UNPAUSED)
        {
            foreach (Animator anim in anims)
            {
                anim.enabled = true;
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