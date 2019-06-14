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
    public class WorkflowManager<TIn, TOut> : IWorkflowManager, IAsyncHandler<TIn, ActionResult<TOut>>
        where TOut : new()
    {
        private static VoidHandlersFactory _voidHandlersFactory = new VoidHandlersFactory();
        private static RollBackHandlerFactory _rollBackHandlerFactory = new RollBackHandlerFactory();
        private static ResultHandlersFactory _resultHandlersFactory = new ResultHandlersFactory();
        private static ResultHandlersExecutor _resultHandlersExecutor = new ResultHandlersExecutor();
        private static VoidHandlersExecutor _voidHandlersExecutor = new VoidHandlersExecutor();
        private static ResultHandlersExecutor _rollBackHandlersExecutor = new ResultHandlersExecutor();
        private List<object> ChainHandlersInputs { get; } = new List<object>();
        private readonly IServiceProvider _serviceProvider;
        private readonly Type _workflowType;
        private List<Type> ChainTypes { get; set; }

        private static InputTypeParametersToObject InvokeVoidTryHandler =>
            typeof(VoidHandlersExecutor).Invoke(_voidHandlersExecutor, "TryHandle");

        private static InputTypeParametersToObject InvokeResultTryHandler =>
            typeof(ResultHandlersExecutor).Invoke(_resultHandlersExecutor, "TryHandle");

        private static InputTypeParametersToObject InvokeRollBackTryHandler =>
            typeof(ResultHandlersExecutor).Invoke(_rollBackHandlersExecutor, "TryHandle");

        private static GetPropertyValue ResultPropertyValue => ReflectionExtensions.GetPropertyValue("Result");

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
            if (WorkflowNotContainsWithoutParametersConstructor(input))
            {
                throw new Exception("У типа задающего ворклфоу должен быть безпараметрический конструктор");
            }

            var services = input.WorkflowName.GetNestedTypes().Select(_serviceProvider.GetService).ToList();

            var chainMethodParameter = services.Select(x => new
                {
                    method = x.GetType().GetMethod("Handle"),
                    parameter = x.GetType().GetParametersByMethodName("Handle").FirstOrDefault()
                })
                .Where(x => x.parameter != default)
                .ToList();

            var parameterInfosList = chainMethodParameter.ToList();

            ChainTypes = chainMethodParameter.SkipLast(1)
                .Aggregate(new[] {typeof(TIn)}.AsEnumerable(), (a, c) =>
                {
                    var currentHandlerInChain = parameterInfosList.First(x => x.parameter == a.Last());
                    return a.Concat(currentHandlerInChain.method.GetGenericTypesReturnValue());
                })
                .Concat(new[] {typeof(TOut)})
                .ToList();


            foreach (var service in services)
            {
                var voidHandler = _voidHandlersFactory.Create((dynamic) service);
                if (voidHandler != null)
                {
                    _voidHandlersExecutor.AddHandler(voidHandler);
                    continue;
                }

                var rollBackHandler = _rollBackHandlerFactory.Create((dynamic) service);
                if (rollBackHandler != null)
                {
                    _rollBackHandlersExecutor.AddHandler(rollBackHandler);
                }

                var resultHandler = _resultHandlersFactory.Create((dynamic) service);
                if (resultHandler != null)
                {
                    _resultHandlersExecutor.AddHandler(resultHandler);
                }
            }
        }

        private static bool WorkflowNotContainsWithoutParametersConstructor(WorkflowInfo input)
        {
            return input.WorkflowName.GetConstructors().All(x => x.GetParameters().Length != 0);
        }

        // массив типов

        public async Task<ActionResult<TOut>> Handle(TIn input, CancellationToken ct)
        {
            object chainInputOut = input;
            var chainTypesCounter = 0;

            while (chainInputOut.GetType() != typeof(TOut))
            {
                try
                {
                    ChainHandlersInputs.Add(chainInputOut);

                    var inputHandlerParameterType = ChainTypes[chainTypesCounter];
                    var outputHandlerReturnType = ChainTypes[++chainTypesCounter];

                    var voidHandler = InvokeVoidTryHandler(inputHandlerParameterType)(chainInputOut, ct);

                    await (Task) voidHandler;

                    var resultTask =
                        InvokeResultTryHandler(inputHandlerParameterType, outputHandlerReturnType)(chainInputOut, ct);

                    await (Task) resultTask;

                    chainInputOut = ResultPropertyValue(resultTask);
                }
                catch (WorkflowException)
                {
                    object error = null;
                    while (chainTypesCounter > 0)
                    {
                        var inputHandlerType = ChainTypes[--chainTypesCounter];

                        var rollBackHandler =
                            InvokeRollBackTryHandler(inputHandlerType, typeof(ErrorMessage))(
                                ChainHandlersInputs[chainTypesCounter], ct);

                        await (Task) rollBackHandler;

                        if (error == null)
                        {
                            error = ResultPropertyValue(rollBackHandler);
                        }
                    }

                    return new JsonResult(error)
                    {
                        StatusCode = 422
                    };
                }
            }

            return (TOut) chainInputOut;
        }
    }
}