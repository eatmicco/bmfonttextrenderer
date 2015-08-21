// ---- AngelCode BmFont Unity Renderer ----------------------------------------------
// ---- By eatmicco @ eatmicco@hotmail.com -------------------------------------------
// ---- Using BMFont XML Serializer by DeadlyDan and BMFont by AngleCode--------------
// ---- Credits to deadlydan@gmail.com and http://www.angelcode.com/ -----------------

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BmFont;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode()]
public class BMFontText : MonoBehaviour {

	public enum PivotPosition {
		TOP_LEFT,
		TOP_CENTER,
		TOP_RIGHT,
		MIDDLE_LEFT,
		MIDDLE_CENTER,
		MIDDLE_RIGHT,
		BOTTOM_LEFT,
		BOTTOM_CENTER,
		BOTTOM_RIGHT
	}

	private class SpriteChar {
		public Rect rect;
		public FontChar fontChar;
	}

	public TextAsset fontConfig;
	public Material[] fontMaterials;
	public PivotPosition pivotPosition = PivotPosition.TOP_LEFT;
	public float textScale = 1.0f;
    public Color topColor;
    public Color bottomColor;
    public bool isUnicode = false;

    [Multiline()]
    public string text;

    private FontFile _fontFile;
	private Dictionary<int, SpriteChar> _charMap;
	private float _devider;
	private Vector2 _textureSize;
	private List<Vector3> _vertexList;
	private List<Vector2> _uvList;
	private List<Vector3> _normalList;
    private List<Color> _colorList;
    private List<List<int>> _indexLists;
    private PivotPosition _lastPivot = PivotPosition.MIDDLE_CENTER;
    private Color _lastColor;

	private void CreatePlaneMesh(Vector2 offset, Rect newTexCoord, Vector2 texSize, float scale, int page) {	
		float xTemp = newTexCoord.width / _devider;
		float yTemp = newTexCoord.height / _devider;
		
		Vector3[] newVertices = {
			new Vector3(0.0f, 0.0f, 0.0f),
			new Vector3(xTemp * scale, 0.0f, 0.0f),
			new Vector3(xTemp * scale, -(yTemp * scale), 0.0f),
			new Vector3(0.0f, -(yTemp * scale), 0.0f)
		};
		
		float offsetXTemp = offset.x / _devider;
		float offsetYTemp = offset.y / _devider;
		for (int i = 0; i < 4; ++i) {
			newVertices[i].x -= offsetXTemp * scale;
			newVertices[i].y += offsetYTemp * scale;
		}

		/*The y position seems to be always upside down, I don't know why*/
		newTexCoord.y = (int)(_textureSize.y - newTexCoord.height) - (int)newTexCoord.y;
		
		Vector2[] newUV = {
			new Vector2(newTexCoord.x / _textureSize.x, (newTexCoord.y + newTexCoord.height) / _textureSize.y),/*new Vector2(0.0f, 1.0f),*/
			new Vector2((newTexCoord.x + newTexCoord.width) / _textureSize.x, (newTexCoord.y + newTexCoord.height) / _textureSize.y),/*new Vector2(1.0f, 1.0f),*/
			new Vector2((newTexCoord.x + newTexCoord.width) / _textureSize.x, newTexCoord.y / _textureSize.y),/*new Vector2(1.0f, 0.0f),*/
			new Vector2(newTexCoord.x / _textureSize.x, newTexCoord.y / _textureSize.y)/*new Vector2(0.0f, 0.0f),*/
		};
		
		Vector3[] newNormals = {
			new Vector3(0.0f, 0.0f, -1.0f),
			new Vector3(0.0f, 0.0f, -1.0f),
			new Vector3(0.0f, 0.0f, -1.0f),
			new Vector3(0.0f, 0.0f, -1.0f)
		};
		
		int[] newIndices = {
			0, 1, 3,
			3, 1, 2
		};

        Color[] newColors = {
            topColor, topColor,
            bottomColor, bottomColor
        };

		int lastVertexIndex = _vertexList.Count;
		for (int i = 0; i < newVertices.Length; ++i) {
			_vertexList.Add(newVertices[i]);
			_uvList.Add(newUV[i]);
			_normalList.Add(newNormals[i]);
            _colorList.Add(newColors[i]);
		}

        Debug.Log("_indexLists.Count " + _indexLists.Count);
		for (int i = 0; i < newIndices.Length; ++i) {
			_indexLists[page].Add(newIndices[i] + lastVertexIndex);
		}
	}

