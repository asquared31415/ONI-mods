using STRINGS;

namespace MicroTransformer;

public static class MicroTransformerStrings
{
	public static class STRINGS
	{
		public static class BUILDINGS
		{
			public static class PREFABS
			{
				public static class ASQUARED31415
				{
					public static class MICROTRANSFORMER
					{
						public static readonly LocString NAME = "Micro Transformer";

						public static readonly LocString DESC =
							$"Connect {UI.FormatAsLink("Batteries", "BATTERY")} on the large side to act as a valve and prevent {UI.FormatAsLink("Wires", "WIRE")} from drawing more than 750 W and suffering overload damage.";

						public static readonly LocString EFFECT =
							$"Limits {UI.FormatAsLink("Power", "POWER")} flowing through the Transformer to 750 W.";
					}
				}
			}
		}
	}
}
