using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static Dictionary<EventType, List<IEventSubscriber>> events;

    public void Awake()
    {
        events = new Dictionary<EventType, List<IEventSubscriber>>();

        for (int i = 0; i < (int)EventType.COUNT; i++)
        {
            events.Add((EventType)i, new List<IEventSubscriber>());
        }
    }

    public static void Subscribe(EventType eventType, IEventSubscriber eventSubscriber)
    {
        if (events[eventType].Contains(eventSubscriber)) return;

        events[eventType].Add(eventSubscriber);
    }

    public static void Unsubscribe(EventType eventType, IEventSubscriber eventSubscriber)
    {
        if (events[eventType].Contains(eventSubscriber))
        {
            events[eventType].Remove(eventSubscriber);
        }
    }


    public static void PublishEvent(EventType eventType)
    {
        foreach (IEventSubscriber eventSubscriber in events[eventType])
        {
            eventSubscriber.EventReceived(eventType);
        }
    }
}