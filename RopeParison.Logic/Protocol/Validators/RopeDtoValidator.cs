using FluentValidation;
using RopeParison.Protocol;
using RopeParison.Logic.Protocol.Extensions;
using RopeParison.Logic.Services;
using System;
using RopeParison.Data.Services;

namespace RopeParison.Logic.Protocol.Validators
{
    public class RopeDtoValidator : AbstractValidator<RopeDto>
    {
        private IRopeService _ropeService;

        public RopeDtoValidator(IRopeService ropeService)
        {
            _ropeService = ropeService;

            RuleFor(p => p.Name).NotEmpty().WithMessage(MustBeSpecified("Name"));
            RuleFor(p => p.Name).MaximumLength(150).WithMessage("Name cannot be longer than 150 characters.");
            RuleFor(p => p.Name).Must((rope, name) => _ropeService.IsRopeNameUnique(rope.RopeId, name!))
                .WithMessage("Name must be unique.").When(p => !string.IsNullOrWhiteSpace(p.Name));
            
            RuleFor(p => p.Brand).NotEmpty().WithMessage(MustBeSpecified("Brand"));
            RuleFor(p => p.Brand).Must(b => b == null || b.BrandId > 0).WithMessage("Select a valid brand.");

            RuleFor(p => p.Category).NotEmpty().WithMessage(MustBeSpecified("Category"));
            RuleFor(p => p.Category).Must(c => c == null || Enum.IsDefined(c.Category)).WithMessage("Select a valid category.");

            RuleFor(p => p.Diameter).NotEmpty().WithMessage(MustBeSpecified("Diameter"));
            RuleFor(p => p.Diameter).GreaterThan(0).WithMessage(NotZeroOrLess("Diameter"));

            RuleFor(p => p.MassPerUnitLength).NotEmpty().WithMessage(MustBeSpecified("Mass/m"));
            RuleFor(p => p.MassPerUnitLength).GreaterThan(0).WithMessage(NotZeroOrLess("Mass/m"));

            RuleFor(p => p.SheathPercentage).Must(v => v == null || (double.IsFinite(v.Value) && v > 0 && v < 100))
                .WithMessage("Sheath percentage must be between 0 and 100, exclusive, when specified.");
            RuleFor(p => p.SheathSlippage).Must(v => v == null || (double.IsFinite(v.Value) && v >= 0 && v <= 100))
                .WithMessage("Sheath slippage must be between 0 and 100 when specified.");
            RuleFor(p => p.Diameter).Must(double.IsFinite).WithMessage("Diameter must be finite.");
            RuleFor(p => p.MassPerUnitLength).Must(double.IsFinite).WithMessage("Mass/m must be finite.");
            RuleFor(p => p.ImpactForce55kgOneStrand).Must(FiniteOrAbsent).When(p => p.Has_ImpactForce55kgOneStrand());
            RuleFor(p => p.ImpactForce80kgOneStrand).Must(FiniteOrAbsent).When(p => p.Has_ImpactForce80kgOneStrand());
            RuleFor(p => p.ImpactForce80kgTwoStrand).Must(FiniteOrAbsent).When(p => p.Has_ImpactForce80kgTwoStrand());
            RuleFor(p => p.StaticElongation80kgOneStrand).Must(FiniteOrAbsent).When(p => p.Has_StaticElongation80kgOneStrand());
            RuleFor(p => p.StaticElongation80kgTwoStrand).Must(FiniteOrAbsent).When(p => p.Has_StaticElongation80kgTwoStrand());
            RuleFor(p => p.DynamicElongation55kgOneStrand).Must(FiniteOrAbsent).When(p => p.Has_DynamicElongation55kgOneStrand());
            RuleFor(p => p.DynamicElongation80kgOneStrand).Must(FiniteOrAbsent).When(p => p.Has_DynamicElongation80kgOneStrand());
            RuleFor(p => p.DynamicElongation80kgTwoStrand).Must(FiniteOrAbsent).When(p => p.Has_DynamicElongation80kgTwoStrand());
            RuleFor(p => p.DropsBeforeBreak55kgOneStrand).NotNull().GreaterThan(0)
                .When(p => p.Category?.Category is Category.Half or Category.HalfTwin or Category.Triple);
            RuleFor(p => p.DropsBeforeBreak80kgOneStrand).NotNull().GreaterThan(0)
                .When(p => p.Category?.Category is Category.Single or Category.Triple);
            RuleFor(p => p.DropsBeforeBreak80kgTwoStrand).NotNull().GreaterThan(0)
                .When(p => p.Category?.Category is Category.Twin or Category.HalfTwin or Category.Triple);

            RuleFor(p => p.ImpactForce55kgOneStrand).NotEmpty().WithMessage(MustBeSpecified("Impact Force (55kg, One Strand)"))
                .When(p => p.Has_ImpactForce55kgOneStrand());
            RuleFor(p => p.ImpactForce55kgOneStrand).GreaterThan(0).WithMessage(NotZeroOrLess("Impact Force (55kg, One Strand)"))
                .When(p => p.Has_ImpactForce55kgOneStrand());

            RuleFor(p => p.ImpactForce80kgOneStrand).NotEmpty().WithMessage(MustBeSpecified("Impact Force (80kg, One Strand)"))
                .When(p => p.Has_ImpactForce80kgOneStrand());
            RuleFor(p => p.ImpactForce80kgOneStrand).GreaterThan(0).WithMessage(NotZeroOrLess("Impact Force (80kg, One Strand)"))
                .When(p => p.Has_ImpactForce80kgOneStrand());

            RuleFor(p => p.ImpactForce80kgTwoStrand).NotEmpty().WithMessage(MustBeSpecified("Impact Force (80kg, Two Strand)"))
                .When(p => p.Has_ImpactForce80kgTwoStrand());
            RuleFor(p => p.ImpactForce80kgTwoStrand).GreaterThan(0).WithMessage(NotZeroOrLess("Impact Force (80kg, Two Strand)"))
                .When(p => p.Has_ImpactForce80kgTwoStrand());

            RuleFor(p => p.StaticElongation80kgOneStrand).NotEmpty().WithMessage(MustBeSpecified("Static Elongation (80kg, One Strand)"))
                .When(p => p.Has_StaticElongation80kgOneStrand());
            RuleFor(p => p.StaticElongation80kgOneStrand).GreaterThan(0).WithMessage(NotZeroOrLess("Static Elongation (80kg, One Strand)"))
                .When(p => p.Has_StaticElongation80kgOneStrand());

            RuleFor(p => p.StaticElongation80kgTwoStrand).NotEmpty().WithMessage(MustBeSpecified("Static Elongation (80kg, Two Strand)"))
                .When(p => p.Has_StaticElongation80kgTwoStrand());
            RuleFor(p => p.StaticElongation80kgTwoStrand).GreaterThan(0).WithMessage(NotZeroOrLess("Static Elongation (80kg, Two Strand)"))
                .When(p => p.Has_StaticElongation80kgTwoStrand());

            RuleFor(p => p.DynamicElongation55kgOneStrand).NotEmpty().WithMessage(MustBeSpecified("Dynamic Elongation (55kg, One Strand)"))
                .When(p => p.Has_DynamicElongation55kgOneStrand());
            RuleFor(p => p.DynamicElongation55kgOneStrand).GreaterThan(0).WithMessage(NotZeroOrLess("Dynamic Elongation (55kg, One Strand)"))
                .When(p => p.Has_DynamicElongation55kgOneStrand());

            RuleFor(p => p.DynamicElongation80kgOneStrand).NotEmpty().WithMessage(MustBeSpecified("Dynamic Elongation (80kg, One Strand)"))
                .When(p => p.Has_DynamicElongation80kgOneStrand());
            RuleFor(p => p.DynamicElongation80kgOneStrand).GreaterThan(0).WithMessage(NotZeroOrLess("Dynamic Elongation (80kg, One Strand)"))
                .When(p => p.Has_DynamicElongation80kgOneStrand());

            RuleFor(p => p.DynamicElongation80kgTwoStrand).NotEmpty().WithMessage(MustBeSpecified("Dynamic Elongation (80kg, Two Strand)"))
                .When(p => p.Has_DynamicElongation80kgTwoStrand());
            RuleFor(p => p.DynamicElongation80kgTwoStrand).GreaterThan(0).WithMessage(NotZeroOrLess("Dynamic Elongation (80kg, Two Strand)"))
                .When(p => p.Has_DynamicElongation80kgTwoStrand());

            //Not all ropes specify sheath slippage. Annoying. Not a required field.
        }

        private static bool FiniteOrAbsent(double? value) => value == null || double.IsFinite(value.Value);

        public string MustBeSpecified(string param)
        {
            return $"{param} must be specified.";
        }
        public string NotZeroOrLess(string param)
        {
            return $"{param} cannot be zero or less!";
        }
        public string Not100OrMore(string param)
        {
            return $"{param} cannot be 100 or more!";
        }
    }
}
