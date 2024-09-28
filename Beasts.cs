using ExileCore;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using RareBeasts.Data;
using RareBeasts.ExileCore;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace RareBeasts;

public partial class Beasts : BaseSettingsPlugin<BeastsSettings>
{
    private readonly Dictionary<long, Entity> _trackedBeasts = new();

    private static Random random = new Random();
    private SharpDX.Vector2 windowOffset;

    SlotInventory[,] playerInventory = new SlotInventory[12, 5];

    Dictionary<string, List<SlotInventory>> stashTabs = [];

    private bool prevActionRelease = false;
    private bool inventoryIsFull = false;
    int startTime = 0;

    public override void OnLoad()
    {
        Name = "RareBeast";
    }

    public override bool Initialise()
    {

        this.windowOffset = this.GameController.Window.GetWindowRectangle().TopLeft;
        return base.Initialise();
    }

    public override Job Tick()
    {
        if (Settings.StartStopHotKey.PressedOnce())
        {
            Settings.WorkGrabber.Value = !Settings.WorkGrabber.Value;
            startTime = Environment.TickCount;

            Utils.Mouse.MouseMoveNonLinear(this.GameController.Window.GetWindowRectangle().Center);

        }

        if (Input.GetKeyState(Settings.StopHotKey.Value))
        {
            Settings.WorkGrabber.Value = false;
            Settings.WorkStasher.Value = false;

            return null;
        }

        if (!Settings.FoldToStash.Value && inventoryIsFull)
        {
            inventoryIsFull = false;
            Settings.WorkGrabber.Value = false;
            Settings.WorkStasher.Value = false;
        }

        if (inventoryIsFull && Settings.WorkStasher.Value)
        {
            UpdateInventoryPlayer();

            if (OpenStash())
            {
                Thread.Sleep(500);
                if (OpenTab(Settings.BeastTabName.Value))
                {
                    Thread.Sleep(500);
                    if (AllItemsToStash())
                    {
                        Thread.Sleep(500);
                        if (TakeConsumablesFromTab(Settings.ConsumablesTabName.Value))
                        {
                            inventoryIsFull = false;
                            Settings.WorkGrabber.Value = true;
                            Settings.WorkStasher.Value = false;
                        }
                    }
                }
            }
        }

        if (Settings.WorkGrabber.Value)
        {
            if ((Environment.TickCount - startTime) < Settings.DSettings.BeastDelay)
                return null;

            this.windowOffset = this.GameController.Window.GetWindowRectangle().TopLeft;

            Work();
            startTime = Environment.TickCount;
            DestroyWindowCheck();

            if (Input.GetKeyState(Settings.StopHotKey.Value))
            {
                Settings.WorkGrabber.Value = false;
                return null;
            }
        }

        return null;
    }

