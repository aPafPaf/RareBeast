using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using RareBeasts.Data;
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Helpers;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using ImGuiNET;
using Newtonsoft.Json;
using SharpDX;

namespace RareBeasts;

public class BeastsSettings : ISettings
{
    public List<Beast> Beasts { get; set; } = new();
    public Dictionary<string, float> BeastPrices { get; set; } = new();
    public DateTime LastUpdate { get; set; } = DateTime.MinValue;

    public ToggleNode Enable { get; set; } = new ToggleNode(false);

    public ButtonNode FetchBeastPrices { get; set; } = new ButtonNode();

    [JsonIgnore] public CustomNode LastUpdated { get; set; }

    [JsonIgnore] public CustomNode BeastPicker { get; set; }

    [Menu("Start")]
    public HotkeyNode StartStopHotKey { get; set; } = new HotkeyNode(Keys.F7);

    [Menu("Stop")]
    public HotkeyNode StopHotKey { get; set; } = new HotkeyNode(Keys.Delete);

    [Menu("Test")]
    public HotkeyNode TestHotKey { get; set; } = new HotkeyNode(Keys.NumPad0);

    [Menu("Beast Tab Name")]
    public TextNode BeastTabName { get; set; } = new();

    [Menu("Consumables Tab Name")]
    public TextNode ConsumablesTabName { get; set; } = new();

    public ToggleNode WorkGrabber { get; set; } = new ToggleNode(false);

    public ToggleNode WorkStasher { get; set; } = new ToggleNode(false);

    public ToggleNode OrbCheck { get; set; } = new ToggleNode(false);

    public DSettings DSettings { get; set; } = new();
}

[Submenu(CollapsedByDefault = true)]
public class DSettings
{
    [Menu("MouseStep")]
    public RangeNode<int> MouseStep { get; set; } = new RangeNode<int>(1, 1, 20);

    [Menu("MouseClickDelay")]
    public RangeNode<int> MouseClickDelay { get; set; } = new RangeNode<int>(50, 30, 300);

    [Menu("Action Delay")]
    public RangeNode<int> ActionDelay { get; set; } = new RangeNode<int>(0, 0, 2000);

    [Menu("Check Delay")]
    public RangeNode<int> CheckDelay { get; set; } = new RangeNode<int>(0, 10, 2000);

    [Menu("Delay per Beast")]
    public RangeNode<int> BeastDelay { get; set; } = new RangeNode<int>(0, 10, 20000);
}