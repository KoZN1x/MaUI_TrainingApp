using MauiTrainApp.Application.CQRS.Commands.AddPerformedExerciseSet;
using MauiTrainApp.Application.CQRS.Commands.AddPerformedWorkingSet;
using MauiTrainApp.Application.CQRS.Commands.AddPlannedExerciseSet;
using MauiTrainApp.Application.CQRS.Commands.CompleteExerciseSet;
using MauiTrainApp.Application.CQRS.Commands.CompleteWorkout;
using MauiTrainApp.Application.CQRS.Commands.CompleteWorkingSet;
using MauiTrainApp.Application.CQRS.Commands.CreateExercise;
using MauiTrainApp.Application.CQRS.Commands.CreateTrainingPlan;
using MauiTrainApp.Application.CQRS.Commands.DeleteExercise;
using MauiTrainApp.Application.CQRS.Commands.DeleteTrainingPlan;
using MauiTrainApp.Application.CQRS.Commands.DeleteWorkout;
using MauiTrainApp.Application.CQRS.Commands.MovePlannedExerciseSet;
using MauiTrainApp.Application.CQRS.Commands.RemovePerformedWorkingSet;
using MauiTrainApp.Application.CQRS.Commands.RemovePlannedExerciseSet;
using MauiTrainApp.Application.CQRS.Commands.RenameTrainingPlan;
using MauiTrainApp.Application.CQRS.Commands.SetTrainingPlanSchedule;
using MauiTrainApp.Application.CQRS.Commands.ResetWorkingSet;
using MauiTrainApp.Application.CQRS.Commands.StartWorkoutFromPlan;
using MauiTrainApp.Application.CQRS.Commands.UpdatePerformedWorkingSet;
using MauiTrainApp.Application.CQRS.Commands.UpdateExercise;
using MauiTrainApp.Application.CQRS.Commands.UpdatePlannedExerciseSet;
using MauiTrainApp.Application.CQRS.Queries.GetActiveWorkout;
using MauiTrainApp.Application.CQRS.Queries.GetExerciseProgress;
using MauiTrainApp.Application.CQRS.Queries.GetExercises;
using MauiTrainApp.Application.CQRS.Queries.GetNextTrainingPlan;
using MauiTrainApp.Application.CQRS.Queries.GetProgressSummary;
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
                    .AddCommand<ResetWorkingSetCommand, ResetWorkingSetResult, ResetWorkingSetCommandHandler>()
                    .AddCommand<DeleteWorkoutCommand, DeleteWorkoutResult, DeleteWorkoutCommandHandler>()
                    .AddCommand<UpdatePlannedExerciseSetCommand, UpdatePlannedExerciseSetResult, UpdatePlannedExerciseSetCommandHandler>()
                    .AddCommand<UpdatePerformedWorkingSetCommand, UpdatePerformedWorkingSetResult, UpdatePerformedWorkingSetCommandHandler>()
                    .AddCommand<UpdateExerciseCommand, UpdateExerciseResult, UpdateExerciseCommandHandler>()
                    .AddCommand<CompleteWorkoutCommand, CompleteWorkoutResult, CompleteWorkoutCommandHandler>()
                    .AddCommand<RenameTrainingPlanCommand, RenameTrainingPlanResult, RenameTrainingPlanCommandHandler>()
                    .AddCommand<SetTrainingPlanScheduleCommand, SetTrainingPlanScheduleResult, SetTrainingPlanScheduleCommandHandler>()
                    .AddCommand<AddPlannedExerciseSetCommand, AddPlannedExerciseSetResult, AddPlannedExerciseSetCommandHandler>()
                    .AddCommand<RemovePlannedExerciseSetCommand, RemovePlannedExerciseSetResult, RemovePlannedExerciseSetCommandHandler>()
                    .AddCommand<AddPerformedWorkingSetCommand, AddPerformedWorkingSetResult, AddPerformedWorkingSetCommandHandler>()
                    .AddCommand<AddPerformedExerciseSetCommand, AddPerformedExerciseSetResult, AddPerformedExerciseSetCommandHandler>()
                    .AddCommand<RemovePerformedWorkingSetCommand, RemovePerformedWorkingSetResult, RemovePerformedWorkingSetCommandHandler>()
                    .AddCommand<MovePlannedExerciseSetCommand, MovePlannedExerciseSetResult, MovePlannedExerciseSetCommandHandler>();
            }

            private IServiceCollection AddQueries()
            {
                return services
                    .AddQuery<GetExercisesQuery, GetExercisesResult, GetExercisesQueryHandler>()
                    .AddQuery<GetTrainingPlansQuery, GetTrainingPlansResult, GetTrainingPlansQueryHandler>()
                    .AddQuery<GetTrainingPlanDetailsQuery, GetTrainingPlanDetailsResult, GetTrainingPlanDetailsQueryHandler>()
                    .AddQuery<GetWorkoutsQuery, GetWorkoutsResult, GetWorkoutsQueryHandler>()
                    .AddQuery<GetWorkoutsByPlanQuery, GetWorkoutsByPlanResult, GetWorkoutsByPlanQueryHandler>()
                    .AddQuery<GetWorkoutDetailsQuery, GetWorkoutDetailsResult, GetWorkoutDetailsQueryHandler>()
                    .AddQuery<GetProgressSummaryQuery, GetProgressSummaryResult, GetProgressSummaryQueryHandler>()
                    .AddQuery<GetExerciseProgressQuery, GetExerciseProgressResult, GetExerciseProgressQueryHandler>()
                    .AddQuery<GetActiveWorkoutQuery, GetActiveWorkoutResult, GetActiveWorkoutQueryHandler>()
                    .AddQuery<GetNextTrainingPlanQuery, GetNextTrainingPlanResult, GetNextTrainingPlanQueryHandler>();
            }
        }
    }
}
