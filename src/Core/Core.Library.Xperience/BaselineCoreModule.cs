using CMS;
using CMS.Base;
using CMS.Core;
using Core;
using Core.Installers;
using Microsoft.Extensions.DependencyInjection;
using XperienceCommunity.MemberRoles;

[assembly: RegisterModule(typeof(BaselineCoreModule))]

namespace Core
{
    public class BaselineCoreModule : Module
    {
        private IServiceProvider? _services = null;
        private MemberRolesInstaller? _installerMemberRoles = null;
        private BaselineModuleInstaller? _installer = null;

        public BaselineCoreModule() : base("BaselineCoreModule")
        {

        }

        protected override void OnInit(ModuleInitParameters parameters)
        {
            ApplicationEvents.Initialized.Execute += Initialized_Execute;
            _services = parameters.Services;
            _installerMemberRoles = _services.GetRequiredService<MemberRolesInstaller>();

            _installer = _services.GetRequiredService<BaselineModuleInstaller>();

            base.OnInit();
        }

        private void Initialized_Execute(object? sender, EventArgs e)
        {
            // Make sure dependent Member Roles installs first
            if (_installerMemberRoles != null) {
                if (!_installerMemberRoles.InstallationRan) {
                    _installerMemberRoles?.Install();
                }
                _installer?.Install();
            }
        }


    }
}
