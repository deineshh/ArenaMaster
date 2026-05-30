using ArenaMaster.Api.DTOs.Tournament;
using FluentValidation;

namespace ArenaMaster.Api.Validators;

public class CreateTournamentValidator : AbstractValidator<CreateTournamentRequest>
{
    private static readonly int[] AllowedSizes = [4, 8, 16, 32];

    public CreateTournamentValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.DisciplineId).NotEmpty();
        RuleFor(x => x.Format).Must(f => f is "single_elimination" or "double_elimination");
        RuleFor(x => x.ParticipantType).Must(t => t is "solo" or "team");
        RuleFor(x => x.MaxParticipants).Must(s => AllowedSizes.Contains(s));
        RuleFor(x => x.TeamSize).NotNull().When(x => x.ParticipantType == "team");
        RuleFor(x => x.RegistrationEndsAt).LessThan(x => x.StartsAt);
    }
}

public class CreateTeamValidator : AbstractValidator<DTOs.Team.CreateTeamRequest>
{
    public CreateTeamValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
