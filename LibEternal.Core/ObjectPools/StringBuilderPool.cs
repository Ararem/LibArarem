using JetBrains.Annotations;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Text;
using System.Threading;

namespace LibEternal.Core.ObjectPools;

/// <summary>
///  An object pool implementation of <see cref="Microsoft.Extensions.ObjectPool.ObjectPool"/>. but for a <see cref="System.Text.StringBuilder"/>
/// </summary>
/// <remarks>
///  This implementation keeps a cache of retained objects. This means that if objects are returned when the pool has already reached "maximumRetained"
///  objects they will be available to be Garbage Collected.
/// </remarks>
[PublicAPI]
public sealed class StringBuilderPool : ConcurrentObjectPoolBase<StringBuilder>
{
	/// <summary>The default initial capacity for when a <see cref="StringBuilder"/> is created</summary>
	private const int InitialCapacity = 1024;

	/// <summary>The maximum size of a <see cref="StringBuilder"/> that will be accepted when returned</summary>
	private const int MaxCapacity = 66536;

	/// <summary>The max number of <see cref="StringBuilder"/>s that will be cached</summary>
	private const int MaxCount = 64;

	/// <summary>The current singleton instance of a <see cref="StringBuilderPool"/></summary>
	private static readonly StringBuilderPool Instance = new();

	/// <summary>The number of <see cref="StringBuilder"/>s that had to be created because the cache was empty</summary>
	private ulong buildersCreated;

	/// <summary>The number of <see cref="StringBuilder"/>s that had to be discarded because the cache was full or they were too large</summary>
	private ulong buildersDiscarded;

	/// <summary>The number of <see cref="StringBuilder"/>s that were returned after use</summary>
	private ulong buildersReturned;

	private StringBuilderPool() : base(MaxCount)
	{
		buildersCreated = buildersDiscarded = buildersReturned = 0;
	}

	/// <summary>The number of <see cref="StringBuilder"/>s that were returned after use</summary>
	public ulong BuildersReturned => buildersReturned;

	/// <summary>The number of <see cref="StringBuilder"/>s that had to be created because the cache was empty</summary>
	public ulong BuildersCreated => buildersCreated;

	/// <summary>The number of <see cref="StringBuilder"/>s that had to be discarded because the cache was full or they were too large</summary>
	public ulong BuildersDiscarded => buildersDiscarded;

	/// <inheritdoc cref="DefaultObjectPool{T}.Get"/>
	[MustUseReturnValue]
	public static StringBuilder GetPooled() => Instance.Get();

	/// <inheritdoc cref="DefaultObjectPool{T}.Return"/>
	public static void ReturnPooled(StringBuilder obj)
	{
		Instance.Return(obj);
	}

	/// <summary>Gets the string stored in the <see cref="StringBuilder"/> and returns it to the pool</summary>
	/// <seealso cref="StringBuilder.ToString()"/>
	/// <seealso cref="ReturnPooled"/>
	[MustUseReturnValue]
	public static string ReturnToString(StringBuilder sb)
	{
		string result = sb.ToString();
		ReturnPooled(sb);
		return result;
	}

	/// <summary>An inline way to borrow a builder from the pool, modify it, return it to the pool and get the return value.</summary>
	/// <param name="borrowFunction">The function that will apply transformations to the <see cref="StringBuilder"/> to obtain the required output</param>
	/// <example>
	///  Simple:
	///  <code>
	///  Point p = new(69, 420);
	///  string toString = BorrowInline(sb => sb.AppendFormat("{0}", p));
	///  </code>
	///  Complex:
	///  <code>
	///  string result = BorrowInline(sb => sb
	/// 		.Append("Hello")
	/// 		.Append("There")
	/// 		.AppendLine("!")
	/// 		.AppendJoin(' ', "*cough*".ToUpper(), "General", "Kenobi", ".")
	/// 		);
	///  //Result == "HelloThere!\r\n*COUGH* General Kenobi ."
	/// 	</code>
	/// </example>
	[MustUseReturnValue]
	public static string BorrowInline([RequireStaticDelegate] Action<StringBuilder> borrowFunction)
	{
		StringBuilder sb = GetPooled();
		borrowFunction(sb);
		return ReturnToString(sb);
	}

	/// <inheritdoc cref="BorrowInline"/>
	/// <param name="argument">The argument passed into the <paramref name="borrowFunction"/></param>
	/// <param name="borrowFunction">he function that will apply transformations to the <see cref="StringBuilder"/> to obtain the required output</param>
	/// <typeparam name="T">The generic type of the <paramref name="argument"/></typeparam>
	/// <seealso cref="BorrowInline(Action{StringBuilder})"/>
	[MustUseReturnValue]
	public static string BorrowInline<T>([RequireStaticDelegate] Action<StringBuilder, T> borrowFunction, T argument)
	{
		StringBuilder sb = GetPooled();
		borrowFunction(sb, argument);
		return ReturnToString(sb);
	}

	/// <inheritdoc/>
	protected override StringBuilder CreateNew()
	{
		Interlocked.Increment(ref buildersCreated);
		return new StringBuilder(InitialCapacity);
	}

	/// <inheritdoc/>
	public override void Return(StringBuilder obj)
	{
		//Only return if small enough
		if (obj.Capacity <= MaxCapacity)
		{
			Interlocked.Increment(ref buildersReturned);
			obj.Clear();
			base.Return(obj);
		}
		else
		{
			Interlocked.Increment(ref buildersDiscarded);
		}
	}
}