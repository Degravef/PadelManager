using Bll.Validators;
using Core.Dtos;

namespace BllTest.Validators;

public class AddPlayerDtoValidatorTests
{
    private readonly AddPlayerDtoValidator _sut = new();

    [Theory]
    [InlineData("G1")]
    [InlineData("S12345")]
    [InlineData("L99999")]
    public async Task Validate_ValidMatricule_Passes(string matricule)
    {
        var result = await _sut.ValidateAsync(new AddPlayerDto(matricule));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("X1")]
    [InlineData("G")]
    [InlineData("G123456")]
    [InlineData("")]
    public async Task Validate_InvalidMatricule_Fails(string matricule)
    {
        var result = await _sut.ValidateAsync(new AddPlayerDto(matricule));

        Assert.False(result.IsValid);
    }
}
