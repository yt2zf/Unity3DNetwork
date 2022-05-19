using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CtrlHuman : BaseHuman
{
	// Use this for initialization
	new void Start()
	{
		base.Start();
	}

	// Update is called once per frame
	new void Update()
	{
		base.Update();
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			Physics.Raycast(ray, out hit);
			if (hit.collider.tag == "Terrain")
			{
				MoveTo(hit.point);
				string moveMsg = "Move|";
				moveMsg += NetManager.GetDesc() + ",";
				moveMsg += hit.point.x + ",";
				moveMsg += hit.point.y + ",";
				moveMsg += hit.point.z;
				NetManager.Send(moveMsg);
			}
		}
		if (Input.GetMouseButtonDown(1))
		{
			if (isAttacking)
				return;
			if (isMoving)
				return;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			Physics.Raycast(ray, out hit);
			// 发送攻击协议
			if (hit.collider.tag == "Terrain")
			{
				transform.LookAt(hit.point);
				Attack();
				string attackMsg = "Attack|";
				attackMsg += NetManager.GetDesc() + ",";
				attackMsg += transform.eulerAngles.y + ",";
				NetManager.Send(attackMsg);
			}
			// 发送Hit协议
			RaycastHit attackRayCast;
			Vector3 lineEnd = transform.position + 0.5f * Vector3.up + 0.5f * transform.forward;
			Vector3 lineStart = lineEnd + 20 * transform.forward;
			if (Physics.Linecast(lineStart, lineEnd, out attackRayCast))
			{	
				GameObject hitObj = attackRayCast.collider.gameObject;
				if (hitObj == this.gameObject)
				{
					return;
				}
					
				SyncHuman puppet = hitObj.GetComponent<SyncHuman>();
				if (puppet == null)
					return;

				string sendStr = "Hit|" + puppet.desc + ",";
				NetManager.Send(sendStr);
			}


		}
	}
}
