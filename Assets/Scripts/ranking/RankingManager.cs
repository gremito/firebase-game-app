using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class UserRanking {
    private string name = string.Empty;
	public string Name {
		get { return this.name; }
		set { this.name = value; }
	}
    private int point = -1;
	public int Point {
		get { 
			if(point < 0) return 0;
			else return point;
		}
		set { this.point = value; }
	}
	public bool HasData {
		get {
			if(name == string.Empty || point == -1) {
				return false;
			}
			return true;
		}
	}
	public UserRanking() {}
	public UserRanking(string name, int point) {
		this.name = name;
		this.point = point;
	}
}

public class RankingManager : MonoBehaviour
{

	[SerializeField]
	private RankingView rankingView;
	[SerializeField]
	private GameObject backGround;
	[SerializeField]
	private GameObject rankingScrollView;

	private DatabaseReference reference = null;

	private List<UserRanking> userRankingList = new List<UserRanking>();



	void Start ()
	{
		backGround.SetActive(true);
		rankingScrollView.SetActive(false);

		FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://【Firebase アプリ名】.firebaseio.com/");
		FirebaseApp.DefaultInstance.SetEditorP12FileName("【Firebase アプリ名】-xxxxxxxxxxxxx.p12");
		FirebaseApp.DefaultInstance.SetEditorServiceAccountEmail("【Firebase アプリ名】@【Firebase アプリ名】.iam.gserviceaccount.com");
		FirebaseApp.DefaultInstance.SetEditorP12Password("notasecret");

		reference = FirebaseDatabase.DefaultInstance.RootReference;
	
		GetRanking();
	}
	
	public void GoToStart()
	{
		SceneManager.LoadScene("start");
	}

	/// <summary>
	/// ランキング情報取得
	/// </summary>
	private void GetRanking()
	{
		FirebaseDatabase.DefaultInstance
		.GetReference("rankings")
		.GetValueAsync().ContinueWith(task => {
			if (task.IsCompleted) {
				DataSnapshot snapshot = task.Result;
				foreach(DataSnapshot rankingData in snapshot.Children) {
					Debug.Log("ID: " + rankingData.Key);
					Debug.Log("rankingData.Child(name).Value: " + rankingData.Child("name").Value);
					Debug.Log("rankingData.Child(point).Value: " + rankingData.Child("point").Value);
					userRankingList.Add(new UserRanking(
						(string) rankingData.Child("name").Value,
						// longになっているらしい
						(int)(long) rankingData.Child("point").Value
					));
				}
				if(userRankingList.Count > 0) {
					UpdateRanking();
				}
			}
		});		
	}

	private void UpdateRanking() {
		backGround.SetActive(false);
		rankingScrollView.SetActive(true);

		// TODO: firebase側で対応
		userRankingList.Sort((a, b) => b.Point - a.Point);

		// ランキング表示
		foreach(UserRanking rankingData in userRankingList) {
			rankingView.AddPoint(rankingData.Name, rankingData.Point);
		}
	}

}
