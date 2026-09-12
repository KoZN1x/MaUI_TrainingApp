namespace MauiTrainApp.Domain.ReadModels;

public sealed record ExerciseSetReadModel(
    Guid Id,
    Guid ExerciseId,
    string ExerciseName,
    IReadOnlyCollection<WorkingSetReadModel> WorkingSets)
{
    public bool IsCompleted => WorkingSets.Count > 0 && WorkingSets.All(x => x.IsCompleted);
}
