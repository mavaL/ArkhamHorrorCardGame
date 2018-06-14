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

namespace frame8.ScrollRectItemsAdapter.HierarchyExample
{
    /// <summary>Demonstrating a hierarchy view (aka Tree View) implemented with SRIA</summary>
    public class HierarchyExample : SRIA<MyParams, PageViewsHolder>
	{
		bool _BusyWithAnimation;


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
			var bpanel = DrawerCommandPanel.Instance.AddButtonsPanel("Collapse All", "ExpandAll");
			bpanel.button1.onClick.AddListener(OnCollapseAll);
			bpanel.button2.onClick.AddListener(OnExpandAll);

			// Initially set the number of items to the number in the input field
			DrawerCommandPanel.Instance.RequestChangeItemCountToSpecified();
		}

		/// <inheritdoc/>
		protected override PageViewsHolder CreateViewsHolder(int itemIndex)
		{
			var instance = new PageViewsHolder();
			instance.Init(_Params.itemPrefab, itemIndex);
			instance.foldoutButton.onClick.AddListener(() => OnDirectoryFoldOutClicked(instance));

			return instance;
		}

		/// <inheritdoc/>
		protected override void UpdateViewsHolder(PageViewsHolder newOrRecycled)
		{
			// Initialize the views from the associated model
			FSEntryNodeModel model = _Params.flattenedVisibleHierarchy[newOrRecycled.ItemIndex];
			newOrRecycled.UpdateViews(model);
		}
		#endregion

		#region events from DrawerCommandPanel
		void OnItemCountChangeRequested(int newCount)
		{
			if (_BusyWithAnimation)
				return;

			int itemIndex = 0;

			_Params.hierarchyRootNode = CreateRandomNodeModel(ref itemIndex, 0, true, 7);
			_Params.flattenedVisibleHierarchy = new List<FSEntryNodeModel>(_Params.hierarchyRootNode.children);
			ResetItems(_Params.flattenedVisibleHierarchy.Count);
		}
		void OnCollapseAll()
		{
			if (_BusyWithAnimation)
				return;

			for (int i = 0; i < _Params.flattenedVisibleHierarchy.Count;)
			{
				var m = _Params.flattenedVisibleHierarchy[i];
				if (m.depth > 1)
				{
					m.expanded = false;
					_Params.flattenedVisibleHierarchy.RemoveAt(i);
				}
				else
					++i;
			}
			ResetItems(_Params.flattenedVisibleHierarchy.Count);
		}
		void OnExpandAll()
		{
			if (_BusyWithAnimation)
				return;

			_Params.flattenedVisibleHierarchy = _Params.hierarchyRootNode.GetFlattenedHierarchyAndExpandAll();
			ResetItems(_Params.flattenedVisibleHierarchy.Count);
		}
		#endregion

		//protected override void OnScrollViewSizeChanged()
		//{
		//	_BusyWithAnimation = false; // cancel any animation

		//	base.OnScrollViewSizeChanged();
		//}

		void OnDirectoryFoldOutClicked(PageViewsHolder vh)
		{
			if (_BusyWithAnimation)
				return;

			var model = _Params.flattenedVisibleHierarchy[vh.ItemIndex];
			int nextIndex = vh.ItemIndex + 1;
			bool wasExpanded = model.expanded;
			model.expanded = !wasExpanded;
			if (wasExpanded)
			{
				// Remove all following models with bigger depth, until a model with a less than- or equal depth is found
				int i = vh.ItemIndex + 1;
				int count = _Params.flattenedVisibleHierarchy.Count;
				for (; i < count;)
				{
					var m = _Params.flattenedVisibleHierarchy[i];
					if (m.depth > model.depth)
					{
						m.expanded = false;
						++i;
						continue;
					}

					break; // found with depth less than- or equal to the collapsed item
				}

				int countToRemove = i - nextIndex;
				if (countToRemove > 0)
				{
					if (_Params.animatedFoldOut)
						GradualRemove(nextIndex, countToRemove);
					else
					{
						_Params.flattenedVisibleHierarchy.RemoveRange(nextIndex, countToRemove);
						RemoveItems(nextIndex, countToRemove);
					}
				}
			}
			else
			{
				if (model.children.Length > 0)
				{
					if (_Params.animatedFoldOut)
						GradualAdd(nextIndex, model.children);
					else
					{
						_Params.flattenedVisibleHierarchy.InsertRange(nextIndex, model.children);
						InsertItems(nextIndex, model.children.Length);
					}
				}
			}
		}

		void GradualAdd(int index, FSEntryNodeModel[] children) { StartCoroutine(GradualAddOrRemove(index, children.Length, children)); }

		void GradualRemove(int index, int countToRemove) { StartCoroutine(GradualAddOrRemove(index, countToRemove, null)); }

