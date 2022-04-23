using Avalonia.Logging;
using Serilog;
using Serilog.Core.Enrichers;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace SS14.Launcher;

internal sealed class AvaloniaSeriLogger : ILogSink
{
    private readonly ILogger _logger;

    public AvaloniaSeriLogger(ILogger logger)
    {
        _logger = logger;
    }

    private ILogger Context(string area, object? source)
    {
        return _logger.ForContext(new[]
        {
            new PropertyEnricher("Area", area),
            new PropertyEnricher("SourceHash", source?.GetHashCode().ToString("X8")),
            new PropertyEnricher("SourceType", source?.GetType())
        });
    }

    public bool IsEnabled(LogEventLevel level, string area)
    {
        return _logger.IsEnabled((Serilog.Events.LogEventLevel) level);
    }

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate)
    {
        Context(area, source).Write((Serilog.Events.LogEventLevel) level, messageTemplate);
    }

    public void Log<T0>(
        LogEventLevel level,
        string area,
        object? source,
        string messageTemplate,
        T0 propertyValue0)
    {
        Context(area, source).Write((Serilog.Events.LogEventLevel) level, messageTemplate, propertyValue0);
    }

    public void Log<T0, T1>(
        LogEventLevel level,
        string area,
        object? source,
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1)
    {
        Context(area, source).Write((Serilog.Events.LogEventLevel) level, messageTemplate, propertyValue0, propertyValue1);
    }

    public void Log<T0, T1, T2>(
        LogEventLevel level,
        string area,
        object? source,
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2)
    {
        Context(area, source).Write((Serilog.Events.LogEventLevel) level, messageTemplate, propertyValue0, propertyValue1,
            propertyValue2);
    }

    public void Log(
        LogEventLevel level,
        string area,
        object? source,
        string messageTemplate,
        params object?[] propertyValues)
    {
        Context(area, source).Write((Serilog.Events.LogEventLevel) level, messageTemplate, propertyValues);
    }
}
