using ExileCore.PoEMemory.Components;
using ExileCore.Shared.Enums;
using ImGuiNET;
using RareBeasts.Data;
using RareBeasts.ExileCore;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace RareBeasts;

public partial class Beasts
{
    public override void Render()
    {
        DrawBestiaryPanel();

        var inventoryItems = GameController.IngameState.ServerData.PlayerInventories[0].Inventory.InventorySlotItems;

        // track each inventory slot
        InventoryPlayer[,] inventorySlot = new InventoryPlayer[12, 5];

        //get pos inventory slots
        var defRect = inventoryItems.Where(item => item.PosX == 0 && item.PosY == 0).FirstOrDefault();

        var firtItem = inventoryItems.Where(item => item.PosX == 0 && item.PosY == 0).FirstOrDefault();

        float offsetX = firtItem.GetClientRect().TopLeft.X;
        float offsetY = firtItem.GetClientRect().TopLeft.Y;

        this.windowOffset = this.GameController.Window.GetWindowRectangle().TopLeft;

        for (int x = 0; x < 12; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                RectangleF rectSlot = new RectangleF(offsetX, offsetY, defRect.GetClientRect().Width, defRect.GetClientRect().Height);

                Graphics.DrawFrame(rectSlot, Color.White, 2);

                offsetY += defRect.GetClientRect().Width;
            }
            offsetY = firtItem.GetClientRect().TopLeft.Y;
            offsetX += defRect.GetClientRect().Height;
        }

    }

    private void DrawBestiaryPanel()
    {
        var bestiary = GameController.IngameState.IngameUi.GetBestiaryPanel();
        if (bestiary == null || bestiary.IsVisible == false) return;

        var capturedBeastsPanel = bestiary.CapturedBeastsPanel;
        if (capturedBeastsPanel == null || capturedBeastsPanel.IsVisible == false) return;

        var beasts = bestiary.CapturedBeastsPanel.CapturedBeasts;

        var rectCaptureBeast = bestiary.CapturedBeastsPanel.GetChildFromIndices(1).GetClientRectCache;

        foreach (var beast in beasts)
        {
            var beastRect = beast.GetClientRect().Center;
            if (!rectCaptureBeast.Contains(beastRect)) continue;

            var capturedBeast = BeastsDatabase.AllBeasts.Find(b => b.DisplayName == beast.DisplayName);
            if (capturedBeast == null)
            {
                Graphics.DrawFrame(beast.GetClientRect(), Color.White, 2);
                Graphics.DrawFrame(beast.GetChildAtIndex(3).GetClientRect(), Color.White, 2);

            }
            else
            {
                Graphics.DrawFrame(beast.GetClientRect(), Color.Red, 2);
                Graphics.DrawFrame(beast.GetChildAtIndex(3).GetClientRect(), Color.Red, 2);
            }
        }
    }

    private void DrawFilledCircleInWorldPosition(Vector3 position, float radius, Color color)
    {
        var circlePoints = new List<Vector2>();
        const int segments = 15;
        const float segmentAngle = 2f * MathF.PI / segments;

        for (var i = 0; i < segments; i++)
        {
            var angle = i * segmentAngle;
            var currentOffset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
            var nextOffset = new Vector2(MathF.Cos(angle + segmentAngle), MathF.Sin(angle + segmentAngle)) * radius;

            var currentWorldPos = position + new Vector3(currentOffset, 0);
            var nextWorldPos = position + new Vector3(nextOffset, 0);

            circlePoints.Add(GameController.Game.IngameState.Camera.WorldToScreen(currentWorldPos));
            circlePoints.Add(GameController.Game.IngameState.Camera.WorldToScreen(nextWorldPos));
        }

        Graphics.DrawConvexPolyFilled(circlePoints.ToArray(), color with { A = Color.ToByte((int)((double)0.2f * byte.MaxValue)) });
        Graphics.DrawPolyLine(circlePoints.ToArray(), color, 2);
    }

}