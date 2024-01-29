using RopeParison.Protocol;

namespace RopeParison.GraphHelpers
{
    public class SeriesData
    {
        public List<RopeDto> DataSource { get; set; }

        public string Name { get; set; }
        public string XName { get; set; }
        public string YName { get; set; }
        public string Size { get; set; }
        public string XAxisName { get; set; }
        public string YAxisName { get; set; }
    }
}
