using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class start : MonoBehaviour {

	private FirebaseAuthManager fibaseAuth;	

	public void GoToGame()
	{
		SceneManager.LoadScene("_scene");
	}

	public void GoToRanking()
	{
		SceneManager.LoadScene("ranking");
	}

	public void Awake() {
		if(fibaseAuth == null) {
			fibaseAuth = new FirebaseAuthManager();
			fibaseAuth.InitializeFirebase();
			fibaseAuth.SignInWithEmailAndPassword("xxxxx@xxx.xx","root123");
		}
	}

}
