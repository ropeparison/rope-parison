namespace RopeParison.Protocol
{
    public class RopePropertyInformationDto
    {
        public string LogicalName { get; set; }

        public string Name { get; set; }

        public string Unit { get; set; }

        public string NameUnit { get; set; }

        public RopePropertyInformationType Type { get; set; }

        public RopePropertyAxisType AxisType { get; set; }



        public bool Visible { get; set; }

        public RopePropertyInformationDto(string logicalName, string name, string unit, RopePropertyInformationType type, RopePropertyAxisType axisType, bool visible)
        { 
            LogicalName = logicalName;
            Name = name;
            Unit = unit;
            NameUnit = name + ((unit != "") ? $" ({unit})" : "");
            Type = type;
            AxisType = axisType;
            Visible = visible;
        }

        public RopePropertyInformationDto() { }

        public int SetVisible(bool display)
        {
            if (Visible != display)
            {
                Visible = display;

                if (Visible)
                    return 1;
                else
                    return -1;
            }
            else
            {
                return 0;
            }
        }

    }

    public enum RopePropertyInformationType
    {
        Int,
        Double,
        String

    }

    public enum RopePropertyAxisType
    {
        Other,
        RopeName,
        BrandName,
        CategoryName,
        NumberLessThan50,
        KiloNewton,
        Percentage,
        GramPerMetre,
        Millimetre,
        MillimetreSquared,
        GramPerMillimetreCubed,
    }
}
