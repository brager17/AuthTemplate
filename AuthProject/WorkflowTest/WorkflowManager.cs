using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Force;
using Microsoft.AspNetCore.Mvc;

namespace AuthProject.WorkflowTest
{
    public class WorkflowInfo
    {
        public Type WorkflowName { get; set; }
    }

    public interface IWorkflowManager
    {
        void Initialize(WorkflowInfo input);
    }

    public class WorkflowManager<TIn, TOut> : IWorkflowManager, IAsyncHandler<TIn, TOut>
        where TOut : new()
    {
        private VoidHandlersFactory _voidHandlersFactory = new VoidHandlersFactory();
        private ResultHandlersFactory _resultHandlersFactory = new ResultHandlersFactory();
        private ResultHandlersExecutor _resultHandlersExecutor = new ResultHandlersExecutor();
        private VoidHandlersExecutor _voidHandlersExecutor = new VoidHandlersExecutor();

        private readonly IServiceProvider _serviceProvider;
        private readonly Type _workflowType;
        private List<Type> ChainTypes { get; set; }

        public WorkflowManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="input">тип workflow объекта</param>
        /// <returns></returns>
        void IWorkflowManager.Initialize(WorkflowInfo input)
        {
            if (input.WorkflowName.GetConstructors().All(x => x.GetParameters().Length != 0))
            {
                throw new Exception("У типа задающего ворклфоу должен быть безпараметрический конструктор");
            }

            var workflowNestedTypes = input.WorkflowName.GetNestedTypes();


            var services = workflowNestedTypes.Select(_serviceProvider.GetService).ToList();


            var parameterInfos = services.Select(x => new
                    {
                        method = x.GetType().GetMethod("Handle"),
                        parameter = x.GetType().GetMethod("Handle").GetParameters().FirstOrDefault().ParameterType
                    })
                    .Where(x => x.parameter != default)
                ;

            var parameterInfosList = parameterInfos.ToList();

            ChainTypes = parameterInfos.SkipLast(1)
                .Aggregate(new[] {typeof(TIn)}.AsEnumerable(), (a, c) =>
                {
                    var handler = parameterInfosList.First(x => x.parameter == a.Last());
                    return a.Concat(handler.method.ReturnParameter.ParameterType.GetGenericArguments());
                }).ToList();

            foreach (var service in services)
            {
                var voidHandler = _voidHandlersFactory.Create((dynamic) service);
                if (voidHandler != null)
                {
                    _voidHandlersExecutor.AddHandler(voidHandler);
                    continue;
                }

                var resultHandler = _resultHandlersFactory.Create((dynamic) service);
                if (resultHandler != null)
                {
                    _resultHandlersExecutor.AddHandler(resultHandler);
                }
            }
        }

        // массив типов

        public async Task<TOut> Handle(TIn input, CancellationToken ct)
        {
            object chainInputOut = input;
            var chainTypesCounter = 0;

            while (chainInputOut.GetType() != typeof(TOut))
            {
                try
                {
                    var inputHandlerType = ChainTypes[chainTypesCounter];
                    var outputHandlerType = ChainTypes[++chainTypesCounter];

                    var voidHandler = typeof(VoidHandlersExecutor).GetMethod("TryHandle")
                        .MakeGenericMethod(inputHandlerType)
                        .Invoke(_voidHandlersExecutor, new object[] {chainInputOut, ct});

                    await (Task) voidHandler;

                    var resultTask = typeof(ResultHandlersExecutor).GetMethod("TryHandle")
                        .MakeGenericMethod(inputHandlerType, outputHandlerType)
                        .Invoke(_resultHandlersExecutor, new object[] {chainInputOut, ct});

                    await (Task) resultTask;

                    chainInputOut = resultTask.GetType().GetProperty("Result").GetValue(resultTask);
                }
                catch (WorkflowException workflowException)
                {
                    while (chainTypesCounter > 0)
                    {
                        var inputHandlerType = ChainTypes[--chainTypesCounter];
                        var outputHandlerType = ChainTypes[chainTypesCounter];
                        
                    }
                }
            }

            return (TOut) chainInputOut;
        }
    }
}