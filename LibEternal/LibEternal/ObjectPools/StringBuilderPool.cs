using JetBrains.Annotations;
using Microsoft.Extensions.ObjectPool;
using System.Text;
using System.Xml.XPath;

namespace LibEternal.ObjectPools
{
	/// <inheritdoc />
	/// <summary>
	/// Default implementation of <see cref="Microsoft.Extensions.ObjectPool.ObjectPool" />. but for a <see cref="System.Text.StringBuilder" />
	/// </summary>
	/// <remarks>This implementation keeps a cache of retained objects. This means that if objects are returned when the pool has already reached "maximumRetained" objects they will be available to be Garbage Collected.</remarks>
	public sealed class StringBuilderPool : DefaultObjectPool<StringBuilder>
	{
		/// <summary>
		/// The <see cref="IPooledObjectPolicy{T}"/> we use
		/// </summary>
		private static readonly StringBuilderPooledObjectPolicy Policy = new();

		/// <summary>
		/// The current singleton instance of a <see cref="StringBuilderPool"/>
		/// </summary>
		private static readonly StringBuilderPool Instance = new();

		/// <inheritdoc cref="DefaultObjectPool{T}.Get"/>
		[MustUseReturnValue]
		public static StringBuilder GetPooled()
		{
			return Instance.Get();
		}

		/// <inheritdoc cref="DefaultObjectPool{T}.Return"/>
		public static void ReturnPooled(StringBuilder obj)
		{
			Instance.Return(obj);
		}

		/// <summary>
		/// Returns the <see cref="StringBuilder"/> to the pool, and returns the string it just built.
		/// </summary>
		/// <param name="obj">The object to return</param>
		[MustUseReturnValue]
		public static string ToStringAndReturn(in StringBuilder obj)
		{
			string result = obj.ToString();
			ReturnPooled(obj);
			return result;
		}

		/// <inheritdoc />
		private StringBuilderPool() : base(Policy) //Just pass in the policy, nothing more to do
		{
		}
	}
}