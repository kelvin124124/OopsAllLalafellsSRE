using Dalamud.Game.Gui.NamePlate;

namespace OopsAllLalafellsSRE.Utils
{
    internal class Nameplate
    {
        public Nameplate()
        {
            Service.namePlateGui.OnNamePlateUpdate += (context, handlers) =>
            {
                if (!Service.configuration.enabled || !Service.configuration.nameHQ)
                    return;

                foreach (var handler in handlers)
                {
                    if (handler.NamePlateKind == NamePlateKind.PlayerCharacter)
                    {
                        unsafe
                        {
                            if (handler.PlayerCharacter == null) return;

                            // if native lalafells
                            if (!Drawer.NonNativeID.Contains(handler.PlayerCharacter.Name.TextValue))
                            {
                                // Plugin.OutputChatLine($"Adding HQ to {handler.PlayerCharacter.Name.TextValue}");
                                handler.NameParts.Text = $"\uE03C {handler.Name}";
                            }
                        }
                    }
                }
            };
        }

        public void Dispose() { }
    }
}
