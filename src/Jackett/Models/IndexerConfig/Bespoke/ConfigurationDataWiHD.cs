﻿namespace Jackett.Models.IndexerConfig.Bespoke
{
    class ConfigurationDataWiHD : ConfigurationData
    {
        public DisplayItem CredentialsWarning { get; private set; }
        public StringItem Username { get; private set; }
        public StringItem Password { get; private set; }
        public DisplayItem PagesWarning { get; private set; }
        public StringItem Pages { get; private set; }
        public BoolItem Exclusive { get; private set; }
        public BoolItem Freeleech { get; private set; }
        public BoolItem Reseed { get; private set; }
        public DisplayItem SecurityWarning { get; private set; }
        public BoolItem Latency { get; private set; }
        public BoolItem Browser { get; private set; }
        public DisplayItem LatencyWarning { get; private set; }
        public StringItem LatencyStart { get; private set; }
        public StringItem LatencyEnd { get; private set; }
        public DisplayItem HeadersWarning { get; private set; }
        public StringItem HeaderAccept { get; private set; }
        public StringItem HeaderAcceptLang { get; private set; }
        public BoolItem HeaderDNT { get; private set; }
        public BoolItem HeaderUpgradeInsecure { get; private set; }
        public StringItem HeaderUserAgent { get; private set; }
        public DisplayItem DevWarning { get; private set; }
        public BoolItem DevMode { get; private set; }

        public ConfigurationDataWiHD()
            : base()
        {
            CredentialsWarning = new DisplayItem("<b>Credentials Configuration</b> (<i>Private Tracker</i>),<br /><br /> <ul><li><b>Username</b> is your account name on this tracker.</li><li><b>Password</b> is your password associated to our account name.</li></ul>") { Name = "Credentials" };
            Username = new StringItem { Name = "Username (Required)", Value = "" };
            Password = new StringItem { Name = "Password (Required)", Value = "" };
            PagesWarning = new DisplayItem("<b>Preferences Configuration</b> (<i>Tweak your search settings</i>),<br /><br /> <ul><li><b>Max Pages to Process</b> let you specify how many page (max) Jackett can process when doing a search. Setting a value <b>higher than 4 is dangerous</b> for you account ! (<b>Result of too many requests to tracker...that <u>will be suspect</u></b>).</li><li><b>Exclusive Only</b> let you search <u>only</u> for torrents which are marked Exclusive.</li><li><b>Freeleech Only</b> let you search <u>only</u> for torrents which are marked Freeleech.</li><li><b>Reseed Only</b> let you search <u>only</u> for torrents which need to be seeded.</li></ul>") { Name  = "Preferences" };
            Pages = new StringItem { Name = "Max Pages to Process (Required)", Value = "4" };
            Exclusive = new BoolItem() { Name = "Exclusive Only (Optional)", Value = false };
            Freeleech = new BoolItem() { Name = "Freeleech Only (Optional)", Value = false };
            Reseed = new BoolItem() { Name = "Reseed Needed Only (Optional)", Value = false };
            SecurityWarning = new DisplayItem("<b>Security Configuration</b> (<i>Read this area carefully !</i>),<br /><br /> <ul><li><b>Latency Simulation</b> will simulate human browsing with Jacket by pausing Jacket for an random time between each request, to fake a real content browsing.</li><li><b>Browser Simulation</b> will simulate a real human browser by injecting additionals headers when doing requests to tracker.</li></ul>") { Name = "Security" };
            Latency = new BoolItem() { Name = "Latency Simulation (Optional)", Value = true };
            Browser = new BoolItem() { Name = "Browser Simulation (Optional)", Value = true };
            LatencyWarning = new DisplayItem("<b>Latency Configuration</b> (<i>Required if latency simulation enabled</i>),<br /><br/> <ul><li>By filling this range, <b>Jackett will make a random timed pause</b> <u>between requests</u> to tracker <u>to simulate a real browser</u>.</li><li>MilliSeconds <b>only</b></li></ul>") { Name = "Simulate Latency" };
            LatencyStart = new StringItem { Name = "Minimum Latency (ms)", Value = "1589" };
            LatencyEnd = new StringItem { Name = "Maximum Latency (ms)", Value = "3674" };
            HeadersWarning = new DisplayItem("<b>Browser Headers Configuration</b> (<i>Required if browser simulation enabled</i>),<br /><br /> <ul><li>By filling these fields, <b>Jackett will inject headers</b> with your values <u>to simulate a real browser</u>.</li><li>You can get <b>your browser values</b> here: <a href='https://www.whatismybrowser.com/detect/what-http-headers-is-my-browser-sending' target='blank'>www.whatismybrowser.com</a></li></ul><br /><i><b>Note that</b> some headers are not necessary because they are injected automatically by this provider such as Accept_Encoding, Connection, Host or X-Requested-With</i>") { Name = "Injecting headers" };
            HeaderAccept = new StringItem { Name = "Accept", Value = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8" };
            HeaderAcceptLang = new StringItem { Name = "Accept-Language", Value = "fr-FR,fr;q=0.8,en-US;q=0.6,en;q=0.4,es;q=0.2" };
            HeaderDNT = new BoolItem { Name = "DNT", Value = true };
            HeaderUpgradeInsecure = new BoolItem { Name = "Upgrade-Insecure-Requests", Value = true };
            HeaderUserAgent = new StringItem { Name = "User-Agent", Value = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36" };
            DevWarning = new DisplayItem("<b>Devlopement Facility</b> (<i>For Developers ONLY</i>),<br /><br /> <ul><li>By enabling devlopement mode, <b>Jackett will bypass his cache</b> and will <u>output debug messages to console</u> instead of his log file.</li></ul>") { Name = "Devlopement" };
            DevMode = new BoolItem { Name = "Enable DEV", Value = false };
        }
    }
}
