namespace RopeParison.Protocol
{
    public class RopeCalculatedParameterSetPropertyInformationsDto
    {
        public RopePropertyInformationDto Area { get; set; } = new RopePropertyInformationDto("RopeCalculatedParameterSet.Area", "Area", "mm²", RopePropertyInformationType.Double, RopePropertyAxisType.MillimetreSquared, false);
        public RopePropertyInformationDto SheathArea { get; set; } = new RopePropertyInformationDto("RopeCalculatedParameterSet.SheathArea", "Sheath Area", "mm\u00b2", RopePropertyInformationType.Double, RopePropertyAxisType.MillimetreSquared, false); //mm^2
        public RopePropertyInformationDto SheathThickness { get; set; } = new RopePropertyInformationDto("RopeCalculatedParameterSet.SheathThickness", "Sheath Thickness", "mm", RopePropertyInformationType.Double, RopePropertyAxisType.Millimetre, false);
        public RopePropertyInformationDto CoreArea { get; set; } = new RopePropertyInformationDto("RopeCalculatedParameterSet.CoreArea", "Core Area", "mm²", RopePropertyInformationType.Double, RopePropertyAxisType.MillimetreSquared, false);
        public RopePropertyInformationDto CoreDiameter { get; set; } = new RopePropertyInformationDto("RopeCalculatedParameterSet.CoreDiameter", "Core Diameter", "mm", RopePropertyInformationType.Double, RopePropertyAxisType.Millimetre, false);
        public RopePropertyInformationDto Density { get; set; } = new RopePropertyInformationDto("RopeCalculatedParameterSet.Density", "Density", "g/mm\u00b3", RopePropertyInformationType.Double, RopePropertyAxisType.GramPerMillimetreCubed, false); //g/mm^3

    }
}
