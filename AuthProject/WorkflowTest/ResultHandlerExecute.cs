using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Force;

namespace AuthProject.WorkflowTest
{
    public class ResultHandlersExecutor
    {
        private IList<object> handlers = new List<object>();

        public void AddHandler<TIn, TOut>(IAsyncHandler<TIn, TOut> asyncHandler)
        {
            if (GetHandler<TIn, TOut>() != null)
                throw new ArgumentException("Уже добавлен");

            handlers.Add(asyncHandler);
        }

        public async Task<TOut> TryHandle<TIn, TOut>(TIn input, CancellationToken cancellationToken)
        {
            var asyncHandler = GetHandler<TIn, TOut>();
            if (asyncHandler == null)
                return await Task.FromResult<TOut>(default);
            return await asyncHandler.Handle(input, cancellationToken);
        }

        private IAsyncHandler<TIn, TOut> GetHandler<TIn, TOut>()
        {
            var s = typeof(IAsyncHandler<,>).MakeGenericType(typeof(TIn), typeof(TOut));
            var handler = handlers.FirstOrDefault(x => x.GetType().GetInterfaces().Single() == s);
            return (IAsyncHandler<TIn, TOut>) handler;
        }
    }

    public class VoidHandlersExecutor
    {
        private IList<object> handlers = new List<object>();

        public void AddHandler<TIn>(IAsyncHandler<TIn> voidAsyncHandler)
        {
            handlers.Add(voidAsyncHandler);
        }

        public async Task TryHandle<TIn>(TIn input, CancellationToken cancellationToken = default)
        {
            var asyncHandlers = GetHandler<TIn>();
            if (asyncHandlers == null)
                return;

            foreach (var asyncHandler in asyncHandlers)
            {
                await asyncHandler.Handle(input, cancellationToken);
            }
        }

        private IEnumerable<IAsyncHandler<TIn>> GetHandler<TIn>()
        {
            var s = typeof(IAsyncHandler<>).MakeGenericType(typeof(TIn));

            var foundedHandlers = handlers.Where(x => x.GetType().GetInterfaces().Single() == s)
                .Cast<IAsyncHandler<TIn>>();

            if (!foundedHandlers.Any())
            {
                return null;
            }

            return foundedHandlers;
        }
    }

    public class ResultHandlersFactory
    {
        public IAsyncHandler<TIn, TOut> Create<TIn, TOut>(IAsyncHandler<TIn, TOut> handler) =>
            new ResultAsyncHandlerExecute<TIn, TOut>(handler);

        public IAsyncHandler<TIn, TOut> Create<TIn, TOut>(IHandler<TIn, TOut> handler) =>
            new ResultHandlerExecute<TIn, TOut>(handler);

        public IAsyncHandler<TIn, TOut> Create<TIn, TOut>(object obj) => null;
    }

    public class VoidHandlersFactory
    {
        public IAsyncHandler<TIn> Create<TIn>(IHandler<TIn> asyncRollBackHandler)
            => new VoidHandlerExecute<TIn>(asyncRollBackHandler);

        public IAsyncHandler<TIn> Create<TIn>(IAsyncHandler<TIn> asyncRollBackHandler)
            => new VoidAsyncHandlerExecute<TIn>(asyncRollBackHandler);

        public IAsyncHandler<object> Create(object obj) => null;
    }

    public class RollBackHandlerFactory
    {
        public IAsyncHandler<TIn, ErrorMessage> Create<TIn>(
            ICanAsyncRollBack<TIn> asyncRollBackHandler)
            => new AsyncRollBackExecute<TIn>(asyncRollBackHandler);

        public IAsyncHandler<TIn, ErrorMessage> Create<TIn>(ICanRollBack<TIn> asyncRollBackHandler)
            => new RollBackExecute<TIn, ErrorMessage>(asyncRollBackHandler);

        public IAsyncHandler<object> Create(object obj) => null;
    }


    public class ResultHandlerExecute<TIn, TOut> : IAsyncHandler<TIn, TOut>
    {
        private readonly IHandler<TIn, TOut> _handler;

        public ResultHandlerExecute(IHandler<TIn, TOut> handler)
        {
            _handler = handler;
        }

        public Task<TOut> Handle(TIn obj, CancellationToken ct)
        {
            var result = _handler.Handle(obj);
            return Task.FromResult(result);
        }
    }


    public class ResultAsyncHandlerExecute<TIn, TOut> : IAsyncHandler<TIn, TOut>
    {
        private readonly IAsyncHandler<TIn, TOut> _asyncHandler;

        public ResultAsyncHandlerExecute(IAsyncHandler<TIn, TOut> asyncHandler)
        {
            _asyncHandler = asyncHandler;
        }

        public async Task<TOut> Handle(TIn input, CancellationToken ct)
        {
            return await _asyncHandler.Handle(input, ct);
        }
    }

    public class RollBackExecute<TIn, TOut> : IAsyncHandler<TIn, ErrorMessage>
    {
        private readonly ICanRollBack<TIn> _canRollBack;

        public RollBackExecute(ICanRollBack<TIn> canRollBack)
        {
            _canRollBack = canRollBack;
        }

        public async Task<ErrorMessage> Handle(TIn input, CancellationToken cancellationToken)
        {
            return await Task.FromResult(_canRollBack.RollBack(input));
        }
    }

    public class AsyncRollBackExecute<TIn> : IAsyncHandler<TIn, ErrorMessage>
    {
        private readonly ICanAsyncRollBack<TIn> _asyncRollBack;

        public AsyncRollBackExecute(ICanAsyncRollBack<TIn> asyncRollBack)
        {
            _asyncRollBack = asyncRollBack;
        }

        public async Task<ErrorMessage> Handle(TIn input, CancellationToken cancellationToken)
        {
            return await _asyncRollBack.RollBack(input, cancellationToken);
        }
    }

    public class VoidHandlerExecute<TIn> : IAsyncHandler<TIn>
    {
        private readonly IHandler<TIn> _handler;

        public VoidHandlerExecute(IHandler<TIn> handler)
        {
            _handler = handler;
        }

        public Task Handle(TIn input, CancellationToken cancellationToken)
        {
            _handler.Handle(input);
            return Task.CompletedTask;
        }
    }

    public class VoidAsyncHandlerExecute<TIn> : IAsyncHandler<TIn>
    {
        private readonly IAsyncHandler<TIn> _handler;

        public VoidAsyncHandlerExecute(IAsyncHandler<TIn> handler)
        {
            _handler = handler;
        }

        public async Task Handle(TIn input, CancellationToken cancellationToken)
        {
            await _handler.Handle(input, cancellationToken);
        }
    }
}