using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Stockat.API.ActionFilters;

public class ValidationFilterAttribute: Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // execute any code before the action executes

        var action = context.RouteData.Values["action"];
        var controller = context.RouteData.Values["controller"];
        var param = context.ActionArguments.SingleOrDefault(x => x.Value.ToString().Contains("Dto")).Value;

        if (param is null) // if the dto object itself was null
        {
            context.Result = new BadRequestObjectResult($"Object is null. Controller: { controller }, action: { action}");
            return;
        }
        if (!context.ModelState.IsValid) // if the properties within the dto were invalid
        {
            context.Result = new UnprocessableEntityObjectResult(context.ModelState);
            return;
        }


        var result = await next();

        // execute any code after the action executes
    }
}
