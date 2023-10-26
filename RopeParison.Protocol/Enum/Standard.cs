using RopeParison.Helpers;

namespace RopeParison.Protocol
{
    public enum Standard
    {
        [Standard(DisplayName = "Dynamic", UiaaStandardCode = "101", UiaaStandardName = "Dynamic Ropes", EnStandardCode = "892:2012", EnStandardName = "Mountaineering equipment - Dynamic mountaineering ropes - Safety requirements and test methods")] Dynamic,
        [Standard(DisplayName = "Static", UiaaStandardCode = "107", UiaaStandardName = "Low Stretch Ropes", EnStandardCode = "1891:1998", EnStandardName = "Personal protective equipment for the prevention of falls from a height. Low stretch kernmantel ropes")] Static,
        [Standard(DisplayName = "Cord", UiaaStandardCode = "102",UiaaStandardName = "Accessory Cord", EnStandardCode = "564:2014", EnStandardName = "Mountaineering equipment - Accessory cords - Safety requirements and test methods")] Cord,
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class StandardAttribute : System.Attribute
    {
        public required string DisplayName { get; set; }
        public required string UiaaStandardCode { get; set; }
        public required string UiaaStandardName { get; set; }
        public required string EnStandardCode { get; set; }
        public required string EnStandardName { get; set; }
    }

    public static class StandardExtension
    {
        public static Protocol.StandardDto ToDto(this Standard standard)
        {
            var dto = new Protocol.StandardDto();

            dto.StandardId = (int)standard;
            dto.Name = standard.ToString();

            var attribute = standard.GetAttributeOfType<StandardAttribute>();

            dto.DisplayName = attribute.DisplayName;
            dto.UiaaStandardCode = attribute.UiaaStandardCode;
            dto.UiaaStandardName = attribute.UiaaStandardName;
            dto.EnStandardCode = attribute.EnStandardCode;
            dto.EnStandardName = attribute.EnStandardName;

            return dto;
        }
    }



}
