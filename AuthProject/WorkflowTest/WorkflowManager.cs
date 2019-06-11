using System;
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

    public class WorkflowManager<TIn, TOut> : IWorkflowManager, IAsyncHandler<TIn, TOut>
        where TOut : new()
    {
        private readonly Type _workflowType;

        public WorkflowManager()
        {
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
            return (object) 1;
        }

        public async Task<TOut> Handle(TIn input, CancellationToken cancellationToken)
        {
            await Task.Delay(0);
            return await Task.FromResult(new TOut());
        }
    }
}