using Bll.Validators;
using Core.Dtos;

namespace BllTest.Validators;

public class CreateReservationDtoValidatorTests
{
    private static readonly TimeProvider FakeTimeProvider =
        new FakeTimeProviderStub(new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero));

    private readonly CreateReservationDtoValidator _sut = new(FakeTimeProvider);

    [Fact]
    public async Task Validate_ValidDto_Passes()
    {
        var dto = new CreateReservationDto(1, new DateOnly(2026, 9, 5), new TimeOnly(10, 0));

        var result = await _sut.ValidateAsync(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_CourtIdZero_Fails()
    {
        var dto = new CreateReservationDto(0, new DateOnly(2026, 9, 5), new TimeOnly(10, 0));

        var result = await _sut.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateReservationDto.CourtId));
    }

    [Fact]
    public async Task Validate_DateInThePast_Fails()
    {
        var dto = new CreateReservationDto(1, new DateOnly(2026, 8, 1), new TimeOnly(10, 0));

        var result = await _sut.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateReservationDto.Date));
    }

    [Fact]
    public async Task Validate_DateIsToday_Passes()
    {
        var dto = new CreateReservationDto(1, new DateOnly(2026, 9, 1), new TimeOnly(10, 0));

        var result = await _sut.ValidateAsync(dto);

        Assert.True(result.IsValid);
    }

    // RG-RES-008: both the date AND the time must be later than the current instant — not just the date.
    [Fact]
    public async Task Validate_TodayButStartTimeAlreadyPassed_Fails()
    {
        var todayAtNoon = new FakeTimeProviderStub(new DateTimeOffset(2026, 9, 1, 12, 0, 0, TimeSpan.Zero));
        var sut = new CreateReservationDtoValidator(todayAtNoon);
        var dto = new CreateReservationDto(1, new DateOnly(2026, 9, 1), new TimeOnly(10, 0));

        var result = await sut.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateReservationDto.Date));
    }

    private sealed class FakeTimeProviderStub(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
