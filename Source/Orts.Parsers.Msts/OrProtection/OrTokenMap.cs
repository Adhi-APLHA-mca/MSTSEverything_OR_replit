// OR Route Protection — Layer 1: Token remapping
// Maps every known STF token used in .trk / .tit / .tdb / .rdb / .rit files
// to a short opaque code.  Only this assembly knows the mapping.

using System;
using System.Collections.Generic;

namespace Orts.Parsers.Msts.OrProtection
{
    /// <summary>
    /// Bidirectional dictionary of STF token names ↔ obfuscated codes.
    /// Forward (Map): original lowercase token → "@XXXX" code  — used by packager.
    /// Reverse (Unmap): "@XXXX" code → original lowercase token — used by STFReader.
    ///
    /// Tokens NOT in the table are passed through unchanged so future tokens
    /// or tokens from other file types never break the parser.
    /// The STF structural tokens "(", ")", and string/numeric values are never remapped.
    /// The special STF directives "include", "comment", "skip" are intentionally excluded
    /// so that the built-in STFReader include mechanism keeps working.
    /// </summary>
    internal static class OrTokenMap
    {
        // ── Forward map (token → code) ─────────────────────────────────────────
        private static readonly Dictionary<string, string> _fwd =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // ── Route file (.trk) ──────────────────────────────────────────────
            { "tr_routefile",                               "@3a7f" },
            { "_openrails",                                 "@c1e4" },
            { "routeid",                                    "@8b2d" },
            { "name",                                       "@4f9e" },
            { "filename",                                   "@7c3a" },
            { "description",                                "@2e8b" },
            { "environment",                                "@9d1c" },
            { "routestart",                                 "@6a4f" },
            { "maxlinevoltage",                             "@1b7e" },
            { "speedlimit",                                 "@e3c9" },
            { "electrified",                                "@5f2a" },
            { "overheadwireheight",                         "@b4d6" },
            { "graphic",                                    "@d8e1" },
            { "loadingscreen",                              "@3c7b" },
            { "trackgauge",                                 "@a6f2" },
            { "milepostunitskilometers",                    "@7e1d" },
            { "temprestrictedspeed",                        "@c9b5" },
            { "defaultcrossingsms",                         "@4a8e" },
            { "defaultcoaltowersms",                        "@2f6c" },
            { "defaultdieseltowersms",                      "@8d3b" },
            { "defaultwatertowersms",                       "@1e7a" },
            { "defaultsignalsms",                           "@5c4f" },
            { "precision",                                  "@f1b3" },
            { "maxfreightunderbalance",                     "@6e9d" },
            { "maxpassengerunderbalance",                   "@3b5c" },
            { "minimumcant",                                "@9a2e" },
            { "maximumcant",                                "@c7f4" },
            { "minimumspeed",                               "@4d1b" },
            { "maximumspeed",                               "@8f6a" },
            { "maxrunoffslope",                             "@1c3e" },
            { "maxrunoffspeed",                             "@7a9f" },
            { "ortssingletunnelarea",                       "@b2e5" },
            { "ortssingletunnelperimeter",                  "@5d8c" },
            { "ortsdoubletunnelarea",                       "@e4b1" },
            { "ortsdoubletunnelperimeter",                  "@2a6f" },
            { "ortstracksuperelevation",                    "@9c7e" },
            { "ortssuperelevation",                         "@3f4d" },
            { "ortsforcesuperelevation",                    "@c5a8" },
            { "ortsmaxviewingdistance",                     "@6b1e" },
            { "ortsloadingscreenwide",                      "@d3f7" },
            { "ortsdoublewireenabled",                      "@4e9b" },
            { "ortsdoublewireheight",                       "@a1c6" },
            { "ortstriphaseenabled",                        "@7f2d" },
            { "ortstriphasewidth",                          "@1d8a" },
            { "ortsdefaultturntablesms",                    "@b9e3" },
            { "ortsswitchsmsnumber",                        "@5a7c" },
            { "ortscurvesmsnumber",                         "@2c4f" },
            { "ortscurveswitchsmsnumber",                   "@8e6b" },
            { "ortsopendoorsinaitrains",                    "@f4d2" },
            { "ortsplaytracksoundsbasecontinuous",          "@3d9a" },
            { "ortsdistancebetweentrackjoints",             "@c6f1" },
            { "ortsconcretesleepers",                       "@7b4e" },
            { "ortsuserpreferenceforestcleardistance",      "@1a5d" },
            { "ortsuserpreferenceremoveforesttreesfromroads","@9f3c" },

