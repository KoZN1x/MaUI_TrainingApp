using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Application.CQRS.Queries.GetProgressSummary;

internal sealed class GetProgressSummaryQueryHandler
    : IQueryHandler<GetProgressSummaryQuery, GetProgressSummaryResult>
{
    private readonly IProgressReadRepository _progress;

    public GetProgressSummaryQueryHandler(IProgressReadRepository progress)
    {
        _progress = progress;
    }

    public async Task<GetProgressSummaryResult> HandleAsync(
        GetProgressSummaryQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.To < query.From)
        {
            throw new InvariantException("Progress period end couldn't be earlier than its start");
        }

        return new GetProgressSummaryResult(
            await _progress.GetSummaryAsync(query.From, query.To, cancellationToken));
    }
}
