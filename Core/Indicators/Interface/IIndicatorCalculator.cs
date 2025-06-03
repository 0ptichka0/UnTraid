namespace UnTraid.Core.Indicators.Interface
{
    public interface IIndicatorCalculator<T>
    {
        T Calculate();
        string Name { get; } // Для идентификации индикатора
    }
}
