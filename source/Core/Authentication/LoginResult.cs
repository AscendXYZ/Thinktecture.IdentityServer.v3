﻿/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Authentication
{
    public class LoginResult : IHttpActionResult
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly SignInMessage _message;
        private readonly IDictionary<string, object> _env;
        private readonly InternalConfiguration _internalConfig;

        public static string GetRedirectUrl(SignInMessage message, IDictionary<string, object> env, InternalConfiguration internalConfig)
        {
            var result = new LoginResult(message, env, internalConfig);
            var response = result.Execute();

            return response.Headers.Location.AbsoluteUri;
        }

        public LoginResult(SignInMessage message, IDictionary<string, object> env, InternalConfiguration internalConfig)
        {
            _message = message;
            _env = env;
            _internalConfig = internalConfig;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            var response = new HttpResponseMessage(HttpStatusCode.Redirect);

            try
            {
                var sim = _message.Protect(600, _internalConfig.DataProtector);
                var url = _env.GetIdentityServerBaseUrl() + Constants.RoutePaths.Login;
                url += "?message=" + sim;

                var uri = new Uri(url);
                response.Headers.Location = uri;
            }
            catch
            {
                response.Dispose();
                throw;
            }

            Logger.Info("Redirecting to login page");
            return response;
        }
    }
}