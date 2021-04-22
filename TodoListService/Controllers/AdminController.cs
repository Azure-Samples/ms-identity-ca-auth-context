extern alias BetaLib;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoListService.Common;
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
        //private CommonDBContext _commonDBContext;
        private AuthenticationContextClassReferencesOperations _authContextClassReferencesOperations;
        private IConfiguration _configuration;

        private string TenantId;

        public AdminController(IConfiguration configuration, AuthenticationContextClassReferencesOperations authContextClassReferencesOperations, CommonDBContext commonDBContext)
        {
            _configuration = configuration;
            _authContextClassReferencesOperations = authContextClassReferencesOperations;
            TenantId = _configuration["AzureAd:TenantId"];
        }

        public async Task<IActionResult> Index()
        {
            // Defaults
            IList<SelectListItem> AuthContextValues = new List<SelectListItem>();
            //{
            //    new SelectListItem{Text= dictACRValues["C1"]},
            //    new SelectListItem{Text= dictACRValues["C2"]},
            //    new SelectListItem { Text = dictACRValues["C3"]}
            //};

            IEnumerable<SelectListItem> Operations = new List<SelectListItem>
                {
                    new SelectListItem{Text= "Get"},
                    new SelectListItem{Text= "Post"},
                    new SelectListItem{ Text= "Delete"}
                };

            // If this tenant already has authcontext available, we use those instead.
            var existingAuthContexts = await getAuthenticationContextValues();

            if (existingAuthContexts.Count() > 0)
            {
                AuthContextValues.Clear();

                foreach (var authContext in existingAuthContexts)
                {
                    AuthContextValues.Add(new SelectListItem() { Text = authContext.Value, Value = authContext.Key});
                }
            }

            // Set data to be used in the UI
            TempData["TenantId"] = TenantId;
            TempData["AuthContextValues"] = AuthContextValues;
            TempData["Operations"] = Operations;

            return View();
        }

        // returns a default set of AuthN context values for the app to work with, either from Graph a or a default hard coded set
        private async Task<Dictionary<string, string>> getAuthenticationContextValues()
        {
            // Default values, if no values anywhere, this table will be used.
            Dictionary<string, string> dictACRValues = new Dictionary<string, string>()
                {
                    {"C1","Regular privilege" },
                    {"C2","Medium-high privilege" },
                    {"C3","High privilege" }
                };

            string sessionKey = "ACRS";

            // If already saved in Session, use it
            if (HttpContext.Session.Get<Dictionary<string, string>>(sessionKey) != default)
            {
                dictACRValues = HttpContext.Session.Get<Dictionary<string, string>>(sessionKey);
            }
            else
            {
                var existingAuthContexts = await _authContextClassReferencesOperations.ListAuthenticationContextClassReferencesAsync();

                if (existingAuthContexts.Count() > 0)                 // If this tenant already has authcontext available, we use those instead.
                {
                    dictACRValues.Clear();

                    foreach (var authContext in existingAuthContexts)
                    {
                        dictACRValues.Add(authContext.Id, authContext.DisplayName);
                    }

                    // Save this in session
                    HttpContext.Session.Set<Dictionary<string, string>>(sessionKey, dictACRValues);
                }
            }

            return dictACRValues;
        }

        /// <summary>
        /// Retreives the authentication context and operation mapping saved in database for the tenant.
        /// </summary>
        /// <returns></returns>
        public IActionResult ViewDetails()
        {
            List<AuthContext> authContexts = new List<AuthContext>();

            using (var commonDBContext = new CommonDBContext(_configuration))
            {
                authContexts = commonDBContext.AuthContext.Where(x => x.TenantId == TenantId).ToList();
            }

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
            // Call Graph to check first
            var lstPolicies = await _authContextClassReferencesOperations.ListAuthenticationContextClassReferencesAsync();

            if (lstPolicies?.Count > 0)
            {
                return lstPolicies;
            }
            else
            {
                await CreateAuthContextViaGraph();
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
            Dictionary<string, string> dictACRValues = await getAuthenticationContextValues();
            authContext.AuthContextId = dictACRValues.FirstOrDefault(x => x.Value == authContext.AuthContextDisplayName).Key;

            using (var commonDBContext = new CommonDBContext(_configuration))
            {

                if(commonDBContext.AuthContext.Where(x => x.AuthContextId == authContext.AuthContextId && x.Operation == authContext.Operation).Count() == 0)
                {
                    commonDBContext.AuthContext.Add(authContext);
                   
                }
                else
                {
                    commonDBContext.AuthContext.Update(authContext);
                }


                await commonDBContext.SaveChangesAsync();
            }

        }


        /// <summary>
        /// Create Authentication context for the tenant.
        /// Save the values in database.
        /// </summary>
        /// <returns></returns>
        private async Task CreateAuthContextViaGraph()
        {
            Dictionary<string, string> dictACRValues = await getAuthenticationContextValues();


            IQueryable<AuthContext> authContexts = null;

            using (var commonDBContext = new CommonDBContext(_configuration))
            {
                authContexts = commonDBContext.AuthContext.Where(x => x.TenantId == TenantId);
            }

            foreach (KeyValuePair<string, string> acr in dictACRValues)
            {
                if (authContexts?.Count() < dictACRValues.Count())
                {
                    await AddInDB(acr.Key, TenantId, acr.Value);
                }
                await _authContextClassReferencesOperations.CreateAuthenticationContextClassReferenceAsync(acr.Key, acr.Value, $"A new Authentication Context Class Reference created at {DateTime.Now.ToString()}", true);
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
            authContext.AuthContextId = id;
            authContext.AuthContextDisplayName = value;

            using (var commonDBContext = new CommonDBContext(_configuration))
            {
                commonDBContext.AuthContext.Add(authContext);
                await commonDBContext.SaveChangesAsync();
            }
        }
    }
}