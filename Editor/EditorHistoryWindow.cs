using UnityEngine;
using UnityEditor;
using UnityToolbarExtender;

namespace BedtimeCore.EditorHistory
{
	internal class EditorHistoryWindow : EditorWindow
	{
		[SerializeField]
		private Vector2 _scroll;
		private bool _clickedHistoryEntry;
		private bool _isModal;
		
		private GUIStyle _scrollbarStyle;
		private GUIStyle _listStyle;
		private GUIStyle _navStyle;
		private GUIContent _popoutButtonContent;

		private const int ENTRY_HEIGHT = 16;
		private const int TOOLBAR_BUTTON_SPACE = 2;
		private const string TOOLBAR_BUTTON_ICON = "d_UnityEditor.AnimationWindow";
		private const string POPOUT_ICON = "d_ScaleTool On";
		private const string TOOLBAR_BUTTON_TITLE = "Selection History";

		[InitializeOnLoadMethod]
		static void InitializeOnLoad() => ToolbarExtender.RightToolbarGUI.Add(DrawToolbarButton);

		private static void DrawToolbarButton()
		{
			var icon = EditorGUIUtility.IconContent(TOOLBAR_BUTTON_ICON);
			GUIContent buttonText = new GUIContent(icon.image, TOOLBAR_BUTTON_TITLE);
			GUIStyle buttonStyle = EditorStyles.toolbarButton;
			
			GUILayout.FlexibleSpace();
			var rect = GUILayoutUtility.GetRect(buttonText, buttonStyle, GUILayout.Width(32));
			var clicked = GUI.Button(rect, buttonText, buttonStyle);
			
			if(clicked)
			{
				ShowModalWindow(GUIUtility.GUIToScreenRect(rect));
			}
			
			GUILayout.Space(TOOLBAR_BUTTON_SPACE);
		}

		private static void ShowModalWindow(Rect ownerButtonRect)
		{
			var window = CreateInstance<EditorHistoryWindow>();
			var size = new Vector2(200, 300);
			var pos = ownerButtonRect;
			pos.x -= size.x - pos.width - TOOLBAR_BUTTON_SPACE;
			window._scroll = Vector2.up * int.MaxValue;
			window._isModal = true;
			window.ShowAsDropDown(pos, size);
		}
		
		private void ShowPopoutWindow()
		{
			var window = CreateInstance<EditorHistoryWindow>();
			var rect = position;
			rect.size = rect.size * 1.2f;
			window.Show();
			window.position = rect;
		}
		
		private void OnEnable()
		{
			_listStyle = null;
			_popoutButtonContent = EditorGUIUtility.TrTextContentWithIcon(string.Empty, "Pop Out", POPOUT_ICON);
			titleContent = EditorGUIUtility.TrTextContentWithIcon(TOOLBAR_BUTTON_TITLE, TOOLBAR_BUTTON_ICON);
			EditorHistory.OnHistoryUpdated += OnHistoryChanged;
			Selection.selectionChanged += Repaint;
		}

		private void OnDisable()
		{
			EditorHistory.OnHistoryUpdated -= OnHistoryChanged;
			Selection.selectionChanged -= Repaint;
		}

		private void OnGUI()
		{
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			DrawModalToolbar();
			DrawList();
			EditorGUILayout.EndVertical();
		}

		private void DrawModalToolbar()
		{
			if (!_isModal)
			{
				return;
			}
			
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			GUILayout.Label(TOOLBAR_BUTTON_TITLE, EditorStyles.boldLabel);
			GUILayout.FlexibleSpace();
			if (GUILayout.Button(_popoutButtonContent, EditorStyles.toolbarButton))
			{
				ShowPopoutWindow();
			}
			EditorGUILayout.EndHorizontal();
		}

		private void OnHistoryChanged(int location)
		{
			if (!_clickedHistoryEntry)
			{
				_scroll.y = (location * ENTRY_HEIGHT) - ENTRY_HEIGHT;
			}
			Repaint();
		}

		private void DrawList()
		{
			var selectedColor = new Color(.6f, .6f, .6f, 1f);
			var orgBGColor = GUI.backgroundColor;

			_scroll = GUILayout.BeginScrollView(_scroll, GUIStyle.none, GUI.skin.verticalScrollbar);
			EditorGUILayout.BeginVertical();
			for (int i = 0; i < EditorHistory.HistoryObjects.Count; i++)
			{
				var entry = EditorHistory.HistoryObjects[i];
				if (!entry.Exists)
				{
					GUI.enabled = false;
				}

				if (i == EditorHistory.Location)
				{
					GUI.backgroundColor = selectedColor;
				}
				
				if (GUILayout.Button(entry.GUIContent, ListStyle))
				{
					_clickedHistoryEntry = true;
					EditorHistory.SetSelection(i);
					_clickedHistoryEntry = false;
				}
				GUI.backgroundColor = orgBGColor;
				GUI.enabled = true;
			}
			EditorGUILayout.EndVertical();
			GUILayout.EndScrollView();
		}

		private GUIStyle ListStyle
		{
			get
			{
				if (_listStyle != null)
				{
					return _listStyle;
				}

				_listStyle = new GUIStyle(EditorStyles.toolbarButton)
				{
					alignment = TextAnchor.MiddleLeft, 
					fixedHeight = ENTRY_HEIGHT,
				};
				return _listStyle;
			}
		}
	}
}