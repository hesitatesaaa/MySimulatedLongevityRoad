using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class ActorBag
{
	[JsonProperty]
	internal Dictionary<string, ResourceContainer> dict;

	[JsonProperty]
	internal string last_item_to_render;
}
