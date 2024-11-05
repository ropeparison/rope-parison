using RopeParison.Protocol;
using System.Text.Json;

namespace RopeParison.GraphHelpers
{
    public class PointData
    {
        public string? Name { get; set; }
        public string? Diameter { get; set; }
        public string? BrandName { get; set; }
        public string? Category { get; set; }

        public string? XAxisName { get; set; }
        public string? XAxisValue { get; set; }
        public string? XAxisUnit { get; set; }

        public string? YAxisName { get; set; }
        public string? YAxisValue { get; set; }
        public string? YAxisUnit { get; set; }

        public bool IsBubble { get; set; }
        public string? BubbleSizeName { get; set; }
        public string? BubbleSizeValue { get; set; }
        public string? BubbleSizeUnit { get; set; }

        public PointData() { }

        public PointData(bool isBubble, RopeDto rope, RopePropertyInformationDto xRoPrIn, RopePropertyInformationDto yRoPrIn, RopePropertyInformationDto bubbleRoPrIn)
        {
            Name = rope.Name;
            Diameter = rope.Diameter.ToString("N1");
            BrandName = rope.Brand.Name;

            XAxisName = xRoPrIn.Name;
            XAxisValue = rope.XString;
            XAxisUnit = xRoPrIn.Unit;

            YAxisName = yRoPrIn.Name;
            YAxisValue = rope.YString;
            YAxisUnit = yRoPrIn.Unit;

            IsBubble = isBubble;

            if (isBubble)
            {
                BubbleSizeName = bubbleRoPrIn.Name;
                BubbleSizeValue = rope.BubbleSizeString;
                BubbleSizeUnit = bubbleRoPrIn.Unit;
            }
        }

        public static PointData GetPointData(string pointDataString)
        {
            var pointDataError = new PointData();
            pointDataError.Name = "Error";

            if (pointDataString is null || pointDataString == string.Empty)
            {
                return pointDataError;
            }

            var pointData = JsonSerializer.Deserialize<PointData>(pointDataString);
            
            if (pointData is null)
            {
                return pointDataError;
            }

            return pointData;
        }

        public string GetPointDataJson()
        {
            var pointDataJson = JsonSerializer.Serialize(this);
            return pointDataJson;
        }

    }
}
