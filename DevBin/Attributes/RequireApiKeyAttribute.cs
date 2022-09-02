using DevBin.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DevBin.Attributes;
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireApiKeyAttribute : TypeFilterAttribute
{
    public RequireApiKeyAttribute(ApiPermission permission = ApiPermission.None) : base(typeof(RequireApiKeyFilter))
    {
        Arguments = new object[] { permission };
    }
}

public class RequireApiKeyFilter : IAsyncAuthorizationFilter
{
    private readonly ApplicationDbContext _context;
    private readonly ApiPermission _permission;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public RequireApiKeyFilter(ApplicationDbContext context, ApiPermission permission, SignInManager<ApplicationUser> signInManager)
    {
        _context = context;
        _permission = permission;
        _signInManager = signInManager;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext filterContext)
    {
        var authorizationKey = filterContext.HttpContext.Request.Headers.Authorization.ToString();
        var token = _context.ApiTokens.FirstOrDefault(q => q.Token == authorizationKey);
        if (token == null)
        {
            filterContext.Result = new UnauthorizedResult();
            return;
        }

        // there's probably a better way
        var isAuthorized = true;
        if (_permission.HasFlag(ApiPermission.Get) && !token.AllowGet)
            isAuthorized = false;
        if (_permission.HasFlag(ApiPermission.Create) && !token.AllowCreate)
            isAuthorized = false;
        if (_permission.HasFlag(ApiPermission.Update) && !token.AllowUpdate)
            isAuthorized = false;
        if (_permission.HasFlag(ApiPermission.Delete) && !token.AllowDelete)
            isAuthorized = false;
        if (_permission.HasFlag(ApiPermission.GetUser) && !token.AllowGetUser)
            isAuthorized = false;
        if (_permission.HasFlag(ApiPermission.CreateFolder) && !token.AllowCreateFolders)
            isAuthorized = false;
        if (_permission.HasFlag(ApiPermission.DeleteFolder) && !token.AllowDeleteFolders)
            isAuthorized = false;

        if (!isAuthorized)
        {
            filterContext.Result = new UnauthorizedResult();
            return;
        }

        var userPrincipal = await _signInManager.CreateUserPrincipalAsync(token.Owner);
        var httpContext = filterContext.HttpContext;
        httpContext.User = userPrincipal;
    }

}