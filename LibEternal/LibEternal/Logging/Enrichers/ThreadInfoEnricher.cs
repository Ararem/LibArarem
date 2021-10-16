using JetBrains.Annotations;
using Serilog.Core;
using Serilog.Events;
using System.Threading;

namespace LibEternal.Logging.Enrichers
{
	/// <inheritdoc />
	/// <summary>
	/// Enriches log events with information about the <see cref="System.Threading.Thread.CurrentThread" />
	/// </summary>
	[UsedImplicitly]
	public sealed class ThreadInfoEnricher : ILogEventEnricher
	{
		public const string ThreadNameProp = "ThreadName";
		public const string ThreadIdProp   = "ThreadId";
		public const string ThreadTypeProp = "ThreadType";

		/// <inheritdoc />
		public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
		{
			Thread curr = Thread.CurrentThread;
			logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(ThreadNameProp, curr.Name ?? "Unnamed Thread"));
			logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(ThreadIdProp, curr.ManagedThreadId));
			string threadType;
			// if (MicroThread.Current is not null)
			// threadType = "MicroThread";
			// ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
			if (curr.IsThreadPoolThread)
				threadType = "Dotnet Pool";
			// else if (ThreadPool.IsWorkedThread)
			// threadType = "Stride Pool";
			//Default is user thread
			else
				threadType = "User Thread";
			logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(ThreadTypeProp, threadType));
		}
	}
}