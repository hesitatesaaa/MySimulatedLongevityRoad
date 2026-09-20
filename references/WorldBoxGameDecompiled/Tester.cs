using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
	public GlowParticles smoke;

	public GlowParticles fire;

	public TestStage testStage;

	private List<TestingEvent> events;

	private List<TestingEvent> eventsCivs;

	private float timer = 1f;

	public float testStageTimer = 20f;

	public bool enableFastBuilding;

	public bool enableRandomSpawn = true;

	private void init()
	{
		events = new List<TestingEvent>();
		eventsCivs = new List<TestingEvent>();
		foreach (GodPower item in AssetManager.powers.list)
		{
			if (item.id[0] != '_')
			{
				TestingEvent pEvent = add(new TestingEvent
				{
					type = TestingEventType.RandomClick,
					powerID = item.id
				});
				if (item.type == PowerActionType.PowerDrawTile)
				{
					add(pEvent);
					add(pEvent);
					add(pEvent);
				}
			}
		}
		eventsCivs.Add(new TestingEvent
		{
			powerID = "humans",
			type = TestingEventType.RandomClick
		});
		eventsCivs.Add(new TestingEvent
		{
			powerID = "orcs",
			type = TestingEventType.RandomClick
		});
		eventsCivs.Add(new TestingEvent
		{
			powerID = "elves",
			type = TestingEventType.RandomClick
		});
		eventsCivs.Add(new TestingEvent
		{
			powerID = "dwarfs",
			type = TestingEventType.RandomClick
		});
		setTestStage(TestStage.SPAWN_CIVS);
		((Behaviour)smoke).enabled = false;
		((Behaviour)fire).enabled = false;
	}

	private void setTestStage(TestStage pStage)
	{
		testStage = pStage;
		switch (testStage)
		{
		case TestStage.SPAWN_CIVS:
			testStageTimer = 10f;
			break;
		case TestStage.WAIT_CIVS:
			testStageTimer = 60f;
			break;
		case TestStage.SPAWN_CHAOS:
			testStageTimer = 30f;
			break;
		case TestStage.REGENERATE:
			testStageTimer = 1f;
			break;
		}
	}

	private TestingEvent add(TestingEvent pEvent)
	{
		events.Add(pEvent);
		return pEvent;
	}

	private void Update()
	{
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		if (!Config.game_loaded)
		{
			return;
		}
		if (events == null)
		{
			init();
		}
		else
		{
			if (!enableRandomSpawn)
			{
				return;
			}
			if (timer > 0f)
			{
				timer -= Time.deltaTime;
			}
			else if (testStageTimer > 0f)
			{
				testStageTimer -= Time.deltaTime;
				TestingEvent testingEvent = null;
				switch (testStage)
				{
				default:
					return;
				case TestStage.SPAWN_CIVS:
					testingEvent = eventsCivs.GetRandom();
					break;
				case TestStage.SPAWN_CHAOS:
					testingEvent = events.GetRandom();
					break;
				}
				ScrollWindow.hideAllEvent(pWithAnimation: false);
				if (testingEvent == null)
				{
					return;
				}
				if (testingEvent.type != TestingEventType.RandomClick)
				{
					_ = 1;
					return;
				}
				int num = Randy.randomInt(0, MapBox.width);
				int num2 = Randy.randomInt(0, MapBox.height);
				LogText.log(testingEvent.powerID, "Test Power", "st");
				if (!AssetManager.powers.dict.ContainsKey(testingEvent.powerID))
				{
					MonoBehaviour.print((object)("TESTER ERROR... " + testingEvent.powerID));
				}
				GodPower godPower = AssetManager.powers.get(testingEvent.powerID);
				if (godPower.tester_enabled)
				{
					Config.current_brush = Brush.getRandom();
					World.world.player_control.clickedFinal(new Vector2Int(num, num2), godPower);
					LogText.log(testingEvent.powerID, "Test Power", "en");
				}
			}
			else
			{
				switch (testStage)
				{
				case TestStage.SPAWN_CIVS:
					setTestStage(TestStage.WAIT_CIVS);
					break;
				case TestStage.WAIT_CIVS:
					setTestStage(TestStage.SPAWN_CHAOS);
					break;
				case TestStage.SPAWN_CHAOS:
					setTestStage(TestStage.REGENERATE);
					break;
				case TestStage.REGENERATE:
					Config.customZoneX = 7;
					Config.customZoneY = 7;
					World.world.generateNewMap();
					testStageTimer = 20f;
					setTestStage(TestStage.SPAWN_CIVS);
					break;
				}
			}
		}
	}
}
