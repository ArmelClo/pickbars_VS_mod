using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace pick_bars;

public class ItemPickbar : Item
{
    public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel,
        EntitySelection entitySel,
        bool firstEvent, ref EnumHandHandling handling)
    {
        handling = EnumHandHandling.PreventDefault;

        if (api.Side == EnumAppSide.Client)
        {
            return;
        }
        if (byEntity.Controls.CtrlKey && byEntity.Controls.ShiftKey)
        {
            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
            return;
        }
        if (!firstEvent)
        {
            return;
        }
        EntityPlayer entityPlayer = byEntity as EntityPlayer;
        blockSel = entityPlayer!.BlockSelection;
        
        if (blockSel?.Block == null)
        {
            return;
        }

        if (!blockSel.Block.BlockBehaviors.OfType<BlockBehaviorUnstableRock>().Any())
        {
            return;
        }

        var blockSelList = getAllRockBlocks(api, blockSel);

        var color = new List<int>();
        foreach (var blockSele in blockSelList)
        {
            var instableblock = blockSele.Block.BlockBehaviors.OfType<BlockBehaviorUnstableRock>().First();
            var intablility = instableblock.getInstability(blockSele.Position); 
            color.Add(ColorUtil.ColorFromRgba((int)(255 * intablility), (int)(255 - 255 * intablility), 0, 50));
        }

        var blockPosList = new List<BlockPos>();
        foreach (var sel in blockSelList)
        {
            blockPosList.Add(sel.Position);
        }
        
        BlockPos pos = blockSel.Position;
        DamageItem(api.World, byEntity, entityPlayer?.Player.InventoryManager.ActiveHotbarSlot, 1);
        byEntity.World.PlaySoundAt(new AssetLocation("pickbars:sounds/pickbarhit"), (double)pos.X, (double)pos.Y, (double)pos.Z, null, false, 32f, 1f);
        api.World.HighlightBlocks(entityPlayer?.Player, 1, blockPosList, color, EnumHighlightBlocksMode.Absolute,
            EnumHighlightShape.Arbitrary, 1f);
    }

    void Clear(IPlayer p)
    {
        api.World.HighlightBlocks(p, 0, new List<BlockPos>(), new List<int>(), EnumHighlightBlocksMode.Absolute,
            EnumHighlightShape.Arbitrary, 1f);
    }

    List<BlockSelection> getAllRockBlocks(ICoreAPI api, BlockSelection blockorigine)
    {
        var blockSelList = new List<BlockSelection>();
        var blockSelStack = new Stack<BlockSelection>();
        blockSelStack.Push(blockorigine);

        while (blockSelStack.Count != 0)
        {
            var blockse = blockSelStack.Pop();
            var voisins = adjacentBlock(api, blockse, blockorigine.Position);
            foreach (var voisin in voisins)
            {
                if (blockSelList.Any(sel => sel.Position == voisin.Position) ||
                    blockSelStack.Any(sel => sel.Position == voisin.Position))
                {
                    continue;
                }

                blockSelStack.Push(voisin);
            }

            blockSelList.Add(blockse);
        }


        return blockSelList;
    }

    List<BlockSelection> adjacentBlock(ICoreAPI api, BlockSelection block, BlockPos originepos)
    {
        var allAdjacentBlockPos = new List<BlockPos>();
        //listadjacentblockpos.Add(block.Position.UpCopy());
        allAdjacentBlockPos.Add(block.Position.UpCopy().EastCopy());
        allAdjacentBlockPos.Add(block.Position.UpCopy().WestCopy());

        allAdjacentBlockPos.Add(block.Position.UpCopy().NorthCopy());
        allAdjacentBlockPos.Add(block.Position.UpCopy().NorthCopy().EastCopy());
        allAdjacentBlockPos.Add(block.Position.UpCopy().NorthCopy().WestCopy());

        allAdjacentBlockPos.Add(block.Position.UpCopy().SouthCopy());
        allAdjacentBlockPos.Add(block.Position.UpCopy().SouthCopy().EastCopy());
        allAdjacentBlockPos.Add(block.Position.UpCopy().SouthCopy().WestCopy());


        //listadjacentblockpos.Add(block.Position);
        allAdjacentBlockPos.Add(block.Position.EastCopy());
        allAdjacentBlockPos.Add(block.Position.WestCopy());

        allAdjacentBlockPos.Add(block.Position.NorthCopy());
        allAdjacentBlockPos.Add(block.Position.NorthCopy().EastCopy());
        allAdjacentBlockPos.Add(block.Position.NorthCopy().WestCopy());

        allAdjacentBlockPos.Add(block.Position.SouthCopy());
        allAdjacentBlockPos.Add(block.Position.SouthCopy().EastCopy());
        allAdjacentBlockPos.Add(block.Position.SouthCopy().WestCopy());


        //listadjacentblockpos.Add(block.Position.DownCopy());
        allAdjacentBlockPos.Add(block.Position.DownCopy().EastCopy());
        allAdjacentBlockPos.Add(block.Position.DownCopy().WestCopy());

        allAdjacentBlockPos.Add(block.Position.DownCopy().NorthCopy());
        allAdjacentBlockPos.Add(block.Position.DownCopy().NorthCopy().EastCopy());
        allAdjacentBlockPos.Add(block.Position.DownCopy().NorthCopy().WestCopy());

        allAdjacentBlockPos.Add(block.Position.DownCopy().SouthCopy());
        allAdjacentBlockPos.Add(block.Position.DownCopy().SouthCopy().EastCopy());
        allAdjacentBlockPos.Add(block.Position.DownCopy().SouthCopy().WestCopy());

        var listAdjacentBlockSel = new List<BlockSelection>();

        foreach (var adjacentBlockPos in allAdjacentBlockPos)
        {
            var adjacentBlockSel = new BlockSelection(adjacentBlockPos, BlockFacing.UP, api.World.BlockAccessor.GetBlock(adjacentBlockPos));
            if (blockposrespectrules(api, adjacentBlockSel, originepos))
            {
                listAdjacentBlockSel.Add(adjacentBlockSel);
            }
        }

        return listAdjacentBlockSel;
    }

    bool blockposrespectrules(ICoreAPI api, BlockSelection block, BlockPos originepos)
    {
        if (block.Position.DistanceTo(originepos) > 2*ToolTier)
        {
            return false;
        }

        if (!block.Block.BlockBehaviors.OfType<BlockBehaviorUnstableRock>().Any())
        {
            return false;
        }
        
        return api.World.BlockAccessor.GetBlock(block.Position.DownCopy()).LightAbsorption == 0 || api.World.BlockAccessor.GetBlock(block.Position.UpCopy()).LightAbsorption == 0 || api.World.BlockAccessor.GetBlock(block.Position.NorthCopy()).LightAbsorption == 0 || api.World.BlockAccessor.GetBlock(block.Position.SouthCopy()).LightAbsorption == 0 || api.World.BlockAccessor.GetBlock(block.Position.EastCopy()).LightAbsorption == 0 || api.World.BlockAccessor.GetBlock(block.Position.WestCopy()).LightAbsorption == 0;
    }
}