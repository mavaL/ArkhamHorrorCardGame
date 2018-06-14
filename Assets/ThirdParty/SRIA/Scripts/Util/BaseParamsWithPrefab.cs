using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace frame8.ScrollRectItemsAdapter.Util
{
	/// <summary>
	/// Custom params containing a single prefab. <see cref="ItemPrefabSize"/> is calculated on first accessing and invalidated each time <see cref="InitIfNeeded(ISRIA)"/> is called.
	/// </summary>
	[Obsolete("This class was moved to frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter.BaseParamsWithPrefab. Use it instead (it's the same implementation).")]
	public class BaseParamsWithPrefab : Logic.Misc.Visual.UI.ScrollRectItemsAdapter.BaseParamsWithPrefab { }
}
