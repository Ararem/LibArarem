using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LibArarem.Core.Logging.Destructurers;

public class IncludePublicFieldsDestructurer : IDestructuringPolicy
{
	/// <inheritdoc/>
	public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
	{
		Type typeInfo = value.GetType();
		IEnumerable<LogEventProperty> fieldsWithValues = typeInfo.GetFields(BindingFlags.Public | BindingFlags.Instance).Select(
				f =>
				{
					try
					{
						object?                val     = f.GetValue(value);
						LogEventPropertyValue? propVal = propertyValueFactory.CreatePropertyValue(val);
						LogEventProperty       ret     = new(f.Name, propVal);
						return ret;
					}
					catch (Exception)
					{
						return null;
					}
				}
		).Where(static o => o is not null)!;

		IEnumerable<LogEventProperty> propertiesWithValues = typeInfo.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(f => f.CanRead).Select(
				p =>
				{
					try
					{
						object?                val     = p.GetValue(value);
						LogEventPropertyValue? propVal = propertyValueFactory.CreatePropertyValue(val);
						LogEventProperty       ret     = new(p.Name, propVal);
						return ret;
					}
					catch (Exception)
					{
						return null;
					}
				}
		).Where(static o => o is not null)!;

		result = new StructureValue(fieldsWithValues.Union(propertiesWithValues));
		return true;
	}
}