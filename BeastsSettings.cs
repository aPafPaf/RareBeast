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

    [Menu("Start/Stop")]
    public HotkeyNode StartStopHotKey { get; set; } = new HotkeyNode(Keys.Space);

    public ToggleNode Work { get; set; } = new ToggleNode(false);

    public ToggleNode OrbCheck { get; set; } = new ToggleNode(false);

    public DSettings DSettings { get; set; } = new();
}

[Submenu(CollapsedByDefault = true)]
public class DSettings
{
    [Menu("MouseStep")]
    public RangeNode<int> MouseStep { get; set; } = new RangeNode<int>(1, 1, 20);

    [Menu("MouseStepDelayMin")]
    public RangeNode<int> MouseStepDelayMin { get; set; } = new RangeNode<int>(50, 0, 300);

    [Menu("MouseStepDelayMax")]
    public RangeNode<int> MouseStepDelayMax { get; set; } = new RangeNode<int>(50, 0, 300);

    [Menu("MouseClickDelay")]
    public RangeNode<int> MouseClickDelay { get; set; } = new RangeNode<int>(50, 30, 300);

    [Menu("MouseMoveDelay")]
    public RangeNode<int> MouseMoveDelay { get; set; } = new RangeNode<int>(50, 10, 300);

    [Menu("Action Delay")]
    public RangeNode<int> ActionDelay { get; set; } = new RangeNode<int>(0, 10, 2000);

    [Menu("Check Delay")]
    public RangeNode<int> CheckDelay { get; set; } = new RangeNode<int>(0, 10, 2000);
}