using System;
using System.Collections.Generic;
using System.Text;
using Sandbox;
using Sandbox.Game.Entities;
using Sandbox.Game.GameSystems;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using SpaceEngineers.Game.ModAPI;
using VRage;
using VRage.Audio;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

namespace JumpSelector.Plugin
{
    public class JumpSelectorGui : MyGuiScreenBase
    {
        public JumpSelectorGui(MyJumpDrive block) : base(new Vector2?(new Vector2(0.5f, 0.5f)), new Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR * MySandboxGame.Config.UIBkOpacity), new Vector2?(new Vector2(0.55f, 0.4f)), false, null, 0f, 0f, null)
        {
            gpsList = new SortedList<string, IMyGps>();
            CanHideOthers = false;
            EnabledBackgroundFade = false;
            CloseButtonEnabled = true;
            JumpDrive = block;
            MyPlayer localHumanPlayer = MySession.Static.LocalHumanPlayer;
            if (localHumanPlayer != null && localHumanPlayer.Controller != null && localHumanPlayer.Controller.ControlledEntity != null && JumpDrive != null)
            {
                JumpSystem = block.CubeGrid.GridSystems.JumpSystem;
            }
            foreach (MyJumpDrive myJumpDrive in JumpDrive.CubeGrid.GetFatBlocks<MyJumpDrive>())
            {
                JumpDrives.Add(myJumpDrive);
                if (myJumpDrive.StoredPowerRatio < 1.0)
                {
                    charging.Add(myJumpDrive);
                }
            }
            RecreateControls(true);
        }

