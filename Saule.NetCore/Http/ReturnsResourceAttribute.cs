﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Saule.Http
{
    /// <summary>
    /// Attribute used to specify the api resource related to a controller action.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class ReturnsResourceAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReturnsResourceAttribute"/> class.
        /// </summary>
        /// <param name="resourceType">The type of the resource this controller action returns.</param>
        public ReturnsResourceAttribute(Type resourceType)
        {
            if (!resourceType.IsSubclassOf(typeof(ApiResource)))
            {
                throw new ArgumentException("Resource types must inherit from Saule.ApiResource");
            }

            Resource = resourceType.CreateInstance<ApiResource>();
        }

        /// <summary>
        /// Gets the type of the resource this controller action returns.
        /// </summary>
        public ApiResource Resource { get; }

        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            var requestHeaders = actionContext.HttpContext.Request.GetTypedHeaders();

            var accept = requestHeaders.Accept
                .Where(a => a.MediaType == Constants.MediaType);

            if (accept.Count() > 0 && accept.All(a => a.Parameters.Any()))
            {
                // no json api media type without parameters
                actionContext.Result = new StatusCodeResult(StatusCodes.Status406NotAcceptable);
            }

            var contentType = requestHeaders.ContentType;
            if (contentType != null && contentType.Parameters.Any())
            {
                // client is sending json api media type with parameters
                actionContext.Result = new StatusCodeResult(StatusCodes.Status415UnsupportedMediaType);
            }

            actionContext.HttpContext.Items.Add(Constants.PropertyNames.ResourceDescriptor, Resource);
            base.OnActionExecuting(actionContext);
        }
    }
}