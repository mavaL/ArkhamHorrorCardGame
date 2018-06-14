/*
 * * * * This bare-bones script was auto-generated * * * *
 * The code commented with "/ * * /" demonstrates how data is retrieved and passed to the adapter, plus other common commands. You can remove/replace it once you've got the idea
 * Complete it according to your specific use-case
 * Consult the Example scripts if you get stuck, as they provide solutions to most common scenarios
 * 
 * Main terms to understand:
 *		Model = class that contains the data associated with an item (title, content, icon etc.)
 *		Views Holder = class that contains references to your views (Text, Image, MonoBehavior, etc.)
 * 
 * Default expected UI hiererchy:
 *	  ...
 *		-Canvas
 *		  ...
 *			-ScrollRect
 *				-Viewport
 *					-Content
 *				-Scrollbar (Optional)
 *				-ItemPrefab (Optional)
 * 
 * Note: If using Visual Studio and opening generated scripts for the first time, sometimes Intellisense (autocompletion)
 * won't work. This is a well-known bug and the solution is here: https://developercommunity.visualstudio.com/content/problem/130597/unity-intellisense-not-working-after-creating-new-1.html (or google "unity intellisense not working new script")
 */

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.Logic.Misc.Other.Extensions;
using frame8.ScrollRectItemsAdapter.Util;
using frame8.ScrollRectItemsAdapter.Util.GridView;

