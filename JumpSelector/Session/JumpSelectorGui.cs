using System;
using System.Collections.Generic;
using System.Text;
using Sandbox;
using Sandbox.Game.Entities;
using Sandbox.Game.GameSystems;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using VRage;
using VRage.Audio;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

namespace JumpSelector.Session
{
	public class JumpSelectorGui : MyGuiScreenBase
	{
		public override string GetFriendlyName()
		{
			return "JumpSelector";
		}

		public override void RecreateControls(bool contructor)
		{
			base.RecreateControls(contructor);
			Vector2 value = JumpSelectorGui.listpos;
			float num = JumpSelectorGui.listoffset;
			Vector2 vector = JumpSelectorGui.listsize;
			vector.X -= (float)(this.JumpDrives.Count - 1) * num;
			vector.X /= (float)this.JumpDrives.Count;
			for (int i = 0; i < this.JumpDrives.Count; i++)
			{
				MyTuple<string, Color> tooltip = this.GetTooltip(this.JumpDrives[i]);
				string item = tooltip.Item1;
				Color item2 = tooltip.Item2;
				MyGuiControlButton myGuiControlButton = new MyGuiControlButton(new Vector2?(value), MyGuiControlButtonStyleEnum.Rectangular, new Vector2?(vector), null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, item, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, null, GuiSounds.MouseClick, 1f, new int?(i), false, false, false, null);
				myGuiControlButton.DrawCrossTextureWhenDisabled = true;
				myGuiControlButton.Enabled = this.JumpDrives[i].IsBuilt;
				myGuiControlButton.ShowTooltipWhenDisabled = true;
				myGuiControlButton.BorderColor = item2;
				myGuiControlButton.BorderEnabled = true;
				myGuiControlButton.BorderSize = 2;
				myGuiControlButton.TooltipDelay = 0;
				this.Controls.Add(myGuiControlButton);
				myGuiControlButton.ButtonClicked += this.ToggleJDPower;
				this.buttonlist.Add(myGuiControlButton);
				value.X += vector.X + num;
			}
			this.listlabel = new MyGuiControlLabel(new Vector2?(JumpSelectorGui.labelpos), new Vector2?(JumpSelectorGui.labelsize), "Power Toggles", null, 0.7f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, false, float.PositiveInfinity, false);
			this.Controls.Add(this.listlabel);
			this.rangelabel = new MyGuiControlLabel(new Vector2?(JumpSelectorGui.rangepos), new Vector2?(JumpSelectorGui.rangesize), string.Format("Max Range: {0:N0}km", this.JumpSystem.GetMaxJumpDistance(this.ControlledBy.OwnerId) / 1000.0), null, 0.7f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, false, float.PositiveInfinity, false);
			this.Controls.Add(this.rangelabel);
			this.radioButtonGPS = new MyGuiControlRadioButton
			{
				Position = JumpSelectorGui.gpspos,
				Size = JumpSelectorGui.radiosize,
				Name = "radioGPS",
				OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER,
				Key = 0,
				VisualStyle = MyGuiControlRadioButtonStyleEnum.Rectangular,
				CanHaveFocus = false,
				Text = new StringBuilder("Jump to GPS")
			};
			this.radioButtonBlind = new MyGuiControlRadioButton
			{
				Position = JumpSelectorGui.blindpos,
				Size = JumpSelectorGui.radiosize,
				Name = "radioBlind",
				OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER,
				Key = 1,
				VisualStyle = MyGuiControlRadioButtonStyleEnum.Rectangular,
				CanHaveFocus = false,
				Text = new StringBuilder("Blind Jump")
			};
			this.radioButtonGroup = new MyGuiControlRadioButtonGroup();
			this.radioButtonGroup.Add(this.radioButtonGPS);
			this.radioButtonGroup.Add(this.radioButtonBlind);
			this.distanceTextbox = new MyGuiControlTextbox(new Vector2?(JumpSelectorGui.textpos), JumpSelectorGui.lastDistance, 10, null, 0.8f, MyGuiControlTextboxType.DigitsOnly, MyGuiControlTextboxStyleEnum.Default, false);
			this.distanceTextbox.Size = JumpSelectorGui.textsize;
			Vector2 value2 = new Vector2(JumpSelectorGui.combopos.X, JumpSelectorGui.combopos.Y - JumpSelectorGui.textsize.Y - 0.01f);
			this.searchTextbox = new MyGuiControlTextbox(new Vector2?(value2), null, 20, null, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default, false);
			this.searchTextbox.Size = JumpSelectorGui.textsize;
			this.searchTextbox.SetToolTip("Search GPS Entries");
			this.gpsCombobox = new MyGuiControlCombobox(new Vector2?(JumpSelectorGui.combopos), new Vector2?(JumpSelectorGui.combosize), null, null, 10, null, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, false, false);
			this.confirmButton = new MyGuiControlButton(new Vector2?(JumpSelectorGui.confirmpos), MyGuiControlButtonStyleEnum.Default, new Vector2?(JumpSelectorGui.confirmsize), null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, new StringBuilder("Confirm"), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, null, GuiSounds.MouseClick, 1f, null, false, false, false, null);
			this.cancelButton = new MyGuiControlButton(new Vector2?(JumpSelectorGui.cancelpos), MyGuiControlButtonStyleEnum.Default, new Vector2?(JumpSelectorGui.cancelsize), null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, new StringBuilder("Cancel"), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, null, GuiSounds.MouseClick, 1f, null, false, false, false, null);
			this.GetGPSList();
			this.Controls.Add(this.radioButtonGPS);
			this.Controls.Add(this.radioButtonBlind);
			this.Controls.Add(this.distanceTextbox);
			this.Controls.Add(this.searchTextbox);
			this.Controls.Add(this.gpsCombobox);
			this.Controls.Add(this.confirmButton);
			this.Controls.Add(this.cancelButton);
			base.FocusedControl = this.distanceTextbox;
			this.gpsCombobox.SelectItemByIndex(JumpSelectorGui.lastSelectedGps);
			this.radioButtonGroup.SelectByKey(1);
			this.distanceTextbox.SelectAll();
			this.confirmButton.ButtonClicked += this.confirmButton_OnButtonClick;
			this.cancelButton.ButtonClicked += this.cancelButton_OnButtonClick;
			this.distanceTextbox.FocusChanged += this.SelectBlindOption;
			this.distanceTextbox.TextChanged += this.DistanceTextChanged;
			this.distanceTextbox.EnterPressed += this.BlindJump;
			this.gpsCombobox.FocusChanged += this.SelectGPSOption;
			this.searchTextbox.TextChanged += this.FilterGpsList;
			this.searchTextbox.FocusChanged += this.SearchFocusChanged;
		}