        public override string GetFriendlyName()
        {
            return "JumpSelector";
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            Vector2 value = listpos;
            float num = listoffset;
            Vector2 vector = listsize;
            try
            {
                vector.X -= (float)(JumpDrives.Count - 1) * num;
                vector.X /= (float)JumpDrives.Count;
                for (int i = 0; i < JumpDrives.Count; i++)
                {
                    MyTuple<string, Color> tooltip = GetTooltip(JumpDrives[i]);
                    string item = tooltip.Item1;
                    Color item2 = tooltip.Item2;
                    MyGuiControlButton myGuiControlButton = new MyGuiControlButton(new Vector2?(value), MyGuiControlButtonStyleEnum.Rectangular, new Vector2?(vector), null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, item, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, null, GuiSounds.MouseClick, 1f, new int?(i), false, false, false, null);
                    myGuiControlButton.DrawCrossTextureWhenDisabled = true;
                    myGuiControlButton.Enabled = JumpDrives[i].IsBuilt;
                    myGuiControlButton.ShowTooltipWhenDisabled = true;
                    myGuiControlButton.BorderColor = item2;
                    myGuiControlButton.BorderEnabled = true;
                    myGuiControlButton.BorderSize = 2;
                    myGuiControlButton.TooltipDelay = 0;
                    Controls.Add(myGuiControlButton);
                    myGuiControlButton.ButtonClicked += ToggleJDPower;
                    myGuiControlButton.SecondaryButtonClicked += ToggleAllJDPower;
                    buttonlist.Add(myGuiControlButton);
                    value.X += vector.X + num;
                }
            }
            catch { }
            listlabel = new MyGuiControlLabel(new Vector2?(labelpos), new Vector2?(labelsize), "Power Toggles", null, 0.7f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, false, float.PositiveInfinity, false);
            Controls.Add(listlabel);
            rangelabel = new MyGuiControlLabel(new Vector2?(rangepos), new Vector2?(rangesize), string.Format("Max Range: {0:N0}km", JumpSystem.GetMaxJumpDistance(MySession.Static.LocalPlayerId) / 1000.0), null, 0.7f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, false, float.PositiveInfinity, false);
            Controls.Add(rangelabel);
            radioButtonGPS = new MyGuiControlRadioButton
            {
                Position = gpspos,
                Size = radiosize,
                Name = "radioGPS",
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER,
                Key = 0,
                VisualStyle = MyGuiControlRadioButtonStyleEnum.Rectangular,
                CanHaveFocus = false,
                Text = new StringBuilder("Jump to GPS")
            };
            radioButtonBlind = new MyGuiControlRadioButton
            {
                Position = blindpos,
                Size = radiosize,
                Name = "radioBlind",
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER,
                Key = 1,
                VisualStyle = MyGuiControlRadioButtonStyleEnum.Rectangular,
                CanHaveFocus = false,
                Text = new StringBuilder("Blind Jump")
            };
            radioButtonGroup = new MyGuiControlRadioButtonGroup();
            radioButtonGroup.Add(radioButtonGPS);
            radioButtonGroup.Add(radioButtonBlind);
            distanceTextbox = new MyGuiControlTextbox(new Vector2?(textpos), lastDistance, 10, null, 0.8f, MyGuiControlTextboxType.DigitsOnly, MyGuiControlTextboxStyleEnum.Default, false);
            distanceTextbox.Size = textsize;
            Vector2 value2 = new Vector2(combopos.X, combopos.Y - textsize.Y - 0.01f);
            searchTextbox = new MyGuiControlTextbox(new Vector2?(value2), null, 20, null, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default, false);
            searchTextbox.Size = textsize;
            searchTextbox.SetToolTip("Search GPS Entries");
            gpsCombobox = new MyGuiControlCombobox(new Vector2?(combopos), new Vector2?(combosize), null, null, 10, null, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, false, false);
            confirmButton = new MyGuiControlButton(new Vector2?(confirmpos), MyGuiControlButtonStyleEnum.Default, new Vector2?(confirmsize), null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, new StringBuilder("Confirm"), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, null, GuiSounds.MouseClick, 1f, null, false, false, false, null);
            cancelButton = new MyGuiControlButton(new Vector2?(cancelpos), MyGuiControlButtonStyleEnum.Default, new Vector2?(cancelsize), null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, new StringBuilder("Cancel"), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, null, GuiSounds.MouseClick, 1f, null, false, false, false, null);
            GetGPSList();
            Controls.Add(radioButtonGPS);
            Controls.Add(radioButtonBlind);
            Controls.Add(distanceTextbox);
            Controls.Add(searchTextbox);
            Controls.Add(gpsCombobox);
            Controls.Add(confirmButton);
            Controls.Add(cancelButton);
            FocusedControl = distanceTextbox;
            gpsCombobox.SelectItemByIndex(lastSelectedGps);
            radioButtonGroup.SelectByKey(1);
            distanceTextbox.SelectAll();
            confirmButton.ButtonClicked += confirmButton_OnButtonClick;
            cancelButton.ButtonClicked += cancelButton_OnButtonClick;
            distanceTextbox.FocusChanged += SelectBlindOption;
            distanceTextbox.TextChanged += DistanceTextChanged;
            distanceTextbox.EnterPressed += BlindJump;
            gpsCombobox.FocusChanged += SelectGPSOption;
            searchTextbox.TextChanged += FilterGpsList;
            searchTextbox.FocusChanged += SearchFocusChanged;
        }

        public override void HandleUnhandledInput(bool receivedFocusInThisUpdate)
        {
            base.HandleUnhandledInput(receivedFocusInThisUpdate);
            if (charging.Count == 0)
            {
                return;
            }
            for (int i = charging.Count - 1; i >= 0; i--)
            {
                RefreshButton(buttonlist[JumpDrives.IndexOf(charging[i])]);
                if (charging[i].StoredPowerRatio == 1.0)
                {
                    charging.RemoveAt(i);
                }
            }
        }

        private void confirmButton_OnButtonClick(MyGuiControlButton sender)
        {
            if (radioButtonGroup.SelectedButton.Key == 1)
            {
                BlindJump(distanceTextbox);
            }
            else
            {
                JumpToGPS();
            }
        }

        private void cancelButton_OnButtonClick(MyGuiControlButton sender)
        {
            JumpSystem.RequestAbort();
            CloseScreen(false);
        }

        public void GetGPSList()
        {
            List<IMyGps> list = new List<IMyGps>();
            MySession.Static.Gpss.GetGpsList(MySession.Static.LocalPlayerId, list);
            foreach (IMyGps myGps in list)
            {
                string text = myGps.Name;
                if (!gpsList.ContainsKey(text))
                {
                    gpsList.Add(text, myGps);
                }
            }
            gpsCombobox.ClearItems();
            foreach (KeyValuePair<string, IMyGps> keyValuePair in gpsList)
            {
                gpsCombobox.AddItem((long)gpsList.IndexOfKey(keyValuePair.Key), keyValuePair.Key, null, null, true);
            }
        }

