using RopeParison.Protocol;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace RopeParison.Data.Model
{
    public class Rope
    {
        public int RopeId { get; set; }
        public string? Name { get; set; }
        public int BrandId { get; set; }
        public virtual Brand Brand { get; set; }
        public Category Category { get; set; }

        public double Diameter { get; set; }
        public double MassPerUnitLength { get; set; }
        public double? SheathPercentage { get; set; } //Nullable. Often not specified.

        //Nullable. Not all ropes have values for all ImpactForce, StaticElong, DynamicElong, DropsBefore options.
        public double? ImpactForce55kgOneStrand { get; set; }
        public double? ImpactForce80kgOneStrand { get; set; }
        public double? ImpactForce80kgTwoStrand { get; set; }

        public double? StaticElongation80kgOneStrand { get; set; }
        public double? StaticElongation80kgTwoStrand { get; set; }

        public double? DynamicElongation55kgOneStrand { get; set; }
        public double? DynamicElongation80kgOneStrand { get; set; }
        public double? DynamicElongation80kgTwoStrand { get; set; }

        public int? DropsBeforeBreak55kgOneStrand { get; set; }
        public int? DropsBeforeBreak80kgOneStrand { get; set; }
        public int? DropsBeforeBreak80kgTwoStrand { get; set; }

        public double? SheathSlippage { get; set; } //Nullable. Often not specified.

        public virtual RopeCalculatedParameterSet RopeCalculatedParameterSet { get; set; }

        public bool Verified { get; set; }

        public virtual ICollection<RopeEditSuggestion> RopeEditSuggestions { get; set; } = new List<RopeEditSuggestion>();

    }
}
