using System;
using System.Collections.Generic;

namespace NoEACVisualEntityTracker
{
    public class XUiC_VisualEntityTrackerOptions : XUiController
    {
        private static readonly List<XUiC_VisualEntityTrackerOptions> LiveControllers = new List<XUiC_VisualEntityTrackerOptions>();

        private bool pendingRefresh;
        private bool buttonHandlersBound;
        private XUiC_SimpleButton btnVetOff;
        private XUiC_SimpleButton btnVetOn;
        private VisualEntityTrackerMode cachedMode = VisualEntityTrackerMode.On;

        public override void Init()
        {
            base.Init();
            RegisterInstance(this);
            EnsureButtonHandlers();
            pendingRefresh = true;
        }

        public override void OnOpen()
        {
            base.OnOpen();
            EnsureButtonHandlers();
            pendingRefresh = true;
        }

        public override void OnClose()
        {
            base.OnClose();
            RemoveButtonHandlers();
            UnregisterInstance(this);
        }

        public override void Update(float _dt)
        {
            base.Update(_dt);
            if (pendingRefresh)
            {
                pendingRefresh = false;
                RefreshModeCache();
                RefreshBindingsSelfAndChildren();
                ApplySelectedState();
            }
        }

        public override bool GetBindingValueInternal(ref string value, string bindingName)
        {
            if (string.IsNullOrEmpty(bindingName))
            {
                return base.GetBindingValueInternal(ref value, bindingName);
            }

            string normalized = bindingName.ToLowerInvariant();
            switch (normalized)
            {
                case "opt_row_2_visible":
                    value = "true";
                    return true;
                case "opt_row_2_label":
                    value = "Visual Entity Tracker";
                    return true;
                case "opt_row_2_value":
                    value = VisualEntityTrackerModeSettings.GetModeLabel(cachedMode);
                    return true;
                case "opt_row_2_offtext":
                case "opt_row_2_ontext":
                    value = string.Empty;
                    return true;
                case "opt_row_2_off_selected_visible":
                    value = cachedMode == VisualEntityTrackerMode.Off ? "true" : "false";
                    return true;
                case "opt_row_2_on_selected_visible":
                    value = cachedMode == VisualEntityTrackerMode.On ? "true" : "false";
                    return true;
                default:
                    return base.GetBindingValueInternal(ref value, bindingName);
            }
        }

        public static void MarkAllDirty()
        {
            for (int i = LiveControllers.Count - 1; i >= 0; i--)
            {
                XUiC_VisualEntityTrackerOptions controller = LiveControllers[i];
                if (controller == null)
                {
                    LiveControllers.RemoveAt(i);
                    continue;
                }

                controller.pendingRefresh = true;
                if (controller.ViewComponent != null && controller.ViewComponent.IsVisible)
                {
                    controller.RefreshModeCache();
                    controller.RefreshBindingsSelfAndChildren();
                    controller.ApplySelectedState();
                }
            }
        }

        private static void RegisterInstance(XUiC_VisualEntityTrackerOptions controller)
        {
            if (controller == null)
            {
                return;
            }

            if (!LiveControllers.Contains(controller))
            {
                LiveControllers.Add(controller);
            }
        }

        private static void UnregisterInstance(XUiC_VisualEntityTrackerOptions controller)
        {
            if (controller == null)
            {
                return;
            }

            LiveControllers.Remove(controller);
        }

        private void EnsureButtonHandlers()
        {
            if (buttonHandlersBound)
            {
                return;
            }

            btnVetOff = GetChildById("btnVetOff") as XUiC_SimpleButton;
            btnVetOn = GetChildById("btnVetOn") as XUiC_SimpleButton;

            bool anyBound = false;

            if (btnVetOff != null)
            {
                btnVetOff.OnPressed -= BtnVetOffOnPressed;
                btnVetOff.OnPressed += BtnVetOffOnPressed;
                anyBound = true;
            }

            if (btnVetOn != null)
            {
                btnVetOn.OnPressed -= BtnVetOnOnPressed;
                btnVetOn.OnPressed += BtnVetOnOnPressed;
                anyBound = true;
            }

            if (!anyBound)
            {
                Console.WriteLine("[NoEACVisualEntityTracker] No ESC tracker option buttons found for controller wiring.");
                return;
            }

            buttonHandlersBound = true;
            ApplySelectedState();
        }

        private void RemoveButtonHandlers()
        {
            if (!buttonHandlersBound)
            {
                return;
            }

            if (btnVetOff != null)
            {
                btnVetOff.OnPressed -= BtnVetOffOnPressed;
            }

            if (btnVetOn != null)
            {
                btnVetOn.OnPressed -= BtnVetOnOnPressed;
            }

            btnVetOff = null;
            btnVetOn = null;
            buttonHandlersBound = false;
        }

        private void BtnVetOffOnPressed(XUiController _sender, int _mouseButton)
        {
            SetModeAndRefresh(VisualEntityTrackerMode.Off);
        }

        private void BtnVetOnOnPressed(XUiController _sender, int _mouseButton)
        {
            SetModeAndRefresh(VisualEntityTrackerMode.On);
        }

        private void SetModeAndRefresh(VisualEntityTrackerMode mode)
        {
            bool changed = VisualEntityTrackerModeSettings.SetModeForLocalPlayer(mode);
            if (!changed)
            {
                EntityPlayer player = GameManager.Instance?.World?.GetPrimaryPlayer();
                if (player != null)
                {
                    changed = VisualEntityTrackerModeSettings.SetModeForEntityId(player.entityId, mode);
                }
            }

            if (!changed)
            {
                Console.WriteLine("[NoEACVisualEntityTracker] Failed to set local mode from ESC options.");
                return;
            }

            cachedMode = mode;
            VisualEntityTrackerService.RefreshTrackerMode();
            RefreshBindingsSelfAndChildren();
            ApplySelectedState();
            MarkAllDirty();
        }

        private void RefreshModeCache()
        {
            cachedMode = VisualEntityTrackerModeSettings.GetModeForLocalPlayer(VisualEntityTrackerMode.On);
        }

        private void ApplySelectedState()
        {
            SetButtonSelected(btnVetOff, cachedMode == VisualEntityTrackerMode.Off);
            SetButtonSelected(btnVetOn, cachedMode == VisualEntityTrackerMode.On);
        }

        private static void SetButtonSelected(XUiC_SimpleButton button, bool isSelected)
        {
            if (button?.Button != null)
            {
                button.Button.Selected = isSelected;
            }
        }
    }
}