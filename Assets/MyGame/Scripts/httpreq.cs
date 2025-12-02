using UnityEngine;
using System.Net.Http;           // Für HTTP-Webanfragen
using System.Text.RegularExpressions; // Für Regex-Textsuche
using System.Threading.Tasks;    // Für async/await
using System.Collections.Generic; // Für List<T>

public class Httpreq : MonoBehaviour
{
    // Basis-Seite, auf der die Lehrer*innen gelistet sind.
    // Von dieser Seite holen wir die Detail-Links.
    private const string BASIS_URL = "https://www.htl-salzburg.ac.at/lehrerinnen.html";

    private void Start()
    {
        // Wird beim Start des Unity-Objekts ausgeführt.
        // Startet den gesamten Ablauf: Links holen → Detailseiten laden → Ausgabe.
        LadeAlleLehrerDaten();
    }

    async void LadeAlleLehrerDaten()
    {
        // Holt die ersten 5 Lehrer-Detail-URLs von der Übersicht
        List<string> detailLinks = await HoleErste5LehrerURLs();

        if (detailLinks.Count == 0)
        {
            Debug.LogError("Keine Lehrer-Links gefunden!");
            return; // Abbrechen, falls nichts gefunden wurde
        }

        // Debug-Ausgabe der gefundenen URLs
        Debug.Log($"Gefundene Detail-Links (bis zu 5):\n{string.Join("\n", detailLinks)}");

        // Gehe die Links durch und lade ihre HTML-Seiten
        for (int i = 0; i < detailLinks.Count; i++)
        {
            string html = await HoleDatenVonURL(detailLinks[i]);

            // Gibt gefundene Informationen im Debug-Log aus
            DebugAusgabe(i + 1, html);
        }
    }

    // -----------------------------------------------------------
    // Diese Methode lädt die Lehrer*innen-Liste und extrahiert
    // die ersten 5 Links zur Detail-Seite.
    // -----------------------------------------------------------
    async Task<List<string>> HoleErste5LehrerURLs()
    {
        List<string> links = new List<string>();

        // Holt HTML der Lehrer-Seite
        string html = await HoleDatenVonURL(BASIS_URL);
        if (string.IsNullOrEmpty(html)) return links;

        // Regex zum Finden der Detail-Links
        // Erklärung:
        //  href=" /lehrerinnen-details/irgendwas.html "
        //
        //  [^"] bedeutet "alle Zeichen außer Anführungszeichen"
        //  Dadurch fängt es *alle* gültigen Links ein, egal ob Sonderzeichen, Bindestriche, Groß-/Kleinschreibung etc.
        string pattern = @"href\s*=\s*""(?<link>\/lehrerinnen-details\/[^""]+\.html)""";

        // Sucht ALLE Vorkommen in der HTML-Seite
        MatchCollection matches = Regex.Matches(html, pattern, RegexOptions.IgnoreCase);

        Debug.Log($"Matches auf Übersichtsseite: {matches.Count}");

        // Gehe die gefundenen Matches durch
        foreach (Match m in matches)
        {
            if (links.Count >= 5) break; // Nur die ersten 5 benötigen wir

            string rel = m.Groups["link"].Value;  // Relativer Pfad (/lehrerinnen-details/...html)
            string full = "https://www.htl-salzburg.ac.at" + rel; // In absolute URL umwandeln

            // Prüft, ob der Link schon existiert (manchmal gibt's Duplikate)
            if (!links.Contains(full))
            {
                links.Add(full); // Zum Ergebnis hinzufügen
                Debug.Log($"hinzugefügt: {full}");
            }
            else
            {
                Debug.Log($"duplikat übersprungen: {full}");
            }
        }

        // Falls weniger als 5 gefunden wurden → ALLE Matches debuggen
        if (links.Count < 5)
        {
            Debug.LogWarning($"Nur {links.Count} Lehrer-Links gefunden. Alle Matches:");
            foreach (Match m in matches)
            {
                Debug.Log($"match: {m.Groups["link"].Value}");
            }
        }

        return links;
    }

    // -----------------------------------------------------------
    // Lädt HTML-Inhalt einer URL mit HttpClient
    // async/await = nicht blockierend, ideal für Unity
    // -----------------------------------------------------------
    async Task<string> HoleDatenVonURL(string url)
    {
        using (HttpClient client = new HttpClient()) // HttpClient erzeugen
        {
            try
            {
                // Webanfrage starten (GET)
                var response = await client.GetAsync(url);

                // Fehler werfen, falls Status-Code nicht OK (200)
                response.EnsureSuccessStatusCode();

                // HTML-Text der Antwort zurückgeben
                return await response.Content.ReadAsStringAsync();
            }
            catch (System.Exception e)
            {
                // Fehlerausgabe für Debugging
                Debug.LogError($"Fehler beim Laden von {url}: {e.Message}");
                return ""; // Leere Seite zurückgeben, damit nichts crasht
            }
        }
    }

    // -----------------------------------------------------------
    // Extrahiert Name / Raum / Sprechstunde aus der HTML-Seite
    // und gibt sie im Debug-Fenster aus
    // -----------------------------------------------------------
    void DebugAusgabe(int nummer, string htmlContent)
    {
        // Regex um den Namen zu extrahieren
        // Die Lehrer-Detailseite hat den Namen in:
        //
        // <h1 class="value"><span class="text">NAME</span>
        //
        // .*? = nicht-gieriges Match (so wenig wie möglich)
        string nameMuster = @"<h1 class=""value"">\s*<span class=""text"">(.*?)<\/span>";

        // Regex für Raum
        // Wird in einem eigenen <div> angezeigt:
        // <div class="field Raum">  <span class="text">...</span>
        string raumMuster = @"field Raum"".*?<span class=""text"">(.*?)<\/span>";

        // Regex für Sprechstunde
        string stundeMuster = @"field SprStunde"".*?<span class=""text"">(.*?)<\/span>";

        // Match extrahieren (Group[1] = Inhalt zwischen den Klammern)
        string name = Regex.Match(htmlContent, nameMuster, RegexOptions.Singleline).Groups[1].Value.Trim();
        string raum = Regex.Match(htmlContent, raumMuster, RegexOptions.Singleline).Groups[1].Value.Trim();
        string stunde = Regex.Match(htmlContent, stundeMuster, RegexOptions.Singleline).Groups[1].Value.Trim();

        // Debug-Ausgabe formatieren
        Debug.Log(
            $"Lehrer #{nummer}\n" +
            $"Name: {name}\n" +
            $"Raum: {raum}\n" +
            $"Sprechstunde: {stunde}\n"
        );
    }
}
