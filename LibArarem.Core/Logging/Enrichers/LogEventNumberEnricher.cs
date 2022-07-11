using JetBrains.Annotations;
using Serilog.Core;
using Serilog.Events;
using System.Threading;

namespace LibArarem.Core.Logging.Enrichers;

/// <inheritdoc/>
/// <summary>Enriches log events with a counter that counts how many log events have been logged.</summary>
/// <remarks>
///  This class is <see cref="System.Threading.Thread"/> safe, but contains (and modifies) global state, so use with multiple
///  <see cref="Serilog.ILogger"/>s is not supported
/// </remarks>
[UsedImplicitly]
public sealed class LogEventNumberEnricher : ILogEventEnricher
{
	/// <summary>The name of the property for the event number</summary>
	public const string EventNumberProp = "EventNumber";

	private static long counter = 0;

	/// <inheritdoc/>
	public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
	{
		//Increment our counter in a 'thread safe' manner
		long             c        = Interlocked.Increment(ref counter);
		LogEventProperty property = propertyFactory.CreateProperty(EventNumberProp, c)!;
		logEvent.AddOrUpdateProperty(property);
	}
}