		public override void HandleUnhandledInput(bool receivedFocusInThisUpdate)
		{
			base.HandleUnhandledInput(receivedFocusInThisUpdate);
			if (this.charging.Count == 0)
			{
				return;
			}
			for (int i = this.charging.Count - 1; i >= 0; i--)
			{
				this.RefreshButton(this.buttonlist[this.JumpDrives.IndexOf(this.charging[i])]);
				if (this.charging[i].IsFull)
				{
					this.charging.RemoveAt(i);
				}
			}
		}

		private void confirmButton_OnButtonClick(MyGuiControlButton sender)
		{
			if (this.radioButtonGroup.SelectedButton.Key == 1)
			{
				this.BlindJump(this.distanceTextbox);
			}
			else
			{
				this.JumpToGPS();
			}
			this.CloseScreen(false);
		}

		private void cancelButton_OnButtonClick(MyGuiControlButton sender)
		{
			this.JumpSystem.RequestAbort();
			this.CloseScreen(false);
		}

		public JumpSelectorGui() : base(new Vector2?(new Vector2(0.5f, 0.5f)), new Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR * MySandboxGame.Config.UIBkOpacity), new Vector2?(new Vector2(0.55f, 0.4f)), false, null, 0f, 0f, null)
		{
			this.gpsList = new SortedList<string, IMyGps>();
			base.CanHideOthers = false;
			base.EnabledBackgroundFade = false;
			base.CloseButtonEnabled = true;
			MyPlayer localHumanPlayer = MySession.Static.LocalHumanPlayer;
			if (localHumanPlayer != null && localHumanPlayer.Controller != null && localHumanPlayer.Controller.ControlledEntity != null && localHumanPlayer.Controller.ControlledEntity.Entity is MyCubeBlock)
			{
				this.ControlledBy = (MyCubeBlock)localHumanPlayer.Controller.ControlledEntity.Entity;
				if (!(this.ControlledBy is MyCockpit) && !(this.ControlledBy is MyRemoteControl))
				{
					return;
				}
				this.JumpSystem = this.ControlledBy.CubeGrid.GridSystems.JumpSystem;
			}
			foreach (MyJumpDrive myJumpDrive in this.ControlledBy.CubeGrid.GetFatBlocks<MyJumpDrive>())
			{
				this.JumpDrives.Add(myJumpDrive);
				if (!myJumpDrive.IsFull)
				{
					this.charging.Add(myJumpDrive);
				}
			}
			this.RecreateControls(true);
		}

