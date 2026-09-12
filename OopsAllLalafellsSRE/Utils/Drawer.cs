using FFXIVClientStructs.FFXIV.Client.Game.Object;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static OopsAllLalafellsSRE.Utils.Constant;

namespace OopsAllLalafellsSRE.Utils
{
    internal class Drawer : IDisposable
    {
        public static HashSet<string> NonNativeID = [];

        public Drawer()
        {
            Service.configWindow.OnConfigChanged += RefreshAllPlayers;
            if (Service.configuration.enabled)
            {
                Plugin.OutputChatLine("OopsAllLalafellsSRE starting...");
                RefreshAllPlayers();
            }
        }

        private static void RefreshAllPlayers()
        {
            Plugin.OutputChatLine("Refreshing all players");
            NonNativeID.Clear();
            Service.penumbraApi.RedrawAll(RedrawType.Redraw);
            Service.namePlateGui.RequestRedraw();
        }

        public static unsafe void OnCreatingCharacterBase(nint gameObjectAddress, Guid _1, nint _2, nint customizePtr, nint _3)
        {
            if (!Service.configuration.enabled) return;

            // return if not player character
            var gameObj = (GameObject*)gameObjectAddress;
            if (gameObj->ObjectKind != ObjectKind.Pc) return;

            var customData = Marshal.PtrToStructure<CharaCustomizeData>(customizePtr);
            if (customData.Race == Service.configuration.SelectedRace || customData.Race == Race.UNKNOWN)
                return;

            NonNativeID.Add(gameObj->NameString);
            ChangeRace(customData, customizePtr, Service.configuration.SelectedRace);
        }

        private static unsafe void ChangeRace(CharaCustomizeData customData, nint customizePtr, Race selectedRace)
        {
            customData.Race = selectedRace;
            customData.Tribe = (byte)(((byte)selectedRace * 2) - (customData.Tribe % 2));
            customData.FaceType %= 4;
            customData.ModelType %= 2;
            customData.HairStyle = (byte)((customData.HairStyle % RaceMappings.RaceHairs[selectedRace]) + 1);
            Marshal.StructureToPtr(customData, customizePtr, true);
        }

        public void Dispose()
        {
            Service.configWindow.OnConfigChanged -= RefreshAllPlayers;
        }
    }
}
