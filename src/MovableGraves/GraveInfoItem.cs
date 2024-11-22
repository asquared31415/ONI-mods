using System.Globalization;
using KSerialization;
using STRINGS;

namespace MovableGraves;

[SerializationConfig(MemberSerialization.OptIn)]
public class GraveInfoItem : KMonoBehaviour
{
	[Serialize] public string graveName;
	[Serialize] public int epitaphIdx = -1;
	[Serialize] public float burialTime = -1f;
	[Serialize] public string graveAnim = "";

#pragma warning disable CS0649
	[MyCmpReq] private KSelectable selectable;
#pragma warning restore CS0649

	private static StatusItem infoStatus;

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();

		infoStatus ??= new StatusItem(
			"MovableGraves.GraveInfoItemStatus",
			"PREFABS",
			"",
			StatusItem.IconType.Info,
			NotificationType.Neutral,
			false,
			OverlayModes.None.ID,
			resolve_string_callback: ResolveString
		);
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		selectable.AddStatusItem(infoStatus, this);
	}

	public void SetInfo(string dupeName, int idx, float time, string anim)
	{
		graveName = dupeName;
		gameObject.name = dupeName;
		epitaphIdx = idx;
		burialTime = time;
		graveAnim = anim;
	}

	private static string ResolveString(string format, object data)
	{
		if (data is not GraveInfoItem graveInfoItem)
		{
			return "";
		}

		var newStr = format;

		newStr = newStr.Replace("{name}", graveInfoItem.graveName);
		newStr = newStr.Replace(
			"{time}",
			((int) (graveInfoItem.burialTime / Constants.SECONDS_PER_CYCLE)).ToString(CultureInfo.InvariantCulture)
		);
		var epitaphs = LocString.GetStrings(typeof(NAMEGEN.GRAVE.EPITAPHS));
		var idx = graveInfoItem.epitaphIdx % epitaphs.Length;
		newStr = newStr.Replace("{epitaph}", epitaphs[idx]);

		return newStr;
	}
}
