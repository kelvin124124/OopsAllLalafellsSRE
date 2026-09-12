using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using System;

namespace OopsAllLalafellsSRE.Utils
{
    /// <summary>
    /// The way a specific game object shall be redrawn.
    /// Mirrors Penumbra.Api.Enums.RedrawType.
    /// </summary>
    internal enum RedrawType
    {
        Redraw,
        AfterGPose,
    }

    /// <summary>
    /// Minimal, self-contained Penumbra IPC wrapper.
    /// <para/>
    /// We only need two of Penumbra's gates, so we subscribe to them directly instead of
    /// referencing Penumbra.Api: since 5.17 that package moved its subscriber helpers into
    /// Ottermandias' Luna library, which is not published on NuGet (the "Luna" id there is an
    /// unrelated package), so Penumbra.Api can no longer be consumed as a plain PackageReference.
    /// The gate labels below are stable parts of Penumbra's public API.
    /// </summary>
    internal class PenumbraIpc : IDisposable
    {
        private const string RedrawAllLabel = "Penumbra.RedrawAll.V5";
        private const string CreatingCharacterBaseLabel = "Penumbra.CreatingCharacterBase.V5";

        private readonly ICallGateSubscriber<int, object?>? redrawAll;
        private readonly ICallGateSubscriber<nint, Guid, nint, nint, nint, object?>? creatingCharacterBase;
        private readonly Action<nint, Guid, nint, nint, nint> creatingCharacterBaseHandler = Drawer.OnCreatingCharacterBase;

        public PenumbraIpc(IDalamudPluginInterface pluginInterface)
        {
            try
            {
                redrawAll = pluginInterface.GetIpcSubscriber<int, object?>(RedrawAllLabel);
                creatingCharacterBase =
                    pluginInterface.GetIpcSubscriber<nint, Guid, nint, nint, nint, object?>(CreatingCharacterBaseLabel);
                creatingCharacterBase.Subscribe(creatingCharacterBaseHandler);
            }
            catch (Exception ex)
            {
                Service.pluginLog.Error(ex, "Failed to register Penumbra IPC subscribers.");
                redrawAll = null;
                creatingCharacterBase = null;
            }
        }

        public void Dispose()
        {
            try
            {
                creatingCharacterBase?.Unsubscribe(creatingCharacterBaseHandler);
            }
            catch (Exception ex)
            {
                Service.pluginLog.Error(ex, "Failed to unregister Penumbra IPC subscribers.");
            }
        }

        internal void RedrawAll(RedrawType setting)
        {
            try
            {
                redrawAll?.InvokeAction((int)setting);
            }
            catch (Exception ex)
            {
                Plugin.OutputChatLine($"Warning: Penumbra not found. Error: {ex.Message}\n" +
                                      "Note: if you disable Penumbra before this plugin, lalafells will stay there until updated.");
            }
        }
    }
}
