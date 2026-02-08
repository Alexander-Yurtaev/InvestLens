using CorrelationId.Abstractions;
using InvestLens.Shared.Constants;
using InvestLens.Shared.Interfaces.Services;

namespace InvestLens.Shared.Services;

public class CorrelationIdService : ICorrelationIdService
{
    private readonly ICorrelationContextAccessor _correlationContextAccessor;

    public CorrelationIdService(ICorrelationContextAccessor correlationContextAccessor)
    {
        _correlationContextAccessor = correlationContextAccessor;
    }

    public string GetOrCreateCorrelationId(string prefix)
    {
        var correlationId = _correlationContextAccessor.CorrelationContext?.CorrelationId;

        if (!string.IsNullOrEmpty(correlationId)) return correlationId;

        correlationId = $"{prefix}-{Guid.NewGuid():N}";
        _correlationContextAccessor.CorrelationContext =
            new CorrelationId.CorrelationContext(correlationId, HeaderConstants.CorrelationHeader);

        return correlationId;
    }

    public void SetCorrelationId(string correlationId)
    {
        _correlationContextAccessor.CorrelationContext =
            new CorrelationId.CorrelationContext(correlationId, HeaderConstants.CorrelationHeader);
    }
}