using UnityEngine;

public class DebugVariables : MonoBehaviour
{
	public static DebugVariables instance;

	[Range(1f, 1000f)]
	public float multiplier = 1f;

	[Range(1f, 10000000f)]
	public float bonus = 1f;

	public float time;

	[Range(0f, 1000f)]
	public float gravity = 9.8f;

	[Range(0f, 10f)]
	public float unit_force_multiplier = 1f;

	[Range(0f, 10f)]
	public float test_mass = 2f;

	public bool layout_city_test;

	public bool layout_lines_horizontal;

	public bool layout_lines_vertical;

	public bool layout_cross;

	public bool layout_diagonal;

	public bool layout_lattice_small;

	public bool layout_lattice_medium;

	public bool layout_lattice_big;

	public bool layout_clusters_small;

	public bool layout_clusters_medium;

	public bool layout_clusters_big;

	public bool layout_ring;

	public bool layout_diamond;

	public bool layout_diamond_cluster;

	public bool layout_honeycomb;

	public bool layout_brick_vertical;

	public bool layout_brick_horizontal;

	public bool layout_madman_labyrinth;

	public bool layout_map_ring;
}
