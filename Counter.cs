namespace MWH.KeyPressCounter;

public class Counter
{
    public int TotalCount { get; private set; }
    public int MaxPerInterval { get; private set; }
    public int LongestIntervalWithoutIncrement { get; private set; }

    private int _currentCount;
    private int _intervalsWithoutIncrement;

    public void Increment()
    {
        _currentCount++;
        TotalCount++;
        _intervalsWithoutIncrement = 0;
    }

    public void UpdateIntervalMetrics()
    {
        if (_currentCount > MaxPerInterval)
        {
            MaxPerInterval = _currentCount;
        }

        if (_currentCount == 0)
        {
            _intervalsWithoutIncrement++;
        }

        if (_intervalsWithoutIncrement > LongestIntervalWithoutIncrement)
        {
            LongestIntervalWithoutIncrement = _intervalsWithoutIncrement;
        }

        _currentCount = 0;
    }

    public void ResetTotalMetrics()
    {
        TotalCount = 0;
        MaxPerInterval = 0;
        LongestIntervalWithoutIncrement = 0;
    }

    public override string ToString()
    {
        return
            $"Total Count: {TotalCount}, Current Count: {_currentCount}, Max Per Interval: {MaxPerInterval}, Longest Interval Without Increment: {LongestIntervalWithoutIncrement}";
    }
}