namespace MauiTrainApp.Application.CQRS.Commands.SetTrainingPlanSchedule;

public sealed record SetTrainingPlanScheduleResult(Guid TrainingPlanId, IReadOnlyCollection<DayOfWeek> Days);
