using System.Reflection;
using System.Text.Json;

namespace Beaversims.Core.Data.Dbc;

internal static partial class ItemDatabase
{
    // Now keyed by item ID
    public static readonly Dictionary<int, ItemData> Items = LoadFromResource();

    private static Dictionary<int, ItemData> LoadFromResource()
    {
        var asm = Assembly.GetExecutingAssembly();
        using var s = asm.GetManifestResourceStream("Beaversims.Core.Data.Dbc.equippable-items-full.json")
                   ?? throw new FileNotFoundException("Embedded resource Items.json not found.");

        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // JSON is now: [ { "id": 25, "name": "...", ... }, ... ]
        var array = JsonSerializer.Deserialize<ItemData[]>(s, opts)
                    ?? throw new InvalidDataException("Items.json deserialized to null.");

        var dict = new Dictionary<int, ItemData>(array.Length);
        foreach (var item in array)
        {
            dict[item.Id] = item;
        }

        return dict;
    }
}

// rest of your types stay the same
public sealed class StatAlloc
{
    public int Id { get; init; }
    public int Alloc { get; init; }
}

public sealed class SocketSpec
{
    public string Type { get; init; } = "";
}

public sealed class SocketInfo
{
    public SocketSpec[] Sockets { get; init; } = Array.Empty<SocketSpec>();
}

public sealed class ItemData
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Icon { get; init; } = "";
    public int Quality { get; init; }
    public int ItemClass { get; init; }
    public int ItemSubClass { get; init; }
    public int InventoryType { get; init; }
    public int ItemLevel { get; init; }
    public int Expansion { get; init; }

    public bool HasSockets { get; init; }
    public SocketInfo? SocketInfo { get; init; }
    public StatAlloc[]? Stats { get; init; }
}