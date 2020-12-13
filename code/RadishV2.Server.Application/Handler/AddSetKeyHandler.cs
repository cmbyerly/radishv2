﻿using MediatR;
using Microsoft.Extensions.Logging;
using RadishV2.Server.Application.Command;
using RadishV2.Server.Application.Utils;
using RadishV2.Shared;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RadishV2.Server.Application.Handler
{
    /// <summary>
    /// Add Set Key Handler
    /// </summary>
    /// <seealso cref="MediatR.IRequestHandler{RadishV2.Server.Application.Command.AddSetKey, RadishV2.Shared.ApplicationResponse}" />
    public class AddSetKeyHandler : IRequestHandler<AddSetKey, ApplicationResponse>
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<AddSetKeyHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddSetKeyHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public AddSetKeyHandler(ILogger<AddSetKeyHandler> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handles a request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>
        /// Response from the request
        /// </returns>
        public Task<ApplicationResponse> Handle(AddSetKey request, CancellationToken cancellationToken)
        {
            ApplicationResponse response;

            try
            {
                var redisServer = ConnectionBuilder.BuildConnectToRedis(request.KeyPayload.RedisSetting);

                if (redisServer != null)
                {
                    var db = redisServer.GetDatabase(request.KeyPayload.RedisSetting.SelectedDatabase);

                    if (ConnectionBuilder.DoesKeyExist(db, request.KeyPayload.KeyListItem.KeyName))
                    {
                        response = new ApplicationResponse(false, "Key Exists");
                    }
                    else
                    {
                        foreach (var value in request.KeyPayload.KeyListItem.KeyValues)
                        {
                            db.SetAdd(request.KeyPayload.KeyListItem.KeyName, value.Value);
                        }

                        response = new ApplicationResponse(true, "Added or Updated Keys");
                    }
                }
                else
                {
                    response = new ApplicationResponse(true, "Failed to Add or Update Keys");
                }

                redisServer.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                response = new ApplicationResponse(false, ex.Message);
            }

            return Task.FromResult(response);
        }
    }
}