        private void JumpToGPS()
        {
            if (gpsCombobox.GetItemsCount() == 0)
            {
                return;
            }
            lastSelectedGps = (int)gpsCombobox.GetSelectedKey();
            IMyGps myGps = gpsList.Values[lastSelectedGps];
            CloseScreen(false);
            JumpSystem.RequestJump(myGps.Name, myGps.Coords, MySession.Static.LocalPlayerId);
        }

        private void BlindJump(MyGuiControlTextbox obj)
        {
            StringBuilder stringBuilder = new StringBuilder();
            distanceTextbox.GetText(stringBuilder);
            lastDistance = stringBuilder.ToString();
            double num = 0.0;
            try
            {
                num = Convert.ToDouble(stringBuilder.ToString()) * 1000.0;
            }
            catch
            {
                distanceTextbox.Clear();
                return;
            }
            if (num < 5000.01)
            {
                num = 5000.01;
            }
			MyPlayer Player = MySession.Static.LocalHumanPlayer;
			Vector3D value;
			if (Player.Controller.ControlledEntity is MyShipController)
				value = Vector3D.Transform(Base6Directions.GetVector((Player.Controller.ControlledEntity as MyShipController).Orientation.Forward), JumpDrive.CubeGrid.WorldMatrix.GetOrientation());
			else if (Player.Controller.ControlledEntity is IMyTurretControlBlock)
				value = (Player.Controller.ControlledEntity as IMyTurretControlBlock).GetShootDirection();
			else
				value = Player.Controller.ControlledEntity.GetHeadMatrix(true).GetDirectionVector(Base6Directions.Direction.Forward);
			Vector3D destination = JumpDrive.CubeGrid.WorldMatrix.Translation + value * num;
            CloseScreen(false);
            JumpSystem.RequestJump("Blind Jump", destination, MySession.Static.LocalPlayerId);
        }

        private void ToggleJDPower(MyGuiControlButton obj)
        {
            JumpDrives[obj.Index].Enabled = !JumpDrives[obj.Index].Enabled;
            RefreshButton(obj);
        }

        private void ToggleAllJDPower(MyGuiControlButton obj)
        {
            foreach (var drive in JumpDrives)
            {
                if  (drive != null)
                    drive.Enabled = true;
            }
            RecreateControls(false);
        }

        private void RefreshButton(MyGuiControlButton obj)
        {
            if (JumpDrives[obj.Index] != null && !JumpDrives[obj.Index].SlimBlock.IsDestroyed)
            {
                MyTuple<string, Color> tooltip = GetTooltip(JumpDrives[obj.Index]);
                obj.BorderColor = tooltip.Item2;
                obj.SetToolTip(tooltip.Item1);
                rangelabel.Text = string.Format("Max Range: {0:N0}km", JumpSystem.GetMaxJumpDistance(MySession.Static.LocalPlayerId) / 1000.0);
                return;
            }
            obj.BorderColor = Color.Gray;
            obj.Enabled = false;
        }

        private void SelectBlindOption(MyGuiControlBase obj, bool focus)
        {
            if (focus)
            {
                distanceTextbox.SelectAll();
                radioButtonGroup.SelectByKey(1);
            }
        }

        private void SelectGPSOption(MyGuiControlBase obj, bool focus)
        {
            if (focus)
            {
                gpsCombobox.IsActiveControl = true;
                radioButtonGroup.SelectByKey(0);
            }
        }

        private MyTuple<string, Color> GetTooltip(MyJumpDrive jd)
        {
            string text = "[On] ";
            Color item = Color.Green;
            if (jd.StoredPowerRatio < 1.0)
            {
                float num = jd.StoredPowerRatio * 100f;
                text = string.Format("[Charging {0:N1}%] ", num);
                item = Color.Yellow;
            }
            if (!jd.Enabled)
            {
                text = "[Off] ";
                item = Color.Red;
            }
            if (jd.SlimBlock.IsDestroyed)
            {
                text = "[Busted] ";
                item = Color.Gray;
            }
            text += jd.CustomName.ToString();
            return new MyTuple<string, Color>(text, item);
        }

