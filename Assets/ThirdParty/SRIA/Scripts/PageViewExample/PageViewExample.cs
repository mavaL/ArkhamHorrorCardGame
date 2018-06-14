using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.Logic.Misc.Other.Extensions;
using frame8.ScrollRectItemsAdapter.Util;
using frame8.Logic.Misc.Visual.UI.MonoBehaviours;
using frame8.ScrollRectItemsAdapter.Util.Drawer;
using frame8.ScrollRectItemsAdapter.Util.PullToRefresh;
using frame8.Logic.Misc.Visual.UI;

namespace frame8.ScrollRectItemsAdapter.PageViewExample
{
    /// <summary>
	/// Demonstrating a Page View which also allows for transitioning to the next/prev page when the drag speed exceeds a certain value, 
	/// not only when the current page is more than half outside. This is mainly thanks to <see cref="Snapper8.minSpeedToAllowSnapToNext"/>
	/// </summary>
    public class PageViewExample : SRIA<MyParams, PageViewsHolder>
	{

		#region SRIA implementation
		/// <inheritdoc/>
		protected override void Start()
		{
			base.Start();

			DrawerCommandPanel.Instance.Init(this, true, false, false, false, false);
			DrawerCommandPanel.Instance.galleryEffectSetting.slider.value = 0f;
			DrawerCommandPanel.Instance.simulateLowEndDeviceSetting.gameObject.SetActive(false);
			DrawerCommandPanel.Instance.galleryEffectSetting.gameObject.SetActive(false);
			DrawerCommandPanel.Instance.ItemCountChangeRequested += OnItemCountChangeRequested;

			GetComponentInChildren<DiscreteScrollbar>().getItemsCountFunc = () => _Params.Data.Count;

			// Initially set the number of items to the number in the input field
			DrawerCommandPanel.Instance.RequestChangeItemCountToSpecified();
		}

		/// <inheritdoc/>
		protected override PageViewsHolder CreateViewsHolder(int itemIndex)
		{
			var instance = new PageViewsHolder();
			instance.Init(_Params.itemPrefab, itemIndex);

			return instance;
		}

		/// <inheritdoc/>
		protected override void UpdateViewsHolder(PageViewsHolder newOrRecycled)
		{
			// Initialize the views from the associated model
			PageModel model = _Params.Data[newOrRecycled.ItemIndex];
			newOrRecycled.UpdateViews(model);
		}
		#endregion

		public void ScrollToPage(int index)
		{
			SmoothScrollTo(index, .7f, .5f, .5f);
		}

		#region events from DrawerCommandPanel
		void OnItemCountChangeRequested(int newCount)
		{
			_Params.Data.Clear();
			for (int i = 0; i < newCount; i++)
				_Params.Data.Add(CreateNewModel(i, C.GetRandomTextBody(180), UnityEngine.Random.Range(0, _Params.availableImages.Length)));
			ResetItems(_Params.Data.Count);
		}
		#endregion


		PageModel CreateNewModel(int itemIdex, string body, int iconIndex)
		{
			return new PageModel()
			{
				title = "Page " + itemIdex,
				body = body,
				image = _Params.availableImages[iconIndex]
			};
		}
	}


	// This in almost all cases will contain the prefab and your list of models
	[Serializable] // serializable, so it can be shown in inspector
	public class MyParams : BaseParamsWithPrefabAndData<PageModel>
	{
		public Sprite[] availableImages; // used to randomly generate models;
	}


	public class PageModel
	{
		public string title, body;
		public Sprite image;
	}


	public class PageViewsHolder : BaseItemViewsHolder
	{
		public Text titleText, bodyText;
		public Image image;


		/// <inheritdoc/>
		public override void CollectViews()
		{
			base.CollectViews();

			root.GetComponentAtPath("TitlePanel/TitleText", out titleText);
			root.GetComponentAtPath("BodyPanel/BodyText", out bodyText);
			root.GetComponentAtPath("BackgroundMask/BackgroundImage", out image);
		}

		public void UpdateViews(PageModel model)
		{
			titleText.text = model.title;
			bodyText.text = model.body;
			image.sprite = model.image;
		}
	}
}
