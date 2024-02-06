namespace RopeParison.Protocol
{
    //This should really be a dictionary with key of RopeProperty Enum. Deal with VisibleColCount separately.
    public class RopePropertyInformationsDto
    {
        public int VisibleColCount { get; set; } = 0;
        public RopePropertyInformationDto RopeId { get; set; } = new RopePropertyInformationDto("RopeId", "Rope Id", "", RopePropertyInformationType.Int, RopePropertyAxisType.Other, false);
        public RopePropertyInformationDto Name { get; set; } = new RopePropertyInformationDto("Name", "Name", "", RopePropertyInformationType.String, RopePropertyAxisType.RopeName, true);
        public RopePropertyInformationDto Brand { get; set; } = new RopePropertyInformationDto("Brand.Name", "Brand", "", RopePropertyInformationType.String, RopePropertyAxisType.BrandName, true);
        public RopePropertyInformationDto Category { get; set; } = new RopePropertyInformationDto("Category.Name", "Category", "", RopePropertyInformationType.String, RopePropertyAxisType.CategoryName, true);

        public RopePropertyInformationDto Diameter { get; set; } = new RopePropertyInformationDto("Diameter", "Diameter", "mm", RopePropertyInformationType.Double, RopePropertyAxisType.Millimetre, true);
        public RopePropertyInformationDto MassPerUnitLength { get; set; } = new RopePropertyInformationDto("MassPerUnitLength", "Weight", "g/m", RopePropertyInformationType.Double, RopePropertyAxisType.GramPerMetre, true);
        public RopePropertyInformationDto SheathPercentage { get; set; } = new RopePropertyInformationDto("SheathPercentage", "Sheath Percentage", "%", RopePropertyInformationType.Double, RopePropertyAxisType.Percentage, true);

        public RopePropertyInformationDto ImpactForce55kgOneStrand { get; set; } = new RopePropertyInformationDto("ImpactForce55kgOneStrand", "Impact Force [55kg, 1 Strand]", "kN", RopePropertyInformationType.Double, RopePropertyAxisType.KiloNewton, true);
        public RopePropertyInformationDto ImpactForce80kgOneStrand { get; set; } = new RopePropertyInformationDto("ImpactForce80kgOneStrand", "Impact Force [80kg, 1 Strand]", "kN", RopePropertyInformationType.Double, RopePropertyAxisType.KiloNewton, false);
        public RopePropertyInformationDto ImpactForce80kgTwoStrand { get; set; } = new RopePropertyInformationDto("ImpactForce80kgTwoStrand", "Impact Force [80kg, 2 Strand]", "kN", RopePropertyInformationType.Double, RopePropertyAxisType.KiloNewton, false);

        public RopePropertyInformationDto StaticElongation80kgOneStrand { get; set; } = new RopePropertyInformationDto("StaticElongation80kgOneStrand", "Static Elongation [80kg, 1 Strand]", "%", RopePropertyInformationType.Double, RopePropertyAxisType.Percentage, true);
        public RopePropertyInformationDto StaticElongation80kgTwoStrand { get; set; } = new RopePropertyInformationDto("StaticElongation80kgTwoStrand", "Static Elongation [80kg, 2 Strand]", "%", RopePropertyInformationType.Double, RopePropertyAxisType.Percentage, false);

        public RopePropertyInformationDto DynamicElongation55kgOneStrand { get; set; } = new RopePropertyInformationDto("DynamicElongation55kgOneStrand", "Dynamic Elongation [55kg, 1 Strand]", "%", RopePropertyInformationType.Double, RopePropertyAxisType.Percentage, true);
        public RopePropertyInformationDto DynamicElongation80kgOneStrand { get; set; } = new RopePropertyInformationDto("DynamicElongation80kgOneStrand", "Dynamic Elongation [80kg, 1 Strand]", "%", RopePropertyInformationType.Double, RopePropertyAxisType.Percentage, false);
        public RopePropertyInformationDto DynamicElongation80kgTwoStrand { get; set; } = new RopePropertyInformationDto("DynamicElongation80kgTwoStrand", "Dynamic Elongation [80kg, 2 Strand]", "%", RopePropertyInformationType.Double, RopePropertyAxisType.Percentage, false);

        public RopePropertyInformationDto DropsBeforeBreak55kgOneStrand { get; set; } = new RopePropertyInformationDto("DropsBeforeBreak55kgOneStrand", "Drops Before Break [55kg, 1 Strand]", "", RopePropertyInformationType.Int, RopePropertyAxisType.NumberLessThan50, true);
        public RopePropertyInformationDto DropsBeforeBreak80kgOneStrand { get; set; } = new RopePropertyInformationDto("DropsBeforeBreak80kgOneStrand", "Drops Before Break [80kg, 1 Strand]", "", RopePropertyInformationType.Int, RopePropertyAxisType.NumberLessThan50, false);
        public RopePropertyInformationDto DropsBeforeBreak80kgTwoStrand { get; set; } = new RopePropertyInformationDto("DropsBeforeBreak80kgTwoStrand", "Drops Before Break [80kg, 2 Strand]", "%", RopePropertyInformationType.Int, RopePropertyAxisType.NumberLessThan50, false);

        public RopePropertyInformationDto SheathSlippage { get; set; } = new RopePropertyInformationDto("SheathSlippage", "Sheath Slippage", "mm", RopePropertyInformationType.Double, RopePropertyAxisType.Millimetre, false);

        //public RopeCalculatedParameterSetPropertyInformationsDto RopeCalculatedParameterSetPropertyInformations { get; set; } = new RopeCalculatedParameterSetPropertyInformationsDto();
        public RopeCalculatedParameterSetPropertyInformationsDto RoCaPaSePrIn { get; set; } = new RopeCalculatedParameterSetPropertyInformationsDto();

    }
}
