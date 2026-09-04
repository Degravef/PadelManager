using Bll.Services;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Interfaces.Repositories;
using Moq;

namespace BllTest.Services;

public class StatisticsServiceTests
{
    private readonly Mock<IPaymentRepository> _paymentRepository = new();
    private readonly StatisticsService _sut;

    public StatisticsServiceTests()
    {
        _sut = new StatisticsService(_paymentRepository.Object);
    }

    [Fact]
    public async Task CalculateRevenueAsync_SumsValidatedPayments()
    {
        int[] siteIds = [1, 2];
        var start = new DateOnly(2026, 9, 1);
        var end = new DateOnly(2026, 9, 30);
        _paymentRepository.Setup(r => r.GetValidatedBySitesAndPeriodAsync(siteIds, start, end)).ReturnsAsync(
        [
            new Payment { MemberId = 1, Amount = 15m, Status = PaymentStatus.Validated },
            new Payment { MemberId = 2, Amount = 45m, Status = PaymentStatus.Validated }
        ]);

        var result = await _sut.CalculateRevenueAsync(siteIds, start, end);

        Assert.Equal(60m, result.Amount);
        Assert.Equal(start, result.Start);
        Assert.Equal(end, result.End);
    }

    [Fact]
    public async Task CalculateRevenueAsync_NoPayments_ReturnsZero()
    {
        _paymentRepository.Setup(r => r.GetValidatedBySitesAndPeriodAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync([]);

        var result = await _sut.CalculateRevenueAsync([1], new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30));

        Assert.Equal(0m, result.Amount);
    }
}
