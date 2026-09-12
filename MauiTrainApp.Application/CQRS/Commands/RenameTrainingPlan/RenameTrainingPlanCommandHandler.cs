using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.RenameTrainingPlan;

internal sealed class RenameTrainingPlanCommandHandler
    : ICommandHandler<RenameTrainingPlanCommand, RenameTrainingPlanResult>
{
    private readonly IWriteRepository<TrainingPlan> _trainingPlans;

    public RenameTrainingPlanCommandHandler(IWriteRepository<TrainingPlan> trainingPlans)
    {
        _trainingPlans = trainingPlans;
    }

    public async Task<RenameTrainingPlanResult> HandleAsync(
        RenameTrainingPlanCommand command,
        CancellationToken cancellationToken = default)
    {
        var trainingPlan = await _trainingPlans.GetByIdAsync(command.TrainingPlanId, cancellationToken)
            ?? throw NotFoundException.For<TrainingPlan>(command.TrainingPlanId);

        trainingPlan.SetName(command.Name);

        await _trainingPlans.UpdateAsync(trainingPlan, cancellationToken);

        return new RenameTrainingPlanResult(trainingPlan.Id, trainingPlan.Name);
    }
}
