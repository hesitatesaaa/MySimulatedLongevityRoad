using UnityEngine;

public class LivingIcon : MonoBehaviour
{
	private Vector3 init_position;

	private float speed_back;

	private float speed_away;

	private float return_timer;

	public static int killed_mod = 1;

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		init_position = ((Component)this).transform.position;
	}

	public void kill()
	{
		killed_mod++;
		((Behaviour)this).enabled = false;
	}

	public void Update()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		Vector3 mousePosition = Input.mousePosition;
		float num = Vector2.Distance(Vector2.op_Implicit(((Component)this).transform.position), Vector2.op_Implicit(mousePosition));
		float num2 = 80 + killed_mod * 10;
		if (num < num2)
		{
			if (speed_away == 0f && killed_mod > 6)
			{
				speed_away = killed_mod * 10;
			}
			speed_away += 200f * Time.deltaTime * (float)killed_mod;
		}
		else if (speed_away > 0f)
		{
			speed_away -= 500f * Time.deltaTime;
			if (speed_away < 0f)
			{
				speed_away = 0f;
			}
		}
		if (speed_away > 0f)
		{
			((Component)this).transform.position = Vector2.op_Implicit(Vector2.MoveTowards(Vector2.op_Implicit(((Component)this).transform.position), Vector2.op_Implicit(mousePosition), -1f * speed_away * Time.deltaTime));
			return_timer = 1f;
			speed_back = 0f;
			rotate();
		}
		else if (return_timer > 0f)
		{
			return_timer -= Time.deltaTime;
		}
		else if (Vector2.Distance(Vector2.op_Implicit(((Component)this).transform.position), Vector2.op_Implicit(init_position)) > 1f)
		{
			speed_back += Time.deltaTime * 400f;
			((Component)this).transform.position = Vector2.op_Implicit(Vector2.MoveTowards(Vector2.op_Implicit(((Component)this).transform.position), Vector2.op_Implicit(init_position), Time.deltaTime * speed_back));
		}
		else
		{
			speed_back = 0f;
		}
		void rotate()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			Vector3 eulerAngles = ((Component)this).transform.eulerAngles;
			eulerAngles.z += 10f;
			((Component)this).transform.eulerAngles = eulerAngles;
		}
	}
}
