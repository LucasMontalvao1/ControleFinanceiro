using FluentValidation;
using ControleFinanceiro.Application.DTOs;

namespace ControleFinanceiro.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("o login (username) é obrigatório.")
            .MinimumLength(3).WithMessage("O login deve ter pelo menos 3 caracteres.")
            .MaximumLength(50).WithMessage("O login deve ter no máximo 50 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("O e-mail informado é inválido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("A senha é obrigatória.")
            .MinimumLength(6).WithMessage("A senha deve ter pelo menos 6 caracteres.");
    }
}
