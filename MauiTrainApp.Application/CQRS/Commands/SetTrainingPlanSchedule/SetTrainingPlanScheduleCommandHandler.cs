using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.Interfaces;
using MauiTrainApp.Domain.ValueObjects;

namespace MauiTrainApp.Application.CQRS.Commands.SetTrainingPlanSchedule;

internal sealed class SetTrainingPlanScheduleCommandHandler
    : ICommandHandler<SetTrainingPlanScheduleCommand, SetTrainingPlanScheduleResult>
{
    private readonly IWriteRepository<TrainingPlan> _trainingPlans;

    public SetTrainingPlanScheduleCommandHandler(IWriteRepository<TrainingPlan> trainingPlans)
    {
        _trainingPlans = trainingPlans;
    }

    public async Task<SetTrainingPlanScheduleResult> HandleAsync(
        SetTrainingPlanScheduleCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command.Days);

        var trainingPlan = await _trainingPlans.GetByIdAsync(command.TrainingPlanId, cancellationToken)
            ?? throw NotFoundException.For<TrainingPlan>(command.TrainingPlanId);

        trainingPlan.SetSchedule(WeekSchedule.From(command.Days));

        await _trainingPlans.UpdateAsync(trainingPlan, cancellationToken);

        return new SetTrainingPlanScheduleResult(trainingPlan.Id, trainingPlan.Schedule.Days);
    }
}
