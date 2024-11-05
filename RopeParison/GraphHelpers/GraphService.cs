using RopeParison.Data.Model;
using RopeParison.Pages;
using RopeParison.Protocol;
using System;
using System.Xml.Linq;

namespace RopeParison.GraphHelpers
{
    public interface IGraphService
    {
        Syncfusion.Blazor.Charts.ValueType GetAxisValueType(RopePropertyInformationType ropePropertyInformationType);

        string RopeInformationText(bool isBubble, RopeDto rope, RopePropertyInformationDto xRoPrIn, RopePropertyInformationDto yRoPrIn, RopePropertyInformationDto bubbleRoPrIn);

        void SetXYBubblePlotValues(bool isBubble, RopeDto rope, bool xDouble, bool xInt, bool yDouble, bool yInt, bool bubbleSizeDouble, bool bubbleSizeInt, RopePropertyInformationDto xRoPrIn, RopePropertyInformationDto yRoPrIn, RopePropertyInformationDto bubbleRoPrIn);
        void SetXYPlotValues(RopeDto rope, bool xDouble, bool xInt, bool yDouble, bool yInt, RopePropertyInformationDto xRoPrIn, RopePropertyInformationDto yRoPrIn);
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

        public string RopeInformationText(bool isBubble, RopeDto rope, RopePropertyInformationDto xRoPrIn, RopePropertyInformationDto yRoPrIn, RopePropertyInformationDto bubbleRoPrIn)
        {
            PointData pointData = new PointData(isBubble, rope, xRoPrIn, yRoPrIn, bubbleRoPrIn);

            var ropeInformationText = pointData.GetPointDataJson();

            return ropeInformationText;
        }

        public void SetXYPlotValues(RopeDto rope, bool xDouble, bool xInt, bool yDouble, bool yInt, RopePropertyInformationDto xRoPrIn, RopePropertyInformationDto yRoPrIn)
        {
            SetXYBubblePlotValues(false, rope, xDouble, xInt, yDouble, yInt, false, false, xRoPrIn, yRoPrIn, new RopePropertyInformationDto());
        }

        public void SetXYBubblePlotValues(bool isBubble, RopeDto rope, bool xDouble, bool xInt, bool yDouble, bool yInt, bool bubbleSizeDouble, bool bubbleSizeInt, RopePropertyInformationDto xRoPrIn, RopePropertyInformationDto yRoPrIn, RopePropertyInformationDto bubbleRoPrIn)
        {
            //----------------------------------------
            //X-Y-BubbleSize Plot Values

            //For reasons known best to themselves syncfusion blazor only allow nested data binding on the y-axis (see line graph).
            //Also, they don't seem to allow category (basically discrete, relevantly for us string) data on the y-axis.
            //Have to do this annoying workaround. Raise bug/feature request with syncfusion.

            //Possible null values is ok. E.g. for unknown sheath percentage.
            //If the value is null it will just not be plotted, which is desired behaviour.

            //X
            if (xDouble)
            {
                rope.XDouble = rope[xRoPrIn.LogicalName] as double?;
                rope.XString = rope.XDouble?.ToString("#0.0"); //1 d.p. for all doubles is appropriate. Nothing in ropes is more precise.
            }
            else if (xInt)
            {
                rope.XDouble = rope[xRoPrIn.LogicalName] as int?; //Have to get int values as int? not as double?.
                rope.XString = rope.XDouble?.ToString("#0"); //No dp for int.
            }
            else
            {
                rope.XDouble = null;
                rope.XString = rope[xRoPrIn.LogicalName] as string;
            }

            //Y
            if (yDouble)
            {
                rope.YDouble = rope[yRoPrIn.LogicalName] as double?;
                rope.YString = rope.YDouble?.ToString("#0.0");
            }
            else if (yInt)
            {
                rope.YDouble = rope[yRoPrIn.LogicalName] as int?;
                rope.YString = rope.YDouble?.ToString("#0");
            }
            else
            {
                rope.YDouble = null;
                rope.YString = rope[yRoPrIn.LogicalName] as string;
            }

            //Bubble
            if (isBubble)
            {
                if (bubbleSizeDouble)
                {
                    rope.BubbleSizeDouble = rope[bubbleRoPrIn.LogicalName] as double?;
                    rope.BubbleSizeString = rope.BubbleSizeDouble?.ToString("#0.0");
                }
                else if (bubbleSizeInt)
                {
                    rope.BubbleSizeDouble = rope[bubbleRoPrIn.LogicalName] as int?;
                    rope.BubbleSizeString = rope.BubbleSizeDouble?.ToString("#0");
                }
                else
                {
                    rope.BubbleSizeDouble = null;
                    rope.BubbleSizeString = rope[bubbleRoPrIn.LogicalName] as string;
                }
            }
            //----------------------------------------
        }

    }
}
