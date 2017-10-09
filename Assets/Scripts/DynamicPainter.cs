using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicPainter : MonoBehaviour
{
    public Camera mainCamera;
    public Camera RTCamera;

    public GameObject brushCursor;
    public GameObject brushContainer;
    public GameObject brushStrokeImage;

    public RenderTexture canvasTexture;
    public Material baseMaterial;

    private bool isSavingTexture = false;

    // brush properties...
    public float brushSize = 1.0f;
    public Color brushColor = new Color(1,0,0,1);
    private int brushCounter = 0;
    private const int MAX_BRUSH_LIMIT = 1000;

	// Use this for initialization
	void Start ()
    {
		
	}

    /// <summary>
    /// Find out UV coordinate of ray hit position on mesh!
    /// </summary>
    /// <param name="uvHitPosition"></param>
    /// <returns></returns>
    bool ObjectHitUVPosition(ref Vector3 uvHitPosition)
    {
        RaycastHit hit;

        Vector3 cursorPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.0f);
        Ray cursorRay = mainCamera.ScreenPointToRay(cursorPos);

        if(Physics.Raycast(cursorRay, out hit, 200))
        {
            MeshCollider meshCollider = hit.collider as MeshCollider;
            if(meshCollider == null || meshCollider.sharedMesh == null)
                return false;
                
            Vector2 pixelUV = new Vector2(hit.textureCoord.x, hit.textureCoord.y);
            uvHitPosition.x = pixelUV.x - RTCamera.orthographicSize;
            uvHitPosition.y = pixelUV.y - RTCamera.orthographicSize;
            uvHitPosition.z = 0.0f;
            return true;
        }
        else
        {
            return false;
        }
    }

    private void Paint()
    {
        // If we are saving texture, then bail out!
        if (isSavingTexture)
            return;

        // Get brush position
        Vector3 brushPosition = Vector3.zero;
        if(ObjectHitUVPosition(ref brushPosition))
        {
            // Instantiate brush object 
            GameObject brushObject;
            brushObject = GameObject.Instantiate(brushStrokeImage);
            // set it's color, parent, position & scale
            brushObject.GetComponent<SpriteRenderer>().color = brushColor;
            brushObject.transform.parent = brushContainer.transform;
            brushObject.layer = brushContainer.layer;
            brushObject.transform.localPosition = brushPosition;
            brushObject.transform.localScale = Vector3.one * brushSize;
        }

        // increment brush counter, if more than limit, save/update the texture!
        ++brushCounter;
        if(brushCounter >= MAX_BRUSH_LIMIT)
        {
            isSavingTexture = true;
            Invoke("SaveTexture", 0.1f);
        }
    }

    private void SaveTexture()
    {
        // reset brush counter
        brushCounter = 0;

        // hide brush cursor while saving the texture
        brushCursor.SetActive(false);

        // Set canvas texture as active render texture
        RenderTexture.active = canvasTexture;
        Texture2D tex = new Texture2D(canvasTexture.width, canvasTexture.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, canvasTexture.width, canvasTexture.height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;

        // assign texture2d as main texture to base material
        baseMaterial.mainTexture = tex;

        // Remove all child elements within brush container
        foreach(Transform child in brushContainer.transform)
        {
            Destroy(child.gameObject);
        }

        Invoke("ShowCursor", 0.1f);
    }

    private void ShowCursor()
    {
        isSavingTexture = false;
    }

    private void ResetRenderTexture()
    {

    }

    private void UpdateBrushCursor()
    {
        Vector3 uvPosition = Vector3.zero;
        if(ObjectHitUVPosition(ref uvPosition) && !isSavingTexture)
        {
            brushCursor.SetActive(true);
            brushCursor.transform.position = uvPosition + brushContainer.transform.position;    
        }
        else
        {
            brushCursor.SetActive(false);
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
		if(Input.GetMouseButton(0))
        {
            Paint();
        }

        UpdateBrushCursor();
	}
}
