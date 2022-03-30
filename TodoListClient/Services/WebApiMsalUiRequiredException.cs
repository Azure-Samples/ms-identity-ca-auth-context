﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;

namespace TodoListClient.Services;

/// <summary>
/// Specialized excpetion handler for the TodoListService
/// </summary>
public class WebApiMsalUiRequiredException : Exception
{
    private readonly HttpResponseMessage httpResponseMessage;

    public WebApiMsalUiRequiredException(string message, HttpResponseMessage response) : base(message)
    {
        httpResponseMessage = response;
    }

    public HttpStatusCode StatusCode
    {
        get { return httpResponseMessage.StatusCode; }
    }

    public HttpResponseHeaders Headers
    {
        get { return httpResponseMessage.Headers; }
    }

    public HttpResponseMessage HttpResponseMessage
    {
        get { return httpResponseMessage; }
    }
}