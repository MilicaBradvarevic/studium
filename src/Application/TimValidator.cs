using Domain;

namespace Application;

public class TimValidator
{
    public ValidationResult Validate(Tim? tim)
    {
        var result = new ValidationResult();

        if (tim is null)
        {
            result.AddError("Tim ne sme biti null.");
            return result;
        }

        if (string.IsNullOrWhiteSpace(tim.Naziv))
        {
            result.AddError("Naziv tima je obavezan.");
        }

        if (tim.Clanovi is null || !tim.Clanovi.Any())
        {
            result.AddError("Tim mora imati bar jednog člana.");
        }

        return result;
    }
}
