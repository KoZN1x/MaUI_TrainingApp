using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Application.CQRS.Queries.GetTrainingPlans;

internal sealed class GetTrainingPlansQueryHandler : IQueryHandler<GetTrainingPlansQuery, GetTrainingPlansResult>
{
    private readonly ITrainingPlanReadRepository _trainingPlans;

    public GetTrainingPlansQueryHandler(ITrainingPlanReadRepository trainingPlans)
    {
        _trainingPlans = trainingPlans;
    }

    public async Task<GetTrainingPlansResult> HandleAsync(
        GetTrainingPlansQuery query,
        CancellationToken cancellationToken = default)
    {
        return new GetTrainingPlansResult(await _trainingPlans.GetAllAsync(cancellationToken));
    }
}
