using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TexturePaint : MonoBehaviour
{

    // ======================================================================================================================
    // PARAMETERS -----------------------------------------------------------------------------------------------
    public Texture baseTexture;                  // used to deterimne the dimensions of the runtime texture
    public Material meshMaterial;                 // used to bind the runtime texture as the albedo of the mesh
    public GameObject meshGameobject;
    public Shader UVShader;                     // the shader usedto draw in the texture of the mesh
    public Mesh meshToDraw;
    public Shader ilsandMarkerShader;
    public Shader fixIlsandEdgesShader;
    public static Vector3 mouseWorldPosition;
    public Camera mainCamera;
    // --------------------------------
    private RenderTexture markedIlsandes;
    private CommandBuffer cb_markingIlsdands;
    private int numberOfFrames;

    // ---------------------------------
    private PaintableTexture albedo;
    // ======================================================================================================================
    // INITIALIZE -------------------------------------------------------------------
    void Start()
    {
        Vector4 mwp = new Vector4(100, 100, 100, 100);
        Shader.SetGlobalVector("_Mouse", mwp);

        // Texture and Mat initalization ---------------------------------------------
        markedIlsandes = new RenderTexture(baseTexture.width, baseTexture.height, 0, RenderTextureFormat.R8);
        albedo = new PaintableTexture(Color.white, baseTexture.width, baseTexture.height, "_MainTex", UVShader, meshToDraw, fixIlsandEdgesShader, markedIlsandes);


        meshMaterial.SetTexture(albedo.id, albedo.runTimeTexture);

        // Command buffer inialzation ------------------------------------------------

        cb_markingIlsdands = new CommandBuffer();
        cb_markingIlsdands.name = "markingIlsnads";


        cb_markingIlsdands.SetRenderTarget(markedIlsandes);
        Material mIlsandMarker = new Material(ilsandMarkerShader);
        cb_markingIlsdands.DrawMesh(meshToDraw, Matrix4x4.identity, mIlsandMarker);
        mainCamera.AddCommandBuffer(CameraEvent.AfterEverything, cb_markingIlsdands);


        albedo.SetActiveTexture(mainCamera);

        Shader.SetGlobalFloat("_BrushOpacity", 1);
        Shader.SetGlobalColor("_BrushColor", Color.red);
        Shader.SetGlobalFloat("_BrushSize", .13f);
        Shader.SetGlobalFloat("_BrushHardness", .1f);

        mainCamera.depthTextureMode = DepthTextureMode.Depth;

        albedo.UpdateShaderParameters(meshGameobject.transform.localToWorldMatrix);
    }
    // ======================================================================================================================
    // LOOP ---------------------------------------------------------------------------

    private void Update()
    {
        if (numberOfFrames == 3) mainCamera.RemoveCommandBuffer(CameraEvent.AfterEverything, cb_markingIlsdands);

        numberOfFrames++;


        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Vector4 mwp = Vector3.positiveInfinity;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject.tag != "PaintObject")
                return;
            else
                mwp = hit.point;


            mwp.w = Input.GetMouseButton(0) ? 1 : 0;

            mouseWorldPosition = mwp;
            Shader.SetGlobalVector("_Mouse", mwp);

        }
    }


    [System.Serializable]
    public class PaintableTexture
    {
        public string id;
        public RenderTexture runTimeTexture;
        public RenderTexture paintedTexture;

        public CommandBuffer cb;

        private Material mPaintInUV;
        private Material mFixedEdges;
        public RenderTexture fixedIlsands;

        public PaintableTexture(Color clearColor, int width, int height, string id,
            Shader sPaintInUV, Mesh mToDraw, Shader fixIlsandEdgesShader, RenderTexture markedIlsandes)
        {
            this.id = id;

            runTimeTexture = new RenderTexture(width, height, 0)
            {
                anisoLevel = 0,
                useMipMap = false,
                filterMode = FilterMode.Bilinear
            };

            paintedTexture = new RenderTexture(width, height, 0)
            {
                anisoLevel = 0,
                useMipMap = false,
                filterMode = FilterMode.Bilinear
            };


            fixedIlsands = new RenderTexture(paintedTexture.descriptor);

            Graphics.SetRenderTarget(runTimeTexture);
            GL.Clear(false, true, clearColor);
            Graphics.SetRenderTarget(paintedTexture);
            GL.Clear(false, true, clearColor);


            mPaintInUV = new Material(sPaintInUV);
            if (!mPaintInUV.SetPass(0)) Debug.LogError("Invalid Shader Pass: ");
            mPaintInUV.SetTexture("_MainTex", paintedTexture);

            mFixedEdges = new Material(fixIlsandEdgesShader);
            mFixedEdges.SetTexture("_IlsandMap", markedIlsandes);
            mFixedEdges.SetTexture("_MainTex", paintedTexture);

            // ----------------------------------------------

            cb = new CommandBuffer();
            cb.name = "TexturePainting" + id;


            cb.SetRenderTarget(runTimeTexture,0);
            cb.DrawMesh(mToDraw, Matrix4x4.identity, mPaintInUV);

            cb.Blit(runTimeTexture, fixedIlsands, mFixedEdges);
            cb.Blit(fixedIlsands, runTimeTexture);
            cb.Blit(runTimeTexture, paintedTexture);



        }

        public void SetActiveTexture(Camera mainC)
        {
            mainC.AddCommandBuffer(CameraEvent.AfterEverything, cb);
        }

        public void UpdateShaderParameters(Matrix4x4 localToWorld)
        {
            mPaintInUV.SetMatrix("mesh_Object2World", localToWorld); // Mus be updated every time the mesh moves, and also at start
        }
    }
}

