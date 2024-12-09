using System;

namespace Course.Classes
{
    [Serializable]
    public class SettingsA
    {
        public bool Theme { get; set; }
        public bool View { get; set; }
        public SettingsA()
        {
            Theme = false;
            View = false;
        }
    }
}
