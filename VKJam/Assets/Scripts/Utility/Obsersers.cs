using UnityEngine.Events;

public class VariableObserver<T>
{
    public VariableObserver(T value)
    {
        this.value = value;
    }

    private T value;

    public T Value
    {
        get
        {
            return value;
        }
        set
        {
            this.value = value;
            OnValueChanged?.Invoke(value);
        }
    }

    public UnityEvent<T> OnValueChanged { get; private set; } = new();
}