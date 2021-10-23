namespace E1Translator.Core.Config
{
    public interface IAISConfiguration
    {
        public string AisBaseUrl { get; }
        public string AisEnvironment { get; }
        public string AisRole { get;}
        public string AisUsername { get; }
        public string AisPassword { get; }
    }
}
