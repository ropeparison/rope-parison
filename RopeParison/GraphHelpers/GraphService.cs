using RopeParison.Protocol;

namespace RopeParison.GraphHelpers
{
    public interface IGraphService
    {
        Syncfusion.Blazor.Charts.ValueType GetAxisValueType(RopePropertyInformationType ropePropertyInformationType);
    }

    public class GraphService : IGraphService
    {

        //-------------------------------------------------------------------------

        public Syncfusion.Blazor.Charts.ValueType GetAxisValueType(RopePropertyInformationType ropePropertyInformationType)
        {
            if (new Enum[] { RopePropertyInformationType.Double, RopePropertyInformationType.Int }.Contains(ropePropertyInformationType))
            {
                return Syncfusion.Blazor.Charts.ValueType.Double;
            }
            else if (new Enum[] { RopePropertyInformationType.String }.Contains(ropePropertyInformationType))
            {
                return Syncfusion.Blazor.Charts.ValueType.Category;
            }
            else
            {
                //Shouldn't get here. Use double as default
                return Syncfusion.Blazor.Charts.ValueType.Double;
            }
        }

    }
}
