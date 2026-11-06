using System;
using System.Collections.Generic;
public class EventBus
{
    private class Reactor
    {
        public int Priority;
        public Action<object> Callback;
    }

    private class Modifier
    {
        public int Priority;
        public Func<object, object> Callback;
    }

    private readonly Dictionary<GameEvent, List<Reactor>> reactors = new();
    private readonly Dictionary<GameEvent, List<Modifier>> modifiers = new();

    public void Subscribe(GameEvent evt, Action<object> callback, int priority = 0)
    {
        if (!reactors.ContainsKey(evt))
            reactors[evt] = new List<Reactor>();

        reactors[evt].Add(new Reactor { Priority = priority, Callback = callback });
        reactors[evt].Sort((a, b) => a.Priority.CompareTo(b.Priority));
    }

    public void Unsubscribe(GameEvent evt, Action<object> callback)
    {
        if (reactors.TryGetValue(evt, out var list))
            list.RemoveAll(r => r.Callback == callback);
    }

    public void Trigger(GameEvent evt, object context = null)
    {
        if (!reactors.TryGetValue(evt, out var list)) return;

        foreach (var reactor in new List<Reactor>(list))// these are for the ALREADY EXISTING effects that triggers when a new thing is applied. dont worry
            reactor.Callback(context);
    }

    public void SubscribeModifier(GameEvent evt, Func<object, object> callback, int priority = 0)
    {
        if (!modifiers.ContainsKey(evt))
            modifiers[evt] = new List<Modifier>();

        modifiers[evt].Add(new Modifier { Priority = priority, Callback = callback });
        modifiers[evt].Sort((a, b) => a.Priority.CompareTo(b.Priority));
    }

    public void UnsubscribeModifier(GameEvent evt, Func<object, object> callback)
    {
        if (modifiers.TryGetValue(evt, out var list))
            list.RemoveAll(m => m.Callback == callback);
    }

    public T Modify<T>(GameEvent evt, T value)
    {
        if (!modifiers.TryGetValue(evt, out var list)) return value;

        object current = value;
        foreach (var modifier in new List<Modifier>(list))
            current = modifier.Callback(current);
        return (T)current;
    }
}