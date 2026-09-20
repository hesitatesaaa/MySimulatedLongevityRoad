using System.Collections.Generic;

public class AnimationDataBoat
{
	internal string id;

	internal Dictionary<int, ActorAnimation> dict = new Dictionary<int, ActorAnimation>();

	internal ActorAnimation broken;

	internal ActorAnimation normal;
}
