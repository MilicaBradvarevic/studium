using Domain;

namespace Application.Validators;

public class TimskiZadatakValidator
{
    public ValidationResult Validate(TimskiZadatak? zadatak)
    {
        var result = new ValidationResult();

        if (zadatak is null)
        {
            result.AddError("Timski zadatak ne sme biti null.");
            return result;
        }

        return result;
    }
}
