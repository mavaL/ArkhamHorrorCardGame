using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using UnityEngine;

namespace frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter
{
	/// <summary>
	/// Custom params containing a single prefab. <see cref="ItemPrefabSize"/> is calculated on first accessing and invalidated each time <see cref="InitIfNeeded(ISRIA)"/> is called.
	/// </summary>
	public class BaseParamsWithPrefab : BaseParams
	{
		public RectTransform itemPrefab;

		public float ItemPrefabSize
		{
			get
			{
				if (!itemPrefab)
					throw new UnityException("SRIA: " + typeof(BaseParamsWithPrefab) + ": the prefab was not set. Please set it through inspector or in code");

				if (_PrefabSize == -1f)
					_PrefabSize = scrollRect.horizontal ? itemPrefab.rect.width : itemPrefab.rect.height;

				return _PrefabSize;
			}
		}

		float _PrefabSize = -1f;

		/// <inheritdoc/>
		public override void InitIfNeeded(ISRIA sria)
		{
			base.InitIfNeeded(sria);

			_PrefabSize = -1f; // so the prefab's size will be recalculated

			_DefaultItemSize = ItemPrefabSize;
		}
	}
}
