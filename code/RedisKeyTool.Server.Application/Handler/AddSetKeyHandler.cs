﻿using MediatR;
using Microsoft.Extensions.Logging;
using RedisKeyTool.Server.Application.Command;
using RedisKeyTool.Server.Application.Utils;
using RedisKeyTool.Shared;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RedisKeyTool.Server.Application.Handler
{
    /// <summary>
    /// Add Set Key Handler
    /// </summary>
    /// <seealso cref="MediatR.IRequestHandler{RedisKeyTool.Server.Application.Command.AddSetKey, RedisKeyTool.Shared.ApplicationResponse}" />
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

                    if (request.KeyPayload.KeyListItem.Expiry != null && request.KeyPayload.KeyListItem.Expiry != "00:00:00")
                    {
                        var expTime = TimeSpan.Parse(request.KeyPayload.KeyListItem.Expiry);
                        db.KeyExpire(request.KeyPayload.KeyListItem.KeyName, expTime);
                    }
                    else
                    {
                        var expiry = db.KeyTimeToLive(request.KeyPayload.KeyListItem.KeyName);
                        if (expiry != null)
                        {
                            TimeSpan? timeSpan = null;
                            db.KeyExpire(request.KeyPayload.KeyListItem.KeyName, timeSpan);
                        }
                    }
                }
                else
                {
                    response = new ApplicationResponse(true, "Failed to Add or Update Keys");
                }

                redisServer.Close();
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