using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BedtimeCore.EditorHistory
{
	[Serializable]
	internal struct HistoryObject
	{
		[SerializeField]
		private Object _selection;
		
		[SerializeField]
		private string _scenePath;

		[SerializeField]
		private string _scene;
		
		[SerializeField]
		private string _name;

		[SerializeField]
		private GUIContent _guiContent;

		[SerializeField]
		private bool _isSceneObject;

		public HistoryObject(Object selection)
		{
			_name = selection.name;
			_selection = selection;
			_scenePath = null;
			_scene = null;
			_guiContent	= new GUIContent(_name, AssetPreview.GetMiniThumbnail(selection));
			if (selection is GameObject go && !AssetDatabase.Contains(selection))
			{
				_scenePath = GetObjectPath(go);
				_scene = go.scene.name;
			}

			_isSceneObject = _scenePath != null;
		}

		private static string GetObjectPath(Object obj)
		{
			if (obj is GameObject go)
			{
				Transform transform = go.transform;
				var root = transform.root;
				if (transform == root)
				{
					return transform.name;
				}
				return $"{root.name}/{AnimationUtility.CalculateTransformPath(transform, root)}";
			}

			return null;
		}

		public HistoryObject UpdateName()
		{
			if (!_isSceneObject || !Exists)
			{
				return this;
			}

			_name = Selection.name;
			_scenePath = GetObjectPath(Selection);
			return this;
		}

		public HistoryObject UpdateSelection(string scene)
		{
			if (!_isSceneObject || Exists || scene != _scene)
			{
				return this;
			}

			_selection = GameObject.Find(ScenePath);
			UpdateName();
			return this;
		}
		
		public bool Exists => Selection != null;

		public string Name
		{
			get
			{
				if (Exists)
				{
					return Selection.name;
				}

				return !string.IsNullOrEmpty(_scene) ? $"[{_scene}] {_name}" : _name;
			}
		}

		public string Scene => _scene ?? string.Empty;

		public Object Selection => _selection;

		public GUIContent GUIContent
		{
			get
			{
				_guiContent.text = Name;
				return _guiContent;
			}
		}

		public bool IsSceneObject => _isSceneObject;

		public string ScenePath => _scenePath;
	}
}