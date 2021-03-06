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
