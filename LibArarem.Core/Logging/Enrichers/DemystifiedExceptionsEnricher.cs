using JetBrains.Annotations;
using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;

namespace LibArarem.Core.Logging.Enrichers;

//Yes I did just copy and paste this from their github
/// <summary>An <see cref="ILogEventEnricher"/> that demystifies exceptions (courtesy of good ol ben adams!)</summary>
[UsedImplicitly]
public sealed class DemystifiedExceptionsEnricher : ILogEventEnricher
{
	/// <inheritdoc/>
	public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
	{
		logEvent.Exception?.Demystify();
	}
}