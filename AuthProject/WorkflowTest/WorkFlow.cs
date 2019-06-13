using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace AuthProject.WorkflowTest
{
    public interface IAsyncHandler<in TIn>
    {
        Task Handle(TIn input, CancellationToken cancellationToken);
    }

    public interface ICanAsyncRollBack<in TIn>
    {
        Task RollBack(TIn input, CancellationToken cancellationToken);
    }

    public interface ICanRollBack<in TIn>
    {
        void RollBack(TIn input);
    }
}