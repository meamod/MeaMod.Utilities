using System;
using System.ComponentModel;
using System.Reflection;

namespace MeaMod.Utilities
{
    /// <summary>
    /// Description attribute extension for Enum
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Return the enum description attribute
        /// </summary>
        /// <param name="value">Enum value</param>
        /// <returns>The <see cref="DescriptionAttribute"/> text, or null when the value has none or is not a named member.</returns>
        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null && Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attr)
                {
                    return attr.Description;
                }
            }
            return null;
        }
    }
}
