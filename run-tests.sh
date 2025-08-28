#!/bin/bash

# Script d'exécution des tests pour Perim'App
# Usage: ./run-tests.sh [options]

set -e

echo "🧪 Suite de Tests Perim'App"
echo "=========================="
echo ""

# Couleurs pour l'affichage
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Fonction d'aide
show_help() {
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  -h, --help          Afficher cette aide"
    echo "  -v, --verbose       Mode verbeux"
    echo "  -c, --coverage      Générer la couverture de code"
    echo "  -f, --filter FILTER Filtrer les tests par nom/catégorie"
    echo "  -q, --quiet         Mode silencieux"
    echo "  --unit              Exécuter seulement les tests unitaires"
    echo "  --integration       Exécuter seulement les tests d'intégration"
    echo "  --e2e               Exécuter seulement les tests end-to-end"
    echo ""
    echo "Exemples:"
    echo "  $0                                 # Tous les tests"
    echo "  $0 --verbose                       # Avec détails"
    echo "  $0 --filter ProductInfosTests      # Tests d'un modèle spécifique"
    echo "  $0 --unit                          # Tests unitaires seulement"
    echo "  $0 --coverage                      # Avec couverture de code"
}

# Variables par défaut
VERBOSE=false
COVERAGE=false
QUIET=false
FILTER=""
TEST_TYPE=""

# Parse des arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -h|--help)
            show_help
            exit 0
            ;;
        -v|--verbose)
            VERBOSE=true
            shift
            ;;
        -c|--coverage)
            COVERAGE=true
            shift
            ;;
        -f|--filter)
            FILTER="$2"
            shift 2
            ;;
        -q|--quiet)
            QUIET=true
            shift
            ;;
        --unit)
            TEST_TYPE="unit"
            shift
            ;;
        --integration)
            TEST_TYPE="integration"
            shift
            ;;
        --e2e)
            TEST_TYPE="e2e"
            shift
            ;;
        *)
            echo -e "${RED}Option inconnue: $1${NC}"
            show_help
            exit 1
            ;;
    esac
done

# Vérification des prérequis
echo -e "${BLUE}🔍 Vérification des prérequis...${NC}"

if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}❌ .NET SDK n'est pas installé${NC}"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo -e "${GREEN}✅ .NET SDK $DOTNET_VERSION trouvé${NC}"

# Vérification des projets
if [ ! -f "PerimApp.Core/PerimApp.Core.csproj" ]; then
    echo -e "${RED}❌ Projet PerimApp.Core introuvable${NC}"
    exit 1
fi

if [ ! -f "PerimApp.Tests/PerimApp.Tests.csproj" ]; then
    echo -e "${RED}❌ Projet PerimApp.Tests introuvable${NC}"
    exit 1
fi

echo -e "${GREEN}✅ Projets trouvés${NC}"
echo ""

# Restoration des packages
echo -e "${BLUE}📦 Restoration des packages...${NC}"
dotnet restore PerimApp.Tests/PerimApp.Tests.csproj --verbosity quiet
echo -e "${GREEN}✅ Packages restaurés${NC}"
echo ""

# Construction
echo -e "${BLUE}🔨 Construction des projets...${NC}"
dotnet build PerimApp.Tests/PerimApp.Tests.csproj --configuration Release --no-restore --verbosity quiet
echo -e "${GREEN}✅ Construction réussie${NC}"
echo ""

# Préparation de la commande de test
TEST_CMD="dotnet test PerimApp.Tests/PerimApp.Tests.csproj --configuration Release --no-build"

# Configuration du niveau de verbosité
if [ "$VERBOSE" = true ]; then
    TEST_CMD="$TEST_CMD --verbosity normal"
elif [ "$QUIET" = true ]; then
    TEST_CMD="$TEST_CMD --verbosity quiet"
else
    TEST_CMD="$TEST_CMD --logger:\"console;verbosity=minimal\""
fi

# Ajout du filtre si spécifié
if [ -n "$FILTER" ]; then
    TEST_CMD="$TEST_CMD --filter \"$FILTER\""
elif [ -n "$TEST_TYPE" ]; then
    case $TEST_TYPE in
        "unit")
            TEST_CMD="$TEST_CMD --filter \"FullyQualifiedName~Models|FullyQualifiedName~Services.PasswordHasherTests|FullyQualifiedName~Converters|FullyQualifiedName~Utilities\""
            ;;
        "integration")
            TEST_CMD="$TEST_CMD --filter \"FullyQualifiedName~Integration&!FullyQualifiedName~EndToEndWorkflowTests\""
            ;;
        "e2e")
            TEST_CMD="$TEST_CMD --filter \"FullyQualifiedName~EndToEndWorkflowTests\""
            ;;
    esac
fi

# Ajout de la couverture de code si demandée
if [ "$COVERAGE" = true ]; then
    TEST_CMD="$TEST_CMD --collect:\"XPlat Code Coverage\""
fi

# Exécution des tests
echo -e "${BLUE}🚀 Exécution des tests...${NC}"
echo -e "${YELLOW}Commande: $TEST_CMD${NC}"
echo ""

# Exécution avec gestion des erreurs
set +e
eval $TEST_CMD
TEST_EXIT_CODE=$?
set -e

# Affichage des résultats
echo ""
if [ $TEST_EXIT_CODE -eq 0 ]; then
    echo -e "${GREEN}✅ Tous les tests sont passés avec succès !${NC}"
else
    echo -e "${RED}❌ Certains tests ont échoué (code de sortie: $TEST_EXIT_CODE)${NC}"
fi

# Traitement de la couverture de code si demandée
if [ "$COVERAGE" = true ]; then
    echo ""
    echo -e "${BLUE}📊 Génération du rapport de couverture...${NC}"
    
    # Recherche du fichier de couverture
    COVERAGE_FILE=$(find . -name "*.cobertura.xml" | head -1)
    
    if [ -n "$COVERAGE_FILE" ]; then
        echo -e "${GREEN}✅ Fichier de couverture trouvé: $COVERAGE_FILE${NC}"
        
        # Génération d'un rapport HTML si reportgenerator est disponible
        if command -v reportgenerator &> /dev/null; then
            echo -e "${BLUE}📈 Génération du rapport HTML...${NC}"
            reportgenerator -reports:"$COVERAGE_FILE" -targetdir:"coverage-report" -reporttypes:Html
            echo -e "${GREEN}✅ Rapport HTML généré dans: coverage-report/index.html${NC}"
        else
            echo -e "${YELLOW}⚠️  reportgenerator non disponible. Installez-le avec:${NC}"
            echo "   dotnet tool install -g dotnet-reportgenerator-globaltool"
        fi
    else
        echo -e "${YELLOW}⚠️  Aucun fichier de couverture trouvé${NC}"
    fi
fi

# Statistiques finales
echo ""
echo -e "${BLUE}📈 Statistiques des tests:${NC}"
echo "🎯 Tests couvrant tous les aspects de l'application"
echo "🧪 Tests unitaires pour modèles, services, utilitaires"
echo "🔗 Tests d'intégration pour les workflows"
echo "🎭 Tests end-to-end pour les scénarios complets"
echo "⚡ Tests de performance et de validation"

echo ""
echo -e "${BLUE}📚 Pour plus d'informations, consultez: README_TESTS.md${NC}"

exit $TEST_EXIT_CODE