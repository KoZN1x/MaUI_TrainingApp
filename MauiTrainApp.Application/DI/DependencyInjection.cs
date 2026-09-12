using MauiTrainApp.Application.CQRS.Commands.CompleteExerciseSet;
using MauiTrainApp.Application.CQRS.Commands.CompleteWorkingSet;
using MauiTrainApp.Application.CQRS.Commands.CreateExercise;
using MauiTrainApp.Application.CQRS.Commands.CreateTrainingPlan;
using MauiTrainApp.Application.CQRS.Commands.DeleteExercise;
using MauiTrainApp.Application.CQRS.Commands.DeleteTrainingPlan;
using MauiTrainApp.Application.CQRS.Commands.DeleteWorkout;
using MauiTrainApp.Application.CQRS.Commands.StartWorkoutFromPlan;
using MauiTrainApp.Application.CQRS.Commands.UpdatePerformedWorkingSet;
using MauiTrainApp.Application.CQRS.Commands.UpdatePlannedExerciseSet;
using MauiTrainApp.Application.CQRS.Queries.GetExercises;
using MauiTrainApp.Application.CQRS.Queries.GetTrainingPlanDetails;
using MauiTrainApp.Application.CQRS.Queries.GetTrainingPlans;
using MauiTrainApp.Application.CQRS.Queries.GetWorkoutDetails;
using MauiTrainApp.Application.CQRS.Queries.GetWorkouts;
using MauiTrainApp.Application.CQRS.Queries.GetWorkoutsByPlan;
using MauiTrainApp.Application.ExceptionHandler;
using MauiTrainApp.Core.DI;
using Microsoft.Extensions.DependencyInjection;

namespace MauiTrainApp.Application.DI
{
    public static class ServiceCollectionExtensions
    {
        extension(IServiceCollection services)
        {
            public IServiceCollection AddApplication()
            {
                return services
                    .AddCqrs()
                    .AddExceptionHandler<DomainExceptionHandler>()
                    .AddCommands()
                    .AddQueries();
            }

            private IServiceCollection AddCommands()
            {
                return services
                    .AddCommand<CreateExerciseCommand, CreateExerciseResult, CreateExerciseCommandHandler>()
                    .AddCommand<DeleteExerciseCommand, DeleteExerciseResult, DeleteExerciseCommandHandler>()
                    .AddCommand<CreateTrainingPlanCommand, CreateTrainingPlanResult, CreateTrainingPlanCommandHandler>()
                    .AddCommand<DeleteTrainingPlanCommand, DeleteTrainingPlanResult, DeleteTrainingPlanCommandHandler>()
                    .AddCommand<StartWorkoutFromPlanCommand, StartWorkoutFromPlanResult, StartWorkoutFromPlanCommandHandler>()
                    .AddCommand<CompleteWorkingSetCommand, CompleteWorkingSetResult, CompleteWorkingSetCommandHandler>()
                    .AddCommand<CompleteExerciseSetCommand, CompleteExerciseSetResult, CompleteExerciseSetCommandHandler>()
                    .AddCommand<DeleteWorkoutCommand, DeleteWorkoutResult, DeleteWorkoutCommandHandler>()
                    .AddCommand<UpdatePlannedExerciseSetCommand, UpdatePlannedExerciseSetResult, UpdatePlannedExerciseSetCommandHandler>()
                    .AddCommand<UpdatePerformedWorkingSetCommand, UpdatePerformedWorkingSetResult, UpdatePerformedWorkingSetCommandHandler>();
            }

            private IServiceCollection AddQueries()
            {
                return services
                    .AddQuery<GetExercisesQuery, GetExercisesResult, GetExercisesQueryHandler>()
                    .AddQuery<GetTrainingPlansQuery, GetTrainingPlansResult, GetTrainingPlansQueryHandler>()
                    .AddQuery<GetTrainingPlanDetailsQuery, GetTrainingPlanDetailsResult, GetTrainingPlanDetailsQueryHandler>()
                    .AddQuery<GetWorkoutsQuery, GetWorkoutsResult, GetWorkoutsQueryHandler>()
                    .AddQuery<GetWorkoutsByPlanQuery, GetWorkoutsByPlanResult, GetWorkoutsByPlanQueryHandler>()
                    .AddQuery<GetWorkoutDetailsQuery, GetWorkoutDetailsResult, GetWorkoutDetailsQueryHandler>();
            }
        }
    }
}
