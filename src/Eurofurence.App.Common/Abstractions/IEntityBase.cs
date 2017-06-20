using System;

namespace Eurofurence.App.Common.Abstractions
{
    public interface IEntityBase
    {
        Guid Id { get; }

        int IsDeleted { get; }

        void Touch();

        void NewId();
    }
}