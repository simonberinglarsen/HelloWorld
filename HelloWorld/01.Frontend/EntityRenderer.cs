﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsFormsApplication7.Business;
using WindowsFormsApplication7.CrossCutting.Entities;
using SlimDX;
using SlimDX.Direct3D11;
using WindowsFormsApplication7._01.Frontend;
using WindowsFormsApplication7.Business.Repositories;

namespace WindowsFormsApplication7.Frontend
{
    class EntityRenderer
    {
        Entity entity;
        SlimDX.Direct3D11.Buffer vertices;
        int vertexCount;

        public EntityRenderer(Entity entity)
        {
            this.entity = entity;
            Build();
        }

        internal void Build()
        {
            Tessellator t = Tessellator.Instance;

            t.StartDrawingColoredQuads();
            Vector4 c = entity.Color;
            Vector3 min = entity.AABB.Min;
            Vector3 max = entity.AABB.Max;
            Vector4[] v = new Vector4[] {
            new Vector4(min.X, min.Y, min.Z,1),
            new Vector4(max.X, min.Y, min.Z,1),
            new Vector4(max.X, min.Y, max.Z,1),
            new Vector4(min.X, min.Y, max.Z,1),
            new Vector4(min.X, max.Y, min.Z,1),
            new Vector4(max.X, max.Y, min.Z,1),
            new Vector4(max.X, max.Y, max.Z,1),
            new Vector4(min.X, max.Y, max.Z,1),
            };

            // left
            t.AddVertexWithColor(v[0], c);
            t.AddVertexWithColor(v[4], c);
            t.AddVertexWithColor(v[7], c);
            t.AddVertexWithColor(v[3], c);

            //front
            t.AddVertexWithColor(v[3], c);
            t.AddVertexWithColor(v[7], c);
            t.AddVertexWithColor(v[6], c);
            t.AddVertexWithColor(v[2], c);

            //right
            t.AddVertexWithColor(v[2], c);
            t.AddVertexWithColor(v[6], c);
            t.AddVertexWithColor(v[5], c);
            t.AddVertexWithColor(v[1], c);

            //back
            t.AddVertexWithColor(v[1], c);
            t.AddVertexWithColor(v[5], c);
            t.AddVertexWithColor(v[4], c);
            t.AddVertexWithColor(v[0], c);

            //top
            t.AddVertexWithColor(v[4], c);
            t.AddVertexWithColor(v[5], c);
            t.AddVertexWithColor(v[6], c);
            t.AddVertexWithColor(v[7], c);

            //bottom
            t.AddVertexWithColor(v[0], c);
            t.AddVertexWithColor(v[3], c);
            t.AddVertexWithColor(v[2], c);
            t.AddVertexWithColor(v[1], c);

            vertices = t.GetDrawBuffer();
            vertexCount = t.VertexCount;
        }

        internal void Render(float partialStep)
        {
            Tessellator t = Tessellator.Instance;
            Vector3 position = Interpolate.Position(entity, partialStep);

            if (entity != TheGame.Instance.entityToControl)
            {
                t.StartDrawingColoredQuads();
                Camera.Instance.World = Matrix.Multiply(Matrix.RotationYawPitchRoll(entity.Yaw, 0, 0), Matrix.Translation(position));
                t.Draw(vertices, vertexCount);
            }
           
            if (typeof(Player) == entity.GetType() && World.Instance.Player.BreakCompletePercentage > 0)
            {
                t.StartDrawingTiledQuads2();
                t.Translate.X = World.Instance.Player.BreakPosition.X + 0.5f;
                t.Translate.Y = World.Instance.Player.BreakPosition.Y + 0.5f;
                t.Translate.Z = World.Instance.Player.BreakPosition.Z + 0.5f;
                float s = 1.005f;
                t.Scale = new Vector3(s, s, s);
                t.Draw(TileTextures.Instance.GetDestroyBlockVertexBuffer(World.Instance.Player.BreakCompletePercentage));
                t.ResetTransformation();
            }
        }

        public void Dispose()
        {
            vertices.Dispose();
        }
    }
}
