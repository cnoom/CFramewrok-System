namespace CFramework.Data.Bindable
{
    public class BindableValue<T>
    {
        protected T value;

        public BindableValue(T value)
        {
            Value = value;
        }
        public T Value
        {
            get => value;
            set
            {
                if(this.value.Equals(value)) return;
                this.value = value;
                OnValueChangedEvent.Trigger(value);
            }
        }
        public EasyEvent<T> OnValueChangedEvent { get; } = new EasyEvent<T>();
    }
}