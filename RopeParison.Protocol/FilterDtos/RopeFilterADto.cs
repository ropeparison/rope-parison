using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeParison.Protocol
{
    public class RopeFilterADto
    {
        public List<int> RopeIds { get; set; } = new List<int>();
        public List<int> BrandIds { get; set; } = new List<int>();
        public List<Category> Categorys { get; set; } = new List<Category>();

        public double? DiameterMin { get; set; }
        public double? DiameterMax { get; set; }
        public double? MassPerUnitLengthMin { get; set; }
        public double? MassPerUnitLengthMax { get; set; }
        public double? SheathPercentageMin { get; set; }
        public double? SheathPercentageMax { get; set; }

        public bool IncludeUnverified { get; set; }

    }
}
