
namespace RopeParison.Helpers
{
    public static class EnumHelper
    {
        // Note, an evivalent method exists in this package, but I don't need anything else from that package atm. https://learn.microsoft.com/en-us/dotnet/api/microsoft.openapi.extensions.enumextensions.getattributeoftype
        // Example. string desc = myEnumVariable.GetAttributeOfType<DisplayAttribute>().DisplayName;
        public static T GetAttributeOfType<T>(this Enum enumVal) where T : System.Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }

        public static IEnumerable<T>  GetEnumAsIEnumerable<T>() where T : System.Enum
        {
            var x = Enum.GetValues(typeof(T)).Cast<T>();
            return x;
        }



    }
}
