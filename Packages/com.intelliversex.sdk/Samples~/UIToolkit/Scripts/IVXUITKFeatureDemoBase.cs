using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace IntelliVerseX.Samples.UIToolkit
{
    /// <summary>
    /// Helpers for feature demos that share IVXFeatureShell.uxml.
    /// </summary>
    public abstract class IVXUITKFeatureDemoBase : IVXUITKDemoShell
    {
        protected Label TitleLabel { get; private set; }
        protected Label SubtitleLabel { get; private set; }
        protected Label ResultLabel { get; private set; }
        protected VisualElement FieldsHost { get; private set; }
        protected VisualElement ActionsHost { get; private set; }
        protected VisualElement MainPanel { get; private set; }

        protected override void OnUIReady()
        {
            TitleLabel = Q<Label>("titleLabel");
            SubtitleLabel = Q<Label>("subtitleLabel");
            ResultLabel = Q<Label>("resultLabel");
            FieldsHost = Q<VisualElement>("fieldsHost");
            ActionsHost = Q<VisualElement>("actionsHost");
            MainPanel = Q<VisualElement>("mainPanel");

            FieldsHost?.Clear();
            ActionsHost?.Clear();
            if (ResultLabel != null)
                ResultLabel.text = string.Empty;

            BuildFeatureUI();
            ShowPanel(MainPanel);
            StartCoroutine(PlayFeatureEnterMotion());
        }

        protected abstract void BuildFeatureUI();

        protected void SetTitle(string title, string subtitle = null)
        {
            if (TitleLabel != null)
                TitleLabel.text = title ?? string.Empty;
            if (SubtitleLabel != null)
                SubtitleLabel.text = subtitle ?? string.Empty;
        }

        protected void SetResult(string text)
        {
            if (ResultLabel == null)
                return;

            ResultLabel.text = text ?? string.Empty;
            ResultLabel.RemoveFromClassList("ivx-result--flash");
            ResultLabel.schedule.Execute(() => ResultLabel.AddToClassList("ivx-result--flash")).StartingIn(16);
            ResultLabel.schedule.Execute(() => ResultLabel.RemoveFromClassList("ivx-result--flash")).StartingIn(420);
        }

        protected TextField AddField(string name, string label, string value = "", bool password = false)
        {
            var wrap = new VisualElement();
            wrap.AddToClassList("ivx-field");
            wrap.AddToClassList("ivx-stagger-item");
            wrap.Add(new Label(label));
            var field = new TextField { name = name, value = value ?? string.Empty };
            field.isPasswordField = password;
            wrap.Add(field);
            FieldsHost?.Add(wrap);
            return field;
        }

        protected Button AddAction(string label, Action onClick, string extraClass = null)
        {
            var button = new Button(() => onClick?.Invoke()) { text = label };
            button.AddToClassList("ivx-btn");
            button.AddToClassList("ivx-stagger-item");
            if (!string.IsNullOrEmpty(extraClass))
                button.AddToClassList(extraClass);
            ActionsHost?.Add(button);
            return button;
        }

        private IEnumerator PlayFeatureEnterMotion()
        {
            var host = Q<VisualElement>("featureContent") ?? MainPanel;
            yield return StaggerEnter(host, "ivx-stagger-item", 0.035f);
        }
    }
}
