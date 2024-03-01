public class VariableObserver<T>
{
    public event System.Action<T> ValueChanged;

    private T value;
    public T Value
    {
        get => value;
        set
        {
            this.value = value;
            ValueChanged?.Invoke(value);
        }
    }

    public VariableObserver(T value, System.Action<T> valueChanged = null)
    {
        this.value = value;
        ValueChanged = valueChanged;
    }
}