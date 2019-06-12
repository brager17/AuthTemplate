using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        object Handle(WorkflowInfo input);
    }

    public class WorkflowHandlerType
    {
        public Type Type { get; set; }
        public Type InputHandlerType { get; set; }
        public Type OutputHandlerType { get; set; }

        public object Entity { get; set; }
        public bool CanRoolBack { get; set; }
    }

    public class WorkflowManager<TIn, TOut> : IWorkflowManager, IAsyncHandler<TIn, TOut>
        where TOut : new()
    {
        private Type[] InnerWorkflowHandlers = VoidHandlers.Concat(ResultHandlers).ToArray();


        private static readonly Type[] VoidHandlers =
        {
            typeof(IAsyncHandler<>),
            typeof(IHandler<>)
        };

        private static readonly Type[] ResultHandlers =
        {
            typeof(IHandler<,>),
            typeof(IAsyncHandler<,>),
        };

        private readonly IServiceProvider _serviceProvider;
        private readonly Type _workflowType;

        public WorkflowManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="input">тип workflow объекта</param>
        /// <returns></returns>
        object IWorkflowManager.Handle(WorkflowInfo input)
        {
            if (input.WorkflowName.GetConstructors().All(x => x.GetParameters().Length != 0))
            {
                throw new Exception("У типа задающего ворклфоу должен быть безпараметрический конструктор");
            }

            var workflowNestedTypes = input.WorkflowName.GetNestedTypes();

            var handlers = workflowNestedTypes
                .Select(x => new WorkflowHandlerType
                {
                    Type = x,
                    InputHandlerType = GetInputHandlerType(x),
                    OutputHandlerType = GetOutputHandlerType(x),
                    Entity = _serviceProvider.GetService(x),
                    CanRoolBack = x.GetInterfaces().Any(xx => xx.GetGenericTypeDefinition() == typeof(ICanRollBack<>))
                })
                .ToList();

            var firstHandler = handlers.Where(x => x.InputHandlerType == typeof(TIn));
            if (firstHandler.Count() != 1)
            {
                throw new Exception($"В воркфлоу не удается определить входной хэндлер, с инпутом {typeof(TIn).Name}");
            }

            var allTypes = handlers.Select(x => x.InputHandlerType).Where(x => x != typeof(TIn)).ToList();

            var desiredType = typeof(TOut);

            while (allTypes.Count > 0)
            {
                var newDesiredType = handlers.FirstOrDefault(x => x.InputHandlerType == desiredType)?.OutputHandlerType;
                if (newDesiredType == null)
                {
                    throw new Exception($"Не найден хэндлер отвечающий за переход в {desiredType} ");
                }

                allTypes.Remove(desiredType);
                desiredType = newDesiredType;
            }

            return 1;
        }


        private Type GetInputHandlerType(Type handlerType)
        {
            var interfaces = handlerType.GetInterfaces().ToList();

            var handlerInterface = interfaces
                .FirstOrDefault(x => InnerWorkflowHandlers.Contains(x.GetGenericTypeDefinition()));

            if (handlerInterface != null)
            {
                return handlerInterface.GetGenericArguments().First();
            }

            throw new ArgumentException("Внутренний тип в воркфлоу должен быть хендлером");
        }

        private Type GetOutputHandlerType(Type handlerType)
        {
            var interfaces = handlerType.GetInterfaces();

            var handlerInterface = interfaces
                .FirstOrDefault(x => ResultHandlers.Contains(x.GetGenericTypeDefinition()));

            if (handlerInterface != null)
            {
                return handlerInterface.GetGenericArguments().Last();
            }

            if (VoidHandlers.Contains(handlerType))
            {
                return null;
            }

            throw new ArgumentException("Внутренний тип в воркфлоу должен быть хендлером");
        }


        public async Task<TOut> Handle(TIn input, CancellationToken cancellationToken)
        {
            await Task.Delay(0);
            return await Task.FromResult(new TOut());
        }
    }
}