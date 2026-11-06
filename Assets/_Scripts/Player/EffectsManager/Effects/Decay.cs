public class Decay : EffectBase
{
    public override string Id => "decay";
    private const float PoiseDamageMultiplier =100f;

    protected override void OnApply(EffectController owner)
    {
        owner.Bus.SubscribeModifier(GameEvent.DamageTaken, IncreasePoiseDamageTaken, priority: 0);
    }

    protected override void OnRemove(EffectController owner)
    {
        owner.Bus.UnsubscribeModifier(GameEvent.DamageTaken, IncreasePoiseDamageTaken);
    }

    private object IncreasePoiseDamageTaken(object value)
    {
        float amount = (float)value;
        return amount * PoiseDamageMultiplier;
    }
}