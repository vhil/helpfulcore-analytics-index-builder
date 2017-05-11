namespace Helpfulcore.AnalyticsIndexBuilder.Collections
{
    public interface ILazyUniqueItem<out TItem>
    {
        string UniqueId { get; }
        TItem Value { get; }
    }
}
