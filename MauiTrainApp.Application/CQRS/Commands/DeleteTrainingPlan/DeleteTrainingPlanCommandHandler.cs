using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.DeleteTrainingPlan;

internal sealed class DeleteTrainingPlanCommandHandler
    : ICommandHandler<DeleteTrainingPlanCommand, DeleteTrainingPlanResult>
{
    private readonly IWriteRepository<TrainingPlan> _trainingPlans;

    public DeleteTrainingPlanCommandHandler(IWriteRepository<TrainingPlan> trainingPlans)
    {
        _trainingPlans = trainingPlans;
    }

    public async Task<DeleteTrainingPlanResult> HandleAsync(
        DeleteTrainingPlanCommand command,
        CancellationToken cancellationToken = default)
    {
        var trainingPlan = await _trainingPlans.GetByIdAsync(command.TrainingPlanId, cancellationToken)
            ?? throw NotFoundException.For<TrainingPlan>(command.TrainingPlanId);

        await _trainingPlans.DeleteAsync(trainingPlan, cancellationToken);

        return new DeleteTrainingPlanResult(trainingPlan.Id);
    }
}
