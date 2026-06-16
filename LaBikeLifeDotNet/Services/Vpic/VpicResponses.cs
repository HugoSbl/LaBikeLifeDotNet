namespace LaBikeLifeDotNet.Services.Vpic;

// vPIC répond toujours pareil : { Count, Message, Results: [...] }
public class VpicListResponse<T>
{
    public int Count { get; set; }
    public string? Message { get; set; }
    public List<T> Results { get; set; } = [];
}

public class VpicMake
{
    public int MakeId { get; set; }
    public string MakeName { get; set; } = "";
}

// attention : côté JSON les clés ont des underscores, faut respecter exactement
public class VpicModel
{
    public int Make_ID { get; set; }
    public string Make_Name { get; set; } = "";
    public int Model_ID { get; set; }
    public string Model_Name { get; set; } = "";
}

// les champs du VIN qu'on garde (vPIC en renvoie une tonne d'autres)
public class VpicVinValues
{
    public string? Make { get; set; }
    public string? Model { get; set; }
    public string? ModelYear { get; set; }
    public string? DisplacementCC { get; set; }
    public string? EngineCylinders { get; set; }
    public string? FuelTypePrimary { get; set; }
    public string? BrakeSystemType { get; set; }
    public string? BodyClass { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorText { get; set; }
}
