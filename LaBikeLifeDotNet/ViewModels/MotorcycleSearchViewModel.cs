namespace LaBikeLifeDotNet.ViewModels;

// juste ce qu'il faut pour remplir le formulaire de recherche (les listes déroulantes)
public class MotorcycleSearchViewModel
{
    public IReadOnlyList<string> Makes { get; set; } = [];
    public IReadOnlyList<int> Years { get; set; } = [];
}
