using System.Reflection;
using System.Text.Json;

namespace Beaversims.Core.Data.Dbc;

internal static partial class ItemDatabase
{
    public static readonly Dictionary<string, ItemData> Items = LoadFromResource();

    public static Dictionary<string, ItemData> LoadFromResource()
    {
        var asm = Assembly.GetExecutingAssembly();
        using var s = asm.GetManifestResourceStream("Beaversims.Core.Data.Dbc.Items.json")
                   ?? throw new FileNotFoundException("Embedded resource Items.json not found.");

        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var data = JsonSerializer.Deserialize<Dictionary<string, ItemData>>(s, opts);

        if (data == null)
            throw new InvalidDataException("Items.json deserialized to null.");

        return new Dictionary<string, ItemData>(data, StringComparer.OrdinalIgnoreCase);
    }
}

// Matches one stat entry: { "id": 74, "alloc": 5259 }
public sealed class StatAlloc
{
    public int Id { get; init; }
    public int Alloc { get; init; }
}

// Matches one socket: { "type": "PRISMATIC" }
public sealed class SocketSpec
{
    public string Type { get; init; } = "";
}

// Matches socketInfo: { "sockets": [ { "type": "PRISMATIC" }, ... ] }
public sealed class SocketInfo
{
    public SocketSpec[] Sockets { get; init; } = Array.Empty<SocketSpec>();
}

// Matches a full item object (your flat JSON entries)
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
    public SocketInfo? SocketInfo { get; init; }   // may be null/missing
    public StatAlloc[]? Stats { get; init; }       // may be null/missing
}