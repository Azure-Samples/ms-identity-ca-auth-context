extern alias BetaLib;
using Microsoft.AspNetCore.Authorization;
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
    /// <summary>
    /// Functionality for Admin user account.
    /// Admin will add the Authentication context for the tenant
    /// Save and update data in database.
    /// </summary>
    [Authorize]
    public class AdminController : Controller
    {
        CommonDBContext _commonDBContext;
        AuthenticationContextClassReferencesOperations _authContextClassReferencesOperations;
        IConfiguration _configuration;

        // Default values for acrs claim.
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

        /// <summary>
        /// Retreives the authentication context and operation mapping saved in database for the tenant.
        /// </summary>
        /// <returns></returns>
        public IActionResult ViewDetails()
        {
            List<AuthContext> authContexts= _commonDBContext.AuthContext.Where(x => x.TenantId == TenantId).ToList();
            return View(authContexts);
        }

        /// <summary>
        /// Checks if AuthenticationContext exists.
        /// If not then create with default values and save in database.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Update the database to save mapping of operation and auth context.
        /// </summary>
        /// <param name="authContext"></param>
        /// <returns></returns>
        public async Task UpdateAuthContextDB(AuthContext authContext)
        {
            authContext.AuthContextType = dictACRValues.FirstOrDefault(x => x.Value == authContext.AuthContextValue).Key;
            _commonDBContext.AuthContext.Update(authContext);
            await _commonDBContext.SaveChangesAsync();
        }

        /// <summary>
        /// Create Authentication context for the tenant.
        /// Save the values in database.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Save the values in database.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tenantId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
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
