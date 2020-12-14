﻿using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace RadishV2.Server.Application.Handler.Tests
{
    public class GetRedisKeyHandlerTests
    {
        [Fact()]
        public void GetRedisKeyHandlerTest()
        {
            var iLoggerMock = new Mock<ILogger<GetRedisKeyHandler>>();
            var handler = new GetRedisKeyHandler(iLoggerMock.Object);
            Assert.NotNull(handler);
        }
    }
}