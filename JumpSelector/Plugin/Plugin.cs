using HarmonyLib;
using System;
using Sandbox;
using VRage.Plugins;

namespace JumpSelector.Plugin
{
	public class Plugin : IPlugin, IDisposable
	{
		public void Dispose()
		{
		}

		public void Init(object gameInstance)
        {
			Harmony harmony = new Harmony("JumpSelector");
			harmony.Patch(AccessTools.Method("Sandbox.Game.Entities.MyJumpDrive:CreateTerminalControls", null, null), null, null, new HarmonyMethod(AccessTools.Method("JumpSelector.Plugin.JumpSelectorPatch:JumpSelectTranspiler", null, null)), null);
			harmony.Patch(AccessTools.Method("Sandbox.Game.GameSystems.MyGridJumpDriveSystem:UpdateJumpEffect", null, null), new HarmonyMethod(AccessTools.Method("JumpSelector.Plugin.JumpSelectorPatch:JumpEffectPatch", null, null)), null, null, null);
			harmony.Patch(AccessTools.Method("Sandbox.Game.GameSystems.MyGridJumpDriveSystem:PerformJump", null, null), null, null, new HarmonyMethod(AccessTools.Method("JumpSelector.Plugin.JumpSelectorPatch:PerformJumpTranspiler", null, null)), null);
			harmony.Patch(AccessTools.Method("Sandbox.Game.GameSystems.MyGridJumpDriveSystem:CleanupAfterJump", null, null), null, null, new HarmonyMethod(AccessTools.Method("JumpSelector.Plugin.JumpSelectorPatch:PerformJumpTranspiler", null, null)), null);
			MySandboxGame.Log.WriteLine("Jump Selector Plugin Loaded.");
		}

		public void Update()
		{
		}
	}
}
