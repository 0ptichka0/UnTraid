namespace UnTraid.Properties
{


    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.0.3.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase
    {

        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));

        public static Settings Default
        {
            get
            {
                return defaultInstance;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("12")]
        public int MacdFastPeriod
        {
            get
            {
                return ((int)(this["MacdFastPeriod"]));
            }
            set
            {
                this["MacdFastPeriod"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("26")]
        public int MacdSlowPeriod
        {
            get
            {
                return ((int)(this["MacdSlowPeriod"]));
            }
            set
            {
                this["MacdSlowPeriod"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("9")]
        public int MacdSignalPeriod
        {
            get
            {
                return ((int)(this["MacdSignalPeriod"]));
            }
            set
            {
                this["MacdSignalPeriod"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("14")]
        public int RsiPeriod
        {
            get
            {
                return ((int)(this["RsiPeriod"]));
            }
            set
            {
                this["RsiPeriod"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("70")]
        public double RsiOverboughtLevel
        {
            get
            {
                return ((double)(this["RsiOverboughtLevel"]));
            }
            set
            {
                this["RsiOverboughtLevel"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("30")]
        public double RsiOversoldLevel
        {
            get
            {
                return ((double)(this["RsiOversoldLevel"]));
            }
            set
            {
                this["RsiOversoldLevel"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("14")]
        public int StochasticKPeriod
        {
            get
            {
                return ((int)(this["StochasticKPeriod"]));
            }
            set
            {
                this["StochasticKPeriod"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("3")]
        public int StochasticDPeriod
        {
            get
            {
                return ((int)(this["StochasticDPeriod"]));
            }
            set
            {
                this["StochasticDPeriod"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("3")]
        public int StochasticSmoothK
        {
            get
            {
                return ((int)(this["StochasticSmoothK"]));
            }
            set
            {
                this["StochasticSmoothK"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("14")]
        public int TrixPeriod
        {
            get
            {
                return ((int)(this["TrixPeriod"]));
            }
            set
            {
                this["TrixPeriod"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("14")]
        public int AdxPeriod
        {
            get
            {
                return ((int)(this["AdxPeriod"]));
            }
            set
            {
                this["AdxPeriod"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("25")]
        public double AdxStrongTrend
        {
            get
            {
                return ((double)(this["AdxStrongTrend"]));
            }
            set
            {
                this["AdxStrongTrend"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("50")]
        public double AdxVeryStrongTrend
        {
            get
            {
                return ((double)(this["AdxVeryStrongTrend"]));
            }
            set
            {
                this["AdxVeryStrongTrend"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool BackgroundMode
        {
            get
            {
                return ((bool)(this["BackgroundMode"]));
            }
            set
            {
                this["BackgroundMode"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string UserName
        {
            get
            {
                return ((string)(this["UserName"]));
            }
            set
            {
                this["UserName"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string TelegramId
        {
            get
            {
                return ((string)(this["TelegramId"]));
            }
            set
            {
                this["TelegramId"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string TinkoffApiToken
        {
            get
            {
                return ((string)(this["TinkoffApiToken"]));
            }
            set
            {
                this["TinkoffApiToken"] = value;
            }
        }
    }
}