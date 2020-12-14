﻿using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace RadishV2.Server.Application.Handler.Tests
{
    public class AddSetKeyHandlerTests
    {
        [Fact()]
        public void AddSetKeyHandlerTest()
        {
            var iLoggerMock = new Mock<ILogger<AddSetKeyHandler>>();
            var handler = new AddSetKeyHandler(iLoggerMock.Object);
            Assert.NotNull(handler);
        }
    }
}