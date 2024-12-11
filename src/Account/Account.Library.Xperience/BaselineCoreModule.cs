using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using Account;
using Account.Installers;
using Microsoft.Extensions.DependencyInjection;
using Core.Installers;

[assembly: RegisterModule(typeof(BaselineCoreAccountModule))]

namespace Account
{
    public class BaselineCoreAccountModule : Module
    {
        private IServiceProvider? _services = null;
        private BaselineAccountModuleInstaller? _installer = null;
        private BaselineModuleInstaller? _installerCore = null;

        public BaselineCoreAccountModule() : base("BaselineCoreAccountModule")
        {

        }

        protected override void OnInit(ModuleInitParameters parameters)
        {
            ApplicationEvents.Initialized.Execute += Initialized_Execute;
            _services = parameters.Services;
            _installerCore = _services.GetRequiredService<BaselineModuleInstaller>();
            _installer = _services.GetRequiredService<BaselineAccountModuleInstaller>();
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
