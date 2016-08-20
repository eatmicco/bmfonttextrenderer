using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(BMFontText))]
[CanEditMultipleObjects()]
public class BMFontTextEditor : Editor
{

    [MenuItem("GameObject/Create Other/BMFont Text")]
    private static void CreateBMFontText()
    {
        GameObject bmFontGO = new GameObject("BMFont Text");
        bmFontGO.AddComponent<BMFontText>();
    }

    private BMFontText _target;
    private SerializedProperty _fontConfigProp;
    private SerializedProperty _fontMaterialProp;
    private SerializedProperty _pivotPositionProp;
    private SerializedProperty _textScaleProp;
    private SerializedProperty _topColorProp;
    private SerializedProperty _bottomColorProp;
    private SerializedProperty _isUnicodeProp;
    private SerializedProperty _textProp;

    private TextAsset _oldConfig;
    private int _oldMaterialCount;
    private BMFontText.PivotPosition _oldPivotPosition;
    private float _oldTextScale;
    private Color _oldTopColor;
    private Color _oldBottomColor;
    private string _oldText;

    void OnEnable()
    {
        //_target = (BMFontText)target;
        _target = (BMFontText)serializedObject.targetObject;
        _fontConfigProp = serializedObject.FindProperty("fontConfig");
        _pivotPositionProp = serializedObject.FindProperty("pivotPosition");
        _textScaleProp = serializedObject.FindProperty("textScale");
        _topColorProp = serializedObject.FindProperty("topColor");
        _bottomColorProp = serializedObject.FindProperty("bottomColor");
        _isUnicodeProp = serializedObject.FindProperty("isUnicode");
        _textProp = serializedObject.FindProperty("text");

        _oldConfig = (TextAsset)_fontConfigProp.objectReferenceValue;
        _oldMaterialCount = 0;
        _oldPivotPosition = (BMFontText.PivotPosition)_pivotPositionProp.enumValueIndex;
        _oldTextScale = _textScaleProp.floatValue;
        _oldTopColor = _topColorProp.colorValue;
        _oldBottomColor = _bottomColorProp.colorValue;
        _oldText = _textProp.stringValue;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_fontConfigProp, new GUIContent("Font Config"));
        
        //materials
        EditorGUILayout.LabelField("Font Materials");
        int matCount = serializedObject.FindProperty("fontMaterials.Array.size").intValue;
        EditorGUI.indentLevel = 3;
        int c = EditorGUILayout.IntField("Size", matCount);
        if (c != matCount)
        {
            serializedObject.FindProperty("fontMaterials.Array.size").intValue = c;
            matCount = c;
        }

        for (int i = 0; i < matCount; ++i)
        {
            var prop = serializedObject.FindProperty(string.Format("fontMaterials.Array.data[{0}]", i));
            EditorGUILayout.PropertyField(prop);
        }

        EditorGUI.indentLevel = 0;
        EditorGUILayout.PropertyField(_pivotPositionProp, new GUIContent("Pivot"));
        EditorGUILayout.LabelField("Color");
        EditorGUILayout.PropertyField(_topColorProp, new GUIContent("Top"));
        EditorGUILayout.PropertyField(_bottomColorProp, new GUIContent("Bottom"));
        EditorGUILayout.PropertyField(_isUnicodeProp, new GUIContent("Is Unicode"));

        EditorGUILayout.PropertyField(_textProp, new GUIContent("Text"));

        serializedObject.ApplyModifiedProperties();

        if (_oldConfig != (TextAsset)_fontConfigProp.objectReferenceValue || (_oldMaterialCount != matCount && serializedObject.FindProperty("fontMaterials.Array.data[0]").objectReferenceValue != null))
        {
            Debug.Log("Initialize");
            _oldConfig = (TextAsset)_fontConfigProp.objectReferenceValue;
            _oldMaterialCount = matCount;
            _target.Initialize();
            _target.InitializeFont();
        }

        if (_oldPivotPosition != (BMFontText.PivotPosition)_pivotPositionProp.enumValueIndex)
        {
            _oldPivotPosition = (BMFontText.PivotPosition)_pivotPositionProp.enumValueIndex;
            _target.UpdatePivot();
        }

        if (_oldTextScale != _textScaleProp.floatValue ||
             _oldTopColor != _topColorProp.colorValue ||
             _oldBottomColor != _bottomColorProp.colorValue ||
             _oldText != _textProp.stringValue)
        {
            Debug.Log(_oldText + ", " + _textProp.stringValue);
            _oldTextScale = _textScaleProp.floatValue;
            _oldTopColor = _topColorProp.colorValue;
            _oldBottomColor = _bottomColorProp.colorValue;
            _oldText = _textProp.stringValue;
            _target.Commit();
        }
    }
}