        private void FilterGpsList(MyGuiControlTextbox obj)
        {
            radioButtonGroup.SelectByKey(0);
            StringBuilder stringBuilder = new StringBuilder();
            obj.GetText(stringBuilder);
            string filterText = stringBuilder.ToString();
            SortedList<string, IMyGps> filteredList = new SortedList<string, IMyGps>();
            string[] array = filterText.ToLower().Split(new char[] { ' ' });
            if (filterText != null)
            {
                foreach (var keyValuePair in gpsList)
                {
                    string text2 = keyValuePair.Value.Name.ToString().ToLower();
                    bool flag = true;
                    foreach (string text3 in array)
                    {
                        if (!text2.Contains(text3.ToLower()))
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        filteredList.Add(keyValuePair.Key, keyValuePair.Value);
                    }
                }
            }
            else
                filteredList = gpsList;
            gpsCombobox.ClearItems();
            foreach (KeyValuePair<string, IMyGps> keyValuePair2 in filteredList)
            {
                gpsCombobox.AddItem((long)gpsList.IndexOfKey(keyValuePair2.Key), keyValuePair2.Key, null, null, true);
            }
            if (gpsCombobox.GetItemsCount() > 0)
            {
                gpsCombobox.SelectItemByIndex(0);
            }
        }

        private void SearchFocusChanged(MyGuiControlBase obj, bool focus)
        {
            if (focus)
            {
                searchTextbox.SelectAll();
                radioButtonGroup.SelectByKey(0);
            }
        }

        private void DistanceTextChanged(MyGuiControlTextbox obj)
        {
            radioButtonGroup.SelectByKey(1);
        }

        private MyGuiControlButton cancelButton;

        private MyGuiControlButton confirmButton;

        private SortedList<string, IMyGps> gpsList;

        private MyGuiControlCombobox gpsCombobox;

        private MyGuiControlTextbox distanceTextbox;

        private MyGuiControlRadioButton radioButtonGPS;

        private MyGuiControlRadioButton radioButtonBlind;

        private MyGuiControlRadioButtonGroup radioButtonGroup;

        private static Vector2 textsize = new Vector2(0.25f, 0.04f);

        private static Vector2 textpos = new Vector2(0.1f, -0.056f);

        private static Vector2 combopos = new Vector2(0.1f, 0.065f);

        private static Vector2 combosize = new Vector2(0.25f, 0.04f);

        private static Vector2 confirmpos = new Vector2(-0.12f, 0.13f);

        private static Vector2 confirmsize = new Vector2(0.15f, 0.04f);

        private static Vector2 cancelpos = new Vector2(0.12f, 0.13f);

        private static Vector2 cancelsize = new Vector2(0.15f, 0.04f);

        private static Vector2 gpspos = new Vector2(-0.15f, 0.065f);

        private static Vector2 blindpos = new Vector2(-0.15f, -0.056f);

        private static Vector2 radiosize = new Vector2(0.1f, 0.05f);

        private static Vector2 listpos = new Vector2(-0.25f, -0.127f);

        private static Vector2 listsize = new Vector2(0.5f, 0.04f);

        private static float listoffset = 0.002f;

        private static Vector2 labelpos = new Vector2(-0.25f, -0.17f);

        private static Vector2 labelsize = new Vector2(0.1f, 0.02f);

        private static Vector2 rangepos = new Vector2(0.024f, -0.09f);

        private static Vector2 rangesize = new Vector2(0.14f, 0.02f);

        private MyGridJumpDriveSystem JumpSystem;

        private MyJumpDrive JumpDrive;

        private List<MyJumpDrive> JumpDrives = new List<MyJumpDrive>();

        private MyGuiControlLabel listlabel;

        private MyGuiControlLabel rangelabel;

        private List<MyGuiControlButton> buttonlist = new List<MyGuiControlButton>();

        private static int lastSelectedGps = 0;

        private static string lastDistance = "5";

        private List<MyJumpDrive> charging = new List<MyJumpDrive>();

        private MyGuiControlTextbox searchTextbox;
    }
}
