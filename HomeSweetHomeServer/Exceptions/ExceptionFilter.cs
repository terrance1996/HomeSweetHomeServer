using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace HomeSweetHomeServer.Exceptions
{
    //Catches exceptions which generated custom or automatically and responses to client
    public class ExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext Context)
        {
            string message;
            int statusCode;

            Exception _exception = Context.Exception;

            if (_exception is CustomException) //Generated exception about any error
            {
                var exception = (CustomException)_exception;
                message = exception.Message;
                statusCode = exception.StatusCode;
            } else //Generated about unhandled system exception
            {
                message = JsonConvert.SerializeObject(new Dictionary<string, string> { { "Unhandled Error", _exception.Message } });
                statusCode = (int)HttpStatusCode.InternalServerError;
            }

            Context.ExceptionHandled = true;
            
            //Response creation
            HttpResponse Response = Context.HttpContext.Response;
            Response.StatusCode = statusCode;
            Response.ContentType = "application/json";
            Response.WriteAsync(message);
        }
    }
}
