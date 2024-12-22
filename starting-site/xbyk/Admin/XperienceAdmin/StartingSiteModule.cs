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

            base.OnInit();
        }

        private void Initialized_Execute(object? sender, EventArgs e)
        {
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
