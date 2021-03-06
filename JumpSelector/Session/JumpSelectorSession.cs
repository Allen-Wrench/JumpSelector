using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.Game.Gui;
using Sandbox.Game.Screens.Terminal.Controls;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.Game;
using VRage.Game.Components;

namespace JumpSelector.Session
{
	[MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, 1000)]
	public class JumpSelectorSession : MySessionComponentBase
	{
		public JumpSelectorSession()
		{
			JumpSelectorSession.Static = this;
		}

		public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
		{
			MyGuiScreenLoading.Static.OnScreenLoadingFinished += this.Load;
		}

		public void Load()
		{
			MyGuiScreenLoading.Static.OnScreenLoadingFinished -= this.Load;
			IMyTerminalAction myTerminalAction = MyAPIGateway.TerminalControls.CreateAction<IMyJumpDrive>("JumpSelect");
			myTerminalAction.Name = new StringBuilder("Jump Select");
			myTerminalAction.Icon = MyTerminalActionIcons.STATION_ON;
			myTerminalAction.Action = new Action<IMyTerminalBlock>(this.ShowJumpSelector);
			myTerminalAction.Writer = delegate(IMyTerminalBlock block, StringBuilder builder)
			{
				builder.Append("Jump Select");
			};
			myTerminalAction.ValidForGroups = false;
			myTerminalAction.InvalidToolbarTypes = new List<MyToolbarType>
			{
				MyToolbarType.Character,
				MyToolbarType.Seat
			};
			MyAPIGateway.TerminalControls.AddAction<IMyJumpDrive>(myTerminalAction);
		}

		public void ShowJumpSelector(IMyTerminalBlock block)
		{
			MyRelationsBetweenPlayerAndBlock userRelationToOwner = block.GetUserRelationToOwner(MySession.Static.LocalPlayerId);
			if (userRelationToOwner == MyRelationsBetweenPlayerAndBlock.FactionShare || userRelationToOwner == MyRelationsBetweenPlayerAndBlock.Owner)
			{
				MyGuiSandbox.AddScreen(new JumpSelectorGui());
			}
		}

		public static JumpSelectorSession Static;
	}
}
