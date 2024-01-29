namespace RopeParison.Protocol
{
    public class RopeCalculatedParameterSetDto
    {
        public int RopeCalculatedParameterSetId { get; set; }
        
        public double? Area { get; set; }
        public double? SheathArea { get; set; }
        public double? SheathThickness { get; set; }
        public double? CoreArea { get; set; }
        public double? CoreDiameter { get; set; }
        public double? Density { get; set; }

        public int RopeID { get; set; }

    }
}
