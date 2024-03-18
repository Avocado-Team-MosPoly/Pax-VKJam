public class VariableObserver<T>
{
    public event System.Action<T> ValueChanged;

    public T Value
    {
        get => value;
        set
        {
            this.value = value;
            ValueChanged?.Invoke(value);
        }
    }
    private T value;

    public VariableObserver(T value, System.Action<T> valueChanged = null)
    {
        this.value = value;
        ValueChanged = valueChanged;
    }
}