using System;
using System.Collections.Generic;
using System.Text;

namespace MauiTrainApp.Domain.Interfaces
{
    public interface IEntity
    {
        Guid Id { get; }
        DateTime CreatedAt { get; }
        DateTime? UpdatedAt { get; }
        string Name { get; }
        string? Description { get; }

        public void SetUpdatedTime();
        public void SetName(string name);
        public void SetDescription(string description);
    }
}
