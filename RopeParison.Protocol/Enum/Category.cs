using RopeParison.Helpers;

namespace RopeParison.Protocol
{
    public enum Category
    {
        [Category(DisplayName = "Single", Standard = Standard.Dynamic)] Single,
        [Category(DisplayName = "Half", Standard = Standard.Dynamic)] Half,
        [Category(DisplayName = "Twin", Standard = Standard.Dynamic)] Twin,
        [Category(DisplayName = "Half and Twin", Standard = Standard.Dynamic)] HalfTwin,
        [Category(DisplayName = "Triple", Standard = Standard.Dynamic)] Triple,
        //[Category(Name = "Static TypeA", Standard = Standard.Static)] StaticA,
        //[Category(Name = "Static TypeB", Standard = Standard.Static)] StaticB,
        //[Category(Name = "Cord", Standard = Standard.Cord)] Cord,

    }

    [AttributeUsage(AttributeTargets.Field)]
    public class CategoryAttribute : System.Attribute
    {
        public required string DisplayName { get; set; }

        public Standard Standard { get; set; }
    }


    [AttributeUsage(AttributeTargets.Field)]
    public class DisplayAttribute : System.Attribute
    {
        public required string DisplayName { get; set; }

        public string? Description { get; set; }
    }

    public static class CategoryExtension
    {
        public static Protocol.CategoryDto ToDto(this Category category)
        {
            var dto = new Protocol.CategoryDto();

            //dto.CategoryId = (int)category;
            //dto.Name = category.ToString();
            dto.Category = category;

            var attribute = category.GetAttributeOfType<CategoryAttribute>();

            dto.DisplayName = attribute.DisplayName;
            dto.Standard = attribute.Standard.ToDto();

            return dto;
        }
        public static Protocol.CategoryDto? ToDto(this Category? category)
        {
            if (category.HasValue)
            {
                return category.Value.ToDto();
            }
            else
            {
                return null;
            }
        }

        public static Category ToModel(this Protocol.CategoryDto dto)
        {
            if (Enum.IsDefined(typeof(Category), dto.CategoryId))
            {
                var model = (Category)dto.CategoryId;
                return model;
            }
            else
            {
                throw new InvalidOperationException("");
            }
        }

    }

}
