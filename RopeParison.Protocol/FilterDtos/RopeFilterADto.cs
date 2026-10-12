using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeParison.Protocol
{
    public class RopeFilterADto
    {
        // Null means no restriction; an explicitly empty selection matches nothing.
        public List<int>? RopeIds { get; set; }
        public List<int> BrandIds { get; set; } = new List<int>();
        public List<Category>? Categorys { get; set; }

        public double? DiameterMin { get; set; }
        public double? DiameterMax { get; set; }
        public double? MassPerUnitLengthMin { get; set; }
        public double? MassPerUnitLengthMax { get; set; }
        public double? SheathPercentageMin { get; set; }
        public double? SheathPercentageMax { get; set; }

        public bool IncludeUnverified { get; set; }

    }
}
