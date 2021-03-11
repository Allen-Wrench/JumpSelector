using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using Sandbox.Game.Entities;
using Sandbox.Game.Gui;
using Sandbox.Game.Screens.Terminal.Controls;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using VRage.Game;

namespace JumpSelector.Plugin
{
    public class JumpSelectorPatch
    {
        public JumpSelectorPatch()
        {
        }

        public static void JumpSelectAction()
        {
            MyTerminalAction<MyJumpDrive> myTerminalAction = new MyTerminalAction<MyJumpDrive>("JumpSelect", new StringBuilder("Jump Select"), MyTerminalActionIcons.STATION_ON);
            myTerminalAction.Action = new Action<MyJumpDrive>(ShowJumpSelector);
            myTerminalAction.Writer = delegate (MyJumpDrive block, StringBuilder builder)
            {
                builder.Append("Jump Select");
            };
            myTerminalAction.ValidForGroups = false;
            myTerminalAction.InvalidToolbarTypes = new List<MyToolbarType>
            {
                MyToolbarType.Character,
                MyToolbarType.Seat
            };
            MyTerminalControlFactory.AddAction<MyJumpDrive>(myTerminalAction);
        }

        public static void ShowJumpSelector(MyJumpDrive block)
        {
            MyRelationsBetweenPlayerAndBlock userRelationToOwner = block.GetUserRelationToOwner(MySession.Static.LocalPlayerId);
            if (userRelationToOwner == MyRelationsBetweenPlayerAndBlock.FactionShare || userRelationToOwner == MyRelationsBetweenPlayerAndBlock.Owner)
            {
                MyGuiSandbox.AddScreen(new JumpSelectorGui());
            }
        }

        public static IEnumerable<CodeInstruction> JumpSelectTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            int index = instructions.Count() - 1;
            for (int i = 0; i < index; i++)
            {
                yield return instructions.ElementAt(i);
            }
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(JumpSelectorPatch), "JumpSelectAction", null, null));
            yield return new CodeInstruction(OpCodes.Ret, null);
            yield break;
        }
    }
}
