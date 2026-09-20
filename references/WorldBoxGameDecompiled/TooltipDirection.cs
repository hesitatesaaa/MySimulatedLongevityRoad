using System;

[Flags]
public enum TooltipDirection
{
	None = 0,
	Up = 1,
	Down = 2,
	Right = 4,
	Left = 8,
	MagnetUp = 0x10,
	MagnetDown = 0x20,
	MagnetRight = 0x40,
	MagnetLeft = 0x80
}
