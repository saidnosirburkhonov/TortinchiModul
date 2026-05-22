using System;
using System.Collections.Generic;

// !!! Predefined class name 'Solution' and method names !!!
//                  !!! DO NOT CHANGE !!!

public static class Solution
{
    public static IList<char> HowToUseCharSequenceGenerator(int count, char previous, char current)
    {
        ISequenceGenerator<char> generator = new CharSequenceGenerator(previous, current);
        var sequence = new List<char> { generator.Previous, generator.Current };
        for (int i = 1; i <= count; i++) { sequence.Add(generator.Next); }
        return sequence;
    }

    public static IList<int> HowToUseIntegerSequenceGenerator(int count, int previous, int current)
    {
        ISequenceGenerator<int> generator = new IntegerSequenceGenerator(previous, current);
        var sequence = new List<int> { generator.Previous, generator.Current };
        for (int i = 1; i <= count; i++) { sequence.Add(generator.Next); }
        return sequence;
    }

    public static IList<int> HowToUseFibonacciSequenceGenerator(int count, int previous, int current)
    {
        ISequenceGenerator<int> generator = new FibonacciSequenceGenerator(previous, current);
        var sequence = new List<int> { generator.Previous, generator.Current };
        for (int i = 1; i <= count; i++) { sequence.Add(generator.Next); }
        return sequence;
    }

    public static IList<double> HowToUseDoubleSequenceGenerator(int count, double previous, double current)
    {
        ISequenceGenerator<double> generator = new DoubleSequenceGenerator(previous, current);
        var sequence = new List<double> { generator.Previous, generator.Current };
        for (int i = 1; i <= count; i++) { sequence.Add(generator.Next); }
        return sequence;
    }

    public static IList<T> HowToUseDelegateSequenceGenerator<T>(int count, T previous, T current, Func<T, T, T> nextFunc)
    {
        ISequenceGenerator<T> generator = new DelegateSequenceGenerator<T>(previous, current, nextFunc);
        var sequence = new List<T>();

        if (count <= 0) return sequence;
        sequence.Add(generator.Previous);

        if (count == 1) return sequence;
        sequence.Add(generator.Current);

        while (sequence.Count < count)
        {
            sequence.Add(generator.Next);
        }

        return sequence;
    }
}

// --- INTERFACES & BASE CLASSES ---

public interface ISequenceGenerator<T>
{
    T Previous { get; }
    T Current { get; }
    T Next { get; }
}

public abstract class SequenceGenerator<T> : ISequenceGenerator<T>
{
    // FIX: Using private backing fields makes the properties strictly read-only
    private T _previous;
    private T _current;

    public T Previous { get { return _previous; } }
    public T Current { get { return _current; } }

    public int Count { get; private set; }

    public T Next
    {
        get
        {
            T next = GetNext();
            _previous = _current;
            _current = next;
            Count++;
            return next;
        }
    }

    protected SequenceGenerator(T previous, T current)
    {
        _previous = previous;
        _current = current;
        Count = 2;
    }

    protected abstract T GetNext();
}

// --- SPECIFIC IMPLEMENTATIONS ---

public class FibonacciSequenceGenerator : SequenceGenerator<int>
{
    public FibonacciSequenceGenerator(int previous, int current) : base(previous, current) { }

    protected override int GetNext()
    {
        return Previous + Current;
    }
}

public class IntegerSequenceGenerator : SequenceGenerator<int>
{
    public IntegerSequenceGenerator(int previous, int current) : base(previous, current) { }

    protected override int GetNext()
    {
        return 6 * Current - 8 * Previous;
    }
}

public class DoubleSequenceGenerator : SequenceGenerator<double>
{
    public DoubleSequenceGenerator(double previous, double current) : base(previous, current) { }

    protected override double GetNext()
    {
        return Current + Previous / Current;
    }
}
public class CharSequenceGenerator : SequenceGenerator<char>
{
    public CharSequenceGenerator(char previous, char current) : base(previous, current) { }

    protected override char GetNext()
    {
        int prev = Previous - 'A';
        int curr = Current - 'A';
        int next = (prev + curr) % 26;
        return (char)(next + 'A');
    }
}

public class DelegateSequenceGenerator<T> : SequenceGenerator<T>
{
    private readonly Func<T, T, T> _nextFunc;

    public DelegateSequenceGenerator(T previous, T current, Func<T, T, T> nextFunc) : base(previous, current)
    {
        _nextFunc = nextFunc;
    }

    protected override T GetNext()
    {
        return _nextFunc(Previous, Current);
    }
}