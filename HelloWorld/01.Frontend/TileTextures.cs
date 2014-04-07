﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using SlimDX.D3DCompiler;
using Device = SlimDX.Direct3D11.Device;
using System.IO;
using WindowsFormsApplication7.Business;
using WindowsFormsApplication7.CrossCutting.Entities;
using WindowsFormsApplication7.Business.Repositories;

namespace WindowsFormsApplication7.Frontend
{
    class TileTextures
    {
        public static TileTextures Instance = new TileTextures();
        private Dictionary<int, int> topBlockTextures = new Dictionary<int, int>();
        private Dictionary<int, int> sideBlockTextures = new Dictionary<int, int>();
        private Dictionary<int, int> bottomBlockTextures = new Dictionary<int, int>();
        private Dictionary<string, int> indexMap = new Dictionary<string, int>();
        private Dictionary<int, VertexBuffer> blockVertexBuffers = new Dictionary<int, VertexBuffer>();
        private Dictionary<int, VertexBuffer> itemVertexBuffers = new Dictionary<int, VertexBuffer>();
        private VertexBuffer[] destroyBlocksBuffers = new VertexBuffer[10];
        private VertexBuffer selectionBlockBuffers;
        public ShaderResourceView View;

        public void Dispose()
        {
            foreach (VertexBuffer vertexBuffer in blockVertexBuffers.Values)
            {
                vertexBuffer.Dispose();
            }
            blockVertexBuffers.Clear();
            foreach (VertexBuffer vertexBuffer in itemVertexBuffers.Values)
            {
                vertexBuffer.Dispose();
            }
            itemVertexBuffers.Clear();
            if (View != null)
                View.Dispose();
            foreach (VertexBuffer vertexBuffer in destroyBlocksBuffers)
            {
                vertexBuffer.Dispose();
            }
            destroyBlocksBuffers = null;
            selectionBlockBuffers.Dispose();
            selectionBlockBuffers = null;
        }

        internal void Initialize()
        {
            LoadAllTileTextures();

            // define blocks
            DefineBlock(BlockRepository.DirtWithGrass.Id, "grass_top", "grass_side", "dirt");
            DefineBlock(BlockRepository.Dirt.Id, "dirt");
            DefineBlock(BlockRepository.Stone.Id, "stone");
            DefineBlock(BlockRepository.Wood.Id, "log_oak_top", "log_oak", "log_oak_top");
            DefineBlock(BlockRepository.Leaf.Id, "leaves_oak_opaque");
            DefineBlock(BlockRepository.Brick.Id, "brick");
            DefineBlock(BlockRepository.Sand.Id, "sand");
            DefineBlock(BlockRepository.BedRock.Id, "bedrock");
            DefineBlock(BlockRepository.Diamond.Id, "diamond_ore");
            DefineBlock(BlockRepository.Plank.Id, "planks_oak");
            DefineBlock(BlockRepository.CobbleStone.Id, "cobblestone");
            DefineBlock(BlockRepository.TallGrass.Id, "tallgrass");
            DefineBlock(BlockRepository.FarmlandDry.Id, "farmland_dry", "dirt", "dirt");
            DefineBlock(BlockRepository.FarmlandWet.Id, "farmland_wet", "dirt", "dirt");
            DefineBlock(BlockRepository.Wheat.Id, "wheat_stage_5");

            // Define destroy blocks...
            DefineDestroyAndSelectionBlocks();

            // define items
            BuildItemVertexBuffer(ItemRepository.Stick.Id, "stick");
            BuildItemVertexBuffer(ItemRepository.StoneAxe.Id, "stone_axe");
            BuildItemVertexBuffer(ItemRepository.StoneHoe.Id, "stone_hoe");
            BuildItemVertexBuffer(ItemRepository.StonePickAxe.Id, "stone_pickaxe");
            BuildItemVertexBuffer(ItemRepository.StoneShovel.Id, "stone_shovel");
            BuildItemVertexBuffer(ItemRepository.StoneSword.Id, "stone_sword");
            BuildItemVertexBuffer(ItemRepository.SeedsWheat.Id, "seeds_wheat");
            BuildItemVertexBuffer(ItemRepository.Wheat.Id, "wheat");
            BuildItemVertexBuffer(ItemRepository.Bread.Id, "bread");
        }

