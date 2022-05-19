using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHuman : MonoBehaviour {

	protected bool isMoving = false;
	private Vector3 targetPosition;
	public float speed = 1.2f;
	public int hp = 100;
	private Animator animator;
	// 描述
	public string desc = "";

	// 攻击相关
	internal bool isAttacking = false;
	internal float attackTime = float.MinValue;

	public void MoveTo(Vector3 pos)
	{
		targetPosition = pos;
		transform.LookAt(targetPosition);
		isMoving = true;
		animator.SetBool("isMoving", true);
	}

	public void MoveUpdate()
	{
		if (!isMoving)
			return;
		Vector3 pos = transform.position;
		transform.position = Vector3.MoveTowards(pos, targetPosition, speed * Time.deltaTime);
		if (Vector3.Distance(pos, targetPosition) < 0.05f)
		{
			isMoving = false;
			animator.SetBool("isMoving", false);
		}
	}

	public void Attack()
	{
		isAttacking = true;
		attackTime = Time.time;
		animator.SetBool("isAttacking", true);
	}

	public void AttackUpdate()
	{
		if (!isAttacking)
			return;
		if (Time.time - attackTime < 1.2f)
			return;
		isAttacking = false;
		animator.SetBool("isAttacking", false);
	}

	// Use this for initialization
	protected void Start () {
		animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	protected void Update () {
		MoveUpdate();
		AttackUpdate();
	}
}
