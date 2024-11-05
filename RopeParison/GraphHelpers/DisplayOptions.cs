namespace RopeParison.GraphHelpers
{
    public class DisplayOptions
    {
        public bool ChartDataLabelVisible { get; set; } = true;

        public DisplayOptions() { }

        public DisplayOptions(bool chartDataLabelVisible)
        {
            ChartDataLabelVisible = chartDataLabelVisible;
        }
    }

}
