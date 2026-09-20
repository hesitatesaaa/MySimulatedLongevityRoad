using System;

namespace UnityEngine.Purchasing.Security;

public class AppleTangle
{
	private static byte[] data = Convert.FromBase64String("K7kpflnrzFWnC4KQoki4nljwqXMioaCmqYom6CZXw8SloZAhUpCKpscvqBSAV2sMjIDP0BafoZAsF+Nv19eOwdDQzMWOw8/Nj8HQ0MzFw8GtpqmKJugmV62hoaWloKMioaGg/IyAw8XS1MnGycPB1MWA0M/MycPZhpCEpqP1pKuzveHQ0MzFgOPF0tTyxczJwc7DxYDPzoDUyMnTgMPF0oDBzsSAw8XS1MnGycPB1MnPzoDQrz2dU4vpiLpoXm4VGa55/rx2a50eVNM7TnLEr2vZ75R4Ap5Z2F/LaNDMxYDyz8/UgOPhkL63rZCWkJSSwszFgNPUwc7EwdLEgNTF0s3TgMFgw5PXV5qnjPZLeq+Brnoa07nvFaj+kCKhsaaj9b2ApCKhqJAioaSQljnsjdgXTSw7fFPXO1LWcteQ72HQzMWA48XS1MnGycPB1MnPzoDh1cnGycPB1MnPzoDh1dTIz9LJ1NmRgOPhkCKhgpCtpqmKJugmV62hoaGmkK+mo/W9s6GhX6SlkKOhoV+Qvaaj9b2upLaktItwyec01qleVMstnYbHgCqTyletIm9+SwOPWfPK+8STlvqQwpGrkKmmo/WkprOi9fORs9TJxsnDwdTFgMLZgMHO2YDQwdLUpKazovXzkbOQsaaj9aSqs6rh0NBpudJV/a513/87UoWjGvUv7f2tUdTIz9LJ1NmRtpC0pqP1pKOzreHQCHzegpVqhXV5r3bLdAKEg7FXAQy2kLSmo/Wko7Ot4dDQzMWA8s/P1HmW32En9XkHORmS4lt4ddE+3gHy0sHD1MnDxYDT1MHUxc3FztTTjpCKJugmV62hoaWloJDCkauQqaaj9fkHpanct+D2sb7Ucxcrg5vnA3XPvzF7vufwS6VN/tkkjUuWAvfs9UzakCKh1pCupqP1va+hoV+kpKOioczFgOnOw46RhpCEpqP1pKuzveHQp0zdmSMr84BzmGQRHzrvqstfi1zpeNY/k7TFAdc0aY2io6GgoQMioSC0i3DJ5zTWqV5Uyy2O4AZX5+3fF7sdM+KEsopnr70W7Tz+w2jrILeO4AZX5+3fqP6Qv6aj9b2DpLiQtt/hCDhZcWrGPITLsXADG0S7imO/xJWDteu1+b0TNFdWPD5v8Bph+PCloKMioa+gkCKhqqIioaGgRDEJqb8lIyW7OZ3nl1IJO+AujHQRMLJ4CwPRMufz9WEPj+ETWFtD0G1GA+wv0yHAZrv7qY8yEljk6FDAmD61VdmAwdPT1c3F04DBw8PF0NTBzsPFj5AhY6aoi6ahpaWnoqKQIRa6IROVkpGUkJOW+retk5WQkpCZkpGUkJCxpqP1pKqzquHQ0MzFgOnOw46RgM/GgNTIxYDUyMXOgMHQ0MzJw8EVmg1Ur66gMqsRgbaO1HWcrXvCtuXev+zL8DbhKWTUwquwI+EnkyohqIumoaWlp6Khtr7I1NTQ05qPj9fOxIDDz87EydTJz87TgM/GgNXTxRGQ+Ez6pJIsyBMvvX7F01/H/sUckCKkG5AiowMAo6KhoqKhopCtpqk1PtqsBOcr+3S2l5NrZK/tbrTJcYRCS3EX0H+v5UGHalHN2E1HFbe38AoqdXpEXHCpp5cQ1dWB");

	private static int[] order = new int[61]
	{
		16, 34, 21, 36, 47, 44, 10, 38, 45, 22,
		50, 50, 42, 57, 33, 48, 47, 50, 57, 29,
		30, 56, 45, 39, 56, 29, 44, 54, 42, 58,
		46, 34, 52, 48, 42, 36, 51, 54, 52, 59,
		42, 50, 46, 48, 49, 56, 55, 55, 54, 52,
		56, 56, 59, 58, 56, 56, 59, 59, 59, 59,
		60
	};

	private static int key = 160;

	public static readonly bool IsPopulated = true;

	public static byte[] Data()
	{
		if (!IsPopulated)
		{
			return null;
		}
		return Obfuscator.DeObfuscate(data, order, key);
	}
}
