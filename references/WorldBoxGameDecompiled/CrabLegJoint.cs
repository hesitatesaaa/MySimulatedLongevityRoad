using UnityEngine;

public class CrabLegJoint : MonoBehaviour
{
	[Header("Joints")]
	public Transform Joint0;

	public Transform Joint1;

	public Transform Hand;

	[Header("Target")]
	public Transform Target;

	private float length0;

	private float length1;

	public float targetDistance;

	public bool mirrored;

	internal Crabzilla crabzilla;

	public float angleMax;

	public float angleMin;

	public float defaultAngle;

	private float atan;

	private float jointAngle0;

	private float jointAngle1;

	public float angle0;

	public float angle1;

	public float groundAngleMin = 50f;

	public float groundAngleMax = 140f;

	internal Transform bodyPoint;

	public float actual_z_pos;

	internal void create()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Expected O, but got Unknown
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		actual_z_pos = ((Component)this).transform.localPosition.z;
		length0 = Vector2.Distance(Vector2.op_Implicit(Joint0.position), Vector2.op_Implicit(Joint1.position));
		length1 = Vector2.Distance(Vector2.op_Implicit(Joint1.position), Vector2.op_Implicit(Hand.position));
		_ = mirrored;
		targetDistance = Vector2.Distance(Vector2.op_Implicit(Joint0.position), Vector2.op_Implicit(Target.position));
		Vector2 val = Vector2.op_Implicit(Target.position - Joint0.position);
		Quaternion rotation = ((Component)crabzilla).transform.rotation;
		atan = 0f - ((Quaternion)(ref rotation)).eulerAngles.z + Mathf.Atan2(val.y, val.x) * 57.29578f;
		float num = (targetDistance * targetDistance + length0 * length0 - length1 * length1) / (2f * targetDistance * length0);
		defaultAngle = Mathf.Acos(num) * 57.29578f;
		angleMin = defaultAngle + 20f;
		angleMax = defaultAngle + 20f;
		GameObject val2 = new GameObject("leg_point_" + ((Object)((Component)this).transform).name);
		val2.transform.position = new Vector3(((Component)this).transform.position.x, ((Component)this).transform.position.y, 0f);
		val2.transform.parent = ((Component)crabzilla.mainBody).transform;
		bodyPoint = val2.transform;
	}

	public bool isAngleOk(float pMinAngle, float pMaxAngle)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		angleMin = defaultAngle + pMinAngle;
		angleMax = defaultAngle + pMaxAngle;
		bool num = Toolbox.inBounds(angle0, angleMin, angleMax);
		Vector2 val = Vector2.op_Implicit(((Component)Joint1).transform.position - ((Component)Hand).transform.position);
		bool flag = Toolbox.inBounds(Mathf.Atan2(val.y, val.x) * 57.29578f, groundAngleMin, groundAngleMax);
		return num & flag;
	}

	internal void LateUpdate()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = bodyPoint.position;
		position.z = 0f;
		((Component)this).transform.position = position;
		((Component)this).transform.localPosition = new Vector3(((Component)this).transform.localPosition.x, ((Component)this).transform.localPosition.y, actual_z_pos);
		targetDistance = Vector2.Distance(Vector2.op_Implicit(Joint0.position), Vector2.op_Implicit(Target.position));
		Vector2 val = Vector2.op_Implicit(Target.position - Joint0.position);
		Quaternion rotation = ((Component)crabzilla).transform.rotation;
		atan = 0f - ((Quaternion)(ref rotation)).eulerAngles.z + Mathf.Atan2(val.y, val.x) * 57.29578f;
		if (length0 + length1 < targetDistance)
		{
			jointAngle0 = atan;
			jointAngle1 = 0f;
		}
		else
		{
			float num = (targetDistance * targetDistance + length0 * length0 - length1 * length1) / (2f * targetDistance * length0);
			angle0 = Mathf.Acos(num) * 57.29578f;
			float num2 = (length1 * length1 + length0 * length0 - targetDistance * targetDistance) / (2f * length1 * length0);
			angle1 = Mathf.Acos(num2) * 57.29578f;
			if (mirrored)
			{
				jointAngle0 = atan + angle0;
				jointAngle1 = 180f + angle1;
			}
			else
			{
				jointAngle0 = atan - angle0;
				jointAngle1 = 180f - angle1;
			}
		}
		if (!float.IsNaN(jointAngle0))
		{
			Vector3 localEulerAngles = ((Component)Joint0).transform.localEulerAngles;
			localEulerAngles.z = jointAngle0;
			((Component)Joint0).transform.localEulerAngles = localEulerAngles;
			Vector3 localEulerAngles2 = ((Component)Joint1).transform.localEulerAngles;
			localEulerAngles2.z = jointAngle1;
			((Component)Joint1).transform.localEulerAngles = localEulerAngles2;
		}
	}
}
