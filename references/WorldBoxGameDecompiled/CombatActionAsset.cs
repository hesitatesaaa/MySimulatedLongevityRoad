using System;

[Serializable]
public class CombatActionAsset : Asset
{
	public bool play_unit_attack_sounds;

	public CombatActionPool[] pools;

	public string tag_required;

	public int rate = 1;

	public float chance = 0.2f;

	public float cooldown = 1f;

	public bool is_spell_use;

	public int cost_stamina;

	public int cost_mana;

	public bool basic;

	public CombatAction action;

	public CombatActionActor action_actor;

	public CombatActionActorTargetPosition action_actor_target_position;

	public CombatActionCheckStart can_do_action;
}