        private void DefineDestroyAndSelectionBlocks()
        {
            float alpha = 0.6f;
            Vector4[] BlockColors = new Vector4[] { 
                new Vector4(1,1,1,alpha),
                new Vector4(1,1,1,alpha),
                new Vector4(1,1,1,alpha),
                new Vector4(1,1,1,alpha),
                new Vector4(1,1,1,alpha),
                new Vector4(1,1,1,alpha)};
            int tileIndex;
            for (int i = 0; i < 10; i++)
            {
                tileIndex = indexMap["destroy_stage_" + i];
                destroyBlocksBuffers[i] = GenerateBlockBuffers(
                tileIndex,
                tileIndex,
                tileIndex,
                tileIndex,
                tileIndex,
                tileIndex,
                BlockColors);
            }
            tileIndex = indexMap["selection_box"];
            selectionBlockBuffers = GenerateBlockBuffers(
            tileIndex,
            tileIndex,
            tileIndex,
            tileIndex,
            tileIndex,
            tileIndex,
            BlockColors);
        }

        private void BuildItemVertexBuffer(int itemId, string name)
        {
            Tessellator t = Tessellator.Instance;
            t.StartDrawingTiledQuads();
            Vector4 c1 = new Vector4(1, 1, 1, 1);
            t.ArrayIndex = indexMap[name];
            Vector3 normal = new Vector3(0, 0, 1);
            float s = 1f;
            t.AddVertexWithColor(new Vector4(0f, 0f, 0, 1.0f), c1, normal);
            t.AddVertexWithColor(new Vector4(0f, s, 0, 1.0f), c1, normal);
            t.AddVertexWithColor(new Vector4(s, s, 0, 1.0f), c1, normal);
            t.AddVertexWithColor(new Vector4(s, 0f, 0, 1.0f), c1, normal);
            itemVertexBuffers.Add(itemId, t.GetVertexBuffer());
        }

        private void DefineBlock(int blockid, string allSides)
        {
            DefineBlock(blockid, allSides, allSides, allSides);
        }

        private void BuildBlockVertexBuffer(int blockId)
        {
            int tileIndexFront = TileTextures.Instance.SideIndex(blockId);
            int tileIndexTop = TileTextures.Instance.TopIndex(blockId);
            int tileIndexBottom = TileTextures.Instance.BottomIndex(blockId);

            blockVertexBuffers.Add(blockId, GenerateBlockBuffers(
                tileIndexFront,
                tileIndexFront,
                tileIndexFront,
                tileIndexFront,
                tileIndexTop,
                tileIndexBottom,
                BlockRepository.Blocks[blockId].BlockColors));
        }

        private VertexBuffer GenerateBlockBuffers(
            int tileIndexFront,
            int tileIndexBack,
            int tileIndexLeft,
            int tileIndexRight,
            int tileIndexTop,
            int tileIndexBottom,
            Vector4[] blockColors)
        {
            Tessellator t = Tessellator.Instance;
            t.StartDrawingTiledQuads();
            Vector4 c1, c2, c3, c4, c5, c6;
            float vx = -0.5f;
            float vy = -0.5f;
            float vz = -0.5f;
            c1 = blockColors[0];
            c2 = blockColors[1];
            c3 = blockColors[2];
            c4 = blockColors[3];
            c5 = blockColors[4];
            c6 = blockColors[5];
            float r = 0.5f;
            t.ArrayIndex = tileIndexFront;
            Vector3 normal = new Vector3(0, 0, 1);
            t.AddVertexWithColor(new Vector4(vx + 0f, vy + 0f, vz + 1f, 1.0f), c1 * r, normal);
            t.AddVertexWithColor(new Vector4(vx + 0f, vy + 1f, vz + 1f, 1.0f), c1, normal);
            t.AddVertexWithColor(new Vector4(vx + 1f, vy + 1f, vz + 1f, 1.0f), c1, normal);
            t.AddVertexWithColor(new Vector4(vx + 1f, vy + 0f, vz + 1f, 1.0f), c1 * r, normal);
            normal = new Vector3(0, 0, -1);
            t.AddVertexWithColor(new Vector4(vx + 1f, vy + 0f, vz + 0f, 1.0f), c2 * r, normal);
            t.AddVertexWithColor(new Vector4(vx + 1, vy + 1f, vz + 0f, 1.0f), c2, normal);
            t.AddVertexWithColor(new Vector4(vx + 0f, vy + 1f, vz + 0f, 1.0f), c2, normal);
            t.AddVertexWithColor(new Vector4(vx + 0f, vy + 0f, vz + 0f, 1.0f), c2 * r, normal);
            normal = new Vector3(-1, 0, 0);
            t.AddVertexWithColor(new Vector4(vx + 0f, vy + 0f, vz + 0f, 1.0f), c3 * r, normal);
            t.AddVertexWithColor(new Vector4(vx + 0f, vy + 1f, vz + 0f, 1.0f), c3, normal);
            t.AddVertexWithColor(new Vector4(vx + 0f, vy + 1f, vz + 1f, 1.0f), c3, normal);
            t.AddVertexWithColor(new Vector4(vx + 0f, vy + 0f, vz + 1f, 1.0f), c3 * r, normal);
            normal = new Vector3(1, 0, 0);
            t.AddVertexWithColor(new Vector4(vx + 1f, vy + 0f, vz + 1f, 1.0f), c4 * r, normal);
            t.AddVertexWithColor(new Vector4(vx + 1f, vy + 1f, vz + 1f, 1.0f), c4, normal);
            t.AddVertexWithColor(new Vector4(vx + 1f, vy + 1f, vz + 0f, 1.0f), c4, normal);
            t.AddVertexWithColor(new Vector4(vx + 1f, vy + 0f, vz + 0f, 1.0f), c4 * r, normal);
            t.ArrayIndex = tileIndexTop;
            normal = new Vector3(0, 1, 0);
            t.AddVertexWithColor(new Vector4(vx + 0f, vy + 1f, vz + 1f, 1.0f), c5, normal);
            t.AddVertexWithColor(new Vector4(vx + 0f, vy + 1f, vz + 0f, 1.0f), c5, normal);
            t.AddVertexWithColor(new Vector4(vx + 1f, vy + 1f, vz + 0f, 1.0f), c5, normal);
            t.AddVertexWithColor(new Vector4(vx + 1f, vy + 1f, vz + 1f, 1.0f), c5, normal);
            t.ArrayIndex = tileIndexBottom;
            normal = new Vector3(0, -1, 0);
            t.AddVertexWithColor(new Vector4(vx + 0f, vy + 0f, vz + 0f, 1.0f), c6 * r, normal);
            t.AddVertexWithColor(new Vector4(vx + 0f, vy + 0f, vz + 1f, 1.0f), c6 * r, normal);
            t.AddVertexWithColor(new Vector4(vx + 1f, vy + 0f, vz + 1f, 1.0f), c6 * r, normal);
            t.AddVertexWithColor(new Vector4(vx + 1f, vy + 0f, vz + 0f, 1.0f), c6 * r, normal);
            return t.GetVertexBuffer();
        }

