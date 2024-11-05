using RopeParison.Protocol;

namespace RopeParison.GraphHelpers
{
    public static class SeriesColors
    {
        public static string GetSeriesColor(Category category)
        {
            switch (category)
            {
                case Category.Single: return "Red";
                case Category.Half: return "Purple";
                case Category.Twin: return "Magenta";
                case Category.HalfTwin: return "Blue";
                case Category.Triple: return "Green";
                default: return "Black";
            }
        }
    }
}
