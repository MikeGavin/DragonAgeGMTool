﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Minion.depreciated.Settings {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    internal sealed partial class Shockwave : global::System.Configuration.ApplicationSettingsBase {
        
        private static Shockwave defaultInstance = ((Shockwave)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Shockwave())));
        
        public static Shockwave Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("-accepteula -realtime -h msiexec /i c:\\temp\\sw_lic_full_installer.msi /qn")]
        public string install_msi {
            get {
                return ((string)(this["install_msi"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("-accepteula -realtime -h c:\\temp\\sw_uninstaller.exe /s")]
        public string uninstall_app {
            get {
                return ((string)(this["uninstall_app"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\\\\fs1\\HelpDesk\\PLUGINS\\sw_lic_full_installer.msi")]
        public string msi_location {
            get {
                return ((string)(this["msi_location"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\\\\fs1\\HelpDesk\\PLUGINS\\uninstallers\\sw_uninstaller.exe")]
        public string uninstaller_location {
            get {
                return ((string)(this["uninstaller_location"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Shockwave")]
        public string name {
            get {
                return ((string)(this["name"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("-accepteula -realtime -h wmic product where \"name like \'Adobe Shockwave%%\'\" call " +
            "uninstall /nointeractive")]
        public string uninstall_WMIC {
            get {
                return ((string)(this["uninstall_WMIC"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\\temp\\")]
        public string remotedir {
            get {
                return ((string)(this["remotedir"]));
            }
            set {
                this["remotedir"] = value;
            }
        }
    }
}
