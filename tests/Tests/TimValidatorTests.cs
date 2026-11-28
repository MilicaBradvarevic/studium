using Application;
using Domain;
using Xunit;

namespace Tests;

public class TimValidatorTests
{
    private readonly TimValidator _validator = new();

    [Fact]
    public void Validate_NullTim_VracaGresku()
    {
        var result = _validator.Validate(null);

        Assert.False(result.IsValid);
        Assert.Contains("Tim ne sme biti null.", result.Errors);
    }

    [Fact]
    public void Validate_ValidanTim_VracaIsValidTrue()
    {
        var projekatId = Guid.NewGuid();
        var clan = new ClanTimaSnapshot(Guid.NewGuid(), "Milica");
        var tim = new Tim(Guid.NewGuid(), projekatId, "Tim A", [clan]);

        var result = _validator.Validate(tim);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
