using MauiTrainApp.Domain.ReadModels;
using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Domain.Interfaces;

public interface ITrainingPlanReadRepository
    : IReadRepository<TrainingPlanListItemReadModel, TrainingPlanDetailsReadModel>;
