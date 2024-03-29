﻿using RopeParison.Protocol;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace RopeParison.Data.Model
{
    public class RopeEditSuggestion
    {
        public int RopeEditSuggestionId { get; set; }

        public int RopeId { get; set; }

        public RopeProperty RopeProperty { get; set; } //Indicates which rope property this EditSuggestion is about. Sometimes the EditSuggestion will be to make a value null, which is impossible to identify otherwise!

        public string? Name { get; set; }
        public int? BrandId { get; set; }
        public virtual Brand? Brand { get; set; }
        public Category? Category { get; set; }

        public double? Diameter { get; set; }
        public double? MassPerUnitLength { get; set; }
        public double? SheathPercentage { get; set; }
        
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

        public double? SheathSlippage { get; set; }

        public double UpVoteCount { get; set; }
        public double DownVoteCount { get; set; }

    }
}
