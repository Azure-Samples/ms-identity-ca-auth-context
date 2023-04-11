---
page_type: sample
languages:
  - csharp
products:
  - aspnet-core
  - ms-graph
  - azure-active-directory  
name: Use the Conditional Access auth context to perform step-up authentication for high-privilege operations in a Web API
urlFragment: ms-identity-ca-auth-context
description: "This sample demonstrates using the Conditional Access auth context to perform step-up authentication for high-privilege and sensitive operations in a web API."
---
# Use the Conditional Access auth context to perform step-up authentication for high-privilege operations in a Web API

 1. [Overview](#overview)
 1. [Scenario](#scenario)
 1. [Contents](#contents)
 1. [Prerequisites](#prerequisites)
 1. [Setup](#setup)
 1. [Registration](#registration)
 1. [Running the sample](#running-the-sample)
 1. [Explore the sample](#explore-the-sample)
 1. [About the code](#about-the-code)
 1. [Deployment](#deployment)
 1. [More information](#more-information)
 1. [Community Help and Support](#community-help-and-support)
 1. [Contributing](#contributing)

## Overview

This code sample uses the Conditional Access Auth Context to demand a higher bar of authentication for certain high-privileged and sensitive operations in a [protected Web API](https://docs.microsoft.com/azure/active-directory/develop/scenario-protected-web-api-overview).

> To use the CA Auth context in a Web app, please try the[Use the Conditional Access auth context to perform step\-up authentication for high\-privilege operations in a Web app](https://github.com/Azure-Samples/ms-identity-dotnetcore-ca-auth-context-app/blob/main/README.md) code sample.


## Scenario

1. The client ASP.NET Core Web App uses the [Microsoft.Identity.Web](https://aka.ms/microsoft-identity-web) and Microsoft Authentication Library for .NET ([MSAL.NET](https://aka.ms/msal-net)) to sign-in and obtain a JWT access token from **Azure AD**.
1. The access token is used as a bearer token to authorize the user to call the ASP.NET Core Web API protected **Azure AD**.
1. For sensitive operations, the Web API can be configured to demand step-up authentication, like MFA, from the signed-in user

![Overview](./ReadmeFiles/topology.png)

> :information_source: Check out the recorded session on this topic: [Use Conditional Access Auth Context in your app for step-up authentication](https://www.youtube.com/watch?v=_iO7CfoktTY&ab_channel=Microsoft365Community)
> 
## Prerequisites

- Either [Visual Studio](https://visualstudio.microsoft.com/downloads/) or [Visual Studio Code](https://code.visualstudio.com/download) and [.NET Core SDK](https://www.microsoft.com/net/learn/get-started)
- An **Azure AD** tenant. For more information see: [How to get an Azure AD tenant](https://docs.microsoft.com/azure/active-directory/develop/quickstart-create-new-tenant)
- A user account in your **Azure AD** tenant. This sample will not work with a **personal Microsoft account**. Therefore, if you signed in to the [Azure portal](https://portal.azure.com) with a personal account and have never created a user account in your directory before, you need to do that now.
- [Azure AD premium P1](https://azure.microsoft.com/pricing/details/active-directory/) is required to work with Conditional Access policies.

## Setup

### Step 1: Clone or download this repository

From your shell or command line:

```console
git clone https://github.com/Azure-Samples/ms-identity-ca-auth-context.git
```

or download and extract the repository .zip file.

> :warning: To avoid path length limitations on Windows, we recommend cloning into a directory near the root of your drive.

### Step 2: Install project dependencies

```console
    dotnet restore
```

```console
    dotnet restore
```

### Register the sample application(s) with your Azure Active Directory tenant

There are two projects in this sample. Each needs to be separately registered in your Azure AD tenant. To register these projects, you can:

- follow the steps below for manually register your apps
- or use PowerShell scripts that:
  - **automatically** creates the Azure AD applications and related objects (passwords, permissions, dependencies) for you.
  - modify the projects' configuration files.

<details>
  <summary>Expand this section if you want to use this automation:</summary>

> :warning: If you have never used **Azure AD Powershell** before, we recommend you go through the [App Creation Scripts](./AppCreationScripts/AppCreationScripts.md) once to ensure that your environment is prepared correctly for this step.

1. On Windows, run PowerShell as **Administrator** and navigate to the root of the cloned directory
1. If you have never used Azure AD Powershell before, we recommend you go through the [App Creation Scripts](./AppCreationScripts/AppCreationScripts.md) once to ensure that your environment is prepared correctly for this step.
1. In PowerShell run:

   ```PowerShell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
   ```

1. Run the script to create your Azure AD application and configure the code of the sample application accordingly.
1. In PowerShell run:

   ```PowerShell
   cd .\AppCreationScripts\
   .\Configure.ps1
   ```

   > Other ways of running the scripts are described in [App Creation Scripts](./AppCreationScripts/AppCreationScripts.md)
   > The scripts also provide a guide to automated application registration, configuration and removal which can help in your CI/CD scenarios.

</details>

### Choose the Azure AD tenant where you want to create your applications

As a first step you'll need to:

1. Sign in to the [Azure portal](https://portal.azure.com).
1. If your account is present in more than one Azure AD tenant, select your profile at the top right corner in the menu on top of the page, and then **switch directory** to change your portal session to the desired Azure AD tenant.

### Register the service app (TodoListService-acrs-webapi)

1. Navigate to the [Azure portal](https://portal.azure.com) and select the **Azure AD** service.
1. Select the **App Registrations** blade on the left, then select **New registration**.
1. In the **Register an application page** that appears, enter your application's registration information:
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `TodoListService-acrs-webapi`.
   - Under **Supported account types**, select **Accounts in this organizational directory only**.
   - In the **Redirect URI (optional)** section, select **Web** in the combo-box and enter the following redirect URI: `https://localhost:44351/`.
     > Note that there are more than one redirect URIs used in this sample. You'll need to add them from the **Authentication** tab later after the app has been created successfully.
1. Select **Register** to create the application.
1. In the app's registration screen, find and note the **Application (client) ID**. You use this value in your app's configuration file(s) later in your code.
1. In the app's registration screen, select **Authentication** in the menu.
   - If you don't have a platform added, select **Add a platform** and select the **Web** option.
   - In the **Redirect URIs** section, enter the following redirect URIs.
      - `https://localhost:44351/signin-oidc`
1. Select **Save** to save your changes.
1. In the app's registration screen, select the **Certificates & secrets** blade in the left to open the page where we can generate secrets and upload certificates.
1. In the **Client secrets** section, select **New client secret**:
   - Type a key description (for instance `app secret`),
   - Select one of the available key durations (**In 1 year**, **In 2 years**, or **Never Expires**) as per your security posture.
   - The generated key value will be displayed when you select the **Add** button. Copy the generated value for use in the steps later.
   - You'll need this key later in your code's configuration files. This key value will not be displayed again, and is not retrievable by any other means, so make sure to note it from the Azure portal before navigating to any other screen or blade.
1. In the app's registration screen, select the **API permissions** blade in the left to open the page where we add access to the APIs that your application needs.
   - Select the **Add a permission** button and then,
   - Ensure that the **Microsoft APIs** tab is selected.
   - In the *Commonly used Microsoft APIs* section, select **Microsoft Graph**
   - In the **Delegated permissions** section, select the **Policy.Read.ConditionalAccess**, **Policy.ReadWrite.ConditionalAccess** in the list. Use the search box if necessary. `Note: The Graph permission, **Policy.ReadWrite.ConditionalAccess** is required for creating new auth context records by this sample. In production, the permission, **Policy.Read.ConditionalAccess** should be sufficient to read existing values and thus is recommended.`
   - Select the **Add permissions** button at the bottom.
   - Select **Grant admin consent for (your tenant)**.
1. In the app's registration screen, select the **Expose an API** blade to the left to open the page where you can declare the parameters to expose this app as an API for which client applications can obtain [access tokens](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) for.
The first thing that we need to do is to declare the unique [resource](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-auth-code-flow) URI that the clients will be using to obtain access tokens for this Api. To declare an resource URI, follow the following steps:
   - Select `Set` next to the **Application ID URI** to generate a URI that is unique for this app.
   - For this sample, accept the proposed Application ID URI (`api://{clientId}`) by selecting **Save**.
1. All APIs have to publish a minimum of one [scope](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-auth-code-flow#request-an-authorization-code) for the client's to obtain an access token successfully. To publish a scope, follow the following steps:
   - Select **Add a scope** button open the **Add a scope** screen and Enter the values as indicated below:
        - For **Scope name**, use `access_as_user`.
        - Select **Admins and users** options for **Who can consent?**.
        - For **Admin consent display name** type `Access TodoListService-acrs-webapi`.
        - For **Admin consent description** type `Allows the app to access TodoListService-acrs-webapi as the signed-in user.`
        - For **User consent display name** type `Access TodoListService-acrs-webapi`.
        - For **User consent description** type `Allow the application to access TodoListService-acrs-webapi on your behalf.`
        - Keep **State** as **Enabled**.
        - Select the **Add scope** button on the bottom to save this scope.
1. Change the app manifest of the API  to request the AuthenticationContextClassReferences `acrs` and client capabilities claim `xms_cc` claim.

```json
"optionalClaims": 
{
  "accessToken": [
    {
      "additionalProperties": [],
      "essential": false,
      "name": "xms_cc",
      "source": null
    }
  ],
  "idToken": [],
  "saml2Token": []
}
```

#### Configure the service app (TodoListService-acrs-webapi) to use your app registration

Open the project in your IDE (like Visual Studio or Visual Studio Code) to configure the code.

> In the steps below, "ClientID" is the same as "Application ID" or "AppId".

1. Open the `TodoListService\appsettings.json` file.
1. Find the key `Domain` and replace the existing value with your Azure AD tenant name.
1. Find the key `TenantId` and replace the existing value with your Azure AD tenant ID.
1. Find the key `ClientId` and replace the existing value with the application ID (clientId) of `TodoListService-acrs-webapi` app copied from the Azure portal.
1. Find the key `ClientSecret` and replace the existing value with the key you saved during the creation of `TodoListService-acrs-webapi` copied from the Azure portal.

### Register the client app (TodoListClient-acrs-webapp)

1. Navigate to the [Azure portal](https://portal.azure.com) and select the **Azure AD** service.
1. Select the **App Registrations** blade on the left, then select **New registration**.
1. In the **Register an application page** that appears, enter your application's registration information:
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `TodoListClient-acrs-webapp`.
   - Under **Supported account types**, select **Accounts in this organizational directory only**.
   - In the **Redirect URI (optional)** section, select **Web** in the combo-box and enter the following redirect URI: `https://localhost:44321/`.
     > Note that there are more than one redirect URIs used in this sample. You'll need to add them from the **Authentication** tab later after the app has been created successfully.
1. Select **Register** to create the application.
1. In the app's registration screen, find and note the **Application (client) ID**. You use this value in your app's configuration file(s) later in your code.
1. In the app's registration screen, select **Authentication** in the menu.
   - If you don't have a platform added, select **Add a platform** and select the **Web** option.
   - In the **Redirect URIs** section, enter the following redirect URIs.
      - `https://localhost:44321/signin-oidc`
   - In the **Front-channel logout URL** section, set it to `https://localhost:44321/signout-oidc`.
1. Select **Save** to save your changes.
1. In the app's registration screen, select the **Certificates & secrets** blade in the left to open the page where we can generate secrets and upload certificates.
1. In the **Client secrets** section, select **New client secret**:
   - Type a key description (for instance `app secret`),
   - Select one of the available key durations (**In 1 year**, **In 2 years**, or **Never Expires**) as per your security posture.
   - The generated key value will be displayed when you select the **Add** button. Copy the generated value for use in the steps later.
   - You'll need this key later in your code's configuration files. This key value will not be displayed again, and is not retrievable by any other means, so make sure to note it from the Azure portal before navigating to any other screen or blade.
1. In the app's registration screen, select the **API permissions** blade in the left to open the page where we add access to the APIs that your application needs.
   - Select the **Add a permission** button and then,
   - Ensure that the **My APIs** tab is selected.
   - In the list of APIs, select the API `TodoListService-acrs-webapi`.
   - In the **Delegated permissions** section, select the **Access 'TodoListService-acrs-webapi'** in the list. Use the search box if necessary.
   - Select the **Add permissions** button at the bottom.

#### Configure the client app (TodoListClient-acrs-webapp) to use your app registration

Open the project in your IDE (like Visual Studio or Visual Studio Code) to configure the code.

> In the steps below, "ClientID" is the same as "Application ID" or "AppId".

1. Open the `TodoListClient\appsettings.json` file.
1. Find the key `Domain` and replace the existing value with your Azure AD tenant name.
1. Find the key `TenantId` and replace the existing value with your Azure AD tenant ID.
1. Find the key `ClientId` and replace the existing value with the application ID (clientId) of `TodoListClient-acrs-webapp` app copied from the Azure portal.
1. Find the key `ClientSecret` and replace the existing value with the key you saved during the creation of `TodoListClient-acrs-webapp` copied from the Azure portal.
1. Find the key `TodoListScope` and replace the existing value with Scope.
1. Find the key `TodoListBaseAddress` and replace the existing value with the base address of `TodoListService-acrs-webapi` (by default `https://localhost:44351`).

## Running the sample

> For Visual Studio Users
>
> Clean the solution, rebuild the solution, and run it.  You might want to go into the solution properties and set both projects as startup projects, with the service project starting first.

```console
cd TodoListClient
dotnet restore
dotnet run
```

```console
cd TodoListService
dotnet restore
dotnet run
```

## Explore the sample

### Configure the Web API

1. We'd first replicate the experience of an admin configuring the auth contexts. For that, browse to `https://localhost:44321` and sign-in using a tenant Admin account. Click on the **Admin** link on the menu.

    ![Overview](./ReadmeFiles/Admin.png)
2. As a first step, you will ensure that a set of Auth Context is already available in this tenant. Click the **CreateOrFetch** button to check if they exist. If they don't , the code will create three sample auth context entries for you. These three entires are named `Require strong authentication`, `Require compliant devices` and `Require trusted locations`.

    > Note: The Graph permission, **Policy.ReadWrite.ConditionalAccess** is required for creating new records. In production, the permission, **Policy.Read.ConditionalAccess** should be sufficient to read existing values and thus is recommended.

    ![Overview](./ReadmeFiles/Create-Fetch_Click.png)

    Select an operation in the Web API and an `Authentication Context` value to apply and select **SaveOrUpdate**. This updates this mapping in the local app's database. We advise you use the same auth context value for operations if possible, as this ensures that the suer is redirected to Azure AD just once to perform the step-up authN.

>Note: When changing auth context mappings, have the user sign-out and sign-back in for the changes to take effect.

1. Go to `View Details page to get details of data saved on the Web API side in its database. You can select **Delete** if you need to delete a mapping from the local database.

    ![Overview](./ReadmeFiles/ViewDetails.png)
The web API is now ready to challenge users for step-up auth for the selected operations.

### Configure a Conditional Access policy to use auth context in Azure portal

1. Navigate to Azure Active Directory> Security > Conditional Access
1. Select **New policy** and go to **Cloud apps or actions**. In dropdown select **Authentication context**. The newly created auth context values will be listed for you to be used in this CA policy.

    ![Overview](./ReadmeFiles/AuthContext.png)

    Select the value and create the policy as required. For example, you might want the user to satisfy a MFA challenge if the auth context value is 'Medium'.

### Test in the Web App

1. Browse `https://localhost:44321` and sign-in.
1. Select `TodoList` page and perform the operations.

    ![Overview](./ReadmeFiles/ToDoList.png)

If an operation was saved for a certain authContext and there is a CA policy configured and enabled, the user  will be redirected to Azure AD and ask to perform the required step(s) like MFA.

> :information_source: Did the sample not work for you as expected? Then please reach out to us using the [GitHub Issues](../../issues) page.

## About the code

### Code for the Web API (ToDoListService)

1. In `Startup.cs`, below lines of code enables Microsoft identity platform endpoint to protect the Web API.

    ```csharp
    services.AddMicrosoftIdentityWebApiAuthentication(Configuration);
    ```

    Below lines of code enables Microsoft identity platform endpoint to authenticate the users and to call MS Graph.

    ```csharp
    services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
             .AddMicrosoftIdentityWebApp(Configuration, "AzureAd", subscribeToOpenIdConnectMiddlewareDiagnosticsEvents: true)
                 .EnableTokenAcquisitionToCallDownstreamApi()
                     .AddMicrosoftGraph(Configuration.GetSection("GraphBeta"))
                 .AddInMemoryTokenCaches();
    ```

1. In `AdminController.cs`, the method **GetAuthenticationContextValues**  returns a default set of AuthN context values for the app to work with, either from Graph or a default hard coded set.

    ```csharp
    private async Task<Dictionary<string, string>> GetAuthenticationContextValues()
    {
         Dictionary<string, string> dictACRValues = new Dictionary<string, string>()
            {
                    {"C1","Require strong authentication" },
                    {"C2","Require compliant devices" },
                    {"C3","Require trusted locations" }
        };

        string sessionKey = "ACRS";

        if (HttpContext.Session.Get<Dictionary<string, string>>(sessionKey) != default)
        {
            dictACRValues = HttpContext.Session.Get<Dictionary<string, string>>(sessionKey);
        }
        else
        {
            var existingAuthContexts = await _authContextClassReferencesOperations.ListAuthenticationContextClassReferencesAsync();

            if (existingAuthContexts.Count() > 0)                 
            {
                dictACRValues.Clear();

                foreach (var authContext in existingAuthContexts)
                {
                    dictACRValues.Add(authContext.Id, authContext.DisplayName);
                }

                HttpContext.Session.Set<Dictionary<string, string>>(sessionKey, dictACRValues);
            }
        }
        return dictACRValues;
    }
    ```

    **CreateOrFetch** method checks if auth context exists then retrieve the list from graph by calling **ListAuthenticationContextClassReferencesAsync** method else call **CreateAuthContextViaGraph** to create the auth context.

    ```csharp
    public async Task<List<Beta.AuthenticationContextClassReference>> CreateOrFetch()
    {
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
    ```

1. `AuthenticationContextClassReferencesOperations.cs` contains methods that call graph to perform various operations. In current sample we have used create and get methods. **CreateAuthenticationContextClassReferenceAsync** method creates the auth context: `Note: this class calls the /beta endpoint of Graph as the API was available only on the /beta endpoint at the time of this sample's publishing`

    ```csharp
   public async Task<Beta.AuthenticationContextClassReference> CreateAuthenticationContextClassReferenceAsync(string id, string displayName, string description, bool IsAvailable)
    {
        Beta.AuthenticationContextClassReference newACRObject = null;
    
        try
        {
            newACRObject = await _graphServiceClient.Identity.ConditionalAccess.AuthenticationContextClassReferences.Request().AddAsync(new Beta.AuthenticationContextClassReference
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
    
        return newACRObject;
    }
    ```

    **ListAuthenticationContextClassReferencesAsync** method get the existing auth context values from graph.

    ```csharp
    public async Task<List<Beta.AuthenticationContextClassReference>> ListAuthenticationContextClassReferencesAsync()
    {
        List<Beta.AuthenticationContextClassReference> allAuthenticationContextClassReferences = new List<Beta.AuthenticationContextClassReference>();
    
        try
        {
            Beta.IConditionalAccessRootAuthenticationContextClassReferencesCollectionPage authenticationContextClassreferences = await _graphServiceClient.Identity.ConditionalAccess.AuthenticationContextClassReferences.Request().GetAsync();
    
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
    ```

1. In `TodoListController.cs`, the method **CheckForRequiredAuthContext** retrieves the acrsValue from database for the request method. Then checks if the access token has `acrs` claim with acrsValue. If does not exists then adds WWW-Authenticate and throws UnauthorizedAccessException exception.

    ```csharp
    public void CheckForRequiredAuthContext(string method)
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
                string clientId = _configuration.GetSection("AzureAd").GetSection("ClientId").Value;
                var base64str = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"access_token\":{\"acrs\":{\"essential\":true,\"value\":\"" + authType + "\"}}}"));
    
                context.Response.Headers.Append("WWW-Authenticate", $"Bearer realm=\"\", authorization_uri=\"https://login.microsoftonline.com/common/oauth2/authorize\", client_id=\"" + clientId + "\", error=\"insufficient_claims\", claims=\"" + base64str + "\", cc_type=\"authcontext\"");
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                string message = string.Format(CultureInfo.InvariantCulture, "The presented access tokens had insufficient claims. Please request for claims requested in the WWW-Authentication header and try again.");
                context.Response.WriteAsync(message);
                context.Response.CompleteAsync();
                throw new UnauthorizedAccessException(message);
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
    ```

### Code for the Web App (TodoListClient)

Methods in `TodoListController.cs` challenges the user if exception is thrown from Web API, as shown in below method:

```csharp
public async Task<ActionResult> Create([Bind("Title,Owner")] Todo todo)
{
    try
    {
        await _todoListService.AddAsync(todo);
    }
    catch (WebApiMsalUiRequiredException hex)
    {
        try
        {
            var claimChallenge = ExtractAuthenticationHeader.ExtractHeaderValues(hex);
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

```

## More information

- [Developers’ guide to Conditional Access authentication context](https://docs.microsoft.com/azure/active-directory/develop/developer-guide-conditional-access-authentication-context)
- [Claims challenges, claims requests, and client capabilities](https://docs.microsoft.com/azure/active-directory/develop/claims-challenge)
- [Microsoft identity platform (Azure Active Directory for developers)](https://docs.microsoft.com/azure/active-directory/develop/)
- [Overview of Microsoft Authentication Library (MSAL)](https://docs.microsoft.com/azure/active-directory/develop/msal-overview)
- [Quickstart: Register an application with the Microsoft identity platform (Preview)](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
- [Quickstart: Configure a client application to access web APIs (Preview)](https://docs.microsoft.com/azure/active-directory/develop/quickstart-configure-app-access-web-apis)
- [Understanding Azure AD application consent experiences](https://docs.microsoft.com/azure/active-directory/develop/application-consent-experience)
- [Understand user and admin consent](https://docs.microsoft.com/azure/active-directory/develop/howto-convert-app-to-be-multi-tenant#understand-user-and-admin-consent)
- [Application and service principal objects in Azure Active Directory](https://docs.microsoft.com/azure/active-directory/develop/app-objects-and-service-principals)
- [National Clouds](https://docs.microsoft.com/azure/active-directory/develop/authentication-national-cloud#app-registration-endpoints)
- [MSAL code samples](https://docs.microsoft.com/azure/active-directory/develop/sample-v2-code)

For more information about how OAuth 2.0 protocols work in this scenario and other scenarios, see [Authentication Scenarios for Azure AD](https://docs.microsoft.com/azure/active-directory/develop/authentication-flows-app-scenarios).

## Community Help and Support

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`azure-active-directory` `azure-ad-b2c` `ms-identity` `adal` `msal`].

If you find a bug in the sample, raise the issue on [GitHub Issues](../../issues).

To provide feedback on or suggest features for Azure Active Directory, visit [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