            // ── Track / Road database (.tdb / .tit / .rdb / .rit) ─────────────
            { "trackdb",                                    "@6d8b" },
            { "tracknodes",                                 "@e2a4" },
            { "tracknode",                                  "@4b7f" },
            { "tritemtable",                                "@c3e9" },
            { "crossoveritem",                              "@8a1d" },
            { "signalitem",                                 "@2f5c" },
            { "speedpostitem",                              "@b7e3" },
            { "platformitem",                               "@5d2a" },
            { "soundregionitem",                            "@1e9f" },
            { "emptyitem",                                  "@7c4b" },
            { "levelcritem",                                "@3a8e" },
            { "sidingitem",                                 "@f6d1" },
            { "hazzarditem",                                "@9b5c" },
            { "pickupitem",                                 "@4e7a" },
            { "uid",                                        "@c1f3" },
            { "trjunctionnode",                             "@8d6e" },
            { "trvectornode",                               "@2b9c" },
            { "trendnode",                                  "@6f4a" },
            { "trpins",                                     "@a3c7" },
            { "trpin",                                      "@1d5f" },
            { "trvectorsections",                           "@e8b2" },
            { "tritemsections",                             "@5c1a" },
            { "tritemrefs",                                 "@9e3d" },
            { "tritemref",                                  "@3b7c" },
            { "tritemid",                                   "@d4f8" },
            { "tritemrdata",                                "@7a2e" },
            { "tritemsdata",                                "@c9b1" },
            { "tritempdata",                                "@4f6d" },
            { "crossovertritemdata",                        "@1c4b" },
            { "trsignaltype",                               "@8e7f" },
            { "trsignaldirs",                               "@5b9c" },
            { "trsignaldir",                                "@2d6a" },
            { "speedposttritemdata",                        "@b1e4" },
            { "platformtritemdata",                         "@6c3f" },
            { "platformname",                               "@a9d2" },
            { "platformminwaitingtime",                     "@3e8b" },
            { "platformnumpassengerswaiting",               "@f7c5" },
            { "sidingname",                                 "@4a1d" },
            { "sidingtritemdata",                           "@9c6e" },
            { "tritemsrdata",                               "@7d4c" },
            { "trinodecount",                               "@c2a9" },
            { "trpincount",                                 "@1f7b" },
            { "carspawneritem",                             "@8b3d" },
        };

        // ── Reverse map (code → token), built once from forward map ───────────
        private static readonly Dictionary<string, string> _rev;

        static OrTokenMap()
        {
            _rev = new Dictionary<string, string>(_fwd.Count, StringComparer.Ordinal);
            foreach (var kv in _fwd)
                _rev[kv.Value] = kv.Key;
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Packager: replace a known token with its obfuscated code.
        /// Returns the original token unchanged if not in the table.
        /// </summary>
        internal static string Map(string token)
        {
            return _fwd.TryGetValue(token, out string code) ? code : token;
        }

        /// <summary>
        /// STFReader: translate an obfuscated code back to the real token name.
        /// Returns the input unchanged if it is not a mapped code (e.g. "(", numbers,
        /// quoted string contents, or unknown future tokens).
        /// </summary>
        internal static string Unmap(string token)
        {
            if (token.Length > 1 && token[0] == '@')
            {
                if (_rev.TryGetValue(token, out string original))
                    return original;
            }
            return token;
        }
    }
}
