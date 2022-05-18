using JetBrains.Annotations;
using System;
using System.Collections.Concurrent;

namespace LibEternal.Core.ObjectPools;

/// <summary>The base class for implementing a thread-safe object pool</summary>
/// <typeparam name="T">The type of object to pool for</typeparam>
[PublicAPI]
public abstract class ConcurrentObjectPoolBase<T> : IDisposable
{
	private readonly BlockingCollection<T> cache;

	/// <summary>The default constructor</summary>
	/// <param name="maxStored">The maximum number of items that can be stored</param>
	protected ConcurrentObjectPoolBase(int maxStored = 1000)
	{
		cache = new BlockingCollection<T>(maxStored);
		//Fill the cache
		//ReSharper disable once VirtualMemberCallInConstructor
		while (cache.Count != maxStored) cache.Add(CreateNew());
	}

	/// <summary>The maximum number of items that can be stored</summary>
	public int MaxStored => cache.BoundedCapacity;

	/// <inheritdoc/>
	public void Dispose()
	{
		cache.Dispose();
		GC.SuppressFinalize(this);
	}

	/// <summary>Gets an item of type <typeparamref name="T"/></summary>
	/// <returns>A cached object, or a new one if the cache was empty</returns>
	public virtual T Get() => cache.TryTake(out T? item) ? item : CreateNew();

	/// <summary></summary>
	/// <param name="obj"></param>
	public virtual void Return(T obj)
	{
		//Try and add it to the cache
		cache.TryAdd(obj);
	}

	/// <summary>Creates a new <typeparamref name="T"/>, often because one was requested but the cache was empty</summary>
	/// <returns>A new <typeparamref name="T"/> that can either be stored or used immediately</returns>
	protected abstract T CreateNew();
}