namespace Jackett.Common.Models.IndexerConfig.Bespoke
{
    internal class ConfigurationDataAniDub : ConfigurationDataBasicLogin
    {
        public BoolItem StripRussianTitle { get; private set; }

        public ConfigurationDataAniDub() => StripRussianTitle = new BoolItem { Name = "Strip Russian Title", Value = true };
    }
}
