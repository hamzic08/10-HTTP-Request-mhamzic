# 10-HTTP-Request-mhamzic

## README – Lehrer*innen-Scraper für Unity

Dieses Skript lädt automatisch die ersten fünf Lehrer*innen-Detailseiten der HTL Salzburg, liest deren HTML aus und extrahiert Name, Raum und Sprechstunde per Regex.
Es zeigt, wie man in Unity HttpClient, async/await und Regex gemeinsam verwendet.

🔧 Was passiert hier?

Start()

- Startet automatisch beim Laden des GameObjects.
- Ruft LadeAlleLehrerDaten() auf.

LadeAlleLehrerDaten()

- Holt bis zu fünf Detail-Links von der Lehrer*innen-Übersichtsseite.
- Lädt jede dieser Seiten einzeln.
- Übergibt die HTML-Inhalte an DebugAusgabe().

HoleErste5LehrerURLs()

- Lädt die Übersichtsseite.
- Findet alle Detail-Links über Regex
- (/lehrerinnen-details/...html).
- Wandelt relative Links in absolute URLs um.
- Entfernt Duplikate, gibt max. fünf Links zurück.

HoleDatenVonURL(url)

- Führt eine HTTP-GET-Anfrage aus.
- Gibt den HTML-Text zurück.
- Fängt Fehler ab, damit Unity nicht abstürzt.

DebugAusgabe()

- Sucht per Regex nach Name, Raum und Sprechstunde.
- Gibt die Infos sauber formatiert in der Console aus.
