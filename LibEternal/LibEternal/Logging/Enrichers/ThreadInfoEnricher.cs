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
		// ReSharper disable InternalOrPrivateMemberNotDocumented
		private const string ThreadNamePropertyName = "ThreadName";
		private const string ThreadIdPropertyName   = "ThreadId";
		private const string ThreadTypePropertyName = "ThreadType";
		// ReSharper restore InternalOrPrivateMemberNotDocumented

		/// <inheritdoc />
		public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
		{
			Thread curr = Thread.CurrentThread;
			logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(ThreadNamePropertyName, curr.Name ?? "Unnamed Thread"));
			logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(ThreadIdPropertyName, curr.ManagedThreadId));
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
			logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(ThreadTypePropertyName, threadType));
		}
	}
}