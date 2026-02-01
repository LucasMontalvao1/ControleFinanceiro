using FluentValidation;
using ControleFinanceiro.Application.DTOs;

namespace ControleFinanceiro.Application.Validators;

public class LancamentoRequestValidator : AbstractValidator<LancamentoRequest>
{
    public LancamentoRequestValidator()
    {
        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("A descrição é obrigatória.")
            .MaximumLength(200).WithMessage("A descrição deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Valor)
            .GreaterThan(0).WithMessage("O valor deve ser maior que zero.");

        RuleFor(x => x.Data)
            .NotEmpty().WithMessage("A data é obrigatória.");

        RuleFor(x => x.Tipo)
            .NotEmpty().WithMessage("O tipo (Entrada ou Saida) é obrigatório.")
            .Must(x => x == "Entrada" || x == "Saida")
            .WithMessage("O tipo deve ser 'Entrada' ou 'Saida'.");

        RuleFor(x => x.CategoriaId)
            .GreaterThan(0).WithMessage("Selecione uma categoria válida.");
    }
}