    private void Work()
    {
        if (!Settings.WorkGrabber) return;

        Vector2 freeSlot = SearchFreeSpace();

        if (freeSlot.IsZero)
        {
            Settings.WorkGrabber.Value = !Settings.WorkGrabber.Value;
            inventoryIsFull = true;
            Settings.WorkStasher.Value = true;

            return;
        }

        var challengesPanel = GameController.IngameState.IngameUi.ChallengesPanel;
        if (challengesPanel == null || challengesPanel.IsVisible == false)
        {
            Utils.Keyboard.KeyPress(System.Windows.Forms.Keys.H);
        }

        var captureButton = GameController.IngameState.IngameUi.GetChildFromIndices(46,2,0,1,1,65,0,19,0);

        //(OpenLeftPanel/SentinelWindow/ChallengesPanel)46->2->0->1->1->65->0->19->1
        var bestiaryTab = challengesPanel.TabContainer.BestiaryTab;
        if (bestiaryTab == null) return;

        if (bestiaryTab.IsVisible == false)
        {
            var tabBestiary = FindChildRecursiveLocal(challengesPanel, "Bestiary");
            if (tabBestiary != null)
            {
                Utils.Mouse.MoveMouse(tabBestiary.GetClientRectCache.Center + windowOffset);
                Utils.Mouse.LeftDown(1);
                Utils.Mouse.LeftUp(1);
            }
        }
        else
        {
            if (bestiaryTab.CapturedBeastsTab == null || bestiaryTab.CapturedBeastsTab.IsVisible == false)
            {
                if (captureButton != null && captureButton.IsVisible)
                {
                    Utils.Mouse.MoveMouse(captureButton.GetClientRectCache.Center + windowOffset);
                    if(captureButton.HasShinyHighlight)
                    {
                        Utils.Mouse.LeftDown(1);
                        Utils.Mouse.LeftUp(1);
                    }
                }
            }
        }

        var inventory = GameController.IngameState.IngameUi.InventoryPanel;
        if (inventory == null || inventory.IsVisible == false)
        {
            Utils.Keyboard.KeyPress(System.Windows.Forms.Keys.I);
        }

        var bestiary = GameController.IngameState.IngameUi.GetBestiaryPanel();
        if (bestiary == null || bestiary.IsVisible == false) return;

        var capturedBeastsPanel = bestiary.CapturedBeastsPanel;
        if (capturedBeastsPanel == null || capturedBeastsPanel.IsVisible == false) return;

        var beasts = bestiary.CapturedBeastsPanel.CapturedBeasts;

        if (!beasts.Any())
        {
            Settings.WorkGrabber.Value = !Settings.WorkGrabber.Value;
            return;
        }

        var rectCaptureBeast = bestiary.CapturedBeastsPanel.GetChildFromIndices(1).GetClientRectCache;

        var beast = beasts.FirstOrDefault();

        var beastRect = beast.GetClientRect().Center;
        if (!rectCaptureBeast.Contains(beastRect)) return;

        var releaseButton = beast.GetChildAtIndex(3).GetClientRect();
        var rectBeast = beast.GetClientRect();

        var capturedBeast = BeastsDatabase.AllBeasts.Find(b => b.DisplayName == beast.DisplayName);
        if (capturedBeast == null)
        {
            prevActionRelease = false;

            if (GetBestiaryOrb())
            {
                GrabBeast(rectBeast.Center);
                if (!PlaceBeast())
                {
                    LogMessage("Inventory Error");

                    return;
                }
            }
            else
            {
                LogMessage("Error Grab Orb");
                Settings.WorkGrabber.Value = !Settings.WorkGrabber.Value;

                return;
            }
        }
        else
        {
            ReleaseBeast(releaseButton.Center);

            prevActionRelease = true;

            Thread.Sleep(Settings.DSettings.ActionDelay * 2);
        }

    }

    public void DestroyWindowCheck()
    {
        var destroyWindow = GameController.IngameState.IngameUi.DestroyConfirmationWindow;
        if (destroyWindow.IsVisible)
        {
            Thread.Sleep(Settings.DSettings.MouseClickDelay * 3);
            Utils.Keyboard.KeyDown(System.Windows.Forms.Keys.Escape);
            Thread.Sleep(Settings.DSettings.MouseClickDelay);
            Utils.Keyboard.KeyUp(System.Windows.Forms.Keys.Escape);
            Thread.Sleep(Settings.DSettings.MouseClickDelay);
        }
    }

    public bool ReleaseBeast(Vector2 pos)
    {

        this.windowOffset = this.GameController.Window.GetWindowRectangle().TopLeft;

        if (!prevActionRelease)
            Utils.Mouse.MouseMoveNonLinear(pos + windowOffset);

        Utils.Keyboard.KeyDown(System.Windows.Forms.Keys.LControlKey);

        Utils.Mouse.LeftDown(Settings.DSettings.MouseClickDelay);
        Utils.Mouse.LeftUp(Settings.DSettings.MouseClickDelay);

        Utils.Keyboard.KeyUp(System.Windows.Forms.Keys.LControlKey);

        Thread.Sleep(Settings.DSettings.ActionDelay);

        return true;
    }

    public bool PlaceBeast()
    {
        Vector2 freeSlot = SearchFreeSpace();

        if (freeSlot.IsZero) return false;

        this.windowOffset = this.GameController.Window.GetWindowRectangle().TopLeft;

        Utils.Mouse.MouseMoveNonLinear(freeSlot + windowOffset);
        Thread.Sleep(Settings.DSettings.ActionDelay);

        Utils.Mouse.LeftDown(Settings.DSettings.MouseClickDelay);
        Utils.Mouse.LeftUp(Settings.DSettings.MouseClickDelay);

        Thread.Sleep(Settings.DSettings.ActionDelay);

        var itemsOnCursor = GameController.IngameState.ServerData.PlayerInventories.Where(x=>x.TypeId == InventoryNameE.Cursor1);
        if(itemsOnCursor.Any())
        {
            if(itemsOnCursor.First().Inventory.ItemCount > 0)
            {
                PlaceBeast();
            }
        }

        return true;
    }

