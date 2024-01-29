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
            RuleFor(p => p.Name).Must(x => true).WithMessage("Name must be unique.")
                .When(p => _ropeService.IsRopeNameUnique(p.RopeId, p.Name)); //Better way of doing this? Not sure how to call the method from inside the must, can't provide p.RopeId.

            RuleFor(p => p.Brand).NotEmpty().WithMessage(MustBeSpecified("Brand"));

            RuleFor(p => p.Category).NotEmpty().WithMessage(MustBeSpecified("Category"));

            RuleFor(p => p.Diameter).NotEmpty().WithMessage(MustBeSpecified("Diameter"));
            RuleFor(p => p.Diameter).GreaterThan(0).WithMessage(NotZeroOrLess("Diameter"));

            RuleFor(p => p.MassPerUnitLength).NotEmpty().WithMessage(MustBeSpecified("Mass/m"));
            RuleFor(p => p.MassPerUnitLength).GreaterThan(0).WithMessage(NotZeroOrLess("Mass/m"));

            //RuleFor(p => p.SheathPercentage).NotEmpty().WithMessage(MustBeSpecified("Sheath %"));
            //RuleFor(p => p.SheathPercentage).GreaterThan(0).WithMessage(NotZeroOrLess("Sheath %"));
            //RuleFor(p => p.SheathPercentage).LessThan(100).WithMessage(Not100OrMore("Sheath %"));
            //Not all ropes specify sheath percentage. Annoying.

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
            _ropeService = ropeService;
        }

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
