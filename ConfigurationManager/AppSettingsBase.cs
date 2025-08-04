using System;

namespace ConfigurationManager
{
    /// <summary>
    /// Base class for application settings classes
    /// </summary>
    public abstract class AppSettingsBase
    {
        /// <summary>
        /// Gets the section name in the configuration file where these settings are stored
        /// </summary>
        public abstract string SectionName { get; }
        
        /// <summary>
        /// Returns a string representation of the settings
        /// </summary>
        public abstract override string ToString();
    }
}
