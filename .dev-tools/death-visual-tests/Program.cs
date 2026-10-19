using UnityEngine;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Systems.Visual;

int assertions = 0;
void Check(bool ok, string message) { assertions++; if (!ok) throw new Exception(message); }
void Equal(Vector3 actual, Vector3 expected, string message)
{
    Check(Math.Abs(actual.x - expected.x) < 0.0001f && Math.Abs(actual.y - expected.y) < 0.0001f && Math.Abs(actual.z - expected.z) < 0.0001f, message);
}
var random = new Random(654);
for (int i = 0; i < 50; i++)
{
    World.world = null;
    var actor = new Actor
    {
        current_position = new(i + 4, i + 9), position_height = i * 0.05f,
        shake_offset = new(-0.2f, 0.3f), move_jump_offset = new(0.4f, -0.1f),
        current_scale = new((float)random.NextDouble() + 0.1f, (float)random.NextDouble() + 0.1f, 1),
        current_rotation = new(0, 0, i), flip = i % 2 == 0,
        color = new() { r = 0.3f, g = 0.6f, b = 0.8f, a = 1 }
    };
    var pose = MclslBeastDeathVisualSystem.Capture(actor);
    Check(pose.Valid, "Supported data should yield a fallback pose");
    Equal(pose.Position, new(i + 4.2f, i + 9.2f + actor.position_height, actor.position_height), "Fallback must retain render offsets and height depth");
    Equal(pose.Scale, actor.current_scale, "Scale must not multiply canvas dimensions");
    var manager = new ActorManager
    {
        visible_units = new() { array = new[] { actor }, count = 1 },
        render_data = new() { positions = new[] { new Vector3(93, 17, 0.4f) }, scales = new[] { new Vector3(0.15f, 0.27f, 1) },
            rotations = new[] { new Vector3(0, 0, 12) }, flip_x_states = new[] { false }, main_sprites = new Sprite[] { null },
            materials = new[] { new Material() }, colors = new[] { new Color { r = 0.7f, a = 1 } } }
    };
    World.world = new() { units = manager };
    Equal(MclslBeastDeathVisualSystem.Capture(actor).Position, pose.Position, "Never-rendered slots must not teleport death to stale positions");
    manager.render_data.main_sprites[0] = new();
    pose = MclslBeastDeathVisualSystem.Capture(actor);
    Equal(pose.Position, manager.render_data.positions[0], "Visible native position must be retained");
    Equal(pose.Scale, manager.render_data.scales[0], "Visible native scale must be retained exactly once");
    var clip = new MclslBeastAnimation.Clip();
    MclslActorAnimationTime.Now = 10;
    Check(MclslBeastDeathVisualSystem.Play(actor, pose, clip), "Complete death clip should create a transient sprite");
    var visual = MclslWorldSpriteRenderLayer.Created[^1];
    Equal(visual.obj.transform.position, pose.Position, "Transient render position");
    Equal(visual.obj.transform.localScale, pose.Scale, "Transient render scale");
    Check(visual.renderer.sharedMaterial == pose.Material && visual.renderer.color.r == pose.Color.r && visual.renderer.flipX == pose.Flip, "Native material, tint and facing must carry over");
    MclslActorAnimationTime.Now = 10.35;
    MclslBeastDeathVisualSystem.Tick();
    Check(visual.renderer.sprite == clip.Sprites[2], "Nonuniform clip durations select the third frame");
    MclslBeastDeathVisualSystem.Tick();
    Check(!visual.obj.Destroyed, "Pause must preserve the transient visual");
    MclslActorAnimationTime.Now = 10.7;
    MclslBeastDeathVisualSystem.Tick();
    Check(visual.obj.Destroyed, "Death visual expires after actual clip duration");
    MclslActorAnimationTime.Now = 20;
    MclslBeastDeathVisualSystem.Play(actor, pose, clip);
    visual = MclslWorldSpriteRenderLayer.Created[^1];
    MclslBeastDeathVisualSystem.Forget(actor);
    Check(visual.obj.Destroyed, "Form reset must remove the old visual");
    MclslBeastDeathVisualSystem.Play(actor, pose, clip);
    visual = MclslWorldSpriteRenderLayer.Created[^1];
    actor.Alive = true;
    MclslBeastDeathVisualSystem.Tick();
    Check(visual.obj.Destroyed, "Resurrection must remove the old death visual");
    actor.Alive = false;
    MclslBeastDeathVisualSystem.Play(actor, pose, clip);
    visual = MclslWorldSpriteRenderLayer.Created[^1];
    MclslBeastDeathVisualSystem.Clear();
    Check(visual.obj.Destroyed, "World load clears transient visuals");
}
Check(!MclslBeastDeathVisualSystem.Capture(null).Valid, "Missing actors have no death pose");
Check(!MclslBeastDeathVisualSystem.Capture(new Actor { data = null }).Valid, "Missing actor data has no death pose");
Check(!MclslBeastDeathVisualSystem.Play(null, default, new()), "Invalid pose must not create a visual");
MclslWorldSpriteRenderLayer.Fail = true;
Check(!MclslBeastDeathVisualSystem.Play(null, new() { Valid = true }, new()), "Failed renderer creation must not retain a transient entry");
Console.WriteLine($"PASS: {assertions} assertions on native pose inheritance and death visual lifecycle; Unity and game boundaries are stubbed.");
