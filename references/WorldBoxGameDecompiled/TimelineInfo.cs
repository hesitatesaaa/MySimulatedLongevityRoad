using System.Runtime.InteropServices;
using FMOD;

[StructLayout(LayoutKind.Sequential)]
internal class TimelineInfo
{
	public int currentMusicBar;

	public StringWrapper lastMarker;
}
