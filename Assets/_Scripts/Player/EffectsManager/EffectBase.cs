public abstract class EffectBase
{
    public abstract string Id { get; }
    public int Priority = 0;
    public int StackCount = 1;
    public int Duration = -1; // -1 = indefinite, otherwise turns remaining

    protected EffectController Owner { get; private set; }

    public void Apply(EffectController owner)
    {
        Owner = owner;
        OnApply(owner);
    }

    public void Remove()
    {
        OnRemove(Owner);
    }

    protected abstract void OnApply(EffectController owner);
    protected abstract void OnRemove(EffectController owner);
}