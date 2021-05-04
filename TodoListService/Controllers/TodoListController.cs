// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web.Resource;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using TodoListService.Models;

namespace TodoListService.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
   // [RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
    [Route("api/[controller]")]
    public class TodoListController : Controller
    {
        private CommonDBContext _commonDBContext;

        private readonly IHttpContextAccessor _contextAccessor;
        private IConfiguration _configuration;

        public TodoListController(IHttpContextAccessor contextAccessor, IConfiguration configuration, CommonDBContext commonDBContext)
        {
            _contextAccessor = contextAccessor;
            _configuration = configuration;
            _commonDBContext = commonDBContext;
        }

        // GET: api/values
        [HttpGet]
        public IEnumerable<Todo> Get()
        {
            return _commonDBContext.Todo.ToList();
        }

        // GET: api/values
        [HttpGet("{id}", Name = "Get")]
        public Todo Get(int id)
        {
            EnsureUserHasElevatedScope(Request.Method);
            return _commonDBContext.Todo.FirstOrDefault(t => t.Id == id);
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            EnsureUserHasElevatedScope(Request.Method);
            var todo = _commonDBContext.Todo.Find(id);
            if (todo != null)
            {
                _commonDBContext.Todo.Remove(todo);
                _commonDBContext.SaveChanges();
            }
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post([FromBody] Todo todo)
        {
            EnsureUserHasElevatedScope(Request.Method);
            Todo todonew = new Todo() { Owner = HttpContext.User.Identity.Name, Title = todo.Title };
            _commonDBContext.Todo.Add(todonew);
            _commonDBContext.SaveChanges();
            return Ok(todo);
        }

        // PATCH api/values
        [HttpPatch("{id}")]
        public IActionResult Patch(int id, [FromBody] Todo todo)
        {
            if (id != todo.Id)
            {
                return NotFound();
            }

            _commonDBContext.Todo.Update(todo);
            _commonDBContext.SaveChanges();

            return Ok(todo);
        }

        /// <summary>
        /// Retreives the acrsValue from database for the request method.
        /// Checks if the access token has acrs claim with acrsValue.
        /// If does not exists then adds WWW-Authenticate and throws UnauthorizedAccessException exception.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public void EnsureUserHasElevatedScope(string method)
        {
            string authType = _commonDBContext.AuthContext.FirstOrDefault(x => x.Operation == method && x.TenantId == _configuration["AzureAD:TenantId"])?.AuthContextId;

            if (!string.IsNullOrEmpty(authType))
            {
                HttpContext context = this.HttpContext;

                string authenticationContextClassReferencesClaim = "acrs";

                if (context == null || context.User == null || context.User.Claims == null || !context.User.Claims.Any())
                {
                    throw new ArgumentNullException("No Usercontext is available to pick claims from");
                }

                Claim acrsClaim = context.User.FindAll(authenticationContextClassReferencesClaim).FirstOrDefault(x => x.Value == authType);

                if (acrsClaim == null || acrsClaim.Value != authType)
                {
                    if (IsClientCapableofClaimsChallenge(context))
                    {
                        string clientId = _configuration.GetSection("AzureAd").GetSection("ClientId").Value;
                        var base64str = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"access_token\":{\"acrs\":{\"essential\":true,\"value\":\"" + authType + "\"}}}"));

                        context.Response.Headers.Append("WWW-Authenticate", $"Bearer realm=\"\", authorization_uri=\"https://login.microsoftonline.com/common/oauth2/authorize\", client_id=\"" + clientId + "\", error=\"insufficient_claims\", claims=\"" + base64str + "\", cc_type=\"authcontext\"");
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        string message = string.Format(CultureInfo.InvariantCulture, "The presented access tokens had insufficient claims. Please request for claims requested in the WWW-Authentication header and try again.");
                        context.Response.WriteAsync(message);
                        context.Response.CompleteAsync();
                        throw new UnauthorizedAccessException(message);
                    }
                    else
                    {
                        throw new UnauthorizedAccessException("The caller does not meet the authentication  bar to carry our this operation. The service cannot allow this operation");
                    }
                }
            }
        }

        /// <summary>
        /// Evaluates for the presence of the client capabilities claim (xms_cc) and accordingly returns a response if present.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool IsClientCapableofClaimsChallenge(HttpContext context)
        {
            string clientCapabilitiesClaim = "xms_cc";

            if (context == null || context.User == null || context.User.Claims == null || !context.User.Claims.Any())
            {
                throw new ArgumentNullException("No Usercontext is available to pick claims from");
            }

            Claim ccClaim = context.User.FindAll(clientCapabilitiesClaim).FirstOrDefault(x => x.Type == "xms_cc");

            if (ccClaim != null && ccClaim.Value == "cp1")
            {
                return true;
            }

            return false;
        }
    }
}