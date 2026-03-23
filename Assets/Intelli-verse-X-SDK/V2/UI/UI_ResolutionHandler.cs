// File: Assets/_QuizVerse/Scripts/UI/UI_ResolutionHandler.cs
// CREATE THIS FILE - Based on terminal-rush implementation

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Trivia.UI
{
    public class UI_ResolutionHandler : MonoBehaviour
    {
        public List<CanvasScaler> canvasScalers;
        public float currentDeviceResolution;

        [Header("UI Items")]
        [SerializeField] private List<UIPanelAndScale> panelsToReScale = new List<UIPanelAndScale>();

        private void Awake()
        {
            SetResolution();
            SetAllCanvasScaler();
        }

        private void Start()
        {
            if (currentDeviceResolution != (Screen.height / (float)Screen.width))
            {
                SetResolution();
                SetAllCanvasScaler();
            }

            if (currentDeviceResolution >= 2.4f)
            {
                SetPanelScaleForLongDevices(); // iPhone 14 Pro Max, Pixel 7 Pro
            }
            else if (currentDeviceResolution <= 1.5f)
            {
                SetPanelScaleForTablets(); // iPad, Samsung Tab
            }
        }

        private void SetPanelScaleForLongDevices()
        {
            foreach (var panel in panelsToReScale)
            {
                if (panel.panel != null)
                {
                    panel.panel.localScale = panel.longScale;
                }
            }
        }

        private void SetPanelScaleForTablets()
        {
            foreach (var panel in panelsToReScale)
            {
                if (panel.panel != null)
                {
                    panel.panel.localScale = panel.tabletScale;
                }
            }
        }

        private void SetAllCanvasScaler()
        {
            foreach (var scaler in canvasScalers)
            {
                if (scaler != null)
                {
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1080, 1920);
                    scaler.matchWidthOrHeight = 0.5f;
                }
            }
        }

        private void SetResolution()
        {
            currentDeviceResolution = Screen.height / (float)Screen.width;
            Debug.Log($"[UI_ResolutionHandler] Device resolution ratio: {currentDeviceResolution}");
        }
    }

    [Serializable]
    public struct UIPanelAndScale
    {
        public string panelName;
        public RectTransform panel;
        public Vector3 longScale;  // For aspect ratio >= 2.4
        public Vector3 tabletScale; // For aspect ratio <= 1.5
    }
}