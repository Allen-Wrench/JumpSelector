using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using Sandbox.Game.Entities;
using Sandbox.Game.Gui;
using Sandbox.Game.Screens.Terminal.Controls;
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
                MyToolbarType.ButtonPanel,
                MyToolbarType.Seat
            };
            MyTerminalControlFactory.AddAction<MyJumpDrive>(myTerminalAction);
        }

        public static void ShowJumpSelector(MyJumpDrive block)
        {
            if (block.IDModule.ShareMode == MyOwnershipShareModeEnum.All || (block.GetPlayerRelationToOwner() == MyRelationsBetweenPlayerAndBlock.Owner || block.GetPlayerRelationToOwner() == MyRelationsBetweenPlayerAndBlock.FactionShare))
            {
                MyGuiSandbox.AddScreen(new JumpSelectorGui(block));
            }
            else
            {
                MyGuiSandbox.Show(new StringBuilder("You do not have permission to use this block"), VRage.Utils.MyStringId.GetOrCompute("Invalid Permissions"));
                return;
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

        public static bool JumpEffectPatch()
        {
            return false;
        }

        public static IEnumerable<CodeInstruction> PerformJumpTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = instructions.ToList();
            for (int i = 0; i < code.Count(); i++)
            {
                if (code[i].Calls(AccessTools.Method("Sandbox.Game.GameSystems.MyGridJumpDriveSystem:IsLocalCharacterAffectedByJump")))
                {
                    code[i-1] = new CodeInstruction(OpCodes.Pop, null);
                    code[i] = new CodeInstruction(OpCodes.Ldc_I4_0, null);
                    break;
                }
            }

            return code;
        }
    }
}
