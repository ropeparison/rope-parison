using System.Reflection;

namespace RopeParison.Protocol
{
    public class RopeDto
    {
        public int RopeId { get; set; }
        public string? Name { get; set; }
        public virtual BrandDto Brand { get; set; }
        public CategoryDto Category { get; set; }

        public double Diameter { get; set; }
        public double MassPerUnitLength { get; set; }
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

        public RopeCalculatedParameterSetDto RopeCalculatedParameterSet { get; set; } = new RopeCalculatedParameterSetDto();

        public bool Verified { get; set; }

        public List<RopeEditSuggestionDto> RopeEditSuggestions { get; set; } = new List<RopeEditSuggestionDto>();
        //------------------------------------------------------------------------------------------

        public string? InformationText { get; set; }

        public string? XString { get; set; }
        public double? XDouble { get; set; }
        public string? YString { get; set; }
        public double? YDouble { get; set; }
        public string? BubbleSizeString { get; set; }
        public double? BubbleSizeDouble { get; set; }

        public object this[string propertyName]
        {
            get
            {
                var val = GetPropertyValue(this, propertyName);
                return val;
            }
            set
            {
                //Setter currently doesn't work for nested properties.
                Type ropeDtoType = typeof(RopeDto);
                PropertyInfo myPropInfo = ropeDtoType.GetProperty(propertyName);
                myPropInfo.SetValue(this, value, null);
            }
        }

        public object GetPropertyValue(object obj, string propertyName)
        {
            //Works for nested.
            var _propertyNames = propertyName.Split('.');

            for (var i = 0; i < _propertyNames.Length; i++)
            {
                if (obj != null)
                {
                    var _propertyInfo = obj.GetType().GetProperty(_propertyNames[i]);
                    if (_propertyInfo != null)
                        obj = _propertyInfo.GetValue(obj);
                    else
                        obj = null;
                }
            }

            return obj;
        }

    }
}
