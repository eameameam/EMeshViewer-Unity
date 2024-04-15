using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class EMeshViewer : EditorWindow
{
    private GameObject _selectedObject;
    private Dictionary<int, bool> _foldoutStates = new Dictionary<int, bool>();
    private MeshRenderer[] _meshRenderers;
    private Texture2D[] _previews;
    private Vector2 _scrollPosition;
    private Vector2 _hierarchyScrollPosition;
    private GameObject _selectedChild;

    [MenuItem("Escripts/EMeshViewer")]
    public static void ShowWindow()
    {
        GetWindow<EMeshViewer>("EMesh Viewer");
    }
    
private void OnGUI()
{
    EditorGUILayout.BeginHorizontal();

    DrawLeftPanel();

    DrawRightPanel();

    EditorGUILayout.EndHorizontal();
}

private void DrawLeftPanel()
{
    EditorGUILayout.BeginVertical(GUILayout.Width(200));
    
    EditorGUILayout.Space(10);
    GUILayout.Label("EMeshViewer", EditorStyles.largeLabel);
    
    EditorGUILayout.Space(5);

    GUILayout.Label("Choose GameObject below.", EditorStyles.miniLabel);
    EditorGUILayout.Space(10);

    _selectedObject = EditorGUILayout.ObjectField("", _selectedObject, typeof(GameObject), true) as GameObject;
    
    EditorGUILayout.Space(10);

    if (GUILayout.Button("Find Meshes") && _selectedObject != null)
    {
        _meshRenderers = _selectedObject.GetComponentsInChildren<MeshRenderer>(true);
        _previews = new Texture2D[_meshRenderers.Length];
    }

    if (_meshRenderers != null && _meshRenderers.Length > 0)
    {
        if (GUILayout.Button("Hide All"))
        {
            HideAllMeshRenderers();
        }
    }
    
    EditorGUILayout.Space(20);
    
    if (_selectedObject != null)
    {
        GUILayout.Label("Hierarchy", EditorStyles.helpBox);

        _hierarchyScrollPosition = GUILayout.BeginScrollView(_hierarchyScrollPosition, GUILayout.ExpandHeight(true));
        DrawHierarchyFoldouts(_selectedObject.transform, true);
        GUILayout.EndScrollView();
    }
    
    EditorGUILayout.EndVertical();
    GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.Width(4));
}

private void DrawRightPanel()
{
    UpdatePreviews();
    EditorGUILayout.BeginVertical();
    if (_meshRenderers != null)
    {
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true));
        
        int numColumns = Mathf.Max(1, (int)(position.width - 200) / 110);
        int numRows = Mathf.CeilToInt((float)_meshRenderers.Length / numColumns);

        for (int row = 0; row < numRows; row++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int col = 0; col < numColumns; col++)
            {
                int index = row * numColumns + col;
                if (index < _meshRenderers.Length)
                {
                    DrawPreview(index);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
    }
    EditorGUILayout.EndVertical();
}

private void DrawPreview(int index)
{
    var meshRenderer = _meshRenderers[index];
    if (_previews[index] == null && meshRenderer != null)
    {
        _previews[index] = AssetPreview.GetAssetPreview(meshRenderer.gameObject);
        if (_previews[index] == null)
        {
            Repaint();
        }
    }

    if (_previews[index] != null)
    {
        if (GUILayout.Button(_previews[index], GUILayout.Width(100), GUILayout.Height(100)))
        {
            meshRenderer.enabled = !meshRenderer.enabled;
            SceneView.RepaintAll();
        }
    }
    else
    {
        GUILayout.Label("Loading preview...", GUILayout.Width(100), GUILayout.Height(100));
    }
}

    private void DrawHierarchyFoldouts(Transform root, bool isRoot)
    {
        int instanceID = root.gameObject.GetInstanceID();

        bool shouldDisplay = root.GetComponent<MeshRenderer>() != null || AnyChildHasMeshRenderer(root);
        if (!shouldDisplay)
        {
            return;
        }

        if (!_foldoutStates.ContainsKey(instanceID))
        {
            _foldoutStates[instanceID] = false; 
        }

        bool currentState = _foldoutStates[instanceID];
        _foldoutStates[instanceID] = EditorGUILayout.Foldout(currentState, root.name, true);

        if (_foldoutStates[instanceID] != currentState && _foldoutStates[instanceID])
        {
            SelectChild(root.gameObject);
        }

        if (_foldoutStates[instanceID])
        {
            EditorGUI.indentLevel++;
            foreach (Transform child in root)
            {
                DrawHierarchyFoldouts(child, false);
            }
            EditorGUI.indentLevel--;
        }
    }

    private void SelectChild(GameObject child)
    {
        _selectedChild = child;
        _meshRenderers = _selectedChild.GetComponentsInChildren<MeshRenderer>(true);
        _previews = new Texture2D[_meshRenderers.Length];
        Repaint();
    }

    private bool AnyChildHasMeshRenderer(Transform root)
    {
        foreach (Transform child in root)
        {
            if (child.GetComponent<MeshRenderer>() != null)
                return true;
            if (AnyChildHasMeshRenderer(child))
                return true;
        }
        return false;
    }
    
    private void UpdatePreviews()
    {
        for (int i = 0; i < _meshRenderers.Length; i++)
        {
            var meshRenderer = _meshRenderers[i];
            if (meshRenderer != null && _previews[i] == null)
            {
                Vector3 originalPosition = meshRenderer.transform.position;
                Quaternion originalRotation = meshRenderer.transform.rotation;
                Vector3 originalScale = meshRenderer.transform.localScale;

                meshRenderer.transform.position = Vector3.zero;
                meshRenderer.transform.rotation = Quaternion.identity;
                meshRenderer.transform.localScale = Vector3.one;

                _previews[i] = AssetPreview.GetAssetPreview(meshRenderer.gameObject);

                meshRenderer.transform.position = originalPosition;
                meshRenderer.transform.rotation = originalRotation;
                meshRenderer.transform.localScale = originalScale;

                if (_previews[i] == null)
                {
                    Repaint();
                }
            }
        }
    }
    
    private void HideAllMeshRenderers()
    {
        foreach (var meshRenderer in _meshRenderers)
        {
            if (meshRenderer != null)
            {
                meshRenderer.enabled = false;
            }
        }
        SceneView.RepaintAll();
    }
}