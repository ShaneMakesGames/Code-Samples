using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EventType
{
    HITSTOP_STARTED,
    HITSTOP_ENDED,
    GAME_PAUSED,
    GAME_UNPAUSED,
    COUNT
}

public interface IEventSubscriber
{
    void EventReceived(EventType eventType);
}