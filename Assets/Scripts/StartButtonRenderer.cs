using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class StartButtonRenderer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    // RIV 檔案
    public Rive.Asset Asset;

    // 按鈕點擊事件 (當按鈕被點擊時觸發)
    public UnityEvent OnClick;

    // Rive 專案、工作區域、狀態機
    private Rive.File _file;
    private Rive.Artboard _artboard;
    private Rive.StateMachine _stateMachine;

    // Rive 動畫控制參數
    private Rive.SMIBool _isPointerHover;
    private Rive.SMIBool _isPointerDown;

    // Rive 透過 RenderTexture 渲染到 RawImage
    private Rive.RenderQueue _renderQueue;
    private Rive.Renderer _renderer;
    private CommandBuffer _commandBuffer;
    private RenderTexture _renderTexture;
    private RawImage _rawImage;

    private void Start()
    {
        // FPS 鎖定為 60
        Application.targetFrameRate = 60;

        // Rive 取得工作區域與狀態機
        _file = Rive.File.Load(Asset);
        _artboard = _file.Artboard(0);
        _stateMachine = _artboard.StateMachine();

        // Rive 取得動畫控制參數
        _isPointerHover = _stateMachine.GetBool("IsPointerHover");
        _isPointerDown = _stateMachine.GetBool("IsPointerDown");

        // Rive 根據工作區域尺寸建立 RenderTexture (DirectX 11 要求 enableRandomWrite = true)
        _renderTexture = new RenderTexture(Mathf.FloorToInt(_artboard.Width), Mathf.FloorToInt(_artboard.Height), 32);
        _renderTexture.enableRandomWrite = true;

        // Rive 使用主相機來渲染
        _renderQueue = new Rive.RenderQueue(_renderTexture);
        _renderer = _renderQueue.Renderer();
        _renderer.Draw(_artboard);
        _commandBuffer = _renderer.ToCommandBuffer();
        _commandBuffer.SetRenderTarget(_renderTexture);
        _commandBuffer.ClearRenderTarget(clearDepth: true,
                                         clearColor: true,
                                         backgroundColor: Color.clear,
                                         depth: 0);
        _renderer.AddToCommandBuffer(_commandBuffer);
        Camera.main.AddCommandBuffer(CameraEvent.AfterEverything, _commandBuffer);

        // RawImage 取得 RenderTexture 並上下翻轉 (DirectX 11 坐標系影響)
        _rawImage = GetComponent<RawImage>();
        _rawImage.texture = _renderTexture;
        _rawImage.transform.localScale = new Vector3(1, -1, 1);
    }

    private void Update()
    {
        // Rive 更新狀態機時間
        _stateMachine.Advance(Time.deltaTime);
    }

    private void OnDisable()
    {
        // 釋放渲染資源
        if (Camera.main != null && _commandBuffer != null)
            Camera.main.RemoveCommandBuffer(CameraEvent.AfterEverything, _commandBuffer);
    }

    #region UI 滑鼠事件 (同步 Rive 的 Listeners 設定)
    public void OnPointerEnter(PointerEventData _)
    {
        _isPointerHover.Value = true;
    }

    public void OnPointerExit(PointerEventData _)
    {
        _isPointerHover.Value = false;
        _isPointerDown.Value = false;
    }

    public void OnPointerDown(PointerEventData _)
    {
        _isPointerDown.Value = true;

        // 當按下按鈕時觸發點擊事件
        OnClick.Invoke();
    }

    public void OnPointerUp(PointerEventData _)
    {
        _isPointerDown.Value = false;
    }
    #endregion
}
