using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BedtimeCore.EditorHistory
{
	[Serializable]
    public struct HistoryObject
	{
		[SerializeField]
		private object _selection;
		
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

		public HistoryObject(object selection)
		{
            var unityObj = selection as Object;
            _name = selection.ToString();

			_selection = selection;
			_scenePath = null;
			_scene = null;
            _guiContent = new GUIContent(_name);
            if (unityObj != null)
            {
                _name = unityObj.name;
                _guiContent	= new GUIContent(_name, AssetPreview.GetMiniThumbnail(unityObj));
                if (unityObj is GameObject go && !AssetDatabase.Contains(unityObj))
                {
                    _scenePath = GetObjectPath(go);
                    _scene = go.scene.name;
                }
            }
			_isSceneObject = _scenePath != null;
		}

		private static string GetObjectPath(object obj)
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

        private string GetName()
        {
            if(_selection is Object unityObj)
            {
                return unityObj.name;
            }
            return _selection.ToString();
        }

		public HistoryObject UpdateName()
		{
			if (!_isSceneObject || !Exists)
			{
				return this;
			}

			_name = GetName();
            
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
					return GetName();
				}

				return !string.IsNullOrEmpty(_scene) ? $"[{_scene}] {_name}" : _name;
			}
		}

		public string Scene => _scene ?? string.Empty;

		public object Selection => _selection;

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