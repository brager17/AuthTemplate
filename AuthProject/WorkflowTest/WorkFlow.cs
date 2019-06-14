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
        Task<ErrorMessage> RollBack(TIn input, CancellationToken cancellationToken);
    }

    public interface ICanRollBack<in TIn>
    {
        ErrorMessage RollBack(TIn input);
    }

    public class ErrorMessage
    {
        public ErrorMessage(string messageInfo)
        {
            MessageInfo = messageInfo;
        }

        public string MessageInfo;

       
    }
}