// The date was temporarily included in the namespace to prevent duplicate class names
// You should modify the namespace to your own or - if you're sure there will be no conflicts - remove it altogether

	// There are 2 important callbacks you need to implement: CreateViewsHolder() and UpdateViewsHolder()
	// See explanations below
	public class CardListView : SRIA<MyListParams, MyListItemViewsHolder>
	{
		#region SRIA implementation
		protected override void Start()
		{
			// Calling this initializes internal data and prepares the adapter to handle item count changes
			//base.Start();

			// Retrieve the models from your data source and set the items count
			/*
			RetrieveDataAndUpdate(500);
			*/
		}

		// This is called initially, as many times as needed to fill the viewport, 
		// and anytime the viewport's size grows, thus allowing more items to be displayed
		// Here you create the "ViewsHolder" instance whose views will be re-used
		// *For the method's full description check the base implementation
		protected override MyListItemViewsHolder CreateViewsHolder(int itemIndex)
		{
			var instance = new MyListItemViewsHolder();

			// Using this shortcut spares you from:
			// - instantiating the prefab yourself
			// - enabling the instance game object
			// - setting its index 
			// - calling its CollectViews()
			instance.Init(_Params.itemPrefab, itemIndex);

			return instance;
		}

		// This is called anytime a previously invisible item become visible, or after it's created, 
		// or when anything that requires a refresh happens
		// Here you bind the data from the model to the item's views
		// *For the method's full description check the base implementation
		protected override void UpdateViewsHolder(MyListItemViewsHolder newOrRecycled)
		{
		// In this callback, "newOrRecycled.ItemIndex" is guaranteed to always reflect the
		// index of item that should be represented by this views holder. You'll use this index
		// to retrieve the model from your data set
		/*
		MyListItemModel model = _Params.Data[newOrRecycled.ItemIndex];

		newOrRecycled.backgroundImage.color = model.color;
		newOrRecycled.titleText.text = model.title + " #" + newOrRecycled.ItemIndex;
		*/

		// Initialize the views from the associated model
		MyListItemModel model = _Params.Data[newOrRecycled.ItemIndex];

		newOrRecycled.UpdateFromModel(model, _Params);
	}

		// This is the best place to clear an item's views in order to prepare it from being recycled, but this is not always needed, 
		// especially if the views' values are being overwritten anyway. Instead, this can be used to, for example, cancel an image 
		// download request, if it's still in progress when the item goes out of the viewport.
		// <newItemIndex> will be non-negative if this item will be recycled as opposed to just being disabled
		// *For the method's full description check the base implementation
		/*
		protected override void OnBeforeRecycleOrDisableViewsHolder(MyListItemViewsHolder inRecycleBinOrVisible, int newItemIndex)
		{

		}
		*/
		#endregion

		// These are common data manipulation methods
		// The list containing the models is managed by you. The adapter only manages the items' sizes and the count
		// The adapter needs to be notified of any change that occurs in the data list. Methods for each
		// case are provided: Refresh, ResetItems, InsertItems, RemoveItems
		#region data manipulation
		public void AddItemsAt(int index, params MyListItemModel[] models)
		{
			_Params.Data.InsertRange(index, models);
			InsertItems(index, models.Length);
		}

		public void RemoveItemFrom(int index, int count)
		{
			_Params.Data.RemoveRange(index, count);
			RemoveItems(index, count);
		}

		public void SetItems(IEnumerable<MyListItemModel> items)
		{
			_Params.Data.Clear();
			_Params.Data.AddRange(items);
			ResetItems(_Params.Data.Count);
		}
		#endregion


		// Here, we're requesting <count> items from the data source
		void RetrieveDataAndUpdate(int count)
		{
			StartCoroutine(FetchItemsFromDataSourceAndUpdate(count));
		}

		// Retrieving <count> models from the data source and calling OnDataRetrieved after.
		// In a real case scenario, you'd query your server, your database or whatever is your data source and call OnDataRetrieved after
		IEnumerator FetchItemsFromDataSourceAndUpdate(int count)
		{
			// Simulating data retrieving delay
			yield return new WaitForSeconds(.5f);

			// Retrieve your data here
			/*
			for (int i = 0; i < count; ++i)
			{
				var model = new MyListItemModel()
				{
					title = "Random item ",
					color = Utils.GetRandomColor()
				};
				_Params.Data.Add(model);
			}
			*/

			OnDataRetrieved();
		}

		void OnDataRetrieved()
		{
			ResetItems(_Params.Data.Count);
		}
	}

	// Class containing the data associated with an item
	public class MyListItemModel
	{
		/*
		public string title;
		public Color color;
		*/
		public Card card;
	}


	// BaseParamsWithPrefabAndData<TModel> is the most commonly used type of parameters for ListViews.
	// It exposes a prefab property and the data is stored in a System.Collections.Generic.List<T>
	// Alternatives are: 
	// - BaseParams (used for more advanced setups, where you need control both over the prefab - maybe you have more than one prefab - and the data list)
	// - BaseParamsWithPrefab (when you have your own way of storing the data)
	// - BaseParamsWithPrefabAndLazyData (similar to the one used here, but it uses a LazyList<T>, as opposed to a simple System.Collections.Generic.List<T>)
	// - GridParams (used for grids. It exposes the cell prefab and some grid-specific parameters)
	// Should be marked as Serializable, so it can be shown in inspector
	[Serializable] 
	public class MyListParams : BaseParamsWithPrefabAndData<MyListItemModel>
	{ }


	// This class keeps references to an item's views.
	// Your views holder should extend BaseItemViewsHolder for ListViews and CellViewsHolder for GridViews
	public class MyListItemViewsHolder : BaseItemViewsHolder
	{
		/*
		public Text titleText;
		public Image backgroundImage;
		*/
		Image img;


		// Retrieving the views from the item's root GameObject
		public override void CollectViews()
		{
			base.CollectViews();

		// GetComponentAtPath is a handy extension method from frame8.Logic.Misc.Other.Extensions
		// which infers the variable's component from its type, so you won't need to specify it yourself
		/*
		root.GetComponentAtPath("TitleText", out titleText);
		root.GetComponentAtPath("BackgroundImage", out backgroundImage);
		*/

		root.GetComponentAtPath("Image", out img);
	}

	public void UpdateFromModel(MyListItemModel model, MyListParams parameters)
	{
		img.sprite = model.card.m_image.sprite;
	}

	// Override this if you have children layout groups. They need to be marked for rebuild when this callback is fired
	/*
	public override void MarkForRebuild()
	{
		base.MarkForRebuild();

		LayoutRebuilder.MarkLayoutForRebuild(yourChildLayout1);
		LayoutRebuilder.MarkLayoutForRebuild(yourChildLayout2);
	}
	*/
}
