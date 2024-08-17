using Vintagestory.API.Common;

namespace pick_bars;

public class pick_barsModSystem : ModSystem
{
    // Called on server and client
    // Useful for registering block/entity classes on both sides
    public override void Start(ICoreAPI api)
    {
        api.RegisterItemClass(Mod.Info.ModID + ".pickbar", typeof(ItemPickbar));
    }
}