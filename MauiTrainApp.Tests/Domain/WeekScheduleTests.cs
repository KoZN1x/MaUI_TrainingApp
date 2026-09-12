using System;
using System.Linq;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.ValueObjects;
using Xunit;

namespace MauiTrainApp.Tests.Domain;

public class WeekScheduleTests
{
    [Fact]
    public void Empty_HasNoDays()
    {
        var schedule = WeekSchedule.Empty;

        Assert.True(schedule.IsEmpty);
        Assert.Empty(schedule.Days);
        Assert.Null(schedule.DaysUntil(DayOfWeek.Monday));
    }

    [Fact]
    public void Days_AreListedFromMondayToSunday()
    {
        var schedule = WeekSchedule.From([DayOfWeek.Sunday, DayOfWeek.Wednesday, DayOfWeek.Monday]);

        Assert.Equal([DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Sunday], schedule.Days);
    }

    [Fact]
    public void DaysUntil_IsZeroOnAScheduledDay()
    {
        var schedule = WeekSchedule.From([DayOfWeek.Monday, DayOfWeek.Thursday]);

        Assert.Equal(0, schedule.DaysUntil(DayOfWeek.Monday));
        Assert.Equal(0, schedule.DaysUntil(DayOfWeek.Thursday));
    }

    [Fact]
    public void DaysUntil_CountsForwardToTheNearestDay()
    {
        var schedule = WeekSchedule.From([DayOfWeek.Monday, DayOfWeek.Thursday]);

        Assert.Equal(3, schedule.DaysUntil(DayOfWeek.Friday));
        Assert.Equal(2, schedule.DaysUntil(DayOfWeek.Saturday));
        Assert.Equal(1, schedule.DaysUntil(DayOfWeek.Sunday));
    }

    [Fact]
    public void DaysUntil_WrapsAroundTheWeek()
    {
        var schedule = WeekSchedule.From([DayOfWeek.Monday]);

        Assert.Equal(6, schedule.DaysUntil(DayOfWeek.Tuesday));
    }

    [Fact]
    public void Toggle_AddsAndRemovesASingleDay()
    {
        var schedule = WeekSchedule.Empty.Toggle(DayOfWeek.Friday);

        Assert.True(schedule.Contains(DayOfWeek.Friday));

        schedule = schedule.Toggle(DayOfWeek.Friday);

        Assert.False(schedule.Contains(DayOfWeek.Friday));
        Assert.True(schedule.IsEmpty);
    }

    [Fact]
    public void Mask_SurvivesARoundTrip()
    {
        var schedule = WeekSchedule.From([DayOfWeek.Tuesday, DayOfWeek.Saturday]);

        Assert.Equal(schedule, WeekSchedule.FromMask(schedule.Mask));
    }

    [Fact]
    public void FromMask_RejectsAValueOutsideTheWeek()
    {
        Assert.Throws<InvariantException>(() => WeekSchedule.FromMask(0b1000_0000));
        Assert.Throws<InvariantException>(() => WeekSchedule.FromMask(-1));
    }

    [Fact]
    public void From_IgnoresRepeatedDays()
    {
        var schedule = WeekSchedule.From([DayOfWeek.Monday, DayOfWeek.Monday]);

        Assert.Single(schedule.Days);
    }
}
