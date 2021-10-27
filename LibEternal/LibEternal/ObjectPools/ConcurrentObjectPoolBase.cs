using System.Collections.Concurrent;

namespace LibEternal.ObjectPools
{
	/// <summary>
	/// The base class for implementing a thread-safe object pool
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class ConcurrentObjectPoolBase<T>
	{
		public virtual T Get()
		{
			return cache.TryTake(out T? item) ? item : CreateNew();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		public virtual void Return(T obj)
		{
			//Try and add it to the cache
			cache.TryAdd(obj);
		}

		/// <summary>
		/// Creates a new <typeparamref name="T"/>, often because one was requested but the cache was empty
		/// </summary>
		/// <returns>A new <typeparamref name="T"/> that can either be stored or used immediately</returns>
		protected abstract T CreateNew();
		private readonly BlockingCollection<T> cache;

		/// <summary>
		/// The maximum number of items that can be stored
		/// </summary>
		public int MaxStored => cache.BoundedCapacity;

		/// <summary>
		/// The default constructor
		/// </summary>
		/// <param name="maxStored">The maximum number of items that can be stored</param>
		protected ConcurrentObjectPoolBase(int maxStored = 1000)
		{
			cache = new BlockingCollection<T>(maxStored);
			//Fill the cache
			//ReSharper disable once VirtualMemberCallInConstructor
			while(cache.Count != maxStored) cache.Add(CreateNew());
		}
	}
}