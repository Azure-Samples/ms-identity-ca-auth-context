// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
    [Route("api/[controller]")]
    public class TodoListController : Controller
    {
        CommonDBContext _commonDBContext;

        private readonly IHttpContextAccessor _contextAccessor;
        IConfiguration _configuration;
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
            if (todo == null)
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
        public void EnsureUserHasElevatedScope(string method)
        {
            string authType = _commonDBContext.AuthContext.FirstOrDefault(x => x.Operation == method && x.TenantId == _configuration["AzureAD:TenantId"])?.AuthContextType;
            if (!string.IsNullOrEmpty(authType))
            {

                HttpContext context = this.HttpContext;

                string authenticationContextClassReferencesClaim = "acrs";

                if (context == null || context.User == null || context.User.Claims == null || !context.User.Claims.Any())
                {
                    throw new ArgumentNullException("No Usercontext is available to pick claims from");
                }

                // Attempt with Scp claim
                Claim acrsClaim = context.User.FindFirst(authenticationContextClassReferencesClaim);

                if (acrsClaim == null || acrsClaim.Value != authType)
                {
                    //string requiredClaims = acrsClaim != null ? acrsClaim.Value + "," + authType : authType;
                    string clientId = _configuration.GetSection("AzureAd").GetSection("ClientId").Value;
                    var base64str = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"access_token\":{\"acrs\":{\"essential\":true,\"value\":\"" + authType + "\"}}}"));

                    context.Response.Headers.Append("WWW-Authenticate", $"Bearer realm=\"\", authorization_uri=\"https://login.microsoftonline.com/common/oauth2/authorize\", client_id=\"" + clientId + "\", error=\"insufficient_claims\", claims=\"" + base64str + "\", cc_type=\"authcontext\"");
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    string message = string.Format(CultureInfo.InvariantCulture, "The presented access tokens had insufficient claims. Please request for claims requested in the WWW-Authentication header and try again.");
                    context.Response.WriteAsync(message);
                    context.Response.CompleteAsync();
                }

            }
        }
    }
}