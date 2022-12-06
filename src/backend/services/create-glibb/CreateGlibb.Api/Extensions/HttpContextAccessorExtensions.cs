namespace CreateGlibb.Api.Extensions;

public static class HttpContextAccessorExtensions
{
    public static string GetOwner(this IHttpContextAccessor accessor)
    {
        //accessor.HttpContext.User.Claims.Single(m => m.ValueType)
        return "<dummy>";
    }
}