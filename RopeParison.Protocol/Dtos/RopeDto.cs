namespace RopeParison.Protocol
{
    public class RopeDto
    {
        public int RopeId { get; set; }
        public string? Name { get; set; }
        public virtual BrandDto Brand { get; set; }
        public CategoryDto Category { get; set; }

        public double Diameter { get; set; }
        public double MassPerUnitLength { get; set; }
        public double SheathPercentage { get; set; }

        public double? ImpactForce55kgOneStrand { get; set; }
        public double? ImpactForce80kgOneStrand { get; set; }
        public double? ImpactForce80kgTwoStrand { get; set; }
        public double? StaticElongation80kgOneStrand { get; set; }
        public double? StaticElongation80kgTwoStrand { get; set; }
        public double? DynamicElongation55kgOneStrand { get; set; }
        public double? DynamicElongation80kgOneStrand { get; set; }
        public double? DynamicElongation80kgTwoStrand { get; set; }
        public int? DropsBeforeBreak55kgOneStrand { get; set; }
        public int? DropsBeforeBreak80kgOneStrand { get; set; }
        public int? DropsBeforeBreak80kgTwoStrand { get; set; }

        public double SheathSlippage { get; set; }

        public RopeCalculatedParameterSetDto RopeCalculatedParameterSet { get; set; }

    }
}
