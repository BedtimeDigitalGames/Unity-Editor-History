using UnityEngine;
using UnityEditor;
using System;
using BedtimeCore.Reflection;

namespace BedtimeCore.Editor
{
	public class EditorHistoryWindow : SearchableEditorWindow
	{
		[SerializeField]
		private Vector2 scroll;
		private Texture lockedIcon;
		private Texture unlockedIcon;
		private const int entryHeight = 16;
		private bool clickedHistoryEntry;

		private GUIStyle lockStyle;
		private GUIStyle listStyle;
		private GUIStyle navStyle;

		private string SearchField
		{
			get
			{
				return this.GetValue<string>("m_SearchFilter");
			}
			set
			{
				this.SetValue("m_SearchFilter", value);
			}
		}

		private SearchMode Mode
		{
			get
			{
				return this.GetValue<SearchMode>("m_SearchMode");
			}
			set
			{
				this.SetValue("m_SearchMode", value);
			}
		}

		[MenuItem("BedtimeCore/Editor History Window")]
		public static void GetWindow()
		{
			EditorHistoryWindow window = EditorWindow.CreateInstance<EditorHistoryWindow>();
			window.Show();
		}

		public override void OnEnable()
		{
			lockedIcon = EditorGUIUtility.IconContent("IN LockButton on act").image;
			unlockedIcon = EditorGUIUtility.IconContent("IN LockButton act").image;

			base.OnEnable();
			SetTitle();
			EditorHistory.OnHistoryUpdated += HandleHistoryChanged;
			Selection.selectionChanged += HandleSelectionChanged;
		}

		private void SetTitle()
		{
			titleContent = typeof(EditorGUIUtility).InvokeMethod<GUIContent, string, string>("TextContentWithIcon", "History", "d_CustomSorting");
		}

		private void OnGUI()
		{
			DrawToolbar();
			DrawList();
			DrawBottomNavigation();
		}

		private void HandleSelectionChanged()
		{
			Repaint();
		}

		private void HandleHistoryChanged(int location)
		{
			if (!clickedHistoryEntry)
			{
				scroll.y = (location * entryHeight) - entryHeight;
			}
			Repaint();
		}

		private void DrawToolbar()
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

			LockStyle.active = EditorHistory.Locked ? EditorStyles.toolbarButton.normal : EditorStyles.toolbarButton.active;
			LockStyle.normal = !EditorHistory.Locked ? EditorStyles.toolbarButton.normal : EditorStyles.toolbarButton.active;
			var lockObj = new GUIContent(EditorHistory.Locked ? lockedIcon : unlockedIcon);

			if (GUILayout.Button(lockObj, LockStyle, GUILayout.Width(32)))
			{
				EditorHistory.Locked = !EditorHistory.Locked;
			}

			if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
			{
				this.InvokeVoid("ClearSearchFilter");
				EditorHistory.Clear();
			}
			EditorGUILayout.Space();
			this.InvokeVoid("SearchFieldGUI");

			EditorGUILayout.EndHorizontal();
		}

		private void DrawList()
		{
			var selectedColor = new Color(.6f, .6f, .6f, 1f);
			var orgBGColor = GUI.backgroundColor;

			scroll = GUILayout.BeginScrollView(scroll, GUIStyle.none, GUI.skin.GetStyle("VerticalScrollbar"));
			EditorGUILayout.BeginVertical();
			for (int i = 0; i < EditorHistory.History.Count; i++)
			{
				var entry = EditorHistory.History[i];
				if (!entry.Exists || !MatchesSearch(entry))
				{
					continue;
				}

				if (i == EditorHistory.Location && EditorHistory.HistoryActive)
				{
					GUI.backgroundColor = selectedColor;
				}

				var icon = AssetPreview.GetMiniThumbnail(entry.selection);

				GUIContent info = new GUIContent(entry.selection.name, icon);
				if (GUILayout.Button(info, ListStyle))
				{
					clickedHistoryEntry = true;
					EditorHistory.SetSelection(i);
					clickedHistoryEntry = false;
				}
				GUI.backgroundColor = orgBGColor;
			}
			EditorGUILayout.EndVertical();
			GUILayout.EndScrollView();
		}

		private void DrawBottomNavigation()
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUI.BeginDisabledGroup(EditorHistory.Location == 0 && EditorHistory.HistoryActive);
				if (GUILayout.Button("◀", NavStyle))
				{
					EditorHistory.Navigate(EditorHistory.NavigationDirection.Backward);
				}
				EditorGUI.EndDisabledGroup();
				EditorGUI.BeginDisabledGroup(EditorHistory.Location == EditorHistory.History.Count - 1 && EditorHistory.HistoryActive);
				if (GUILayout.Button("▶", NavStyle))
				{
					EditorHistory.Navigate(EditorHistory.NavigationDirection.Forward);
				}
				EditorGUI.EndDisabledGroup();
			}
		}

		private bool MatchesSearch(EditorHistory.HistoryEntity entry)
		{
			if (!entry.Exists || string.IsNullOrEmpty(SearchField))
			{
				return true;
			}

			switch (Mode)
			{
				case SearchMode.All:
					return ContainsName(entry) || ContainsType(entry);
				case SearchMode.Name:
					return ContainsName(entry);
				case SearchMode.Type:
					return ContainsType(entry);
				default:
					break;
			}

			return false;
		}

		private bool ContainsName(EditorHistory.HistoryEntity entry)
		{
			return StringContains(entry.Name, SearchField);
		}

		private bool ContainsType(EditorHistory.HistoryEntity entry)
		{
			return StringContains(entry.Type.Name, SearchField);
		}

		private bool StringContains(string source, string toCompare)
		{
			return source.IndexOf(toCompare, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		private GUIStyle LockStyle
		{
			get
			{
				if (lockStyle == null || !lockStyle.name.Equals("toolbarbutton"))
				{
					lockStyle = new GUIStyle(EditorStyles.toolbarButton);
					lockStyle.alignment = TextAnchor.MiddleCenter;
					lockStyle.contentOffset = new Vector2(-2f, -2f);
				}
				return lockStyle;
			}
		}

		private GUIStyle ListStyle
		{
			get
			{
				if (listStyle == null || !listStyle.name.Equals("toolbarbutton"))
				{
					listStyle = new GUIStyle(EditorStyles.toolbarButton);
					listStyle.alignment = TextAnchor.MiddleLeft;
					listStyle.fixedHeight = entryHeight;
				}
				return listStyle;
			}
		}

		private GUIStyle NavStyle
		{
			get
			{
				if (navStyle == null || !navStyle.name.Equals("toolbarbutton"))
				{
					navStyle = new GUIStyle(EditorStyles.toolbarButton);
					navStyle.alignment = TextAnchor.MiddleCenter;
					navStyle.contentOffset = new Vector2(0, -2f);
				}
				return navStyle;
			}
		}
	}
}