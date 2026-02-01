using FluentValidation;
using ControleFinanceiro.Application.DTOs;

namespace ControleFinanceiro.Application.Validators;

public class CategoriaRequestValidator : AbstractValidator<CategoriaRequest>
{
    public CategoriaRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome da categoria é obrigatório.")
            .MaximumLength(50).WithMessage("O nome deve ter no máximo 50 caracteres.");

        RuleFor(x => x.Tipo)
            .NotEmpty().WithMessage("O tipo (Receita ou Despesa) é obrigatório.")
            .Must(x => x == "Receita" || x == "Despesa")
            .WithMessage("O tipo deve ser 'Receita' ou 'Despesa'.");
    }
}