	private Vector2 GetOffset(PivotPosition pivot, Bounds bounds) {
		switch (pivot) {
		case PivotPosition.TOP_LEFT:
			return new Vector2(0, 0);
		case PivotPosition.TOP_CENTER:
			return new Vector2((bounds.max.x-bounds.min.x)/2, 0);
		case PivotPosition.TOP_RIGHT:
			return new Vector2(bounds.max.x - bounds.min.x, 0);
		case PivotPosition.MIDDLE_LEFT:
			return new Vector2(0, (bounds.max.y-bounds.min.y)/2);
		case PivotPosition.MIDDLE_CENTER:
			return new Vector2((bounds.max.x-bounds.min.x)/2, (bounds.max.y-bounds.min.y)/2);
		case PivotPosition.MIDDLE_RIGHT:
			return new Vector2(bounds.max.x-bounds.min.x, (bounds.max.y-bounds.min.y)/2);
		case PivotPosition.BOTTOM_LEFT:
			return new Vector2(0, bounds.max.y-bounds.min.y);
		case PivotPosition.BOTTOM_CENTER:
			return new Vector2((bounds.max.x-bounds.min.x)/2, bounds.max.y-bounds.min.y);
		case PivotPosition.BOTTOM_RIGHT:
			return new Vector2(bounds.max.x-bounds.min.x, bounds.max.y-bounds.min.y);
		}

		return Vector2.zero;
	}
	
	public void UpdatePivot() {

		Mesh mesh;
		if (Application.isPlaying) {
			mesh = gameObject.GetComponent<MeshFilter>().mesh;
		} else {
			mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
		}

        if (mesh == null) return;
		Vector2 offset = GetOffset(pivotPosition, mesh.bounds);
		Vector2 lastOffset = GetOffset(_lastPivot, mesh.bounds);

		Vector3[] vertices = mesh.vertices;

		Vector2 trueOffset = offset - lastOffset;
		for (int i = 0; i < vertices.Length; ++i) {
			vertices[i].x -= trueOffset.x;
			vertices[i].y += trueOffset.y;
		}

		mesh.vertices = vertices;
        _lastPivot = pivotPosition;
	}


