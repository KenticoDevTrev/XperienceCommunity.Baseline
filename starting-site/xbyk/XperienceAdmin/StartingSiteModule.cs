using Admin;
using Admin.Installer;
using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using Core.Installers;
using Microsoft.Extensions.DependencyInjection;

[assembly: RegisterModule(typeof(StartingSiteModule))]

namespace Admin
{
    public class StartingSiteModule : Module
    {
        private IServiceProvider? _services = null;
        private StartingSiteInstaller? _installer = null;
        private StartingSiteInstallerChannels? _installerChannels = null;
        private BaselineModuleInstaller? _installerBaseline = null;

        public StartingSiteModule() : base("StartingSiteModule")
        {

        }

        protected override void OnInit(ModuleInitParameters parameters)
        {
            ApplicationEvents.Initialized.Execute += Initialized_Execute;
            _services = parameters.Services;
            _installerBaseline = _services.GetService<BaselineModuleInstaller>();
            _installer = _services.GetService<StartingSiteInstaller>();
            _installerChannels = _services.GetService<StartingSiteInstallerChannels>();

            base.OnInit();
        }

        private void Initialized_Execute(object? sender, EventArgs e)
        {
            // If this hits first, channel handling should be done before baseline runs, not vital if it doens't,
            // but helps get them on the web channels right away.
            if(_installerChannels != null) {
                if (!_installerChannels.InstallationRan) {
                    _installerChannels?.Install();
                }
            }

            // Make sure dependent Member Roles installs first
            if (_installerBaseline != null) {
                if (!_installerBaseline.InstallationRan) {
                    _installerBaseline?.Install();
                }
                _installer?.Install();
            }
        }


    }
}