		IEnumerator GradualAddOrRemove(int index, int count, FSEntryNodeModel[] childrenIfAdd)
		{
			_BusyWithAnimation = true;
			int curIndexInChildren = 0;
			int remainingLen = count;
			int divider = Mathf.Min(7, count);
			int maxChunkSize = count / divider;
			int curChunkSize;
			float toWait = .01f;
			var toWaitYieldInstr = new WaitForSeconds(toWait);

			if (childrenIfAdd == null)
			{
				index = index + count - 1;
				while (remainingLen > 0)
				{
					curChunkSize = Math.Min(remainingLen, maxChunkSize);

					int curStartIndex = index - curChunkSize + 1;
					for (int i = index; i >= curStartIndex; --i, --index)
						_Params.flattenedVisibleHierarchy.RemoveAt(i);
					RemoveItems(curStartIndex, curChunkSize);
					remainingLen -= curChunkSize;

					yield return toWaitYieldInstr;
				}
			}
			else
			{
				while (remainingLen > 0)
				{
					curChunkSize = Math.Min(remainingLen, maxChunkSize);

					int curStartIndex = index;
					for (int i = 0; i < curChunkSize; ++i, ++index, ++curIndexInChildren)
						_Params.flattenedVisibleHierarchy.Insert(index, childrenIfAdd[curIndexInChildren]);

					InsertItems(curStartIndex, curChunkSize);
					remainingLen -= curChunkSize;

					yield return toWaitYieldInstr;
				}
			}
			_BusyWithAnimation = false;
		}

		FSEntryNodeModel CreateRandomNodeModel(ref int itemIndex, int depth, bool forceDirirectory, int numChildren)
		{
			if (forceDirirectory || depth + 1 < _Params.maxHierarchyDepth && UnityEngine.Random.Range(0, 2) == 0)
			{
				var m = CreateNewModel(ref itemIndex, depth, true);
				m.children = new FSEntryNodeModel[numChildren];
				bool depth1 = depth == 1;
				for (int i = 0; i < numChildren; ++i)
					m.children[i] = CreateRandomNodeModel(ref itemIndex, depth + 1, depth1, UnityEngine.Random.Range(1, 7));

				return m;
			}

			return CreateNewModel(ref itemIndex, depth, false);
		}

		FSEntryNodeModel CreateNewModel(ref int itemIdex, int depth, bool isDirectory)
		{
			return new FSEntryNodeModel()
			{
				title = (isDirectory ? "Directory " : "File ") + (itemIdex++),
				depth = depth
			};
		}
	}


	// This in almost all cases will contain the prefab and your list of models
	[Serializable] // serializable, so it can be shown in inspector
	public class MyParams : Logic.Misc.Visual.UI.ScrollRectItemsAdapter.BaseParamsWithPrefab
	{
		[Range(0, 10)]
		public int maxHierarchyDepth;
		public bool animatedFoldOut = true;
		public FSEntryNodeModel hierarchyRootNode;
		public List<FSEntryNodeModel> flattenedVisibleHierarchy; // doesn't include descendants of non-expanded folders
	}


	public class FSEntryNodeModel
	{
		public FSEntryNodeModel[] children;
		public int depth;
		public string title;
		public bool expanded; // only needed for directories

		public bool IsDirectory { get { return children != null; } }


		public List<FSEntryNodeModel> GetFlattenedHierarchyAndExpandAll()
		{
			var res = new List<FSEntryNodeModel>();
			for (int i = 0; i < children.Length; i++)
			{
				var c = children[i];
				res.Add(c);
				c.expanded = true;
				if (c.IsDirectory)
				{
					res.AddRange(c.GetFlattenedHierarchyAndExpandAll());
				}
			}

			return res;
		}
	}


	public class PageViewsHolder : BaseItemViewsHolder
	{
		public Text titleText;
		public Image foldoutArrowImage;
		public Button foldoutButton;
		Image _FileIconImage, _DirectoryIconImage;
		RectTransform _PanelRT;
		HorizontalLayoutGroup _RootLayoutGroup;


		/// <inheritdoc/>
		public override void CollectViews()
		{
			base.CollectViews();

			_RootLayoutGroup = root.GetComponent<HorizontalLayoutGroup>();
			_PanelRT = root.GetChild(0) as RectTransform;
			_PanelRT.GetComponentAtPath("TitleText", out titleText);
			_PanelRT.GetComponentAtPath("FoldOutButton", out foldoutButton);
			_PanelRT.GetComponentAtPath("DirectoryIconImage", out _DirectoryIconImage);
			_PanelRT.GetComponentAtPath("FileIconImage", out _FileIconImage);
			foldoutButton.transform.GetComponentAtPath("FoldOutArrowImage", out foldoutArrowImage);
		}

		public override void MarkForRebuild()
		{
			LayoutRebuilder.MarkLayoutForRebuild(_PanelRT);
			LayoutRebuilder.MarkLayoutForRebuild(root);
			base.MarkForRebuild();
		}

		public void UpdateViews(FSEntryNodeModel model)
		{
			titleText.text = model.title;
			bool isDir = model.IsDirectory;
			foldoutButton.interactable = isDir;
			_DirectoryIconImage.gameObject.SetActive(isDir);
			_FileIconImage.gameObject.SetActive(!isDir);
			foldoutArrowImage.gameObject.SetActive(isDir);
			if (isDir)
				foldoutArrowImage.rectTransform.localRotation = Quaternion.Euler(0, 0, model.expanded ? -90 : 0);

			_RootLayoutGroup.padding.left = 25 * model.depth;
		}
	}
}
