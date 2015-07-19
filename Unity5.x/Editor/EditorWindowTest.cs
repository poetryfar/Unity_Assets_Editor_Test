using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public class EditorWindowTest : EditorWindow {

	string identifier = "";
	string scheme     = "";
	bool refreshSaveData = true;

	[MenuItem("EditMenu/URLScheme")]
	static void Init ()
	{
		EditorWindow.GetWindow<EditorWindowTest>("AddURLSchem");
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI (){

		if (refreshSaveData) {
			LoadURLTypes ();
			refreshSaveData = false;
			Debug.Log("LoadURLTypes");
		}

		GUILayout.Label ("URL types", EditorStyles.boldLabel);
		identifier = EditorGUILayout.TextField ("URL identifier:", identifier);
		scheme = EditorGUILayout.TextField ("URL Schemes:", scheme);
		GUILayout.Space (20);

		if (GUILayout.Button ("Done")) {
			SaveURLTypes ();
			this.Close();
			Debug.Log("SaveURLTypes");
		} 
	}

	private void SaveURLTypes() {
		PlayerPrefs.DeleteAll ();
		PlayerPrefs.SetString ("identifier", identifier);
		PlayerPrefs.SetString ("scheme", scheme);
		PlayerPrefs.Save();
	}

	private void LoadURLTypes() {
		identifier = PlayerPrefs.GetString ("identifier");
		scheme = PlayerPrefs.GetString ("scheme");
	}
}
