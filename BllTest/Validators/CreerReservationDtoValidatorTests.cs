using Bll.Validators;
using Core.Dtos;

namespace BllTest.Validators;

public class CreerReservationDtoValidatorTests
{
    private static readonly TimeProvider FakeTimeProvider =
        new FakeTimeProviderStub(new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero));

    private readonly CreerReservationDtoValidator _sut = new(FakeTimeProvider);

    [Fact]
    public async Task Validate_ValidDto_Passes()
    {
        var dto = new CreerReservationDto(1, new DateOnly(2026, 9, 5), new TimeOnly(10, 0));

        var result = await _sut.ValidateAsync(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_TerrainIdZero_Fails()
    {
        var dto = new CreerReservationDto(0, new DateOnly(2026, 9, 5), new TimeOnly(10, 0));

        var result = await _sut.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreerReservationDto.TerrainId));
    }

    [Fact]
    public async Task Validate_DateInThePast_Fails()
    {
        var dto = new CreerReservationDto(1, new DateOnly(2026, 8, 1), new TimeOnly(10, 0));

        var result = await _sut.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreerReservationDto.Date));
    }

    [Fact]
    public async Task Validate_DateIsToday_Passes()
    {
        var dto = new CreerReservationDto(1, new DateOnly(2026, 9, 1), new TimeOnly(10, 0));

        var result = await _sut.ValidateAsync(dto);

        Assert.True(result.IsValid);
    }

    
    [Fact]
    public async Task Validate_TodayButStartTimeAlreadyPassed_Fails()
    {
        var todayAtNoon = new FakeTimeProviderStub(new DateTimeOffset(2026, 9, 1, 12, 0, 0, TimeSpan.Zero));
        var sut = new CreerReservationDtoValidator(todayAtNoon);
        var dto = new CreerReservationDto(1, new DateOnly(2026, 9, 1), new TimeOnly(10, 0));

        var result = await sut.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreerReservationDto.Date));
    }

    private sealed class FakeTimeProviderStub(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
