using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeParison.Protocol
{
    public class ColumnVisibilityFilterDto
    {
        public bool ShowId { get; set; }  = true;
        public bool ShowCategory { get; set; }  = true;
        public bool ShowBasic { get; set; }  = true;
        public bool ShowImpactForce { get; set; }  = true;
        public bool ShowStaticElongation { get; set; }  = true;
        public bool ShowDynamicElongation { get; set; }  = true;
        public bool ShowDropsBeforeBreak { get; set; }  = true;
        public bool ShowExtraPhysicalProperties { get; set; }  = false;
        public bool SelectedSingle { get; set; }  = true;
        public bool SelectedHalf { get; set; }  = false;
        public bool SelectedTwin { get; set; }  = false;

    }
}
