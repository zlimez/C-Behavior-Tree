namespace Utils
{
    public class Lazy<T> where T : class, ILazyInit
    {
        private bool _initialized;
        private readonly T _value;

        public Lazy(T value)
        {
            _value = value;
            _initialized = false;
        }

        public T Value()
        {
            if (_initialized) return _value;
            _value.Init();
            _initialized = true;
            return _value;
        }
    }

    public interface ILazyInit { public void Init(); }
}
