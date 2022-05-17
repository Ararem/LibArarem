using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LibEternal.Core.Logging.Enrichers;

/// <summary>
/// Enricher that adds a dictionary property to log events from the <see cref="Exception.Data"/> property
/// </summary>
public sealed class ExceptionDataEnricher : ILogEventEnricher
{
	/// <summary>
	/// Property name used for enriching
	/// </summary>
	public const string ExceptionDataProp = "ExceptionData";

	/// <inheritdoc />
	public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
	{
		if ((logEvent.Exception            == null) ||
			(logEvent.Exception.Data.Count == 0)) return;

		Dictionary<string, object?> dataDictionary = logEvent.Exception.Data
															.Cast<DictionaryEntry>()
															.Where(e => e.Key is string)
															.ToDictionary(e => (string)e.Key, e => e.Value);

		LogEventProperty? property = propertyFactory.CreateProperty(ExceptionDataProp, dataDictionary, true);

		logEvent.AddPropertyIfAbsent(property);
	}
}