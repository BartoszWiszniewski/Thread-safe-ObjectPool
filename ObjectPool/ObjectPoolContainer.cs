namespace ObjectPool
{
    using System;
    using System.Collections.Concurrent;

    using ObjectPool.Extensions;

    public class ObjectPoolContainer<T>
    {
        private const int DefaultItemsTreshold = 50;
        private readonly ConcurrentStack<T> items;
        private readonly int itemsTreshold;
        private Func<T> createItemFunc;

        public ObjectPoolContainer() : this(DefaultItemsTreshold)
        {
        }

        public ObjectPoolContainer(int itemsTreshold) : this(itemsTreshold, null)
        {
        }

        public ObjectPoolContainer(Func<T> createItemFunc) : this(DefaultItemsTreshold, createItemFunc)
        {
        }

        /// <param name="itemsTreshold">if set to equal below 0 will be set to int.MaxValue</param>
        public ObjectPoolContainer(int itemsTreshold, Func<T> createItem)
        {
            this.items = new ConcurrentStack<T>();
            this.itemsTreshold = itemsTreshold;
            if (itemsTreshold <= 0)
            {
                itemsTreshold = int.MaxValue;
            }

            if (createItem != null)
            {
                this.createItemFunc = createItem;
            }
            else
            {
                this.createItemFunc = this.DefaultItemCreator();
            }
        }

        public virtual T Pop()
        {
            if (this.items.TryPop(out var result))
            {
                return result;
            }

            return this.createItemFunc();
        }

        public virtual T[] PopRange(int itemsCount)
        {
            var result = new T[itemsCount];
            var popedCount = this.items.TryPopRange(result);
            if (popedCount < itemsCount)
            {
                var toCreate = itemsCount - popedCount;
                for (var i = 0; i < toCreate; i++)
                {
                    result[i + popedCount] = this.createItemFunc();
                }
            }

            return result;
        }

        public virtual void Push(T item)
        {
            if (this.items.Count + 1 < this.itemsTreshold)
            {
                this.items.Push(item);
            }
        }

        public virtual void PushRange(T[] items)
        {
            var itemsToPush = this.itemsTreshold - this.items.Count;
            if (itemsToPush <= 0)
            {
                return;
            }

            this.items.PushRange(items, 0, itemsToPush);
        }

        public void RegisterInitializer(Func<T> createItem)
        {
            this.createItemFunc = createItem;
        }

        private Func<T> DefaultItemCreator()
        {
            var type = typeof(T);
            if (type.IsNullable())
            {
                if (type.HasEmptyConstructor())
                {
                    return () => type.New<T>();
                }
            }

            return () => default(T);
        }
    }
}