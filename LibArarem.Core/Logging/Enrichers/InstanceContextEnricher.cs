using Serilog.Core;
using Serilog.Events;

namespace LibArarem.Core.Logging.Enrichers;

/// <summary>
/// Enricher that
/// </summary>
public sealed class InstanceContextEnricher : ILogEventEnricher
{
	/// <summary>
	///
	/// </summary>
	public const    string  InstanceContextProp = "InstanceContext";
	public readonly object? InstanceContext;
	public readonly bool    Destructure;

	public InstanceContextEnricher(object? instanceContext, bool destructure)
	{
		InstanceContext = instanceContext;
		Destructure     = destructure;
	}

	/// <inheritdoc />
	public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
	{
		logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(InstanceContextProp, InstanceContext, Destructure));
	}
}