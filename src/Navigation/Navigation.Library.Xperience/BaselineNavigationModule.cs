using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using Account.Installers;
using Microsoft.Extensions.DependencyInjection;
using Core.Installers;
using Navigation;

[assembly: RegisterModule(typeof(BaselineNavigationModule))]

namespace Navigation
{
    public class BaselineNavigationModule : Module
    {
        private IServiceProvider? _services = null;
        private BaselineNavigationModuleInstaller? _installer = null;
        private BaselineModuleInstaller? _installerCore = null;

        public BaselineNavigationModule() : base("BaselineNavigationModule")
        {

        }

        protected override void OnInit(ModuleInitParameters parameters)
        {
            ApplicationEvents.Initialized.Execute += Initialized_Execute;
            _services = parameters.Services;
            _installerCore = _services.GetService<BaselineModuleInstaller>();
            _installer = _services.GetService<BaselineNavigationModuleInstaller>();
            base.OnInit();
        }

        private void Initialized_Execute(object? sender, EventArgs e)
        {
            // Make sure dependent Baseline Core installs first
            if (_installerCore != null) {
                if (!_installerCore.InstallationRan) {
                    _installerCore?.Install();
                }
                _installer?.Install();
            }
        }
    }
}
