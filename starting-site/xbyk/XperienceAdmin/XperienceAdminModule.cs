using Admin;
using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;

[assembly: RegisterModule(typeof(XperienceAdminModule))]
namespace Admin
{
    public class XperienceAdminModule : Module
    {
        public XperienceAdminModule() : base("XperienceAdminModule")
        {

        }

        protected override void OnInit(ModuleInitParameters parameters)
        {
            ApplicationEvents.Initialized.Execute += Initialized_Execute;

            base.OnInit();
        }

        private void Initialized_Execute(object? sender, EventArgs e)
        {
            // For testing

        }

        
    }
}
