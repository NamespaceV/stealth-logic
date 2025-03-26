using System.Diagnostics;
using UnityEditor;
using UnityEditor.Callbacks;
using Debug = UnityEngine.Debug;

public class BuildAutomation {
    [PostProcessBuildAttribute(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
        Debug.Log( "Zipping: " + pathToBuiltProject );
        //7z a  ..\polyExport\web.zip ..\polyExport\web\*
        var p = Process.Start("7z", $"a {pathToBuiltProject}.zip  {pathToBuiltProject}/*" );
        p.WaitForExit();
        Debug.Log( "ZIP COMPLETED: " + pathToBuiltProject );
        
        //butler.exe push {pathToBuiltProject}\web.zip namespacev/stealth-logic:web


    }
}
