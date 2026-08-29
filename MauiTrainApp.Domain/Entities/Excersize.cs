using MauiTrainApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace MauiTrainApp.Domain.Entities
{
    public sealed record Excersize : IEntity
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public DateTime CreatedAt { get; init; } = DateTime.Now;

        public DateTime? UpdatedAt { get; private set; }

        public string Name { get; private set; }

        public string? Description { get; private set; }

        public void SetDescription(string description)
        {
            throw new NotImplementedException();
        }

        public void SetName(string name)
        {
            throw new NotImplementedException();
        }

        public void SetUpdatedTime()
        {
            UpdatedAt = DateTime.Now;
        }
    }
}
