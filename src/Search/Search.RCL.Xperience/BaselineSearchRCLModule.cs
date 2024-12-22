using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using Core;
using Core.Installers;
using Microsoft.Extensions.DependencyInjection;
using Search.Installers;

[assembly: RegisterModule(typeof(BaselineSearchRCLModule))]

namespace Core
{
    public class BaselineSearchRCLModule : Module
    {
        private IServiceProvider? _services = null;
        private BaselineSearchModuleInstaller? _installer = null;
        private BaselineModuleInstaller? _installerCore = null;

        public BaselineSearchRCLModule() : base("BaselineSearchRCLModule")
        {

        }

        protected override void OnInit(ModuleInitParameters parameters)
        {
            ApplicationEvents.Initialized.Execute += Initialized_Execute;
            _services = parameters.Services;
            _installer = _services.GetService<BaselineSearchModuleInstaller>();
            _installerCore = _services.GetService<BaselineModuleInstaller>();

            base.OnInit();
        }

        private void Initialized_Execute(object? sender, EventArgs e)
        {
            // Make sure dependent Member Roles installs first
            if (_installerCore != null) {
                if (!_installerCore.InstallationRan) {
                    _installerCore?.Install();
                }
                _installer?.Install();
            }
        }
    }
}