		public void GetGPSList()
		{
			List<IMyGps> list = new List<IMyGps>();
			MySession.Static.Gpss.GetGpsList(MySession.Static.LocalPlayerId, list);
			foreach (IMyGps myGps in list)
			{
				string text = myGps.Name;
				if (this.gpsList.ContainsKey(text))
				{
					text += " - ";
					text += list.IndexOf(myGps).ToString();
				}
				this.gpsList.Add(text, myGps);
			}
			foreach (KeyValuePair<string, IMyGps> keyValuePair in this.gpsList)
			{
				this.gpsCombobox.AddItem((long)this.gpsList.IndexOfKey(keyValuePair.Key), keyValuePair.Key, null, null, true);
			}
		}

		private void JumpToGPS()
		{
			JumpSelectorGui.lastSelectedGps = (int)this.gpsCombobox.GetSelectedKey();
			IMyGps myGps = this.gpsList.Values[JumpSelectorGui.lastSelectedGps];
			this.JumpSystem.RequestJump(myGps.Name, myGps.Coords, this.ControlledBy.OwnerId);
		}

		private void BlindJump(MyGuiControlTextbox obj)
		{
			StringBuilder stringBuilder = new StringBuilder();
			this.distanceTextbox.GetText(stringBuilder);
			JumpSelectorGui.lastDistance = stringBuilder.ToString();
			double num = 0.0;
			try
			{
				num = Convert.ToDouble(stringBuilder.ToString()) * 1000.0;
			}
			catch
			{
				return;
			}
			if (num < 5000.01)
			{
				num = 5000.01;
			}
			Vector3D value = Vector3D.Transform(Base6Directions.GetVector(this.ControlledBy.Orientation.Forward), this.ControlledBy.CubeGrid.WorldMatrix.GetOrientation());
			value.Normalize();
			Vector3D destination = this.ControlledBy.CubeGrid.WorldMatrix.Translation + value * num;
			this.JumpSystem.RequestJump("Blind Jump", destination, this.ControlledBy.OwnerId);
		}

		static JumpSelectorGui()
		{
		}

		private void ToggleJDPower(MyGuiControlButton obj)
		{
			this.JumpDrives[obj.Index].Enabled = !this.JumpDrives[obj.Index].Enabled;
			this.RefreshButton(obj);
		}

