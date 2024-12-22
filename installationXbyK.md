# Installation for XbyK

Below is a quick and dirty installation.  I'll update it and provide better documentation soon, however it's 3:08am and i'm a little tired.

1. Clone the Repository and retrieve the [starting-site/xbyk](https://github.com/KenticoDevTrev/XperienceCommunity.Baseline/tree/master/starting-site/xbyk) folder (you can get rid of the rest)
2. Go into the **FrontEndDev** folder, and run the following commands to rebuild the css/js (requires Node.js, i have 20.17 but should work on latest)
```
npm install
npm run build
```
3. Go into the **MVC** folder, and open up a terminal there and run the following commands:
```
dotnet restore
dotnet build
```
4. Open the **MVC\\.config** folder and right-click on **dotnet-tools.json** and unblock it (the DB installer, honestly couldn't figure out how to install just that, it comes with the kentico.xperience.templates)
5. Retrieve a license, and add the license content into a file `license.txt` within the **MVC** folder
6. Run a command similar to the below to install the database, modify according to [Kentico's instructions](https://docs.kentico.com/developers-and-admins/installation#create-the-project-database) and picking a random GUID for the hash-string-salt
```
dotnet tool restore
dotnet kentico-xperience-dbmanager -- -s ".\SQL2022" -d "Xperience_Baseline" -a "YourAdminPassword@" --hash-string-salt "11111111-2222-3333-4444-555555555555" --license-file "license.txt"
```
7. Merge the `appsettings.json.backup` with your newly generated `appsettings.json` file (there are configurations for the Baseline you can opt into)
8. Open the entire solution, and search for `BASELINE CUSTOMIZATION` in the code, this will show all the customization points.
9. Once configured to your liking, run the site and go to the Admin
```
dotnet build
dotnet run --project MVC
``` 

You may need to do additional configurations in the admin, depending on your setup.  By default it should create a default website channel, configure all the page types and get you going.
