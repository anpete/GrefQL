﻿using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace GrefQL.Tests
{
    public sealed class TestOutputHelperLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public TestOutputHelperLoggerProvider(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public ILogger CreateLogger(string categoryName) => new Logger(_testOutputHelper);

        public void Dispose()
        {
        }

        private sealed class Logger : ILogger
        {
            private readonly ITestOutputHelper _testOutputHelper;

            public Logger(ITestOutputHelper testOutputHelper)
            {
                _testOutputHelper = testOutputHelper;
            }

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception exception,
                Func<TState, Exception, string> formatter)
            {
                if (logLevel == LogLevel.Information)
                {
                    _testOutputHelper.WriteLine(formatter(state, exception));
                }
            }

            public bool IsEnabled(LogLevel logLevel) => logLevel == LogLevel.Information;

            public IDisposable BeginScopeImpl(object state) => null;
        }
    }
}
