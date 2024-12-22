window.ScriptsLoaded = true;
for (var queuedScripts = window.PreloadQueue || [], i = 0; i < queuedScripts.length; i++)
queuedScripts[i]();