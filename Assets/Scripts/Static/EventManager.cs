using System.Collections.Generic;
using UnityEngine;
public delegate void EventHandler();
public delegate void EventHandlerArg(BecomeEvent obj);
public delegate void EventHandlerArgs(BecomeEvent[] becomeEvents);

#pragma warning disable

public class EventManager  {
    private static readonly Dictionary<string, EventHandler> eventHandlers = new Dictionary<string, EventHandler>();
    private static readonly Dictionary<string, EventHandlerArg> eventHandlersArg = new Dictionary<string, EventHandlerArg>();
    private static readonly Dictionary<string, EventHandlerArgs> eventHandlersArgs = new Dictionary<string, EventHandlerArgs>();

    public static void AddListener(string eventType, EventHandler handler)
    {
        if (eventHandlers.ContainsKey(eventType))
        {
            eventHandlers[eventType] += handler;
        }
        else
        {
            eventHandlers.Add(eventType, handler);
        }
    }

    public static void RemoveListener(string eventType, EventHandler handler)
    {
        if (eventHandlers.ContainsKey(eventType))
        {
            eventHandlers[eventType] -= handler;
        }
    }

    public static void AddListener(string eventType, EventHandlerArg handler)
    {
        if (eventHandlersArg.ContainsKey(eventType))
        {
            eventHandlersArg[eventType] += handler;
        }
        else
        {
            eventHandlersArg.Add(eventType, handler);
        }
    }

    public static void RemoveListener(string eventType, EventHandlerArg handler)
    {
        if (eventHandlersArg.ContainsKey(eventType))
        {
            eventHandlersArg[eventType] -= handler;
        }
        else
        {
            Debug.LogWarning("you tried to remove event named: " + eventType + " , but it's absent or it was removed.");
        }
    }

    public static void Invoke(string eventType)
    {
        if (eventHandlers.ContainsKey(eventType))
        {
            eventHandlers[eventType]();
        }
        else
        {
            Debug.LogWarning("Вызываемый эвент не зарегистрирован: " + eventType);
        }
    }

    public static void Invoke(string eventType, BecomeEvent argument)
    {
        if (eventHandlersArg.ContainsKey(eventType))
        {
            eventHandlersArg[eventType](argument);
        }
        else
        {
            Debug.LogWarning("Вызываемый эвент не зарегистрирован: " + eventType);
        }
    }

}
public struct BecomeEvent
{
    
    public bool come;
    public int id;
    public int power;
    public BecomeEvent(bool _come, int _id, int _power)
    {
        come = _come;
        id = _id;
        power = _power;
    }
}