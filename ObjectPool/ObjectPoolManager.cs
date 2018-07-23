namespace ObjectPool
{
    using System;
    using System.Collections.Concurrent;

    public class ObjectPoolManager
    {
        private readonly ConcurrentDictionary<Type, object> containers;

        public ObjectPoolManager()
        {
            this.containers = new ConcurrentDictionary<Type, object>();
        }

        public bool Add<T>(T item) where T : class
        {
            if (this.GetContainer<T>(out var container))
            {
                container.Push(item);
                return true;
            }

            return false;
        }

        public bool Add<T>(T[] items) where T : class
        {
            if (this.GetContainer<T>(out var container))
            {
                container.PushRange(items);
                return true;
            }

            return false;
        }

        public bool Get<T>(out T item) where T : class
        {
            if (this.GetContainer<T>(out var container))
            {
                item = container.Pop();
                return true;
            }

            item = null;
            return false;
        }

        public bool GetRange<T>(out T[] items, int itemsCount) where T : class
        {
            if (this.GetContainer<T>(out var container))
            {
                items = container.PopRange(itemsCount);
                return true;
            }

            items = null;
            return false;
        }

        public void Register<T>() where T : class
        {
            this.Register(typeof(T));
        }

        public bool Register(Type type)
        {
            var container = this.CreateContainer(type);
            return this.containers.TryAdd(type, container);
        }

        public void Register<T>(int itemsTreshold) where T : class
        {
            this.Register(typeof(T), itemsTreshold);
        }

        public bool Register(Type type, int itemsTreshold)
        {
            var container = this.CreateContainer(type, itemsTreshold);
            return this.containers.TryAdd(type, container);
        }

        public void Register<T>(Func<T> createItemFunc) where T : class
        {
            this.Register(typeof(T), createItemFunc);
        }

        public bool Register(Type type, object createItemFunc)
        {
            var container = this.CreateContainer(type, createItemFunc);
            return this.containers.TryAdd(type, container);
        }

        public void Register<T>(int itemsTreshold, Func<T> createItemFunc) where T : class
        {
            this.Register(typeof(T), itemsTreshold, createItemFunc);
        }

        public bool Register(Type type, int itemsTreshold, object createItemFunc)
        {
            var container = this.CreateContainer(type, itemsTreshold, createItemFunc);
            return this.containers.TryAdd(type, container);
        }

        private object CreateContainer(Type type, params object[] args)
        {
            if (!type.IsClass)
            {
                throw new ArgumentException("Type must be a class.");
            }

            var genericType = typeof(ObjectPoolContainer<>).MakeGenericType(type);
            return Activator.CreateInstance(genericType, args);
        }

        private bool GetContainer<T>(out ObjectPoolContainer<T> continater)
        {
            var res = this.containers.TryGetValue(typeof(T), out var continaterObj);
            continater = continaterObj as ObjectPoolContainer<T>;
            return res;
        }
    }
}