		private void RefreshButton(MyGuiControlButton obj)
		{
			if (this.JumpDrives[obj.Index] != null && this.JumpDrives[obj.Index].IsBuilt)
			{
				MyTuple<string, Color> tooltip = this.GetTooltip(this.JumpDrives[obj.Index]);
				obj.BorderColor = tooltip.Item2;
				obj.SetToolTip(tooltip.Item1);
				this.rangelabel.Text = string.Format("Max Range: {0:N0}km", this.JumpSystem.GetMaxJumpDistance(this.ControlledBy.OwnerId) / 1000.0);
				return;
			}
			obj.BorderColor = Color.Gray;
			obj.Enabled = false;
		}

		private void SelectBlindOption(MyGuiControlBase obj, bool focus)
		{
			if (focus)
			{
				this.distanceTextbox.SelectAll();
				this.radioButtonGroup.SelectByKey(1);
			}
		}

		private void SelectGPSOption(MyGuiControlBase obj, bool focus)
		{
			if (focus)
			{
				this.gpsCombobox.IsActiveControl = true;
				this.radioButtonGroup.SelectByKey(0);
			}
		}

		private MyTuple<string, Color> GetTooltip(MyJumpDrive jd)
		{
			bool enabled = jd.Enabled;
			string text = "[On] ";
			Color item = Color.Green;
			if (!jd.IsFull)
			{
				float num = jd.CurrentStoredPower / jd.BlockDefinition.PowerNeededForJump * 100f;
				text = string.Format("[Charging {0:N1}%] ", num);
				item = Color.Yellow;
			}
			if (!enabled)
			{
				text = "[Off] ";
				item = Color.Red;
			}
			if (!jd.IsBuilt)
			{
				text = "[Busted] ";
				item = Color.Gray;
			}
			text += jd.CustomName.ToString();
			return new MyTuple<string, Color>(text, item);
		}

		private void FilterGpsList(MyGuiControlTextbox obj)
		{
			this.radioButtonGroup.SelectByKey(0);
			StringBuilder stringBuilder = new StringBuilder();
			obj.GetText(stringBuilder);
			string text = stringBuilder.ToString();
			SortedList<string, IMyGps> sortedList = new SortedList<string, IMyGps>();
			string[] array = text.ToLower().Split(new char[]
			{
				' '
			});
			if (text != null)
			{
				using (IEnumerator<KeyValuePair<string, IMyGps>> enumerator = this.gpsList.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<string, IMyGps> keyValuePair = enumerator.Current;
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
							sortedList.Add(keyValuePair.Key, keyValuePair.Value);
						}
					}
					goto IL_DE;
				}
			}
			sortedList = this.gpsList;
			IL_DE:
			this.gpsCombobox.ClearItems();
			foreach (KeyValuePair<string, IMyGps> keyValuePair2 in sortedList)
			{
				this.gpsCombobox.AddItem((long)this.gpsList.IndexOfKey(keyValuePair2.Key), keyValuePair2.Key, null, null, true);
			}
			if (this.gpsCombobox.GetItemsCount() > 0)
			{
				this.gpsCombobox.SelectItemByIndex(0);
			}
		}

		private void SearchFocusChanged(MyGuiControlBase obj, bool focus)
		{
			if (focus)
			{
				this.searchTextbox.SelectAll();
				this.radioButtonGroup.SelectByKey(0);
			}
		}

		private void DistanceTextChanged(MyGuiControlTextbox obj)
		{
			this.radioButtonGroup.SelectByKey(1);
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

		private static float listoffset = 0.005f;

		private static Vector2 labelpos = new Vector2(-0.25f, -0.17f);

		private static Vector2 labelsize = new Vector2(0.1f, 0.02f);

		private static Vector2 rangepos = new Vector2(0.024f, -0.09f);

		private static Vector2 rangesize = new Vector2(0.14f, 0.02f);

		private MyGridJumpDriveSystem JumpSystem;

		private MyCubeBlock ControlledBy;

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
