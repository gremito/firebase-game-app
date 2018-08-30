using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankingView : MonoBehaviour
{

	[SerializeField] private GameObject content;
	[SerializeField] private GameObject textPrefab;

	private List<string> userPointList;

	void Awake()
	{
		Init();
	}

	void Init()
	{
		if (userPointList == null)
		{
			userPointList = new List<string>();
		}
	}

	public void AddPoint(string userName, int userPoint)
	{
		userPointList.Add(userName + ": " + userPoint + " pt");
		AddList(userPointList.Count - 1);
	}

	void AddList(int index)
	{
		GameObject _text = Instantiate(textPrefab, content.transform) as GameObject;
		_text.GetComponent<Text>().text = userPointList[index];
	}

}
