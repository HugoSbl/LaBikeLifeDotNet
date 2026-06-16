using System.Net.Http.Json;
using System.Text.Json;

namespace LaBikeLifeDotNet.Services;

/// <summary>
/// Contrat d'abstraction pour le service de résolution d'illustrations.
/// </summary>
/// <remarks>
/// Cette interface découple la couche applicative de l'implémentation concrète de la source
/// d'imagerie — ce qui facilite la testabilité (substitution par un double de test) ainsi qu'une
/// éventuelle évolution vers un fournisseur alternatif sans impact sur les appelants.
/// </remarks>
public interface IWikipediaImageService
{
    /// <summary>
    /// Résout de manière asynchrone l'URL d'une illustration pertinente pour une requête textuelle donnée.
    /// </summary>
    /// <param name="query">
    /// La requête de recherche en langage naturel — typiquement la concaténation de la marque, du
    /// modèle et d'un qualificatif de domaine afin d'optimiser la précision sémantique.
    /// </param>
    /// <param name="ct">Le jeton d'annulation coopératif propagé tout au long de la chaîne d'appels.</param>
    /// <returns>
    /// L'URL absolue de la vignette du meilleur candidat, ou <c>null</c> lorsqu'aucune correspondance
    /// exploitable n'a pu être déterminée.
    /// </returns>
    Task<string?> GetImageUrlAsync(string query, CancellationToken ct = default);
}

/// <summary>
/// Implémentation du service d'imagerie s'appuyant sur l'API publique MediaWiki de Wikipedia
/// (gratuite et sans clé d'API).
/// </summary>
/// <remarks>
/// La stratégie de résolution procède en deux temps via le module <c>query</c> de MediaWiki :
/// <list type="number">
///   <item><description>un générateur de recherche (<c>generator=search</c>) identifie les articles les plus pertinents ;</description></item>
///   <item><description>la propriété <c>pageimages</c> expose, pour chacun, la vignette de tête (<c>thumbnail</c>).</description></item>
/// </list>
/// Le candidat retenu est celui dont l'indice de pertinence est le plus faible parmi ceux disposant
/// effectivement d'une vignette. Toute défaillance réseau ou de désérialisation est interceptée et
/// dégradée silencieusement en <c>null</c> — la résolution d'image étant strictement non bloquante
/// au regard du parcours utilisateur principal.
/// </remarks>
public class WikipediaImageService(HttpClient http, ILogger<WikipediaImageService> logger) : IWikipediaImageService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    /// <inheritdoc />
    public async Task<string?> GetImageUrlAsync(string query, CancellationToken ct = default)
    {
        // Garde défensive : une requête vide ou exclusivement composée d'espaces ne saurait produire
        // de résultat exploitable — on court-circuite donc tout appel réseau superflu.
        if (string.IsNullOrWhiteSpace(query)) return null;

        try
        {
            // Étape 1 — Construction de l'URI d'interrogation de l'API MediaWiki.
            //   • prop=pageimages + piprop=thumbnail : on ne sollicite que la vignette de tête ;
            //   • pithumbsize=480              : largeur cible, compromis qualité / poids ;
            //   • generator=search             : délègue à MediaWiki la pertinence du classement ;
            //   • gsrnamespace=0               : restreint l'espace de noms aux seuls articles encyclopédiques ;
            //   • gsrlimit=3                   : trois candidats suffisent à couvrir les variations de libellé.
            var url = "w/api.php?action=query&format=json&prop=pageimages&piprop=thumbnail" +
                      "&pithumbsize=480&generator=search&gsrnamespace=0&gsrlimit=3&gsrsearch=" +
                      Uri.EscapeDataString(query);

            // Étape 2 — Invocation HTTP asynchrone et désérialisation fortement typée de la charge utile.
            var resp = await http.GetFromJsonAsync<WikiResponse>(url, JsonOpts, ct);
            var pages = resp?.Query?.Pages?.Values;
            if (pages is null) return null;

            // Étape 3 — Sélection déterministe : on filtre les pages pourvues d'une vignette, on ordonne
            //           par indice de pertinence croissant, puis on projette sur l'URL source du meilleur élément.
            return pages
                .Where(p => !string.IsNullOrWhiteSpace(p.Thumbnail?.Source))
                .OrderBy(p => p.Index)
                .Select(p => p.Thumbnail!.Source)
                .FirstOrDefault();
        }
        catch (Exception ex)
        {
            // Dégradation gracieuse : l'absence d'illustration n'est pas une erreur fonctionnelle.
            logger.LogWarning(ex, "Échec récupération image Wikipedia pour {Query}", query);
            return null;
        }
    }

    // Types de transport (DTO) calqués sur la structure de la réponse MediaWiki.
    private sealed class WikiResponse
    {
        public WikiQuery? Query { get; set; }
    }

    private sealed class WikiQuery
    {
        public Dictionary<string, WikiPage>? Pages { get; set; }
    }

    private sealed class WikiPage
    {
        public int Index { get; set; }
        public string? Title { get; set; }
        public WikiThumb? Thumbnail { get; set; }
    }

    private sealed class WikiThumb
    {
        public string? Source { get; set; }
    }
}
