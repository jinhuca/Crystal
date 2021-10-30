using System;
using System.Globalization;
using System.IO;
using static System.String;
using static Crystal.Constants.StringConstants;

namespace Crystal
{
	/// <summary>
	/// Implementation of <see cref="ILoggerFacade"/> that logs into a <see cref="TextWriter"/>.
	/// </summary>
	public class TextLogger : ILoggerFacade, IDisposable
	{
		private readonly TextWriter _writer;

		/// <summary>
		/// Initializes a new instance of <see cref="TextLogger"/> that writes to
		/// the console output.
		/// </summary>
		public TextLogger()
				: this(Console.Out)
		{
		}

		/// <summary>
		/// Initializes a new instance of <see cref="TextLogger"/>.
		/// </summary>
		/// <param name="writer">The _writer to use for writing log entries.</param>
		public TextLogger(TextWriter writer)
		{
			_writer = writer ?? throw new ArgumentNullException(nameof(writer));
		}

		/// <summary>
		/// Write a new log entry with the specified category and priority.
		/// </summary>
		/// <param name="message">Message body to log.</param>
		/// <param name="category">Category of the entry.</param>
		/// <param name="priority">The priority of the entry.</param>
		public void Log(string message, Category category, Priority priority)
		{
			var messageToLog = Format(DefaultTextLoggerPattern, DateTime.Now,
																					category.ToString().ToUpper(CultureInfo.InvariantCulture), message, priority.ToString());

			_writer.WriteLine(messageToLog);
		}

		/// <summary>
		/// Disposes the associated <see cref="TextWriter"/>.
		/// </summary>
		/// <param name="disposing">When <see langword="true"/>, disposes the associated <see cref="TextWriter"/>.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_writer != null)
				{
					_writer.Dispose();
				}
			}
		}

		///<summary>
		///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		///</summary>
		/// <remarks>Calls <see cref="Dispose(bool)"/></remarks>.
		///<filterpriority>2</filterpriority>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
