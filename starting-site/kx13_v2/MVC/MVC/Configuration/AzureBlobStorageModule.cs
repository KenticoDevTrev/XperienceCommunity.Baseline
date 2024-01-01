/*
 * Below is an example of blob storage usage
 * 
using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.IO;
// Registers the custom module into the system
[assembly: RegisterModule(typeof(AzureBlobStorageModule))]

public class AzureBlobStorageModule : Module
{
    // Module class constructor, the system registers the module under the name "CustomInit"
    public AzureBlobStorageModule()
        : base("AzureBlobStorageModule")
    {
    }


    /// <summary>
    /// WARNING: Any changes here should be replicated to the Admin's Old_App_Code/MMWeb/AzureBlobStorageModule.cs
    /// </summary>
    protected override void OnInit()
    {
        base.OnInit();
        var appSettings = Service.Resolve<IAppSettingsService>();

        string environment = appSettings["Environment"];
        string sharedRoot = appSettings["CMSAzureRootContainerShared"]; // shared
        string specificRoot = appSettings["CMSAzureRootContainerEnvironmentSpecific"]; // localdevspecificfolder
        string mediaRoot;
        string smartSearchRoot;
        string versionHistoryRoot;
        string attachmentRoot;
        bool mapCICD;

        switch (environment.ToLowerInvariant())
        {
            case "dev":
            case "local":
            default:
                mediaRoot = sharedRoot;
                smartSearchRoot = specificRoot;
                versionHistoryRoot = specificRoot;
                attachmentRoot = specificRoot;
                mapCICD = false;
                break;
            case "qa": // Where CI/CD is restored to
                mediaRoot = sharedRoot;
                smartSearchRoot = specificRoot;
                versionHistoryRoot = specificRoot;
                attachmentRoot = specificRoot;
                mapCICD = true;
                break;
            case "content": // Where content is edited
                mediaRoot = sharedRoot;
                smartSearchRoot = sharedRoot;
                versionHistoryRoot = specificRoot;
                attachmentRoot = specificRoot;
                mapCICD = false;
                break;
            case "production": // Production site
                mediaRoot = sharedRoot;
                smartSearchRoot = sharedRoot;
                versionHistoryRoot = specificRoot;
                attachmentRoot = specificRoot;
                mapCICD = false;
                break;
        }

        // Media files are always going to be in the Shared Container
        CreateAzureStorageProvider(mediaRoot, "~/media");
        CreateAzureStorageProvider(smartSearchRoot, "~/App_Data/CMSModules/SmartSearch");
        CreateAzureStorageProvider(versionHistoryRoot, "~/App_Data/VersionHistory");
        CreateAzureStorageProvider(attachmentRoot, "~/attachments");

        // This is so the QA environment, where CI/CD is restored to, uses blob storage and won't be affected by dev slots
        if (mapCICD)
        {
            CreateAzureStorageProvider(specificRoot, "~/App_Data/CIRepository");
        }
    }
    public static void CreateAzureStorageProvider(string rootPath, string path)
    {
        var provider = StorageProvider.CreateAzureStorageProvider();
        provider.CustomRootPath = rootPath;
        provider.PublicExternalFolderObject = false;
        StorageHelper.MapStoragePath(path, provider);
    }
}
*/