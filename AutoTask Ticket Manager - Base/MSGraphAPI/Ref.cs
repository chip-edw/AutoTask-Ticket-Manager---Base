namespace AutoTaskTicketManager_Base.MSGraphAPI
{
    public class Ref<T>
    {
        public Ref() { }
        public Ref(T value) { Value = value; }
        public T Value { get; set; }
        public override string ToString()
        {
            T value = Value;
            return value == null ? "" : value.ToString();
        }
        public static implicit operator T(Ref<T> r) { return r.Value; }
        public static implicit operator Ref<T>(T value) { return new Ref<T>(value); }

    }
}
