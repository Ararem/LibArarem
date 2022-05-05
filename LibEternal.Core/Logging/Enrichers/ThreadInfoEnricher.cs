using JetBrains.Annotations;
using Serilog.Core;
using Serilog.Events;
using System.Threading;

namespace LibEternal.Core.Logging.Enrichers;

/// <inheritdoc/>
/// <summary>
///  Enriches log events with information about the <see cref="System.Threading.Thread.CurrentThread"/>
/// </summary>
[UsedImplicitly]
public sealed class ThreadInfoEnricher : ILogEventEnricher
{
	/// <summary>
	///  The name of the property for the thread name
	/// </summary>
	public const string ThreadNameProp = "ThreadName";

	/// <summary>
	///  The name of the property for the thread ID
	/// </summary>
	public const string ThreadIdProp = "ThreadId";

	/// <summary>
	///  The name of the property for the thread type
	/// </summary>
	public const string ThreadTypeProp = "ThreadType";

	/// <inheritdoc/>
	public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
	{
		Thread curr = Thread.CurrentThread;
		logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(ThreadNameProp, curr.Name ?? "Unnamed Thread")!);
		logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(ThreadIdProp,   curr.ManagedThreadId)!);
		string threadType = curr.IsThreadPoolThread ? "Dotnet Pool" : "User Thread";
		logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(ThreadTypeProp, threadType)!);
	}
}