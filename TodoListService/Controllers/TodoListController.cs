// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TodoListService.Models;

namespace TodoListService.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
// [RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
[Route("api/[controller]")]
public class TodoListController : Controller
{
    private readonly CommonDBContext _commonDBContext;
    private readonly IConfiguration _configuration;

    public TodoListController(IConfiguration configuration, CommonDBContext commonDBContext)
    {
        _configuration = configuration;
        _commonDBContext = commonDBContext;
    }

    // GET: api/values
    [HttpGet]
    public async Task<IEnumerable<Todo>> GetAsync()
    {
        return await _commonDBContext.Todo.ToListAsync();
    }

    // GET: api/values
    [HttpGet("{id}", Name = "Get")]
    public async Task<Todo> GetAsync(int id)
    {
        CheckForRequiredAuthContext(Request.Method);
        return await _commonDBContext.Todo.FirstOrDefaultAsync(t => t.Id == id);
    }

    [HttpDelete("{id}")]
    public async Task DeleteAsync(int id)
    {
        CheckForRequiredAuthContext(Request.Method);
        var todo = await _commonDBContext.Todo.FindAsync(id);

        if (todo != null)
        {
            _commonDBContext.Todo.Remove(todo);
            await _commonDBContext.SaveChangesAsync();
        }
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Todo todo)
    {
        CheckForRequiredAuthContext(Request.Method);
        var todonew = new Todo
        { 
            Owner = HttpContext.User.Identity.Name, 
            Title = todo.Title 
        };

        _commonDBContext.Todo.Add(todonew);
        await _commonDBContext.SaveChangesAsync();

        return Ok(todonew);
    }

    // PATCH api/values
    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchAsync(int id, [FromBody] Todo todo)
    {
        if (id != todo.Id)
        {
            return NotFound();
        }

        _commonDBContext.Todo.Update(todo);
        await _commonDBContext.SaveChangesAsync();

        return Ok(todo);
    }

    /// <summary>
    /// Retrieves the acrsValue from database for the request method.
    /// Checks if the access token has acrs claim with acrsValue.
    /// If does not exists then adds WWW-Authenticate and throws UnauthorizedAccessException exception.
    /// </summary>
    public void CheckForRequiredAuthContext(string method)
    {
        string savedAuthContextId = _commonDBContext.AuthContext.FirstOrDefault(x => x.Operation == method && x.TenantId == _configuration["AzureAD:TenantId"])?.AuthContextId;

        if (!string.IsNullOrEmpty(savedAuthContextId))
        {
            var context = HttpContext;

            string authenticationContextClassReferencesClaim = "acrs";

            if (context == null || context.User == null || context.User.Claims == null || !context.User.Claims.Any())
            {
                throw new ArgumentNullException("context", "No Usercontext is available to pick claims from");
            }

            var acrsClaim = context.User.FindAll(authenticationContextClassReferencesClaim).FirstOrDefault(x => x.Value == savedAuthContextId);

            if (acrsClaim?.Value != savedAuthContextId)
            {
                if (IsClientCapableofClaimsChallenge(context))
                {
                    string clientId = _configuration.GetSection("AzureAd").GetSection("ClientId").Value;
                    var base64str = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"access_token\":{\"acrs\":{\"essential\":true,\"value\":\"" + savedAuthContextId + "\"}}}"));

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
    public bool IsClientCapableofClaimsChallenge(HttpContext context)
    {
        string clientCapabilitiesClaim = "xms_cc";

        if (context == null || context.User == null || context.User.Claims == null || !context.User.Claims.Any())
        {
            throw new ArgumentNullException(nameof(context), "No Usercontext is available to pick claims from");
        }

        var ccClaim = context.User.FindAll(clientCapabilitiesClaim).FirstOrDefault(x => x.Type == "xms_cc");

        if (ccClaim != null && ccClaim.Value == "cp1")
        {
            return true;
        }

        return false;
    }
}