    private SharpDX.Vector2 SearchFreeSpace()
    {
        var inventoryItems = GameController.IngameState.ServerData.PlayerInventories[0].Inventory.InventorySlotItems;
        // quick sanity check
        if (inventoryItems.Count > 60)
        {
            return new Vector2(0, 0);
        }

        // track each inventory slot
        InventoryPlayer[,] inventorySlot = new InventoryPlayer[12, 5];

        //get pos inventory slots
        var inventoryRect = GameController.IngameState.IngameUi.GetChildFromIndices(37, 3, 27).GetClientRectCache;
        var invSlotW = inventoryRect.Width / 12;
        var invSlotH = inventoryRect.Height / 5;

        float offsetX = inventoryRect.X;
        float offsetY = inventoryRect.Y;

        for (int x = 0; x < 12; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                RectangleF rectSlot = new RectangleF(offsetX, offsetY, invSlotW, invSlotH);

                inventorySlot[x, y] = new InventoryPlayer(false, rectSlot);

                offsetY += invSlotW;
            }
            offsetY = inventoryRect.Y;
            offsetX += invSlotH;
        }

        // iterate through each item in the inventory and mark used slots
        foreach (var inventoryItem in inventoryItems)
        {
            int x = inventoryItem.PosX;
            int y = inventoryItem.PosY;
            int height = inventoryItem.SizeY;
            int width = inventoryItem.SizeX;
            for (int row = x; row < x + width; row++)
            {
                for (int col = y; col < y + height; col++)
                {
                    inventorySlot[row, col] = new InventoryPlayer(true, inventoryItem.GetClientRect());
                }
            }
        }

        // check for any empty slots
        for (int x = 0; x < 12; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                if (inventorySlot[x, y].full == false)
                {
                    return inventorySlot[x, y].rect.Center;
                }
            }
        }

        // no empty slots, so inventory is full
        return new Vector2(0, 0);
    }

    public bool GrabBeast(Vector2 beastPos)
    {
        this.windowOffset = this.GameController.Window.GetWindowRectangle().TopLeft;

        Utils.Mouse.MouseMoveNonLinear(beastPos + windowOffset);
        Thread.Sleep(Settings.DSettings.ActionDelay);

        Utils.Mouse.LeftDown(Settings.DSettings.MouseClickDelay);
        Utils.Mouse.LeftUp(Settings.DSettings.MouseClickDelay);

        Thread.Sleep(Settings.DSettings.ActionDelay);

        return true;
    }

    public bool GetBestiaryOrb()
    {
        string bsOrb = "Metadata/Items/Currency/CurrencyItemiseCapturedMonster";

        var playerInventory = GameController.IngameState.ServerData.PlayerInventories[0].Inventory.InventorySlotItems;

        var bestiartOrbs = playerInventory.Where(item => item.Item.Metadata == bsOrb).ToList();

        if (!bestiartOrbs.Any()) return false;

        this.windowOffset = this.GameController.Window.GetWindowRectangle().TopLeft;

        var firstItem = bestiartOrbs.FirstOrDefault();
        if (firstItem == null)
        {
            LogMessage("Could not find item.");
            return false;
        }

        Vector2 itemPos = firstItem.GetClientRect().Center;

        Utils.Mouse.MouseMoveNonLinear(itemPos + windowOffset);
        Thread.Sleep(Settings.DSettings.ActionDelay);

        var sss = GameController.IngameState.UIHover?.Entity?.Metadata;

        Utils.Mouse.RightDown(Settings.DSettings.MouseClickDelay);
        Utils.Mouse.RightUp(Settings.DSettings.MouseClickDelay);

        Thread.Sleep(Settings.DSettings.ActionDelay);

        return true;
    }


    public override void DrawSettings()
    {
        base.DrawSettings();


    }
}