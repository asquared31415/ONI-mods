namespace ItemPermeableTiles
{
	public class ItemPermeableTile : KMonoBehaviour
	{
		protected override void OnSpawn()
		{
			var building = GetComponent<Building>();
			if (building != null)
			{
				foreach (var cell in building.PlacementCells)
				{
					SimMessages.ClearCellProperties(cell, (int) Sim.Cell.Properties.SolidImpermeable);
				}
			}
		}
	}
}
