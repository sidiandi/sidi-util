using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Sidi.CommandLine
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
    public class Usage : System.Attribute
    {
        private string m_description = "";

        public string Description { get { return m_description; } }

        public Usage(string description)
        {
            m_description = description;
        }

        public static string Get(ICustomAttributeProvider cap)
        {
            Usage usage = (Usage)cap.GetCustomAttributes(typeof(Usage), true).FirstOrDefault();
            if (usage != null)
            {
                return usage.Description;
            }

            System.ComponentModel.DescriptionAttribute description = (System.ComponentModel.DescriptionAttribute)
                cap.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), true).FirstOrDefault();
            if (description != null)
            {
                return description.Description;
            }

            return null;
        }
    }

    /// <summary>
    /// Properties marked with this attribute will be stored in the registry
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PersistentAttribute : System.Attribute
    {
    }

    /// <summary>
    /// Properties marked with this attribute will not be displayed to the user
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PasswordAttribute : System.Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SubCommandAttribute : System.Attribute
    {
        public SubCommandAttribute()
        {
        }
    }

}
