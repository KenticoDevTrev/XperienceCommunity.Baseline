using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using Core.Installers;
using Microsoft.Extensions.DependencyInjection;
using TabbedPages;

[assembly: RegisterModule(typeof(BaselineTabbedPagesModule))]

namespace TabbedPages
{
    public class BaselineTabbedPagesModule : Module
    {
        private IServiceProvider? _services = null;
        private TabbedPagesModuleInstaller? _installer = null;
        private BaselineModuleInstaller? _installerCore = null;

        public BaselineTabbedPagesModule() : base("BaselineTabbedPagesModule")
        {

        }

        protected override void OnInit(ModuleInitParameters parameters)
        {
            ApplicationEvents.Initialized.Execute += Initialized_Execute;
            _services = parameters.Services;
            _installer = _services.GetService<TabbedPagesModuleInstaller>();
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
