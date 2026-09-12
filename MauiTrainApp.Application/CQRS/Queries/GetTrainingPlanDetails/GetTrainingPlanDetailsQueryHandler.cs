using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Application.CQRS.Queries.GetTrainingPlanDetails;

internal sealed class GetTrainingPlanDetailsQueryHandler
    : IQueryHandler<GetTrainingPlanDetailsQuery, GetTrainingPlanDetailsResult>
{
    private readonly ITrainingPlanReadRepository _trainingPlans;

    public GetTrainingPlanDetailsQueryHandler(ITrainingPlanReadRepository trainingPlans)
    {
        _trainingPlans = trainingPlans;
    }

    public async Task<GetTrainingPlanDetailsResult> HandleAsync(
        GetTrainingPlanDetailsQuery query,
        CancellationToken cancellationToken = default)
    {
        return new GetTrainingPlanDetailsResult(
            await _trainingPlans.GetByIdAsync(query.TrainingPlanId, cancellationToken));
    }
}
