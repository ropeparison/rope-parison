using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeParison.Protocol
{
    public class RopeFilterDto
    {
        public string? Name { get; set; }
        public string? BrandName { get; set; }
        public string? CategoryName { get; set; }

        public double? DiameterMin { get; set; }
        public double? DiameterMax { get; set; }
        public double? MassPerUnitLengthMin { get; set; }
        public double? MassPerUnitLengthMax { get; set; }
        public double? AreaMin { get; set; }
        public double? AreaMax { get; set; }
        public double? SheathPercentageMin { get; set; }
        public double? SheathPercentageMax { get; set; }
        public double? SheathAreaMin { get; set; }
        public double? SheathAreaMax { get; set; }
        public double? SheathThicknessMin { get; set; }
        public double? SheathThicknessMax { get; set; }
        public double? CoreAreaMin { get; set; }
        public double? CoreAreaMax { get; set; }
        public double? CoreDiameterMin { get; set; }
        public double? CoreDiameterMax { get; set; }
        public double? ImpactForce55kgOneStrandMin { get; set; }
        public double? ImpactForce55kgOneStrandMax { get; set; }
        public double? ImpactForce80kgOneStrandMin { get; set; }
        public double? ImpactForce80kgOneStrandMax { get; set; }
        public double? ImpactForce80kgTwoStrandMin { get; set; }
        public double? ImpactForce80kgTwoStrandMax { get; set; }
        public double? StaticElongation80kgOneStrandMin { get; set; }
        public double? StaticElongation80kgOneStrandMax { get; set; }
        public double? StaticElongation80kgTwoStrandMin { get; set; }
        public double? StaticElongation80kgTwoStrandMax { get; set; }
        public double? DynamicElongation55kgOneStrandMin { get; set; }
        public double? DynamicElongation55kgOneStrandMax { get; set; }
        public double? DynamicElongation80kgOneStrandMin { get; set; }
        public double? DynamicElongation80kgOneStrandMax { get; set; }
        public double? DynamicElongation80kgTwoStrandMin { get; set; }
        public double? DynamicElongation80kgTwoStrandMax { get; set; }
        public int? DropsBeforeBreak55kgOneStrandMin { get; set; }
        public int? DropsBeforeBreak55kgOneStrandMax { get; set; }
        public int? DropsBeforeBreak80kgOneStrandMin { get; set; }
        public int? DropsBeforeBreak80kgOneStrandMax { get; set; }
        public int? DropsBeforeBreak80kgTwoStrandMin { get; set; }
        public int? DropsBeforeBreak80kgTwoStrandMax { get; set; }
        public double? SheathSlippageMin { get; set; }
        public double? SheathSlippageMax { get; set; }
    }
}
