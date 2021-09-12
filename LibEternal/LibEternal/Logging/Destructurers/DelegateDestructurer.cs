using JetBrains.Annotations;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Linq;
using System.Reflection;

namespace LibEternal.Logging.Destructurers
{
	/// <inheritdoc />
	[PublicAPI]
	public sealed class DelegateDestructurer : IDestructuringPolicy
	{
		/// <inheritdoc />
		public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue? result)
		{
			if (value is not Delegate del)
			{
				result = null;
				return false;
			}
			if (del is MulticastDelegate multicast)
			{
				var delegates = multicast.GetInvocationList();
				result = new SequenceValue(delegates.Select(m => propertyValueFactory.CreatePropertyValue(m.Method)));
				return true;
			}
			//If it's not multicast we know it only has a single target, so m.MethodInfo will be accurate
			else
			{
				MethodInfo m = del.Method;
				result = propertyValueFactory.CreatePropertyValue(m);
				return true;
			}
		}
	}
}