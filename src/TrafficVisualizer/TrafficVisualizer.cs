using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TrafficVisualizer
{
	public class CellData
	{
		public int Total;
		public readonly Dictionary<int, int> PastSums = new Dictionary<int, int>();
	}

	public static class NavigatorRecordManager
	{
		private struct CellCycle
		{
			public int Cell;
			public int Cycle;
		}

		// The number of previous cycles to keep
		private const int CycleFalloff = 3;

		// cell to data dict
		private static readonly Dictionary<int, CellData> CellData = new Dictionary<int, CellData>();

		// cell to count for current cycle dict
		private static readonly Dictionary<int, int> CurrentCounts = new Dictionary<int, int>();

		private static int _max;

		public static void UpdateCounts(object _)
		{
			if(GameClock.Instance is GameClock clock)
			{
				var cycle = clock.GetCycle();
				Debug.Log($"cycle: {cycle}");
				foreach(KeyValuePair<int, int> pair in CurrentCounts)
				{
					if(!CellData.ContainsKey(pair.Key))
					{
						CellData.Add(pair.Key, new CellData());
					}

					var data = CellData[pair.Key];
					data.PastSums.Add(cycle, pair.Value);
					var n = data.Total += pair.Value;

					if(n > _max)
					{
						_max = n;
					}
				}

				CurrentCounts.Clear();

				List<CellCycle> oldCycles = (from data in CellData from pair in data.Value.PastSums.Where(e => e.Key + CycleFalloff < cycle + 1) select new CellCycle {Cell = data.Key, Cycle = pair.Key}).ToList();

				foreach(var cellCycle in oldCycles)
				{
					var data = CellData[cellCycle.Cell];
					if(data.Total == _max)
					{
						_max -= data.PastSums[cellCycle.Cycle];
					}

					data.Total -= data.PastSums[cellCycle.Cycle];
					data.PastSums.Remove(cellCycle.Cycle);
				}
			}
		}

		public static void Add(int cell)
		{
			var n = 1;
			if(!CurrentCounts.ContainsKey(cell))
			{
				CurrentCounts.Add(cell, 1);
			}
			else
			{
				n = CurrentCounts[cell] += 1;
			}

			if(n > _max)
			{
				_max = n;
			}
		}

		public static void Clear()
		{
			CellData.Clear();
			CurrentCounts.Clear();
		}

		public static float GetRatio(int cell)
		{
			if(_max == 0)
			{
				return 0;
			}

			var c = 0;
			if(CurrentCounts.TryGetValue(cell, out var count))
			{
				c += count;
			}

			if(CellData.TryGetValue(cell, out var data))
			{
				c += data.Total;
			}

			return c / (float)_max;
		}
	}

	public static class NavigatorColors
	{
		public static readonly Color Low = new Color(0f, 0.4f, 0f);
		public static readonly Color High = Color.red;
		public static readonly Color None = new Color(1f, 1f, 1f, 0f);

		// Threshold for VISUAL effect
		private const float Threshold = 0.001f;

		public static Color GetNavColor(SimDebugView instance, int cell)
		{
			var ratio = NavigatorRecordManager.GetRatio(cell);
			return ratio > Threshold ? Color.Lerp(Low, High, ratio) : None;
		}
	}
}
