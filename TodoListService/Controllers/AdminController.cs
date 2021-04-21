extern alias BetaLib; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoListService.Models;
using Beta = BetaLib.Microsoft.Graph;

namespace TodoListService.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        CommonDBContext _commonDBContext;
        AuthenticationContextClassReferencesOperations _authContextClassReferencesOperations;
        IConfiguration _configuration;

        Dictionary<string, string> dictACRValues = new Dictionary<string, string>()
        {
            {"C1","Low" },
            {"C2","Medium" },
            {"C3","High" }
        };
        
        string TenantId;

        public AdminController(IConfiguration configuration, AuthenticationContextClassReferencesOperations authContextClassReferencesOperations, CommonDBContext commonDBContext)
        {
            _configuration = configuration;
            _authContextClassReferencesOperations = authContextClassReferencesOperations;
            _commonDBContext = commonDBContext;
            TenantId = _configuration["AzureAd:TenantId"];
        }
        public IActionResult Index()
        {
            IEnumerable<SelectListItem> AuthContextValue = new List<SelectListItem>
        {
            new SelectListItem{Text= dictACRValues["C1"]},
            new SelectListItem{Text= dictACRValues["C2"]},
            new SelectListItem { Text = dictACRValues["C3"]}
        };
            IEnumerable<SelectListItem> Operations = new List<SelectListItem>
        {
            new SelectListItem{Text= "Get"},
            new SelectListItem{Text= "Post"},
            new SelectListItem{ Text= "Delete"}
        };
            TempData["TenantId"] = TenantId;

            TempData["AuthContextValue"] = AuthContextValue;

            TempData["Operations"] = Operations;
            return View();
        }
        [AuthorizeForScopes(ScopeKeySection = "GraphBeta:Scopes")]

        public async Task<List<Beta.AuthenticationContextClassReference>> CreateOrFetch()
        {
            var lstPolicies = await _authContextClassReferencesOperations.ListAuthenticationContextClassReferencesAsync();
            if (lstPolicies?.Count > 0)
            {
                return lstPolicies;
            }
            else
            {
                await CreateAuthContext();
            }
            return lstPolicies;
        }
        public async Task UpdateAuthContextDB(AuthContext authContext)
        {
            authContext.AuthContextType = dictACRValues.FirstOrDefault(x => x.Value == authContext.AuthContextValue).Key;
            await UpdateDB(authContext);
        }

        private async Task UpdateDB(AuthContext authContext)
        {
            _commonDBContext.AuthContext.Update(authContext);
            await _commonDBContext.SaveChangesAsync();
        }
        private async Task CreateAuthContext()
        {
            var authContexts = _commonDBContext.AuthContext.Where(x => x.TenantId == TenantId);

            foreach (KeyValuePair<string,string> acr in dictACRValues)
            {
                if (authContexts?.Count() < dictACRValues.Count())
                {
                    await AddInDB(acr.Key, TenantId, acr.Value);
                }
                await _authContextClassReferencesOperations.CreateAuthenticationContextClassReferenceAsync
            (acr.Key, acr.Value, $"A new Authentication Context Class Reference created at {DateTime.Now.ToString()}", true);
            }
        }
        private async Task AddInDB(string id, string tenantId, string value)
        {
            AuthContext authContext = new AuthContext();
            authContext.TenantId = tenantId;
            authContext.AuthContextType = id;
            authContext.AuthContextValue = value;
            _commonDBContext.AuthContext.Add(authContext);
            await _commonDBContext.SaveChangesAsync();
        }
    }
}
