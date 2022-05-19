using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
	public GameObject humanPrefab;
	public BaseHuman myHuman; // 主客户端
	public Dictionary<string, BaseHuman> otherHumans = new Dictionary<string, BaseHuman>();

	void Start()
	{	
		// 网络连接
		NetManager.AddListener("Enter", OnEnter);
		NetManager.AddListener("Move", OnMove);
		NetManager.AddListener("Leave", OnLeave);
		NetManager.AddListener("Attack", OnAttack);
		NetManager.AddListener("Die", OnDie);
		NetManager.Connect("127.0.0.1", 8888);

		// 添加角色
		GameObject humanObj = (GameObject)Instantiate(humanPrefab);
		float x = Random.Range(-5, 5);
		float z = Random.Range(-5, 5);
		Vector3 pos = new Vector3(x, 0, z);
		humanObj.transform.position = pos;
		myHuman = humanObj.AddComponent<CtrlHuman>();
		myHuman.desc = NetManager.GetDesc();

		// 发送Enter协议
		Vector3 eul = humanObj.transform.eulerAngles;
		string sendStr = "Enter|";
		sendStr += NetManager.GetDesc() + ",";
		sendStr += pos.x + ",";
		sendStr += pos.y + ",";
		sendStr += pos.z + ",";
		sendStr += eul.y + ",";
		sendStr += myHuman.hp;
		NetManager.Send(sendStr);

	}

	private void OnEnter(string msgArgs)
	{
		Debug.Log("OnEnter received: " + msgArgs);
		string[] args = msgArgs.Split(',');
		

		int count = (args.Length - 1) / 6;
		
		for (int i = 0; i < count; i++)
		{
			string desc = args[6 * i];
			float x = float.Parse(args[6 * i + 1]);
			float y = float.Parse(args[6 * i + 2]);
			float z = float.Parse(args[6 * i + 3]);
			float eulY = float.Parse(args[6 * i + 4]);
			int hp = int.Parse(args[6 * i + 5]);

			if (desc == NetManager.GetDesc())
				return;

			GameObject otherHumanObj = (GameObject)Instantiate(humanPrefab);
			Vector3 pos = new Vector3(x, y, z);
			Vector3 eulerAngles = new Vector3(0, eulY, 0);
			otherHumanObj.transform.position = pos;
			otherHumanObj.transform.eulerAngles = eulerAngles;
			BaseHuman otherHuman = otherHumanObj.AddComponent<SyncHuman>();
			otherHuman.desc = desc;
			otherHuman.hp = hp;
			otherHumans.Add(desc, otherHuman);
		}
		

	}

	private void OnMove(string msgArgs)
	{
		Debug.Log("OnMove received: " + msgArgs);
		string[] args = msgArgs.Split(',');
		string desc = args[0];
		float x = float.Parse(args[1]);
		float y = float.Parse(args[2]);
		float z = float.Parse(args[3]);

		if (!this.otherHumans.ContainsKey(desc))
			return;

		BaseHuman puppet = this.otherHumans[desc];
		Vector3 targetPos = new Vector3(x, y, z);
		puppet.MoveTo(targetPos);
	}

	private void OnLeave(string msgArgs)
	{
		Debug.Log("OnLeave received: " + msgArgs);
		string[] args = msgArgs.Split(',');
		string desc = args[0];
		if (!this.otherHumans.ContainsKey(desc))
			return;
		BaseHuman puppet = this.otherHumans[desc];
		this.otherHumans.Remove(desc);
		Destroy(puppet.gameObject);
	}

	private void OnAttack(string msgArgs)
	{
		Debug.Log("OnAttack received: " + msgArgs);
		string[] args = msgArgs.Split(',');
		string desc = args[0];
		float eulY = float.Parse(args[1]);
		if (!this.otherHumans.ContainsKey(desc))
			return;
		SyncHuman puppet = (SyncHuman) this.otherHumans[desc];
		puppet.SyncAttack(eulY);
	}

	private void OnDie(string msgArgs)
	{
		Debug.Log("OnDie received: " + msgArgs);
		string[] args = msgArgs.Split(',');
		string desc = args[0];
		if (desc == NetManager.GetDesc())
		{	
			// 主客户端Die
			Debug.Log("GAME OVER!");
			return;
		}
		if (!this.otherHumans.ContainsKey(desc))
			return;
		SyncHuman puppet = (SyncHuman)this.otherHumans[desc];
		puppet.gameObject.SetActive(false);
	}

	// Update is called once per frame
	void Update()
	{
		NetManager.Update();
	}
}
