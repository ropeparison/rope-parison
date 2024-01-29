using RopeParison.Protocol;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace RopeParison.Data.Model
{
    public class RopeCalculatedParameterSet
    {
        public int RopeCalculatedParameterSetId { get; set; }

        public double? Area { get; set; }
        public double? SheathArea { get; set; }
        public double? SheathThickness { get; set; }
        public double? CoreArea { get; set; }
        public double? CoreDiameter { get; set; }
        public double? Density { get; set; }

        public int RopeId { get; set; }
        public virtual Rope Rope { get; set; }
        
    }
}
