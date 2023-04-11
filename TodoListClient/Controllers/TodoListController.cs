using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System;
using System.Threading.Tasks;
using TodoListClient.Services;
using TodoListService.Models;

namespace TodoListClient.Controllers;

public class TodoListController : Controller
{
    private readonly ITodoListService _todoListService;
    private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;

    public TodoListController(ITodoListService todoListService,
                        MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler)
    {
        _todoListService = todoListService;
        _consentHandler = consentHandler;
    }

    // GET: TodoList
    [AuthorizeForScopes(ScopeKeySection = "TodoList:TodoListScope")]
    public async Task<ActionResult> Index()
    {
        return View(await _todoListService.GetAsync()); 
    }

    // GET: TodoList/Details/5
    public async Task<ActionResult> Details(int id)
    {
        try
        {
            return View(await _todoListService.GetAsync(id));
        }
        catch (WebApiMsalUiRequiredException hex)
        {
            // Challenges the user if exception is thrown from Web API.
            try
            {
                var claimChallenge = WwwAuthenticateParameters.GetClaimChallengeFromResponseHeaders(hex.Headers);

                _consentHandler.ChallengeUser(new string[] { "api://[Enter_client_ID_Of_TodoListService-v2_from_Azure_Portal,_e.g._2ec40e65-ba09-4853-bcde-bcb60029e596]/access_as_user" }, claimChallenge);

                return new EmptyResult();

            }
            catch (Exception ex)
            {
                _consentHandler.HandleException(ex);
            }

            Console.WriteLine(hex.Message);
        }
        return View();
    }

    // GET: TodoList/Create
    public ActionResult Create()
    {
        var todo = new Todo{ Owner = HttpContext.User.Identity.Name };
        return View(todo);
    }

    // POST: TodoList/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Create([Bind("Title,Owner")] Todo todo)
    {
        try
        {
            await _todoListService.AddAsync(todo);
        }
        catch (WebApiMsalUiRequiredException hex)
        {
            // Challenges the user if exception is thrown from Web API.
            try
            {
                var claimChallenge = WwwAuthenticateParameters.GetClaimChallengeFromResponseHeaders(hex.Headers);

                _consentHandler.ChallengeUser(new string[] { "api://[Enter_client_ID_Of_TodoListService-v2_from_Azure_Portal,_e.g._2ec40e65-ba09-4853-bcde-bcb60029e596]/access_as_user" }, claimChallenge);

                return new EmptyResult();

            }
            catch (Exception ex)
            {
                _consentHandler.HandleException(ex);
            }

            Console.WriteLine(hex.Message);
        }
        return RedirectToAction("Index");
    }

    // GET: TodoList/Edit/5
    public async Task<ActionResult> Edit(int id)
    {
        try
        {
            var todo = await _todoListService.GetAsync(id);

            if (todo == null)
            {
                return NotFound();
            }

            return View(todo);

        }
        catch (WebApiMsalUiRequiredException hex)
        {
            // Challenges the user if exception is thrown from Web API.
            try
            {
                var claimChallenge = WwwAuthenticateParameters.GetClaimChallengeFromResponseHeaders(hex.Headers);

                _consentHandler.ChallengeUser(new string[] { "api://[Enter_client_ID_Of_TodoListService-v2_from_Azure_Portal,_e.g._2ec40e65-ba09-4853-bcde-bcb60029e596]/access_as_user" }, claimChallenge);

                return new EmptyResult();

            }
            catch (Exception ex)
            {
                _consentHandler.HandleException(ex);
            }

            Console.WriteLine(hex.Message);
        }
        return View();
    }

    // POST: TodoList/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(int id, [Bind("Id,Title,Owner")] Todo todo)
    {
        await _todoListService.EditAsync(todo);

        return RedirectToAction("Index");
    }

    // GET: TodoList/Delete/5
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var todo = await _todoListService.GetAsync(id);

            if (todo == null)
            {
                return NotFound();
            }

            return View(todo);
        }
        catch (WebApiMsalUiRequiredException hex)
        {
            // Challenges the user if exception is thrown from Web API.
            try
            {
                var claimChallenge = WwwAuthenticateParameters.GetClaimChallengeFromResponseHeaders(hex.Headers);

                _consentHandler.ChallengeUser(new string[] { "api://[Enter_client_ID_Of_TodoListService-v2_from_Azure_Portal,_e.g._2ec40e65-ba09-4853-bcde-bcb60029e596]/access_as_user" }, claimChallenge);

                return new EmptyResult();
            }
            catch (Exception ex)
            {
                _consentHandler.HandleException(ex);
            }

            Console.WriteLine(hex.Message);
        }

        return View();
    }

    // POST: TodoList/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Delete(int id, [Bind("Id,Title,Owner")] Todo todo)
    {
        try
        {
            await _todoListService.DeleteAsync(id);
        }
        catch (WebApiMsalUiRequiredException hex)
        {
            // Challenges the user if exception is thrown from Web API.
            try
            {
                var claimChallenge = WwwAuthenticateParameters.GetClaimChallengeFromResponseHeaders(hex.Headers);

                _consentHandler.ChallengeUser(new string[] { "api://[Enter_client_ID_Of_TodoListService-v2_from_Azure_Portal,_e.g._2ec40e65-ba09-4853-bcde-bcb60029e596]/access_as_user" }, claimChallenge);

                return new EmptyResult();
            }
            catch (Exception ex)
            {
                _consentHandler.HandleException(ex);
            }

            Console.WriteLine(hex.Message);
        }

        return RedirectToAction("Index");
    }
}