        private void DefineBlock(int blockId, string top, string side, string bottom)
        {
            topBlockTextures[blockId] = indexMap[top];
            sideBlockTextures[blockId] = indexMap[side];
            bottomBlockTextures[blockId] = indexMap[bottom];
            BuildBlockVertexBuffer(blockId);

            BuildItemVertexBuffer(blockId, side);
        }

        private void LoadAllTileTextures()
        {
            Device device = Tessellator.Instance.Device;
            List<Texture2D> textures = new List<Texture2D>();
            List<string> allFiles = Directory.GetFiles("01.Frontend/Textures/Blocks/", "*.png").ToList();
            allFiles.AddRange(Directory.GetFiles("01.Frontend/Textures/Items/", "*.png"));

            ImageLoadInformation info = new ImageLoadInformation()
            {
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                FilterFlags = FilterFlags.None,
                Format = SlimDX.DXGI.Format.R8G8B8A8_UNorm,
                MipFilterFlags = FilterFlags.Point,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default,
                MipLevels = 2
            };

            foreach (string filename in allFiles)
            {
                textures.Add(Texture2D.FromFile(device, filename, info));
            }

            var textureArrayDescription = textures[0].Description;
            textureArrayDescription.ArraySize = textures.Count;
            var textureArray = new Texture2D(device, textureArrayDescription);
            var mipLevels = textureArrayDescription.MipLevels;
            for (int j = 0; j < textures.Count; j++)
            {
                indexMap.Add(Path.GetFileNameWithoutExtension(allFiles[j]), j);
                for (var i = 0; i < mipLevels; i++)
                {
                    // for both textures
                    device.ImmediateContext.CopySubresourceRegion(textures[j], i, textureArray, mipLevels * j + i, 0, 0, 0);
                }
            }
            View = new ShaderResourceView(device, textureArray);
        }


        internal int TopIndex(int blockId)
        {
            return topBlockTextures[blockId];
        }

        internal int SideIndex(int blockId)
        {
            return sideBlockTextures[blockId];
        }

        internal int BottomIndex(int blockId)
        {
            return bottomBlockTextures[blockId];
        }

        internal VertexBuffer GetBlockVertexBuffer(int blockId)
        {
            return blockVertexBuffers[blockId];
        }

        internal VertexBuffer GetItemVertexBuffer(int itemId)
        {
            return itemVertexBuffers[itemId];
        }

        internal VertexBuffer GetSelectionBlockVertexBuffer()
        {
            return selectionBlockBuffers;
        }

        internal VertexBuffer GetDestroyBlockVertexBuffer(float breakPercentage)
        {
            int index = (int)(breakPercentage / 10f);
            if (index < 0)
                index = 0;
            else if (index > 9)
                index = 9;
            return destroyBlocksBuffers[index];
        }
    }
}
