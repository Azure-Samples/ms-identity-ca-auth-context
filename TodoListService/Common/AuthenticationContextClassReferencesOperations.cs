extern alias BetaLib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Beta = BetaLib.Microsoft.Graph;

namespace TodoListService;

public class AuthenticationContextClassReferencesOperations
{
    private readonly Beta.GraphServiceClient _graphServiceClient;

    public AuthenticationContextClassReferencesOperations(Beta.GraphServiceClient graphServiceClient)
    {
        _graphServiceClient = graphServiceClient;
    }

    public async Task<List<Beta.AuthenticationContextClassReference>> ListAuthenticationContextClassReferencesAsync()
    {
        var allAuthenticationContextClassReferences = new List<Beta.AuthenticationContextClassReference>();

        try
        {
            var authenticationContextClassreferences = await _graphServiceClient.Identity
                    .ConditionalAccess
                    .AuthenticationContextClassReferences
                    .Request()
                    .GetAsync();

            if (authenticationContextClassreferences != null)
            {
                allAuthenticationContextClassReferences = await ProcessIAuthenticationContextClassReferenceRootPoliciesCollectionPage(authenticationContextClassreferences);
            }
        }
        catch (ServiceException e)
        {
            Console.WriteLine($"We could not retrieve the existing ACRs: {e}");

            if (e.InnerException != null)
            {
                var exp = (MicrosoftIdentityWebChallengeUserException)e.InnerException;
                throw exp;
            }

            throw e;
        }

        return allAuthenticationContextClassReferences;
    }

    public async Task<Beta.AuthenticationContextClassReference> GetAuthenticationContextClassReferenceByIdAsync(string acrId)
    {
        try
        {
            return await _graphServiceClient
                .Identity
                .ConditionalAccess
                .AuthenticationContextClassReferences[acrId]
                .Request()
                .GetAsync();
        }
        catch (ServiceException gex)
        {
            if (gex.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                throw;
            }

            return null;
        }
    }

    public async Task<Beta.AuthenticationContextClassReference> CreateAuthenticationContextClassReferenceAsync(string id, string displayName, string description, bool IsAvailable)
    {
        try
        {
            return await _graphServiceClient
                .Identity.ConditionalAccess
                .AuthenticationContextClassReferences
                .Request()
                .AddAsync(new Beta.AuthenticationContextClassReference
                {
                    Id = id,
                    DisplayName = displayName,
                    Description = description,
                    IsAvailable = IsAvailable,
                    ODataType = null
                });
        }
        catch (ServiceException e)
        {
            Console.WriteLine("We could not add a new ACR: " + e.Error.Message);

            return null;
        }
    }

    public async Task<Beta.AuthenticationContextClassReference> UpdateAuthenticationContextClassReferenceAsync(string acrId, bool isAvailable, string displayName = null, string description = null)
    {
        var acrToUpdate = await GetAuthenticationContextClassReferenceByIdAsync(acrId);

        if (acrToUpdate == null)
        {
            throw new ArgumentNullException(nameof(acrId), $"No ACR matching '{acrId}' exists");
        }

        try
        {
            return await _graphServiceClient
                .Identity
                .ConditionalAccess
                .AuthenticationContextClassReferences[acrId]
                .Request()
                .UpdateAsync(new Beta.AuthenticationContextClassReference
                {
                    Id = acrId,
                    DisplayName = displayName ?? acrToUpdate.DisplayName,
                    Description = description ?? acrToUpdate.Description,
                    IsAvailable = isAvailable,
                    ODataType = null
                });
        }
        catch (ServiceException e)
        {
            Console.WriteLine("We could not update the ACR: " + e.Error.Message);

            return null;
        }
    }

    public async Task DeleteAuthenticationContextClassReferenceAsync(string acrId)
    {
        try
        {
            await _graphServiceClient
                .Identity
                .ConditionalAccess
                .AuthenticationContextClassReferences[acrId]
                .Request()
                .DeleteAsync();
        }
        catch (ServiceException e)
        {
            Console.WriteLine($"We could not delete the ACR with Id-{acrId}: {e}");
        }
    }

    private async Task<List<Beta.AuthenticationContextClassReference>> ProcessIAuthenticationContextClassReferenceRootPoliciesCollectionPage(Beta.IConditionalAccessRootAuthenticationContextClassReferencesCollectionPage authenticationContextClassreferencesPage)
    {
        var allAuthenticationContextClassReferences = new List<Beta.AuthenticationContextClassReference>();

        try
        {
            if (authenticationContextClassreferencesPage != null)
            {
                var pageIterator = PageIterator<Beta.AuthenticationContextClassReference>.CreatePageIterator(_graphServiceClient, authenticationContextClassreferencesPage, (authenticationContextClassreference) =>
                {
                    Console.WriteLine(PrintAuthenticationContextClassReference(authenticationContextClassreference));
                    allAuthenticationContextClassReferences.Add(authenticationContextClassreference);
                    return true;
                });

                await pageIterator.IterateAsync();

                while (pageIterator.State != PagingState.Complete)
                {
                    await pageIterator.ResumeAsync();
                }
            }
        }
        catch (ServiceException e)
        {
            Console.WriteLine($"We could not process the authentication context class references list: {e}");
            
            return null;
        }

        return allAuthenticationContextClassReferences;
    }

    public string PrintAuthenticationContextClassReference(Beta.AuthenticationContextClassReference authenticationContextClassReference, bool verbose = false)
    {
        var sb = new StringBuilder();

        if (authenticationContextClassReference != null)
        {
            sb.AppendLine($"DisplayName-{authenticationContextClassReference.DisplayName}, IsAvailable-{authenticationContextClassReference.IsAvailable}, Id- '{authenticationContextClassReference.Id}'");

            if (verbose)
            {
                sb.AppendLine($", Description-'{authenticationContextClassReference.Description}'");
            }
        }
        else
        {
            Console.WriteLine("The provided authenticationContextClassReference is null!");
        }

        return sb.ToString();
    }
}