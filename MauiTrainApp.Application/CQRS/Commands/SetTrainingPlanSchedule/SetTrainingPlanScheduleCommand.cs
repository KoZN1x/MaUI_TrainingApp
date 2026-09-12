using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.SetTrainingPlanSchedule;

public sealed record SetTrainingPlanScheduleCommand(
    Guid TrainingPlanId,
    IReadOnlyCollection<DayOfWeek> Days) : ICommand<SetTrainingPlanScheduleResult>;
