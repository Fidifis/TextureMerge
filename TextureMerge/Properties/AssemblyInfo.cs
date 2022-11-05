using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;

// Obecné informace o sestavení se řídí přes následující 
// sadu atributů. Změnou hodnot těchto atributů se upraví informace
// přidružené k sestavení.
[assembly: AssemblyTitle("TextureMerge")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("TextureMerge")]
[assembly: AssemblyCopyright("Copyright © Fidifis 2022")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Nastavení ComVisible na false způsobí neviditelnost typů v tomto sestavení
// pro komponenty modelu COM. Pokud potřebujete přístup k typu v tomto sestavení
// z modelu COM, nastavte atribut ComVisible tohoto typu na True.
[assembly: ComVisible(false)]

//Pokud chcete začít vytvářet aplikace, které se dají lokalizovat, nastavte
//<UICulture>JazykováVerzeVeKteréPíšeteKód</UICulture> v souboru .csproj
//uvnitř <PropertyGroup>.  Pokud například používáte jazykovou verzi US english
//ve zdrojových souborech, nastavte <UICulture> na en-US.  Pak zrušte komentář
//pro atribut NeutralResourceLanguage.  Aktualizujte hodnotu "en-US" na
//dalším řádku, aby se shodovala s nastavením UICulture v souboru projektu.

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]


[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //kde se nacházejí zdrojové slovníky pro konkrétní motiv
                                     //(používá se, pokud se prostředek nenajde na stránce
                                     // nebo ve zdrojových slovnících aplikace)
    ResourceDictionaryLocation.SourceAssembly //kde se nachází obecný zdrojový slovník
                                              //(používá se, pokud se prostředek nenajde na stránce
                                              // v aplikaci nebo libovolných zdrojových slovnících pro konkrétní motiv)
)]


// Informace o verzi sestavení se skládá z těchto čtyř hodnot:
//
//      Hlavní verze
//      Podverze
//      Číslo sestavení
//      Revize
//
// Můžete zadat všechny hodnoty nebo nastavit výchozí číslo buildu a revize
// pomocí zástupného znaku * takto:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.3.1")]
[assembly: AssemblyFileVersion("1.3.1")]