	/* Call this function after every changes of text variable in your code
	 * example :
	 * 
	 * bmfText.text = "Hello World!";
	 * bmfText.Commit();
	 */
	public void Commit() {
        if (_indexLists == null) return;

		_vertexList.Clear();
		_uvList.Clear();
		_normalList.Clear();
        _colorList.Clear();
        for (int i = 0; i < _indexLists.Count; ++i)
            _indexLists[i].Clear();

        Debug.Log(short.MaxValue);

        _lastPivot = PivotPosition.TOP_LEFT;    //return the pivot to TOP_LEFT

        if (text == null || text.Equals("")) return;

        if (!isUnicode)
        {
            _devider = _textureSize.x > _textureSize.y ? _textureSize.x : _textureSize.y;
            Vector2 d = new Vector2(0, 0);
            int lineHeight = 0;
            char lastChar = ' ';
            foreach (char c in text)
            {
                Debug.Log((int)c);
                if (c != 32 && c != 13 && c != 10)
                {
                    if (lineHeight < _charMap[c].fontChar.YOffset + _charMap[c].fontChar.Height) lineHeight = _charMap[c].fontChar.YOffset + _charMap[c].fontChar.Height;
                    Vector2 offset = new Vector2(d.x - _charMap[c].fontChar.XOffset, d.y - _charMap[c].fontChar.YOffset);
                    CreatePlaneMesh(offset, _charMap[c].rect, _textureSize, textScale, _charMap[c].fontChar.Page);
                }
                else if (c == 13)
                {
                    d.y -= lineHeight;
                    lastChar = c;
                    continue;
                }
                else if (c == 10)
                {
                    if (lastChar != 13) d.y -= lineHeight;
                    d.x = 0;
                    lastChar = c;
                    continue;
                }
                d.x -= _charMap[c].fontChar.XAdvance;
                lastChar = c;
            }
        }
        else
        {
            Encoding utf32 = Encoding.UTF32;
            _devider = _textureSize.x > _textureSize.y ? _textureSize.x : _textureSize.y;
            Vector2 d = new Vector2(0, 0);
            int lineHeight = 0;
            char lastChar = ' ';
            char[] textChar = text.ToCharArray();
            for (int i = 0; i < textChar.Length; ++i)
            {
                char c = textChar[i];
                Debug.Log(c);
                byte[] b = utf32.GetBytes(textChar, i, 1);
                int u = BitConverter.ToInt32(b, 0);
                Debug.Log(u);
                if (c != 32 && c != 13 && c != 10)
                {
                    if (lineHeight < _charMap[u].fontChar.YOffset + _charMap[u].fontChar.Height) lineHeight = _charMap[u].fontChar.YOffset + _charMap[u].fontChar.Height;
                    Vector2 offset = new Vector2(d.x - _charMap[u].fontChar.XOffset, d.y - _charMap[u].fontChar.YOffset);
                    CreatePlaneMesh(offset, _charMap[u].rect, _textureSize, textScale, _charMap[u].fontChar.Page);
                }
                else if (c == 13)
                {
                    d.y -= lineHeight;
                    lastChar = c;
                    continue;
                }
                else if (c == 10)
                {
                    if (lastChar != 13) d.y -= lineHeight;
                    d.x = 0;
                    lastChar = c;
                    continue;
                }
                d.x -= _charMap[u].fontChar.XAdvance;
                lastChar = c;
            }
        }

		Mesh newMesh = new Mesh();
		newMesh.vertices = _vertexList.ToArray();
		newMesh.uv = _uvList.ToArray();
		newMesh.normals = _normalList.ToArray();
        newMesh.colors = _colorList.ToArray();
        List<int[]> subMeshIndices = new List<int[]>();
        List<Material> includedMaterials = new List<Material>();
        for (int i = 0; i < _indexLists.Count; ++i)
        {
            if (_indexLists[i].Count > 0)
            {
                subMeshIndices.Add(_indexLists[i].ToArray());
                includedMaterials.Add(fontMaterials[i]);
            }
        }
        newMesh.subMeshCount = subMeshIndices.Count;
        for (int i = 0; i < subMeshIndices.Count; ++i)
        {
            newMesh.SetTriangles(subMeshIndices[i], i);
        }
        newMesh.RecalculateBounds();

		gameObject.GetComponent<MeshFilter>().mesh = newMesh;
        gameObject.GetComponent<MeshRenderer>().materials = includedMaterials.ToArray();

		UpdatePivot();
	}

    public void Initialize()
    {
        _vertexList = new List<Vector3>();
        _uvList = new List<Vector2>();
        _normalList = new List<Vector3>();
        _colorList = new List<Color>();
    }

    public void InitializeFont()
    {
        if (fontConfig != null && fontMaterials.Length > 0) {
			_fontFile = FontLoader.LoadFromString(fontConfig.text);
            if (_fontFile.Pages.Count != fontMaterials.Length)
            {
                Debug.LogError("Materials count don't match!");
                return;
            }

            _indexLists = new List<List<int>>();
            for (int i = 0; i < _fontFile.Pages.Count; ++i)
                _indexLists.Add(new List<int>());

			_textureSize = new Vector2(_fontFile.Common.ScaleW, _fontFile.Common.ScaleH);
			_charMap = new Dictionary<int, SpriteChar>();
			for (int i = 0; i < _fontFile.Chars.Count; ++i) {
				SpriteChar sc = new SpriteChar();
				Rect rect = new Rect(_fontFile.Chars[i].X, _fontFile.Chars[i].Y,  _fontFile.Chars[i].Width, _fontFile.Chars[i].Height);
				sc.fontChar = _fontFile.Chars[i];
				sc.rect = rect;
				_charMap.Add(_fontFile.Chars[i].ID, sc);
			}

            Debug.Log("Font Initialized");
        }
    }
	
	// Use this for initialization
	void Start () {
        Initialize();

        InitializeFont();

        Commit();
	}
	
	// Update is called once per frame
	void Update () {
	}
}
