using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EffectController : MonoBehaviour
{
    public EventBus Bus { get; } = new EventBus();
    public List<EffectBase> ActiveEffects { get; } = new List<EffectBase>();

    public void AddEffect(EffectBase effect)// these ones are just thea application, called ones. sa eventbus yung actul nag ttrigger
    {
        var existing = ActiveEffects.FirstOrDefault(e => e.Id == effect.Id);
        if (existing != null)
        {
            // Simple additive stacking — swap for your own rule (refresh duration,
            // cap at a max, independent instances, etc.) per effect type.
            //existing.StackCount += effect.StackCount;
            return;
        }

        ActiveEffects.Add(effect);
        effect.Apply(this);
        Bus.Trigger(GameEvent.EffectApplied, effect);
    }

    public void RemoveEffect(EffectBase effect)
    {
        if (!ActiveEffects.Remove(effect)) return;
        effect.Remove();
        Bus.Trigger(GameEvent.EffectRemoved, effect);
    }

    // Call this from your turn manager for each unit at the start of its turn.
    public void OnTurnStart()
    {
        Bus.Trigger(GameEvent.TurnStart, this);
        TickDurations();
    }

    private void TickDurations()
    {
        foreach (var effect in ActiveEffects.ToList())
        {
            if (effect.Duration < 0) continue;
            effect.Duration--;
            if (effect.Duration <= 0)
                RemoveEffect(effect);
        }
    }

    // Placeholder — wire this up to your real health/damage system.
    public void TakeDamage(float amount)
    {
        Debug.Log($"{name} takes {amount} damage");
    }
}