using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MockQueryable.Moq;
using Xunit;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Features.ExchangeRates.Commands.UpdateExchangeRate;
using IkerFinance.Domain.Entities;

namespace IkerFinance.UnitTests.Application.Features.ExchangeRates.Commands
{
    public class UpdateExchangeRateCommandHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly UpdateExchangeRateCommandHandler _handler;

        public UpdateExchangeRateCommandHandlerTests()
        {
            _mockContext = new Mock<IApplicationDbContext>();
            _handler = new UpdateExchangeRateCommandHandler(_mockContext.Object);
        }

    }
}
