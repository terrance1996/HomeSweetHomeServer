using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HomeSweetHomeServer.Exceptions
{
    //Custom exceptions will generate for any exception and error
    public class CustomException : Exception
    {
        public int StatusCode { get; set; }
        public SerializableError Errors { get; set; }

        public CustomException(int statusCode = (int)HttpStatusCode.InternalServerError)
        {
            Errors = new SerializableError();
            this.StatusCode = statusCode;
        }

        public CustomException(string message, int statusCode = (int)HttpStatusCode.InternalServerError) : base(message)
        {
            Errors = new SerializableError();
            this.StatusCode = statusCode;
        }

        public CustomException(string message, Exception Inner, int statusCode = (int)HttpStatusCode.InternalServerError) : base(message, Inner)
        {
            Errors = new SerializableError();
            this.StatusCode = statusCode;
        }

        public CustomException(SerializableError errors, int statusCode = (int)HttpStatusCode.InternalServerError)
        {
            Errors = errors;
            this.StatusCode = statusCode;
        }

        //Adds errors to list
        public void AddError(string errorName, object errorMessage)
        {
            Errors.Add(errorName, errorMessage);
        }

        //Throws after errors added to list
        public void Throw()
        {
            string message = JsonConvert.SerializeObject(Errors);
            throw new CustomException(message, StatusCode);
        }
    }
}
