using UnityEngine;

public class Meteorite : BaseEffect
{
	private SpriteRenderer _shadow_renderer;

	public Vector3 rotationSpeed;

	private float _falling_speed;

	public GameObject mainSprite;

	public GameObject shadowSprite;

	private int _radius;

	private float _shadow_alpha;

	private float _timer_smoke;

	public string terraform_asset;

	private Actor _owner;

	public override void Awake()
	{
		base.Awake();
		_shadow_renderer = shadowSprite.GetComponent<SpriteRenderer>();
	}

	internal override void create()
	{
		base.create();
	}

	public void spawnOn(WorldTile pTile, string pTerraformId, Actor pActor)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		terraform_asset = pTerraformId;
		tile = pTile;
		_radius = 20;
		((Component)this).transform.position = new Vector3(pTile.posV3.x, pTile.posV3.y);
		current_position.x = Randy.randomFloat(-200f, 200f);
		current_position.y = Randy.randomFloat(200f, 250f);
		updateMainSpritePos();
		setShadowAlpha(0f);
	}

	private void updateMainSpritePos()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = new Vector3
		{
			x = current_position.x,
			y = current_position.y
		};
		float y = current_position.y;
		localPosition.z = y;
		mainSprite.transform.localPosition = localPosition;
	}

	protected void smoothMovement(Vector2 end, float pElapsed)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = current_position - end;
		if (((Vector2)(ref val)).sqrMagnitude > float.Epsilon)
		{
			current_position = Vector2.MoveTowards(current_position, end, _falling_speed * pElapsed);
			updateMainSpritePos();
			shadowSprite.transform.localPosition = Vector2.op_Implicit(new Vector2(current_position.x, shadowSprite.transform.localPosition.y));
		}
		else
		{
			explode();
		}
	}

	public override void update(float pElapsed)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		smoothMovement(Vector2.zero, pElapsed);
		_shadow_alpha += World.world.elapsed * 0.2f;
		setShadowAlpha(_shadow_alpha);
		mainSprite.transform.Rotate(rotationSpeed * World.world.elapsed);
		shadowSprite.transform.Rotate(rotationSpeed * World.world.elapsed);
		if (_timer_smoke > 0f)
		{
			_timer_smoke -= World.world.elapsed;
			return;
		}
		EffectsLibrary.spawnAt("fx_fire_smoke", mainSprite.transform.position, 1f);
		_timer_smoke = 0.05f;
	}

	protected void setShadowAlpha(float pVal)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		_shadow_alpha = pVal;
		if (_shadow_alpha < 0f)
		{
			_shadow_alpha = 0f;
		}
		Color color = _shadow_renderer.color;
		color.a = _shadow_alpha;
		_shadow_renderer.color = color;
	}

	private void explode()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		World.world.game_stats.data.meteoritesLaunched++;
		MapAction.damageWorld(tile, _radius, AssetManager.terraform.get(terraform_asset), _owner);
		EffectsLibrary.spawnExplosionWave(tile.posV3, _radius);
		Vector2Int pos = tile.pos;
		float num = ((Vector2Int)(ref pos)).x;
		pos = tile.pos;
		Vector3 pPos = default(Vector3);
		((Vector3)(ref pPos))._002Ector(num, (float)(((Vector2Int)(ref pos)).y - 2));
		float pScale = Randy.randomFloat(0.8f, 0.9f);
		EffectsLibrary.spawnAt("fx_explosion_meteorite", pPos, pScale);
		addRandomMineral(tile);
		addRandomMineral(tile.zone.getRandomTile());
		addRandomMineral(tile.zone.getRandomTile());
		addRandomMineral(tile.zone.getRandomTile());
		addRandomMineral(tile.zone.getRandomTile());
		controller.killObject(this);
	}

	private void addRandomMineral(WorldTile pTile)
	{
		if (pTile != null)
		{
			World.world.buildings.addBuilding("mineral_adamantine", pTile, pCheckForBuild: true);
		}
	}

	public static void spawnMeteoriteDisaster(WorldTile pTile, Actor pActor = null)
	{
		EffectsLibrary.spawn("fx_meteorite", pTile, "meteorite_disaster", null, 0f, -1f, -1f, pActor);
	}

	public static void spawnMeteorite(WorldTile pTile, Actor pActor = null)
	{
		EffectsLibrary.spawn("fx_meteorite", pTile, "meteorite", null, 0f, -1f, -1f, pActor);
	}

	public Meteorite()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		rotationSpeed = new Vector3(0f, 0f, 50f);
		_falling_speed = 200f;
		_timer_smoke = 0.01f;
		terraform_asset = "meteorite";
		base._002Ector();
	}
}
