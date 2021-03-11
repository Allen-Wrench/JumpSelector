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
            new Harmony("JumpSelector").Patch(AccessTools.Method("Sandbox.Game.Entities.MyJumpDrive:CreateTerminalControls", null, null), null, null, new HarmonyMethod(AccessTools.Method("JumpSelector.Plugin.JumpSelectorPatch:JumpSelectTranspiler", null, null)), null);
            MySandboxGame.Log.WriteLine("Jump Selector Plugin Loaded.");
		}

		public void Update()
		{
		}

		public Plugin()
		{
		}
	}
}
