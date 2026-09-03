using Bll.Validators;
using Core.Dtos;

namespace BllTest.Validators;

public class CreateHoraireSiteDtoValidatorTests
{
    private static readonly TimeProvider FakeTimeProvider = new FakeTimeProviderStub(new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero));
    private readonly CreateHoraireSiteDtoValidator _sut = new(FakeTimeProvider);

    [Fact]
    public async Task Validate_ValidDto_Passes()
    {
        var dto = new CreateHoraireSiteDto(2026, new TimeOnly(8, 0), new TimeOnly(21, 0));

        var result = await _sut.ValidateAsync(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_YearInThePast_Fails()
    {
        var dto = new CreateHoraireSiteDto(2025, new TimeOnly(8, 0), new TimeOnly(21, 0));

        var result = await _sut.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateHoraireSiteDto.Annee));
    }

    [Fact]
    public async Task Validate_ClosingTimeBeforeOpeningTime_Fails()
    {
        var dto = new CreateHoraireSiteDto(2026, new TimeOnly(21, 0), new TimeOnly(8, 0));

        var result = await _sut.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateHoraireSiteDto.HeureDerniereReservation));
    }

    [Fact]
    public async Task Validate_NegativePrixMatch_Fails()
    {
        var dto = new CreateHoraireSiteDto(2026, new TimeOnly(8, 0), new TimeOnly(21, 0), -10m);

        var result = await _sut.ValidateAsync(dto);

        Assert.False(result.IsValid);
    }

    private sealed class FakeTimeProviderStub(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
