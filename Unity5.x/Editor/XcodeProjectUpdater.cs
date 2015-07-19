using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using System.Collections.Generic;

/*
https://bitbucket.org/Unity-Technologies/iosnativecodesamples/src/a0bc90e7d6358e456caf25d717134864218740a7/NativeIntegration/Misc/UpdateXcodeProject/Assets/Editor/XcodeProjectUpdater.cs?at=stable
 */
public class XcodeProjectUpdater : MonoBehaviour
{
	internal static void CopyAndReplaceDirectory(string srcPath, string dstPath)
	{
		if (Directory.Exists(dstPath))
			Directory.Delete(dstPath);
		if (File.Exists(dstPath))
			File.Delete(dstPath);
		
		Directory.CreateDirectory(dstPath); 
		
		foreach (var file in Directory.GetFiles(srcPath))
			File.Copy(file, Path.Combine(dstPath, Path.GetFileName(file)));
		
		foreach (var dir in Directory.GetDirectories(srcPath))
			CopyAndReplaceDirectory(dir, Path.Combine(dstPath, Path.GetFileName(dir)));
	}

//	internal static void AddUsrLib(PBXProject projct, string targetGuid, string framework)
//	{
//		string fileGuid = projct.AddFile ("usr/lib/"+ framework, "Frameworks/"+ framework, PBXSourceTree.Sdk);
//		projct.AddFileToBuild (targetGuid, framework);
//	}
	
	[PostProcessBuild]
	static void OnPostprocessBuild(BuildTarget buildTarget, string path)
	{
		if (buildTarget != BuildTarget.iOS) return;

		/*------------------------------------------------------*/
		// for frameworks

		string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
		Debug.Log("Build iOS. path: " + projPath);

		PBXProject proj = new PBXProject();
		proj.ReadFromString(File.ReadAllText(projPath));
		
		string target = proj.TargetGuidByName("Unity-iPhone");
//		string debugConfig = proj.BuildConfigByName(target, "Debug");
//		string releaseConfig = proj.BuildConfigByName(target, "Release");

		// Add custom system frameworks. Duplicate frameworks are ignored.
		// needed by our native plugin in Assets/Plugins/iOS
//		proj.AddFrameworkToProject(target, ".framework", false /*not weak*/);

		// Add usr/lib
		string framenwork1 = "libz.dylib";
		string framenwork2 = "libsqlite3.0.dylib";
		string fileGuid1 = proj.AddFile ("usr/lib/"+framenwork1, "Frameworks/"+framenwork1, PBXSourceTree.Sdk);
		string fileGuid2 = proj.AddFile ("usr/lib/"+framenwork2, "Frameworks/"+framenwork2, PBXSourceTree.Sdk);
		proj.AddFileToBuild (target, fileGuid1);
		proj.AddFileToBuild (target, fileGuid2);

		// Add our framework directory to the framework include path
		proj.SetBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", "$(inherited)");
		proj.AddBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", "$(PROJECT_DIR)/Frameworks");
//		proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-ObjC");
		proj.AddBuildProperty(target, "GCC_ENABLE_OBJC_EXCEPTIONS", "YES");

		File.WriteAllText(projPath, proj.WriteToString());

		/*------------------------------------------------------*/
		// for Info.plist 

		// PlistDocument
		// http://docs.unity3d.com/ScriptReference/iOS.Xcode.PlistDocument.html

		// Get plist
		var plistPath = Path.Combine(path, "Info.plist");
		var plist = new PlistDocument();
		plist.ReadFromFile(plistPath);

		// Get root
		PlistElementDict rootDict = plist.root;

		// Create URL types 
		string identifier = PlayerPrefs.GetString ("identifier");
		string scheme = PlayerPrefs.GetString ("scheme");

		PlistElementArray urlTypesArray = rootDict.CreateArray ("CFBundleURLTypes");
		PlistElementDict dict = urlTypesArray.AddDict ();
		dict.SetString ("CFBundleURLName", identifier);
		PlistElementArray schemesArray = dict.CreateArray ("CFBundleURLSchemes");
		schemesArray.AddString (scheme);

//		PlistElementArray urlTypesArray = rootDict.CreateArray ("CFBundleURLTypes");
//		PlistElementDict dict = urlTypesArray.AddDict ();
//		dict.SetString ("CFBundleURLName", "com.unitybuild.test");
//		PlistElementArray schemesArray = dict.CreateArray ("CFBundleURLSchemes");
//		schemesArray.AddString ("myscheme");

		// Write to file
		File.WriteAllText(plistPath, plist.WriteToString());
	}

}