using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace AuthProject.WorkflowTest
{
    public class WorkFlow : ModelBinderAttribute
    {
        public Type WorkflowType { get; set; }

        public WorkFlow(Type workflowType)
        {
            WorkflowType = workflowType;
            BinderType = typeof(WorkflowBinder);
        }

        public override BindingSource BindingSource => BindingSource.Services;
    }

    public class WorkflowBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var actionParameters = bindingContext?.ActionContext?.ActionDescriptor?.Parameters;

            if (actionParameters.Count != 2)
            {
                throw new Exception("должно быть только два аргумента, один - тип вокфлоу, второй - инпут");
            }

            Type ouputGenericType = null;
            var actionReturnType = ((ControllerActionDescriptor) bindingContext.ActionContext.ActionDescriptor)
                .MethodInfo.ReturnType;

            if (!actionReturnType.IsGenericType)
                throw new Exception(
                    "Используйте action'ы, которые возвращают ActionResult<T> или Task<ActionResult<T>>");

            var genericType = actionReturnType.GetGenericArguments().First();
            if (actionReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                if (genericType.GetGenericTypeDefinition() == typeof(ActionResult<>))
                {
                    ouputGenericType = genericType.GetGenericArguments().First();
                }
            }

            else if (actionReturnType.GetGenericTypeDefinition() == typeof(ActionResult<>))
            {
                ouputGenericType = genericType;
            }


            var workFlowType = ((ControllerParameterDescriptor) actionParameters
                    .First(x => x.Name == bindingContext.FieldName))
                .ParameterInfo
                .GetCustomAttribute<WorkFlow>()
                .WorkflowType;

            var dtoParameter = actionParameters.FirstOrDefault(x => x.Name != bindingContext.FieldName);
            if (dtoParameter == null)
            {
                throw new Exception();
            }

            var dtoType = dtoParameter.ParameterType;

            var workflowManagerType = typeof(WorkflowManager<,>).MakeGenericType(dtoType, ouputGenericType);


            var workflowManager =
                (IInitializeWorkflowManager) bindingContext.HttpContext.RequestServices.GetService(workflowManagerType);

            workflowManager = (IInitializeWorkflowManager) ActivatorUtilities.CreateInstance(
                bindingContext.HttpContext.RequestServices, workflowManagerType,
                bindingContext.HttpContext.RequestServices);

            var workflowInfo = new WorkflowInfo {WorkflowName = workFlowType};
            workflowManager.Initialize(workflowInfo);
            bindingContext.Model = workflowManager;
            bindingContext.Result = ModelBindingResult.Success(workflowManager);
        }
    }

    // use override BinderType in WorkFlowAttribute
    public class WorkflowBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata is DefaultModelMetadata t &&
                t.Attributes.Attributes.Any(x => x.GetType() == typeof(WorkFlow)))
            {
                return new BinderTypeModelBinder(typeof(WorkflowBinder));
            }

            return null;
        }
    }
}