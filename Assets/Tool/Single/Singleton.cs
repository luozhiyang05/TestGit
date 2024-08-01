namespace Tool.Single
{
    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        //懒汉式
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = new T();
                _instance.OnInit();
                return _instance;
            }
        }
        protected abstract void OnInit();
    }
}