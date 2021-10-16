using JetBrains.Annotations;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Linq;

namespace LibEternal.Logging.Destructurers
{
	/// <inheritdoc />
	[PublicAPI]
	public sealed class DelegateDestructurer : IDestructuringPolicy
	{
		//Destructures delegates into their MethodInfos they target
		/// <inheritdoc />
		public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue? result)
		{
			if (value is not Delegate del)
			{
				result = null;
				return false;
			}

			var delegates = del.GetInvocationList();
			//Don't want to treat it as a list if there's only 1 element
			result = delegates.Length == 1
					? propertyValueFactory.CreatePropertyValue(delegates[0].Method)
					: propertyValueFactory.CreatePropertyValue(delegates.Select(m => m.Method));
			return true;
		}
	}
}