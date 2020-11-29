﻿using MediatR;
using RadishV2.Server.Application.Command;
using RadishV2.Server.Application.Utils;
using RadishV2.Shared;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RadishV2.Server.Application.Handler
{
    /// <summary>
    /// Add Hash Key Handler
    /// </summary>
    /// <seealso cref="MediatR.IRequestHandler{RadishV2.Server.Application.Command.AddHashKey, RadishV2.Shared.ApplicationResponse}" />
    public class AddHashKeyHandler : IRequestHandler<AddHashKey, ApplicationResponse>
    {
        /// <summary>
        /// Handles a request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>
        /// Response from the request
        /// </returns>
        public Task<ApplicationResponse> Handle(AddHashKey request, CancellationToken cancellationToken)
        {
            ApplicationResponse response;

            try
            {
                var redisServer = ConnectionBuilder.BuildConnectToRedis(request.KeyPayload.RedisSetting);

                if (redisServer != null)
                {
                    var db = redisServer.GetDatabase(request.KeyPayload.RedisSetting.SelectedDatabase);

                    List<HashEntry> hashEntries = new List<HashEntry>();

                    foreach (var value in request.KeyPayload.KeyListItem.KeyValues)
                    {
                        HashEntry hashEntry = new HashEntry(value.Name, value.Value);
                        hashEntries.Add(hashEntry);
                    }

                    db.HashSet(request.KeyPayload.KeyListItem.KeyName, hashEntries.ToArray());

                    response = new ApplicationResponse(true, "Added or Updated Keys");
                }
                else
                {
                    response = new ApplicationResponse(true, "Failed to Add or Update Keys");
                }

                redisServer.Dispose();
            }
            catch (Exception ex)
            {
                response = new ApplicationResponse(false, ex.Message);
            }

            return Task.FromResult(response);
        }
    }
}